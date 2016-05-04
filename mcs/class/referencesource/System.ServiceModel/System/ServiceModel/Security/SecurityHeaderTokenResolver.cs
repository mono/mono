//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;
    using System.IO;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;

    internal sealed class SecurityHeaderTokenResolver : SecurityTokenResolver, System.IdentityModel.IWrappedTokenKeyResolver
    {
        const int InitialTokenArraySize = 10;
        int tokenCount;
        SecurityTokenEntry[] tokens;
        SecurityToken expectedWrapper;
        SecurityTokenParameters expectedWrapperTokenParameters;
        ReceiveSecurityHeader securityHeader;

        public SecurityHeaderTokenResolver()
            : this(null)
        {
        }

        public SecurityHeaderTokenResolver(ReceiveSecurityHeader securityHeader)
        {
            this.tokens = new SecurityTokenEntry[InitialTokenArraySize];
            this.securityHeader = securityHeader;
        }

        public SecurityToken ExpectedWrapper
        {
            get { return this.expectedWrapper; }
            set { this.expectedWrapper = value; }
        }

        public SecurityTokenParameters ExpectedWrapperTokenParameters
        {
            get { return this.expectedWrapperTokenParameters; }
            set { this.expectedWrapperTokenParameters = value; }
        }

        public void Add(SecurityToken token)
        {
            Add(token, SecurityTokenReferenceStyle.Internal, null);
        }

        public void Add(SecurityToken token, SecurityTokenReferenceStyle allowedReferenceStyle, SecurityTokenParameters tokenParameters)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            if ((allowedReferenceStyle == SecurityTokenReferenceStyle.External) && (tokenParameters == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ResolvingExternalTokensRequireSecurityTokenParameters));
            }

            EnsureCapacityToAddToken();
            this.tokens[this.tokenCount++] = new SecurityTokenEntry(token, tokenParameters, allowedReferenceStyle);
        }

        void EnsureCapacityToAddToken()
        {
            if (this.tokenCount == this.tokens.Length)
            {
                SecurityTokenEntry[] newTokens = new SecurityTokenEntry[this.tokens.Length * 2];
                Array.Copy(this.tokens, 0, newTokens, 0, this.tokenCount);
                this.tokens = newTokens;
            }
        }

        public bool CheckExternalWrapperMatch(SecurityKeyIdentifier keyIdentifier)
        {
            if (this.expectedWrapper == null || this.expectedWrapperTokenParameters == null)
            {
                return false;
            }

            for (int i = 0; i < keyIdentifier.Count; i++)
            {
                if (this.expectedWrapperTokenParameters.MatchesKeyIdentifierClause(this.expectedWrapper, keyIdentifier[i], SecurityTokenReferenceStyle.External))
                {
                    return true;
                }
            }
            return false;
        }

        internal SecurityToken ResolveToken(SecurityKeyIdentifier keyIdentifier, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            for (int i = 0; i < keyIdentifier.Count; i++)
            {
                SecurityToken token = ResolveToken(keyIdentifier[i], matchOnlyExternalTokens, resolveIntrinsicKeyClause);
                if (token != null)
                {
                    return token;
                }
            }
            return null;
        }

        SecurityKey ResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, bool createIntrinsicKeys)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("keyIdentifierClause"));
            }

            SecurityKey securityKey;
            for (int i = 0; i < this.tokenCount; i++)
            {
                securityKey = this.tokens[i].Token.ResolveKeyIdentifierClause(keyIdentifierClause);
                if (securityKey != null)
                {
                    return securityKey;
                }
            }

            if (createIntrinsicKeys)
            {
                if (SecurityUtils.TryCreateKeyFromIntrinsicKeyClause(keyIdentifierClause, this, out securityKey))
                {
                    return securityKey;
                }
            }

            return null;
        }

        bool MatchDirectReference(SecurityToken token, SecurityKeyIdentifierClause keyClause)
        {
            LocalIdKeyIdentifierClause localClause = keyClause as LocalIdKeyIdentifierClause;
            if (localClause == null) return false;
            return token.MatchesKeyIdentifierClause(localClause);
        }

        internal SecurityToken ResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, bool matchOnlyExternal, bool resolveIntrinsicKeyClause)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }

            SecurityToken resolvedToken = null;
            for (int i = 0; i < this.tokenCount; i++)
            {
                if (matchOnlyExternal && tokens[i].AllowedReferenceStyle != SecurityTokenReferenceStyle.External)
                {
                    continue;
                }

                SecurityToken token = tokens[i].Token;
                if (tokens[i].TokenParameters != null && tokens[i].TokenParameters.MatchesKeyIdentifierClause(token, keyIdentifierClause, tokens[i].AllowedReferenceStyle))
                {
                    resolvedToken = token;
                    break;
                }
                else if (tokens[i].TokenParameters == null)
                {
                    // match it according to the allowed reference style
                    if (tokens[i].AllowedReferenceStyle == SecurityTokenReferenceStyle.Internal && MatchDirectReference(token, keyIdentifierClause))
                    {
                        resolvedToken = token;
                        break;
                    }
                }
            }

            if ((resolvedToken == null) && (keyIdentifierClause is EncryptedKeyIdentifierClause))
            {
                EncryptedKeyIdentifierClause keyClause = (EncryptedKeyIdentifierClause)keyIdentifierClause;
                SecurityKeyIdentifier wrappingTokenReference = keyClause.EncryptingKeyIdentifier;
                SecurityToken unwrappingToken;
                if (this.expectedWrapper != null 
                    && CheckExternalWrapperMatch(wrappingTokenReference))
                    unwrappingToken = this.expectedWrapper;
                else
                    unwrappingToken = ResolveToken(wrappingTokenReference, true, resolveIntrinsicKeyClause);
                if (unwrappingToken != null)
                {
                    resolvedToken = SecurityUtils.CreateTokenFromEncryptedKeyClause(keyClause, unwrappingToken);
                }
            }

            if ((resolvedToken == null) && (keyIdentifierClause is X509RawDataKeyIdentifierClause) && (!matchOnlyExternal) && (resolveIntrinsicKeyClause))
            {
                resolvedToken = new X509SecurityToken(new X509Certificate2(((X509RawDataKeyIdentifierClause)keyIdentifierClause).GetX509RawData()));
            }

            byte[] derivationNonce = keyIdentifierClause.GetDerivationNonce();
            if ((resolvedToken != null) && (derivationNonce != null))
            {
                // A Implicit Derived Key is specified. Create a derived key off of the resolve token.
                if (SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(resolvedToken) == null)
                {
                    // The resolved token contains no Symmetric Security key and thus we cannot create 
                    // a derived key off of it.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnableToDeriveKeyFromKeyInfoClause, keyIdentifierClause, resolvedToken)));
                }

                int derivationLength = (keyIdentifierClause.DerivationLength == 0) ? DerivedKeySecurityToken.DefaultDerivedKeyLength : keyIdentifierClause.DerivationLength;
                if (derivationLength > this.securityHeader.MaxDerivedKeyLength)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.DerivedKeyLengthSpecifiedInImplicitDerivedKeyClauseTooLong, keyIdentifierClause.ToString(), derivationLength, this.securityHeader.MaxDerivedKeyLength)));
                bool alreadyDerived = false;
                for (int i = 0; i < this.tokenCount; ++i)
                {
                    DerivedKeySecurityToken derivedKeyToken = this.tokens[i].Token as DerivedKeySecurityToken;
                    if (derivedKeyToken != null)
                    {
                        if ((derivedKeyToken.Length == derivationLength) &&
                            (CryptoHelper.IsEqual(derivedKeyToken.Nonce, derivationNonce)) && 
                            (derivedKeyToken.TokenToDerive.MatchesKeyIdentifierClause(keyIdentifierClause)))
                        {
                            // This is a implcit derived key for which we have already derived the
                            // token.
                            resolvedToken = this.tokens[i].Token;
                            alreadyDerived = true;
                            break;
                        }
                    }
                }

                if (!alreadyDerived)
                {
                    string psha1Algorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.securityHeader.StandardsManager.MessageSecurityVersion.SecureConversationVersion);

                    resolvedToken = new DerivedKeySecurityToken(-1, 0, derivationLength, null, derivationNonce, resolvedToken, keyIdentifierClause, psha1Algorithm, SecurityUtils.GenerateId());
                    ((DerivedKeySecurityToken)resolvedToken).InitializeDerivedKey(derivationLength);
                    Add(resolvedToken, SecurityTokenReferenceStyle.Internal, null);
                    this.securityHeader.EnsureDerivedKeyLimitNotReached();
                }
            }

            return resolvedToken;
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("SecurityTokenResolver");
                writer.WriteLine("    (");
                writer.WriteLine("    TokenCount = {0},", this.tokenCount);
                for (int i = 0; i < this.tokenCount; i++)
                {
                    writer.WriteLine("    TokenEntry[{0}] = (AllowedReferenceStyle={1}, Token={2}, Parameters={3})",
                        i, this.tokens[i].AllowedReferenceStyle, this.tokens[i].Token.GetType(), tokens[i].TokenParameters);
                }
                writer.WriteLine("    )");
                return writer.ToString();
            }
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            token = ResolveToken(keyIdentifier, false, true);
            return token != null;
        }

        internal bool TryResolveToken(SecurityKeyIdentifier keyIdentifier, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause, out SecurityToken token)
        {
            token = ResolveToken(keyIdentifier, matchOnlyExternalTokens, resolveIntrinsicKeyClause);
            return token != null;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            token = ResolveToken(keyIdentifierClause, false, true);
            return token != null;
        }

        internal bool TryResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause, out SecurityToken token)
        {
            token = ResolveToken(keyIdentifierClause, matchOnlyExternalTokens, resolveIntrinsicKeyClause);
            return token != null;
        }

        internal bool TryResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause, bool createIntrinsicKeys, out SecurityKey key)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            key = this.ResolveSecurityKeyCore(keyIdentifierClause, createIntrinsicKeys);
            return key != null;
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            key = ResolveSecurityKeyCore(keyIdentifierClause, true);
            return key != null;
        }

        struct SecurityTokenEntry
        {
            SecurityTokenParameters tokenParameters;
            SecurityToken token;
            SecurityTokenReferenceStyle allowedReferenceStyle;

            public SecurityTokenEntry(SecurityToken token, SecurityTokenParameters tokenParameters, SecurityTokenReferenceStyle allowedReferenceStyle)
            {
                this.token = token;
                this.tokenParameters = tokenParameters;
                this.allowedReferenceStyle = allowedReferenceStyle; 
            }

            public SecurityToken Token
            {
                get { return this.token; }
            }

            public SecurityTokenParameters TokenParameters
            {
                get { return this.tokenParameters; }
            }

            public SecurityTokenReferenceStyle AllowedReferenceStyle
            {
                get { return this.allowedReferenceStyle; }
            }
        }
    }
}
