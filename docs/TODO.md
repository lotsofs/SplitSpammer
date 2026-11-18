# TODO

Open work in this repo, by three-letter code. Each code links to its section in the detail doc named in the group heading: what is wanted, the evidence, a plan, how to verify it, and which docs to update when it is done. Within a group, rows are in suggested order of work. Severity is High, Medium or Low.

When an item is finished, delete its row here and its section in the detail doc, and log it in [DONE.md](DONE.md). When work turns up a defect that isn't being fixed now, add both. A change users would notice also goes under Unreleased in [CHANGELOG.md](../CHANGELOG.md).

The ASL's content is ported one item at a time: one item per mission, side mission or group of splits. [porting.md](porting.md) says how, and what every port relies on; the groundwork items come first.

## Component — [Component.md](Component.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [TST](Component.md#tst--headless-test-project) | Tests | No test project: an xunit project that drives the component headless with a fake game reader and LiveSplit's timer model, and the harness that compares a ported mission with the ASL's logic | High |
| [GSH](Component.md#gsh--split-at-the-start-of-the-grove-street---home-lines) | Missions | The `intro_groveStreetLinePlayed` split fires when the second "Grove Street - Home" line starts, because `$54` is only set then: make it fire when the first line starts, probably by watching the subtitle text with a string watcher, and fix its label | Low |
| [SLD](Component.md#sld--start-and-reset-the-timer-when-loading-a-save) | Timer control | The ASL's "Start/Reset timer when loading save" options are not ported (they never worked in the live script); the recent-update ASL does the start one from the loading byte at `+0x7A67A5`, so both can be added that way, off by default | Low |
| [LRM](Component.md#lrm--load-removal) | Timer control | No load removal: GTA SA is timed in real time and the ASL has no isLoading block; if a loadless category ever wants it, pause game time through `IsGameTimePaused` and `SetGameTime` | Low |

## Porting groundwork — [Groundwork.md](Groundwork.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [PTW](Groundwork.md#ptw--pointer-watchers-and-the-other-always-present-watchers) | Engine | No pointer watchers: `pedStatus` and `weaponSlotMelee` (behind `+0x77CD98`) and the stunt-jump records need them, for Against All Odds, the warps and stunt jumps | Medium |
| [STL](Groundwork.md#stl--stale-old-after-a-gap-kept-from-the-asl) | Engine | Watchers copy the ASL's stale Old on purpose: a watcher read after a gap compares with its last read, possibly a mission ago, and can split on nothing; fix it once the ports are compared, in the engine and the comparison harness alike | Low |

## Los Santos story — [LosSantos.md](LosSantos.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [TUT](LosSantos.md#tut--tagging-up-turf) | Sweet | Port Tagging up Turf (`sweet1`): seven events from dialogue and help locals, with a failsafe for a restarted mission; the ASL repo's example of the stale-Old false split | Medium |
| [CTH](LosSantos.md#cth--cleaning-the-hood) | Sweet | Port Cleaning the Hood (`sweet1b`): B Dup, the dealer, the crack den and the return, from the act, dialogue and dialogue-length locals | Medium |
| [DTH](LosSantos.md#dth--drive-thru) | Sweet | Port Drive-Thru (`sweet3`): six events from the dialogue block; check whether the marker text is "Drive-thru" or "Drive-Thru" | Medium |
| [NAK](LosSantos.md#nak--nines-and-aks) | Sweet | Port Nines and AK's (`sweet2`): the bottle rounds with ids built from counters, the Tampa cutscene, the call and the store; fix the round 2 label | Medium |
| [DBY](LosSantos.md#dby--drive-by) | Sweet | Port Drive-By (`sweet4`): the stages and sixteen Ballas kills in four groups, from `sw4_stage` and `sw4_hood_dead[]` | Medium |
| [SWG](LosSantos.md#swg--sweets-girl) | Sweet | Port Sweet's Girl (`hoods5`): six events from one stage local | Medium |
| [CVP](LosSantos.md#cvp--cesar-vialpando) | Sweet | Port Cesar Vialpando (`sweet6`): the mod shop, the car meet and the dance, from a stage and sub-stage pair | Medium |
| [DOB](LosSantos.md#dob--doberman) | Sweet | Port Doberman (`crash4`): two events from globals; `$2577` is the leak's `flag_gang_war_active`, so check what "intro cutscenes over" really marks | Medium |
| [LSP](LosSantos.md#lsp--los-sepulcros) | Sweet | Port Los Sepulcros (`sweet7`): the homies, the cemetery, Kane and the escape, from four locals named in the ASL's comments | Medium |
| [OGL](LosSantos.md#ogl--og-loc) | Smoke | Port OG Loc (`twar7`): the police station, Freddy's house, the bike chase stages, Burger Shot; keep the `""` return; the ASL is unsure of `68@` | Medium |
| [RDG](LosSantos.md#rdg--running-dog) | Smoke | Port Running Dog (`smoke2`): three flags becoming 1 | Medium |
| [WST](LosSantos.md#wst--wrong-side-of-the-tracks) | Smoke | Port Wrong Side of the Tracks (`smoke3`): the chase, the Sanchez, each Vagos killed and the front three, the return | Medium |
| [JBZ](LosSantos.md#jbz--just-business) | Smoke | Port Just Business (`drugs1`): the atrium shootouts and Smoke's moves, the chase and the sewer | Medium |
| [LAB](LosSantos.md#lab--lifes-a-beach) | OG Loc | Port Life's a Beach (`music1`): the party cutscene and the van | Medium |
| [MDR](LosSantos.md#mdr--madd-doggs-rhymes) | OG Loc | Port Madd Dogg's Rhymes (`music2`): the mansion, its rooms, the rhyme book and the return | Medium |
| [MGI](LosSantos.md#mgi--management-issues) | OG Loc | Port Management Issues (`music3`): the call, the car, the convoy, the chase and the dunk | Medium |
| [HPY](LosSantos.md#hpy--house-party) | OG Loc | Port House Party (`music5`), both parts on one thread told apart by the OG Loc counter, and the fight stages; a hand-written check | Medium |
| [BDS](LosSantos.md#bds--burning-desire) | C.R.A.S.H. | Port Burning Desire (`crash1`): molotovs, five windows, the house, Denise and the escape; the fire extinguisher setting never splits in the ASL | Medium |
| [GRI](LosSantos.md#gri--gray-imports) | C.R.A.S.H. | Port Gray Imports (`drugs3`): the compound, the padlock and the office | Medium |
| [HIN](LosSantos.md#hin--home-invasion) | Ryder | Port Home Invasion (`guns1`): the house, every box picked up, taken out and loaded (three, plus three optional), the van and the locker | Medium |
| [CTL](LosSantos.md#ctl--catalyst) | Ryder | Port Catalyst (`ryder3`): the train, the fights, ten crates caught and the respray | Medium |
| [RUS](LosSantos.md#rus--robbing-uncle-sam) | Ryder | Port Robbing Uncle Sam (`ryder2`): the compound, the gates, six boxes and Emmett's | Medium |
| [HSL](LosSantos.md#hsl--high-stakes-low-rider) | Cesar | Port High Stakes, Low-Rider (`cesar1`): the intro and race start, with the pass and race start read whatever the last started mission | Medium |
| [RTF](LosSantos.md#rtf--reuniting-the-families) | Finale | Port Reuniting the Families (`drugs4`): the motel, the corridors by audio line, Sweet, the helicopter and the chase | Medium |
| [TGS](LosSantos.md#tgs--the-green-sabre) | Finale | Port The Green Sabre (`la1fin2`): seven chapter events, from a rise without a change test | Medium |
| [RFS](LosSantos.md#rfs--ryders-fail-in-a-save-where-it-was-failed-before) | Intro | Ryder's fail split tests its fail flag going 0 to 1, and the flag is saved and never reset, so in a save in which Ryder was failed before, failing it splits nothing, nor do the splits after a fail; detect the fail another way too | Low |
| [CJH](LosSantos.md#cjh--entering-cjs-house-only-in-cjs-house) | Intro | Big Smoke's "Entering CJ's house" split fires on entering any interior the game numbers 3 while `$65` is 0 or 1; add the game's own test, CJ's position near the save point, once PTW gives pointer reads | Low |

## Badlands story — [Badlands.md](Badlands.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [BDL](Badlands.md#bdl--badlands) | Trailer Park | Port Badlands (`bcrash1`): the base marker, the cabin cutscene, the reporter and the photo | Medium |
| [KIE](Badlands.md#kie--king-in-exile) | Trailer Park | Port King in Exile (`catcut`), whose progress counter is the leak's `flag_trailor_cutscene`; check whether its one event is a cutscene or a call | Medium |
| [CDT](Badlands.md#cdt--catalina-dates) | Catalina | Port the four Catalina dates on thread `catalin`: markers by name and starts picked by the Catalina counter, skipped after `audiol` | Medium |
| [TNK](Badlands.md#tnk--tanker-commander) | Catalina | Port Tanker Commander (`cat3`): five stage events, the mission's own passed flag as its counter | Medium |
| [STB](Badlands.md#stb--small-town-bank) | Catalina | Port Small Town Bank (`cat2`): the chapter events, the three ATMs (a loop that returns early) and Catalina's escape | Medium |
| [LLS](Badlands.md#lls--local-liquor-store) | Catalina | Port Local Liquor Store (`cat1`): the bikers, the briefcases and the end; fix the third biker, which tests the first biker's watcher | Medium |
| [AAO](Badlands.md#aao--against-all-odds) | Catalina | Port Against All Odds (`cat4`): the shop, the door, the stars lost and the chainsaw; needs PTW; register `90@`, which the ASL borrows from Just Business | Medium |
| [WZM](Badlands.md#wzm--wu-zi-mu-and-farewell-my-love) | Cesar | Port Wu Zi Mu and Farewell, my love (`bcesar4`, one thread), told apart by the Cesar counter; fix Farewell's marker, registered under an id with no setting | Medium |
| [BHV](Badlands.md#bhv--body-harvest) | The Truth | Port Body Harvest (`truth1`): the farm, the tractors, the combine and the phone; the water tower setting never splits in the ASL | Medium |
| [AYG](Badlands.md#ayg--are-you-going-to-san-fierro) | The Truth | Port Are You Going to San Fierro? (`truth2`): 44 plants, the rocket launcher, the helicopter, the mothership, the garage, with its events on by default | Medium |

## San Fierro story — [SanFierro.md](SanFierro.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [WFH](SanFierro.md#wfh--wear-flowers-in-your-hair) | Garage | Build Wear Flowers in your Hair (`garag1`, Garage counter `$541` at 1): not in the ASL; basics, progress check and events from the SCM dumps | Medium |
| [WTP](SanFierro.md#wtp--555-we-tip) | C.R.A.S.H. | Build 555 WE TIP (`scrash3`, `$546` at 1) the same way | Medium |
| [DCN](SanFierro.md#dcn--deconstruction) | Garage | Build Deconstruction (`decon`, `$541` at 2) the same way | Medium |
| [PHO](SanFierro.md#pho--photo-opportunity) | Syndicate | Build Photo Opportunity (`syn1`, Syndicate counter `$545` at 1) the same way | Medium |
| [JZY](SanFierro.md#jzy--jizzy) | Syndicate | Build Jizzy (`syn2`, cutscene part at 2 and mission at 3 on `$545`), two parts on one thread like House Party | Medium |
| [TBM](SanFierro.md#tbm--t-bone-mendez) | Syndicate | Build T-Bone Mendez (`driv2`, `$545` at 4) the same way | Medium |
| [MTR](SanFierro.md#mtr--mike-toreno) | Syndicate | Build Mike Toreno (`driv3`, `$545` at 5) the same way | Medium |
| [OUT](SanFierro.md#out--outrider) | Syndicate | Build Outrider (`syn3`, `$545` at 6) the same way | Medium |
| [SNT](SanFierro.md#snt--snail-trail) | C.R.A.S.H. | Build Snail Trail (`scrash2`, `$546` at 2) the same way | Medium |
| [ICK](SanFierro.md#ick--ice-cold-killa) | Syndicate | Build Ice Cold Killa (`synd4`, `$545` at 7) the same way | Medium |
| [PIR](SanFierro.md#pir--pier-69) | Syndicate | Build Pier 69 (`syn5`, `$545` at 8) the same way | Medium |
| [TLF](SanFierro.md#tlf--torenos-last-flight) | Syndicate | Build Toreno's Last Flight (`syn6`, `$545` at 9) the same way | Medium |
| [YKB](SanFierro.md#ykb--yay-ka-boom-boom) | Syndicate | Build Yay Ka-Boom-Boom (`syn7`, `$545` at 10) the same way | Medium |
| [MCB](SanFierro.md#mcb--mountain-cloud-boys) | Woozie | Build Mountain Cloud Boys (`wuzi1`, Woozie counter `$543` at 1) the same way | Medium |
| [RFL](SanFierro.md#rfl--ran-fa-li) | Woozie | Build Ran Fa Li (`farlie4`, `$543` at 2) the same way | Medium |
| [LUR](SanFierro.md#lur--lure) | Woozie | Build Lure (`driv6`, `$543` at 3) the same way | Medium |
| [AMA](SanFierro.md#ama--amphibious-assault) | Woozie | Build Amphibious Assault (`wuzi2`, `$543` at 4) the same way | Medium |
| [DNT](SanFierro.md#dnt--the-da-nang-thang) | Woozie | Build The Da Nang Thang (`wuzi5`, `$543` at 5) the same way | Medium |
| [AIR](SanFierro.md#air--air-raid) | Zero | Build Air Raid (`zero1`, Zero counter `$542` at 1) the same way; its block in the leak is not identified | Medium |
| [SPL](SanFierro.md#spl--supply-lines) | Zero | Build Supply Lines (`zero2`, `$542` at 2) the same way | Medium |
| [NMA](SanFierro.md#nma--new-model-army) | Zero | Build New Model Army (`zero4`, `$542` at 3) the same way | Medium |

## Desert story — [Desert.md](Desert.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [MON](Desert.md#mon--monster) | Toreno | Build Monster (`toreno1`, Toreno counter `$593` at 1): not in the ASL; basics, progress check and events from the SCM dumps | Medium |
| [HJK](Desert.md#hjk--highjack) | Toreno | Build Highjack (`toreno2`, `$593` at 2) the same way | Medium |
| [ITD](Desert.md#itd--interdiction) | Toreno | Build Interdiction (`des3`, `$593` at 3) the same way | Medium |
| [VMD](Desert.md#vmd--verdant-meadows) | Toreno | Build Verdant Meadows (`desert4`, `$593` at 4) the same way | Medium |
| [LTF](Desert.md#ltf--learning-to-fly-pilot-school) | Toreno | Build Learning to Fly (`desert5`, `$593` at 5), which is also the pilot school: the old script's per-test splits from `$1952` and its pass from `$87` | Medium |
| [ZRI](Desert.md#zri--zeroing-in) | Wang Cars | Build Zeroing In (`steal1`, Wang Cars counter `$544` at 1) the same way | Medium |
| [TDR](Desert.md#tdr--test-drive) | Wang Cars | Build Test Drive (`steal2`, `$544` at 2) the same way | Medium |
| [CFT](Desert.md#cft--customs-fast-track) | Wang Cars | Build Customs Fast Track (`steal4`, `$544` at 3) the same way | Medium |
| [PNW](Desert.md#pnw--puncture-wounds) | Wang Cars | Build Puncture Wounds (`steal5`, `$544` at 4) the same way | Medium |

## Las Venturas story — [LasVenturas.md](LasVenturas.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [NOE](LasVenturas.md#noe--noe) | Air Strip | Build N.O.E. (`desert6`, Toreno counter `$593` at 6): not in the ASL; basics, progress check and events from the SCM dumps | Medium |
| [STW](LasVenturas.md#stw--stowaway) | Air Strip | Build Stowaway (`desert9`, `$593` at 7) the same way | Medium |
| [BLP](LasVenturas.md#blp--black-project) | Air Strip | Build Black Project (`maf4`, `$593` at 8) the same way | Medium |
| [GGO](LasVenturas.md#ggo--green-goo) | Air Strip | Build Green Goo (`des10`, `$593` at 9) the same way | Medium |
| [FKC](LasVenturas.md#fkc--fender-ketchup) | Casino | Build Fender Ketchup (`casino1`, Casino counter `$597` at 1) the same way | Medium |
| [EXS](LasVenturas.md#exs--explosive-situation) | Casino | Build Explosive Situation (`casino2`, `$597` at 2) the same way | Medium |
| [YHC](LasVenturas.md#yhc--youve-had-your-chips) | Casino | Build You've Had Your Chips (`casino3`, `$597` at 3) the same way | Medium |
| [DPY](LasVenturas.md#dpy--don-peyote) | Casino | Build Don Peyote (`casino4`, `$597` at 4) the same way | Medium |
| [ICR](LasVenturas.md#icr--intensive-care) | Casino | Build Intensive Care (`casino5`, `$597` at 5) the same way | Medium |
| [TMB](LasVenturas.md#tmb--the-meat-business) | Casino | Build The Meat Business (`casino6`, `$597` at 6) the same way | Medium |
| [FIB](LasVenturas.md#fib--fish-in-a-barrel) | Casino | Build Fish in a Barrel (`casino7`, `$597` at 7 in the dead list, before Don Peyote in the mission table): settle its index first | Medium |
| [FRF](LasVenturas.md#frf--freefall) | Casino | Build Freefall (`casino9`, `$597` at 8) the same way | Medium |
| [SMB](LasVenturas.md#smb--saint-marks-bistro) | Casino | Build Saint Mark's Bistro (`casin10`, `$597` at 9) the same way | Medium |
| [MSP](LasVenturas.md#msp--misappropriation) | C.R.A.S.H. | Build Misappropriation (`vcrash1`, `$598` at 1) the same way | Medium |
| [HNN](LasVenturas.md#hnn--high-noon) | C.R.A.S.H. | Build High Noon (`vcr2`, `$598` at 2) the same way | Medium |
| [MDG](LasVenturas.md#mdg--madd-dogg) | Madd Dogg | Build Madd Dogg (`doc2`, `$599` at 1) the same way | Medium |
| [ARE](LasVenturas.md#are--architectural-espionage) | Heist | Build Architectural Espionage (`heist1`, Heist counter `$600` at 1) the same way | Medium |
| [KTH](LasVenturas.md#kth--key-to-her-heart) | Heist | Build Key to her Heart (`heist3`, `$600` at 2) the same way | Medium |
| [DAB](LasVenturas.md#dab--dam-and-blast) | Heist | Build Dam and Blast (`heist2`, `$600` at 3) the same way | Medium |
| [CPW](LasVenturas.md#cpw--cop-wheels) | Heist | Build Cop Wheels (`heist4`, `$600` at 4) the same way | Medium |
| [UUA](LasVenturas.md#uua--up-up-and-away) | Heist | Build Up, Up and Away! (`heist5`, `$600` at 5) the same way | Medium |
| [BTB](LasVenturas.md#btb--breaking-the-bank-at-caligulas) | Heist | Build Breaking the Bank at Caligula's (`heist9`, `$600` at 6) the same way | Medium |

## Return to Los Santos story — [ReturnToLosSantos.md](ReturnToLosSantos.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [AHH](ReturnToLosSantos.md#ahh--a-home-in-the-hills) | Mansion | Build A Home in the Hills (`mansio1`, Mansion counter `$626` at 1): not in the ASL; basics, progress check and events from the SCM dumps | Medium |
| [VBD](ReturnToLosSantos.md#vbd--vertical-bird) | Mansion | Build Vertical Bird (`mansio2`, `$626` at 2) the same way | Medium |
| [HMC](ReturnToLosSantos.md#hmc--home-coming) | Mansion | Build Home Coming (`mansio3`, `$626` at 3) the same way | Medium |
| [CTB](ReturnToLosSantos.md#ctb--cut-throat-business) | Mansion | Build Cut Throat Business (`manson5`, `$626` at 4), with the old video shoot and pier splits, whose two addresses are unexplained | Medium |
| [BDB](ReturnToLosSantos.md#bdb--beat-down-on-b-dup) | Grove | Build Beat Down on B Dup (`grove1`, Grove counter `$627` at 1) the same way | Medium |
| [GFL](ReturnToLosSantos.md#gfl--grove-4-life) | Grove | Build Grove 4 Life (`grove2`, `$627` at 2), with the old Idlewood and two territory splits from its locals | Medium |
| [RIO](ReturnToLosSantos.md#rio--riot) | Finale | Build Riot (`riot1`, Riot counter `$629` at 1) the same way | Medium |
| [LDS](ReturnToLosSantos.md#lds--los-desperados) | Finale | Build Los Desperados (`riot2`, `$629` at 2) the same way | Medium |
| [EOA](ReturnToLosSantos.md#eoa--end-of-the-line-part-1) | Finale | Build End of the Line part 1 (`finalea`, `$629` at 3), passed after killing Big Smoke | Medium |
| [EOB](ReturnToLosSantos.md#eob--end-of-the-line-part-2) | Finale | Build End of the Line part 2 (`finaleb`, `$629` at 4), passed at the start of the chase | Medium |
| [EOC](ReturnToLosSantos.md#eoc--end-of-the-line-part-3) | Finale | Build End of the Line part 3 (`finalec`, `$629` at 5 after the credits), with the old chase cutscene splits and the end of any%, on by default | Medium |

## Side missions — [SideMissions.md](SideMissions.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [CUR](SideMissions.md#cur--courier) | Courier | Port Courier (`bcour`): each city's start, deliveries per level, levels and pass, from the city, level and delivery locals | Low |
| [TRK](SideMissions.md#trk--trucking) | Trucking | Port Trucking (`truck`): eight missions told apart by the trucking counter, the marker from the odd-job text | Low |
| [FFT](SideMissions.md#fft--firefighter) | Vehicle submissions | Port Firefighter (`firetru`): 78 fires, twelve levels and the pass | Low |
| [RAC](SideMissions.md#rac--races) | Races | Port the races (`cprace`): 25 races' checkpoints and wins, the checkpoint local picked by race type | Low |
| [CHL](SideMissions.md#chl--chiliad-challenge) | Vehicle challenges | Port the Chiliad Challenge (`mtbiker`): three routes' marker, countdown, checkpoints and win; it reads mission locals whatever mission runs | Low |
| [PRP](SideMissions.md#prp--properties) | Properties | Port the property purchases: 30 flags of `already_bought_house[]`, read every tick | Low |
| [WVP](SideMissions.md#wvp--wang-cars-and-verdant-meadows-purchases) | Properties | Add the old script's Wang Cars showroom and Verdant Meadows airstrip purchases, split on the `steal` and `psch` threads, under Assets | Low |
| [QRY](SideMissions.md#qry--quarry) | Quarry | Build Quarry from the dead code: seven missions on `$8171` and the asset completed | Low |
| [VAL](SideMissions.md#val--valet-parking) | Valet | Build Valet Parking from the dead code: started, five levels, each car parked, the pass | Low |
| [VIG](SideMissions.md#vig--vigilante) | Vehicle submissions | Build Vigilante from the dead code: started (also after Learning to Fly), levels and completion | Low |
| [TAX](SideMissions.md#tax--taxi-driver) | Vehicle submissions | Build Taxi Driver from the dead code: each fare up to 50 and completion | Low |
| [PMD](SideMissions.md#pmd--paramedic) | Vehicle submissions | Build Paramedic from the dead code: started, levels and completion | Low |
| [PMP](SideMissions.md#pmp--pimping) | Vehicle submissions | Build Pimping from the dead code: started, ten levels from an unconfirmed stat, completion | Low |
| [FRT](SideMissions.md#frt--freight) | Vehicle submissions | Build Freight from the dead code: started, each stop of both levels, each level | Low |
| [SRC](SideMissions.md#src--8-track-and-dirt-track) | Stadium events | Add settings for 8-Track and Dirt Track with the lap and checkpoint ids RAC's check already builds | Low |
| [BLR](SideMissions.md#blr--blood-ring) | Stadium events | Build Blood Ring from the dead code: started and passed | Low |
| [KIK](SideMissions.md#kik--kickstart) | Stadium events | Build Kickstart from the dead code: started, 33 checkpoints, 26 points, complete | Low |
| [BMX](SideMissions.md#bmx--bmx-stunt-challenge) | Vehicle challenges | Build the BMX stunt challenge from the dead code: started, 19 checkpoints, complete | Low |
| [NRG](SideMissions.md#nrg--nrg-500-stunt-challenge) | Vehicle challenges | Build the NRG-500 stunt challenge from the dead code, as BMX plus the starts after export lists 1 and 3 | Low |
| [DRS](SideMissions.md#drs--driving-school) | Schools | Build Driving School (Back to School) from the dead code: started, each exercise, passed | Low |
| [BKS](SideMissions.md#bks--bike-school) | Schools | Build Bike School from the dead code: started, each exercise, passed | Low |
| [BTS](SideMissions.md#bts--boat-school) | Schools | Build Boat School from the dead code: started, each test, passed | Low |
| [IEX](SideMissions.md#iex--import-and-export) | Import/Export | Build Import/Export from the dead code: each list complete, and decide on each vehicle and the exported count | Low |
| [SHR](SideMissions.md#shr--shooting-range) | Shooting range | Build the Shooting Range from the dead code: the rounds and completion; confirm what `$1861` counts | Low |
| [GYM](SideMissions.md#gym--gym-moves) | Gym moves | Build the three gyms from the dead code: entered, fight start, moves learnt | Low |

## Collectibles and other — [Other.md](Other.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [TAG](Other.md#tag--tags) | Collectibles | Port the tags: rewards, total and each of 100; fix the total's off-by-one (the first tag splits "2 Tags") | Low |
| [PKP](Other.md#pkp--horseshoes-snapshots-and-oysters) | Collectibles | Port horseshoes, snapshots and oysters: rewards, totals and each pickup, whose records move and are found again by a signature scan; the totals have the same off-by-one | Low |
| [USJ](Other.md#usj--unique-stunt-jumps) | Collectibles | Port the stunt jumps: totals found and completed, and each of 70 behind a pointer (PTW); the same off-by-one | Low |
| [GTR](Other.md#gtr--gang-territories) | Other | Port gang territories: the held count and each territory's fight and waves, in Los Santos and Return to Los Santos, from 378 territory records | Low |
| [WRP](Other.md#wrp--death-and-busted-warps) | Other | Port the death and busted warps from `pedStatus` (PTW); fix the King in Exile warp, which reads the Badlands counter; add the old warps as their missions arrive | Low |
| [PHC](Other.md#phc--phone-calls) | Other | Port the phone calls that unlock missions or matter in a run, from the `flag_mob_*` arrays | Low |
| [GFS](Other.md#gfs--girlfriends) | Other | Build the girlfriend splits from the dead code: number acquired, killed, half and full progress | Low |
| [HND](Other.md#hnd--100-percent-achieved) | Other | Build "100% Achieved" from the dead code, on `$802` | Low |
| [PLF](Other.md#plf--plane-flight) | Other | Build "Plane Flight" from the dead code, on the `planes` thread | Low |
| [RSK](Other.md#rsk--route-split-skipping) | Splitting | Port route split skipping: an engine skip mode (`SkipSplit` in place of `Split`) and the GI > LS > HP > C > RUS > RTF trigger on chain counters instead of `vars.Passed` | Low |

## Project — [Project.md](Project.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [LIC](Project.md#lic--choose-a-license) | Legal | No license yet; the component will port logic from a script that is a fork of Tduva's, and Tduva's repository has no license, so ask them before porting it | Medium |
| [REL](Project.md#rel--releases-and-the-update-feed) | Release | No releases, no update feed and no Installing section in the README | Low |
