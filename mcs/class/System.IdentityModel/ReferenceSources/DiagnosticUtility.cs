//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.IdentityModel
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Threading;
    using System.Security;
    using System.Xml;
    using System.Globalization;
    using System.Collections.Generic;
    using System.IdentityModel.Diagnostics;

    using DiagnosticTrace = System.IdentityModel.Diagnostics.TraceUtility;

    static partial class DiagnosticUtility
    {
	public static class Utility
	{
		public static byte[] AllocateByteArray(int size)
		{
			return System.Runtime.Fx.AllocateByteArray (size);
		}
	}

	// FIXME: implement?
	public static SomeTraceType DiagnosticTrace { get; set; }

	public class SomeTraceType
	{
		public bool ShouldLogPii { get; set; }
		public TraceSource TraceSource { get; set; }
	}

        public static bool ShouldTraceError { get; set; }
        public static bool ShouldTraceInformation { get; set; }
        public static bool ShouldTraceVerbose { get; set; }

	public static void DebugAssert(bool condition, string format, params object[] parameters)
	{
		// FIXME: implement
	}

        public static bool ShouldTrace(TraceEventLevel level)
        {
            return ShouldTraceToTraceSource(level);
        }

        public static bool ShouldTrace(TraceEventType type)
        {
		// FIXME: implement
		return false;
        }

        public static bool ShouldTraceToTraceSource(TraceEventLevel level)
        {
            return ShouldTrace(TraceLevelHelper.GetTraceEventType(level));
        }


    public static class ExceptionUtility
    {
        internal static Exception ThrowHelperWarning(Exception exception)
        {
	    return exception;
        }

	internal static Exception ThrowHelperError(Exception exception)
	{
	    return exception;
	}

/*
        internal static Exception ThrowHelperError(Exception exception, Guid activityId, object source)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticTrace.TraceEvent(TraceEventType.Error, TraceCode.ThrowingException, GenerateMsdnTraceCode(TraceCode.ThrowingException),
                    TraceSR.GetString(TraceSR.ThrowingException), null, exception, activityId, source);
            }
            return exception;
        }
*/



        internal static ArgumentException ThrowHelperArgument(string message)
        {
		throw new ArgumentException (message);
        }

        internal static ArgumentException ThrowHelperArgument(string paramName, string message)
        {
		throw new ArgumentException (message, paramName);
        }

        internal static ArgumentNullException ThrowHelperArgumentNull(string paramName)
        {
		throw new ArgumentNullException (paramName);
        }

    }

        public static void TraceHandledException(Exception exception, TraceEventType traceEventType)
        {
		throw new NotImplementedException ();
        }

    }
}

