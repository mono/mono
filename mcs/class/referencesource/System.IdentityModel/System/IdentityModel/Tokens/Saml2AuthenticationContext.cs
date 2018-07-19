//-----------------------------------------------------------------------
// <copyright file="Saml2AuthenticationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the AuthnContext element specified in [Saml2Core, 2.7.2.2].
    /// </summary>
    /// <remarks>
    /// <para>
    /// This base class does not directly support any by-value authentication 
    /// context declarations (represented in XML by the AuthnContextDecl element). 
    /// To support by-value declarations, extend this class to support the data 
    /// model and extend Saml2AssertionSerializer, overriding ReadAuthnContext 
    /// and WriteAuthnContext to read and write the by-value declaration.
    /// </para>
    /// </remarks>
    public class Saml2AuthenticationContext
    {
        private Collection<Uri> authenticatingAuthorities = new AbsoluteUriCollection();
        private Uri classReference;
        private Uri declarationReference;

        /// <summary>
        /// Creates an instance of Saml2AuthenticationContext.
        /// </summary>
        public Saml2AuthenticationContext()
            : this(null, null)
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AuthenticationContext.
        /// </summary>
        /// <param name="classReference">The class reference of the authentication context.</param>
        public Saml2AuthenticationContext(Uri classReference)
            : this(classReference, null)
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AuthenticationContext.
        /// </summary>
        /// <param name="classReference">The class reference of the authentication context.</param>
        /// <param name="declarationReference">The declaration reference of the authentication context.</param>
        public Saml2AuthenticationContext(Uri classReference, Uri declarationReference)
        {
            // Must be absolute URIs
            if (null != classReference && !classReference.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("classReference", SR.GetString(SR.ID0013));
            }
            
            if (null != declarationReference && !declarationReference.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("declarationReference", SR.GetString(SR.ID0013));
            }

            this.classReference = classReference;
            this.declarationReference = declarationReference;
        }

        /// <summary>
        /// Gets Zero or more unique identifiers of authentication authorities that 
        /// were involved in the authentication of the principal (not including
        /// the assertion issuer, who is presumed to have been involved without
        /// being explicitly named here). [Saml2Core, 2.7.2.2]
        /// </summary>
        public Collection<Uri> AuthenticatingAuthorities
        {
            get { return this.authenticatingAuthorities; }
        }

        /// <summary>
        /// Gets or sets a URI reference identifying an authentication context class that 
        /// describes the authentication context declaration that follows.
        /// [Saml2Core, 2.7.2.2]
        /// </summary>
        public Uri ClassReference
        {
            get 
            { 
                return this.classReference; 
            }

            set
            {
                if (null != value && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.classReference = value;
            }
        }

        /// <summary>
        /// Gets or sets a URI reference that identifies an authentication context 
        /// declaration. [Saml2Core, 2.7.2.2]
        /// </summary>
        public Uri DeclarationReference
        {
            get 
            { 
                return this.declarationReference; 
            }

            set
            {
                if (null != value && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.declarationReference = value;
            }
        }
    }
}
