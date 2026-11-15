using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>An int in the game's memory, with the value it had at the previous read.</summary>
	public class IntWatcher
	{
		readonly int mask;
		bool hasValue;

		public int Old { get; private set; }
		public int Current { get; private set; }
		public bool Changed { get; private set; }

		internal int ModuleOffset { get; }
		internal int Bytes { get; }

		internal IntWatcher(int moduleOffset, int bytes)
		{
			ModuleOffset = moduleOffset;
			Bytes = bytes;
			mask = bytes == 1 ? 0xFF : bytes == 2 ? 0xFFFF : -1;
		}

		// Same as the ASL's MemoryWatcher<int> (LiveSplit.ComponentUtil in LiveSplit.Core), checked against the real class
		// 2026-11-14: every update moves Current to Old; the first successful read counts as unchanged and leaves Old at 0;
		// a failed read leaves Current alone, so Old becomes equal to it.
		internal void Update(IGameReader reader)
		{
			Old = Current;
			if (!reader.TryReadInt(ModuleOffset, out int value))
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
	/// GetWatcher does: the lazy reads decide what Old is, so they are kept exact. There is one watcher per address, as in
	/// the ASL, under any number of names: the leak names a mission local differently in each mission that uses it, and
	/// every mission shares its Old. Each module registers every watcher it reads. Used from the poll thread only.
	/// </summary>
	public class WatcherSet
	{
		readonly IGameReader reader;
		readonly Dictionary<string, IntWatcher> byName = new Dictionary<string, IntWatcher>();
		readonly Dictionary<int, IntWatcher> byAddress = new Dictionary<int, IntWatcher>();
		readonly HashSet<IntWatcher> readThisTick = new HashSet<IntWatcher>();

		public WatcherSet(IGameReader reader)
		{
			this.reader = reader;
		}

		/// <summary>
		/// A value of 1, 2 or 4 bytes at a module offset; a narrower one is the low bytes of a 4-byte read, as in the ASL.
		/// An address already watched with the same size gives the same watcher, under this name too; adding a name again
		/// for the same address does nothing. A name used for another address, or an address with another size, throws.
		/// </summary>
		public void Add(string name, int moduleOffset, int bytes = 4)
		{
			if (bytes != 1 && bytes != 2 && bytes != 4)
				throw new ArgumentException("A watcher is 1, 2 or 4 bytes, not " + bytes);
			if (byName.TryGetValue(name, out IntWatcher named))
			{
				if (named.ModuleOffset == moduleOffset && named.Bytes == bytes)
					return;
				throw new ArgumentException($"The watcher name {name} is already used for {Describe(named)}, not {Describe(moduleOffset, bytes)}");
			}
			if (byAddress.TryGetValue(moduleOffset, out IntWatcher watcher))
			{
				if (watcher.Bytes != bytes)
					throw new ArgumentException($"{name}: {Describe(moduleOffset, bytes)} is already watched as {watcher.Bytes} bytes");
			}
			else
			{
				watcher = new IntWatcher(moduleOffset, bytes);
				byAddress.Add(moduleOffset, watcher);
			}
			byName.Add(name, watcher);
		}

		static string Describe(IntWatcher watcher) => Describe(watcher.ModuleOffset, watcher.Bytes);

		static string Describe(int moduleOffset, int bytes) => $"{bytes} bytes at +0x{moduleOffset:X}";

		/// <summary>An SCM global variable, $n.</summary>
		public void AddScmGlobal(string name, int n) => Add(name, GameAddresses.ScmGlobal(n));

		/// <summary>
		/// A mission local variable, n@. All missions share these, so only read them while their mission is active. A local
		/// that another mission also reads is one watcher, whatever name each gives it.
		/// </summary>
		public void AddScmMissionLocal(string name, int n) => Add(name, GameAddresses.ScmMissionLocal(n));

		/// <summary>Starts a tick: every watcher may be read again.</summary>
		public void NewTick() => readThisTick.Clear();

		public IntWatcher Get(string name)
		{
			if (!byName.TryGetValue(name, out IntWatcher watcher))
				throw new ArgumentException("No watcher named " + name);
			// Per watcher, not per name: two names for one address read it once a tick
			if (readThisTick.Add(watcher))
				watcher.Update(reader);
			return watcher;
		}
	}
}
