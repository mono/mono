//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    sealed class EtwDiagnosticTrace : DiagnosticTraceBase
    {
        //Diagnostics trace
        const int WindowsVistaMajorNumber = 6;
        const string EventSourceVersion = "4.0.0.0";
        const ushort TracingEventLogCategory = 4;
        const int MaxExceptionStringLength = 28 * 1024;
        const int MaxExceptionDepth = 64;
        const string DiagnosticTraceSource = "System.ServiceModel.Diagnostics";

        const int XmlBracketsLength = 5; // "<></>".Length;

        static readonly public Guid ImmutableDefaultEtwProviderId = new Guid("{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}");
        [Fx.Tag.SecurityNote(Critical = "provider Id to create EtwProvider, which is SecurityCritical")]
        [SecurityCritical]
        static Guid defaultEtwProviderId = ImmutableDefaultEtwProviderId;
        static Hashtable etwProviderCache = new Hashtable();
        static bool isVistaOrGreater = Environment.OSVersion.Version.Major >= WindowsVistaMajorNumber;
        static Func<string> traceAnnotation;

        [Fx.Tag.SecurityNote(Critical = "Stores object created by a critical c'tor")]
        [SecurityCritical]
        EtwProvider etwProvider;
        Guid etwProviderId;
        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        static EventDescriptor transferEventDescriptor = new EventDescriptor(499, 0, (byte)TraceChannel.Analytic, (byte)TraceEventLevel.LogAlways, (byte)TraceEventOpcode.Info, 0x0, 0x20000000001A0065);

        //Compiler will add all static initializers into the static constructor.  Adding an explicit one to mark SecurityCritical.
        [Fx.Tag.SecurityNote(Critical = "setting critical field defaultEtwProviderId")]
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
                        Justification = "SecurityCriticial method")]
        static EtwDiagnosticTrace()
        {
            // In Partial Trust, initialize to Guid.Empty to disable ETW Tracing.
            if (!PartialTrustHelpers.HasEtwPermissions())
            {
                defaultEtwProviderId = Guid.Empty;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider, eventSourceName field")]
        [SecurityCritical]
        public EtwDiagnosticTrace(string traceSourceName, Guid etwProviderId)
            : base(traceSourceName)
        {
            try
            {
                this.TraceSourceName = traceSourceName;
                this.EventSourceName = string.Concat(this.TraceSourceName, " ", EventSourceVersion);
                CreateTraceSource();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

#pragma warning disable 618
                EventLogger logger = new EventLogger(this.EventSourceName, null);
                logger.LogEvent(TraceEventType.Error, TracingEventLogCategory, (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToSetupTracing, false,
                    exception.ToString());
#pragma warning restore 618
            }

            try
            {
                CreateEtwProvider(etwProviderId);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                this.etwProvider = null;
#pragma warning disable 618
                EventLogger logger = new EventLogger(this.EventSourceName, null);
                logger.LogEvent(TraceEventType.Error, TracingEventLogCategory, (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToSetupTracing, false,
                    exception.ToString());
#pragma warning restore 618

            }

            if (this.TracingEnabled || this.EtwTracingEnabled)
            {
#pragma warning disable 618
                this.AddDomainEventHandlersForCleanup();
#pragma warning restore 618
            }
        }

        static public Guid DefaultEtwProviderId
        {
            [Fx.Tag.SecurityNote(Critical = "reading critical field defaultEtwProviderId", Safe = "Doesn't leak info\\resources")]
            [SecuritySafeCritical]
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecuritySafeCriticial method")]
            get
            {
                return EtwDiagnosticTrace.defaultEtwProviderId;
            }
            [Fx.Tag.SecurityNote(Critical = "setting critical field defaultEtwProviderId")]
            [SecurityCritical]
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
                Justification = "SecurityCriticial method")]
            set
            {
                EtwDiagnosticTrace.defaultEtwProviderId = value;
            }
        }

        public EtwProvider EtwProvider
        {
            [Fx.Tag.SecurityNote(Critical = "Exposes the critical etwProvider field")]
            [SecurityCritical]
            get
            {
                return this.etwProvider;
            }
        }

        public bool IsEtwProviderEnabled
        {
            [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
                Safe = "Doesn't leak info\\resources")]
            [SecuritySafeCritical]
            get
            {
                return (this.EtwTracingEnabled && this.etwProvider.IsEnabled());
            }
        }

        public Action RefreshState
        {
            [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
            Safe = "Doesn't leak resources or information")]
            [SecuritySafeCritical]
            get
            {
                return this.EtwProvider.ControllerCallBack;
            }

            [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
            Safe = "Doesn't leak resources or information")]
            [SecuritySafeCritical]
            set
            {
                this.EtwProvider.ControllerCallBack = value;
            }
        }

        public bool IsEnd2EndActivityTracingEnabled
        {
            [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
            Safe = "Doesn't leak resources or information")]
            [SecuritySafeCritical]
            get
            {
                return this.IsEtwProviderEnabled && this.EtwProvider.IsEnd2EndActivityTracingEnabled;
            }
        }

        bool EtwTracingEnabled
        {
            [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
                Safe = "Doesn't leak info\\resources")]
            [SecuritySafeCritical]
            get
            {
                return (this.etwProvider != null);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses the security critical etwProvider field", Safe = "Doesn't leak info\\resources")]
        [SecuritySafeCritical]
        public void SetEnd2EndActivityTracingEnabled(bool isEnd2EndTracingEnabled)
        {
            this.EtwProvider.SetEnd2EndActivityTracingEnabled(isEnd2EndTracingEnabled);
        }

        public void SetAnnotation(Func<string> annotation)
        {
            EtwDiagnosticTrace.traceAnnotation = annotation;
        }

        public override bool ShouldTrace(TraceEventLevel level)
        {
            return base.ShouldTrace(level) || ShouldTraceToEtw(level);
        }

        [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
            Safe = "Doesn't leak information\\resources")]
        [SecuritySafeCritical]
        public bool ShouldTraceToEtw(TraceEventLevel level)
        {
            return (this.EtwProvider != null && this.EtwProvider.IsEnabled((byte)level, 0));
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand",
            Safe = "Doesn't leak information\\resources")]
        [SecuritySafeCritical]
        public void Event(int eventId, TraceEventLevel traceEventLevel, TraceChannel channel, string description)
        {
            if (this.TracingEnabled)
            {
                EventDescriptor eventDescriptor = EtwDiagnosticTrace.GetEventDescriptor(eventId, channel, traceEventLevel);
                this.Event(ref eventDescriptor, description);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public void Event(ref EventDescriptor eventDescriptor, string description)
        {
            if (this.TracingEnabled)
            {
                TracePayload tracePayload = this.GetSerializedPayload(null, null, null);
                this.WriteTraceSource(ref eventDescriptor, description, tracePayload);
            }
        }

        public void SetAndTraceTransfer(Guid newId, bool emitTransfer)
        {
            if (emitTransfer)
            {
                TraceTransfer(newId);
            }
            EtwDiagnosticTrace.ActivityId = newId;
        }

        [Fx.Tag.SecurityNote(Critical = "Access critical transferEventDescriptor field, as well as other critical methods",
            Safe = "Doesn't leak information or resources")]
        [SecuritySafeCritical]
        public void TraceTransfer(Guid newId)
        {
            Guid oldId = EtwDiagnosticTrace.ActivityId;
            if (newId != oldId)
            {
                try
                {
                    if (this.HaveListeners)
                    {
                        this.TraceSource.TraceTransfer(0, null, newId);
                    }
                    //also emit to ETW
                    if (this.IsEtwEventEnabled(ref EtwDiagnosticTrace.transferEventDescriptor, false))
                    {
                        this.etwProvider.WriteTransferEvent(ref EtwDiagnosticTrace.transferEventDescriptor, new EventTraceActivity(oldId), newId,
                            EtwDiagnosticTrace.traceAnnotation == null ? string.Empty : EtwDiagnosticTrace.traceAnnotation(),
                            DiagnosticTraceBase.AppDomainFriendlyName);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    LogTraceFailure(null, e);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public void WriteTraceSource(ref EventDescriptor eventDescriptor, string description, TracePayload payload)
        {
            if (this.TracingEnabled)
            {
                XPathNavigator navigator = null;
                try
                {
                    string msdnTraceCode;
                    int legacyEventId;
                    EtwDiagnosticTrace.GenerateLegacyTraceCode(ref eventDescriptor, out msdnTraceCode, out legacyEventId);

                    string traceString = BuildTrace(ref eventDescriptor, description, payload, msdnTraceCode);
                    XmlDocument traceDocument = new XmlDocument();
                    traceDocument.LoadXml(traceString);
                    navigator = traceDocument.CreateNavigator();
                    this.TraceSource.TraceData(TraceLevelHelper.GetTraceEventType(eventDescriptor.Level, eventDescriptor.Opcode), legacyEventId, navigator);

                    if (this.CalledShutdown)
                    {
                        this.TraceSource.Flush();
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    LogTraceFailure(navigator == null ? string.Empty : navigator.ToString(), exception);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        static string BuildTrace(ref EventDescriptor eventDescriptor, string description, TracePayload payload, string msdnTraceCode)
        {
            StringBuilder sb = StringBuilderPool.Take();
            try
            {
                using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                    {
                        writer.WriteStartElement(DiagnosticStrings.TraceRecordTag);
                        writer.WriteAttributeString(DiagnosticStrings.NamespaceTag, EtwDiagnosticTrace.TraceRecordVersion);
                        writer.WriteAttributeString(DiagnosticStrings.SeverityTag,
                            TraceLevelHelper.LookupSeverity((TraceEventLevel)eventDescriptor.Level, (TraceEventOpcode)eventDescriptor.Opcode));
                        writer.WriteAttributeString(DiagnosticStrings.ChannelTag, EtwDiagnosticTrace.LookupChannel((TraceChannel)eventDescriptor.Channel));

                        writer.WriteElementString(DiagnosticStrings.TraceCodeTag, msdnTraceCode);
                        writer.WriteElementString(DiagnosticStrings.DescriptionTag, description);
                        writer.WriteElementString(DiagnosticStrings.AppDomain, payload.AppDomainFriendlyName);

                        if (!string.IsNullOrEmpty(payload.EventSource))
                        {
                            writer.WriteElementString(DiagnosticStrings.SourceTag, payload.EventSource);
                        }

                        if (!string.IsNullOrEmpty(payload.ExtendedData))
                        {
                            writer.WriteRaw(payload.ExtendedData);
                        }

                        if (!string.IsNullOrEmpty(payload.SerializedException))
                        {
                            writer.WriteRaw(payload.SerializedException);
                        }

                        writer.WriteEndElement();
                        writer.Flush();
                        stringWriter.Flush();

                        return sb.ToString();
                    }
                }
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        static void GenerateLegacyTraceCode(ref EventDescriptor eventDescriptor, out string msdnTraceCode, out int legacyEventId)
        {
            // To avoid breaking changes between 4.0 and 4.5 we have to use the same values for EventID and TraceCode like in 4.0
            // The mapping between legacy trace code and the new ETW event ids has to be done manually - for example
            // because there was only one event for HandledException in system.diagnostics. For ETW there are multiple events
            // because you have to specify the verbosity level per event in the manifest.

            switch (eventDescriptor.EventId)
            {
                case EventIdsWithMsdnTraceCode.AppDomainUnload:
                    msdnTraceCode = GenerateMsdnTraceCode(DiagnosticTraceSource, TraceCodes.AppDomainUnload);
                    legacyEventId = LegacyTraceEventIds.AppDomainUnload;
                    break;
                case EventIdsWithMsdnTraceCode.HandledExceptionError:
                case EventIdsWithMsdnTraceCode.HandledExceptionWarning:
                case EventIdsWithMsdnTraceCode.HandledExceptionInfo:
                case EventIdsWithMsdnTraceCode.HandledExceptionVerbose:
                    msdnTraceCode = GenerateMsdnTraceCode(DiagnosticTraceSource, TraceCodes.TraceHandledException);
                    legacyEventId = LegacyTraceEventIds.TraceHandledException;
                    break;    
                case EventIdsWithMsdnTraceCode.ThrowingExceptionVerbose:
                case EventIdsWithMsdnTraceCode.ThrowingExceptionWarning:
                    msdnTraceCode = GenerateMsdnTraceCode(DiagnosticTraceSource, TraceCodes.ThrowingException);
                    legacyEventId = LegacyTraceEventIds.ThrowingException;
                    break; 
                case EventIdsWithMsdnTraceCode.UnhandledException:
                    msdnTraceCode = GenerateMsdnTraceCode(DiagnosticTraceSource, TraceCodes.UnhandledException);
                    legacyEventId = LegacyTraceEventIds.UnhandledException;
                    break; 
                default:
                    msdnTraceCode = eventDescriptor.EventId.ToString(CultureInfo.InvariantCulture);
                    legacyEventId = eventDescriptor.EventId;
                    break;
            }
        }

        // helper for standardized trace code generation
        static string GenerateMsdnTraceCode(string traceSource, string traceCodeString)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "http://msdn.microsoft.com/{0}/library/{1}.{2}.aspx",
                CultureInfo.CurrentCulture.Name,
                traceSource, traceCodeString);
        }

        static string LookupChannel(TraceChannel traceChannel)
        {
            string channelName;
            switch (traceChannel)
            {
                case TraceChannel.Admin:
                    channelName = "Admin";
                    break;
                case TraceChannel.Analytic:
                    channelName = "Analytic";
                    break;
                case TraceChannel.Application:
                    channelName = "Application";
                    break;
                case TraceChannel.Debug:
                    channelName = "Debug";
                    break;
                case TraceChannel.Operational:
                    channelName = "Operational";
                    break;
                case TraceChannel.Perf:
                    channelName = "Perf";
                    break;
                default:
                    channelName = traceChannel.ToString();
                    break;
            }

            return channelName;
        }

        public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception)
        {
            return this.GetSerializedPayload(source, traceRecord, exception, false);
        }

        public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception, bool getServiceReference)
        {
            string eventSource = null;
            string extendedData = null;
            string serializedException = null;

            if (source != null)
            {
                eventSource = CreateSourceString(source);
            }

            if (traceRecord != null)
            {
                StringBuilder sb = StringBuilderPool.Take();
                try
                {
                    using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
                    {
                        using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                        {
                            writer.WriteStartElement(DiagnosticStrings.ExtendedDataTag);
                            traceRecord.WriteTo(writer);
                            writer.WriteEndElement();
                            writer.Flush();
                            stringWriter.Flush();

                            extendedData = sb.ToString();
                        }
                    }
                }
                finally
                {
                    StringBuilderPool.Return(sb);
                }
            }

            if (exception != null)
            {
                // We want to keep the ETW trace message to under 32k. So we keep the serialized exception to under 28k bytes.
                serializedException = ExceptionToTraceString(exception, MaxExceptionStringLength);
            }

            if (getServiceReference && (EtwDiagnosticTrace.traceAnnotation != null))
            {
                return new TracePayload(serializedException, eventSource, DiagnosticTraceBase.AppDomainFriendlyName, extendedData, EtwDiagnosticTrace.traceAnnotation());
            }

            return new TracePayload(serializedException, eventSource, DiagnosticTraceBase.AppDomainFriendlyName, extendedData, string.Empty);
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand",
            Safe = "Only queries the status of the provider - does not modify the state")]
        [SecuritySafeCritical]
        public bool IsEtwEventEnabled(ref EventDescriptor eventDescriptor)
        {
            return IsEtwEventEnabled(ref eventDescriptor, true);
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand",
            Safe = "Only queries the status of the provider - does not modify the state")]
        [SecuritySafeCritical]
        public bool IsEtwEventEnabled(ref EventDescriptor eventDescriptor, bool fullCheck)
        {
            // A full check queries ETW via a p/invoke call to see if the event is really enabled.
            // Checking against the level and keywords passed in the ETW callback can provide false positives,
            // but never a false negative.
            // The only code which specifies false is two generated classes, System.Runtime.TraceCore and 
            // System.Activities.EtwTrackingParticipantTrackRecords, and the method EtwDiagnosticTrace.TraceTransfer().
            // FxTrace uses IsEtwEventEnabled without the boolean, which then calls this method specifying true.
            if (fullCheck)
            {
                return (this.EtwTracingEnabled && this.etwProvider.IsEventEnabled(ref eventDescriptor));
            }

            return (this.EtwTracingEnabled && this.etwProvider.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords));
        }

        [Fx.Tag.SecurityNote(Critical = "Access the critical Listeners property",
            Safe = "Only Removes the default listener of the local source")]
        [SecuritySafeCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands,
            Justification = "SecuritySafeCriticial method")]
        void CreateTraceSource()
        {
            if (!string.IsNullOrEmpty(this.TraceSourceName))
            {
                SetTraceSource(new DiagnosticTraceSource(this.TraceSourceName));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Sets this.etwProvider and calls EtwProvider constructor, which are Security Critical")]
        [SecurityCritical]
        void CreateEtwProvider(Guid etwProviderId)
        {
            if (etwProviderId != Guid.Empty && EtwDiagnosticTrace.isVistaOrGreater)
            {
                //Pick EtwProvider from cache, add to cache if not found
                this.etwProvider = (EtwProvider)etwProviderCache[etwProviderId];
                if (this.etwProvider == null)
                {
                    lock (etwProviderCache)
                    {
                        this.etwProvider = (EtwProvider)etwProviderCache[etwProviderId];
                        if (this.etwProvider == null)
                        {
                            this.etwProvider = new EtwProvider(etwProviderId);
                            etwProviderCache.Add(etwProviderId, this.etwProvider);
                        }
                    }
                }

                this.etwProviderId = etwProviderId;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        static EventDescriptor GetEventDescriptor(int eventId, TraceChannel channel, TraceEventLevel traceEventLevel)
        {
            unchecked
            {
                //map channel to keywords
                long keyword = (long)0x0;
                if (channel == TraceChannel.Admin)
                {
                    keyword = keyword | (long)0x8000000000000000;
                }
                else if (channel == TraceChannel.Operational)
                {
                    keyword = keyword | 0x4000000000000000;
                }
                else if (channel == TraceChannel.Analytic)
                {
                    keyword = keyword | 0x2000000000000000;
                }
                else if (channel == TraceChannel.Debug)
                {
                    keyword = keyword | 0x100000000000000;
                }
                else if (channel == TraceChannel.Perf)
                {
                    keyword = keyword | 0x0800000000000000;
                }
                return new EventDescriptor(eventId, 0x0, (byte)channel, (byte)traceEventLevel, 0x0, 0x0, (long)keyword);
            }
        }

        protected override void OnShutdownTracing()
        {
            ShutdownTraceSource();
            ShutdownEtwProvider();
        }

        void ShutdownTraceSource()
        {
            try
            {
                if (TraceCore.AppDomainUnloadIsEnabled(this))
                {
                    TraceCore.AppDomainUnload(this, AppDomain.CurrentDomain.FriendlyName,
                        DiagnosticTraceBase.ProcessName, DiagnosticTraceBase.ProcessId.ToString(CultureInfo.CurrentCulture));
                }
                this.TraceSource.Flush();
            }
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

        [Fx.Tag.SecurityNote(Critical = "Access critical etwProvider field",
            Safe = "Doesn't leak info\\resources")]
        [SecuritySafeCritical]
        void ShutdownEtwProvider()
        {
            try
            {
                if (this.etwProvider != null)
                {
                    this.etwProvider.Dispose();
                    //no need to set this.etwProvider as null as Dispose() provides the necessary guard
                    //leaving it non-null protects trace calls from NullReferenceEx, CSDMain Bug 136228
                }
            }
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

        public override bool IsEnabled()
        {
            return TraceCore.TraceCodeEventLogCriticalIsEnabled(this)
                || TraceCore.TraceCodeEventLogVerboseIsEnabled(this)
                || TraceCore.TraceCodeEventLogInfoIsEnabled(this)
                || TraceCore.TraceCodeEventLogWarningIsEnabled(this)
                || TraceCore.TraceCodeEventLogErrorIsEnabled(this);
        }

        public override void TraceEventLogEvent(TraceEventType type, TraceRecord traceRecord)
        {
            switch (type)
            {
                case TraceEventType.Critical:
                    if (TraceCore.TraceCodeEventLogCriticalIsEnabled(this))
                    {
                        TraceCore.TraceCodeEventLogCritical(this, traceRecord);
                    }
                    break;

                case TraceEventType.Verbose:
                    if (TraceCore.TraceCodeEventLogVerboseIsEnabled(this))
                    {
                        TraceCore.TraceCodeEventLogVerbose(this, traceRecord);
                    }
                    break;

                case TraceEventType.Information:
                    if (TraceCore.TraceCodeEventLogInfoIsEnabled(this))
                    {
                        TraceCore.TraceCodeEventLogInfo(this, traceRecord);
                    }
                    break;

                case TraceEventType.Warning:
                    if (TraceCore.TraceCodeEventLogWarningIsEnabled(this))
                    {
                        TraceCore.TraceCodeEventLogWarning(this, traceRecord);
                    }
                    break;

                case TraceEventType.Error:
                    if (TraceCore.TraceCodeEventLogErrorIsEnabled(this))
                    {
                        TraceCore.TraceCodeEventLogError(this, traceRecord);
                    }
                    break;
            }
        }

        protected override void OnUnhandledException(Exception exception)
        {
            if (TraceCore.UnhandledExceptionIsEnabled(this))
            {
                TraceCore.UnhandledException(this, exception != null ? exception.ToString() : string.Empty, exception);
            }
        }

        internal static string ExceptionToTraceString(Exception exception, int maxTraceStringLength)
        {
            StringBuilder sb = StringBuilderPool.Take();
            try
            {
                using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    using (XmlTextWriter xml = new XmlTextWriter(stringWriter))
                    {
                        WriteExceptionToTraceString(xml, exception, maxTraceStringLength, MaxExceptionDepth);
                        xml.Flush();
                        stringWriter.Flush();

                        return sb.ToString();
                    }
                }
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        static void WriteExceptionToTraceString(XmlTextWriter xml, Exception exception, int remainingLength, int remainingAllowedRecursionDepth)
        {
            if (remainingAllowedRecursionDepth < 1)
            {
                return;
            }

            if (!WriteStartElement(xml, DiagnosticStrings.ExceptionTag, ref remainingLength))
            {
                return;
            }

            try
            {
                IList<Tuple<string, string>> exceptionInfo = new List<Tuple<string, string>>() 
                {
                    new Tuple<string, string> (DiagnosticStrings.ExceptionTypeTag, XmlEncode(exception.GetType().AssemblyQualifiedName)),
                    new Tuple<string, string> (DiagnosticStrings.MessageTag, XmlEncode(exception.Message)),
                    new Tuple<string, string> (DiagnosticStrings.StackTraceTag, XmlEncode(StackTraceString(exception))),
                    new Tuple<string, string> (DiagnosticStrings.ExceptionStringTag, XmlEncode(exception.ToString())),
                };

                System.ComponentModel.Win32Exception win32Exception = exception as System.ComponentModel.Win32Exception;
                if (win32Exception != null)
                {
                    exceptionInfo.Add(
                        new Tuple<string, string>(
                            DiagnosticStrings.NativeErrorCodeTag,
                            win32Exception.NativeErrorCode.ToString("X", CultureInfo.InvariantCulture)));
                }

                foreach (Tuple<string, string> item in exceptionInfo)
                {
                    if (!WriteXmlElementString(xml, item.Item1, item.Item2, ref remainingLength))
                    {
                        return;
                    }
                }

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    string exceptionData = GetExceptionData(exception);
                    if (exceptionData.Length < remainingLength)
                    {
                        xml.WriteRaw(exceptionData);
                        remainingLength -= exceptionData.Length;
                    }
                }

                if (exception.InnerException != null)
                {
                    string innerException = GetInnerException(exception, remainingLength, remainingAllowedRecursionDepth - 1);
                    if (!string.IsNullOrEmpty(innerException) && innerException.Length < remainingLength)
                    {
                        xml.WriteRaw(innerException);
                    }
                }
            }
            finally
            {
                xml.WriteEndElement();
            }
        }

        static string GetInnerException(Exception exception, int remainingLength, int remainingAllowedRecursionDepth)
        {
            if (remainingAllowedRecursionDepth < 1)
            {
                return null;
            }

            StringBuilder sb = StringBuilderPool.Take();
            try
            {
                using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    using (XmlTextWriter xml = new XmlTextWriter(stringWriter))
                    {
                        if (!WriteStartElement(xml, DiagnosticStrings.InnerExceptionTag, ref remainingLength))
                        {
                            return null;
                        }

                        WriteExceptionToTraceString(xml, exception.InnerException, remainingLength, remainingAllowedRecursionDepth);
                        xml.WriteEndElement();
                        xml.Flush();
                        stringWriter.Flush();

                        return sb.ToString();
                    }
                }
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        static string GetExceptionData(Exception exception)
        {
            StringBuilder sb = StringBuilderPool.Take();
            try
            {
                using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    using (XmlTextWriter xml = new XmlTextWriter(stringWriter))
                    {

                        xml.WriteStartElement(DiagnosticStrings.DataItemsTag);
                        foreach (object dataItem in exception.Data.Keys)
                        {
                            xml.WriteStartElement(DiagnosticStrings.DataTag);
                            xml.WriteElementString(DiagnosticStrings.KeyTag, XmlEncode(dataItem.ToString()));
                            if (exception.Data[dataItem] == null)
                            {
                                xml.WriteElementString(DiagnosticStrings.ValueTag, string.Empty);
                            }
                            else
                            {
                                xml.WriteElementString(DiagnosticStrings.ValueTag, XmlEncode(exception.Data[dataItem].ToString()));
                            }
                            
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                        xml.Flush();
                        stringWriter.Flush();

                        return sb.ToString();
                    }
                }
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        static bool WriteStartElement(XmlTextWriter xml, string localName, ref int remainingLength)
        {
            int minXmlLength = (localName.Length * 2) + EtwDiagnosticTrace.XmlBracketsLength;
            if (minXmlLength <= remainingLength)
            {
                xml.WriteStartElement(localName);
                remainingLength -= minXmlLength;
                return true;
            }
            return false;            
        }

        static bool WriteXmlElementString(XmlTextWriter xml, string localName, string value, ref int remainingLength)
        {
            int xmlElementLength = (localName.Length * 2) + EtwDiagnosticTrace.XmlBracketsLength + value.Length;
            if (xmlElementLength <= remainingLength)
            {
                xml.WriteElementString(localName, value);
                remainingLength -= xmlElementLength;
                return true;
            }
            return false;
        }

        static class TraceCodes
        {
            public const string AppDomainUnload = "AppDomainUnload";
            public const string TraceHandledException = "TraceHandledException";
            public const string ThrowingException = "ThrowingException";
            public const string UnhandledException = "UnhandledException";
        }

        static class EventIdsWithMsdnTraceCode
        {
            // EventIds for which we need to translate the traceCode and the eventId
            // when system.diagnostics tracing is enabled.
            public const int AppDomainUnload = 57393;
            public const int ThrowingExceptionWarning = 57396;
            public const int ThrowingExceptionVerbose = 57407;
            public const int HandledExceptionInfo = 57394;
            public const int HandledExceptionWarning = 57404;
            public const int HandledExceptionError = 57405;
            public const int HandledExceptionVerbose = 57406;
            public const int UnhandledException = 57397;
        }

        static class LegacyTraceEventIds
        {
            // Diagnostic trace codes
            public const int Diagnostics = 0X20000;
            public const int AppDomainUnload = LegacyTraceEventIds.Diagnostics | 0X0001; 
            public const int EventLog = LegacyTraceEventIds.Diagnostics | 0X0002;
            public const int ThrowingException = LegacyTraceEventIds.Diagnostics | 0X0003;
            public const int TraceHandledException = LegacyTraceEventIds.Diagnostics | 0X0004;
            public const int UnhandledException = LegacyTraceEventIds.Diagnostics | 0X0005;
        }

        static class StringBuilderPool
        {
            const int maxPooledStringBuilders = 64;
            static readonly ConcurrentQueue<StringBuilder> freeStringBuilders = new ConcurrentQueue<StringBuilder>();

            public static StringBuilder Take()
            {
                StringBuilder sb = null;
                if (freeStringBuilders.TryDequeue(out sb))
                {
                    return sb;
                }

                return new StringBuilder();
            }

            public static void Return(StringBuilder sb)
            {
                Fx.Assert(sb != null, "'sb' MUST NOT be NULL.");
                if (freeStringBuilders.Count <= maxPooledStringBuilders)
                {
                    // There is a race condition here so the count could be off a little bit (but insignificantly)
                    sb.Clear();
                    freeStringBuilders.Enqueue(sb);
                }
            }
        }
    }
}
