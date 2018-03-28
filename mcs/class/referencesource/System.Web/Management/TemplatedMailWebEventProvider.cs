//------------------------------------------------------------------------------
// <copyright file="TemplatedMailWebEventProvider  .cs" company="Microsoft">
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

    public sealed class TemplatedMailWebEventProvider  : MailWebEventProvider, IInternalWebEventProvider   {
        int     _nonBufferNotificationSequence = 0;
        
        string  _templateUrl;
        bool    _detailedTemplateErrors = false;
        
        class TemplatedMailErrorFormatterGenerator : ErrorFormatterGenerator {
            int                     _eventsRemaining;
            bool                    _showDetails;
            bool                    _errorFormatterCalled;
            
            internal TemplatedMailErrorFormatterGenerator(int eventsRemaining, bool showDetails) {
                _eventsRemaining = eventsRemaining;
                _showDetails = showDetails;
            }

            internal bool ErrorFormatterCalled {
                get { return _errorFormatterCalled; }
            }

            internal override ErrorFormatter GetErrorFormatter(Exception e) {
                Exception inner = e.InnerException;

                _errorFormatterCalled = true;
                
                while (inner !=  null) {
                    if (inner is HttpCompileException) {
                        return new TemplatedMailCompileErrorFormatter((HttpCompileException)inner, _eventsRemaining, _showDetails);
                    }
                    else {
                        inner = inner.InnerException;
                    }
                }
                
                return new TemplatedMailRuntimeErrorFormatter(e, _eventsRemaining, _showDetails);
            }
        }

        internal TemplatedMailWebEventProvider() { }

        public override void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("TemplatedMailWebEventProvider", "Initializing: name=" + name);

            ProviderUtil.GetAndRemoveStringAttribute(config, "template", name, ref _templateUrl);

            if (_templateUrl == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Provider_missing_attribute, "template", name));
            }

            _templateUrl = _templateUrl.Trim();

            if (_templateUrl.Length == 0) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_provider_attribute, "template", name, _templateUrl));
            }
            
            if (!UrlPath.IsRelativeUrl(_templateUrl)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_mail_template_provider_attribute, 
                                "template", name, _templateUrl));
            }

            _templateUrl = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, _templateUrl);
            
            // VSWhidbey 440081: Guard against templates outside the AppDomain path
            if (!HttpRuntime.IsPathWithinAppRoot(_templateUrl)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_mail_template_provider_attribute,
                                "template", name, _templateUrl));
            }

            ProviderUtil.GetAndRemoveBooleanAttribute(config, "detailedTemplateErrors", name, ref _detailedTemplateErrors);
            
            base.Initialize(name, config);
        }

        void GenerateMessageBody(
                            MailMessage msg,
                            WebBaseEventCollection events, 
                            DateTime lastNotificationUtc,
                            int discardedSinceLastNotification, 
                            int eventsInBuffer,
                            int notificationSequence, 
                            EventNotificationType notificationType, 
                            int eventsInNotification, 
                            int eventsRemaining,
                            int messagesInNotification,
                            int eventsLostDueToMessageLimit,
                            int messageSequence,
                            out bool fatalError) {
                            
            StringWriter  writer = new StringWriter(CultureInfo.InstalledUICulture);
            
            MailEventNotificationInfo info = new MailEventNotificationInfo(
                                                    msg,
                                                    events, 
                                                    lastNotificationUtc, 
                                                    discardedSinceLastNotification, 
                                                    eventsInBuffer, 
                                                    notificationSequence,
                                                    notificationType,
                                                    eventsInNotification,
                                                    eventsRemaining,
                                                    messagesInNotification,
                                                    eventsLostDueToMessageLimit,
                                                    messageSequence);

            CallContext.SetData(CurrentEventsName, info);

            try {
                TemplatedMailErrorFormatterGenerator gen = new TemplatedMailErrorFormatterGenerator(events.Count + eventsRemaining, _detailedTemplateErrors);
                HttpServerUtility.ExecuteLocalRequestAndCaptureResponse(_templateUrl, writer, gen);

                fatalError = gen.ErrorFormatterCalled;

                if (fatalError) {
                    msg.Subject = HttpUtility.HtmlEncode(SR.GetString(SR.WebEvent_event_email_subject_template_error, 
                                                notificationSequence.ToString(CultureInfo.InstalledUICulture),
                                                messageSequence.ToString(CultureInfo.InstalledUICulture),
                                                SubjectPrefix));
                }

                msg.Body = writer.ToString();
                msg.IsBodyHtml = true;
            }
            finally {
                CallContext.FreeNamedDataSlot(CurrentEventsName);
            }
        }
        
        internal override void SendMessage(WebBaseEvent eventRaised) { 
            WebBaseEventCollection events = new WebBaseEventCollection(eventRaised);
            bool templateError;
            
            SendMessageInternal(events,                        // events
                        DateTime.MinValue,              // lastNotificationUtc
                        0,                              // discardedSinceLastNotification
                        0,                              // eventsInBuffer
                        Interlocked.Increment(ref _nonBufferNotificationSequence), // notificationSequence
                        EventNotificationType.Unbuffered,    // notificationType
                        1,                              // eventsInNotification
                        0,                              // eventsRemaining
                        1,                              // messagesInNotification
                        0,                              // eventsLostDueToMessageLimit
                        MessageSequenceBase,            // messageSequence
                        out templateError);             // templateError
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
                            flushInfo.LastNotificationUtc,
                            flushInfo.EventsDiscardedSinceLastNotification,
                            flushInfo.EventsInBuffer, 
                            flushInfo.NotificationSequence,
                            flushInfo.NotificationType,
                            eventsInNotification, 
                            eventsRemaining,
                            messagesInNotification,
                            eventsLostDueToMessageLimit,
                            messageSequence,
                            out fatalError);
        }

        void SendMessageInternal(WebBaseEventCollection events, 
                            DateTime lastNotificationUtc,
                            int discardedSinceLastNotification, 
                            int eventsInBuffer,
                            int notificationSequence, 
                            EventNotificationType notificationType, 
                            int eventsInNotification, 
                            int eventsRemaining,
                            int messagesInNotification,
                            int eventsLostDueToMessageLimit,
                            int messageSequence,
                            out bool fatalError) {

            using (MailMessage msg = GetMessage()) {

                msg.Subject = GenerateSubject(notificationSequence, messageSequence, events, events.Count);

                GenerateMessageBody(
                            msg,
                            events,
                            lastNotificationUtc,
                            discardedSinceLastNotification,
                            eventsInBuffer,
                            notificationSequence,
                            notificationType,
                            eventsInNotification,
                            eventsRemaining,
                            messagesInNotification,
                            eventsLostDueToMessageLimit,
                            messageSequence,
                            out fatalError);

                SendMail(msg);
            }
        }
        
        internal const string   CurrentEventsName = "_TWCurEvt";
        
        public static MailEventNotificationInfo  CurrentNotification   { 
            get {
                return (MailEventNotificationInfo)CallContext.GetData(CurrentEventsName);
            }
        }
    }

    public sealed class MailEventNotificationInfo {
        WebBaseEventCollection  _events;
        DateTime                _lastNotificationUtc;
        int                     _discardedSinceLastNotification;
        int                     _eventsInBuffer;
        int                     _notificationSequence;
        
        EventNotificationType   _notificationType;
        int                     _eventsInNotification;
        int                     _eventsRemaining;
        int                     _messagesInNotification;
        int                     _eventsLostDueToMessageLimit;
        int                     _messageSequence;
        MailMessage             _msg;
        
        internal MailEventNotificationInfo(
                            MailMessage msg,
                            WebBaseEventCollection events, 
                            DateTime lastNotificationUtc,
                            int discardedSinceLastNotification,
                            int eventsInBuffer,
                            int notificationSequence,
                            EventNotificationType notificationType,
                            int eventsInNotification,
                            int eventsRemaining,
                            int messagesInNotification,
                            int eventsLostDueToMessageLimit,
                            int messageSequence) {
            _events = events;
            _lastNotificationUtc = lastNotificationUtc;
            _discardedSinceLastNotification = discardedSinceLastNotification;
            _eventsInBuffer = eventsInBuffer;
            _notificationSequence = notificationSequence;

            _notificationType = notificationType;
            _eventsInNotification = eventsInNotification;
            _eventsRemaining = eventsRemaining  ;
            _messagesInNotification = messagesInNotification;
            _eventsLostDueToMessageLimit = eventsLostDueToMessageLimit;
            _messageSequence = messageSequence;
            _msg = msg;
        }

        public WebBaseEventCollection Events {
            get { return _events; }
        }

        public EventNotificationType NotificationType {
            get { return _notificationType; }
        }

        public int EventsInNotification {
            get { return _eventsInNotification; }
        }

        public int EventsRemaining {
            get { return _eventsRemaining; }
        }

        public int MessagesInNotification {
            get { return _messagesInNotification; }
        }

        public int EventsInBuffer {
            get { return _eventsInBuffer; }
        }

        public int EventsDiscardedByBuffer {
            get { return _discardedSinceLastNotification; }
        }

        public int EventsDiscardedDueToMessageLimit {
            get { return _eventsLostDueToMessageLimit; }
        }        

        public int NotificationSequence {
            get { return _notificationSequence; }
        }

        public int MessageSequence {
            get { return _messageSequence; }
        }

        public DateTime LastNotificationUtc   {
            get { return _lastNotificationUtc; }
        }

        public MailMessage Message {
            get { return _msg; }
        }
    }

    internal class TemplatedMailCompileErrorFormatter : DynamicCompileErrorFormatter {
        int     _eventsRemaining;
        bool    _showDetails;
    
        internal TemplatedMailCompileErrorFormatter(HttpCompileException e, int eventsRemaining,
                                                                    bool showDetails) :
            base(e) {
            _eventsRemaining = eventsRemaining;
            _showDetails = showDetails;

            _hideDetailedCompilerOutput = true;
            _dontShowVersion = true;
        }
    
        protected override string ErrorTitle {
            get {
                return SR.GetString(SR.MailWebEventProvider_template_compile_error, 
                                    _eventsRemaining.ToString(CultureInfo.InstalledUICulture));
            }
        }

        protected override string Description {
            get { 
                if (_showDetails) {
                    return base.Description;
                }
                else {
                    return SR.GetString(SR.MailWebEventProvider_template_error_no_details);
                }
            }
        }

        protected override string MiscSectionTitle {
            get {
                if (_showDetails) {
                    return base.MiscSectionTitle;
                }
                else {
                    return null;
                }
            }
        }
    
        protected override string MiscSectionContent {
            get { 
                if (_showDetails) {
                    return base.MiscSectionContent;
                }
                else {
                    return null;
                }
            }
        }
    }
    

    internal class TemplatedMailRuntimeErrorFormatter : UnhandledErrorFormatter {
        int     _eventsRemaining;
        bool    _showDetails;

        internal TemplatedMailRuntimeErrorFormatter(Exception e, int eventsRemaining,
                                                                    bool showDetails) :
            base(e) {
            _eventsRemaining = eventsRemaining;
            _showDetails = showDetails;

            _dontShowVersion = true;
        }

        protected override string ErrorTitle {
            get {
                if (HttpException.GetHttpCodeForException(Exception) == 404) {
                    return SR.GetString(SR.MailWebEventProvider_template_file_not_found_error, 
                                    _eventsRemaining.ToString(CultureInfo.InstalledUICulture));
                }
                else {
                    return SR.GetString(SR.MailWebEventProvider_template_runtime_error, 
                                    _eventsRemaining.ToString(CultureInfo.InstalledUICulture));
                }
            }
        }
    
        protected override string ColoredSquareTitle {
            get { return null;}
        }
    
        protected override string ColoredSquareContent {
            get { return null; }
        }

        protected override string Description {
            get { 
                if (_showDetails) {
                    return base.Description;
                }
                else {
                    return SR.GetString(SR.MailWebEventProvider_template_error_no_details);
                }
            }
        }

        protected override string MiscSectionTitle {
            get { 
                if (_showDetails) {
                    return base.MiscSectionTitle;
                }
                else {
                    return null;
                }
            }
        }

        protected override string MiscSectionContent {
            get { 
                if (_showDetails) {
                    return base.MiscSectionContent;
                }
                else {
                    return null;
                }
            }
        }

        protected override string ColoredSquare2Title {
            get { 
                if (_showDetails) {
                    return base.ColoredSquare2Title;
                }
                else {
                    return null;
                }
            }
        }
    
        protected override string ColoredSquare2Content {
            get { 
                if (_showDetails) {
                    return base.ColoredSquare2Content;
                }
                else {
                    return null;
                }
            }
        }
    }
}

