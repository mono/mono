//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    class ContextMessageHeader : MessageHeader
    {
        public const string ContextHeaderName = "Context";
        public const string ContextHeaderNamespace = "http://schemas.microsoft.com/ws/2006/05/context";
        public const string ContextPropertyElement = "Property";
        public const string ContextPropertyNameAttribute = "name";

        static ChannelProtectionRequirements encryptAndSignChannelProtectionRequirements;
        static ChannelProtectionRequirements signChannelProtectionRequirements;

        IDictionary<string, string> context;

        public ContextMessageHeader(IDictionary<string, string> context)
            : base()
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            this.context = context;
        }

        public override string Name
        {
            get { return ContextHeaderName; }
        }

        public override string Namespace
        {
            get { return ContextHeaderNamespace; }
        }

        public static ContextMessageProperty GetContextFromHeaderIfExists(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            int i = message.Headers.FindHeader(ContextHeaderName, ContextHeaderNamespace);
            if (i >= 0)
            {
                MessageHeaders headers = message.Headers;
                ContextMessageProperty context = ParseContextHeader(headers.GetReaderAtHeader(i));
                headers.AddUnderstood(i);
                return context;
            }
            else
            {
                return null;
            }
        }

        internal static ChannelProtectionRequirements GetChannelProtectionRequirements(ProtectionLevel protectionLevel)
        {
            ChannelProtectionRequirements result;

            if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                if (encryptAndSignChannelProtectionRequirements == null)
                {
                    MessagePartSpecification header = new MessagePartSpecification();
                    header.HeaderTypes.Add(new XmlQualifiedName(ContextHeaderName, ContextHeaderNamespace));
                    ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
                    requirements.IncomingSignatureParts.AddParts(header);
                    requirements.IncomingEncryptionParts.AddParts(header);
                    requirements.OutgoingSignatureParts.AddParts(header);
                    requirements.OutgoingEncryptionParts.AddParts(header);
                    requirements.MakeReadOnly();
                    encryptAndSignChannelProtectionRequirements = requirements;
                }
                result = encryptAndSignChannelProtectionRequirements;
            }
            else if (protectionLevel == ProtectionLevel.Sign)
            {
                if (signChannelProtectionRequirements == null)
                {
                    MessagePartSpecification header = new MessagePartSpecification();
                    header.HeaderTypes.Add(new XmlQualifiedName(ContextHeaderName, ContextHeaderNamespace));
                    ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
                    requirements.IncomingSignatureParts.AddParts(header);
                    requirements.OutgoingSignatureParts.AddParts(header);
                    requirements.MakeReadOnly();
                    signChannelProtectionRequirements = requirements;
                }
                result = signChannelProtectionRequirements;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("protectionLevel"));
            }

            return result;
        }

        internal static ContextMessageProperty ParseContextHeader(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            ContextMessageProperty result = new ContextMessageProperty();
            try
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement(ContextHeaderName, ContextHeaderNamespace);

                    while (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        if (reader.LocalName != ContextPropertyElement || reader.NamespaceURI != ContextHeaderNamespace)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new ProtocolException(SR.GetString(SR.SchemaViolationInsideContextHeader)));
                        }

                        string propertyName = reader.GetAttribute(ContextPropertyNameAttribute);

                        if (string.IsNullOrEmpty(propertyName) || !ContextDictionary.TryValidateKeyValueSpace(propertyName))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new ProtocolException(SR.GetString(SR.InvalidCookieContent, propertyName)));
                        }
                        result.Context[propertyName] = reader.ReadElementString();
                    }

                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.SchemaViolationInsideContextHeader)));
                    }
                }
            }
            catch (XmlException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.XmlFormatViolationInContextHeader), e));
            }

            return result;
        }

        internal static void WriteHeaderContents(XmlDictionaryWriter writer, IDictionary<string, string> context)
        {
            foreach (KeyValuePair<string, string> pair in context)
            {
                Fx.Assert(!string.IsNullOrEmpty(pair.Key), "ContextProperty name is null");
                writer.WriteStartElement(ContextPropertyElement, ContextHeaderNamespace);
                writer.WriteAttributeString(ContextPropertyNameAttribute, null, pair.Key);
                writer.WriteValue(pair.Value);
                writer.WriteEndElement();
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            WriteHeaderContents(writer, this.context);
        }
    }
}
