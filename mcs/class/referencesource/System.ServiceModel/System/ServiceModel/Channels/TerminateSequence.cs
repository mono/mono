//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    sealed class TerminateSequence : BodyWriter
    {
        UniqueId identifier;
        Int64 lastMsgNumber;
        ReliableMessagingVersion reliableMessagingVersion;

        public TerminateSequence()
            : base(true)
        {
        }

        public TerminateSequence(ReliableMessagingVersion reliableMessagingVersion, UniqueId identifier, Int64 last)
            : base(true)
        {
            this.reliableMessagingVersion = reliableMessagingVersion;
            this.identifier = identifier;
            this.lastMsgNumber = last;
        }

        public static TerminateSequenceInfo Create(ReliableMessagingVersion reliableMessagingVersion,
            XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                Fx.Assert("Argument reader cannot be null.");
            }

            TerminateSequenceInfo terminateSequenceInfo = new TerminateSequenceInfo();
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            reader.ReadStartElement(wsrmFeb2005Dictionary.TerminateSequence, wsrmNs);

            reader.ReadStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            terminateSequenceInfo.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (reader.IsStartElement(DXD.Wsrm11Dictionary.LastMsgNumber, wsrmNs))
                {
                    reader.ReadStartElement();
                    terminateSequenceInfo.LastMsgNumber = WsrmUtilities.ReadSequenceNumber(reader, false);
                    reader.ReadEndElement();
                }
            }

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return terminateSequenceInfo;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            writer.WriteStartElement(wsrmFeb2005Dictionary.TerminateSequence, wsrmNs);
            writer.WriteStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();

            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.lastMsgNumber > 0)
                {
                    writer.WriteStartElement(DXD.Wsrm11Dictionary.LastMsgNumber, wsrmNs);
                    writer.WriteValue(this.lastMsgNumber);
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }
    }
}
