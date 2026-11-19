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
		const string firetru_levels = "firetru_levels";
		const string firetru_fires = "firetru_fires";
		const string firetru_passed = "firetru_passed";
		const int Levels = 12;
		// Level n has n fires: 1 + 2 + ... + 12
		const int Fires = 78;

		static string LevelId(int level) => "firetru_level" + level + "Completed";
		static string FireId(int fire) => "firetru_fire" + fire + "Extinguished";

		public void AddSettings(SettingsTree tree)
		{
			// The group for every vehicle submission; whichever of them comes first in Modules.All adds it
			tree.Add("VehicleSubmissions", "Vehicle submissions", true, null, "SideMissions");
			tree.Add(firetru, "Firefighter", true, null, "VehicleSubmissions");
			tree.Add(firetru_started, "Mission started for the first time", false, null, firetru);
			tree.Add(firetru_levels, "Levels", false, null, firetru);
			for (int level = 1; level <= Levels; level++)
				tree.Add(LevelId(level), "Level " + level, false, null, firetru_levels);
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
			// ends the script, with no wait in between: the level going to 13, this going to 2 and the thread ending are seen
			// in one tick (read in the game's script, 2026-11-18).
			w.AddScmGlobal("done_firetruck_progress", 1489);

			// The ASL's "Mission Started for the first time": once per reset, like every split
			engine.RegisterThread(Thread, firetru_started);

			// The ASL's order of reads
			engine.AddCheck(() =>
			{
				if (engine.LastStartedMission != Thread)
					return null;
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
					// The ASL returns here and reads the pass on the next tick. Finishing level 12 ends the script in the same
					// tick, so the engine has cleared the last started mission by then and the pass would never split. Both are
					// queued now instead; the timer still gets the pass a tick after the level, as the ASL's does.
					engine.TrySplit(LevelId(level.Old));
				}
				IntWatcher passed = w.Get("done_firetruck_progress");
				if (passed.Changed && passed.Current == 2)
					return firetru_passed;
				return null;
			});
		}
	}
}
