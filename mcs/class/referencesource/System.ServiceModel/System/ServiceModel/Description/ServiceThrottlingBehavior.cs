//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    public class ServiceThrottlingBehavior : IServiceBehavior
    {
        //For V1: Default MaxConcurrentInstances should not enforce any throttle
        //But still it should not be set to Int32.MAX;
        //So compute default MaxInstances to be large enough to support MaxCalls & MaxSessions.
        internal static int DefaultMaxConcurrentInstances = ServiceThrottle.DefaultMaxConcurrentCallsCpuCount + ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount;

        int calls = ServiceThrottle.DefaultMaxConcurrentCallsCpuCount;
        int sessions = ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount;
        int instances = Int32.MaxValue;
        bool maxInstanceSetExplicitly;

        public int MaxConcurrentCalls
        {
            get { return this.calls; }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxThrottleLimitMustBeGreaterThanZero0)));

                this.calls = value;
            }
        }

        public int MaxConcurrentSessions
        {
            get { return this.sessions; }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxThrottleLimitMustBeGreaterThanZero0)));

                this.sessions = value;
            }
        }

        public int MaxConcurrentInstances
        {
            get
            {
                if (this.maxInstanceSetExplicitly)
                {
                    return this.instances;
                }
                else
                {
                    //For V1: Default MaxConcurrentInstances should not enforce any throttle
                    //But still it should not be set to Int32.MAX;
                    //So compute default MaxInstances to be large enough to support MaxCalls & MaxSessions.
                    this.instances = this.calls + this.sessions;

                    if (this.instances < 0)
                    {
                        this.instances = Int32.MaxValue;
                    }
                }
                return this.instances;
            }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxThrottleLimitMustBeGreaterThanZero0)));

                this.instances = value;
                this.maxInstanceSetExplicitly = true;
            }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));

            ServiceThrottle serviceThrottle = serviceHostBase.ServiceThrottle;
            serviceThrottle.MaxConcurrentCalls = this.calls;
            serviceThrottle.MaxConcurrentSessions = this.sessions;
            serviceThrottle.MaxConcurrentInstances = this.MaxConcurrentInstances;

            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    channelDispatcher.ServiceThrottle = serviceThrottle;
                }
            }
        }
    }
}
