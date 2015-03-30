//
// bare-bones based implementation based on the references
// from the Microsoft reference source code to get things to build
//
using System.Runtime;
using System.Runtime.Diagnostics;

namespace System.Runtime.Serialization {
	internal static class FxTrace {
		public static EtwDiagnosticTrace Trace {
			get {
				return Fx.Trace;
			}
		}

		public static bool ShouldTraceError = true;
		public static bool ShouldTraceVerbose = true;

		static ExceptionTrace exception;
		public static ExceptionTrace Exception {
			get {
				if (exception == null)
					return new ExceptionTrace ("System.Runtime.Serialization", Trace);
				return exception;
			}
		}

		public static bool IsEventEnabled (int index)
		{
			return false;
		}

		public static void UpdateEventDefinitions (EventDescriptor [] ed, ushort [] events) {}
	}
}

