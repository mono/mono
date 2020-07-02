using System;
using System.IO;

namespace DebuggerTests
{
	[Flags]
	public enum TestFlags
	{
		None = 0,
		NotOnLinux = 1,
		NotOnMacCI = 2
	}

	public static class TestHelper
	{
		const string EnvironmentVariableName = "DEBUGGER_TEST_SUITE_FLAGS";

		// FIXME: is there a better way to detect this on .NET Core?
		internal static bool IsMacOS => File.Exists ("/usr/lib/libc.dylib");

		public static bool IsSupported (TestFlags flags)
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
							return false;
						break;
					case "linux":
						if (!IsMacOS && HasFlag (TestFlags.NotOnLinux))
							return false;
						break;
					case "mac-ci":
						if (IsMacOS & HasFlag (TestFlags.NotOnMacCI))
							return false;
						break;
					default:
						Console.Error.WriteLine ($"Invalid {EnvironmentVariableName} setting: {part}");
						break;
				}
			}

			return true;

			bool HasFlag (TestFlags flag) => (flags & flag) != 0;
		}
	}
}
