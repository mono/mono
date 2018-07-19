//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Diagnostics.Application;

    sealed class Msmq4SubqueuePoisonHandler : IPoisonHandlingStrategy
    {
        MsmqReceiveHelper receiver;

        public Msmq4SubqueuePoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
        }

        public void Open()
        { }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            if (messageProperty.AbortCount > this.receiver.MsmqReceiveParameters.ReceiveRetryCount)
            {
                if (TD.ReceiveRetryCountReachedIsEnabled())
                {
                    TD.ReceiveRetryCountReached(messageProperty.MessageId);
                }
                FinalDisposition(messageProperty);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            switch (this.receiver.MsmqReceiveParameters.ReceiveErrorHandling)
            {
                case ReceiveErrorHandling.Drop:
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
                    break;

                case ReceiveErrorHandling.Fault:
                    MsmqReceiveHelper.TryAbortTransactionCurrent();
                    if (null != this.receiver.ChannelListener)
                        this.receiver.ChannelListener.FaultListener();
                    if (null != this.receiver.Channel)
                        this.receiver.Channel.FaultChannel();
                    break;
                case ReceiveErrorHandling.Reject:
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, true);
                    MsmqDiagnostics.PoisonMessageRejected(messageProperty.MessageId, this.receiver.InstanceId);
                    break;
                default:
                    Fx.Assert("System.ServiceModel.Channels.Msmq4PoisonHandler.FinalDisposition(): (unexpected ReceiveErrorHandling)");
                    break;
            }
        }

        public void Dispose()
        { }
    }
}
