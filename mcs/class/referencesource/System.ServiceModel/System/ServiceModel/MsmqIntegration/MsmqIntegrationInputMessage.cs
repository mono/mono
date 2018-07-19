//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ServiceModel.Channels;

    class MsmqIntegrationInputMessage : MsmqInputMessage
    {
        ByteProperty acknowledge;
        StringProperty adminQueue;
        IntProperty adminQueueLength;
        IntProperty appSpecific;
        IntProperty arrivedTime;
        IntProperty senderIdType;
        ByteProperty authenticated;
        IntProperty bodyType;
        BufferProperty correlationId;
        StringProperty destinationQueue;
        IntProperty destinationQueueLength;
        BufferProperty extension;
        IntProperty extensionLength;
        StringProperty label;
        IntProperty labelLength;
        ByteProperty priority;
        StringProperty responseFormatName;
        IntProperty responseFormatNameLength;
        IntProperty sentTime;
        IntProperty timeToReachQueue;
        IntProperty privacyLevel;
        const int initialQueueNameLength = 256;
        const int initialExtensionLength = 0;
        const int initialLabelLength = 128;
        const int maxSize = 4 * 1024 * 1024;

        public MsmqIntegrationInputMessage()
            : this(maxSize)
        { }

        public MsmqIntegrationInputMessage(int maxBufferSize)
            : this(new SizeQuota(maxBufferSize))
        { }

        protected MsmqIntegrationInputMessage(SizeQuota bufferSizeQuota)
            : base(22, bufferSizeQuota)
        {
            this.acknowledge = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_ACKNOWLEDGE);
            this.adminQueue = new StringProperty(this, UnsafeNativeMethods.PROPID_M_ADMIN_QUEUE, initialQueueNameLength);
            this.adminQueueLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_ADMIN_QUEUE_LEN, initialQueueNameLength);
            this.appSpecific = new IntProperty(this, UnsafeNativeMethods.PROPID_M_APPSPECIFIC);
            this.arrivedTime = new IntProperty(this, UnsafeNativeMethods.PROPID_M_ARRIVEDTIME);
            this.senderIdType = new IntProperty(this, UnsafeNativeMethods.PROPID_M_SENDERID_TYPE);
            this.authenticated = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_AUTHENTICATED);
            this.bodyType = new IntProperty(this, UnsafeNativeMethods.PROPID_M_BODY_TYPE);
            this.correlationId = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_CORRELATIONID,
                                                    UnsafeNativeMethods.PROPID_M_CORRELATIONID_SIZE);
            this.destinationQueue = new StringProperty(this, UnsafeNativeMethods.PROPID_M_DEST_FORMAT_NAME, initialQueueNameLength);
            this.destinationQueueLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_DEST_FORMAT_NAME_LEN, initialQueueNameLength);
            this.extension = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_EXTENSION,
                                                bufferSizeQuota.AllocIfAvailable(initialExtensionLength));
            this.extensionLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_EXTENSION_LEN, initialExtensionLength);
            this.label = new StringProperty(this, UnsafeNativeMethods.PROPID_M_LABEL, initialLabelLength);
            this.labelLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_LABEL_LEN, initialLabelLength);
            this.priority = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_PRIORITY);
            this.responseFormatName = new StringProperty(this, UnsafeNativeMethods.PROPID_M_RESP_FORMAT_NAME, initialQueueNameLength);
            this.responseFormatNameLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_RESP_FORMAT_NAME_LEN, initialQueueNameLength);
            this.sentTime = new IntProperty(this, UnsafeNativeMethods.PROPID_M_SENTTIME);
            this.timeToReachQueue = new IntProperty(this, UnsafeNativeMethods.PROPID_M_TIME_TO_REACH_QUEUE);
            this.privacyLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_PRIV_LEVEL);
        }

        protected override void OnGrowBuffers(SizeQuota bufferSizeQuota)
        {
            base.OnGrowBuffers(bufferSizeQuota);

            this.adminQueue.EnsureValueLength(this.adminQueueLength.Value);
            this.responseFormatName.EnsureValueLength(this.responseFormatNameLength.Value);
            this.destinationQueue.EnsureValueLength(this.destinationQueueLength.Value);
            this.label.EnsureValueLength(this.labelLength.Value);

            bufferSizeQuota.Alloc(this.extensionLength.Value);
            this.extension.EnsureBufferLength(this.extensionLength.Value);
        }

        public void SetMessageProperties(MsmqIntegrationMessageProperty property)
        {
            property.AcknowledgeType = (System.Messaging.AcknowledgeTypes)this.acknowledge.Value;
            property.Acknowledgment = (System.Messaging.Acknowledgment)this.Class.Value;
            property.AdministrationQueue = GetQueueName(this.adminQueue.GetValue(this.adminQueueLength.Value));
            property.AppSpecific = this.appSpecific.Value;
            property.ArrivedTime = MsmqDateTime.ToDateTime(this.arrivedTime.Value).ToLocalTime();
            property.Authenticated = this.authenticated.Value != 0;
            property.BodyType = this.bodyType.Value;
            property.CorrelationId = MsmqMessageId.ToString(this.correlationId.Buffer);
            property.DestinationQueue = GetQueueName(this.destinationQueue.GetValue(this.destinationQueueLength.Value));
            property.Extension = this.extension.GetBufferCopy(this.extensionLength.Value);
            property.Id = MsmqMessageId.ToString(this.MessageId.Buffer);
            property.Label = this.label.GetValue(this.labelLength.Value);

            if (this.Class.Value == UnsafeNativeMethods.MQMSG_CLASS_NORMAL)
                property.MessageType = System.Messaging.MessageType.Normal;
            else if (this.Class.Value == UnsafeNativeMethods.MQMSG_CLASS_REPORT)
                property.MessageType = System.Messaging.MessageType.Report;
            else
                property.MessageType = System.Messaging.MessageType.Acknowledgment;

            property.Priority = (System.Messaging.MessagePriority)this.priority.Value;
            property.ResponseQueue = GetQueueName(this.responseFormatName.GetValue(this.responseFormatNameLength.Value));
            property.SenderId = this.SenderId.GetBufferCopy(this.SenderIdLength.Value);
            property.SentTime = MsmqDateTime.ToDateTime(this.sentTime.Value).ToLocalTime();
            property.InternalSetTimeToReachQueue(MsmqDuration.ToTimeSpan(this.timeToReachQueue.Value));
        }

        static Uri GetQueueName(string formatName)
        {
            if (String.IsNullOrEmpty(formatName))
                return null;
            else
                return new Uri("msmq.formatname:" + formatName);
        }
    }
}

