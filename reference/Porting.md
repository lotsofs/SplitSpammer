# Porting the GTA SA rewrite (reference)

> Reference copy, moved from livesplit-asl's docs/Porting.md on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

Detail for the porting rows of [TODO.md](TODO.md).

GTA SA is moving to the SplitSpammer component (item SPS, `D:\Repositories\SplitSpammer`). These lists stay the source of what is left to port, but new ports go there, not into `GTASA.asl`.

The source for every item here is the dead code in `GTASA.asl`: `#region OLD_SHIT` at the end of `startup`, the tail of `init` after its bare `return;`, and `#region Splits OLDDDDD` in `split`. That is the fork's script as it stood on 2024-06-24 (`git show f7885d5:GTASA.asl` is the last version in which all of it ran), with Tduva's original underneath. Watcher names there (`vars.watchers[...]`, `vars.Passed(...)`) don't exist in the live code; port the logic, not the calls. The way a mission or side mission is laid out in the live code is described in CLAUDE.md under "How a mission is defined".

Dependencies: story chains (SFM, DSM, LVM, RTM) should be ported in game order, because `ValidateMissionProgress` keys on each chain's `$..._TOTAL_PASSED_MISSIONS` counter and on `vars.lastStartedMission`, and because the death and busted warps between missions need both ends ported. The side-mission items are independent of each other and of the story. When an item is ported, delete its dead code; when the dead regions are empty, delete them and the early returns, and update CLAUDE.md's "Live code / dead reference code" passage.

For each item, **Verify** means: with the game running and debug output on, play (or load saves around) the content and see each enabled split fire once, not fire when disabled, and not fire again after loading an earlier save. **Docs when done** for each item: CHANGELOG.md Unreleased (Added), the README GTA SA section (RDM), and this doc's dependency note if it changes.

## SFM — San Fierro story missions

The settings tree has `SF` with `SF_Main` (Garage / Syndicate), `SF_Wuzimu` (Woozie) and `SF_Zero` (Zero), and nothing under them. The dead `vars.missionChains` lists the missions of these chains by counter: `$GARAGE_TOTAL_PASSED_MISSIONS` (541), `$SYNDICATE_TOTAL_PASSED_MISSIONS` (545), `$CRASH_SF_TOTAL_PASSED_MISSIONS` (546), `$WUZIMU_TOTAL_PASSED_MISSIONS` (543) and `$ZERO_TOTAL_PASSED_MISSIONS` (542), and `vars.startMissions` the thread name that starts each. The old code had no mid-mission events for San Fierro beyond the gym (GYM).

**Evidence:** read 2026-11-13.

**Plan:** one `#region` per mission: `vars.RegisterMissionBasics(abbr, name, thread, parent)`, a `_pass` setting, a `CheckSplit` func that calls `vars.ValidateMissionProgress(thread, chainWatcher, indexBefore, indexAfter, abbr+"_pass", abbr)`. Mid-mission events as found in the SCM dumps. Also port the San Fierro deathwarp the first commit had as "[Deathwarp SF]" into the Warps group.

## DSM — Desert story missions

`Des_Toreno` and `Des_WangCars` are empty headers. Chains in the dead code: `$TORENO_TOTAL_PASSED_MISSIONS` (593, Monster onwards; it continues into the Las Venturas Air Strip missions, see LVM) and `$STEAL_TOTAL_PASSED_MISSIONS` (544, Zeroing In through Puncture Wounds). The Properties group's Assets list has only Zero's RC shop; the old script also split on buying the Wang Cars showroom and the Verdant Meadows airstrip, which belong here or under Properties.

**Evidence:** read 2026-11-13.

**Plan:** as SFM. Add the two property purchases to `PropertiesA`, using the property addresses the other `PropertyN` settings use.

## LVM — Las Venturas story missions

`LV_AirStrip`, `LV_Casino`, `LV_Crash`, `LV_MaddDogg` and `LV_Heist` are empty headers. The dead `vars.missionChains` covers `$CASINO_TOTAL_PASSED_MISSIONS` (597, Fender Ketchup through Saint Mark's Bistro), `$598` (Misappropriation, High Noon), `$599` (Madd Dogg) and `$HEIST_TOTAL_PASSED_MISSIONS` (600, Architectural Espionage through Breaking the Bank at Caligula's); the Air Strip missions (Monster through Learning to Fly) count on the Toreno chain (593) shared with DSM.

**Evidence:** read 2026-11-13.

**Plan:** as SFM.

## RTM — Return to Los Santos story missions

`RTLS_Mansion`, `RTLS_Grove` and `RTLS_Riot` are empty headers (`rtls_mansion_chain`, $626, is already watched). Chains: `$MANSION_TOTAL_PASSED_MISSIONS` (626), `$GROVE_TOTAL_PASSED_MISSIONS` (627), `$RIOT_TOTAL_PASSED_MISSIONS` (629). The dead split code has mid-mission events for Cut Throat Business (`ctb_checkpoint1`/`2`: arriving at the video shoot and at the pier), Grove 4 Life (arriving in Idlewood, first and second territory, from locals 56@, 58@, 54@) and End of the Line part 3 (three chase cutscenes from $8014, with "End of any%: Start of firetruck bridge cutscene" on by default). The gang-territory splits during RTLS (`GT_RTLS`) are already live.

**Evidence:** read 2026-11-13.

**Plan:** as SFM, plus the three event sets. Keep the end-of-any% split on by default; it is the one split most players want.

## VSM — Vehicle submissions other than Firefighter

`VehicleSubmissions` holds only `firefighter`. The 2024 script also had Vigilante (level splits, "Vigilante Started", "Vigilante Started after Learning to Fly", dead region "Vigilante"), Taxi Driver (fares; a "51+ fares" option came and went), Paramedic, Pimping (`pimping_level`) and Burglary. Their old settings were level lists built in loops (`vigilante_level`, `pimping_level`, …) and their watchers sat in the dead tail of `init`.

**Evidence:** read 2026-11-13.

**Plan:** one `#region` each under `VehicleSubmissions`, following the Firefighter block (start, per-level, pass). Taxi and the Slamvan route interact with SKP.

## FRT — Freight

No header and no live code. The old script split on Freight started and on each stop of levels 1 and 2 ("Freight Station 1 1" … "Freight Station 2 4") from `freight_stations` (0x648B68); dead region "Freight".

**Evidence:** read 2026-11-13.

**Plan:** a `freight` block under `VehicleSubmissions`.

## SCH — Schools

`Schools` is an empty header. The old script split per exercise of Driving School ("The 360", "The 180", "Whip and Terminate", "Pop and Control", "Burn and Lap", "Cone Coil", "The '90'", "Wheelie Weave", "Spin and Go", "P. I. T. Maneuver", "Alley Oop"), Bike School ("The 360", "The 180", "The Wheelie", "Jump & Stop", "The Stoppie"), Boat School and Pilot School, plus "started for the first time" for each, from `schools_currentexercise` ($247); dead region "Schools". Bike school and driving school share the variable and were told apart by the current thread.

**Evidence:** read 2026-11-13.

**Plan:** four blocks under `Schools`, one func keyed on `vars.lastStartedMission`.

## STD — Stadium events

`StadiumEvents` is an empty header. Old: Blood Ring started; Kickstart started, each checkpoint, 26 points (`kickstart_checkpoints` $8250, `kickstart_points` $8262); 8-Track and Dirt Track as races 25 and 26 with lap and checkpoint splits (`races_stadiumcheckpoint`, `races_laps`); dead regions "Kickstart" and the stadium part of "Races".

**Evidence:** read 2026-11-13.

**Plan:** blocks under `StadiumEvents`; the two stadium races can reuse the live Races code if it exposes laps.

## VAL — Valet Parking

`Valet` is an empty header. Old: Valet started, per level, cars parked (`valet_carstopark` $10933, `valet_carsparked` $1848, `valet_level` 0x68F3B4); dead region "Valet Parking".

**Evidence:** read 2026-11-13.

**Plan:** one block under `Valet`.

## QRY — Quarry missions

`Quarry` is an empty header. Old: the `$QUARRY_TOTAL_PASSED_MISSIONS` (8171) chain in `vars.missionChains`, one split per quarry mission passed.

**Evidence:** read 2026-11-13.

**Plan:** one block, a chain-counter func like a story chain.

## GYM — Gym moves

`GymMoves` is an empty header. Old: for each of the Los Santos, San Fierro and Las Venturas gyms, gym entered, fight start (`gym_fighting` 44@ with threads gymls/gymsf/gymlv) and moves learnt; dead region "Gym Moves".

**Evidence:** read 2026-11-13.

**Plan:** three blocks under `GymMoves`.

## IEX — Import and Export lists

`ImportExport` is an empty header. Old: a split per export list completed (1–3) and per vehicle exported (`export0`–`export9` at 0x64A9F0 + i*4), dead region "Import/Export Lists". The per-vehicle settings ("Export Number 1" …) were commented out in the old mission lists, so that part was never finished. The NRG-500 challenge's "started after export list 1/3" options (STC) depend on list state.

**Evidence:** read 2026-11-13.

**Plan:** one block; decide whether per-vehicle splits are wanted before porting them.

## STC — BMX and NRG-500 stunt challenges

`VehicleChallenges` holds only `mtbiker` (Chiliad). Old: BMX and NRG-500 challenge started (NRG also "after export list 1" and "after export list 3"), checkpoints and completion from `stunt_type` ($726), `stunt_timer` ($8267), `stunt_checkpoint` (59@); dead region "Stunt Challenge (BMX / NRG-500)".

**Evidence:** read 2026-11-13.

**Plan:** two blocks under `VehicleChallenges`, following the Chiliad block.

## SHR — Shooting Range

`ShootingRange` is an empty header. Old: the `$1861` chain in `vars.missionChains` ("Pistol Round 1" … per weapon and round).

**Evidence:** read 2026-11-13.

**Plan:** one block, chain-counter func.

## GFS — Girlfriends

No header. Old: each girlfriend unlocked (bits of `$GIRLS_GIFTS_BITMASK`, 406) and progress (`$GIRL_PROGRESS[0..5]`, 359–364), under Other; the unlocked part is in the dead split code.

**Evidence:** read 2026-11-13.

**Plan:** a `Girlfriends` block under `Other`.

## HND — 100 percent achieved

No setting. Old: "100% Achieved" under Other from `$_100_PERCENT_COMPLETE` (802), in the dead split code next to the girlfriends.

**Evidence:** read 2026-11-13.

**Plan:** one setting and a one-line func under `Other`.

## SKP — Split skipping for non-linear routes

The "Split Skipping" group ("Route: GI > LS > HP > C > RUS > RTF", with Catalyst-first and Slamvan-before-Taxi variants commented out) is registered after the early `return;` in `startup`, so it never appears, and the trigger at the end of the dead split code never runs. The mechanism it drives is live: when `vars.skipSplits` is set, the queue calls `SkipSplit()` instead of splitting until it is cleared. The old trigger set it on starting House Party or Catalyst before Los Sepulcros (after Gray Imports) and cleared it on Reuniting the Families, using `vars.Passed`.

**Evidence:** read 2026-11-13.

**Plan:** move the settings before `#region OLD_SHIT` (under its own top-level group, as before) and add a `CheckSplit` func that watches `thread` and the chain counters (`ls_crash_chain`, `ls_sweet_chain`) instead of `vars.Passed`. Port the commented-out route variants only if someone asks.
