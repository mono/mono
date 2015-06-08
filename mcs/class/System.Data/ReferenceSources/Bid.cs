using System;

static class Bid
{
	static IntPtr NoData          = (IntPtr)(-1);

	internal static void Trace(string fmtPrintfW, params object[] args)
	{
	}

	internal static void TraceEx(uint flags, string fmtPrintfW, params object[] args)
	{
	}

	internal static void ScopeEnter(out IntPtr hScp, string fmt, params object[] args) {
		hScp = NoData;
	}

	internal static void ScopeLeave(ref IntPtr hScp) {
		hScp = NoData;
	}
}