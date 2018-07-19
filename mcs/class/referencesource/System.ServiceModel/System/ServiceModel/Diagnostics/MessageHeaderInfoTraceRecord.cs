//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;
    
    sealed class MessageHeaderInfoTraceRecord : TraceRecord
    {
        MessageHeaderInfo messageHeaderInfo;
        internal MessageHeaderInfoTraceRecord(MessageHeaderInfo messageHeaderInfo)
        {
            this.messageHeaderInfo = messageHeaderInfo;
        }

        internal override string EventId { get { return BuildEventId("MessageHeaderInfo"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.messageHeaderInfo != null)
            {
                xml.WriteStartElement("MessageHeaderInfo");
                if (!string.IsNullOrEmpty(this.messageHeaderInfo.Actor))
                {
                    xml.WriteElementString("Actor", this.messageHeaderInfo.Actor);
                }
                xml.WriteElementString("MustUnderstand", this.messageHeaderInfo.MustUnderstand.ToString());
                if (!string.IsNullOrEmpty(this.messageHeaderInfo.Name))
                {
                    xml.WriteElementString("Name", this.messageHeaderInfo.Name);
                }
                xml.WriteElementString("Relay", this.messageHeaderInfo.Relay.ToString());
                if (!string.IsNullOrEmpty(this.messageHeaderInfo.Namespace))
                {
                    xml.WriteElementString("Namespace", this.messageHeaderInfo.Namespace);
                }
                xml.WriteEndElement();
            }
        }
    }
}
