using System;
using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The settings as a tree, in the ASL's order. Mission modules add their nodes the way the ASL calls settings.Add:
	/// Add(id, label, default, tooltip, parentId). A node with no parent is a root.
	/// </summary>
	public class SettingsTree
	{
		readonly Dictionary<string, SettingNode> byId = new Dictionary<string, SettingNode>();

		public List<SettingNode> Roots { get; } = new List<SettingNode>();

		public SettingNode Add(string id, string label, bool defaultValue, string tooltip = null, string parentId = null)
		{
			if (byId.ContainsKey(id))
				throw new ArgumentException("Duplicate setting id: " + id);
			SettingNode parent = null;
			if (parentId != null && !byId.TryGetValue(parentId, out parent))
				throw new ArgumentException("Unknown parent " + parentId + " for setting " + id);
			var node = new SettingNode(id, label, defaultValue, tooltip, parent);
			byId[id] = node;
			(parent?.Children ?? Roots).Add(node);
			return node;
		}

		public SettingNode Find(string id) => byId.TryGetValue(id, out SettingNode node) ? node : null;

		/// <summary>Every node, parents before children, in the order they were added within a level.</summary>
		public IEnumerable<SettingNode> All()
		{
			foreach (SettingNode root in Roots)
				foreach (SettingNode node in root.SelfAndDescendants())
					yield return node;
		}

		/// <summary>The tree the component ships with: the ASL header nodes, then each mission module's settings.</summary>
		public static SettingsTree Build()
		{
			var tree = new SettingsTree();
			tree.Add("Splits", "Splits", true);
			tree.Add("Missions", "Story missions", true, "Missions with a visible-anywhere minimap marker until completion", "Splits");
			tree.Add("LS", "Los Santos", true, null, "Missions");
			tree.Add("LS_Intro", "Intro", true, null, "LS");
			foreach (IModule module in Modules.All())
				module.AddSettings(tree);
			return tree;
		}
	}
}
