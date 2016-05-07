//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;

    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;
    using DictionaryManager = System.IdentityModel.DictionaryManager;

    class SignatureConfirmationElement : ISignatureValueSecurityElement
    {
        SecurityVersion version;
        string id;
        byte[] signatureValue;

        public SignatureConfirmationElement(string id, byte[] signatureValue, SecurityVersion version)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            if (signatureValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signatureValue");
            }
            this.id = id;
            this.signatureValue = signatureValue;
            this.version = version;
        }

        public bool HasId
        {
            get { return true; }
        }

        public string Id
        {
            get { return this.id; }
        }

        public byte[] GetSignatureValue()
        {
            return this.signatureValue;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.version.WriteSignatureConfirmation(writer, this.id, this.signatureValue);
        }
    }
}
