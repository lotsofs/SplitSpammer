# Reference material for the GTA SA port

Moved from livesplit-asl on 2026-11-14 when GTA SA auto splitting moved to SplitSpammer. Nothing in this folder is part of the component.

| File | What it is |
|---|---|
| `GTASA.asl` | The ASL auto splitter being ported: a fork of Tduva's script, half rewritten. Git-ignored here, because Tduva's repository has no license (see item LIC in `docs/Project.md`). |
| `GTASA_orig_recentupdate.asl` | A recent update of Tduva's original script by someone else: multi-version with Steam offsets, and a thread-based timer start. Kept to compare with. Git-ignored like `GTASA.asl` (`reference/*.asl`); never commit it. |
| `main.scm.txt`, `scm_leak.txt` | Reference dumps of the game's own script. Git-ignored; the leaked source must never be pushed. |
| `Porting.md` | What the script still lacks, by part of the game, with where its logic is. SplitSpammer's own items for these, one per mission, are in `docs/TODO.md`. |
| `Defects.md`, `Verify.md`, `TODO.md`, `DONE.md`, `CHANGELOG.md` | The script's known defects, unverified checks, old index and log rows, and player-facing history. Worth reading for behaviour not to carry over (for example the inverted double-split cooldown and the save-load options that do nothing). |

The sections below are the script's own guide, from livesplit-asl's CLAUDE.md. The script's header comment links a community spreadsheet of addresses.

## GTASA.asl

A fork of Tduva's GTASA.asl, in the middle of a rewrite. Keep the two halves apart:

- **Live code**: `startup` up to `#region OLD_SHIT`, `init` up to its bare `return;`, and `split` up to its `return false;`.
- **Dead reference code**: everything after those early returns (`#region OLD_SHIT`, the "old shit" tail of `init`, `#region Splits OLDDDDD`). It is Tduva's original logic kept to be ported mission by mission. It never runs, and things it references (`vars.Passed`, the "NonLinear …" setting) don't exist in the live code. Port from it; don't call into it.

Porting proceeds in story order (commit messages like "Through Doberman"). Los Santos and Badlands are ported; San Fierro, Desert, Las Venturas and Return to Los Santos exist only as settings headers.

### How a mission is defined

Settings and split logic for a mission live together in one nested `#region` in `startup` (city → chain → mission), deliberately, so a mission can be edited without jumping between `startup` and `init`. Each mission block:

1. **Registers settings**: the mission (`abbr`), then children `abbr_marker`, `abbr_start`, any mid-mission events, and `abbr_pass`. `vars.RegisterMissionBasics(abbr, "Mission Name", "threadname", "ParentSetting")` is the newer shorthand that does the first three plus the thread and marker-name registration below. **A setting ID is also the split ID**: `TrySplit(id)` checks `settings[id]`.
2. **Registers memory to watch** (addresses only; the watchers themselves are built in `init`):
   - `vars.watchScmGlobalVariables.Add(448, "ls_intro_chain")` — SCM global `$448`; the name becomes the watcher name. Put the decompiled `$NAME` in a trailing comment, as existing lines do.
   - `vars.watchScmMissionLocalVariables.Add(46)` — mission-local `46@`; the watcher is named `"46@"`. Mission locals are one shared block reused by every mission, so only read them while your mission is the active one.
   - `vars.AddNonScmAddressWatcher(addr, name, bytes)` and `vars.AddPointerWatcher(deepPointer, name, bytes)` for everything else (stats, pointers); `bytes` is 1, 2 or 4.
   - `vars.significantThreads.Add("sweet1", "tut_start")` — split when this script thread becomes the current one; also sets `vars.lastStartedMission`. An empty split ID means track the thread without splitting.
   - `vars.missionNames.Add("Tagging up Turf", "tut_marker")` — split when the yellow mission-name text appears (marker entered).
3. **Adds a `Func<string> func_abbr` to `vars.CheckSplit`**. It returns a split ID when a condition fires, otherwise `null` (`return;`). Typical shape: bail out unless `vars.ValidateMissionProgress(thread, chainWatcher, indexBefore, indexAfter, "abbr_pass", "abbr")` returns true (it fires the `_pass` split itself when the chain's `$..._TOTAL_PASSED_MISSIONS` counter advances, and returns true only while the mission is active and unpassed), then compare `.Changed` / `.Old` / `.Current` on watchers fetched with `vars.GetWatcher(name)`.

Dialogue-block and sub-phase locals are the usual mid-mission signals. Keep the value → meaning table as a comment next to the check, as existing blocks do.

### Runtime flow

- `init` accepts only game version 1.0 (two version bytes in the exe, or the module size while the CD check still runs) and sets `vars.enabled = false` for anything else, which makes `update` return false and disables everything. It builds `vars.watcherList` from the lists recorded in `startup`. All addresses in the script are module-relative 1.0 addresses. SCM globals live at `0x649960 + n*4`, mission locals at `0x648960 + n*4`. The two `state` blocks (`gta_sa` and `gta-sa`) exist for the two executable spellings and must stay identical.
- `vars.GetWatcher(name)` updates a watcher at most once per tick (`vars.currentWatchers` is cleared in `update`). Don't call `watcherList.UpdateAll`: the lazy per-watcher update was the "Performance boost" commit, and most watchers are per-collectible slots that are rarely read.
- `split` runs each tick: split-prevention guards (main menu, just loaded a save) → thread change → `significantThreads` → `missionStartText` change → `missionNames` → every `CheckSplit` func → dequeue **one** split from `vars.splitQueue`. Simultaneous splits are therefore delivered on consecutive ticks.
- `vars.TrySplit(id)` splits each ID once per timer reset (`vars.completedSplits`, cleared in `onReset`), skips IDs disabled in settings, applies the optional double-split cooldown, and enqueues. With `vars.skipSplits` set, queued splits become `SkipSplit()` calls instead.
- Watchers that always exist: `thread` (name of the most recently started script), `missionStartText`, `oddMissionStartText`, `playingTime`, `wantedLevel`, `interior`, `intro_passed`, `intro_newGameStarted`, `pedStatus`, `weaponSlotMelee`.
- `settings[...]` can't be read directly inside a `Func` defined in `startup`; use `vars.CheckSetting(name)`.
- Collectibles (horseshoes, snapshots, oysters) move in memory. `func_collectibles` detects stale addresses, finds the new block with `vars.ScanForAddress` (a signature scan) and re-registers through `vars.ChangeNonScmAddressWatcher`, which `update` applies via `vars.ChangeAddressWatchers`.

### Finding SCM variables and thread names

Two large reference dumps sit in this folder, ignored by git through the repo's `.gitignore` (over 10 MB each; grep them, never read them whole; **never commit or push them: the leaked source is not ours to publish**):

- `main.scm.txt` — decompiled `main.scm` (Sanny Builder style, SASCM.ini of 2021-07-05). Named globals appear as `$INTRO_TOTAL_PASSED_MISSIONS`, unnamed ones as `$5353`, locals as `46@`, threads as `script_name 'INTRO1'` under a `:INTRO1` label. The number behind a named global is not in this file; the script's comments pair them (e.g. `448 // $INTRO_TOTAL_PASSED_MISSIONS`).
- `scm_leak.txt` — leaked original PC source of the same script, with the original variable names (`VAR_INT intro_bmx …`, `LVAR_INT …`, `SCRIPT_NAME intro1`, `MISSION_START`). Good for working out what a variable means; its names differ from the decompile. A mission's locals are numbered in declaration order plus 34: its `LVAR_INT`/`LVAR_FLOAT` names, read in order, are `34@`, `35@`, … (`0@`–`31@` are reserved, `TIMERA`/`TIMERB` are `32@`/`33@`; checked on Tagging up Turf, where `48@` is `sweet1_cell_index_end` and `58@` is `help_for_tagging`). Globals do not follow declaration order; use the sheet below for them.
- The Google Sheet linked in the header of `GTASA.asl` ("GTASA Global Variable Table 2022") is the bridge between the two dumps: its "Variable Table" tab maps each leaked global name to its decompile ("Community") name and its `$n` on every platform, and the "PC_1.00" tab lists that version alone with array sizes. Arrays appear once, as their first element, so a `$n` with no row is an element of the nearest lower array head. Only PC 1.0 is supported; ignore the other version columns. Export a tab as CSV with `https://docs.google.com/spreadsheets/d/<id>/export?format=csv&gid=<gid>`, or the whole book with `format=xlsx`.

The header comment of `GTASA.asl` also links a community spreadsheet of addresses.
