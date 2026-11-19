namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Ryder (intro2), the second mission of the intro chain: the barbershop, the pizza restaurant robbery and the drive back
	/// to Ryder's house, plus failing and restarting it. The ASL's region "Ryder". Five splits the ASL lacks are added after
	/// its tests, so they never take a tick from one of its splits.
	/// </summary>
	public class Ryder : IModule
	{
		// Internal so Big Smoke's check can tell when Ryder is the last started mission
		internal static readonly StoryMission Mission = new StoryMission("intro2", "Ryder", "intro2", "LS_Intro");

		const string intro2_failed = "intro2_failed";
		const string intro2_markerEnteredAfterFail = "intro2_markerEnteredAfterFail";
		const string intro2_restarted = "intro2_restarted";
		const string intro2_rydersCarEntered = "intro2_rydersCarEntered";
		const string intro2_barbershopEntered = "intro2_barbershopEntered";
		const string intro2_haircutBought = "intro2_haircutBought";
		const string intro2_barbershopLeft = "intro2_barbershopLeft";
		const string intro2_outsideCutsceneEnd = "intro2_outsideCutsceneEnd";
		const string intro2_pizzaRestaurantEntered = "intro2_pizzaRestaurantEntered";
		const string intro2_pizzaMenuOpened = "intro2_pizzaMenuOpened";
		const string intro2_pizzaBought = "intro2_pizzaBought";
		const string intro2_pizzaRestaurantLeft = "intro2_pizzaRestaurantLeft";
		const string intro2_rydersCarEnteredAfterRobbery = "intro2_rydersCarEnteredAfterRobbery";
		const string intro2_returnedToRydersHouse = "intro2_returnedToRydersHouse";

		public void AddSettings(SettingsTree tree)
		{
			Mission.AddBasicSettings(tree);
			// The ASL has the example in the label, with "eg."
			tree.Add(intro2_failed, "Mission failed", false, "(e.g. blowing up Ryder's car)", Mission.Id);
			// Not in the ASL: the marker split above only splits once
			tree.Add(intro2_markerEnteredAfterFail, "Mission marker entered after failing", false, null, Mission.Id);
			// The ASL's "Restarting the mission after failing"
			tree.Add(intro2_restarted, "Mission started after failing", false, null, Mission.Id);
			// Not in the ASL
			tree.Add(intro2_rydersCarEntered, "Entered Ryder's car", false,
				"The first time, as \"Go with Ryder to the barbers.\" is shown", Mission.Id);
			tree.Add(intro2_barbershopEntered, "Entering the barbershop", false, null, Mission.Id);
			tree.Add(intro2_haircutBought, "Haircut purchased", false, null, Mission.Id);
			tree.Add(intro2_barbershopLeft, "Left the barbershop", false,
				"When the cutscene outside starts, with CJ's new haircut", Mission.Id);
			// Not in the ASL
			tree.Add(intro2_outsideCutsceneEnd, "End of outside cutscene", false,
				"When CJ gets control back after Ryder's \"Go and get us something to eat\"", Mission.Id);
			tree.Add(intro2_pizzaRestaurantEntered, "Entering the pizza restaurant", false, null, Mission.Id);
			// Not in the ASL
			tree.Add(intro2_pizzaMenuOpened, "Pizza menu opened", false, "When CJ steps into the marker at the counter", Mission.Id);
			tree.Add(intro2_pizzaBought, "Pizza bought", false, "When the robbery cutscene starts, after buying", Mission.Id);
			tree.Add(intro2_pizzaRestaurantLeft, "Left the pizza restaurant", false, "Outside after the robbery cutscene", Mission.Id);
			// Not in the ASL
			tree.Add(intro2_rydersCarEnteredAfterRobbery, "Entered Ryder's car after the robbery", false,
				"When CJ and Ryder are both in the car outside the pizza restaurant", Mission.Id);
			tree.Add(intro2_returnedToRydersHouse, "Arriving back at Ryder's house", false, "As the car stops in the marker, before the fade to the ending cutscene", Mission.Id);
			Mission.AddPassedSetting(tree);
		}

		public void AddChecks(SplitEngine engine)
		{
			WatcherSet w = engine.Watchers;
			Mission.Register(engine);

			// The intro chain's counter: 1 once Big Smoke is passed, 2 once Ryder is
			w.AddScmGlobal("flag_intro_mission_counter", 448);
			// $ACTIVE_INTERIOR in the decompile, written every frame: 0 outside, 2 in the barbershop, 5 in the pizza restaurant
			w.AddScmGlobal("main_visible_area", 43);
			// The ASL's r_onMission: 0 until Ryder's "Show me how they drive on the East Coast" line has played, then 1 as
			// "Get in Ryder's car" is shown; the mission's cleanup sets it back to 0, so it is 0 after a fail or a pass.
			w.AddScmGlobal("flag_player_on_menace_mission", 169);
			// 1 once the haircut is bought (set by the barbershop's script); 0 at the start and in the cleanup
			w.AddScmGlobal("flag_changed_hair_intro2", 676);
			// The ASL's r_fail. Set to 1 when Ryder fails (it lets the retry skip the smoking cutscene), and only set back to 0
			// when the game starts, so it changes once a game. The failed handler sets it, cleans up and ends the script in the
			// same frame: a tick that sees the thread gone also sees this, and the checks still run in that tick.
			w.AddScmGlobal("can_skip_smoking_cutscene", 1514);
			// The mission's dialogue block, INTRO2_CHAT1 to INTRO2_CHAT5 in the leak. The mission goes 0, 1, 2, 4, 3.
			// 0: Hey, Old Reece still run the barber shop? (set as "Get in Ryder's car" is shown)
			// 1: Man, what's this? Shit looks ridiculous (the cutscene outside the barbershop, after the haircut)
			// 2: Give up the money! This a raid! (the robbery cutscene, after buying the pizza)
			// 4: What you waiting for, fool? (outside after the robbery cutscene)
			// 3: Better drop by and see Sweet (the ending cutscene at Ryder's house)
			w.AddScmMissionLocal("intro2_chat_switch", 40);
			// Not read by the ASL. 1 while CJ is in Ryder's car on the way to a destination: first as "Go with Ryder to the
			// barbers." is shown, again at "Drive Ryder home." after the robbery; 0 again whenever CJ gets out.
			w.AddScmMissionLocal("ryders_car_blip_removed", 38);
			// Not read by the ASL. 1 when the cutscene outside the barbershop ends and "Enter the Pizza shop and buy some food."
			// is shown; 2 when the food shop's script sees CJ in the counter marker and opens its menu; 3 when he has bought
			// and the screen fades for the robbery cutscene; 0 in the cleanup. Only Ryder and the food shops use it.
			w.AddScmGlobal("flag_menace_buyfood", 88);
			// Not read by the ASL here (it reads 88@ in later missions, under their own names). 0 at the start, 1 the first time
			// CJ and Ryder are both in the car after the robbery; "Drive Ryder home." follows 7 s later.
			w.AddScmMissionLocal("got_in_ryders_truck_first", 88);

			// The ASL's order of reads and early returns
			engine.AddCheck(() =>
			{
				// Not in the ASL. First, before any read: the marker is entered with the mission not in progress (the engine
				// has cleared the last started mission after the fail; the text comes about 0.5 s before the thread, seen
				// 2026-11-16), or, when the replay trick hides the text, in the tick the retry's thread starts and the
				// mission is in progress again (SplitEngine.MarkerEnteredThisTick). Queued, so a split below in that tick
				// still comes. After a fail in this run, not just the flag: it is saved, so it is also 1 in a save in which
				// Ryder was failed before.
				if (engine.MarkerEnteredThisTick == Mission.MarkerText && engine.IsDone(intro2_failed))
					engine.TrySplit(intro2_markerEnteredAfterFail);
				if (!Mission.InProgress(engine, "flag_intro_mission_counter", 1, 2))
					return null;
				// Read only while Ryder is in progress, so the first read as it starts comes after a gap and counts as unchanged
				// (WatcherSet). In the ASL it compared with the last attempt's read, and a save loaded with the flag already 1
				// split this on starting Ryder.
				IntWatcher failed = w.Get("can_skip_smoking_cutscene");
				if (failed.Changed && failed.Current == 1)
					return intro2_failed;
				// The engine has cleared the last started mission after the fail, so the thread step tries intro2_started again
				// (already done, not split) before this check runs. After a fail in this run, where the ASL tests the fail flag,
				// which is saved: it split this on starting Ryder in a save in which Ryder was failed before.
				StringWatcher thread = w.GetString("thread");
				IntWatcher missionActive = w.Get("flag_player_on_menace_mission");
				if (thread.Changed && thread.Current == Mission.Thread && engine.IsDone(intro2_failed) && missionActive.Current == 0)
					return intro2_restarted;
				if (missionActive.Current == 0)
					return null;
				IntWatcher haircut = w.Get("flag_changed_hair_intro2");
				if (haircut.Changed && haircut.Current == 1)
					return intro2_haircutBought;
				IntWatcher interior = w.Get("main_visible_area");
				if (interior.Changed)
				{
					// Every time CJ goes in during the mission, not only the first
					if (interior.Current == 2)
						return intro2_barbershopEntered;
					if (interior.Current == 5)
						return intro2_pizzaRestaurantEntered;
				}
				IntWatcher dialogueBlock = w.Get("intro2_chat_switch");
				if (dialogueBlock.Changed)
				{
					if (dialogueBlock.Current == 1 && dialogueBlock.Old == 0)
						return intro2_barbershopLeft;
					if (dialogueBlock.Current == 2 && dialogueBlock.Old == 1)
						return intro2_pizzaBought;
					if (dialogueBlock.Current == 4 && dialogueBlock.Old == 2)
						return intro2_pizzaRestaurantLeft;
					if (dialogueBlock.Current == 3 && dialogueBlock.Old == 4)
						return intro2_returnedToRydersHouse;
				}
				// Not in the ASL, after its tests so they never take a tick from one of its splits. Their watchers are read only
				// here, so a change in a tick that an earlier test returned from is seen on the next read.
				IntWatcher inCar = w.Get("ryders_car_blip_removed");
				if (inCar.Changed && inCar.Current == 1 && inCar.Old == 0 && dialogueBlock.Current == 0)
					return intro2_rydersCarEntered;
				IntWatcher bothInCar = w.Get("got_in_ryders_truck_first");
				if (bothInCar.Changed && bothInCar.Current == 1 && bothInCar.Old == 0)
					return intro2_rydersCarEnteredAfterRobbery;
				IntWatcher buyFood = w.Get("flag_menace_buyfood");
				if (buyFood.Changed && buyFood.Current == 1 && buyFood.Old == 0)
					return intro2_outsideCutsceneEnd;
				if (buyFood.Changed && buyFood.Current == 2 && buyFood.Old == 1)
					return intro2_pizzaMenuOpened;
				return null;
			});
		}
	}
}
