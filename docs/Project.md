# Project

Detail for the project rows of [TODO.md](TODO.md).

## LIC — Choose a license

The repo has no LICENSE file. Without one, copyright law applies as is: the code is all rights reserved, and others may look at it on GitHub but have no permission to copy, change or share it. A license is the author granting that permission on stated terms. Permissive licenses (MIT, Apache 2.0, BSD) let anyone use, change and share the code, closed-source or commercial included, and only require keeping the notice; Apache 2.0 also grants patent rights and is longer. Copyleft licenses (GPL) require derived work to carry the same license.

Two things bear on the choice. The component will port logic from `GTASA.asl`, which is a fork of Tduva's script, and Tduva's repository has no license (GitHub reports none, and neither does the `livesplit-asl` fork): code taken from a work with no license is all rights reserved by its author, and whoever copies it can't license it to others. And `lotsofs/LiveSplit.OutputToFile` is under Apache 2.0 because it is a fork of a project under that license, not because it was chosen for new code.

**Evidence:** read 2026-11-14: OutputToFile's LICENSE is Apache 2.0; GitHub's API reports no license for `tduva/LiveSplit-ASL` or for `lotsofs/livesplit-asl`.

**Plan:** the reference copy of the script in `reference\` is git-ignored until this is settled. Ask Tduva whether the logic ported from their `GTASA.asl` may be reused and under what license, before porting it; until they answer, port only logic that is the fork's own work, or leave the repo unlicensed. Then pick one for this repo, Apache 2.0 or MIT being the usual choices for a LiveSplit component, add the LICENSE file and a README line.

**Verify:** the LICENSE file exists, GitHub shows the license on the repo page, and the porting items note where logic was derived from.

**Docs when done:** README.md, CLAUDE.md if the conventions mention it.

## REL — Releases and the update feed

There are no releases, no update feed and no Installing section in the README. OutputToFile ships a DLL on GitHub releases and an `update.xml` feed that LiveSplit reads at startup, with rules written in its `docs\releasing.md`.

**Evidence:** read 2026-11-14 in OutputToFile's factory (`UpdateName`, `UpdateURL`, `XMLURL`, `Version`), `update.xml` and docs.

**Plan:** once the component does something useful, copy the same scheme: the factory's update members pointing at this repo, `update.xml` on `main`, a releasing doc, a README Installing section, and the first dated section in CHANGELOG.md.

**Verify:** LiveSplit offers the update when the feed lists a newer version than the installed DLL.

**Docs when done:** README.md, CHANGELOG.md, a new docs\releasing.md, CLAUDE.md.
