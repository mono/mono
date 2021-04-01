using System.Diagnostics;

namespace System
{
    internal enum BCLDebugLogLevel {
        Trace  = 0,
        Status = 20,
        Warning= 40,
        Error  = 50,
        Panic  = 100,
    }

	static class BCLDebug
	{
		[Conditional("_DEBUG")]
		static public void Assert(bool condition, string message)
		{
		}

		[Conditional("_DEBUG")]
		internal static void Correctness(bool expr, string msg)
		{
		}

		[Conditional("_DEBUG")]
		static public void Log (string message)
		{
		}

		[Conditional("_DEBUG")]
		static public void Log (string switchName, string message)
		{
		}

		[Conditional("_DEBUG")]
		public static void Log (string switchName, BCLDebugLogLevel level, params object[] messages)
		{
		}

		[Conditional("_DEBUG")]
		internal static void Perf (bool expr, string msg)
		{
		}

		[Conditional("_LOGGING")]
		public static void Trace (string switchName, params object[]messages)
		{
		}

		internal static bool CheckEnabled (string switchName)
		{
			return false;
		}
	}
}
