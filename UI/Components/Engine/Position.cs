using System;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>A point in the game's world, in metres, as the game stores it (x east, y north, z up).</summary>
	public readonly struct Position : IEquatable<Position>
	{
		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public Position(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// CJ's position: the player ped's matrix position, or its placement while it has no matrix. In a vehicle it follows
		/// the vehicle (within about half a metre while driving, seen 2026-11-19), so it serves for both. False while there
		/// is no player ped (in the main menu) or a read fails.
		/// </summary>
		internal static bool TryReadPlayer(IGameReader reader, byte[] buffer, out Position position)
		{
			position = default;
			if (!reader.TryReadInt(GameAddresses.PlayerPed, out int ped) || ped == 0)
				return false;
			var pointer = new byte[4];
			if (!reader.TryReadAbsoluteBytes((uint)ped + (long)GameAddresses.EntityMatrix, pointer))
				return false;
			uint matrix = BitConverter.ToUInt32(pointer, 0);
			long address = matrix != 0 ? matrix + (long)GameAddresses.MatrixPosition : (uint)ped + (long)GameAddresses.EntityPlacement;
			if (!reader.TryReadAbsoluteBytes(address, buffer))
				return false;
			position = new Position(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8));
			return true;
		}

		public bool Equals(Position other) => X == other.X && Y == other.Y && Z == other.Z;

		public override bool Equals(object obj) => obj is Position other && Equals(other);

		public override int GetHashCode() => (X.GetHashCode() * 31 + Y.GetHashCode()) * 31 + Z.GetHashCode();

		public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
	}

	/// <summary>
	/// An axis-aligned box in the world, edges included: what the game's locate and area tests check. A 2D box has no
	/// height limit.
	/// </summary>
	public readonly struct Box
	{
		public float MinX { get; }
		public float MinY { get; }
		public float MinZ { get; }
		public float MaxX { get; }
		public float MaxY { get; }
		public float MaxZ { get; }

		Box(float x1, float y1, float z1, float x2, float y2, float z2)
		{
			MinX = Math.Min(x1, x2);
			MinY = Math.Min(y1, y2);
			MinZ = Math.Min(z1, z2);
			MaxX = Math.Max(x1, x2);
			MaxY = Math.Max(y1, y2);
			MaxZ = Math.Max(z1, z2);
		}

		/// <summary>
		/// Within the given distance of a point along each axis, as the game's LOCATE_CHAR_*_3D commands take it
		/// (x y z, then the radius along x, y and z).
		/// </summary>
		public static Box Around(float x, float y, float z, float radiusX, float radiusY, float radiusZ) =>
			new Box(x - radiusX, y - radiusY, z - radiusZ, x + radiusX, y + radiusY, z + radiusZ);

		/// <summary>The same at any height, as LOCATE_CHAR_*_2D.</summary>
		public static Box Around(float x, float y, float radiusX, float radiusY) =>
			new Box(x - radiusX, y - radiusY, float.NegativeInfinity, x + radiusX, y + radiusY, float.PositiveInfinity);

		/// <summary>Between two opposite corners, in either order, as IS_CHAR_IN_AREA_3D takes them.</summary>
		public static Box Between(float x1, float y1, float z1, float x2, float y2, float z2) => new Box(x1, y1, z1, x2, y2, z2);

		/// <summary>The same at any height, as IS_CHAR_IN_AREA_2D.</summary>
		public static Box Between(float x1, float y1, float x2, float y2) =>
			new Box(x1, y1, float.NegativeInfinity, x2, y2, float.PositiveInfinity);

		public bool Contains(Position p) =>
			p.X >= MinX && p.X <= MaxX && p.Y >= MinY && p.Y <= MaxY && p.Z >= MinZ && p.Z <= MaxZ;

		/// <summary>The area on the map, for picking the smallest of overlapping zones.</summary>
		public double Area => ((double)MaxX - MinX) * ((double)MaxY - MinY);

		public override string ToString() => $"({MinX}, {MinY}, {MinZ})-({MaxX}, {MaxY}, {MaxZ})";
	}

	/// <summary>
	/// CJ's position, with the one at the previous read. Not an ASL watcher, so its rules are its own: the first read and
	/// a read after a gap count as unchanged (Old is the value read), and a failed read keeps Current and sets Old to it.
	/// </summary>
	public class PositionWatcher : Watcher
	{
		readonly byte[] buffer = new byte[12];

		/// <summary>False until the first good read.</summary>
		public bool HasValue { get; private set; }
		public Position Old { get; private set; }
		public Position Current { get; private set; }

		/// <summary>Whether CJ is in the box now. False before the first good read.</summary>
		public bool IsIn(Box box) => HasValue && box.Contains(Current);

		/// <summary>Whether CJ was outside the box at the previous read and is inside it now.</summary>
		public bool Entered(Box box) => HasValue && box.Contains(Current) && !box.Contains(Old);

		/// <summary>Whether CJ was inside the box at the previous read and is outside it now.</summary>
		public bool Left(Box box) => HasValue && !box.Contains(Current) && box.Contains(Old);

		internal override void Update(IGameReader reader, bool afterGap)
		{
			Old = Current;
			if (!Position.TryReadPlayer(reader, buffer, out Position value))
			{
				Changed = false;
				return;
			}
			Current = value;
			if (!HasValue || afterGap)
				Old = Current;
			Changed = !Old.Equals(Current);
			HasValue = true;
		}
	}
}
