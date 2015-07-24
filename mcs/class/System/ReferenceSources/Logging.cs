using System.Diagnostics;

namespace System.Net {
	class Logging {
		internal static bool On {
			get {
				return false;
			}
		}

		internal static TraceSource Web {
			get {
				return null;
			}
		}

		internal static TraceSource HttpListener {
			get {
				return null;
			}
		}

		[Conditional ("TRACE")]
 		internal static void Enter(TraceSource traceSource, object obj, string method, object paramObject) {
 		}

		[Conditional ("TRACE")]
		internal static void Exit(TraceSource traceSource, object obj, string method, object retObject) {
		}

		internal static void PrintWarning(TraceSource traceSource, object obj, string method, string msg) {
		}
	}

#if MOBILE

	class TraceSource
	{
		
	}

#endif
}