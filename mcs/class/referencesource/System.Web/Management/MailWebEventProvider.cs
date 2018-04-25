//------------------------------------------------------------------------------
// <copyright file="MailWebEventProvider.cs" company="Microsoft">
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

    /*
        The class MailWebEventProvider is supposed to be used internally.  But if I don't mark it public,
        I will get this compiler error CS0060 for TemplatedMailWebEventProvider and SimpleMailWebEventProvider
        because the two children classes are public and the base class accessibility has to be the same.
        The solution here is to introduce an internal constructor so user can't inherit from it.
    */
    
    public abstract class MailWebEventProvider  : BufferedWebEventProvider {
        internal const int   DefaultMaxMessagesPerNotification = 10;
        internal const int   DefaultMaxEventsPerMessage = 50;
        internal const int   MessageSequenceBase = 1;    // 1-based
        
        string  _from;
        string  _to;
        string  _cc;
        string  _bcc;
        string  _subjectPrefix;

        SmtpClient  _smtpClient;

        int     _maxMessagesPerNotification = DefaultMaxMessagesPerNotification;
        int     _maxEventsPerMessage = DefaultMaxEventsPerMessage;
        
        internal MailWebEventProvider() {}
        
        override public void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("MailWebEventProvider", "Initializing: name=" + name);

            ProviderUtil.GetAndRemoveRequiredNonEmptyStringAttribute(config, "from", name, ref _from);

            // Read "to", "cc" and "bcc"
            ProviderUtil.GetAndRemoveStringAttribute(config, "to", name, ref _to);
            ProviderUtil.GetAndRemoveStringAttribute(config, "cc", name, ref _cc);
            ProviderUtil.GetAndRemoveStringAttribute(config, "bcc", name, ref _bcc);

            if (String.IsNullOrEmpty(_to) &&
                String.IsNullOrEmpty(_cc) &&
                String.IsNullOrEmpty(_bcc) )
            {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.MailWebEventProvider_no_recipient_error, this.GetType().ToString(),
                    name));
            }

            ProviderUtil.GetAndRemoveStringAttribute(config, "subjectPrefix", name, ref _subjectPrefix);
            
            ProviderUtil.GetAndRemoveNonZeroPositiveOrInfiniteAttribute(config, "maxMessagesPerNotification", name, ref _maxMessagesPerNotification);

            ProviderUtil.GetAndRemoveNonZeroPositiveOrInfiniteAttribute(config, "maxEventsPerMessage", name, ref _maxEventsPerMessage);

            _smtpClient = CreateSmtpClientWithAssert();

            base.Initialize(name, config);
        }

        [SmtpPermission(SecurityAction.Assert, Access = "Connect")]
        [EnvironmentPermission(SecurityAction.Assert, Read = "USERNAME")]
        internal static SmtpClient CreateSmtpClientWithAssert() {
            return new SmtpClient();
        }

        internal string SubjectPrefix {
            get { return _subjectPrefix; }
        }

        internal string GenerateSubject(int notificationSequence, int messageSequence, WebBaseEventCollection events, int count) {
            WebBaseEvent    eventRaised = events[0];
            
            if (count == 1) {
                return HttpUtility.HtmlEncode(SR.GetString(SR.WebEvent_event_email_subject,
                        new string[] {
                            notificationSequence.ToString(CultureInfo.InstalledUICulture),
                            messageSequence.ToString(CultureInfo.InstalledUICulture),
                            _subjectPrefix, 
                            eventRaised.GetType().ToString(), 
                            WebBaseEvent.ApplicationInformation.ApplicationVirtualPath} 
                        ));
            }
            else {
                return HttpUtility.HtmlEncode(SR.GetString(SR.WebEvent_event_group_email_subject, 
                        new string[] {
                            notificationSequence.ToString(CultureInfo.InstalledUICulture),
                            messageSequence.ToString(CultureInfo.InstalledUICulture),
                            _subjectPrefix, 
                            count.ToString(CultureInfo.InstalledUICulture), 
                            WebBaseEvent.ApplicationInformation.ApplicationVirtualPath} 
                        ));
            }
        }

        internal MailMessage GetMessage() {
            MailMessage msg = new System.Net.Mail.MailMessage(_from, _to);

            if (!String.IsNullOrEmpty(_cc)) {
                msg.CC.Add(new MailAddress(_cc));
            }
            
            if (!String.IsNullOrEmpty(_bcc)) {
                msg.Bcc.Add(new MailAddress(_bcc));
            }
            
            return msg;
        }

        [SmtpPermission(SecurityAction.Assert, Access = "Connect")]
        internal void SendMail(MailMessage msg) {
            try {
                Debug.Trace("MailWebEventProvider", "Sending a message: subject=" + msg.Subject);
                _smtpClient.Send(msg);
            }
            catch (Exception e) {
                throw new HttpException(
                    SR.GetString(SR.MailWebEventProvider_cannot_send_mail),
                    e);
            }
        }

        internal abstract void SendMessage(WebBaseEvent eventRaised);
        
        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            Debug.Trace("MailWebEventProvider", "ProcessEvent: type =" + eventRaised.GetType() + 
                            ", ID=" + eventRaised.EventID + ", buffer=" + UseBuffering);
            if (UseBuffering) {
                base.ProcessEvent(eventRaised);
            }
            else {
                SendMessage(eventRaised);
            }
        }
        

        public override void Shutdown() {
            Flush();
        }

        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo) {
            int     eventsInNotification = flushInfo.Events.Count;
            int     eventsRemaining = eventsInNotification;
            bool    split = false;
            int     messageSequence = MessageSequenceBase;
            int     messagesInNotification;
            int     eventsToBeDiscarded = 0;
            bool    fatalError = false;
            
            if (eventsInNotification == 0) {
                return;
            }

            WebBaseEventCollection eventsToSend;
            WebBaseEvent[]  eventsChunk = null;

            // We will split based on MaxEventsPerMessage
            if (eventsInNotification > MaxEventsPerMessage) {
                split = true;

                // I use the below clumsy calculation instead of (a+b-1)/b is to avoid
                // Int32 overflow.
                messagesInNotification = eventsInNotification/MaxEventsPerMessage;
                if (eventsInNotification > messagesInNotification*MaxEventsPerMessage) {
                    messagesInNotification++;
                }

                // We will exceed the limit.
                if (messagesInNotification > MaxMessagesPerNotification) {
                    eventsToBeDiscarded = eventsInNotification - MaxMessagesPerNotification * MaxEventsPerMessage;
                    messagesInNotification = MaxMessagesPerNotification;
                    eventsInNotification -= eventsToBeDiscarded;
                }
            }
            else {
                messagesInNotification = 1;
            }

            // In each email we only send a max of MaxEventsPerMessage events
            for(int eventsSent = 0; 
                eventsSent < eventsInNotification;
                messageSequence++) {
                if (split) {
                    int chunkSize = Math.Min(MaxEventsPerMessage, eventsInNotification - eventsSent);

                    if (eventsChunk == null || eventsChunk.Length != chunkSize) {
                        eventsChunk = new WebBaseEvent[chunkSize];
                    }
                    
                    for(int i = 0; i < chunkSize; i++) {
                        eventsChunk[i] = flushInfo.Events[i + eventsSent];
                    }

                    eventsToSend = new WebBaseEventCollection(eventsChunk);
                }
                else {
                    eventsToSend = flushInfo.Events;
                }

                Debug.Trace("MailWebEventProvider", "Calling SendMessageInternal; # of events: " + eventsToSend.Count);


                SendMessage(
                    eventsToSend,
                    flushInfo,
                    eventsInNotification,
                    eventsInNotification - (eventsSent + eventsToSend.Count),  // eventsRemaining
                    messagesInNotification,
                    eventsToBeDiscarded,
                    messageSequence,
                    eventsSent,
                    out fatalError);

                if (fatalError) {
                    Debug.Trace("MailWebEventProvider", "Stop sending because we hit a fatal error");
                    break;
                }

                eventsSent += eventsToSend.Count;
            }
        }

        internal abstract void SendMessage(WebBaseEventCollection events, 
                            WebEventBufferFlushInfo flushInfo,
                            int eventsInNotification, 
                            int eventsRemaining,
                            int messagesInNotification,
                            int eventsLostDueToMessageLimit,
                            int messageSequence,
                            int eventsSent,
                            out bool fatalError);
        
        internal int MaxMessagesPerNotification {
            get { return _maxMessagesPerNotification; }
        }

        internal int MaxEventsPerMessage {
            get { return _maxEventsPerMessage; }
        }
    }
}

