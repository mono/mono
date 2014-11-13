//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public abstract class DiscoveryServiceExtension : IExtension<ServiceHostBase>
    {
        ServiceHostBase owner;
        PublishedEndpointCollection publishedEndpoints;
        ReadOnlyCollection<EndpointDiscoveryMetadata> readOnlyPublishedEndpoints;

        protected DiscoveryServiceExtension()
        {
            this.publishedEndpoints = new PublishedEndpointCollection();
            this.readOnlyPublishedEndpoints = new ReadOnlyCollection<EndpointDiscoveryMetadata>(this.publishedEndpoints);
        }

        public ReadOnlyCollection<EndpointDiscoveryMetadata> PublishedEndpoints
        {
            get
            {
                return this.readOnlyPublishedEndpoints;
            }
        }

        internal Collection<EndpointDiscoveryMetadata> InternalPublishedEndpoints
        {
            get
            {
                return this.publishedEndpoints;
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw FxTrace.Exception.ArgumentNull("owner");
            }
            if (this.owner != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoveryExtensionAlreadyAttached));
            }

            this.owner = owner;
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw FxTrace.Exception.ArgumentNull("owner");
            }
            if (this.owner != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoveryExtensionCannotBeDetached));
            }
        }

        internal DiscoveryService ValidateAndGetDiscoveryService()
        {
            DiscoveryService discoveryService = this.GetDiscoveryService();

            if (discoveryService == null)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(
                        SR.DiscoveryMethodImplementationReturnsNull("GetDiscoveryService", this.GetType())));
            }

            return discoveryService;
        }

        protected abstract DiscoveryService GetDiscoveryService();

        class PublishedEndpointCollection : NonNullItemCollection<EndpointDiscoveryMetadata>
        {
            protected override void InsertItem(int index, EndpointDiscoveryMetadata item)
            {
                base.InsertItem(index, item);
                item.Open();
            }

            protected override void SetItem(int index, EndpointDiscoveryMetadata item)
            {
                base.SetItem(index, item);
                item.Open();
            }
        }
    }
}
