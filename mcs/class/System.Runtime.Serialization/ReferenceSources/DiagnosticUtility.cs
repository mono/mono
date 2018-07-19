using System.Diagnostics;

namespace System.Runtime.Serialization {

	internal static partial class DiagnosticUtility {
		internal static bool ShouldTraceError = true;
		internal static readonly bool ShouldTraceWarning = false;
		internal static readonly bool ShouldTraceInformation = false;
		internal static bool ShouldTraceVerbose = true;

		internal static class DiagnosticTrace {
			internal static void TraceEvent (params object [] args)
			{
			}
			
		}
		
		internal static class ExceptionUtility {
			internal static Exception ThrowHelperError (Exception e)
			{
				return ThrowHelper (e, TraceEventType.Error);
			}

			internal static Exception ThrowHelperCallback (string msg, Exception e)
			{
				return new CallbackException (msg, e);
			}

			internal static Exception ThrowHelperCallback (Exception e)
			{
				return new CallbackException ("Callback exception", e);
			}

			internal static Exception ThrowHelper (Exception e, TraceEventType type)
			{
				return e;
			}

			internal static Exception ThrowHelperArgument (string arg)
			{
				return new ArgumentException (arg);
			}

			internal static Exception ThrowHelperArgument (string arg, string message)
			{
				return new ArgumentException (message, arg);
			}

			internal static Exception ThrowHelperArgumentNull (string arg)
			{
				return new ArgumentNullException (arg);
			}

			internal static Exception ThrowHelperFatal (string msg, Exception e)
			{
				return new FatalException (msg, e);
			}
		}
	}
}

