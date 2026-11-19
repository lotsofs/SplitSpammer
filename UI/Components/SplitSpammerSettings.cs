using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// The component's settings panel and its XML in the layout. Built in code, with no designer files.
	/// Shows the game's status and the tree of splits, which can be collapsed and expanded.
	/// </summary>
	public class SplitSpammerSettings : UserControl
	{
		// Saved in the layout so a later version can tell which settings an older layout has. A missing key means no version.
		const string Version = "1.0";

		readonly Label statusLabel;
		readonly TreeView treeView;
		readonly ContextMenuStrip menu;
		readonly SettingsState state;

		// Set while the tree is filled or updated in code, so AfterCheck doesn't count it as the user ticking
		bool updatingTree;
		TreeNode menuNode;

		/// <summary>Raised on the UI thread after any change to the settings, including loading them.</summary>
		public event EventHandler SettingsChanged;

		public SettingsState State => state;

		public SplitSpammerSettings(SettingsTree tree)
		{
			state = new SettingsState(tree);

			// Docked, not anchored: LiveSplit's Layout Settings dialog makes the panel briefly much larger as it opens, and an
			// anchored tree grew with it and kept that size, so its scrollbars ended up outside the panel (measured with UI
			// Automation 2026-11-19: a 788 x 813 tree in a 476 x 500 panel). Docking sizes the tree from the panel every time.
			statusLabel = new Label
			{
				AutoSize = false,
				Height = 23,
				Dock = DockStyle.Top,
				TextAlign = ContentAlignment.TopLeft,
			};

			menu = new ContextMenuStrip();
			menu.Items.Add("Expand tree", null, (s, e) => treeView.ExpandAll());
			menu.Items.Add("Collapse tree", null, (s, e) => treeView.CollapseAll());
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add("Check branch", null, (s, e) => ChangeBranch(id => state.CheckBranch(id)));
			menu.Items.Add("Uncheck branch", null, (s, e) => ChangeBranch(id => state.UncheckBranch(id)));
			menu.Items.Add("Reset to default", null, (s, e) => ChangeBranch(id => state.ResetBranch(id)));

			treeView = new TreeView
			{
				CheckBoxes = true,
				ShowNodeToolTips = true,
				HideSelection = false,
				Dock = DockStyle.Fill,
			};
			treeView.AfterCheck += TreeView_AfterCheck;
			treeView.NodeMouseClick += TreeView_NodeMouseClick;
			// The fill first: docking lays out the last added first, so the label takes the top and the tree the rest
			Controls.Add(treeView);
			Controls.Add(statusLabel);

			Padding = new Padding(7);
			Size = new Size(476, 500);
			FillTree(tree);
			SetGameStatus(GameStatus.Waiting);
		}

		void FillTree(SettingsTree tree)
		{
			updatingTree = true;
			try
			{
				treeView.Nodes.Clear();
				foreach (SettingNode root in tree.Roots)
					AddTreeNode(treeView.Nodes, root);
				ShowStates(treeView.Nodes);
				// Everything open, scrolled to the top
				treeView.ExpandAll();
				if (treeView.Nodes.Count > 0)
					treeView.Nodes[0].EnsureVisible();
			}
			finally
			{
				updatingTree = false;
			}
		}

		void AddTreeNode(TreeNodeCollection into, SettingNode node)
		{
			TreeNode treeNode = into.Add(node.Id, node.Label);
			treeNode.Tag = node.Id;
			treeNode.ToolTipText = node.Tooltip ?? "";
			foreach (SettingNode child in node.Children)
				AddTreeNode(treeNode.Nodes, child);
		}

		// Copies the state into the boxes, and greys what is checked but not in effect because an ancestor is unchecked
		void ShowStates(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				string id = (string)node.Tag;
				node.Checked = state.IsChecked(id);
				node.ForeColor = state.IsEffective(id) ? SystemColors.WindowText : SystemColors.GrayText;
				ShowStates(node.Nodes);
			}
		}

		void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			// Only a tick by the user counts. WinForms also raises this while the tree is shown, with Action Unknown, and
			// then the boxes may not hold the state yet.
			if (updatingTree || e.Action == TreeViewAction.Unknown)
				return;
			state.SetChecked((string)e.Node.Tag, e.Node.Checked);
			UpdateAndNotify();
		}

		void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
				return;
			treeView.SelectedNode = e.Node;
			menuNode = e.Node;
			menu.Show(treeView, e.Location);
		}

		void ChangeBranch(Action<string> change)
		{
			if (menuNode == null)
				return;
			change((string)menuNode.Tag);
			UpdateAndNotify();
		}

		// Brings the boxes and the greying up to date with the state and tells the component
		void UpdateAndNotify()
		{
			updatingTree = true;
			try
			{
				ShowStates(treeView.Nodes);
			}
			finally
			{
				updatingTree = false;
			}
			SettingsChanged?.Invoke(this, EventArgs.Empty);
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

		// LiveSplit uses the hash only to decide whether to redraw. Only the settings that differ from their default are
		// written, as <S id="..." v="True|False" />: a missing id takes its default, so layouts saved before an id existed still load.
		int CreateSettingsNode(XmlDocument document, XmlElement parent)
		{
			int hash = SettingsHelper.CreateSetting(document, parent, "Version", Version);
			XmlElement tree = document?.CreateElement("Tree");
			foreach (var change in state.ChangedFromDefault())
			{
				hash = unchecked(hash * 31 + change.Key.GetHashCode() * 2 + (change.Value ? 1 : 0));
				if (tree == null)
					continue;
				XmlElement s = document.CreateElement("S");
				s.SetAttribute("id", change.Key);
				s.SetAttribute("v", change.Value.ToString());
				tree.AppendChild(s);
			}
			parent?.AppendChild(tree);
			return hash;
		}

		public void SetSettings(XmlNode settings)
		{
			state.Reset();
			foreach (XmlNode s in settings.SelectNodes("Tree/S"))
			{
				string id = s.Attributes?["id"]?.Value;
				// An id that no longer exists is ignored, and so is a value that isn't a boolean
				if (id != null && state.Tree.Find(id) != null && bool.TryParse(s.Attributes["v"]?.Value, out bool value))
					state.SetChecked(id, value);
			}
			UpdateAndNotify();
		}
	}
}
