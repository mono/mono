//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public sealed class MsmqMessageProperty
    {
        public const string Name = "MsmqMessageProperty";

        int abortCount;
        int moveCount;
        long lookupId;
        string messageId;
        int acknowledge;

        internal MsmqMessageProperty(MsmqInputMessage msmqMessage)
        {
            if (null == msmqMessage)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("msmqMessage");
            this.lookupId = msmqMessage.LookupId.Value;
            if (msmqMessage.AbortCount != null)
                this.abortCount = msmqMessage.AbortCount.Value;
            if (msmqMessage.MoveCount != null)
                this.moveCount = msmqMessage.MoveCount.Value;
            this.acknowledge = (int)(ushort)msmqMessage.Class.Value;
            this.messageId = MsmqMessageId.ToString(msmqMessage.MessageId.Buffer);
        }

        public DeliveryFailure? DeliveryFailure
        {
            get { return TryGetDeliveryFailure(this.messageId, this.acknowledge); }
        }

        public DeliveryStatus? DeliveryStatus
        {
            get
            {
                DeliveryFailure? deliveryFailure = this.DeliveryFailure;

                if (!deliveryFailure.HasValue)
                    return null;

                if (System.ServiceModel.Channels.DeliveryFailure.ReachQueueTimeout == deliveryFailure.Value
                    || System.ServiceModel.Channels.DeliveryFailure.Unknown == deliveryFailure.Value)
                    return System.ServiceModel.Channels.DeliveryStatus.InDoubt;
                else
                    return System.ServiceModel.Channels.DeliveryStatus.NotDelivered;
            }
        }

        public int AbortCount
        {
            get { return this.abortCount; }
            internal set { this.abortCount = value; }
        }

        internal long LookupId
        {
            get { return this.lookupId; }
        }

        internal string MessageId
        {
            get { return this.messageId; }
        }

        public int MoveCount
        {
            get { return this.moveCount; }
            internal set { this.moveCount = value; }
        }

        public static MsmqMessageProperty Get(Message message)
        {
            if (null == message)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            if (null == message.Properties)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message.Properties");

            return message.Properties[Name] as MsmqMessageProperty;
        }

        static DeliveryFailure? TryGetDeliveryFailure(string messageId, int acknowledgment)
        {
            // DeliveryFailure definintion explains these bit manipulations
            int bit15 = (1 << 15) & acknowledgment;
            if (0 == bit15)
                return null;
            int bit14 = (1 << 14) & acknowledgment;
            int otherBits = ~((1 << 15) | (1 << 14)) & acknowledgment;

            if ((0 == bit14 && otherBits >= 0 && otherBits <= 0x0A) ||
                (0 != bit14 && otherBits >= 0 && otherBits <= 0x02))
                return (DeliveryFailure)acknowledgment;
            else
            {
                MsmqDiagnostics.UnexpectedAcknowledgment(messageId, acknowledgment);
                return System.ServiceModel.Channels.DeliveryFailure.Unknown;
            }
        }
    }
}
