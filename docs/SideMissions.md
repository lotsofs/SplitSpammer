# Side missions

Detail for the side-mission rows of [TODO.md](TODO.md). The ones the live ASL has come first; the ones only in its dead code follow, in the ASL's node order (`SideMissions` "Side Missions": Courier, Trucking, Quarry, Valet Parking, Vehicle Submissions, Races, Stadium Events, Vehicle Challenges, Schools, Properties, Import/Export, Shooting Range, Gym Moves). That node order is also where each module goes in `Modules.All`, whichever is ported first.

Dependencies: TST's harness for all of them. The live ones are ported as [porting.md](porting.md) says. The dead-code ones are built like the later story missions ([SanFierro.md](SanFierro.md), the note at the top): from the old logic in the dead regions of `split` and the dead tail of `init`, written against `vars.Passed`, which no longer exists, and tested with a fake reader instead of compared with the ASL. The ASL repo's porting notes list Burglary among the old Vehicle Submissions, but neither the dead code nor the 2024 script (`git show f7885d5:GTASA.asl` in `D:\Repositories\livesplit-asl`) has it, so it is not an item.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl`, region named in each section, with leak names as porting.md describes.

## CUR — Courier

The ASL's region "Courier": settings `courier` (on) with one node per city (1 Los Santos, 2 San Fierro, 3 Las Venturas; on), each with its start (off), a Levels node (off) holding four levels, each with its deliveries (level n has n + 2) and its pass, and the city's pass (on). Thread `bcour` is registered without a split. The check needs `bcour` as the last started mission; reads the three passed flags (each through `TrySplit`); returns while the courier trigger is 0; reads the city local (a change to a city splits its start), the level local of that city, and the deliveries local, building the ids from city, level and delivery.

Watchers: `$1992` (`courierLA_passed_once`), `$1994` (`courierSF_passed_once`), `$1993` (`courierLV_passed_once`), `$189` (`flag_courier_trigger`), candidates `757@` `bcou_packagesthistrip`, `760@` `bcou_SFrunscompleted`, `761@` `bcou_LArunscompleted`, `762@` `bcou_LVrunscompleted`, `872@` `maxpackages` (the ASL reads `872@` as the city; confirm). The level locals agree with the ASL's city mapping.

**Evidence:** read 2026-11-15, region "Courier". A city of 0 builds ids with no setting (porting.md, step 3). The dead region of the same name in `split` is an older version.

**Plan:** as porting.md. Adds the `SideMissions` node.

**Verify:** the comparison passes; in LiveSplit, one city's start, deliveries, a level and the pass.

**Docs when done:** as porting.md, step 7; the changelog gets a side-missions entry.

## TRK — Trucking

The ASL's region "Trucking": settings `trucking` (on) with eight missions (on), each with marker, start, leaving the compound, the pass (on) and leaving the truck after passing. Thread `truck` is registered without a split. The check reads the odd-job text and the trucking counter first; the mission number is the counter's Old plus 1; the text changing to "Trucking" splits that number's marker, whatever the last started mission. Then it needs `truck`: the counter rising splits the pass; the stage local (1 start; 3 leaving the truck, for the previous number); the depot flag (1 leaving the compound).

Watchers: `$8159` (`g_nTruckMissionsPassed`), `oddMissionStartText` (a string watcher the engine already adds), candidates `36@` `m_stage`, `106@` `flagOutOfDepot`. Shares `36@` with NAK and `106@` with RTF.

**Evidence:** read 2026-11-15, region "Trucking" (VER 17). The number comes from the counter's Old, which is 0 on its first read (no split depends on that read alone).

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, two trucking missions in a row, all their events.

**Docs when done:** as porting.md, step 7.

## FFT — Firefighter

The ASL's region "Firefighter" under "Vehicle submissions": settings `VehicleSubmissions` (on) holding `firefighter` (on) with "Mission Started for the first time", a Levels node with twelve levels, a Fires Extinguished node with 78 fires (labels built in a loop with each fire's level and number), and the pass (off). Thread `firetru` splits the start. The check needs `firetru`; splits every fire between Old and Current; a level rising from above 0 splits the old level and returns; the passed flag becoming 2 splits the pass.

Watchers: `$8214` (`db_fires_extinguished`), `$8213` (`db_firetruck_level`), `$1489` (`done_firetruck_progress`).

**Evidence:** read 2026-11-15, region "Firefighter". "For the first time" relies on once-per-reset.

**Plan:** as porting.md. Adds the `VehicleSubmissions` node.

**Verify:** the comparison passes; in LiveSplit, the start, a few fires and two levels.

**Docs when done:** as porting.md, step 7.

## RAC — Races

The ASL's region "Races": settings `Races` (on) with Los Santos, San Fierro, Las Venturas and Air nodes (on) and races 0 to 24 (on), each with "Race start (Countdown end)", its checkpoints (the last labelled Finish; counts from the ASL's table) and "Race won" (on, tooltip on the double split with the last checkpoint). The stadium races 25 and 26 have no settings here (SRC). Thread `cprace` is registered without a split. The check needs `cprace`; reads the race index; picks the checkpoint local by race type (Badlands A and B, 7 and 8: `260@`; stadium, 25 and over: `268@`, with laps; air, 19 and over: `257@`; others `262@`) and reads it; reads the race's won flag (through `TrySplit`); for stadium races reads the lap; then a checkpoint rise returns `race<n>cp<old>` (with the lap in the id for stadium races).

Watchers: `$352` (`race_selection`), `$2300` to `$2326` (`got_race_made_progress[0]` to `[26]`), candidates `257@` `a`, `260@` `total_racers`, `262@` `cpcounter[1]`, `268@` `cpcounter[7]`, `284@` `lap_cpcounter[7]`. The candidates for `257@` and `260@` do not look like checkpoint counters: the count is probably off in this block, so confirm them.

**Evidence:** read 2026-11-15, region "Races". The race missions Lowrider (High Stakes, Low-Rider) and Wu Zi Mu run on their own threads, so these checks never split during them, though their tooltips point here.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, one ground race, one Badlands race and one air race, start to finish.

**Docs when done:** as porting.md, step 7.

## CHL — Chiliad Challenge

The ASL's region "Chiliad Challenge" under "Vehicle Challenges": settings `VehicleChallenges` (on) holding `mtbiker` (on) with three routes (on), each with "Bicycle mounted", "Countdown start", "Race start (Countdown end)", its checkpoints (route 3 merges 12 and 13, with a tooltip on why), the finish and "Race won" (off). Thread `mtbiker` is registered without a split. The check has no last-started-mission test: it reads the completed flag (once 1 and unchanged it returns; becoming 1 is route 3's finish), the reward money (20000, 30000, 40000 tell the route; a change splits that route's win), the next-race global when the money tells nothing, the total checkpoints (19, 32, 24: the countdown start), `missionStartText` ("The Chiliad Challenge": the route's marker), and the route's checkpoint local (a rise, without a `Changed` test, splits the old checkpoint).

Watchers: `$1801` (`flag_mtbike_passed_1stime`), `$1799` (`mtbikerace_selection`), `missionStartText` (a string watcher the engine already adds), `646@`, `700@`, `747@`, `750@` (the ASL's comments: reward money, total checkpoints, route 3's checkpoint, routes 1 and 2's; the leak's count gives `e`, an array element and two `cpcounter_mtbikerace` elements, so it is off here).

**Evidence:** read 2026-11-15, region "Chiliad Challenge" (VER 18). It reads mission locals while any mission runs, so another mission's locals can trigger its splits (the total checkpoints becoming 19, say); once-per-reset limits each to one.

**Plan:** as porting.md; keep the reads exact for the comparison, and decide afterwards whether to gate the locals on `mtbiker` being the last started mission (a fix, outside the comparison). Adds the `VehicleChallenges` node.

**Verify:** the comparison passes; in LiveSplit, one route start to finish.

**Docs when done:** as porting.md, step 7.

## PRP — Properties

The ASL's region "Properties": settings `Properties` (on) with Assets, Los Santos, Badlands, San Fierro, Desert and Las Venturas nodes (on) and the safehouses, hotel suites and Zero's RC shop (on, listed alphabetically). The property-bought array has a flag per property; Wang Cars (index 0) and Verdant Meadows (index 2) are never set (the ASL's comment), so they are not here (WVP). The check reads every property every tick, whatever the settings, and returns the first that became 1.

Watchers: `$729` and `$731` to `$759` (`already_bought_house[1]` and `[3]` to `[31]`).

**Evidence:** read 2026-11-15, region "Properties". The ASL repo's item BLK notes the 30 reads a tick with Properties off.

**Plan:** as porting.md; keep the reads exact for the comparison. The watchers are read by no other check, so returning early while the Properties node is off changes nothing else; do that afterwards if it is worth it.

**Verify:** the comparison passes; in LiveSplit, buying one safehouse.

**Docs when done:** as porting.md, step 7.

## WVP — Wang Cars and Verdant Meadows purchases

Not in the live ASL. The old script had "Wang Cars (Showroom Bought)" and "Verdant Meadows (Airstrip Bought)" under Properties, splitting when the threads `steal` and `psch` start (dead `vars.startMissions`), because the property-bought array is not set for them. The ASL repo's porting notes put them under Assets.

**Evidence:** read 2026-11-15 (dead "mission lists" and "Side Missions" settings).

**Plan:** two settings under `PropertiesA` (Assets), split by registering the two threads (`engine.RegisterThread`). Check in the SCM that each thread starts only on buying.

**Verify:** tests with a fake reader; in LiveSplit, buying each.

**Docs when done:** as porting.md, step 7.

## QRY — Quarry

Not in the live ASL beyond the `Quarry` node (on). The old script had one split per quarry mission passed from `$8171` (`g_nQuarryMissionsPassed`, 1 to 7) and "Quarry Asset Completed" (off) from `$1493` (`done_quarry_progress`); all quarry missions run on thread `quarry`. The dead warp list has a death warp from Dam and Blast to Quarry (WRP).

**Evidence:** read 2026-11-15 (dead "mission lists", "Side Missions").

**Plan:** as the note above: one module with the seven missions told apart by the counter, as Trucking does, and the asset.

**Verify:** tests with a fake reader; in LiveSplit, two quarry missions and the asset.

**Docs when done:** as porting.md, step 7.

## VAL — Valet Parking

Not in the live ASL beyond the `Valet` node (on). The old script had "Valet Started", each of five levels complete, each car parked (3 to 7 a level, labelled by level and car), and the pass, from `$10933` (`valet_cars_to_park`: 0 to 3 started, 1 to 4 up to 1 to 8 a level complete), `$1848` (`total_cars_parked`, a rise splits that car) and `$1900` (`valet_mission_completed`); dead region "Valet Parking". Its thread test (`valet`) was commented out, and a level watcher at `+0x68F3B4` was commented out.

**Evidence:** read 2026-11-15.

**Plan:** as the note above; gate on the `valet` thread if the SCM shows the counters are touched elsewhere (the dead code notes `$1883` changes during 555 WE TIP).

**Verify:** tests with a fake reader; in LiveSplit, a level.

**Docs when done:** as porting.md, step 7.

## VIG — Vigilante

Not in the live ASL. The old script had "Vigilante Started" and "Vigilante Started after Learning to Fly" (thread `copcar`; the second also needs Learning to Fly passed), the levels from `$8227` (`copcar_print_level`: 1 started, 2 to 12 levels 1 to 11) and completion from `$1488` (`done_copcar_progress`, "Vigilante level 12 (Completion)"); dead region "Vigilante" and dead settings under Vehicle Submissions.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `VehicleSubmissions`, following Firefighter's shape (start, levels, pass). "After Learning to Fly" needs that mission's passed flag (`$87`, `pilot_test_passed`).

**Verify:** tests with a fake reader; in LiveSplit, the start and a level.

**Docs when done:** as porting.md, step 7.

## TAX — Taxi Driver

Not in the live ASL. The old script split on each fare from `$180` (`taxi_passed`: 1, then 2 to 49) and on completion from `$1491` (`done_taxiodd_progress`, "50 Taxi Fares dropped off (Completion)"); thread `taxiodd`. A "51+ fares" option came and went. The dead warp list has death warps from Boat School and from the Freeway race to Taxi Driver (WRP). The old Slamvan-before-Taxi route option (RSK) watched it.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `VehicleSubmissions`.

**Verify:** tests with a fake reader; in LiveSplit, a few fares.

**Docs when done:** as porting.md, step 7.

## PMD — Paramedic

Not in the live ASL. The old script had "Paramedic started" and levels from `$8211` (`db_ambulance_level`: 1 started, 2 to 12 levels 1 to 11) and completion from `$1487` (`done_ambulance_progress`, "Paramedic level 12 (Completion)"); thread `ambulan`. The dead warp list has a death warp from Lure to Paramedic (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `VehicleSubmissions`.

**Verify:** tests with a fake reader; in LiveSplit, the start and a level.

**Docs when done:** as porting.md, step 7.

## PMP — Pimping

Not in the live ASL. The old script had "Pimping started for the first time" from `$162` (`flag_player_on_pimp_mission`), levels 1 to 10 from the stat at `+0x779168` (its comment: "Pimping level stat (ID 210)?") and completion from `$1991` (`pimp_passed_once`); thread `pimp`.

**Evidence:** read 2026-11-15. The level stat's address is unconfirmed in the old script itself.

**Plan:** as the note above, under `VehicleSubmissions`; confirm the stat on the running game.

**Verify:** tests with a fake reader; in LiveSplit, the start and a level.

**Docs when done:** as porting.md, step 7; docs/architecture.md (game memory) for the stat.

## FRT — Freight

Not in the live ASL. The old script had "Freight started for the first time" (thread `freight`), each of stops 1 to 4 of each level from `130@` (the leak's `ft_stations_done`, at `+0x648B68`; a rise below 5), and each level's completion from `$8240` (`flag_freight_passed_0stime`) and `$8239` (`flag_freight_passed_1stime`, which goes to 2 at the end of the level), with tooltips naming the stations of a common 100% route; dead region "Freight" and commented-out settings.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `VehicleSubmissions`; the level comes from the first level's flag, as the old `vars.Passed` test did.

**Verify:** tests with a fake reader; in LiveSplit, a level's stops.

**Docs when done:** as porting.md, step 7.

## SRC — 8-Track and Dirt Track

Not in the live ASL's settings, though its Races check already handles races 25 and 26 (laps from `284@`, checkpoints from `268@`), building ids that have no settings. The old script had, under Stadium Events, each race's start, every checkpoint of every lap (12 laps for 8-Track, 6 for Dirt Track; the lap counter goes to -1 at the finish, so the last checkpoint has its own id), and "Race won" from `$2325` and `$2326` (`got_race_made_progress[25]`, `[26]`); dead region "Races" (the stadium part).

**Evidence:** read 2026-11-15.

**Plan:** settings under `StadiumEvents` with the ids RAC's check builds for races 25 and 26, so the check needs no change; port after RAC.

**Verify:** tests with a fake reader; in LiveSplit, a lap of each.

**Docs when done:** as porting.md, step 7.

## BLR — Blood Ring

Not in the live ASL. The old script had "Blood Ring Started" (thread `blood`) and "Blood Ring Passed" from `$1941` (`blood_passed_once`), under Stadium Events.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `StadiumEvents`. Adds the `StadiumEvents` node if SRC has not.

**Verify:** tests with a fake reader; in LiveSplit, the start and the pass.

**Docs when done:** as porting.md, step 7.

## KIK — Kickstart

Not in the live ASL. The old script had "Kickstart Started" (thread `kicksta`), each of 33 checkpoints from `$8250` (`player_checkpoint_kickstart`, a rise), "26 Points achieved" from `$8262` (`score_kickstart`, crossing 26) and "Kickstart Complete" from `$90` (`flag_kickstart_passed_1stime`); dead region "Kickstart", with no thread test.

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `StadiumEvents`; gate on the thread.

**Verify:** tests with a fake reader; in LiveSplit, the start, a few checkpoints, 26 points.

**Docs when done:** as porting.md, step 7.

## BMX — BMX stunt challenge

Not in the live ASL. The old script had "Challenge Started", each of 19 checkpoints and the completion, under Vehicle Challenges. Thread `stunt` runs both stunt challenges; `$726` (`stunt_course`) is 0 for BMX; the start is the timer `$8267` (`stunt_timer`) jumping by more than 10001; checkpoints are `59@` (the leak's `total_coronas_collected`) rising; completion is `$2795` (`done_bmx_stunt_progress`); dead region "Stunt Challenge (BMX / NRG-500)".

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `VehicleChallenges`; one module can hold both challenges (NRG shares the thread and the locals).

**Verify:** tests with a fake reader; in LiveSplit, the start and a few checkpoints.

**Docs when done:** as porting.md, step 7.

## NRG — NRG-500 stunt challenge

Not in the live ASL. As BMX with `$726` at 1, 18 checkpoints and completion from `$2796` (`done_nrg500_stunt_progress`), plus "Challenge Started after completion of Export List 1" and "after completion of Export List 3" (with tooltips), which need the export state (IEX).

**Evidence:** read 2026-11-15.

**Plan:** as BMX; the two export-dependent starts after IEX.

**Verify:** tests with a fake reader; in LiveSplit, the start and a few checkpoints.

**Docs when done:** as porting.md, step 7.

## DRS — Driving School

Not in the live ASL beyond the `Schools` node (on). The old script had "Driving School started for the first time" (thread `dskool`), each exercise from `$247` (`mission_selection`, the exercise just left: 1 The 360, 2 The 180, 4 Whip and Terminate, 5 Pop and Control, 7 Burn and Lap, 9 Cone Coil, 10 The '90', 11 Wheelie Weave, 13 Spin and Go, 14 P. I. T. Maneuver, 15 Alley Oop) and the pass, "City Slicking (Driving School Passed)", from `$86` (`driving_test_passed`); dead region "Schools". It is the story's Back to School, needed for San Fierro.

**Evidence:** read 2026-11-15.

**Plan:** as the note above. Adds the `Schools` node.

**Verify:** tests with a fake reader; in LiveSplit, the start, a few exercises.

**Docs when done:** as porting.md, step 7.

## BKS — Bike School

Not in the live ASL. The old script had "Bike School started for the first time" (thread `bskool`), each exercise from `$247` (shared with Driving School, told apart by the thread: 1 The 360, 2 The 180, 3 The Wheelie, 4 Jump & Stop, 5 The Stoppie) and the pass, "Jump & Stoppie (Bike School Passed)", from `$2201` (`flag_bikeschool_passed_1stime`).

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `Schools`.

**Verify:** tests with a fake reader; in LiveSplit, the start, a few exercises.

**Docs when done:** as porting.md, step 7.

## BTS — Boat School

Not in the live ASL. The old script had "Boat School started for the first time" (thread `boat`), each test from `$8189` (`boat_mission_selection`, starting at 1: 2 Basic Seamanship, 3 Plot a Course, 4 Fresh Slalom, 5 Flying Fish) and the pass, "Land, Sea and Air (Boat School Passed)", from `$1969` (`boat_passed_once`). The dead warp list has death warps from Boat School to Yay Ka-Boom-Boom and to Taxi Driver (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above, under `Schools`.

**Verify:** tests with a fake reader; in LiveSplit, the start, a test.

**Docs when done:** as porting.md, step 7.

## IEX — Import and Export

Not in the live ASL beyond the `ImportExport` node (on). The old script had each list complete (lists 1 and 2 from `$1049`, `current_wanted_list`; list 3 from `$1184`, `impexp_is_complete`), each vehicle exported, by name per list (from `$1060` to `$1069`, `current_wanted_car_status[0]` to `[9]`, which the next list reuses: a 1 to 0 change means the list moved on), and an exported-vehicle count 1 to 30 (from a stat at `+0x779174`, commented out; "Exported 30 Vehicles" on by default); dead region "Import/Export Lists". The dead warp list has several warps between exported vehicles (WRP).

**Evidence:** read 2026-11-15. The per-vehicle settings were unfinished in the old script (the ASL repo's notes).

**Plan:** as the note above; decide whether the per-vehicle splits are wanted before porting them; the list-complete splits first.

**Verify:** tests with a fake reader; in LiveSplit, a vehicle exported and a list completed.

**Docs when done:** as porting.md, step 7.

## SHR — Shooting Range

Not in the live ASL beyond the `ShootingRange` node (on). The old script listed the rounds (Pistol, Micro SMG, Shotgun and AK47, three rounds each) on `$1861` as a counter, and "Shooting Range Complete" from `$5272` (`player_has_fast_reload`); thread `shrange`. The leak calls `$1861` `SR_skill_award_given`, which does not sound like a round counter. The dead warp list has a death warp from the BF Injection export to the completion (WRP).

**Evidence:** read 2026-11-15.

**Plan:** as the note above; confirm what `$1861` counts.

**Verify:** tests with a fake reader; in LiveSplit, a round and the completion.

**Docs when done:** as porting.md, step 7.

## GYM — Gym moves

Not in the live ASL beyond the `GymMoves` node (on). The old script had, for each of the Los Santos, San Fierro and Las Venturas gyms, the gym entered (threads `gymls`, `gymsf`, `gymlv`), the fight start (`44@` becoming 1; the leak's `gym_teach`, `gymsf_teach` and `gymlv_teach`) and the moves learnt (`$8153` `gym_ls_defeated`, `$8154` `gym_sf_defeated`, `$8158` `gym_lv_defeated`, on by default); dead region "Gym Moves".

**Evidence:** read 2026-11-15.

**Plan:** as the note above, three gyms in one module.

**Verify:** tests with a fake reader; in LiveSplit, one gym's three splits.

**Docs when done:** as porting.md, step 7.
