using System.Reflection;
using System.Runtime.InteropServices;
using LiveSplit.UI.Components;
using LiveSplit.UI.Components.SplitSpammer;

[assembly: AssemblyTitle("LiveSplit.SplitSpammer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LiveSplit.SplitSpammer")]
[assembly: AssemblyCopyright("Copyright © 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Not visible to COM components.
[assembly: ComVisible(false)]

// The ID of the typelib if this project is exposed to COM
[assembly: Guid("7ffbdf9e-35fd-4fa5-9308-671d4a443bf5")]

// Always four parts, so a feed entry for this release isn't newer than the DLL (see OutputToFile's docs/releasing.md).
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

// How LiveSplit finds the component in the DLL
[assembly: ComponentFactory(typeof(SplitSpammerFactory))]
