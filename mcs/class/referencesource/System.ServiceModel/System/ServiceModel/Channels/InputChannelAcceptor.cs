//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    class InputChannelAcceptor : SingletonChannelAcceptor<IInputChannel, InputChannel, Message>
    {
        public InputChannelAcceptor(ChannelManagerBase channelManager)
            : base(channelManager)
        {
        }

        protected override InputChannel OnCreateChannel()
        {
            return new InputChannel(this.ChannelManager, null);
        }

        protected override void OnTraceMessageReceived(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageReceived,
                    SR.GetString(SR.TraceCodeMessageReceived),
                    MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
            }
        }
    }
}
