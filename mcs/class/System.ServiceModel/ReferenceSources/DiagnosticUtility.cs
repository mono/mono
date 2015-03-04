//
// Stubs for DiagnosticUtility methods
//
// Copyright 2015 Xamarin Inc
//
using System;
using System.Diagnostics;
using System.Runtime.Diagnostics;

namespace System.ServiceModel {
	
	static partial class DiagnosticUtility {
		public static bool ShouldTraceError { get { return FxTrace.ShouldTraceError; } }
		public static bool ShouldTraceWarning { get { return FxTrace.ShouldTraceWarning; } }
		public static bool ShouldTrace (TraceEventType severity) { return false; } 
		public static bool ShouldTraceInformation { get { return FxTrace.ShouldTraceInformation; } }
		public static bool ShouldTraceVerbose { get { return FxTrace.ShouldTraceVerbose; } }
		public static bool ShouldUseActivity { get { return FxTrace.ShouldUseActivity; } }
		public static bool TracingEnabled { get { return FxTrace.TracingEnabled; } }
		
		public static Exception ThrowHelperArgumentNullOrEmptyString (string arg)
		{
			return new ArgumentException ("Argument null or empty", arg);
		}

		[Conditional("DEBUG")]
		public static void DebugAssert(bool condition, string format, params object[] parameters)
		{
			if (condition)
				return;
			Console.WriteLine ("Assert failure: " + format, parameters);
			Environment.Exit (1);
		}
 
 		// TODO: this is a stub
		internal static partial class ExceptionUtility {

			public static Exception ThrowHelperArgumentNull (string arg)
			{
				return new ArgumentNullException ("Argument is null", arg);
			}

			public static Exception ThrowHelperError (Exception e)
			{
				return e;
			}

			public static Exception ThrowHelperCritical (Exception e)
			{
				return e;
			}
			
			internal static ArgumentException ThrowHelperArgument(string paramName, string message)
			{
				return new ArgumentException(message, paramName);
			}

			internal static ArgumentException ThrowHelperArgument(string paramName)
			{
				return new ArgumentException(message);
			}
		}

		internal static class DiagnosticTrace {
			public static void TraceEvent (params object [] args) {}
				
		}
		
	}
}
