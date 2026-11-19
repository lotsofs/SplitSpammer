namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Firefighter (firetru), the fire truck side mission: the first start, every fire extinguished, every level and the
	/// pass (level 12 finished). The ASL's region "Firefighter" under "Vehicle submissions".
	/// </summary>
	public class Firefighter : IModule
	{
		const string Thread = "firetru";
		const string firetru = "firetru";
		const string firetru_started = "firetru_started";
		const string firetru_startedInNortheastLosSantos = "firetru_startedInNortheastLosSantos";
		const string firetru_fireTruckEntered = "firetru_fireTruckEntered";
		const string firetru_fireTruckEnteredOffMission = "firetru_fireTruckEnteredOffMission";
		const string firetru_fireTruckEnteredInNortheastLosSantos = "firetru_fireTruckEnteredInNortheastLosSantos";
		const string firetru_fireTruckEnteredBeforeRyderPassed = "firetru_fireTruckEnteredBeforeRyderPassed";
		const string firetru_levels = "firetru_levels";
		const string firetru_fires = "firetru_fires";
		const string firetru_passed = "firetru_passed";
		const int Levels = 12;
		// Level n has n fires: 1 + 2 + ... + 12
		const int Fires = 78;
		// FIRETRUK, the only fire truck that starts the mission (the game's script, R3_1138); not the ladder truck (544)
		const int FireTruckModel = 407;
		// Ryder is passed when the intro chain's counter reaches this
		const int RyderPassed = 2;

		// Zone CHC4A (Las Colinas, x 2747..2959, y -1120..-945) and the strip north of it that no district covers, up to the
		// edge of Los Santos (zone LA ends at y -768, Red County starts there), at any height. From the game's zone array,
		// 2026-11-19.
		static readonly Box NortheastLosSantos = Box.Between(2747, -1120, 2959, -768);
		const string NortheastLosSantosTooltip = "Only in Las Colinas zone CHC4A or north of it up to the edge of Los Santos (x 2747 to 2959, y -1120 to -768).";
		const string HeldBackTooltip = " Otherwise it doesn't split, and a later one still can.";

		static string HeldBack(string reason)
		{
			DebugOutput.Write($"split held back ({reason}): {firetru_fireTruckEntered}");
			return null;
		}

		static string LevelHeaderId(int level) => "firetru_level" + level;
		static string LevelId(int level) => "firetru_level" + level + "Completed";
		static string FireId(int fire) => "firetru_fire" + fire + "Extinguished";
		// The nth car (or car with its occupants) put out in a level, in the order they go out
		static string CarId(int level, int n) => "firetru_level" + level + "Car" + n + "Extinguished";
		static string CarOccupantsId(int level, int n) => "firetru_level" + level + "Car" + n + "OccupantsExtinguished";

		// The fires of a level, read 2026-11-19 in the leak (firetruck.sc) and the decompiled FIRETRU, not yet seen live.
		// Level L uses the first L fires of a fixed order: car 1, its driver and two passengers, then car 2 and its three,
		// then car 3 and its three. Each car has a flag that is 0 while it burns and 10 once it is out, each person one that
		// is 0 while in the car, 1 burning and 2 once out; the people only get out, and catch fire, once their car is out.
		// When a level's last fire goes out, the script raises the level and sets every flag to 0 in the same frame, so the
		// last car never shows as done: the level rising stands for it. At a mission start the flags keep what the previous
		// mission left there for a frame, before the script sets them to 0.
		const int CarFlagOut = 10;
		const int PersonFlagOut = 2;
		const int CarsPerLevelMax = 3;
		const int PeoplePerCar = 3;
		static string CarFlag(int car) => "onfire_car_" + car + "_flag";
		static string PersonFlag(int person) => "onfire_ped_" + person + "_flag";

		static int CarsInLevel(int level) => (level + 3) / 4;

		// Per attempt and level: whether counting is armed (every flag seen at 0), and how many cars and cars with their
		// occupants have been split
		bool attemptSeen;
		int countedLevel;
		bool armed;
		int carsSplit;
		int carOccupantsSplit;

		public void AddSettings(SettingsTree tree)
		{
			// The group for every vehicle submission; whichever of them comes first in Modules.All adds it
			tree.Add("VehicleSubmissions", "Vehicle submissions", true, null, "SideMissions");
			tree.Add(firetru, "Firefighter", true, null, "VehicleSubmissions");
			tree.Add(firetru_fireTruckEntered, "Firetruck entered", false, "When CJ gets into a Firetruck, the truck that starts the mission", firetru);
			tree.Add(firetru_fireTruckEnteredOffMission, "...while not on a mission", false,
				"Only while no mission is running." + HeldBackTooltip, firetru_fireTruckEntered);
			tree.Add(firetru_fireTruckEnteredInNortheastLosSantos, "...in Northeast Los Santos", false,
				NortheastLosSantosTooltip + HeldBackTooltip, firetru_fireTruckEntered);
			tree.Add(firetru_fireTruckEnteredBeforeRyderPassed, "...before passing Ryder", false,
				"Only until Ryder is passed.", firetru_fireTruckEntered);
			tree.Add(firetru_started, "Mission started for the first time", false, null, firetru);
			tree.Add(firetru_startedInNortheastLosSantos, "...in Northeast Los Santos", false,
				NortheastLosSantosTooltip + " A start elsewhere doesn't split, and a later start there still can.", firetru_started);
			tree.Add(firetru_levels, "Levels", false, null, firetru);
			const string orderTooltip = "Counted in the order they are put out. A car with nobody in it has its occupants extinguished as soon as it is.";
			for (int level = 1; level <= Levels; level++)
			{
				string header = LevelHeaderId(level);
				tree.Add(header, "Level " + level, false, null, firetru_levels);
				int cars = CarsInLevel(level);
				for (int n = 1; n <= cars; n++)
				{
					tree.Add(CarId(level, n), n == 1 ? "1 car extinguished" : n + " cars extinguished", false, orderTooltip, header);
					tree.Add(CarOccupantsId(level, n), n == 1 ? "1 car's occupants extinguished" : n + " cars' occupants extinguished", false, orderTooltip, header);
				}
				tree.Add(LevelId(level), "Level completed", true, null, header);
			}
			tree.Add(firetru_fires, "Fires extinguished", false, null, firetru);
			// The ASL's labels: the total so far, and which fire of which level it is
			tree.Add(FireId(1), "1 fire extinguished (level 1 fire 1)", false, null, firetru_fires);
			int levelFire = 0;
			int fireLevel = 2;
			for (int fire = 2; fire <= Fires; fire++)
			{
				levelFire++;
				tree.Add(FireId(fire), fire + " fires extinguished (level " + fireLevel + " fire " + levelFire + ")", false, null, firetru_fires);
				if (levelFire >= fireLevel)
				{
					fireLevel++;
					levelFire = 0;
				}
			}
			tree.Add(firetru_passed, "Mission passed", false, null, firetru);
		}

		public void AddChecks(SplitEngine engine)
		{
			WatcherSet w = engine.Watchers;
			// Set to 0 as the mission starts (a frame after its script name appears), 1 more for every fire put out, over
			// all levels of the attempt
			w.AddScmGlobal("db_fires_extinguished", 8214);
			// Set to 1 as the mission starts, 1 more when a level's fires are all out; 13 once level 12 is finished
			w.AddScmGlobal("db_firetruck_level", 8213);
			// 0 until the mission is passed. Finishing level 12 the first time sets it to 1, and the cleanup sets it to 2 and
			// ends the script, with no wait in between, so the script reads as if the level going to 13, this going to 2 and
			// the thread ending come in one tick (2026-11-18). On the running game (2026-11-19) this and the thread's end came
			// a tick after the level; the check below handles both.
			w.AddScmGlobal("done_firetruck_progress", 1489);
			// Not in the ASL: each car's and person's fire (see CarFlag)
			for (int car = 1; car <= CarsPerLevelMax; car++)
				w.AddScmMissionLocal(CarFlag(car), 91 + car);
			for (int person = 1; person <= CarsPerLevelMax * PeoplePerCar; person++)
				w.AddScmMissionLocal(PersonFlag(person), 94 + person);

			// Not in the ASL: where CJ is, what he drives, whether a mission runs ($ONMISSION, 0 off a mission, seen
			// 2026-11-19) and the intro chain (2 once Ryder is passed)
			w.AddPlayerPosition();
			w.AddVehicleModel();
			w.AddScmGlobal("flag_player_on_mission", 409);
			w.AddScmGlobal("flag_intro_mission_counter", 448);

			// The ASL's "Mission Started for the first time": once per reset, like every split. Not the ASL: optionally only
			// in Northeast Los Santos, the position read only then
			engine.RegisterThread(Thread, firetru_started, () =>
				!engine.Settings.IsEnabled(firetru_startedInNortheastLosSantos)
				|| w.GetPosition("playerPosition").IsIn(NortheastLosSantos));

			// Not in the ASL: getting into a Firetruck, whatever mission runs. Nothing is read while the setting is off, and
			// each condition's watcher only in the tick CJ gets in with that condition on, so the shared intro chain counter
			// is read as Ryder's and Big Smoke's checks read it at any other time.
			engine.AddCheck(() =>
			{
				SettingsSnapshot settings = engine.Settings;
				if (!settings.IsEnabled(firetru_fireTruckEntered))
					return null;
				IntWatcher vehicle = w.Get("vehicleModel");
				if (!vehicle.Changed || vehicle.Current != FireTruckModel)
					return null;
				if (settings.IsEnabled(firetru_fireTruckEnteredOffMission) && w.Get("flag_player_on_mission").Current != 0)
					return HeldBack("on a mission");
				if (settings.IsEnabled(firetru_fireTruckEnteredInNortheastLosSantos) && !w.GetPosition("playerPosition").IsIn(NortheastLosSantos))
					return HeldBack("not in Northeast Los Santos");
				if (settings.IsEnabled(firetru_fireTruckEnteredBeforeRyderPassed) && w.Get("flag_intro_mission_counter").Current >= RyderPassed)
					return HeldBack("Ryder passed");
				return firetru_fireTruckEntered;
			});

			// The ASL's order of reads
			engine.AddCheck(() =>
			{
				if (engine.LastStartedMission != Thread)
				{
					attemptSeen = false;
					return null;
				}
				if (!attemptSeen)
				{
					// A new attempt: the flags may still hold another mission's values
					attemptSeen = true;
					countedLevel = 0;
				}
				// Fire, level, pass: the order the mission does them in
				IntWatcher fires = w.Get("db_fires_extinguished");
				if (fires.Changed)
				{
					// Every fire, in case two go out in one tick
					for (int fire = fires.Old + 1; fire <= fires.Current; fire++)
						engine.TrySplit(FireId(fire));
				}
				IntWatcher level = w.Get("db_firetruck_level");
				if (level.Changed && level.Old > 0 && level.Current > level.Old)
				{
					// Not the ASL: whatever cars of the finished level weren't seen done went out with its last fire
					if (armed && countedLevel == level.Old)
						SplitCars(engine, level.Old, CarsInLevel(level.Old), CarsInLevel(level.Old));
					// The ASL returns here and reads the pass on the next tick. As the script reads, finishing level 12 can end
					// the script in the same tick, and then the engine would have cleared the last started mission by the next and
					// the pass would never split. So the pass is read on in this tick too (on the running game it came a tick
					// later, with the thread's end, and split then); the timer gets it a tick after the level either way.
					engine.TrySplit(LevelId(level.Old));
				}
				CountCars(engine, w, level.Current);
				IntWatcher passed = w.Get("done_firetruck_progress");
				if (passed.Changed && passed.Current == 2)
					return firetru_passed;
				return null;
			});
		}

		// Not in the ASL: the cars of this level that are out, and those whose occupants are out too, in the order they go
		void CountCars(SplitEngine engine, WatcherSet w, int level)
		{
			if (level < 1 || level > Levels)
				return;
			if (level != countedLevel)
			{
				countedLevel = level;
				armed = false;
				carsSplit = 0;
				carOccupantsSplit = 0;
			}
			int cars = CarsInLevel(level);
			int carsOut = 0;
			int carOccupantsOut = 0;
			bool allZero = true;
			for (int car = 1; car <= cars; car++)
			{
				int carFlag = w.Get(CarFlag(car)).Current;
				allZero &= carFlag == 0;
				bool occupantsOut = true;
				for (int seat = 1; seat <= PeoplePerCar; seat++)
				{
					// Fire number (car - 1) * 4 + 1 is the car, the next three its people; level L has fires 1 to L
					if ((car - 1) * 4 + 1 + seat > level)
						break;
					int personFlag = w.Get(PersonFlag((car - 1) * PeoplePerCar + seat)).Current;
					allZero &= personFlag == 0;
					occupantsOut &= personFlag == PersonFlagOut;
				}
				if (carFlag == CarFlagOut)
				{
					carsOut++;
					if (occupantsOut)
						carOccupantsOut++;
				}
			}
			if (!armed)
			{
				// The script sets every flag to 0 as a level starts and no fire goes out within a frame, so a level is only
				// counted once it has been seen at 0: this skips the values another mission left at a start
				armed = allZero;
				return;
			}
			SplitCars(engine, level, carsOut, carOccupantsOut);
		}

		void SplitCars(SplitEngine engine, int level, int carsOut, int carOccupantsOut)
		{
			for (int n = carsSplit + 1; n <= carsOut; n++)
				engine.TrySplit(CarId(level, n));
			carsSplit = System.Math.Max(carsSplit, carsOut);
			for (int n = carOccupantsSplit + 1; n <= carOccupantsOut; n++)
				engine.TrySplit(CarOccupantsId(level, n));
			carOccupantsSplit = System.Math.Max(carOccupantsSplit, carOccupantsOut);
		}
	}
}
