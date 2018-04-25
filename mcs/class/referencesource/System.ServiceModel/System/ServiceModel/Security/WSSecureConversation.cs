//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;

    abstract class WSSecureConversation : WSSecurityTokenSerializer.SerializerEntries
    {
        WSSecurityTokenSerializer tokenSerializer;
        DerivedKeyTokenEntry derivedKeyEntry;

        protected WSSecureConversation(WSSecurityTokenSerializer tokenSerializer, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength)
        {
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }
            this.tokenSerializer = tokenSerializer;
            this.derivedKeyEntry = new DerivedKeyTokenEntry(this, maxKeyDerivationOffset, maxKeyDerivationLabelLength, maxKeyDerivationNonceLength);
        }

        public abstract SecureConversationDictionary SerializerDictionary
        {
            get;
        }

        public WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get { return this.tokenSerializer; }
        }

        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            if (tokenEntryList == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenEntryList");
            }
            tokenEntryList.Add(this.derivedKeyEntry);
        }

        public virtual bool IsAtDerivedKeyToken(XmlDictionaryReader reader)
        {
            return this.derivedKeyEntry.CanReadTokenCore(reader);
        }

        public virtual void ReadDerivedKeyTokenParameters(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, out string id, out string derivationAlgorithm, out string label, out int length, out byte[] nonce, out int offset, out int generation, out SecurityKeyIdentifierClause tokenToDeriveIdentifier, out SecurityToken tokenToDerive)
        {
            this.derivedKeyEntry.ReadDerivedKeyTokenParameters(reader, tokenResolver, out id, out derivationAlgorithm, out label,
                out length, out nonce, out offset, out generation, out tokenToDeriveIdentifier, out tokenToDerive);
        }

        public virtual SecurityToken CreateDerivedKeyToken(string id, string derivationAlgorithm, string label, int length, byte[] nonce, int offset, int generation, SecurityKeyIdentifierClause tokenToDeriveIdentifier, SecurityToken tokenToDerive)
        {
            return this.derivedKeyEntry.CreateDerivedKeyToken(id, derivationAlgorithm, label, length, nonce, offset, generation,
                tokenToDeriveIdentifier, tokenToDerive);
        }

        public virtual string DerivationAlgorithm
        {
            get { return SecurityAlgorithms.Psha1KeyDerivation; }
        }

        protected class DerivedKeyTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            public const string DefaultLabel = "WS-SecureConversation";

            WSSecureConversation parent;
            int maxKeyDerivationOffset;
            int maxKeyDerivationLabelLength;
            int maxKeyDerivationNonceLength;

            public DerivedKeyTokenEntry(WSSecureConversation parent, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength)
            {
                if (parent == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
                }
                this.parent = parent;
                this.maxKeyDerivationOffset = maxKeyDerivationOffset;
                this.maxKeyDerivationLabelLength = maxKeyDerivationLabelLength;
                this.maxKeyDerivationNonceLength = maxKeyDerivationNonceLength;
            }

            protected override XmlDictionaryString LocalName { get { return parent.SerializerDictionary.DerivedKeyToken; } }
            protected override XmlDictionaryString NamespaceUri { get { return parent.SerializerDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(DerivedKeySecurityToken) }; }
            public override string TokenTypeUri { get { return parent.SerializerDictionary.DerivedKeyTokenType.Value; } }
            protected override string ValueTypeUri { get { return null; } }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, UtilityStrings.IdAttribute, UtilityStrings.Namespace, typeof(DerivedKeySecurityToken));
                    case SecurityTokenReferenceStyle.External:
                        // DerivedKeys aren't referred to externally
                        return null;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }

            }

            // xml format
            //<DerivedKeyToken wsu:Id="..." wsse:Algorithm="..."> id required, alg optional (curr disallowed)
            //  <SecurityTokenReference>...</SecurityTokenReference> - required
            //  <Properties>...</Properties> - disallowed (optional in spec, but we disallow it)
            // choice begin - (schema requires a choice - we allow neither on read - we always write one)
            //  <Generation>...</Generation> - optional
            //  <Offset>...</Offset> - optional
            // choice end
            //  <Length>...</Length> - optional - default 32 on read (default specified in spec, not in schema - we always write it)
            //  <Label>...</Label> - optional
            //  <Nonce>...</Nonce> - required (optional in spec, but we require it)
            //</DerivedKeyToken>
            public virtual void ReadDerivedKeyTokenParameters(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, out string id, out string derivationAlgorithm, out string label, out int length, out byte[] nonce, out int offset, out int generation, out SecurityKeyIdentifierClause tokenToDeriveIdentifier, out SecurityToken tokenToDerive)
            {
                if (tokenResolver == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
                }

                id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);

                derivationAlgorithm = reader.GetAttribute(XD.XmlSignatureDictionary.Algorithm, null);
                if (derivationAlgorithm == null)
                {
                    derivationAlgorithm = parent.DerivationAlgorithm;
                }

                reader.ReadStartElement();

                tokenToDeriveIdentifier = null;
                tokenToDerive = null;

                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    tokenToDeriveIdentifier = parent.WSSecurityTokenSerializer.ReadKeyIdentifierClause(reader);
                    tokenResolver.TryResolveToken(tokenToDeriveIdentifier, out tokenToDerive);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.DerivedKeyTokenRequiresTokenReference)));
                }

                // no support for properties

                generation = -1;
                if (reader.IsStartElement(parent.SerializerDictionary.Generation, parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    generation = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                    if (generation < 0)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.DerivedKeyInvalidGenerationSpecified, generation)));
                }

                offset = -1;
                if (reader.IsStartElement(parent.SerializerDictionary.Offset, parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    offset = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                    if (offset < 0)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.DerivedKeyInvalidOffsetSpecified, offset)));
                }

                length = DerivedKeySecurityToken.DefaultDerivedKeyLength;
                if (reader.IsStartElement(parent.SerializerDictionary.Length, parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    length = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                }

                if ((offset == -1) && (generation == -1))
                    offset = 0;

                // verify that the offset is not larger than the max allowed
                DerivedKeySecurityToken.EnsureAcceptableOffset(offset, generation, length, this.maxKeyDerivationOffset);

                label = null;
                if (reader.IsStartElement(parent.SerializerDictionary.Label, parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    label = reader.ReadString();
                    reader.ReadEndElement();
                }
                if (label != null && label.Length > this.maxKeyDerivationLabelLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.DerivedKeyTokenLabelTooLong, label.Length, this.maxKeyDerivationLabelLength)));
                }

                nonce = null;
                reader.ReadStartElement(parent.SerializerDictionary.Nonce, parent.SerializerDictionary.Namespace);
                nonce = reader.ReadContentAsBase64();
                reader.ReadEndElement();

                if (nonce != null && nonce.Length > this.maxKeyDerivationNonceLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.DerivedKeyTokenNonceTooLong, nonce.Length, this.maxKeyDerivationNonceLength)));
                }

                reader.ReadEndElement();
            }

            public virtual SecurityToken CreateDerivedKeyToken(string id, string derivationAlgorithm, string label, int length, byte[] nonce, int offset, int generation, SecurityKeyIdentifierClause tokenToDeriveIdentifier, SecurityToken tokenToDerive)
            {
                if (tokenToDerive == null)
                {
                    return new DerivedKeySecurityTokenStub(generation, offset, length,
                        label, nonce, tokenToDeriveIdentifier, derivationAlgorithm, id);
                }
                else
                {
                    return new DerivedKeySecurityToken(generation, offset, length,
                        label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm, id);
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
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
                this.ReadDerivedKeyTokenParameters(reader, tokenResolver, out id, out derivationAlgorithm, out label, out length,
                    out nonce, out offset, out generation, out tokenToDeriveIdentifier, out tokenToDerive);

                return CreateDerivedKeyToken(id, derivationAlgorithm, label, length, nonce, offset, generation,
                    tokenToDeriveIdentifier, tokenToDerive);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                DerivedKeySecurityToken derivedKeyToken = token as DerivedKeySecurityToken;
                string serializerPrefix = parent.SerializerDictionary.Prefix.Value;

                writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.DerivedKeyToken, parent.SerializerDictionary.Namespace);
                if (derivedKeyToken.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, derivedKeyToken.Id);
                }
                if (derivedKeyToken.KeyDerivationAlgorithm != parent.DerivationAlgorithm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnsupportedKeyDerivationAlgorithm, derivedKeyToken.KeyDerivationAlgorithm)));
                }
                parent.WSSecurityTokenSerializer.WriteKeyIdentifierClause(writer, derivedKeyToken.TokenToDeriveIdentifier);

                // Don't support Properties element
                if (derivedKeyToken.Generation > 0 || derivedKeyToken.Offset > 0 || derivedKeyToken.Length != 32)
                {
                    // this means they're both specified (offset must be gen * length) - we'll write generation
                    if (derivedKeyToken.Generation >= 0 && derivedKeyToken.Offset >= 0)
                    {
                        writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Generation, parent.SerializerDictionary.Namespace);
                        writer.WriteValue(derivedKeyToken.Generation);
                        writer.WriteEndElement();
                    }
                    else if (derivedKeyToken.Generation != -1)
                    {
                        writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Generation, parent.SerializerDictionary.Namespace);
                        writer.WriteValue(derivedKeyToken.Generation);
                        writer.WriteEndElement();
                    }
                    else if (derivedKeyToken.Offset != -1)
                    {
                        writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Offset, parent.SerializerDictionary.Namespace);
                        writer.WriteValue(derivedKeyToken.Offset);
                        writer.WriteEndElement();
                    }

                    if (derivedKeyToken.Length != 32)
                    {
                        writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Length, parent.SerializerDictionary.Namespace);
                        writer.WriteValue(derivedKeyToken.Length);
                        writer.WriteEndElement();
                    }
                }

                if (derivedKeyToken.Label != null)
                {
                    writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Generation, parent.SerializerDictionary.Namespace);
                    writer.WriteString(derivedKeyToken.Label);
                    writer.WriteEndElement();
                }
                writer.WriteStartElement(serializerPrefix, parent.SerializerDictionary.Nonce, parent.SerializerDictionary.Namespace);
                writer.WriteBase64(derivedKeyToken.Nonce, 0, derivedKeyToken.Nonce.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        protected abstract class SecurityContextTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            WSSecureConversation parent;
            SecurityContextCookieSerializer cookieSerializer;

            public SecurityContextTokenEntry(WSSecureConversation parent, SecurityStateEncoder securityStateEncoder, IList<Type> knownClaimTypes)
            {
                this.parent = parent;
                this.cookieSerializer = new SecurityContextCookieSerializer(securityStateEncoder, knownClaimTypes);
            }

            protected WSSecureConversation Parent
            {
                get { return this.parent; }
            }

            protected override XmlDictionaryString LocalName { get { return parent.SerializerDictionary.SecurityContextToken; } }
            protected override XmlDictionaryString NamespaceUri { get { return parent.SerializerDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(SecurityContextSecurityToken) }; }
            public override string TokenTypeUri { get { return parent.SerializerDictionary.SecurityContextTokenType.Value; } }
            protected override string ValueTypeUri { get { return null; } }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {

                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, UtilityStrings.IdAttribute, UtilityStrings.Namespace, typeof(SecurityContextSecurityToken));
                    case SecurityTokenReferenceStyle.External:
                        UniqueId contextId = null;
                        UniqueId generation = null;
                        foreach (XmlNode node in issuedTokenXml.ChildNodes)
                        {
                            XmlElement element = node as XmlElement;
                            if (element != null)
                            {
                                if (element.LocalName == parent.SerializerDictionary.Identifier.Value && element.NamespaceURI == parent.SerializerDictionary.Namespace.Value)
                                {
                                    contextId = XmlHelper.ReadTextElementAsUniqueId(element);
                                }
                                else if (CanReadGeneration(element))
                                {
                                    generation = ReadGeneration(element);
                                }
                            }
                        }
                        return new SecurityContextKeyIdentifierClause(contextId, generation);
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            protected abstract bool CanReadGeneration(XmlDictionaryReader reader);
            protected abstract bool CanReadGeneration(XmlElement element);
            protected abstract UniqueId ReadGeneration(XmlDictionaryReader reader);
            protected abstract UniqueId ReadGeneration(XmlElement element);

            SecurityContextSecurityToken TryResolveSecurityContextToken(UniqueId contextId, UniqueId generation, string id, SecurityTokenResolver tokenResolver, out ISecurityContextSecurityTokenCache sctCache)
            {
                SecurityContextSecurityToken cachedSct = null;
                sctCache = null;
                if (tokenResolver is ISecurityContextSecurityTokenCache)
                {
                    sctCache = ((ISecurityContextSecurityTokenCache)tokenResolver);
                    cachedSct = sctCache.GetContext(contextId, generation);
                }
                else if (tokenResolver is AggregateSecurityHeaderTokenResolver)
                {
                    // We will see if we have a ISecurityContextSecurityTokenCache in the 
                    // AggregateTokenResolver. We will hold the reference to the first sctCache
                    // we find.
                    AggregateSecurityHeaderTokenResolver aggregateTokenResolve = tokenResolver as AggregateSecurityHeaderTokenResolver;
                    for (int i = 0; i < aggregateTokenResolve.TokenResolvers.Count; ++i)
                    {
                        ISecurityContextSecurityTokenCache oobTokenResolver = aggregateTokenResolve.TokenResolvers[i] as ISecurityContextSecurityTokenCache;
                        if (oobTokenResolver == null)
                        {
                            continue;
                        }
                        if (sctCache == null)
                        {
                            sctCache = oobTokenResolver;
                        }
                        cachedSct = oobTokenResolver.GetContext(contextId, generation);
                        if (cachedSct != null)
                        {
                            break;
                        }
                    }
                }
                if (cachedSct == null)
                {
                    return null;
                }
                else if (cachedSct.Id == id)
                {
                    return cachedSct;
                }
                else
                {
                    return new SecurityContextSecurityToken(cachedSct, id);
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                UniqueId contextId = null;
                byte[] encodedCookie = null;
                UniqueId generation = null;
                bool isCookieMode = false;

                Fx.Assert(reader.NodeType == XmlNodeType.Element, "");

                // check if there is an id
                string id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);

                SecurityContextSecurityToken sct = null;

                // There needs to be at least a contextId in here.
                reader.ReadFullStartElement();
                reader.MoveToStartElement(parent.SerializerDictionary.Identifier, parent.SerializerDictionary.Namespace);
                contextId = reader.ReadElementContentAsUniqueId();
                if (CanReadGeneration(reader))
                {
                    generation = ReadGeneration(reader);
                }
                if (reader.IsStartElement(parent.SerializerDictionary.Cookie, XD.DotNetSecurityDictionary.Namespace))
                {
                    isCookieMode = true;
                    ISecurityContextSecurityTokenCache sctCache;
                    sct = TryResolveSecurityContextToken(contextId, generation, id, tokenResolver, out sctCache);
                    if (sct == null)
                    {
                        encodedCookie = reader.ReadElementContentAsBase64();
                        if (encodedCookie != null)
                        {
                            sct = cookieSerializer.CreateSecurityContextFromCookie(encodedCookie, contextId, generation, id, reader.Quotas);
                            if (sctCache != null)
                            {
                                sctCache.AddContext(sct);
                            }
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();

                if (contextId == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoSecurityContextIdentifier)));
                }

                if (sct == null && !isCookieMode)
                {
                    ISecurityContextSecurityTokenCache sctCache;
                    sct = TryResolveSecurityContextToken(contextId, generation, id, tokenResolver, out sctCache);
                }
                if (sct == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityContextTokenValidationException(SR.GetString(SR.SecurityContextNotRegistered, contextId, generation)));
                }
                return sct;
            }

            protected virtual void WriteGeneration(XmlDictionaryWriter writer, SecurityContextSecurityToken sct)
            {
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                SecurityContextSecurityToken sct = (token as SecurityContextSecurityToken);

                // serialize the name and any wsu:Id attribute
                writer.WriteStartElement(parent.SerializerDictionary.Prefix.Value, parent.SerializerDictionary.SecurityContextToken, parent.SerializerDictionary.Namespace);
                if (sct.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, sct.Id);
                }

                // serialize the context id
                writer.WriteStartElement(parent.SerializerDictionary.Prefix.Value, parent.SerializerDictionary.Identifier, parent.SerializerDictionary.Namespace);
                XmlHelper.WriteStringAsUniqueId(writer, sct.ContextId);
                writer.WriteEndElement();

                WriteGeneration(writer, sct);

                // if cookie-mode, then it must have a cookie
                if (sct.IsCookieMode)
                {
                    if (sct.CookieBlob == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoCookieInSct)));
                    }

                    // if the token has a cookie, write it out
                    writer.WriteStartElement(XD.DotNetSecurityDictionary.Prefix.Value, parent.SerializerDictionary.Cookie, XD.DotNetSecurityDictionary.Namespace);
                    writer.WriteBase64(sct.CookieBlob, 0, sct.CookieBlob.Length);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        public abstract class Driver : SecureConversationDriver
        {
            public Driver()
            {
            }

            protected abstract SecureConversationDictionary DriverDictionary
            {
                get;
            }

            public override XmlDictionaryString IssueAction
            {
                get
                {
                    return DriverDictionary.RequestSecurityContextIssuance;
                }
            }

            public override XmlDictionaryString IssueResponseAction
            {
                get
                {
                    return DriverDictionary.RequestSecurityContextIssuanceResponse;
                }
            }

            public override XmlDictionaryString RenewNeededFaultCode
            {
                get { return DriverDictionary.RenewNeededFaultCode; }
            }

            public override XmlDictionaryString BadContextTokenFaultCode
            {
                get { return DriverDictionary.BadContextTokenFaultCode; }
            }

            public override UniqueId GetSecurityContextTokenId(XmlDictionaryReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                reader.ReadStartElement(DriverDictionary.SecurityContextToken, DriverDictionary.Namespace);
                UniqueId contextId = XmlHelper.ReadElementStringAsUniqueId(reader, DriverDictionary.Identifier, DriverDictionary.Namespace);
                while (reader.IsStartElement())
                {
                    reader.Skip();
                }
                reader.ReadEndElement();
                return contextId;
            }

            public override bool IsAtSecurityContextToken(XmlDictionaryReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                return reader.IsStartElement(DriverDictionary.SecurityContextToken, DriverDictionary.Namespace);
            }
        }
    }
}
