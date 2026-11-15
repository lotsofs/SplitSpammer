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
		long? lastSplitMs;

		public WatcherSet Watchers { get; }

		/// <summary>The settings in effect, for the modules' checks.</summary>
		public SettingsSnapshot Settings => settings();

		/// <summary>The split id handed out by the latest tick that asked for a split.</summary>
		public string LastSplitId { get; private set; }

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
			if (!done.Add(id))
				return false;
			SettingsSnapshot snapshot = settings();
			if (!snapshot.IsEnabled(id))
				return false;
			long now = nowMs();
			if (snapshot.IsEnabled(DoubleSplitSetting) && lastSplitMs.HasValue && now - lastSplitMs.Value < DoubleSplitCooldownMs)
				return false;
			lastSplitMs = now;
			queue.Enqueue(id);
			return true;
		}

		/// <summary>The timer was reset: every split may happen again.</summary>
		public void Reset()
		{
			done.Clear();
			queue.Clear();
			lastSplitMs = null;
		}

		/// <summary>
		/// One step, in the ASL runtime's order: while the timer runs, the reset checks and, if none fires, the split
		/// checks; then, if the timer is not running (or was just reset), the start checks. Nothing while it has ended.
		/// </summary>
		public TimerActions Tick(TimerPhase phase)
		{
			Watchers.NewTick();
			if (reader.Status != GameStatus.Found)
				return TimerActions.None;

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

			// One split per tick, so splits that happen together reach the timer one after the other
			if (queue.Count == 0)
				return false;
			LastSplitId = queue.Dequeue();
			return true;
		}
	}
}
