# TODO

Open work in this repo, by three-letter code. Each code links to its section in the detail doc named in the group heading: what is wanted, the evidence, a plan, how to verify it, and which docs to update when it is done. Within a group, rows are in suggested order of work. Severity is High, Medium or Low.

When an item is finished, delete its row here and its section in the detail doc, and log it in [DONE.md](DONE.md). When work turns up a defect that isn't being fixed now, add both. A change users would notice also goes under Unreleased in [CHANGELOG.md](../CHANGELOG.md).

## Component — [Component.md](Component.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [CSP](Component.md#csp--check-that-the-component-loads-in-livesplit) | Build | The skeleton builds and passes a headless smoke test but has never been loaded in LiveSplit: run `deploy.cmd` with LiveSplit closed, add Split Spammer under Control, open its settings and remove it | High |
| [TST](Component.md#tst--headless-test-project) | Tests | No test project: an xunit project that drives the component headless with a fake game reader and LiveSplit's timer model | High |
| [STR](Component.md#str--collapsible-settings-tree-saved-in-the-layout) | Settings | The settings tree: collapsible nodes with checkboxes and tooltips, a branch menu, saved in the layout by ID, and a setting counts only when its ancestors are checked | High |
| [RDR](Component.md#rdr--game-reader-for-the-supported-version) | Game memory | Attach to the game, accept only PC 1.0, read memory through LiveSplit.ComponentUtil behind a small interface, and show a status line | High |
| [ENG](Component.md#eng--split-engine-and-poll-thread) | Splitting | The split engine: a poll thread, once-per-tick watchers as in the ASL, the split guards, once-per-reset splits, one split per tick, timer actions on the UI thread | High |
| [ITB](Component.md#itb--in-the-beginning-the-first-mission) | Missions | The first mission, In the Beginning, with its settings and checks, tested against traces recorded from the real ASL | Medium |
| [STT](Component.md#stt--start-reset-save-load-options-and-load-removal) | Timer control | Start and reset the timer, the save-load options and load removal, as in the ASL's start and reset blocks | Medium |
| [MIS](Component.md#mis--port-the-remaining-missions) | Missions | Port the rest of the ASL's live missions and side content, one file each, in story order | Medium |

## Project — [Project.md](Project.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [LIC](Project.md#lic--choose-a-license) | Legal | No license yet; the component will port logic from a script that is a fork of Tduva's, and Tduva's repository has no license, so ask them before porting it | Medium |
| [REL](Project.md#rel--releases-and-the-update-feed) | Release | No releases, no update feed and no Installing section in the README | Low |
