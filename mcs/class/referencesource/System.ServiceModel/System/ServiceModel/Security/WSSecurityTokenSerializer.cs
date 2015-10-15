//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;

    public class WSSecurityTokenSerializer : SecurityTokenSerializer
    {
        const int DefaultMaximumKeyDerivationOffset = 64; // bytes 
        const int DefaultMaximumKeyDerivationLabelLength = 128; // bytes
        const int DefaultMaximumKeyDerivationNonceLength = 128; // bytes

        static WSSecurityTokenSerializer instance;
        readonly bool emitBspRequiredAttributes;
        readonly SecurityVersion securityVersion;
        readonly List<SerializerEntries> serializerEntries;
        WSSecureConversation secureConversation;
        readonly List<TokenEntry> tokenEntries;
        int maximumKeyDerivationOffset;
        int maximumKeyDerivationLabelLength;
        int maximumKeyDerivationNonceLength;

        KeyInfoSerializer keyInfoSerializer;

        public WSSecurityTokenSerializer()
            : this(SecurityVersion.WSSecurity11)
        {
        }

        public WSSecurityTokenSerializer(bool emitBspRequiredAttributes)
            : this(SecurityVersion.WSSecurity11, emitBspRequiredAttributes)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion)
            : this(securityVersion, false)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, bool emitBspRequiredAttributes)
            : this(securityVersion, emitBspRequiredAttributes, null)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer)
            : this(securityVersion, emitBspRequiredAttributes, samlSerializer, null, null)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes)
            : this(securityVersion, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, DefaultMaximumKeyDerivationOffset, DefaultMaximumKeyDerivationLabelLength, DefaultMaximumKeyDerivationNonceLength)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes)
            : this(securityVersion, trustVersion, secureConversationVersion, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, DefaultMaximumKeyDerivationOffset, DefaultMaximumKeyDerivationLabelLength, DefaultMaximumKeyDerivationNonceLength)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes,
            int maximumKeyDerivationOffset, int maximumKeyDerivationLabelLength, int maximumKeyDerivationNonceLength)
            : this(securityVersion, TrustVersion.Default, SecureConversationVersion.Default, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength)
        {
        }

        public WSSecurityTokenSerializer(SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes,
            int maximumKeyDerivationOffset, int maximumKeyDerivationLabelLength, int maximumKeyDerivationNonceLength)
        {
            if (securityVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("securityVersion"));

            if (maximumKeyDerivationOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (maximumKeyDerivationLabelLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationLabelLength", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (maximumKeyDerivationNonceLength <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationNonceLength", SR.GetString(SR.ValueMustBeGreaterThanZero)));
            }

            this.securityVersion = securityVersion;
            this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            this.maximumKeyDerivationOffset = maximumKeyDerivationOffset;
            this.maximumKeyDerivationNonceLength = maximumKeyDerivationNonceLength;
            this.maximumKeyDerivationLabelLength = maximumKeyDerivationLabelLength;

            this.serializerEntries = new List<SerializerEntries>();

            if (secureConversationVersion == SecureConversationVersion.WSSecureConversationFeb2005)
            {
                this.secureConversation = new WSSecureConversationFeb2005(this, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength);
            }
            else if (secureConversationVersion == SecureConversationVersion.WSSecureConversation13)
            {
                this.secureConversation = new WSSecureConversationDec2005(this, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            if (securityVersion == SecurityVersion.WSSecurity10)
            {
                this.serializerEntries.Add(new WSSecurityJan2004(this, samlSerializer));
            }
            else if (securityVersion == SecurityVersion.WSSecurity11)
            {
                this.serializerEntries.Add(new WSSecurityXXX2005(this, samlSerializer));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("securityVersion", SR.GetString(SR.MessageSecurityVersionOutOfRange)));
            }
            this.serializerEntries.Add(this.secureConversation);
            IdentityModel.TrustDictionary trustDictionary;
            if (trustVersion == TrustVersion.WSTrustFeb2005)
            {
                this.serializerEntries.Add(new WSTrustFeb2005(this));
                trustDictionary = new IdentityModel.TrustFeb2005Dictionary(new CollectionDictionary(DXD.TrustDec2005Dictionary.Feb2005DictionaryStrings));
            }
            else if (trustVersion == TrustVersion.WSTrust13)
            {
                this.serializerEntries.Add(new WSTrustDec2005(this));
                trustDictionary = new IdentityModel.TrustDec2005Dictionary(new CollectionDictionary(DXD.TrustDec2005Dictionary.Dec2005DictionaryString));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            this.tokenEntries = new List<TokenEntry>();

            for (int i = 0; i < this.serializerEntries.Count; ++i)
            {
                SerializerEntries serializerEntry = this.serializerEntries[i];
                serializerEntry.PopulateTokenEntries(this.tokenEntries);
            }

            IdentityModel.DictionaryManager dictionaryManager = new IdentityModel.DictionaryManager(ServiceModelDictionary.CurrentVersion);
            dictionaryManager.SecureConversationDec2005Dictionary = new IdentityModel.SecureConversationDec2005Dictionary(new CollectionDictionary(DXD.SecureConversationDec2005Dictionary.SecureConversationDictionaryStrings));
            dictionaryManager.SecurityAlgorithmDec2005Dictionary = new IdentityModel.SecurityAlgorithmDec2005Dictionary(new CollectionDictionary(DXD.SecurityAlgorithmDec2005Dictionary.SecurityAlgorithmDictionaryStrings));

            this.keyInfoSerializer = new WSKeyInfoSerializer(this.emitBspRequiredAttributes, dictionaryManager, trustDictionary, this, securityVersion, secureConversationVersion);
        }

        public static WSSecurityTokenSerializer DefaultInstance
        {
            get
            {
                if (instance == null)
                    instance = new WSSecurityTokenSerializer();
                return instance;
            }
        }

        public bool EmitBspRequiredAttributes
        {
            get { return this.emitBspRequiredAttributes; }
        }

        public SecurityVersion SecurityVersion
        {
            get { return this.securityVersion; }
        }

        public int MaximumKeyDerivationOffset
        {
            get { return this.maximumKeyDerivationOffset; }
        }

        public int MaximumKeyDerivationLabelLength
        {
            get { return this.maximumKeyDerivationLabelLength; }
        }

        public int MaximumKeyDerivationNonceLength
        {
            get { return this.maximumKeyDerivationNonceLength; }
        }

        internal WSSecureConversation SecureConversation
        {
            get { return this.secureConversation; }
        }

        bool ShouldWrapException(Exception e)
        {
            if (Fx.IsFatal(e))
            {
                return false;
            }
            return ((e is ArgumentException) || (e is FormatException) || (e is InvalidOperationException));
        }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry tokenEntry = this.tokenEntries[i];
                if (tokenEntry.CanReadTokenCore(localReader))
                    return true;
            }
            return false;
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry tokenEntry = this.tokenEntries[i];
                if (tokenEntry.CanReadTokenCore(localReader))
                {
                    try
                    {
                        return tokenEntry.ReadTokenCore(localReader, tokenResolver);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorDeserializingTokenXml), e));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CannotReadToken, reader.LocalName, reader.NamespaceURI, localReader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null))));
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry tokenEntry = this.tokenEntries[i];
                if (tokenEntry.SupportsCore(token.GetType()))
                    return true;
            }
            return false;
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            bool wroteToken = false;
            XmlDictionaryWriter localWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            if (token.GetType() == typeof(ProviderBackedSecurityToken))
            {
                token = (token as ProviderBackedSecurityToken).Token;
            }
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry tokenEntry = this.tokenEntries[i];
                if (tokenEntry.SupportsCore(token.GetType()))
                {
                    try
                    {
                        tokenEntry.WriteTokenCore(localWriter, token);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorSerializingSecurityToken), e));
                    }
                    wroteToken = true;
                    break;
                }
            }

            if (!wroteToken)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StandardsManagerCannotWriteObject, token.GetType())));

            localWriter.Flush();
        }

        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            try
            {
                return this.keyInfoSerializer.CanReadKeyIdentifier(reader);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            try
            {
                return this.keyInfoSerializer.ReadKeyIdentifier(reader);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            try
            {

                return this.keyInfoSerializer.CanWriteKeyIdentifier(keyIdentifier);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            try
            {
                this.keyInfoSerializer.WriteKeyIdentifier(writer, keyIdentifier);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            try
            {
                return this.keyInfoSerializer.CanReadKeyIdentifierClause(reader);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            try
            {
                return this.keyInfoSerializer.ReadKeyIdentifierClause(reader);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            try
            {
                return this.keyInfoSerializer.CanWriteKeyIdentifierClause(keyIdentifierClause);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            try
            {
                this.keyInfoSerializer.WriteKeyIdentifierClause(writer, keyIdentifierClause);
            }
            catch (System.IdentityModel.SecurityMessageSerializationException ex)
            {
                throw FxTrace.Exception.AsError(new MessageSecurityException(ex.Message));
            }
        }

        internal Type[] GetTokenTypes(string tokenTypeUri)
        {
            if (tokenTypeUri != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry tokenEntry = this.tokenEntries[i];

                    if (tokenEntry.SupportsTokenTypeUri(tokenTypeUri))
                    {
                        return tokenEntry.GetTokenTypes();
                    }
                }
            }
            return null;
        }

        protected internal virtual string GetTokenTypeUri(Type tokenType)
        {
            if (tokenType != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry tokenEntry = this.tokenEntries[i];

                    if (tokenEntry.SupportsCore(tokenType))
                    {
                        return tokenEntry.TokenTypeUri;
                    }
                }
            }
            return null;
        }

        public virtual bool TryCreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle, out SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (element == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

            securityKeyIdentifierClause = null;

            try
            {
                securityKeyIdentifierClause = CreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle);
            }
            catch (XmlException e)
            {
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.Security, SR.GetString(SR.TraceCodeSecurity), null, e);
                }
                return false;
            }

            return true;
        }

        public virtual SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            if (element == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry tokenEntry = this.tokenEntries[i];
                if (tokenEntry.CanReadTokenCore(element))
                {
                    try
                    {
                        return tokenEntry.CreateKeyIdentifierClauseFromTokenXmlCore(element, tokenReferenceStyle);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorDeserializingKeyIdentifierClauseFromTokenXml), e));
                    }
                }
            }

            // PreSharp 
#pragma warning suppress 56506
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CannotReadToken, element.LocalName, element.NamespaceURI, element.GetAttribute(SecurityJan2004Strings.ValueType, null))));
        }

        internal abstract new class TokenEntry
        {
            Type[] tokenTypes = null;
            public virtual IAsyncResult BeginReadTokenCore(XmlDictionaryReader reader,
                SecurityTokenResolver tokenResolver, AsyncCallback callback, object state)
            {
                SecurityToken result = this.ReadTokenCore(reader, tokenResolver);
                return new CompletedAsyncResult<SecurityToken>(result, callback, state);
            }

            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }
            public Type TokenType { get { return GetTokenTypes()[0]; } }
            public abstract string TokenTypeUri { get; }
            protected abstract string ValueTypeUri { get; }

            protected abstract Type[] GetTokenTypesCore();

            public Type[] GetTokenTypes()
            {
                if (this.tokenTypes == null)
                    this.tokenTypes = GetTokenTypesCore();
                return this.tokenTypes;
            }

            public bool SupportsCore(Type tokenType)
            {
                Type[] tokenTypes = GetTokenTypes();
                for (int i = 0; i < tokenTypes.Length; ++i)
                {
                    if (tokenTypes[i].IsAssignableFrom(tokenType))
                        return true;
                }
                return false;
            }

            public virtual bool SupportsTokenTypeUri(string tokenTypeUri)
            {
                return (this.TokenTypeUri == tokenTypeUri);
            }

            protected static SecurityKeyIdentifierClause CreateDirectReference(XmlElement issuedTokenXml, string idAttributeLocalName, string idAttributeNamespace, Type tokenType)
            {
                string id = issuedTokenXml.GetAttribute(idAttributeLocalName, idAttributeNamespace);
                if (id == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.RequiredAttributeMissing, idAttributeLocalName, issuedTokenXml.LocalName)));
                }
                return new LocalIdKeyIdentifierClause(id, tokenType);
            }

            public virtual bool CanReadTokenCore(XmlElement element)
            {
                string valueTypeUri = null;

                if (element.HasAttribute(SecurityJan2004Strings.ValueType, null))
                {
                    valueTypeUri = element.GetAttribute(SecurityJan2004Strings.ValueType, null);
                }

                return element.LocalName == LocalName.Value && element.NamespaceURI == NamespaceUri.Value && valueTypeUri == this.ValueTypeUri;
            }

            public virtual bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(this.LocalName, this.NamespaceUri) &&
                       reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) == this.ValueTypeUri;
            }

            public virtual SecurityToken EndReadTokenCore(IAsyncResult result)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }

            public abstract SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle);

            public abstract SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver);

            public abstract void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token);
        }

        internal abstract new class SerializerEntries
        {
            public virtual void PopulateTokenEntries(IList<TokenEntry> tokenEntries) { }
        }

        internal class CollectionDictionary : IXmlDictionary
        {
            List<XmlDictionaryString> dictionaryStrings;

            public CollectionDictionary(List<XmlDictionaryString> dictionaryStrings)
            {
                if (dictionaryStrings == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dictionaryStrings"));

                this.dictionaryStrings = dictionaryStrings;
            }

            public bool TryLookup(string value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

                for (int i = 0; i < this.dictionaryStrings.Count; ++i)
                {
                    if (this.dictionaryStrings[i].Value.Equals(value))
                    {
                        result = this.dictionaryStrings[i];
                        return true;
                    }
                }
                result = null;
                return false;
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                for (int i = 0; i < this.dictionaryStrings.Count; ++i)
                {
                    if (this.dictionaryStrings[i].Key == key)
                    {
                        result = this.dictionaryStrings[i];
                        return true;
                    }
                }
                result = null;
                return false;
            }

            public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

                for (int i = 0; i < this.dictionaryStrings.Count; ++i)
                {
                    if ((this.dictionaryStrings[i].Key == value.Key) &&
                        (this.dictionaryStrings[i].Value.Equals(value.Value)))
                    {
                        result = this.dictionaryStrings[i];
                        return true;
                    }
                }
                result = null;
                return false;
            }
        }

    }
}
