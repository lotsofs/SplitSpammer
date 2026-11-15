# Return to Los Santos story missions

Detail for the Return to Los Santos rows of [TODO.md](TODO.md): one item per mission, in the order of the ASL's dead mission lists.

**What these items share** is as for San Fierro ([SanFierro.md](SanFierro.md), the note at the top): built from the ASL's dead mission lists and the SCM dumps, with tests in place of the ASL comparison. Unlike the other later cities, the old script had mid-mission events for three of these (Cut Throat Business, Grove 4 Life, End of the Line part 3), in the dead region `#region Splits OLDDDDD` of `split`, written against `vars.Passed`, which no longer exists; port their logic with the progress check instead. The ASL's group nodes are `RTLS` "Return to Los Santos", with `RTLS_Mansion` "Mansion", `RTLS_Grove` "Grove" and `RTLS_Riot` "Finale". The live ASL already watches the Mansion counter `$626` (`flag_mansion_mission_counter`), for the gang territories (GTR, which uses it to tell Los Santos from Return to Los Santos).

Dependencies: TST for the tests. The same checks of each index against the SCM apply.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`#region OLD_SHIT`, the dead tail of `init`, the dead regions of `split`) and `reference\main.scm.txt`.

## AHH — A Home in the Hills

Thread `mansio1`; Mansion counter `$626`, index 1; under `RTLS_Mansion`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `RTLS` and `RTLS_Mansion` nodes.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7; the changelog gets a Return to Los Santos entry.

## VBD — Vertical Bird

Thread `mansio2`; Mansion counter `$626`, index 2; under `RTLS_Mansion`. The gang territory splits switch to their Return to Los Santos group once the Mansion counter reaches 2 (GTR).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## HMC — Home Coming

Thread `mansio3`; Mansion counter `$626`, index 3; under `RTLS_Mansion`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## CTB — Cut Throat Business

Thread `manson5`; Mansion counter `$626`, index 4; under `RTLS_Mansion`. The old script had "Arriving at the video shoot" and "Arriving at the pier" from two ints at `+0x55EE68` and `+0x57F10C` (the dead tail of `init` calls them locals used across several missions), each splitting on leaving 0 while this mission was the last started one and not yet passed (dead region "Cut Throat Business"). Those addresses are outside the SCM variable blocks and are not explained.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Find what the two addresses are before using them; if they cannot be explained, find the same events in the mission's locals instead.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, both events and the pass.

**Docs when done:** as porting.md, step 7.

## BDB — Beat Down on B Dup

Thread `grove1`; Grove counter `$627` (`flag_grove_mission_counter`), index 1; under `RTLS_Grove`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `RTLS_Grove` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## GFL — Grove 4 Life

Thread `grove2`; Grove counter `$627`, index 2; under `RTLS_Grove`. The old script had "Arriving in Idlewood", "First territory captured" and "Second territory captured" from `56@`, `58@` and `54@`, each splitting on leaving 0 while this mission was the last started one and not yet passed (dead region "Grove 4 Life"); the start split came from the thread (dead `vars.startMissions`).

**Evidence:** read 2026-11-15.

**Plan:** as the note above, with the three events; look up the three locals' leak names (porting.md, "Leak names").

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, the three events and the pass.

**Docs when done:** as porting.md, step 7.

## RIO — Riot

Thread `riot1`; Riot counter `$629` (`flag_riot_mission_counter`), index 1; under `RTLS_Riot`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `RTLS_Riot` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## LDS — Los Desperados

Thread `riot2`; Riot counter `$629`, index 2; under `RTLS_Riot`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## EOA — End of the Line, part 1

Thread `finalea`; Riot counter `$629`, index 3; under `RTLS_Riot`. The old script labelled its pass "End of the Line Part 1 (after killing Big Smoke)", on by default.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, with the old label.

**Verify:** tests with a fake reader; in LiveSplit, the start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## EOB — End of the Line, part 2

Thread `finaleb`; Riot counter `$629`, index 4; under `RTLS_Riot`. The old script labelled its pass "End of the Line Part 2 (start of chase)", on by default.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, with the old label.

**Verify:** tests with a fake reader; in LiveSplit, the start and the pass.

**Docs when done:** as porting.md, step 7.

## EOC — End of the Line, part 3

Thread `finalec`; Riot counter `$629`, index 5, set after the credits; under `RTLS_Riot`, as `eotlp3` (on). The old script had three events from `$8014` (`finalerails_fcflag`), each its value rising while part 2 was passed: "Start of cutscene after catching Sweet" (1), "Start of cutscene near Cluckin' Bell" (2), and "End of any%: Start of firetruck bridge cutscene" (3), the one split most players want, on by default; the pass is "After credits" (dead region "End of the Line").

**Evidence:** read 2026-11-15.

**Plan:** as the note above, with the three events; keep the end-of-any% split on by default.

**Verify:** tests with a fake reader; in LiveSplit, the three events, the any% end in particular, and the pass after the credits.

**Docs when done:** as porting.md, step 7.
