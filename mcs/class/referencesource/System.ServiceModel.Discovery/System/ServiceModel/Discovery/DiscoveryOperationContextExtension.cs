//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [Fx.Tag.XamlVisible(false)] 
    public class DiscoveryOperationContextExtension : IExtension<OperationContext>
    {
        TimeSpan maxResponseDelay;
        ServiceDiscoveryMode discoveryMode;
        DiscoveryVersion discoveryVersion;

        internal DiscoveryOperationContextExtension()
            : this(TimeSpan.Zero, ServiceDiscoveryMode.Adhoc, DiscoveryVersion.DefaultDiscoveryVersion)
        {
        }

        internal DiscoveryOperationContextExtension(TimeSpan maxResponseDelay, ServiceDiscoveryMode discoveryMode, DiscoveryVersion discoveryVersion)
        {            
            TimeoutHelper.ThrowIfNegativeArgument(maxResponseDelay, "maxResponseDelay");
            Fx.Assert(discoveryVersion != null, "discoveryVersion can't be null");

            this.maxResponseDelay = maxResponseDelay;
            this.discoveryMode = discoveryMode;
            this.discoveryVersion = discoveryVersion;
        }

        public TimeSpan MaxResponseDelay
        {
            get
            {
                return this.maxResponseDelay;
            }
            internal set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value, "values");
                this.maxResponseDelay = value;
            }
        }

        public ServiceDiscoveryMode DiscoveryMode
        {
            get
            {
                return this.discoveryMode;
            }
        }

        public DiscoveryVersion DiscoveryVersion
        {
            get
            {
                return this.discoveryVersion;
            }
        }

        void IExtension<OperationContext>.Attach(OperationContext owner)
        {
        }

        void IExtension<OperationContext>.Detach(OperationContext owner)
        {
        }
    }
}
