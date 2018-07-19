//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Xml;

    sealed class CloseSequenceResponse : BodyWriter
    {
        UniqueId identifier;

        public CloseSequenceResponse(UniqueId identifier)
            : base(true)
        {
            this.identifier = identifier;
        }

        public static CloseSequenceResponseInfo Create(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                Fx.Assert("Argument reader cannot be null.");
            }

            CloseSequenceResponseInfo closeSequenceResponseInfo = new CloseSequenceResponseInfo();

            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);

            reader.ReadStartElement(DXD.Wsrm11Dictionary.CloseSequenceResponse, wsrmNs);
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            closeSequenceResponseInfo.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return closeSequenceResponseInfo;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);

            writer.WriteStartElement(DXD.Wsrm11Dictionary.CloseSequenceResponse, wsrmNs);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
