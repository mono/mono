//------------------------------------------------------------------------------
// <copyright file="ResourceProviderFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Security.Permissions;

    /*
     * Interface used to create app and page IResourceProvider objects
     */
    public abstract class ResourceProviderFactory {
        public abstract IResourceProvider CreateGlobalResourceProvider(string classKey);
        public abstract IResourceProvider CreateLocalResourceProvider(string virtualPath);
    }

    /*
     * Implementation of ResourceProviderFactory for ResourceManager based resources
     */
    internal class ResXResourceProviderFactory: ResourceProviderFactory {
        public override IResourceProvider CreateGlobalResourceProvider(string classKey) {
            return new GlobalResXResourceProvider(classKey);
        }

        public override IResourceProvider CreateLocalResourceProvider(string virtualPath) {
            return new LocalResXResourceProvider(VirtualPath.Create(virtualPath));
        }
    }

}

