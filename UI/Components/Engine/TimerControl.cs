namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Starts and resets the timer when a new game starts. The rule: timing starts as soon as the game has started, before
	/// the intro cutscene, which is when the loading screen has ended and the game's playing time starts counting.
	/// </summary>
	public class TimerControl : IModule
	{
		// A new game is only recognised this early into it, so nothing later in a run can look like one
		const int MaxPlayingTime = 60000;

		public void AddSettings(SettingsTree tree)
		{
			tree.Add("timer", "Timer", true);
			tree.Add("timer_reset", "Reset timer on new game", true,
				"Start timer when the initial loading screen ends after starting a new game, before the intro cutscene begins.", "timer");
			tree.Add("timer_start", "Start timer on new game", true,
				"Start timer when the initial loading screen ends after starting a new game, before the intro cutscene begins.", "timer");
			tree.Add(SplitEngine.DoubleSplitSetting, "Double split prevention (2.5 seconds)", false,
				"Ignore splits that come within 2.5 seconds of the previous. It will still count as done, to prevent later misfires.", "timer");
		}

		public void AddChecks(SplitEngine engine)
		{
			WatcherSet w = engine.Watchers;
			engine.AddStartCheck(() => engine.Settings.IsEnabled("timer_start") && NewGameStarted(w));
			engine.AddResetCheck(() => engine.Settings.IsEnabled("timer_reset") && NewGameStarted(w));
		}

		/// <summary>
		/// Playing time is zeroed when New Game is confirmed and counts up once loading has ended (seen on the running game,
		/// 2026-11-14: the previous value, then 0, then 45 in consecutive 47 ms samples). The 0 can fall between two ticks,
		/// so a drop counts as well as leaving 0. A loaded save brings its own, larger playing time with the intro passed,
		/// so it never matches; a restart from the pause menu looks like a new game. Changed is false on a watcher's first
		/// read, which keeps attaching to a game already in its first minute from starting the timer.
		/// </summary>
		static bool NewGameStarted(WatcherSet w)
		{
			IntWatcher playingTime = w.Get("playingTime");
			IntWatcher introPassed = w.Get("game_starts_from_scratch");
			if (introPassed.Current != 0 || playingTime.Current >= MaxPlayingTime || !playingTime.Changed)
				return false;
			return (playingTime.Old == 0 && playingTime.Current > 0) || playingTime.Current < playingTime.Old;
		}
	}
}
