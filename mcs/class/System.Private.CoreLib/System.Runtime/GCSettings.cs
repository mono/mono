namespace System.Runtime
{
	partial class GCSettings
	{
		public static bool IsServerGC => throw new NotImplementedException ();

		static GCLatencyMode GetGCLatencyMode() => throw new NotImplementedException ();

		static SetLatencyModeStatus SetGCLatencyMode(GCLatencyMode newLatencyMode) => throw new NotImplementedException ();

		static GCLargeObjectHeapCompactionMode GetLOHCompactionMode() => throw new NotImplementedException ();

		static void SetLOHCompactionMode (GCLargeObjectHeapCompactionMode newLOHCompactionMode) => throw new NotImplementedException ();
	}
}