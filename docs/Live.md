# Live checks

Detail for the LIV row of [TODO.md](TODO.md): things built and checked headless, but not yet seen working in LiveSplit on the running game. Each check keeps its number; a check that passes is deleted and logged in [DONE.md](DONE.md) by its number (`LIV 2`). Capture the trace with DebugView (Include filter `*[SplitSpammer]*`): it logs every thread and marker text change and every split, or why one was held back.

## LIV — Seen working on the running game

### Playing a story mission

1. **Ryder.** The first marker entry: on 2026-11-19, straight after Big Smoke, "Mission marker entered" didn't split on the first attempt, because no `"Ryder"` marker text appeared in memory (the thread went from `int` to `intro2`, 4 s after Ryder's marker appeared, with no `missionStartText` change); after a fail the text showed for 0.3 s and both marker splits fired. Find out whether the game shows the title on that first entry (if not, the split can't come from the text there, in the ASL either), and if it doesn't, pick another signal for it. Also still to see: nothing splits after loading an earlier save, or when disabled. Reproduced 2026-11-19 for the rest.

### Playing a side mission

2. **Firefighter.** "Firetruck entered": getting into a Firetruck splits once, the ladder truck doesn't; with "...while not on a mission" or "...before passing Ryder" checked, getting in on a mission or after Ryder is passed splits nothing, and getting in again where the options hold splits (outside Northeast Los Santos held back, reproduced 2026-11-19). The start with "...in Northeast Los Santos": a first start elsewhere logs `split held back`, and a later start in the area splits (a first start inside split, reproduced 2026-11-19). A fail in the middle of a level and a retry split no fire, car or level twice, and loading a save after a fail splits nothing (a cancel at the first start and a restart after the pass split nothing, reproduced 2026-11-19). Checked headless (FFT 2026-11-18, the rest 2026-11-19).
