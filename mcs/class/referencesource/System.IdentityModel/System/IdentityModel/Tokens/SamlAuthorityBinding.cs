//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using System.IdentityModel.Selectors;

    [DataContract]
    public class SamlAuthorityBinding
    {
        XmlQualifiedName authorityKind;
        string binding;
        string location;
        [DataMember]
        bool isReadOnly = false;

        public SamlAuthorityBinding(XmlQualifiedName authorityKind, string binding, string location)
        {
            this.AuthorityKind = authorityKind;
            this.Binding = binding;
            this.Location = location;

            CheckObjectValidity();
        }

        public SamlAuthorityBinding()
        {
        }

        [DataMember]
        public XmlQualifiedName AuthorityKind
        {
            get { return this.authorityKind; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

                if (string.IsNullOrEmpty(value.Name))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAuthorityKindMissingName));

                this.authorityKind = value;
            }
        }

        [DataMember]
        public string Binding
        {
            get { return this.binding; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAuthorityBindingRequiresBinding));

                this.binding = value;
            }
        }

        [DataMember]
        public string Location
        {
            get { return this.location; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAuthorityBindingRequiresLocation));

                this.location = value;
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void CheckObjectValidity()
        {
            if (this.authorityKind == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingMissingAuthorityKind)));

            if (string.IsNullOrEmpty(authorityKind.Name))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityKindMissingName)));

            if (string.IsNullOrEmpty(this.binding))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingRequiresBinding)));

            if (string.IsNullOrEmpty(this.location))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingRequiresLocation)));
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            string authKind = reader.GetAttribute(dictionary.AuthorityKind, null);
            if (string.IsNullOrEmpty(authKind))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingMissingAuthorityKindOnRead)));

            string[] authKindParts = authKind.Split(':');
            if (authKindParts.Length > 2)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingInvalidAuthorityKind)));

            string localName;
            string prefix;
            string nameSpace;
            if (authKindParts.Length == 2)
            {
                prefix = authKindParts[0];
                localName = authKindParts[1];
            }
            else
            {
                prefix = String.Empty;
                localName = authKindParts[0];
            }

            nameSpace = reader.LookupNamespace(prefix);

            this.authorityKind = new XmlQualifiedName(localName, nameSpace);

            this.binding = reader.GetAttribute(dictionary.Binding, null);
            if (string.IsNullOrEmpty(this.binding))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingMissingBindingOnRead)));

            this.location = reader.GetAttribute(dictionary.Location, null);
            if (string.IsNullOrEmpty(this.location))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorityBindingMissingLocationOnRead)));

            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.MoveToContent();
                reader.Read();
                reader.ReadEndElement();
            }
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

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AuthorityBinding, dictionary.Namespace);

            string prefix = null;
            if (!string.IsNullOrEmpty(this.authorityKind.Namespace))
            {
                writer.WriteAttributeString(String.Empty, dictionary.NamespaceAttributePrefix.Value, null, this.authorityKind.Namespace);
                prefix = writer.LookupPrefix(this.authorityKind.Namespace);
            }

            writer.WriteStartAttribute(dictionary.AuthorityKind, null);
            if (string.IsNullOrEmpty(prefix))
                writer.WriteString(this.authorityKind.Name);
            else
                writer.WriteString(prefix + ":" + this.authorityKind.Name);
            writer.WriteEndAttribute();

            writer.WriteStartAttribute(dictionary.Location, null);
            writer.WriteString(this.location);
            writer.WriteEndAttribute();

            writer.WriteStartAttribute(dictionary.Binding, null);
            writer.WriteString(this.binding);
            writer.WriteEndAttribute();

            writer.WriteEndElement();
        }
    }
}
