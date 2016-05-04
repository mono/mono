//-----------------------------------------------------------------------
// <copyright file="Saml2Conditions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Conditions element specified in [Saml2Core, 2.5.1].
    /// </summary>
    public class Saml2Conditions
    {
        private Collection<Saml2AudienceRestriction> audienceRestrictions = new Collection<Saml2AudienceRestriction>();
        private DateTime? notBefore;
        private DateTime? notOnOrAfter;
        private bool oneTimeUse;
        private Saml2ProxyRestriction proxyRestriction;

        /// <summary>
        /// Initializes a new instance of <see cref="Saml2Conditions"/>. class.
        /// </summary>
        public Saml2Conditions()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2AudienceRestriction"/> that the assertion is addressed to.
        /// [Saml2Core, 2.5.1]
        /// </summary>
        public Collection<Saml2AudienceRestriction> AudienceRestrictions
        {
            get { return this.audienceRestrictions; }
        }

        /// <summary>
        /// Gets or sets the earliest time instant at which the assertion is valid.
        /// [Saml2Core, 2.5.1]
        /// </summary>
        public DateTime? NotBefore
        {
            get 
            { 
                return this.notBefore; 
            }

            set
            {
                value = DateTimeUtil.ToUniversalTime(value);

                // NotBefore must be earlier than NotOnOrAfter
                if (null != value && null != this.notOnOrAfter)
                {
                    if (value.Value >= this.notOnOrAfter.Value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4116));
                    }
                }
             
                this.notBefore = value;
            }
        }

        /// <summary>
        /// Gets or sets the time instant at which the assertion has expired.
        /// [Saml2Core, 2.5.1]
        /// </summary>
        public DateTime? NotOnOrAfter
        {
            get 
            { 
                return this.notOnOrAfter; 
            }

            set
            {
                value = DateTimeUtil.ToUniversalTime(value);

                // NotBefore must be earlier than NotOnOrAfter
                if (null != value && null != this.notBefore)
                {
                    if (value.Value <= this.notBefore.Value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4116));
                    }
                }

                this.notOnOrAfter = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the assertion SHOULD be used immediately and MUST NOT
        /// be retained for future use. [Saml2Core, 2.5.1]
        /// </summary>
        public bool OneTimeUse
        {
            get { return this.oneTimeUse; }
            set { this.oneTimeUse = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2ProxyRestriction"/> that specified limitations that the asserting party imposes on relying parties
        /// that wish to subsequently act as asserting parties themselves and issue assertions of their own on the basis of the information contained in
        /// the original assertion. [Saml2Core, 2.5.1]
        /// </summary>
        public Saml2ProxyRestriction ProxyRestriction
        {
            get { return this.proxyRestriction; }
            set { this.proxyRestriction = value; }
        }
    }
}
