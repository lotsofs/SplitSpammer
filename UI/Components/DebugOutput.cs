using System;
using System.Runtime.InteropServices;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Trace lines for DebugView, each sent whole in one OutputDebugString call with the [SplitSpammer] prefix. LiveSplit's
	/// Log.Info goes through the default trace listener, which sends a "LiveSplit.exe Information: 0 : " header and the
	/// message as two calls; DebugView then joins or splits them unpredictably, and a filter on the prefix misses some
	/// lines. Errors still go through LiveSplit's Log.Error, which also reaches the event log.
	/// </summary>
	public static class DebugOutput
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		static extern void OutputDebugString(string text);

		public static void Write(string message)
		{
			try
			{
				OutputDebugString("[SplitSpammer] " + message + Environment.NewLine);
			}
			catch
			{
				// Tracing must never break a tick
			}
		}
	}
}
