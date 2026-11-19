# Live checks

Detail for the LIV row of [TODO.md](TODO.md): things built and checked headless, but not yet seen working in LiveSplit on the running game. Each check keeps its number; a check that passes is deleted and logged in [DONE.md](DONE.md) by its number (`LIV 2`). Capture the trace with DebugView (Include filter `*[SplitSpammer]*`): it logs every thread and marker text change and every split, or why one was held back.

## LIV — Seen working on the running game

### Playing a story mission

1. **Ryder.** Every Ryder split enabled, the five the ASL lacks included: each fires once, at the moment its label says (the marker, the start, getting in Ryder's car, the barbershop entered, the haircut, the barbershop left, the end of the cutscene outside, the pizza restaurant entered, the menu, the pizza bought, the restaurant left, back in the car, arriving at Ryder's house, the pass). Then a fail (blowing up the car, say) and a retry: the fail splits, the marker entered after failing and the restart split, and nothing splits twice. Not after loading an earlier save, and not when disabled. Read in the code and checked headless (RYD, 2026-11-18); not reproduced.

### Playing a side mission

2. **Firefighter.** Every Firefighter split enabled: the trace shows the thread as `"firetru"` when the mission starts, and the start splits only the first time in a run. Each fire splits as it goes out, and each level as its last fire goes out. If you play it to the end: finishing level 12 splits level 12, and the pass on the next tick, with the last started mission cleared after it. A fail and a restart split no fire or level twice, and loading a save after a fail splits nothing. Read in the game's script and checked headless (FFT, 2026-11-18); not reproduced.
