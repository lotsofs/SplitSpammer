using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The component's settings panel and its XML in the layout. Built in code, with no designer files.
	/// For now it only says there is nothing to configure.
	/// </summary>
	public class SplitSpammerSettings : UserControl
	{
		// Saved in the layout so a later version can tell which settings an older layout has. A missing key means no version.
		const string Version = "1.0";

		public SplitSpammerSettings()
		{
			var label = new Label
			{
				Text = "Nothing to configure yet.",
				AutoSize = true,
				Location = new Point(7, 7),
			};
			Controls.Add(label);
			Size = new Size(476, 80);
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
