# GTA SA changelog (reference)

> Reference copy, moved from livesplit-asl's CHANGELOG.md (the GTA SA entries) on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

Each section is a day the script changed on master, newest first; Unreleased is what was in the working tree when this was moved.

## Unreleased

### Added

- **GTA SA: splits for the rest of Los Santos and all of Badlands.** Gray Imports, Catalyst, Robbing Uncle Sam, Reuniting the Families, The Green Sabre, Badlands, King in Exile, Tanker Commander, Small Town Bank, Local Liquor Store, Against All Odds, Wu Zi Mu, Farewell My Love, Body Harvest and Are You Going to San Fierro? each get *Mission Marker Entered*, *Mission Started*, *Mission Passed* and mid-mission events (every crate caught in Catalyst, every box loaded in Robbing Uncle Sam, and so on). First Date, First Base, Gone Courting and Made in Heaven get marker and start splits. None of these have been seen working in game yet.
- **GTA SA: mid-mission events for Los Sepulcros**, from the end of the intro cinematic to the return to Grove Street.
- **GTA SA: Trucking splits**, started and passed for each trucking mission.
- **GTA SA: Chiliad Challenge splits** for the three routes.
- **GTA SA: death and busted warps**, under *Other > Death & Busted Warps*, off by default: Badlands to First Date, King in Exile to First Base, Body Harvest to King in Exile.
- **GTA SA: five more phone calls**: Cesar after Badlands and after First Base, The Truth after First Date and after Farewell My Love, Sweet after Badlands.

### Changed

- **GTA SA: the *Trucking* option goes back to its default (on)** when you load this version, because its internal name changed.
- **GTA SA: the Wrong Side of the Tracks event "Start of the case" is now spelled "Start of the chase".**

### Removed

- **GTA SA: game versions other than 1.0 are no longer supported.** On 1.01, 2.00 and the Steam builds the detected version now reads "Unsupported (use 1.0)" and the script does nothing. Before, those builds ran with partly wrong addresses.

## 2025-09-01

### Changed

- **GTA SA: all split options now sit under one *Splits* root** in the settings tree.

## 2025-08-31

### Added

- **GTA SA: a *Mission Marker Entered* option for every mission**, off by default, which splits when the yellow mission name appears on screen.
- **GTA SA: mid-mission events for Wrong Side of the Tracks, Just Business and Home Invasion**, the two Doberman cutscenes, and a split when the "Sweet & Kendl" mission text shows.
- **GTA SA: gang territory splits** for Los Santos and Return to Los Santos, under *Other > Gang Territories*, with options for specific territories.
- **GTA SA: two more phone calls.**

### Changed

- **GTA SA: *Phone Calls* moved under *Other*.**
- **GTA SA: *Double-Split Prevention* stopped working.** With it on, a split more than 2.5 seconds after the previous one is blocked instead of allowed, so nearly nothing splits. Leave it off.
- **GTA SA: *Start timer when loading save* stopped working.** The option is still shown but does nothing.

## 2025-08-20

### Added

- **GTA SA: Madd Dogg's Rhymes, Management Issues, House Party (cutscene and mission) and Burning Desire**, each with *Mission Started*, *Mission Passed* and mid-mission events.
- **GTA SA: phone calls** from Kendl after High Stakes, Low-Rider and from Tenpenny after Madd Dogg's Rhymes.

## 2025-08-18

### Added

- **GTA SA: mid-mission events and *Mission Passed* for Drive-Thru, Nines and AK's, Drive-By, Sweet's Girl, Cesar Vialpando, Doberman, OG Loc, Running Dog, Life's a Beach and High Stakes, Low-Rider**, and *Mission Passed* for Los Sepulcros.
- **GTA SA: race splits** for every street, dirt and air race, grouped by city.
- **GTA SA: property splits** for every safehouse and hotel suite, grouped by area, plus Zero's RC shop.
- **GTA SA: courier splits** per city, level and delivery.
- **GTA SA: a *Phone Calls* group** with the first five calls.

## 2025-08-14

### Added

- **GTA SA: mid-mission events and *Mission Passed* for Tagging up Turf and Cleaning the Hood.**

## 2025-08-08

### Added

- **GTA SA: Big Smoke and Ryder are back**, now with *Mission Started*, *Mission Passed* and more mid-mission events than before (barber and pizza shop entered, bought, left; CJ's house after Big Smoke).
- **GTA SA: *Mission Started* options**, which split when a mission's script begins.
- **GTA SA: Firefighter splits**: first start, each level, each fire, and passed.
- **GTA SA: the Sweet chain missions and the side-mission groups appear in the settings again** (Courier, Trucking, Quarry, Valet Parking, Races, Stadium Events, Vehicle Challenges, Schools, Properties, Import/Export, Shooting Range, Gym Moves), but as headers only: apart from Firefighter they don't split yet.

### Changed

- **GTA SA: stunt jump options are now *Completed* and *Found*, each with a total and specific jumps.**

## 2025-08-04

### Added

- **GTA SA: a *Settings* group** with *Start timer when loading finishes (before cutscene)*, *Start timer when loading save*, *Reset timer when loading save* and *Double-Split Prevention*. *Reset timer when loading save* has no effect.
- **GTA SA: In the Beginning** with three events: intro cutscenes skipped or finished, bicycle entered or abandoned, "Grove Street - Home" line played.
- **GTA SA: the story-mission tree** (cities and chains) is back in the settings, empty for now.

### Changed

- **GTA SA: the timer starts and resets on the game's own clock** when *Start timer when loading finishes* is on: at the moment the clock starts after the loading bar, or when it restarts from the pause menu. Without it, start and reset still happen when the intro cutscene is skipped or ends.

## 2025-07-30

### Removed

- **GTA SA: the script was replaced by a rewrite in progress.** Of its options only *Collectibles* remains (tags, snapshots, horseshoes, oysters, stunt jumps: totals, specific ones, rewards given). Story missions, side missions, warps, gang territories, girlfriends, 100%, split skipping and the save-load options are gone until they are ported; the sections above show them returning piece by piece.

### Changed

- **GTA SA: *Collectibles* is now off by default.**

## 2024-06-24

### Added

- **GTA SA: Badlands mid-mission events** (mountain base, cabin reached, cabin cutscene, reporter dead, photo taken), **Against All Odds events** (robbery cutscene, satchel placed, door blown, store left, each wanted star lost, final marker), **King in Exile started**, and **a started split for each Catalina date**.
- **GTA SA: gym splits** for the Los Santos, San Fierro and Las Venturas gyms: entered, fight start, moves learnt.
- **GTA SA: Vigilante started**, also as "started after Learning to Fly".
- **GTA SA: NRG-500 challenge started after export list 1 and after export list 3** (replacing the single "after export lists" option), a Dirt Track race-start split, and Chiliad route 3 checkpoint 12 & 13.

### Fixed

- **GTA SA: Los Sepulcros "Homies Entered Car" fires when the doors unlock**, not on a later change.

## 2023-11-12

### Added

- **GTA SA: Los Sepulcros, The Green Sabre, Are You Going to San Fierro?, Wu Zi Mu and Farewell, My Love**, with started splits and mid-mission events for the first three (homies recruited, cemetery, Kane; Bravura, parking lot shootout, Tenpenny; plants destroyed, RPG, helicopter, mothership, garage).
- **GTA SA: more Ryder events** (restart after failing, barber entered, haircut, pizza shop entered, pizza bought) and **Freight stops per level**.
- **GTA SA: specific-collectible splits** for snapshots and tags.
- **GTA SA: a *Death & Busted Warps* group.**

### Changed

- **GTA SA: Chiliad Challenge options were renamed** ("Chilliad") and now include a started split per route; saved settings for them reset.
- **GTA SA: Big Smoke event options were renamed**; saved settings for them reset.

## 2023-09-16

### Added

- **GTA SA: Big Smoke, Ryder, High Stakes Lowrider, Cut Throat Business and Grove 4 Life** get *Mission Started* and mid-mission events (parking lot cutscene, Grove Street; failing Ryder, barber and pizza shop left, back at his house; video shoot and pier; Idlewood and the two territories).
- **GTA SA: Quarry, 100% Achieved, Girlfriends, Verdant Meadows airstrip bought**, and gang territory splits during Los Santos and during Return to Los Santos.

### Changed

- **GTA SA: *Collectibles* is on by default.**

### Removed

- **GTA SA: the "51+ taxi fares" and "Vigilante level 13+" options, and the Verdant Meadows safehouse split.**

## 2023-08-21

### Added

- **GTA SA: Valet Parking, Blood Ring started, Kickstart (started, checkpoints, 26 points), BMX and NRG-500 stunt challenge started, Chiliad Challenge** (each route, checkpoints, complete) **and Schools** with a split per exercise of the driving, bike, boat and pilot schools, plus a Trucking "started for the first time" split.

## 2023-08-17

### Added

- **GTA SA: a Grand Theft Auto: San Andreas auto splitter**, based on Tduva's. It starts the timer on a new game, resets, and splits once per attempt on each story mission passed (every chain of every city, with a tree of options), on mission starts, collectibles, properties, death and busted warps, gang territories, export lists, courier, trucking, freight, pimping, taxi fares and vigilante levels, races by city, and the End of the Line chase cutscenes (the firetruck bridge cutscene, the end of any%, on by default). *Double-Split Prevention* imposes a 2.5 second cooldown; an experimental *Start timer when loading save* exists.
