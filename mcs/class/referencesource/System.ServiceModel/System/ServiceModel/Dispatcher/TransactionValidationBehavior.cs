//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Transactions;

    class TransactionValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        static TransactionValidationBehavior instance;

        internal static TransactionValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                    instance = new TransactionValidationBehavior();
                return instance;
            }
        }

        TransactionValidationBehavior() { }

        void ValidateTransactionFlowRequired(string resource, string name, ServiceEndpoint endpoint)
        {
            bool anOperationRequiresTxFlow = false;
            for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
            {
                OperationDescription operationDescription = endpoint.Contract.Operations[i];
                TransactionFlowAttribute transactionFlow = operationDescription.Behaviors.Find<TransactionFlowAttribute>();
                if (transactionFlow != null && transactionFlow.Transactions == TransactionFlowOption.Mandatory)
                {
                    anOperationRequiresTxFlow = true;
                    break;
                }
            }

            if (anOperationRequiresTxFlow)
            {
                CustomBinding binding = new CustomBinding(endpoint.Binding);
                TransactionFlowBindingElement transactionFlowBindingElement =
                                              binding.Elements.Find<TransactionFlowBindingElement>();

                if (transactionFlowBindingElement == null || !transactionFlowBindingElement.Transactions)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        String.Format(Globalization.CultureInfo.CurrentCulture, SR.GetString(resource), name, binding.Name)));
                }
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            ValidateTransactionFlowRequired(SR.ChannelHasAtLeastOneOperationWithTransactionFlowEnabled,
                           serviceEndpoint.Contract.Name,
                           serviceEndpoint);
            EnsureNoOneWayTransactions(serviceEndpoint);
            ValidateNoMSMQandTransactionFlow(serviceEndpoint);
            ValidateCallbackBehaviorAttributeWithNoScopeRequired(serviceEndpoint);
            OperationDescription autoCompleteFalseOperation = GetAutoCompleteFalseOperation(serviceEndpoint);
            if (autoCompleteFalseOperation != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxTransactionAutoCompleteFalseOnCallbackContract, autoCompleteFalseOperation.Name, serviceEndpoint.Contract.Name)));
            }

        }

        void ValidateCallbackBehaviorAttributeWithNoScopeRequired(ServiceEndpoint endpoint)
        {
            // If the endpoint has no operations with TransactionScopeRequired=true, disallow any
            // transaction-related properties on the CallbackBehaviorAttribute
            if (!HasTransactedOperations(endpoint))
            {
                CallbackBehaviorAttribute attribute = endpoint.Behaviors.Find<CallbackBehaviorAttribute>();
                if (attribute != null)
                {
                    if (attribute.TransactionTimeoutSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionTransactionTimeoutNeedsScope, endpoint.Contract.Name)));
                    }

                    if (attribute.IsolationLevelSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionIsolationLevelNeedsScope, endpoint.Contract.Name)));
                    }
                }
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

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription service, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription service, ServiceHostBase serviceHostBase)
        {
            if (service == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");

            ValidateNotConcurrentWhenReleaseServiceInstanceOnTxComplete(service);
            bool singleThreaded = IsSingleThreaded(service);

            for (int i = 0; i < service.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = service.Endpoints[i];

                ValidateTransactionFlowRequired(SR.ServiceHasAtLeastOneOperationWithTransactionFlowEnabled,
                               service.Name,
                               endpoint);
                EnsureNoOneWayTransactions(endpoint);
                ValidateNoMSMQandTransactionFlow(endpoint);

                ContractDescription contract = endpoint.Contract;
                for (int j = 0; j < contract.Operations.Count; j++)
                {
                    OperationDescription operation = contract.Operations[j];
                    ValidateScopeRequiredAndAutoComplete(operation, singleThreaded, contract.Name);
                }

                ValidateAutoCompleteFalseRequirements(service, endpoint);
            }

            ValidateServiceBehaviorAttributeWithNoScopeRequired(service);
            ValidateTransactionAutoCompleteOnSessionCloseHasSession(service);
        }


        void ValidateAutoCompleteFalseRequirements(ServiceDescription service, ServiceEndpoint endpoint)
        {
            OperationDescription autoCompleteFalseOperation = GetAutoCompleteFalseOperation(endpoint);

            if (autoCompleteFalseOperation != null)
            {
                // Does the service have InstanceContextMode.PerSession or Shareable?
                ServiceBehaviorAttribute serviceBehavior = service.Behaviors.Find<ServiceBehaviorAttribute>();
                if (serviceBehavior != null)
                {
                    InstanceContextMode instanceMode = serviceBehavior.InstanceContextMode;
                    if (instanceMode != InstanceContextMode.PerSession)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionAutoCompleteFalseAndInstanceContextMode,
                            endpoint.Contract.Name, autoCompleteFalseOperation.Name)));
                    }
                }

                // Does the binding support sessions?
                if (!autoCompleteFalseOperation.IsInsideTransactedReceiveScope)
                {
                    if (!RequiresSessions(endpoint))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionAutoCompleteFalseAndSupportsSession,
                            endpoint.Contract.Name, autoCompleteFalseOperation.Name)));
                    }
                }
            }
        }

        OperationDescription GetAutoCompleteFalseOperation(ServiceEndpoint endpoint)
        {
            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                if (!IsAutoComplete(operation))
                {
                    return operation;
                }
            }
            return null;
        }

        void ValidateTransactionAutoCompleteOnSessionCloseHasSession(ServiceDescription service)
        {
            ServiceBehaviorAttribute serviceBehavior = service.Behaviors.Find<ServiceBehaviorAttribute>();

            if (serviceBehavior != null)
            {
                InstanceContextMode instanceMode = serviceBehavior.InstanceContextMode;
                if (serviceBehavior.TransactionAutoCompleteOnSessionClose &&
                    instanceMode != InstanceContextMode.PerSession)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxTransactionAutoCompleteOnSessionCloseNoSession, service.Name)));
                }
            }

        }


        void ValidateServiceBehaviorAttributeWithNoScopeRequired(ServiceDescription service)
        {
            // If the service has no operations with TransactionScopeRequired=true, disallow any
            // transaction-related properties on the ServiceBehaviorAttribute
            if (!HasTransactedOperations(service))
            {
                ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
                if (attribute != null)
                {
                    if (attribute.TransactionTimeoutSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionTransactionTimeoutNeedsScope, service.Name)));
                    }

                    if (attribute.IsolationLevelSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionIsolationLevelNeedsScope, service.Name)));
                    }

                    if (attribute.ReleaseServiceInstanceOnTransactionCompleteSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionReleaseServiceInstanceOnTransactionCompleteNeedsScope, service.Name)));
                    }

                    if (attribute.TransactionAutoCompleteOnSessionCloseSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxTransactionTransactionAutoCompleteOnSessionCloseNeedsScope, service.Name)));
                    }
                }
            }
        }

        void EnsureNoOneWayTransactions(ServiceEndpoint endpoint)
        {
            CustomBinding binding = new CustomBinding(endpoint.Binding);
            TransactionFlowBindingElement txFlowBindingElement = binding.Elements.Find<TransactionFlowBindingElement>();
            if (txFlowBindingElement != null)
            {
                for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
                {
                    OperationDescription operation = endpoint.Contract.Operations[i];
                    if (operation.IsOneWay)
                    {
                        TransactionFlowAttribute tfbp = operation.Behaviors.Find<TransactionFlowAttribute>();
                        TransactionFlowOption transactions;
                        if (tfbp != null)
                        {
                            transactions = tfbp.Transactions;
                        }
                        else
                        {
                            transactions = TransactionFlowOption.NotAllowed;
                        }
                        if (TransactionFlowOptionHelper.AllowedOrRequired(transactions))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.SFxOneWayAndTransactionsIncompatible, endpoint.Contract.Name, operation.Name)));
                        }
                    }
                }
            }
        }

        bool HasTransactedOperations(ServiceDescription service)
        {
            for (int i = 0; i < service.Endpoints.Count; i++)
            {
                if (HasTransactedOperations(service.Endpoints[i]))
                {
                    return true;
                }
            }
            return false;
        }

        bool HasTransactedOperations(ServiceEndpoint endpoint)
        {
            for (int j = 0; j < endpoint.Contract.Operations.Count; j++)
            {
                OperationDescription operation = endpoint.Contract.Operations[j];
                OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();

                if (attribute != null && attribute.TransactionScopeRequired)
                {
                    return true;
                }
            }
            return false;
        }

        bool IsSingleThreaded(ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();

            if (attribute != null)
            {
                return (attribute.ConcurrencyMode == ConcurrencyMode.Single);
            }

            // The default is ConcurrencyMode.Single
            return true;
        }

        bool IsAutoComplete(OperationDescription operation)
        {
            OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();

            if (attribute != null)
            {
                return attribute.TransactionAutoComplete;
            }

            // The default is TransactionAutoComplete=true
            return true;
        }

        bool RequiresSessions(ServiceEndpoint endpoint)
        {
            return endpoint.Contract.SessionMode == SessionMode.Required;
        }

        void ValidateScopeRequiredAndAutoComplete(OperationDescription operation,
                                                  bool singleThreaded,
                                                  string contractName)
        {
            OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();

            if (attribute != null)
            {
                if (!singleThreaded && !attribute.TransactionAutoComplete)
                {
                    string id = SR.SFxTransactionNonConcurrentOrAutoComplete2;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(id, contractName, operation.Name)));
                }
            }
        }

        void ValidateNoMSMQandTransactionFlow(ServiceEndpoint endpoint)
        {
            BindingElementCollection bindingElements = endpoint.Binding.CreateBindingElements();

            if (bindingElements.Find<TransactionFlowBindingElement>() != null &&
                bindingElements.Find<MsmqTransportBindingElement>() != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxTransactionFlowAndMSMQ, endpoint.Address.Uri.AbsoluteUri)));
            }
        }

        void ValidateNotConcurrentWhenReleaseServiceInstanceOnTxComplete(ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();

            if (attribute != null && HasTransactedOperations(service))
            {
                if (attribute.ReleaseServiceInstanceOnTransactionComplete && !IsSingleThreaded(service))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(
                            SR.SFxTransactionNonConcurrentOrReleaseServiceInstanceOnTxComplete, service.Name)));
                }

            }
        }
    }
}
