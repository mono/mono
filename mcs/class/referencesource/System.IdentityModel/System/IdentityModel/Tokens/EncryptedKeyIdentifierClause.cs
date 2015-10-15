//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Tokens
{
    using System.Globalization;

    sealed public class EncryptedKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        readonly string carriedKeyName;
        readonly string encryptionMethod;
        readonly SecurityKeyIdentifier encryptingKeyIdentifier;

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod)
            : this(encryptedKey, encryptionMethod, null)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier)
            : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, null)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName)
            : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName, true, null, 0)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName, byte[] derivationNonce, int derivationLength)
            : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName, true, derivationNonce, derivationLength)
        {
        }

        internal EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName, bool cloneBuffer, byte[] derivationNonce, int derivationLength)
            : base("http://www.w3.org/2001/04/xmlenc#EncryptedKey", encryptedKey, cloneBuffer, derivationNonce, derivationLength)
        {
            if (encryptionMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionMethod");
            }
            this.carriedKeyName = carriedKeyName;
            this.encryptionMethod = encryptionMethod;
            this.encryptingKeyIdentifier = encryptingKeyIdentifier;
        }

        public string CarriedKeyName
        {
            get { return this.carriedKeyName; }
        }

        public SecurityKeyIdentifier EncryptingKeyIdentifier
        {
            get { return this.encryptingKeyIdentifier; }
        }

        public string EncryptionMethod
        {
            get { return this.encryptionMethod; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            EncryptedKeyIdentifierClause that = keyIdentifierClause as EncryptedKeyIdentifierClause;

            // PreSharp 
            #pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.GetRawBuffer(), this.encryptionMethod, this.carriedKeyName));
        }

        public bool Matches(byte[] encryptedKey, string encryptionMethod, string carriedKeyName)
        {
            return Matches(encryptedKey) && this.encryptionMethod == encryptionMethod && this.carriedKeyName == carriedKeyName;
        }

        public byte[] GetEncryptedKey()
        {
            return GetBuffer();
        }
        
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EncryptedKeyIdentifierClause(EncryptedKey = {0}, Method '{1}')",
                Convert.ToBase64String(GetRawBuffer()), this.EncryptionMethod);
        }
    }
}
