using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DebuggerTests
{
	[Flags]
	public enum TestFlags
	{
		None = 0,
		NotOnLinux = 1,
		NotOnLinuxDev = 2,
		NotOnMac = 4,
		NotOnMacDev = 8,
		NotWorking = 16
	}

	public static class TestHelper
	{
		const string EnvironmentVariableName = "DEBUGGER_TEST_SUITE_FLAGS";

		// FIXME: is there a better way to detect this on .NET Core?
		internal static bool IsMacOS => File.Exists ("/usr/lib/libc.dylib");

		public static bool IsSupported (
			TestFlags flags,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var variable = Environment.GetEnvironmentVariable (EnvironmentVariableName);
			if (string.IsNullOrEmpty (variable))
				return true;

			var parts = variable.Split (',', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				return true;

			foreach (var part in parts) {
				switch (part.ToLowerInvariant ()) {
					case "stable":
						if (flags != TestFlags.None)
							return LogDisabled ();
						break;
					case "linux":
						if (!IsMacOS && HasFlag (TestFlags.NotOnLinux, TestFlags.NotWorking))
							return LogDisabled ();
						break;
					case "linux-dev":
						if (!IsMacOS && HasFlag (TestFlags.NotOnLinux, TestFlags.NotOnLinuxDev, TestFlags.NotWorking))
							return LogDisabled ();
						break;
					case "mac":
						if (IsMacOS & HasFlag (TestFlags.NotOnMac, TestFlags.NotWorking))
							return LogDisabled ();
						break;
					case "mac-dev":
						if (IsMacOS & HasFlag (TestFlags.NotOnMac, TestFlags.NotOnMacDev, TestFlags.NotWorking))
							return LogDisabled ();
						break;
					default:
						Console.Error.WriteLine ($"Invalid {EnvironmentVariableName} setting: {part}");
						break;
				}
			}

			return true;

			bool HasFlag (params TestFlags[] check) => check.Any (f => (flags & f) != 0);

			bool LogDisabled ()
			{
				Console.WriteLine ($"Ignoring test {memberName} in {sourceFilePath}:{sourceLineNumber}");
				return false;
			}
		}
	}
}
