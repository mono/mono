using System.Diagnostics;

namespace System
{
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
	}
}