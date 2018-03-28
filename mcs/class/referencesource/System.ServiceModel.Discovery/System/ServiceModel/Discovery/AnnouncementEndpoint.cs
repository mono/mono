//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public class AnnouncementEndpoint : ServiceEndpoint
    {        
        TimeSpan maxAnnouncementDelay;
        DiscoveryVersion discoveryVersion;

        public AnnouncementEndpoint()
            : this(DiscoveryVersion.DefaultDiscoveryVersion)
        {
        }

        public AnnouncementEndpoint(Binding binding, EndpointAddress address)
            : this(DiscoveryVersion.DefaultDiscoveryVersion, binding, address)
        {
        }

        public AnnouncementEndpoint(DiscoveryVersion discoveryVersion)
            : this(discoveryVersion, null, null)
        {
        }

        public AnnouncementEndpoint(DiscoveryVersion discoveryVersion, Binding binding, EndpointAddress address)
            : base(GetAnnouncementContract(discoveryVersion))
        {
            // Send replies async to maintain performance
            this.EndpointBehaviors.Add(new DispatcherSynchronizationBehavior { AsynchronousSendEnabled = true });

            this.discoveryVersion = discoveryVersion;
            base.Address = address;
            base.Binding = binding;
        }

        public TimeSpan MaxAnnouncementDelay
        {
            get
            {
                return this.maxAnnouncementDelay;
            }
            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value, "value");
                this.maxAnnouncementDelay = value;
            }
        }

        public DiscoveryVersion DiscoveryVersion
        {
            get
            {
                return this.discoveryVersion;
            }
        }

        static ContractDescription GetAnnouncementContract(DiscoveryVersion discoveryVersion)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }

            return discoveryVersion.Implementation.GetAnnouncementContract();
        }
    }
}
