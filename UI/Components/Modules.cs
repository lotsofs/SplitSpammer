using System.Collections.Generic;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// A part of the auto splitter that brings its own settings and checks: the timer control, and one module per mission
	/// with its settings and its split check kept together, as the ASL's regions do. The settings go in the tree when it
	/// is built; the checks and their watchers go in the engine.
	/// </summary>
	public interface IModule
	{
		/// <summary>Adds the module's settings. Its parents must already exist.</summary>
		void AddSettings(SettingsTree tree);

		/// <summary>
		/// Adds the module's watchers and checks. Register every watcher the checks read, shared ones too: an address that
		/// is already watched gives the same watcher.
		/// </summary>
		void AddChecks(SplitEngine engine);
	}

	public static class Modules
	{
		/// <summary>Every module: the timer control first, then the missions in the ASL's story order. Adding a mission is one line here.</summary>
		public static IEnumerable<IModule> All()
		{
			yield return new TimerControl();
			yield return new InTheBeginning();
			yield return new BigSmoke();
			yield return new Ryder();
		}
	}
}
