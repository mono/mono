//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Diagnostics;
    using System.IO;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.ServiceModel.Diagnostics;

    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;

    sealed class WSSecurityOneDotOneSendSecurityHeader : WSSecurityOneDotZeroSendSecurityHeader
    {
        public WSSecurityOneDotOneSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager, 
            SecurityAlgorithmSuite algorithmSuite,
            MessageDirection direction)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
        }

        protected override ISignatureValueSecurityElement[] CreateSignatureConfirmationElements(SignatureConfirmations signatureConfirmations)
        {
            if (signatureConfirmations == null || signatureConfirmations.Count == 0)
            {
                return null;
            }
            ISignatureValueSecurityElement[] result = new ISignatureValueSecurityElement[signatureConfirmations.Count];
            for (int i = 0; i < signatureConfirmations.Count; ++i)
            {
                byte[] sigValue;
                bool isEncrypted;
                signatureConfirmations.GetConfirmation(i, out sigValue, out isEncrypted);
                result[i] = new SignatureConfirmationElement(this.GenerateId(), sigValue, this.StandardsManager.SecurityVersion);
            }
            return result;
        }

        protected override EncryptedHeader EncryptHeader(MessageHeader plainTextHeader, SymmetricAlgorithm algorithm, 
            SecurityKeyIdentifier keyIdentifier, MessageVersion version, string id, MemoryStream stream)
        {
            // We are not reading EncryptedData from the wire here, hence pass false.
            EncryptedHeaderXml encryptedHeaderXml = new EncryptedHeaderXml(version, false);
            encryptedHeaderXml.SecurityTokenSerializer = this.StandardsManager.SecurityTokenSerializer;
            encryptedHeaderXml.EncryptionMethod = this.EncryptionAlgorithm;
            encryptedHeaderXml.EncryptionMethodDictionaryString = this.EncryptionAlgorithmDictionaryString;
            encryptedHeaderXml.KeyIdentifier = keyIdentifier;
            encryptedHeaderXml.Id = id;
            // The Encrypted Headers MustUnderstand, Relay and Actor attributes will always match the
            // Security Headers value. The values for these on the Encrypted Header and its decrypted 
            // form can be different.
            encryptedHeaderXml.MustUnderstand = this.MustUnderstand;
            encryptedHeaderXml.Relay = this.Relay;
            encryptedHeaderXml.Actor = this.Actor;

            encryptedHeaderXml.SetUpEncryption(algorithm, stream);

            return new EncryptedHeader(plainTextHeader, encryptedHeaderXml, EncryptedHeaderXml.ElementName.Value, EncryptedHeaderXml.NamespaceUri.Value, version);
        }
    }
}
    
