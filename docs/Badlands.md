# Badlands story missions

Detail for the Badlands rows of [TODO.md](TODO.md): the live ASL's Badlands missions, one item each, in the ASL's order.

Dependencies: as for Los Santos ([LosSantos.md](LosSantos.md)): THR and MHL, and TST's comparison harness. AAO also needs PTW (the wanted level and the melee slot). The group nodes (`BL`, `BL_Intro`, `BL_Catalina`, `BL_Cesar`, `BL_Truth`) are added to `SettingsTree.Build` by the first port under each. The warps (WRP) read the Badlands, King in Exile, Catalina and Truth counters.

How to port is in [porting.md](porting.md). Everything is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl`, region named in each section. None of these was recorded working in the ASL repo (its checks VER 7 to VER 16, `reference\Verify.md`).

## BDL — Badlands

The ASL's region "Badlands" (under "Trailer Park"), written with `RegisterMissionBasics`: thread `bcrash1`, Badlands counter 0 to 1, settings `bl` (on) with marker, start, the mountain base marker, the cabin cutscene starting and ending, the reporter killed, the photograph, and the pass. Reads the chapter (-1 to 0 the base, 1 the cabin end, 7, 8), then the trip-skip flag becoming 1 (the cabin cutscene start).

Watchers: `$493` (`flag_bcrash_mission_counter`), `$6965` (`bc1_stage`), `$2208` (`flag_bcr1_skip`). No locals.

**Evidence:** read 2026-11-15, region "Badlands" (VER 7).

**Plan:** as porting.md. Adds the `BL` and `BL_Intro` nodes.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7; the changelog gets a Badlands entry.

## KIE — King in Exile

The ASL's region "King In Exile": thread `catcut`, settings `kie` (on) with marker, start, "Cutscene ended" and the pass. The progress check uses `$2163` as the mission's counter, 0 to 1; the leak calls it `flag_trailor_cutscene`. The event is the local becoming 7.

Watchers: `$2163` (`flag_trailor_cutscene`), candidate `38@` `catcut_cell_index_end`.

**Evidence:** read 2026-11-15, region "King In Exile" (VER 8). The label says "Cutscene ended" and the id says phone call; check live which it is.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, the event and the pass.

**Docs when done:** as porting.md, step 7.

## CDT — Catalina dates

The ASL's region "Mission Picker": the Catalina thread `catalin` runs the four dates, First Date, First Base, Gone Courting and Made In Heaven, which have settings under `catDate` ("Dates", off), each with marker and start and no pass. The thread is registered without a split; the four mission names split the markers. The check returns once the Catalina counter is 4 or more; on a thread change, returns when the previous thread was `audiol`, and otherwise, when the new thread is `catalin`, splits the start the counter picks (0 to 3).

Watchers: `$64` (`cat_counter`), `thread`.

**Evidence:** read 2026-11-15, region "Mission Picker" (VER 9). The `audiol` test needs `thread`'s Old, which THR's string watcher keeps.

**Plan:** as porting.md. Adds the `BL_Catalina` node; the Catalina missions below share it.

**Verify:** the comparison passes; in LiveSplit, each date's marker and start, and no start when `audiol` runs just before `catalin`.

**Docs when done:** as porting.md, step 7.

## TNK — Tanker Commander

The ASL's region "Tanker Commander", written with `RegisterMissionBasics`: thread `cat3`, settings `tc` (on) with marker, start, the intro end, the tanker entered, the trailer coupled, the chase start, the arrival at the trucking compound, and the pass. Its progress check uses the mission's own passed flag as the counter, 0 to 1. The check returns unless the stage changed, then switches on it (1 to 5).

Watchers: `$716` (`flag_cat_mission3_passed`), candidate `35@` `c3_stage` (shared).

**Evidence:** read 2026-11-15, region "Tanker Commander" (VER 10). In the ASL the pass setting and the passed flag's watcher have the same name, `tc_pass`; here they will not.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## STB — Small Town Bank

The ASL's region "Small Town Bank", written with `RegisterMissionBasics`: thread `cat2`, its passed flag 0 to 1, settings `stb` (on) with marker, start, the crowd control, the alarm and the fade after it, the ATM segment, each of the three machines shot once, twice and its money picked up, all money picked up, Catalina leaving the building, the alley, the biker cops arriving and killed, the chase, Catalina running to and mounting CJ's bike, and the pass. Reads the chapter (`Current > Old` with no `Changed` test, each event through `TrySplit`), then the three machines in a loop that returns from inside it (a change from 1 to 3 sends the "shot twice" split first), so the later machines and locals are not read in that tick; then three Catalina locals.

Watchers: `$715` (`flag_cat_mission2_passed`), candidates `41@` `mission_flag`, `97@` to `99@` `atm_flag[0]` to `[2]`, `344@` `catalina_advance_flag`, `362@` `cat2_i`, `366@` `cat2_play_getaway_dialogue`. Shares `97@` and `99@` with AAO.

**Evidence:** read 2026-11-15, region "Small Town Bank" (VER 11). The ASL's ids for this mission start with `tgs_` (The Green Sabre's prefix) or have no mission prefix (`machine0_1`); ours are new anyway.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event, each machine.

**Docs when done:** as porting.md, step 7.

## LLS — Local Liquor Store

The ASL's region "Local Liquor Store", written with `RegisterMissionBasics`: thread `cat1`, its passed flag 0 to 1, settings `lls` (on) with marker, start, the cutscene end, the three bikers shot, one, two and three briefcases, the end marker, the end cutscene and Catalina done talking, and the pass. Reads the chapter (a rise to 2 is the cutscene end), then, in chapter 4, the sub-chapter (2, 4, 5 through `TrySplit`), then the pickups (a rise to 2, 3 or 4 splits the old count), then the three bikers.

Watchers: `$714` (`flag_cat_mission1_passed`), `$6987` (`cat1_pickups_collected`), candidates `34@` `m_stage`, `35@` `m_goals`, `121@` `camera_in_position_look`, `122@` `cat1_cops_dialogue`, `123@` `cat1_take_home_dialogue` (the last three do not sound like bikers shot; confirm the count). `34@`, `35@` and `122@` are shared.

**Evidence:** read 2026-11-15, region "Local Liquor Store" (VER 12). Defect: the third biker's check reads `123@` but tests the first biker's watcher, so "third biker" splits together with the first and never on its own.

**Plan:** as porting.md; fix the third biker (it diverges from the ASL there, so leave that id out of the comparison).

**Verify:** the comparison passes apart from the third biker; in LiveSplit, each event, the bikers in turn.

**Docs when done:** as porting.md, step 7.

## AAO — Against All Odds

The ASL's region "Against All Odds", written with `RegisterMissionBasics`: thread `cat4`, its flag `$717` 0 to 1 (the decompile calls it "all Catalina missions passed"), settings `aao` (on) with marker, start, the intro end, the shop entered, the back room door blown, the shop left, the car entered, the chainsaw (tooltip: not a requirement), each of four wanted stars lost, the final marker, and the pass. Reads four locals first, then a cutscene counter reaching 10 (intro end or shop left, picked by the other flags), then the shop, door, end marker and car flags, then the wanted level (a drop splits `aao_wantedLevel<old>`), then the melee slot becoming 9.

Watchers: `$717` (`flag_cat_mission4_passed`), `wantedLevel`, `weaponSlotMelee` (PTW), candidates `90@` `flag_mission_passed_cat4`, `96@` `flag_robbery_cutscene_otb`, `97@` `flag_cutscene1_cat4`, `99@` `flag_cat_shoot_cops`, `103@` `flag_has_otb_door_been_open`. `108@` is registered and never read. `90@`, `97@`, `99@` and `103@` are shared.

**Evidence:** read 2026-11-15, region "Against All Odds" (VER 13). The ASL reads `90@` without registering it itself (Just Business registers it). A drop from five or six stars names a setting that does not exist (porting.md, step 3). The dead region of the same name in `split` is an older version from other locals.

**Plan:** as porting.md; register `90@` in this module.

**Verify:** the comparison passes; in LiveSplit, each event, the stars lost one by one.

**Docs when done:** as porting.md, step 7.

## WZM — Wu Zi Mu and Farewell, my love

The ASL's region "Wu Zi Mu / Farewell My Love": one thread, `bcesar4`, for both races, settings `bcesar4_wzm` and `bcesar4_fml` (both on, tooltips pointing at the Badlands races under Races; Farewell's start has a tooltip on starting it straight after Wu Zi Mu), each with marker, start, the intro end, the race won, the ending cutscene, and the pass. No progress check: every tick it reads `thread` (a change to `bcesar4` splits Wu Zi Mu's start at counter 0, Farewell's at 5) and the Cesar counter (a rise from 9 or less splits the event its new value names, 2 to 5 for Wu Zi Mu, 7 to 10 for Farewell).

Watchers: `$492` (`flag_bcesar_mission_counter`), `thread`.

**Evidence:** read 2026-11-15, region "Wu Zi Mu / Farewell My Love" (VER 14). Defect: Farewell's mission name is registered with the id `fml_marker`, which has no setting, so its marker never splits.

**Plan:** as porting.md; give Farewell's mission name its own marker id (a fix, outside the comparison). Check the exact text of both mission names. Adds the `BL_Cesar` node.

**Verify:** the comparison passes apart from Farewell's marker; in LiveSplit, both races' events and markers.

**Docs when done:** as porting.md, step 7.

## BHV — Body Harvest

The ASL's region "Body Harvest": thread `truth1`, Truth counter 0 to 1, settings `bh` (on) with marker, start, the farm cutscene starting and ending, the tractor ambush, the combine driver getting out, the combine entered, leaving the field, the corner at the water tower, leaving the farm, the combine delivered, the phone ringing and hung up, and the pass. Reads two staged locals (arrival cutscene 1 and 5; combine status 1, 3 and 9) and five flags becoming 1.

Watchers: `$491` (`flag_truth_mission_counter`), candidates `117@` `truth1_flag`, `125@` `tractorchase2_t1flag`, `128@` `combinedriverattack_t1flag`, `129@` `playerincombine_t1flag`, `136@` `chaser5_t1flag`, `142@` `walton2_t1flag`, `146@` `skipphonecall_t1flag`. `128@` is shared.

**Evidence:** read 2026-11-15, region "Body Harvest" (VER 15). The ASL has no split for the corner at the water tower: its setting exists but no check returns it.

**Plan:** as porting.md. Adds the `BL_Truth` node.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## AYG — Are You Going to San Fierro?

The ASL's region "Are You Going to San Fierro?", written with `RegisterMissionBasics` with the mission name "Are you going to San Fierro?": thread `truth2`, Truth counter 1 to 2, settings `truth2` (on) with marker, start, the intro end, a group of plants destroyed (1 to 44), the rocket launcher cutscene, the helicopter, the mothership, the garage, and the pass. Unlike other missions, its event settings are on by default. Reads the plants global (44 means the intro ended; every step down splits each plant between), then the goals (2, 3, 5) and the control flag (5).

Watchers: `$491`, `$7011` (`t2_total_weed_burnt`; the ASL reads it as plants remaining, counting down from 44), candidates `87@` `t2_goals`, `88@` `t2_control_flag`. Both locals are shared.

**Evidence:** read 2026-11-15, region "Are You Going to San Fierro?" (VER 16). The leak's name for `$7011` suggests it counts burnt plants up, not down; check live. The dead region of the same name in `split` is an older version.

**Plan:** as porting.md; keep the ASL's defaults; check the mission name's exact text.

**Verify:** the comparison passes; in LiveSplit, each event and a few plants.

**Docs when done:** as porting.md, step 7.
