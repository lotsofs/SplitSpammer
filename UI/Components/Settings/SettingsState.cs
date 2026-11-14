using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>The checked state of every setting. No WinForms; used from the UI thread only.</summary>
	public class SettingsState
	{
		readonly SettingsTree tree;
		readonly Dictionary<string, bool> values = new Dictionary<string, bool>();

		public SettingsTree Tree => tree;

		public SettingsState(SettingsTree tree)
		{
			this.tree = tree;
			Reset();
		}

		/// <summary>The setting's own box, whatever its ancestors are.</summary>
		public bool IsChecked(string id) => values[id];

		/// <summary>True if the setting and all its ancestors are checked.</summary>
		public bool IsEffective(string id)
		{
			for (SettingNode node = tree.Find(id); node != null; node = node.Parent)
				if (!values[node.Id])
					return false;
			return true;
		}

		/// <summary>Unchecking a group keeps its children's own states; they just stop being effective.</summary>
		public void SetChecked(string id, bool value) => values[id] = value;

		public void CheckBranch(string id) => SetBranch(id, true);

		public void UncheckBranch(string id) => SetBranch(id, false);

		void SetBranch(string id, bool value)
		{
			foreach (SettingNode node in tree.Find(id).SelfAndDescendants())
				values[node.Id] = value;
		}

		public void ResetBranch(string id)
		{
			foreach (SettingNode node in tree.Find(id).SelfAndDescendants())
				values[node.Id] = node.Default;
		}

		public void Reset()
		{
			foreach (SettingNode node in tree.All())
				values[node.Id] = node.Default;
		}

		/// <summary>The settings whose state differs from their default: all that is saved in the layout.</summary>
		public IEnumerable<KeyValuePair<string, bool>> ChangedFromDefault()
		{
			foreach (SettingNode node in tree.All())
				if (values[node.Id] != node.Default)
					yield return new KeyValuePair<string, bool>(node.Id, values[node.Id]);
		}

		public SettingsSnapshot Snapshot()
		{
			var enabled = new HashSet<string>();
			foreach (SettingNode node in tree.All())
				if (IsEffective(node.Id))
					enabled.Add(node.Id);
			return new SettingsSnapshot(enabled);
		}
	}
}
