using LiveSplit.Model;
using LiveSplit.Options.SettingsFactories;
using LiveSplit.UI.Components;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer.Preview
{
	/// <summary>
	/// Shows the component's settings panel in a window of its own, outside LiveSplit, so changes to it can be seen at once.
	/// The settings are kept in a file in the temp folder between runs; delete it to start fresh. This doesn't prove
	/// the component loads in LiveSplit: deploy -r does that.
	/// </summary>
	static class Program
	{
		static readonly string SettingsFile = Path.Combine(Path.GetTempPath(), "SplitSpammerPreview.xml");

		[STAThread]
		static void Main()
		{
			// LiveSplit.Core saves speedrun.com's game list in the current directory once a run has a game name; the run here has none
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// StandardLayoutSettingsFactory throws outside LiveSplit, so Settings is constructed directly (as OutputToFile's tests do)
			var run = new Run(new LiveSplit.Model.Comparisons.StandardComparisonGeneratorsFactory()) { CategoryName = "Preview" };
			var state = new LiveSplitState(run, null, new Layout(), new StandardLayoutSettingsFactory().Create(), new LiveSplit.Options.Settings());
			state.CurrentComparison = Run.PersonalBestComparisonName;

			IComponent component = new SplitSpammerFactory().Create(state);
			LoadSettings(component);

			Control settings = component.GetSettingsControl(LayoutMode.Vertical);
			var form = new Form
			{
				Text = "Split Spammer settings (preview)",
				ClientSize = settings.Size,
				StartPosition = FormStartPosition.CenterScreen,
			};
			settings.Dock = DockStyle.Fill;
			form.Controls.Add(settings);

			// LiveSplit calls Update every frame; once every half second is enough here
			var timer = new Timer { Interval = 500 };
			timer.Tick += (s, e) => component.Update(null, state, 0, 0, LayoutMode.Vertical);
			timer.Start();

			form.FormClosing += (s, e) =>
			{
				timer.Stop();
				SaveSettings(component);
			};
			Application.Run(form);
			component.Dispose();
		}

		static void LoadSettings(IComponent component)
		{
			try
			{
				if (!File.Exists(SettingsFile))
					return;
				var document = new XmlDocument();
				document.Load(SettingsFile);
				component.SetSettings(document.DocumentElement);
			}
			catch (Exception)
			{
				// A broken file just means starting with the defaults
			}
		}

		static void SaveSettings(IComponent component)
		{
			var document = new XmlDocument();
			document.AppendChild(component.GetSettings(document));
			document.Save(SettingsFile);
		}
	}
}
