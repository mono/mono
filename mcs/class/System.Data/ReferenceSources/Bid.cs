static class Bid
{
	[BidMethod]
	internal static void Trace(string strConst) {
	}

	[BidMethod]
	internal static void TraceEx(uint flags, string strConst) {
	}

	[BidMethod]
	internal static void Trace(string fmtPrintfW, string a1) {
	}

	[BidMethod]
	internal static void TraceEx(uint flags, string fmtPrintfW, string a1) {
	}
}