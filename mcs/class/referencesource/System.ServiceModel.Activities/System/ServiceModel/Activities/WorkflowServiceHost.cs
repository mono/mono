//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Activities.Diagnostics;
    using System.Xml;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowServiceHost : ServiceHostBase
    {
        static readonly XName mexContractXName = XName.Get(ServiceMetadataBehavior.MexContractName, ServiceMetadataBehavior.MexContractNamespace);
        static readonly Type mexBehaviorType = typeof(ServiceMetadataBehavior);
        static readonly TimeSpan defaultPersistTimeout = TimeSpan.FromSeconds(30);
        static readonly TimeSpan defaultTrackTimeout = TimeSpan.FromSeconds(30);
        static readonly Type baseActivityType = typeof(Activity);
        static readonly Type correlationQueryBehaviorType = typeof(CorrelationQueryBehavior);
        static readonly Type bufferedReceiveServiceBehaviorType = typeof(BufferedReceiveServiceBehavior);

        WorkflowServiceHostExtensions workflowExtensions;
        DurableInstanceManager durableInstanceManager;

        WorkflowDefinitionProvider workflowDefinitionProvider;
    
        Activity activity;
        WorkflowService serviceDefinition;
        IDictionary<XName, ContractDescription> inferredContracts;
        IDictionary<XName, Collection<CorrelationQuery>> correlationQueries;

        WorkflowUnhandledExceptionAction unhandledExceptionAction;
        TimeSpan idleTimeToPersist;
        TimeSpan idleTimeToUnload;

        WorkflowServiceHostPerformanceCounters workflowServiceHostPerformanceCounters; 

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors, 
            Justification = "Based on prior are from WCF3: By design, don't want to complicate ServiceHost state model")]
        public WorkflowServiceHost(object serviceImplementation, params Uri[] baseAddresses)
            : base()
        {
            if (serviceImplementation == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceImplementation");
            }

            if (serviceImplementation is WorkflowService)
            {
                InitializeFromConstructor((WorkflowService)serviceImplementation, baseAddresses);
            }
            else
            {
                Activity activity = serviceImplementation as Activity;
                if (activity == null)
                {
                    throw FxTrace.Exception.Argument("serviceImplementation", SR.InvalidServiceImplementation);
                }
                InitializeFromConstructor(activity, baseAddresses);
            }
        }


        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors, 
            Justification = "Based on prior are from WCF3: By design, don't want to complicate ServiceHost state model")]
        public WorkflowServiceHost(Activity activity, params Uri[] baseAddresses)
            : base()
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            InitializeFromConstructor(activity, baseAddresses);
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors,
            Justification = "Based on prior art from WCF 3.0: By design, don't want to complicate ServiceHost state model")]
        public WorkflowServiceHost(WorkflowService serviceDefinition, params Uri[] baseAddresses)
            : base()
        {
            if (serviceDefinition == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDefinition");
            }

            InitializeFromConstructor(serviceDefinition, baseAddresses);
      
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors,
            Justification = "Based on prior art from WCF 3.0: By design, don't want to complicate ServiceHost state model")]
        protected WorkflowServiceHost()
        {
            InitializeFromConstructor((WorkflowService)null);
        }

        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        public WorkflowInstanceExtensionManager WorkflowExtensions
        {
            get
            {
                return this.workflowExtensions;
            }
        }

        public DurableInstancingOptions DurableInstancingOptions
        {
            get
            {
                return this.durableInstanceManager.DurableInstancingOptions;
            }
        }

        public ICollection<WorkflowService> SupportedVersions
        {
            get
            {
                return this.workflowDefinitionProvider.SupportedVersions;
            }
        }

        internal XName ServiceName
        {
            get;
            set;
        }

        internal TimeSpan PersistTimeout
        {
            get;
            set;
        }

        internal TimeSpan TrackTimeout
        {
            get;
            set;
        }

        // 
        internal TimeSpan FilterResumeTimeout
        {
            get;
            set;
        }

        internal DurableInstanceManager DurableInstanceManager
        {
            get
            {
                return this.durableInstanceManager;
            }
        }

        internal bool IsLoadTransactionRequired
        {
            get;
            private set;
        }

        // set by WorkflowUnhandledExceptionBehavior.ApplyDispatchBehavior, used by WorkflowServiceInstance.UnhandledExceptionPolicy
        internal WorkflowUnhandledExceptionAction UnhandledExceptionAction
        {
            get { return this.unhandledExceptionAction; }
            set 
            {
                Fx.Assert(WorkflowUnhandledExceptionActionHelper.IsDefined(value), "Undefined WorkflowUnhandledExceptionAction");
                this.unhandledExceptionAction = value; 
            }
        }

        // set by WorkflowIdleBehavior.ApplyDispatchBehavior, used by WorkflowServiceInstance.UnloadInstancePolicy
        internal TimeSpan IdleTimeToPersist
        {
            get { return this.idleTimeToPersist; }
            set 
            {
                Fx.Assert(value >= TimeSpan.Zero, "IdleTimeToPersist cannot be less than zero");
                this.idleTimeToPersist = value; 
            }
        }
        internal TimeSpan IdleTimeToUnload
        {
            get { return this.idleTimeToUnload; }
            set 
            {
                Fx.Assert(value >= TimeSpan.Zero, "IdleTimeToUnload cannot be less than zero");
                this.idleTimeToUnload = value; 
            }
        }

        internal bool IsConfigurable
        {
            get
            {
                lock (this.ThisLock)
                {
                    return this.State == CommunicationState.Created || this.State == CommunicationState.Opening;
                }
            }
        }

        internal WorkflowServiceHostPerformanceCounters WorkflowServiceHostPerformanceCounters
        {
            get
            {
                return this.workflowServiceHostPerformanceCounters;
            }
        }
        
        internal bool OverrideSiteName
        {
            get;
            set;
        }

        void InitializeFromConstructor(Activity activity, params Uri[] baseAddresses)
        {
            WorkflowService serviceDefinition = new WorkflowService 
            {
                Body = activity
            };

            InitializeFromConstructor(serviceDefinition, baseAddresses);
        }

        void InitializeFromConstructor(WorkflowService serviceDefinition, params Uri[] baseAddresses)
        {
            // first initialize some values to their defaults
            this.idleTimeToPersist = WorkflowIdleBehavior.defaultTimeToPersist;
            this.idleTimeToUnload = WorkflowIdleBehavior.defaultTimeToUnload;
            this.unhandledExceptionAction = WorkflowUnhandledExceptionBehavior.defaultAction;
            this.workflowExtensions = new WorkflowServiceHostExtensions();

            // If the AppSettings.DefaultAutomaticInstanceKeyDisassociation is specified and is true, create a DisassociateInstanceKeysExtension, set its
            // AutomaticDisassociationEnabled property to true, and add it to the extensions collection so that System.Activities.BookmarkScopeHandle will
            // unregister its BookmarkScope, which will cause key disassociation. KB2669774.
            if (AppSettings.DefaultAutomaticInstanceKeyDisassociation)
            {
                DisassociateInstanceKeysExtension extension = new DisassociateInstanceKeysExtension();
                extension.AutomaticDisassociationEnabled = true;
                this.workflowExtensions.Add(extension);
            }

            if (TD.CreateWorkflowServiceHostStartIsEnabled())
            {
                TD.CreateWorkflowServiceHostStart();
            }
            if (serviceDefinition != null)
            {
                this.workflowDefinitionProvider = new WorkflowDefinitionProvider(serviceDefinition, this);
                InitializeDescription(serviceDefinition, new UriSchemeKeyedCollection(baseAddresses));
            }
            this.durableInstanceManager = new DurableInstanceManager(this);            

            if (TD.CreateWorkflowServiceHostStopIsEnabled())
            {
                TD.CreateWorkflowServiceHostStop();
            }

            this.workflowServiceHostPerformanceCounters = new WorkflowServiceHostPerformanceCounters(this);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ServiceEndpoint AddServiceEndpoint(XName serviceContractName, Binding binding, string address,
            Uri listenUri = null, string behaviorConfigurationName = null)
        {
            return AddServiceEndpoint(serviceContractName, binding, new Uri(address, UriKind.RelativeOrAbsolute), listenUri, behaviorConfigurationName);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ServiceEndpoint AddServiceEndpoint(XName serviceContractName, Binding binding, Uri address,
            Uri listenUri = null, string behaviorConfigurationName = null)
        {
            if (binding == null)
            {
                throw FxTrace.Exception.ArgumentNull("binding");
            }
            if (address == null)
            {
                throw FxTrace.Exception.ArgumentNull("address");
            }

            Uri via = this.MakeAbsoluteUri(address, binding);
            return AddServiceEndpointCore(serviceContractName, binding, new EndpointAddress(via), listenUri, behaviorConfigurationName);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        ServiceEndpoint AddServiceEndpointCore(XName serviceContractName, Binding binding, EndpointAddress address,
            Uri listenUri = null, string behaviorConfigurationName = null)
        {
            if (serviceContractName == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceContractName");
            }
            if (this.inferredContracts == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(
                    SR.ContractNotFoundInAddServiceEndpoint(serviceContractName.LocalName, serviceContractName.NamespaceName)));
            }

            ServiceEndpoint serviceEndpoint;
            ContractDescription description;
            
            ContractInferenceHelper.ProvideDefaultNamespace(ref serviceContractName);

            if (this.inferredContracts.TryGetValue(serviceContractName, out description))
            {
                serviceEndpoint = new ServiceEndpoint(description, binding, address);

                if (!string.IsNullOrEmpty(behaviorConfigurationName))
                {
                    ConfigLoader.LoadChannelBehaviors(behaviorConfigurationName, null, serviceEndpoint.Behaviors);
                }
            }
            else if (serviceContractName == mexContractXName)  // Special case for mex endpoint
            {
                if (!this.Description.Behaviors.Contains(mexBehaviorType))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        SR.ServiceMetadataBehaviorNotFoundForServiceMetadataEndpoint(this.Description.Name)));
                }

                serviceEndpoint = new ServiceMetadataEndpoint(binding, address);
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(
                    SR.ContractNotFoundInAddServiceEndpoint(serviceContractName.LocalName, serviceContractName.NamespaceName)));
            }

            if (listenUri != null)
            {
                listenUri = base.MakeAbsoluteUri(listenUri, binding);
                serviceEndpoint.ListenUri = listenUri;
            }

            base.Description.Endpoints.Add(serviceEndpoint);

            if (TD.ServiceEndpointAddedIsEnabled())
            {
                TD.ServiceEndpointAdded(address.Uri.ToString(), binding.GetType().ToString(), serviceEndpoint.Contract.Name);
            }

            return serviceEndpoint;
        }

        // Duplicate public AddServiceEndpoint methods from the base class
        // This is to ensure that base class methods with string are not hidden by derived class methods with XName
        public new ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address);
        }

        public new ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address);
        }

        public new ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address, Uri listenUri)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address, listenUri);
        }

        public new ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address, listenUri);
        }

        public override void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            if (!endpoint.IsSystemEndpoint)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotUseAddServiceEndpointOverloadForWorkflowServices));
            }
            
            base.AddServiceEndpoint(endpoint);
        }        

        internal override void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
            if (this.inferredContracts != null)
            {
                foreach (XName contractName in this.inferredContracts.Keys)
                {
                    ServiceEndpoint endpoint = AddServiceEndpoint(contractName, defaultBinding, String.Empty);
                    ConfigLoader.LoadDefaultEndpointBehaviors(endpoint);
                    AddCorrelationQueryBehaviorToServiceEndpoint(endpoint);
                    defaultEndpoints.Add(endpoint);
                }
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, MessageId = "0#", Justification = "This is defined by the ServiceHost base class")]
        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            Fx.AssertAndThrow(this.serviceDefinition != null, "serviceDefinition is null");

            this.activity = this.serviceDefinition.Body;

            Dictionary<string, ContractDescription> result = new Dictionary<string, ContractDescription>();

            // Note: We do not check whether this.inferredContracts == null || this.inferredContracts.Count == 0,
            // because we need to support hosting workflow with zero contract.
            this.inferredContracts = this.serviceDefinition.GetContractDescriptions();

            if (this.inferredContracts != null)
            {
                foreach (ContractDescription contract in this.inferredContracts.Values)
                {
                    if (!string.IsNullOrEmpty(contract.ConfigurationName))
                    {
                        if (result.ContainsKey(contract.ConfigurationName))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DifferentContractsSameConfigName));
                        }
                        result.Add(contract.ConfigurationName, contract);
                    }
                }
            }

            implementedContracts = result;

            // Currently, only WorkflowService has CorrelationQueries property
            this.correlationQueries = this.serviceDefinition.CorrelationQueries;
            ServiceDescription serviceDescription = this.serviceDefinition.GetEmptyServiceDescription();
            serviceDescription.Behaviors.Add(new WorkflowServiceBehavior(this.workflowDefinitionProvider));
            return serviceDescription;
        }

        void InitializeDescription(WorkflowService serviceDefinition, UriSchemeKeyedCollection baseAddresses)
        {
            Fx.Assert(serviceDefinition != null, "caller must verify");

            this.serviceDefinition = serviceDefinition;
            base.InitializeDescription(baseAddresses);

            foreach (Endpoint endpoint in serviceDefinition.Endpoints)
            {
                if (endpoint.Binding == null)
                {
                    string endpointName = ContractValidationHelper.GetErrorMessageEndpointName(endpoint.Name);
                    string contractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(endpoint.ServiceContractName);
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MissingBindingInEndpoint(endpointName, contractName)));
                }

                ServiceEndpoint serviceEndpoint = AddServiceEndpointCore(endpoint.ServiceContractName, endpoint.Binding,
                        endpoint.GetAddress(this), endpoint.ListenUri, endpoint.BehaviorConfigurationName);

                if (!string.IsNullOrEmpty(endpoint.Name))
                {
                    serviceEndpoint.Name = endpoint.Name;
                }
                serviceEndpoint.UnresolvedAddress = endpoint.AddressUri;
                serviceEndpoint.UnresolvedListenUri = endpoint.ListenUri;
            }

            this.PersistTimeout = defaultPersistTimeout;
            this.TrackTimeout = defaultTrackTimeout;
            this.FilterResumeTimeout = TimeSpan.FromSeconds(AppSettings.FilterResumeTimeoutInSeconds);
        }

        protected override void InitializeRuntime()
        {
            if (base.Description != null)
            {
                FixupEndpoints();
                this.SetScopeName();
                if (this.DurableInstancingOptions.ScopeName == null)
                {
                    this.DurableInstancingOptions.ScopeName = XNamespace.Get(this.Description.Namespace).GetName(this.Description.Name);
                }
            }

            base.InitializeRuntime();

            this.WorkflowServiceHostPerformanceCounters.InitializePerformanceCounters();
            
            this.ServiceName = XNamespace.Get(this.Description.Namespace).GetName(this.Description.Name);

            // add a host-wide SendChannelCache (with default settings) if one doesn't exist
            this.workflowExtensions.EnsureChannelCache();

            // add a host-wide (free-threaded) CorrelationExtension based on our ServiceName
            this.WorkflowExtensions.Add(new CorrelationExtension(this.DurableInstancingOptions.ScopeName));

            this.WorkflowExtensions.MakeReadOnly();

            // now calculate if IsLoadTransactionRequired
            this.IsLoadTransactionRequired = WorkflowServiceInstance.IsLoadTransactionRequired(this);

            if (this.serviceDefinition != null)
            {
                ValidateBufferedReceiveProperty();
                this.serviceDefinition.ResetServiceDescription();
            }
        }

        internal override void AfterInitializeRuntime(TimeSpan timeout)
        {
            this.durableInstanceManager.Open(timeout);
        }

        internal override IAsyncResult BeginAfterInitializeRuntime(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.durableInstanceManager.BeginOpen(timeout, callback, state);
        }

        internal override void EndAfterInitializeRuntime(IAsyncResult result)
        {
            this.durableInstanceManager.EndOpen(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            base.OnClose(timeoutHelper.RemainingTime());

            this.durableInstanceManager.Close(timeoutHelper.RemainingTime());

            this.workflowServiceHostPerformanceCounters.Dispose();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnAbort()
        {
            base.OnAbort();

            this.durableInstanceManager.Abort();

            this.workflowServiceHostPerformanceCounters.Dispose();
        }

        internal void FaultServiceHostIfNecessary(Exception exception)
        {
            if (exception is InstancePersistenceException && !(exception is InstancePersistenceCommandException))
            {
                this.Fault(exception);
            }
        }

        IAsyncResult BeginHostClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.OnBeginClose(timeout, callback, state);
        }
        void EndHostClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        void AddCorrelationQueryBehaviorToServiceEndpoint(ServiceEndpoint serviceEndpoint)
        {
            Fx.Assert(serviceEndpoint != null, "Argument cannot be null!");
            Fx.Assert(serviceEndpoint.Contract != null, "ServiceEndpoint must have a contract!");
            Fx.Assert(this.serviceDefinition != null, "Missing WorkflowService!");
            Fx.Assert(!serviceEndpoint.Behaviors.Contains(correlationQueryBehaviorType),
                "ServiceEndpoint should not have CorrelationQueryBehavior before this point!");

            XName endpointContractName = XName.Get(serviceEndpoint.Contract.Name, serviceEndpoint.Contract.Namespace);

            Collection<CorrelationQuery> queries;
            if (this.correlationQueries != null && this.correlationQueries.TryGetValue(endpointContractName, out queries))
            {
                // Filter out duplicate CorrelationQueries in the collection.
                // Currently, we only do reference comparison and Where message filter comparison.
                Collection<CorrelationQuery> uniqueQueries = new Collection<CorrelationQuery>();
                foreach (CorrelationQuery correlationQuery in queries)
                {
                    if (!uniqueQueries.Contains(correlationQuery))
                    {
                        uniqueQueries.Add(correlationQuery);
                    }
                    else
                    {
                        if (TD.DuplicateCorrelationQueryIsEnabled())
                        {
                            TD.DuplicateCorrelationQuery(correlationQuery.Where.ToString());
                        }
                    }
                }
                serviceEndpoint.Behaviors.Add(new CorrelationQueryBehavior(uniqueQueries) { ServiceContractName = endpointContractName });
            }
            else if (CorrelationQueryBehavior.BindingHasDefaultQueries(serviceEndpoint.Binding))
            {
                if (!serviceEndpoint.Behaviors.Contains(typeof(CorrelationQueryBehavior)))
                {
                    serviceEndpoint.Behaviors.Add(new CorrelationQueryBehavior(new Collection<CorrelationQuery>()) { ServiceContractName = endpointContractName });
                }
            }
        }

        void FixupEndpoints()
        {
            Fx.Assert(this.Description != null, "ServiceDescription cannot be null");

            Dictionary<Type, ContractDescription> contractDescriptionDictionary = new Dictionary<Type, ContractDescription>();
            foreach (ServiceEndpoint serviceEndpoint in this.Description.Endpoints)
            {
                if (this.serviceDefinition.AllowBufferedReceive)
                {
                    // All application-level endpoints need to support ReceiveContext
                    SetupReceiveContextEnabledAttribute(serviceEndpoint);
                }

                // Need to add CorrelationQueryBehavior here so that endpoints added from config are included.
                // It is possible that some endpoints already have CorrelationQueryBehavior from
                // the AddDefaultEndpoints code path. We should skip them.
                if (!serviceEndpoint.Behaviors.Contains(correlationQueryBehaviorType))
                {
                    AddCorrelationQueryBehaviorToServiceEndpoint(serviceEndpoint);
                }

                // Need to ensure that any WorkflowHostingEndpoints using the same contract type actually use the
                // same contractDescription instance since this is required by WCF.
                if (serviceEndpoint is WorkflowHostingEndpoint)
                {
                    ContractDescription contract;
                    if (contractDescriptionDictionary.TryGetValue(serviceEndpoint.Contract.ContractType, out contract))
                    {
                        serviceEndpoint.Contract = contract;
                    }
                    else
                    {
                        contractDescriptionDictionary[serviceEndpoint.Contract.ContractType] = serviceEndpoint.Contract;
                    }
                }
            }

            if (this.serviceDefinition.AllowBufferedReceive && !this.Description.Behaviors.Contains(bufferedReceiveServiceBehaviorType))
            {
                this.Description.Behaviors.Add(new BufferedReceiveServiceBehavior());
            }
        }

        void SetScopeName()
        {
            VirtualPathExtension virtualPathExtension = this.Extensions.Find<VirtualPathExtension>();
            if (virtualPathExtension != null)
            {
                // Web Hosted scenario            
                WorkflowHostingOptionsSection hostingOptions = (WorkflowHostingOptionsSection)ConfigurationManager.GetSection(ConfigurationStrings.WorkflowHostingOptionsSectionPath);
                if (hostingOptions != null && hostingOptions.OverrideSiteName)
                {
                    this.OverrideSiteName = hostingOptions.OverrideSiteName;

                    string fullVirtualPath = virtualPathExtension.VirtualPath.Substring(1);
                    fullVirtualPath = ("/" == virtualPathExtension.ApplicationVirtualPath) ? fullVirtualPath : virtualPathExtension.ApplicationVirtualPath + fullVirtualPath;

                    int index = fullVirtualPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase); 
                    string virtualDirectoryPath = fullVirtualPath.Substring(0, index + 1);

                    this.DurableInstancingOptions.ScopeName = XName.Get(XmlConvert.EncodeLocalName(Path.GetFileName(virtualPathExtension.VirtualPath)),
                        string.Format(CultureInfo.InvariantCulture, "/{0}{1}", this.Description.Name, virtualDirectoryPath));
                }
            }
        }
        
        void SetupReceiveContextEnabledAttribute(ServiceEndpoint serviceEndpoint)
        {
            if (BufferedReceiveServiceBehavior.IsWorkflowEndpoint(serviceEndpoint))
            {
                foreach (OperationDescription operation in serviceEndpoint.Contract.Operations)
                {
                    ReceiveContextEnabledAttribute behavior = operation.Behaviors.Find<ReceiveContextEnabledAttribute>();
                    if (behavior == null)
                    {
                        operation.Behaviors.Add(new ReceiveContextEnabledAttribute() { ManualControl = true });
                    }
                    else
                    {
                        behavior.ManualControl = true;
                    }
                }
            }
        }

        void ValidateBufferedReceiveProperty()
        {
            // Validate that the AttachedProperty is indeed being used when the behavior is also used
            bool hasBehavior = this.Description.Behaviors.Contains(bufferedReceiveServiceBehaviorType);
            if (hasBehavior && !this.serviceDefinition.AllowBufferedReceive)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BufferedReceiveBehaviorUsedWithoutProperty));
            }
        }

        // specialized WorkflowInstanceExtensionManager that can default in a SendMessageChannelCache
        class WorkflowServiceHostExtensions : WorkflowInstanceExtensionManager
        {
            static Type SendReceiveExtensionType = typeof(SendReceiveExtension);

            bool hasChannelCache;

            public WorkflowServiceHostExtensions()
                : base()
            {
            }

            public override void Add<T>(Func<T> extensionCreationFunction)
            {
                ThrowIfNotSupported(typeof(T));

                if (TypeHelper.AreTypesCompatible(typeof(T), typeof(SendMessageChannelCache)))
                {
                    this.hasChannelCache = true;
                }
                base.Add<T>(extensionCreationFunction);
            }

            public override void Add(object singletonExtension)
            {
                ThrowIfNotSupported(singletonExtension.GetType());

                if (singletonExtension is SendMessageChannelCache)
                {
                    this.hasChannelCache = true;
                }
                base.Add(singletonExtension);
            }

            public void EnsureChannelCache()
            {
                if (!this.hasChannelCache)
                {
                    Add(new SendMessageChannelCache());
                    this.hasChannelCache = true;
                }
            }

            void ThrowIfNotSupported(Type type)
            {
                if (TypeHelper.AreTypesCompatible(type, SendReceiveExtensionType))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ExtensionTypeNotSupported(SendReceiveExtensionType.FullName)));
                }
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static AsyncCompletion handleDurableInstanceManagerEndClose = new AsyncCompletion(HandleDurableInstanceManagerEndClose);
            static AsyncCompletion handleEndHostClose = new AsyncCompletion(HandleEndHostClose);

            TimeoutHelper timeoutHelper;
            WorkflowServiceHost host;

            public CloseAsyncResult(WorkflowServiceHost host, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.host = host;

                if (CloseHost())
                {
                    Complete(true);
                }
            }

            bool CloseDurableInstanceManager()
            {
                IAsyncResult result = this.host.durableInstanceManager.BeginClose(
                    this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleDurableInstanceManagerEndClose), this);
                return SyncContinue(result);
            }

            bool CloseHost()
            {
                IAsyncResult result = this.host.BeginHostClose(
                    this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndHostClose), this);
                return SyncContinue(result);
            }

            static bool HandleDurableInstanceManagerEndClose(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;

                thisPtr.host.durableInstanceManager.EndClose(result);
                
                thisPtr.host.WorkflowServiceHostPerformanceCounters.Dispose();
                return true;
            }

            static bool HandleEndHostClose(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;

                thisPtr.host.EndHostClose(result);
                return thisPtr.CloseDurableInstanceManager();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }
}
