//
// bare-bones based implementation based on the references
// from the Microsoft reference source code to get things to build
//
using System.Runtime;

namespace System.Runtime.Diagnostics {
	internal static class FxTrace {
		public static EtwDiagnosticTrace Trace {
			get {
				return Fx.Trace;
			}
		}

		public static bool ShouldTrace = true;
		public static bool ShouldTraceError = true;
		public static bool ShouldTraceVerbose = true;
		public static bool ShouldTraceWarning = false;
		public static bool ShouldTraceInformation = false;
		public static bool ShouldTraceWarningToTraceSource = false;
		public static bool ShouldTraceCritical = true;
		public static bool ShouldUseActivity = false;
		public static bool ShouldTraceInformationToTraceSource = false;
		
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
