namespace LiveSplit.UI.Components.SplitSpammer
{
	public enum GameStatus
	{
		/// <summary>No game process, or it can't be read yet.</summary>
		Waiting,
		/// <summary>GTA SA PC 1.0 is running and readable.</summary>
		Found,
		/// <summary>A GTA SA process is running, but it is not PC 1.0.</summary>
		Unsupported,
	}

	/// <summary>
	/// What the split logic reads the game through. All offsets are relative to the game's main module
	/// (PC 1.0 addresses). A read never throws: it returns false when the game is gone or the read fails.
	/// </summary>
	public interface IGameReader
	{
		GameStatus Status { get; }

		/// <summary>Attaches to the game if it isn't attached, checks the version, and drops a game that has exited.</summary>
		void Refresh();

		bool TryReadInt(int moduleOffset, out int value);

		/// <summary>Fills the buffer from a module offset; false unless every byte was read.</summary>
		bool TryReadBytes(int moduleOffset, byte[] buffer);

		/// <summary>Fills the buffer from an absolute address in the game, such as a pointer's target; false unless every byte was read.</summary>
		bool TryReadAbsoluteBytes(long address, byte[] buffer);
	}

	public static class GameAddresses
	{
		const int ScmGlobalBase = 0x649960;
		const int ScmMissionLocalBase = 0x648960;

		/// <summary>The int that points at the first script in the game's active list (CTheScripts::pActiveScripts).</summary>
		public const int ActiveScripts = 0x68B42C;
		/// <summary>Where a script's name is in its record: 8 characters after the two list links.</summary>
		public const int ScriptName = 0x8;
		/// <summary>The yellow mission name shown when a mission marker is entered.</summary>
		public const int MissionStartText = 0x7AAD40;
		/// <summary>The same for odd jobs (Trucking).</summary>
		public const int OddMissionStartText = 0x7AAEC0;

		/// <summary>Module offset of SCM global variable $n.</summary>
		public static int ScmGlobal(int n) => ScmGlobalBase + n * 4;

		/// <summary>Module offset of mission local variable n@. All missions share this block.</summary>
		public static int ScmMissionLocal(int n) => ScmMissionLocalBase + n * 4;
	}
}
