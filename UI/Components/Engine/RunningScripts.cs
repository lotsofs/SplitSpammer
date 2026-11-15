using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The names of every script in the game's active list, not just the first one the thread watcher reads. Both list
	/// heads and the whole script array are read in one go and the lists walked locally. Every record is in either the
	/// active or the idle list, so a read the game tore while starting or ending a script (seen on the running game: the
	/// first script ended between two reads, and the walk went on through the idle list, whose records keep the names of
	/// scripts that ended) shows as a record in neither list or in both, and is rejected. A rejected or failed read keeps
	/// the previous names. Used from the poll thread only.
	/// </summary>
	public class RunningScripts
	{
		const int NameBytes = 10;
		// The idle head, the active head, then the array, which follow each other in memory
		const int HeadsBytes = GameAddresses.ScriptsArray - GameAddresses.IdleScripts;

		readonly byte[] block = new byte[HeadsBytes + GameAddresses.ScriptCount * GameAddresses.ScriptRecordSize];
		readonly byte[] nameBytes = new byte[NameBytes];
		readonly bool[] seen = new bool[GameAddresses.ScriptCount];
		HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
		bool hasValue;

		/// <summary>The names in the list at the last good read, each once (the list can hold a name more than once).</summary>
		public IReadOnlyCollection<string> Names => names;

		public bool Contains(string name) => names.Contains(name);

		/// <summary>Reads the list. False when it could not be read or did not hold together; the names stay as they were.</summary>
		public bool Update(IGameReader reader)
		{
			if (reader.ModuleBase == 0 || !reader.TryReadBytes(GameAddresses.IdleScripts, block))
				return false;
			long arrayStart = reader.ModuleBase + GameAddresses.ScriptsArray;
			Array.Clear(seen, 0, seen.Length);
			var found = new HashSet<string>(StringComparer.Ordinal);
			int active = Walk(BitConverter.ToInt32(block, GameAddresses.ActiveScripts - GameAddresses.IdleScripts), arrayStart, found);
			int idle = active < 0 ? -1 : Walk(BitConverter.ToInt32(block, 0), arrayStart, null);
			if (idle < 0 || active + idle != GameAddresses.ScriptCount)
				return false;

			LogChanges(found);
			names = found;
			hasValue = true;
			return true;
		}

		// Walks one list from its head, checking every link; adds the names when asked. The number of records, or -1 when
		// the list does not hold together or meets a record already walked.
		int Walk(int head, long arrayStart, HashSet<string> found)
		{
			int count = 0;
			long previous = 0;
			long pointer = (uint)head;
			while (pointer != 0)
			{
				long offset = pointer - arrayStart;
				if (offset < 0 || offset >= block.Length - HeadsBytes || offset % GameAddresses.ScriptRecordSize != 0)
					return -1;
				int slot = (int)(offset / GameAddresses.ScriptRecordSize);
				int record = HeadsBytes + (int)offset;
				if (seen[slot] || (uint)BitConverter.ToInt32(block, record + 4) != previous)
					return -1;
				seen[slot] = true;
				count++;
				if (found != null)
				{
					// The same bytes and decoding as the thread watcher, so a name always matches its own thread
					Array.Copy(block, record + GameAddresses.ScriptName, nameBytes, 0, NameBytes);
					found.Add(StringWatcher.Decode(nameBytes));
				}
				previous = pointer;
				pointer = (uint)BitConverter.ToInt32(block, record);
			}
			return count;
		}

		// To see on the running game which scripts come and go
		void LogChanges(HashSet<string> found)
		{
			if (!hasValue)
			{
				Log.Info("[SplitSpammer] scripts running: " + string.Join(" ", found.OrderBy(n => n, StringComparer.Ordinal)));
				return;
			}
			string[] started = found.Where(n => !names.Contains(n)).ToArray();
			string[] ended = names.Where(n => !found.Contains(n)).ToArray();
			if (started.Length + ended.Length > 0)
				Log.Info("[SplitSpammer] scripts:" + string.Concat(started.Select(n => " +" + n)) + string.Concat(ended.Select(n => " -" + n)));
		}
	}
}
