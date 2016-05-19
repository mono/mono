//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Xml;

    static class MsmqDiagnostics
    {
        public static void CannotPeekOnQueue(string formatName, Exception ex)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqCannotPeekOnQueue,
                    SR.GetString(SR.TraceCodeMsmqCannotPeekOnQueue),
                    new StringTraceRecord("QueueFormatName", formatName),
                    null,
                    ex);
            }
        }

        public static void CannotReadQueues(string host, bool publicQueues, Exception ex)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                dictionary["Host"] = host;
                dictionary["PublicQueues"] = Convert.ToString(publicQueues, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqCannotReadQueues,
                    SR.GetString(SR.TraceCodeMsmqCannotReadQueues),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    ex);
            }
        }

        public static ServiceModelActivity StartListenAtActivity(MsmqReceiveHelper receiver)
        {
            ServiceModelActivity activity = receiver.Activity;
            if (DiagnosticUtility.ShouldUseActivity && null == activity)
            {
                activity = ServiceModelActivity.CreateActivity(true);
                if (null != FxTrace.Trace)
                {
                    FxTrace.Trace.TraceTransfer(activity.Id);
                }
                ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityListenAt, receiver.ListenUri.ToString()), ActivityType.ListenAt);
            }
            return activity;
        }

        public static Activity BoundOpenOperation(MsmqReceiveHelper receiver)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.TransportListen,
                    SR.GetString(SR.TraceCodeTransportListen, receiver.ListenUri.ToString()),
                    receiver);
            }
            return ServiceModelActivity.BoundOperation(receiver.Activity);
        }

        public static Activity BoundReceiveOperation(MsmqReceiveHelper receiver)
        {
            if (DiagnosticUtility.ShouldUseActivity && null != ServiceModelActivity.Current && ActivityType.ProcessAction != ServiceModelActivity.Current.ActivityType)
            {
                return ServiceModelActivity.BoundOperation(receiver.Activity);
            }
            else
            {
                return null;
            }
        }


        public static ServiceModelActivity BoundDecodeOperation()
        {
            ServiceModelActivity activity = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateBoundedActivity(true);
                ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
            }
            return activity;
        }

        public static ServiceModelActivity BoundReceiveBytesOperation()
        {
            ServiceModelActivity activity = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateBoundedActivityWithTransferInOnly(Guid.NewGuid());
                ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityReceiveBytes, TraceUtility.RetrieveMessageNumber()), ActivityType.ReceiveBytes);
            }
            return activity;
        }

        public static void TransferFromTransport(Message message)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.TransferFromTransport(message);
            }
        }

        public static void ExpectedException(Exception ex)
        {
            DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
        }

        public static void ScanStarted()
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Verbose,
                    TraceCode.MsmqScanStarted,
                    SR.GetString(SR.TraceCodeMsmqScanStarted),
                    null,
                    null,
                    null);
            }
        }

        public static void MatchedApplicationFound(string host, string queueName, bool isPrivate, string canonicalPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(4);
                dictionary["Host"] = host;
                dictionary["QueueName"] = queueName;
                dictionary["Private"] = Convert.ToString(isPrivate, CultureInfo.InvariantCulture);
                dictionary["CanonicalPath"] = canonicalPath;
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqMatchedApplicationFound,
                    SR.GetString(SR.TraceCodeMsmqMatchedApplicationFound),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }

        public static void StartingApplication(string application)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqStartingApplication,
                    SR.GetString(SR.TraceCodeMsmqStartingApplication),
                    new StringTraceRecord("Application", application),
                    null,
                    null);
            }
        }


        public static void StartingService(string host, string name, bool isPrivate, string processedVirtualPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(4);
                dictionary["Host"] = host;
                dictionary["Name"] = name;
                dictionary["Private"] = Convert.ToString(isPrivate, CultureInfo.InvariantCulture);
                dictionary["VirtualPath"] = processedVirtualPath;
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqStartingService,
                    SR.GetString(SR.TraceCodeMsmqStartingService),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }

        public static void FoundBaseAddress(Uri uri, string virtualPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2)
                {
                    { "Uri", uri.ToString() },
                    { "VirtualPath", virtualPath }
                };
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqFoundBaseAddress,
                    SR.GetString(SR.TraceCodeMsmqFoundBaseAddress),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }

        static void DatagramSentOrReceived(NativeMsmqMessage.BufferProperty messageId, Message message, int traceCode, string traceDescription)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Guid msmqId = MessageIdToGuid(messageId);
                UniqueId indigoId = message.Headers.MessageId;
                TraceRecord record = null;
                if (null == indigoId)
                {
                    record = new StringTraceRecord("MSMQMessageId", msmqId.ToString());
                }
                else
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(2)
                    {
                        { "MSMQMessageId", msmqId.ToString() },
                        { "WCFMessageId", indigoId.ToString() }
                    };
                    record = new DictionaryTraceRecord(dictionary);
                }
                TraceUtility.TraceEvent(TraceEventType.Verbose, traceCode, traceDescription, record, null, null);
            }
        }

        public static void DatagramReceived(NativeMsmqMessage.BufferProperty messageId, Message message)
        {
            DatagramSentOrReceived(messageId, message, TraceCode.MsmqDatagramReceived, SR.GetString(SR.TraceCodeMsmqDatagramReceived));
        }

        public static void DatagramSent(NativeMsmqMessage.BufferProperty messageId, Message message)
        {
            DatagramSentOrReceived(messageId, message, TraceCode.MsmqDatagramSent, SR.GetString(SR.TraceCodeMsmqDatagramSent));
        }

        static Guid MessageIdToGuid(NativeMsmqMessage.BufferProperty messageId)
        {
            if (UnsafeNativeMethods.PROPID_M_MSGID_SIZE != messageId.Buffer.Length)
                Fx.Assert(String.Format("Unexpected messageId size: {0}", messageId.Buffer.Length));

            byte[] buffer = new byte[16];
            Buffer.BlockCopy(messageId.Buffer, 4, buffer, 0, 16);
            return new Guid(buffer);
        }

        public static void MessageConsumed(string uri, string messageId, bool rejected)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    rejected ? TraceCode.MsmqMessageRejected : TraceCode.MsmqMessageDropped,
                    rejected ? SR.GetString(SR.TraceCodeMsmqMessageRejected) : SR.GetString(SR.TraceCodeMsmqMessageDropped),
                    new StringTraceRecord("MSMQMessageId", messageId),
                    null,
                    null);
            }

            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (rejected)
                {
                    PerformanceCounters.MsmqRejectedMessage(uri);
                }
                else
                {
                    PerformanceCounters.MsmqDroppedMessage(uri);
                }
            }
        }

        public static void MessageLockedUnderTheTransaction(long lookupId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqMessageLockedUnderTheTransaction,
                    SR.GetString(SR.TraceCodeMsmqMessageLockedUnderTheTransaction),
                    new StringTraceRecord("MSMQMessageLookupId", Convert.ToString(lookupId, CultureInfo.InvariantCulture)),
                    null,
                    null);
            }
        }

        public static void MoveOrDeleteAttemptFailed(long lookupId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqMoveOrDeleteAttemptFailed,
                    SR.GetString(SR.TraceCodeMsmqMoveOrDeleteAttemptFailed),
                    new StringTraceRecord("MSMQMessageLookupId", Convert.ToString(lookupId, CultureInfo.InvariantCulture)),
                    null,
                    null);
            }
        }

        public static void MsmqDetected(Version version)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqDetected,
                    SR.GetString(SR.TraceCodeMsmqDetected),
                    new StringTraceRecord("MSMQVersion", version.ToString()),
                    null,
                    null);
            }
        }

        public static void PoisonMessageMoved(string messageId, bool poisonQueue, string uri)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    poisonQueue ? TraceCode.MsmqPoisonMessageMovedPoison : TraceCode.MsmqPoisonMessageMovedRetry,
                    poisonQueue ? SR.GetString(SR.TraceCodeMsmqPoisonMessageMovedPoison) : SR.GetString(SR.TraceCodeMsmqPoisonMessageMovedRetry),
                    new StringTraceRecord("MSMQMessageId", messageId),
                    null,
                    null);
            }
            if (poisonQueue && PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MsmqPoisonMessage(uri);
            }
        }

        public static void PoisonMessageRejected(string messageId, string uri)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqPoisonMessageRejected,
                    SR.GetString(SR.TraceCodeMsmqPoisonMessageRejected),
                    new StringTraceRecord("MSMQMessageId", messageId),
                    null,
                    null);
            }
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MsmqPoisonMessage(uri);
            }
        }

        static bool poolFullReported = false;

        public static void PoolFull(int poolSize)
        {
            if (DiagnosticUtility.ShouldTraceInformation && !poolFullReported)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqPoolFull,
                    SR.GetString(SR.TraceCodeMsmqPoolFull),
                    null,
                    null,
                    null);

                poolFullReported = true;
            }
        }

        public static void PotentiallyPoisonMessageDetected(string messageId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqPotentiallyPoisonMessageDetected,
                    SR.GetString(SR.TraceCodeMsmqPotentiallyPoisonMessageDetected),
                    new StringTraceRecord("MSMQMessageId", messageId),
                    null,
                    null);
            }
        }

        public static void QueueClosed(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqQueueClosed,
                    SR.GetString(SR.TraceCodeMsmqQueueClosed),
                    new StringTraceRecord("FormatName", formatName),
                    null,
                    null);
            }
        }

        public static void QueueOpened(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MsmqQueueOpened,
                    SR.GetString(SR.TraceCodeMsmqQueueOpened),
                    new StringTraceRecord("FormatName", formatName),
                    null,
                    null);
            }
        }

        public static void QueueTransactionalStatusUnknown(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Warning,
                    TraceCode.MsmqQueueTransactionalStatusUnknown,
                    SR.GetString(SR.TraceCodeMsmqQueueTransactionalStatusUnknown),
                    new StringTraceRecord("FormatName", formatName),
                    null,
                    null);
            }
        }

        public static void SessiongramSent(string sessionId, NativeMsmqMessage.BufferProperty messageId, int numberOfMessages)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                dictionary["SessionId"] = sessionId;
                dictionary["MSMQMessageId"] = MsmqMessageId.ToString(messageId.Buffer);
                dictionary["NumberOfMessages"] = Convert.ToString(numberOfMessages, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(
                    TraceEventType.Verbose,
                    TraceCode.MsmqSessiongramSent,
                    SR.GetString(SR.TraceCodeMsmqSessiongramSent),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }

        public static void SessiongramReceived(string sessionId, NativeMsmqMessage.BufferProperty messageId, int numberOfMessages)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                dictionary["SessionId"] = sessionId;
                dictionary["MSMQMessageId"] = MsmqMessageId.ToString(messageId.Buffer);
                dictionary["NumberOfMessages"] = Convert.ToString(numberOfMessages, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(
                    TraceEventType.Verbose,
                    TraceCode.MsmqSessiongramReceived,
                    SR.GetString(SR.TraceCodeMsmqSessiongramReceived),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }

        public static void UnexpectedAcknowledgment(string messageId, int acknowledgment)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                dictionary["MSMQMessageId"] = messageId;
                dictionary["Acknowledgment"] = Convert.ToString(acknowledgment, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(
                    TraceEventType.Verbose,
                    TraceCode.MsmqUnexpectedAcknowledgment,
                    SR.GetString(SR.TraceCodeMsmqUnexpectedAcknowledgment),
                    new DictionaryTraceRecord(dictionary),
                    null,
                    null);
            }
        }
    }
}
