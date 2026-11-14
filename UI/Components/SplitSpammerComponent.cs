using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The LiveSplit side of the component. It draws nothing; for now it only looks for the game: this is the skeleton
	/// the auto splitter is built on (see docs/TODO.md).
	/// </summary>
	public class SplitSpammerComponent : IComponent
	{
		public SplitSpammerSettings Settings { get; set; }
		protected LiveSplitState State { get; set; }

		// Refreshed from Update, which runs on the UI thread; the poll thread comes with item ENG
		readonly ProcessGameReader reader = new ProcessGameReader();
		// The settings in effect. The UI thread replaces it whole on every change; the poll thread (item ENG) only reads it.
		volatile SettingsSnapshot snapshot;
		public SettingsSnapshot Snapshot => snapshot;

		readonly Stopwatch refreshTimer = Stopwatch.StartNew();
		bool refreshedOnce;
		const int RefreshIntervalMs = 1000;

		public string ComponentName => "Split Spammer";

		// No visible part: zero size, no padding
		public float HorizontalWidth => 0f;
		public float MinimumWidth => 0f;
		public float VerticalHeight => 0f;
		public float MinimumHeight => 0f;

		public float PaddingTop => 0f;
		public float PaddingLeft => 0f;
		public float PaddingBottom => 0f;
		public float PaddingRight => 0f;

		public IDictionary<string, Action> ContextMenuControls => null;

		public SplitSpammerComponent(LiveSplitState state)
		{
			State = state;
			Settings = new SplitSpammerSettings(SettingsTree.Build());
			Settings.SettingsChanged += (sender, e) => snapshot = Settings.State.Snapshot();
			snapshot = Settings.State.Snapshot();
		}

		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }

		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }

		public Control GetSettingsControl(LayoutMode mode) => Settings;

		public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

		public void SetSettings(XmlNode settings) => Settings.SetSettings(settings);

		// Called every frame, on the UI thread. Looks for the game about once a second.
		public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
		{
			if (refreshedOnce && refreshTimer.ElapsedMilliseconds < RefreshIntervalMs)
				return;
			refreshedOnce = true;
			refreshTimer.Restart();
			reader.Refresh();
			Settings.SetGameStatus(reader.Status);
		}

		public int GetSettingsHashCode() => Settings.GetSettingsHashCode();

		public void Dispose()
		{
			reader.Dispose();
			Settings.Dispose();
		}
	}
}
