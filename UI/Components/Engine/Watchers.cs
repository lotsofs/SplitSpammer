using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>An int in the game's memory, with the value it had at the previous read.</summary>
	public class IntWatcher
	{
		readonly int moduleOffset;
		readonly int mask;
		bool hasValue;

		public int Old { get; private set; }
		public int Current { get; private set; }
		public bool Changed { get; private set; }

		internal IntWatcher(int moduleOffset, int bytes)
		{
			this.moduleOffset = moduleOffset;
			mask = bytes == 1 ? 0xFF : bytes == 2 ? 0xFFFF : -1;
		}

		// Same as the ASL's MemoryWatcher<int> (LiveSplit.ComponentUtil in LiveSplit.Core), checked against the real class
		// 2026-11-14: every update moves Current to Old; the first successful read counts as unchanged and leaves Old at 0;
		// a failed read leaves Current alone, so Old becomes equal to it.
		internal void Update(IGameReader reader)
		{
			Old = Current;
			if (!reader.TryReadInt(moduleOffset, out int value))
			{
				Changed = false;
				return;
			}
			Current = value & mask;
			Changed = hasValue && Old != Current;
			hasValue = true;
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

		/// <summary>A value of 1, 2 or 4 bytes at a module offset; a narrower one is the low bytes of a 4-byte read, as in the ASL.</summary>
		public void Add(string name, int moduleOffset, int bytes = 4)
		{
			if (bytes != 1 && bytes != 2 && bytes != 4)
				throw new ArgumentException("A watcher is 1, 2 or 4 bytes, not " + bytes);
			watchers.Add(name, new IntWatcher(moduleOffset, bytes));
		}

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
