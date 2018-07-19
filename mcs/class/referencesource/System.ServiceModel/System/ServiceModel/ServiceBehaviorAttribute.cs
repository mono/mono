//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Configuration;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Threading;
    using System.Transactions;
    using System.Runtime.CompilerServices;
    using System.ComponentModel;
    using System.Globalization;

    [AttributeUsage(ServiceModelAttributeTargets.ServiceBehavior)]
    public sealed class ServiceBehaviorAttribute : Attribute, IServiceBehavior
    {
        internal static IsolationLevel DefaultIsolationLevel = IsolationLevel.Unspecified;
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Single;
        bool ensureOrderedDispatch = false;
        string configurationName;
        bool includeExceptionDetailInFaults = false;
        InstanceContextMode instanceMode;
        bool releaseServiceInstanceOnTransactionComplete = true;
        bool releaseServiceInstanceOnTransactionCompleteSet = false;
        bool transactionAutoCompleteOnSessionClose = false;
        bool transactionAutoCompleteOnSessionCloseSet = false;
        object wellKnownSingleton = null;  // if the user passes an object to the ServiceHost, it is stored here
        object hiddenSingleton = null;     // if the user passes a type to the ServiceHost, and instanceMode==Single, we store the instance here
        bool validateMustUnderstand = true;
        bool ignoreExtensionDataObject = DataContractSerializerDefaults.IgnoreExtensionDataObject;
        int maxItemsInObjectGraph = DataContractSerializerDefaults.MaxItemsInObjectGraph;
        IsolationLevel transactionIsolationLevel = DefaultIsolationLevel;
        bool isolationLevelSet = false;
        bool automaticSessionShutdown = true;
        IInstanceProvider instanceProvider = null;
        TimeSpan transactionTimeout = TimeSpan.Zero;
        string transactionTimeoutString;
        bool transactionTimeoutSet = false;
        bool useSynchronizationContext = true;
        string serviceName = null;
        string serviceNamespace = null;
        AddressFilterMode addressFilterMode = AddressFilterMode.Exact;

        [DefaultValue(null)]
        public string Name
        {
            get { return serviceName; }
            set { serviceName = value; }
        }

        [DefaultValue(null)]
        public string Namespace
        {
            get { return serviceNamespace; }
            set { serviceNamespace = value; }
        }

        internal IInstanceProvider InstanceProvider
        {
            set { this.instanceProvider = value; }
        }

        [DefaultValue(AddressFilterMode.Exact)]
        public AddressFilterMode AddressFilterMode
        {
            get { return this.addressFilterMode; }
            set
            {
                if (!AddressFilterModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.addressFilterMode = value;
            }
        }

        [DefaultValue(true)]
        public bool AutomaticSessionShutdown
        {
            get { return this.automaticSessionShutdown; }
            set { this.automaticSessionShutdown = value; }
        }

        [DefaultValue(null)]
        public string ConfigurationName
        {
            get { return this.configurationName; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.SFxConfigurationNameCannotBeEmpty)));
                }
                this.configurationName = value;
            }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get { return this.transactionIsolationLevel; }
            set
            {
                switch (value)
                {
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                    case IsolationLevel.ReadCommitted:
                    case IsolationLevel.ReadUncommitted:
                    case IsolationLevel.Unspecified:
                    case IsolationLevel.Chaos:
                    case IsolationLevel.Snapshot:
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.transactionIsolationLevel = value;
                isolationLevelSet = true;
            }
        }

        public bool ShouldSerializeTransactionIsolationLevel()
        {
            return IsolationLevelSet;
        }

        internal bool IsolationLevelSet
        {
            get { return this.isolationLevelSet; }
        }

        [DefaultValue(false)]
        public bool IncludeExceptionDetailInFaults
        {
            get { return this.includeExceptionDetailInFaults; }
            set { this.includeExceptionDetailInFaults = value; }
        }

        [DefaultValue(ConcurrencyMode.Single)]
        public ConcurrencyMode ConcurrencyMode
        {
            get { return this.concurrencyMode; }
            set
            {
                if (!ConcurrencyModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.concurrencyMode = value;
            }
        }

        [DefaultValue(false)]
        public bool EnsureOrderedDispatch
        {
            get { return this.ensureOrderedDispatch; }
            set { this.ensureOrderedDispatch = value; }
        }

        [DefaultValue(InstanceContextMode.PerSession)]
        public InstanceContextMode InstanceContextMode
        {
            get { return this.instanceMode; }
            set
            {
                if (!InstanceContextModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.instanceMode = value;
            }
        }

        public bool ReleaseServiceInstanceOnTransactionComplete
        {
            get { return releaseServiceInstanceOnTransactionComplete; }
            set
            {
                this.releaseServiceInstanceOnTransactionComplete = value;
                this.releaseServiceInstanceOnTransactionCompleteSet = true;
            }
        }

        public bool ShouldSerializeConfigurationName()
        {
            return this.configurationName != null;
        }

        public bool ShouldSerializeReleaseServiceInstanceOnTransactionComplete()
        {
            return ReleaseServiceInstanceOnTransactionCompleteSet;
        }

        internal bool ReleaseServiceInstanceOnTransactionCompleteSet
        {
            get { return this.releaseServiceInstanceOnTransactionCompleteSet; }
        }

        public bool TransactionAutoCompleteOnSessionClose
        {
            get { return transactionAutoCompleteOnSessionClose; }
            set
            {
                this.transactionAutoCompleteOnSessionClose = value;
                this.transactionAutoCompleteOnSessionCloseSet = true;
            }
        }

        public bool ShouldSerializeTransactionAutoCompleteOnSessionClose()
        {
            return TransactionAutoCompleteOnSessionCloseSet;
        }

        internal bool TransactionAutoCompleteOnSessionCloseSet
        {
            get { return this.transactionAutoCompleteOnSessionCloseSet; }
        }

        public string TransactionTimeout
        {
            get { return transactionTimeoutString; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }

                try
                {
                    TimeSpan timeout = TimeSpan.Parse(value, CultureInfo.InvariantCulture);

                    if (timeout < TimeSpan.Zero)
                    {
                        string message = SR.GetString(SR.SFxTimeoutOutOfRange0);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
                    }

                    this.transactionTimeout = timeout;
                    this.transactionTimeoutString = value;
                    this.transactionTimeoutSet = true;
                }
                catch (FormatException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxTimeoutInvalidStringFormat), "value", e));
                }
                catch (OverflowException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
            }
        }

        public bool ShouldSerializeTransactionTimeout()
        {
            return TransactionTimeoutSet;
        }

        internal TimeSpan TransactionTimeoutTimespan
        {
            get { return this.transactionTimeout; }
        }

        internal bool TransactionTimeoutSet
        {
            get { return this.transactionTimeoutSet; }
        }

        [DefaultValue(true)]
        public bool ValidateMustUnderstand
        {
            get { return validateMustUnderstand; }
            set { validateMustUnderstand = value; }
        }

        [DefaultValue(DataContractSerializerDefaults.IgnoreExtensionDataObject)]
        public bool IgnoreExtensionDataObject
        {
            get { return ignoreExtensionDataObject; }
            set { ignoreExtensionDataObject = value; }
        }

        [DefaultValue(DataContractSerializerDefaults.MaxItemsInObjectGraph)]
        public int MaxItemsInObjectGraph
        {
            get { return maxItemsInObjectGraph; }
            set { maxItemsInObjectGraph = value; }
        }

        [DefaultValue(true)]
        public bool UseSynchronizationContext
        {
            get { return this.useSynchronizationContext; }
            set { this.useSynchronizationContext = value; }
        }

        public object GetWellKnownSingleton()
        {
            return this.wellKnownSingleton;
        }

        public void SetWellKnownSingleton(object value)
        {
            if (value == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            this.wellKnownSingleton = value;
        }

        internal object GetHiddenSingleton()
        {
            return this.hiddenSingleton;
        }

        internal void SetHiddenSingleton(object value)
        {
            if (value == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            this.hiddenSingleton = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void SetIsolationLevel(ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");

            channelDispatcher.TransactionIsolationLevel = this.transactionIsolationLevel;
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (this.concurrencyMode != ConcurrencyMode.Single && this.ensureOrderedDispatch)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNonConcurrentOrEnsureOrderedDispatch, description.Name)));
            }
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    channelDispatcher.IncludeExceptionDetailInFaults = this.includeExceptionDetailInFaults;

                    if (channelDispatcher.HasApplicationEndpoints())
                    {
                        channelDispatcher.TransactionTimeout = transactionTimeout;
                        if (isolationLevelSet)
                            SetIsolationLevel(channelDispatcher);

                        foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                        {
                            if (endpointDispatcher.IsSystemEndpoint)
                            {
                                continue;
                            }
                            DispatchRuntime behavior = endpointDispatcher.DispatchRuntime;
                            behavior.ConcurrencyMode = this.concurrencyMode;
                            behavior.EnsureOrderedDispatch = this.ensureOrderedDispatch;
                            behavior.ValidateMustUnderstand = validateMustUnderstand;
                            behavior.AutomaticInputSessionShutdown = this.automaticSessionShutdown;
                            behavior.TransactionAutoCompleteOnSessionClose = this.transactionAutoCompleteOnSessionClose;
                            behavior.ReleaseServiceInstanceOnTransactionComplete = this.releaseServiceInstanceOnTransactionComplete;
                            if (!this.useSynchronizationContext)
                            {
                                behavior.SynchronizationContext = null;
                            }

                            if (!endpointDispatcher.AddressFilterSetExplicit)
                            {
                                EndpointAddress address = endpointDispatcher.OriginalAddress;
                                if (address == null || this.AddressFilterMode == AddressFilterMode.Any)
                                {
                                    endpointDispatcher.AddressFilter = new MatchAllMessageFilter();
                                }
                                else if (this.AddressFilterMode == AddressFilterMode.Prefix)
                                {
                                    endpointDispatcher.AddressFilter = new PrefixEndpointAddressMessageFilter(address);
                                }
                                else if (this.AddressFilterMode == AddressFilterMode.Exact)
                                {
                                    endpointDispatcher.AddressFilter = new EndpointAddressMessageFilter(address);
                                }
                            }
                        }
                    }
#pragma warning suppress 56506
                }
            }
            DataContractSerializerServiceBehavior.ApplySerializationSettings(description, ignoreExtensionDataObject, maxItemsInObjectGraph);
            ApplyInstancing(description, serviceHostBase);
        }

        void ApplyInstancing(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            Type serviceType = description.ServiceType;
            InstanceContext singleton = null;

            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (endpointDispatcher.IsSystemEndpoint)
                        {
                            continue;
                        }
                        DispatchRuntime dispatch = endpointDispatcher.DispatchRuntime;
                        if (dispatch.InstanceProvider == null)
                        {
                            if (instanceProvider == null)
                            {
                                if (serviceType == null && this.wellKnownSingleton == null)
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InstanceSettingsMustHaveTypeOrWellKnownObject0)));

                                if (this.instanceMode != InstanceContextMode.Single && this.wellKnownSingleton != null)
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWellKnownNonSingleton0)));
                            }
                            else
                            {
                                dispatch.InstanceProvider = instanceProvider;
                            }
                        }
                        dispatch.Type = serviceType;
                        dispatch.InstanceContextProvider = InstanceContextProviderBase.GetProviderForMode(this.instanceMode, dispatch);

                        if ((this.instanceMode == InstanceContextMode.Single) &&
                            (dispatch.SingletonInstanceContext == null))
                        {
                            if (singleton == null)
                            {
                                if (this.wellKnownSingleton != null)
                                {
                                    singleton = new InstanceContext(serviceHostBase, this.wellKnownSingleton, true, false);
                                }
                                else if (this.hiddenSingleton != null)
                                {
                                    singleton = new InstanceContext(serviceHostBase, this.hiddenSingleton, false, false);
                                }
                                else
                                {
                                    singleton = new InstanceContext(serviceHostBase, false);
                                }

                                singleton.AutoClose = false;
                            }
                            dispatch.SingletonInstanceContext = singleton;
                        }
                    }
                }
            }
        }
    }
}
