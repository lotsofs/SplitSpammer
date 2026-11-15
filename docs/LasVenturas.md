# Las Venturas story missions

Detail for the Las Venturas rows of [TODO.md](TODO.md): one item per mission, in the order of the ASL's dead mission lists.

**What these items share** is as for San Fierro ([SanFierro.md](SanFierro.md), the note at the top): built from the ASL's dead mission lists and the SCM dumps, with tests in place of the ASL comparison. The ASL's group nodes are `LV` "Las Venturas", with `LV_AirStrip` "Air Strip", `LV_Casino` "Casino", `LV_Crash` "C.R.A.S.H.", `LV_MaddDogg` "Madd Dogg" and `LV_Heist` "Heist". The air strip missions count on the Toreno counter `$593` shared with the Desert ([Desert.md](Desert.md)).

Dependencies: MHL ([Groundwork.md](Groundwork.md)); TST for the tests. The same checks of each index against the SCM apply; the casino missions in particular are listed in one order in the dead code and in another in the mission table, so check whether they can be passed out of order.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`#region OLD_SHIT`) and `reference\main.scm.txt`.

## NOE — N.O.E.

Thread `desert6`; Toreno counter `$593`, index 6; under `LV_AirStrip`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `LV` and `LV_AirStrip` nodes.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7; the changelog gets a Las Venturas entry.

## STW — Stowaway

Thread `desert9`; Toreno counter `$593`, index 7; under `LV_AirStrip`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## BLP — Black Project

Thread `maf4`; Toreno counter `$593`, index 8; under `LV_AirStrip`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## GGO — Green Goo

Thread `des10`; Toreno counter `$593`, index 9; under `LV_AirStrip`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## FKC — Fender Ketchup

Thread `casino1`; Casino counter `$597` (`flag_casino_mission_counter`), index 1; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `LV_Casino` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## EXS — Explosive Situation

Thread `casino2`; Casino counter `$597`, index 2; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## YHC — You've Had Your Chips

Thread `casino3`; Casino counter `$597`, index 3; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## DPY — Don Peyote

Thread `casino4`; Casino counter `$597`, index 4; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## ICR — Intensive Care

Thread `casino5`; Casino counter `$597`, index 5; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## TMB — The Meat Business

Thread `casino6`; Casino counter `$597`, index 6; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## FIB — Fish in a Barrel

Thread `casino7`; Casino counter `$597`, index 7 in the dead list, though the mission table lists it before Don Peyote; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above; settle its index first.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## FRF — Freefall

Thread `casino9`; Casino counter `$597`, index 8; under `LV_Casino`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## SMB — Saint Mark's Bistro

Thread `casin10`; Casino counter `$597`, index 9; under `LV_Casino`. The dead warp list has a death warp from Barnstorming to it (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## MSP — Misappropriation

Thread `vcrash1`; C.R.A.S.H. Las Venturas counter `$598` (`flag_vcrash_mission_counter`), index 1; under `LV_Crash`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `LV_Crash` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## HNN — High Noon

Thread `vcr2` (the leak's `vcrash2`); counter `$598`, index 2; under `LV_Crash`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## MDG — Madd Dogg

Thread `doc2`; Madd Dogg counter `$599` (`flag_doc_mission_counter`), index 1; under `LV_MaddDogg`. The dead warp list has a death warp from Trucking 7 to it (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `LV_MaddDogg` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## ARE — Architectural Espionage

Thread `heist1`; Heist counter `$600` (`flag_heist_mission_counter`), index 1; under `LV_Heist`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `LV_Heist` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## KTH — Key to her Heart

Thread `heist3`; Heist counter `$600`, index 2; under `LV_Heist`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## DAB — Dam and Blast

Thread `heist2`; Heist counter `$600`, index 3; under `LV_Heist`. The dead warp list has a death warp from it to Quarry (WRP), and the ASL's comment on `para_pickup_flag` says it goes back to 0 during this mission (harmless for the guard, as LTF explains).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## CPW — Cop Wheels

Thread `heist4`; Heist counter `$600`, index 4; under `LV_Heist`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## UUA — Up, Up and Away!

Thread `heist5`; Heist counter `$600`, index 5; under `LV_Heist`. The dead warp list has a death warp from it to World War Ace (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## BTB — Breaking the Bank at Caligula's

Thread `heist9`; Heist counter `$600`, index 6; under `LV_Heist`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.
