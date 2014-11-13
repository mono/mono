//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    [Flags]
    internal enum MessageLoggingSource : int
    {
        None = 0,
        TransportReceive = 2,
        TransportSend = 4,
        Transport = TransportReceive | TransportSend,
        ServiceLevelReceiveDatagram = 16,
        ServiceLevelSendDatagram = 32,
        ServiceLevelReceiveRequest = 64,
        ServiceLevelSendRequest = 128,
        ServiceLevelReceiveReply = 256,
        ServiceLevelSendReply = 512,
        ServiceLevelReceive = ServiceLevelReceiveReply | ServiceLevelReceiveRequest | ServiceLevelReceiveDatagram,
        ServiceLevelSend = ServiceLevelSendReply | ServiceLevelSendRequest | ServiceLevelSendDatagram,
        ServiceLevelService = ServiceLevelSendReply | ServiceLevelReceiveRequest | ServiceLevelReceiveDatagram,
        ServiceLevelProxy = ServiceLevelReceiveReply | ServiceLevelSendRequest | ServiceLevelSendDatagram,
        ServiceLevel = ServiceLevelReceive | ServiceLevelSend,
        Malformed = 1024,
        LastChance = 2048,
        All = int.MaxValue
    }

    internal static class MessageLogger
    {
        const string MessageTraceSourceName = "System.ServiceModel.MessageLogging";
        const string DefaultTraceListenerName = "Default";
        const int Unlimited = -1;

        static MessageLoggingSource sources = default(MessageLoggingSource);
        static bool logKnownPii;
        static bool logMessageBody = false;
        static int maxMessagesToLog;
        static int numberOfMessagesToLog;
        static int maxMessageSize;
        static PiiTraceSource messageTraceSource;
        static bool attemptedTraceSourceInitialization = false;
        static bool initialized = false;
        static bool initializing = false;
        static bool inPartialTrust = false;
        static object syncObject = new object();
        static object filterLock = new object();
        static List<XPathMessageFilter> messageFilterTable;
        static bool lastWriteSucceeded = true;
        static string[][] piiBodyPaths;
        static string[][] piiHeadersPaths;
        static string[] securityActions;

        static int FilterCount
        {
            get { return MessageLogger.Filters.Count; }
        }

        static bool FilterMessages
        {
            get { return MessageLogger.FilterCount > 0 && (MessageLogger.numberOfMessagesToLog > 0 || MessageLogger.numberOfMessagesToLog == MessageLogger.Unlimited); }
        }

        internal static bool LogKnownPii
        {
            get 
            {
                Fx.Assert(MessageLogger.initialized, "MessageLogger should be initialized before trying to retrieve LogKnownPii");
                return MessageLogger.logKnownPii; 
            }
            set
            {
                MessageLogger.logKnownPii = value;
            }
        }

        internal static bool LogMalformedMessages
        {
            get { return (MessageLogger.Sources & MessageLoggingSource.Malformed) != 0; }
            set
            {
                lock (MessageLogger.syncObject)
                {
                    bool shouldProcessAudit = ShouldProcessAudit(MessageLoggingSource.Malformed, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!MessageLogger.inPartialTrust)
                        {
                            MessageLogger.sources |= MessageLoggingSource.Malformed;
                        }
                    }
                    else
                    {
                        MessageLogger.sources &= MessageLoggingSource.All & ~MessageLoggingSource.Malformed;
                    }
                    if (shouldProcessAudit)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static bool LogMessagesAtServiceLevel
        {
            get { return (MessageLogger.Sources & MessageLoggingSource.ServiceLevel) != 0; }
            set
            {
                lock (MessageLogger.syncObject)
                {
                    bool shouldProcessAudit = ShouldProcessAudit(MessageLoggingSource.ServiceLevel, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!MessageLogger.inPartialTrust)
                        {
                            MessageLogger.sources |= MessageLoggingSource.ServiceLevel;
                        }
                    }
                    else
                    {
                        MessageLogger.sources &= MessageLoggingSource.All & ~MessageLoggingSource.ServiceLevel;
                    }
                    if (shouldProcessAudit)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static bool LogMessagesAtTransportLevel
        {
            get { return (MessageLogger.Sources & MessageLoggingSource.Transport) != 0; }
            set
            {
                lock (MessageLogger.syncObject)
                {
                    bool shouldProcessAudit = ShouldProcessAudit(MessageLoggingSource.Transport, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!MessageLogger.inPartialTrust)
                        {
                            MessageLogger.sources |= MessageLoggingSource.Transport;
                        }
                    }
                    else
                    {
                        MessageLogger.sources &= MessageLoggingSource.All & ~MessageLoggingSource.Transport;
                    }
                    if (shouldProcessAudit)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static bool LogMessageBody
        {
            get
            {
                Fx.Assert(MessageLogger.initialized, "");
                return logMessageBody;
            }
            set { logMessageBody = value; }
        }

        internal static bool LoggingEnabled
        {
            get { return MessageLogger.Sources != default(MessageLoggingSource); }
        }

        internal static int MaxMessageSize
        {
            get
            {
                Fx.Assert(MessageLogger.initialized, "");
                return maxMessageSize;
            }
            set { maxMessageSize = value; }
        }

        internal static int MaxNumberOfMessagesToLog
        {
            get
            {
                Fx.Assert(MessageLogger.initialized, "");
                return maxMessagesToLog;
            }
            set
            {
                //resetting the max resets the actual counter
                lock (MessageLogger.syncObject)
                {
                    maxMessagesToLog = value;
                    MessageLogger.numberOfMessagesToLog = maxMessagesToLog;
                }
            }
        }

        static List<XPathMessageFilter> Filters
        {
            get
            {
                if (MessageLogger.messageFilterTable == null)
                {
                    lock (MessageLogger.filterLock)
                    {
                        if (MessageLogger.messageFilterTable == null)
                        {
                            List<XPathMessageFilter> temp = new List<XPathMessageFilter>();
                            MessageLogger.messageFilterTable = temp;
                        }
                    }
                }

                return MessageLogger.messageFilterTable;
            }
        }

        static MessageLoggingSource Sources
        {
            get
            {
                if (!MessageLogger.initialized)
                {
                    MessageLogger.EnsureInitialized();
                }
                return MessageLogger.sources;
            }
        }

        static bool AddFilter(XPathMessageFilter filter)
        {
            if (filter == null)
            {
                filter = new XPathMessageFilter(""); //if there is an empty add filter tag, add a match-all filter
            }

            MessageLogger.Filters.Add(filter);
            return true;
        }

        internal static bool ShouldLogMalformed
        {
            get { return ShouldLogMessages(MessageLoggingSource.Malformed); }
        }

        static bool ShouldLogMessages(MessageLoggingSource source)
        {
            return (source & MessageLogger.Sources) != 0 &&
                   ((MessageLogger.MessageTraceSource != null) ||
                   ((source & MessageLoggingSource.Malformed) != 0 && TD.MessageLogWarningIsEnabled()) ||
                   TD.MessageLogInfoIsEnabled());
        }

        internal static void LogMessage(MessageLoggingSource source, string data)
        {
            try
            {
                if (ShouldLogMessages(MessageLoggingSource.Malformed))
                {
                    LogInternal(source, data);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                FailedToLogMessage(e);
            }
        }

        internal static void LogMessage(Stream stream, MessageLoggingSource source)
        {
            try
            {
                ThrowIfNotMalformed(source);
                if (ShouldLogMessages(source))
                {
                    LogInternal(new MessageLogTraceRecord(stream, source));
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                FailedToLogMessage(e);
            }
        }

        internal static void LogMessage(ArraySegment<byte> buffer, MessageLoggingSource source)
        {
            try
            {
                ThrowIfNotMalformed(source);
                if (ShouldLogMessages(source))
                {
                    LogInternal(new MessageLogTraceRecord(buffer, source));
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                FailedToLogMessage(e);
            }
        }

        internal static void LogMessage(ref Message message, XmlReader reader, MessageLoggingSource source)
        {
            Fx.Assert(null != message, "");
            try
            {
                if (ShouldLogMessages(source))
                {
                    LogMessageImpl(ref message, reader, source);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                FailedToLogMessage(e);
            }
        }

        internal static void LogMessage(ref Message message, MessageLoggingSource source)
        {
            LogMessage(ref message, null, source);
        }

        static void LogMessageImpl(ref Message message, XmlReader reader, MessageLoggingSource source)
        {
            ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(message) : null;
            using (ServiceModelActivity.BoundOperation(activity))
            {
                if (ShouldLogMessages(source) && (MessageLogger.numberOfMessagesToLog > 0 || MessageLogger.numberOfMessagesToLog == MessageLogger.Unlimited))
                {
                    bool lastChance = (source & MessageLoggingSource.LastChance) != 0 || (source & MessageLoggingSource.TransportSend) != 0;
                    source &= ~MessageLoggingSource.LastChance;
                    // MessageLogger doesn't log AddressingVersion.None in the encoder since we want to make sure we log 
                    // as much of the message as possible. So let the Transport log later.
                    if ((lastChance || message is NullMessage || message.Version.Addressing != AddressingVersion.None)
                            && MatchFilters(message, source))
                    {
                        if (MessageLogger.numberOfMessagesToLog == MessageLogger.Unlimited || MessageLogger.numberOfMessagesToLog > 0)
                        {
                            MessageLogTraceRecord record = new MessageLogTraceRecord(ref message, reader, source, MessageLogger.LogMessageBody);
                            LogInternal(record);
                        }
                    }
                }
            }
        }

        static bool HasSecurityAction(Message message)
        {
            Fx.Assert(null != message, "");

            string action = message.Headers.Action;
            bool result = false;
            if (String.IsNullOrEmpty(action))
            {
                result = true;
            }
            else
            {
                foreach (string securityAction in MessageLogger.SecurityActions)
                {
                    if (0 == String.CompareOrdinal(action, securityAction))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        static void LogInternal(MessageLogTraceRecord record)
        {
            Fx.Assert(null != record, "record cannot be null");

            PlainXmlWriter xmlWriter = new PlainXmlWriter(MessageLogger.MaxMessageSize);

            try
            {
                record.WriteTo(xmlWriter);
                xmlWriter.Close();
                TraceXPathNavigator navigator = xmlWriter.Navigator;
                
                if ((MessageLogger.messageTraceSource != null && 
                    !MessageLogger.messageTraceSource.ShouldLogPii) ||
                    !MessageLogger.LogKnownPii)
                {
                    navigator.RemovePii(MessageLogger.PiiHeadersPaths);
                    if (MessageLogger.LogMessageBody && null != record.Message)
                    {
                        if (HasSecurityAction(record.Message))
                        {
                            navigator.RemovePii(MessageLogger.PiiBodyPaths);
                        }
                    }
                }

                LogInternal(record.MessageLoggingSource, navigator);
            }
            catch (PlainXmlWriter.MaxSizeExceededException)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.MessageNotLoggedQuotaExceeded,
                        SR.GetString(SR.TraceCodeMessageNotLoggedQuotaExceeded), record.Message);
                }
            }
        }

        static void IncrementLoggedMessagesCount(object data)
        {
            if (MessageLogger.numberOfMessagesToLog > 0)
            {
                lock (MessageLogger.syncObject)
                {
                    if (MessageLogger.numberOfMessagesToLog > 0)
                    {
                        MessageLogger.numberOfMessagesToLog--;
                        if (0 == MessageLogger.numberOfMessagesToLog)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageCountLimitExceeded,
                                    SR.GetString(SR.TraceCodeMessageCountLimitExceeded), data);
                            }
                        }
                    }
                }
            }

            lock (MessageLogger.syncObject)
            {
                if (!MessageLogger.lastWriteSucceeded)
                {
                    MessageLogger.lastWriteSucceeded = true;
                }
            }
        }

        static void FailedToLogMessage(Exception e)
        {
            //If something goes wrong, we do not want to fail the app, just log one event log entry per block of failures
            bool shouldLogError = false;
            lock (MessageLogger.syncObject)
            {
                if (MessageLogger.lastWriteSucceeded)
                {
                    MessageLogger.lastWriteSucceeded = false;
                    shouldLogError = true;
                }
            }
            if (shouldLogError)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToLogMessage,
                    e.ToString());
            }
        }

        static void LogInternal(MessageLoggingSource source, object data)
        {
            if ((source & MessageLoggingSource.Malformed) != 0)
            {
                if (!TD.MessageLogWarning(data.ToString()))
                {
                    if (TD.MessageLogEventSizeExceededIsEnabled())
                    {
                        TD.MessageLogEventSizeExceeded();
                    }
                }
            }
            else
            {
                if (!TD.MessageLogInfo(data.ToString()))
                {
                    if (TD.MessageLogEventSizeExceededIsEnabled())
                    {
                        TD.MessageLogEventSizeExceeded();
                    }                   
                }
            }
            if (MessageLogger.MessageTraceSource != null)
            {
                MessageLogger.MessageTraceSource.TraceData(TraceEventType.Information, 0, data);
            }
            IncrementLoggedMessagesCount(data);
        }

        static bool MatchFilters(Message message, MessageLoggingSource source)
        {
            bool result = true;
            if (MessageLogger.FilterMessages && (source & MessageLoggingSource.Malformed) == 0)
            {
                result = false;
                List<XPathMessageFilter> filtersToRemove = new List<XPathMessageFilter>();
                lock (MessageLogger.syncObject)
                {
                    foreach (XPathMessageFilter filter in MessageLogger.Filters)
                    {
                        try
                        {
                            if (filter.Match(message))
                            {
                                result = true;
                                break;
                            }
                        }
                        catch (FilterInvalidBodyAccessException)
                        {
                            filtersToRemove.Add(filter);
                        }
                        catch (MessageFilterException e)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.FilterNotMatchedNodeQuotaExceeded,
                                    SR.GetString(SR.TraceCodeFilterNotMatchedNodeQuotaExceeded), e, message);
                            }
                        }
                    }

                    foreach (XPathMessageFilter filter in filtersToRemove)
                    {
                        MessageLogger.Filters.Remove(filter);
                        PlainXmlWriter writer = new PlainXmlWriter();
                        filter.WriteXPathTo(writer, null, ConfigurationStrings.Filter, null, true);
                        DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                            (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                            (uint)System.Runtime.Diagnostics.EventLogEventId.RemovedBadFilter,
                            writer.Navigator.ToString());
                    }

                    if (MessageLogger.FilterCount == 0)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        static void ReadFiltersFromConfig(DiagnosticSection section)
        {
            for (int i = 0; i < section.MessageLogging.Filters.Count; i++)
            {
                XPathMessageFilterElement xfe = section.MessageLogging.Filters[i];
                AddFilter(xfe.Filter);
            }
        }

        internal static TraceSource MessageTraceSource
        {
            get
            {
                return MessageLogger.messageTraceSource;
            }
        }

        internal static void EnsureInitialized()
        {
            lock (MessageLogger.syncObject)
            {
                if (!MessageLogger.initialized && !MessageLogger.initializing)
                {
                    try
                    {
                        Initialize();
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (SecurityException securityException)
                    {
                        // message logging is not support in PT, write the trace
                        MessageLogger.inPartialTrust = true;
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning,
                                TraceCode.TraceHandledException,
                                SR.GetString(SR.PartialTrustMessageLoggingNotEnabled),
                                null,
                                securityException);
                        }
                        // also write to event log
                        LogNonFatalInitializationException(
                            new SecurityException(
                                SR.GetString(SR.PartialTrustMessageLoggingNotEnabled),
                                securityException));
                        
                    }
                    MessageLogger.initialized = true;
                }
            }
        }

        static void EnsureMessageTraceSource()
        {
            if (!MessageLogger.initialized)
            {
                MessageLogger.EnsureInitialized();
            }
            if (null == MessageLogger.MessageTraceSource && !MessageLogger.attemptedTraceSourceInitialization)
            {
                InitializeMessageTraceSource();
            }
        }

        static string[][] PiiBodyPaths
        {
            get
            {
                if (piiBodyPaths == null)
                    piiBodyPaths = new string[][] { 
                                new string[] { MessageLogTraceRecord.MessageLogTraceRecordElementName, "Envelope", "Body", "RequestSecurityToken" },
                                new string[] { MessageLogTraceRecord.MessageLogTraceRecordElementName, "Envelope", "Body", "RequestSecurityTokenResponse" },
                                new string[] { MessageLogTraceRecord.MessageLogTraceRecordElementName, "Envelope", "Body", "RequestSecurityTokenResponseCollection" }
                            };
                return piiBodyPaths;
            }
        }

        static string[][] PiiHeadersPaths
        {
            get
            {
                if (piiHeadersPaths == null)
                    piiHeadersPaths = new string[][] { 
                                new string[] { MessageLogTraceRecord.MessageLogTraceRecordElementName, "Envelope", "Header", "Security" },
                                new string[] { MessageLogTraceRecord.MessageLogTraceRecordElementName, "Envelope", "Header", "IssuedTokens" }
                            };
                return piiHeadersPaths;
            }
        }

        static string[] SecurityActions
        {
            get
            {
                if (securityActions == null)
                    securityActions = new string[] { 
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue", 
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Renew",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Renew",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Cancel",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Cancel",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Amend",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Amend",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Renew",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Renew",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Cancel",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Cancel",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/KET",
                                      "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/KET",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT-Amend",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT-Amend",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Issue",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Issue",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Renew",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Renew",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Validate",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Validate",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/KET",
                                      "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/KET"
                                    };
                return securityActions;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method UnsafeGetSection which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        static void Initialize()
        {
            MessageLogger.initializing = true;
            DiagnosticSection section = DiagnosticSection.UnsafeGetSection();

            if (section != null)
            {
                MessageLogger.LogKnownPii = section.MessageLogging.LogKnownPii && MachineSettingsSection.EnableLoggingKnownPii;
                MessageLogger.LogMalformedMessages = section.MessageLogging.LogMalformedMessages;
                MessageLogger.LogMessageBody = section.MessageLogging.LogEntireMessage;
                MessageLogger.LogMessagesAtServiceLevel = section.MessageLogging.LogMessagesAtServiceLevel;
                MessageLogger.LogMessagesAtTransportLevel = section.MessageLogging.LogMessagesAtTransportLevel;
                MessageLogger.MaxNumberOfMessagesToLog = section.MessageLogging.MaxMessagesToLog;
                MessageLogger.MaxMessageSize = section.MessageLogging.MaxSizeOfMessageToLog;

                ReadFiltersFromConfig(section);
            }
        }

        static void InitializeMessageTraceSource()
        {
            try
            {
                MessageLogger.attemptedTraceSourceInitialization = true;
                PiiTraceSource tempSource = new PiiTraceSource(MessageLogger.MessageTraceSourceName, DiagnosticUtility.EventSourceName);
                tempSource.Switch.Level = SourceLevels.Information;
                tempSource.Listeners.Remove(MessageLogger.DefaultTraceListenerName);
                if (tempSource.Listeners.Count > 0)
                {
                    AppDomain.CurrentDomain.DomainUnload += new EventHandler(ExitOrUnloadEventHandler);
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitOrUnloadEventHandler);
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExitOrUnloadEventHandler);
                }
                else
                {
                    tempSource = null;
                }

                MessageLogger.messageTraceSource = tempSource;
            }
            catch (System.Configuration.ConfigurationErrorsException)
            {
                throw;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (SecurityException securityException)
            {
                // message logging is not support in PT, write the trace
                MessageLogger.inPartialTrust = true;
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.TraceHandledException,
                        SR.GetString(SR.PartialTrustMessageLoggingNotEnabled),
                        null,
                        securityException);
                }
                // also write to event log
                LogNonFatalInitializationException(
                    new SecurityException(
                        SR.GetString(SR.PartialTrustMessageLoggingNotEnabled),
                        securityException));
            }
            catch (Exception e)
            {
                MessageLogger.messageTraceSource = null;

                if (Fx.IsFatal(e)) throw;

                LogNonFatalInitializationException(e);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into an unsafe method to log event.",
            Safe = "Event identities cannot be spoofed as they are constants determined inside the method.")]
        [SecuritySafeCritical]
        static void LogNonFatalInitializationException(Exception e)
        {
            DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Critical,
                (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToCreateMessageLoggingTraceSource,
                true,
                e.ToString());
        }

        static void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            lock (MessageLogger.syncObject)
            {
                if (null != MessageLogger.MessageTraceSource)
                {
                    //Flush is called automatically on close by StreamWriter
                    MessageLogger.MessageTraceSource.Close();
                    MessageLogger.messageTraceSource = null;
                }
            }
        }

        static void ThrowIfNotMalformed(MessageLoggingSource source)
        {
            if ((source & MessageLoggingSource.Malformed) == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.OnlyMalformedMessagesAreSupported), "source"));
            }
        }

        static void ProcessAudit(bool turningOn)
        {
            if (turningOn)
            {
                if (null != MessageLogger.messageTraceSource)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information,
                        (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.MessageLoggingOn);
                }
            }
            else
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.MessageLoggingOff);
            }
        }

        static bool ShouldProcessAudit(MessageLoggingSource source, bool turningOn)
        {
            bool result = false;
            if (turningOn)
            {
                result = MessageLogger.sources == MessageLoggingSource.None;
            }
            else
            {
                result = MessageLogger.sources == source;
            }

            return result;
        }
    }
}

