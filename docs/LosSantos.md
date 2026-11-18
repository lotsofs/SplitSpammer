# Los Santos story missions

Detail for the Los Santos rows of [TODO.md](TODO.md): the live ASL's Los Santos missions after In the Beginning, one item each, in the ASL's order.

Dependencies: every item here needs the comparison harness from TST. The order is the suggested one, not required: each mission is its own module. A chain's group node (`LS_Sweet`, `LS_Smoke`, `LS_Ogloc`, `LS_Crash`, `LS_Ryder`, `LS_Cesar`, `LS_Final`) is added to `SettingsTree.Build` by its first port. Gang territories (GTR) and route split skipping (RSK) read the Sweet, C.R.A.S.H. and finale counters, which any of these items may register first.

How to port, and the facts every item relies on, are in [porting.md](porting.md); each section here says what is particular to its mission. Everything is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl`, region named in each section, with leak names for locals as candidates (porting.md, "Leak names"). The ASL repo's check numbers (VER n) are in `reference\Verify.md`.

## TUT — Tagging up Turf

The ASL's region "Tagging up Turf": thread `sweet1`, Sweet counter 0 to 1, settings `tut` (on, with the tooltip naming the tags involved) with marker, start, events (intro end, Sweet's tag cutscene end, entering the car after the first group of tags, leaving it before the second, approaching the gang members, entering the car after the second group, the ending cutscene) and the pass. After the progress check it reads the dialogue block (0 to 4 intro end, 4 to 6 final cutscene), returns while that block is 0 (a failsafe for a restarted mission), then the help flags in the ASL's order.

Watchers: `$452` (`flag_sweet_mission_counter`), and the candidates `40@` `played_mission_audio_sweet1`, `46@` `gang_hassle`, `48@` `sweet1_cell_index_end`, `58@` `help_for_tagging`, `65@` `played_last_line_of_audio` (`48@` and `58@` confirmed in `reference\README.md`). Shares `40@` with Ryder (`intro2_chat_switch`, ported), `46@` with Big Smoke (`intro1_chat_switch`, ported), `48@` with NAK.

**Evidence:** read 2026-11-15, region "Tagging up Turf". It is the ASL repo's example for STL (`58@` going stale).

**Plan:** as porting.md. Adds the `LS_Sweet` node.

**Verify:** the comparison passes; in LiveSplit, each event, and the STL scenario noted as a known false split until STL is done.

**Docs when done:** as porting.md, step 7.

## CTH — Cleaning the Hood

The ASL's region "Cleaning the Hood": thread `sweet1b`, Sweet counter 1 to 2, settings `cth` (on) with marker, start, intro end, after visiting B Dup, approaching the dealer (tooltip: skipped in common speedruns), the cutscene after killing the dealer, arriving at the crack den, the den cleared, arriving back at Grove Street, and the pass. Reads the act (1 B Dup visited, 3 crack den), then the dialogue block (4, 1, 7), then the dialogue length (7 means the intro ended), then the den-cleared flag.

Watchers: `$452`, candidates `43@` `killed_all_crack_fucks`, `87@` `sweet1b_chat_switch`, `91@` `sweet1b_cell_index_end`, `102@` `flag_sweet1b`. Shares `87@` with WST and AYG.

**Evidence:** read 2026-11-15, region "Cleaning the Hood".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event, the dealer one by talking to him instead of skipping him.

**Docs when done:** as porting.md, step 7.

## DTH — Drive-Thru

The ASL's region "Drive-Thru": thread `sweet3`, Sweet counter 2 to 3, settings `dt` (on) with marker, start, events from the dialogue block (1 intro end, with a tooltip; 2 chase start; 3 Ballas killed; 4 and 5 the cutscene back at Grove Street starting and ending; 6 arriving at Big Smoke's house) and the pass. Its mission name is registered as "Drive-thru", with a lower-case t, unlike the setting's label.

Watchers: `$452`, candidate `79@` `sweet3_chat_switch`.

**Evidence:** read 2026-11-15, region "Drive-Thru". Whether the game shows "Drive-thru" or "Drive-Thru" is not checked; the marker split only fires if the text matches exactly.

**Plan:** as porting.md; check the mission name's case in `reference\main.scm.txt` or on screen, and fix it if the ASL's is wrong.

**Verify:** the comparison passes; in LiveSplit, each event including the marker.

**Docs when done:** as porting.md, step 7.

## NAK — Nines and AK's

The ASL's region "Nines and AK's": thread `sweet2`, Sweet counter 3 to 4, settings `naak` (on) with marker, start, intro end, the three bottle-shooting rounds (start, and each bottle shot: one in round 1, three in round 2, five in round 3), the cutscene after blowing up the Tampa (start, end), Sweet's phone call, entering and leaving the clothing store, and the pass. Reads the dialogue block (1, 2, 3), the dialogue length (0 to 8 is the intro end), then, while combat is active, Smoke's action counter (0, 3, 6 tells the round) and the bottles shot, building the id from both; then the interior only while the dialogue block is 3 (0 to 15 enter, 15 to 0 leave).

Watchers: `$452`, candidates `36@` `tag_target_counter`, `39@` `sweet2_chat_switch`, `48@` `sweet2_cell_index_end`, `62@` `smoke_shots_off`, `64@` `Return_Progress`, `86@` `new_visible_area`. Shares `36@` with TRK and `48@` with TUT.

**Evidence:** read 2026-11-15, region "Nines and AK's". Defects: the round 2 start setting is labelled "Bottleshooter round 1 started"; the bottle ids are built from the counter, so a count above the round's bottles names a setting that does not exist (porting.md, step 3).

**Plan:** as porting.md; fix the round 2 label.

**Verify:** the comparison passes; in LiveSplit, each round's start and bottles, the store.

**Docs when done:** as porting.md, step 7.

## DBY — Drive-By

The ASL's region "Drive-By": thread `sweet4`, Sweet counter 4 to 5, settings `db` (on) with marker, start, entering Sweet's car, arriving at the marker, the shootout start, sixteen Ballas killed in four groups of four (each kill, and each whole group, in sub-nodes built in a loop), all Ballas killed, the respray, arriving at Grove Street, and the pass. Reads the stage (3 to 8, one event each), then all sixteen kill flags in order, splitting each new kill with `TrySplit` and a group's "entire group killed" when a kill completes it.

Watchers: `$452`, `$6637` (`sw4_stage`), `$6640` to `$6655` (`sw4_hood_dead[0]` to `[15]`). No locals.

**Evidence:** read 2026-11-15, region "Drive-By".

**Plan:** as porting.md. The settings loop builds nested group nodes; keep their order. The kill flags are the first array watchers ported. CLAUDE.md's Conventions name an array element with the leak's brackets (`"sw4_hood_dead[3]"`), a rule set with MHL before any mission needed it: check that it reads well in the check's loop (`"sw4_hood_dead[" + i + "]"`), and change the rule here if it doesn't, before the races, properties and phone calls follow it.

**Verify:** the comparison passes; in LiveSplit, the stages and a few kills and a whole group.

**Docs when done:** as porting.md, step 7.

## SWG — Sweet's Girl

The ASL's region "Sweet's Girl": thread `hoods5`, Sweet counter 5 to 6, settings `sg` (on) with marker, start, events from the stage (2 intro end, 3 enemies killed, 4 phone call end, 5 marker entered with a four-door car, 6 chase start, 7 arrival at Sweet's house) and the pass.

Watchers: `$452`, candidate `127@` `sw5_flag` (the leak's script is `sweet5`). Shares `127@` with RUS.

**Evidence:** read 2026-11-15, region "Sweet's Girl".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## CVP — Cesar Vialpando

The ASL's region "Cesar Vialpando": thread `sweet6`, Sweet counter 6 to 7, settings `cv` (on) with marker, start, events (intro end; at the mod shop: arriving, the cutscene starting and ending, entering, leaving; the car meet: arriving, the wager screen, the wager placed, the dance starting and ending; the final cinematic) and the pass. All come from the sub-stage changing, picked by the stage (a stage.sub-stage table in the ASL's comment).

Watchers: `$452`, candidates `34@` `m_stage`, `35@` `m_goals`. Both are shared with several missions (porting.md lists them).

**Evidence:** read 2026-11-15, region "Cesar Vialpando".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## DOB — Doberman

The ASL's region "Doberman": thread `crash4`, Sweet counter 7 to 8, settings `d` (on, with a tooltip pointing at the Glen Park territory split) with marker, start, "intro cutscenes over", "Glen Park cutscene over" and the pass.

Watchers: `$452`, `$2577` (the ASL's "cut 1"; the leak's `flag_gang_war_active`), `$2414` (`played_cut2_before_crash4`).

**Evidence:** read 2026-11-15, region "Doberman". The leak name of `$2577` suggests the first split fires when the gang war becomes active, not when a cutscene ends; check live which it is, and fix the label if it is wrong.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, both events land where their labels say.

**Docs when done:** as porting.md, step 7.

## LSP — Los Sepulcros

The ASL's region "Los Sepulcros": thread `sweet7`, Sweet counter 8 to 9, settings `ls` (on) with marker, start, events (intro end, one and two homies recruited, all homies in the car, arriving at the cemetery, the wall climbed, waiting for Kane starting and ending, Kane killed, the escape car entered, the return to Grove Street) and the pass. Reads the group size (0 to 1, 1 to 2), the next prompt (3, 20), the mission progress (1, 5, 6, 20, 1352) and a counter (69, 70). The dead region "Los Sepulcros" in `split` is an older version of the same events from other variables; ignore it.

Watchers: `$452`, `53@` `sw7_missionprogress`, `55@` `sw7_nextprompt`, `261@` `sw7_groupsize`, `534@` `sw7_counter` (names from the ASL's comments, and the leak's count agrees).

**Evidence:** read 2026-11-15, region "Los Sepulcros". The ASL repo never recorded it working (VER 1).

**Plan:** as porting.md; delete nothing in the ASL (it stays as reference).

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## OGL — OG Loc

The ASL's region "OG Loc": thread `twar7`, Smoke counter 0 to 1, settings `ogl` (on) with marker, start, the intro end, the police station cutscene (start, end), the cutscene at Freddy's (start, end), knocking on his door, the chase start, Freddy at the corner in Los Flores, a sub-group of extra chase moments (tooltip: skipped in a normal speedrun; eight stages and Freddy getting off his bike), Freddy dead, the cutscene after it ending, arriving at Burger Shot, and the pass. Reads the stage (a switch whose default builds `ogl_stage<n>`, and which returns `""` when the stage becomes 1, so the sub-stage is not read that tick), then the sub-stage, picked by the stage.

Watchers: `$454` (`flag_smoke_mission_counter`), candidates `67@` `tw7_reenter_state`, `68@` `tw7_control_blip` (the leak's script is `smoke1`; both names look unlike what the ASL reads, so confirm the count).

**Evidence:** read 2026-11-15, region "OG Loc". The ASL's comment says it is unsure what `68@` does (VER 22).

**Plan:** as porting.md. Adds the `LS_Smoke` node. The `""` return must end the check without splitting.

**Verify:** the comparison passes; in LiveSplit, each event, the sub-stage ones especially.

**Docs when done:** as porting.md, step 7.

## RDG — Running Dog

The ASL's region "Running Dog": thread `smoke2`, Smoke counter 1 to 2, settings `rd` (on) with marker, start, entering Big Smoke's car, the mid-mission cinematic starting and ending, and the pass; each a flag becoming 1.

Watchers: `$454`, candidates `162@` `s2_player_entered_first_car`, `166@` `s2_player_and_smoke_arrived`, `181@` `s2_cutscene_skipped`.

**Evidence:** read 2026-11-15, region "Running Dog".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## WST — Wrong Side of the Tracks

The ASL's region "Wrong Side of the Tracks": thread `smoke3`, Smoke counter 2 to 3, settings `wsott` (on) with marker, start, intro end, the chase start, the Sanchez mounted, each of the four Vagos killed (with tooltips placing them), the three front ones killed, all killed, the return to Smoke's house, and the pass. Reads the stage (1, 2, 4 returned; 3 sends "all killed" through `TrySplit` and goes on), then an audio label (35420, Smoke's "Follow that train", means the bike was mounted), then the four kill flags in a loop that sends "three front ones" before reading the fourth.

Watchers: `$454`, `80@` `smoke_s3flag`, `87@` to `90@` `mex1dead_s3flag` to `mex4dead_s3flag`, `128@` `audio_label_s3` (names from the ASL's comments; the leak's count agrees). Shares `80@`, `87@` to `90@` and `128@` with other missions.

**Evidence:** read 2026-11-15, region "Wrong Side of the Tracks".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event, the kills in the order they happen.

**Docs when done:** as porting.md, step 7.

## JBZ — Just Business

The ASL's region "Just Business": thread `drugs1`, Smoke counter 3 to 4, settings `jb` (on) with marker, start, events (intro end, arriving at the atrium, the shootout inside starting, Smoke's moves through the atrium, the shootout outside starting and ending, the cutscene before the chase starting and ending, Smoke driving into the sewer) and the pass. Reads the stage, then the atrium stage, then the chase block.

Watchers: `$454`, `$6898` (`smokeshoot_s4flag`), `90@` `smoke_s4flag`, `243@` `firstchase_s4flag` (from the ASL's comments; the leak's script is `smoke4`, whose count agrees). Shares `90@` with WST and AAO.

**Evidence:** read 2026-11-15, region "Just Business".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## LAB — Life's a Beach

The ASL's region "Life's a Beach": thread `music1`, OG Loc counter 0 to 1, settings `lab` (on) with marker, start, the cutscene near the party, the van entered, and the pass.

Watchers: `$455` (`flag_strap_mission_counter`), candidates `207@` `m1_player_entered_sound_van`, `279@` `m1_cutscene_skipped` (the leak's script is `strap1`).

**Evidence:** read 2026-11-15, region "Life's a Beach". The ASL's comment lists two more locals it could not make reliable.

**Plan:** as porting.md. Adds the `LS_Ogloc` node.

**Verify:** the comparison passes; in LiveSplit, both events.

**Docs when done:** as porting.md, step 7.

## MDR — Madd Dogg's Rhymes

The ASL's region "Madd Dogg's Rhymes": thread `music2`, OG Loc counter 1 to 2, settings `mdr` (on) with marker, start, approaching the mansion (tooltip), entering, three cutscene ends inside (pool, plant hallway, lounge), the rhyme book, leaving, the return to Burger Shot, and the pass. Reads the stage, returns unless the stage is 2 (inside), then the room and the alcove flag.

Watchers: `$455`, candidates `70@` `rhymebook_s2flag`, `71@` `helptext_s2flag`, `73@` `guard3_s2flag` (the leak's script is `strap2`; the names do not obviously match the ASL's use, so confirm the count).

**Evidence:** read 2026-11-15, region "Madd Dogg's Rhymes".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## MGI — Management Issues

The ASL's region "Management Issues": thread `music3`, OG Loc counter 2 to 3, settings `mi` (on) with marker, start, OG Loc's call, the car taken, the convoy approached, joined, moving, passing the road block, arriving, the chase start, the car dunked, and the pass. Reads the stage (1 to 7; 6 and 7 both mean dunked), then the dialogue block (7 phone, 8 "keep frosty"), then the park-straight objective flag.

Watchers: `$455`, candidates `122@` `m3_goals`, `160@` `m3_paynspray_text_flag`, `174@` `m3_speech_goals` (the leak's script is `strap3`). Shares `122@` with LLS.

**Evidence:** read 2026-11-15, region "Management Issues".

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## HPY — House Party

The ASL's region "House Party": one thread, `music5`, for two missions that share it, the cutscene part (`hp1`, start and pass, no marker) and the mission (`hp2`, marker, start, the cutscenes ended, the first two cars cleared, the bridge cleared, the last fight cleared, and the pass). The thread is registered with no split, so only the last started mission is set; the check itself reads the OG Loc counter and, while it is 3 or 4, `thread`, splitting the start of the part the counter picks. Then, with the last started mission `music5`, the counter changing from 3 or 4 splits that part's pass, and the goals local gives the events.

Watchers: `$455`, `thread`, candidate `80@` `music5_goals`. Shares `80@` with WST.

**Evidence:** read 2026-11-15, region "House Party".

**Plan:** as porting.md; `StoryMission`'s progress check does not fit, the check is hand-written (`engine.MissionInProgress` with the part's indices may serve for each part's pass).

**Verify:** the comparison passes; in LiveSplit, both parts' start and pass, and the events.

**Docs when done:** as porting.md, step 7.

## BDS — Burning Desire

The ASL's region "Burning Desire": thread `crash1`, C.R.A.S.H. counter 0 to 1, settings `bd` (on) with marker, start, events (the cinematic end, the molotovs, 300 m from the house, arriving, each of five windows and all of them, entering, the extinguisher, Denise's room reached and entered, the escape, leaving with Denise, arriving at her place, her wave) and the pass. Reads the stage; while it is or was 2, the windows torched (each change splits `bd_window<n>` through `TrySplit`); then the stage's events; then the goals, picked by the stage.

Watchers: `$456` (`flag_crash_mission_counter`), `34@` `m_stage`, `35@` `m_goals`, `209@` `countTargetRoomsHit` (names from the ASL's comments; the leak's count is two too high at `209@`, porting.md). `34@` and `35@` are shared.

**Evidence:** read 2026-11-15, region "Burning Desire". The fire extinguisher setting has no check in the ASL: it never splits.

**Plan:** as porting.md; keep or drop the extinguisher setting (dropping it is a visible change to note in the log). Adds the `LS_Crash` node.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## GRI — Gray Imports

The ASL's region "Gray Imports": thread `drugs3`, C.R.A.S.H. counter 1 to 2, settings `gi` (on) with marker, start, the intro end, the red marker at the compound, the compound cutscene end, the padlock destroyed, the office entered, and the pass.

Watchers: `$456`, `141@` `crash_c3flag`, `173@` `tolotov_c3flag` (from the ASL's comments; the leak's script is `crash3`, whose count agrees).

**Evidence:** read 2026-11-15, region "Gray Imports". The ASL repo never recorded it working (VER 2).

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## HIN — Home Invasion

The ASL's region "Home Invasion": thread `guns1`, Ryder counter 0 to 1, settings `hi` (on) with marker, start, the two intro cutscenes ending, arriving at the house, the cutscene there ending, entering, the burglary tutorial ending, three boxes (each picked up, taken outside, put in the van, and re-entering after it), a sub-group of the optional fourth to sixth boxes, entering the van early (tooltip), driving away, the locker box, and the pass. Reads the progression flag, the boxes in the truck (a change above 1 splits `hi_boxDrop<old>`), the clothes flag (1 to 0 is the end cutscene), then, once a box is in the van, inside-the-house and holding-a-box flags with ids built from the box count, then an audio counter (38: "Let's get up out of here").

Watchers: `$453` (`flag_ryder_mission_counter`), `35@` `ryd1_number_gunbox_truck`, `51@` `ryd1_mission_progression_flag`, `60@` `ryd1_player_holding_box_flag`, `88@` `ryd1_clothes_changed`, `95@` `ryd1_inside_house_flag`, `254@` `ryd1_audio_counter` (from the ASL's comments; the leak agrees). Shares `35@` and `88@`.

**Evidence:** read 2026-11-15, region "Home Invasion". The box count starts at 1 (the drop split names the old value from 1 on), so `hi_houseEnter1` is the first entry.

**Plan:** as porting.md. Adds the `LS_Ryder` node.

**Verify:** the comparison passes; in LiveSplit, each event for three boxes and for all six.

**Docs when done:** as porting.md, step 7.

## CTL — Catalyst

The ASL's region "Catalyst": thread `ryder3`, Ryder counter 1 to 2, settings `c` (on) with marker, start, events from the mission flag (intro end, approaching the train, Ryder out of the car, Vagos and Ballas killed, the Ballas cutscene, the crate throwing, off the train), ten crates caught and all caught, the respray, and the pass. Reads the mission flag (11 sends "all crates" through `TrySplit`), then the crate count (every crate between Old and Current, in case two land in one tick), then the respray flag.

Watchers: `$453`, `82@` `r3_crate_cnt`, `89@` `r3_mission_flag`, `103@` `r3_car_been_sprayed` (from the ASL's comments; the leak agrees). Shares `89@` with WST, `103@` with AAO.

**Evidence:** read 2026-11-15, region "Catalyst". The ASL repo never recorded it working (VER 3).

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event and all ten crates.

**Docs when done:** as porting.md, step 7.

## RUS — Robbing Uncle Sam

The ASL's region "Robbing Uncle Sam": thread `ryder2`, Ryder counter 2 to 3, settings `rus` (on) with marker, start, the intro end, the van at Grove Street, arriving at the compound, the infiltration, the gate and garage door, six boxes loaded, the last box entered, back in the van, arriving at Emmett's, and the pass. Reads the goals, picked by the stage, then the boxes loaded (an increase splits `rus_box<n>`).

Watchers: `$453`, `34@` `m_stage`, `35@` `m_goals`, `127@` `loaded_boxes` (from the ASL's comments; the leak agrees). All three are shared.

**Evidence:** read 2026-11-15, region "Robbing Uncle Sam". The ASL repo never recorded it working (VER 4).

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event and all six boxes.

**Docs when done:** as porting.md, step 7.

## HSL — High Stakes, Low-Rider

The ASL's region "High Stakes, Low-Rider": thread `cesar1`, settings `hslr` (on, tooltip pointing at the Lowrider race under Races) with marker, start, the intro end, the race start, and the pass. No progress check: it reads the Cesar counter every tick (at 1 or more: the pass on the change from 0, then returns), then the race-is-go flag (2 is the race start) whatever the last started mission, and only then requires `cesar1` for the intro end.

Watchers: `$457` (`flag_cesar_mission_counter`), `$2336` (`cs1_race_is_go`), candidate `34@` `cs1_stage` (shared).

**Evidence:** read 2026-11-15, region "High Stakes, Low-Rider".

**Plan:** as porting.md; hand-written check. Adds the `LS_Cesar` node.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## RTF — Reuniting the Families

The ASL's region "Reuniting the Families": thread `drugs4`, finale counter 0 to 1, settings `rtf` (on) with marker, start, events (Sweet's car, the motel marker, the motel cutscene starting and ending, the motel entered, five points inside from the audio lines, reunited with Sweet, out on the roof, the helicopter fight starting, the helicopter killed, the chase) and the pass. Reads act 1 (stage values), act 2 (audio line ids), act 3 (stage values; 21, or 22 coming from below 21, is the helicopter).

Watchers: `$458` (`flag_la1fin1_mission_counter`), candidates `106@` `audio_label_f1`, `128@` `moteldeal_f1flag`, `171@` `sweetexit_f1flag` (the leak's script is `la1fin1`). Shares `106@` and `128@`.

**Evidence:** read 2026-11-15, region "Reuniting the Families". The ASL repo never recorded it working (VER 5). The helicopter condition reads `Current == 21 || Current == 22 && Old < 21`, which C# groups as `21 || (22 && Old < 21)`; keep the grouping.

**Plan:** as porting.md. Adds the `LS_Final` node.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## TGS — The Green Sabre

The ASL's region "The Green Sabre", written with `RegisterMissionBasics`: thread `la1fin2`, finale counter 1 to 2, settings `tgs` (on) with marker, start, events from the chapter (the intro cutscenes, the Bravura entered, the cinematic with Cesar, the parking lot, the shootout starting and ending, the Badlands cinematic) and the pass. The chapter check is `Current > Old` with no `Changed` test, sending each through `TrySplit`.

Watchers: `$458`, candidate `100@` `lf2_flag`.

**Evidence:** read 2026-11-15, region "The Green Sabre". The ASL repo never recorded it working (VER 6). A chapter that jumps by two splits only the new value.

**Plan:** as porting.md.

**Verify:** the comparison passes; in LiveSplit, each event.

**Docs when done:** as porting.md, step 7.

## RFS — Ryder's fail in a save where it was failed before

Ryder's `intro2_failed` splits when `$1514` (`can_skip_smoking_cutscene`) goes from 0 to 1, as the ASL's does. The mission's failed handler sets it to 1 and only the game's start sets it back to 0, so it is saved as 1. In a save in which Ryder was failed before, failing it again changes nothing, so the fail doesn't split, and neither do `intro2_markerEnteredAfterFail` and `intro2_restarted`, which need the fail split done in this run. A run from a new game is not affected.

**Evidence:** read 2026-11-18 in `reference\main.scm.txt` (`$1514 = 0` once at the start of the game, `$1514 = 1` in Ryder's failed handler); not reproduced.

**Plan:** also count as a fail the tick Ryder's thread leaves the script list with the intro counter still 1 while the on-mission flag `$169` was 1, or while `$1514` was already 1 when the mission started. A save loaded during Ryder also ends the thread: decide whether that should split the fail, and check what the counter and `$169` read in that tick.

**Verify:** a comparison scenario for each case (a save with the flag set, a fail early and late, a pass, a save loaded during the mission), and in LiveSplit: fail Ryder in a save in which it was failed before.

**Docs when done:** the comments in `Ryder.cs`; CHANGELOG.md Unreleased, the Ryder entry, if users of a release could have met it.

## CJH — "Entering CJ's house" only in CJ's house

Big Smoke's `intro1_houseEntered` copies the ASL: it splits when `$43` (`main_visible_area`) becomes 3 while `$65` (`funeral_mission_finished`) is 0 or 1, after the pass and before Ryder. The game numbers more interiors than CJ's house 3, so entering another of them in that window splits it too. The game's own test (the intro's ambient script `INTROST`, before the save help) is the interior being 3 and CJ within 10 × 10 × 5 m of the save point at (2496.05, −1698.01, 1013.75).

**Evidence:** read 2026-11-16 in `reference\main.scm.txt` (`INTROST`, the `$HELP_GROOVE_SHOWN == 1` block) and the leak's `intro_stuff_loop`. Which other interiors are 3 is not checked.

**Plan:** after PTW, watch CJ's position through the player pointer the ASL already uses for `pedStatus`: `[+0x77CD98]` is the player ped, its matrix pointer is at `+0x14` and the position at `+0x30` (x, y, z floats). In the house test, also require CJ inside a box around the house's interior (near x 2496, y −1696, z above 1000). Check whether the position is already inside on the tick the interior number changes; if not, test it on the following ticks while the interior is 3 and the split hasn't fired. The ASL comparison then leaves this id out (porting.md, step 4).

**Verify:** in LiveSplit, after Big Smoke: entering another interior numbered 3 first splits nothing, and entering CJ's house still splits at the door.

**Docs when done:** the item's log row; the changelog's Big Smoke entry only if its wording changes.
