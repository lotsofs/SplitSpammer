using LiveSplit.Model;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>What a tick asks the timer to do, in this order: Reset, then Start, then Split.</summary>
	[Flags]
	public enum TimerActions
	{
		None = 0,
		Reset = 1,
		Start = 2,
		Split = 4,
	}

	/// <summary>
	/// The split logic: once per tick it runs the start, reset and split checks the way the ASL runtime does and says what
	/// the timer should do. No threads and no WinForms, so it is tested without the game. Used from the poll thread only;
	/// the settings come as a snapshot.
	/// </summary>
	public class SplitEngine
	{
		// While playing time is below this nothing splits: memory values still change after loading a save
		const int MinPlayingTime = 600;
		// The optional double-split prevention: a split this soon after the previous one is skipped
		const int DoubleSplitCooldownMs = 2500;
		public const string DoubleSplitSetting = "timer_doubleSplitPrevention";

		static readonly Stopwatch Clock = Stopwatch.StartNew();

		readonly IGameReader reader;
		readonly Func<SettingsSnapshot> settings;
		readonly Func<long> nowMs;
		readonly List<Func<string>> checks = new List<Func<string>>();
		readonly List<Func<bool>> startChecks = new List<Func<bool>>();
		readonly List<Func<bool>> resetChecks = new List<Func<bool>>();
		readonly HashSet<string> done = new HashSet<string>();
		readonly Queue<string> queue = new Queue<string>();
		// The ASL's vars.significantThreads and vars.missionNames: a thread or a marker text, and the split id it splits
		// (null or empty for none); a thread's split can have a condition
		readonly Dictionary<string, string> threads = new Dictionary<string, string>();
		readonly Dictionary<string, Func<bool>> threadSplitConditions = new Dictionary<string, Func<bool>>();
		readonly Dictionary<string, string> missionNames = new Dictionary<string, string>();
		// Not the ASL: a story mission's thread and its marker text, for the marker entered without its text (ThreadStep)
		readonly Dictionary<string, string> threadMarkers = new Dictionary<string, string>();
		// How long after $ONMISSION rises a mission's thread may start and still count as its marker entered: the marker
		// script's walk and fade take about 3.5 s
		const int MarkerFallbackWindowMs = 5000;
		long? onMissionRoseMs;
		bool markerTextSeen;
		long? lastSplitMs;

		public WatcherSet Watchers { get; }

		/// <summary>The settings in effect, for the modules' checks.</summary>
		public SettingsSnapshot Settings => settings();

		/// <summary>The split id handed out by the latest tick that asked for a split.</summary>
		public string LastSplitId { get; private set; }

		readonly RunningScripts scripts = new RunningScripts();

		/// <summary>
		/// The registered thread that was last seen starting, as the ASL's vars.lastStartedMission: "" until one starts and
		/// after a timer reset. Unlike the ASL it is also cleared once its thread has left the game's script list (the
		/// mission was passed or failed, or a save was loaded), at the end of that tick, so the checks still see it in the
		/// tick the mission ends and split its pass.
		/// </summary>
		public string LastStartedMission { get; private set; } = "";

		/// <summary>Whether a script of this name was in the game's script list at this tick's read (the last good one).</summary>
		public bool IsRunning(string thread) => scripts.Contains(thread);

		/// <summary>
		/// Whether a script of this name is in this tick's read of the game's script list and wasn't in the previous good
		/// one: for a check that splits as a script starts, once, rather than while it runs.
		/// </summary>
		public bool HasStarted(string thread) => scripts.HasStarted(thread);

		/// <summary>
		/// The marker text of the mission whose marker was entered in this tick, or null: from the text itself, or, when the
		/// text never appeared, from the mission's thread starting just after $ONMISSION rose (see ThreadStep). For a check
		/// that splits on a marker entry; reading missionStartText itself misses the second case.
		/// </summary>
		public string MarkerEnteredThisTick { get; private set; }

		/// <summary>
		/// Whether the id was tried since the last reset, split or blocked (TrySplit counts both as done): for a check that
		/// splits only after another event of this run, whatever that event's setting.
		/// </summary>
		public bool IsDone(string id) => done.Contains(id);

		/// <param name="nowMs">A clock for the double-split cooldown; the tests pass their own</param>
		public SplitEngine(IGameReader reader, Func<SettingsSnapshot> settings, Func<long> nowMs = null)
		{
			this.reader = reader;
			this.settings = settings;
			this.nowMs = nowMs ?? (() => Clock.ElapsedMilliseconds);
			Watchers = new WatcherSet(reader);
			// What the guards and the timer control read, as in the ASL's init and startup. para_pickup_flag is 1 from
			// the start of the game, 0 once the parachute pickup is used. The ASL uses it as "a game is running" (with
			// game_starts_from_scratch, $MISSION_INTRO_PASSED, which is 1 once the intro mission is over).
			Watchers.Add("playingTime", 0x77CB84);
			Watchers.AddScmGlobal("para_pickup_flag", 1510);
			Watchers.AddScmGlobal("game_starts_from_scratch", 24);
			// The ASL's string watchers, read by the thread and marker steps (oddMissionStartText only by Trucking). thread is
			// the name of the first script in the game's active list, usually the one started last.
			Watchers.AddString("thread", new MemoryLocation(GameAddresses.ActiveScripts, GameAddresses.ScriptName), 10);
			Watchers.AddString("missionStartText", new MemoryLocation(GameAddresses.MissionStartText), 32);
			Watchers.AddString("oddMissionStartText", new MemoryLocation(GameAddresses.OddMissionStartText), 32);
			// Not the ASL: $ONMISSION, which every marker script sets to 1 as its marker is entered, in the frame it shows the
			// mission's title (and so does a save point, while its menu is open). Read by the marker fallback.
			Watchers.AddScmGlobal("flag_player_on_mission", 409);
		}

		/// <summary>
		/// When this thread starts (other than as the last started mission), it becomes the last started mission and splits
		/// the id, if there is one. A thread registered twice throws, as the ASL's dictionary does.
		/// </summary>
		/// <param name="splitIf">Not the ASL: when given, the id is only tried while this is true. The thread still becomes the
		/// last started mission, and a start that doesn't split isn't counted as done, so a later start can split.</param>
		public void RegisterThread(string thread, string splitId, Func<bool> splitIf = null)
		{
			threads.Add(thread, splitId);
			if (splitIf != null)
				threadSplitConditions.Add(thread, splitIf);
		}

		/// <summary>When this mission name appears as the marker text, split the id. A name registered twice throws.</summary>
		public void RegisterMissionName(string text, string splitId) => missionNames.Add(text, splitId);

		/// <summary>
		/// A story mission's marker: its text splits the id, as <see cref="RegisterMissionName"/>. Not the ASL: when the text
		/// never appears (the replay trick can skip it), the mission's thread starting within a few seconds of $ONMISSION
		/// rising splits the id instead, just before the start. Register the thread with <see cref="RegisterThread"/> too.
		/// </summary>
		public void RegisterMissionMarker(string thread, string text, string splitId)
		{
			RegisterMissionName(text, splitId);
			threadMarkers.Add(thread, text);
		}

		/// <summary>
		/// Adds a split check: it reads watchers and calls TrySplit itself, or returns a split id (or null) for the engine
		/// to try. Checks run in the order they were added, only while the timer runs.
		/// </summary>
		public void AddCheck(Func<string> check) => checks.Add(check);

		/// <summary>Adds a start check, run while the timer is not running. True starts the timer.</summary>
		public void AddStartCheck(Func<bool> check) => startChecks.Add(check);

		/// <summary>Adds a reset check, run while the timer runs, before the split checks. True resets the timer.</summary>
		public void AddResetCheck(Func<bool> check) => resetChecks.Add(check);

		/// <summary>
		/// Queues the split unless it already split since the last reset, is disabled in the settings, or comes within the
		/// cooldown with double-split prevention on. A split that is blocked still counts as done, as in the ASL, so enabling
		/// it later in the run doesn't split it late.
		/// </summary>
		public bool TrySplit(string id)
		{
			// Every call is logged, so the trace shows which splits happen, and why one didn't reach the timer
			if (!done.Add(id))
			{
				DebugOutput.Write("split prevented (already done): " + id);
				return false;
			}
			SettingsSnapshot snapshot = settings();
			if (!snapshot.IsEnabled(id))
			{
				DebugOutput.Write("split prevented (disabled in settings): " + id);
				return false;
			}
			long now = nowMs();
			if (snapshot.IsEnabled(DoubleSplitSetting) && lastSplitMs.HasValue && now - lastSplitMs.Value < DoubleSplitCooldownMs)
			{
				DebugOutput.Write("split prevented (double split prevention): " + id);
				return false;
			}
			lastSplitMs = now;
			queue.Enqueue(id);
			DebugOutput.Write("split: " + id);
			return true;
		}

		/// <summary>
		/// The ASL's ValidateMissionProgress, with its exact reads, for the start of a mission's check. False without reading
		/// anything when the setting (or an ancestor) is off or the last started mission is not this thread. Otherwise reads
		/// the chain counter: once it is at or past <paramref name="after"/>, splits <paramref name="passId"/> if it just
		/// changed from <paramref name="before"/>, and returns false. True only while the mission is active and not passed.
		/// </summary>
		/// <param name="chain">The name of the counter's watcher, which the module registers</param>
		public bool MissionInProgress(string thread, string chain, int before, int after, string passId, string settingId)
		{
			if (!settings().IsEnabled(settingId))
				return false;
			if (LastStartedMission != thread)
				return false;
			IntWatcher counter = Watchers.Get(chain);
			if (counter.Current >= after)
			{
				if (counter.Changed && counter.Old == before)
					TrySplit(passId);
				return false;
			}
			return true;
		}

		/// <summary>The timer was reset: every split may happen again.</summary>
		public void Reset()
		{
			done.Clear();
			queue.Clear();
			lastSplitMs = null;
			onMissionRoseMs = null;
			markerTextSeen = false;
			LastStartedMission = "";
			Watchers.NewContext();
		}

		/// <summary>
		/// One step, in the ASL runtime's order: while the timer runs, the reset checks and, if none fires, the split
		/// checks; then, if the timer is not running (or was just reset), the start checks. Nothing while it has ended.
		/// </summary>
		public TimerActions Tick(TimerPhase phase)
		{
			Watchers.NewTick();
			if (reader.Status != GameStatus.Found)
			{
				// What is read next comes from another game, or from one that went on without us
				Watchers.NewContext();
				return TimerActions.None;
			}

			TimerActions actions = TimerActions.None;
			if (phase == TimerPhase.Running || phase == TimerPhase.Paused)
			{
				if (Any(resetChecks))
				{
					// Clear now rather than when LiveSplit's reset event comes back round, so nothing from before the reset
					// survives into the ticks in between
					Reset();
					actions |= TimerActions.Reset;
					phase = TimerPhase.NotRunning;
				}
				else if (TickSplit())
				{
					actions |= TimerActions.Split;
				}
			}
			if (phase == TimerPhase.NotRunning && Any(startChecks))
				actions |= TimerActions.Start;
			return actions;
		}

		// Every check runs, so the watchers they read are read every tick; any true wins
		static bool Any(List<Func<bool>> list)
		{
			bool result = false;
			foreach (Func<bool> check in list)
			{
				try
				{
					if (check())
						result = true;
				}
				catch (Exception ex)
				{
					Log.Error(ex);
				}
			}
			return result;
		}

		bool TickSplit()
		{
			// Nothing splits in the main menu, or just after loading a save. The queue waits.
			// All three are read before any test, as the ASL does: a condition that skipped a read would change that
			// watcher's Old, and the checks (which share the watchers) would see a change the ASL does not.
			IntWatcher playingTime = Watchers.Get("playingTime");
			IntWatcher parachuteSpawned = Watchers.Get("para_pickup_flag");
			IntWatcher introPassed = Watchers.Get("game_starts_from_scratch");
			if (parachuteSpawned.Current == 0 && introPassed.Current == 0)
				return false;
			if (playingTime.Current < MinPlayingTime)
				return false;

			// Before the checks, which read LastStartedMission. One try for both steps: in the ASL an exception here ends
			// the split block for the tick, but it would skip the checks too, which is worse.
			bool scriptsRead = false;
			MarkerEnteredThisTick = null;
			try
			{
				// Read every tick, so a rise is seen in its tick. A rise starts a new wait for the marker text.
				IntWatcher onMission = Watchers.Get("flag_player_on_mission");
				if (onMission.Changed && onMission.Old == 0 && onMission.Current == 1)
				{
					onMissionRoseMs = nowMs();
					markerTextSeen = false;
				}
				ThreadStep();
				// After the thread step, before the checks: a passed mission writes its counter before it ends, so when the
				// list shows it gone, the checks below already see the counter
				scriptsRead = scripts.Update(reader);
				MarkerStep();
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}

			foreach (Func<string> check in checks)
			{
				try
				{
					string id = check();
					if (!string.IsNullOrEmpty(id))
						TrySplit(id);
				}
				catch (Exception ex)
				{
					// One broken check mustn't stop the others
					Log.Error(ex);
				}
			}

			// After the checks, so they still saw the mission in the tick it ended. Only on a good read of the list.
			if (scriptsRead && LastStartedMission != "" && !scripts.Contains(LastStartedMission))
			{
				DebugOutput.Write("last started mission ended: " + LastStartedMission);
				LastStartedMission = "";
				Watchers.NewContext();
			}

			// One split per tick, so splits that happen together reach the timer one after the other
			if (queue.Count == 0)
				return false;
			LastSplitId = queue.Dequeue();
			return true;
		}

		// The ASL's "Starting a certain mission": the first script in the active list is probably the one started last
		void ThreadStep()
		{
			StringWatcher thread = Watchers.GetString("thread");
			if (!thread.Changed)
				return;
			if (LastStartedMission == thread.Current || !threads.TryGetValue(thread.Current, out string id))
				return;
			MarkerFallback(thread.Current);
			if (!string.IsNullOrEmpty(id))
			{
				// A start already done only says so: its condition isn't tested (nor its watchers read)
				if (IsDone(id) || !threadSplitConditions.TryGetValue(thread.Current, out Func<bool> splitIf) || splitIf())
					TrySplit(id);
				else
					DebugOutput.Write("split held back (its condition is not met): " + id);
			}
			LastStartedMission = thread.Current;
			Watchers.NewContext();
		}

		// The ASL's "Marker entry": the yellow text shown when a mission marker is entered
		void MarkerStep()
		{
			StringWatcher text = Watchers.GetString("missionStartText");
			if (!text.Changed)
				return;
			DebugOutput.Write($"missionStartText: {Quote(text.Old)} -> {Quote(text.Current)}");
			if (string.IsNullOrEmpty(text.Current) || !missionNames.TryGetValue(text.Current, out string id))
				return;
			markerTextSeen = true;
			MarkerEnteredThisTick = text.Current;
			if (!string.IsNullOrEmpty(id))
				TrySplit(id);
		}

		// Not the ASL. Every marker script, as its marker is entered, sets $ONMISSION to 1 and shows the mission's title in
		// the same frame, walks CJ in and fades (about 3.5 s), then starts the mission (INT, SWEET and the others in the
		// decompiled main.scm). The replay trick skips the walk and the fade and can skip the title too: on 2026-11-19 Ryder's
		// thread started 62 ms after $ONMISSION rose, with no text. So when a story mission's thread starts within the window
		// of a rise and no marker text came with it, its marker counts as entered now, just before its start. A save point
		// also raises $ONMISSION, but starts no mission thread.
		void MarkerFallback(string thread)
		{
			if (!threadMarkers.TryGetValue(thread, out string text) || markerTextSeen || !onMissionRoseMs.HasValue)
				return;
			if (nowMs() - onMissionRoseMs.Value > MarkerFallbackWindowMs)
				return;
			// The text in this very tick, which the marker step reads after this one
			if (Watchers.GetString("missionStartText").Current == text)
				return;
			markerTextSeen = true;
			MarkerEnteredThisTick = text;
			DebugOutput.Write("marker text not seen; marker entered from $ONMISSION: " + text);
			string id = missionNames[text];
			if (!string.IsNullOrEmpty(id))
				TrySplit(id);
		}

		static string Quote(string text) => text == null ? "null" : "\"" + text + "\"";
	}
}
