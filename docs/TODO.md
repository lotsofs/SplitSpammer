# TODO

Open work in this repo, by three-letter code. Each code links to its section in the detail doc named in the group heading: what is wanted, the evidence, a plan, how to verify it, and which docs to update when it is done. Within a group, rows are in suggested order of work. Severity is High, Medium or Low.

When an item is finished, delete its row here and its section in the detail doc, and log it in [DONE.md](DONE.md). When work turns up a defect that isn't being fixed now, add both. A change users would notice also goes under Unreleased in [CHANGELOG.md](../CHANGELOG.md).

## Component — [Component.md](Component.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [TST](Component.md#tst--headless-test-project) | Tests | No test project: an xunit project that drives the component headless with a fake game reader and LiveSplit's timer model | High |
| [MIS](Component.md#mis--port-the-remaining-missions) | Missions | Port the rest of the ASL's live missions and side content, one file each, in story order | Medium |
| [GSH](Component.md#gsh--split-at-the-start-of-the-grove-street---home-lines) | Missions | The `intro_groveStreetLinePlayed` split fires when the second "Grove Street - Home" line starts, because `$54` is only set then: make it fire when the first line starts, probably by watching the subtitle text, and fix its label | Low |
| [SLD](Component.md#sld--start-and-reset-the-timer-when-loading-a-save) | Timer control | The ASL's "Start/Reset timer when loading save" options are not ported (they never worked in the live script); the recent-update ASL does the start one from the loading byte at `+0x7A67A5`, so both can be added that way, off by default | Low |
| [LRM](Component.md#lrm--load-removal) | Timer control | No load removal: GTA SA is timed in real time and the ASL has no isLoading block; if a loadless category ever wants it, pause game time through `IsGameTimePaused` and `SetGameTime` | Low |

## Project — [Project.md](Project.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [LIC](Project.md#lic--choose-a-license) | Legal | No license yet; the component will port logic from a script that is a fork of Tduva's, and Tduva's repository has no license, so ask them before porting it | Medium |
| [REL](Project.md#rel--releases-and-the-update-feed) | Release | No releases, no update feed and no Installing section in the README | Low |
