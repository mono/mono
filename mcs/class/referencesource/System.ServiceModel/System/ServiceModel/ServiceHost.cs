//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Runtime.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics.Application;
    using System.Reflection;
    using System.Linq.Expressions;

    public abstract class ServiceHostBase : CommunicationObject, IExtensibleObject<ServiceHostBase>, IDisposable
    {
        internal static readonly Uri EmptyUri = new Uri(string.Empty, UriKind.RelativeOrAbsolute);

        bool initializeDescriptionHasFinished;
        UriSchemeKeyedCollection baseAddresses;
        ChannelDispatcherCollection channelDispatchers;
        TimeSpan closeTimeout = ServiceDefaults.ServiceHostCloseTimeout;
        ServiceDescription description;
        ExtensionCollection<ServiceHostBase> extensions;
        ReadOnlyCollection<Uri> externalBaseAddresses;
        IDictionary<string, ContractDescription> implementedContracts;
        IInstanceContextManager instances;
        TimeSpan openTimeout = ServiceDefaults.OpenTimeout;
        ServicePerformanceCountersBase servicePerformanceCounters;
        DefaultPerformanceCounters defaultPerformanceCounters;
        ServiceThrottle serviceThrottle;
        ServiceCredentials readOnlyCredentials;
        ServiceAuthorizationBehavior readOnlyAuthorization;
        ServiceAuthenticationBehavior readOnlyAuthentication;
        Dictionary<DispatcherBuilder.ListenUriInfo, Collection<ServiceEndpoint>> endpointsByListenUriInfo;
        int busyCount;
        EventTraceActivity eventTraceActivity;

        internal event EventHandler BusyCountIncremented;

        public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

        protected ServiceHostBase()
        {
            TraceUtility.SetEtwProviderId();
            this.baseAddresses = new UriSchemeKeyedCollection(this.ThisLock);
            this.channelDispatchers = new ChannelDispatcherCollection(this, this.ThisLock);
            this.extensions = new ExtensionCollection<ServiceHostBase>(this, this.ThisLock);
            this.instances = new InstanceContextManager(this.ThisLock);
            this.serviceThrottle = new ServiceThrottle(this);
            this.TraceOpenAndClose = true;
            this.Faulted += new EventHandler(OnServiceHostFaulted);
        }


        internal EventTraceActivity EventTraceActivity
        {
            get 
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = new EventTraceActivity();
                }

                return eventTraceActivity; 
            }            
        }

        public ServiceAuthorizationBehavior Authorization
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                else if (this.State == CommunicationState.Created || this.State == CommunicationState.Opening)
                {
                    return EnsureAuthorization(this.Description);
                }
                else
                {
                    return this.readOnlyAuthorization;
                }
            }
        }

        public ServiceAuthenticationBehavior Authentication
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                else if (this.State == CommunicationState.Created || this.State == CommunicationState.Opening)
                {
                    return EnsureAuthentication(this.Description);
                }
                else
                {
                    return this.readOnlyAuthentication;
                }
            }
        }

        public ReadOnlyCollection<Uri> BaseAddresses
        {
            get
            {
                externalBaseAddresses = new ReadOnlyCollection<Uri>(new List<Uri>(this.baseAddresses));
                return externalBaseAddresses;
            }
        }

        public ChannelDispatcherCollection ChannelDispatchers
        {
            get { return this.channelDispatchers; }
        }

        public TimeSpan CloseTimeout
        {
            get { return this.closeTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = SR.GetString(SR.SFxTimeoutOutOfRange0);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                lock (this.ThisLock)
                {
                    this.ThrowIfClosedOrOpened();
                    this.closeTimeout = value;
                }
            }
        }

        internal ServicePerformanceCountersBase Counters
        {
            get
            {
                return this.servicePerformanceCounters;
            }
            set
            {
                this.servicePerformanceCounters = value;
                this.serviceThrottle.SetServicePerformanceCounters(this.servicePerformanceCounters);
            }
        }

        internal DefaultPerformanceCounters DefaultCounters
        {
            get
            {
                return this.defaultPerformanceCounters;
            }
            set
            {
                this.defaultPerformanceCounters = value;
            }
        }

        public ServiceCredentials Credentials
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                else if (this.State == CommunicationState.Created || this.State == CommunicationState.Opening)
                {
                    return EnsureCredentials(this.Description);
                }
                else
                {
                    return this.readOnlyCredentials;
                }
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.OpenTimeout; }
        }

        public ServiceDescription Description
        {
            get { return this.description; }
        }

        public IExtensionCollection<ServiceHostBase> Extensions
        {
            get { return this.extensions; }
        }

        protected internal IDictionary<string, ContractDescription> ImplementedContracts
        {
            get { return this.implementedContracts; }
        }

        internal UriSchemeKeyedCollection InternalBaseAddresses
        {
            get { return this.baseAddresses; }
        }

        public int ManualFlowControlLimit
        {
            get { return this.ServiceThrottle.ManualFlowControlLimit; }
            set { this.ServiceThrottle.ManualFlowControlLimit = value; }
        }

        public TimeSpan OpenTimeout
        {
            get { return this.openTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = SR.GetString(SR.SFxTimeoutOutOfRange0);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                lock (this.ThisLock)
                {
                    this.ThrowIfClosedOrOpened();
                    this.openTimeout = value;
                }
            }
        }

        internal ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
        }

        internal virtual object DisposableInstance
        {
            get
            {
                return null;
            }
        }

        internal Dictionary<DispatcherBuilder.ListenUriInfo, Collection<ServiceEndpoint>> EndpointsByListenUriInfo
        {
            get
            {
                if (this.endpointsByListenUriInfo == null)
                {
                    this.endpointsByListenUriInfo = this.GetEndpointsByListenUriInfo();
                }
                return this.endpointsByListenUriInfo;
            }
        }

        Dictionary<DispatcherBuilder.ListenUriInfo, Collection<ServiceEndpoint>> GetEndpointsByListenUriInfo()
        {
            Dictionary<DispatcherBuilder.ListenUriInfo, Collection<ServiceEndpoint>> endpointDictionary = new Dictionary<DispatcherBuilder.ListenUriInfo, Collection<ServiceEndpoint>>();
            foreach (ServiceEndpoint endpoint in this.Description.Endpoints)
            {
                DispatcherBuilder.ListenUriInfo listenUriInfo = DispatcherBuilder.GetListenUriInfoForEndpoint(this, endpoint);
                if (!endpointDictionary.ContainsKey(listenUriInfo))
                {
                    endpointDictionary.Add(listenUriInfo, new Collection<ServiceEndpoint>());
                }
                endpointDictionary[listenUriInfo].Add(endpoint);
            }
            return endpointDictionary;
        }

        protected void AddBaseAddress(Uri baseAddress)
        {
            if (this.initializeDescriptionHasFinished)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxCannotCallAddBaseAddress)));
            }
            this.baseAddresses.Add(baseAddress);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, (Uri)null);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }

            ServiceEndpoint endpoint = this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute));
            if (listenUri != null)
            {
                endpoint.UnresolvedListenUri = listenUri;
                listenUri = MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, (Uri)null);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }

            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("binding"));
            }

            if (implementedContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("implementedContract"));
            }

            if (this.State != CommunicationState.Created && this.State != CommunicationState.Opening)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotAddEndpointAfterOpen)));
            }

            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotAddEndpointWithoutDescription)));
            }

            Uri via = this.MakeAbsoluteUri(address, binding);

            ConfigLoader configLoader = new ConfigLoader(GetContractResolver(this.implementedContracts));
            ContractDescription contract = configLoader.LookupContract(implementedContract, this.Description.Name);

            ServiceEndpoint serviceEndpoint = new ServiceEndpoint(contract, binding, new EndpointAddress(via));
            this.Description.Endpoints.Add(serviceEndpoint);
            serviceEndpoint.UnresolvedAddress = address;

            if (listenUri != null)
            {
                serviceEndpoint.UnresolvedListenUri = listenUri;
                listenUri = MakeAbsoluteUri(listenUri, binding);
                serviceEndpoint.ListenUri = listenUri;
            }
            return serviceEndpoint;
        }

        public virtual void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (this.State != CommunicationState.Created && this.State != CommunicationState.Opening)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotAddEndpointAfterOpen)));
            }
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotAddEndpointWithoutDescription)));
            }
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxEndpointAddressNotSpecified));
            }
            if (endpoint.Contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxEndpointContractNotSpecified));
            }
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxEndpointBindingNotSpecified));
            }
            if (!endpoint.IsSystemEndpoint || endpoint.Contract.ContractType == typeof(IMetadataExchange))
            {
                ConfigLoader loader = new ConfigLoader(GetContractResolver(this.implementedContracts));
                loader.LookupContract(endpoint.Contract.ConfigurationName, this.Description.Name);
            }
            this.Description.Endpoints.Add(endpoint);
        }

        public void SetEndpointAddress(ServiceEndpoint endpoint, string relativeAddress)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (relativeAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("relativeAddress");
            }
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxEndpointBindingNotSpecified));
            }
            Uri absoluteUri = MakeAbsoluteUri(new Uri(relativeAddress, UriKind.Relative), endpoint.Binding);
            endpoint.Address = new EndpointAddress(absoluteUri);
        }

        internal Uri MakeAbsoluteUri(Uri relativeOrAbsoluteUri, Binding binding)
        {
            return MakeAbsoluteUri(relativeOrAbsoluteUri, binding, this.InternalBaseAddresses);
        }

        internal static Uri MakeAbsoluteUri(Uri relativeOrAbsoluteUri, Binding binding, UriSchemeKeyedCollection baseAddresses)
        {
            Uri result = relativeOrAbsoluteUri;
            if (!result.IsAbsoluteUri)
            {
                if (binding.Scheme == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCustomBindingWithoutTransport)));
                }
                result = GetVia(binding.Scheme, result, baseAddresses);
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxEndpointNoMatchingScheme, binding.Scheme, binding.Name, GetBaseAddressSchemes(baseAddresses))));
                }
            }
            return result;
        }

        protected virtual void ApplyConfiguration()
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotApplyConfigurationWithoutDescription)));
            }

            ConfigLoader configLoader = new ConfigLoader(GetContractResolver(implementedContracts));

            // Call the overload of LoadConfigurationSectionInternal which looks up the serviceElement from ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            LoadConfigurationSectionInternal(configLoader, this.Description, this.Description.ConfigurationName);

            EnsureAuthenticationAuthorizationDebug(this.Description);
        }

        internal void EnsureAuthenticationAuthorizationDebug(ServiceDescription description)
        {
            EnsureAuthentication(description);
            EnsureAuthorization(description);
            EnsureDebug(description);
        }

        public virtual ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            List<ServiceEndpoint> defaultEndpoints = new List<ServiceEndpoint>();
            foreach (Uri baseAddress in this.InternalBaseAddresses)
            {
                ProtocolMappingItem protocolMappingItem = ConfigLoader.LookupProtocolMapping(baseAddress.Scheme);
                if (protocolMappingItem != null)
                {
                    Binding defaultBinding = ConfigLoader.LookupBinding(protocolMappingItem.Binding, protocolMappingItem.BindingConfiguration);
                    if (defaultBinding != null)
                    {
                        AddDefaultEndpoints(defaultBinding, defaultEndpoints);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Exception(SR.GetString(SR.BindingProtocolMappingNotDefined, baseAddress.Scheme)));
                    }
                }
            }
            if (DiagnosticUtility.ShouldTraceInformation && defaultEndpoints.Count > 0)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["ServiceConfigurationName"] = this.description.ConfigurationName;
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.DefaultEndpointsAdded, SR.GetString(SR.TraceCodeDefaultEndpointsAdded), new DictionaryTraceRecord(dictionary));
            }
            return new ReadOnlyCollection<ServiceEndpoint>(defaultEndpoints);
        }

        internal virtual void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
        }

        internal virtual void BindInstance(InstanceContext instance)
        {
            this.instances.Add(instance);
            if (null != this.servicePerformanceCounters)
            {
                lock (this.ThisLock)
                {
                    if (null != this.servicePerformanceCounters)
                    {
                        this.servicePerformanceCounters.ServiceInstanceCreated();
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        protected abstract ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts);

        protected virtual void InitializeRuntime()
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotInitializeRuntimeWithoutDescription)));
            }

            if (this.Description.Endpoints.Count == 0)
            {
                this.AddDefaultEndpoints();
            }

            this.EnsureAuthenticationSchemes();

            DispatcherBuilder dispatcherBuilder = new DispatcherBuilder();
            dispatcherBuilder.InitializeServiceHost(description, this);

            SecurityValidationBehavior.Instance.AfterBuildTimeValidation(description);
        }

        internal virtual void AfterInitializeRuntime(TimeSpan timeout)
        {
        }

        internal virtual IAsyncResult BeginAfterInitializeRuntime(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        internal virtual void EndAfterInitializeRuntime(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        ServiceAuthorizationBehavior EnsureAuthorization(ServiceDescription description)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            ServiceAuthorizationBehavior a = description.Behaviors.Find<ServiceAuthorizationBehavior>();

            if (a == null)
            {
                a = new ServiceAuthorizationBehavior();
                description.Behaviors.Add(a);
            }

            return a;
        }

        ServiceAuthenticationBehavior EnsureAuthentication(ServiceDescription description)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            ServiceAuthenticationBehavior a = description.Behaviors.Find<ServiceAuthenticationBehavior>();

            if (a == null)
            {
                a = new ServiceAuthenticationBehavior();
                description.Behaviors.Add(a);
            }
            return a;
        }

        ServiceDebugBehavior EnsureDebug(ServiceDescription description)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            ServiceDebugBehavior m = description.Behaviors.Find<ServiceDebugBehavior>();

            if (m == null)
            {
                m = new ServiceDebugBehavior();
                description.Behaviors.Add(m);
            }

            return m;
        }

        ServiceCredentials EnsureCredentials(ServiceDescription description)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            ServiceCredentials c = description.Behaviors.Find<ServiceCredentials>();

            if (c == null)
            {
                c = new ServiceCredentials();
                description.Behaviors.Add(c);
            }

            return c;
        }

        internal void FaultInternal()
        {
            this.Fault();
        }

        internal string GetBaseAddressSchemes()
        {
            return GetBaseAddressSchemes(baseAddresses);
        }

        internal static String GetBaseAddressSchemes(UriSchemeKeyedCollection uriSchemeKeyedCollection)
        {
            StringBuilder buffer = new StringBuilder();
            bool firstScheme = true;
            foreach (Uri address in uriSchemeKeyedCollection)
            {
                if (firstScheme)
                {
                    buffer.Append(address.Scheme);
                    firstScheme = false;
                }
                else
                {
                    buffer.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator).Append(address.Scheme);
                }
            }
            return buffer.ToString();
        }

        internal BindingParameterCollection GetBindingParameters()
        {
            return DispatcherBuilder.GetBindingParameters(this, new Collection<ServiceEndpoint>());
        }

        internal BindingParameterCollection GetBindingParameters(ServiceEndpoint inputEndpoint)
        {
            Collection<ServiceEndpoint> endpoints;
            if (inputEndpoint == null)
            {
                endpoints = new Collection<ServiceEndpoint>();
            }
            else if (!this.EndpointsByListenUriInfo.TryGetValue(DispatcherBuilder.GetListenUriInfoForEndpoint(this, inputEndpoint), out endpoints) || !endpoints.Contains(inputEndpoint))
            {
                endpoints = new Collection<ServiceEndpoint>();
                endpoints.Add(inputEndpoint);
            }

            return DispatcherBuilder.GetBindingParameters(this, endpoints);
        }

        internal BindingParameterCollection GetBindingParameters(Collection<ServiceEndpoint> endpoints)
        {
            return DispatcherBuilder.GetBindingParameters(this, endpoints);
        }

        internal ReadOnlyCollection<InstanceContext> GetInstanceContexts()
        {
            return Array.AsReadOnly<InstanceContext>(this.instances.ToArray());
        }

        internal virtual IContractResolver GetContractResolver(IDictionary<string, ContractDescription> implementedContracts)
        {
            ServiceAndBehaviorsContractResolver resolver = new ServiceAndBehaviorsContractResolver(new ImplementedContractsContractResolver(implementedContracts));
            resolver.AddBehaviorContractsToResolver(this.description == null ? null : this.description.Behaviors);
            return resolver;
        }

        internal static Uri GetUri(Uri baseUri, Uri relativeUri)
        {
            return GetUri(baseUri, relativeUri.OriginalString);
        }

        internal static Uri GetUri(Uri baseUri, string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("\\", StringComparison.Ordinal))
            {
                int i = 1;
                for (; i < path.Length; ++i)
                {
                    if (path[i] != '/' && path[i] != '\\')
                    {
                        break;
                    }
                }
                path = path.Substring(i);
            }

            // VSWhidbey#541152: new Uri(Uri, string.Empty) is broken
            if (path.Length == 0)
                return baseUri;

            if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
            {
                baseUri = new Uri(baseUri.AbsoluteUri + "/");
            }
            return new Uri(baseUri, path);
        }

        internal Uri GetVia(string scheme, Uri address)
        {
            return ServiceHost.GetVia(scheme, address, InternalBaseAddresses);
        }

        internal static Uri GetVia(string scheme, Uri address, UriSchemeKeyedCollection baseAddresses)
        {
            Uri via = address;
            if (!via.IsAbsoluteUri)
            {
                if (!baseAddresses.Contains(scheme))
                {
                    return null;
                }

                via = GetUri(baseAddresses[scheme], address);
            }
            return via;
        }

        public int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.ServiceThrottle.IncrementManualFlowControlLimit(incrementBy);
        }

        protected void InitializeDescription(UriSchemeKeyedCollection baseAddresses)
        {
            foreach (Uri baseAddress in baseAddresses)
            {
                this.baseAddresses.Add(baseAddress);
            }
            IDictionary<string, ContractDescription> implementedContracts = null;
            ServiceDescription description = CreateDescription(out implementedContracts);
            this.description = description;
            this.implementedContracts = implementedContracts;

            ApplyConfiguration();
            this.initializeDescriptionHasFinished = true;
        }

        protected void LoadConfigurationSection(ServiceElement serviceSection)
        {
            if (serviceSection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceSection");
            }
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotLoadConfigurationSectionWithoutDescription)));
            }

            ConfigLoader configLoader = new ConfigLoader(GetContractResolver(this.ImplementedContracts));
            LoadConfigurationSectionInternal(configLoader, this.Description, serviceSection);
        }

        internal void LoadConfigurationSectionHelper(Uri baseAddress)
        {
            this.AddBaseAddress(baseAddress);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls LookupService which is critical.",
            Safe = "Doesn't leak ServiceElement out of SecurityCritical code.")]
        [SecuritySafeCritical]
        void LoadConfigurationSectionInternal(ConfigLoader configLoader, ServiceDescription description, string configurationName)
        {
            ServiceElement serviceSection = configLoader.LookupService(configurationName);
            LoadConfigurationSectionInternal(configLoader, description, serviceSection);
        }

        [Fx.Tag.SecurityNote(Critical = "Handles a ServiceElement, which should not be leaked out of SecurityCritical code.",
            Safe = "Doesn't leak ServiceElement out of SecurityCritical code.")]
        [SecuritySafeCritical]
        void LoadConfigurationSectionInternal(ConfigLoader configLoader, ServiceDescription description, ServiceElement serviceSection)
        {
            // caller must validate arguments before calling
            configLoader.LoadServiceDescription(this, description, serviceSection, this.LoadConfigurationSectionHelper);
        }

        protected override void OnAbort()
        {
            this.instances.Abort();

            foreach (ChannelDispatcherBase dispatcher in this.ChannelDispatchers)
            {
                if (dispatcher.Listener != null)
                {
                    dispatcher.Listener.Abort();
                }
                dispatcher.Abort();
            }
            ThreadTrace.StopTracing();
        }

        internal void OnAddChannelDispatcher(ChannelDispatcherBase channelDispatcher)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfClosedOrOpened();
                channelDispatcher.AttachInternal(this);
                channelDispatcher.Faulted += new EventHandler(OnChannelDispatcherFaulted);
            }
        }
       
        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(timeout, callback, state, this);
        }

        void OnBeginOpen()
        {
            this.TraceServiceHostOpenStart();
            this.TraceBaseAddresses();
            MessageLogger.EnsureInitialized(); //force config validation instead of waiting for the first message exchange
            InitializeRuntime();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnBeginOpen();
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        IAsyncResult BeginOpenChannelDispatchers(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCollectionAsyncResult(timeout, callback, state, this.SnapshotChannelDispatchers());
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                if (ManagementExtension.IsEnabled && null != this.Description)
                {
                    ManagementExtension.OnServiceClosing(this);
                }

                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcherBase dispatcher = this.ChannelDispatchers[i];
                    if (dispatcher.Listener != null)
                    {
                        dispatcher.Listener.Close(timeoutHelper.RemainingTime());
                    }
                }

                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcherBase dispatcher = this.ChannelDispatchers[i];
                    dispatcher.CloseInput(timeoutHelper.RemainingTime());
                }

                // Wait for existing work to complete
                this.instances.CloseInput(timeoutHelper.RemainingTime());

                // Close instances (closes contexts/channels)
                this.instances.Close(timeoutHelper.RemainingTime());

                // Close dispatchers
                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcherBase dispatcher = this.ChannelDispatchers[i];
                    dispatcher.Close(timeoutHelper.RemainingTime());
                }

                this.ReleasePerformanceCounters();

                this.TraceBaseAddresses();
                ThreadTrace.StopTracing();
            }
            catch (TimeoutException e)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(SR.GetString(SR.TraceCodeServiceHostTimeoutOnClose));
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.ServiceHostTimeoutOnClose, SR.GetString(SR.TraceCodeServiceHostTimeoutOnClose), this, e);
                }
                this.Abort();
            }

        }

        protected override void OnClosed()
        {
            try
            {
                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher dispatcher = this.ChannelDispatchers[i] as ChannelDispatcher;
                    if (dispatcher != null)
                    {
                        dispatcher.ReleasePerformanceCounters();
                    }
                }
            }
            finally
            {
                base.OnClosed();
            }
        }

        void TraceBaseAddresses()
        {
            if (DiagnosticUtility.ShouldTraceInformation && this.baseAddresses != null
                && this.baseAddresses.Count > 0)
            {
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.ServiceHostBaseAddresses,
                    SR.GetString(SR.TraceCodeServiceHostBaseAddresses),
                    new CollectionTraceRecord("BaseAddresses", "Address", this.baseAddresses),
                    this, null);
            }
        }

        void TraceServiceHostOpenStart()
        {
            if (TD.ServiceHostOpenStartIsEnabled())
            {
                TD.ServiceHostOpenStart(this.EventTraceActivity);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            try
            {
                CloseAsyncResult.End(result);
                this.TraceBaseAddresses();
                ThreadTrace.StopTracing();
            }
            catch (TimeoutException e)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(SR.GetString(SR.TraceCodeServiceHostTimeoutOnClose));
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.ServiceHostTimeoutOnClose,
                        SR.GetString(SR.TraceCodeServiceHostTimeoutOnClose), this, e);
                }
                this.Abort();
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        void EndOpenChannelDispatchers(IAsyncResult result)
        {
            OpenCollectionAsyncResult.End(result);
        }

        void EnsureAuthenticationSchemes()
        {
            if (this.Authentication == null)
            {
                return;
            }

            //Exit immediately when not hosted in IIS or if VirtualPathExtension is not set. VirtualPathExtension is used as a flag to indicate whether a ServiceHost
            // is webhosted (WsDualHttpBinding-ChannelFactory is using HttpListener instead of IIS even when running in IIS)
            if (!AspNetEnvironment.Enabled ||
                this.Extensions.Find<VirtualPathExtension>() == null)
            {
                return;
            }

            foreach (ServiceEndpoint serviceEndpoint in this.Description.Endpoints)
            {
                if (serviceEndpoint.Binding != null &&
                    serviceEndpoint.ListenUri != null &&
                    ("http".Equals(serviceEndpoint.ListenUri.Scheme, StringComparison.OrdinalIgnoreCase) || "https".Equals(serviceEndpoint.ListenUri.Scheme, StringComparison.OrdinalIgnoreCase)) &&
                    this.baseAddresses.Contains(serviceEndpoint.ListenUri.Scheme))
                {
                    HttpTransportBindingElement httpTransportBindingElement = serviceEndpoint.Binding.CreateBindingElements().Find<HttpTransportBindingElement>();

                    if (httpTransportBindingElement != null)
                    {
                        AuthenticationSchemes hostSupportedAuthenticationSchemes = AspNetEnvironment.Current.GetAuthenticationSchemes(this.baseAddresses[serviceEndpoint.ListenUri.Scheme]);

                        if (hostSupportedAuthenticationSchemes != AuthenticationSchemes.None)
                        {
                            //If no authentication schemes are explicitly defined for the ServiceHost...
                            if (this.Authentication.AuthenticationSchemes == AuthenticationSchemes.None)
                            {
                                //Inherit authentication schemes from IIS
                                this.Authentication.AuthenticationSchemes = hostSupportedAuthenticationSchemes;
                            }
                            else
                            {
                                // Build intersection between authenticationSchemes on the ServiceHost and in IIS
                                this.Authentication.AuthenticationSchemes &= hostSupportedAuthenticationSchemes;
                            }
                        }
                    }

                    break;
                }
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.OnBeginOpen();

            AfterInitializeRuntime(timeoutHelper.RemainingTime());

            for (int i = 0; i < this.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcherBase dispatcher = this.ChannelDispatchers[i];
                dispatcher.Open(timeoutHelper.RemainingTime());
            }
        }

        protected override void OnOpened()
        {
            if (this.Description != null)
            {
                ServiceCredentials c = description.Behaviors.Find<ServiceCredentials>();
                if (c != null)
                {
                    ServiceCredentials credentialsCopy = c.Clone();
                    credentialsCopy.MakeReadOnly();
                    this.readOnlyCredentials = credentialsCopy;
                }

                ServiceAuthorizationBehavior authorization = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                if (authorization != null)
                {
                    ServiceAuthorizationBehavior authorizationCopy = authorization.Clone();
                    authorizationCopy.MakeReadOnly();
                    this.readOnlyAuthorization = authorizationCopy;
                }

                ServiceAuthenticationBehavior authentication = description.Behaviors.Find<ServiceAuthenticationBehavior>();
                if (authentication != null)
                {
                    ServiceAuthenticationBehavior authenticationCopy = authentication.Clone();
                    authentication.MakeReadOnly();
                    this.readOnlyAuthentication = authenticationCopy;
                }

                if (ManagementExtension.IsEnabled)
                {
                    ManagementExtension.OnServiceOpened(this);
                }

                // log telemetry data for the current WCF service.
                TelemetryTraceLogging.LogSeriveKPIData(this.Description);
            }
            base.OnOpened();

            if (TD.ServiceHostOpenStopIsEnabled())
            {
                TD.ServiceHostOpenStop(this.EventTraceActivity);
            }
        }

        internal void OnRemoveChannelDispatcher(ChannelDispatcherBase channelDispatcher)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfClosedOrOpened();
                channelDispatcher.DetachInternal(this);
            }
        }

        void OnChannelDispatcherFaulted(object sender, EventArgs e)
        {
            this.Fault();
        }

        void OnServiceHostFaulted(object sender, EventArgs args)
        {
            if (TD.ServiceHostFaultedIsEnabled())
            {
                TD.ServiceHostFaulted(this.EventTraceActivity, this);
            }

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.ServiceHostFaulted,
                    SR.GetString(SR.TraceCodeServiceHostFaulted), this);
            }

            foreach (ICommunicationObject channelDispatcher in this.SnapshotChannelDispatchers())
            {
                if (channelDispatcher.State == CommunicationState.Opened)
                {
                    channelDispatcher.Abort();
                }
            }
        }

        internal void RaiseUnknownMessageReceived(Message message)
        {
            try
            {
                EventHandler<UnknownMessageReceivedEventArgs> handler = UnknownMessageReceived;
                if (handler != null)
                {
                    handler(this, new UnknownMessageReceivedEventArgs(message));
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        protected void ReleasePerformanceCounters()
        {
            if (this.servicePerformanceCounters != null)
            {
                lock (this.ThisLock)
                {
                    if (this.servicePerformanceCounters != null)
                    {
                        this.servicePerformanceCounters.Dispose();
                        this.servicePerformanceCounters = null;
                    }
                }
            }
            if (this.defaultPerformanceCounters != null)
            {
                lock (this.ThisLock)
                {
                    if (this.defaultPerformanceCounters != null)
                    {
                        this.defaultPerformanceCounters.Dispose();
                        this.defaultPerformanceCounters = null;
                    }
                }
            }
        }

        ICommunicationObject[] SnapshotChannelDispatchers()
        {
            lock (this.ThisLock)
            {
                ICommunicationObject[] array = new ICommunicationObject[this.ChannelDispatchers.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = this.ChannelDispatchers[i];
                }
                return array;
            }
        }

        internal virtual void UnbindInstance(InstanceContext instance)
        {
            this.instances.Remove(instance);
            if (null != this.servicePerformanceCounters)
            {
                lock (this.ThisLock)
                {
                    if (null != this.servicePerformanceCounters)
                    {
                        this.servicePerformanceCounters.ServiceInstanceRemoved();
                    }
                }
            }
        }

        internal void IncrementBusyCount()
        {
            if (AspNetEnvironment.Enabled)
            {
                AspNetEnvironment.Current.IncrementBusyCount();
                Interlocked.Increment(ref this.busyCount);
            }

            EventHandler handler = this.BusyCountIncremented;
            if (handler != null)
            {
                try
                {
                    handler(this, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        internal void DecrementBusyCount()
        {
            if (AspNetEnvironment.Enabled)
            {
                Interlocked.Decrement(ref this.busyCount);
                AspNetEnvironment.Current.DecrementBusyCount();
            }
        }

        internal int BusyCount
        {
            get
            {
                return this.busyCount;
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndAfterInitializeRuntime = new AsyncCompletion(HandleEndAfterInitializeRuntime);
            static AsyncCompletion handleEndOpenChannelDispatchers = new AsyncCompletion(HandleEndOpenChannelDispatchers);

            TimeoutHelper timeoutHelper;
            ServiceHostBase host;

            public OpenAsyncResult(ServiceHostBase host, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.host = host;

                if (ProcessAfterInitializeRuntime())
                {
                    Complete(true);
                }
            }

            bool ProcessAfterInitializeRuntime()
            {
                IAsyncResult result = this.host.BeginAfterInitializeRuntime(
                    this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndAfterInitializeRuntime), this);

                return SyncContinue(result);
            }

            static bool HandleEndAfterInitializeRuntime(IAsyncResult result)
            {
                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                thisPtr.host.EndAfterInitializeRuntime(result);

                return thisPtr.ProcessOpenChannelDispatchers();
            }

            bool ProcessOpenChannelDispatchers()
            {
                IAsyncResult result = this.host.BeginOpenChannelDispatchers(
                    this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndOpenChannelDispatchers), this);

                return SyncContinue(result);
            }

            static bool HandleEndOpenChannelDispatchers(IAsyncResult result)
            {
                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                thisPtr.host.EndOpenChannelDispatchers(result);

                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            ServiceHostBase serviceHost;
            TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ServiceHostBase serviceHost)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.serviceHost = serviceHost;

                if (ManagementExtension.IsEnabled && null != serviceHost.Description)
                {
                    ManagementExtension.OnServiceClosing(serviceHost);
                }

                this.CloseListeners(true);
            }

            void CloseListeners(bool completedSynchronously)
            {
                List<ICommunicationObject> listeners = new List<ICommunicationObject>();
                for (int i = 0; i < this.serviceHost.ChannelDispatchers.Count; i++)
                {
                    if (this.serviceHost.ChannelDispatchers[i].Listener != null)
                    {
                        listeners.Add(this.serviceHost.ChannelDispatchers[i].Listener);
                    }
                }

                AsyncCallback callback = Fx.ThunkCallback(this.CloseListenersCallback);
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = new CloseCollectionAsyncResult(timeout, callback, this, listeners);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseListeners(result, completedSynchronously);
                }
            }

            void CloseListenersCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((CloseAsyncResult)result.AsyncState).FinishCloseListeners(result, false);
                }
            }

            void FinishCloseListeners(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    CloseCollectionAsyncResult.End(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseInput(completedSynchronously);
                }
            }

            // Wait for existing work to complete
            void CloseInput(bool completedSynchronously)
            {
                AsyncCallback callback = Fx.ThunkCallback(this.CloseInputCallback);
                Exception exception = null;
                IAsyncResult result = null;

                try
                {
                    for (int i = 0; i < this.serviceHost.ChannelDispatchers.Count; i++)
                    {
                        ChannelDispatcherBase dispatcher = this.serviceHost.ChannelDispatchers[i];
                        dispatcher.CloseInput(this.timeoutHelper.RemainingTime());
                    }

                    result = this.serviceHost.instances.BeginCloseInput(this.timeoutHelper.RemainingTime(), callback, this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }

                    exception = e;
                }

                if (exception != null)
                {
                    // Any exception during async processing causes this
                    // async callback to report the error and then relies on
                    // Abort to cleanup any unclosed channels or instance contexts.
                    FxTrace.Exception.AsWarning(exception);
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseInput(result, completedSynchronously);
                }
            }

            void CloseInputCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((CloseAsyncResult)result.AsyncState).FinishCloseInput(result, false);
                }
            }

            void FinishCloseInput(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    serviceHost.instances.EndCloseInput(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseInstances(completedSynchronously);
                }
            }

            // Close instances (closes contexts/channels)
            void CloseInstances(bool completedSynchronously)
            {
                AsyncCallback callback = Fx.ThunkCallback(this.CloseInstancesCallback);
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;

                try
                {
                    result = this.serviceHost.instances.BeginClose(timeout, callback, this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseInstances(result, completedSynchronously);
                }
            }

            void CloseInstancesCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((CloseAsyncResult)result.AsyncState).FinishCloseInstances(result, false);
                }
            }

            void FinishCloseInstances(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    this.serviceHost.instances.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseChannelDispatchers(completedSynchronously);
                }
            }

            void CloseChannelDispatchers(bool completedSynchronously)
            {
                IList<ICommunicationObject> channelDispatchers = this.serviceHost.SnapshotChannelDispatchers();
                AsyncCallback callback = Fx.ThunkCallback(this.CloseChannelDispatchersCallback);
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = new CloseCollectionAsyncResult(timeout, callback, this, channelDispatchers);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseChannelDispatchers(result, completedSynchronously);
                }
            }

            void CloseChannelDispatchersCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((CloseAsyncResult)result.AsyncState).FinishCloseChannelDispatchers(result, false);
                }
            }

            void FinishCloseChannelDispatchers(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    CloseCollectionAsyncResult.End(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = e;
                }

                this.CallComplete(completedSynchronously, exception);
            }

            void CallComplete(bool completedSynchronously, Exception exception)
            {
                this.Complete(completedSynchronously, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }

        class ImplementedContractsContractResolver : IContractResolver
        {
            IDictionary<string, ContractDescription> implementedContracts;

            public ImplementedContractsContractResolver(IDictionary<string, ContractDescription> implementedContracts)
            {
                this.implementedContracts = implementedContracts;
            }

            public ContractDescription ResolveContract(string contractName)
            {
                return this.implementedContracts != null && this.implementedContracts.ContainsKey(contractName) ? this.implementedContracts[contractName] : null;
            }
        }

        internal class ServiceAndBehaviorsContractResolver : IContractResolver
        {
            IContractResolver serviceResolver;
            Dictionary<string, ContractDescription> behaviorContracts;

            public Dictionary<string, ContractDescription> BehaviorContracts
            {
                get { return behaviorContracts; }
            }

            public ServiceAndBehaviorsContractResolver(IContractResolver serviceResolver)
            {
                this.serviceResolver = serviceResolver;
                behaviorContracts = new Dictionary<string, ContractDescription>();
            }

            public ContractDescription ResolveContract(string contractName)
            {
                ContractDescription contract = serviceResolver.ResolveContract(contractName);

                if (contract == null)
                {
                    contract = this.behaviorContracts.ContainsKey(contractName) ? this.behaviorContracts[contractName] : null;
                }

                return contract;
            }

            public void AddBehaviorContractsToResolver(KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                // It would be nice to make this loop over all Behaviors... someday.
                if (behaviors != null && behaviors.Contains(typeof(ServiceMetadataBehavior)))
                {
                    behaviors.Find<ServiceMetadataBehavior>().AddImplementedContracts(this);
                }
            }
        }
    }

    public class ServiceHost : ServiceHostBase
    {
        object singletonInstance;
        Type serviceType;
        ReflectedContractCollection reflectedContracts;
        IDisposable disposableInstance;

        protected ServiceHost()
        {
        }

        public ServiceHost(Type serviceType, params Uri[] baseAddresses)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceType"));
            }

            this.serviceType = serviceType;
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructServiceHost, serviceType.FullName), ActivityType.Construct);
                }

                InitializeDescription(serviceType, new UriSchemeKeyedCollection(baseAddresses));
            }
        }

        public ServiceHost(object singletonInstance, params Uri[] baseAddresses)
        {
            if (singletonInstance == null)
            {
                throw new ArgumentNullException("singletonInstance");
            }

            this.singletonInstance = singletonInstance;
            this.serviceType = singletonInstance.GetType();
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructServiceHost, serviceType.FullName), ActivityType.Construct);
                }

                InitializeDescription(singletonInstance, new UriSchemeKeyedCollection(baseAddresses));
            }
        }

        public object SingletonInstance
        {
            get
            {
                return this.singletonInstance;
            }
        }

        internal override object DisposableInstance
        {
            get
            {
                return this.disposableInstance;
            }
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, string address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, (Uri)null);
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, string address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }

            ServiceEndpoint endpoint = this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute));
            if (listenUri != null)
            {
                listenUri = MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, Uri address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, (Uri)null);
        }

        void ValidateContractType(Type implementedContract, ReflectedAndBehaviorContractCollection reflectedAndBehaviorContracts)
        {
            if (!implementedContract.IsDefined(typeof(ServiceContractAttribute), false))
            {
#pragma warning suppress 56506 // implementedContract is never null at this point
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxServiceContractAttributeNotFound, implementedContract.FullName)));
            }
            if (!reflectedAndBehaviorContracts.Contains(implementedContract))
            {
                if (implementedContract == typeof(IMetadataExchange))
#pragma warning suppress 56506 // ServiceType is never null at this point
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFoundIMetadataExchange, this.serviceType.FullName)));
                else
#pragma warning suppress 56506 // implementedContract and ServiceType are never null at this point
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFound2, implementedContract.FullName, this.serviceType.FullName)));
            }
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            if (implementedContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("implementedContract"));
            }
            if (this.reflectedContracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractsNotInitialized1, implementedContract.FullName)));
            }
            ReflectedAndBehaviorContractCollection reflectedAndBehaviorContracts = new ReflectedAndBehaviorContractCollection(this.reflectedContracts, this.Description.Behaviors);
            ValidateContractType(implementedContract, reflectedAndBehaviorContracts);
            ServiceEndpoint endpoint = AddServiceEndpoint(reflectedAndBehaviorContracts.GetConfigKey(implementedContract), binding, address);
            if (listenUri != null)
            {
                listenUri = MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        internal override void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
            // don't generate endpoints for contracts that serve as the base type for other reflected contracts
            List<ContractDescription> mostSpecificContracts = new List<ContractDescription>();
            for (int i = 0; i < this.reflectedContracts.Count; i++)
            {
                bool addContractEndpoint = true;
                ContractDescription contract = this.reflectedContracts[i];
                Type contractType = contract.ContractType;
                if (contractType != null)
                {
                    for (int j = 0; j < this.reflectedContracts.Count; j++)
                    {
                        ContractDescription otherContract = this.reflectedContracts[j];
                        Type otherContractType = otherContract.ContractType;
                        if (i == j || otherContractType == null)
                        {
                            continue;
                        }
                        if (contractType.IsAssignableFrom(otherContractType))
                        {
                            addContractEndpoint = false;
                            break;
                        }
                    }
                }
                if (addContractEndpoint)
                {
                    mostSpecificContracts.Add(contract);
                }
            }

            foreach (ContractDescription contract in mostSpecificContracts)
            {
                ServiceEndpoint endpoint = AddServiceEndpoint(contract.ConfigurationName, defaultBinding, string.Empty);
                ConfigLoader.LoadDefaultEndpointBehaviors(endpoint);
                defaultEndpoints.Add(endpoint);
            }
        }

        // Run static Configure method on service type if it exists, else load configuration from Web.config/App.config
        protected override void ApplyConfiguration()
        {
            // Load from static Configure method if it exists with the right signature
            Type serviceType = this.Description.ServiceType;
            if (serviceType != null)
            {
                MethodInfo configure = GetConfigureMethod(serviceType);
                if (configure != null)
                {
                    // load <host> config
                    ConfigLoader configLoader = new ConfigLoader(GetContractResolver(this.ImplementedContracts));
                    LoadHostConfigurationInternal(configLoader, this.Description, this.Description.ConfigurationName);

                    // Invoke configure method for service
                    ServiceConfiguration configuration = new ServiceConfiguration(this);
                    InvokeConfigure(configure, configuration);

                    return;
                }
            }

            // else just load from Web.config/App.config
            base.ApplyConfiguration();
        }

        // Find the Configure method with the required signature, closest to serviceType in the type hierarchy
        static MethodInfo GetConfigureMethod(Type serviceType)
        {
            // Use recursion instead of BindingFlags.FlattenHierarchy because we require return type to be void
            
            // base case: all Types are rooted in object eventually
            if (serviceType == typeof(object))
            {
                return null;
            }

            // signature: "public static void Configure(ServiceConfiguration)"
            MethodInfo configure = serviceType.GetMethod("Configure", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(ServiceConfiguration) }, null);

            if (configure != null && configure.ReturnType == typeof(void))
            {
                return configure;
            }
            else
            {
                return GetConfigureMethod(serviceType.BaseType);
            }
        }

        static void InvokeConfigure(MethodInfo configureMethod, ServiceConfiguration configuration)
        {
            Action<ServiceConfiguration> call = Delegate.CreateDelegate(typeof(Action<ServiceConfiguration>), configureMethod) as Action<ServiceConfiguration>;
            call(configuration);
        }

        // called from ServiceConfiguration.LoadFromConfiguration()
        internal void LoadFromConfiguration()
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotApplyConfigurationWithoutDescription)));
            }

            ConfigLoader configLoader = new ConfigLoader(GetContractResolver(this.ImplementedContracts));

            // Call the overload of LoadConfigurationSectionInternal which looks up the serviceElement from ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            LoadConfigurationSectionExceptHostInternal(configLoader, this.Description, this.Description.ConfigurationName);
            EnsureAuthenticationAuthorizationDebug(this.Description);
        }

        // called from ServiceConfiguration.LoadFromConfiguration(configuration)
        internal void LoadFromConfiguration(System.Configuration.Configuration configuration)
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostBaseCannotApplyConfigurationWithoutDescription)));
            }

            ConfigLoader configLoader = new ConfigLoader(GetContractResolver(this.ImplementedContracts));

            // Look up the serviceElement explicitly on configuration, then call the overload of LoadConfigurationSectionInternal that loads the rest of the config from the same configuration as serviceElement
            ServicesSection servicesSection = (ServicesSection)configuration.GetSection(ConfigurationStrings.ServicesSectionPath);
            ServiceElement serviceElement = configLoader.LookupService(this.Description.ConfigurationName, servicesSection);
            configLoader.LoadServiceDescription(this, this.Description, serviceElement, this.LoadConfigurationSectionHelper, skipHost: true);

            EnsureAuthenticationAuthorizationDebug(this.Description);
        }

        // Load only "host" section within "service" tag
        [Fx.Tag.SecurityNote(Critical = "Calls LookupService which is critical.",
            Safe = "Doesn't leak ServiceElement out of SecurityCritical code.")]
        [SecuritySafeCritical]
        void LoadHostConfigurationInternal(ConfigLoader configLoader, ServiceDescription description, string configurationName)
        {
            ServiceElement serviceSection = configLoader.LookupService(configurationName);
            if (serviceSection != null)
            {
                configLoader.LoadHostConfig(serviceSection, this, (addr => this.InternalBaseAddresses.Add(addr)));
            }
        }

        // Load service description for service from config, but skip "host" section within "service" tag
        [Fx.Tag.SecurityNote(Critical = "Calls LookupService which is critical.",
            Safe = "Doesn't leak ServiceElement out of SecurityCritical code.")]
        [SecuritySafeCritical]
        void LoadConfigurationSectionExceptHostInternal(ConfigLoader configLoader, ServiceDescription description, string configurationName)
        {
            ServiceElement serviceSection = configLoader.LookupService(configurationName);
            configLoader.LoadServiceDescription(this, description, serviceSection, this.LoadConfigurationSectionHelper, skipHost: true);
        }
        
        internal override string CloseActivityName
        {
            get { return SR.GetString(SR.ActivityCloseServiceHost, this.serviceType.FullName); }
        }

        internal override string OpenActivityName
        {
            get { return SR.GetString(SR.ActivityOpenServiceHost, this.serviceType.FullName); }
        }

        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            if (this.serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceHostCannotCreateDescriptionWithoutServiceType)));
            }

            ServiceDescription description;
            if (this.SingletonInstance != null)
            {
                description = ServiceDescription.GetService(this.SingletonInstance);
            }
            else
            {
                description = ServiceDescription.GetService(this.serviceType);
            }
            ServiceBehaviorAttribute serviceBehavior = description.Behaviors.Find<ServiceBehaviorAttribute>();
            object serviceInstanceUsedAsABehavior = serviceBehavior.GetWellKnownSingleton();
            if (serviceInstanceUsedAsABehavior == null)
            {
                serviceInstanceUsedAsABehavior = serviceBehavior.GetHiddenSingleton();
                this.disposableInstance = serviceInstanceUsedAsABehavior as IDisposable;
            }

            if ((typeof(IServiceBehavior).IsAssignableFrom(this.serviceType) || typeof(IContractBehavior).IsAssignableFrom(this.serviceType))
                && serviceInstanceUsedAsABehavior == null)
            {
                serviceInstanceUsedAsABehavior = ServiceDescription.CreateImplementation(this.serviceType);
                this.disposableInstance = serviceInstanceUsedAsABehavior as IDisposable;
            }

            if (this.SingletonInstance == null)
            {
                if (serviceInstanceUsedAsABehavior is IServiceBehavior)
                {
                    description.Behaviors.Add((IServiceBehavior)serviceInstanceUsedAsABehavior);
                }
            }

            ReflectedContractCollection reflectedContracts = new ReflectedContractCollection();
            List<Type> interfaces = ServiceReflector.GetInterfaces(this.serviceType);
            for (int i = 0; i < interfaces.Count; i++)
            {
                Type contractType = interfaces[i];
                if (!reflectedContracts.Contains(contractType))
                {
                    ContractDescription contract = null;
                    if (serviceInstanceUsedAsABehavior != null)
                    {
                        contract = ContractDescription.GetContract(contractType, serviceInstanceUsedAsABehavior);
                    }
                    else
                    {
                        contract = ContractDescription.GetContract(contractType, this.serviceType);
                    }

                    reflectedContracts.Add(contract);
                    Collection<ContractDescription> inheritedContracts = contract.GetInheritedContracts();
                    for (int j = 0; j < inheritedContracts.Count; j++)
                    {
                        ContractDescription inheritedContract = inheritedContracts[j];
                        if (!reflectedContracts.Contains(inheritedContract.ContractType))
                        {
                            reflectedContracts.Add(inheritedContract);
                        }
                    }
                }
            }
            this.reflectedContracts = reflectedContracts;

            implementedContracts = reflectedContracts.ToImplementedContracts();
            return description;
        }

        protected void InitializeDescription(object singletonInstance, UriSchemeKeyedCollection baseAddresses)
        {
            if (singletonInstance == null)
            {
                throw new ArgumentNullException("singletonInstance");
            }

            this.singletonInstance = singletonInstance;
            InitializeDescription(singletonInstance.GetType(), baseAddresses);
        }

        protected void InitializeDescription(Type serviceType, UriSchemeKeyedCollection baseAddresses)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceType"));
            }

            this.serviceType = serviceType;

            base.InitializeDescription(baseAddresses);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.disposableInstance != null)
            {
                this.disposableInstance.Dispose();
            }
        }

        class ReflectedContractCollection : KeyedCollection<Type, ContractDescription>
        {
            public ReflectedContractCollection()
                : base(null, 4)
            {
            }

            protected override Type GetKeyForItem(ContractDescription item)
            {
                if (item == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

                return item.ContractType;
            }

            public IDictionary<string, ContractDescription> ToImplementedContracts()
            {
                Dictionary<string, ContractDescription> implementedContracts = new Dictionary<string, ContractDescription>();
                foreach (ContractDescription contract in this.Items)
                {
                    implementedContracts.Add(GetConfigKey(contract), contract);
                }
                return implementedContracts;
            }

            internal static string GetConfigKey(ContractDescription contract)
            {
                return contract.ConfigurationName;
            }
        }

        class ReflectedAndBehaviorContractCollection
        {
            ReflectedContractCollection reflectedContracts;
            KeyedByTypeCollection<IServiceBehavior> behaviors;
            public ReflectedAndBehaviorContractCollection(ReflectedContractCollection reflectedContracts, KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                this.reflectedContracts = reflectedContracts;
                this.behaviors = behaviors;
            }

            internal bool Contains(Type implementedContract)
            {
                if (this.reflectedContracts.Contains(implementedContract))
                {
                    return true;
                }

                if (this.behaviors.Contains(typeof(ServiceMetadataBehavior)) && ServiceMetadataBehavior.IsMetadataImplementedType(implementedContract))
                {
                    return true;
                }

                return false;
            }

            internal string GetConfigKey(Type implementedContract)
            {
                if (this.reflectedContracts.Contains(implementedContract))
                {
                    return ReflectedContractCollection.GetConfigKey(reflectedContracts[implementedContract]);
                }

                if (this.behaviors.Contains(typeof(ServiceMetadataBehavior)) && ServiceMetadataBehavior.IsMetadataImplementedType(implementedContract))
                {
                    return ServiceMetadataBehavior.MexContractName;
                }

                Fx.Assert("Calls to GetConfigKey are preceeded by calls to Contains.");
#pragma warning suppress 56506 // implementedContract is never null at this point
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxReflectedContractKeyNotFound2, implementedContract.FullName, string.Empty)));

            }
        }
    }
}
