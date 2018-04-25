//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    [Fx.Tag.XamlVisible(false)]
    public sealed class RoutingExtension : IExtension<ServiceHostBase>
    {
        volatile RoutingConfiguration configuration;

        internal RoutingExtension(RoutingConfiguration configuration)
        {
            Fx.Assert(configuration != null, "Configuration required");

            if (TD.RoutingServiceDisplayConfigIsEnabled())
            {
                TD.RoutingServiceDisplayConfig(
                    configuration.RouteOnHeadersOnly.ToString(TD.Culture), 
                    configuration.SoapProcessingEnabled.ToString(TD.Culture),
                    configuration.EnsureOrderedDispatch.ToString(TD.Culture));
            }
            this.configuration = configuration;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "This gets called in RoutingService..ctor")]
        internal RoutingConfiguration RoutingConfiguration
        {
            get
            {
                return this.configuration;
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
        }

        public void ApplyConfiguration(RoutingConfiguration routingConfiguration)
        {
            if (routingConfiguration == null)
            {
                throw FxTrace.Exception.ArgumentNull("routingConfiguration");
            }

            if (TD.RoutingServiceConfigurationAppliedIsEnabled())
            {
                TD.RoutingServiceConfigurationApplied();
            }
            if (TD.RoutingServiceDisplayConfigIsEnabled())
            {
                TD.RoutingServiceDisplayConfig(
                    routingConfiguration.RouteOnHeadersOnly.ToString(TD.Culture), 
                    routingConfiguration.SoapProcessingEnabled.ToString(TD.Culture),
                    routingConfiguration.EnsureOrderedDispatch.ToString(TD.Culture));
            }

            this.configuration = routingConfiguration;
        }
    }
}
