using System;
using System.Collections.Generic;
using System.Text;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>One of the game's map zones, from info.zon: its label (JEF2, CHC4A), its text key and its box.</summary>
	public sealed class Zone
	{
		/// <summary>The zone's own name in info.zon, such as JEF2 or CHC4A.</summary>
		public string Label { get; }
		/// <summary>The key of the area name the HUD shows, such as JEF (Jefferson); several zones share one.</summary>
		public string TextKey { get; }
		public Box Box { get; }

		internal Zone(string label, string textKey, Box box)
		{
			Label = label;
			TextKey = textKey;
			Box = box;
		}

		public override string ToString() => $"{Label} ({TextKey}) {Box}";
	}

	/// <summary>
	/// The game's zone array (CTheZones::NavigationZoneArray), read in one go. Its records are 0x20 bytes: the label and
	/// the text key (8 characters each), the box as six shorts (x1, y1, z1, x2, y2, z2), then an index into the zone-info
	/// records, a type and a level byte, which this doesn't use. Seen on the running game 2026-11-19: 379 zones, the first
	/// SAN_AND (the whole map, level 0), all others level 1, cities (LA) and their districts alike, so nesting is only seen
	/// in the boxes.
	/// </summary>
	public sealed class ZoneTable
	{
		const int MaxZones = 2000;

		readonly List<Zone> zones;

		public IReadOnlyList<Zone> Zones => zones;

		ZoneTable(List<Zone> zones)
		{
			this.zones = zones;
		}

		/// <summary>Null when the count or a record doesn't look right, as while the game is still loading.</summary>
		internal static ZoneTable TryRead(IGameReader reader, int count)
		{
			if (count <= 0 || count > MaxZones)
				return null;
			var bytes = new byte[count * GameAddresses.ZoneRecordSize];
			if (!reader.TryReadBytes(GameAddresses.ZoneArray, bytes))
				return null;
			var zones = new List<Zone>(count);
			for (int i = 0; i < count; i++)
			{
				int at = i * GameAddresses.ZoneRecordSize;
				string label = Name(bytes, at);
				string key = Name(bytes, at + 8);
				if (string.IsNullOrEmpty(label) || key == null)
					return null;
				short x1 = BitConverter.ToInt16(bytes, at + 16), y1 = BitConverter.ToInt16(bytes, at + 18), z1 = BitConverter.ToInt16(bytes, at + 20);
				short x2 = BitConverter.ToInt16(bytes, at + 22), y2 = BitConverter.ToInt16(bytes, at + 24), z2 = BitConverter.ToInt16(bytes, at + 26);
				if (x1 > x2 || y1 > y2 || z1 > z2)
					return null;
				zones.Add(new Zone(label, key, Box.Between(x1, y1, z1, x2, y2, z2)));
			}
			return new ZoneTable(zones);
		}

		// Up to 8 characters ended by a 0; null if it isn't text
		static string Name(byte[] bytes, int at)
		{
			int length = 0;
			while (length < 8 && bytes[at + length] != 0)
			{
				byte c = bytes[at + length];
				if (c < 0x20 || c > 0x7E)
					return null;
				length++;
			}
			return Encoding.ASCII.GetString(bytes, at, length);
		}

		/// <summary>
		/// The smallest zone by area that contains the point, as the game's FindSmallestZoneForPosition picks; the first in
		/// the array of equal ones. Null when none does.
		/// </summary>
		public Zone Find(Position p)
		{
			Zone best = null;
			foreach (Zone zone in zones)
				if (zone.Box.Contains(p) && (best == null || zone.Box.Area < best.Box.Area))
					best = zone;
			return best;
		}
	}

	/// <summary>
	/// The label of the zone CJ is in (JEF2, CHC4A), with the one at the previous read; null when no zone contains him.
	/// The zone table is read when the watcher is first read and again whenever the game's zone count changes or the
	/// context does (a mission start or end, a timer reset, the game lost). Rules as for <see cref="PositionWatcher"/>:
	/// the first read and a read after a gap count as unchanged, and a failed read keeps Current and sets Old to it.
	/// </summary>
	public class ZoneWatcher : Watcher
	{
		readonly byte[] buffer = new byte[12];
		readonly byte[] countBytes = new byte[2];
		ZoneTable table;
		bool hasValue;

		public string Old { get; private set; }
		public string Current { get; private set; }

		/// <summary>The zone CJ is in, with its text key and box; null when no zone contains him or before the first read.</summary>
		public Zone CurrentZone { get; private set; }

		/// <summary>Whether CJ just came into the zone with this label.</summary>
		public bool Entered(string label) => Changed && string.Equals(Current, label, StringComparison.Ordinal);

		/// <summary>Whether CJ just left the zone with this label.</summary>
		public bool Left(string label) => Changed && string.Equals(Old, label, StringComparison.Ordinal);

		internal override void OnNewContext() => table = null;

		internal override void Update(IGameReader reader, bool afterGap)
		{
			Old = Current;
			if (!TryRead(reader, out Zone zone))
			{
				Changed = false;
				return;
			}
			CurrentZone = zone;
			Current = zone?.Label;
			if (!hasValue || afterGap)
				Old = Current;
			Changed = !string.Equals(Old, Current, StringComparison.Ordinal);
			hasValue = true;
		}

		bool TryRead(IGameReader reader, out Zone zone)
		{
			zone = null;
			if (!reader.TryReadBytes(GameAddresses.ZoneCount, countBytes))
				return false;
			int count = BitConverter.ToInt16(countBytes, 0);
			if (table == null || table.Zones.Count != count)
				table = ZoneTable.TryRead(reader, count);
			if (table == null)
				return false;
			if (!Position.TryReadPlayer(reader, buffer, out Position position))
				return false;
			zone = table.Find(position);
			return true;
		}
	}
}
