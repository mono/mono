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

		[Conditional ("TRACE")]
 		internal static void Enter(TraceSource traceSource, object obj, string method, object paramObject) {
 		}

		[Conditional ("TRACE")]
		internal static void Exit(TraceSource traceSource, object obj, string method, object retObject) {
		}
	}

#if MOBILE

	class TraceSource
	{
		
	}

#endif
}