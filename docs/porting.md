# Porting a mission from the ASL

How one mission (or side mission, collectible group, …) moves from `reference\GTASA.asl` into the component. Each mission has its own item in [TODO.md](TODO.md), with what is particular to it; this doc holds what they share. Read `reference\README.md` first for how the ASL and the two SCM dumps are laid out, and CLAUDE.md's "Adding a mission" for the file layout.

## Before starting

- Do the groundwork items the mission's section names (see the dependency note at the top of [Groundwork.md](Groundwork.md)). TST gives the comparison harness.
- Open the mission's region in the ASL (the item names it) and the helpers it calls (`RegisterMissionBasics`, `ValidateMissionProgress`, `TrySplit`, in `startup` and `init`). `StoryMission` (`UI\Components\Missions\StoryMission.cs`) is the component's copy of the first two.
- Item LIC still applies: the logic comes from a script with no license.

## Steps

1. **Settings.** One `IModule` file in `UI\Components\Missions\`, added to `Modules.All` at the mission's place in the ASL's order (that order is also the order of the checks, so it decides which split is queued first when two fire in one tick). Add the settings in the ASL's order with the ASL's label, default and tooltip, and ids of our own (CLAUDE.md, Conventions). A story mission keeps a `static readonly StoryMission` (id, label, thread, parent, and the marker text and the mission's tooltip when they differ: the ASL labels Drive-Thru "Drive-Thru" and its marker text is "Drive-thru"): `AddBasicSettings` adds the mission (on) with "Mission Marker Entered" and "Mission Started" (off), as `RegisterMissionBasics` or the same lines written out; then the mission's events; then `AddPassedSetting` ("Mission Passed", on), which the ASL always adds last. A mission whose names don't fit (a second marker text, a thread shared by two missions or registered with no id) uses the ids and adds the rest by hand. The ASL's group nodes (`LS_Sweet`, `BL_Catalina`, `SideMissions`, …) keep their ASL ids and go in `SettingsTree.Build` in the ASL's order, added by the first item that puts something under them. A label with an obvious typo may be fixed; say so in the item's log row.
2. **Watchers.** Name each SCM watcher with its exact leak name: globals from the table below, locals from the candidates in the item's section (confirm them, see "Leak names" below). Register every watcher the module reads, including shared ones such as chain counters: registering an address that is already watched gives the same watcher, under the module's own name for it. Other watchers (not SCM variables) are camelCase.
3. **The check.** In `AddChecks`, the `StoryMission`'s `Register(engine)` registers the thread (splitting `_started`) and the marker text (splitting `_markerEntered`), and the ASL's `if (!vars.ValidateMissionProgress(thread, chain, before, after, pass, setting)) return;` becomes `if (!mission.InProgress(engine, "<chain's leak name>", before, after)) return null;` (`engine.MissionInProgress` takes all six arguments, for a check whose pass id or setting is not the mission's own). Copy the ASL func's order of reads and early returns exactly, including reads that look pointless: which watchers are read in a tick decides their Old value (STL). Keep the ASL's value tables (dialogue block numbers and so on) as comments. A `return ""` in the ASL ends the func like `return null` and splits nothing. Splits the func sends through `TrySplit` itself go through `engine.TrySplit`, in the same place. Some funcs build split ids from a counter (`"naak_round" + n + "Bottle" + k`) and can produce an id with no setting: the component counts an unknown id as disabled, but whether LiveSplit's ASL settings reader returns false or throws for one (which would end the ASL's split block for that tick) has not been checked; look it up in LiveSplit.Core the first time an item meets it, and record the answer here.
4. **ASL defects.** The item lists the defects found when reading the region. For each one, either copy it (the comparison stays exact) or fix it and leave that id, or that path, out of the comparison; say which in the item's log row. Also check every test of the last started mission for what happens once the mission has ended, since the engine then clears it and the ASL does not (Big Smoke's early return while it is `intro2`, for example). A fix to a defect that users of the ASL would notice belongs in the changelog entry.
5. **Compare with the ASL.** Transcribe the ASL func into the comparison harness from TST (the ASL's own func on LiveSplit.Core's real watchers, against the module on the same fake memory) and run scripted runs through the mission's events plus seeded random runs. Run them with the mission's setting on and off, because `ValidateMissionProgress` reads nothing when the mission's setting is off. The splits must come out equal, apart from the fixes chosen in step 4.
6. **See it in LiveSplit.** The user plays the mission (or loads saves around it) with the splits enabled: each fires once, at the moment its label says, not when disabled, and not again after loading an earlier save. If the port is finished before that has been seen, finish the item anyway and add the live check to a `LIV` checklist (create [Live.md](Live.md) and its index row with the first one, as CLAUDE.md's "Checklists under one code" describes).
7. **Docs.** In the same change: delete the item's index row and section and log it in [DONE.md](DONE.md); in [CHANGELOG.md](../CHANGELOG.md) under Unreleased, Added, extend the entry for that part of the game (one entry per city or side-content group, listing what can be split) instead of adding one per mission; update the README's Status paragraph; add any new non-SCM address to the Game memory table in [architecture.md](architecture.md).

## ASL behaviour the engine keeps

These are facts about the ASL runtime that the ports depend on. The groundwork items build them; a port relies on them.

- **Tick order:** the guards, then the thread step (read `thread`; when it changed to a thread other than the last started mission and that thread is registered, split its id if it has one and remember it as the last started mission), then the marker step (read `missionStartText`; when it changed to a registered mission name, split that id), then every check in order, then one queued split. A module registers its thread with `engine.RegisterThread(thread, id)` (a null id only remembers it) and its marker text with `engine.RegisterMissionName(text, id)`; registering one twice throws, as the ASL's dictionaries do. Both compare exactly, case included.
- **The last started mission** (`engine.LastStartedMission`) is cleared on a timer reset, and, **unlike the ASL**, at the end of the tick in which its thread is no longer in the game's script list (passed, failed, or a save loaded). The checks still see it in that tick, and a passed mission's counter is already written when its thread leaves the list (seen on the running game, 2026-11-15: Big Smoke's `$448` and the end of `intro1` in the same millisecond), so the pass still splits. From the next tick a mission's checks read nothing, where the ASL's went on reading mission locals that the next mission script overwrites. `engine.IsRunning(thread)` asks whether any script of that name is running.
- **String watchers** (`thread`, `missionStartText`, `oddMissionStartText`, added by the engine) copy LiveSplit's `StringWatcher`: Old is null after the first read, and a failed read (a null pointer too) leaves both Old and Current alone, unlike an int watcher, so a change across a failed read is seen on the next good one.
- **`ValidateMissionProgress(thread, chain, before, after, passId, settingId)`**, copied as `engine.MissionInProgress` with the same arguments: false without reading anything when `settingId` is off; false when the last started mission is not `thread`; otherwise reads the chain counter, and when it is at or past `after` splits `passId` if it just changed from `before`, and returns false; true only while the mission is active and not passed. With the last started mission cleared when its thread ends, a failed mission's check reads nothing from the next tick on.
- **`settings[id]`** counts a setting only when it and all its ancestors are checked, as `SettingsSnapshot.IsEnabled` does. A check that tests a group setting (Collectibles, the Specific nodes) therefore reads less while that group is off.
- **Shared watchers:** one watcher per address (`WatcherSet` keeps this whatever names the modules give it). Mission locals are shared by every mission that reads the same `n@`, so the first read in a mission can see an Old from another mission, and report Changed where a fresh watcher would not (STL). The leak names a local differently in each mission (`46@` is `intro1_chat_switch` in Big Smoke and `gang_hassle` in Tagging up Turf). Locals the live ASL reads in more than one mission, from its `watchScmMissionLocalVariables` lists and `GetWatcher` calls (read 2026-11-15): `34@` (CVP, BDS, RUS, HSL, LLS), `35@` (CVP, BDS, HIN, RUS, LLS, TNK), `36@` (NAK, TRK), `40@` (RYD, TUT), `46@` (BSM, TUT), `48@` (TUT, NAK), `80@` (WST, HPY), `87@` (CTH, WST, AYG), `88@` (WST, HIN, AYG), `89@` (WST, CTL), `90@` (WST, JBZ, AAO), `97@` and `99@` (STB, AAO), `103@` (CTL, AAO), `106@` (RTF, TRK), `122@` (MGI, LLS), `127@` (SWG, RUS), `128@` (WST, RTF, BHV). Chain counters are read by every mission of their chain and by gang territories and warps.
- **One split per tick, once per reset**, and the queue order is the order of the checks (already in the engine).

## Leak names

**Globals** come from the "GTASA Global Variable Table 2022" sheet (`reference\README.md` explains it), "PC_1.00" tab. The globals the live ASL reads, looked up there on 2026-11-15:

| `$n` | ASL watcher | Leak name | Read by |
|---|---|---|---|
| 24 | `intro_passed` | `game_starts_from_scratch` | engine guards, timer control, ITB (ported) |
| 1510 | `intro_newGameStarted` | `para_pickup_flag` | engine guards (ported) |
| 5353 | `intro_cutsceneState` | `cutscene_skip_CUT` | ITB (ported) |
| 54 | `intro_groveStreet` | `voice_over_at_hub` | ITB (ported), GSH |
| 43 | `interior` | `main_visible_area` | BSM, RYD |
| 448 | `ls_intro_chain` | `flag_intro_mission_counter` | BSM, RYD |
| 65 | `bs_houseHelp` | `funeral_mission_finished` | BSM |
| 169 | `r_onMission` | `flag_player_on_menace_mission` | RYD |
| 676 | `r_barberBought` | `flag_changed_hair_intro2` | RYD |
| 1514 | `r_fail` | `can_skip_smoking_cutscene` | RYD |
| 452 | `ls_sweet_chain` | `flag_sweet_mission_counter` | TUT to LSP, GTR, RSK |
| 6637 | `db_stage` | `sw4_stage` | DBY |
| 6640–6655 | `db_kill0`–`15` | `sw4_hood_dead[0]`–`[15]` | DBY |
| 2577 | `d_cut1` | `flag_gang_war_active` | DOB |
| 2414 | `d_cut2` | `played_cut2_before_crash4` | DOB |
| 454 | `ls_smoke_chain` | `flag_smoke_mission_counter` | OGL, RDG, WST, JBZ |
| 6898 | `jb_atriumStage` | `smokeshoot_s4flag` | JBZ |
| 455 | `ls_ogloc_chain` | `flag_strap_mission_counter` | LAB, MDR, MGI, HPY |
| 456 | `ls_crash_chain` | `flag_crash_mission_counter` | BDS, GRI, GTR, RSK |
| 453 | `ls_ryder_chain` | `flag_ryder_mission_counter` | HIN, CTL, RUS |
| 457 | `ls_cesar_chain` | `flag_cesar_mission_counter` | HSL |
| 2336 | `hslr_raceStart` | `cs1_race_is_go` | HSL |
| 458 | `ls_final_chain` | `flag_la1fin1_mission_counter` | RTF, TGS, GTR |
| 493 | `bl_intro_chain` | `flag_bcrash_mission_counter` | BDL, WRP |
| 2208 | `bl_tripSkipUnlocked` | `flag_bcr1_skip` | BDL |
| 6965 | `bl_chapter` | `bc1_stage` | BDL |
| 2163 | `bl_kie_chain` | `flag_trailor_cutscene` | KIE, WRP |
| 64 | `catalina_count` | `cat_counter` | CDT, WRP |
| 714 | `lls_pass` | `flag_cat_mission1_passed` | LLS |
| 715 | `stb_pass` | `flag_cat_mission2_passed` | STB |
| 716 | `tc_pass` | `flag_cat_mission3_passed` | TNK |
| 717 | `aao_pass` | `flag_cat_mission4_passed` | AAO |
| 6987 | `lls_pickupsGrabbed` | `cat1_pickups_collected` | LLS |
| 492 | `bl_cesar_chain` | `flag_bcesar_mission_counter` | WZM |
| 491 | `bl_truth_chain` | `flag_truth_mission_counter` | BHV, AYG, WRP |
| 7011 | `truth2_plantsRemaining` | `t2_total_weed_burnt` | AYG |
| 626 | `rtls_mansion_chain` | `flag_mansion_mission_counter` | GTR, AHH, VBD, HMC, CTB |
| 189 | `courier_active` | `flag_courier_trigger` | CUR |
| 1992 | `courier1_pass` | `courierLA_passed_once` | CUR |
| 1993 | `courier3_pass` | `courierLV_passed_once` | CUR |
| 1994 | `courier2_pass` | `courierSF_passed_once` | CUR |
| 8159 | `trucking_totalPassed` | `g_nTruckMissionsPassed` | TRK |
| 1489 | `firefighter_passed` | `done_firetruck_progress` | FFT |
| 8213 | `firefighter_currentLevel` | `db_firetruck_level` | FFT |
| 8214 | `firefighter_firesExtinguished` | `db_fires_extinguished` | FFT |
| 352 | `race_index` | `race_selection` | RAC |
| 2300–2326 | `race0`–`race26` | `got_race_made_progress[0]`–`[26]` | RAC, SRC |
| 1799 | `mtbiker_nextRace` | `mtbikerace_selection` | CHL |
| 1801 | `mtbiker_complete` | `flag_mtbike_passed_1stime` | CHL |
| 729–759 | `Property1`–`Property31` | `already_bought_house[1]`–`[31]` | PRP |
| 1519 | `TagAll` | `flag_returned_tags` | TAG |
| 1516 | `OysterAll` | `returned_oysters_flag` | PKP |
| 1517 | `HorseshoeAll` | `flag_returned_shoehorses` | PKP |
| 1518 | `SnapshotAll` | `flag_returned_snapshots` | PKP |
| 1342–1349 | `PhoneCall6`, `12`, `7`, …, `9` | `flag_mob_la1[0]`–`[7]` | PHC |
| 1356–1363 | `PhoneCall8`, `15`, …, `16` | `flag_mob_cat[0]`–`[7]` | PHC |
| 1371 | `PhoneCall11` | `flag_mob_random[5]` | PHC |

Where the leak name disagrees with the ASL's watcher name (`$65`, `$1514`, `$2577`, `$2163`), the ASL's name says what the variable was seen to do and the leak's what it was written for; the item says which. An array element's watcher is named with its index as the leak and the table write it, brackets included (`"sw4_hood_dead[3]"`). The dead code's globals are in the items that use them.

**Locals** are numbered in declaration order from `34@` in the mission's block of `reference\scm_leak.txt` (`reference\README.md`). The items list candidates computed that way on 2026-11-15, counting arrays by their size (named sizes through the leak's `CONST_INT` values) and text labels as two. They agree with the ASL's own comments for Los Sepulcros, Wrong Side of the Tracks, Gray Imports, Home Invasion, Catalyst and Robbing Uncle Sam, but not for Burning Desire, where the count is two too high by `209@`: treat every candidate as a guess until the leak's use of it (what it is compared with, where it is set) matches what the ASL's check expects.

**The leak is an earlier revision than the shipped game.** Some missions have another script name there than the thread the game runs (`script_name` in `reference\main.scm.txt`, which is the shipped one). The blocks are in the shipped mission table's order, which gives this mapping:

| Game thread | Leak `SCRIPT_NAME` |
|---|---|
| `hoods5` (Sweet's Girl) | `sweet5` |
| `twar7` (OG Loc) | `smoke1` |
| `drugs1` (Just Business) | `smoke4` |
| `music1`, `music2`, `music3` | `strap1`, `strap2`, `strap3` |
| `drugs3` (Gray Imports) | `crash3` |
| `drugs4` (Reuniting the Families) | `la1fin1` |
| `decon` (Deconstruction) | `garag2` |
| `scrash3` (555 WE TIP) | `scrash1` |
| `driv6` (Lure) | `farlie5` |
| `wuzi5` (The Da Nang Thang) | `wuzi4` |
| `vcr2` (High Noon) | `vcrash2` |
| `zero1`, `zero2`, `zero4` (Air Raid, Supply Lines, New Model Army) | only `ZERO2` and `ZERO4` in that place; which game mission each belongs to is not known |

Every other mission thread is the leak's script name. The ASL compares thread names in lower case (`"intro2"`), though both dumps write most of them in capitals. In memory the name is lower case with nothing after it (reproduced 2026-11-15: `"introst" -> "intro1"` when Big Smoke started), so register threads in lower case. Unregistered scripts such as `noname`, `gfagnt`, `a_cont` and `dealer` also come and go as the first thread, which is why only registered ones count. The engine logs every change of `thread` and `missionStartText` to Trace; capture it with DebugView to see a mission's names.
