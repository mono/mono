//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public class DiscoveryEndpoint : ServiceEndpoint
    {
        readonly DiscoveryOperationContextExtension discoveryOperationContextExtension;                

        public DiscoveryEndpoint()
            : this(DiscoveryVersion.DefaultDiscoveryVersion, ServiceDiscoveryMode.Managed)
        {            
        }

        public DiscoveryEndpoint(Binding binding, EndpointAddress endpointAddress)
            : this(DiscoveryVersion.DefaultDiscoveryVersion, ServiceDiscoveryMode.Managed, binding, endpointAddress)
        {
        }

        public DiscoveryEndpoint(DiscoveryVersion discoveryVersion, ServiceDiscoveryMode discoveryMode)
            : this(discoveryVersion, discoveryMode, null, null)
        {            
        }

        public DiscoveryEndpoint(DiscoveryVersion discoveryVersion, ServiceDiscoveryMode discoveryMode, Binding binding, EndpointAddress endpointAddress)
            : base(GetDiscoveryContract(discoveryVersion, discoveryMode))
        {
            base.IsSystemEndpoint = true;
                           
            this.discoveryOperationContextExtension = new DiscoveryOperationContextExtension(TimeSpan.Zero, discoveryMode, discoveryVersion);

            base.Behaviors.Add(new DiscoveryOperationContextExtensionInitializer(this.discoveryOperationContextExtension));
            base.Behaviors.Add(new DiscoveryEndpointValidator());

            base.Address = endpointAddress;
            base.Binding = binding;
        }

        public TimeSpan MaxResponseDelay
        {
            get
            {
                return this.discoveryOperationContextExtension.MaxResponseDelay;
            }

            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value, "value");
                this.discoveryOperationContextExtension.MaxResponseDelay = value;
            }
        }

        public DiscoveryVersion DiscoveryVersion
        {
            get
            {
                return this.discoveryOperationContextExtension.DiscoveryVersion;
            }
        }

        public ServiceDiscoveryMode DiscoveryMode
        {
            get
            {
                return this.discoveryOperationContextExtension.DiscoveryMode;
            }
        }

        static ContractDescription GetDiscoveryContract(DiscoveryVersion discoveryVersion, ServiceDiscoveryMode discoveryMode)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }

            return discoveryVersion.Implementation.GetDiscoveryContract(discoveryMode);
        }
    }
}
