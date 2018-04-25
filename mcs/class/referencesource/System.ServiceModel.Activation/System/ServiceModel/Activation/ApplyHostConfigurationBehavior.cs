//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    //This is the behavior is intended to auto apply IIS/AspNet configuration to WCF post design time
    class ApplyHostConfigurationBehavior : IServiceBehavior
    {
        internal ApplyHostConfigurationBehavior()
        {
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase service)
        {
            if (service.Description.Endpoints != null && ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                FailActivationIfEndpointsHaveAbsoluteAddress(service);
            }
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase service, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase service)
        {
            if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                SetEndpointAddressFilterToIgnorePort(service);
            }
        }

        void SetEndpointAddressFilterToIgnorePort(ServiceHostBase service)
        {
            for (int i = 0; i < service.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = service.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    if (IsSchemeHttpOrHttps(channelDispatcher.Listener.Uri.Scheme))
                    {
                        for (int j = 0; j < channelDispatcher.Endpoints.Count; j++)
                        {
                            EndpointDispatcher endpointDispatcher = channelDispatcher.Endpoints[j];
                            EndpointAddressMessageFilter endpointAddressMessageFilter = endpointDispatcher.AddressFilter as EndpointAddressMessageFilter;
                            if (endpointAddressMessageFilter != null)
                            {
                                endpointAddressMessageFilter.ComparePort = false;
                            }
                        }
                    }
                }
            }
        }

        void FailActivationIfEndpointsHaveAbsoluteAddress(ServiceHostBase service)
        {
            foreach (ServiceEndpoint endpoint in service.Description.Endpoints)
            {
                if (IsSchemeHttpOrHttps(endpoint.Binding.Scheme))
                {
                    if (endpoint.UnresolvedListenUri != null)
                    {
                        ThrowIfAbsolute(endpoint.UnresolvedListenUri);
                    }
                    else
                    {
                        //If the listen URI is not null, we shouldn't care about address. Because there are 
                        //customers who have following config (for load balancer scenarios) - Note ExtraFolder and https
                        //listen URI - http://localhost/App1/x.svc
                        //Address -    https://externalhost/ExtranFolder/App1/x.svc
                        ThrowIfAbsolute(endpoint.UnresolvedAddress);
                    }
                }
            }

            ServiceDebugBehavior debugBehavior = service.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior != null)
            {
                if (debugBehavior.HttpHelpPageEnabled)
                {
                    ThrowIfAbsolute(debugBehavior.HttpHelpPageUrl);
                }
                if (debugBehavior.HttpsHelpPageEnabled)
                {
                    ThrowIfAbsolute(debugBehavior.HttpsHelpPageUrl);
                }
            }

            ServiceMetadataBehavior metadataBehavior = service.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior != null)
            {
                if (metadataBehavior.HttpGetEnabled)
                {
                    ThrowIfAbsolute(metadataBehavior.HttpGetUrl);
                }
                if (metadataBehavior.HttpsGetEnabled)
                {
                    ThrowIfAbsolute(metadataBehavior.HttpsGetUrl);
                }
            }
        }

        static void ThrowIfAbsolute(Uri uri)
        {
            if (uri != null && uri.IsAbsoluteUri)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_SharedEndpointRequiresRelativeEndpoint(uri.ToString())));
            }
        }

        static bool IsSchemeHttpOrHttps(string scheme)
        {
            return string.Compare(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0 ||
                   string.Compare(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
            

