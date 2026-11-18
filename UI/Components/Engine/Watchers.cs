using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Where a watcher reads: a module offset, or, like LiveSplit's DeepPointer, a module offset holding a pointer followed
	/// through any further offsets. The game is 32-bit, so every pointer is 4 bytes; a null pointer is a failed read.
	/// </summary>
	public sealed class MemoryLocation : IEquatable<MemoryLocation>
	{
		readonly int[] offsets;

		public int ModuleOffset { get; }

		/// <param name="offsets">None: read at the module offset. Otherwise the module offset holds a pointer, every offset
		/// but the last is added to the pointer and read as the next one, and the last is added to give the address read.</param>
		public MemoryLocation(int moduleOffset, params int[] offsets)
		{
			ModuleOffset = moduleOffset;
			this.offsets = offsets ?? new int[0];
		}

		public bool TryRead(IGameReader reader, byte[] buffer)
		{
			if (offsets.Length == 0)
				return reader.TryReadBytes(ModuleOffset, buffer);
			if (!reader.TryReadInt(ModuleOffset, out int pointer) || pointer == 0)
				return false;
			var pointerBytes = new byte[4];
			for (int i = 0; i < offsets.Length - 1; i++)
			{
				if (!reader.TryReadAbsoluteBytes(Address(pointer, offsets[i]), pointerBytes))
					return false;
				pointer = BitConverter.ToInt32(pointerBytes, 0);
				if (pointer == 0)
					return false;
			}
			return reader.TryReadAbsoluteBytes(Address(pointer, offsets[offsets.Length - 1]), buffer);
		}

		// A 32-bit pointer is unsigned
		static long Address(int pointer, int offset) => (uint)pointer + (long)offset;

		public bool Equals(MemoryLocation other) =>
			other != null && ModuleOffset == other.ModuleOffset && offsets.SequenceEqual(other.offsets);

		public override bool Equals(object obj) => Equals(obj as MemoryLocation);

		public override int GetHashCode() => offsets.Aggregate(ModuleOffset, (hash, offset) => hash * 31 + offset);

		public override string ToString() =>
			offsets.Length == 0 ? $"+0x{ModuleOffset:X}" : $"[+0x{ModuleOffset:X}]" + string.Concat(offsets.Select(o => $"+0x{o:X}"));
	}

	/// <summary>A value in the game's memory, read at most once per tick by <see cref="WatcherSet"/>.</summary>
	public abstract class Watcher
	{
		public bool Changed { get; protected set; }

		// For WatcherSet: the tick and the context of the last read
		internal int LastReadTick = -1;
		internal int LastReadContext;

		/// <param name="afterGap">The read counts as unchanged: Old becomes the value read</param>
		internal abstract void Update(IGameReader reader, bool afterGap);
	}

	/// <summary>An int in the game's memory, with the value it had at the previous read.</summary>
	public class IntWatcher : Watcher
	{
		readonly int mask;
		bool hasValue;

		public int Old { get; private set; }
		public int Current { get; private set; }

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
		// a failed read leaves Current alone, so Old becomes equal to it. Unlike it, a read after a gap counts as unchanged.
		internal override void Update(IGameReader reader, bool afterGap)
		{
			Old = Current;
			if (!reader.TryReadInt(ModuleOffset, out int value))
			{
				Changed = false;
				return;
			}
			Current = value & mask;
			if (afterGap)
				Old = Current;
			Changed = hasValue && Old != Current;
			hasValue = true;
		}
	}

	/// <summary>A string in the game's memory, with the value it had at the previous read; null until the first read.</summary>
	public class StringWatcher : Watcher
	{
		readonly byte[] buffer;
		bool hasValue;

		public string Old { get; private set; }
		public string Current { get; private set; }

		internal MemoryLocation Location { get; }
		internal int Bytes => buffer.Length;

		internal StringWatcher(MemoryLocation location, int bytes)
		{
			Location = location;
			buffer = new byte[bytes];
		}

		// Same as the ASL's StringWatcher with ReadStringType.AutoDetect (LiveSplit.ComponentUtil), checked against the real
		// class 2026-11-15. Unlike MemoryWatcher<int>, a failed read leaves Old alone too, so a change across it is still seen
		// on the next good read. A pointer that is null or can't be read is a failed read. The first good read counts as
		// unchanged and leaves Old null. Unlike it, a read after a gap counts as unchanged.
		internal override void Update(IGameReader reader, bool afterGap)
		{
			if (!Location.TryRead(reader, buffer))
			{
				Changed = false;
				return;
			}
			string value = Decode(buffer);
			Old = afterGap ? value : Current;
			Current = value;
			Changed = hasValue && !string.Equals(Old, Current, StringComparison.Ordinal);
			hasValue = true;
		}

		// LiveSplit's ReadString with AutoDetect: UTF-16 when the second byte is 0, UTF-8 otherwise, cut at the first null
		internal static string Decode(byte[] bytes)
		{
			string text = bytes.Length >= 2 && bytes[1] == 0 ? Encoding.Unicode.GetString(bytes) : Encoding.UTF8.GetString(bytes);
			int end = text.IndexOf('\0');
			return end < 0 ? text : text.Substring(0, end);
		}
	}

	/// <summary>
	/// The memory the checks watch. A watcher is read lazily, at most once per tick, when a check asks for it, as the ASL's
	/// GetWatcher does: the lazy reads decide what Old is, so they are kept exact. Old is the value at the last read, so a
	/// change in a tick in which a check skipped the read is seen on the next one; but unlike the ASL, a read after a gap
	/// across a new context (a mission started or ended, a timer reset, the game lost) counts as unchanged, so it can't
	/// compare with a value from another mission or another game. There is one watcher per address, as in the ASL, under
	/// any number of names: the leak names a mission local differently in each mission that uses it, and every mission
	/// shares its Old. Each module registers every watcher it reads. Used from the poll thread only.
	/// </summary>
	public class WatcherSet
	{
		readonly IGameReader reader;
		readonly Dictionary<string, Watcher> byName = new Dictionary<string, Watcher>();
		readonly Dictionary<int, IntWatcher> byAddress = new Dictionary<int, IntWatcher>();
		readonly Dictionary<MemoryLocation, StringWatcher> stringsByLocation = new Dictionary<MemoryLocation, StringWatcher>();
		readonly HashSet<Watcher> readThisTick = new HashSet<Watcher>();
		int tick;
		int context;

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
			if (byName.TryGetValue(name, out Watcher named))
			{
				if (named is IntWatcher same && same.ModuleOffset == moduleOffset && same.Bytes == bytes)
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

		/// <summary>
		/// A string of up to the given number of bytes, as the ASL's StringWatcher reads it. The same sharing rules as for
		/// ints apply: one watcher per location, a location watched with another size throws.
		/// </summary>
		public void AddString(string name, MemoryLocation location, int bytes)
		{
			if (bytes < 1)
				throw new ArgumentException("A string watcher reads at least 1 byte, not " + bytes);
			if (byName.TryGetValue(name, out Watcher named))
			{
				if (named is StringWatcher same && same.Location.Equals(location) && same.Bytes == bytes)
					return;
				throw new ArgumentException($"The watcher name {name} is already used for {Describe(named)}, not {Describe(location, bytes)}");
			}
			if (stringsByLocation.TryGetValue(location, out StringWatcher watcher))
			{
				if (watcher.Bytes != bytes)
					throw new ArgumentException($"{name}: {Describe(location, bytes)} is already watched as {watcher.Bytes} bytes");
			}
			else
			{
				watcher = new StringWatcher(location, bytes);
				stringsByLocation.Add(location, watcher);
			}
			byName.Add(name, watcher);
		}

		static string Describe(Watcher watcher) =>
			watcher is IntWatcher i ? Describe(i.ModuleOffset, i.Bytes) : Describe(((StringWatcher)watcher).Location, ((StringWatcher)watcher).Bytes);

		static string Describe(int moduleOffset, int bytes) => $"{bytes} bytes at +0x{moduleOffset:X}";

		static string Describe(MemoryLocation location, int bytes) => $"a string of {bytes} bytes at {location}";

		/// <summary>An SCM global variable, $n.</summary>
		public void AddScmGlobal(string name, int n) => Add(name, GameAddresses.ScmGlobal(n));

		/// <summary>
		/// A mission local variable, n@. All missions share these, so only read them while their mission is active. A local
		/// that another mission also reads is one watcher, whatever name each gives it.
		/// </summary>
		public void AddScmMissionLocal(string name, int n) => Add(name, GameAddresses.ScmMissionLocal(n));

		/// <summary>Starts a tick: every watcher may be read again.</summary>
		public void NewTick()
		{
			readThisTick.Clear();
			tick++;
		}

		/// <summary>
		/// Something changed that makes an old read meaningless: a mission started or ended, the timer was reset, the game
		/// was lost. A watcher not read in the previous tick counts its next read as unchanged; one read every tick still
		/// reports a change across it, since that happened between two ticks.
		/// </summary>
		public void NewContext() => context++;

		public IntWatcher Get(string name) => Get<IntWatcher>(name);

		public StringWatcher GetString(string name) => Get<StringWatcher>(name);

		T Get<T>(string name) where T : Watcher
		{
			if (!byName.TryGetValue(name, out Watcher watcher))
				throw new ArgumentException("No watcher named " + name);
			if (!(watcher is T typed))
				throw new ArgumentException($"The watcher {name} is not a {typeof(T).Name}");
			// Per watcher, not per name: two names for one address read it once a tick
			if (readThisTick.Add(typed))
			{
				bool afterGap = typed.LastReadTick >= 0 && typed.LastReadTick < tick - 1 && typed.LastReadContext != context;
				typed.Update(reader, afterGap);
				typed.LastReadTick = tick;
				typed.LastReadContext = context;
			}
			return typed;
		}
	}
}
