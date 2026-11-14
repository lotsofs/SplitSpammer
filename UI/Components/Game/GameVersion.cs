using System;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>The PC 1.0 check, separate from the process so it can be tested against a fake module.</summary>
	public static class GameVersion
	{
		public const int ModuleSize = 18313216;
		const int VersionValue = 38079;
		const int VersionOffsetEu = 0x4245BC;
		const int VersionOffsetUs = 0x42457C;

		/// <summary>
		/// PC 1.0 if the module has its size, or the version int is at either offset. The int isn't there yet
		/// while the game is still checking the CD, hence the size.
		/// </summary>
		public static bool IsSupported(int moduleSize, TryReadInt read)
		{
			if (moduleSize == ModuleSize)
				return true;
			return Matches(read, VersionOffsetEu) || Matches(read, VersionOffsetUs);
		}

		public delegate bool TryReadInt(int moduleOffset, out int value);

		static bool Matches(TryReadInt read, int offset) => read(offset, out int v) && v == VersionValue;
	}
}
