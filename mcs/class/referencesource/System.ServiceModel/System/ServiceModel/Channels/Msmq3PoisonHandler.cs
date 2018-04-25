//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Diagnostics.Application;

    sealed class Msmq3PoisonHandler : IPoisonHandlingStrategy
    {
        const int maxTrackedMessages = 256;

        MsmqReceiveHelper receiver;
        SortedList<long, int> trackedMessages;
        object thisLock = new object();

        internal Msmq3PoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
            this.trackedMessages = new SortedList<long, int>(maxTrackedMessages);
        }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            long lookupId = messageProperty.LookupId;
            int seen;

            lock (thisLock)
            {
                seen = this.UpdateSeenCount(lookupId);
                if (seen > (receiver.MsmqReceiveParameters.ReceiveRetryCount + 1) && receiver.MsmqReceiveParameters.ReceiveRetryCount != Int32.MaxValue)
                {
                    if (TD.ReceiveRetryCountReachedIsEnabled())
                    {
                        TD.ReceiveRetryCountReached(messageProperty.MessageId);
                    }
                    FinalDisposition(messageProperty);
                    this.trackedMessages.Remove(lookupId);
                    return true;
                }
            }

            messageProperty.AbortCount = seen - 1;
            return false;
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            switch (receiver.MsmqReceiveParameters.ReceiveErrorHandling)
            {
                case ReceiveErrorHandling.Drop:
                    //Unlocking message here since the message is locked under internal transaction and 
                    //cannot be unlocked by aborting ambient transaction
                    MsmqDefaultLockingQueue queue = this.receiver.Queue as MsmqDefaultLockingQueue;
                    if ((queue != null) && this.receiver.Transactional)
                        queue.UnlockMessage(messageProperty.LookupId, TimeSpan.Zero);
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
                    break;

                case ReceiveErrorHandling.Fault:
                    MsmqReceiveHelper.TryAbortTransactionCurrent();
                    if (null != this.receiver.ChannelListener)
                        this.receiver.ChannelListener.FaultListener();
                    if (null != this.receiver.Channel)
                        this.receiver.Channel.FaultChannel();
                    break;
                default:
                    Fx.Assert("System.ServiceModel.Channels.Msmq3PoisonHandler.FinalDisposition(): (unexpected ReceiveErrorHandling)");
                    break;
            }
        }

        int UpdateSeenCount(long lookupId)
        {
            int value;
            if (this.trackedMessages.TryGetValue(lookupId, out value))
            {
                ++value;
                this.trackedMessages[lookupId] = value;
                return value;
            }
            else
            {
                if (maxTrackedMessages == this.trackedMessages.Count)
                {
                    this.trackedMessages.RemoveAt(0);
                }
                this.trackedMessages.Add(lookupId, 1);
                return 1;
            }
        }

        public void Open()
        { }

        public void Dispose()
        { }
    }
}
