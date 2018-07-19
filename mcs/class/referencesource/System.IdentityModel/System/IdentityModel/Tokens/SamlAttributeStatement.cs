//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.Xml;

    public class SamlAttributeStatement : SamlSubjectStatement
    {
        readonly ImmutableCollection<SamlAttribute> attributes = new ImmutableCollection<SamlAttribute>();
        bool isReadOnly = false;

        public SamlAttributeStatement()
        {
        }

        public SamlAttributeStatement(SamlSubject samlSubject, IEnumerable<SamlAttribute> attributes)
            : base(samlSubject)
        {
            if (attributes == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("attributes"));

            foreach (SamlAttribute attribute in attributes)
            {
                if (attribute == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Attribute.Value));

                this.attributes.Add(attribute);
            }

            CheckObjectValidity();
        }

        public IList<SamlAttribute> Attributes
        {
            get { return this.attributes; }
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAttribute attribute in attributes)
                {
                    attribute.MakeReadOnly();
                }

                this.attributes.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        void CheckObjectValidity()
        {
            if (this.SamlSubject == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectStatementRequiresSubject)));

            if (this.attributes.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeShouldHaveOneValue)));
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            reader.MoveToContent();
            reader.Read();

            if (reader.IsStartElement(dictionary.Subject, dictionary.Namespace))
            {
                SamlSubject subject = new SamlSubject();
                subject.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                base.SamlSubject = subject;
            }
            else
            {
                // SAML Subject is a required Attribute Statement clause.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeStatementMissingSubjectOnRead)));
            }

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.Attribute, dictionary.Namespace))
                {
                    // SAML Attribute is a extensibility point. So ask the SAML serializer 
                    // to load this part.
                    SamlAttribute attribute = samlSerializer.LoadAttribute(reader, keyInfoSerializer, outOfBandTokenResolver);
                    if (attribute == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLUnableToLoadAttribute)));
                    this.attributes.Add(attribute);
                }
                else
                {
                    break;
                }
            }

            if (this.attributes.Count == 0)
            {
                // Each Attribute statement should have at least one attribute.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeStatementMissingAttributeOnRead)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AttributeStatement, dictionary.Namespace);

            this.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);

            for (int i = 0; i < this.attributes.Count; i++)
            {
                this.attributes[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }

            writer.WriteEndElement();
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");

            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] != null)
                {
                    ReadOnlyCollection<Claim> attributeClaims = attributes[i].ExtractClaims();
                    if (attributeClaims != null)
                    {
                        for (int j = 0; j < attributeClaims.Count; ++j)
                            if (attributeClaims[j] != null)
                                claims.Add(attributeClaims[j]);
                    }
                }
            }
        }
    }
}
