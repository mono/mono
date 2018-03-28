//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;

    class MessageTransmitTraceRecord : MessageTraceRecord
    {
        Uri address = null;
        string addressElementName = null;

        MessageTransmitTraceRecord(Message message) : base(message) { }

        MessageTransmitTraceRecord(Message message, string addressElementName)
            :
            this(message)
        {
            this.addressElementName = addressElementName;
        }

        MessageTransmitTraceRecord(Message message, string addressElementName, EndpointAddress address)
            :
            this(message, addressElementName)
        {
            if (address != null)
            {
                this.address = address.Uri;
            }
        }

        MessageTransmitTraceRecord(Message message, string addressElementName, Uri uri)
            :
            this(message, addressElementName)
        {
            this.address = uri;
        }

        internal override string EventId { get { return BuildEventId("MessageTransmit"); } }

        internal static MessageTransmitTraceRecord CreateSendTraceRecord(Message message, EndpointAddress address)
        {
            return new MessageTransmitTraceRecord(message, "RemoteAddress", address);
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message, Uri uri)
        {
            return new MessageTransmitTraceRecord(message, "LocalAddress", uri);
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message, EndpointAddress address)
        {
            return new MessageTransmitTraceRecord(message, "LocalAddress", address);
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message)
        {
            return new MessageTransmitTraceRecord(message);
        }

        internal override void WriteTo(XmlWriter xml)
        {
            base.WriteTo(xml);
            if (this.address != null)
            {
                xml.WriteElementString(this.addressElementName, this.address.ToString());
            }
        }
    }
}
