namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The intro mission (INTRO in the game's script), shown as In the Beginning. Not technically part of the intro chain,
	/// but the ASL puts it there as it fits in narratively. The engine's guards read two of its variables too,
	/// game_starts_from_scratch and para_pickup_flag, and register them.
	/// </summary>
	public class InTheBeginning : IModule
	{
		public void AddSettings(SettingsTree tree)
		{
			tree.Add("intro", "In the Beginning", false, null, "LS_Intro");
			tree.Add("intro_cutsceneEnded", "Intro cutscenes skipped or finished", false, null, "intro");
			// Not the ASL's wording: its tooltip is a code comment
			tree.Add("intro_passed", "Bicycle entered or abandoned", false,
				"Split when the blue arrow over the bicycle goes away: the player enters or moves the bike, or leaves the starting area", "intro");
			tree.Add("intro_groveStreetLinePlayed", "\"Grove Street - Home\" line played", false, null, "intro");
		}

		public void AddChecks(SplitEngine engine)
		{
			WatcherSet w = engine.Watchers;
			// 1 = the cutscene was skipped, 2 = the scripted scene is running, 3 = it finished,
			// 0 = it ended or was skipped
			w.AddScmGlobal("cutscene_skip_CUT", 5353);
			// $HELP_INTRO_SHOWN in the decompile: becomes 1 when the second line of the
			// "Grove Street - Home" voice over starts, later 2 when the red marker help is shown
			w.AddScmGlobal("voice_over_at_hub", 54);
			// Not identified. A byte that is 0 from a new game through every intro scene and its natural end, and becomes 1
			// within a frame of skipping a scripted scene; it toggles again later in gameplay, which does not matter here.
			// Found by diffing memory on the running game, 2026-11-14: four skips and two unskipped runs. See docs/architecture.md.
			w.Add("unknownAB3E1C", 0x6B3E1C, 1);

			// The order and the early returns are the ASL's: a watcher is only read when the checks before it did not return,
			// and that decides what its Old value is.
			engine.AddCheck(() =>
			{
				IntWatcher cutsceneState = w.Get("cutscene_skip_CUT");
				IntWatcher playingTime = w.Get("playingTime");
				// The ASL requires 2000 ms of playing time; 1500 leaves more room for a fast skip. The unskippable splash at
				// a new game lasts over 2 s, so neither should be reached in practice.
				if (cutsceneState.Changed && playingTime.Current > 1500)
				{
					if (cutsceneState.Current == 1 || (cutsceneState.Current == 0 && cutsceneState.Old == 3))
						return "intro_cutsceneEnded";
				}
				// Skipping the police-car scene or the voice-over scene leaves cutscene_skip_CUT at 2 or 3: the skip jumps past
				// the line that would clear it, so the ASL never splits there. The unknown byte marks the skip itself.
				if (cutsceneState.Current == 2 || cutsceneState.Current == 3)
				{
					IntWatcher cutsceneSkipped = w.Get("unknownAB3E1C");
					if (cutsceneSkipped.Changed && cutsceneSkipped.Current == 1 && playingTime.Current > 1500)
						return "intro_cutsceneEnded";
				}
				// game_starts_from_scratch ($MISSION_INTRO_PASSED) gets set when the player enters the bike, moves the bike,
				// or leaves the starting area: anything that makes the blue arrow over the bicycle disappear
				IntWatcher missionPassed = w.Get("game_starts_from_scratch");
				if (missionPassed.Changed && missionPassed.Current == 1)
					return "intro_passed";
				IntWatcher voiceOverAtHub = w.Get("voice_over_at_hub");
				if (voiceOverAtHub.Changed && voiceOverAtHub.Current == 1)
					return "intro_groveStreetLinePlayed";
				return null;
			});
		}
	}
}
