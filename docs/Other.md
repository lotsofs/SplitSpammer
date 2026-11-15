# Collectibles and other splits

Detail for the collectibles and other rows of [TODO.md](TODO.md): the live ASL's collectibles (`Collectibles`, off) and its `Other` group (off), then the old script's other splits that only its dead code has, and the route split skipping.

Dependencies: THR ([Groundwork.md](Groundwork.md)) for all, TST's harness for the live ones. USJ and WRP also need PTW. PKP needs a signature scan and watchers that can move. GTR reads the Sweet, C.R.A.S.H., finale and Mansion counters and `thread`; WRP reads the Badlands, King in Exile, Catalina and Truth counters. The live ones are ported as [porting.md](porting.md) says; the dead-code ones are built as the note at the top of [SideMissions.md](SideMissions.md) says.

The per-slot ones (TAG, PKP, USJ, GTR) read hundreds of watchers a tick while their Specific settings are on; the ASL repo's item BLK (`reference\Defects.md`) planned block reads instead. Port them watcher for watcher first, so the comparison holds, and consider block reads afterwards. They also add hundreds to thousands of settings, so check how the settings panel copes with the tree that size.

Everything here is read, not reproduced: read 2026-11-15 in `reference\GTASA.asl`, region named in each section.

## TAG — Tags

The ASL's regions "Collectibles info & settings" and "Tags": settings `Collectibles` (off) holding `Tags` (off; its label is its id), with "All Tags (Rewards Given)", "Total Collected" (100 counts) and "Specific Tags" (100 tags, labelled in the order of the ehgames.com map list through the ASL's index table), each with a tooltip. The check reads the rewards flag (0 to 1), the total (a rise splits that count; otherwise it returns once the old total is 100 or more), returns unless Specific Tags is on, then reads the tags in memory order until one crosses from 228 or less to 229 or more, and splits it.

Watchers: `$1519` (`flag_returned_tags`), the total at `+0x69AD74` (1 byte), the 100 tags at `+0x69A8C0 + 4 + 8 * i` (1 byte each).

**Evidence:** read 2026-11-15, regions "Collectibles info & settings" and "Tags". Defect: the total's setting for i is labelled "i + 1 Tags", but the check splits the id for the new total, so the first tag splits "2 Tags" and the hundredth names no setting. The same is true of the horseshoe, snapshot, oyster and stunt jump totals (PKP, USJ).

**Plan:** as porting.md; fix the off-by-one (a fix, outside the comparison for those ids). The index table is game data; port it as data. Adds the `Collectibles` node.

**Verify:** the comparison passes apart from the totals; in LiveSplit, spraying a tag splits its total, its specific tag, and on the last one the rewards.

**Docs when done:** as porting.md, step 7; the changelog gets a collectibles entry.

## PKP — Horseshoes, snapshots and oysters

The ASL's region "Snapshots, Horseshoes, & Oysters", with its settings in "Collectibles info & settings": `Horseshoes`, `Snapshots` and `Oysters` (off), each with "All … (Rewards Given)", "Total Collected" (50 counts) and "Specific …" (50 each, ids numbered 0 to 149 over the three kinds). The check returns while `Collectibles` is off; reads the three rewards flags (only the first that changed splits, an else-if chain), the three totals (the same), returns once all 150 are collected, returns unless a Specific node is on, then walks the 150 pickups: each record is 0x20 bytes, with an id word at +0xC compared with the ASL's identifier table and a status byte (+0x19 for horseshoes and oysters, 0 or 8 to 1 or 9 when collected; +0x18 for snapshots, 20 to 0). The pickups move in memory (after loading a save, mostly): when more ids are wrong than pickups collected, it signature-scans the module for any pickup's pattern, computes the block's new start, and moves all 300 watchers there, which `update` applies the next tick.

Watchers: `$1517` (`flag_returned_shoehorses`), `$1518` (`flag_returned_snapshots`), `$1516` (`returned_oysters_flag`), the totals at `+0x7791E4`, `+0x7791BC`, `+0x7791EC` (1 byte), and the 150 records starting at `+0x578EE4` until moved.

**Evidence:** read 2026-11-15. The totals have TAG's off-by-one. Reads depend on settings (`Collectibles`, the Specific nodes).

**Plan:** as porting.md, with two engine pieces first: a signature scan of the game's module in `IGameReader` (the ASL uses LiveSplit's `SignatureScanner` with `??` wildcards) and a way to move a watcher to a new address that behaves like the ASL's replacement watchers (a new watcher: its first read counts as unchanged). Port the identifier tables as data. Fix the off-by-one as TAG does.

**Verify:** the comparison passes apart from the totals, including a relocation; in LiveSplit, a pickup of each kind, then loading a save and collecting another.

**Docs when done:** as porting.md, step 7; docs/architecture.md (the scan, game memory).

## USJ — Unique stunt jumps

The ASL's region "Stunt Jumps", with its settings in "Collectibles info & settings": `Stunt Jump` ("Stunt Jumps", off) with Completed and Found nodes, each with "Total …" (70 counts) and "Specific Stunt Jumps" (70). The check reads the completed total (a rise splits), the found total (a rise splits; otherwise it returns once the old completed total is 70 or more), returns unless a Specific node is on, then reads the 70 jumps until one goes from 0 to 256 (found) or from 256 to 257 (completed).

Watchers: the totals at `+0x779064` (completed) and `+0x779060` (found) (1 byte), and 70 two-byte records behind a pointer, `[[+0x69A888] + 0] + 0x40 + 0x44 * i` (PTW).

**Evidence:** read 2026-11-15, region "Stunt Jumps". The totals have TAG's off-by-one. The ASL's ids contain spaces (`Completed Stunt JumpEach3`); ours are new anyway.

**Plan:** as porting.md; fix the off-by-one as TAG does.

**Verify:** the comparison passes apart from the totals; in LiveSplit, finding and completing a jump.

**Docs when done:** as porting.md, step 7.

## GTR — Gang territories

The ASL's region "Gang Territories": settings `GangTerritories` (off) under `Other` (off), with a Los Santos and a Return to Los Santos group (tooltips: before The Green Sabre, after Vertical Bird), each with "Territories Held" (counts 1 to 379, several labelled with what that count means in a run) and "Specific Territories" (the Los Santos zones, plus "Other territories (from glitch)" with the rest of the map by zone, each territory with "Fight started" and waves 1 to 3). The territory table (378 territories in the order of the game's `info.zon`, with names and zones) is in the region. The check returns until Cesar Vialpando or Burning Desire is passed; returns while `thread` is `sweet7` (Los Sepulcros changes territories for a while); returns between The Green Sabre and Vertical Bird; picks the group by the Mansion counter; splits a rise of the count; then, if a Specific node is on, walks the territories (only the original 53 unless the Other zone is on), splitting the waves from the gang percentages and the fight start from the zone's flashing byte, unless the territory is already ours.

Watchers: `$452`, `$456`, `$458`, `$626`, `thread`, the count at `+0x7791D0` (1 byte), and per territory `+0x7A1E01 + 17 * i` (4 bytes: Ballas, Grove and Vagos percentages in the low three) and `+0x7A1E01 + 15 + 17 * i` (1 byte, the flashing state).

**Evidence:** read 2026-11-15, region "Gang Territories". The ASL repo's check VER 23 says the flashing byte and the branch its comment calls impossible were never confirmed. It tests `thread` for Los Sepulcros, not the last started mission.

**Plan:** as porting.md. Port the territory table as data. Settings are by far the largest group; check the tree's performance with it.

**Verify:** the comparison passes; in LiveSplit, taking a territory in Los Santos and one in Return to Los Santos: the count, the fight start and the waves.

**Docs when done:** as porting.md, step 7.

## WRP — Death and busted warps

The ASL's region "Warps": settings `Warps` ("Death & Busted Warps", off) under `Other`, with Busted Warps (Badlands to First Date, King in Exile to First Base) and Death Warps (Body Harvest to King in Exile). The check needs `pedStatus` to change: 63 (busted) with Badlands passed and the Catalina counter 0, or with King in Exile passed and the counter 1; 55 (wasted) with Body Harvest passed and King in Exile not.

Watchers: `pedStatus` (PTW), `$493`, `$64`, `$2163`, `$491`.

**Evidence:** read 2026-11-15, region "Warps" (VER 19). Defect: the King in Exile busted warp reads the Badlands counter where it means King in Exile's, so it splits whenever Badlands is passed and one Catalina mission is done. The ids name Tanker Commander and Small Town Bank while the labels name First Date and First Base. The region keeps the old script's longer warp lists commented out (Tanker Commander to Body Harvest, Jizzy, Mike Toreno, Lure, The Da Nang Thang, Boat School, several export vehicles, Freeway, Trucking 7, Dam and Blast, Up, Up and Away!, Barnstorming); the ASL repo's notes add the first commit's San Fierro death warp.

**Plan:** as porting.md; fix the King in Exile warp. Add the other warps as the missions they join are ported, each a setting under Death Warps or Busted Warps.

**Verify:** the comparison passes apart from the fix; in LiveSplit, each warp, by dying or getting busted at the right point.

**Docs when done:** as porting.md, step 7.

## PHC — Phone calls

The ASL's region "Phone Calls": settings `PhoneCalls` (off) under `Other`, with the calls that matter in a run (Sweet, Officer Hernandez, Cesar, Kendl, OG Loc, Tenpenny, The Truth), each with a tooltip quoting the call and saying what it unlocks. The check walks call numbers 0 to 29, skipping those with no call, and returns the first whose flag became 1.

Watchers: `$1342` to `$1349` (`flag_mob_la1[0]` to `[7]`), `$1356` to `$1363` (`flag_mob_cat[0]` to `[7]`), `$1371` (`flag_mob_random[5]`), as the table in porting.md maps them to call numbers.

**Evidence:** read 2026-11-15, region "Phone Calls" (VER 20 for calls 8, 10, 15, 16 and 24). It builds its set of skipped numbers anew on every step of the loop and reads every flag every tick with Phone Calls off (the ASL repo's BLK).

**Plan:** as porting.md; the set once, as a constant. Adds the `Other` node.

**Verify:** the comparison passes; in LiveSplit, taking two of the calls.

**Docs when done:** as porting.md, step 7.

## GFS — Girlfriends

Not in the live ASL. The old script had `Girlfriends` under `Other`, and for Denise, Michelle, Helena, Barbara, Katie and Millie: the number acquired, killed, 50% progress (car unlocked) and maximum progress (outfit unlocked), with odd defaults (Millie's node and her killed split on, Katie's number on). Progress is `$359` to `$364` (`iGFLikesPlayer[0]` to `[5]`: -999 killed, 100 maximum, crossing 50 the car); the number acquired is a bit per girlfriend in `$406` (`iActiveGF`, bits 1 to 32 in that order, only the lowest new bit split per tick).

**Evidence:** read 2026-11-15 (dead "Other" settings, the dead tail of `split` and of `init`).

**Plan:** as the note above; keep the old defaults unless they turn out to be a mistake.

**Verify:** tests with a fake reader; in LiveSplit, getting a number and raising progress past 50.

**Docs when done:** as porting.md, step 7.

## HND — 100 percent achieved

Not in the live ASL. The old script had "100% Achieved" (off) under `Other`, with a tooltip (the game checks every 3 seconds and gives the rewards 2 seconds later), from `$802` (`game_complete`) becoming true; its check is commented out.

**Evidence:** read 2026-11-15.

**Plan:** as the note above.

**Verify:** tests with a fake reader; in LiveSplit, on a save just short of 100%.

**Docs when done:** as porting.md, step 7.

## PLF — Plane flight

Not in the live ASL. The old script had "Plane Flight" (off) under `Other`, with a tooltip (entering the ticket machine marker for the first time), split when the thread `planes` starts (dead `vars.startMissions`).

**Evidence:** read 2026-11-15.

**Plan:** as the note above: register the thread (THR).

**Verify:** tests with a fake reader; in LiveSplit, entering a ticket machine marker.

**Docs when done:** as porting.md, step 7.

## RSK — Route split skipping

The ASL registers a root node "Split Skipping" (off), with the route "GI > LS > HP > C > RUS > RTF" (a tooltip explains it), after its early return in `startup`, so it never appears, and its trigger is in the dead tail of `split`. The skipping itself is live: while `vars.skipSplits` is set, a queued split calls `SkipSplit()` on the timer instead of splitting, and `onReset` clears it. The old trigger set it when Catalyst (`ryder3`) or House Party (`music5`, with its cutscene part passed) started after Gray Imports and before Los Sepulcros, and cleared it when Reuniting the Families (`drugs4`) started, testing passes with `vars.Passed`. Two more routes (Catalyst first; Slamvan before Taxi Driver) are commented out.

**Evidence:** read 2026-11-15 (`split`, the dead "Non-Linear Split Skipping" settings and the end of `#region Splits OLDDDDD`).

**Plan:** add a skip mode to the engine (a timer action that calls `TimerModel.SkipSplit` on the UI thread in place of `Split`, cleared by a reset), the settings at the root as in the ASL, and the trigger on thread changes (THR) with the chain counters in place of `vars.Passed` (Los Sepulcros passed: Sweet counter 9 or more; Gray Imports: C.R.A.S.H. counter 2 or more; House Party's cutscene: OG Loc counter 4 or more). Port the other routes only if someone asks.

**Verify:** tests with a fake reader: skipping starts and stops at the right thread changes and turns splits into skips; in LiveSplit, the deviation from the route.

**Docs when done:** as porting.md, step 7; docs/architecture.md (one engine tick).
