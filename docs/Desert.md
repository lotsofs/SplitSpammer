# Desert story missions

Detail for the Desert rows of [TODO.md](TODO.md): one item per mission, in the order of the ASL's dead mission lists.

**What these items share** is as for San Fierro ([SanFierro.md](SanFierro.md), the note at the top): built from the ASL's dead mission lists and the SCM dumps, with tests in place of the ASL comparison. The ASL's group nodes are `Des` "Desert", with `Des_Toreno` "Toreno" and `Des_WangCars` "Wang Cars". The Toreno counter `$593` (`flag_desert_mission_counter`) goes on into the Las Venturas air strip missions (NOE onwards, [LasVenturas.md](LasVenturas.md)). Buying the Wang Cars showroom and the Verdant Meadows airstrip are splits in the old script; they are item WVP in [SideMissions.md](SideMissions.md).

Dependencies: THR and MHL ([Groundwork.md](Groundwork.md)); TST for the tests. The same checks of each index against the SCM apply.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`#region OLD_SHIT`) and `reference\main.scm.txt`.

## MON — Monster

Thread `toreno1`; Toreno counter `$593`, index 1; under `Des_Toreno`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `Des` and `Des_Toreno` nodes.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7; the changelog gets a Desert entry.

## HJK — Highjack

Thread `toreno2`; Toreno counter `$593`, index 2; under `Des_Toreno`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## ITD — Interdiction

Thread `des3`; Toreno counter `$593`, index 3; under `Des_Toreno`. The old script's Slamvan-before-Taxi route option (RSK) ended its skipping when Interdiction started.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## VMD — Verdant Meadows

Thread `desert4`; Toreno counter `$593`, index 4; under `Des_Toreno`. Buying the airstrip itself is WVP.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## LTF — Learning to Fly (Pilot School)

Thread `desert5`; Toreno counter `$593`, index 5; under `Des_Toreno`. It is also the pilot school: the old script had a "Pilot School" node under Schools (off by default) with "Pilot School started for the first time" (thread `desert5`), a split per test from `$1952` (`d5_total_missions_open`, starting at 1: 2 Takeoff, 4 Land Plane, 6 Circle Airstrip, 7 Circle Airstrip and Land, 8 Helicopter Takeoff, 9 Land Helicopter, 10 Destroy Targets, 11 Loop-the-Loop, 12 Barrel Roll; Parachute Onto Target is not in it) and the pass from `$87` (`pilot_test_passed`, "Parachute Onto Target (Pilot School Passed)"). The ASL's comment on `para_pickup_flag` says it goes back to 0 during flight school; the engine's guard only blocks while the intro is also unpassed, so that does not stop splits.

**Evidence:** read 2026-11-15 (dead "mission lists", "Schools" settings).

**Plan:** as the note above, plus the per-test splits. Decide whether the tests go under the mission or under the Schools node, where the other schools go (DRS, BKS, BTS); the old script used both places. Keep one setting per event either way.

**Verify:** tests with a fake reader; in LiveSplit, the start, a few tests and the pass.

**Docs when done:** as porting.md, step 7.

## ZRI — Zeroing In

Thread `steal1`; Wang Cars counter `$544` (`flag_steal_mission_counter`), index 1; under `Des_WangCars`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `Des_WangCars` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## TDR — Test Drive

Thread `steal2`; Wang Cars counter `$544`, index 2; under `Des_WangCars`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## CFT — Customs Fast Track

Thread `steal4`; Wang Cars counter `$544`, index 3; under `Des_WangCars`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## PNW — Puncture Wounds

Thread `steal5`; Wang Cars counter `$544`, index 4; under `Des_WangCars`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.
