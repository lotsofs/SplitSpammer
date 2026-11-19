# Live checks

Detail for the LIV row of [TODO.md](TODO.md): things built and checked headless, but not yet seen working in LiveSplit on the running game. Each check keeps its number; a check that passes is deleted and logged in [DONE.md](DONE.md) by its number (`LIV 2`). Capture the trace with DebugView (Include filter `*[SplitSpammer]*`): it logs every marker text change, the last started mission ending and every split, or why one was held back. Changes of `thread` and the script list are not logged (they flooded the trace); to see them, run a probe of the game's memory beside it.

## LIV — Seen working on the running game

### Playing a story mission

1. **Ryder.** Entering the marker with the replay trick: on 2026-11-19 the trick hid the title on the first attempt (no `missionStartText` change, the thread 62 ms after `$409` rose) and "Mission marker entered" never split; the marker now also counts as entered as the thread starts. Enter Big Smoke's and Ryder's markers with the trick, and Ryder's again after a fail: each splits "Mission marker entered" once, as the mission starts, with `marker text not seen; marker entered from $ONMISSION` in the trace when the title was hidden, and "...after failing" and "Restarted" on the retry; four saves in a row in between split nothing. (On 2026-11-19 at 18:24 the title showed on both entries, so both came from the text.) Also still to see: nothing splits in Ryder after loading an earlier save, or when disabled. Checked headless 2026-11-19; the rest of Ryder reproduced 2026-11-19.

### Playing a side mission

2. **Firefighter.** "Firetruck entered": getting into a Firetruck splits once, the ladder truck doesn't; with "...while not on a mission" or "...before passing Ryder" checked, getting in on a mission or after Ryder is passed splits nothing, and getting in again where the options hold splits; with "...in Northeast Los Santos", getting in at the spot just south of CHC4A held back on 2026-11-19 (y −1120) now splits, as does anywhere up to y −1297. The start with "...in Northeast Los Santos": a first start elsewhere logs `split held back`, and a later start in the area splits (a first start inside split, reproduced 2026-11-19). A fail in the middle of a level and a retry split no fire, car or level twice, and loading a save after a fail splits nothing (a cancel at the first start and a restart after the pass split nothing, reproduced 2026-11-19). Checked headless (FFT 2026-11-18, the rest 2026-11-19).
