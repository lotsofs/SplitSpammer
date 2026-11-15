# Component

Detail for the component rows of [TODO.md](TODO.md).

Dependencies: TST comes first, so each later item is built with tests, and its comparison harness is what every mission port uses ([porting.md](porting.md)). The missions themselves are items of their own, grouped by part of the game in the index, after the groundwork in [Groundwork.md](Groundwork.md). The shared design facts are in docs/architecture.md.

Everything here is read, not reproduced. Facts about the ASL come from `reference\GTASA.asl` (see `reference\README.md`), read 2026-11-14, and about how a component is built from `D:\Repositories\LiveSplit.OutputToFile`.

## TST — Headless test project

There is no test project. OutputToFile drives its component headless through LiveSplit's `TimerModel` in an SDK-style net481 xunit project with its own `TestRun` helper, and packages that restore offline from the NuGet cache.

**Evidence:** read 2026-11-14 in OutputToFile's `Tests\` and docs/testing.md.

**Plan:** `Tests\LiveSplit.SplitSpammer.Tests.csproj`, SDK-style, net481, xunit 2.9.2 with the test SDK and runner, referencing the component project and, copied into its own output, `LiveSplit.Core.dll` and `UpdateManager.dll`. A `TestRun` helper builds a run and a `LiveSplitState`, creates the component with a fake `IGameReader`, plays timer actions through `TimerModel`, and ticks the poll step by hand instead of running its thread. Add tests for `SettingsState` and the settings XML (defaults, effective only with ancestors checked, branch check, uncheck and reset, the snapshot being a copy, the XML round trip with an unknown id, a bad value and an old layout). Add tests for `SplitEngine`, `WatcherSet` and `EnginePoller` with a fake `IGameReader` (two splits at once come one per tick, once per reset, a disabled id remembered after enabling, `Reset()` clearing the queue, each guard, a watcher read once per tick with Old kept, two names on one address sharing Current, Old and one read a tick, a repeated add doing nothing, a name reused for another address or an address added with another size throwing, a failed read keeping the value, a throwing check not stopping the others, the poller splitting and resetting and ending on `Dispose()`). Move the scratch comparison of In the Beginning with the ASL (the ASL's guards and `func_intro` run on LiveSplit.Core's real `MemoryWatcher<int>` against the engine and module, on the same fake memory, scripted and seeded random runs) into a test, with the ASL's SHA-256 beside it. Shape it as a harness each mission port adds to: the ASL's side gets the runtime pieces the ports need as the groundwork items add them to the engine (the thread and marker steps, `ValidateMissionProgress`, one watcher per address, LiveSplit's `StringWatcher`), so a port only transcribes its own func and writes its runs. Add `docs\testing.md` in the style of OutputToFile's.

**Verify:** `dotnet test` builds and runs a first test that adds the component, ticks it and checks nothing throws, and the In the Beginning comparison passes in it.

**Docs when done:** CLAUDE.md (test command), docs/testing.md, porting.md (step 5, how to add a mission's comparison).

## GSH — Split at the start of the "Grove Street - Home" lines

The `intro_groveStreetLinePlayed` split (label "\"Grove Street - Home\" line played", in `Missions\InTheBeginning.cs`) fires when `$54` (`voice_over_at_hub`) becomes 1. The game's script sets that only after the first line ("Grove Street - Home.") has finished playing and the second line ("At least it was before I fucked everything up.") starts, so the split is about one line late and the label is wrong about when it lands. The user wants it to fire as the first line starts. It waits until the other systems are in place: it depends on TST and on THR's string watchers.

**Evidence:** read 2026-11-14 in `reference\scm_leak.txt` (`intro_stuff_loop`, `SCRIPT_NAME INTROST`) and `reference\main.scm.txt` (`:INTROST_...`). The thread's flow: with the player within 90 units of CJ's house and no mission running, it loads the two audio lines, shows line 1 with `PRINT_NOW VO_AF` and plays it, waits for it to end, shows line 2 with `PRINT_NOW VO_AG` and plays it, then sets `voice_over_at_hub = 1`. The thread has no local variables of its own, so there is no flag for the start of line 1. The ASL does the same as the component here.

**Plan:** use THR's string watcher. Find the address of the on-screen subtitle buffer (not known from the ASL; expected near its mission-text addresses `0x7AAD40` and `0x7AAEC0`, to be confirmed by reading the running game's memory while the line is shown). Split when that text becomes "Grove Street - Home."; keep `$54` as a fallback only if the text proves unreliable. Rename the setting's label to say it is the first line starting. If the subtitle route fails, the alternatives are accepting the lag and relabelling the split as the second line, or using `intro_passed` (`$24`, set just before this thread can run) as a rough proxy.

**Verify:** by hand in LiveSplit: the split lands as the first line starts, once per reset; the ASL comparison still passes for the other intro splits (this one deliberately diverges from the ASL, so its ASL comparison is dropped for this id).

**Docs when done:** README (the setting), CHANGELOG.md Unreleased (the split now lands at the first line, and the label).

## SLD — Start and reset the timer when loading a save

The ASL has the settings "Start timer when loading save" and "Reset timer when loading save", but in the live script they do nothing (its items SVL and LDG: the `loading` watcher was never registered), so they are not ported. The recent-update ASL (`reference\GTASA_orig_recentupdate.asl`) made the start one work: it watches the byte at `+0x7A67A5` (the ASL's `loadingAddr`, true only while a save is being loaded, not during in-game loading screens) and starts when it goes from true to false.

**Evidence:** read 2026-11-14 in both scripts' `start` blocks and the ASL repo's `Defects.md`. The byte's behaviour is not reproduced here.

**Plan:** add `timer_startOnSaveLoad` and `timer_resetOnSaveLoad` (off) to the Timer group, a 1-byte watcher on `+0x7A67A5`, and start and reset checks on its true-to-false edge. Log the byte on the running game first, to confirm when it is set: a save load, a new game, in-game loading screens.

**Verify:** by hand: loading a save starts (and resets) the timer with the options on and does nothing with them off; a new game and in-game loading screens do not trigger them.

**Docs when done:** README, CHANGELOG.md Unreleased, docs/architecture.md (game memory).

## LRM — Load removal

GTA SA is timed in real time and the ASL has no `isLoading` block, so the component does not pause game time. If a loadless category ever wants it, LiveSplit's game time can be paused through `LiveSplitState.IsGameTimePaused` and set with `SetGameTime`, posted to the UI thread like the other timer actions.

**Evidence:** read 2026-11-14: neither ASL has an `isLoading` or `gameTime` block.

**Plan:** only on request: find a reliable loading flag (the `loading` byte of SLD covers save loads only), pause game time while it is set, and post the pause and unpause to the UI thread.

**Verify:** game time stops during loads and real time does not.

**Docs when done:** README, CHANGELOG.md Unreleased.
