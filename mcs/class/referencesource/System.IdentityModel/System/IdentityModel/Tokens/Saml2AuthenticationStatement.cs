//-----------------------------------------------------------------------
// <copyright file="Saml2AuthenticationStatement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    /// <summary>
    /// Represents the AuthnStatement element specified in [Saml2Core, 2.7.2]. 
    /// </summary>
    public class Saml2AuthenticationStatement : Saml2Statement
    {
        private Saml2AuthenticationContext authnContext;
        private DateTime authnInstant;
        private string sessionIndex;
        private DateTime? sessionNotOnOrAfter;
        private Saml2SubjectLocality subjectLocality;

        /// <summary>
        /// Creates a Saml2AuthenticationStatement.
        /// </summary>
        /// <param name="authenticationContext">The authentication context of this statement.</param>
        public Saml2AuthenticationStatement(Saml2AuthenticationContext authenticationContext)
            : this(authenticationContext, DateTime.UtcNow)
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AuthenticationContext.
        /// </summary>
        /// <param name="authenticationContext">The authentication context of this statement.</param>
        /// <param name="authenticationInstant">The time of the authentication.</param>
        public Saml2AuthenticationStatement(Saml2AuthenticationContext authenticationContext, DateTime authenticationInstant)
        {
            if (null == authenticationContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticationContext");
            }

            this.authnContext = authenticationContext;
            this.authnInstant = DateTimeUtil.ToUniversalTime(authenticationInstant);
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2AuthenticationContext"/> used by the authenticating authority up to and including 
        /// the authentication event that yielded this statement. [Saml2Core, 2.7.2]
        /// </summary>
        public Saml2AuthenticationContext AuthenticationContext
        {
            get
            { 
                return this.authnContext; 
            }

            set
            {
                if (null == value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.authnContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the time at which the authentication took place. [Saml2Core, 2.7.2]
        /// </summary>
        public DateTime AuthenticationInstant
        {
            get { return this.authnInstant; }
            set { this.authnInstant = DateTimeUtil.ToUniversalTime(value); }
        }

        /// <summary>
        /// Gets or sets the index of a particular session between the principal 
        /// identified by the subject and the authenticating authority. [Saml2Core, 2.7.2]
        /// </summary>
        public string SessionIndex
        {
            get { return this.sessionIndex; }
            set { this.sessionIndex = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets the time instant at which the session between the principal 
        /// identified by the subject and the SAML authority issuing this statement
        /// must be considered ended. [Saml2Core, 2.7.2]
        /// </summary>
        public DateTime? SessionNotOnOrAfter
        {
            get { return this.sessionNotOnOrAfter; }
            set { this.sessionNotOnOrAfter = DateTimeUtil.ToUniversalTime(value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2SubjectLocality"/> which contains the DNS domain name and IP address for the system from which 
        /// the assertion subject was authenticated. [Saml2Core, 2.7.2]
        /// </summary>
        public Saml2SubjectLocality SubjectLocality
        {
            get { return this.subjectLocality; }
            set { this.subjectLocality = value; }
        }
    }
}
