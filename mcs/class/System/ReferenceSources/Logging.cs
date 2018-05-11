using System.Diagnostics;

namespace System.Net {
	static class Logging
	{

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

		internal static TraceSource Sockets {
			get {
				return null;
			}
		}

		[Conditional ("TRACE")]
		internal static void Enter(TraceSource traceSource, object obj, string method, object paramObject) {
		}

		[Conditional ("TRACE")]
		internal static void Enter(TraceSource traceSource, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void Enter(TraceSource traceSource, string msg, string parameters) {
		}

		[Conditional ("TRACE")]
		internal static void Exception(TraceSource traceSource, object obj, string method, Exception e) {
		}

		[Conditional ("TRACE")]
		internal static void Exit(TraceSource traceSource, object obj, string method, object retObject) {
		}

		[Conditional ("TRACE")]
		internal static void Exit(TraceSource traceSource, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void Exit(TraceSource traceSource, string msg, string parameters) {
		}

		[Conditional ("TRACE")]
		internal static void PrintInfo(TraceSource traceSource, object obj, string method, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void PrintInfo(TraceSource traceSource, object obj, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void PrintInfo(TraceSource traceSource, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void PrintWarning(TraceSource traceSource, object obj, string method, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void PrintWarning(TraceSource traceSource, string msg) {
		}

		[Conditional ("TRACE")]
		internal static void PrintError(TraceSource traceSource, string msg) {
		}
	}

#if MOBILE

	class TraceSource
	{
		
	}

#endif
}
