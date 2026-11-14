using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>One setting in the tree. The id is also the split's id, as in the ASL.</summary>
	public class SettingNode
	{
		public string Id { get; }
		public string Label { get; }
		public bool Default { get; }
		public string Tooltip { get; }
		public SettingNode Parent { get; }
		public List<SettingNode> Children { get; } = new List<SettingNode>();

		internal SettingNode(string id, string label, bool defaultValue, string tooltip, SettingNode parent)
		{
			Id = id;
			Label = label;
			Default = defaultValue;
			Tooltip = tooltip;
			Parent = parent;
		}

		/// <summary>This node and everything under it, parents first.</summary>
		public IEnumerable<SettingNode> SelfAndDescendants()
		{
			yield return this;
			foreach (SettingNode child in Children)
				foreach (SettingNode node in child.SelfAndDescendants())
					yield return node;
		}
	}
}
