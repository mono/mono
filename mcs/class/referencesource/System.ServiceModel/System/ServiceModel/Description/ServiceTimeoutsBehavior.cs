//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    internal class ServiceTimeoutsBehavior : IServiceBehavior
    {
        TimeSpan transactionTimeout = TimeSpan.Zero;

        internal ServiceTimeoutsBehavior(TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
        }

        internal TimeSpan TransactionTimeout
        {
            get { return this.transactionTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));

                }
                this.transactionTimeout = value;
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
            if (this.transactionTimeout != TimeSpan.Zero)
            {
                for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if (channelDispatcher != null && channelDispatcher.HasApplicationEndpoints())
                    {
                        if ((channelDispatcher.TransactionTimeout == TimeSpan.Zero) ||
                            (channelDispatcher.TransactionTimeout > this.transactionTimeout))
                        {
                            channelDispatcher.TransactionTimeout = this.transactionTimeout;
                        }
                    }
                }
            }
        }
    }
}

