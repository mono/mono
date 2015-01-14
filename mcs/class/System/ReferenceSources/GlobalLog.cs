using System.Diagnostics;

namespace System.Net {

	internal static class GlobalLog {

        [Conditional("DEBUG")]
        [Conditional("_FORCE_ASSERTS")]
        public static void Assert(bool condition, string messageFormat, params object[] data) {        	
        }

        [Conditional("TRAVE")]
        public static void Print(string msg) {
        }
	}

}