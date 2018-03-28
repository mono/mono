//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;

    // This attribute specifies what the service implementation 
    // requires from the binding that dispatches messages.
    [AttributeUsage(ServiceModelAttributeTargets.ContractBehavior, AllowMultiple = true)]
    public sealed class DeliveryRequirementsAttribute : Attribute, IContractBehavior, IContractBehaviorAttribute
    {
        Type contractType;
        QueuedDeliveryRequirementsMode queuedDeliveryRequirements = QueuedDeliveryRequirementsMode.Allowed;
        bool requireOrderedDelivery = false;

        // Used to implement IContractBehaviorAttribute; if null, DeliveryRequirementsAttribute applies to any contract
        public Type TargetContract
        {
            get { return contractType; }
            set { contractType = value; }
        }

        // RequireQueuedDelivery: Validates that any binding associated
        // with the service/channel supports Queued 
        // delivery. 
        //
        // DisallowQueuedDelivery: Validates that no binding associated
        // with the service/channel supports Queued 
        // delivery. 
        //
        // Ignore: Agnostic
        public QueuedDeliveryRequirementsMode QueuedDeliveryRequirements
        {
            get { return queuedDeliveryRequirements; }
            set
            {
                if (QueuedDeliveryRequirementsModeHelper.IsDefined(value))
                {
                    queuedDeliveryRequirements = value;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
            }
        }

        // True: Validates that any binding associated
        // with the service/channel supports Ordered 
        // delivery. 
        //
        // False: Does no validation.
        public bool RequireOrderedDelivery
        {
            get { return requireOrderedDelivery; }
            set { requireOrderedDelivery = value; }
        }

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            ValidateEndpoint(endpoint);
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
        }

        void ValidateEndpoint(ServiceEndpoint endpoint)
        {
            string name = endpoint.Contract.ContractType.Name;
            EnsureQueuedDeliveryRequirements(name, endpoint.Binding);
            EnsureOrderedDeliveryRequirements(name, endpoint.Binding);
        }

        void EnsureQueuedDeliveryRequirements(string name, Binding binding)
        {
            if (QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.Required
                || QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.NotAllowed)
            {
                IBindingDeliveryCapabilities caps = binding.GetProperty<IBindingDeliveryCapabilities>(new BindingParameterCollection());
                if (caps == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SinceTheBindingForDoesnTSupportIBindingCapabilities2_1, name)));
                }
                else
                {
                    bool queuedTransport = caps.QueuedDelivery;
                    if (QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.Required && !queuedTransport)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.BindingRequirementsAttributeRequiresQueuedDelivery1, name)));
                    }
                    else if (QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.NotAllowed && queuedTransport)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.BindingRequirementsAttributeDisallowsQueuedDelivery1, name)));
                    }
                }
            }
        }

        void EnsureOrderedDeliveryRequirements(string name, Binding binding)
        {
            if (RequireOrderedDelivery)
            {
                IBindingDeliveryCapabilities caps = binding.GetProperty<IBindingDeliveryCapabilities>(new BindingParameterCollection());
                if (caps == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SinceTheBindingForDoesnTSupportIBindingCapabilities1_1, name)));
                }
                else
                {
                    if (!caps.AssuresOrderedDelivery)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.TheBindingForDoesnTSupportOrderedDelivery1, name)));
                    }
                }
            }
        }
    }
}

