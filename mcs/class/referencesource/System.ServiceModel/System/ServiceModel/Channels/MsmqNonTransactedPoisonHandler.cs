//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    sealed class MsmqNonTransactedPoisonHandler : IPoisonHandlingStrategy
    {
        MsmqReceiveHelper receiver;

        internal MsmqNonTransactedPoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
        }

        public void Open()
        { }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            return false;
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
        }

        public void Dispose()
        { }
    }
}
