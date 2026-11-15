namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The frame every story mission shares in the ASL (its RegisterMissionBasics and ValidateMissionProgress): the mission's
	/// setting with the marker, start and pass splits, the thread and the marker text, and the progress check. A mission
	/// module keeps one in a static field and uses it from both AddSettings and AddChecks. A mission that registers its
	/// names differently uses only the parts that fit and writes the rest by hand.
	/// </summary>
	public sealed class StoryMission
	{
		/// <summary>The mission's setting id, also the prefix of its split ids.</summary>
		public string Id { get; }
		/// <summary>The setting's label: the mission's name.</summary>
		public string Label { get; }
		/// <summary>The mission's script, in lower case as the game has it in memory.</summary>
		public string Thread { get; }
		public string ParentId { get; }
		/// <summary>The yellow text shown when its marker is entered; usually the label, compared exactly, case included.</summary>
		public string MarkerText { get; }
		public string Tooltip { get; }

		public string MarkerEnteredId => Id + "_markerEntered";
		public string StartedId => Id + "_started";
		public string PassedId => Id + "_passed";

		/// <param name="markerText">Null when it is the label</param>
		public StoryMission(string id, string label, string thread, string parentId, string markerText = null, string tooltip = null)
		{
			Id = id;
			Label = label;
			Thread = thread;
			ParentId = parentId;
			MarkerText = markerText ?? label;
			Tooltip = tooltip;
		}

		/// <summary>
		/// First in AddSettings: the mission (on), then "Mission Marker Entered" and "Mission Started" (off) under it. The
		/// mission's event settings follow, then <see cref="AddPassedSetting"/>.
		/// </summary>
		public void AddBasicSettings(SettingsTree tree)
		{
			tree.Add(Id, Label, true, Tooltip, ParentId);
			tree.Add(MarkerEnteredId, "Mission Marker Entered", false, null, Id);
			tree.Add(StartedId, "Mission Started", false, null, Id);
		}

		/// <summary>Last in AddSettings, after the events: "Mission Passed" (on).</summary>
		public void AddPassedSetting(SettingsTree tree) => tree.Add(PassedId, "Mission Passed", true, null, Id);

		/// <summary>In AddChecks: the thread splits the start and becomes the last started mission; the marker text splits the marker.</summary>
		public void Register(SplitEngine engine)
		{
			engine.RegisterThread(Thread, StartedId);
			engine.RegisterMissionName(MarkerText, MarkerEnteredId);
		}

		/// <summary>
		/// The first line of the mission's check, as the ASL's ValidateMissionProgress: true while the mission runs and its
		/// counter has not reached <paramref name="after"/>; splits the pass when it does, from <paramref name="before"/>.
		/// See <see cref="SplitEngine.MissionInProgress"/>.
		/// </summary>
		/// <param name="chain">The name of the counter's watcher, which the module registers</param>
		public bool InProgress(SplitEngine engine, string chain, int before, int after) =>
			engine.MissionInProgress(Thread, chain, before, after, PassedId, Id);
	}
}
