using LiveSplit.Options;
using System;
using System.Diagnostics;
using System.Threading;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>
	/// Runs the engine about 30 times a second on a thread of its own, and refreshes the game reader about once a second.
	/// Nothing here may throw into LiveSplit: every step is caught and logged.
	/// </summary>
	public class EnginePoller : IDisposable
	{
		const int TickMs = 33;
		const int RefreshMs = 1000;

		readonly IGameReader reader;
		readonly SplitEngine engine;
		readonly Func<int> resetCount;
		readonly Action split;
		readonly Thread thread;
		volatile bool stop;
		int seenResets;

		/// <param name="resetCount">Counts the timer resets, raised on any thread; the poller resets the engine when it changes</param>
		/// <param name="split">Sends one split to the timer. Called on the poll thread, so it has to post to the UI thread itself.</param>
		public EnginePoller(IGameReader reader, SplitEngine engine, Func<int> resetCount, Action split)
		{
			this.reader = reader;
			this.engine = engine;
			this.resetCount = resetCount;
			this.split = split;
			seenResets = resetCount();
			thread = new Thread(Run) { IsBackground = true, Name = "SplitSpammer poll" };
		}

		public void Start() => thread.Start();

		public bool IsRunning => thread.IsAlive;

		void Run()
		{
			var clock = Stopwatch.StartNew();
			long nextRefresh = 0;
			while (!stop)
			{
				long started = clock.ElapsedMilliseconds;
				try
				{
					if (started >= nextRefresh)
					{
						reader.Refresh();
						nextRefresh = started + RefreshMs;
					}
					int resets = resetCount();
					if (resets != seenResets)
					{
						seenResets = resets;
						engine.Reset();
					}
					if (engine.Tick())
						split();
				}
				catch (Exception ex)
				{
					Log.Error(ex);
				}
				long wait = TickMs - (clock.ElapsedMilliseconds - started);
				if (wait > 0)
					Thread.Sleep((int)wait);
			}
		}

		public void Dispose()
		{
			stop = true;
			try
			{
				if (thread.IsAlive && Thread.CurrentThread != thread)
					thread.Join(1000);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}
