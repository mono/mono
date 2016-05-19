//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines an SSO descriptor.
    /// </summary>
    public class SingleSignOnDescriptor : RoleDescriptor
    {
        IndexedProtocolEndpointDictionary artifactResolutionServices = new IndexedProtocolEndpointDictionary();
        Collection<ProtocolEndpoint> singleLogoutServices = new Collection<ProtocolEndpoint>();
        Collection<Uri> nameIdFormats = new Collection<Uri>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SingleSignOnDescriptor()
        {
        }

        /// <summary>
        /// Gets the a collection of URIs representing the supported name identifier formats.
        /// </summary>
        public ICollection<Uri> NameIdentifierFormats
        {
            get { return this.nameIdFormats; }
        }

        /// <summary>
        /// Gets the <see cref="IndexedProtocolEndpointDictionary"/> instance representing the artifact resolution services.
        /// </summary>
        public IndexedProtocolEndpointDictionary ArtifactResolutionServices
        {
            get { return this.artifactResolutionServices; }
        }

        /// <summary>
        /// Gets the collection of <see cref="ProtocolEndpoint"/> representing the single logout service endpoints.
        /// </summary>
        public Collection<ProtocolEndpoint> SingleLogoutServices
        {
            get { return this.singleLogoutServices; }
        }
    }
}
