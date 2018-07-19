//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    sealed class MsmqInputChannel
        : MsmqInputChannelBase
    {
        public MsmqInputChannel(MsmqInputChannelListener listener)
            : base(listener, new MsmqInputMessagePool((listener.ReceiveParameters as MsmqTransportReceiveParameters).MaxPoolSize))
        { }

        protected override Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            MsmqInputChannelListener listener = this.Manager as MsmqInputChannelListener;
            return MsmqDecodeHelper.DecodeTransportDatagram(listener, this.MsmqReceiveHelper, msmqMessage, messageProperty);
        }
    }
}

