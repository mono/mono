//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Persistence;
    using System.Xml;

    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public sealed class DurableServiceAttribute : Attribute, IServiceBehavior, IContextSessionProvider, IWmiInstanceProvider
    {
        static DurableOperationAttribute defaultDurableOperationBehavior = new DurableOperationAttribute();
        bool saveStateInOperationTransaction;
        UnknownExceptionAction unknownExceptionAction;

        public DurableServiceAttribute()
        {
            this.unknownExceptionAction = UnknownExceptionAction.TerminateInstance;
        }

        public bool SaveStateInOperationTransaction
        {
            get { return this.saveStateInOperationTransaction; }
            set { this.saveStateInOperationTransaction = value; }
        }

        public UnknownExceptionAction UnknownExceptionAction
        {
            get
            {
                return this.unknownExceptionAction;
            }
            set
            {
                if (!UnknownExceptionActionHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.unknownExceptionAction = value;
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // empty
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }

            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");
            }

            if (serviceDescription.Endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("serviceDescription", SR2.GetString(SR2.NoEndpoints));
            }

            PersistenceProviderBehavior providerBehavior = null;

            if (serviceDescription.Behaviors != null)
            {
                providerBehavior = serviceDescription.Behaviors.Find<PersistenceProviderBehavior>();
            }

            if (providerBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.NonNullPersistenceProviderRequired,
                    typeof(PersistenceProvider).Name,
                    typeof(DurableServiceAttribute).Name)));
            }

            if (providerBehavior.PersistenceProviderFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.NonNullPersistenceProviderRequired,
                    typeof(PersistenceProvider).Name,
                    typeof(DurableServiceAttribute).Name)));
            }

            providerBehavior.PersistenceProviderFactory.Open();
            serviceHostBase.Closed += new EventHandler(
                delegate(object sender, EventArgs args)
            {
                Fx.Assert(sender is ServiceHostBase, "The sender should be serviceHostBase.");
                // We have no way of knowing whether the service host closed or aborted
                // so we err on the side of abort for right now.
                providerBehavior.PersistenceProviderFactory.Abort();
            }
                );

            DurableInstanceContextProvider instanceContextProvider = new ServiceDurableInstanceContextProvider(
                serviceHostBase,
                false,
                serviceDescription.ServiceType,
                providerBehavior.PersistenceProviderFactory,
                this.saveStateInOperationTransaction,
                this.unknownExceptionAction,
                new DurableRuntimeValidator(this.saveStateInOperationTransaction, this.unknownExceptionAction),
                providerBehavior.PersistenceOperationTimeout);

            DurableInstanceContextProvider singleCallInstanceContextProvider = null;

            IInstanceProvider instanceProvider = new DurableInstanceProvider(instanceContextProvider);

            bool includeExceptionDetails = false;

            if (serviceDescription.Behaviors != null)
            {
                ServiceBehaviorAttribute serviceBehavior = serviceDescription.Behaviors.Find<ServiceBehaviorAttribute>();

                if (serviceBehavior != null)
                {
                    includeExceptionDetails |= serviceBehavior.IncludeExceptionDetailInFaults;
                }

                ServiceDebugBehavior serviceDebugBehavior = serviceDescription.Behaviors.Find<ServiceDebugBehavior>();

                if (serviceDebugBehavior != null)
                {
                    includeExceptionDetails |= serviceDebugBehavior.IncludeExceptionDetailInFaults;
                }
            }

            IErrorHandler errorHandler = new ServiceErrorHandler(includeExceptionDetails);

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if (channelDispatcher != null && channelDispatcher.HasApplicationEndpoints())
                {
                    if (this.unknownExceptionAction == UnknownExceptionAction.AbortInstance)
                    {
                        channelDispatcher.ErrorHandlers.Add(errorHandler);
                    }

                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (endpointDispatcher.IsSystemEndpoint)
                        {
                            continue;
                        }
                        ServiceEndpoint serviceEndPoint = serviceDescription.Endpoints.Find(new XmlQualifiedName(endpointDispatcher.ContractName, endpointDispatcher.ContractNamespace));

                        if (serviceEndPoint != null)
                        {
                            if (serviceEndPoint.Contract.SessionMode != SessionMode.NotAllowed)
                            {
                                endpointDispatcher.DispatchRuntime.InstanceContextProvider = instanceContextProvider;
                            }
                            else
                            {
                                if (singleCallInstanceContextProvider == null)
                                {
                                    singleCallInstanceContextProvider = new ServiceDurableInstanceContextProvider(
                                        serviceHostBase,
                                        true,
                                        serviceDescription.ServiceType,
                                        providerBehavior.PersistenceProviderFactory,
                                        this.saveStateInOperationTransaction,
                                        this.unknownExceptionAction,
                                        new DurableRuntimeValidator(this.saveStateInOperationTransaction, this.unknownExceptionAction),
                                        providerBehavior.PersistenceOperationTimeout);
                                }
                                endpointDispatcher.DispatchRuntime.InstanceContextProvider = singleCallInstanceContextProvider;
                            }
                            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new DurableMessageDispatchInspector(serviceEndPoint.Contract.SessionMode));
                            endpointDispatcher.DispatchRuntime.InstanceProvider = instanceProvider;
                            WorkflowServiceBehavior.SetContractFilterToIncludeAllOperations(endpointDispatcher, serviceEndPoint.Contract);
                        }
                    }
                }
            }

            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (!endpoint.InternalIsSystemEndpoint(serviceDescription))
                {
                    foreach (OperationDescription opDescription in endpoint.Contract.Operations)
                    {
                        if (!opDescription.Behaviors.Contains(typeof(DurableOperationAttribute)))
                        {
                            opDescription.Behaviors.Add(DurableOperationAttribute.DefaultInstance);
                        }
                    }
                }
            }
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("SaveStateInOperationTransaction", this.saveStateInOperationTransaction);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "DurableServiceAttribute";
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }

            ContextBindingElement.ValidateContextBindingElementOnAllEndpointsWithSessionfulContract(serviceDescription, this);

            if (serviceDescription.Behaviors != null)
            {
                ServiceBehaviorAttribute serviceBehavior = serviceDescription.Behaviors.Find<ServiceBehaviorAttribute>();

                if (serviceBehavior != null)
                {
                    if (serviceBehavior.InstanceContextMode != InstanceContextMode.PerSession)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR2.GetString(SR2.InstanceContextModeMustBePerSession, serviceBehavior.InstanceContextMode)));
                    }

                    if (serviceBehavior.ConcurrencyMode == ConcurrencyMode.Multiple)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR2.GetString(SR2.ConcurrencyMultipleNotSupported)));
                    }

                    if (serviceBehavior.ConcurrencyMode == ConcurrencyMode.Reentrant
                        && this.UnknownExceptionAction == UnknownExceptionAction.AbortInstance)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR2.GetString(SR2.ConcurrencyReentrantAndAbortNotSupported)));
                    }
                }
            }

            bool foundSessionfulContract = false;

            foreach (ServiceEndpoint serviceEndpoint in serviceDescription.Endpoints)
            {
                if (serviceEndpoint != null && !serviceEndpoint.InternalIsSystemEndpoint(serviceDescription))
                {
                    if (serviceEndpoint.Contract.SessionMode != SessionMode.NotAllowed)
                    {
                        foundSessionfulContract = true;
                    }

                    foreach (OperationDescription operation in serviceEndpoint.Contract.Operations)
                    {
                        DurableOperationAttribute durableBehavior =
                            operation.Behaviors.Find<DurableOperationAttribute>();

                        if (durableBehavior == null)
                        {
                            durableBehavior = defaultDurableOperationBehavior;
                        }

                        if (serviceEndpoint.Contract.SessionMode == SessionMode.NotAllowed)
                        {
                            if (!durableBehavior.CanCreateInstanceForOperation(operation.IsOneWay))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new InvalidOperationException(
                                    SR2.GetString(
                                    SR2.CanCreateInstanceMustBeTrue,
                                    serviceEndpoint.Contract.Name,
                                    operation.Name)));
                            }
                        }
                        else
                        {
                            if (operation.IsOneWay &&
                                durableBehavior.CanCreateInstanceForOperation(operation.IsOneWay))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new InvalidOperationException(
                                    SR2.GetString(
                                    SR2.CanCreateInstanceMustBeTwoWay,
                                    serviceEndpoint.Contract.Name,
                                    serviceEndpoint.Contract.SessionMode,
                                    operation.Name)));
                            }
                        }

                        if (this.saveStateInOperationTransaction)
                        {
                            bool hasTransaction = false;

                            OperationBehaviorAttribute operationBehavior = operation.Behaviors.Find<OperationBehaviorAttribute>();

                            if (operationBehavior != null)
                            {
                                if (operationBehavior.TransactionScopeRequired)
                                {
                                    hasTransaction = true;
                                }
                            }

                            TransactionFlowAttribute transactionBehavior = operation.Behaviors.Find<TransactionFlowAttribute>();

                            if (transactionBehavior != null)
                            {
                                if (transactionBehavior.Transactions == TransactionFlowOption.Mandatory)
                                {
                                    hasTransaction = true;
                                }
                            }

                            if (!hasTransaction)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new InvalidOperationException(
                                    SR2.GetString(
                                    SR2.SaveStateInTransactionValidationFailed,
                                    operation.Name,
                                    serviceEndpoint.ListenUri)));
                            }
                        }
                    }
                }
            }

            if (!foundSessionfulContract)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(SR2.SessionfulContractNotFound)));
            }
        }
    }
}
