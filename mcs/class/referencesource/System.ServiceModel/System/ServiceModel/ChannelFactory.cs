//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using SecurityToken = System.IdentityModel.Tokens.SecurityToken;
    using FederatedClientCredentialsParameters = System.IdentityModel.Protocols.WSTrust.FederatedClientCredentialsParameters;
    
    public abstract class ChannelFactory : CommunicationObject, IChannelFactory, IDisposable
    {
        string configurationName;
        IChannelFactory innerFactory;
        ServiceEndpoint serviceEndpoint;
        ClientCredentials readOnlyClientCredentials;
        object openLock = new object();

        //Overload for activation DuplexChannelFactory
        protected ChannelFactory()
            : base()
        {
            TraceUtility.SetEtwProviderId();
            this.TraceOpenAndClose = true;
        }

        public ClientCredentials Credentials
        {
            get
            {
                if (this.Endpoint == null)
                    return null;
                if (this.State == CommunicationState.Created || this.State == CommunicationState.Opening)
                {
                    return EnsureCredentials(this.Endpoint);
                }
                else
                {
                    if (this.readOnlyClientCredentials == null)
                    {
                        ClientCredentials c = new ClientCredentials();
                        c.MakeReadOnly();
                        this.readOnlyClientCredentials = c;
                    }
                    return this.readOnlyClientCredentials;
                }
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if (this.Endpoint != null && this.Endpoint.Binding != null)
                {
                    return this.Endpoint.Binding.CloseTimeout;
                }
                else
                {
                    return ServiceDefaults.CloseTimeout;
                }
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                if (this.Endpoint != null && this.Endpoint.Binding != null)
                {
                    return this.Endpoint.Binding.OpenTimeout;
                }
                else
                {
                    return ServiceDefaults.OpenTimeout;
                }
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return this.serviceEndpoint;
            }
        }

        internal IChannelFactory InnerFactory
        {
            get { return this.innerFactory; }
        }

        // This boolean is used to determine if we should read ahead by a single
        // Message for IDuplexSessionChannels in order to detect null and
        // autoclose the underlying channel in that case.
        // Currently only accessed from the Send activity.
        [Fx.Tag.FriendAccessAllowed("System.ServiceModel.Activities")]
        internal bool UseActiveAutoClose
        {
            get;
            set;
        }

        protected internal void EnsureOpened()
        {
            base.ThrowIfDisposed();
            if (this.State != CommunicationState.Opened)
            {
                lock (this.openLock)
                {
                    if (this.State != CommunicationState.Opened)
                    {
                        this.Open();
                    }
                }
            }
        }

        // configurationName can be:
        // 1. null: don't bind any per-endpoint config (load common behaviors only)
        // 2. "*" (wildcard): match any endpoint config provided there's exactly 1
        // 3. anything else (including ""): match the endpoint config with the same name
        protected virtual void ApplyConfiguration(string configurationName)
        {
            this.ApplyConfiguration(configurationName, null);
        }

        void ApplyConfiguration(string configurationName, System.Configuration.Configuration configuration)
        {
            if (this.Endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryCannotApplyConfigurationWithoutEndpoint)));
            }

            if (!this.Endpoint.IsFullyConfigured)
            {
                ConfigLoader configLoader;
                if (configuration != null)
                {
                    configLoader = new ConfigLoader(configuration.EvaluationContext);
                }
                else
                {
                    configLoader = new ConfigLoader();
                }

                if (configurationName == null)
                {
                    configLoader.LoadCommonClientBehaviors(this.Endpoint);
                }
                else
                {
                    configLoader.LoadChannelBehaviors(this.Endpoint, configurationName);
                }
            }
        }

        protected abstract ServiceEndpoint CreateDescription();

        internal EndpointAddress CreateEndpointAddress(ServiceEndpoint endpoint)
        {
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryEndpointAddressUri)));
            }

            return endpoint.Address;
        }

        protected virtual IChannelFactory CreateFactory()
        {
            if (this.Endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryCannotCreateFactoryWithoutDescription)));
            }

            if (this.Endpoint.Binding == null)
            {
                if (this.configurationName != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryNoBindingFoundInConfig1, configurationName)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryNoBindingFoundInConfigOrCode)));
                }
            }

            return ServiceChannelFactory.BuildChannelFactory(this.Endpoint, this.UseActiveAutoClose);
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        void EnsureSecurityCredentialsManager(ServiceEndpoint endpoint)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            if (endpoint.Behaviors.Find<SecurityCredentialsManager>() == null)
            {
                endpoint.Behaviors.Add(new ClientCredentials());
            }
        }

        ClientCredentials EnsureCredentials(ServiceEndpoint endpoint)
        {
            Fx.Assert(this.State == CommunicationState.Created || this.State == CommunicationState.Opening, "");
            ClientCredentials c = endpoint.Behaviors.Find<ClientCredentials>();
            if (c == null)
            {
                c = new ClientCredentials();
                endpoint.Behaviors.Add(c);
            }
            return c;
        }

        public T GetProperty<T>() where T : class
        {
            if (this.innerFactory != null)
            {
                return this.innerFactory.GetProperty<T>();
            }
            else
            {
                return null;
            }
        }

        internal bool HasDuplexOperations()
        {
            OperationDescriptionCollection operations = this.Endpoint.Contract.Operations;
            for (int i = 0; i < operations.Count; i++)
            {
                OperationDescription operation = operations[i];
                if (operation.IsServerInitiated())
                {
                    return true;
                }
            }

            return false;
        }

        protected void InitializeEndpoint(string configurationName, EndpointAddress address)
        {
            this.serviceEndpoint = this.CreateDescription();

            ServiceEndpoint serviceEndpointFromConfig = null;
            if (configurationName != null)
            {
                serviceEndpointFromConfig = ConfigLoader.LookupEndpoint(configurationName, address, this.serviceEndpoint.Contract);
            }

            if (serviceEndpointFromConfig != null)
            {
                this.serviceEndpoint = serviceEndpointFromConfig;
            }
            else
            {
                if (address != null)
                {
                    this.Endpoint.Address = address;
                }

                ApplyConfiguration(configurationName);
            }
            this.configurationName = configurationName;
            EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        internal void InitializeEndpoint(string configurationName, EndpointAddress address, System.Configuration.Configuration configuration)
        {
            this.serviceEndpoint = this.CreateDescription();

            ServiceEndpoint serviceEndpointFromConfig = null;
            if (configurationName != null)
            {
                serviceEndpointFromConfig = ConfigLoader.LookupEndpoint(configurationName, address, this.serviceEndpoint.Contract, configuration.EvaluationContext);
            }

            if (serviceEndpointFromConfig != null)
            {
                this.serviceEndpoint = serviceEndpointFromConfig;
            }
            else
            {
                if (address != null)
                {
                    this.Endpoint.Address = address;
                }

                ApplyConfiguration(configurationName, configuration);
            }

            this.configurationName = configurationName;
            EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected void InitializeEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }

            this.serviceEndpoint = endpoint;

            ApplyConfiguration(null);
            EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected void InitializeEndpoint(Binding binding, EndpointAddress address)
        {
            this.serviceEndpoint = this.CreateDescription();

            if (binding != null)
            {
                this.Endpoint.Binding = binding;
            }
            if (address != null)
            {
                this.Endpoint.Address = address;
            }

            ApplyConfiguration(null);
            EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected override void OnOpened()
        {
            // if a client credentials has been configured cache a readonly snapshot of it
            if (this.Endpoint != null)
            {
                ClientCredentials credentials = this.Endpoint.Behaviors.Find<ClientCredentials>();
                if (credentials != null)
                {
                    ClientCredentials credentialsCopy = credentials.Clone();
                    credentialsCopy.MakeReadOnly();
                    this.readOnlyClientCredentials = credentialsCopy;
                }
            }
            base.OnOpened();
        }

        protected override void OnAbort()
        {
            if (this.innerFactory != null)
            {
                this.innerFactory.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this.innerFactory, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this.innerFactory, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.innerFactory != null)
                this.innerFactory.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerFactory.Open(timeout);
        }

        protected override void OnOpening()
        {
            base.OnOpening();

            this.innerFactory = CreateFactory();


            if (TD.ChannelFactoryCreatedIsEnabled())
            {
                TD.ChannelFactoryCreated(this);
            }


            if (this.innerFactory == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InnerChannelFactoryWasNotSet)));
        }

        class OpenAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnOpenComplete));

            public OpenAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;

                if (this.communicationObject == null)
                {
                    this.Complete(true);
                    return;
                }

                IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpenComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.communicationObject.EndOpen(result);
                    this.Complete(true);
                }
            }

            static void OnOpenComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.communicationObject.EndOpen(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnCloseComplete));

            public CloseAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;

                if (this.communicationObject == null)
                {
                    this.Complete(true);
                    return;
                }

                IAsyncResult result = this.communicationObject.BeginClose(timeout, onCloseComplete, this);

                if (result.CompletedSynchronously)
                {
                    this.communicationObject.EndClose(result);
                    this.Complete(true);
                }
            }

            static void OnCloseComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.communicationObject.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }

    public class ChannelFactory<TChannel> : ChannelFactory, IChannelFactory<TChannel>
    {
        InstanceContext callbackInstance;
        Type channelType;
        TypeLoader typeLoader;
        Type callbackType;

        //Overload for activation DuplexChannelFactory
        protected ChannelFactory(Type channelType)
            : base()
        {
            if (channelType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelType");
            }

            if (!channelType.IsInterface)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryTypeMustBeInterface)));
            }

            this.channelType = channelType;
        }

        // TChannel provides ContractDescription
        public ChannelFactory()
            : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, typeof(TChannel).FullName), ActivityType.Construct);
                }
                this.InitializeEndpoint((string)null, null);
            }
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Address,Binding
        public ChannelFactory(string endpointConfigurationName)
            : this(endpointConfigurationName, null)
        {
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Binding, provide Address explicitly
        public ChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress)
            : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, typeof(TChannel).FullName), ActivityType.Construct);
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }

                this.InitializeEndpoint(endpointConfigurationName, remoteAddress);
            }
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Address,Binding
        public ChannelFactory(Binding binding)
            : this(binding, (EndpointAddress)null)
        {
        }

        public ChannelFactory(Binding binding, String remoteAddress)
            : this(binding, new EndpointAddress(remoteAddress))
        {
        }

        // TChannel provides ContractDescription, provide Address,Binding explicitly
        public ChannelFactory(Binding binding, EndpointAddress remoteAddress)
            : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, typeof(TChannel).FullName), ActivityType.Construct);
                }
                if (binding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
                }

                this.InitializeEndpoint(binding, remoteAddress);
            }
        }

        // provide ContractDescription,Address,Binding explicitly
        public ChannelFactory(ServiceEndpoint endpoint)
            : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, typeof(TChannel).FullName), ActivityType.Construct);
                }
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }

                this.InitializeEndpoint(endpoint);
            }
        }

        internal InstanceContext CallbackInstance
        {
            get { return this.callbackInstance; }
            set { this.callbackInstance = value; }
        }

        internal Type CallbackType
        {
            get { return this.callbackType; }
            set { this.callbackType = value; }
        }

        internal ServiceChannelFactory ServiceChannelFactory
        {
            get { return (ServiceChannelFactory)InnerFactory; }
        }

        internal TypeLoader TypeLoader
        {
            get
            {
                if (this.typeLoader == null)
                {
                    this.typeLoader = new TypeLoader();
                }

                return this.typeLoader;
            }
        }

        internal override string CloseActivityName
        {
            get { return SR.GetString(SR.ActivityCloseChannelFactory, typeof(TChannel).FullName); }
        }

        internal override string OpenActivityName
        {
            get { return SR.GetString(SR.ActivityOpenChannelFactory, typeof(TChannel).FullName); }
        }

        internal override ActivityType OpenActivityType
        {
            get { return ActivityType.OpenClient; }
        }

        public TChannel CreateChannel(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            return CreateChannel(address, address.Uri);
        }

        public virtual TChannel CreateChannel(EndpointAddress address, Uri via)
        {
            bool traceOpenAndClose = this.TraceOpenAndClose;
            try
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose ? ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, this.OpenActivityName, this.OpenActivityType);
                        // Turn open and close off for this open on contained objects.
                        this.TraceOpenAndClose = false;
                    }
                    if (address == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
                    }

                    if (this.HasDuplexOperations())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCreateNonDuplexChannel1, this.Endpoint.Contract.Name)));
                    }

                    EnsureOpened();
                    return (TChannel)this.ServiceChannelFactory.CreateChannel(typeof(TChannel), address, via);
                }
            }
            finally
            {
                this.TraceOpenAndClose = traceOpenAndClose;
            }
        }

        public TChannel CreateChannel()
        {
            return CreateChannel(this.CreateEndpointAddress(this.Endpoint), null);
        }

        public TChannel CreateChannelWithIssuedToken(SecurityToken issuedToken)
        {
            TChannel channel = this.CreateChannel();
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.IssuedSecurityToken = issuedToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithIssuedToken(SecurityToken issuedToken, EndpointAddress address)
        {
            TChannel channel = this.CreateChannel(address);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.IssuedSecurityToken = issuedToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithIssuedToken(SecurityToken issuedToken, EndpointAddress address, Uri via)
        {
            TChannel channel = this.CreateChannel(address, via);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.IssuedSecurityToken = issuedToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithActAsToken(SecurityToken actAsToken)
        {
            TChannel channel = this.CreateChannel();
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.ActAs = actAsToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithActAsToken(SecurityToken actAsToken, EndpointAddress address)
        {
            TChannel channel = this.CreateChannel(address);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.ActAs = actAsToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithActAsToken(SecurityToken actAsToken, EndpointAddress address, Uri via)
        {
            TChannel channel = this.CreateChannel(address, via);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.ActAs = actAsToken;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithOnBehalfOfToken(SecurityToken onBehalfOf)
        {
            TChannel channel = this.CreateChannel();
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.OnBehalfOf = onBehalfOf;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithOnBehalfOfToken(SecurityToken onBehalfOf, EndpointAddress address)
        {
            TChannel channel = this.CreateChannel(address);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.OnBehalfOf = onBehalfOf;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        public TChannel CreateChannelWithOnBehalfOfToken(SecurityToken onBehalfOf, EndpointAddress address, Uri via)
        {
            TChannel channel = this.CreateChannel(address, via);
            FederatedClientCredentialsParameters parameters = new FederatedClientCredentialsParameters();
            parameters.OnBehalfOf = onBehalfOf;
            ((IChannel)channel).GetProperty<ChannelParameterCollection>().Add(parameters);
            return channel;
        }

        internal UChannel CreateChannel<UChannel>(EndpointAddress address)
        {
            EnsureOpened();
            return this.ServiceChannelFactory.CreateChannel<UChannel>(address);
        }

        internal UChannel CreateChannel<UChannel>(EndpointAddress address, Uri via)
        {
            EnsureOpened();
            return this.ServiceChannelFactory.CreateChannel<UChannel>(address, via);
        }

        internal bool CanCreateChannel<UChannel>()
        {
            EnsureOpened();
            return this.ServiceChannelFactory.CanCreateChannel<UChannel>();
        }

        protected override ServiceEndpoint CreateDescription()
        {
            ContractDescription contractDescription = this.TypeLoader.LoadContractDescription(this.channelType);

            ServiceEndpoint endpoint = new ServiceEndpoint(contractDescription);
            ReflectOnCallbackInstance(endpoint);
            this.TypeLoader.AddBehaviorsSFx(endpoint, channelType);

            return endpoint;
        }

        void ReflectOnCallbackInstance(ServiceEndpoint endpoint)
        {
            if (callbackType != null)
            {
                if (endpoint.Contract.CallbackContractType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxCallbackTypeCannotBeNull, endpoint.Contract.ContractType.FullName)));
                }

                this.TypeLoader.AddBehaviorsFromImplementationType(endpoint, callbackType);
            }
            else if (this.CallbackInstance != null && this.CallbackInstance.UserObject != null)
            {
                if (endpoint.Contract.CallbackContractType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxCallbackTypeCannotBeNull, endpoint.Contract.ContractType.FullName)));
                }

                object implementation = this.CallbackInstance.UserObject;
                Type implementationType = implementation.GetType();

                this.TypeLoader.AddBehaviorsFromImplementationType(endpoint, implementationType);

                IEndpointBehavior channelBehavior = implementation as IEndpointBehavior;
                if (channelBehavior != null)
                {
                    endpoint.Behaviors.Add(channelBehavior);
                }
                IContractBehavior contractBehavior = implementation as IContractBehavior;
                if (contractBehavior != null)
                {
                    endpoint.Contract.Behaviors.Add(contractBehavior);
                }
            }
        }

        //Static funtions to create channels
        protected static TChannel CreateChannel(String endpointConfigurationName)
        {
            ChannelFactory<TChannel> channelFactory = new ChannelFactory<TChannel>(endpointConfigurationName);

            if (channelFactory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStaticOverloadCalledForDuplexChannelFactory1, channelFactory.channelType.Name)));
            }

            TChannel channel = channelFactory.CreateChannel();
            SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(Binding binding, EndpointAddress endpointAddress)
        {
            ChannelFactory<TChannel> channelFactory = new ChannelFactory<TChannel>(binding, endpointAddress);

            if (channelFactory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStaticOverloadCalledForDuplexChannelFactory1, channelFactory.channelType.Name)));
            }

            TChannel channel = channelFactory.CreateChannel();
            SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            ChannelFactory<TChannel> channelFactory = new ChannelFactory<TChannel>(binding);

            if (channelFactory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidStaticOverloadCalledForDuplexChannelFactory1, channelFactory.channelType.Name)));
            }

            TChannel channel = channelFactory.CreateChannel(endpointAddress, via);
            SetFactoryToAutoClose(channel);
            return channel;
        }

        internal static void SetFactoryToAutoClose(TChannel channel)
        {
            //Set the Channel to auto close its ChannelFactory.
            ServiceChannel serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            serviceChannel.CloseFactory = true;
        }
    }
}
