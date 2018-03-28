//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Security.Principal;
    using System.Security;
    using System.Xml;
    using System.Xml.Serialization;

    public class SamlSubject
    {
        // Saml SubjectConfirmation parts.
        readonly ImmutableCollection<string> confirmationMethods = new ImmutableCollection<string>();
        string confirmationData;
        SecurityKeyIdentifier securityKeyIdentifier;
        SecurityKey crypto;
        SecurityToken subjectToken;

        // Saml NameIdentifier element parts.
        string name;
        string nameFormat;
        string nameQualifier;

        List<Claim> claims;
        IIdentity identity;
        ClaimSet subjectKeyClaimset;

        bool isReadOnly = false;

        public SamlSubject()
        {
        }

        public SamlSubject(string nameFormat, string nameQualifier, string name)
            : this(nameFormat, nameQualifier, name, null, null, null)
        {
        }

        public SamlSubject(string nameFormat, string nameQualifier, string name, IEnumerable<string> confirmations, string confirmationData, SecurityKeyIdentifier securityKeyIdentifier)
        {
            if (confirmations != null)
            {
                foreach (string method in confirmations)
                {
                    if (string.IsNullOrEmpty(method))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.SubjectConfirmationMethod.Value));

                    this.confirmationMethods.Add(method);
                }
            }

            if ((this.confirmationMethods.Count == 0) && (string.IsNullOrEmpty(name)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLSubjectRequiresNameIdentifierOrConfirmationMethod));

            if ((this.confirmationMethods.Count == 0) && ((confirmationData != null) || (securityKeyIdentifier != null)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified));

            this.name = name;
            this.nameFormat = nameFormat;
            this.nameQualifier = nameQualifier;
            this.confirmationData = confirmationData;
            this.securityKeyIdentifier = securityKeyIdentifier;
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLSubjectNameIdentifierRequiresNameValue));

                this.name = value;
            }
        }

        public string NameFormat
        {
            get { return this.nameFormat; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.nameFormat = value;
            }
        }

        public string NameQualifier
        {
            get { return this.nameQualifier; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.nameQualifier = value;
            }
        }

        public static string NameClaimType
        {
            get
            {
                return ClaimTypes.NameIdentifier;
            }
        }

        public IList<string> ConfirmationMethods
        {
            get { return this.confirmationMethods; }
        }

        internal IIdentity Identity
        {
            get { return this.identity; }
        }

        public string SubjectConfirmationData
        {
            get
            {
                return this.confirmationData;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                this.confirmationData = value;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get { return this.securityKeyIdentifier; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                this.securityKeyIdentifier = value;
            }
        }

        public SecurityKey Crypto
        {
            get { return this.crypto; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                this.crypto = value;
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (securityKeyIdentifier != null)
                    securityKeyIdentifier.MakeReadOnly();

                this.confirmationMethods.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        void CheckObjectValidity()
        {
            if ((this.confirmationMethods.Count == 0) && (string.IsNullOrEmpty(name)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectRequiresNameIdentifierOrConfirmationMethod)));

            if ((this.confirmationMethods.Count == 0) && ((this.confirmationData != null) || (this.securityKeyIdentifier != null)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified)));
        }

        public virtual ReadOnlyCollection<Claim> ExtractClaims()
        {
            if (this.claims == null)
            {
                this.claims = new List<Claim>();
                if (!string.IsNullOrEmpty(this.name))
                {
                    this.claims.Add(new Claim(ClaimTypes.NameIdentifier, new SamlNameIdentifierClaimResource(this.name, this.nameQualifier, this.nameFormat), Rights.Identity));
                    this.claims.Add(new Claim(ClaimTypes.NameIdentifier, new SamlNameIdentifierClaimResource(this.name, this.nameQualifier, this.nameFormat), Rights.PossessProperty));
                }
            }

            return this.claims.AsReadOnly();
        }

        public virtual ClaimSet ExtractSubjectKeyClaimSet(SamlSecurityTokenAuthenticator samlAuthenticator)
        {
            if ((this.subjectKeyClaimset == null) && (this.securityKeyIdentifier != null))
            {
                if (samlAuthenticator == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlAuthenticator");

                if (this.subjectToken != null)
                {
                    this.subjectKeyClaimset = samlAuthenticator.ResolveClaimSet(this.subjectToken);

                    this.identity = samlAuthenticator.ResolveIdentity(this.subjectToken);
                    if ((this.identity == null) && (this.subjectKeyClaimset != null))
                    {
                        Claim identityClaim = null;
                        foreach (Claim claim in this.subjectKeyClaimset.FindClaims(null, Rights.Identity))
                        {
                            identityClaim = claim;
                            break;
                        }

                        if (identityClaim != null)
                        {
                            this.identity = SecurityUtils.CreateIdentity(identityClaim.Resource.ToString(), this.GetType().Name);
                        }
                    }
                }

                if (this.subjectKeyClaimset == null)
                {
                    // Add the type of the primary claim as the Identity claim.
                    this.subjectKeyClaimset = samlAuthenticator.ResolveClaimSet(this.securityKeyIdentifier);
                    this.identity = samlAuthenticator.ResolveIdentity(this.securityKeyIdentifier);
                }
            }

            return this.subjectKeyClaimset;
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSerializer");

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            reader.MoveToContent();
            reader.Read();
            if (reader.IsStartElement(dictionary.NameIdentifier, dictionary.Namespace))
            {
                this.nameFormat = reader.GetAttribute(dictionary.NameIdentifierFormat, null);
                this.nameQualifier = reader.GetAttribute(dictionary.NameIdentifierNameQualifier, null);

                reader.MoveToContent();
                this.name = reader.ReadString();

                if (this.name == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLNameIdentifierMissingIdentifierValueOnRead)));

                reader.MoveToContent();
                reader.ReadEndElement();
            }

            if (reader.IsStartElement(dictionary.SubjectConfirmation, dictionary.Namespace))
            {
                reader.MoveToContent();
                reader.Read();

                while (reader.IsStartElement(dictionary.SubjectConfirmationMethod, dictionary.Namespace))
                {
                    string method = reader.ReadString();
                    if (string.IsNullOrEmpty(method))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.SubjectConfirmationMethod.Value)));

                    this.confirmationMethods.Add(method);
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }

                if (this.confirmationMethods.Count == 0)
                {
                    // A SubjectConfirmaton clause should specify at least one 
                    // ConfirmationMethod.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectConfirmationClauseMissingConfirmationMethodOnRead)));
                }

                if (reader.IsStartElement(dictionary.SubjectConfirmationData, dictionary.Namespace))
                {
                    reader.MoveToContent();
                    // An Authentication protocol specified in the confirmation method might need this
                    // data. Just store this content value as string.
                    this.confirmationData = reader.ReadString();
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
                if (reader.IsStartElement(samlSerializer.DictionaryManager.XmlSignatureDictionary.KeyInfo, samlSerializer.DictionaryManager.XmlSignatureDictionary.Namespace))
                {
                    XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateDictionaryReader(reader);
                    this.securityKeyIdentifier = SamlSerializer.ReadSecurityKeyIdentifier(dictionaryReader, keyInfoSerializer);
                    this.crypto = SamlSerializer.ResolveSecurityKey(this.securityKeyIdentifier, outOfBandTokenResolver);
                    if (this.crypto == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SamlUnableToExtractSubjectKey)));
                    }
                    this.subjectToken = SamlSerializer.ResolveSecurityToken(this.securityKeyIdentifier, outOfBandTokenResolver);
                }


                if ((this.confirmationMethods.Count == 0) && (string.IsNullOrEmpty(name)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectRequiresNameIdentifierOrConfirmationMethodOnRead)));

                reader.MoveToContent();
                reader.ReadEndElement();
            }

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Subject, dictionary.Namespace);

            if (this.name != null)
            {
                writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.NameIdentifier, dictionary.Namespace);
                if (this.nameFormat != null)
                {
                    writer.WriteStartAttribute(dictionary.NameIdentifierFormat, null);
                    writer.WriteString(this.nameFormat);
                    writer.WriteEndAttribute();
                }
                if (this.nameQualifier != null)
                {
                    writer.WriteStartAttribute(dictionary.NameIdentifierNameQualifier, null);
                    writer.WriteString(this.nameQualifier);
                    writer.WriteEndAttribute();
                }
                writer.WriteString(this.name);
                writer.WriteEndElement();
            }

            if (this.confirmationMethods.Count > 0)
            {
                writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.SubjectConfirmation, dictionary.Namespace);
                foreach (string method in this.confirmationMethods)
                    writer.WriteElementString(dictionary.SubjectConfirmationMethod, dictionary.Namespace, method);

                if (!string.IsNullOrEmpty(this.confirmationData))
                    writer.WriteElementString(dictionary.SubjectConfirmationData, dictionary.Namespace, this.confirmationData);

                if (this.securityKeyIdentifier != null)
                {
                    XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
                    SamlSerializer.WriteSecurityKeyIdentifier(dictionaryWriter, this.securityKeyIdentifier, keyInfoSerializer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

    }
}
