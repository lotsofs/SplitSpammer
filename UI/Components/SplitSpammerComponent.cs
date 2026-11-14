using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The LiveSplit side of the component. It draws nothing and, for now, does nothing: this is the skeleton
	/// the auto splitter is built on (see docs/TODO.md).
	/// </summary>
	public class SplitSpammerComponent : IComponent
	{
		public SplitSpammerSettings Settings { get; set; }
		protected LiveSplitState State { get; set; }

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
			Settings = new SplitSpammerSettings();
		}

		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }

		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }

		public Control GetSettingsControl(LayoutMode mode) => Settings;

		public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

		public void SetSettings(XmlNode settings) => Settings.SetSettings(settings);

		// Called every frame, on the UI thread. Nothing to do yet.
		public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) { }

		public int GetSettingsHashCode() => Settings.GetSettingsHashCode();

		public void Dispose()
		{
			Settings.Dispose();
		}
	}
}
