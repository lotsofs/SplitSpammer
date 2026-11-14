using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The settings that are in effect, copied at one moment. Immutable, so the poll thread can read it while the UI thread
	/// builds and publishes the next one.
	/// </summary>
	public class SettingsSnapshot
	{
		readonly HashSet<string> enabled;

		internal SettingsSnapshot(HashSet<string> enabled)
		{
			this.enabled = enabled;
		}

		/// <summary>True if the setting and all its ancestors are checked. Unknown ids are not enabled.</summary>
		public bool IsEnabled(string id) => enabled.Contains(id);
	}
}
