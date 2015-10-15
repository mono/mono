//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    abstract class DiagnosticTraceBase
    {
        //Diagnostics trace
        protected const string DefaultTraceListenerName = "Default";
        protected const string TraceRecordVersion = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";

        protected static string AppDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
        const ushort TracingEventLogCategory = 4;

        object thisLock;
        bool tracingEnabled = true;
        bool calledShutdown;
        bool haveListeners;
        SourceLevels level;
        protected string TraceSourceName;
        TraceSource traceSource;
        [Fx.Tag.SecurityNote(Critical = "This determines the event source name.")]
        [SecurityCritical]
        string eventSourceName;

        public DiagnosticTraceBase(string traceSourceName)
        {
            this.thisLock = new object();
            this.TraceSourceName = traceSourceName;
            this.LastFailure = DateTime.MinValue;
        }

        protected DateTime LastFailure { get; set; }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecurityCritical method. Does not expose critical resources returned by methods with Link Demands")]
        [Fx.Tag.SecurityNote(Critical = "Critical because we are invoking TraceSource.Listeners which has a Link Demand for UnmanagedCode permission.",
            Miscellaneous = "Asserting Unmanaged Code causes traceSource.Listeners to be successfully initiated and cached. But the Listeners property has a LinkDemand for UnmanagedCode, so it can't be read by partially trusted assemblies in heterogeneous appdomains")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        static void UnsafeRemoveDefaultTraceListener(TraceSource traceSource)
        {
            traceSource.Listeners.Remove(DiagnosticTraceBase.DefaultTraceListenerName);
        }

        public TraceSource TraceSource
        {
            get
            {
                return this.traceSource;
            }

            set
            {
                SetTraceSource(value);
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "Does not expose critical resources returned by methods with Link Demands")]
        [Fx.Tag.SecurityNote(Critical = "Critical because we are invoking TraceSource.Listeners which has a Link Demand for UnmanagedCode permission.",
            Safe = "Safe because are only retrieving the count of listeners and removing the default trace listener - we aren't leaking any critical resources.")]
        [SecuritySafeCritical]
        protected void SetTraceSource(TraceSource traceSource)
        {
            if (traceSource != null)
            {
                UnsafeRemoveDefaultTraceListener(traceSource);
                this.traceSource = traceSource;
                this.haveListeners = this.traceSource.Listeners.Count > 0;
            }
        }

        public bool HaveListeners
        {
            get
            {
                return this.haveListeners;
            }
        }

        SourceLevels FixLevel(SourceLevels level)
        {
            //the bit fixing below is meant to keep the trace level legal even if somebody uses numbers in config
            if (((level & ~SourceLevels.Information) & SourceLevels.Verbose) != 0)
            {
                level |= SourceLevels.Verbose;
            }
            else if (((level & ~SourceLevels.Warning) & SourceLevels.Information) != 0)
            {
                level |= SourceLevels.Information;
            }
            else if (((level & ~SourceLevels.Error) & SourceLevels.Warning) != 0)
            {
                level |= SourceLevels.Warning;
            }
            if (((level & ~SourceLevels.Critical) & SourceLevels.Error) != 0)
            {
                level |= SourceLevels.Error;
            }
            if ((level & SourceLevels.Critical) != 0)
            {
                level |= SourceLevels.Critical;
            }

            // If only the ActivityTracing flag is set, then
            // we really have Off. Do not do ActivityTracing then.
            if (level == SourceLevels.ActivityTracing)
            {
                level = SourceLevels.Off;
            }

            return level;
        }

        protected virtual void OnSetLevel(SourceLevels level)
        {
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "Does not expose critical resources returned by methods with Link Demands")]
        [Fx.Tag.SecurityNote(Critical = "Critical because we are invoking TraceSource.Listeners and SourceSwitch.Level which have Link Demands for UnmanagedCode permission.")]
        [SecurityCritical]
        void SetLevel(SourceLevels level)
        {
            SourceLevels fixedLevel = FixLevel(level);
            this.level = fixedLevel;

            if (this.TraceSource != null)
            {
                // Need this for setup from places like TransactionBridge.
                this.haveListeners = this.TraceSource.Listeners.Count > 0;
                OnSetLevel(level);

#pragma warning disable 618
                this.tracingEnabled = this.HaveListeners && (level != SourceLevels.Off);
#pragma warning restore 618
                this.TraceSource.Switch.Level = level;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are invoking SetLevel.")]
        [SecurityCritical]
        void SetLevelThreadSafe(SourceLevels level)
        {
            lock (this.thisLock)
            {
                SetLevel(level);
            }
        }

        public SourceLevels Level
        {
            get
            {
                if (this.TraceSource != null && (this.TraceSource.Switch.Level != this.level))
                {
                    this.level = this.TraceSource.Switch.Level;
                }

                return this.level;
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because we are invoking SetLevelTheadSafe.")]
            [SecurityCritical]
            set
            {
                SetLevelThreadSafe(value);
            }
        }

        protected string EventSourceName
        {
            [Fx.Tag.SecurityNote(Critical = "Access critical eventSourceName field",
                Safe = "Doesn't leak info\\resources")]
            [SecuritySafeCritical]
            get
            {
                return this.eventSourceName;
            }

            [Fx.Tag.SecurityNote(Critical = "This determines the event source name.")]
            [SecurityCritical]
            set
            {
                this.eventSourceName = value;
            }
        }

        public bool TracingEnabled
        {
            get
            {
                return this.tracingEnabled && this.traceSource != null;
            }
        }

        protected static string ProcessName
        {
            [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for 'PermissionSetAttribute' on type 'Process' when calling method GetCurrentProcess",
            Safe = "Does not leak any resource and has been reviewed")]
            [SecuritySafeCritical]
            get
            {
                string retval = null;
                using (Process process = Process.GetCurrentProcess())
                {
                    retval = process.ProcessName;
                }
                return retval;
            }
        }

        protected static int ProcessId
        {
            [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for 'PermissionSetAttribute' on type 'Process' when calling method GetCurrentProcess",
            Safe = "Does not leak any resource and has been reviewed")]
            [SecuritySafeCritical]
            get
            {
                int retval = -1;
                using (Process process = Process.GetCurrentProcess())
                {
                    retval = process.Id;
                }
                return retval;
            }
        }

        public virtual bool ShouldTrace(TraceEventLevel level)
        {
            return ShouldTraceToTraceSource(level);
        }

        public bool ShouldTrace(TraceEventType type)
        {
            return this.TracingEnabled && this.HaveListeners &&
                (this.TraceSource != null) &&
                0 != ((int)type & (int)this.Level);
        }

        public bool ShouldTraceToTraceSource(TraceEventLevel level)
        {
            return ShouldTrace(TraceLevelHelper.GetTraceEventType(level));
        }

        //only used for exceptions, perf is not important
        public static string XmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            int len = text.Length;
            StringBuilder encodedText = new StringBuilder(len + 8); //perf optimization, expecting no more than 2 > characters

            for (int i = 0; i < len; ++i)
            {
                char ch = text[i];
                switch (ch)
                {
                    case '<':
                        encodedText.Append("&lt;");
                        break;
                    case '>':
                        encodedText.Append("&gt;");
                        break;
                    case '&':
                        encodedText.Append("&amp;");
                        break;
                    default:
                        encodedText.Append(ch);
                        break;
                }
            }
            return encodedText.ToString();
        }

        [Fx.Tag.SecurityNote(Critical = "Sets global event handlers for the AppDomain",
            Safe = "Doesn't leak resources\\Information")]
        [SecuritySafeCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecuritySafeCritical method, Does not expose critical resources returned by methods with Link Demands")]
        protected void AddDomainEventHandlersForCleanup()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            if (this.TraceSource != null)
            {
                this.haveListeners = this.TraceSource.Listeners.Count > 0;
            }

            this.tracingEnabled = this.haveListeners;
            if (this.TracingEnabled)
            {
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
                this.SetLevel(this.TraceSource.Switch.Level);
                currentDomain.DomainUnload += new EventHandler(ExitOrUnloadEventHandler);
                currentDomain.ProcessExit += new EventHandler(ExitOrUnloadEventHandler);
            }
        }

        void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            ShutdownTracing();
        }

        protected abstract void OnUnhandledException(Exception exception);

        protected void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            OnUnhandledException(e);
            ShutdownTracing();
        }

        protected static string CreateSourceString(object source)
        {
            var traceSourceStringProvider = source as ITraceSourceStringProvider;
            if (traceSourceStringProvider != null)
            {
                return traceSourceStringProvider.GetSourceString();
            }

            return CreateDefaultSourceString(source);
        }

        internal static string CreateDefaultSourceString(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return String.Format(CultureInfo.CurrentCulture, "{0}/{1}", source.GetType().ToString(), source.GetHashCode());
        }

        protected static void AddExceptionToTraceString(XmlWriter xml, Exception exception)
        {
            xml.WriteElementString(DiagnosticStrings.ExceptionTypeTag, XmlEncode(exception.GetType().AssemblyQualifiedName));
            xml.WriteElementString(DiagnosticStrings.MessageTag, XmlEncode(exception.Message));
            xml.WriteElementString(DiagnosticStrings.StackTraceTag, XmlEncode(StackTraceString(exception)));
            xml.WriteElementString(DiagnosticStrings.ExceptionStringTag, XmlEncode(exception.ToString()));
            Win32Exception win32Exception = exception as Win32Exception;
            if (win32Exception != null)
            {
                xml.WriteElementString(DiagnosticStrings.NativeErrorCodeTag, win32Exception.NativeErrorCode.ToString("X", CultureInfo.InvariantCulture));
            }

            if (exception.Data != null && exception.Data.Count > 0)
            {
                xml.WriteStartElement(DiagnosticStrings.DataItemsTag);
                foreach (object dataItem in exception.Data.Keys)
                {
                    xml.WriteStartElement(DiagnosticStrings.DataTag);
                    xml.WriteElementString(DiagnosticStrings.KeyTag, XmlEncode(dataItem.ToString()));
                    xml.WriteElementString(DiagnosticStrings.ValueTag, XmlEncode(exception.Data[dataItem].ToString()));
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            if (exception.InnerException != null)
            {
                xml.WriteStartElement(DiagnosticStrings.InnerExceptionTag);
                AddExceptionToTraceString(xml, exception.InnerException);
                xml.WriteEndElement();
            }
        }

        protected static string StackTraceString(Exception exception)
        {
            string retval = exception.StackTrace;
            if (string.IsNullOrEmpty(retval))
            {
                // This means that the exception hasn't been thrown yet. We need to manufacture the stack then.
                StackTrace stackTrace = new StackTrace(false);
                // Figure out how many frames should be throw away
                System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();

                int frameCount = 0;
                bool breakLoop = false;
                foreach (StackFrame frame in stackFrames)
                {
                    string methodName = frame.GetMethod().Name;
                    switch (methodName)
                    {
                        case "StackTraceString":
                        case "AddExceptionToTraceString":
                        case "BuildTrace":
                        case "TraceEvent":
                        case "TraceException":
                        case "GetAdditionalPayload":
                            ++frameCount;
                            break;
                        default:
                            if (methodName.StartsWith("ThrowHelper", StringComparison.Ordinal))
                            {
                                ++frameCount;
                            }
                            else
                            {
                                breakLoop = true;
                            }
                            break;
                    }
                    if (breakLoop)
                    {
                        break;
                    }
                }

                stackTrace = new StackTrace(frameCount, false);
                retval = stackTrace.ToString();
            }
            return retval;
        }

        //CSDMain:109153, Duplicate code from System.ServiceModel.Diagnostics
        [Fx.Tag.SecurityNote(Critical = "Calls unsafe methods, UnsafeCreateEventLogger and UnsafeLogEvent.",
            Safe = "Event identities cannot be spoofed as they are constants determined inside the method, Demands the same permission that is asserted by the unsafe method.")]
        [SecuritySafeCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts,
            Justification = "Should not demand permission that is asserted by the EtwProvider ctor.")]
        protected void LogTraceFailure(string traceString, Exception exception)
        {
            const int FailureBlackoutDuration = 10;
            TimeSpan FailureBlackout = TimeSpan.FromMinutes(FailureBlackoutDuration);
            try
            {
                lock (this.thisLock)
                {
                    if (DateTime.UtcNow.Subtract(this.LastFailure) >= FailureBlackout)
                    {
                        this.LastFailure = DateTime.UtcNow;
#pragma warning disable 618
                        EventLogger logger = EventLogger.UnsafeCreateEventLogger(this.eventSourceName, this);
#pragma warning restore 618
                        if (exception == null)
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, TracingEventLogCategory, (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToTraceEvent, false,
                                traceString);
                        }
                        else
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, TracingEventLogCategory, (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToTraceEventWithException, false,
                                traceString, exception.ToString());
                        }
                    }
                }
            }
            catch (Exception eventLoggerException)
            {
                if (Fx.IsFatal(eventLoggerException))
                {
                    throw;
                }
            }
        }

        protected abstract void OnShutdownTracing();

        void ShutdownTracing()
        {
            if (!this.calledShutdown)
            {
                this.calledShutdown = true;
                try
                {
                    OnShutdownTracing();
                }
#pragma warning suppress 56500 //Microsoft; Taken care of by FxCop
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    //log failure
                    LogTraceFailure(null, exception);
                }
            }
        }

        protected bool CalledShutdown
        {
            get
            {
                return this.calledShutdown;
            }
        }

        public static Guid ActivityId
        {
            [Fx.Tag.SecurityNote(Critical = "gets the CorrelationManager, which does a LinkDemand for UnmanagedCode",
                Safe = "only uses the CM to get the ActivityId, which is not protected data, doesn't leak the CM")]
            [SecuritySafeCritical]
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecuritySafeCriticial method")]
            get
            {
                object id = Trace.CorrelationManager.ActivityId;
                return id == null ? Guid.Empty : (Guid)id;
            }

            [Fx.Tag.SecurityNote(Critical = "gets the CorrelationManager, which does a LinkDemand for UnmanagedCode",
                Safe = "only uses the CM to get the ActivityId, which is not protected data, doesn't leak the CM")]
            [SecuritySafeCritical]
            set
            {
                Trace.CorrelationManager.ActivityId = value;
            }
        }

#pragma warning restore 56500

        protected static string LookupSeverity(TraceEventType type)
        {
            string s;
            switch (type)
            {
                case TraceEventType.Critical:
                    s = "Critical";
                    break;
                case TraceEventType.Error:
                    s = "Error";
                    break;
                case TraceEventType.Warning:
                    s = "Warning";
                    break;
                case TraceEventType.Information:
                    s = "Information";
                    break;
                case TraceEventType.Verbose:
                    s = "Verbose";
                    break;
                case TraceEventType.Start:
                    s = "Start";
                    break;
                case TraceEventType.Stop:
                    s = "Stop";
                    break;
                case TraceEventType.Suspend:
                    s = "Suspend";
                    break;
                case TraceEventType.Transfer:
                    s = "Transfer";
                    break;
                default:
                    s = type.ToString();
                    break;
            }

#pragma warning disable 618
            Fx.Assert(s == type.ToString(), "Return value should equal the name of the enum");
#pragma warning restore 618
            return s;
        }

        public abstract bool IsEnabled();
        public abstract void TraceEventLogEvent(TraceEventType type, TraceRecord traceRecord);
    }
}
