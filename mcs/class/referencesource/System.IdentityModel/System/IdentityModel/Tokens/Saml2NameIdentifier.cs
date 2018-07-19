//-----------------------------------------------------------------------
// <copyright file="Saml2NameIdentifier.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Represents the NameID element as specified in [Saml2Core, 2.2.3].
    /// </summary>
    public class Saml2NameIdentifier
    {
        private Uri format;
        private string nameQualifier;
        private string serviceProviderPointNameQualifier;
        private string serviceProviderdId;
        private string value;

        private EncryptingCredentials encryptingCredentials;
        private Collection<EncryptedKeyIdentifierClause> externalEncryptedKeys;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2NameIdentifier"/> from a name.
        /// </summary>
        /// <param name="name">Name string to initialize with.</param>
        public Saml2NameIdentifier(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2NameIdentifier"/> from a name and format.
        /// </summary>
        /// <param name="name">Name string to initialize with.</param>
        /// <param name="format"><see cref="Uri"/> specifying the identifier format.</param>
        public Saml2NameIdentifier(string name, Uri format)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (null != format && !format.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("format", SR.GetString(SR.ID0013));
            }

            this.format = format;
            this.value = name;
            this.externalEncryptedKeys = new Collection<EncryptedKeyIdentifierClause>();
        }

        /// <summary>
        /// Gets or sets the <see cref="EncryptingCredentials"/> used for encrypting. 
        /// </summary>
        public EncryptingCredentials EncryptingCredentials
        {
            get { return this.encryptingCredentials; }
            set { this.encryptingCredentials = value; }
        }

        /// <summary>
        /// Gets additional encrypted keys which will be specified external to the 
        /// EncryptedData element, as children of the EncryptedId element.
        /// </summary>
        public Collection<EncryptedKeyIdentifierClause> ExternalEncryptedKeys
        {
            get { return this.externalEncryptedKeys; }
        }

        /// <summary>
        /// Gets or sets a URI reference representing the classification of string-based identifier 
        /// information. [Saml2Core, 2.2.2]
        /// </summary>
        public Uri Format
        {
            get
            { 
                return this.format;
            }
            
            set
            {
                if (null != value && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.format = value;
            }
        }

        /// <summary>
        ///  Gets or sets the security or administrative domain that qualifies the name. [Saml2Core, 2.2.2]
        /// </summary>
        public string NameQualifier
        {
            get { return this.nameQualifier; }
            set { this.nameQualifier = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets a name that further qualifies the name of a service provider or affiliation 
        /// of providers. [Saml2Core, 2.2.2]
        /// </summary>
        public string SPNameQualifier
        {
            get { return this.serviceProviderPointNameQualifier; }
            set { this.serviceProviderPointNameQualifier = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets a name identifier established by a service provider or affiliation of providers 
        /// for the entity, if different from the primary name identifier. [Saml2Core, 2.2.2]
        /// </summary>
        public string SPProvidedId
        {
            get { return this.serviceProviderdId; }
            set { this.serviceProviderdId = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets the value of the name identifier.
        /// </summary>
        public string Value
        {
            get 
            { 
                return this.value; 
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.value = value;
            }
        }
    }
}
