//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [Fx.Tag.XamlVisible(false)]
    public class ServiceDiscoveryBehavior : IServiceBehavior
    {
        NonNullItemCollection<AnnouncementEndpoint> announcementEndpoints;

        public ServiceDiscoveryBehavior()
        {
            this.announcementEndpoints = new NonNullItemCollection<AnnouncementEndpoint>();
        }

        public Collection<AnnouncementEndpoint> AnnouncementEndpoints
        {
            get
            {
                return this.announcementEndpoints;
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }

            List<ServiceEndpoint> appEndpoints = this.GetApplicationEndpoints(serviceDescription);

            DiscoveryServiceExtension discoveryServiceExtension = 
                serviceHostBase.Extensions.Find<DiscoveryServiceExtension>();

            if (discoveryServiceExtension == null)
            {
                if (serviceDescription.Endpoints.Count > appEndpoints.Count)
                {
                    discoveryServiceExtension = 
                        new DefaultDiscoveryServiceExtension(DiscoveryDefaults.DuplicateMessageHistoryLength);
                }
                else
                {
                    discoveryServiceExtension = 
                        new DefaultDiscoveryServiceExtension(0);
                }

                serviceHostBase.Extensions.Add(discoveryServiceExtension);
            }            

            for (int i = 0; i < appEndpoints.Count; i++)
            {
                appEndpoints[i].Behaviors.Add(
                    new EndpointDiscoveryMetadataInitializer(
                    discoveryServiceExtension.InternalPublishedEndpoints));
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }

            DiscoveryServiceExtension discoveryServiceExtension = serviceHostBase.Extensions.Find<DiscoveryServiceExtension>();
            if (discoveryServiceExtension != null)
            {
                DiscoveryService discoveryService = discoveryServiceExtension.ValidateAndGetDiscoveryService();

                ServiceDiscoveryBehavior.SetDiscoveryImplementation(serviceHostBase, discoveryService);

                if (this.announcementEndpoints.Count > 0)
                {
                    serviceHostBase.ChannelDispatchers.Add(
                        new OnlineAnnouncementChannelDispatcher(
                        serviceHostBase,
                        this.announcementEndpoints,
                        discoveryServiceExtension.InternalPublishedEndpoints,
                        discoveryService.MessageSequenceGenerator));

                    serviceHostBase.ChannelDispatchers.Insert(0,
                        new OfflineAnnouncementChannelDispatcher(
                        serviceHostBase,
                        this.announcementEndpoints,
                        discoveryServiceExtension.InternalPublishedEndpoints,
                        discoveryService.MessageSequenceGenerator));
                }
            }
        }

        static void SetDiscoveryImplementation(ServiceHostBase host, DiscoveryService discoveryService)
        {
            foreach (ChannelDispatcherBase channelDispatcherBase in host.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if ((endpointDispatcher != null) && EndpointDiscoveryMetadata.IsDiscoverySystemEndpoint(endpointDispatcher))
                        {
                            SetDiscoveryImplementation(endpointDispatcher, discoveryService);
                        }
                    }
                }
            }
        }

        static void SetDiscoveryImplementation(EndpointDispatcher endpointDispatcher, DiscoveryService discoveryService)
        {
            DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
            dispatchRuntime.SynchronizationContext = null;
            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;
            ServiceDiscoveryInstanceContextProvider provider = new ServiceDiscoveryInstanceContextProvider(discoveryService);
            dispatchRuntime.InstanceContextProvider = provider;
            dispatchRuntime.InstanceProvider = provider;
            dispatchRuntime.Type = discoveryService.GetType();
        }

        List<ServiceEndpoint> GetApplicationEndpoints(ServiceDescription serviceDescription)
        {
            List<ServiceEndpoint> appEndpoints = new List<ServiceEndpoint>(serviceDescription.Endpoints.Count);
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (!EndpointDiscoveryMetadata.IsDiscoverySystemEndpoint(endpoint))
                {
                    appEndpoints.Add(endpoint);
                }
            }

            return appEndpoints;
        }

        class EndpointDiscoveryMetadataInitializer : IEndpointBehavior
        {
            Collection<EndpointDiscoveryMetadata> publishedEndpointCollection;

            internal EndpointDiscoveryMetadataInitializer(Collection<EndpointDiscoveryMetadata> publishedEndpointCollection)
            {
                this.publishedEndpointCollection = publishedEndpointCollection;
            }

            void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                EndpointDiscoveryMetadata endpointDiscoveryMetadata = EndpointDiscoveryMetadata.FromServiceEndpoint(endpoint, endpointDispatcher);

                if (endpointDiscoveryMetadata != null)
                {
                    this.publishedEndpointCollection.Add(endpointDiscoveryMetadata);
                }
            }

            void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
            {
            }
        }
    }
}
