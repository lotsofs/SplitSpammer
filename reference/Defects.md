# GTASA.asl defects and tasks (reference)

> Reference copy, moved from livesplit-asl's docs/Defects.md on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

Detail for the defect and task rows of [TODO.md](TODO.md). SVL and LDG share the `loading` watcher, so do them together. STL and BLK both change `GetWatcher`; do STL first, it is a few lines. TBL's generic func reads watchers conditionally, so it also comes after STL.

## DSP — Double-split prevention blocks splits instead of spacing them

With the "Double-Split Prevention" setting on, `TrySplit` (defined in `init` of `GTASA.asl`) refuses a split when *more* than 2.5 s have passed since the previous split. The tooltip promises the opposite: a 2.5 s cooldown between auto-splits. In practice, with the setting on, almost every split is blocked and only splits within 2.5 s of another go through. The setting is off by default, so most players never see it.

Before the rewrite the condition read "split if prevention is off or 2.5 s have passed"; the rewrite turned it into "prevent if prevention is on and 2.5 s have passed", flipping the comparison.

**Evidence:** read 2026-11-13. The inversion arrived in commit 0e7dbbb (2025-08-31); compare `TrySplit` in `git show f7885d5:GTASA.asl`.

**Plan:** change `> 2500` to `< 2500` in the cooldown test of `TrySplit`. Nothing else uses `vars.lastSplit`.

**Verify:** setting on: a mission's marker and start splits (which fire within a second of each other) produce one split; two splits a minute apart both fire. Setting off: unchanged.

**Docs when done:** CHANGELOG.md Unreleased, under its Fixed group, and the README once it has a GTA SA section (RDM).

## SVL — Start and reset on save load settings do nothing

The Settings group shows "Start timer when loading save" and "Reset timer when loading save". Neither does anything: the code that read them in `start` and `reset` is commented out, and the `loading` watcher (`loadingAddr`, which is true only while a save is being loaded) is created only in the dead tail of `init`. "Start timer when loading save" worked (as "experimental") up to the 2025-08-20 commit; commit 0e7dbbb (2025-08-31) commented its `start` branch out and moved the watcher behind the early return. "Reset timer when loading save" has never had an implementation.

**Evidence:** read 2026-11-13 (`grep -n "SaveLoad\|\"loading\"" GTASA.asl`).

**Plan:** register `loadingAddr` with the basic addresses in `init` (`vars.AddNonScmAddressWatcher(loadingAddr, "loading", 1)`; it's a one-byte flag), restore the `start` branch from the 2024 code (`!loading.Current && loading.Old`, setting `vars.lastLoad`), and either implement the reset branch the same way while the timer is running or remove the reset setting until it exists. Do LDG in the same change.

**Verify:** load a save with each setting on and off: the timer starts or resets only with the setting on, and never on an in-game loading screen.

**Docs when done:** CHANGELOG.md Unreleased, under Fixed or under Removed depending on the choice; README GTA SA section (RDM).

## LDG — The after-load split guard never arms

`split` returns early for 600 ms after `vars.lastLoad`, with the comment that memory values changing while a save loads can otherwise trigger splits. Nothing in the live code assigns `vars.lastLoad`; every assignment is in commented-out code (the split-prevention block, `start`, `reset`). The guard that remains is `playingTime.Current < 600`, which covers only a new game. Whether loading a save mid-run actually produces spurious splits with the current watchers hasn't been checked; the once-per-reset `completedSplits` list limits the damage to splits not yet done.

**Evidence:** read 2026-11-13 (`grep -n lastLoad GTASA.asl`).

**Plan:** with the `loading` watcher from SVL, set `vars.lastLoad = Environment.TickCount` in `update` when `loading` goes from true to false. If the guard turns out to be unnecessary, delete it and `vars.lastLoad` instead of leaving a guard that can't fire.

**Verify:** load an earlier save mid-run with debug output on: "Wait.." and "Done waiting.." appear around the load and no split fires from the load itself.

**Docs when done:** CLAUDE.md "Runtime flow", the split-prevention bullet, if the mechanism changes.

## STL — Lazily read watchers compare against their last read, not the last tick

`vars.GetWatcher` updates a watcher only when a func asks for it, and LiveSplit's `MemoryWatcher<T>.Update` sets `Old` to the value from the previous *update* (`base.Old = Current` before the read). For a watcher read every tick that is the previous tick. For one read only while a condition holds, `Old` is the value from when it was last asked for, possibly a mission or a save ago, and its first read after the gap reports `Changed` for a transition that never happened in one frame. The old `UpdateAll` design didn't have this problem; the lazy update (commit 7edd512, "Performance boost") introduced it.

Mission locals are the exposed case: one block shared by every mission, zeroed when a mission starts, and read only while `ValidateMissionProgress` returns true. Example in `func_tut` (Tagging up Turf): `58@` splits `tut_carEnterAfterTag3` on `Old == 2 && Current == 0`. Leave the mission after the spray help box has shown (58@ at 2), start another mission so `vars.lastStartedMission` changes, then come back to Tagging up Turf. Once the dialogue-block failsafe passes, the first read of 58@ sees Old 2 (stale) and Current 0 and splits before the car is entered. Every `Old == X && Current == Y` check on a conditionally read watcher has the same shape, and chain counters have it after loading an earlier save. `completedSplits` limits each false split to once per reset.

**Evidence:** read 2026-11-13, in `GTASA.asl` and in LiveSplit's `MemoryWatcher.cs` (`src/LiveSplit.Core/ComponentUtil`). Not reproduced in game.

**Plan:** in `update`, keep the previous tick's `vars.currentWatchers` set (swap two sets instead of clearing one). In `GetWatcher`, when the watcher wasn't in the previous tick's set, call `Update` twice before returning, so `Old == Current` and `Changed` is false on the first read after a gap; the extra read happens only on re-entry. Watchers read every tick are unaffected.

**Verify:** the Tagging up Turf scenario above with debug output on: no `tut_carEnterAfterTag3` on return to the mission, and the split still fires when the car is actually entered after tag 3. Loading an earlier save mid-mission produces no `_pass` split from the chain counter.

**Docs when done:** CLAUDE.md "Runtime flow", the `GetWatcher` bullet.

## TBL — Mission blocks repeat the same split-check shape by hand

Each story mission is a `#region` of settings plus a `Func<string>` that, once `vars.ValidateMissionProgress` lets it through (32 of the 50 funcs use it), reads a few watchers and returns a split ID when a value changes. Nearly all of those checks have one of three shapes: a value becoming B (`x.Changed && x.Current == B`, 53 of them), a transition from A to B (`x.Changed && x.Old == A && x.Current == B`, ten), or a `switch` on the new value with one `case` per event (Catalyst, Robbing Uncle Sam). The story-mission regions alone run to about 2,900 lines, and every mission still to port adds 50 to 70 more written the same way. The bespoke parts are few: loops over a counter (crates in Catalyst, boxes in Robbing Uncle Sam, plants in Body Harvest), interior and wanted-level checks, and the warps.

**Evidence:** read 2026-11-13 (`grep -c` of the three shapes over the live part of `startup`). A first conversion of Tagging up Turf, checked against the old script in a simulation harness, exists in `git stash` ("TBL step 1").

**Plan:** add helpers next to `RegisterMissionBasics` in `startup`: `RegisterMissionChain(abbr, thread, chain, before, after)` records what `ValidateMissionProgress` needs once per mission, and `AddValueSplit(abbr, watcher, value, splitId)` / `AddTransitionSplit(abbr, watcher, from, to, splitId)` append to a per-mission table. One generic `CheckSplit` func walks the table of the active mission (`vars.lastStartedMission`): it calls `ValidateMissionProgress` once, reads each entry's watcher through `GetWatcher`, and returns the split ID when the shape matches. Each event's `settings.Add` and its table line sit next to each other inside the mission's region, so a block reads as a list of events; a hand-written func stays only for bespoke logic and lives in the same region. Convert Tagging up Turf first (all its checks are transitions), compare, then convert region by region.

**Verify:** for a converted mission, each event split fires once in the same place as before, with debug output on. Without the game: dump the settings tree before and after and diff; IDs, labels, defaults and parents must be unchanged.

**Docs when done:** CLAUDE.md "How a mission is defined" (step 3 becomes the table), and the "as SFM" plan lines in [Porting.md](Porting.md).

## BLK — Per-slot watchers cost a memory read and a list scan each

`init` builds about 1,400 watchers: SCM globals and locals, the basics, and one per slot for tags (100), horseshoes, snapshots and oysters (an id word and a state byte for each of the 150), stunt jumps (70) and gang territories (2 per territory, 378 territories). `vars.watcherList[name]` is LiveSplit's `MemoryWatcherList` indexer, `this.First(w => w.Name == name)`, a linear scan, so every `GetWatcher` call scans on average half the list and then does one `ReadProcessMemory`. The per-slot loops in `func_collectibles`, `func_tags`, `func_usj` and `func_GangTerritories` do that once per slot per tick while their "Specific" settings are on: with everything on (including the "other" territory zones) roughly 1,200 reads and 850,000 string comparisons per tick at the script's 30 Hz. Estimated from the code, not measured.

With default settings the tick is cheap (on the order of 100 reads), but two funcs run unconditionally: `func_properties` reads 30 `PropertyN` watchers every tick, and `func_phonecall` reads up to 30 `PhoneCallN` watchers and allocates a new `HashSet<int>` of excluded numbers on each of its 30 iterations, neither checking its group setting ("PhoneCalls" is off by default).

**Plan:**
1. Replace the per-slot watchers with one block read per table per tick (`game.ReadBytes` on the module-relative base plus offset): collectibles are 150 × 0x20 bytes from the found base, tags 100 × 8 bytes at 0x69A8C0, territories 378 × 17 bytes at 0x7A1E01, stunt jumps 70 × 0x44 bytes behind the 0x69A888 pointer. Keep the previous block in `vars` and diff the two; the collectible relocation keeps working by rescanning when the id column stops matching, as now.
2. Index watchers through a `Dictionary<string, MemoryWatcher>` built in `init` beside the list, and have `GetWatcher` and `ChangeAddressWatchers` use it.
3. Hoist the excluded-numbers set out of the loop in `func_phonecall` and return early from `func_phonecall` and `func_properties` when their group setting is off.

**Verify:** wrap the `CheckSplit` loop in `split` in a `Stopwatch` and debug-print the tick time before and after with every "Specific" setting on; a specific tag, horseshoe, snapshot, oyster, stunt jump and territory still split; collectible addresses still relocate after loading a save.

**Docs when done:** CLAUDE.md "Runtime flow" (the `GetWatcher` and `watcherList` bullets) and "How a mission is defined" (watcher registration) where the API changes.
