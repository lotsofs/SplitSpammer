# Porting groundwork

Detail for the groundwork rows of [TODO.md](TODO.md): what the engine needs before the missions can be ported the way [porting.md](porting.md) describes.

Dependencies: TST (in [Component.md](Component.md)) gives the comparison harness every port uses. PTW reuses the reader's pointer reads (`MemoryLocation`). MHL builds on the engine's last started mission. Big Smoke (BSM) is the first port that needs all of them. STL waits until the ports have been compared with the ASL.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`startup`, `init`, `split`) and in the component's `Engine\` and `Game\` folders.

## MHL — Mission helpers: basic settings and the progress check

Every story mission has the same frame in the ASL. `RegisterMissionBasics(abbr, name, thread, parent)` (or the same lines written out) adds the mission's setting, on by default, with children "Mission Marker Entered" and "Mission Started", off by default, registers the thread with the start id and the mission name with the marker id; each mission then adds its event settings and a "Mission Passed" setting, on by default. Its check starts with `ValidateMissionProgress`, whose semantics and read order are in porting.md ("ASL behaviour the engine keeps"). Written out by hand in each module, this is the repetition the ASL repo tracked as its item TBL (`reference\Defects.md`), and the id names for the three common splits are not decided yet (ITB used `intro_passed` for the bicycle event).

**Evidence:** read 2026-11-15; the helpers in `startup` and `init`, and their use in the live funcs.

**Plan:** a helper that adds the mission's three common settings with the ASL's labels and defaults and returns nothing more, used in `AddSettings`, and one used in `AddChecks` that registers the thread and the mission name (`engine.RegisterThread`, `engine.RegisterMissionName`); an `engine.MissionInProgress(thread, chainWatcher, before, after, passId, settingId)` with `ValidateMissionProgress`'s exact semantics, using `SettingsSnapshot.IsEnabled` for the setting. Decide the common ids, for example `<mission>_markerEntered`, `<mission>_started` and `<mission>_passed`, and write the rule into CLAUDE.md's Conventions. Consider a small table of event checks (a value becoming B, a change from A to B, a switch on the new value) only if it reads the watchers in the same order as the ASL's funcs; a hand-written check stays fine.

**Verify:** tests of the progress check against the ASL's semantics: the setting off reads nothing, another last started mission returns false, the counter reaching the pass index splits the pass once and only from the index before it, also in the tick the thread ends, and after a fail (the thread gone, the counter unchanged) it reads nothing from the next tick on; ported with Big Smoke or Ryder, whose comparison passes.

**Docs when done:** CLAUDE.md ("Adding a mission", Conventions), porting.md (steps 1 and 3).

## PTW — Pointer watchers and the other always-present watchers

The ASL's `init` always adds `wantedLevel` (`+0x7AA420`, 4 bytes), `pedStatus` (1 byte at the address in the int at `+0x77CD98`, plus `0x530`) and `weaponSlotMelee` (the same pointer, plus `0x5BC`), and `startup` adds `interior` (`$43`). Against All Odds reads the wanted level and the melee slot, the warps read `pedStatus`, and stunt jumps read 70 two-byte records behind a two-level pointer (`[[+0x69A888] + 0] + 0x40 + 0x44 * i`). `WatcherSet` has no pointer watchers.

**Evidence:** read 2026-11-15. The ASL repo's check VER 21 says the new always-on watchers were never seen showing sane values.

**Plan:** `WatcherSet.AddPointer(name, location, bytes)` on `MemoryLocation`, which already follows pointers as LiveSplit's `DeepPointer` does (checked against it for the string watchers, 2026-11-15), behaving like a `MemoryWatcher<int>`: a null pointer or a failed read is a failed read, which keeps Current and sets Old to it. Check that against the real class too, since the string watcher turned out to differ there. The plain watchers (`wantedLevel`, `interior`) are added by each module that reads them; several modules registering one address share its watcher.

**Verify:** tests with a fake reader that changes the pointer; on the running game, log `pedStatus` while getting busted (63) and wasted (55), and `weaponSlotMelee` after picking up the chainsaw (9).

**Docs when done:** docs/architecture.md (game memory table).

## STL — Stale Old after a gap, kept from the ASL

`IntWatcher` copies LiveSplit's `MemoryWatcher<int>` on purpose, so the ports compare equal with the ASL, and that includes the ASL's defect STL (`reference\Defects.md`): a watcher read only while a condition holds compares its first read after a gap with its last read, possibly a mission or a save ago, and an `Old == X && Current == Y` check can split on a change that never happened in one frame. The ASL repo's example is Tagging up Turf's `58@` after leaving and re-entering the mission. Once-per-reset limits each false split to one.

**Evidence:** read 2026-11-13 in the ASL repo, not reproduced; the component's `IntWatcher` has the same behaviour by design (docs/architecture.md).

**Plan:** after the ports have been compared, keep the previous tick's read set as well, and when a watcher was not read in the previous tick, read it twice on its first read, so that Changed is false after a gap. Apply the same change to the ASL side of the comparison harness, so the comparisons keep passing. Decide then whether any check relies on the stale value (none is known).

**Verify:** a test of the Tagging up Turf scenario: no split on returning to the mission, and the split still comes when the car is entered after the third tag; the comparisons still pass with the harness changed the same way.

**Docs when done:** docs/architecture.md (the watcher rules), porting.md (step 3 and "ASL behaviour the engine keeps"), CHANGELOG.md Unreleased (Fixed) if a released split behaved wrongly.
