//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    sealed class TerminateSequenceResponse : BodyWriter
    {
        UniqueId identifier;

        public TerminateSequenceResponse()
            : base(true)
        {
        }

        public TerminateSequenceResponse(UniqueId identifier)
            : base(true)
        {
            this.identifier = identifier;
        }

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public static TerminateSequenceResponseInfo Create(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                Fx.Assert("Argument reader cannot be null.");
            }

            TerminateSequenceResponseInfo terminateSequenceInfo = new TerminateSequenceResponseInfo();
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);

            reader.ReadStartElement(DXD.Wsrm11Dictionary.TerminateSequenceResponse, wsrmNs);

            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            terminateSequenceInfo.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return terminateSequenceInfo;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            writer.WriteStartElement(DXD.Wsrm11Dictionary.TerminateSequenceResponse, wsrmNs);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
