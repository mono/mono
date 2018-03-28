//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    class ReplyChannelAcceptor : SingletonChannelAcceptor<IReplyChannel, ReplyChannel, RequestContext>
    {
        public ReplyChannelAcceptor(ChannelManagerBase channelManager)
            : base(channelManager)
        {
        }

        protected override ReplyChannel OnCreateChannel()
        {
            return new ReplyChannel(this.ChannelManager, null);
        }

        protected override void OnTraceMessageReceived(RequestContext requestContext)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageReceived,
                    SR.GetString(SR.TraceCodeMessageReceived),
                    MessageTransmitTraceRecord.CreateReceiveTraceRecord((requestContext == null) ? null : requestContext.RequestMessage), this, null);
            }
        }
    }
}
