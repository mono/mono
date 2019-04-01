namespace System.Runtime
{
	partial class GCSettings
	{
		public static bool IsServerGC => false;

		static GCLatencyMode GetGCLatencyMode() => GCLatencyMode.Batch;

		static SetLatencyModeStatus SetGCLatencyMode(GCLatencyMode newLatencyMode)
		{
			if (newLatencyMode != GCLatencyMode.Batch)
				throw new PlatformNotSupportedException ();

			return SetLatencyModeStatus.Succeeded;
		}

		static GCLargeObjectHeapCompactionMode GetLOHCompactionMode() => GCLargeObjectHeapCompactionMode.Default;

		static void SetLOHCompactionMode (GCLargeObjectHeapCompactionMode newLOHCompactionMode)
		{
			if (newLOHCompactionMode != GCLargeObjectHeapCompactionMode.Default)
				throw new PlatformNotSupportedException ();
		}
	}
}