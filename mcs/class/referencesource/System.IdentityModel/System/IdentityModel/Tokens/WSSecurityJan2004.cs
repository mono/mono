//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using KeyIdentifierClauseEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.KeyIdentifierClauseEntry;
    using StrEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.StrEntry;
    using TokenEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.TokenEntry;

    class WSSecurityJan2004 : SecurityTokenSerializer.SerializerEntries
    {
        KeyInfoSerializer securityTokenSerializer;

        public WSSecurityJan2004(KeyInfoSerializer securityTokenSerializer)
        {
            this.securityTokenSerializer = securityTokenSerializer;
        }

        public KeyInfoSerializer SecurityTokenSerializer
        {
            get { return this.securityTokenSerializer; }
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<KeyIdentifierClauseEntry> clauseEntries)
        {
            List<StrEntry> strEntries = new List<StrEntry>();
            this.securityTokenSerializer.PopulateStrEntries(strEntries);
            SecurityTokenReferenceJan2004ClauseEntry strClause = new SecurityTokenReferenceJan2004ClauseEntry(this.securityTokenSerializer.EmitBspRequiredAttributes, strEntries);
            clauseEntries.Add(strClause);
        }

        protected void PopulateJan2004StrEntries(IList<StrEntry> strEntries)
        {
            strEntries.Add(new LocalReferenceStrEntry(this.securityTokenSerializer.EmitBspRequiredAttributes, this.securityTokenSerializer));
            strEntries.Add(new KerberosHashStrEntry(this.securityTokenSerializer.EmitBspRequiredAttributes));
            strEntries.Add(new X509SkiStrEntry(this.securityTokenSerializer.EmitBspRequiredAttributes));
            strEntries.Add(new X509IssuerSerialStrEntry());
            strEntries.Add(new RelDirectStrEntry());
            strEntries.Add(new SamlJan2004KeyIdentifierStrEntry());
            strEntries.Add(new Saml2Jan2004KeyIdentifierStrEntry());
        }


        public override void PopulateStrEntries(IList<StrEntry> strEntries)
        {
            PopulateJan2004StrEntries(strEntries);
        }

        protected void PopulateJan2004TokenEntries(IList<TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new GenericXmlTokenEntry());
            tokenEntryList.Add(new UserNamePasswordTokenEntry());
            tokenEntryList.Add(new KerberosTokenEntry());
            tokenEntryList.Add(new X509TokenEntry());
        }

        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            PopulateJan2004TokenEntries(tokenEntryList);
            tokenEntryList.Add(new SamlTokenEntry());
            tokenEntryList.Add(new WrappedKeyTokenEntry());
        }

        internal abstract class BinaryTokenEntry : TokenEntry
        {
            internal static readonly XmlDictionaryString ElementName = XD.SecurityJan2004Dictionary.BinarySecurityToken;
            internal static readonly XmlDictionaryString EncodingTypeAttribute = XD.SecurityJan2004Dictionary.EncodingType;
            internal const string EncodingTypeAttributeString = SecurityJan2004Strings.EncodingType;
            internal const string EncodingTypeValueBase64Binary = SecurityJan2004Strings.EncodingTypeValueBase64Binary;
            internal const string EncodingTypeValueHexBinary = SecurityJan2004Strings.EncodingTypeValueHexBinary;
            internal static readonly XmlDictionaryString ValueTypeAttribute = XD.SecurityJan2004Dictionary.ValueType;

            string[] valueTypeUris = null;

            protected BinaryTokenEntry(string valueTypeUri)
            {
                this.valueTypeUris = new string[1];
                this.valueTypeUris[0] = valueTypeUri;
            }

            protected BinaryTokenEntry(string[] valueTypeUris)
            {
                if (valueTypeUris == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("valueTypeUris");

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
        }

        class GenericXmlTokenEntry : TokenEntry
        {
            protected override XmlDictionaryString LocalName { get { return null; } }
            protected override XmlDictionaryString NamespaceUri { get { return null; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(GenericXmlSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }
        }

        class KerberosTokenEntry : BinaryTokenEntry
        {
            public KerberosTokenEntry()
                : base(new string[] { SecurityJan2004Strings.KerberosTokenTypeGSS, SecurityJan2004Strings.KerberosTokenType1510 })
            {
            }

            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(KerberosReceiverSecurityToken), typeof(KerberosRequestorSecurityToken) }; }
        }

        protected class SamlTokenEntry : TokenEntry
        {
            protected override XmlDictionaryString LocalName { get { return XD.SecurityJan2004Dictionary.SamlAssertion; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.SecurityJan2004Dictionary.SamlUri; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(SamlSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }
        }

        class UserNamePasswordTokenEntry : TokenEntry
        {
            protected override XmlDictionaryString LocalName { get { return XD.SecurityJan2004Dictionary.UserNameTokenElement; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.SecurityJan2004Dictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(UserNameSecurityToken) }; }
            public override string TokenTypeUri { get { return SecurityJan2004Strings.UPTokenType; } }
            protected override string ValueTypeUri { get { return null; } }

        }

        protected class WrappedKeyTokenEntry : TokenEntry
        {
            protected override XmlDictionaryString LocalName { get { return EncryptedKey.ElementName; } }
            protected override XmlDictionaryString NamespaceUri { get { return XD.XmlEncryptionDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(WrappedKeySecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }
        }

        protected class X509TokenEntry : BinaryTokenEntry
        {
            internal const string ValueTypeAbsoluteUri = SecurityJan2004Strings.X509TokenType;

            public X509TokenEntry()
                : base(ValueTypeAbsoluteUri)
            {
            }

            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(X509SecurityToken), typeof(X509WindowsSecurityToken) }; }
        }

        protected class SecurityTokenReferenceJan2004ClauseEntry : KeyIdentifierClauseEntry
        {
            const int DefaultDerivedKeyLength = 32;

            bool emitBspRequiredAttributes;
            IList<StrEntry> strEntries;

            public SecurityTokenReferenceJan2004ClauseEntry(bool emitBspRequiredAttributes, IList<StrEntry> strEntries)
            {
                this.emitBspRequiredAttributes = emitBspRequiredAttributes;
                this.strEntries = strEntries;
            }
            protected bool EmitBspRequiredAttributes
            {
                get
                {
                    return this.emitBspRequiredAttributes;
                }
            }
            protected IList<StrEntry> StrEntries
            {
                get
                {
                    return this.strEntries;
                }
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.SecurityTokenReference;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.Namespace;
                }
            }

            protected virtual string ReadTokenType(XmlDictionaryReader reader)
            {
                return null;
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                byte[] nonce = null;
                int length = 0;
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, NamespaceUri))
                {
                    string nonceString = reader.GetAttribute(XD.SecureConversationFeb2005Dictionary.Nonce, XD.SecureConversationFeb2005Dictionary.Namespace);
                    if (nonceString != null)
                    {
                        nonce = Convert.FromBase64String(nonceString);
                    }

                    string lengthString = reader.GetAttribute(XD.SecureConversationFeb2005Dictionary.Length, XD.SecureConversationFeb2005Dictionary.Namespace);
                    if (lengthString != null)
                    {
                        length = Convert.ToInt32(lengthString, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        length = DefaultDerivedKeyLength;
                    }
                }
                string tokenType = ReadTokenType(reader);
                string strId = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                reader.ReadStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, NamespaceUri);
                SecurityKeyIdentifierClause clause = null;
                for (int i = 0; i < this.strEntries.Count; ++i)
                {
                    if (this.strEntries[i].CanReadClause(reader, tokenType))
                    {
                        clause = this.strEntries[i].ReadClause(reader, nonce, length, tokenType);
                        break;
                    }
                }

                if (clause == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CannotReadKeyIdentifierClause, reader.LocalName, reader.NamespaceURI)));
                }

                if (!string.IsNullOrEmpty(strId))
                {
                    clause.Id = strId;
                }

                reader.ReadEndElement();
                return clause;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                for (int i = 0; i < this.strEntries.Count; ++i)
                {
                    if (this.strEntries[i].SupportsCore(keyIdentifierClause))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                for (int i = 0; i < this.strEntries.Count; ++i)
                {
                    if (this.strEntries[i].SupportsCore(keyIdentifierClause))
                    {
                        writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace);
                        this.strEntries[i].WriteContent(writer, keyIdentifierClause);
                        writer.WriteEndElement();
                        return;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StandardsManagerCannotWriteObject, keyIdentifierClause.GetType())));
            }
        }

        protected abstract class KeyIdentifierStrEntry : StrEntry
        {
            bool emitBspRequiredAttributes;

            protected const string EncodingTypeValueBase64Binary = SecurityJan2004Strings.EncodingTypeValueBase64Binary;
            protected const string EncodingTypeValueHexBinary = SecurityJan2004Strings.EncodingTypeValueHexBinary;
            protected const string EncodingTypeValueText = SecurityJan2004Strings.EncodingTypeValueText;

            protected abstract Type ClauseType { get; }
            protected virtual string DefaultEncodingType { get { return EncodingTypeValueBase64Binary; } }
            public abstract Type TokenType { get; }
            protected abstract string ValueTypeUri { get; }
            protected bool EmitBspRequiredAttributes { get { return this.emitBspRequiredAttributes; } }

            protected KeyIdentifierStrEntry(bool emitBspRequiredAttributes)
            {
                this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    return (ValueTypeUri == valueType);
                }
                return false;
            }

            protected abstract SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength);

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return this.TokenType;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                string encodingType = reader.GetAttribute(XD.SecurityJan2004Dictionary.EncodingType, null);
                if (encodingType == null)
                {
                    encodingType = DefaultEncodingType;
                }

                reader.ReadStartElement();

                byte[] bytes;
                if (encodingType == EncodingTypeValueBase64Binary)
                {
                    bytes = reader.ReadContentAsBase64();
                }
                else if (encodingType == EncodingTypeValueHexBinary)
                {
                    bytes = HexBinary.Parse(reader.ReadContentAsString()).Value;
                }
                else if (encodingType == EncodingTypeValueText)
                {
                    bytes = new UTF8Encoding().GetBytes(reader.ReadContentAsString());
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityMessageSerializationException(SR.GetString(SR.UnknownEncodingInKeyIdentifier)));
                }

                reader.ReadEndElement();

                return CreateClause(bytes, derivationNonce, derivationLength);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return ClauseType.IsAssignableFrom(clause.GetType());
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, ValueTypeUri);
                if (this.emitBspRequiredAttributes)
                {
                    // Emit the encodingType attribute.
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.EncodingType, null, DefaultEncodingType);
                }
                string encoding = DefaultEncodingType;
                BinaryKeyIdentifierClause binaryClause = clause as BinaryKeyIdentifierClause;

                byte[] keyIdentifier = binaryClause.GetBuffer();
                if (encoding == EncodingTypeValueBase64Binary)
                {
                    writer.WriteBase64(keyIdentifier, 0, keyIdentifier.Length);
                }
                else if (encoding == EncodingTypeValueHexBinary)
                {
                    writer.WriteBinHex(keyIdentifier, 0, keyIdentifier.Length);
                }
                else if (encoding == EncodingTypeValueText)
                {
                    writer.WriteString(new UTF8Encoding().GetString(keyIdentifier, 0, keyIdentifier.Length));
                }
                writer.WriteEndElement();
            }
        }

        protected class KerberosHashStrEntry : KeyIdentifierStrEntry
        {
            protected override Type ClauseType { get { return typeof(KerberosTicketHashKeyIdentifierClause); } }
            public override Type TokenType { get { return typeof(KerberosRequestorSecurityToken); } }
            protected override string ValueTypeUri { get { return SecurityJan2004Strings.KerberosHashValueType; } }

            public KerberosHashStrEntry(bool emitBspRequiredAttributes)
                : base(emitBspRequiredAttributes)
            {
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new KerberosTicketHashKeyIdentifierClause(bytes, derivationNonce, derivationLength);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityJan2004Dictionary.KerberosTokenTypeGSS.Value;
            }
            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, ValueTypeUri);
                KerberosTicketHashKeyIdentifierClause binaryClause = clause as KerberosTicketHashKeyIdentifierClause;
                if (this.EmitBspRequiredAttributes)
                {
                    // Emit the encodingType attribute.
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.EncodingType, null, DefaultEncodingType);
                }
                string encoding = DefaultEncodingType;

                byte[] keyIdentifier = binaryClause.GetBuffer();
                if (encoding == EncodingTypeValueBase64Binary)
                {
                    writer.WriteBase64(keyIdentifier, 0, keyIdentifier.Length);
                }
                else if (encoding == EncodingTypeValueHexBinary)
                {
                    writer.WriteBinHex(keyIdentifier, 0, keyIdentifier.Length);
                }
                else if (encoding == EncodingTypeValueText)
                {
                    writer.WriteString(new UTF8Encoding().GetString(keyIdentifier, 0, keyIdentifier.Length));
                }
                writer.WriteEndElement();
            }
        }

        protected class X509SkiStrEntry : KeyIdentifierStrEntry
        {
            protected override Type ClauseType { get { return typeof(X509SubjectKeyIdentifierClause); } }
            public override Type TokenType { get { return typeof(X509SecurityToken); } }
            protected override string ValueTypeUri { get { return SecurityJan2004Strings.X509SKIValueType; } }

            public X509SkiStrEntry(bool emitBspRequiredAttributes)
                : base(emitBspRequiredAttributes)
            {
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new X509SubjectKeyIdentifierClause(bytes);
            }
            public override string GetTokenTypeUri()
            {
                return SecurityJan2004Strings.X509TokenType;
            }
        }

        protected class LocalReferenceStrEntry : StrEntry
        {
            bool emitBspRequiredAttributes;
            KeyInfoSerializer tokenSerializer;

            public LocalReferenceStrEntry(bool emitBspRequiredAttributes, KeyInfoSerializer tokenSerializer)
            {
                this.emitBspRequiredAttributes = emitBspRequiredAttributes;
                this.tokenSerializer = tokenSerializer;
            }

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                LocalIdKeyIdentifierClause localClause = clause as LocalIdKeyIdentifierClause;
                return localClause.OwnerType;
            }

            public string GetLocalTokenTypeUri(SecurityKeyIdentifierClause clause)
            {
                Type tokenType = GetTokenType(clause);
                return this.tokenSerializer.GetTokenTypeUri(tokenType);
            }
            public override string GetTokenTypeUri()
            {
                return null;
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string uri = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                    if (uri != null && uri.Length > 0 && uri[0] == '#')
                    {
                        return true;
                    }
                }
                return false;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                string uri = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                string tokenTypeUri = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                Type[] tokenTypes = null;
                if (tokenTypeUri != null)
                {
                    tokenTypes = this.tokenSerializer.GetTokenTypes(tokenTypeUri);
                }
                SecurityKeyIdentifierClause clause = new LocalIdKeyIdentifierClause(uri.Substring(1), derivationNonce, derivationLength, tokenTypes);
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    reader.ReadEndElement();
                }
                return clause;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return clause is LocalIdKeyIdentifierClause;
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                LocalIdKeyIdentifierClause localIdClause = clause as LocalIdKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                if (this.emitBspRequiredAttributes)
                {
                    string tokenTypeUri = GetLocalTokenTypeUri(localIdClause);
                    if (tokenTypeUri != null)
                    {
                        writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, tokenTypeUri);
                    }
                }
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.URI, null, "#" + localIdClause.LocalId);
                writer.WriteEndElement();
            }
        }

        protected class SamlJan2004KeyIdentifierStrEntry : StrEntry
        {
            protected virtual bool IsMatchingValueType(string valueType)
            {
                return (valueType == SecurityJan2004Strings.SamlAssertionIdValueType);
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri))
                {
                    return true;
                }
                else if (reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    return IsMatchingValueType(valueType);
                }
                return false;
            }

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return typeof(SamlSecurityToken);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.SamlTokenType.Value;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNone, int derivationLength, string tokenType)
            {
                bool readAuthorityBinding = false;
                bool readKeyIdentifier = false;
                string id = null;
                string valueType = null;
                string binding = null;
                string location = null;
                string authorityKind = null;
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri))
                    {
                        if (readAuthorityBinding)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MultipleSamlAuthorityBindingsInReference)));
                        }
                        readAuthorityBinding = true;
                        binding = reader.GetAttribute(XD.SamlDictionary.Binding, null);
                        if (string.IsNullOrEmpty(binding))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.RequiredAttributeMissing, XD.SamlDictionary.Binding.Value, XD.SamlDictionary.AuthorityBinding.Value)));
                        }
                        location = reader.GetAttribute(XD.SamlDictionary.Location, null);
                        if (string.IsNullOrEmpty(location))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.RequiredAttributeMissing, XD.SamlDictionary.Location.Value, XD.SamlDictionary.AuthorityBinding.Value)));
                        }
                        authorityKind = reader.GetAttribute(XD.SamlDictionary.AuthorityKind, null);
                        if (string.IsNullOrEmpty(authorityKind))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.RequiredAttributeMissing, XD.SamlDictionary.AuthorityKind.Value, XD.SamlDictionary.AuthorityBinding.Value)));
                        }
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                        }
                        else
                        {
                            reader.ReadStartElement();
                            reader.ReadEndElement();
                        }
                    }
                    else if (reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                    {
                        if (readKeyIdentifier)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MultipleKeyIdentifiersInReference)));
                        }
                        readKeyIdentifier = true;
                        valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                        id = reader.ReadElementContentAsString();
                    }
                    else
                    {
                        break;
                    }
                }
                if (!readKeyIdentifier)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.DidNotFindKeyIdentifierInReference)));
                }
                return new SamlAssertionKeyIdentifierClause(id, derivationNone, derivationLength, valueType, tokenType, binding, location, authorityKind);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {

                if (typeof(SamlAssertionKeyIdentifierClause).IsAssignableFrom(clause.GetType()))
                {
                    SamlAssertionKeyIdentifierClause samlclause = clause as SamlAssertionKeyIdentifierClause;
                    if ((samlclause.TokenTypeUri == null) || (samlclause.TokenTypeUri == this.GetTokenTypeUri()))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                SamlAssertionKeyIdentifierClause samlClause = clause as SamlAssertionKeyIdentifierClause;
                if (!string.IsNullOrEmpty(samlClause.Binding) || !string.IsNullOrEmpty(samlClause.Location) || !string.IsNullOrEmpty(samlClause.AuthorityKind))
                {
                    writer.WriteStartElement(XD.SamlDictionary.PreferredPrefix.Value, XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri);
                    if (!string.IsNullOrEmpty(samlClause.Binding))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.Binding, null, samlClause.Binding);
                    }
                    if (!string.IsNullOrEmpty(samlClause.Location))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.Location, null, samlClause.Location);
                    }
                    if (!string.IsNullOrEmpty(samlClause.AuthorityKind))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.AuthorityKind, null, samlClause.AuthorityKind);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                string valueType = string.IsNullOrEmpty(samlClause.ValueType) ? XD.SecurityJan2004Dictionary.SamlAssertionIdValueType.Value : samlClause.ValueType;
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, valueType);
                writer.WriteString(samlClause.AssertionId);
                writer.WriteEndElement();
            }
        }

        class Saml2Jan2004KeyIdentifierStrEntry : SamlJan2004KeyIdentifierStrEntry
        {
            // handles SAML2.0
            protected override bool IsMatchingValueType(string valueType)
            {
                return (valueType == XD.SecurityXXX2005Dictionary.Saml11AssertionValueType.Value);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.Saml20TokenType.Value;
            }
        }

        protected class RelDirectStrEntry : StrEntry
        {
            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    return (valueType == SecurityJan2004Strings.RelAssertionValueType);
                }
                return false;
            }

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return null;
            }
            public override string GetTokenTypeUri()
            {
                return XD.SecurityJan2004Dictionary.RelAssertionValueType.Value;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNone, int derivationLength, string tokenType)
            {
                string assertionId = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    reader.ReadEndElement();
                }
                return new RelAssertionDirectKeyIdentifierClause(assertionId, derivationNone, derivationLength);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return typeof(RelAssertionDirectKeyIdentifierClause).IsAssignableFrom(clause.GetType());
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                RelAssertionDirectKeyIdentifierClause relClause = clause as RelAssertionDirectKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, SecurityJan2004Strings.RelAssertionValueType);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.URI, null, relClause.AssertionId);
                writer.WriteEndElement();
            }
        }

        protected class X509IssuerSerialStrEntry : StrEntry
        {
            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return typeof(X509SecurityToken);
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                return reader.IsStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
            }
            public override string GetTokenTypeUri()
            {
                return SecurityJan2004Strings.X509TokenType;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace);
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace);
                string issuerName = reader.ReadContentAsString();
                reader.ReadEndElement();
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace);
                string serialNumber = reader.ReadContentAsString();
                reader.ReadEndElement();
                reader.ReadEndElement();
                reader.ReadEndElement();

                return new X509IssuerSerialKeyIdentifierClause(issuerName, serialNumber);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return clause is X509IssuerSerialKeyIdentifierClause;
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                X509IssuerSerialKeyIdentifierClause issuerClause = clause as X509IssuerSerialKeyIdentifierClause;
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace);
                writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace, issuerClause.IssuerName);
                writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace, issuerClause.IssuerSerialNumber);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public class IdManager : SignatureTargetIdManager
        {
            internal static readonly XmlDictionaryString ElementName = XD.XmlEncryptionDictionary.EncryptedData;

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

                if (reader.IsStartElement(ElementName, XD.XmlEncryptionDictionary.Namespace))
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
