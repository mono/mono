//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Xml;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Collections.Generic;
    using System.ServiceModel.Security.Tokens;

    class DerivedKeyCachingSecurityTokenSerializer : SecurityTokenSerializer
    {
        DerivedKeySecurityTokenCache[] cachedTokens;
        WSSecureConversation secureConversation;
        SecurityTokenSerializer innerTokenSerializer;
        bool isInitiator;
        int indexToCache = 0;
        Object thisLock;

        internal DerivedKeyCachingSecurityTokenSerializer(int cacheSize, bool isInitiator, WSSecureConversation secureConversation, SecurityTokenSerializer innerTokenSerializer)
            : base()
        {
            if (innerTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerTokenSerializer");
            }
            if (secureConversation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("secureConversation");
            }
            if (cacheSize <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("cacheSize", SR.GetString(SR.ValueMustBeGreaterThanZero)));
            }
            this.cachedTokens = new DerivedKeySecurityTokenCache[cacheSize];
            this.isInitiator = isInitiator;
            this.secureConversation = secureConversation;
            this.innerTokenSerializer = innerTokenSerializer;
            this.thisLock = new Object();
        }

        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadKeyIdentifierClause(reader);
        }

        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadKeyIdentifier(reader);
        }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadToken(reader);
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            if (this.secureConversation.IsAtDerivedKeyToken(dictionaryReader))
            {
                string id;
                string derivationAlgorithm;
                string label;
                int length;
                byte[] nonce;
                int offset;
                int generation;
                SecurityKeyIdentifierClause tokenToDeriveIdentifier;
                SecurityToken tokenToDerive;
                this.secureConversation.ReadDerivedKeyTokenParameters(dictionaryReader, tokenResolver, out id, out derivationAlgorithm, out label,
                    out length, out nonce, out offset, out generation, out tokenToDeriveIdentifier, out tokenToDerive);

                DerivedKeySecurityToken cachedToken = GetCachedToken(id, generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm);
                if (cachedToken != null)
                {
                    return cachedToken;
                }

                lock (this.thisLock)
                {
                    cachedToken = GetCachedToken(id, generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm);
                    if (cachedToken != null)
                    {
                        return cachedToken;
                    }
                    SecurityToken result = this.secureConversation.CreateDerivedKeyToken( id, derivationAlgorithm, label, length, nonce, offset, generation, tokenToDeriveIdentifier, tokenToDerive );
                    DerivedKeySecurityToken newToken = result as DerivedKeySecurityToken;
                    if (newToken != null)
                    {
                        int pos = this.indexToCache;
                        if (this.indexToCache == int.MaxValue)
                            this.indexToCache = 0;
                        else
                            this.indexToCache = (++this.indexToCache) % this.cachedTokens.Length;
                        this.cachedTokens[pos] = new DerivedKeySecurityTokenCache(newToken);
                    }
                    return result;
                }
            }
            else
            {
                return this.innerTokenSerializer.ReadToken(reader, tokenResolver);
            }
        }

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return this.innerTokenSerializer.CanWriteKeyIdentifierClause(keyIdentifierClause);
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            return this.innerTokenSerializer.CanWriteKeyIdentifier(keyIdentifier);
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            return this.innerTokenSerializer.CanWriteToken(token);
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            return this.innerTokenSerializer.ReadKeyIdentifierClause(reader);
        }

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            return this.innerTokenSerializer.ReadKeyIdentifier(reader);
        }

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            this.innerTokenSerializer.WriteKeyIdentifierClause(writer, keyIdentifierClause);
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            this.innerTokenSerializer.WriteKeyIdentifier(writer, keyIdentifier);
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            this.innerTokenSerializer.WriteToken(writer, token);
        }

        bool IsMatch(DerivedKeySecurityTokenCache cachedToken, string id, int generation, int offset, int length,
            string label, byte[] nonce, SecurityToken tokenToDerive, string derivationAlgorithm)
        {
            if ((cachedToken.Generation == generation)
                && (cachedToken.Offset == offset)
                && (cachedToken.Length == length)
                && (cachedToken.Label == label)
                && (cachedToken.KeyDerivationAlgorithm == derivationAlgorithm))
            {
                if (!cachedToken.IsSourceKeyEqual(tokenToDerive))
                {
                    return false;
                }
                // since derived key token keys are delay initialized during security processing, it may be possible
                // that the cached derived key token does not have its keys initialized as yet. If so return false for
                // the match so that the framework doesnt try to reference a null key.
                return (CryptoHelper.IsEqual(cachedToken.Nonce, nonce) && (cachedToken.SecurityKeys != null));
            }
            else
            {
                return false;
            }
        }

        DerivedKeySecurityToken GetCachedToken(string id, int generation, int offset, int length,
            string label, byte[] nonce, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm)
        {
            for (int i = 0; i < this.cachedTokens.Length; ++i)
            {
                DerivedKeySecurityTokenCache cachedToken = this.cachedTokens[i];
                if (cachedToken != null && IsMatch(cachedToken, id, generation, offset, length,
                    label, nonce, tokenToDerive, derivationAlgorithm))
                {
                    DerivedKeySecurityToken token = new DerivedKeySecurityToken(generation, offset, length, label, nonce, tokenToDerive,
                        tokenToDeriveIdentifier, derivationAlgorithm, id);
                    token.InitializeDerivedKey(cachedToken.SecurityKeys);
                    return token;
                }
            }
            return null;
        }

        class DerivedKeySecurityTokenCache
        {
            byte[] keyToDerive;
            int generation;
            int offset;
            int length;
            string label;
            string keyDerivationAlgorithm;
            byte[] nonce;
            ReadOnlyCollection<SecurityKey> keys;
            DerivedKeySecurityToken cachedToken;

            public DerivedKeySecurityTokenCache(DerivedKeySecurityToken cachedToken)
            {
                this.keyToDerive = ((SymmetricSecurityKey)cachedToken.TokenToDerive.SecurityKeys[0]).GetSymmetricKey();
                this.generation = cachedToken.Generation;
                this.offset = cachedToken.Offset;
                this.length = cachedToken.Length;
                this.label = cachedToken.Label;
                this.keyDerivationAlgorithm = cachedToken.KeyDerivationAlgorithm;
                this.nonce = cachedToken.Nonce;
                this.cachedToken = cachedToken;
            }

            public int Generation
            {
                get { return this.generation; }
            }

            public int Offset
            {
                get { return this.offset; }
            }

            public int Length
            {
                get { return this.length; }
            }

            public string Label
            {
                get { return this.label; }
            }

            public string KeyDerivationAlgorithm
            {
                get { return this.keyDerivationAlgorithm; }
            }

            public byte[] Nonce
            {
                get { return this.nonce; }
            }

            public ReadOnlyCollection<SecurityKey> SecurityKeys
            {
                get
                {
                    // we would need to hold onto the cached token till a hit is obtained because of
                    // the delay initialization of derived key crypto by the security header.
                    lock (this)
                    {
                        if (this.keys == null)
                        {
                            ReadOnlyCollection<SecurityKey> computedKeys;
                            if (this.cachedToken.TryGetSecurityKeys(out computedKeys))
                            {
                                this.keys = computedKeys;
                                this.cachedToken = null;
                            }
                        }
                    }
                    return this.keys;
                }
            }

            public bool IsSourceKeyEqual(SecurityToken token)
            {
                if (token.SecurityKeys.Count != 1)
                {
                    return false;
                }
                SymmetricSecurityKey key = token.SecurityKeys[0] as SymmetricSecurityKey;
                if (key == null)
                {
                    return false;
                }
                return CryptoHelper.IsEqual(this.keyToDerive, key.GetSymmetricKey());
            }
        }
    }
}
