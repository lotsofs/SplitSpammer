# Porting groundwork

Detail for the groundwork rows of [TODO.md](TODO.md): what the engine needs before the missions can be ported the way [porting.md](porting.md) describes.

Dependencies: TST (in [Component.md](Component.md)) gives the comparison harness every port uses. PTW reuses the reader's pointer reads (`MemoryLocation`); CJH (in [LosSantos.md](LosSantos.md)) needs PTW.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`startup`, `init`, `split`) and in the component's `Engine\` and `Game\` folders.

## PTW — Pointer watchers and the other always-present watchers

The ASL's `init` always adds `wantedLevel` (`+0x7AA420`, 4 bytes), `pedStatus` (1 byte at the address in the int at `+0x77CD98`, plus `0x530`) and `weaponSlotMelee` (the same pointer, plus `0x5BC`), and `startup` adds `interior` (`$43`). Against All Odds reads the wanted level and the melee slot, the warps read `pedStatus`, and stunt jumps read 70 two-byte records behind a two-level pointer (`[[+0x69A888] + 0] + 0x40 + 0x44 * i`). `WatcherSet` has no pointer watchers.

**Evidence:** read 2026-11-15. The ASL repo's check VER 21 says the new always-on watchers were never seen showing sane values.

**Plan:** `WatcherSet.AddPointer(name, location, bytes)` on `MemoryLocation`, which already follows pointers as LiveSplit's `DeepPointer` does (checked against it for the string watchers, 2026-11-15), behaving like a `MemoryWatcher<int>`: a null pointer or a failed read is a failed read, which keeps Current and sets Old to it. Check that against the real class too, since the string watcher turned out to differ there. The plain watchers (`wantedLevel`, `interior`) are added by each module that reads them; several modules registering one address share its watcher.

**Verify:** tests with a fake reader that changes the pointer; on the running game, log `pedStatus` while getting busted (63) and wasted (55), and `weaponSlotMelee` after picking up the chainsaw (9).

**Docs when done:** docs/architecture.md (game memory table).
