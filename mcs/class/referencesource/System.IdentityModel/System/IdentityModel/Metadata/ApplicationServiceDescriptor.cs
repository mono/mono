//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Protocols.WSTrust;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the application service descriptor and its endpoints.
    /// </summary>
    public class ApplicationServiceDescriptor : WebServiceDescriptor
    {
        Collection<EndpointReference> endpoints = new Collection<EndpointReference>();
        Collection<EndpointReference> passiveRequestorEndpoints = new Collection<EndpointReference>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ApplicationServiceDescriptor()
        {
        }

        /// <summary>
        /// Gets the endpoints of this application service.
        /// </summary>
        public ICollection<EndpointReference> Endpoints
        {
            get { return this.endpoints; }
        }

        /// <summary>
        /// Gets the passive requestor endpoints of this application service.
        /// </summary>
        public ICollection<EndpointReference> PassiveRequestorEndpoints
        {
            get { return this.passiveRequestorEndpoints; }
        }
    }
}
