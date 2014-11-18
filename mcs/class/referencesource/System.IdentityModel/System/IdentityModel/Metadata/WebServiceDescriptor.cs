//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Protocols.WSTrust;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines a web service descriptor.
    /// </summary>
    public abstract class WebServiceDescriptor : RoleDescriptor
    {
        Collection<DisplayClaim> _claimTypesOffered = new Collection<DisplayClaim>();
        Collection<DisplayClaim> _claimTypesRequested = new Collection<DisplayClaim>();
        string _serviceDisplayName;
        string _serviceDescription;
        Collection<EndpointReference> _targetScopes = new Collection<EndpointReference>();
        Collection<Uri> _tokenTypesOffered = new Collection<Uri>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected WebServiceDescriptor()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="DisplayClaim"/> representing the claim types offered.
        /// </summary>
        public ICollection<DisplayClaim> ClaimTypesOffered
        {
            get { return _claimTypesOffered; }
        }

        /// <summary>
        /// Gets a collection of <see cref="DisplayClaim"/> representing the claim types requested.
        /// </summary>
        public ICollection<DisplayClaim> ClaimTypesRequested
        {
            get { return _claimTypesRequested; }
        }

        /// <summary>
        /// Gets or sets the service description.
        /// </summary>
        public string ServiceDescription
        {
            get { return _serviceDescription; }
            set { _serviceDescription = value; }
        }

        /// <summary>
        /// Gets or sets the service display name.
        /// </summary>
        public string ServiceDisplayName
        {
            get { return _serviceDisplayName; }
            set { _serviceDisplayName = value; }
        }

        /// <summary>
        /// Gets a collection of <see cref="EndpointReference"/> representing the target scopes.
        /// </summary>
        public ICollection<EndpointReference> TargetScopes
        {
            get { return _targetScopes; }
        }

        /// <summary>
        /// Gets the collection of token types offered.
        /// </summary>
        public ICollection<Uri> TokenTypesOffered
        {
            get { return _tokenTypesOffered; }
        }
    }
}
