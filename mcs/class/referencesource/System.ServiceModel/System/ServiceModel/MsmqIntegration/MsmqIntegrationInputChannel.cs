//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.MsmqIntegration
{
    using System.ServiceModel.Channels;

    sealed class MsmqIntegrationInputChannel
        : MsmqInputChannelBase
    {
        public MsmqIntegrationInputChannel(MsmqIntegrationChannelListener listener)
            : base(listener, new MsmqIntegrationMessagePool(MsmqDefaults.MaxPoolSize))
        { }

        protected override Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty property)
        {
            MsmqIntegrationChannelListener listener = this.Manager as MsmqIntegrationChannelListener;
            return MsmqDecodeHelper.DecodeIntegrationDatagram(listener, this.MsmqReceiveHelper, msmqMessage as MsmqIntegrationInputMessage, property);
        }
    }
}

