//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// The identity provider single sign-on descriptor (IDPSSODescriptor) class.
    /// </summary>
    public class IdentityProviderSingleSignOnDescriptor : SingleSignOnDescriptor
    {
        bool _wantAuthenticationRequestsSigned;
        Collection<ProtocolEndpoint> _singleSignOnServices = new Collection<ProtocolEndpoint>();
        Collection<Saml2Attribute> _supportedAttributes = new Collection<Saml2Attribute>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public IdentityProviderSingleSignOnDescriptor()
        {
        }

        /// <summary>
        /// Gets the collection of <see cref="ProtocolEndpoint"/> representing single signon services.
        /// </summary>
        public ICollection<ProtocolEndpoint> SingleSignOnServices
        {
            get { return _singleSignOnServices; }
        }

        /// <summary>
        /// Gets the supported <see cref="Saml2Attribute"/> collection.
        /// </summary>
        public ICollection<Saml2Attribute> SupportedAttributes
        {
            get { return _supportedAttributes; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether authentication requests should be signed.
        /// </summary>
        public bool WantAuthenticationRequestsSigned
        {
            get { return _wantAuthenticationRequestsSigned; }
            set { _wantAuthenticationRequestsSigned = value; }
        }
    }
}
