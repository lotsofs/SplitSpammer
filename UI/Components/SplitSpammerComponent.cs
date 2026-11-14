using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The LiveSplit side of the component. It draws nothing. A poll thread (EnginePoller) reads the game and runs the
	/// SplitEngine; this class only wires it to LiveSplit. See docs/architecture.md.
	/// </summary>
	public class SplitSpammerComponent : IComponent
	{
		public SplitSpammerSettings Settings { get; set; }
		protected LiveSplitState State { get; set; }

		readonly ProcessGameReader reader = new ProcessGameReader();
		readonly SplitEngine engine;
		readonly EnginePoller poller;
		readonly TimerModel timerModel;
		// The UI thread's context, captured where LiveSplit creates the component, so the poll thread can split on it
		readonly SynchronizationContext uiContext = SynchronizationContext.Current;
		// Counts timer resets. The handler runs on any thread (LiveSplit's server raises events on its own), so it only counts.
		int resets;
		GameStatus shownStatus = GameStatus.Waiting;

		// The settings in effect. The UI thread replaces it whole on every change; the poll thread only reads it.
		volatile SettingsSnapshot snapshot;
		public SettingsSnapshot Snapshot => snapshot;

		/// <summary>The engine, for the mission modules to add their watchers and checks to.</summary>
		public SplitEngine Engine => engine;

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

			timerModel = new TimerModel { CurrentState = state };
			engine = new SplitEngine(reader, () => snapshot);
			state.OnReset += State_OnReset;
			poller = new EnginePoller(reader, engine, () => Volatile.Read(ref resets), SplitOnUiThread);
			poller.Start();
		}

		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }

		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }

		public Control GetSettingsControl(LayoutMode mode) => Settings;

		public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

		public void SetSettings(XmlNode settings) => Settings.SetSettings(settings);

		void State_OnReset(object sender, TimerPhase e) => Interlocked.Increment(ref resets);

		// Runs on the poll thread. TimerModel and the timer's events belong to the UI thread.
		void SplitOnUiThread()
		{
			if (uiContext != null)
				uiContext.Post(_ => timerModel.Split(), null);
			else
				timerModel.Split();
		}

		// Called every frame, on the UI thread. Cheap: the work is on the poll thread. Shows the game's status.
		public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
		{
			GameStatus status = reader.Status;
			if (status == shownStatus)
				return;
			shownStatus = status;
			Settings.SetGameStatus(status);
		}

		public int GetSettingsHashCode() => Settings.GetSettingsHashCode();

		public void Dispose()
		{
			State.OnReset -= State_OnReset;
			poller.Dispose();
			reader.Dispose();
			Settings.Dispose();
		}
	}
}
