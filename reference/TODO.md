# GTA SA items (reference)

> Reference copy, moved from livesplit-asl's docs/TODO.md (the index rows) on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

The Verification row also covered Far Cry 2 and Serious Sam 2; those checks stayed in livesplit-asl.

## Defects and tasks — [Defects.md](Defects.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [DSP](Defects.md#dsp--double-split-prevention-blocks-splits-instead-of-spacing-them) | GTA SA splitting | With "Double-Split Prevention" on, the cooldown test is inverted: a split more than 2.5 s after the previous one is blocked and only near-simultaneous ones pass. On master since 2025-08-31 | Medium |
| [SVL](Defects.md#svl--start-and-reset-on-save-load-settings-do-nothing) | GTA SA settings | "Start timer when loading save" and "Reset timer when loading save" are shown under Settings, but the code behind them is commented out and the `loading` watcher it needs isn't created | Medium |
| [LDG](Defects.md#ldg--the-after-load-split-guard-never-arms) | GTA SA splitting | The 600 ms no-split guard after loading a save never arms because nothing sets `vars.lastLoad`; only the new-game playing-time check remains | Medium |
| [STL](Defects.md#stl--lazily-read-watchers-compare-against-their-last-read-not-the-last-tick) | GTA SA splitting | A watcher read only while a mission is active compares its first read after a gap with the value from its previous read, so a stale `Old` can match an `Old == X && Current == Y` check and split on nothing; mission locals, which every mission shares, are the exposed case | Medium |
| [TBL](Defects.md#tbl--mission-blocks-repeat-the-same-split-check-shape-by-hand) | GTA SA code size | Every story mission hand-writes the same split-check shapes (value becomes B, transition A to B, switch on the new value) in a 50 to 70 line func; a per-mission table and one generic func would shrink the ~2,900 lines of mission regions and every mission still to port | Medium |
| [BLK](Defects.md#blk--per-slot-watchers-cost-a-memory-read-and-a-list-scan-each) | GTA SA performance | Every `GetWatcher` call scans the ~1,400-entry watcher list by name and does one process read; the per-slot collectible, tag, stunt-jump and territory loops do that up to ~1,200 times per tick when their Specific settings are on, and the properties and phone-call funcs read 30 watchers a tick without checking their group setting | Low |

## Porting the GTA SA rewrite — [Porting.md](Porting.md)

Features of the 2024-06-24 script that the rewrite (on master since 2025-07-30) doesn't have yet. Their old logic is still in the file as dead code.

| Code | Area | Summary | Severity |
|---|---|---|---|
| [SFM](Porting.md#sfm--san-fierro-story-missions) | Story missions | San Fierro (Garage / Syndicate, Woozie, Zero) exists only as empty settings headers; the dead code lists its missions by chain counter ($541, $545, $546, $543, $542) | High |
| [DSM](Porting.md#dsm--desert-story-missions) | Story missions | Desert (Toreno $593, Wang Cars $544) headers are empty; the Wang Cars showroom and Verdant Meadows airstrip purchases are also missing from Properties | High |
| [LVM](Porting.md#lvm--las-venturas-story-missions) | Story missions | Las Venturas (Air Strip on the Toreno chain, Casino $597, C.R.A.S.H. $598, Madd Dogg $599, Heist $600) headers are empty; the dead code lists all their missions | High |
| [RTM](Porting.md#rtm--return-to-los-santos-story-missions) | Story missions | Return to Los Santos (Mansion $626, Grove $627, Riot $629) headers are empty; the dead code has mid-mission events for Cut Throat Business, Grove 4 Life and End of the Line, and the old default-on "End of any%" split | High |
| [VSM](Porting.md#vsm--vehicle-submissions-other-than-firefighter) | Side missions | Vehicle Submissions holds only Firefighter; the old Vigilante, Taxi, Paramedic, Pimping and Burglary splits are dead | Medium |
| [FRT](Porting.md#frt--freight) | Side missions | Freight (started, each stop of each level) has no header and no live code | Medium |
| [SCH](Porting.md#sch--schools) | Side missions | Schools header is empty; the old per-exercise splits for Driving, Bike, Boat and Pilot school are dead | Medium |
| [STD](Porting.md#std--stadium-events) | Side missions | Stadium Events header is empty; Blood Ring, Kickstart (checkpoints, 26 points), 8-Track and Dirt Track splits are dead | Medium |
| [VAL](Porting.md#val--valet-parking) | Side missions | Valet Parking header is empty; the level and cars-parked splits are dead | Medium |
| [QRY](Porting.md#qry--quarry-missions) | Side missions | Quarry header is empty; the $8171 chain splits are dead | Medium |
| [GYM](Porting.md#gym--gym-moves) | Side missions | Gym Moves header is empty; gym entered, fight start and moves learnt for the three gyms are dead | Medium |
| [IEX](Porting.md#iex--import-and-export-lists) | Side missions | Import/Export header is empty; per-list and per-vehicle export splits are dead, and the per-vehicle settings were unfinished even in the old code | Medium |
| [STC](Porting.md#stc--bmx-and-nrg-500-stunt-challenges) | Side missions | Vehicle Challenges holds only Chiliad; BMX and NRG-500 stunt challenge splits are dead | Medium |
| [SHR](Porting.md#shr--shooting-range) | Side missions | Shooting Range header is empty; the Ammu-Nation round splits ($1861) are dead | Low |
| [GFS](Porting.md#gfs--girlfriends) | Other | Girlfriend unlocked and progress splits are dead and have no header | Low |
| [HND](Porting.md#hnd--100-percent-achieved) | Other | The "100% Achieved" split is dead and has no setting | Low |
| [SKP](Porting.md#skp--split-skipping-for-non-linear-routes) | Splitting | The "Split Skipping" settings and the route-deviation trigger sit after the early return and never run, though the split queue still honours `vars.skipSplits` | Low |

## Verification — [Verify.md](Verify.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [VER](Verify.md#ver--checks-not-yet-seen-working) | All scripts | Numbered checklist of things built but not recorded as seen working: the GTA SA missions and side content added since the last commit, variables the comments call uncertain, the Far Cry 2 v1.00 addresses, Serious Sam 2 build 252822 | Medium |

## Project and README — [Project.md](Project.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [SPS](Project.md#sps--move-gta-sa-to-the-splitspammer-component) | GTA SA | GTA SA auto splitting is moving to the SplitSpammer LiveSplit component (own repo, collapsible settings); `GTASA.asl`, the Rust `gtasa/` spike and the GTA SA items here stay until it reaches parity, then go | Medium |
| [RDM](Project.md#rdm--readme-has-no-gta-san-andreas-section) | README | No GTA San Andreas section: supported game versions, what splits exist, the state of the rewrite and the settings are undocumented for players | Medium |
