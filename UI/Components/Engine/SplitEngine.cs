using LiveSplit.Options;
using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The split logic: once per tick it runs the guards and the checks and hands out at most one split. No threads and no
	/// WinForms, so it is tested without the game. Used from the poll thread only; the settings come as a snapshot.
	/// </summary>
	public class SplitEngine
	{
		// While playing time is below this nothing splits: memory values still change after loading a save
		const int MinPlayingTime = 600;

		readonly IGameReader reader;
		readonly Func<SettingsSnapshot> settings;
		readonly List<Func<string>> checks = new List<Func<string>>();
		readonly HashSet<string> done = new HashSet<string>();
		readonly Queue<string> queue = new Queue<string>();

		public WatcherSet Watchers { get; }

		/// <summary>The split id handed out by the latest Tick that returned true.</summary>
		public string LastSplitId { get; private set; }

		public SplitEngine(IGameReader reader, Func<SettingsSnapshot> settings)
		{
			this.reader = reader;
			this.settings = settings;
			Watchers = new WatcherSet(reader);
			// What the guards read, as in the ASL's init and startup
			Watchers.Add("playingTime", 0x77CB84);
			Watchers.AddScmGlobal("intro_newGameStarted", 1510);
			Watchers.AddScmGlobal("intro_passed", 24);
		}

		/// <summary>
		/// Adds a check: it reads watchers and calls TrySplit itself, or returns a split id (or null) for the engine to try.
		/// Checks run in the order they were added.
		/// </summary>
		public void AddCheck(Func<string> check) => checks.Add(check);

		/// <summary>
		/// Queues the split unless it already split since the last reset or is disabled in the settings. A split that is
		/// disabled still counts as done, as in the ASL, so enabling it later in the run doesn't split it late.
		/// </summary>
		public bool TrySplit(string id)
		{
			if (!done.Add(id))
				return false;
			if (!settings().IsEnabled(id))
				return false;
			queue.Enqueue(id);
			return true;
		}

		/// <summary>The timer was reset: every split may happen again.</summary>
		public void Reset()
		{
			done.Clear();
			queue.Clear();
		}

		/// <summary>One step. True if a split should be sent to the timer now.</summary>
		public bool Tick()
		{
			Watchers.NewTick();
			if (reader.Status != GameStatus.Found)
				return false;

			// Nothing splits in the main menu, or just after loading a save. The queue waits.
			if (Watchers.Get("intro_newGameStarted").Current == 0 && Watchers.Get("intro_passed").Current == 0)
				return false;
			if (Watchers.Get("playingTime").Current < MinPlayingTime)
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
