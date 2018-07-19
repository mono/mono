//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;

    class ChannelTraceRecord : TraceRecord
    {
        string channelType;

        internal ChannelTraceRecord(IChannel channel)
        {
            this.channelType = channel == null ? null : channel.ToString();
        }

        internal override string EventId { get { return BuildEventId("Channel"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.channelType != null)
            {
                xml.WriteElementString("ChannelType", this.channelType);
            }
        }
    }
}
