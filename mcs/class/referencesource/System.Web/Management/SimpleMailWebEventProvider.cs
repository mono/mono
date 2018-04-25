//------------------------------------------------------------------------------
// <copyright file="SimpleMailWebEventProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Net.Mail;
    using System.Globalization;
    using System.Web.Configuration;
    using System.Text;
    using System.IO;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class SimpleMailWebEventProvider : MailWebEventProvider, IInternalWebEventProvider {
        const int   DefaultMaxEventLength = 8 * 1024;    
        
        const int   MessageIdDiscard = 100;
        const int   MessageIdEventsToDrop = 101;

        static string  s_header_warnings = SR.GetString(SR.MailWebEventProvider_Warnings);
        static string  s_header_summary = SR.GetString(SR.MailWebEventProvider_Summary);
        static string  s_header_app_info = SR.GetString(SR.MailWebEventProvider_Application_Info);
        static string  s_header_events = SR.GetString(SR.MailWebEventProvider_Events);
        
        string  _separator = "---------------\n";
        string  _bodyHeader;
        string  _bodyFooter;

        int     _maxEventLength = DefaultMaxEventLength;            // in no of chars
        int     _nonBufferNotificationSequence = 0;

        internal SimpleMailWebEventProvider() { }

        public override void Initialize(string name, NameValueCollection config)
        {
            string  temp = null;
            
            Debug.Trace("SimpleMailWebEventProvider", "Initializing: name=" + name);

            ProviderUtil.GetAndRemoveStringAttribute(config, "bodyHeader", name, ref _bodyHeader);
            if (_bodyHeader != null) {
                _bodyHeader += "\n";
            }
            
            ProviderUtil.GetAndRemoveStringAttribute(config, "bodyFooter", name, ref _bodyFooter);
            if (_bodyFooter != null) {
                _bodyFooter += "\n";
            }

            ProviderUtil.GetAndRemoveStringAttribute(config, "separator", name, ref temp);
            if (temp != null) {
                _separator = temp + "\n";
            }
            
            ProviderUtil.GetAndRemovePositiveOrInfiniteAttribute(config, "maxEventLength", name, ref _maxEventLength);

            base.Initialize(name, config);
        }

        void GenerateWarnings(StringBuilder sb, DateTime lastFlush, int discardedSinceLastFlush, 
                                int seq, int eventsToDrop) {
            if (!UseBuffering) {
                return;
            }

            bool    headerAdded = false;
            bool    hasWarnings = false;

            // This warning is issued only in the 1st message (vswhidbey 217578)
            if (discardedSinceLastFlush != 0 && seq == MessageSequenceBase) {
                sb.Append(s_header_warnings);
                sb.Append("\n");
                sb.Append(_separator);
                headerAdded = true;
                
                sb.Append(SR.GetString(SR.MailWebEventProvider_discard_warning,
                            MessageIdDiscard.ToString(CultureInfo.InstalledUICulture),
                            discardedSinceLastFlush.ToString(CultureInfo.InstalledUICulture),
                            lastFlush.ToString("r", CultureInfo.InstalledUICulture)));
                sb.Append("\n\n");
                hasWarnings = true;
            }

            if (eventsToDrop > 0) {
                if (!headerAdded) {
                    sb.Append(s_header_warnings);
                    sb.Append("\n");
                    sb.Append(_separator);
                    headerAdded = true;
                }
                
                sb.Append(SR.GetString(SR.MailWebEventProvider_events_drop_warning,
                    MessageIdEventsToDrop.ToString(CultureInfo.InstalledUICulture),
                    eventsToDrop.ToString(CultureInfo.InstalledUICulture)));
                sb.Append("\n\n");
                hasWarnings = true;
            }

            if (hasWarnings) {
                sb.Append("\n");
            }                
        }

        void GenerateApplicationInformation(StringBuilder sb) {
            sb.Append(s_header_app_info);
            sb.Append("\n");
            sb.Append(_separator);
            sb.Append(WebBaseEvent.ApplicationInformation.ToString());
            sb.Append("\n\n");
        }

        void GenerateSummary(StringBuilder sb, int firstEvent, int lastEvent, int eventsInNotif, int eventsInBuffer) {
            if (!UseBuffering) {
                return;
            }
            
            sb.Append(s_header_summary);
            sb.Append("\n");
            sb.Append(_separator);

            // The sequence numbers will be displayed as one-baesd.
            firstEvent++;
            lastEvent++;

            sb.Append(SR.GetString(SR.MailWebEventProvider_summary_body, 
                            firstEvent.ToString(CultureInfo.InstalledUICulture),
                            lastEvent.ToString(CultureInfo.InstalledUICulture),
                            eventsInNotif.ToString(CultureInfo.InstalledUICulture),
                            eventsInBuffer.ToString(CultureInfo.InstalledUICulture)));
            sb.Append("\n\n");
            sb.Append("\n");
        }
        
        string GenerateBody(WebBaseEventCollection events, 
                            int begin,
                            DateTime lastFlush,
                            int discardedSinceLastFlush, 
                            int eventsInBuffer,
                            int messageSequence, 
                            int eventsInNotification, 
                            int eventsLostDueToMessageLimit) {
            StringBuilder   sb = new StringBuilder();
            int             totalEvents = events.Count;

            if (_bodyHeader != null) {
                sb.Append(_bodyHeader);
            }

            // Warnings
            GenerateWarnings(sb, lastFlush, discardedSinceLastFlush, messageSequence, eventsLostDueToMessageLimit);

            // Event Summary
            GenerateSummary(sb, begin, begin + totalEvents - 1, eventsInNotification, eventsInBuffer);

            // Application Info
            Debug.Assert(events.Count > 0, "events.Count > 0");
            GenerateApplicationInformation(sb);

            // Please note that it's a text message, and thus we shouldn't need to HtmlEncode it.
            for (int i = 0; i < totalEvents; i++) {
                WebBaseEvent    eventRaised = events[i];
                string          details = eventRaised.ToString(false, true);
                
                if (_maxEventLength != ProviderUtil.Infinite &&
                    details.Length > _maxEventLength) {
                    details = details.Substring(0, _maxEventLength);
                }

                if (i == 0) {
                    sb.Append(s_header_events);
                    sb.Append("\n");
                    sb.Append(_separator);
                }

                sb.Append(details);
                sb.Append("\n");
                sb.Append(_separator);
            }

            if (_bodyFooter != null) {
                sb.Append(_bodyFooter);
            }

            return sb.ToString();
        }

        internal override void SendMessage(WebBaseEvent eventRaised) { 
            WebBaseEventCollection events = new WebBaseEventCollection(eventRaised);
            
            SendMessageInternal(
                events,             // events
                Interlocked.Increment(ref _nonBufferNotificationSequence), // notificationSequence
                0,                  // begin
                DateTime.MinValue,  // lastFlush
                0,                  // discardedSinceLastFlush
                0,                  // eventsInBuffer
                MessageSequenceBase,// messageSequence
                1,                  // messagesInNotification
                1,                  // eventsInNotification
                0);                 // eventsLostDueToMessageLimit
        }

        internal override void SendMessage(WebBaseEventCollection events,
                            WebEventBufferFlushInfo flushInfo,
                            int eventsInNotification, 
                            int eventsRemaining,
                            int messagesInNotification,
                            int eventsLostDueToMessageLimit,
                            int messageSequence,
                            int eventsSent,
                            out bool fatalError) {
                            
            SendMessageInternal(events, 
                flushInfo.NotificationSequence, 
                eventsSent, 
                flushInfo.LastNotificationUtc,
                flushInfo.EventsDiscardedSinceLastNotification, 
                flushInfo.EventsInBuffer, 
                messageSequence,
                messagesInNotification,
                eventsInNotification,
                eventsLostDueToMessageLimit);

            fatalError = false;
        }
                            

        void SendMessageInternal(WebBaseEventCollection events,
                            int notificationSequence,
                            int begin, 
                            DateTime lastFlush,
                            int discardedSinceLastFlush,
                            int eventsInBuffer,
                            int messageSequence, 
                            int messagesInNotification,
                            int eventsInNotification,
                            int eventsLostDueToMessageLimit) {
            using (MailMessage msg = GetMessage()) {
                // Don't report eventsLostDueToMessageLimit unless it's the last message in this notification
                if (messageSequence != messagesInNotification) {
                    eventsLostDueToMessageLimit = 0;
                }

                msg.Body = GenerateBody(events,
                                begin,
                                lastFlush,
                                discardedSinceLastFlush,
                                eventsInBuffer,
                                messageSequence,
                                eventsInNotification,
                                eventsLostDueToMessageLimit);

                msg.Subject = GenerateSubject(notificationSequence, messageSequence, events, events.Count);

                SendMail(msg);
            }
        }
        
    }
}

