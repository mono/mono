using System.Diagnostics;

namespace System
{
	static class BCLDebug
	{
		[Conditional("_DEBUG")]
		internal static void Correctness(bool expr, string msg)
		{
		}
	}
}