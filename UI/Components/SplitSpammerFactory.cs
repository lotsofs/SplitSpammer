using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components.SplitSpammer
{
	public class SplitSpammerFactory : IComponentFactory
	{
		// The displayed name of the component in the Layout Editor.
		public string ComponentName => "Split Spammer";

		public string Description => "Automatically splits GTA San Andreas (PC version 1.0)";

		// The sub-menu this component appears under when adding it to the layout; LiveSplit's own auto splitters are under Control.
		public ComponentCategory Category => ComponentCategory.Control;

		public IComponent Create(LiveSplitState state) => new SplitSpammerComponent(state);

		// LiveSplit checks the feed at startup and offers any newer version listed there. The feed doesn't
		// exist yet (item REL in docs/TODO.md), so for now the check finds nothing.
		public string UpdateName => ComponentName;

		// Each file's path in the feed is relative to this: the release tag, then the asset name.
		public string UpdateURL => "https://github.com/lotsofs/SplitSpammer/releases/download/";

		public string XMLURL => "https://raw.githubusercontent.com/lotsofs/SplitSpammer/main/update.xml";

		// AssemblyVersion in AssemblyInfo.cs, always four parts.
		public Version Version => typeof(SplitSpammerFactory).Assembly.GetName().Version;
	}
}
