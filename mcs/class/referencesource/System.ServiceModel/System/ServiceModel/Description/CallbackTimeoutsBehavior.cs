//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class CallbackTimeoutsBehavior : IEndpointBehavior
    {
        TimeSpan transactionTimeout = TimeSpan.Zero;

        public TimeSpan TransactionTimeout
        {
            get { return this.transactionTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.transactionTimeout = value;
            }
        }

        public CallbackTimeoutsBehavior()
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.SFXEndpointBehaviorUsedOnWrongSide, typeof(CallbackTimeoutsBehavior).Name)));
        }
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (this.transactionTimeout != TimeSpan.Zero)
            {
                ChannelDispatcher channelDispatcher = behavior.CallbackDispatchRuntime.ChannelDispatcher;
                if ((channelDispatcher != null) &&
                    (channelDispatcher.TransactionTimeout == TimeSpan.Zero) ||
                    (channelDispatcher.TransactionTimeout > this.transactionTimeout))
                {
                    channelDispatcher.TransactionTimeout = this.transactionTimeout;
                }
            }
        }
    }
}
