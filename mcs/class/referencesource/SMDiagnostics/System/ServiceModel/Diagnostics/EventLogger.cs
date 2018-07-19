//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    // 
    [Obsolete("This has been replaced by System.Runtime.Diagnostics.EventLogger")]
    class EventLogger
    {
        System.Runtime.Diagnostics.EventLogger innerEventLogger;

        EventLogger()
        {
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.EventLog instead")]
        internal EventLogger(string eventLogSourceName, object diagnosticTrace)
        {
            this.innerEventLogger = new System.Runtime.Diagnostics.EventLogger(eventLogSourceName, (System.Runtime.Diagnostics.DiagnosticTraceBase)diagnosticTrace);
        }

        [System.Runtime.Fx.Tag.SecurityNote(Critical = "Calling SecurityCritical method/property")]
        [SecurityCritical]
        internal static EventLogger UnsafeCreateEventLogger(string eventLogSourceName, object diagnosticTrace)
        {
            EventLogger logger = new EventLogger();
            logger.innerEventLogger = System.Runtime.Diagnostics.EventLogger.UnsafeCreateEventLogger(eventLogSourceName, (System.Runtime.Diagnostics.DiagnosticTraceBase)diagnosticTrace);
            return logger;
        }

        internal void LogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, bool shouldTrace, params string[] values)
        {
            this.innerEventLogger.LogEvent(type, (ushort)category, (uint)eventId, shouldTrace, values);
        }

        [System.Runtime.Fx.Tag.SecurityNote(Critical = "Calling SecurityCritical method/property")]
        [SecurityCritical]
        internal void UnsafeLogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, bool shouldTrace, params string[] values)
        {
            this.innerEventLogger.UnsafeLogEvent(type, (ushort)category, (uint)eventId,
                shouldTrace, values);
        }

        internal void LogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, params string[] values)
        {
            this.innerEventLogger.LogEvent(type, (ushort)category, (uint)eventId, values);
        }

        internal static string NormalizeEventLogParameter(string param)
        {
            return System.Runtime.Diagnostics.EventLogger.NormalizeEventLogParameter(param);
        }
    }
}
