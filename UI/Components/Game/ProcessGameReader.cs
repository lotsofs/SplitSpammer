using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveSplit.UI.Components.SplitSpammer
{
	/// <summary>Reads GTA SA's memory with ReadProcessMemory. The game may be 32-bit while LiveSplit is 64-bit.</summary>
	public class ProcessGameReader : IGameReader, IDisposable
	{
		static readonly string[] ProcessNames = { "gta_sa", "gta-sa" };

		const uint ProcessQueryInformation = 0x0400;
		const uint ProcessVmRead = 0x0010;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, IntPtr size, out IntPtr read);

		Process process;
		IntPtr handle;
		long moduleBase;
		int moduleSize;

		// Written by the poll thread, read by the UI thread
		volatile GameStatus status = GameStatus.Waiting;
		public GameStatus Status => status;

		public long ModuleBase => handle == IntPtr.Zero ? 0 : moduleBase;

		public void Refresh()
		{
			try
			{
				if (process != null && process.HasExited)
					Detach();
				if (process == null && !Attach())
					return;
				// Retry the check until it passes: while the game checks the CD the version int isn't there yet
				if (status != GameStatus.Found)
					status = GameVersion.IsSupported(moduleSize, TryReadInt) ? GameStatus.Found : GameStatus.Unsupported;
			}
			catch (Exception)
			{
				Detach();
			}
		}

		bool Attach()
		{
			foreach (string name in ProcessNames)
			{
				foreach (Process candidate in Process.GetProcessesByName(name))
				{
					if (process != null)
					{
						candidate.Dispose();
						continue;
					}
					try
					{
						ProcessModule module = candidate.MainModule;
						IntPtr h = OpenProcess(ProcessQueryInformation | ProcessVmRead, false, candidate.Id);
						if (h == IntPtr.Zero)
						{
							candidate.Dispose();
							continue;
						}
						process = candidate;
						handle = h;
						moduleBase = module.BaseAddress.ToInt64();
						moduleSize = module.ModuleMemorySize;
					}
					catch (Exception)
					{
						// Access denied or the process is starting or exiting: try again on the next refresh
						candidate.Dispose();
					}
				}
			}
			return process != null;
		}

		void Detach()
		{
			if (handle != IntPtr.Zero)
				CloseHandle(handle);
			handle = IntPtr.Zero;
			process?.Dispose();
			process = null;
			status = GameStatus.Waiting;
		}

		public bool TryReadInt(int moduleOffset, out int value)
		{
			var buffer = new byte[4];
			if (!TryReadBytes(moduleOffset, buffer))
			{
				value = 0;
				return false;
			}
			value = BitConverter.ToInt32(buffer, 0);
			return true;
		}

		public bool TryReadBytes(int moduleOffset, byte[] buffer)
		{
			if (handle == IntPtr.Zero)
				return false;
			return TryReadAbsoluteBytes(moduleBase + moduleOffset, buffer);
		}

		public bool TryReadAbsoluteBytes(long address, byte[] buffer)
		{
			if (handle == IntPtr.Zero)
				return false;
			try
			{
				return ReadProcessMemory(handle, new IntPtr(address), buffer, new IntPtr(buffer.Length), out IntPtr read)
					&& read.ToInt64() == buffer.Length;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void Dispose() => Detach();
	}
}
