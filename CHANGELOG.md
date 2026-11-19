# Changelog

What changed for users of the SplitSpammer component in each release. There are no releases yet.

## Unreleased

### Added

- The timer starts by itself when a new game starts, the moment its loading screen ends, and a running timer is reset and started again then, also after Start New Game from the pause menu. Both can be turned off under Timer in the settings. An option there blocks a second split within 2.5 seconds of the previous one (off by default).
- Splitting on the Los Santos story missions, in the settings under Story missions > Los Santos:
  - In the Beginning: when the intro cutscenes end or are skipped, when the bicycle arrow goes away, and at the "Grove Street - Home" line. All off by default.
  - Big Smoke: entering its marker, the mission starting, the "Sweet & Kendl" title, "Get on the bike" after the funeral, the parking lot cutscene starting and ending, the Grove Street cutscene starting, and the mission passed (on by default). After the pass: entering CJ's house, the end of the save point camera there, leaving the house, and having left it (when Ryder's marker appears). "Entering CJ's house" also splits if you enter some other buildings first.
  - Ryder: entering its marker, the mission starting, failing it, entering its marker and starting it again after a fail, getting in Ryder's car, the barbershop (entering, the haircut bought, leaving), the end of the cutscene outside it, the pizza restaurant (entering, the menu opened, the pizza bought, leaving), getting back in Ryder's car after the robbery, arriving back at Ryder's house, and the mission passed (on by default).
- Splitting on side missions, in the settings under Side missions > Vehicle submissions:
  - Firefighter: getting into a Firetruck, the mission started for the first time, each of the 78 fires extinguished, and the mission passed, all off by default. Under Levels, each of the 12 levels has its own group (off by default) with the level completed (on), and each car put out and each car with its occupants put out, counted in the order they go out; a car with nobody in it counts as having its occupants put out with it. Getting into a Firetruck can be limited to while no mission is running, to Northeast Los Santos (Las Colinas zone CHC4A and north of it up to the edge of Los Santos), and to before Ryder is passed; the mission start can be limited to Northeast Los Santos too. A split held back by these options still splits the next time they hold.
- The settings show a tree of splits, opened fully when you open the settings, that you can collapse and expand and scroll through, with a checkbox for each. A split only counts when it and the groups above it are checked. Right-click a group to expand or collapse the tree, or to check, uncheck or reset a branch. Your choices are saved in the layout.
- The component's settings show whether GTA San Andreas was found, and say if it is not version 1.0. If the game runs as administrator, LiveSplit must too.
