# Done

Finished [TODO](TODO.md) items, newest first. A code can be reused once its item is finished, so it may appear here more than once; the date tells them apart. An item's detail section (evidence, plan) is deleted when it is finished; it is in git history: `git log -S "## CODE " -- docs/`.

"Change" is one line, usable as a commit message. "Why / how" is one or two short sentences: the cause and the fix, then how it was verified.

| Date | Code | Area | Change | Why / how |
|---|---|---|---|---|
| 2026-11-14 | — | Project | Add `reference\` with the GTA SA script, the SCM dumps and the old GTA SA notes moved from livesplit-asl | The port needs the script and the game's own script as reference, and its known defects and porting lists. The script and both dumps are git-ignored (checked with `git check-ignore`), since the leaked source is not ours to publish and the script has no license. |
| 2026-11-14 | CSP (partial) | Build | Add the component skeleton: project and solution files, the factory, an empty component and settings panel, and deploy.cmd | Copied from OutputToFile's setup: a Split Spammer component under Control that draws nothing and does nothing. Builds with no warnings in Debug and Release and a headless smoke test creates it, round-trips its settings and disposes it; not yet loaded in LiveSplit. |
| 2026-11-14 | — | Project | Create the repo with its docs, tracking index and .gitignore | The GTA SA component needed a home, with its work tracked before any code exists. Checked with the tracking audit. |
