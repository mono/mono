//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    // This behavior sets the contract filter and unhandled operation invoker in the dispatch 
    // runtime to avoid contract filter mismatch exceptions raised by runtime during normal operation.
    // Since the different discovery messages from different versions are sent over the same multicast 
    // address and port, it is normal for an endpoint to receive the messages that are not matching 
    // its contract. This behavior is only added to UdpDiscoveryEndpoint and UdpAnnouncementEndpoint.
    class UdpContractFilterBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (clientRuntime != null && clientRuntime.CallbackDispatchRuntime != null && clientRuntime.CallbackDispatchRuntime.UnhandledDispatchOperation != null)
            {
                clientRuntime.CallbackDispatchRuntime.UnhandledDispatchOperation.Invoker = new UnhandledActionOperationInvoker();
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDispatcher");
            }

            endpointDispatcher.ContractFilter = new MatchAllMessageFilter();
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker = new UnhandledActionOperationInvoker();
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        class UnhandledActionOperationInvoker : IOperationInvoker
        {
            public bool IsSynchronous
            {
                get
                {
                    return true;
                }
            }

            public object[] AllocateInputs()
            {
                return EmptyArray.Allocate(1);
            }

            public object Invoke(object instance, object[] inputs, out object[] outputs)
            {
                outputs = EmptyArray.Allocate(0);
                return new NullMessage();
            }

            public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }
        }
    }
}
