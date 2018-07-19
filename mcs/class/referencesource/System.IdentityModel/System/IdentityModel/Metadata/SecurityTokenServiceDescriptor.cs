//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Collections.ObjectModel;
using System.IdentityModel.Protocols.WSTrust;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines a Service Descriptor for a security token service.
    /// </summary>
    public class SecurityTokenServiceDescriptor : WebServiceDescriptor
    {
        Collection<EndpointReference> securityTokenServiceEndpoints = new Collection<EndpointReference>();
        Collection<EndpointReference> passiveRequestorEndpoints = new Collection<EndpointReference>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SecurityTokenServiceDescriptor()
        {
        }

        /// <summary>
        /// Gets the collection of <see cref="EndpointReference"/> representing the endpoints of the security token service.
        /// </summary>
        public Collection<EndpointReference> SecurityTokenServiceEndpoints
        {
            get { return this.securityTokenServiceEndpoints; }
        }

        /// <summary>
        /// Gets the collection of <see cref="EndpointReference"/> representing the passive requestor endpoints.
        /// </summary>
        public Collection<EndpointReference> PassiveRequestorEndpoints
        {
            get { return this.passiveRequestorEndpoints; }
        }
    }
}
