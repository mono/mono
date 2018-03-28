//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines a descriptor for the SPSSO.
    /// </summary>
    public class ServiceProviderSingleSignOnDescriptor : SingleSignOnDescriptor
    {
        bool _authenticationRequestsSigned;
        bool _wantAssertionsSigned;
        IndexedProtocolEndpointDictionary _assertionConsumerServices = new IndexedProtocolEndpointDictionary();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ServiceProviderSingleSignOnDescriptor()
            : this(new IndexedProtocolEndpointDictionary())
        {
        }

        /// <summary>
        /// Constructs an SPSSO descriptor with the input <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">A <see cref="IndexedProtocolEndpointDictionary"/> object for this instance.</param>
        public ServiceProviderSingleSignOnDescriptor(IndexedProtocolEndpointDictionary collection)
        {
            _assertionConsumerServices = collection;
        }

        /// <summary>
        /// Gets the <see cref="IndexedProtocolEndpointDictionary"/> for this instance.
        /// </summary>
        public IndexedProtocolEndpointDictionary AssertionConsumerServices
        {
            get { return _assertionConsumerServices; }
        }

        /// <summary>
        /// Gets or sets the AuthnRequestsSigned attribute.
        /// </summary>
        public bool AuthenticationRequestsSigned
        {
            get { return _authenticationRequestsSigned; }
            set { _authenticationRequestsSigned = value; }
        }

        /// <summary>
        /// Gets or sets the WantAssertionsSigned attribute.
        /// </summary>
        public bool WantAssertionsSigned
        {
            get { return _wantAssertionsSigned; }
            set { _wantAssertionsSigned = value; }
        }
    }
}
