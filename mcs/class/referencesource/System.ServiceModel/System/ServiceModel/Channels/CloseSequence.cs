//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Xml;

    sealed class CloseSequence : BodyWriter
    {
        UniqueId identifier;
        Int64 lastMsgNumber;

        public CloseSequence(UniqueId identifier, Int64 lastMsgNumber)
            : base(true)
        {
            this.identifier = identifier;
            this.lastMsgNumber = lastMsgNumber;
        }

        public static CloseSequenceInfo Create(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                Fx.Assert("Argument reader cannot be null.");
            }

            CloseSequenceInfo closeSequenceInfo = new CloseSequenceInfo();

            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;

            reader.ReadStartElement(wsrm11Dictionary.CloseSequence, wsrmNs);
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            closeSequenceInfo.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            if (reader.IsStartElement(wsrm11Dictionary.LastMsgNumber, wsrmNs))
            {
                reader.ReadStartElement();
                closeSequenceInfo.LastMsgNumber = WsrmUtilities.ReadSequenceNumber(reader, false);
                reader.ReadEndElement();
            }

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return closeSequenceInfo;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;

            writer.WriteStartElement(wsrm11Dictionary.CloseSequence, wsrmNs);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();

            if (this.lastMsgNumber > 0)
            {
                writer.WriteStartElement(wsrm11Dictionary.LastMsgNumber, wsrmNs);
                writer.WriteValue(this.lastMsgNumber);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
