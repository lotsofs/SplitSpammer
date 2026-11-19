# Porting groundwork

Detail for the groundwork rows of [TODO.md](TODO.md): what the engine needs before the missions can be ported the way [porting.md](porting.md) describes.

Dependencies: TST (in [Component.md](Component.md)) gives the comparison harness every port uses. CJH (in [LosSantos.md](LosSantos.md)) reads CJ's position with `WatcherSet.AddPlayerPosition`, which exists, and needs nothing else here.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`startup`, `init`, `split`) and in the component's `Engine\` and `Game\` folders.

## PTW — The always-present pointer watchers

The ASL's `init` always adds `wantedLevel` (`+0x7AA420`, 4 bytes), `pedStatus` (1 byte at the address in the int at `+0x77CD98`, plus `0x530`) and `weaponSlotMelee` (the same pointer, plus `0x5BC`), and `startup` adds `interior` (`$43`). Against All Odds reads the wanted level and the melee slot, the warps read `pedStatus`, and stunt jumps read 70 two-byte records behind a two-level pointer (`[[+0x69A888] + 0] + 0x40 + 0x44 * i`). None of these is ported. `WatcherSet.AddPointer` follows pointers and, without a `whenNull` value, treats a null pointer as a failed read, as a `MemoryWatcher<int>` on a `DeepPointer` is assumed to; that is not checked against the real class, and the string watcher turned out to differ from it there.

**Evidence:** read 2026-11-15. The ASL repo's check VER 21 says the new always-on watchers were never seen showing sane values. `pedStatus` was seen on the running game on 2026-11-19 (50 driving, 0 getting out, 1 on foot) by a scratch probe, not through a watcher.

**Plan:** check `AddPointer`'s null pointer against LiveSplit's `MemoryWatcher<int>` on a `DeepPointer` (a scratch program on LiveSplit.Core, as for the string watcher), and match it if it differs. Then helpers like `AddVehicleModel` for `pedStatus`, `weaponSlotMelee` and the stunt-jump records, called by each module that reads them; the plain watchers (`wantedLevel`, `interior`) are added by each module with `Add`. Several modules registering one location share its watcher.

**Verify:** on the running game, log `pedStatus` while getting busted (63) and wasted (55), and `weaponSlotMelee` after picking up the chainsaw (9).

**Docs when done:** docs/architecture.md (game memory table).
