# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

SplitSpammer will be a LiveSplit layout component (.NET Framework 4.8.1 class library, WinForms) that auto splits GTA San Andreas. It reads the game's memory itself, has a settings tree you can collapse and expand, splits through LiveSplit's timer, and is unit-tested without the game. It replaces `GTASA.asl`, which grew too large for one ASL file (ASL has no includes and can't be tested without LiveSplit and the game), and the auto-splitting runtime was ruled out because its settings dialog can't collapse.

**Status: no code yet.** The repo holds only its docs and `.gitignore`. All planned work is tracked as items, each with its design written down.

## Tracked work

Start at [docs/TODO.md](docs/TODO.md): the index of open work by three-letter code, each linking to its detail section (what, evidence, plan, how to verify, docs to update) in a doc under `docs/`. Finished items move to [docs/DONE.md](docs/DONE.md) in the same change, one row per commit. Anything a user would notice goes under Unreleased in [CHANGELOG.md](CHANGELOG.md). The docs refer to code by member and file, not line number, and say whether a behaviour was reproduced or only read. Don't leave `// TODO` comments; add an index row instead.

## Related projects and places

- `D:\Repositories\LiveSplit.OutputToFile`: the user's other LiveSplit component, and the template for this one: an old-style csproj at the repo root targeting v4.8.1, a `deploy.cmd` that copies the DLL into LiveSplit's `Components` folder, an SDK-style xunit test project in `Tests\`, an update feed, and the same docs layout. Its CLAUDE.md and `docs\` describe the build, the thread rules and the testing approach to copy.
- `D:\Repositories\livesplit-asl`: the ASL scripts. `GTASA.asl` there is the behaviour to port, its `docs\Porting.md` lists what is left to port, and its CLAUDE.md explains the script and the SCM dumps. Test fixtures here are recorded from that script's committed version.
- `D:\Programs\Streaming\LiveSplit`: the user's LiveSplit. The component references `LiveSplit.Core.dll` from there (the `LiveSplitDir` property).
- **Never copy `main.scm.txt` or `scm_leak.txt` into this repo.** They are reference dumps of the game's script kept in the ASL repo; the leaked source is not ours to publish. `.gitignore` lists both as a guard.

## Conventions

- Follow OutputToFile's code style: tabs for indentation, namespace `LiveSplit.UI.Components.SplitSpammer`, an old-style csproj that lists every source file, and thread rules (LiveSplit's event handlers may run on another thread, so they only count; the work happens elsewhere).
- Only GTA SA PC version 1.0 is supported.
- Split and setting IDs match the ASL's (a short lowercase mission abbreviation plus `_event`, such as `intro_passed`), so recorded fixtures and users' habits carry over.
