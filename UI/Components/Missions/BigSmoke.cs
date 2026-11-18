namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Big Smoke (intro1), the first mission of the intro chain, with its second title ("Sweet & Kendl"), three cutscenes,
	/// and CJ's house after the pass. The ASL's region "Big Smoke / Sweet & Kendl". Its check is written by hand: it has no
	/// progress check, and the pass and the house splits run whatever the last started mission is, once the intro counter
	/// is 1. Three splits the ASL lacks are added where they can't displace one of its splits.
	/// </summary>
	public class BigSmoke : IModule
	{
		static readonly StoryMission Mission = new StoryMission("intro1", "Big Smoke", "intro1", "LS_Intro");

		// The intro chain's marker script, MS_INTRO_MISSIONS: it starts Big Smoke and Ryder from their markers. Big Smoke's
		// pass ends it, and the intro's ambient script starts it again as Ryder's marker appears. Lower case in memory, and
		// gone at the pass and back with the marker, as read (seen on the running game, 2026-11-16).
		const string IntroMarkerScript = "int";

		const string intro1_sweetAndKendlShown = "intro1_sweetAndKendlShown";
		const string intro1_getOnTheBikeShown = "intro1_getOnTheBikeShown";
		const string intro1_parkingLotCutsceneStarted = "intro1_parkingLotCutsceneStarted";
		const string intro1_parkingLotCutsceneEnded = "intro1_parkingLotCutsceneEnded";
		const string intro1_groveStreetCutsceneStarted = "intro1_groveStreetCutsceneStarted";
		const string intro1_houseEntered = "intro1_houseEntered";
		const string intro1_savePointCameraEnded = "intro1_savePointCameraEnded";
		const string intro1_houseLeft = "intro1_houseLeft";
		const string intro1_ryderMarkerShown = "intro1_ryderMarkerShown";

		public void AddSettings(SettingsTree tree)
		{
			Mission.AddBasicSettings(tree);
			tree.Add(intro1_sweetAndKendlShown, "Sweet & Kendl mission text shown", false, null, Mission.Id);
			// Not in the ASL
			tree.Add(intro1_getOnTheBikeShown, "\"Get on the bike\" shown after the funeral", false,
				"When the ride to Grove Street starts, after Sweet's \"Follow my lead\"", Mission.Id);
			tree.Add(intro1_parkingLotCutsceneStarted, "Parking lot cutscene start", false, null, Mission.Id);
			tree.Add(intro1_parkingLotCutsceneEnded, "Parking lot cutscene end", false, null, Mission.Id);
			tree.Add(intro1_groveStreetCutsceneStarted, "Grove Street cutscene start", false, null, Mission.Id);
			// The ASL adds the house events after the pass, the only mission that adds anything after it
			Mission.AddPassedSetting(tree);
			tree.Add(intro1_houseEntered, "Entering CJ's house interior after passing", false,
				"Also splits when entering certain other interiors that share an ID, like the strip club in Bone County", Mission.Id);
			// Not in the ASL
			tree.Add(intro1_savePointCameraEnded, "Save point camera in CJ's house ended", false,
				"When CJ can move again after the camera shows the save point and the save help", Mission.Id);
			tree.Add(intro1_houseLeft, "Leaving CJ's house after passing", false, null, Mission.Id);
			// Not in the ASL
			tree.Add(intro1_ryderMarkerShown, "Left CJ's house after passing", false,
				"Immediately upon appearing outside CJ's house after exiting", Mission.Id);
		}

		public void AddChecks(SplitEngine engine)
		{
			WatcherSet w = engine.Watchers;
			Mission.Register(engine);
			// The mission's second title, shown right after it starts (0.2 s after the thread on 2026-11-16, 3 s on 2026-11-15)
			engine.RegisterMissionName("Sweet & Kendl", intro1_sweetAndKendlShown);

			// The intro chain's counter: 1 once Big Smoke is passed, 2 once Ryder is
			w.AddScmGlobal("flag_intro_mission_counter", 448);
			// $HELP_GROOVE_SHOWN in the decompile, the ASL's house help: how far CJ is through the help that the intro's ambient
			// script (INTROST) shows after Big Smoke is passed. It is 0 from a new game until then.
			// 0 -> 1: 5 s after the pass, as "Go inside CJ's house to continue." is shown.
			// 1 -> 2: CJ is inside CJ's house within 10 m of the save point (5 m vertically). The game takes control, points the
			//         camera at the save point and shows "To save the game, pick up the diskette icon..."; 5 s later CJ gets
			//         control back and it becomes 2.
			// 2 -> 3: CJ is outside within 20 m of the front door (6 m vertically). The intro marker script restarts, Ryder's
			//         marker appears and "Go and see Ryder, he lives down the street." is shown; 2 s later the help "Follow the
			//         'R' blip on the radar..." is shown; 4 s after that (6 s after Ryder's marker) it becomes 3 and INTROST ends.
			w.AddScmGlobal("funeral_mission_finished", 65);
			// $ACTIVE_INTERIOR in the decompile, written every frame: 0 outside, 3 in CJ's house (and in other interiors)
			w.AddScmGlobal("main_visible_area", 43);
			// The mission's dialogue block, INTRO1_CHAT1 to INTRO1_CHAT7 in the leak. The mission goes 0, 1, 4, 3, 6, (5), 2.
			// 0: You wanna drive?
			// 1: Ballas! Drive by! Incoming!
			// 2: I got with them motherfuckers though
			// 3: Shit, a Ballas car is onto us
			// 4: Takes you back some huh CJ? Yeah
			// 5: Straight back into the game right dog?
			// 6: You're just a liability CJ
			w.AddScmMissionLocal("intro1_chat_switch", 46);

			// The ASL's order of reads and early returns
			engine.AddCheck(() =>
			{
				// While Ryder runs. The engine clears it once Ryder has ended, unlike the ASL, so this reads on after that (below)
				if (engine.LastStartedMission == Ryder.Mission.Thread)
					return null;
				IntWatcher counter = w.Get("flag_intro_mission_counter");
				if (counter.Current == 2)
					return null;
				if (counter.Current == 1)
				{
					if (counter.Changed)
						return Mission.PassedId;
					// Read whatever the last started mission is, so also after a failed Ryder, once the engine has cleared it
					// (the ASL keeps intro2 and reads nothing). That can't split: Ryder's marker only appears once the house
					// help is 3, and neither house split fires at 3.
					IntWatcher houseHelp = w.Get("funeral_mission_finished");
					IntWatcher interior = w.Get("main_visible_area");
					if (interior.Changed)
					{
						// Any interior the game numbers 3, not only CJ's house; the game itself also tests the position (CJH)
						if ((houseHelp.Current == 1 || houseHelp.Current == 0) && interior.Current == 3)
							return intro1_houseEntered;
						if (houseHelp.Current == 2 && interior.Current == 0)
							return intro1_houseLeft;
					}
					// Not in the ASL, after its tests so they never take a tick from one of its splits
					if (houseHelp.Changed && houseHelp.Current == 2 && houseHelp.Old == 1)
						return intro1_savePointCameraEnded;
					// The marker script never runs while the help is 2 before it is restarted, so this fires on the tick it
					// starts, or on the next if leaving the house took that tick
					if (houseHelp.Current == 2 && engine.IsRunning(IntroMarkerScript))
						return intro1_ryderMarkerShown;
					return null;
				}
				if (engine.LastStartedMission != Mission.Thread)
					return null;
				// After a failed Big Smoke the engine stops reading this until the retry, whose first read compares with the
				// value at the fail. The retry starts at 0, and no split ends at 0.
				IntWatcher dialogueBlock = w.Get("intro1_chat_switch");
				if (dialogueBlock.Changed)
				{
					// The "split up" cutscene, after its fade
					if (dialogueBlock.Current == 3 && dialogueBlock.Old == 4)
						return intro1_parkingLotCutsceneStarted;
					// CJ has control again
					if (dialogueBlock.Current == 6 && dialogueBlock.Old == 3)
						return intro1_parkingLotCutsceneEnded;
					if (dialogueBlock.Current == 2 && (dialogueBlock.Old == 5 || dialogueBlock.Old == 6))
						return intro1_groveStreetCutsceneStarted;
					// Not in the ASL: set in the same frame as the "Get on the bike." text
					if (dialogueBlock.Current == 4 && dialogueBlock.Old == 1)
						return intro1_getOnTheBikeShown;
				}
				return null;
			});
		}
	}
}
