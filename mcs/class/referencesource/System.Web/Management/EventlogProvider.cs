//------------------------------------------------------------------------------
// <copyright file="EventlogProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web.Util;
    using System.Globalization;
    using System.Collections;
    using System.Web.UI;
    using System.Security.Permissions;
    using System.Text;

    ////////////
    // Events
    ////////////

    public sealed class EventLogWebEventProvider : WebEventProvider, IInternalWebEventProvider {

        const int    EventLogParameterMaxLength = 30 * 1024 - 2;
        const string _truncateWarning = "...";
        int          _maxTruncatedParamLen;

        internal EventLogWebEventProvider() { }

        public override void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("WebEventLogEventProvider", "Initializing: name=" + name);

            _maxTruncatedParamLen = EventLogParameterMaxLength - _truncateWarning.Length;

            base.Initialize(name, config);

            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        void AddBasicDataFields(ArrayList dataFields, WebBaseEvent eventRaised) {
            WebApplicationInformation       appInfo = WebBaseEvent.ApplicationInformation;

            // Data contained in WebBaseEvent
            dataFields.Add(eventRaised.EventCode.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.Message);
            dataFields.Add(eventRaised.EventTime.ToString());
            dataFields.Add(eventRaised.EventTimeUtc.ToString());
            dataFields.Add(eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventSequence.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventOccurrence.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventDetailCode.ToString(CultureInfo.InstalledUICulture));

            dataFields.Add(appInfo.ApplicationDomain);
            dataFields.Add(appInfo.TrustLevel);
            dataFields.Add(appInfo.ApplicationVirtualPath);
            dataFields.Add(appInfo.ApplicationPath);
            dataFields.Add(appInfo.MachineName);

            if (eventRaised.IsSystemEvent) {
                dataFields.Add(null);   // custom event details
            }
            else {
                WebEventFormatter   formatter = new WebEventFormatter();
                eventRaised.FormatCustomEventDetails(formatter);
                dataFields.Add(formatter.ToString());
            }
        }

        void AddWebProcessInformationDataFields(ArrayList dataFields, WebProcessInformation     processEventInfo) {
            dataFields.Add(processEventInfo.ProcessID.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(processEventInfo.ProcessName);
            dataFields.Add(processEventInfo.AccountName);
        }

        void AddWebRequestInformationDataFields(ArrayList dataFields, WebRequestInformation reqInfo) {
            string      user;
            string      authType;
            bool        authed;
            IPrincipal  iprincipal = reqInfo.Principal;

            if (iprincipal == null) {
                user = null;
                authed = false;
                authType = null;
            }
            else {
                IIdentity    id = iprincipal.Identity;

                user = id.Name;
                authed = id.IsAuthenticated;
                authType = id.AuthenticationType;
            }

            dataFields.Add(HttpUtility.UrlDecode(reqInfo.RequestUrl));
            dataFields.Add(reqInfo.RequestPath);
            dataFields.Add(reqInfo.UserHostAddress);
            dataFields.Add(user);
            dataFields.Add(authed.ToString());
            dataFields.Add(authType);
            dataFields.Add(reqInfo.ThreadAccountName);
        }

        void AddWebProcessStatisticsDataFields(ArrayList dataFields, WebProcessStatistics procStats) {
            dataFields.Add(procStats.ProcessStartTime.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.ThreadCount.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.WorkingSet.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.AppDomainCount.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsExecuting.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsQueued.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsRejected.ToString(CultureInfo.InstalledUICulture));
        }

        private const int MAX_CHARS_IN_EXCEPTION_MSG = 8000;
        void AddExceptionDataFields(ArrayList dataFields, Exception exception) {
            if (exception == null) {
                dataFields.Add(null);
                dataFields.Add(null);
            }
            else {
                dataFields.Add(exception.GetType().Name);

                // Get the message and stack with all InnerExceptions
                StringBuilder sb = new StringBuilder(1024);
                for(int totalLength = 0; totalLength<MAX_CHARS_IN_EXCEPTION_MSG && exception != null; ) {

                    // Append message
                    // Dev10 720406: ReportEvent treats %N as an insertion string, so we can't include this in the message.
                    string msg =  ReplaceInsertionStringPlaceholders(exception.Message);
                    sb.Append(msg); // Always append full message (shouldn't be too big)

                    totalLength += msg.Length;
                    int remainingSpace = MAX_CHARS_IN_EXCEPTION_MSG - totalLength;

                    if (remainingSpace > 0) {
                        // Append stack if there is space
                        string stackTrace = exception.StackTrace;
                        if (!String.IsNullOrEmpty(stackTrace)) {
                            if (stackTrace.Length > remainingSpace) {
                                stackTrace = stackTrace.Substring(0, remainingSpace);
                            }
                            sb.Append("\n");
                            sb.Append(stackTrace);
                            totalLength += stackTrace.Length + 1;
                        }
                        sb.Append("\n\n");
                        totalLength += 2;
                    }

                    // deal with next InnerException in next iteration
                    exception = exception.InnerException;
                }

                dataFields.Add(sb.ToString());
             }
        }

        void AddWebThreadInformationDataFields(ArrayList dataFields, WebThreadInformation threadInfo) {
            dataFields.Add(threadInfo.ThreadID.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(threadInfo.ThreadAccountName);
            dataFields.Add(threadInfo.IsImpersonating.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(threadInfo.StackTrace);
        }

        void AddViewStateExceptionDataFields(ArrayList dataFields, ViewStateException vse) {
            dataFields.Add(SR.GetString(vse.ShortMessage));
            dataFields.Add(vse.RemoteAddress);
            dataFields.Add(vse.RemotePort);
            dataFields.Add(vse.UserAgent);
            dataFields.Add(vse.PersistedState);
            dataFields.Add(vse.Referer);
            dataFields.Add(vse.Path);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            Debug.Trace("EventLogWebEventProvider", "ProcessEvent: event=" + eventRaised.GetType().Name);

            int             hr;
            ArrayList       dataFields = new ArrayList(35);
            WebEventType    eventType = WebBaseEvent.WebEventTypeFromWebEvent(eventRaised);

            // !!! IMPORTANT note:
            // The order of fields added to dataFields MUST match that of the fields defined in the events
            // in msg.mc

            AddBasicDataFields(dataFields, eventRaised);

            if (eventRaised is WebManagementEvent) {
                AddWebProcessInformationDataFields(dataFields, ((WebManagementEvent)eventRaised).ProcessInformation);
            }

            if (eventRaised is WebHeartbeatEvent) {
                AddWebProcessStatisticsDataFields(dataFields, ((WebHeartbeatEvent)eventRaised).ProcessStatistics);
            }

            if (eventRaised is WebRequestEvent) {
                AddWebRequestInformationDataFields(dataFields, ((WebRequestEvent)eventRaised).RequestInformation);
            }

            if (eventRaised is WebBaseErrorEvent) {
                AddExceptionDataFields(dataFields, ((WebBaseErrorEvent)eventRaised).ErrorException);
            }

            if (eventRaised is WebAuditEvent) {
                AddWebRequestInformationDataFields(dataFields, ((WebAuditEvent)eventRaised).RequestInformation);
            }

            if (eventRaised is WebRequestErrorEvent) {
                AddWebRequestInformationDataFields(dataFields, ((WebRequestErrorEvent)eventRaised).RequestInformation);
                AddWebThreadInformationDataFields(dataFields, ((WebRequestErrorEvent)eventRaised).ThreadInformation);
            }

            if (eventRaised is WebErrorEvent) {
                AddWebRequestInformationDataFields(dataFields, ((WebErrorEvent)eventRaised).RequestInformation);
                AddWebThreadInformationDataFields(dataFields, ((WebErrorEvent)eventRaised).ThreadInformation);
            }

            if (eventRaised is WebAuthenticationSuccessAuditEvent) {
                dataFields.Add(((WebAuthenticationSuccessAuditEvent)eventRaised).NameToAuthenticate);
            }

            if (eventRaised is WebAuthenticationFailureAuditEvent) {
                dataFields.Add(((WebAuthenticationFailureAuditEvent)eventRaised).NameToAuthenticate);
            }

            if (eventRaised is WebViewStateFailureAuditEvent) {
                AddViewStateExceptionDataFields(dataFields, ((WebViewStateFailureAuditEvent)eventRaised).ViewStateException );
            }

            for (int i = 0; i < dataFields.Count; i++) {
                object field = dataFields[i];

                if (field == null) {
                    continue;
                }

                int len = ((string)field).Length;

                if (len > EventLogParameterMaxLength) {
                    // Truncate it and append a warning message to the end
                    dataFields[i] = ((string)field).Substring(0, _maxTruncatedParamLen) + _truncateWarning;
                }
            }

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            hr = UnsafeNativeMethods.RaiseEventlogEvent((int)eventType, (string[])dataFields.ToArray(typeof(string)), dataFields.Count);
            if (hr != 0) {
                throw new HttpException(SR.GetString(SR.Event_log_provider_error, "0x" + hr.ToString("X8", CultureInfo.InstalledUICulture)));
            }
#endif // !FEATURE_PAL
        }

        public override void Flush() {
        }

        public override void Shutdown() {
        }

        // Dev10 720406: ReportEvent treats %N as an insertion string, so we can't include this in the message.  
        // Instead, we will replace such occurrences with [%]N.
        private static string ReplaceInsertionStringPlaceholders(string s) {
            if (String.IsNullOrEmpty(s)) {
                return s;
            }
            int len = s.Length;
            int maxIndex = len - 1;
            int matches = 0;
            for (int i = 0; i < maxIndex; i++) {
                if (s[i] == '%' && Char.IsDigit(s[i+1])) {
                    matches++;
                } 
            }
            if (matches == 0) {
                return s;
            }
            char[] newChars = new char[len + (2 * matches)];
            int idx = 0;
            for (int i = 0; i < maxIndex; i++) {
                if (s[i] == '%' && Char.IsDigit(s[i+1])) {
                    newChars[idx++] = '[';
                    newChars[idx++] = '%';
                    newChars[idx++] = ']';
                } 
                else {
                    newChars[idx++] = s[i];
                }
            }
            newChars[newChars.Length-1] = s[maxIndex];
            return new String(newChars);
        }
    }

}

