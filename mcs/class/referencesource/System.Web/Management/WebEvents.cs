//------------------------------------------------------------------------------
// <copyright file="WebEvents.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.Util;

    using Debug = System.Web.Util.Debug;

    // This enum matches the native one enum WebEventType (in webevent.h).
    internal enum WebEventType : int {
        WEBEVENT_BASE_EVENT = 0,
        WEBEVENT_MANAGEMENT_EVENT,
        WEBEVENT_APP_LIFETIME_EVENT,
        WEBEVENT_REQUEST_EVENT,
        WEBEVENT_HEARTBEAT_EVENT,
        WEBEVENT_BASE_ERROR_EVENT,
        WEBEVENT_REQUEST_ERROR_EVENT,
        WEBEVENT_ERROR_EVENT,
        WEBEVENT_AUDIT_EVENT,
        WEBEVENT_SUCCESS_AUDIT_EVENT,
        WEBEVENT_AUTHENTICATION_SUCCESS_AUDIT_EVENT,
        WEBEVENT_FAILURE_AUDIT_EVENT,
        WEBEVENT_AUTHENTICATION_FAILURE_AUDIT_EVENT,
        WEBEVENT_VIEWSTATE_FAILURE_AUDIT_EVENT,
    };

    internal enum WebEventFieldType : int {
        String = 0,
        Int = 1,
        Bool = 2,
        Long = 3,
        Date = 4,
    }

    // Used for marshalling over to IIS Trace
    internal class WebEventFieldData {
        string _name;
        public string Name {
            get {
                return _name;
            }
        }

        string _data;
        public string Data {
            get {
                return _data;
            }
        }

        WebEventFieldType _type;
        public WebEventFieldType Type {
            get {
                return _type;
            }
        }

        public WebEventFieldData(string name, string data, WebEventFieldType type) {
            _name = name;
            _data = data;
            _type = type;
        }
    }


    // Interface for event provider
    public abstract class WebEventProvider : ProviderBase {

        // methods
        public abstract void ProcessEvent(WebBaseEvent raisedEvent);
        public abstract void Shutdown();
        public abstract void Flush();

        int _exceptionLogged;

        internal void LogException(Exception e) {
            // In order to not overflow the eventlog, we only log one exception per provider instance.
            if (Interlocked.CompareExchange( ref _exceptionLogged, 1, 0) == 0) {
                // Log all errors in eventlog
                UnsafeNativeMethods.LogWebeventProviderFailure(
                                        HttpRuntime.AppDomainAppVirtualPath,
                                        Name,
                                        e.ToString());
            }
        }
    }

    // Interface for custom event evaluator

    public interface IWebEventCustomEvaluator {
        bool CanFire(WebBaseEvent raisedEvent, RuleFiringRecord record);
    }

    ////////////////
    // Events
    ////////////////

    public class WebBaseEvent {
        DateTime        _eventTimeUtc;
        int             _code;
        int             _detailCode;
        Object          _source;
        string          _message;
        long            _sequenceNumber;
        long            _occurrenceNumber;
        Guid            _id  = Guid.Empty;

        static long                         s_globalSequenceNumber = 0;
        static WebApplicationInformation    s_applicationInfo = new WebApplicationInformation();
        const string                        WEBEVENT_RAISE_IN_PROGRESS = "_WEvtRIP";

        // A array that cache the result of eventCode to SystemEventType mapping.
        static readonly SystemEventType[,]  s_eventCodeToSystemEventTypeMappings = new SystemEventType[WebEventCodes.GetEventArrayDimensionSize(0),
                                                                  WebEventCodes.GetEventArrayDimensionSize(1)];

        // A array that store the # of occurrence per custom event code.
        static readonly long[,]             s_eventCodeOccurrence = new long[WebEventCodes.GetEventArrayDimensionSize(0),
                                                                  WebEventCodes.GetEventArrayDimensionSize(1)];

        static Hashtable                    s_customEventCodeOccurrence = new Hashtable();
        #pragma warning disable 0649
        static ReadWriteSpinLock            s_lockCustomEventCodeOccurrence;
        #pragma warning restore 0649

        static WebBaseEvent() {

            // Initialize the mappings.  We will fill up each entry on demand by calling
            // SystemEventTypeFromEventCode().

            for (int i = 0; i < s_eventCodeToSystemEventTypeMappings.GetLength(0); i++) {
                for (int j = 0; j < s_eventCodeToSystemEventTypeMappings.GetLength(1); j++) {
                    s_eventCodeToSystemEventTypeMappings[i,j] = SystemEventType.Unknown;
                }
            }

            for (int i = 0; i < s_eventCodeOccurrence.GetLength(0); i++) {
                for (int j = 0; j < s_eventCodeOccurrence.GetLength(1); j++) {
                    s_eventCodeOccurrence[i,j] = 0;
                }
            }
        }

        void Init(string message, Object eventSource, int eventCode, int eventDetailCode) {
            if (eventCode < 0) {
                throw new ArgumentOutOfRangeException("eventCode",
                    SR.GetString(SR.Invalid_eventCode_error));
            }

            if (eventDetailCode < 0) {
                throw new ArgumentOutOfRangeException("eventDetailCode",
                    SR.GetString(SR.Invalid_eventDetailCode_error));
            }

            _code = eventCode;
            _detailCode = eventDetailCode;
            _source = eventSource;
            _eventTimeUtc = DateTime.UtcNow;
            _message = message;

            // Creation of _id is always delayed until it's needed.
        }

        // ctors
        internal protected WebBaseEvent(string message, Object eventSource, int eventCode) {
            Init(message, eventSource, eventCode, WebEventCodes.UndefinedEventDetailCode);
        }

        internal protected WebBaseEvent(string message, Object eventSource, int eventCode, int eventDetailCode) {
            Init(message, eventSource, eventCode, eventDetailCode);
        }

        internal WebBaseEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        internal bool IsSystemEvent {
            get {
                return (_code < WebEventCodes.WebExtendedBase);
            }
        }

        // Properties

        public DateTime EventTime { get { return _eventTimeUtc.ToLocalTime(); } }

        public DateTime EventTimeUtc { get { return _eventTimeUtc; } }

        public String Message { get { return _message; } }

        public Object EventSource { get { return _source; } }

        public long EventSequence { get { return _sequenceNumber; } }

        public long EventOccurrence { get { return _occurrenceNumber; } }

        public int EventCode { get { return _code; } }

        public int EventDetailCode { get { return _detailCode; } }

        public Guid EventID {
            get {
                if (_id == Guid.Empty) {
                    lock(this) {
                        if (_id == Guid.Empty) {
                            _id = Guid.NewGuid();
                        }
                    }
                }

                return _id;
            }
        }

        public static WebApplicationInformation ApplicationInformation {
            get { return s_applicationInfo; }
        }

        virtual internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_code, EventCode.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_message, Message));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_time, EventTime.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_time_Utc, EventTimeUtc.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_id, EventID.ToString("N", CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_sequence, EventSequence.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_occurrence, EventOccurrence.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_detail_code, EventDetailCode.ToString(CultureInfo.InstalledUICulture)));

            if (includeAppInfo) {
                formatter.AppendLine(String.Empty);
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_application_information));
                formatter.IndentationLevel += 1;
                ApplicationInformation.FormatToString(formatter);
                formatter.IndentationLevel -= 1;
            }
        }


        public override string ToString() {
            return ToString(true, true);
        }

        public virtual string ToString(bool includeAppInfo, bool includeCustomEventDetails) {
            WebEventFormatter   formatter = new WebEventFormatter();

            FormatToString(formatter, includeAppInfo);

            if (!IsSystemEvent && includeCustomEventDetails) {
                formatter.AppendLine(String.Empty);
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_custom_event_details));
                formatter.IndentationLevel += 1;
                FormatCustomEventDetails(formatter);
                formatter.IndentationLevel -= 1;
            }

            return formatter.ToString();
        }

        virtual public void FormatCustomEventDetails(WebEventFormatter formatter) {
        }

        internal int InferEtwTraceVerbosity() {
            WebEventType type = WebBaseEvent.WebEventTypeFromWebEvent(this);
            switch (type) {
                case WebEventType.WEBEVENT_VIEWSTATE_FAILURE_AUDIT_EVENT:
                case WebEventType.WEBEVENT_BASE_ERROR_EVENT:
                case WebEventType.WEBEVENT_REQUEST_ERROR_EVENT:
                case WebEventType.WEBEVENT_FAILURE_AUDIT_EVENT:
                case WebEventType.WEBEVENT_AUTHENTICATION_FAILURE_AUDIT_EVENT:
                case WebEventType.WEBEVENT_ERROR_EVENT:
                    return EtwTraceLevel.Warning;
                case WebEventType.WEBEVENT_AUDIT_EVENT:
                case WebEventType.WEBEVENT_SUCCESS_AUDIT_EVENT:
                case WebEventType.WEBEVENT_AUTHENTICATION_SUCCESS_AUDIT_EVENT:
                    return EtwTraceLevel.Information;
                case WebEventType.WEBEVENT_BASE_EVENT:
                case WebEventType.WEBEVENT_MANAGEMENT_EVENT:
                case WebEventType.WEBEVENT_REQUEST_EVENT:
                default:
                    return EtwTraceLevel.Verbose;
            }
        }

        internal void DeconstructWebEvent(out int eventType, out int fieldCount, out string[] fieldNames, out int[] fieldTypes, out string[] fieldData) {
            List<WebEventFieldData> fields = new List<WebEventFieldData>();

            eventType = (int)WebBaseEvent.WebEventTypeFromWebEvent(this);
            GenerateFieldsForMarshal(fields);
            fieldCount = fields.Count;
            fieldNames = new string[fieldCount];
            fieldData = new string[fieldCount];
            fieldTypes = new int[fieldCount];

            for (int i = 0; i < fieldCount; ++i) {
                fieldNames[i] = fields[i].Name;
                fieldData[i] = fields[i].Data;
                fieldTypes[i] = (int)fields[i].Type;
            }
        }

        internal virtual void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            fields.Add(new WebEventFieldData("EventTime", EventTimeUtc.ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventID", EventID.ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventMessage", Message, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationDomain", WebBaseEvent.ApplicationInformation.ApplicationDomain, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("TrustLevel", WebBaseEvent.ApplicationInformation.TrustLevel, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationVirtualPath", WebBaseEvent.ApplicationInformation.ApplicationVirtualPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationPath", WebBaseEvent.ApplicationInformation.ApplicationPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("MachineName", WebBaseEvent.ApplicationInformation.MachineName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventCode", EventCode.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("EventDetailCode", EventDetailCode.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("SequenceNumber", EventSequence.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Long));
            fields.Add(new WebEventFieldData("Occurrence", EventOccurrence.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Long));
        }

        internal virtual void PreProcessEventInit() {
        }

        static void FindEventCode(Exception e, ref int eventCode, ref int eventDetailsCode, ref Exception eStack) {
            eventDetailsCode = WebEventCodes.UndefinedEventDetailCode;

            if (e is ConfigurationException) {
                eventCode = WebEventCodes.WebErrorConfigurationError;
            }
            else if (e is HttpRequestValidationException) {
                eventCode = WebEventCodes.RuntimeErrorValidationFailure;
            }
            else if (e is HttpCompileException) {
                eventCode = WebEventCodes.WebErrorCompilationError;
            }
            else if (e is SecurityException) {
                eventCode = WebEventCodes.AuditUnhandledSecurityException;
            }
            else if (e is UnauthorizedAccessException) {
                eventCode = WebEventCodes.AuditUnhandledAccessException;
            }
            else  if (e is HttpParseException) {
                eventCode = WebEventCodes.WebErrorParserError;
            }
            else if (e is HttpException && e.InnerException is ViewStateException) {
                ViewStateException vse = (ViewStateException)e.InnerException;
                eventCode = WebEventCodes.AuditInvalidViewStateFailure;
                if (vse._macValidationError) {
                    eventDetailsCode = WebEventCodes.InvalidViewStateMac;
                }
                else {
                    eventDetailsCode = WebEventCodes.InvalidViewState;
                }

                eStack = vse;
            }
            else if (e is HttpException && ((HttpException)e).WebEventCode != WebEventCodes.UndefinedEventCode) {
                eventCode = ((HttpException)e).WebEventCode;
            }
            else {
                // We don't know what it is.  Let's see if we can find it out by using the inner exception.

                if (e.InnerException != null) {
                    // We will call FindEventCode recusively to find out if e.InnerException is the real one.

                    if (eStack == null) {
                        // Set eStack here.  If the recursive call ends up landing in
                        // WebEventCodes.RuntimeErrorUnhandledException, we'll use the original
                        // inner exception as our final result.
                        eStack = e.InnerException;
                    }

                    FindEventCode(e.InnerException, ref eventCode, ref eventDetailsCode, ref eStack);
                }
                else {
                    // It doesn't have an inner exception.  Just return the generic unhandled-exception
                    eventCode = WebEventCodes.RuntimeErrorUnhandledException;
                }
            }

            if (eStack == null) {
                eStack = e;
            }
        }

        static internal void RaiseRuntimeError(Exception e, object source) {
            Debug.Trace("WebEventRaiseError", "Error Event is raised; type=" + e.GetType().Name);

            if (!HealthMonitoringManager.Enabled) {
                return;
            }

            try {
                int         eventCode = WebEventCodes.UndefinedEventCode;
                int         eventDetailsCode = WebEventCodes.UndefinedEventDetailCode;
                HttpContext context = HttpContext.Current;
                Exception   eStack = null;

                if (context != null) {
                    Page    page = context.Handler as Page;

                    // Errors from Transacted pages can be wrapped by a
                    // HttpException
                    if (page != null &&
                        page.IsTransacted &&
                        e.GetType() == typeof(HttpException) &&
                        e.InnerException != null) {

                        e = e.InnerException;
                    }
                }

                FindEventCode(e, ref eventCode, ref eventDetailsCode, ref eStack);
                WebBaseEvent.RaiseSystemEvent(source, eventCode, eventDetailsCode, eStack);
            }
            catch {
            }
        }

        virtual internal protected void IncrementPerfCounters() {
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_TOTAL);
        }

        class CustomEventCodeOccurrence {
            internal long    _occurrence;
        }

        internal void IncrementTotalCounters(int index0, int index1) {
            _sequenceNumber = Interlocked.Increment(ref s_globalSequenceNumber);

            if (index0 != -1) {
                _occurrenceNumber = Interlocked.Increment(ref s_eventCodeOccurrence[index0, index1]);
            }
            else {
                CustomEventCodeOccurrence ceco = (CustomEventCodeOccurrence)s_customEventCodeOccurrence[_code];

                if (ceco == null) {
                    s_lockCustomEventCodeOccurrence.AcquireWriterLock();
                    try {
                        ceco = (CustomEventCodeOccurrence)s_customEventCodeOccurrence[_code];
                        if (ceco == null) {
                            ceco = new CustomEventCodeOccurrence();
                            s_customEventCodeOccurrence[_code] = ceco;
                        }
                    }
                    finally {
                        s_lockCustomEventCodeOccurrence.ReleaseWriterLock();
                    }
                }

                _occurrenceNumber = Interlocked.Increment(ref ceco._occurrence);
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        virtual public void Raise() {
            Raise(this);
        }

        // Internally raised events don't go thru this method.  They go directly to RaiseSystemEvent --> RaiseInternal
        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        static public void Raise(WebBaseEvent eventRaised) {
            if (eventRaised.EventCode < WebEventCodes.WebExtendedBase) {
                throw new HttpException(SR.GetString(SR.System_eventCode_not_allowed,
                        eventRaised.EventCode.ToString(CultureInfo.CurrentCulture),
                        WebEventCodes.WebExtendedBase.ToString(CultureInfo.CurrentCulture)));
            }

            if (!HealthMonitoringManager.Enabled) {
                Debug.Trace(
                    "WebEventRaiseDetails", "Can't fire event because we are disabled or we can't configure HealthMonManager");
                return;
            }

            RaiseInternal(eventRaised, null, -1, -1);
        }

        static internal void RaiseInternal(WebBaseEvent eventRaised, ArrayList firingRuleInfos, int index0, int index1) {
            bool    preProcessEventInitCalled = false;
            bool    inProgressSet = false;
            object  o;
            ProcessImpersonationContext ictx = null;
            HttpContext context = HttpContext.Current;

            Debug.Trace(
                "WebEventRaiseDetails", "Event is raised; event class = " + eventRaised.GetType().Name);

            // Use CallContext to make sure we detect an infinite loop where a provider calls Raise().
            o = CallContext.GetData(WEBEVENT_RAISE_IN_PROGRESS);
            if (o != null && (bool)o) {
                Debug.Trace(
                    "WebEventRaiseDetails", "An event is raised while we're raising an event.  Ignore it.");
                return;
            }

            eventRaised.IncrementPerfCounters();
            eventRaised.IncrementTotalCounters(index0, index1);

            // Find the list of rules that match this event
            if (firingRuleInfos == null) {
                HealthMonitoringManager manager = HealthMonitoringManager.Manager();

                Debug.Assert(manager != null, "manager != null");

                firingRuleInfos = manager._sectionHelper.FindFiringRuleInfos(eventRaised.GetType(), eventRaised.EventCode);
            }

            if (firingRuleInfos.Count == 0) {
                return;
            }

            try {
                bool[]  matchingProviderArray = null;

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure) && context != null)
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_RAISE_START,
                                   context.WorkerRequest,
                                   eventRaised.GetType().FullName,
                                   eventRaised.EventCode.ToString(CultureInfo.InstalledUICulture),
                                   eventRaised.EventDetailCode.ToString(CultureInfo.InstalledUICulture),
                                   null);

                try {
                    foreach (HealthMonitoringSectionHelper.FiringRuleInfo firingRuleInfo in firingRuleInfos) {
                        HealthMonitoringSectionHelper.RuleInfo  ruleInfo = firingRuleInfo._ruleInfo;
                        RuleFiringRecord record = ruleInfo._ruleFiringRecord;

                        // Check if we should fire the event based on its throttling settings
                        if (!record.CheckAndUpdate(eventRaised)) {
                            Debug.Trace("WebEventRaiseDetails",
                                    "Throttling settings not met; not fired");
                            continue;
                        }

                        // It's valid for a rule to have no referenced provider
                        if (ruleInfo._referencedProvider != null) {
                            if (!preProcessEventInitCalled) {
                                // The event may need to do pre-ProcessEvent initialization
                                eventRaised.PreProcessEventInit();
                                preProcessEventInitCalled = true;
                            }

                            // For rule infos that share the same provider, the _indexOfFirstRuleInfoWithSameProvider field
                            // is the index of the first ruleInfo among them.  We use that index in the boolean array
                            // matchingProviderArray to remember if we've already fired that provider.
                            // This is for the scenario where several rules are pointing to the same provider,
                            // and even if >1 rule actually fire and pass all throttling check,
                            // the provider is stilled fired only once.
                            if (firingRuleInfo._indexOfFirstRuleInfoWithSameProvider != -1) {
                                if (matchingProviderArray == null) {
                                    matchingProviderArray = new bool[firingRuleInfos.Count];
                                }

                                if (matchingProviderArray[firingRuleInfo._indexOfFirstRuleInfoWithSameProvider]) {
                                    Debug.Trace("WebEventRaiseDetails",
                                            "Rule with a matching provider already fired.");
                                    continue;
                                }

                                matchingProviderArray[firingRuleInfo._indexOfFirstRuleInfoWithSameProvider] = true;
                            }

                            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure) && context != null)
                                EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_DELIVER_START,
                                               context.WorkerRequest,
                                               ruleInfo._ruleSettings.Provider,
                                               ruleInfo._ruleSettings.Name,
                                               ruleInfo._ruleSettings.EventName,
                                               null);

                            // In retail build, ignore errors from provider
                            try {
                                if (ictx == null) {
                                    ictx = new ProcessImpersonationContext();
                                }

                                if (!inProgressSet) {
                                    CallContext.SetData(WEBEVENT_RAISE_IN_PROGRESS, true);
                                    inProgressSet = true;
                                }

                                Debug.Trace("WebEventRaiseDetails", "Calling ProcessEvent under " + HttpApplication.GetCurrentWindowsIdentityWithAssert().Name);
                                ruleInfo._referencedProvider.ProcessEvent(eventRaised);
                            }
                            catch (Exception e) {
                                try {
                                    ruleInfo._referencedProvider.LogException(e);
                                }
                                catch {
                                    // ignore all errors
                                }
                            }
                            finally {
                                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure) && context != null)
                                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_DELIVER_END,
                                                   context.WorkerRequest);
                            }
                        }
                    }
                }
                finally {
                    // Resume client impersonation
                    if (ictx != null) {
                        ictx.Undo();
                    }

                    if (inProgressSet) {
                        CallContext.FreeNamedDataSlot(WEBEVENT_RAISE_IN_PROGRESS);
                    }

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure) && context != null)
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_RAISE_END,
                                       context.WorkerRequest);
                }
            }
            catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)
        }

        internal static void RaiseSystemEvent(string message, object source, int eventCode, int eventDetailCode, Exception exception) {
            RaiseSystemEventInternal(message, source, eventCode, eventDetailCode, exception, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode) {
            RaiseSystemEventInternal(null, source, eventCode, WebEventCodes.UndefinedEventDetailCode, null, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, int eventDetailCode) {
            RaiseSystemEventInternal(null, source, eventCode, eventDetailCode, null, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, int eventDetailCode, Exception exception) {
            RaiseSystemEventInternal(null, source, eventCode, eventDetailCode, exception, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, string nameToAuthenticate) {
            RaiseSystemEventInternal(null, source, eventCode, WebEventCodes.UndefinedEventDetailCode, null, nameToAuthenticate);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static void RaiseSystemEventInternal(string message, object source,
                        int eventCode, int eventDetailCode, Exception exception,
                        string nameToAuthenticate) {
            HealthMonitoringManager manager;
            ArrayList       firingRuleInfos;
            SystemEventTypeInfo typeInfo;
            SystemEventType     systemEventType;
            int                 index0, index1;

            Debug.Trace(
                "WebEventRaiseDetails", "RaiseSystemEventInternal called; eventCode=" + eventCode + "; eventDetailCode=" + eventDetailCode);

            if (!HealthMonitoringManager.Enabled) {
                Debug.Trace(
                    "WebEventRaiseDetails", "Can't fire event because we are disabled or we can't configure HealthMonManager");
                return;
            }

            WebEventCodes.GetEventArrayIndexsFromEventCode(eventCode, out index0, out index1);

            GetSystemEventTypeInfo(eventCode, index0, index1, out typeInfo, out systemEventType);
            if (typeInfo == null) {
                Debug.Assert(false, "Unexpected system event code = " + eventCode);
                return;
            }

            manager = HealthMonitoringManager.Manager();

            // Figure out if there is any provider that subscribes to it
            firingRuleInfos = manager._sectionHelper.FindFiringRuleInfos(typeInfo._type, eventCode);
            if (firingRuleInfos.Count == 0) {
                // Even if we're not firing it, we still have to increment some global counters
                typeInfo._dummyEvent.IncrementPerfCounters();
                typeInfo._dummyEvent.IncrementTotalCounters(index0, index1);
            }
            else {
                // We will fire the event
                WebBaseEvent.RaiseInternal(
                                NewEventFromSystemEventType(false, systemEventType, message, source, eventCode, eventDetailCode,
                                                        exception, nameToAuthenticate),
                                firingRuleInfos, index0, index1);
            }
        }

        // An enum of all the types of web event we will fire
        enum SystemEventType {
            Unknown = -1,
            WebApplicationLifetimeEvent,
            WebHeartbeatEvent,
            WebRequestEvent,
            WebRequestErrorEvent,
            WebErrorEvent,
            WebAuthenticationSuccessAuditEvent,
            WebSuccessAuditEvent,
            WebAuthenticationFailureAuditEvent,
            WebFailureAuditEvent,
            WebViewStateFailureAuditEvent,
            Last
        };

        class SystemEventTypeInfo {
            internal WebBaseEvent       _dummyEvent;    // For calling IncrementPerfCounters. See RaiseSystemEventInternal for details.
            internal Type               _type;

            internal SystemEventTypeInfo(WebBaseEvent dummyEvent) {
                _dummyEvent = dummyEvent;
                _type = dummyEvent.GetType();
            }
        };

        // An array to cache the event type info for each system event type
        static SystemEventTypeInfo[]    s_systemEventTypeInfos = new SystemEventTypeInfo[(int)SystemEventType.Last];

        static void GetSystemEventTypeInfo(int eventCode, int index0, int index1,
                        out SystemEventTypeInfo info, out SystemEventType systemEventType) {

            // Figure out what SystemEventType this eventCode maps to.
            // For each eventCode, we store the result in a cache.
            systemEventType = s_eventCodeToSystemEventTypeMappings[index0, index1];
            if (systemEventType == SystemEventType.Unknown) {
                systemEventType = SystemEventTypeFromEventCode(eventCode);
                s_eventCodeToSystemEventTypeMappings[index0, index1] = systemEventType;
            }

            // Based on the systemEventType, we read the SystemEventTypeInfo.  For each
            // event type, we also cache the info
            info = s_systemEventTypeInfos[(int)systemEventType];
            if (info != null) {
                return;
            }

            info = new SystemEventTypeInfo(CreateDummySystemEvent(systemEventType));
            s_systemEventTypeInfos[(int)systemEventType] = info;
        }

        static SystemEventType SystemEventTypeFromEventCode(int eventCode) {
            if (eventCode >= WebEventCodes.ApplicationCodeBase &&
                eventCode <= WebEventCodes.ApplicationCodeBaseLast) {
                switch(eventCode) {
                    case WebEventCodes.ApplicationStart:
                    case WebEventCodes.ApplicationShutdown:
                    case WebEventCodes.ApplicationCompilationStart:
                    case WebEventCodes.ApplicationCompilationEnd:
                        return SystemEventType.WebApplicationLifetimeEvent;

                    case WebEventCodes.ApplicationHeartbeat:
                        return SystemEventType.WebHeartbeatEvent;
                }
            }

            if (eventCode >= WebEventCodes.RequestCodeBase &&
                eventCode <= WebEventCodes.RequestCodeBaseLast) {
                switch(eventCode) {

                    case WebEventCodes.RequestTransactionComplete:
                    case WebEventCodes.RequestTransactionAbort:
                        return SystemEventType.WebRequestEvent;
                }
            }

            if (eventCode >= WebEventCodes.ErrorCodeBase &&
                eventCode <= WebEventCodes.ErrorCodeBaseLast) {
                switch(eventCode) {
                    case WebEventCodes.RuntimeErrorRequestAbort:
                    case WebEventCodes.RuntimeErrorViewStateFailure:
                    case WebEventCodes.RuntimeErrorValidationFailure:
                    case WebEventCodes.RuntimeErrorPostTooLarge:
                    case WebEventCodes.RuntimeErrorUnhandledException:
                    case WebEventCodes.RuntimeErrorWebResourceFailure:
                        return SystemEventType.WebRequestErrorEvent;

                    case WebEventCodes.WebErrorParserError:
                    case WebEventCodes.WebErrorCompilationError:
                    case WebEventCodes.WebErrorConfigurationError:
                    case WebEventCodes.WebErrorOtherError:
                    case WebEventCodes.WebErrorPropertyDeserializationError:
                    case WebEventCodes.WebErrorObjectStateFormatterDeserializationError:
                        return SystemEventType.WebErrorEvent;
                }
            }

            if (eventCode >= WebEventCodes.AuditCodeBase &&
                eventCode <= WebEventCodes.AuditCodeBaseLast) {
                switch(eventCode) {
                    case WebEventCodes.AuditFormsAuthenticationSuccess:
                    case WebEventCodes.AuditMembershipAuthenticationSuccess:
                        return SystemEventType.WebAuthenticationSuccessAuditEvent;

                    case WebEventCodes.AuditUrlAuthorizationSuccess:
                    case WebEventCodes.AuditFileAuthorizationSuccess:
                        return SystemEventType.WebSuccessAuditEvent;

                    case WebEventCodes.AuditFormsAuthenticationFailure:
                    case WebEventCodes.AuditMembershipAuthenticationFailure:
                        return SystemEventType.WebAuthenticationFailureAuditEvent;

                    case WebEventCodes.AuditUrlAuthorizationFailure:
                    case WebEventCodes.AuditFileAuthorizationFailure:
                    case WebEventCodes.AuditUnhandledSecurityException:
                    case WebEventCodes.AuditUnhandledAccessException:
                        return SystemEventType.WebFailureAuditEvent;

                    case WebEventCodes.AuditInvalidViewStateFailure:
                        return SystemEventType.WebViewStateFailureAuditEvent;
                }
            }

            if (eventCode >= WebEventCodes.MiscCodeBase &&
                eventCode <= WebEventCodes.MiscCodeBaseLast) {
                switch(eventCode) {
                    case WebEventCodes.WebEventProviderInformation:
                        Debug.Assert(false, "WebEventProviderInformation shouldn't be used to Raise an event");
                        return SystemEventType.Unknown;
                }
            }

            return SystemEventType.Unknown;
        }

        static WebBaseEvent CreateDummySystemEvent(SystemEventType systemEventType) {
            return NewEventFromSystemEventType(true, systemEventType, null,
                        null, 0, 0, null, null);
        }

        static WebBaseEvent NewEventFromSystemEventType(bool createDummy, SystemEventType systemEventType, string message,
                        object source,int eventCode, int eventDetailCode, Exception exception,
                        string nameToAuthenticate) {
            // If createDummy == true, it means we're only creating a dummy event for the sake of using
            // it to call IncrementPerfCounters()

            if (!createDummy && message == null) {
                message = WebEventCodes.MessageFromEventCode(eventCode, eventDetailCode);
            }

            // Code view note for the future:
            // If the number of systemEventType increases tremendoulsy, we may need to
            // avoid using switch, and change to use a "factory" to create new event.

            switch(systemEventType) {
                case SystemEventType.WebApplicationLifetimeEvent:
                    return createDummy ? new WebApplicationLifetimeEvent() : new WebApplicationLifetimeEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebHeartbeatEvent:
                    return createDummy ? new WebHeartbeatEvent() : new WebHeartbeatEvent(message, eventCode);

                case SystemEventType.WebRequestEvent:
                    return createDummy ? new WebRequestEvent() : new WebRequestEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType. WebRequestErrorEvent:
                    return createDummy ? new WebRequestErrorEvent() : new WebRequestErrorEvent(message, source, eventCode, eventDetailCode, exception);

                case SystemEventType.WebErrorEvent:
                    return createDummy ? new WebErrorEvent() : new WebErrorEvent(message, source, eventCode, eventDetailCode, exception);

                case SystemEventType.WebAuthenticationSuccessAuditEvent:
                    return createDummy ? new WebAuthenticationSuccessAuditEvent() : new WebAuthenticationSuccessAuditEvent(message, source, eventCode, eventDetailCode, nameToAuthenticate);

                case SystemEventType.WebSuccessAuditEvent:
                    return createDummy ? new WebSuccessAuditEvent() : new WebSuccessAuditEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebAuthenticationFailureAuditEvent:
                    return createDummy ? new WebAuthenticationFailureAuditEvent() : new WebAuthenticationFailureAuditEvent(message, source, eventCode, eventDetailCode, nameToAuthenticate);

                case SystemEventType.WebFailureAuditEvent:
                    return createDummy ? new WebFailureAuditEvent() : new WebFailureAuditEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebViewStateFailureAuditEvent:
                    return createDummy ? new WebViewStateFailureAuditEvent() : new WebViewStateFailureAuditEvent(message, source, eventCode, eventDetailCode, (System.Web.UI.ViewStateException)exception);

                default:
                    Debug.Assert(false, "Unexpected event type = " + systemEventType);
                    return null;
            }
        }

        static string CreateWebEventResourceCacheKey(String key) {
            return CacheInternal.PrefixWebEventResource + key;
        }

        internal static String FormatResourceStringWithCache(String key) {
            // HealthMonitoring, in some scenarios, can call into the cache hundreds of 
            // times during shutdown, after the cache has been disposed.  To improve 
            // shutdown performance, skip the cache when it is disposed.
            if (HealthMonitoringManager.IsCacheDisposed) {
                return SR.Resources.GetString(key, CultureInfo.InstalledUICulture);
            }

            CacheStoreProvider cacheInternal = HttpRuntime.Cache.InternalCache;
            string s;

            string cacheKey = CreateWebEventResourceCacheKey(key);

            s = (string) cacheInternal.Get(cacheKey);
            if (s != null) {
                return s;
            }

            s = SR.Resources.GetString(key, CultureInfo.InstalledUICulture);
            if (s != null) {
                cacheInternal.Insert(cacheKey, s, null);
            }

            return s;
        }

        internal static String FormatResourceStringWithCache(String key, String arg0) {
            string fmt = FormatResourceStringWithCache(key);
            return(fmt != null) ? String.Format(fmt, arg0) : null;
        }

        internal static WebEventType WebEventTypeFromWebEvent(WebBaseEvent eventRaised) {
            // Note:
            // eventRaised can belong to one of the following classes, or can inherit from one of them.
            // In order to figure out precisely the WebEventType closest to the type of eventRaised,
            // we will start our comparison from the leaf nodes in the class hierarchy and work our
            // way up.

            // Webevent class hierarchy (with the info contained in each class):

            /*

            - WebBaseEvent (basic)
                - WebManagementEvent (+ WebProcessInformation)
                    - WebHeartbeatEvent (+ WebProcessStatistics)
                    - WebApplicationLifetimeEvent
                    - WebRequestEvent (+ WebRequestInformation)
                    - WebBaseErrorEvent (+ Exception)
                        - WebRequestErrorEvent (+ WebRequestInformation + WebThreadInformation)
                        - WebErrorEvent (+ WebRequestInformation + WebThreadInformation)
                    - WebAuditEvent (+ WebRequestInformation)
                        - WebSuccessAuditEvent
                            - WebAuthenticationSuccessAuditEvent (+ NameToAuthenticate)
                        - WebFailureAuditEvent
                            - WebAuthenticationFailureAuditEvent (+ NameToAuthenticate)
                            - WebViewStateFailureAuditEvent (+ ViewStateException)
            */

            // Hierarchy level 5

            if (eventRaised is WebAuthenticationSuccessAuditEvent) {
                return WebEventType.WEBEVENT_AUTHENTICATION_SUCCESS_AUDIT_EVENT;
            }

            if (eventRaised is WebAuthenticationFailureAuditEvent) {
                return WebEventType.WEBEVENT_AUTHENTICATION_FAILURE_AUDIT_EVENT;
            }

            if (eventRaised is WebViewStateFailureAuditEvent) {
                return WebEventType.WEBEVENT_VIEWSTATE_FAILURE_AUDIT_EVENT;
            }

            // Hierarchy level 4

            if (eventRaised is WebRequestErrorEvent) {
                return WebEventType.WEBEVENT_REQUEST_ERROR_EVENT;
            }

            if (eventRaised is WebErrorEvent) {
                return WebEventType.WEBEVENT_ERROR_EVENT;
            }

            if (eventRaised is WebSuccessAuditEvent) {
                return WebEventType.WEBEVENT_SUCCESS_AUDIT_EVENT;
            }

            if (eventRaised is WebFailureAuditEvent) {
                return WebEventType.WEBEVENT_FAILURE_AUDIT_EVENT;
            }

            // Hierarchy level 3

            if (eventRaised is WebHeartbeatEvent) {
                return WebEventType.WEBEVENT_HEARTBEAT_EVENT;
            }

            if (eventRaised is WebApplicationLifetimeEvent) {
                return WebEventType.WEBEVENT_APP_LIFETIME_EVENT;
            }

            if (eventRaised is WebRequestEvent) {
                return WebEventType.WEBEVENT_REQUEST_EVENT;
            }

            if (eventRaised is WebBaseErrorEvent) {
                return WebEventType.WEBEVENT_BASE_ERROR_EVENT;
            }

            if (eventRaised is WebAuditEvent) {
                return WebEventType.WEBEVENT_AUDIT_EVENT;
            }

            // Hierarchy level 2

            if (eventRaised is WebManagementEvent) {
                return WebEventType.WEBEVENT_MANAGEMENT_EVENT;
            }

            // Hierarchy level 1

            return WebEventType.WEBEVENT_BASE_EVENT;
        }

        static internal void RaisePropertyDeserializationWebErrorEvent(SettingsProperty property, object source, Exception exception) {
            if (HttpContext.Current == null) {
                return;
            }

            WebBaseEvent.RaiseSystemEvent(
                SR.GetString(SR.Webevent_msg_Property_Deserialization,
                            property.Name, property.SerializeAs.ToString(), property.PropertyType.AssemblyQualifiedName ),
                source,
                WebEventCodes.WebErrorPropertyDeserializationError,
                WebEventCodes.UndefinedEventDetailCode,
                exception);
        }
    }

    public class WebEventFormatter {
        int             _level;
        StringBuilder   _sb;
        int             _tabSize;

        void AddTab() {
            int level = _level;

            while (level > 0) {
                _sb.Append(' ', _tabSize);
                level--;
            }
        }

        internal WebEventFormatter() {
            _level = 0;
            _sb = new StringBuilder();
            _tabSize = 4;
        }

        public void AppendLine(string s) {
            AddTab();
            _sb.Append(s);
            _sb.Append('\n');
        }

        new public string ToString() {
            return _sb.ToString();
        }

        public int IndentationLevel {
            get { return _level; }
            set { _level = Math.Max(value, 0); }
        }

        public int TabSize {
            get { return _tabSize; }
            set { _tabSize = Math.Max(value, 0); }
        }
    }

    // This class is a base class for all events that require application and process information.
    //
    // WebManagementEvent is the base class for all our webevent classes (except for WebBaseEvent)
    // Please note that we allow customer to inherit from our webevent classes to make it easier to
    // create custom webevent that contains useful information.
    // However, WebManagementEvent (and other child events) contains sensitive information (e.g. process id, process account name)
    // that cannot be obtained unless in full-trust.
    //
    // In non-fulltrust app, we still want code inside system.web.dll to create these webevents
    // even if there is user code on the stack (we either assert or call unmanaged code to get those info).
    // So to protect these full-trust information from non-fulltrust app, we InheritanceDemand FullTrust
    // if the customer wants to inherit from WebManagementEvent.
    //
    // For details, see VSWhidbey 256684.
    //
    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public class WebManagementEvent : WebBaseEvent {
        static WebProcessInformation        s_processInfo = new WebProcessInformation();

        internal protected WebManagementEvent(string message, object eventSource, int eventCode)
            :base(message, eventSource, eventCode) {
        }

        internal protected WebManagementEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode) {
        }

        internal WebManagementEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        // properties
        public WebProcessInformation ProcessInformation {
            get { return s_processInfo; }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("AccountName", ProcessInformation.AccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ProcessName", ProcessInformation.ProcessName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ProcessID", ProcessInformation.ProcessID.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_process_information));

            formatter.IndentationLevel += 1;
            ProcessInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }

    }


    // This event is raised at periodic intervals (default 30 seconds) and provides information
    // relative to the state of the running appdomin.

    public class WebHeartbeatEvent : WebManagementEvent {
        static WebProcessStatistics    s_procStats = new WebProcessStatistics();

        internal protected WebHeartbeatEvent(string message, int eventCode)
            :base(message, null, eventCode)
        {
        }

        internal WebHeartbeatEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }


        public WebProcessStatistics ProcessStatistics {get {return s_procStats;}}

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_process_statistics));

            formatter.IndentationLevel += 1;
            s_procStats.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }
    }


    // This event represents a notable event in the lifetime of an application (app domain)
    // including startup and shutdown events.  When an app domain is terminated, the reason
    // will be expressed in the Message field (e.g. compilation threshold exceed, Shutdown
    // called explicitly, etc.).

    public class WebApplicationLifetimeEvent : WebManagementEvent {

        internal protected WebApplicationLifetimeEvent(string message, object eventSource, int eventCode)
            :base(message, eventSource, eventCode)
        {
        }


        internal protected WebApplicationLifetimeEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal WebApplicationLifetimeEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        static internal int DetailCodeFromShutdownReason(ApplicationShutdownReason reason) {
            switch (reason) {
            case ApplicationShutdownReason.HostingEnvironment:
                return WebEventCodes.ApplicationShutdownHostingEnvironment;

            case ApplicationShutdownReason.ChangeInGlobalAsax:
                return WebEventCodes.ApplicationShutdownChangeInGlobalAsax;

            case ApplicationShutdownReason.ConfigurationChange:
                return WebEventCodes.ApplicationShutdownConfigurationChange;

            case ApplicationShutdownReason.UnloadAppDomainCalled:
                return WebEventCodes.ApplicationShutdownUnloadAppDomainCalled;

            case ApplicationShutdownReason.ChangeInSecurityPolicyFile:
                return WebEventCodes.ApplicationShutdownChangeInSecurityPolicyFile;

            case ApplicationShutdownReason.BinDirChangeOrDirectoryRename:
                return WebEventCodes.ApplicationShutdownBinDirChangeOrDirectoryRename;

            case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename:
                return WebEventCodes.ApplicationShutdownBrowsersDirChangeOrDirectoryRename;

            case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename:
                return WebEventCodes.ApplicationShutdownCodeDirChangeOrDirectoryRename;

            case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename:
                return WebEventCodes.ApplicationShutdownResourcesDirChangeOrDirectoryRename;

            case ApplicationShutdownReason.IdleTimeout:
                return WebEventCodes.ApplicationShutdownIdleTimeout;

            case ApplicationShutdownReason.PhysicalApplicationPathChanged:
                return WebEventCodes.ApplicationShutdownPhysicalApplicationPathChanged;

            case ApplicationShutdownReason.HttpRuntimeClose:
                return WebEventCodes.ApplicationShutdownHttpRuntimeClose;

            case ApplicationShutdownReason.InitializationError:
                return WebEventCodes.ApplicationShutdownInitializationError;

            case ApplicationShutdownReason.MaxRecompilationsReached:
                return WebEventCodes.ApplicationShutdownMaxRecompilationsReached;

            case ApplicationShutdownReason.BuildManagerChange:
                return WebEventCodes.ApplicationShutdownBuildManagerChange;

            default:
                return WebEventCodes.ApplicationShutdownUnknown;
            }
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_APP);
        }
    }

    // This class serves as a base for non-error events that provide 
    public class WebRequestEvent : WebManagementEvent {
        WebRequestInformation   _requestInfo;

        override internal void PreProcessEventInit() {
            base.PreProcessEventInit();
            InitRequestInformation();
        }

        internal protected WebRequestEvent(string message, object eventSource, int eventCode)
        :base(message, eventSource, eventCode)
        {
        }

        internal protected WebRequestEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal WebRequestEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        void InitRequestInformation() {
            if (_requestInfo == null) {
                _requestInfo = new WebRequestInformation();
            }
        }

        public WebRequestInformation RequestInformation {
            get {
                InitRequestInformation();
                return _requestInfo;
            }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("RequestUrl", RequestInformation.RequestUrl, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestPath", RequestInformation.RequestPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserHostAddress", RequestInformation.UserHostAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserName", RequestInformation.Principal.Identity.Name, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAuthenticated", RequestInformation.Principal.Identity.IsAuthenticated.ToString(), WebEventFieldType.Bool));
            fields.Add(new WebEventFieldData("UserAuthenticationType", RequestInformation.Principal.Identity.AuthenticationType, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestThreadAccountName", RequestInformation.ThreadAccountName, WebEventFieldType.String));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_information));

            formatter.IndentationLevel += 1;
            RequestInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_WEB_REQ);
        }
    }

    // This is the base class for all error events.
    public class WebBaseErrorEvent : WebManagementEvent {
        Exception               _exception;

        void Init(Exception e) {
            _exception = e;
        }

        internal protected WebBaseErrorEvent(string message, object eventSource, int eventCode, Exception e)
            :base(message, eventSource, eventCode)
        {
            Init(e);
        }

        internal protected WebBaseErrorEvent(string message, object eventSource, int eventCode, int eventDetailCode, Exception e)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
            Init(e);
        }

        internal WebBaseErrorEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        public Exception ErrorException {
            get { return _exception; }
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            if (_exception == null) {
                return;
            }

            Exception   ex = _exception;

            // Please note we arbitrary pick a level limit per bug VSWhidbey 143859
            for (int level = 0;
                  ex != null && level <= 2;
                  ex = ex.InnerException, level++)  {

                formatter.AppendLine(String.Empty);

                if (level == 0) {
                    formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_exception_information));
                }
                else {
                    formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_inner_exception_information, level.ToString(CultureInfo.InstalledUICulture)));
                }

                formatter.IndentationLevel += 1;
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_exception_type, ex.GetType().ToString()));
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_exception_message, ex.Message));
                formatter.IndentationLevel -= 1;
            }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("ExceptionType", ErrorException.GetType().ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ExceptionMessage", ErrorException.Message, WebEventFieldType.String));
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_ERROR);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_EVENTS_ERROR);
        }
    }

    // This class contains information about systemic errors, e.g. things related to
    // configuration or application code (parser errors, compilation errors).

    public class WebErrorEvent : WebBaseErrorEvent {
        WebRequestInformation   _requestInfo;
        WebThreadInformation    _threadInfo;

        void Init(Exception e) {
        }

        override internal void PreProcessEventInit() {
            base.PreProcessEventInit();
            InitRequestInformation();
            InitThreadInformation();
        }

        internal protected WebErrorEvent(string message, object eventSource, int eventCode, Exception exception)
            :base(message, eventSource, eventCode, exception)
        {
            Init(exception);
        }

        internal protected WebErrorEvent(string message, object eventSource, int eventCode, int eventDetailCode, Exception exception)
            :base(message, eventSource, eventCode, eventDetailCode, exception)
        {
            Init(exception);
        }

        internal WebErrorEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        void InitRequestInformation() {
            if (_requestInfo == null) {
                _requestInfo = new WebRequestInformation();
            }
        }

        public WebRequestInformation RequestInformation {
            get {
                InitRequestInformation();
                return _requestInfo;
            }
        }

        void InitThreadInformation() {
            if (_threadInfo == null) {
                _threadInfo = new WebThreadInformation(base.ErrorException);
            }
        }

        public WebThreadInformation ThreadInformation {
            get {
                InitThreadInformation();
                return _threadInfo;
            }
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_information));

            formatter.IndentationLevel += 1;
            RequestInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_information));

            formatter.IndentationLevel += 1;
            ThreadInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("RequestUrl", RequestInformation.RequestUrl, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestPath", RequestInformation.RequestPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserHostAddress", RequestInformation.UserHostAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserName", RequestInformation.Principal.Identity.Name, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAuthenticated", RequestInformation.Principal.Identity.IsAuthenticated.ToString(), WebEventFieldType.Bool));
            fields.Add(new WebEventFieldData("UserAuthenticationType", RequestInformation.Principal.Identity.AuthenticationType, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestThreadAccountName", RequestInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ThreadID", ThreadInformation.ThreadID.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("ThreadAccountName", ThreadInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("StackTrace", ThreadInformation.StackTrace, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("IsImpersonating", ThreadInformation.IsImpersonating.ToString(), WebEventFieldType.Bool));
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_HTTP_INFRA_ERROR);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_EVENTS_HTTP_INFRA_ERROR);
        }
    }

    // This class provides information about errors that occur while servicing a request.
    // This include unhandled exceptions, viewstate errors, input validation errors, etc.
    public class WebRequestErrorEvent : WebBaseErrorEvent {
        WebRequestInformation   _requestInfo;
        WebThreadInformation    _threadInfo;

        void Init(Exception e) {
        }

        override internal void PreProcessEventInit() {
            base.PreProcessEventInit();
            InitRequestInformation();
            InitThreadInformation();
        }

        internal protected WebRequestErrorEvent(string message, object eventSource, int eventCode, Exception exception)
            :base(message, eventSource, eventCode, exception)
        {
            Init(exception);
        }

        internal protected WebRequestErrorEvent(string message, object eventSource, int eventCode, int eventDetailCode, Exception exception)
            :base(message, eventSource, eventCode, eventDetailCode, exception)
        {
            Init(exception);
        }

        internal WebRequestErrorEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        // properties

        void InitRequestInformation() {
            if (_requestInfo == null) {
                _requestInfo = new WebRequestInformation();
            }
        }

        public WebRequestInformation RequestInformation {
            get {
                InitRequestInformation();
                return _requestInfo;
            }
        }

        void InitThreadInformation() {
            if (_threadInfo == null) {
                _threadInfo = new WebThreadInformation(base.ErrorException);
            }
        }

        public WebThreadInformation ThreadInformation {
            get {
                InitThreadInformation();
                return _threadInfo;
            }
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_information));

            formatter.IndentationLevel += 1;
            RequestInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_information));

            formatter.IndentationLevel += 1;
            ThreadInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("RequestUrl", RequestInformation.RequestUrl, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestPath", RequestInformation.RequestPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserHostAddress", RequestInformation.UserHostAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserName", RequestInformation.Principal.Identity.Name, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAuthenticated", RequestInformation.Principal.Identity.IsAuthenticated.ToString(), WebEventFieldType.Bool));
            fields.Add(new WebEventFieldData("UserAuthenticationType", RequestInformation.Principal.Identity.AuthenticationType, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestThreadAccountName", RequestInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ThreadID", ThreadInformation.ThreadID.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("ThreadAccountName", ThreadInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("StackTrace", ThreadInformation.StackTrace, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("IsImpersonating", ThreadInformation.IsImpersonating.ToString(), WebEventFieldType.Bool));
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_HTTP_REQ_ERROR);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_EVENTS_HTTP_REQ_ERROR);
        }
    }


    // The base class for all audit events.

    public class WebAuditEvent : WebManagementEvent {
        WebRequestInformation   _requestInfo;

        override internal void PreProcessEventInit() {
            base.PreProcessEventInit();
            InitRequestInformation();
        }

        internal protected WebAuditEvent(string message, object eventSource, int eventCode)
            :base(message, eventSource, eventCode)
        {
        }

        internal protected WebAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal WebAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        void InitRequestInformation() {
            if (_requestInfo == null) {
                _requestInfo = new WebRequestInformation();
            }
        }

        public WebRequestInformation RequestInformation {
            get {
                InitRequestInformation();
                return _requestInfo;
            }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("RequestUrl", RequestInformation.RequestUrl, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestPath", RequestInformation.RequestPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserHostAddress", RequestInformation.UserHostAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserName", RequestInformation.Principal.Identity.Name, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAuthenticated", RequestInformation.Principal.Identity.IsAuthenticated.ToString(), WebEventFieldType.Bool));
            fields.Add(new WebEventFieldData("UserAuthenticationType", RequestInformation.Principal.Identity.AuthenticationType, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestThreadAccountName", RequestInformation.ThreadAccountName, WebEventFieldType.String));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_information));

            formatter.IndentationLevel += 1;
            RequestInformation.FormatToString(formatter);
            formatter.IndentationLevel -= 1;
        }
    }


    // This class provides information about all failure audits.  In most cases,
    // applications will only want to enable failure audits.

    public class WebFailureAuditEvent : WebAuditEvent {
        internal protected WebFailureAuditEvent(string message, object eventSource, int eventCode)
            :base(message, eventSource, eventCode)
        {
        }

        internal protected WebFailureAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal WebFailureAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.AUDIT_FAIL);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_AUDIT_FAIL);
        }
    }

    public class WebAuthenticationFailureAuditEvent : WebFailureAuditEvent {

        string  _nameToAuthenticate;

        void Init(string name) {
            _nameToAuthenticate = name;
        }

        internal protected WebAuthenticationFailureAuditEvent(string message, object eventSource, int eventCode, string nameToAuthenticate)
            :base(message, eventSource, eventCode)
        {
            Init(nameToAuthenticate);
        }


        internal protected WebAuthenticationFailureAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode, string nameToAuthenticate)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
            Init(nameToAuthenticate);
        }

        internal WebAuthenticationFailureAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        public string NameToAuthenticate {
            get { return _nameToAuthenticate; }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("NameToAuthenticate", NameToAuthenticate, WebEventFieldType.String));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_name_to_authenticate, _nameToAuthenticate));

        }
    }

    public class WebViewStateFailureAuditEvent : WebFailureAuditEvent {

        ViewStateException  _viewStateException;

        internal protected WebViewStateFailureAuditEvent(string message, object eventSource, int eventCode, ViewStateException viewStateException)
            :base(message, eventSource, eventCode)
        {
            _viewStateException = viewStateException;
        }


        internal protected WebViewStateFailureAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode, ViewStateException viewStateException)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
            _viewStateException = viewStateException;
        }

        internal WebViewStateFailureAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        public ViewStateException ViewStateException {
            get { return _viewStateException; }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("ViewStateExceptionMessage", ViewStateException.Message, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RemoteAddress", ViewStateException.RemoteAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RemotePort", ViewStateException.RemotePort, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAgent", ViewStateException.UserAgent, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("PersistedState", ViewStateException.PersistedState, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("Path", ViewStateException.Path, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("Referer", ViewStateException.Referer, WebEventFieldType.String));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_ViewStateException_information));
            formatter.IndentationLevel += 1;
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_exception_message, _viewStateException.Message));

            formatter.IndentationLevel -= 1;
        }

    }


    // This class provides information about all success audits.  In most cases,
    // applications will only want to enable failure audits.

    public class WebSuccessAuditEvent : WebAuditEvent {
        internal protected WebSuccessAuditEvent(string message, object eventSource, int eventCode)
            :base(message, eventSource, eventCode)
        {
        }

        internal protected WebSuccessAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal WebSuccessAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        override internal protected void IncrementPerfCounters() {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.AUDIT_SUCCESS);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_AUDIT_SUCCESS);
        }
    }

    public class WebAuthenticationSuccessAuditEvent : WebSuccessAuditEvent {

        string  _nameToAuthenticate;

        void Init(string name) {
            _nameToAuthenticate = name;
        }

        internal protected WebAuthenticationSuccessAuditEvent(string message, object eventSource, int eventCode, string nameToAuthenticate)
            :base(message, eventSource, eventCode)
        {
            Init(nameToAuthenticate);
        }

        internal protected WebAuthenticationSuccessAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode, string nameToAuthenticate)
            :base(message, eventSource, eventCode, eventDetailCode)
        {
            Init(nameToAuthenticate);
        }

        internal WebAuthenticationSuccessAuditEvent() {
            // For creating dummy event.  See GetSystemDummyEvent()
        }

        public string NameToAuthenticate {
            get { return _nameToAuthenticate; }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields) {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("NameToAuthenticate", NameToAuthenticate, WebEventFieldType.String));
        }

        override internal void FormatToString(WebEventFormatter formatter, bool includeAppInfo) {
            base.FormatToString(formatter, includeAppInfo);

            formatter.AppendLine(String.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_name_to_authenticate, _nameToAuthenticate));
        }
    }


    ////////////////
    // Information
    ////////////////
    
    public sealed class WebProcessInformation {
        // The information is per worker process instance.

        int     _processId;
        string  _processName;
        string  _accountName;

        internal WebProcessInformation() {
            // Can't use Process.ProcessName because it requires the running
            // account to be part of the Performance Monitor Users group.
            StringBuilder buf = new StringBuilder(256);
            if (UnsafeNativeMethods.GetModuleFileName(IntPtr.Zero, buf, 256) == 0) {
                _processName = String.Empty;
            }
            else {
                int lastIndex;

                _processName = buf.ToString();
                lastIndex = _processName.LastIndexOf('\\');
                if (lastIndex != -1) {
                    _processName = _processName.Substring(lastIndex + 1);
                }
            }

            _processId = SafeNativeMethods.GetCurrentProcessId() ;
            _accountName = HttpRuntime.WpUserId;
        }

        public int ProcessID { get { return _processId; } }

        public string ProcessName { get { return _processName; } }

        public string AccountName { get { return (_accountName != null ? _accountName : String.Empty); } }

        public void FormatToString(WebEventFormatter formatter) {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_process_id, ProcessID.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_process_name, ProcessName));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_account_name, AccountName));
        }
    }

    public sealed class WebApplicationInformation {
        // The information is per appdomain.

        string  _appDomain;
        string  _trustLevel;
        string  _appUrl;
        string  _appPath;
        string  _machineName;

        internal WebApplicationInformation() {
            _appDomain = Thread.GetDomain().FriendlyName;
            _trustLevel = HttpRuntime.TrustLevel;
            _appUrl = HttpRuntime.AppDomainAppVirtualPath;

            try {
                // We will get an exception if it's a non-ASP.NET app.
                _appPath = HttpRuntime.AppDomainAppPathInternal;
            }
            catch {
                _appPath = null;
            }
#if FEATURE_PAL // FEATURE_PAL does not fully implement Environment.MachineName
            _machineName = "dummymachinename";
#else // FEATURE_PAL
            _machineName = GetMachineNameWithAssert();
#endif // FEATURE_PAL
        }

        public string ApplicationDomain { get { return _appDomain; } }

        public string TrustLevel { get { return _trustLevel; } }

        public string ApplicationVirtualPath { get { return _appUrl; } }

        public string ApplicationPath { get { return _appPath; } }

        public string MachineName { get { return _machineName; } }

        public void FormatToString(WebEventFormatter formatter) {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_application_domain, ApplicationDomain));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_trust_level, TrustLevel));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_application_virtual_path, ApplicationVirtualPath));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_application_path, ApplicationPath));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_machine_name, MachineName));
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private string GetMachineNameWithAssert() {
            return Environment.MachineName;
        }

        public override string ToString() {
            WebEventFormatter formatter = new WebEventFormatter();

            FormatToString(formatter);
            return formatter.ToString();
        }
    }

    public sealed class WebRequestInformation {

        string      _requestUrl;
        string      _requestPath;
        IPrincipal  _iprincipal;
        string      _userHostAddress = null;
        string      _accountName;

        internal    WebRequestInformation() {
            // Need to Assert() in order to get request information regardless of trust level. See VSWhidbey 416733
            InternalSecurityPermissions.ControlPrincipal.Assert();

            HttpContext context = HttpContext.Current;
            HttpRequest request = null;

            if (context != null) {
                bool hideRequestResponseOriginal = context.HideRequestResponse;
                context.HideRequestResponse = false;
                request = context.Request;
                context.HideRequestResponse = hideRequestResponseOriginal;

                _iprincipal = context.User;

                // Dev11 #80084 - DTS Bug
                // In integrated pipeline, we are very aggressive about disposing
                // WindowsIdentity's.  If this WebRequestInformation is being used
                // post-request (eg, while formatting data for an email provider
                // that is reporting batched events), then the User.Identity is
                // likely to be disposed.  So lets create a clone that will stick
                // around.  This condition should vaguely match that found in
                // HttpContext.DisposePrincipal().
                if (_iprincipal is WindowsPrincipal
                    && _iprincipal != WindowsAuthenticationModule.AnonymousPrincipal
                    && (context.WorkerRequest is IIS7WorkerRequest)) {
                    WindowsIdentity winIdentity = _iprincipal.Identity as WindowsIdentity;
                    if (winIdentity != null) {
                        _iprincipal = new WindowsPrincipal(new WindowsIdentity(winIdentity.Token, winIdentity.AuthenticationType));
                    }
                }
            } else {
                _iprincipal = null;
            }

            if (request == null) {
                _requestUrl = String.Empty;
                _requestPath = String.Empty;
                _userHostAddress = String.Empty;
            }
            else {
                _requestUrl = request.UrlInternal;
                _requestPath = request.Path;
                _userHostAddress = request.UserHostAddress;
            }
            _accountName = WindowsIdentity.GetCurrent().Name;
        }

        // The information is per request.

        public string RequestUrl {get {return _requestUrl;}}

        public string RequestPath {get {return _requestPath;}}

        public IPrincipal Principal {get {return _iprincipal;}}

        public string UserHostAddress { get { return _userHostAddress;} }

        public string ThreadAccountName {get {return _accountName;}}

        public void FormatToString(WebEventFormatter formatter) {
            string      user;
            string      authType;
            bool        authed;

            if (Principal == null) {
                user = String.Empty;
                authType = String.Empty;
                authed = false;
            }
            else {
                IIdentity    id = Principal.Identity;

                user = id.Name;
                authed = id.IsAuthenticated;
                authType = id.AuthenticationType;
            }

            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_url, RequestUrl));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_path, RequestPath));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_user_host_address, UserHostAddress));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_user, user));

            if (authed) {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_is_authenticated));
            }
            else {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_is_not_authenticated));
            }
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_authentication_type, authType));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_account_name, ThreadAccountName));

        }
    }


    // Note that even all the information contained in WebProcessStatistics is obtained from static variables,
    // but we still don't want this class to be static.
    //
    // Currently, WebProcessStatistics can be obtained only thru WebHeartbeatEvent, which in turn can be
    // created only by Full-trust app thru class inheritance. (System.Web.dll will also internally create it
    // and the object will be available to our provider.)  Thus, WebProcessStatistics is available only
    // to Full-trust app or provider.
    //
    // But if we make WebProcessStatistics static, then all its public methods have to be static, and
    // they'll be fully available to all users.  No good.
    public class WebProcessStatistics {
        static DateTime     s_startTime = DateTime.MinValue;
        static DateTime     s_lastUpdated = DateTime.MinValue;
        static int          s_threadCount;
        static long         s_workingSet;
        static long         s_peakWorkingSet;
        static long         s_managedHeapSize;
        static int          s_appdomainCount;
        static int          s_requestsExecuting;
        static int          s_requestsQueued;
        static int          s_requestsRejected;
        static bool         s_getCurrentProcFailed = false;
        static object       s_lockObject = new object();

        static TimeSpan      TS_ONE_SECOND = new TimeSpan(0, 0, 1);

        static WebProcessStatistics() {
            try {
#if !FEATURE_PAL // FEATURE_PAL does not support this.  Make into a noop.
                s_startTime = Process.GetCurrentProcess().StartTime;
#endif // !FEATURE_PAL
            }
            catch {
                s_getCurrentProcFailed = true;
            }
        }

        void Update() {
            DateTime    now = DateTime.Now;

            if (now - s_lastUpdated < TS_ONE_SECOND) {
                return;
            }

            lock (s_lockObject) {
                if (now - s_lastUpdated < TS_ONE_SECOND) {
                    return;
                }

                if (!s_getCurrentProcFailed) {
                    Process process = Process.GetCurrentProcess();
#if !FEATURE_PAL // FEATURE_PAL does not support these Process properties

                    s_threadCount = process.Threads.Count;
                    s_workingSet = (long)process.WorkingSet64;
                    s_peakWorkingSet = (long)process.PeakWorkingSet64;
#else
                    throw new NotImplementedException ("ROTORTODO");
#endif
                }

                s_managedHeapSize = GC.GetTotalMemory(false);

                s_appdomainCount = HostingEnvironment.AppDomainsCount;

                s_requestsExecuting = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
                s_requestsQueued = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_QUEUED);
                s_requestsRejected = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_REJECTED);

                s_lastUpdated = now;
            }
        }


        public DateTime ProcessStartTime {get {Update(); return s_startTime;}}

        public int ThreadCount {get {Update(); return s_threadCount;}}

        public long WorkingSet {get {Update(); return s_workingSet;}}

        public long PeakWorkingSet {get {Update(); return s_peakWorkingSet;}}

        public long ManagedHeapSize {get {Update(); return s_managedHeapSize;}}

        public int AppDomainCount {get {Update(); return s_appdomainCount;}}

        public int RequestsExecuting {get {Update(); return s_requestsExecuting;}}

        public int RequestsQueued {get {Update(); return s_requestsQueued;}}

        public int RequestsRejected {get {Update(); return s_requestsRejected;}}


        virtual public void FormatToString(WebEventFormatter formatter) {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_process_start_time, ProcessStartTime.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_count, ThreadCount.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_working_set, WorkingSet.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_peak_working_set, PeakWorkingSet.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_managed_heap_size, ManagedHeapSize.ToString(CultureInfo.InstalledUICulture)));

            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_application_domain_count, AppDomainCount.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_requests_executing, RequestsExecuting.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_queued, RequestsQueued.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_request_rejected, RequestsRejected.ToString(CultureInfo.InstalledUICulture)));
        }
    }

    public sealed class WebThreadInformation {
        int     _threadId;
        string  _accountName;
        string  _stackTrace;
        bool    _isImpersonating;
        internal const string IsImpersonatingKey = "ASPIMPERSONATING";

        internal WebThreadInformation(Exception exception) {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _accountName = HttpApplication.GetCurrentWindowsIdentityWithAssert().Name;

            if (exception != null) {
                _stackTrace = new StackTrace(exception, true).ToString();
                _isImpersonating = exception.Data.Contains(IsImpersonatingKey);
            }
            else {
                _stackTrace = String.Empty;
                _isImpersonating = false;
            }
        }


        public int ThreadID {get {return _threadId;}}

        public string ThreadAccountName {get {return _accountName;}}

        public string StackTrace {get {return _stackTrace;}}

        public bool IsImpersonating {get {return _isImpersonating;}}


        public void FormatToString(WebEventFormatter formatter) {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_id, ThreadID.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_thread_account_name, ThreadAccountName));

            if (IsImpersonating) {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_is_impersonating));
            }
            else {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_is_not_impersonating));
            }

            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_event_stack_trace, StackTrace));
        }
    }

    // Class that represents the firing record (how many times, last firing time, etc)
    // for each rule that inherits from WebBaseEvent.

    public sealed class RuleFiringRecord {
        internal DateTime   _lastFired;
        internal int        _timesRaised;
        internal int        _updatingLastFired;

        static TimeSpan     TS_ONE_SECOND = new TimeSpan(0, 0, 1);

        public DateTime LastFired { get { return _lastFired; } }


        public int TimesRaised { get { return _timesRaised; } }

        // Point to the ruleInfo that is used by this class
        internal HealthMonitoringSectionHelper.RuleInfo    _ruleInfo;

        internal RuleFiringRecord(HealthMonitoringSectionHelper.RuleInfo ruleInfo) {

            Debug.Assert(ruleInfo != null, "ruleInfo != null");

            _ruleInfo = ruleInfo;

            _lastFired = DateTime.MinValue;
            _timesRaised = 0;
            _updatingLastFired = 0;
        }

        void UpdateLastFired(DateTime now, bool alreadyLocked) {
            TimeSpan    tsDiff = now - _lastFired;

            if (tsDiff < TS_ONE_SECOND) {
                // If _lastFired was updated within one second, don't bother.
                return;
            }

            if (!alreadyLocked) {
                // If several threads are firing at the same time, only one thread will
                // update record._lastFired.
                if (Interlocked.CompareExchange(ref _updatingLastFired, 1, 0) == 0) {
                    try {
                        _lastFired = now;
                    }
                    finally {
                        Interlocked.Exchange(ref _updatingLastFired, 0);
                    }
                }
            }
            else {
                _lastFired = now;
            }
        }

        // Note: this method is thread safe
        internal bool CheckAndUpdate(WebBaseEvent eventRaised) {
            DateTime    now = DateTime.Now;
            int         timesRaised;
            HealthMonitoringManager manager = HealthMonitoringManager.Manager();

            timesRaised = Interlocked.Increment(ref _timesRaised);

            if (manager == null) {
                // Won't fire because we cannot configure healthmonitor
                Debug.Trace("RuleFiringRecord", "Can't configure healthmonitor");
                return false;
            }

            // Call custom evaluator first
            if (_ruleInfo._customEvaluatorType != null) {

                IWebEventCustomEvaluator    icustom = (IWebEventCustomEvaluator)
                        manager._sectionHelper._customEvaluatorInstances[_ruleInfo._customEvaluatorType];

#if (!DBG)
                try {
#endif
                    // The event may need to do pre-ProcessEvent initialization
                    eventRaised.PreProcessEventInit();

                    if (!icustom.CanFire(eventRaised, this)) {
                        Debug.Trace("RuleFiringRecord", "Custom evaluator returns false.");
                        return false;
                    }
#if (!DBG)
                }
                catch {
                    Debug.Trace("RuleFiringRecord", "Hit an exception when calling Custom evaluator");
                    // Return if we hit an error
                    return false;
                }
#endif
            }

            if (timesRaised < _ruleInfo._minInstances) {
                Debug.Trace("RuleFiringRecord",
                        "MinInterval not met; timesRaised=" + timesRaised +
                        "; _minInstances=" + _ruleInfo._minInstances);
                return false;
            }

            if (timesRaised > _ruleInfo._maxLimit) {
                Debug.Trace("RuleFiringRecord",
                        "MaxLimit exceeded; timesRaised=" + timesRaised +
                        "; _maxLimit=" + _ruleInfo._maxLimit);
                return false;
            }

            // Last step: Check MinInterval and update _lastFired

            if (_ruleInfo._minInterval == TimeSpan.Zero) {
                UpdateLastFired(now, false);
                return true;
            }

            if ((now - _lastFired) <= _ruleInfo._minInterval) {
                Debug.Trace("RuleFiringRecord",
                        "MinInterval not met; now=" + now +
                        "; _lastFired=" + _lastFired +
                        "; _ruleInfo._minInterval=" + _ruleInfo._minInterval);
                return false;
            }

            // The lock is to prevent multiple threads from passing the
            // same test simultaneously.
            lock (this) {
                if ((now - _lastFired) <= _ruleInfo._minInterval) {
                    Debug.Trace("RuleFiringRecord",
                            "MinInterval not met; now=" + now +
                            "; _lastFired=" + _lastFired +
                            "; _ruleInfo._tsMinInterval=" + _ruleInfo._minInterval);
                    return false;
                }

                UpdateLastFired(now, true);
                return true;
            }
        }

    }

    internal class HealthMonitoringManager {
        internal HealthMonitoringSectionHelper          _sectionHelper;
        internal bool           _enabled = false;

        static Timer            s_heartbeatTimer = null;
        static HealthMonitoringManager s_manager = null;
        static bool             s_inited = false;
        static bool             s_initing = false;
        static object           s_lockObject = new object();
        static bool             s_isCacheDisposed = false;

        // If this method returns null, it means we failed during configuration.
        internal static HealthMonitoringManager Manager() {

            if (s_initing) {
                Debug.Assert(!s_inited);
                // If this is true, that means we are calling WebEventBase.Raise while
                // we are initializing.  That means Init() has caused a config exception.
                return null;
            }

            if (s_inited) {
                return s_manager;
            }

            lock (s_lockObject) {
                if (s_inited) {
                    return s_manager;
                }

                try {
                    Debug.Assert(s_manager == null);
                    s_initing = true;
                    s_manager = new HealthMonitoringManager();;
                }
                finally {
                    s_initing = false;
                    s_inited = true;
                }
            }

            return s_manager;
        }

        internal static bool Enabled {
            get {
                // DevDiv 92252: Visual Studio 2010 freezes when opening .cs file in App_Code directory
                // Never raise webevents from CBM process
                if (HostingEnvironment.InClientBuildManager) {
                    return false;
                }

                HealthMonitoringManager manager = HealthMonitoringManager.Manager();
                if (manager == null) {
                    return false;
                }

                return manager._enabled;
            }
        }

        internal static bool IsCacheDisposed { get { return s_isCacheDisposed; } set { s_isCacheDisposed = value; } }

        internal static void StartHealthMonitoringHeartbeat() {
            HealthMonitoringManager manager = Manager();
            if (manager == null) {
                Debug.Trace(
                    "HealthMonitoringManager", "Can't fire heartbeat because we cannot configure HealthMon");
                return;
            }

            if (!manager._enabled) {
                Debug.Trace(
                    "WebEventRaiseDetails", "Can't fire heartbeat because we are disabled");
                return;
            }

            manager.StartHeartbeatTimer();
        }

        private HealthMonitoringManager() {
            _sectionHelper = HealthMonitoringSectionHelper.GetHelper();

            _enabled = _sectionHelper.Enabled;

            if (!_enabled) {
                return;
            }
        }

        internal static void Shutdown() {
            WebEventManager.Shutdown();
            Dispose();
        }

        internal static void Dispose() {
            // Make sure this function won't throw
            try {
                if (s_heartbeatTimer != null) {
                    s_heartbeatTimer.Dispose();
                    s_heartbeatTimer = null;
                }
            }
            catch {
            }
        }

        internal void HeartbeatCallback(object state) {
            Debug.Assert(HealthMonitoringManager.Enabled);
            WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.ApplicationHeartbeat);
        }

        internal void StartHeartbeatTimer() {
            TimeSpan interval = _sectionHelper.HealthMonitoringSection.HeartbeatInterval;

            if (interval == TimeSpan.Zero) {
                return;
            }

#if DBG
            if (!Debug.IsTagPresent("Timer") || Debug.IsTagEnabled("Timer"))
#endif
            {
                s_heartbeatTimer = new Timer(new TimerCallback(this.HeartbeatCallback), null,
                    TimeSpan.Zero, interval);
            }
        }

        internal static HealthMonitoringSectionHelper.ProviderInstances ProviderInstances {
            get {
                HealthMonitoringManager manager = Manager();

                if (manager == null) {
                    return null;
                }

                if (!manager._enabled) {
                    return null;
                }

                return manager._sectionHelper._providerInstances;
            }
        }
    }

    public sealed class WebBaseEventCollection : ReadOnlyCollectionBase 
    {
        public WebBaseEventCollection(ICollection events) {
            if (events == null) {
                throw new ArgumentNullException("events");
            }

            foreach (WebBaseEvent eventRaised in events) {
                InnerList.Add(eventRaised);
            }
        }

        internal WebBaseEventCollection(WebBaseEvent eventRaised) {
            if (eventRaised == null) {
                throw new ArgumentNullException("eventRaised");
            }

            InnerList.Add(eventRaised);
        }

        // overloaded collection access methods
        public WebBaseEvent this[int index] {
            get {
                return (WebBaseEvent) InnerList[index];
            }
        }

        public int IndexOf(WebBaseEvent value) {
            return InnerList.IndexOf(value);
        }

        public bool Contains(WebBaseEvent value) {
            return InnerList.Contains(value);
        }
    }

    public static class WebEventManager
    {

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Flush(string providerName) {
            HealthMonitoringSectionHelper.ProviderInstances   providers = HealthMonitoringManager.ProviderInstances;

            if (providers == null) {
                return;
            }

            if (!providers.ContainsKey(providerName)) {
                throw new ArgumentException(SR.GetString(SR.Health_mon_provider_not_found, providerName));
            }

            using (new ApplicationImpersonationContext()) {
                providers[providerName].Flush();
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Flush() {
            HealthMonitoringSectionHelper.ProviderInstances providers = HealthMonitoringManager.ProviderInstances;

            if (providers == null) {
                return;
            }

            using (new ApplicationImpersonationContext()) {
                foreach(DictionaryEntry de in providers) {
                    WebEventProvider    provider = (WebEventProvider)de.Value;

                    Debug.Trace("WebEventManager", "Flushing provider " + provider.Name);
                    provider.Flush();
                }
            }
        }

        internal static void Shutdown() {
            HealthMonitoringSectionHelper.ProviderInstances   providers = HealthMonitoringManager.ProviderInstances;

            if (providers == null) {
                return;
            }

            foreach(DictionaryEntry de in providers) {
                WebEventProvider    provider = (WebEventProvider)de.Value;

                Debug.Trace("WebEventManager", "Shutting down provider " + provider.Name);
                provider.Shutdown();
            }
        }
    }

}

