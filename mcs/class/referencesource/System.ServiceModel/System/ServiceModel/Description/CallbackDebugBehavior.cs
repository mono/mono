//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description 
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    public class CallbackDebugBehavior : IEndpointBehavior
    {
        bool includeExceptionDetailInFaults = false;

        public CallbackDebugBehavior(bool includeExceptionDetailInFaults)
        {
            this.includeExceptionDetailInFaults = includeExceptionDetailInFaults;
        }

        public bool IncludeExceptionDetailInFaults
        {
            get { return this.includeExceptionDetailInFaults; }
            set { this.includeExceptionDetailInFaults = value; }
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
                SR.GetString(SR.SFXEndpointBehaviorUsedOnWrongSide, typeof(CallbackDebugBehavior).Name)));
        }
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            ChannelDispatcher channelDispatcher = behavior.CallbackDispatchRuntime.ChannelDispatcher;
            if (channelDispatcher != null && this.includeExceptionDetailInFaults)
            {
                channelDispatcher.IncludeExceptionDetailInFaults = true;
            }
        }
    }
}
