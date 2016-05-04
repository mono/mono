//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    class WSSecurityOneDotOneReceiveSecurityHeader : WSSecurityOneDotZeroReceiveSecurityHeader
    {
        public WSSecurityOneDotOneReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager,
            SecurityAlgorithmSuite algorithmSuite,
            int headerIndex, MessageDirection direction)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, headerIndex, direction)
        {
        }

        protected override DecryptedHeader DecryptHeader(XmlDictionaryReader reader, WrappedKeySecurityToken wrappedKeyToken)
        {
            // If it is the client, then we may need to read the GenericXmlSecurityKeyIdentoifoer clause while reading EncryptedData. 
            EncryptedHeaderXml headerXml = new EncryptedHeaderXml(this.Version, this.MessageDirection == MessageDirection.Output);
            headerXml.SecurityTokenSerializer = this.StandardsManager.SecurityTokenSerializer;
            headerXml.ReadFrom(reader, MaxReceivedMessageSize);

            // The Encrypted Headers MustUnderstand, Relay and Actor attributes should match the
            // Security Headers value.
            if (headerXml.MustUnderstand != this.MustUnderstand)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedHeaderAttributeMismatch, XD.MessageDictionary.MustUnderstand.Value, headerXml.MustUnderstand, this.MustUnderstand)));

            if (headerXml.Relay != this.Relay)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedHeaderAttributeMismatch, XD.Message12Dictionary.Relay.Value, headerXml.Relay, this.Relay)));

            if (headerXml.Actor != this.Actor)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedHeaderAttributeMismatch, this.Version.Envelope.DictionaryActor, headerXml.Actor, this.Actor)));
            
            SecurityToken token;
            if (wrappedKeyToken == null)
            {
                token = ResolveKeyIdentifier(headerXml.KeyIdentifier, this.CombinedPrimaryTokenResolver, false);
            }
            else
            {
                token = wrappedKeyToken;
            }
            RecordEncryptionToken(token);
            using (SymmetricAlgorithm algorithm = CreateDecryptionAlgorithm(token, headerXml.EncryptionMethod, this.AlgorithmSuite))
            {
                headerXml.SetUpDecryption(algorithm);
                return new DecryptedHeader(
                    headerXml.GetDecryptedBuffer(),
                    this.SecurityVerifiedMessage.GetEnvelopeAttributes(), this.SecurityVerifiedMessage.GetHeaderAttributes(),
                    this.Version, this.StandardsManager.IdManager, this.ReaderQuotas);
            }
        }
    }
}
