//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.ServiceModel.Security;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class WrappedKeySecurityToken : SecurityToken
    {
        string id;
        DateTime effectiveTime;

        EncryptedKey encryptedKey;
        ReadOnlyCollection<SecurityKey> securityKey;
        byte[] wrappedKey;
        string wrappingAlgorithm;
        ISspiNegotiation wrappingSspiContext;
        SecurityToken wrappingToken;
        SecurityKey wrappingSecurityKey;
        SecurityKeyIdentifier wrappingTokenReference;
        bool serializeCarriedKeyName;
        byte[] wrappedKeyHash;
        XmlDictionaryString wrappingAlgorithmDictionaryString;

        // sender use
        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, ISspiNegotiation wrappingSspiContext)
            : this(id, keyToWrap, (wrappingSspiContext != null) ? (wrappingSspiContext.KeyEncryptionAlgorithm) : null, wrappingSspiContext, null)
        {
        }

        // sender use
        public WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference)
            : this(id, keyToWrap, wrappingAlgorithm, null, wrappingToken, wrappingTokenReference)
        {
        }

        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference)
            : this(id, keyToWrap, wrappingAlgorithm, wrappingAlgorithmDictionaryString, wrappingToken, wrappingTokenReference, null, null)
        {
        }

        // direct receiver use, chained sender use
        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, ISspiNegotiation wrappingSspiContext, byte[] wrappedKey)
            : this(id, keyToWrap, wrappingAlgorithm, null)
        {
            if (wrappingSspiContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingSspiContext");
            }
            this.wrappingSspiContext = wrappingSspiContext;
            if (wrappedKey == null)
            {
                this.wrappedKey = wrappingSspiContext.Encrypt(keyToWrap);
            }
            else
            {
                this.wrappedKey = wrappedKey;
            }
            this.serializeCarriedKeyName = false;
        }

        // receiver use
        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference, byte[] wrappedKey, SecurityKey wrappingSecurityKey)
            : this(id, keyToWrap, wrappingAlgorithm, null, wrappingToken, wrappingTokenReference, wrappedKey, wrappingSecurityKey)
        {
        }

        WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference, byte[] wrappedKey, SecurityKey wrappingSecurityKey)
            : this(id, keyToWrap, wrappingAlgorithm, wrappingAlgorithmDictionaryString)
        {
            if (wrappingToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingToken");
            }
            this.wrappingToken = wrappingToken;
            this.wrappingTokenReference = wrappingTokenReference;
            if (wrappedKey == null)
            {
                this.wrappedKey = SecurityUtils.EncryptKey(wrappingToken, wrappingAlgorithm, keyToWrap);
            }
            else
            {
                this.wrappedKey = wrappedKey;
            }
            this.wrappingSecurityKey = wrappingSecurityKey;
            this.serializeCarriedKeyName = true;
        }

        WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString)
        {
            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            if (wrappingAlgorithm == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingAlgorithm");
            if (keyToWrap == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyToWrap");

            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.securityKey = SecurityUtils.CreateSymmetricSecurityKeys(keyToWrap);
            this.wrappingAlgorithm = wrappingAlgorithm;
            this.wrappingAlgorithmDictionaryString = wrappingAlgorithmDictionaryString;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            // Never expire
            get { return DateTime.MaxValue; }
        }

        internal EncryptedKey EncryptedKey
        {
            get { return this.encryptedKey; }
            set { this.encryptedKey = value; }
        }

        internal ReferenceList ReferenceList
        {
            get
            {
                return this.encryptedKey == null ? null : this.encryptedKey.ReferenceList;
            }
        }

        public string WrappingAlgorithm
        {
            get { return this.wrappingAlgorithm; }
        }

        internal SecurityKey WrappingSecurityKey
        {
            get { return this.wrappingSecurityKey; }
        }

        public SecurityToken WrappingToken
        {
            get { return this.wrappingToken; }
        }

        public SecurityKeyIdentifier WrappingTokenReference
        {
            get { return this.wrappingTokenReference; }
        }

        internal string CarriedKeyName
        {
            get { return null; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return this.securityKey; }
        }

        internal byte[] GetHash()
        {
            if (this.wrappedKeyHash == null)
            {
                EnsureEncryptedKeySetUp();
                using (HashAlgorithm hash = CryptoHelper.NewSha1HashAlgorithm())
                {
                    this.wrappedKeyHash = hash.ComputeHash(this.encryptedKey.GetWrappedKey());
                }
            }
            return wrappedKeyHash;
        }

        public byte[] GetWrappedKey()
        {
            return SecurityUtils.CloneBuffer(this.wrappedKey);
        }

        internal void EnsureEncryptedKeySetUp()
        {
            if (this.encryptedKey == null)
            {
                EncryptedKey ek = new EncryptedKey();
                ek.Id = this.Id;
                if (this.serializeCarriedKeyName)
                {
                    ek.CarriedKeyName = this.CarriedKeyName;
                }
                else
                {
                    ek.CarriedKeyName = null;
                }
                ek.EncryptionMethod = this.WrappingAlgorithm;
                ek.EncryptionMethodDictionaryString = this.wrappingAlgorithmDictionaryString;
                ek.SetUpKeyWrap(this.wrappedKey);
                if (this.WrappingTokenReference != null)
                {
                    ek.KeyIdentifier = this.WrappingTokenReference;
                }
                this.encryptedKey = ek;
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(EncryptedKeyHashIdentifierClause))
                return true;

            return base.CanCreateKeyIdentifierClause<T>();
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(EncryptedKeyHashIdentifierClause))
                return new EncryptedKeyHashIdentifierClause(GetHash()) as T;

            return base.CreateKeyIdentifierClause<T>();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            EncryptedKeyHashIdentifierClause encKeyIdentifierClause = keyIdentifierClause as EncryptedKeyHashIdentifierClause;
            if (encKeyIdentifierClause != null)
                return encKeyIdentifierClause.Matches(GetHash());

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }
    }
}
