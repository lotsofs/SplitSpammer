using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The component's settings panel and its XML in the layout. Built in code, with no designer files.
	/// For now it only shows the game's status.
	/// </summary>
	public class SplitSpammerSettings : UserControl
	{
		// Saved in the layout so a later version can tell which settings an older layout has. A missing key means no version.
		const string Version = "1.0";

		readonly Label statusLabel;

		public SplitSpammerSettings()
		{
			statusLabel = new Label
			{
				AutoSize = true,
				Location = new Point(7, 7),
			};
			Controls.Add(statusLabel);
			Controls.Add(new Label
			{
				Text = "Nothing else to configure yet.",
				AutoSize = true,
				Location = new Point(7, 30),
			});
			Size = new Size(476, 80);
			SetGameStatus(GameStatus.Waiting);
		}

		/// <summary>Shows whether the game was found. Call it on the UI thread.</summary>
		public void SetGameStatus(GameStatus status)
		{
			switch (status)
			{
				case GameStatus.Found:
					statusLabel.Text = "Game: found (GTA SA PC 1.0)";
					break;
				case GameStatus.Unsupported:
					statusLabel.Text = "Game: unsupported version (use PC 1.0)";
					break;
				default:
					statusLabel.Text = "Game: waiting for GTA San Andreas";
					break;
			}
		}

		public XmlNode GetSettings(XmlDocument document)
		{
			XmlElement parent = document.CreateElement("Settings");
			CreateSettingsNode(document, parent);
			return parent;
		}

		public int GetSettingsHashCode() => CreateSettingsNode(null, null);

		// LiveSplit uses the hash only to decide whether to redraw.
		int CreateSettingsNode(XmlDocument document, XmlElement parent)
		{
			return SettingsHelper.CreateSetting(document, parent, "Version", Version);
		}

		public void SetSettings(XmlNode settings)
		{
			// Nothing is saved yet besides the version, so there is nothing to load.
		}
	}
}
