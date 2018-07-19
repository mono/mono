//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Interop;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Diagnostics.CodeAnalysis;

    sealed class EventLogger
    {
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - In PT log no more than 5 events.")]
        const int MaxEventLogsInPT = 5;

        [SecurityCritical]
        static int logCountForPT;
        static bool canLogEvent = true;

        DiagnosticTraceBase diagnosticTrace;        
        [Fx.Tag.SecurityNote(Critical = "Protect the string that defines the event source name.",
            Safe = "It demands UnmanagedCode=true so PT cannot call.")]
        [SecurityCritical]
        string eventLogSourceName;
        bool isInPartialTrust;

        EventLogger()
        {
            this.isInPartialTrust = IsInPartialTrust();
        }

        [Obsolete("For System.Runtime.dll use only. Call FxTrace.EventLog instead")]
        public EventLogger(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
        {
            try
            {
                this.diagnosticTrace = diagnosticTrace;
                //set diagnostics trace prior to calling SafeSetLogSourceName
                if (canLogEvent)
                {
                    SafeSetLogSourceName(eventLogSourceName);
                }
            }
            catch (SecurityException)
            {
                // running in PT, do not try to log events anymore
                canLogEvent = false;
                // not throwing exception on purpose
            }
        }       

        [Fx.Tag.SecurityNote(Critical = "Unsafe method to create event logger (sets the event source name).")]
        [SecurityCritical]
        public static EventLogger UnsafeCreateEventLogger(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
        {
            EventLogger logger = new EventLogger();
            logger.SetLogSourceName(eventLogSourceName, diagnosticTrace);
            return logger;
        }       

        [Fx.Tag.SecurityNote(Critical = "Logs event to the event log and asserts Unmanaged code.")]
        [SecurityCritical]
        public void UnsafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            if (logCountForPT < MaxEventLogsInPT)
            {
                try
                {
                    // Vista introduces a new limitation:  a much smaller max
                    // event log entry size that we need to track.  All strings cannot
                    // exceed 31839 characters in length when totalled together.
                    // Choose a max length of 25600 characters (25k) to allow for
                    // buffer since this max length may be reduced without warning.
                    const int MaxEventLogEntryLength = 25600;

                    int eventLogEntryLength = 0;

                    string[] logValues = new string[values.Length + 2];
                    for (int i = 0; i < values.Length; ++i)
                    {
                        string stringValue = values[i];
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            stringValue = NormalizeEventLogParameter(stringValue);
                        }
                        else
                        {
                            stringValue = String.Empty;
                        }

                        logValues[i] = stringValue;
                        eventLogEntryLength += stringValue.Length + 1;
                    }

                    string normalizedProcessName = NormalizeEventLogParameter(UnsafeGetProcessName());
                    logValues[logValues.Length - 2] = normalizedProcessName;
                    eventLogEntryLength += (normalizedProcessName.Length + 1);

                    string invariantProcessId = UnsafeGetProcessId().ToString(CultureInfo.InvariantCulture);
                    logValues[logValues.Length - 1] = invariantProcessId;
                    eventLogEntryLength += (invariantProcessId.Length + 1);

                    // If current event log entry length is greater than max length
                    // need to truncate to max length.  This probably means that we
                    // have a very long exception and stack trace in our parameter
                    // strings.  Truncate each string by MaxEventLogEntryLength
                    // divided by number of strings in the entry.
                    // Truncation algorithm is overly aggressive by design to 
                    // simplify the code change due to Product Cycle timing.
                    if (eventLogEntryLength > MaxEventLogEntryLength)
                    {
                        // logValues.Length is always > 0 (minimum value = 2)
                        // Subtract one to insure string ends in '\0'
                        int truncationLength = (MaxEventLogEntryLength / logValues.Length) - 1;

                        for (int i = 0; i < logValues.Length; i++)
                        {
                            if (logValues[i].Length > truncationLength)
                            {
                                logValues[i] = logValues[i].Substring(0, truncationLength);
                            }
                        }
                    }

                    SecurityIdentifier sid = WindowsIdentity.GetCurrent().User;
                    byte[] sidBA = new byte[sid.BinaryLength];
                    sid.GetBinaryForm(sidBA, 0);
                    IntPtr[] stringRoots = new IntPtr[logValues.Length];
                    GCHandle stringsRootHandle = new GCHandle(); 
                    GCHandle[] stringHandles = null;

                    try
                    {
                        stringsRootHandle = GCHandle.Alloc(stringRoots, GCHandleType.Pinned);
                        stringHandles = new GCHandle[logValues.Length];
                        for (int strIndex = 0; strIndex < logValues.Length; strIndex++)
                        {
                            stringHandles[strIndex] = GCHandle.Alloc(logValues[strIndex], GCHandleType.Pinned);
                            stringRoots[strIndex] = stringHandles[strIndex].AddrOfPinnedObject();
                        }
                        UnsafeWriteEventLog(type, eventLogCategory, eventId, logValues, sidBA, stringsRootHandle);
                    }
                    finally
                    {
                        if (stringsRootHandle.AddrOfPinnedObject() != IntPtr.Zero)
                        {
                            stringsRootHandle.Free();
                        }
                        if (stringHandles != null)
                        {
                            foreach (GCHandle gcHandle in stringHandles)
                            {
                                if (gcHandle != null)
                                {
                                    gcHandle.Free();
                                }
                            }
                        }
                    }
                    
                    if (shouldTrace && this.diagnosticTrace != null && this.diagnosticTrace.IsEnabled())
                    {
                        const int RequiredValueCount = 4;
                        Dictionary<string, string> eventValues = new Dictionary<string, string>(logValues.Length + RequiredValueCount);
                        eventValues["CategoryID.Name"] = "EventLogCategory";
                        eventValues["CategoryID.Value"] = eventLogCategory.ToString(CultureInfo.InvariantCulture);
                        eventValues["InstanceID.Name"] = "EventId";
                        eventValues["InstanceID.Value"] = eventId.ToString(CultureInfo.InvariantCulture);
                        for (int i = 0; i < values.Length; ++i)
                        {
                            eventValues.Add("Value" + i.ToString(CultureInfo.InvariantCulture), values[i] == null ? string.Empty : DiagnosticTraceBase.XmlEncode(values[i]));
                        }

                        this.diagnosticTrace.TraceEventLogEvent(type, new DictionaryTraceRecord((eventValues)));
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    // If not fatal, just eat the exception
                }
                // In PT, we only limit 5 event logging per session
                if (this.isInPartialTrust)
                {
                    logCountForPT++;
                }
            }
        }

        public void LogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            if (canLogEvent)
            {
                try
                {
                    SafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
                }
                catch (SecurityException ex)
                {
                    // running in PT, do not try to log events anymore
                    canLogEvent = false;

                    // not throwing exception on purpose
                    if (shouldTrace)
                    {
                        Fx.Exception.TraceHandledException(ex, TraceEventType.Information);
                    }
                }
            }
        }

        public void LogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, params string[] values)
        {
            this.LogEvent(type, eventLogCategory, eventId, true, values);
        }

        // Converts incompatible serverity enumeration TraceEventType into EventLogEntryType
        static EventLogEntryType EventLogEntryTypeFromEventType(TraceEventType type)
        {
            EventLogEntryType retval = EventLogEntryType.Information;
            switch (type)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    retval = EventLogEntryType.Error;
                    break;
                case TraceEventType.Warning:
                    retval = EventLogEntryType.Warning;
                    break;
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Logs event to the event log by calling unsafe method.",
           Safe = "Demands the same permission that is asserted by the unsafe method.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        void SafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            UnsafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
        }

        [Fx.Tag.SecurityNote(Critical = "Protect the string that defines the event source name.",
            Safe = "It demands UnmanagedCode=true so PT cannot call.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        void SafeSetLogSourceName(string eventLogSourceName)
        {
            this.eventLogSourceName = eventLogSourceName;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets event source name.")]
        [SecurityCritical]
        void SetLogSourceName(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
        {
            this.eventLogSourceName = eventLogSourceName;
            this.diagnosticTrace = diagnosticTrace;
        }

        [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for 'PermissionSetAttribute' on type 'Process' when calling method GetCurrentProcess", 
            Safe = "Does not leak any resource")]
        [SecuritySafeCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecuritySafeCritical method, Does not expose critical resources returned by methods with Link Demands")]
        bool IsInPartialTrust()
        {
            bool retval = false;
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    retval = string.IsNullOrEmpty(process.ProcessName);
                }
            }
            catch (SecurityException)
            {
                // we are just testing, ignore exception
                retval = true;
            }

            return retval;
        }

        [SecurityCritical]
        [Fx.Tag.SecurityNote(Critical = "Accesses security critical code RegisterEventSource and ReportEvent")]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        [ResourceConsumption(ResourceScope.Machine)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts)]
        void UnsafeWriteEventLog(TraceEventType type, ushort eventLogCategory, uint eventId, string[] logValues, byte[] sidBA, GCHandle stringsRootHandle)
        {
            using (SafeEventLogWriteHandle handle = SafeEventLogWriteHandle.RegisterEventSource(null, this.eventLogSourceName))
            {
                if (handle != null)
                {
                    HandleRef data = new HandleRef(handle, stringsRootHandle.AddrOfPinnedObject());
                    UnsafeNativeMethods.ReportEvent(
                         handle,
                         (ushort)EventLogEntryTypeFromEventType(type),
                         eventLogCategory,
                         eventId,
                         sidBA,
                         (ushort)logValues.Length,
                         0,
                         data,
                         null);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for 'PermissionSetAttribute' on type 'Process' when calling method GetCurrentProcess",
            Safe = "Does not leak any resource")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecurityCritical method, Does not expose critical resources returned by methods with Link Demands")]
        string UnsafeGetProcessName()
        {
            string retval = null;
            using (Process process = Process.GetCurrentProcess())
            {
                retval = process.ProcessName;
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for 'PermissionSetAttribute' on type 'Process' when calling method GetCurrentProcess",
            Safe = "Does not leak any resource")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecurityCritical method, Does not expose critical resources returned by methods with Link Demands")]
        int UnsafeGetProcessId()
        {
            int retval = -1;
            using (Process process = Process.GetCurrentProcess())
            {
                retval = process.Id;
            }
            return retval;
        }
        
        internal static string NormalizeEventLogParameter(string eventLogParameter)
        {
            if (eventLogParameter.IndexOf('%') < 0)
            {
                return eventLogParameter;
            }

            StringBuilder parameterBuilder = null;
            int len = eventLogParameter.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = eventLogParameter[i];

                // Not '%'
                if (c != '%')
                {
                    if (parameterBuilder != null) parameterBuilder.Append(c);
                    continue;
                }

                // Last char
                if ((i + 1) >= len)
                {
                    if (parameterBuilder != null) parameterBuilder.Append(c);
                    continue;
                }

                // Next char is not number
                if (eventLogParameter[i + 1] < '0' || eventLogParameter[i + 1] > '9')
                {
                    if (parameterBuilder != null) parameterBuilder.Append(c);
                    continue;
                }

                // initialize str builder
                if (parameterBuilder == null)
                {
                    parameterBuilder = new StringBuilder(len + 2);
                    for (int j = 0; j < i; ++j)
                    {
                        parameterBuilder.Append(eventLogParameter[j]);
                    }
                }
                parameterBuilder.Append(c);
                parameterBuilder.Append(' ');
            }

            return parameterBuilder != null ? parameterBuilder.ToString() : eventLogParameter;
        }
    }
}
