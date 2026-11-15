# Architecture

How the component works internally. Code is referred to by member name. Open work is in [TODO.md](TODO.md).

## Files

| File | Role |
|---|---|
| `UI/Components/SplitSpammerFactory.cs` | Name, description and category (Control) shown in the Layout Editor. `Create(state)` builds the component. |
| `UI/Components/SplitSpammerComponent.cs` | The LiveSplit side. Owns the reader, the engine, the poller and a `TimerModel`; counts timer resets; shows the game status. Draws nothing. |
| `UI/Components/SplitSpammerSettings.cs` | The settings panel: game status line and the settings tree, and the settings' XML. |
| `UI/Components/Settings/` | `SettingsTree` (the nodes, built by `Build`), `SettingsState` (checked states, effectiveness), `SettingsSnapshot` (an immutable copy of what is in effect). |
| `UI/Components/Game/` | `IGameReader`, `GameVersion` (the PC 1.0 check) and `ProcessGameReader` (ReadProcessMemory). |
| `UI/Components/Modules.cs` | `IModule` and `Modules.All`: the timer control first, then the missions in story order. |
| `UI/Components/Missions/` | One file per mission with its settings and its check, as the ASL's regions do. |
| `UI/Components/Engine/` | `WatcherSet` (lazy int and string watchers, at a `MemoryLocation` that can follow pointers), `RunningScripts` (every script in the game's active list), `SplitEngine` (the tick: reset, split and start checks, guards, thread and marker steps, once-per-reset, queue, cooldown), `EnginePoller` (the thread) and `TimerControl` (start and reset on a new game, the Timer settings). |
| `Preview/` | A stand-alone window that shows the settings panel; run with `deploy -p`. Not part of the component. |

## Lifecycle

1. LiveSplit calls `SplitSpammerFactory.Create(state)` on the UI thread. The component captures that thread's `SynchronizationContext`, builds the settings, the engine and the poller, subscribes to `state.OnReset` and starts the poll thread.
2. LiveSplit calls `SetSettings(xml)`; that updates the settings state, and a changed settings event publishes a new `SettingsSnapshot`.
3. `Update()` runs every frame on the UI thread. It only copies the reader's status to the settings label when that changed.
4. `Dispose()` unsubscribes, stops the poller (it waits up to a second for the thread), then disposes the reader and the settings.

## One engine tick

`EnginePoller` runs this about every 33 ms on its own thread:

1. About once a second, `reader.Refresh()` (attach, version check, detach when the game exited).
2. If the reset counter changed, `engine.Reset()` clears the set of splits already done, the queue and the last started mission.
3. `engine.Tick(phase)`: starts a new tick for the watchers; returns no action unless the game is Found. While the timer is Running or Paused: the reset checks first, and if one fires the engine clears its done set and queue, asks for Reset and skips the split checks this tick; otherwise the guards (nothing splits while `para_pickup_flag` and `game_starts_from_scratch` are both 0, or while playing time is below 600; the queue waits), the thread step (when `thread` changed to a registered thread other than the last started mission, split its id if it has one and make it `LastStartedMission`; then read the whole script list, `RunningScripts`), the marker step (when `missionStartText` changed to a registered mission name, split its id), every split check in the order it was added, each in try/catch, then, if the list was read well and the last started mission is not in it, clear `LastStartedMission` (not the ASL: so a mission's checks stop when it ends; after the checks, so they still split a pass in the tick the mission ends), then one queued split, if any. Then, if the timer is NotRunning, or was just reset: the start checks; true asks for Start. Nothing once the run has Ended. This is the ASL runtime's order, so a reset and the start land in the same tick and the lazy reads match the ASL's.
4. The poller hands the actions to the component's `ActOnUiThread`, which posts them to the UI thread and runs `TimerModel.Reset()`, `Start()` and `Split()` in that order. The phase comes from `State.CurrentPhase`, read on the poll thread; a value a frame old is harmless, because every condition is an edge and `TimerModel` ignores a Start while running.

Modules plug in through `IModule`: `AddSettings(tree)` is called when `SettingsTree.Build` runs, and `AddChecks(engine)` when the component creates its engine. A split check returns a split id or null; it reads watchers in the ASL's order and returns early as the ASL does, because the early returns decide which watchers are read that tick. `TimerControl` is the first module: it adds the Timer settings and the start and reset checks. A new game is `game_starts_from_scratch` at 0, playing time below 60 s, and playing time leaving 0 or dropping (it is zeroed when New Game is confirmed and counts once loading has ended; the 0 can fall between two ticks). `Changed` is false on a watcher's first read, so attaching to a game already in its first minute starts nothing; a loaded save brings a larger playing time with the intro passed and never matches.

Rules the engine keeps from the ASL:
- A watcher is 1, 2 or 4 bytes (`WatcherSet.Add(name, offset, bytes)`), the low bytes of a 4-byte read. There is one watcher per address, under any number of names, as in the ASL: each module registers every watcher it reads, an address already watched with the same size gives the same watcher (so the names share Current and Old), adding a name again for the same address does nothing, and a name used for another address or an address with another size throws. A watcher is read at most once per tick, whichever of its names asks, when a check asks for it. The guards read all three of their watchers before testing any, as the ASL does. `IntWatcher` copies the real `MemoryWatcher<int>`: the first read counts as unchanged and leaves Old at 0; a failed read leaves Current alone and sets Old to it. The reads decide what `Old` is, so a watcher that is read only while a mission is active compares against its last read. A failed read keeps the last value.
- A string watcher (`WatcherSet.AddString(name, location, bytes)`) copies the real `StringWatcher` with AutoDetect, checked against it on 2026-11-15: the bytes are UTF-16 when the second is 0 and UTF-8 otherwise, cut at the first null; the first read counts as unchanged and leaves Old null; a failed read, a null pointer included, leaves both Old and Current alone (unlike `MemoryWatcher<int>`). Its `MemoryLocation` follows pointers as `DeepPointer` does for a 32-bit game: the module offset holds a 4-byte pointer, every offset but the last leads to the next pointer, the last is added, and a null pointer fails the read. The same sharing rules apply, per location and size.
- `RunningScripts` reads both list heads and the whole script array in one read and walks both lists, checking that each pointer is a record in the array, that each record's prev is the record before it, and that every record is in exactly one of the two lists. Anything else is a torn read and keeps the previous names. On the running game (2026-11-15), reading the head and the array separately let the walk run into the idle list 8 times in about 420,000 reads at 1 ms. That happens when the first script ends between the two reads. The one read and the partition check are there because of that. In 3000 reads of the fixed walk none was rejected.
- The thread and marker steps log every change of `thread` and `missionStartText`, the script list logs the scripts that start and end, and the engine logs the last started mission ending, all with `Log.Info`, which goes to Trace (DebugView) and not to the event log.
- `TrySplit(id)` lets a split through once per timer reset. A split disabled in the settings is remembered as done too, and so is one blocked by the optional cooldown (`timer_doubleSplitPrevention`: no split within 2.5 s of the previous queued one; the ASL's intent, its comparison was inverted). The setting id is the split id, and it counts only when it and all its ancestors are checked (`SettingsSnapshot.IsEnabled`).
- One split per tick, so splits that happen together reach the timer on consecutive ticks.

## Threads

- **UI thread:** LiveSplit's calls, the settings panel, `Update()`, and `TimerModel`'s `Reset()`, `Start()` and `Split()`.
- **Poll thread:** the reader, the watchers, the engine. Nothing else touches them.
- **Any thread:** LiveSplit's timer events (its server raises them on a connection thread). The handler only does `Interlocked.Increment` on the reset counter; a handler that throws there would close LiveSplit.
- **Between threads:** `SplitSpammerComponent.Snapshot` is a `volatile` reference to an immutable object, replaced whole on each settings change; `ProcessGameReader.Status` is a `volatile` field.
- Every poll step is caught and logged with `Log.Error`; nothing may throw into LiveSplit.

## Game memory

Module-relative addresses of PC 1.0 (absolute = `+0x400000`). SCM globals are at `+0x649960 + n*4`, mission locals at `+0x648960 + n*4`; their names come from the leaked script (`reference\README.md`).

| Address | Name | What it is |
|---|---|---|
| `+0x4245BC`, `+0x42457C` | version check | 38079 in a 1.0 exe (EU and US builds); not there yet while the game checks the CD, hence the module size 18313216 as well |
| `[+0x68B42C]+0x8` | `thread` | 10 bytes: the name of the first script in the game's active list (`CTheScripts::pActiveScripts`, a script's name 8 bytes into its record), usually the one started last; the ASL's `DeepPointer(0x68B42C, 0x8)`. Lower case, nothing after the name; ambient scripts (`noname`, `dealer`, …) show up too (seen 2026-11-15) |
| `+0x68B428`, `+0x68B42C`, `+0x68B430` | script lists | `CTheScripts::pIdleScripts`, `pActiveScripts` and `ScriptsArray`, one after the other: the two list heads, then 96 records of `0xE0` bytes (next at `+0`, prev at `+4`, name at `+8`). Every record is in one of the two lists; a script that ends goes to the front of the idle list and keeps its name (seen 2026-11-15) |
| `+0x7AAD40` | `missionStartText` | 32 bytes: the yellow mission name shown when a mission marker is entered, and when a mission shows a new title ("Big Smoke", then "Sweet & Kendl" 3 s into the mission); "" in between (seen 2026-11-15) |
| `+0x7AAEC0` | `oddMissionStartText` | 32 bytes: the same for odd jobs (Trucking) |
| `+0x77CB84` | `playingTime` | the playing-time stat, in ms; 0 at a new game |
| `+0x77CE50` | money | `CPlayerInfo` money; not used by the component, but a handy marker in logs (350 after the intro cutscenes) |
| `+0x6B3E1C` | `unknownAB3E1C` | **not identified.** A byte that is 0 from a new game through every intro scene and its natural end, and 1 within a frame of skipping a scripted scene (found by diffing memory across skips on the running game, 2026-11-14: four skips and two unskipped runs). It toggles again later in gameplay (seen while riding the bike), so it is only used while `$5353` is 2 or 3, and once-per-reset makes a later toggle harmless. If it is ever identified, rename it. |

Bytes tried and rejected for the skip, for the record (same day): `+0x76092C` drops to 0 at a skip but also mid-scene when CJ is thrown out of the car and while riding; `+0x7AA472`, `+0x87C6D8`, `+0x87DDA9` mark the end of the cutscene tail, 0.8 s after an early skip and 3 s after a late one; the money comes 0.5 to 2 s after a skip.
