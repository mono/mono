using System.Runtime.CompilerServices;

namespace System
{
	partial class GC
	{
		internal static ulong GetSegmentSize ()
		{
			// coreclr default
			return 1024 * 1024 * 16;
		}

		public static GCMemoryInfo GetGCMemoryInfo ()
		{
			return default;
		}
	}
}
