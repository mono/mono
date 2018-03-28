//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.Net.Security;

    class CallbackContextMessageHeader : MessageHeader
    {
        public const string CallbackContextHeaderName = "CallbackContext";
        public const string CallbackContextHeaderNamespace = "http://schemas.microsoft.com/ws/2008/02/context";
        public const string CallbackEndpointReference = "CallbackEndpointReference";

        static ChannelProtectionRequirements encryptAndSignChannelProtectionRequirements;
        static ChannelProtectionRequirements signChannelProtectionRequirements;

        EndpointAddress callbackAddress;
        AddressingVersion version;

        public CallbackContextMessageHeader(EndpointAddress callbackAddress, AddressingVersion version)
            : base()
        {
            if (callbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackAddress");
            }
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }

            if (version != AddressingVersion.WSAddressing10)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CallbackContextOnlySupportedInWSAddressing10, version)));
            }

            this.callbackAddress = callbackAddress;
            this.version = version;
        }

        public override string Name
        {
            get { return CallbackContextHeaderName; }
        }

        public override string Namespace
        {
            get { return CallbackContextHeaderNamespace; }
        }

        internal static ChannelProtectionRequirements GetChannelProtectionRequirements(ProtectionLevel protectionLevel)
        {
            ChannelProtectionRequirements result;

            if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                if (encryptAndSignChannelProtectionRequirements == null)
                {
                    MessagePartSpecification header = new MessagePartSpecification();
                    header.HeaderTypes.Add(new XmlQualifiedName(CallbackContextHeaderName, CallbackContextHeaderNamespace));
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
                    header.HeaderTypes.Add(new XmlQualifiedName(CallbackContextHeaderName, CallbackContextHeaderNamespace));
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

        internal static CallbackContextMessageProperty ParseCallbackContextHeader(XmlReader reader, AddressingVersion version)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (version != AddressingVersion.WSAddressing10)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.CallbackContextOnlySupportedInWSAddressing10, version)));
            }

            try
            {
                reader.ReadStartElement(CallbackContextHeaderName, CallbackContextHeaderNamespace);
                EndpointAddress callbackAddress = EndpointAddress.ReadFrom(version, reader, CallbackEndpointReference, CallbackContextHeaderNamespace);
                reader.ReadEndElement();
                return new CallbackContextMessageProperty(callbackAddress);
            }
            catch (XmlException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.XmlFormatViolationInCallbackContextHeader), e));
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            this.callbackAddress.WriteTo(this.version, writer, CallbackEndpointReference, CallbackContextHeaderNamespace);
        }
    }
}
