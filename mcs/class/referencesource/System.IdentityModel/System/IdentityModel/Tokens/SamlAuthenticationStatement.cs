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

    public class SamlAuthenticationStatement : SamlSubjectStatement
    {
        DateTime authenticationInstant = DateTime.UtcNow.ToUniversalTime();
        string authenticationMethod = XD.SamlDictionary.UnspecifiedAuthenticationMethod.Value;
        readonly ImmutableCollection<SamlAuthorityBinding> authorityBindings = new ImmutableCollection<SamlAuthorityBinding>();
        string dnsAddress;
        string ipAddress;
        bool isReadOnly = false;

        public SamlAuthenticationStatement()
        {
        }

        public SamlAuthenticationStatement(SamlSubject samlSubject,
            string authenticationMethod,
            DateTime authenticationInstant,
            string dnsAddress,
            string ipAddress,
            IEnumerable<SamlAuthorityBinding> authorityBindings)
            : base(samlSubject)
        {
            if (string.IsNullOrEmpty(authenticationMethod))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("authenticationMethod", SR.GetString(SR.SAMLAuthenticationStatementMissingAuthenticationMethod));

            this.authenticationMethod = authenticationMethod;
            this.authenticationInstant = authenticationInstant.ToUniversalTime();
            this.dnsAddress = dnsAddress;
            this.ipAddress = ipAddress;

            if (authorityBindings != null)
            {
                foreach (SamlAuthorityBinding binding in authorityBindings)
                {
                    if (binding == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Assertion.Value));

                    this.authorityBindings.Add(binding);
                }
            }

            CheckObjectValidity();
        }

        public DateTime AuthenticationInstant
        {
            get { return this.authenticationInstant; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.authenticationInstant = value;
            }
        }

        public string AuthenticationMethod
        {
            get { return this.authenticationMethod; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    this.authenticationMethod = XD.SamlDictionary.UnspecifiedAuthenticationMethod.Value;
                else
                    this.authenticationMethod = value;
            }
        }

        public static string ClaimType
        {
            get
            {
                return ClaimTypes.Authentication;
            }
        }

        public IList<SamlAuthorityBinding> AuthorityBindings
        {
            get { return this.authorityBindings; }
        }

        public string DnsAddress
        {
            get { return this.dnsAddress; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.dnsAddress = value;
            }
        }

        public string IPAddress
        {
            get { return this.ipAddress; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.ipAddress = value;
            }
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAuthorityBinding binding in this.authorityBindings)
                {
                    binding.MakeReadOnly();
                }

                this.authorityBindings.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");

            claims.Add(new Claim(ClaimTypes.Authentication, new SamlAuthenticationClaimResource(this.authenticationInstant, this.authenticationMethod, this.dnsAddress, this.ipAddress, this.authorityBindings), Rights.PossessProperty));
        }

        void CheckObjectValidity()
        {
            if (this.SamlSubject == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectStatementRequiresSubject)));

            // Authenticaton instant is required. We will throw an exception if it is not present while 
            // deserializing a SAML Authentication statement. When creating a new Authentication statement 
            // we set this value to UtcNow.

            if (string.IsNullOrEmpty(this.authenticationMethod))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthenticationStatementMissingAuthenticationMethod)));
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            string authInstance = reader.GetAttribute(dictionary.AuthenticationInstant, null);
            if (string.IsNullOrEmpty(authInstance))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthenticationStatementMissingAuthenticationInstanceOnRead)));
            this.authenticationInstant = DateTime.ParseExact(
                authInstance, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();

            this.authenticationMethod = reader.GetAttribute(dictionary.AuthenticationMethod, null);
            if (string.IsNullOrEmpty(this.authenticationMethod))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthenticationStatementMissingAuthenticationMethodOnRead)));

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
                // Subject is a required element for a Authentication Statement clause.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthenticationStatementMissingSubject)));
            }

            if (reader.IsStartElement(dictionary.SubjectLocality, dictionary.Namespace))
            {
                this.dnsAddress = reader.GetAttribute(dictionary.SubjectLocalityDNSAddress, null);
                this.ipAddress = reader.GetAttribute(dictionary.SubjectLocalityIPAddress, null);

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

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.AuthorityBinding, dictionary.Namespace))
                {
                    SamlAuthorityBinding binding = new SamlAuthorityBinding();
                    binding.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.authorityBindings.Add(binding);
                }
                else
                {
                    // We do not understand this element.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.AuthenticationStatement)));
                }
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

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AuthenticationStatement, dictionary.Namespace);

            writer.WriteStartAttribute(dictionary.AuthenticationMethod, null);
            writer.WriteString(this.authenticationMethod);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(dictionary.AuthenticationInstant, null);
            writer.WriteString(this.authenticationInstant.ToString(SamlConstants.GeneratedDateTimeFormat, CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();

            this.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);

            if ((this.ipAddress != null) || (this.dnsAddress != null))
            {
                writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.SubjectLocality, dictionary.Namespace);

                if (this.ipAddress != null)
                {
                    writer.WriteStartAttribute(dictionary.SubjectLocalityIPAddress, null);
                    writer.WriteString(this.ipAddress);
                    writer.WriteEndAttribute();
                }

                if (this.dnsAddress != null)
                {
                    writer.WriteStartAttribute(dictionary.SubjectLocalityDNSAddress, null);
                    writer.WriteString(this.dnsAddress);
                    writer.WriteEndAttribute();
                }

                writer.WriteEndElement();
            }

            for (int i = 0; i < this.authorityBindings.Count; i++)
            {
                this.authorityBindings[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }

            writer.WriteEndElement();
        }
    }
}

