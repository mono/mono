//------------------------------------------------------------------------------
// <copyright file="events.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Security.Principal;
    using System.Configuration;
    using System.Text;
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Security.Permissions;

    ////////////
    // Events
    ////////////

    public class WmiWebEventProvider : WebEventProvider {
        
        public override void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("WmiWebEventProvider", "Initializing: name=" + name);

            int         hr;
            
            hr = UnsafeNativeMethods.InitializeWmiManager();
            if (hr != 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Wmi_provider_cant_initialize, "0x" + hr.ToString("X8", CultureInfo.CurrentCulture)));
            }
            
            base.Initialize(name, config);
            
            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        string WmiFormatTime(DateTime dt) {
            // CIM DATETIME has this format:
            // yyyymmddHHMMSS.mmmmmmsUUU
            // where    s = [+|-}
            //          UUU = Three-digit offset indicating the number of minutes that the 
            //                originating time zone deviates from UTC.

            StringBuilder sb = new StringBuilder(26);

            sb.Append(dt.ToString("yyyyMMddHHmmss.ffffff", CultureInfo.InstalledUICulture));
            double offset = TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalMinutes;
            if (offset >= 0) {
                sb.Append('+');
            }
            sb.Append(offset);

            return sb.ToString();
        }

        void FillBasicWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebBaseEvent eventRaised) {
            WebApplicationInformation       appInfo = WebBaseEvent.ApplicationInformation;
            
            wmiData.eventType = (int)WebBaseEvent.WebEventTypeFromWebEvent(eventRaised);

            // Note: WMI sint64 requires a string param
            
            // Data contained in WebBaseEvent
            wmiData.eventCode = eventRaised.EventCode;
            wmiData.eventDetailCode = eventRaised.EventDetailCode;
            wmiData.eventTime = WmiFormatTime(eventRaised.EventTime);
            wmiData.eventMessage = eventRaised.Message;
            wmiData.sequenceNumber = eventRaised.EventSequence.ToString(CultureInfo.InstalledUICulture);   
            wmiData.occurrence = eventRaised.EventOccurrence.ToString(CultureInfo.InstalledUICulture);   
            wmiData.eventId = eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture);   
        
            wmiData.appDomain = appInfo.ApplicationDomain;
            wmiData.trustLevel = appInfo.TrustLevel;
            wmiData.appVirtualPath = appInfo.ApplicationVirtualPath;
            wmiData.appPath = appInfo.ApplicationPath;
            wmiData.machineName = appInfo.MachineName;

            if (eventRaised.IsSystemEvent) {
                wmiData.details = String.Empty;
            }
            else {
                WebEventFormatter   formatter = new WebEventFormatter();
                eventRaised.FormatCustomEventDetails(formatter);
                wmiData.details = formatter.ToString();
            }
        }

        void FillRequestWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebRequestInformation reqInfo) {
            string      user;
            string      authType;
            bool        authed;
            IPrincipal  iprincipal = reqInfo.Principal;
            
            if (iprincipal == null) {
                user = String.Empty;
                authType = String.Empty;
                authed = false;
            }
            else {
                IIdentity    id = iprincipal.Identity;
            
                user = id.Name;
                authed = id.IsAuthenticated;
                authType = id.AuthenticationType;
            }
            
            wmiData.requestUrl = reqInfo.RequestUrl;
            wmiData.requestPath = reqInfo.RequestPath;
            wmiData.userHostAddress = reqInfo.UserHostAddress;
            wmiData.userName = user;
            wmiData.userAuthenticated = authed;
            wmiData.userAuthenticationType = authType;
            wmiData.requestThreadAccountName = reqInfo.ThreadAccountName;
        }

        void FillErrorWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebThreadInformation threadInfo) {
            wmiData.threadId = threadInfo.ThreadID;
            wmiData.threadAccountName = threadInfo.ThreadAccountName;
            wmiData.stackTrace = threadInfo.StackTrace;
            wmiData.isImpersonating = threadInfo.IsImpersonating;

        }
        
        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            Debug.Trace("WmiWebEventProvider", "ProcessEvent: event=" + eventRaised.GetType().Name);
            UnsafeNativeMethods.WmiData     wmiData = new UnsafeNativeMethods.WmiData();
            
            // Note: WMI sint64 requires a string param
            
            FillBasicWmiDataFields(ref wmiData, eventRaised);
            
            if (eventRaised is WebApplicationLifetimeEvent) {
                // Nothing special for this class.
            }

            if (eventRaised is WebManagementEvent) {
                WebProcessInformation       processEventInfo = ((WebManagementEvent)eventRaised).ProcessInformation;
                
                wmiData.processId = processEventInfo.ProcessID;
                wmiData.processName = processEventInfo.ProcessName;
                wmiData.accountName = processEventInfo.AccountName;
            }
            
            if (eventRaised is WebRequestEvent) {
                FillRequestWmiDataFields(ref wmiData, ((WebRequestEvent)eventRaised).RequestInformation);
            }
            
            if (eventRaised is WebAuditEvent) {
                FillRequestWmiDataFields(ref wmiData, ((WebAuditEvent)eventRaised).RequestInformation);
            }

            if (eventRaised is WebAuthenticationSuccessAuditEvent) {
                wmiData.nameToAuthenticate = ((WebAuthenticationSuccessAuditEvent)eventRaised).NameToAuthenticate;
            }
            
            if (eventRaised is WebAuthenticationFailureAuditEvent) {
                wmiData.nameToAuthenticate = ((WebAuthenticationFailureAuditEvent)eventRaised).NameToAuthenticate;
            }

            if (eventRaised is WebViewStateFailureAuditEvent) {
                ViewStateException  vse = ((WebViewStateFailureAuditEvent)eventRaised).ViewStateException;
                wmiData.exceptionMessage = SR.GetString(vse.ShortMessage);
                wmiData.remoteAddress = vse.RemoteAddress;
                wmiData.remotePort = vse.RemotePort;
                wmiData.userAgent = vse.UserAgent;
                wmiData.persistedState = vse.PersistedState;
                wmiData.referer = vse.Referer;
                wmiData.path = vse.Path;
            }
            
            if (eventRaised is WebHeartbeatEvent) {
#if DBG            
                try {
#endif                
                WebHeartbeatEvent       hbEvent = eventRaised as WebHeartbeatEvent;
                WebProcessStatistics    procStats = hbEvent.ProcessStatistics;

                wmiData.processStartTime = WmiFormatTime(procStats.ProcessStartTime);
                wmiData.threadCount = procStats.ThreadCount;
                wmiData.workingSet = procStats.WorkingSet.ToString(CultureInfo.InstalledUICulture);
                wmiData.peakWorkingSet = procStats.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture);
                wmiData.managedHeapSize = procStats.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture);
                wmiData.appdomainCount = procStats.AppDomainCount;
                wmiData.requestsExecuting = procStats.RequestsExecuting;
                wmiData.requestsQueued = procStats.RequestsQueued;
                wmiData.requestsRejected = procStats.RequestsRejected;
#if DBG            
                }
                catch (Exception e) {
                    Debug.Trace("WmiWebEventProvider", e.ToString());
                    throw;
                }
#endif                
            }

            if (eventRaised is WebBaseErrorEvent) {
                Exception   exception = ((WebBaseErrorEvent)eventRaised).ErrorException;
                if (exception == null) {
                    wmiData.exceptionType = String.Empty;
                    wmiData.exceptionMessage = String.Empty;
                }
                else {
                    wmiData.exceptionType = exception.GetType().Name;
                    wmiData.exceptionMessage = exception.Message;
                }
            }
            
            if (eventRaised is WebRequestErrorEvent) {
                WebRequestErrorEvent    reEvent = eventRaised as WebRequestErrorEvent;
                WebRequestInformation   reqInfo = reEvent.RequestInformation;
                WebThreadInformation    threadInfo = reEvent.ThreadInformation;

                FillRequestWmiDataFields(ref wmiData, reqInfo);
                FillErrorWmiDataFields(ref wmiData, threadInfo);
            }
            
            if (eventRaised is WebErrorEvent) {
                WebErrorEvent           eEvent = eventRaised as WebErrorEvent;
                WebRequestInformation   reqInfo = eEvent.RequestInformation;
                WebThreadInformation    threadInfo = eEvent.ThreadInformation;
            
                FillRequestWmiDataFields(ref wmiData, reqInfo);
                FillErrorWmiDataFields(ref wmiData, threadInfo);
            }

            int hr = UnsafeNativeMethods.RaiseWmiEvent(ref wmiData, AspCompatApplicationStep.IsInAspCompatMode);
            if (hr != 0) {
                throw new HttpException(SR.GetString(SR.Wmi_provider_error, "0x" + hr.ToString("X8", CultureInfo.InstalledUICulture)));
            }
            
        }
        

        public override void Flush() {
        }


        public override void Shutdown() {
        }
    }
}

