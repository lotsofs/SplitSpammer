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
	}

	public static class GameAddresses
	{
		const int ScmGlobalBase = 0x649960;
		const int ScmMissionLocalBase = 0x648960;

		/// <summary>Module offset of SCM global variable $n.</summary>
		public static int ScmGlobal(int n) => ScmGlobalBase + n * 4;

		/// <summary>Module offset of mission local variable n@. All missions share this block.</summary>
		public static int ScmMissionLocal(int n) => ScmMissionLocalBase + n * 4;
	}
}
