# Porting groundwork

Detail for the groundwork rows of [TODO.md](TODO.md): what the engine needs before the missions can be ported the way [porting.md](porting.md) describes.

Dependencies: TST (in [Component.md](Component.md)) gives the comparison harness every port uses. THR needs string and pointer reads in the reader; PTW reuses the pointer reads. MHL builds on THR (the last started mission). Big Smoke (BSM) is the first port that needs all of them. STL waits until the ports have been compared with the ASL.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`startup`, `init`, `split`) and in the component's `Engine\` and `Game\` folders.

## THR — Thread, mission-name text and the last started mission

Most missions need three things the engine lacks. The ASL's `split` block, after the guards and before the checks, reads `thread` every tick: when it changed to a thread other than `vars.lastStartedMission` and the thread is in `vars.significantThreads`, it splits that thread's id (when it has one) and makes it the last started mission. Then it reads `missionStartText` every tick: when it changed to a name in `vars.missionNames`, it splits that id. `onReset` clears the last started mission; nothing else does, so it survives a failed mission. The checks read `vars.lastStartedMission`, and some read `thread` (House Party, the Catalina dates, Wu Zi Mu, gang territories) or `oddMissionStartText` (Trucking) or compare `missionStartText` themselves (Chiliad Challenge).

The watchers are strings: `thread` is 10 bytes at the address in the int at `+0x68B42C`, plus 8 (the ASL's `DeepPointer(0x68B42C, 0x8)`, the name of the first script in the game's active list, usually the one last started); `missionStartText` is 32 bytes at `+0x7AAD40` (the yellow mission name shown when a marker is entered); `oddMissionStartText` is 32 bytes at `+0x7AAEC0`. The ASL uses LiveSplit's `StringWatcher` for them. `IGameReader` reads only module-relative ints, so neither the string nor the pointer can be read yet.

**Evidence:** read 2026-11-15. How the thread name appears in memory (case, padding) is not known; the ASL compares it with lower-case names.

**Plan:** add to `IGameReader` a read of bytes at a module offset and at an absolute address (the pointer target), and make `ProcessGameReader` and the test fake do both. Add a string watcher to `WatcherSet` that copies LiveSplit's `StringWatcher` exactly (how it decodes the bytes and where it stops, the first read counting as unchanged, a failed read), checked against LiveSplit.Core as `IntWatcher` was; a string watcher can follow a pointer. In `SplitEngine`: `RegisterThread(thread, splitId)` (null id: remember the thread without splitting), `RegisterMissionName(text, splitId)`, a `LastStartedMission` the checks read, cleared in `Reset()`, and the two steps in `TickSplit` after the guards and before the checks, in the ASL's order. A thread registered twice keeps the ASL's behaviour (its dictionary would throw; here, throw too). Log the thread and the text when they change, at debug level, to see on the running game what the strings look like before relying on them. GSH needs the string watcher too.

**Verify:** tests with a fake reader: a registered thread splits once and becomes the last started mission, an unregistered one does neither, a mission name splits, a reset clears the last started mission; the In the Beginning comparison still passes. On the running game: start Big Smoke and see `intro1` in the log, and the "Big Smoke" text when entering its marker.

**Docs when done:** docs/architecture.md (one engine tick, the watcher rules, the game memory table), porting.md if a fact in "ASL behaviour the engine keeps" turns out different.

## MHL — Mission helpers: basic settings and the progress check

Every story mission has the same frame in the ASL. `RegisterMissionBasics(abbr, name, thread, parent)` (or the same lines written out) adds the mission's setting, on by default, with children "Mission Marker Entered" and "Mission Started", off by default, registers the thread with the start id and the mission name with the marker id; each mission then adds its event settings and a "Mission Passed" setting, on by default. Its check starts with `ValidateMissionProgress`, whose semantics and read order are in porting.md ("ASL behaviour the engine keeps"). Written out by hand in each module, this is the repetition the ASL repo tracked as its item TBL (`reference\Defects.md`), and the id names for the three common splits are not decided yet (ITB used `intro_passed` for the bicycle event).

**Evidence:** read 2026-11-15; the helpers in `startup` and `init`, and their use in the live funcs.

**Plan:** a helper that adds the mission's three common settings with the ASL's labels and defaults and returns nothing more, used in `AddSettings`, and one used in `AddChecks` that registers the thread and the mission name (THR); an `engine.MissionInProgress(thread, chainWatcher, before, after, passId, settingId)` with `ValidateMissionProgress`'s exact semantics, using `SettingsSnapshot.IsEnabled` for the setting. Decide the common ids, for example `<mission>_markerEntered`, `<mission>_started` and `<mission>_passed`, and write the rule into CLAUDE.md's Conventions. Consider a small table of event checks (a value becoming B, a change from A to B, a switch on the new value) only if it reads the watchers in the same order as the ASL's funcs; a hand-written check stays fine.

**Verify:** tests of the progress check against the ASL's semantics: the setting off reads nothing, another last started mission returns false, the counter reaching the pass index splits the pass once and only from the index before it; ported with Big Smoke or Ryder, whose comparison passes.

**Docs when done:** CLAUDE.md ("Adding a mission", Conventions), porting.md (steps 1 and 3).

## PTW — Pointer watchers and the other always-present watchers

The ASL's `init` always adds `wantedLevel` (`+0x7AA420`, 4 bytes), `pedStatus` (1 byte at the address in the int at `+0x77CD98`, plus `0x530`) and `weaponSlotMelee` (the same pointer, plus `0x5BC`), and `startup` adds `interior` (`$43`). Against All Odds reads the wanted level and the melee slot, the warps read `pedStatus`, and stunt jumps read 70 two-byte records behind a two-level pointer (`[[+0x69A888] + 0] + 0x40 + 0x44 * i`). `WatcherSet` has no pointer watchers.

**Evidence:** read 2026-11-15. The ASL repo's check VER 21 says the new always-on watchers were never seen showing sane values.

**Plan:** `WatcherSet.AddPointer(name, offsets, bytes)` on the absolute read from THR, behaving like LiveSplit's `DeepPointer` with a `MemoryWatcher`: a null pointer or a failed read is a failed read, which keeps Current and sets Old to it. The plain watchers (`wantedLevel`, `interior`) are added by each module that reads them; several modules registering one address share its watcher.

**Verify:** tests with a fake reader that changes the pointer; on the running game, log `pedStatus` while getting busted (63) and wasted (55), and `weaponSlotMelee` after picking up the chainsaw (9).

**Docs when done:** docs/architecture.md (game memory table).

## STL — Stale Old after a gap, kept from the ASL

`IntWatcher` copies LiveSplit's `MemoryWatcher<int>` on purpose, so the ports compare equal with the ASL, and that includes the ASL's defect STL (`reference\Defects.md`): a watcher read only while a condition holds compares its first read after a gap with its last read, possibly a mission or a save ago, and an `Old == X && Current == Y` check can split on a change that never happened in one frame. The ASL repo's example is Tagging up Turf's `58@` after leaving and re-entering the mission. Once-per-reset limits each false split to one.

**Evidence:** read 2026-11-13 in the ASL repo, not reproduced; the component's `IntWatcher` has the same behaviour by design (docs/architecture.md).

**Plan:** after the ports have been compared, keep the previous tick's read set as well, and when a watcher was not read in the previous tick, read it twice on its first read, so that Changed is false after a gap. Apply the same change to the ASL side of the comparison harness, so the comparisons keep passing. Decide then whether any check relies on the stale value (none is known).

**Verify:** a test of the Tagging up Turf scenario: no split on returning to the mission, and the split still comes when the car is entered after the third tag; the comparisons still pass with the harness changed the same way.

**Docs when done:** docs/architecture.md (the watcher rules), porting.md (step 3 and "ASL behaviour the engine keeps"), CHANGELOG.md Unreleased (Fixed) if a released split behaved wrongly.
