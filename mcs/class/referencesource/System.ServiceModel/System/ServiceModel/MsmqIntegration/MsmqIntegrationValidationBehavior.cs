//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Channels;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    class MsmqIntegrationValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        static MsmqIntegrationValidationBehavior instance;

        internal static MsmqIntegrationValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                    instance = new MsmqIntegrationValidationBehavior();
                return instance;
            }
        }

        MsmqIntegrationValidationBehavior() { }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");

            ContractDescription contract = serviceEndpoint.Contract;
            Binding binding = serviceEndpoint.Binding;
            if (NeedValidateBinding(binding))
            {
                ValidateHelper(contract, binding, null);
            }
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
                if (NeedValidateBinding(endpoint.Binding))
                {
                    ValidateHelper(endpoint.Contract, endpoint.Binding, description);
                    break;
                }
            }
        }

        bool NeedValidateBinding(Binding binding)
        {
            if (binding is MsmqIntegrationBinding)
                return true;

            if (binding is CustomBinding)
            {
                CustomBinding customBinding = new CustomBinding(binding);
                return (customBinding.Elements.Find<MsmqIntegrationBindingElement>() != null);
            }

            return false;
        }

        void ValidateHelper(ContractDescription contract, Binding binding, ServiceDescription description)
        {
            foreach (OperationDescription operation in contract.Operations)
            {
                // since this is one-way, we can only have one message (one-way requirement is validated elsewhere)
                MessageDescription message = operation.Messages[0];

                if ((message.Body.Parts.Count == 0) && (message.Headers.Count == 0))
                    // all message parts are properties, great
                    continue;

                if (message.Body.Parts.Count == 1) // Single MsmqMessage<> argument is also legal
                {
                    Type type = message.Body.Parts[0].Type;
                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(MsmqMessage<>)))
                        continue;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                                                              SR.GetString(SR.MsmqInvalidServiceOperationForMsmqIntegrationBinding, binding.Name, operation.Name, contract.Name)));
            }
        }
    }
}

