//-----------------------------------------------------------------------
// <copyright file="Saml2SubjectConfirmationData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Represents the SubjectConfirmationData element and the associated 
    /// KeyInfoConfirmationDataType defined in [Saml2Core, 2.4.1.2-2.4.1.3].
    /// </summary>
    public class Saml2SubjectConfirmationData
    {
        private string address;
        private Saml2Id inResponseTo;
        private Collection<SecurityKeyIdentifier> keyIdentifiers = new Collection<SecurityKeyIdentifier>();
        private DateTime? notBefore;
        private DateTime? notOnOrAfter;
        private Uri recipient;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SubjectConfirmationData"/>.
        /// </summary>
        public Saml2SubjectConfirmationData()
        {
        }

        /// <summary>
        /// Gets or sets the network address/location from which an attesting entity can present the 
        /// assertion. [Saml2Core, 2.4.1.2]
        /// </summary>
        public string Address
        {
            get 
            { 
                return this.address; 
            }

            set
            {
                this.address = XmlUtil.NormalizeEmptyString(value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2Id"/> of a SAML protocol message in response to which an attesting entity can 
        /// present the assertion. [Saml2Core, 2.4.1.2]
        /// </summary>
        public Saml2Id InResponseTo
        {
            get { return this.inResponseTo; }
            set { this.inResponseTo = value; }
        }

        /// <summary>
        /// Gets a collection of <see cref="SecurityKeyIdentifier"/> which can be used to authenticate an attesting entity. [Saml2Core, 2.4.1.3]
        /// </summary>
        public Collection<SecurityKeyIdentifier> KeyIdentifiers
        {
            get { return this.keyIdentifiers; }
        }

        /// <summary>
        /// Gets or sets a time instant before which the subject cannot be confirmed. [Saml2Core, 2.4.1.2]
        /// </summary>
        public DateTime? NotBefore
        {
            get { return this.notBefore; }
            set { this.notBefore = DateTimeUtil.ToUniversalTime(value); }
        }

        /// <summary>
        /// Gets or sets a time instant at which the subject can no longer be confirmed. [Saml2Core, 2.4.1.2]
        /// </summary>
        public DateTime? NotOnOrAfter
        {
            get { return this.notOnOrAfter; }
            set { this.notOnOrAfter = DateTimeUtil.ToUniversalTime(value); }
        }

        /// <summary>
        /// Gets or sets a URI specifying the entity or location to which an attesting entity can present 
        /// the assertion. [Saml2Core, 2.4.1.2]
        /// </summary>
        public Uri Recipient
        {
            get 
            { 
                return this.recipient; 
            }

            set
            {
                if (null != value && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.recipient = value;
            }
        }
    }
}
