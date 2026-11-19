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

		/// <summary>The absolute address of the main module, to turn the game's own pointers into offsets; 0 when not attached.</summary>
		long ModuleBase { get; }

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

		/// <summary>The int that points at the first record in the game's idle list (CTheScripts::pIdleScripts), just before the active one.</summary>
		public const int IdleScripts = 0x68B428;
		/// <summary>The int that points at the first script in the game's active list (CTheScripts::pActiveScripts), just before the array.</summary>
		public const int ActiveScripts = 0x68B42C;
		/// <summary>Where a script's name is in its record: 8 characters after the two list links (next at 0, prev at 4).</summary>
		public const int ScriptName = 0x8;
		/// <summary>CTheScripts::ScriptsArray: every script record, running or idle, is one of these.</summary>
		public const int ScriptsArray = 0x68B430;
		public const int ScriptRecordSize = 0xE0;
		public const int ScriptCount = 96;
		/// <summary>The yellow mission name shown when a mission marker is entered.</summary>
		public const int MissionStartText = 0x7AAD40;
		/// <summary>The same for odd jobs (Trucking).</summary>
		public const int OddMissionStartText = 0x7AAEC0;

		/// <summary>
		/// The int that points at the vehicle CJ is in, 0 while he is in none. Seen on the running game 2026-11-19: it is
		/// cleared once he is out, about a second after he starts getting out. The ped's own vehicle pointer (+0x58C in the
		/// ped) keeps the last vehicle, so it can't tell.
		/// </summary>
		public const int PlayerVehicle = 0x7A18FC;
		/// <summary>Where an entity's model id is, a short (in a vehicle: 566 Tahoma, 420 Taxi, seen 2026-11-19).</summary>
		public const int ModelIndex = 0x22;

		/// <summary>The int that points at the player ped (CWorld::Players[0].m_pPed); 0 in the main menu.</summary>
		public const int PlayerPed = 0x77CD98;
		/// <summary>Where an entity's position is while it has no matrix: the first 12 bytes of its placement.</summary>
		public const int EntityPlacement = 0x4;
		/// <summary>Where an entity's matrix pointer is.</summary>
		public const int EntityMatrix = 0x14;
		/// <summary>Where the position (x, y, z floats) is in a matrix.</summary>
		public const int MatrixPosition = 0x30;

		/// <summary>The number of zones in the zone array, a short (379, seen 2026-11-19).</summary>
		public const int ZoneCount = 0x7A3794;
		/// <summary>The zone array (CTheZones::NavigationZoneArray), right after the count.</summary>
		public const int ZoneArray = 0x7A3798;
		public const int ZoneRecordSize = 0x20;

		/// <summary>Module offset of SCM global variable $n.</summary>
		public static int ScmGlobal(int n) => ScmGlobalBase + n * 4;

		/// <summary>Module offset of mission local variable n@. All missions share this block.</summary>
		public static int ScmMissionLocal(int n) => ScmMissionLocalBase + n * 4;
	}
}
