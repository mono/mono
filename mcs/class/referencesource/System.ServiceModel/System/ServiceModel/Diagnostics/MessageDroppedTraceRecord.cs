//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    sealed class MessageDroppedTraceRecord : MessageTraceRecord
    {
        EndpointAddress endpointAddress;
        internal MessageDroppedTraceRecord(Message message, EndpointAddress endpointAddress) :
            base(message)
        {
            this.endpointAddress = endpointAddress;
        }

        internal override string EventId { get { return BuildEventId("MessageDropped"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            base.WriteTo(xml);
            if (this.endpointAddress != null)
            {
                xml.WriteStartElement("EndpointAddress");
                this.endpointAddress.WriteTo(AddressingVersion.WSAddressing10, xml);
                xml.WriteEndElement();
            }
        }
    }
}
