//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    class PeerValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        public static PeerValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                    instance = new PeerValidationBehavior();
                return instance;
            }
        }

        static PeerValidationBehavior instance;

        PeerValidationBehavior() { }

        static bool IsRequestReplyContract(ContractDescription contract)
        {
            bool requestReply = false;

            foreach (OperationDescription operation in contract.Operations)
            {
                if (operation.Messages.Count > 1)   // Request-reply
                {
                    requestReply = true;
                    break;
                }
            }
            return requestReply;
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");

            ContractDescription contract = serviceEndpoint.Contract;
            Binding binding = serviceEndpoint.Binding;
            ValidateHelper(contract, binding);
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                ValidateHelper(endpoint.Contract, endpoint.Binding);
            }
        }

        // SM doesn't support request-reply message pattern over multi-point channels correctly, so, disabling
        // request-reply for NetPeerTcpBinding.  (Advanced users may find a way to implement request-reply over
        // a CustomBinding that includes PeerTransportBE.)
        void ValidateHelper(ContractDescription contract, Binding binding)
        {
            if (binding is NetPeerTcpBinding && IsRequestReplyContract(contract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.BindingDoesnTSupportRequestReplyButContract1, binding.Name)));
            }
        }
    }
}
