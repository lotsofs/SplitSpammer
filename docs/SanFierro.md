# San Fierro story missions

Detail for the San Fierro rows of [TODO.md](TODO.md): one item per mission, in the order of the ASL's dead mission lists.

**What these items share.** The ASL has no logic for any of them: only its empty group nodes (`SF` "San Fierro", with `SF_Main` "Garage / Syndicate", `SF_Wuzimu` "Woozie" and `SF_Zero` "Zero"), and in its dead code, `vars.missionChains` (each mission's counter and its value once passed) and `addMissionList` (which node it sits under, in story order). The threads come from the shipped mission table in `reference\main.scm.txt`. So each item is built rather than ported: the basic settings and the progress check from `StoryMission` (counter at the index before to the mission's index), and mid-mission events found in the SCM dumps the way the Los Santos ones were (a stage, goal or dialogue local of the mission, and what its values mean), with labels in the ASL's style. Step 5 of [porting.md](porting.md) has no ASL to compare with: test the check with a fake reader instead (each event and the pass once, nothing while another mission is the last started one, nothing once passed). The rest of porting.md applies.

Dependencies: TST for the tests. Check each index against the SCM (the value the mission's pass sets on its counter) before relying on it, and check whether missions on one counter can be passed in another order; if they can, a fixed before and after does not fit and the check needs the mission's own passed flag instead. The ASL's first commit had a San Fierro death warp ("[Deathwarp SF]"), which belongs to WRP ([Other.md](Other.md)).

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl` (`#region OLD_SHIT`, "mission lists" and "Main Missions") and `reference\main.scm.txt` (`DEFINE MISSION`, `script_name`).

## WFH — Wear Flowers in your Hair

Thread `garag1`; Garage counter `$541` (`flag_garage_mission_counter`), index 1; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `SF` and `SF_Main` nodes.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7; the changelog gets a San Fierro entry.

## WTP — 555 WE TIP

Thread `scrash3` (the leak's `scrash1`); C.R.A.S.H. San Fierro counter `$546` (`flag_scrash_mission_counter`), index 1; under `SF_Main`, after Wear Flowers in your Hair. The ASL's dead code notes that `$1883` gets set during this mission and could track its progress.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## DCN — Deconstruction

Thread `decon` (the leak's `garag2`); Garage counter `$541`, index 2; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## PHO — Photo Opportunity

Thread `syn1`; Syndicate counter `$545` (`flag_synd_mission_counter`), index 1; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## JZY — Jizzy

Thread `syn2`, for two parts on the Syndicate counter: "Jizzy (Cutscene)" at index 2 and "Jizzy" at index 3, as House Party has two parts on one thread (HPY in [LosSantos.md](LosSantos.md)); under `SF_Main`. The dead warp list has death warps from the cutscene part to the mission and from the mission to Mountain Cloud Boys (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above, with the two parts told apart by the counter as HPY does.

**Verify:** tests with a fake reader; in LiveSplit, both parts' start and pass, and each event.

**Docs when done:** as porting.md, step 7.

## TBM — T-Bone Mendez

Thread `driv2`; Syndicate counter `$545`, index 4; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## MTR — Mike Toreno

Thread `driv3`; Syndicate counter `$545`, index 5; under `SF_Main`.

**Evidence:** read 2026-11-15. The dead warp list has a death warp from Mike Toreno to Lure (WRP).

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## OUT — Outrider

Thread `syn3`; Syndicate counter `$545`, index 6; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## SNT — Snail Trail

Thread `scrash2`; C.R.A.S.H. San Fierro counter `$546`, index 2; under `SF_Main`, after Outrider.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## ICK — Ice Cold Killa

Thread `synd4`; Syndicate counter `$545`, index 7; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## PIR — Pier 69

Thread `syn5`; Syndicate counter `$545`, index 8; under `SF_Main`.

**Evidence:** read 2026-11-15. The mission table lists `syn5` after `syn7`; the counter index is what the check needs.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## TLF — Toreno's Last Flight

Thread `syn6`; Syndicate counter `$545`, index 9; under `SF_Main`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## YKB — Yay Ka-Boom-Boom

Thread `syn7`; Syndicate counter `$545`, index 10; under `SF_Main`, last. The dead warp list has death warps to it from The Da Nang Thang and from Boat School (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## MCB — Mountain Cloud Boys

Thread `wuzi1`; Woozie counter `$543` (`flag_wuzi_mission_counter`), index 1; under `SF_Wuzimu`. The dead warp list has a death warp from Jizzy to it (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `SF_Wuzimu` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## RFL — Ran Fa Li

Thread `farlie4`; Woozie counter `$543`, index 2; under `SF_Wuzimu`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## LUR — Lure

Thread `driv6` (the leak's `farlie5`); Woozie counter `$543`, index 3; under `SF_Wuzimu`. The dead warp list has death warps from Mike Toreno to it and from it to Paramedic (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## AMA — Amphibious Assault

Thread `wuzi2`; Woozie counter `$543`, index 4; under `SF_Wuzimu`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## DNT — The Da Nang Thang

Thread `wuzi5` (the leak's `wuzi4`); Woozie counter `$543`, index 5; under `SF_Wuzimu`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## AIR — Air Raid

Thread `zero1`; Zero counter `$542` (`flag_zero_mission_counter`), index 1; under `SF_Zero`. Its block in the leak is not identified (porting.md, the script-name table), so local names need care.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `SF_Zero` node.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## SPL — Supply Lines

Thread `zero2`, mission "Supply Lines..."; Zero counter `$542`, index 2; under `SF_Zero`. As with Air Raid, which leak block is its own is not certain.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.

## NMA — New Model Army

Thread `zero4`; Zero counter `$542`, index 3; under `SF_Zero`.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, the marker, start, each event and the pass.

**Docs when done:** as porting.md, step 7.
