//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Xml;
    using System.Text;
    using System.Globalization;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.Runtime.Serialization;

    [Serializable]
    internal class MustUnderstandSoapException : CommunicationException
    {
        // for serialization
        public MustUnderstandSoapException() { }
        protected MustUnderstandSoapException(SerializationInfo info, StreamingContext context) : base(info, context) { }


        Collection<MessageHeaderInfo> notUnderstoodHeaders;
        EnvelopeVersion envelopeVersion;

        public MustUnderstandSoapException(Collection<MessageHeaderInfo> notUnderstoodHeaders, EnvelopeVersion envelopeVersion)
        {
            this.notUnderstoodHeaders = notUnderstoodHeaders;
            this.envelopeVersion = envelopeVersion;
        }

        public Collection<MessageHeaderInfo> NotUnderstoodHeaders { get { return this.notUnderstoodHeaders; } }
        public EnvelopeVersion EnvelopeVersion { get { return this.envelopeVersion; } }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            string name = this.notUnderstoodHeaders[0].Name;
            string ns = this.notUnderstoodHeaders[0].Namespace;
            FaultCode code = new FaultCode(MessageStrings.MustUnderstandFault, this.envelopeVersion.Namespace);
            FaultReason reason = new FaultReason(SR.GetString(SR.SFxHeaderNotUnderstood, name, ns), CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code, reason);
            string faultAction = messageVersion.Addressing.DefaultFaultAction;
            Message message = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, fault, faultAction);
            if (this.envelopeVersion == EnvelopeVersion.Soap12)
            {
                this.AddNotUnderstoodHeaders(message.Headers);
            }
            return message;
        }

        void AddNotUnderstoodHeaders(MessageHeaders headers)
        {
            for (int i = 0; i < notUnderstoodHeaders.Count; ++i)
            {
                headers.Add(new NotUnderstoodHeader(notUnderstoodHeaders[i].Name, notUnderstoodHeaders[i].Namespace));
            }
        }

        class NotUnderstoodHeader : MessageHeader
        {
            string notUnderstoodName;
            string notUnderstoodNs;

            public NotUnderstoodHeader(string name, string ns)
            {
                this.notUnderstoodName = name;
                this.notUnderstoodNs = ns;
            }

            public override string Name
            {
                get { return Message12Strings.NotUnderstood; }
            }

            public override string Namespace
            {
                get { return Message12Strings.Namespace; }
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(this.Name, this.Namespace);
                writer.WriteXmlnsAttribute(null, notUnderstoodNs);
                writer.WriteStartAttribute(Message12Strings.QName);
                writer.WriteQualifiedName(notUnderstoodName, notUnderstoodNs);
                writer.WriteEndAttribute();
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                // empty
            }
        }
    }
}

