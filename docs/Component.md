# Component

Detail for the component rows of [TODO.md](TODO.md).

Dependencies: TST comes first, so each later item is built with tests. ENG needs the game reader (`IGameReader`) and the settings snapshot (`SettingsSnapshot`). ITB is the first real mission and needs ENG. STT and MIS build on ITB. The shared design facts are in the sections where they matter: the threading rules in ENG, the settings semantics in STR, the reading and version check in RDR.

Everything here is read, not reproduced: the repo has no code yet. Facts about the ASL come from `reference\GTASA.asl` (see `reference\README.md`), read 2026-11-14, and about how a component is built from `D:\Repositories\LiveSplit.OutputToFile`.

## TST — Headless test project

There is no test project. OutputToFile drives its component headless through LiveSplit's `TimerModel` in an SDK-style net481 xunit project with its own `TestRun` helper, and packages that restore offline from the NuGet cache.

**Evidence:** read 2026-11-14 in OutputToFile's `Tests\` and docs/testing.md.

**Plan:** `Tests\LiveSplit.SplitSpammer.Tests.csproj`, SDK-style, net481, xunit 2.9.2 with the test SDK and runner, referencing the component project and, copied into its own output, `LiveSplit.Core.dll` and `UpdateManager.dll`. A `TestRun` helper builds a run and a `LiveSplitState`, creates the component with a fake `IGameReader`, plays timer actions through `TimerModel`, and ticks the poll step by hand instead of running its thread. Add tests for `SettingsState` and the settings XML (defaults, effective only with ancestors checked, branch check, uncheck and reset, the snapshot being a copy, the XML round trip with an unknown id, a bad value and an old layout). Add `docs\testing.md` in the style of OutputToFile's.

**Verify:** `dotnet test` builds and runs a first test that adds the component, ticks it and checks nothing throws.

**Docs when done:** CLAUDE.md (test command), docs/testing.md.

## ENG — Split engine and poll thread

The ASL runs its checks on a script thread at 30 Hz. Its watchers update lazily, at most once per tick, only when a check reads them, and that decides what the Old value of a watcher is. Splits fire once per timer reset, one per tick.

**Evidence:** read 2026-11-14 in `GTASA.asl`: `GetWatcher`, `split`, `TrySplit` and the split queue. The lazy reads are why a watcher read only while a mission is active compares against its last read.

**Plan:**
- A poll thread at 30 Hz: attach, read, run the engine, and post `TimerModel.Split()` to the UI thread through the captured `SynchronizationContext`.
- Per tick: lazy watchers like `GetWatcher` (kept exact, because the test fixtures are recorded from the ASL); the guards (nothing splits while the new-game flag and the intro-passed flag are both 0, or while playing time is below 600); once-per-reset ids, where a disabled id is remembered too; one queued split delivered per tick.
- Threads, as in OutputToFile: LiveSplit's event handlers can run on another thread (its server raises them on a connection thread), so they only count with `Interlocked`; the poll thread acts on the count, for example clearing what already split after a reset. Every poll step is wrapped so an exception is logged and can't reach LiveSplit (a handler that throws on the server's thread closes it). `Update()` stays cheap. `Dispose()` stops the thread and unsubscribes.

**Verify:** tests with a fake reader and a fake clock for one split per tick, once per reset, disabled ids remembered, a reset clearing the done set, the guards, and a failing reader; traces from the real ASL (ITB).

**Docs when done:** CLAUDE.md (thread rules), docs/architecture.md describing the flow.

## ITB — In the Beginning, the first mission

The first mission slice, with its ASL behaviour. Settings under Splits > Story Missions > Los Santos > Intro: `intro` ("In the Beginning", off) with children `intro_cutsceneEnd`, `intro_passed` ("Bicycle entered or abandoned"), `intro_groveStreet`, all off. Watchers (module-relative): `playingTime` at `0x77CB84`; SCM globals `$5353` (cutscene state), `$24` (intro passed), `$54` (the "Grove Street - Home" line shown), `$1510` (new game started). The check: the cutscene state changed and playing time is above 2000, and the state is now 1 or went from 3 to 0: `intro_cutsceneEnd`; `$24` changed to 1: `intro_passed`, which fires when the player enters the bike, moves it or leaves the starting area; `$54` changed to 1: `intro_groveStreet`. Reads are lazy: `$24` and `$54` are only read if the earlier checks did not return.

**Evidence:** read 2026-11-14 in `GTASA.asl`: the "In the Beginning" region and the guards at the top of `split`.

**Plan:** one `Missions/InTheBeginning.cs` that holds the settings and the check together, as the ASL's regions do. Record traces from the real `reference\GTASA.asl` (the local copy; note its SHA-256 beside the fixtures): a scratch recorder (kept in `Tests\Recorder\`) compiles its `startup` and `split` blocks with stubs for settings (all on), a fake `GetWatcher` and the split queue, restricted to this mission's func, and records per tick the five memory values and the id `split` delivers, for a scripted playthrough (menu, loading, cutscene, bike, leaving the area, the Grove Street line, a restart) and two seeded random runs. Commit them in `Tests\Fixtures\` with the ASL commit they came from. A test replays them through the engine and requires the same id on the same tick.

**Verify:** the replay tests; a mutation check that breaking one guard fails them; by hand in LiveSplit: start the timer, play the start of a new game, and see the three splits once each, and again only after a timer reset.

**Docs when done:** README (the settings), CHANGELOG.md Unreleased, docs/testing.md (the recorder).

## STT — Start, reset, save-load options and load removal

The ASL starts the timer on a new game, from the playing-time counter and the intro cutscene state, with the option to start when loading finishes, and options for starting and resetting when loading a save. In the ASL the two save-load options are inert since the rewrite, and the after-load guard never arms (see the ASL repo's items SVL and LDG).

**Evidence:** read 2026-11-14 in the `start`, `reset` and `isLoading` blocks of `GTASA.asl`.

**Plan:** port the new-game start and reset conditions, and decide with the user whether the save-load options come back; load removal, if wanted, through `LiveSplitState.IsGameTimePaused` and `SetGameTime`. Record traces from the ASL's `start` block the way ITB does.

**Verify:** replay tests; by hand on a new game, a loaded save and a restart from the pause menu.

**Docs when done:** README, CHANGELOG.md Unreleased.

## MIS — Port the remaining missions

The rest of the ASL's live missions (Los Santos and Badlands, side missions, collectibles, gang territories, warps, phone calls) and what the ASL itself has not ported yet (the lists in `reference\Porting.md`: San Fierro, Desert, Las Venturas, Return to Los Santos, vehicle submissions and so on).

**Evidence:** read 2026-11-14 in `GTASA.asl` and its docs.

**Plan:** one file per mission or side-mission group under `Missions/`, holding its settings and checks together, ported in story order. Because the component is plain C#, a mission's checks can be written as small tables or helpers instead of the ASL's long funcs, as long as the replayed ASL traces still match. Missions that exist only as ASL settings headers are ported from the dead code and the SCM dumps in the ASL repo, never by copying the dumps here.

**Verify:** replay tests per mission against the ASL where the ASL has the logic; by hand otherwise.

**Docs when done:** README (settings), CHANGELOG.md Unreleased.
