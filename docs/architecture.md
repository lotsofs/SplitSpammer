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
| `UI/Components/Engine/` | `WatcherSet` (lazy watchers), `SplitEngine` (guards, checks, once-per-reset, queue) and `EnginePoller` (the thread). |
| `Preview/` | A stand-alone window that shows the settings panel; run with `deploy -p`. Not part of the component. |

## Lifecycle

1. LiveSplit calls `SplitSpammerFactory.Create(state)` on the UI thread. The component captures that thread's `SynchronizationContext`, builds the settings, the engine and the poller, subscribes to `state.OnReset` and starts the poll thread.
2. LiveSplit calls `SetSettings(xml)`; that updates the settings state, and a changed settings event publishes a new `SettingsSnapshot`.
3. `Update()` runs every frame on the UI thread. It only copies the reader's status to the settings label when that changed.
4. `Dispose()` unsubscribes, stops the poller (it waits up to a second for the thread), then disposes the reader and the settings.

## One engine tick

`EnginePoller` runs this about every 33 ms on its own thread:

1. About once a second, `reader.Refresh()` (attach, version check, detach when the game exited).
2. If the reset counter changed, `engine.Reset()` clears the set of splits already done and the queue.
3. `engine.Tick()`: starts a new tick for the watchers; returns false unless the game is Found; the guards (nothing splits while the new-game flag and the intro-passed flag are both 0, or while playing time is below 600; the queue waits); every check in the order it was added, each in try/catch; then one queued split, if any.
4. If the tick returned true, the poller calls the component's `SplitOnUiThread`, which posts `TimerModel.Split()` to the UI thread. `Split()` ignores any phase but Running.

Rules the engine keeps from the ASL:
- A watcher is read at most once per tick, when a check asks for it. The reads decide what `Old` is, so a watcher that is read only while a mission is active compares against its last read. A failed read keeps the last value.
- `TrySplit(id)` lets a split through once per timer reset. A split disabled in the settings is remembered as done too. The setting id is the split id, and it counts only when it and all its ancestors are checked (`SettingsSnapshot.IsEnabled`).
- One split per tick, so splits that happen together reach the timer on consecutive ticks.

## Threads

- **UI thread:** LiveSplit's calls, the settings panel, `Update()`, `TimerModel.Split()`.
- **Poll thread:** the reader, the watchers, the engine. Nothing else touches them.
- **Any thread:** LiveSplit's timer events (its server raises them on a connection thread). The handler only does `Interlocked.Increment` on the reset counter; a handler that throws there would close LiveSplit.
- **Between threads:** `SplitSpammerComponent.Snapshot` is a `volatile` reference to an immutable object, replaced whole on each settings change; `ProcessGameReader.Status` is a `volatile` field.
- Every poll step is caught and logged with `Log.Error`; nothing may throw into LiveSplit.
