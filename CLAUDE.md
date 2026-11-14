# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

SplitSpammer will be a LiveSplit layout component (.NET Framework 4.8.1 class library, WinForms) that auto splits GTA San Andreas. It reads the game's memory itself, has a settings tree you can collapse and expand, splits through LiveSplit's timer, and is unit-tested without the game. It replaces `GTASA.asl`, which grew too large for one ASL file (ASL has no includes and can't be tested without LiveSplit and the game), and the auto-splitting runtime was ruled out because its settings dialog can't collapse.

**Status: skeleton only.** The component loads (once deployed) and does nothing: a factory, an empty component and an empty settings panel. All planned work is tracked as items, each with its design written down.

## Tracked work

Start at [docs/TODO.md](docs/TODO.md): the index of open work by three-letter code, each linking to its detail section (what, evidence, plan, how to verify, docs to update) in a doc under `docs/`. Finished items move to [docs/DONE.md](docs/DONE.md) in the same change, one row per commit. Anything a user would notice goes under Unreleased in [CHANGELOG.md](CHANGELOG.md). The docs refer to code by member and file, not line number, and say whether a behaviour was reproduced or only read. Don't leave `// TODO` comments; add an index row instead.

## Build and deploy

```sh
dotnet build LiveSplit.SplitSpammer.sln -c Debug     # or -c Release; add -p:LiveSplitDir=<folder> for another LiveSplit
deploy.cmd                                             # build the component, then copy it into LiveSplit's Components folder; deploy.cmd Release for Release
```

- `deploy.cmd` refuses to run while LiveSplit is running (LiveSplit locks the DLL), builds the project, and copies `bin\<config>\LiveSplit.SplitSpammer.dll` into `Components\` in the `LiveSplitDir` folder. It is a batch file because PowerShell scripts are disabled on the user's machine, and it keeps CRLF line endings. Run it from PowerShell (`.\deploy`), cmd or Git Bash (`cmd //c deploy.cmd`).
- An old-style (non-SDK) csproj: **every new `.cs` file needs its own `<Compile Include>` line**, or it isn't built. It targets .NET Framework 4.8.1, because LiveSplit does from 1.8.30; with a lower target MSBuild can't resolve its DLLs. `dotnet build` works without Visual Studio as long as the 4.8.1 targeting pack is installed.
- `LiveSplit.Core.dll` and `UpdateManager.dll` come from the `LiveSplitDir` folder, by default `..\..\Programs\Streaming\LiveSplit` from the repo, which is `D:\Programs\Streaming\LiveSplit`. Both are `<Private>False</Private>`, so `bin\<config>` holds only the component's DLL and PDB. Give a full path without a trailing backslash to `-p:LiveSplitDir`.
- The build has no warnings, so any warning is new. There is no test project yet (item TST). Until then, a headless check can be made with a scratch program that loads the DLL, builds a `LiveSplitState` the way OutputToFile's `Tests\TestRun.cs` does, and calls the factory and component members.

## Related projects and places

- `D:\Repositories\LiveSplit.OutputToFile`: the user's other LiveSplit component, and the template for this one: an old-style csproj at the repo root targeting v4.8.1, a `deploy.cmd` that copies the DLL into LiveSplit's `Components` folder, an SDK-style xunit test project in `Tests\`, an update feed, and the same docs layout. Its CLAUDE.md and `docs\` describe the build, the thread rules and the testing approach to copy.
- `reference\` (in this repo): the ASL script `GTASA.asl` being ported, the two SCM dumps, and the notes moved from the ASL repo. **`reference\README.md` explains how to read them** and `reference\Porting.md` lists what is left to port. Test fixtures are recorded from that script. All three big files are local only (git-ignored). `D:\Repositories\livesplit-asl` holds the user's other ASL scripts; GTA SA is no longer there.
- `D:\Programs\Streaming\LiveSplit`: the user's LiveSplit. The component references `LiveSplit.Core.dll` from there (the `LiveSplitDir` property).
- **Never commit or push `reference\main.scm.txt`, `reference\scm_leak.txt` or `reference\GTASA.asl`.** They sit in that folder only to be read. The leaked source is not ours to publish and the script has no license (item LIC). `.gitignore` lists all three; after any change to it, run `git check-ignore -v reference/GTASA.asl reference/main.scm.txt reference/scm_leak.txt` and `git status`, and check none of them shows as addable. This repo is public.

## Conventions

- Follow OutputToFile's code style: tabs for indentation, namespace `LiveSplit.UI.Components.SplitSpammer`, an old-style csproj that lists every source file, and thread rules (LiveSplit's event handlers may run on another thread, so they only count; the work happens elsewhere).
- Only GTA SA PC version 1.0 is supported.
- Split and setting IDs match the ASL's (a short lowercase mission abbreviation plus `_event`, such as `intro_passed`), so recorded fixtures and users' habits carry over.
