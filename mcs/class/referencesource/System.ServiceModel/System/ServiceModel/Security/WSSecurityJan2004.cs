//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using KeyIdentifierClauseEntry = WSSecurityTokenSerializer.KeyIdentifierClauseEntry;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;

    class WSSecurityJan2004 : WSSecurityTokenSerializer.SerializerEntries
    {
        WSSecurityTokenSerializer tokenSerializer;
        SamlSerializer samlSerializer;

        public WSSecurityJan2004(WSSecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer)
        {
            this.tokenSerializer = tokenSerializer;
            this.samlSerializer = samlSerializer;
        }

        public WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get { return this.tokenSerializer; }
        }

        public SamlSerializer SamlSerializer
        {
            get { return this.samlSerializer; }
        }

        protected void PopulateJan2004TokenEntries(IList<TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new GenericXmlTokenEntry());
            tokenEntryList.Add(new UserNamePasswordTokenEntry(this.tokenSerializer));
            tokenEntryList.Add(new KerberosTokenEntry(this.tokenSerializer));
            tokenEntryList.Add(new X509TokenEntry(this.tokenSerializer));
        }

        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            PopulateJan2004TokenEntries(tokenEntryList);
            tokenEntryList.Add(new SamlTokenEntry(this.tokenSerializer, this.samlSerializer));
            tokenEntryList.Add(new WrappedKeyTokenEntry(this.tokenSerializer));
        }

        internal abstract class BinaryTokenEntry : TokenEntry
        {
            internal static readonly XmlDictionaryString ElementName = XD.SecurityJan2004Dictionary.BinarySecurityToken;
            internal static readonly XmlDictionaryString EncodingTypeAttribute = XD.SecurityJan2004Dictionary.EncodingType;
            internal const string EncodingTypeAttributeString = SecurityJan2004Strings.EncodingType;
            internal const string EncodingTypeValueBase64Binary = SecurityJan2004Strings.EncodingTypeValueBase64Binary;
            internal const string EncodingTypeValueHexBinary = SecurityJan2004Strings.EncodingTypeValueHexBinary;
            internal static readonly XmlDictionaryString ValueTypeAttribute = XD.SecurityJan2004Dictionary.ValueType;

            WSSecurityTokenSerializer tokenSerializer;
            string[] valueTypeUris = null;

            protected BinaryTokenEntry(WSSecurityTokenSerializer tokenSerializer, string valueTypeUri)
            {
                this.tokenSerializer = tokenSerializer;
                this.valueTypeUris = new string[1];
                this.valueTypeUris[0] = valueTypeUri;
            }

            protected BinaryTokenEntry(WSSecurityTokenSerializer tokenSerializer, string[] valueTypeUris)
            {
                if (valueTypeUris == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("valueTypeUris");

                this.tokenSerializer = tokenSerializer;
                this.valueTypeUris = new string[valueTypeUris.GetLength(0)];
                for (int i = 0; i < this.valueTypeUris.GetLength(0); ++i)
                    this.valueTypeUris[i] = valueTypeUris[i];
            }

            protected override XmlDictionaryString LocalName { get { return ElementName; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.SecurityJan2004Dictionary.Namespace; } }
            public override string TokenTypeUri { get { return this.valueTypeUris[0]; } }
            protected override string ValueTypeUri { get { return this.valueTypeUris[0]; } }
            public override bool SupportsTokenTypeUri(string tokenTypeUri)
            {
                for (int i = 0; i < this.valueTypeUris.GetLength(0); ++i)
                {
                    if (this.valueTypeUris[i] == tokenTypeUri)
                        return true;
                }

                return false;
            }

            public abstract SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData);

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, UtilityStrings.IdAttribute, UtilityStrings.Namespace, this.TokenType);
                    case SecurityTokenReferenceStyle.External:
                        string encoding = issuedTokenXml.GetAttribute(EncodingTypeAttributeString, null);
                        string encodedData = issuedTokenXml.InnerText;

                        byte[] binaryData;
                        if (encoding == null || encoding == EncodingTypeValueBase64Binary)
                        {
                            binaryData = Convert.FromBase64String(encodedData);
                        }
                        else if (encoding == EncodingTypeValueHexBinary)
                        {
                            binaryData = HexBinary.Parse(encodedData).Value;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnknownEncodingInBinarySecurityToken)));
                        }

                        return CreateKeyIdentifierClauseFromBinaryCore(binaryData);
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            public abstract SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData);

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string wsuId = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                string valueTypeUri = reader.GetAttribute(ValueTypeAttribute, null);
                string encoding = reader.GetAttribute(EncodingTypeAttribute, null);

                byte[] binaryData;
                if (encoding == null || encoding == EncodingTypeValueBase64Binary)
                {
                    binaryData = reader.ReadElementContentAsBase64();
                }
                else if (encoding == EncodingTypeValueHexBinary)
                {
                    binaryData = HexBinary.Parse(reader.ReadElementContentAsString()).Value;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnknownEncodingInBinarySecurityToken)));
                }

                return ReadBinaryCore(wsuId, valueTypeUri, binaryData);
            }

            public abstract void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData);

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                string id;
                byte[] rawData;

                WriteBinaryCore(token, out id, out rawData);

                if (rawData == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rawData");
                }

                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, ElementName, XD.SecurityJan2004Dictionary.Namespace);
                if (id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
                }
                if (valueTypeUris != null)
                {
                    writer.WriteAttributeString(ValueTypeAttribute, null, this.valueTypeUris[0]);
                }
                if (this.tokenSerializer.EmitBspRequiredAttributes)
                {
                    writer.WriteAttributeString(EncodingTypeAttribute, null, EncodingTypeValueBase64Binary);
                }
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement(); // BinarySecurityToken
            }
        }

        class GenericXmlTokenEntry : TokenEntry
        {
            protected override XmlDictionaryString LocalName { get { return null; } }
            protected override XmlDictionaryString NamespaceUri { get { return null; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(GenericXmlSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }

            public GenericXmlTokenEntry()
            {
            }


            public override bool CanReadTokenCore(XmlElement element)
            {
                return false;
            }

            public override bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                return false;
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                BufferedGenericXmlSecurityToken bufferedXmlToken = token as BufferedGenericXmlSecurityToken;
                if (bufferedXmlToken != null && bufferedXmlToken.TokenXmlBuffer != null)
                {
                    using (XmlDictionaryReader reader = bufferedXmlToken.TokenXmlBuffer.GetReader(0))
                    {
                        writer.WriteNode(reader, false);
                    }
                }
                else
                {
                    GenericXmlSecurityToken xmlToken = (GenericXmlSecurityToken)token;
                    xmlToken.TokenXml.WriteTo(writer);
                }
            }
        }

        class KerberosTokenEntry : BinaryTokenEntry
        {
            public KerberosTokenEntry(WSSecurityTokenSerializer tokenSerializer)
                : base(tokenSerializer, new string[] { SecurityJan2004Strings.KerberosTokenTypeGSS, SecurityJan2004Strings.KerberosTokenType1510 })
            {
            }

            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(KerberosReceiverSecurityToken), typeof(KerberosRequestorSecurityToken) }; }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData)
            {
                byte[] tokenHash;
                using (HashAlgorithm hasher = CryptoHelper.NewSha1HashAlgorithm())
                {
                    tokenHash = hasher.ComputeHash(rawData, 0, rawData.Length);
                }
                return new KerberosTicketHashKeyIdentifierClause(tokenHash);
            }

            public override SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData)
            {
                return new KerberosReceiverSecurityToken(rawData, id, false, valueTypeUri);
            }

            public override void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData)
            {
                KerberosRequestorSecurityToken kerbToken = (KerberosRequestorSecurityToken)token;
                id = token.Id;
                rawData = kerbToken.GetRequest();
            }
        }

        protected class SamlTokenEntry : TokenEntry
        {
            const string samlAssertionId = "AssertionID";
            SamlSerializer samlSerializer;
            SecurityTokenSerializer tokenSerializer;

            public SamlTokenEntry(SecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer)
            {
                this.tokenSerializer = tokenSerializer;
                if (samlSerializer != null)
                {
                    this.samlSerializer = samlSerializer;
                }
                else
                {
                    this.samlSerializer = new SamlSerializer();
                }
                this.samlSerializer.PopulateDictionary(BinaryMessageEncoderFactory.XmlDictionary);
            }

            protected override XmlDictionaryString LocalName { get { return XD.SecurityJan2004Dictionary.SamlAssertion; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.SecurityJan2004Dictionary.SamlUri; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(SamlSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    // SAML uses same reference for internal and external
                    case SecurityTokenReferenceStyle.Internal:
                    case SecurityTokenReferenceStyle.External:
                        string assertionId = issuedTokenXml.GetAttribute(samlAssertionId);
                        return new SamlAssertionKeyIdentifierClause(assertionId);
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                SamlSecurityToken samlToken = this.samlSerializer.ReadToken(reader, this.tokenSerializer, tokenResolver);
                return samlToken;
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                SamlSecurityToken samlToken = token as SamlSecurityToken;
                this.samlSerializer.WriteToken(samlToken, writer, this.tokenSerializer);
            }
        }

        class UserNamePasswordTokenEntry : TokenEntry
        {
            WSSecurityTokenSerializer tokenSerializer;

            public UserNamePasswordTokenEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            protected override XmlDictionaryString LocalName { get { return XD.SecurityJan2004Dictionary.UserNameTokenElement; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.SecurityJan2004Dictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(UserNameSecurityToken) }; }
            public override string TokenTypeUri { get { return SecurityJan2004Strings.UPTokenType; } }
            protected override string ValueTypeUri { get { return null; } }

            public override IAsyncResult BeginReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, AsyncCallback callback, object state)
            {
                string id;
                string userName;
                string password;

                ParseToken(reader, out id, out userName, out password);

                SecurityToken token = new UserNameSecurityToken(userName, password, id);
                return new CompletedAsyncResult<SecurityToken>(token, callback, state);
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, UtilityStrings.IdAttribute, UtilityStrings.Namespace, typeof(UserNameSecurityToken));
                    case SecurityTokenReferenceStyle.External:
                        // UP tokens aren't referred to externally
                        return null;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            public override SecurityToken EndReadTokenCore(IAsyncResult result)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string id;
                string userName;
                string password;

                ParseToken(reader, out id, out userName, out password);

                if (id == null)
                    id = SecurityUniqueId.Create().Value;

                return new UserNameSecurityToken(userName, password, id);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                UserNameSecurityToken upToken = (UserNameSecurityToken)token;
                WriteUserNamePassword(writer, upToken.Id, upToken.UserName, upToken.Password);
            }

            void WriteUserNamePassword(XmlDictionaryWriter writer, string id, string userName, string password)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.UserNameTokenElement,
                    XD.SecurityJan2004Dictionary.Namespace); // <wsse:UsernameToken
                writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute,
                    XD.UtilityDictionary.Namespace, id); // wsu:Id="..."
                writer.WriteElementString(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.UserNameElement,
                    XD.SecurityJan2004Dictionary.Namespace, userName); // ><wsse:Username>...</wsse:Username>
                if (password != null)
                {
                    writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.PasswordElement,
                        XD.SecurityJan2004Dictionary.Namespace);
                    if (this.tokenSerializer.EmitBspRequiredAttributes)
                    {
                        writer.WriteAttributeString(XD.SecurityJan2004Dictionary.TypeAttribute, null, SecurityJan2004Strings.UPTokenPasswordTextValue);
                    }
                    writer.WriteString(password); // <wsse:Password>...</wsse:Password>
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // </wsse:UsernameToken>
            }

            static string ParsePassword(XmlDictionaryReader reader)
            {
                string type = reader.GetAttribute(XD.SecurityJan2004Dictionary.TypeAttribute, null);
                if (type != null && type.Length > 0 && type != SecurityJan2004Strings.UPTokenPasswordTextValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedPasswordType, type)));
                }

                return reader.ReadElementString();
            }

            static void ParseToken(XmlDictionaryReader reader, out string id, out string userName, out string password)
            {
                id = null;
                userName = null;
                password = null;

                reader.MoveToContent();
                id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);

                reader.ReadStartElement(XD.SecurityJan2004Dictionary.UserNameTokenElement, XD.SecurityJan2004Dictionary.Namespace);
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(XD.SecurityJan2004Dictionary.UserNameElement, XD.SecurityJan2004Dictionary.Namespace))
                    {
                        userName = reader.ReadElementString();
                    }
                    else if (reader.IsStartElement(XD.SecurityJan2004Dictionary.PasswordElement, XD.SecurityJan2004Dictionary.Namespace))
                    {
                        password = ParsePassword(reader);
                    }
                    else if (reader.IsStartElement(XD.SecurityJan2004Dictionary.NonceElement, XD.SecurityJan2004Dictionary.Namespace))
                    {
                        // Nonce can be safely ignored
                        reader.Skip();
                    }
                    else if (reader.IsStartElement(XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace))
                    {
                        // wsu:Created can be safely ignored
                        reader.Skip();
                    }
                    else
                    {
                        XmlHelper.OnUnexpectedChildNodeError(SecurityJan2004Strings.UserNameTokenElement, reader);
                    }
                }
                reader.ReadEndElement();

                if (userName == null)
                    XmlHelper.OnRequiredElementMissing(SecurityJan2004Strings.UserNameElement, SecurityJan2004Strings.Namespace);
            }
        }

        protected class WrappedKeyTokenEntry : TokenEntry
        {
            WSSecurityTokenSerializer tokenSerializer;

            public WrappedKeyTokenEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            protected override XmlDictionaryString LocalName { get { return EncryptedKey.ElementName; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.XmlEncryptionDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(WrappedKeySecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {

                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, XmlEncryptionStrings.Id, null, null);
                    case SecurityTokenReferenceStyle.External:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CantInferReferenceForToken, EncryptedKey.ElementName.Value)));
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                EncryptedKey encryptedKey = new EncryptedKey();
                encryptedKey.SecurityTokenSerializer = this.tokenSerializer;
                encryptedKey.ReadFrom(reader);
                SecurityKeyIdentifier unwrappingTokenIdentifier = encryptedKey.KeyIdentifier;
                byte[] wrappedKey = encryptedKey.GetWrappedKey();
                WrappedKeySecurityToken wrappedKeyToken = CreateWrappedKeyToken(encryptedKey.Id, encryptedKey.EncryptionMethod,
                    encryptedKey.CarriedKeyName, unwrappingTokenIdentifier, wrappedKey, tokenResolver);
                wrappedKeyToken.EncryptedKey = encryptedKey;

                return wrappedKeyToken;
            }

            WrappedKeySecurityToken CreateWrappedKeyToken(string id, string encryptionMethod, string carriedKeyName,
                SecurityKeyIdentifier unwrappingTokenIdentifier, byte[] wrappedKey, SecurityTokenResolver tokenResolver)
            {
                ISspiNegotiationInfo sspiResolver = tokenResolver as ISspiNegotiationInfo;
                if (sspiResolver != null)
                {
                    ISspiNegotiation unwrappingSspiContext = sspiResolver.SspiNegotiation;
                    // ensure that the encryption algorithm is compatible
                    if (encryptionMethod != unwrappingSspiContext.KeyEncryptionAlgorithm)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.BadKeyEncryptionAlgorithm, encryptionMethod)));
                    }
                    byte[] unwrappedKey = unwrappingSspiContext.Decrypt(wrappedKey);
                    return new WrappedKeySecurityToken(id, unwrappedKey, encryptionMethod, unwrappingSspiContext, unwrappedKey);
                }
                else
                {
                    if (tokenResolver == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("tokenResolver"));
                    }
                    if (unwrappingTokenIdentifier == null || unwrappingTokenIdentifier.Count == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MissingKeyInfoInEncryptedKey)));
                    }

                    SecurityToken unwrappingToken;
                    SecurityHeaderTokenResolver resolver = tokenResolver as SecurityHeaderTokenResolver;
                    if (resolver != null)
                    {
                        unwrappingToken = resolver.ExpectedWrapper;
                        if (unwrappingToken != null)
                        {
                            if (!resolver.CheckExternalWrapperMatch(unwrappingTokenIdentifier))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                    SR.GetString(SR.EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken, unwrappingToken)));
                            }
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.UnableToResolveKeyInfoForUnwrappingToken, unwrappingTokenIdentifier, resolver)));
                        }
                    }
                    else
                    {
                        try
                        {
                            unwrappingToken = tokenResolver.ResolveToken(unwrappingTokenIdentifier);
                        }
                        catch (Exception exception)
                        {
                            if (exception is MessageSecurityException)
                                throw;

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.UnableToResolveKeyInfoForUnwrappingToken, unwrappingTokenIdentifier, tokenResolver), exception));
                        }
                    }
                    SecurityKey unwrappingSecurityKey;
                    byte[] unwrappedKey = SecurityUtils.DecryptKey(unwrappingToken, encryptionMethod, wrappedKey, out unwrappingSecurityKey);
                    return new WrappedKeySecurityToken(id, unwrappedKey, encryptionMethod, unwrappingToken, unwrappingTokenIdentifier, wrappedKey, unwrappingSecurityKey);
                }
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                WrappedKeySecurityToken wrappedKeyToken = token as WrappedKeySecurityToken;
                wrappedKeyToken.EnsureEncryptedKeySetUp();
                wrappedKeyToken.EncryptedKey.SecurityTokenSerializer = this.tokenSerializer;
                wrappedKeyToken.EncryptedKey.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }
        }

        protected class X509TokenEntry : BinaryTokenEntry
        {
            internal const string ValueTypeAbsoluteUri = SecurityJan2004Strings.X509TokenType;

            public X509TokenEntry(WSSecurityTokenSerializer tokenSerializer)
                : base(tokenSerializer, ValueTypeAbsoluteUri)
            {
            }

            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(X509SecurityToken), typeof(X509WindowsSecurityToken) }; }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CantInferReferenceForToken, ValueTypeAbsoluteUri)));
            }

            public override SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData)
            {
                X509Certificate2 certificate;
                if (!SecurityUtils.TryCreateX509CertificateFromRawData(rawData, out certificate))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.InvalidX509RawData)));
                }
                return new X509SecurityToken(certificate, id, false);
            }

            public override void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData)
            {
                id = token.Id;
                X509SecurityToken x509Token = token as X509SecurityToken;
                if (x509Token != null)
                {
                    rawData = x509Token.Certificate.GetRawCertData();
                }
                else
                {
                    rawData = ((X509WindowsSecurityToken)token).Certificate.GetRawCertData();
                }
            }
        }

        public class IdManager : SignatureTargetIdManager
        {
            static readonly IdManager instance = new IdManager();

            IdManager()
            {
            }

            public override string DefaultIdNamespacePrefix
            {
                get { return UtilityStrings.Prefix; }
            }

            public override string DefaultIdNamespaceUri
            {
                get { return UtilityStrings.Namespace; }
            }

            internal static IdManager Instance
            {
                get { return instance; }
            }

            public override string ExtractId(XmlDictionaryReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                if (reader.IsStartElement(EncryptedData.ElementName, XD.XmlEncryptionDictionary.Namespace))
                {
                    return reader.GetAttribute(XD.XmlEncryptionDictionary.Id, null);
                }
                else
                {
                    return reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                }
            }

            public override void WriteIdAttribute(XmlDictionaryWriter writer, string id)
            {
                if (writer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

                writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
            }
        }
    }
}
