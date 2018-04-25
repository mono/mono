//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Xml;

    class WorkflowServiceBehavior : IServiceBehavior, IContextSessionProvider, IWmiInstanceProvider
    {
        static readonly object[] emptyObjectArray = new object[] { };
        AddressFilterMode addressFilterMode;
        string configurationName;
        bool ignoreExtensionDataObject;
        bool includeExceptionDetailInFaults;
        int maxItemsInObjectGraph = -1;
        string name;
        string nameSpace;
        bool useSynchronizationContext;
        bool validateMustUnderstand;
        WorkflowDefinitionContext workflowDefinitionContext;
        string workflowDefinitionPath;
        string workflowRulesPath;

        public WorkflowServiceBehavior(Type workflowType) :
            this(new CompiledWorkflowDefinitionContext(workflowType))
        {

        }

        public WorkflowServiceBehavior(string workflowDefinitionPath)
            :
            this(workflowDefinitionPath, null)
        {

        }

        public WorkflowServiceBehavior(string workflowDefinitionPath, string ruleDefinitionPath)
            :
            this(new StreamedWorkflowDefinitionContext(workflowDefinitionPath, ruleDefinitionPath, null))
        {
            this.workflowDefinitionPath = workflowDefinitionPath;
            this.workflowRulesPath = ruleDefinitionPath;
        }

        public WorkflowServiceBehavior(Stream workflowDefinitionStream)
            : this(new StreamedWorkflowDefinitionContext(workflowDefinitionStream, null, null))
        {

        }

        public WorkflowServiceBehavior(Stream workflowDefinitionStream, Stream ruleDefinitionStream)
            : this(new StreamedWorkflowDefinitionContext(workflowDefinitionStream, ruleDefinitionStream, null))
        {

        }

        internal WorkflowServiceBehavior(WorkflowDefinitionContext workflowDefinitionContext)
        {
            if (workflowDefinitionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinitionContext");
            }

            this.workflowDefinitionContext = workflowDefinitionContext;
            this.name = this.workflowDefinitionContext.WorkflowName;
            this.configurationName = this.workflowDefinitionContext.ConfigurationName;
        }

        public AddressFilterMode AddressFilterMode
        {
            get
            {
                return this.addressFilterMode;
            }
            set
            {
                if (!AddressFilterModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.addressFilterMode = value;
            }
        }

        public string ConfigurationName
        {
            get
            {
                return this.configurationName;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.configurationName = value;
            }
        }

        public bool IgnoreExtensionDataObject
        {
            get
            {
                return this.ignoreExtensionDataObject;
            }
            set
            {
                this.ignoreExtensionDataObject = value;
            }
        }

        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                return this.includeExceptionDetailInFaults;
            }
            set
            {
                this.includeExceptionDetailInFaults = value;
            }
        }

        public int MaxItemsInObjectGraph
        {
            get
            {
                return this.maxItemsInObjectGraph;
            }
            set
            {
                this.maxItemsInObjectGraph = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.nameSpace;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.nameSpace = value;
            }
        }

        public bool UseSynchronizationContext
        {
            get
            {
                return this.useSynchronizationContext;
            }
            set
            {
                this.useSynchronizationContext = value;
            }
        }

        public bool ValidateMustUnderstand
        {
            get
            {
                return this.validateMustUnderstand;
            }
            set
            {
                this.validateMustUnderstand = value;
            }
        }


        public void AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }

            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");
            }
            if (description.Behaviors == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("description", SR2.GetString(SR2.NoBehaviors));
            }
            if (description.Endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("description", SR2.GetString(SR2.NoEndpoints));
            }

            bool syncContextRegistered = false;
            WorkflowRuntimeBehavior workflowRuntimeBehavior = description.Behaviors.Find<WorkflowRuntimeBehavior>();

            if (workflowRuntimeBehavior == null)
            {
                workflowRuntimeBehavior = new WorkflowRuntimeBehavior();
                description.Behaviors.Add(workflowRuntimeBehavior);
            }

            WorkflowPersistenceService persistenceService = workflowRuntimeBehavior.WorkflowRuntime.GetService<WorkflowPersistenceService>();
            if (persistenceService != null)
            {
                bool wasRuntimeStarted = workflowRuntimeBehavior.WorkflowRuntime.IsStarted;
                if (wasRuntimeStarted)
                {
                    workflowRuntimeBehavior.WorkflowRuntime.StopRuntime();
                }
                workflowRuntimeBehavior.WorkflowRuntime.RemoveService(persistenceService);
                workflowRuntimeBehavior.WorkflowRuntime.AddService(new SkipUnloadOnFirstIdleWorkflowPersistenceService(persistenceService));
                if (wasRuntimeStarted)
                {
                    workflowRuntimeBehavior.WorkflowRuntime.StartRuntime();
                }
            }

            this.workflowDefinitionContext.Register(workflowRuntimeBehavior.WorkflowRuntime, workflowRuntimeBehavior.ValidateOnCreate);

            WorkflowInstanceContextProvider instanceContextProvider = new WorkflowInstanceContextProvider(
                serviceHostBase,
                false,
                this.workflowDefinitionContext
                );

            WorkflowInstanceContextProvider singleCallInstanceContextProvider = null;

            IInstanceProvider instanceProvider = new WorkflowInstanceProvider(instanceContextProvider);
            ServiceDebugBehavior serviceDebugBehavior = description.Behaviors.Find<ServiceDebugBehavior>();

            bool includeExceptionDetailsInFaults = this.IncludeExceptionDetailInFaults;
            if (serviceDebugBehavior != null)
            {
                includeExceptionDetailsInFaults |= serviceDebugBehavior.IncludeExceptionDetailInFaults;
            }

            IErrorHandler workflowOperationErrorHandler = new WorkflowOperationErrorHandler(includeExceptionDetailsInFaults);

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if (channelDispatcher != null && channelDispatcher.HasApplicationEndpoints())
                {
                    channelDispatcher.IncludeExceptionDetailInFaults = includeExceptionDetailsInFaults;
                    channelDispatcher.ErrorHandlers.Add(workflowOperationErrorHandler);
                    foreach (EndpointDispatcher endPointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (endPointDispatcher.IsSystemEndpoint)
                        {
                            continue;
                        }

                        ServiceEndpoint serviceEndPoint = description.Endpoints.Find(new XmlQualifiedName(endPointDispatcher.ContractName, endPointDispatcher.ContractNamespace));

                        if (serviceEndPoint != null)
                        {

                            DispatchRuntime dispatchRuntime = endPointDispatcher.DispatchRuntime;

                            dispatchRuntime.AutomaticInputSessionShutdown = true;
                            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Single;
                            dispatchRuntime.ValidateMustUnderstand = this.ValidateMustUnderstand;

                            if (!this.UseSynchronizationContext)
                            {
                                dispatchRuntime.SynchronizationContext = null;
                            }
                            else if (!syncContextRegistered)
                            {
                                SynchronizationContextWorkflowSchedulerService syncSchedulerService = workflowRuntimeBehavior.WorkflowRuntime.GetService<SynchronizationContextWorkflowSchedulerService>();
                                Fx.Assert(syncSchedulerService != null, "Wrong Synchronization Context Set");
                                syncSchedulerService.SetSynchronizationContext(dispatchRuntime.SynchronizationContext);
                                syncContextRegistered = true;
                            }

                            if (!endPointDispatcher.AddressFilterSetExplicit)
                            {
                                EndpointAddress endPointAddress = endPointDispatcher.OriginalAddress;
                                if ((endPointAddress == null) || (this.AddressFilterMode == AddressFilterMode.Any))
                                {
                                    endPointDispatcher.AddressFilter = new MatchAllMessageFilter();
                                }
                                else if (this.AddressFilterMode == AddressFilterMode.Prefix)
                                {
                                    endPointDispatcher.AddressFilter = new PrefixEndpointAddressMessageFilter(endPointAddress);
                                }
                                else if (this.AddressFilterMode == AddressFilterMode.Exact)
                                {
                                    endPointDispatcher.AddressFilter = new EndpointAddressMessageFilter(endPointAddress);
                                }
                            }

                            if (serviceEndPoint.Contract.SessionMode != SessionMode.NotAllowed)
                            {
                                endPointDispatcher.DispatchRuntime.InstanceContextProvider = instanceContextProvider;
                            }
                            else
                            {
                                if (singleCallInstanceContextProvider == null)
                                {
                                    singleCallInstanceContextProvider = new WorkflowInstanceContextProvider(
                                        serviceHostBase,
                                        true,
                                        this.workflowDefinitionContext);
                                }
                                endPointDispatcher.DispatchRuntime.InstanceContextProvider = singleCallInstanceContextProvider;
                            }
                            endPointDispatcher.DispatchRuntime.MessageInspectors.Add(new DurableMessageDispatchInspector(serviceEndPoint.Contract.SessionMode));
                            endPointDispatcher.DispatchRuntime.InstanceProvider = instanceProvider;
                            SetContractFilterToIncludeAllOperations(endPointDispatcher, serviceEndPoint.Contract);
                        }
                    }
                }
            }
            DataContractSerializerServiceBehavior.ApplySerializationSettings(description, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("AddressFilterMode", this.AddressFilterMode.ToString());
            wmiInstance.SetProperty("ConfigurationName", this.ConfigurationName);
            wmiInstance.SetProperty("IgnoreExtensionDataObject", this.IgnoreExtensionDataObject);
            wmiInstance.SetProperty("IncludeExceptionDetailInFaults", this.IncludeExceptionDetailInFaults);
            wmiInstance.SetProperty("MaxItemsInObjectGraph", this.MaxItemsInObjectGraph);
            wmiInstance.SetProperty("Name", this.Name);
            wmiInstance.SetProperty("Namespace", this.Namespace);
            wmiInstance.SetProperty("UseSynchronizationContext", this.UseSynchronizationContext);
            wmiInstance.SetProperty("ValidateMustUnderstand", this.ValidateMustUnderstand);
            wmiInstance.SetProperty("WorkflowType", this.workflowDefinitionContext.WorkflowName);
            wmiInstance.SetProperty("WorkflowDefinitionPath", this.workflowDefinitionPath);
            wmiInstance.SetProperty("WorkflowRulesPath", this.workflowRulesPath);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "WorkflowServiceBehavior";
        }

        public void Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            ContextBindingElement.ValidateContextBindingElementOnAllEndpointsWithSessionfulContract(description, this);
        }

        internal static void SetContractFilterToIncludeAllOperations(EndpointDispatcher dispatcher, ContractDescription contract)
        {
            if (dispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatcher");
            }
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }

            if (contract.SessionMode == SessionMode.Required)
            {
                EndpointFilterProvider provider = new EndpointFilterProvider();
                foreach (OperationDescription operation in contract.Operations)
                {
                    if (!operation.IsServerInitiated())
                    {
                        provider.InitiatingActions.Add(operation.Messages[0].Action);
                    }
                }
                int priority;
                dispatcher.ContractFilter = provider.CreateFilter(out priority);
                dispatcher.FilterPriority = priority;
            }
        }

        class SkipUnloadOnFirstIdleWorkflowPersistenceService : WorkflowPersistenceService
        {
            WorkflowPersistenceService inner;

            public SkipUnloadOnFirstIdleWorkflowPersistenceService(WorkflowPersistenceService inner)
            {
                if (inner == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inner");
                }

                this.inner = inner;
            }

            protected internal override Activity LoadCompletedContextActivity(Guid scopeId, Activity outerActivity)
            {
                return this.inner.LoadCompletedContextActivity(scopeId, outerActivity);
            }

            protected internal override Activity LoadWorkflowInstanceState(Guid instanceId)
            {
                return this.inner.LoadWorkflowInstanceState(instanceId);
            }

            protected internal override void SaveCompletedContextActivity(Activity activity)
            {
                this.inner.SaveCompletedContextActivity(activity);
            }

            protected internal override void SaveWorkflowInstanceState(Activity rootActivity, bool unlock)
            {
                this.inner.SaveWorkflowInstanceState(rootActivity, unlock);
            }

            protected internal override void Start()
            {
                this.inner.SetRuntime(this.Runtime);
                this.inner.Start();
            }

            protected internal override void Stop()
            {
                this.inner.Stop();
                this.inner.SetRuntime(null);
            }

            protected internal override bool UnloadOnIdle(Activity activity)
            {
                if (WorkflowDispatchContext.Current != null && WorkflowDispatchContext.Current.IsWorkflowStarting)
                {
                    return false;
                }
                else
                {
                    return this.inner.UnloadOnIdle(activity);
                }
            }

            protected internal override void UnlockWorkflowInstanceState(Activity rootActivity)
            {
                this.inner.UnlockWorkflowInstanceState(rootActivity);
            }
        }
    }
}
