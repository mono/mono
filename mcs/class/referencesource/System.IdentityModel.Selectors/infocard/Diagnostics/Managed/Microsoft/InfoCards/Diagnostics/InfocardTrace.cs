//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards.Diagnostics
{
    using System;
    using System.Xml;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.ComponentModel;    //win32exception
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using Microsoft.Win32.SafeHandles;
    using System.Security;
    using System.Security.Principal;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    //
    // For InfoCardBaseException
    //
    using System.IdentityModel.Selectors;

    // Summary
    // InfoCardTrace is the main driver class for the managed tracing infrastructure.
    // Essentially it is a wrapper over the Indigo DiagnosticsAndTracing classes. 
    // Externally a facade of simple TraceXXXX calls is provided which 
    // internally thunk across to the indigo classes to perform the work.
    //
    // The trace class also provides support for flowing of correlation ids allowing
    // tracing of requests across process and managed / unmanaged boundaries
    // See the Infocard Tracing documentation at http://team/sites/infocard for
    // detail on configuration and usage.
    //
    // Remarks
    // All functions are thread safe
    // 
    // Example usage looks like:
    // using IDT=Microsoft.InfoCards.Diagnostics.InfoCardTrace
    // IDT.TraceVerbose( InfoCardTraceCode.StoreInvalidKey, myKey );
    // IDT.TraceDebug( "Got an infocard {0} with name {1}", card, card.Name );
    // 
    //
    static class InfoCardTrace
    {
        static class TraceCode
        {
            public const int IdentityModelSelectors = 0xD0000;
            public const int GeneralInformation = TraceCode.IdentityModelSelectors | 0X0001;
            public const int StoreLoading = TraceCode.IdentityModelSelectors | 0X0002;
            public const int StoreBeginTransaction = TraceCode.IdentityModelSelectors | 0X0003;
            public const int StoreCommitTransaction = TraceCode.IdentityModelSelectors | 0X0004;
            public const int StoreRollbackTransaction = TraceCode.IdentityModelSelectors | 0X0005;
            public const int StoreClosing = TraceCode.IdentityModelSelectors | 0X0006;
            public const int StoreFailedToOpenStore = TraceCode.IdentityModelSelectors | 0X0007;
            public const int StoreSignatureNotValid = TraceCode.IdentityModelSelectors | 0X0008;
            public const int StoreDeleting = TraceCode.IdentityModelSelectors | 0X0009;
        }

        static Dictionary<int, string> traceCodes = new Dictionary<int, string>(9)
        {
            { TraceCode.GeneralInformation, "GeneralInformation" },
            { TraceCode.StoreLoading, "StoreLoading" },
            { TraceCode.StoreBeginTransaction, "StoreBeginTransaction" },
            { TraceCode.StoreCommitTransaction, "StoreCommitTransaction" },
            { TraceCode.StoreRollbackTransaction, "StoreRollbackTransaction" },
            { TraceCode.StoreClosing, "StoreClosing" },
            { TraceCode.StoreFailedToOpenStore, "StoreFailedToOpenStore" },
            { TraceCode.StoreSignatureNotValid, "StoreSignatureNotValid" },
            { TraceCode.StoreDeleting, "StoreDeleting" },
        };

        static string GetTraceString(int traceCode)
        {
            return traceCodes[traceCode];
        }

        static string GetMsdnTraceCode(int traceCode)
        {
            return LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.IdentityModel.Selectors", GetTraceString(traceCode));
        }

        [DllImport("advapi32",
                CharSet = CharSet.Unicode,
                EntryPoint = "ReportEventW",
                ExactSpelling = true,
                SetLastError = true)]
        private static extern bool ReportEvent([In] SafeHandle hEventLog,
                                               [In] short type,
                                               [In] ushort category,
                                               [In] uint eventID,
                                               [In] byte[] userSID,
                                               [In] short numStrings,
                                               [In] int dataLen,
                                               [In] HandleRef strings,
                                               [In] byte[] rawData);



        //
        // Summary:
        // Provides a wrapper over a handle retrieved by RegisterEventSource
        //
        internal class SafeEventLogHandle : SafeHandle
        {

            [DllImport("advapi32",
                    CharSet = CharSet.Unicode,
                    EntryPoint = "RegisterEventSourceW",
                    ExactSpelling = true,
                    SetLastError = true)]
            private static extern SafeEventLogHandle RegisterEventSource(string uncServerName, string sourceName);

            [DllImport("advapi32",
                    CharSet = CharSet.Unicode,
                    EntryPoint = "DeregisterEventSource",
                    ExactSpelling = true,
                    SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private static extern bool DeregisterEventSource(IntPtr eventLog);

            public static SafeEventLogHandle Construct()
            {
                SafeEventLogHandle h = RegisterEventSource(null, InfoCardTrace.InfoCardEventSource);

                if (null == h || h.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    TraceDebug("failed to registereventsource with error {0}", error);

                }
                return h;
            }
            //
            // Summary:
            // Manages the lifetime of a native handle retrieved by register event source.
            // Parameters:
            // handle - the handle to wrap.
            //
            private SafeEventLogHandle()
                : base(IntPtr.Zero, true)
            {


            }


            public override bool IsInvalid
            {
                get
                {
                    return (IntPtr.Zero == base.handle);
                }
            }

            //
            // Summary:
            // Releases the eventlog handle.
            //
            protected override bool ReleaseHandle()
            {
#pragma warning suppress 56523
                return DeregisterEventSource(base.handle);

            }
        }

        //
        // Summary:
        // Returns whether the current exception is fatal.
        // Notes:
        // Currently this delegates to the code in ExceptionUtility.cs
        //
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool IsFatal(Exception e)
        {
            return Fx.IsFatal(e);
        }

        public static TimerCallback ThunkCallback(TimerCallback callback)
        {
            return Fx.ThunkCallback(callback);
        }

        public static WaitCallback ThunkCallback(WaitCallback callback)
        {
            return Fx.ThunkCallback(callback);
        }

        public static void CloseInvalidOutSafeHandle(SafeHandle handle)
        {
            Utility.CloseInvalidOutSafeHandle(handle);
        }

        //
        // The event source we log against. May need to be updated should our name change before rtm
        //

        const string InfoCardEventSource = "CardSpace 4.0.0.0";


        //
        // Summary:
        // Writes an audit message to the application's event log
        //
        public static void Audit(EventCode code)
        {
            LogEvent(code, null, EventLogEntryType.Information);
        }

        public static void Audit(EventCode code, string message)
        {
            LogEvent(code, message, EventLogEntryType.Information);
        }
        public static void Assert(bool condition, string format, params object[] parameters)
        {

            if (condition)
            {
                return;
            }

            string message = format;
            if (null != parameters && 0 != parameters.Length)
            {
                message = String.Format(CultureInfo.InvariantCulture, format, parameters);
            }
            TraceDebug("An assertion fired: {0}", message);
#if DEBUG
            // 
            // Let DebugAssert handle this for us....
            // If not in debugger,  Assertion Failed: Abort=Quit, Retry=Debug, Ignore=Continue
            // If in debugger, will hit a DebugBreak()
            //
            DiagnosticUtility.DebugAssert( false, message );
#else
            //
            // Retail assert failfasts service
            //
            FailFast(message);
#endif

        }


        [Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string format, params object[] parameters)
        {
#if DEBUG
            if (condition)
            {
                return;
            }

            string message = format;
            if (null != parameters && 0 != parameters.Length)
            {
                message = String.Format( CultureInfo.InvariantCulture, format, parameters );
            }
            TraceDebug( "An assertion fired: {0}", message );
            if (Debugger.IsAttached)
            {
                Debugger.Launch();
                Debugger.Break();
            }
            DiagnosticUtility.DebugAssert( false, message );
            FailFast( message );
#endif
        }


        // 
        // Facade functions to allow simple call semantics.
        //
        public static void FailFast(string message)
        {
            DiagnosticUtility.FailFast(message);
        }
        [Conditional("DEBUG")]
        public static void TraceVerbose(int traceCode)
        {
            TraceInternal(TraceEventType.Verbose, traceCode, null);

        }
        [Conditional("DEBUG")]
        public static void TraceVerbose(int traceCode, params object[] parameters)
        {
            TraceInternal(TraceEventType.Verbose, traceCode, parameters);
        }
        [Conditional("DEBUG")]
        public static void TraceInfo(int traceCode)
        {
            TraceInternal(TraceEventType.Information, traceCode, null);
        }
        [Conditional("DEBUG")]
        public static void TraceInfo(int traceCode, params object[] parameters)
        {
            TraceInternal(TraceEventType.Information, traceCode, parameters);
        }
        [Conditional("DEBUG")]
        public static void TraceWarning(int traceCode)
        {
            TraceInternal(TraceEventType.Warning, traceCode, null);
        }
        [Conditional("DEBUG")]
        public static void TraceWarning(int traceCode, params object[] parameters)
        {
            TraceInternal(TraceEventType.Warning, traceCode, parameters);
        }
        [Conditional("DEBUG")]
        public static void TraceError(int traceCode)
        {
            TraceInternal(TraceEventType.Error, traceCode, null);
        }
        [Conditional("DEBUG")]
        public static void TraceError(int traceCode, params object[] parameters)
        {
            TraceInternal(TraceEventType.Error, traceCode, parameters);
        }
        [Conditional("DEBUG")]
        public static void TraceCritical(int traceCode)
        {
            TraceInternal(TraceEventType.Critical, traceCode, null);
        }
        [Conditional("DEBUG")]
        public static void TraceCritical(int traceCode, params object[] parameters)
        {
            TraceInternal(TraceEventType.Critical, traceCode, parameters);
        }

        //
        // Enable the setting of level explicitly.
        //
        [Conditional("DEBUG")]
        public static void Trace(TraceEventType level, int traceCode)
        {
            TraceInternal(level, traceCode, null);
        }
        [Conditional("DEBUG")]
        public static void Trace(TraceEventType level, int traceCode, params object[] parameters)
        {
            TraceInternal(level, traceCode, parameters);
        }

        //
        // Summary
        // DebugTrace is an additional level of tracing, intended for 
        // use by the devleopment team during the product development cycle.
        // The trace funcitons need no localization and can be fed arbitrary strings as 
        // the format specifier.
        //
        // Remarks
        // Will be turned off in RETAIL builds.
        // All tracing is done at the VERBOSE level.
        //
        // Parameters
        // format       - a format string using the standard .net string format specifier syntax
        // parameters   - optional parmaters to be embedded in the format string.
        //
        [Conditional("DEBUG")]
        public static void TraceDebug(string format, params object[] parameters)
        {
#if DEBUG
            if (DiagnosticUtility.ShouldTraceVerbose)
            {


                // Retrieve the string from resources and build the message.
                //
                string message = format;

                if (null != parameters && 0 != parameters.Length)
                {
                    message = String.Format( CultureInfo.InvariantCulture, format, parameters );
                }


                //
                // If we were passed a null message, at least flag it
                //
                if (String.IsNullOrEmpty(message))
                {
                    message = "NULL DEBUG TRACE MESSAGE!";
                }
                //
                // Build a trace message conforming to the ETL trace schema and 
                // call down through the diagnostic support classes to trace the call.
                //
                InfoCardTraceRecord tr = new InfoCardTraceRecord(
                                            GetTraceString(TraceCode.GeneralInformation),
                                            message );

                DiagnosticUtility.DiagnosticTrace.TraceEvent(
                                    TraceEventType.Verbose,
                                    TraceCode.GeneralInformation,
                                    SR.GetString(GetTraceString(TraceCode.GeneralInformation)),
                                    GetMsdnTraceCode(TraceCode.GeneralInformation),
                                    tr, null, message);
            }
#endif
        }

        [Conditional("DEBUG")]
        public static void TraceDebug(string message)
        {
#if DEBUG
            if (DiagnosticUtility.ShouldTraceVerbose)
            {



                //
                // If we were passed a null message, at least flag it
                //
                if (String.IsNullOrEmpty(message))
                {
                    message = "NULL DEBUG TRACE MESSAGE!";
                }
                //
                // Build a trace message conforming to the ETL trace schema and 
                // call down through the diagnostic support classes to trace the call.
                //
                InfoCardTraceRecord tr = new InfoCardTraceRecord(
                                            GetTraceString(TraceCode.GeneralInformation),
                                            message );

                DiagnosticUtility.DiagnosticTrace.TraceEvent(
                                    TraceEventType.Verbose,
                                    TraceCode.GeneralInformation,
                                    SR.GetString(GetTraceString(TraceCode.GeneralInformation)),
                                    GetMsdnTraceCode(TraceCode.GeneralInformation),
                                    tr, null, message);
            }
#endif
        }

        //
        // Summary:
        // Logs the event for the appropriate infocard error code. This code should 
        // match the entries in messages,mc
        // Parameters:
        // code         - the event code to log
        // Notes: 
        // This code may need to be extended to support an array of string parameters. We will do this if our event
        // log messages require it.
        // 
        private static void LogEvent(EventCode code, string message, EventLogEntryType type)
        {




            using (SafeEventLogHandle handle = SafeEventLogHandle.Construct())
            {
                string parameter = message;
                if (null != handle)
                {
                    if (String.IsNullOrEmpty(parameter))
                    {
                        parameter = SR.GetString(SR.GeneralExceptionMessage);
                    }


                    //
                    // Report event expects a LPCTSTR* lpStrings. Use GCHandle, instead 
                    // of writing code with unsafe because InfoCard client uses this 
                    // and our client cannot contain any unsafe code.
                    //

                    //
                    // This is the array of LPCTSTRs 
                    //
                    IntPtr[] stringRoots = new IntPtr[1];

                    //
                    // This is to pin the parameter string itself. Use an array here if you want more than 1 string
                    //
                    GCHandle stringParamHandle = new GCHandle();

                    //
                    // This is to pin the pointer to the array of LPCTSTRs
                    //
                    GCHandle stringsRootHandle = new GCHandle();

                    try
                    {
                        //
                        // Pin the IntPtrs (ie array of LPCTSTRs)
                        //
                        stringsRootHandle = GCHandle.Alloc(stringRoots, GCHandleType.Pinned);

                        //
                        // Pin the parameter string itself
                        //
                        stringParamHandle = GCHandle.Alloc(parameter, GCHandleType.Pinned);

                        //
                        // Give the intptr address of the pinned string
                        //
                        stringRoots[0] = stringParamHandle.AddrOfPinnedObject();

                        //
                        // From msdn: The interop marshaler passes only the handle [2nd arg to constructor in our case] 
                        // to unmanaged code, and guarantees that the wrapper (passed as the first parameter
                        // to the constructor of the HandleRef) remains alive for the duration of the [PInvoke] call.
                        //
                        HandleRef data = new HandleRef(handle, stringsRootHandle.AddrOfPinnedObject());


                        SecurityIdentifier sid = WindowsIdentity.GetCurrent().User;
                        byte[] sidBA = new byte[sid.BinaryLength];
                        sid.GetBinaryForm(sidBA, 0);

                        if (!ReportEvent(
                                 handle,
                                 (short)type,
                                 (ushort)InfoCardEventCategory.General,
                                 (uint)code,
                                 sidBA,
                                 1,
                                 0,
                                 data,
                                 null))
                        {
                            //
                            // Errors in the eventlog API should be ignored by applications
                            //
                            int error = Marshal.GetLastWin32Error();
                            TraceDebug("Failed to report the event with error {0}", error);
                        }
                    }
                    finally
                    {
                        if (stringsRootHandle.IsAllocated)
                        {
                            stringsRootHandle.Free();
                        }

                        if (stringParamHandle.IsAllocated)
                        {
                            stringParamHandle.Free();
                        }
                    }
                }
            }

        }

        public static void TraceAndLogException(Exception e)
        {
            bool shouldLog = false;
            bool isInformational = false;
            InfoCardBaseException ie = e as InfoCardBaseException;

            //
            // We only log if this is an infocard exception that hasnt been previous logged, 
            // and isnt the user cancelled exception.
            //
            if (null != ie && !(ie is UserCancelledException) && !ie.Logged)
            {
                shouldLog = true;
            }
            if (shouldLog)
            {
                //
                // If this is the parent of a previously logged exception then log as
                // informational. 
                // If one of the children is UserCancelled, don't log at all
                //
                Exception current = ie.InnerException;
                while (null != current)
                {
                    if (current is UserCancelledException)
                    {
                        shouldLog = false;
                        break;
                    }
                    else if (current is InfoCardBaseException)
                    {
                        if ((current as InfoCardBaseException).Logged)
                        {
                            isInformational = true;
                        }
                    }
                    current = current.InnerException;
                }
            }
            if (shouldLog)
            {
                EventLogEntryType logType = isInformational ? EventLogEntryType.Information : EventLogEntryType.Error;
                string message = ie.Message;
                if (!isInformational)
                {
                    message = BuildMessage(ie);
                }
                LogEvent((EventCode)ie.NativeHResult, message, logType);

            }
            TraceException(e);
        }

        private static string BuildMessage(InfoCardBaseException ie)
        {

            Exception ex = ie;
            String errString = ex.Message + "\n";

            if (null != ex.InnerException)
            {
                while (null != ex.InnerException)
                {
                    errString += String.Format(System.Globalization.CultureInfo.CurrentUICulture,
                                         SR.GetString(SR.InnerExceptionTraceFormat),
                                         ex.InnerException.Message);
                    ex = ex.InnerException;
                }
                errString += String.Format(System.Globalization.CultureInfo.CurrentUICulture,
                                         SR.GetString(SR.CallStackTraceFormat),
                                         ie.ToString());

            }
            else
            {
                if (!String.IsNullOrEmpty(Environment.StackTrace))
                {
                    errString += String.Format(System.Globalization.CultureInfo.CurrentUICulture,
                                            SR.GetString(SR.CallStackTraceFormat),
                                            Environment.StackTrace);
                }
            }

            return errString;

        }
        //
        // Summary:
        // Logs a general exception in the event log
        // Parameters:
        // e        - the exception to log.
        //
        [Conditional("DEBUG")]
        public static void TraceException(Exception e)
        {
            Exception current = e;
            int indent = 0;
            while (null != current)
            {
                TraceDebug("{0}Exception: message={1}\n stack trace={2}",
                                new string(' ', indent * 2),
                                e.Message,
                                e.StackTrace);
                current = current.InnerException;
                indent++;
            }

        }

        //
        // Summary
        //  Throw an exception and log an error in the event log
        //
        public static Exception ThrowHelperError(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
        }


        //
        // Summary
        // Throw an exception but don't log in the event log
        //
        public static Exception ThrowHelperErrorWithNoLogging(Exception e)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
        }


        //
        // Summary
        //  Throw an exception and log a warning in the event log
        //
        public static Exception ThrowHelperWarning(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(e);
        }

        //
        // Summary
        //  Throw an exception and log a critical event in the event log
        //
        public static Exception ThrowHelperCritical(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(e);
        }

        //
        // Summary:
        // Throws an infocard argument exception. Currently mapped to a communication exception,
        //
        public static void ThrowInvalidArgumentConditional(bool condition, string argument)
        {
            if (condition)
            {
                string message = string.Format(
                                    System.Globalization.CultureInfo.CurrentUICulture,
                                    SR.GetString(SR.ServiceInvalidArgument),
                                    argument);
                throw ThrowHelperError(new InfoCardArgumentException(message));
            }
        }



        //
        // Summary
        //  Throw an ArgumentNullException and log an error in the event log
        //
        public static Exception ThrowHelperArgumentNull(string err)
        {

            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(err);
        }

        //
        // Summary
        //  Throw an ArgumentException and log an error in the event log
        //
        public static Exception ThrowHelperArgument(string message)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(message);
        }

        //
        // Summary
        //  Throw an ArgumentNullException and log an error in the event log
        //
        public static Exception ThrowHelperArgumentNull(string err, string message)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(err, message);
        }

        //
        // Summary
        // The following series of calls enable finer grained control over tracing in the client
        // All calls simply delegate down to the indigo DiagnosticTrace implementation which
        // triggers it's behaviour based on the currently configured listeners.
        //
        // Remarks
        // Typical usage is
        // if( IDT.ShouldTraceVerbose() )
        // {
        //     string toTrace = this.SafeDumpState();
        //     IDT.TraceVerbose( InfocardTraceCode.InfoCardCreated, toTrace );
        // }
        //
        public static bool ShouldTrace(TraceEventType type)
        {
            return DiagnosticUtility.ShouldTrace(type);
        }
        public static bool ShouldTraceCritical
        {
            get { return DiagnosticUtility.ShouldTraceCritical; }
        }
        public static bool ShouldTraceError
        {
            get { return DiagnosticUtility.ShouldTraceError; }
        }
        public static bool ShouldTraceWarning
        {
            get { return DiagnosticUtility.ShouldTraceWarning; }
        }
        public static bool ShouldTraceInformation
        {
            get { return DiagnosticUtility.ShouldTraceInformation; }
        }
        public static bool ShouldTraceVerbose
        {
            get { return DiagnosticUtility.ShouldTraceVerbose; }
        }


        //
        // Summary
        // Expose the activity ids associated with the current flow of activity.
        // ActivityIDs allow the correlation of events across process and managed / unmanaged bounda
        // Normally they are managed implicitly. The .net runtime will ensure they flow across thread 
        // intra-process ( appdomain ) boundaries, and the indigo runtime will ensure they 
        // flow across indigo interactions ( cross process and cross machine ). 
        // We have a couple of responsibilities:
        // When transitioning from mananged to unmanaged code:
        //      grab the activity id
        //      pass it across to native code through the activityID rpc parameter.
        // When transitioning from unmanaged code
        //      call SetActivityId passing in the received id.
        //
        // Remarks
        // Trace calls automatically attach the activityID on all calls.
        //
        public static Guid GetActivityId()
        {
            return System.Runtime.Diagnostics.DiagnosticTraceBase.ActivityId;
        }
        public static void SetActivityId(Guid activityId)
        {
            //
            // This will trace by default at level verbose. 
            //
            System.Runtime.Diagnostics.DiagnosticTraceBase.ActivityId = activityId;
        }

        //
        // Summary
        // The main trace function. Responsible for extracting the appropriate string
        // from the application's resource file, formatting the string with the set of paramters
        // if appropriate, 
        // and passing the request down to the IndigoDiagnostics classes.
        // 
        // Parameters
        // level        - the level to trace at. verbose <= level <= critical
        // code         - the infocard trace code - a unique numeric / string identifier.
        // parameters   - an optional set of parameters used to supply additional diagnostic information
        //
        // Remarks
        // Trace calls automatically attach the activityID on all calls.
        //
        [Conditional("DEBUG")]
        private static void TraceInternal(
                            TraceEventType level,
                            int traceCode,
                            params object[] parameters)
        {
#if DEBUG
            if (DiagnosticUtility.ShouldTrace(level))
            {
                //
                // Retrieve the string from resources and build the message.
                //
#if INFOCARD_CLIENT
                string message = SR.GetString(GetTraceString(traceCode));
#else
                string message = SR.GetString(traceCode);
#endif
                Assert( !String.IsNullOrEmpty( message ), "resource string lookup failed!!!" );

                if (!String.IsNullOrEmpty( message ) && null != parameters)
                {
                    try
                    {
                        message = String.Format(
                                    System.Globalization.CultureInfo.CurrentUICulture,
                                    message,
                                    parameters );
                    }
                    catch (FormatException f)
                    {
                        Assert( false, "Invalid format: " + traceCode );
                        TraceException( f );
                        message = SR.GetString( SR.GeneralTraceMessage, traceCode );

                    }

                }

                //
                // Build a trace message conforming to the ETL trace schema and 
                // call down through the diagnostic support classes to trace the call.
                //
                DiagnosticUtility.DiagnosticTrace.TraceEvent( level,
                                            traceCode,
                                    SR.GetString(GetTraceString(traceCode)),
                                    GetMsdnTraceCode(TraceCode.GeneralInformation),
                                    new InfoCardTraceRecord( GetTraceString(traceCode), message ), null, message);

            }
#endif
        }


    }
}
