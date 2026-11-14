using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>An int in the game's memory, with the value it had at the previous read.</summary>
	public class IntWatcher
	{
		readonly int moduleOffset;
		bool hasValue;

		public int Old { get; private set; }
		public int Current { get; private set; }
		public bool Changed { get; private set; }

		internal IntWatcher(int moduleOffset)
		{
			this.moduleOffset = moduleOffset;
		}

		internal void Update(IGameReader reader)
		{
			// A failed read keeps the last value, as the ASL's MemoryWatcher does
			if (!reader.TryReadInt(moduleOffset, out int value))
			{
				Changed = false;
				return;
			}
			// The first read has no earlier value to differ from. Not confirmed against the ASL's MemoryWatcher yet (item ITB).
			Old = hasValue ? Current : value;
			Current = value;
			hasValue = true;
			Changed = Old != Current;
		}
	}

	/// <summary>
	/// The memory the checks watch. A watcher is read lazily, at most once per tick, when a check asks for it, as the ASL's
	/// GetWatcher does: the lazy reads decide what Old is, so they are kept exact. Used from the poll thread only.
	/// </summary>
	public class WatcherSet
	{
		readonly IGameReader reader;
		readonly Dictionary<string, IntWatcher> watchers = new Dictionary<string, IntWatcher>();
		readonly HashSet<string> readThisTick = new HashSet<string>();

		public WatcherSet(IGameReader reader)
		{
			this.reader = reader;
		}

		public void Add(string name, int moduleOffset) => watchers.Add(name, new IntWatcher(moduleOffset));

		/// <summary>An SCM global variable, $n.</summary>
		public void AddScmGlobal(string name, int n) => Add(name, GameAddresses.ScmGlobal(n));

		/// <summary>A mission local variable, n@. All missions share these, so only read them while their mission is active.</summary>
		public void AddScmMissionLocal(string name, int n) => Add(name, GameAddresses.ScmMissionLocal(n));

		/// <summary>Starts a tick: every watcher may be read again.</summary>
		public void NewTick() => readThisTick.Clear();

		public IntWatcher Get(string name)
		{
			if (!watchers.TryGetValue(name, out IntWatcher watcher))
				throw new ArgumentException("No watcher named " + name);
			if (readThisTick.Add(name))
				watcher.Update(reader);
			return watcher;
		}
	}
}
