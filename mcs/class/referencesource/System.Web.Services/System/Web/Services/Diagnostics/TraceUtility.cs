
//------------------------------------------------------------------------------
// <copyright file="TraceUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Diagnostics {
    using System.Threading;
    using System.Net;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Xml.Serialization;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Collections.Generic;

    internal static class Tracing {
        private static bool tracingEnabled = true;
        private static bool tracingInitialized;
        private static bool appDomainShutdown;
        private const string TraceSourceAsmx = "System.Web.Services.Asmx";
        private static TraceSource asmxTraceSource;

        private static object internalSyncObject;
        private static object InternalSyncObject {
            get {
                if (internalSyncObject == null) {
                    object o = new Object();
                    Interlocked.CompareExchange(ref internalSyncObject, o, null);
                }
                return internalSyncObject;
            }
        }

        internal static bool On {
            get {
                if (!tracingInitialized) {
                    InitializeLogging();
                }
                return tracingEnabled;
            }
        }

        internal static bool IsVerbose {
            get {
                return ValidateSettings(Asmx, TraceEventType.Verbose);
            }
        }

        internal static TraceSource Asmx {
            get {
                if (!tracingInitialized)
                    InitializeLogging();

                if (!tracingEnabled)
                    return null;

                return asmxTraceSource;
            }
        }

        /// <devdoc>
        ///    <para>Sets up internal config settings for logging. (MUST be called under critsec) </para>
        /// </devdoc>
        private static void InitializeLogging() {
            lock (InternalSyncObject) {
                if (!tracingInitialized) {
                    bool loggingEnabled = false;
                    asmxTraceSource = new TraceSource(TraceSourceAsmx);
                    if (asmxTraceSource.Switch.ShouldTrace(TraceEventType.Critical)) {
                        loggingEnabled = true;
                        AppDomain currentDomain = AppDomain.CurrentDomain;
                        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
                        currentDomain.DomainUnload += new EventHandler(AppDomainUnloadEvent);
                        currentDomain.ProcessExit += new EventHandler(ProcessExitEvent);
                    }
                    tracingEnabled = loggingEnabled;
                    tracingInitialized = true;
                }
            }
        }

        private static void Close() {
            if (asmxTraceSource != null) {
                asmxTraceSource.Close();
            }
        }

        /// <devdoc>
        ///    <para>Logs any unhandled exception through this event handler</para>
        /// </devdoc>
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
            Exception e = (Exception)args.ExceptionObject;
            ExceptionCatch(TraceEventType.Error, sender, "UnhandledExceptionHandler", e);
        }

        private static void ProcessExitEvent(object sender, EventArgs e) {
            Close();
            appDomainShutdown = true;
        }
        /// <devdoc>
        ///    <para>Called when the system is shutting down, used to prevent additional logging post-shutdown</para>
        /// </devdoc>
        private static void AppDomainUnloadEvent(object sender, EventArgs e) {
            Close();
            appDomainShutdown = true;
        }


        /// <devdoc>
        ///    <para>Confirms logging is enabled, given current logging settings</para>
        /// </devdoc>
        private static bool ValidateSettings(TraceSource traceSource, TraceEventType traceLevel) {
            if (!tracingEnabled) {
                return false;
            }
            if (!tracingInitialized) {
                InitializeLogging();
            }
            if (traceSource == null || !traceSource.Switch.ShouldTrace(traceLevel)) {
                return false;
            }
            if (appDomainShutdown) {
                return false;
            }
            return true;
        }

        internal static void Information(string format, params object[] args) {
            if (!ValidateSettings(Asmx, TraceEventType.Information))
                return;

            TraceEvent(TraceEventType.Information, Res.GetString( format, args));
        }

        static void TraceEvent(TraceEventType eventType, string format) {
            Asmx.TraceEvent(eventType, 0, format);
        }

        internal static Exception ExceptionThrow(TraceMethod method, Exception e) {
            return ExceptionThrow(TraceEventType.Error, method, e);
        }

        internal static Exception ExceptionThrow(TraceEventType eventType, TraceMethod method, Exception e) {
            if (!ValidateSettings(Asmx, eventType))
                return e;

            TraceEvent(eventType, Res.GetString(Res.TraceExceptionThrown, method.ToString(), e.GetType(), e.Message));
            StackTrace(eventType, e);

            return e;
        }


        internal static Exception ExceptionCatch(TraceMethod method, Exception e) {
            return ExceptionCatch(TraceEventType.Error, method, e);
        }

        internal static Exception ExceptionCatch(TraceEventType eventType, TraceMethod method, Exception e) {
            if (!ValidateSettings(Asmx, eventType))
                return e;

            TraceEvent(eventType, Res.GetString(Res.TraceExceptionCought, method, e.GetType(), e.Message));
            StackTrace(eventType, e);

            return e;
        }

        internal static Exception ExceptionCatch(TraceEventType eventType, object target, string method, Exception e) {
            if (!ValidateSettings(Asmx, eventType))
                return e;

            TraceEvent(eventType, Res.GetString(Res.TraceExceptionCought, TraceMethod.MethodId(target, method), e.GetType(), e.Message));
            StackTrace(eventType, e);

            return e;
        }

        internal static Exception ExceptionIgnore(TraceEventType eventType, TraceMethod method, Exception e) {
            if (!ValidateSettings(Asmx, eventType))
                return e;

            TraceEvent(eventType, Res.GetString(Res.TraceExceptionIgnored, method, e.GetType(), e.Message));
            StackTrace(eventType, e);

            return e;
        }

        static void StackTrace(TraceEventType eventType, Exception e) {
            if (IsVerbose && !string.IsNullOrEmpty(e.StackTrace)) {
                TraceEvent(eventType, Res.GetString(Res.TraceExceptionDetails, e.ToString()));
            }
        }

        internal static string TraceId(string id) {
            return Res.GetString(id);
        }

        static string GetHostByAddress(string ipAddress) {
            try {
                return Dns.GetHostByAddress(ipAddress).HostName;
            }
            catch {
                return null;
            }
        }

        internal static List<string> Details(HttpRequest request) {
            if (request == null)
                return null;
            List<string> requestDetails = null;
            requestDetails = new List<string>();
            requestDetails.Add(Res.GetString(Res.TraceUserHostAddress, request.UserHostAddress));
            string hostName = request.UserHostAddress == request.UserHostName ? GetHostByAddress(request.UserHostAddress) : request.UserHostName;
            if (!string.IsNullOrEmpty(hostName))
                requestDetails.Add(Res.GetString(Res.TraceUserHostName, hostName));
            requestDetails.Add(Res.GetString(Res.TraceUrl, request.HttpMethod, request.Url));
            if (request.UrlReferrer != null)
                requestDetails.Add(Res.GetString(Res.TraceUrlReferrer, request.UrlReferrer));

            return requestDetails;
        }

        internal static void Enter(string callId, TraceMethod caller) {
            Enter(callId, caller, null, null);
        }

        internal static void Enter(string callId, TraceMethod caller, List<string> details) {
            Enter(callId, caller, null, details);
        }

        internal static void Enter(string callId, TraceMethod caller, TraceMethod callDetails) {
            Enter(callId, caller, callDetails, null);
        }

        internal static void Enter(string callId, TraceMethod caller, TraceMethod callDetails, List<string> details) {
            if (!ValidateSettings(Asmx, TraceEventType.Information))
                return;
            string trace = callDetails == null ? Res.GetString(Res.TraceCallEnter, callId, caller) : Res.GetString(Res.TraceCallEnterDetails, callId, caller, callDetails);
            if (details != null && details.Count > 0) {
                StringBuilder sb = new StringBuilder(trace);
                foreach (string detail in details) {
                    sb.Append(Environment.NewLine);
                    sb.Append("    ");
                    sb.Append(detail);
                }
                trace = sb.ToString();
            }
            TraceEvent(TraceEventType.Information, trace);
        }

        internal static XmlDeserializationEvents GetDeserializationEvents() {
            XmlDeserializationEvents events = new XmlDeserializationEvents();
            events.OnUnknownElement = new XmlElementEventHandler(OnUnknownElement);
            events.OnUnknownAttribute = new XmlAttributeEventHandler(OnUnknownAttribute);
            return events;
        }

        internal static void Exit(string callId, TraceMethod caller) {
            if (!ValidateSettings(Asmx, TraceEventType.Information))
                return;

            TraceEvent(TraceEventType.Information, Res.GetString(Res.TraceCallExit, callId, caller));
        }

        internal static void OnUnknownElement(object sender, XmlElementEventArgs e) {
            if (!ValidateSettings(Asmx, TraceEventType.Warning))
                return;
            if (e.Element == null)
                return;
            string xml = RuntimeUtils.ElementString(e.Element);
            string format = e.ExpectedElements == null ? Res.WebUnknownElement : e.ExpectedElements.Length == 0 ? Res.WebUnknownElement1 : Res.WebUnknownElement2;
            TraceEvent(TraceEventType.Warning, Res.GetString(format, xml, e.ExpectedElements));
        }

        internal static void OnUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            if (!ValidateSettings(Asmx, TraceEventType.Warning))
                return;
            if (e.Attr == null)
                return;
            // ignore attributes from known namepsaces
            if (RuntimeUtils.IsKnownNamespace(e.Attr.NamespaceURI))
                return;
            string format = e.ExpectedAttributes == null ? Res.WebUnknownAttribute : e.ExpectedAttributes.Length == 0 ? Res.WebUnknownAttribute2 : Res.WebUnknownAttribute3;
            TraceEvent(TraceEventType.Warning, Res.GetString(format, e.Attr.Name, e.Attr.Value, e.ExpectedAttributes));
        }
    }

    internal class TraceMethod {
        object target;
        string name;
        object[] args;
        string call;

        internal TraceMethod(object target, string name, params object[] args) {
            this.target = target;
            this.name = name;
            this.args = args;
        }

        public override string ToString() {
            if (call == null)
                call = CallString(this.target, this.name, this.args);
            return call;
        }

        internal static string CallString(object target, string method, params object[] args) {
            StringBuilder sb = new StringBuilder();
            WriteObjectId(sb, target);
            sb.Append(':');
            sb.Append(':');
            sb.Append(method);
            sb.Append('(');

            for (int i = 0; i < args.Length; i++) {
                object o = args[i];
                WriteObjectId(sb, o);
                if (o != null) {
                    sb.Append('=');
                    WriteValue(sb, o);
                }
                if (i + 1 < args.Length) {
                    sb.Append(',');
                    sb.Append(' ');
                }
            }
            sb.Append(')');

            return sb.ToString();
        }

        internal static string MethodId(object target, string method) {
            StringBuilder sb = new StringBuilder();
            WriteObjectId(sb, target);
            sb.Append(':');
            sb.Append(':');
            sb.Append(method);

            return sb.ToString();
        }

        static void WriteObjectId(StringBuilder sb, object o) {

            if (o == null) {
                sb.Append("(null)");
            }
            else if (o is Type) {
                Type type = (Type)o;
                sb.Append(type.FullName);
                if (!(type.IsAbstract && type.IsSealed)) {
                    sb.Append('#');
                    sb.Append(HashString(o));
                }
            }
            else {
                sb.Append(o.GetType().FullName);
                sb.Append('#');
                sb.Append(HashString(o));
            }
        }

        static void WriteValue(StringBuilder sb, object o) {
            if (o == null) {
                return;
            }

            if (o is string) {
                sb.Append('"');
                sb.Append(o);
                sb.Append('"');
            }
            else {
                Type type = o.GetType();
                if (type.IsArray) {
                    sb.Append('[');
                    sb.Append(((Array)o).Length);
                    sb.Append(']');
                }
                else {
                    string value = o.ToString();
                    if (type.FullName == value) {
                        sb.Append('.');
                        sb.Append('.');
                    }
                    else {
                        sb.Append(value);
                    }
                }
            }
        }

        static string HashString(object objectValue) {
            if (objectValue == null) {
                return "(null)";
            }
            else {
                return objectValue.GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
            }
        }
    }
}
