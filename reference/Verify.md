# GTA SA verification checks (reference)

> Reference copy, moved from livesplit-asl's docs/Verify.md on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

## VER — Checks not yet seen working

Things built but not recorded as seen working. A check keeps its number for good; a new one takes the next number after the highest. Delete a check when it passes and log it in [DONE.md](DONE.md) by number (`VER 3`, or `VER 3, 7` in one row). When none are left, delete this doc and the row in [TODO.md](TODO.md), and log VER as finished.

**Evidence** for all of them: read 2026-11-13; none has been run in LiveSplit by the person who wrote this list. A check passes when, with the game running and LiveSplit's debug output visible (the `[GTASA Autosplitter]` lines), each enabled split fires once at the described moment, doesn't fire when disabled, and doesn't fire again after loading an earlier save.

### With GTA San Andreas running, using the working tree's GTASA.asl

Added after the last commit (2025-09-01).

1. Los Sepulcros mid-mission events, from the intro cinematic end to the return to Grove Street.
2. Gray Imports.
3. Catalyst, including the ten crate splits and the respray.
4. Robbing Uncle Sam, including the six box splits and the last box.
5. Reuniting the Families.
6. The Green Sabre.
7. Badlands.
8. King in Exile.
9. First Date, First Base, Gone Courting and Made in Heaven: the start and marker splits picked by the Catalina counter, and no split when the `audiol` thread precedes `catalin`.
10. Tanker Commander.
11. Small Town Bank.
12. Local Liquor Store.
13. Against All Odds.
14. Wu Zi Mu and Farewell, My Love, told apart by the Cesar counter.
15. Body Harvest.
16. Are You Going to San Fierro?
17. Trucking: start and pass for each trucking mission.
18. Chiliad Challenge: the three routes.
19. Death and busted warps: Badlands → First Date, King in Exile → First Base, Body Harvest → King in Exile.
20. Phone calls 8, 10, 15, 16 and 24.
21. The new always-on watchers `wantedLevel`, `pedStatus`, `weaponSlotMelee` and `oddMissionStartText` show sane values in debug output.

Older code whose comments say it is uncertain.

22. OG Loc: the `68@` sub-stage variable, marked "Unsure what this variable does exactly", drives the mid-mission events; confirm each fires where its label says.
23. Gang territories: the flashing-zone byte ("Didn't check" whether it is the last byte of the zone or the first of the next) and the branch marked "I don't think this can happen", during a territory attack in Los Santos and again in Return to Los Santos.

Version handling changed on 2026-11-13.

26. Version detection after the 1.0-only change: on 1.0 (with SSP) the component shows "1.0", the debug output prints the version line with the module size, and splits fire as before. On a 1.01 or Steam executable, if one is at hand, it shows "Unsupported (use 1.0)" and neither starts nor splits.
