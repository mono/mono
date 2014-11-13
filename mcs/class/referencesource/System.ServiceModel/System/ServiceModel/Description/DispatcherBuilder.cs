//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.MsmqIntegration;
    using System.ServiceModel.Security;
    using System.Xml;

    class DispatcherBuilder
    {
        // get Contract info msmq integration needs, and put in it's BE
        // also update Contract
        static void AddMsmqIntegrationContractInformation(ServiceEndpoint endpoint)
        {
            MsmqIntegrationBinding binding = endpoint.Binding as MsmqIntegrationBinding;
            if (null != binding)
            {
                Type[] types = ProcessDescriptionForMsmqIntegration(endpoint, binding.TargetSerializationTypes);
                binding.TargetSerializationTypes = types;
            }
            else
            {
                CustomBinding customBinding = endpoint.Binding as CustomBinding;
                if (null != customBinding)
                {
                    MsmqIntegrationBindingElement element = customBinding.Elements.Find<MsmqIntegrationBindingElement>();
                    if (null != element)
                    {
                        Type[] types = ProcessDescriptionForMsmqIntegration(endpoint, element.TargetSerializationTypes);
                        element.TargetSerializationTypes = types;
                    }
                }
            }
        }

        static Type[] ProcessDescriptionForMsmqIntegration(ServiceEndpoint endpoint, Type[] existingSerializationTypes)
        {
            List<Type> targetSerializationTypes;
            if (existingSerializationTypes == null)
            {
                targetSerializationTypes = new List<Type>();
            }
            else
            {
                targetSerializationTypes = new List<Type>(existingSerializationTypes);
            }

            foreach (OperationDescription operationDesc in endpoint.Contract.Operations)
            {
                foreach (Type type in operationDesc.KnownTypes)
                {
                    // add contract known types to targetSerializationTypes
                    if (!targetSerializationTypes.Contains(type))
                    {
                        targetSerializationTypes.Add(type);
                    }
                }
                // Default document style doesn't work for Integration Transport
                // because messages that SFx layer deals with are not wrapped
                // We need to change style for each operation
                foreach (MessageDescription messageDescription in operationDesc.Messages)
                {
                    messageDescription.Body.WrapperName = messageDescription.Body.WrapperNamespace = null;
                }
            }
            return targetSerializationTypes.ToArray();
        }

        internal static ClientRuntime BuildProxyBehavior(ServiceEndpoint serviceEndpoint, out BindingParameterCollection parameters)
        {
            parameters = new BindingParameterCollection();
            SecurityContractInformationEndpointBehavior.ClientInstance.AddBindingParameters(serviceEndpoint, parameters);

            AddBindingParameters(serviceEndpoint, parameters);

            ContractDescription contractDescription = serviceEndpoint.Contract;
            ClientRuntime clientRuntime = new ClientRuntime(contractDescription.Name, contractDescription.Namespace);
            clientRuntime.ContractClientType = contractDescription.ContractType;

            IdentityVerifier identityVerifier = serviceEndpoint.Binding.GetProperty<IdentityVerifier>(parameters);
            if (identityVerifier != null)
            {
                clientRuntime.IdentityVerifier = identityVerifier;
            }

            for (int i = 0; i < contractDescription.Operations.Count; i++)
            {
                OperationDescription operation = contractDescription.Operations[i];

                if (!operation.IsServerInitiated())
                {
                    DispatcherBuilder.BuildProxyOperation(operation, clientRuntime);
                }
                else
                {
                    DispatcherBuilder.BuildDispatchOperation(operation, clientRuntime.CallbackDispatchRuntime, null);
                }
            }

            DispatcherBuilder.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            return clientRuntime;
        }

        class EndpointInfo
        {
            ServiceEndpoint endpoint;
            EndpointDispatcher endpointDispatcher;
            EndpointFilterProvider provider;

            public EndpointInfo(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher, EndpointFilterProvider provider)
            {
                this.endpoint = endpoint;
                this.endpointDispatcher = endpointDispatcher;
                this.provider = provider;
            }
            public ServiceEndpoint Endpoint { get { return this.endpoint; } }
            public EndpointFilterProvider FilterProvider { get { return this.provider; } }
            public EndpointDispatcher EndpointDispatcher { get { return this.endpointDispatcher; } }
        }

        internal class ListenUriInfo
        {
            Uri listenUri;
            ListenUriMode listenUriMode;

            public ListenUriInfo(Uri listenUri, ListenUriMode listenUriMode)
            {
                this.listenUri = listenUri;
                this.listenUriMode = listenUriMode;
            }

            public Uri ListenUri
            {
                get { return this.listenUri; }
            }

            public ListenUriMode ListenUriMode
            {
                get { return this.listenUriMode; }
            }

            // implement Equals and GetHashCode so that we can use this as a key in a dictionary
            public override bool Equals(Object other)
            {
                return this.Equals(other as ListenUriInfo);
            }

            public bool Equals(ListenUriInfo other)
            {
                if (other == null)
                {
                    return false;
                }

                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }

                return (this.listenUriMode == other.listenUriMode)
                    && EndpointAddress.UriEquals(this.listenUri, other.listenUri, true /* ignoreCase */, true /* includeHost */);
            }

            public override int GetHashCode()
            {
                return EndpointAddress.UriGetHashCode(this.listenUri, true /* includeHost */);
            }
        }

        class StuffPerListenUriInfo
        {
            public BindingParameterCollection Parameters = new BindingParameterCollection();
            public Collection<ServiceEndpoint> Endpoints = new Collection<ServiceEndpoint>();
            public ChannelDispatcher ChannelDispatcher = null;
        }

        void ValidateDescription(ServiceDescription description, ServiceHostBase serviceHost)
        {
            description.EnsureInvariants();
            (PartialTrustValidationBehavior.Instance as IServiceBehavior).Validate(description, serviceHost);
#pragma warning disable 0618
            (PeerValidationBehavior.Instance as IServiceBehavior).Validate(description, serviceHost);
#pragma warning restore 0618
            (TransactionValidationBehavior.Instance as IServiceBehavior).Validate(description, serviceHost);
            (System.ServiceModel.MsmqIntegration.MsmqIntegrationValidationBehavior.Instance as IServiceBehavior).Validate(description, serviceHost);
            (SecurityValidationBehavior.Instance as IServiceBehavior).Validate(description, serviceHost);
            (new UniqueContractNameValidationBehavior() as IServiceBehavior).Validate(description, serviceHost);
            for (int i = 0; i < description.Behaviors.Count; i++)
            {
                IServiceBehavior iServiceBehavior = description.Behaviors[i];
                iServiceBehavior.Validate(description, serviceHost);
            }
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                ContractDescription contract = endpoint.Contract;
                bool alreadyProcessedThisContract = false;
                for (int j = 0; j < i; j++)
                {
                    if (description.Endpoints[j].Contract == contract)
                    {
                        alreadyProcessedThisContract = true;
                        break;
                    }
                }
                endpoint.ValidateForService(!alreadyProcessedThisContract);
            }
        }

        static void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
            foreach (IContractBehavior icb in endpoint.Contract.Behaviors)
            {
                icb.AddBindingParameters(endpoint.Contract, endpoint, parameters);
            }
            foreach (IEndpointBehavior ieb in endpoint.Behaviors)
            {
                ieb.AddBindingParameters(endpoint, parameters);
            }
            foreach (OperationDescription op in endpoint.Contract.Operations)
            {
                foreach (IOperationBehavior iob in op.Behaviors)
                {
                    iob.AddBindingParameters(op, parameters);
                }
            }
        }

        Type BuildChannelListener(StuffPerListenUriInfo stuff,
                                  ServiceHostBase serviceHost,
                                  Uri listenUri,
                                  ListenUriMode listenUriMode,
                                  bool supportContextSession,
                                  out IChannelListener result)
        {
            Binding originalBinding = stuff.Endpoints[0].Binding;
            CustomBinding binding = new CustomBinding(originalBinding);
            BindingParameterCollection parameters = stuff.Parameters;

            Uri listenUriBaseAddress;
            string listenUriRelativeAddress;
            GetBaseAndRelativeAddresses(serviceHost, listenUri, binding.Scheme, out listenUriBaseAddress, out listenUriRelativeAddress);

            InternalDuplexBindingElement internalDuplex = null;
            InternalDuplexBindingElement.AddDuplexListenerSupport(binding, ref internalDuplex);

            // All types are supported to start
            bool reply = true;
            bool replySession = true;
            bool input = true;
            bool inputSession = true;
            bool duplex = true;
            bool duplexSession = true;
            string sessionContractName = null;
            string datagramContractName = null;
            // each endpoint adds constraints
            for (int i = 0; i < stuff.Endpoints.Count; ++i)
            {
                ContractDescription contract = stuff.Endpoints[i].Contract;
                if (contract.SessionMode == SessionMode.Required)
                {
                    sessionContractName = contract.Name;
                }
                if (contract.SessionMode == SessionMode.NotAllowed)
                {
                    datagramContractName = contract.Name;
                }

                System.Collections.IList endpointTypes = GetSupportedChannelTypes(contract);
                if (!endpointTypes.Contains(typeof(IReplyChannel)))
                {
                    reply = false;
                }
                if (!endpointTypes.Contains(typeof(IReplySessionChannel)))
                {
                    replySession = false;
                }
                if (!endpointTypes.Contains(typeof(IInputChannel)))
                {
                    input = false;
                }
                if (!endpointTypes.Contains(typeof(IInputSessionChannel)))
                {
                    inputSession = false;
                }
                if (!endpointTypes.Contains(typeof(IDuplexChannel)))
                {
                    duplex = false;
                }
                if (!endpointTypes.Contains(typeof(IDuplexSessionChannel)))
                {
                    duplexSession = false;
                }
            }

            if ((sessionContractName != null) && (datagramContractName != null))
            {
                string text = SR.GetString(SR.SFxCannotRequireBothSessionAndDatagram3, datagramContractName, sessionContractName, binding.Name);
                Exception error = new InvalidOperationException(text);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }

            List<Type> supportedChannelTypes = new List<Type>();
            if (input)
            {
                supportedChannelTypes.Add(typeof(IInputChannel));
            }
            if (inputSession)
            {
                supportedChannelTypes.Add(typeof(IInputSessionChannel));
            }
            if (reply)
            {
                supportedChannelTypes.Add(typeof(IReplyChannel));
            }
            if (replySession)
            {
                supportedChannelTypes.Add(typeof(IReplySessionChannel));
            }
            if (duplex)
            {
                supportedChannelTypes.Add(typeof(IDuplexChannel));
            }
            if (duplexSession)
            {
                supportedChannelTypes.Add(typeof(IDuplexSessionChannel));
            }
            // now we know what channel types we can use to support the contracts at this ListenUri
            Type returnValue = DispatcherBuilder.MaybeCreateListener(true, supportedChannelTypes.ToArray(), binding, parameters,
                                                                     listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, serviceHost.ServiceThrottle, out result,
                                                                     supportContextSession && sessionContractName != null);
            if (result == null)
            {
                // we put a lot of work into creating a good error message, as this is a common case
                Dictionary<Type, byte> setOfChannelTypesSupportedByBinding = new Dictionary<Type, byte>();
                if (binding.CanBuildChannelListener<IInputChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IInputChannel), 0);
                }
                if (binding.CanBuildChannelListener<IReplyChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IReplyChannel), 0);
                }
                if (binding.CanBuildChannelListener<IDuplexChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IDuplexChannel), 0);
                }
                if (binding.CanBuildChannelListener<IInputSessionChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IInputSessionChannel), 0);
                }
                if (binding.CanBuildChannelListener<IReplySessionChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IReplySessionChannel), 0);
                }
                if (binding.CanBuildChannelListener<IDuplexSessionChannel>())
                {
                    setOfChannelTypesSupportedByBinding.Add(typeof(IDuplexSessionChannel), 0);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ChannelRequirements.CantCreateListenerException(
                                                                              setOfChannelTypesSupportedByBinding.Keys, supportedChannelTypes, originalBinding.Name));
            }
            return returnValue;
        }

        static internal Type MaybeCreateListener(bool actuallyCreate, Type[] supportedChannels,
                                                 Binding binding, BindingParameterCollection parameters,
                                                 Uri listenUriBaseAddress, string listenUriRelativeAddress,
                                                 ListenUriMode listenUriMode, ServiceThrottle throttle,
                                                 out IChannelListener result)
        {
            return MaybeCreateListener(actuallyCreate, supportedChannels, binding, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, throttle,
                out result, false);
        }

        static Type MaybeCreateListener(bool actuallyCreate, Type[] supportedChannels,
                                                 Binding binding, BindingParameterCollection parameters,
                                                 Uri listenUriBaseAddress, string listenUriRelativeAddress,
                                                 ListenUriMode listenUriMode, ServiceThrottle throttle,
                                                 out IChannelListener result, bool supportContextSession)
        {
            // if actuallyCreate is true, then this behaves like CreateListener()
            // else this behaves like CanCreateListener()
            // result is channel type that was (would be) created, null if can't create
            // 
            // Ugly API helps refactor common code in these two similar-but-different methods

            result = null;

            for (int i = 0; i < supportedChannels.Length; i++)
            {
                Type channelType = supportedChannels[i];

                if (channelType == typeof(IInputChannel))
                {
                    if (binding.CanBuildChannelListener<IInputChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IInputChannel);
                    }
                }
                if (channelType == typeof(IReplyChannel))
                {
                    if (binding.CanBuildChannelListener<IReplyChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IReplyChannel);
                    }
                }
                if (channelType == typeof(IDuplexChannel))
                {
                    if (binding.CanBuildChannelListener<IDuplexChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IDuplexChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IDuplexChannel);
                    }
                }
                if (channelType == typeof(IInputSessionChannel))
                {
                    if (binding.CanBuildChannelListener<IInputSessionChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IInputSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IInputSessionChannel);
                    }
                }
                if (channelType == typeof(IReplySessionChannel))
                {
                    if (binding.CanBuildChannelListener<IReplySessionChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IReplySessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IReplySessionChannel);
                    }
                }
                if (channelType == typeof(IDuplexSessionChannel))
                {
                    if (binding.CanBuildChannelListener<IDuplexSessionChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            result = binding.BuildChannelListener<IDuplexSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        }
                        return typeof(IDuplexSessionChannel);
                    }
                }
            }

            // If the binding does not support the type natively, try to adapt
            for (int i = 0; i < supportedChannels.Length; i++)
            {
                Type channelType = supportedChannels[i];

                // For SessionMode.Allowed or SessionMode.NotAllowed we will accept session-ful variants as well and adapt them
                if (channelType == typeof(IInputChannel))
                {
                    if (binding.CanBuildChannelListener<IInputSessionChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            IChannelListener<IInputSessionChannel> temp = binding.BuildChannelListener<IInputSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                            result = DatagramAdapter.GetInputListener(temp, throttle, binding);
                        }
                        return typeof(IInputSessionChannel);
                    }
                }

                if (channelType == typeof(IReplyChannel))
                {
                    if (binding.CanBuildChannelListener<IReplySessionChannel>(parameters))
                    {
                        if (actuallyCreate)
                        {
                            IChannelListener<IReplySessionChannel> temp = binding.BuildChannelListener<IReplySessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                            result = DatagramAdapter.GetReplyListener(temp, throttle, binding);
                        }
                        return typeof(IReplySessionChannel);
                    }
                }

                if (supportContextSession)
                {
                    // and for SessionMode.Required, it is possible that the InstanceContextProvider is handling the session management, so 
                    // accept datagram variants if that is the case
                    if (channelType == typeof(IReplySessionChannel))
                    {
                        if (binding.CanBuildChannelListener<IReplyChannel>(parameters)
                            && binding.GetProperty<IContextSessionProvider>(parameters) != null)
                        {
                            if (actuallyCreate)
                            {
                                result = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                            }
                            return typeof(IReplyChannel);
                        }
                    }
                }
            }

            return null;
        }

        void EnsureThereAreApplicationEndpoints(ServiceDescription description)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (!endpoint.InternalIsSystemEndpoint(description))
                {
                    return;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                                                          SR.GetString(SR.ServiceHasZeroAppEndpoints, description.ConfigurationName)));
        }

        static Uri EnsureListenUri(ServiceHostBase serviceHost, ServiceEndpoint endpoint)
        {
            Uri listenUri = endpoint.ListenUri;
            if (listenUri == null)
            {
                listenUri = serviceHost.GetVia(endpoint.Binding.Scheme, ServiceHost.EmptyUri);
            }
            if (listenUri == null)
            {
                AspNetEnvironment.Current.ProcessNotMatchedEndpointAddress(listenUri, endpoint.Binding.Name);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxEndpointNoMatchingScheme, endpoint.Binding.Scheme, endpoint.Binding.Name, serviceHost.GetBaseAddressSchemes())));
            }
            return listenUri;
        }

        void GetBaseAndRelativeAddresses(ServiceHostBase serviceHost, Uri listenUri, string scheme, out Uri listenUriBaseAddress, out string listenUriRelativeAddress)
        {
            // set the ListenUri (old EndpointListener EnsureListenUri() logic)
            listenUriBaseAddress = listenUri;
            listenUriRelativeAddress = String.Empty;

            if (serviceHost.InternalBaseAddresses.Contains(scheme))
            {
                Uri baseAddress = serviceHost.InternalBaseAddresses[scheme];
                if (!baseAddress.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
                {
                    baseAddress = new Uri(baseAddress.AbsoluteUri + "/");
                }
                string baseAddressString = baseAddress.ToString();
                string thisAddressString = listenUri.ToString();
                if (thisAddressString.StartsWith(baseAddressString, StringComparison.OrdinalIgnoreCase))
                {
                    listenUriBaseAddress = baseAddress;
                    listenUriRelativeAddress = thisAddressString.Substring(baseAddressString.Length);
                }
            }
        }

        void InitializeServicePerformanceCounters(ServiceHostBase serviceHost)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                ServicePerformanceCountersBase tempCounters = PerformanceCountersFactory.CreateServiceCounters(serviceHost);
                if (tempCounters != null && tempCounters.Initialized)
                {
                    serviceHost.Counters = tempCounters;
                }
            }
            // Some perf. counters are enabled by default
            else if (PerformanceCounters.MinimalPerformanceCountersEnabled)
            {
                DefaultPerformanceCounters tempCounters = new DefaultPerformanceCounters(serviceHost);
                if (tempCounters.Initialized)
                {
                    serviceHost.DefaultCounters = tempCounters;
                }
            }
        }

        //This method generates the BindingParameterCollection in the same way it is created during DispatcherBuilder.InitializeServiceHost
        internal static BindingParameterCollection GetBindingParameters(ServiceHostBase serviceHost, Collection<ServiceEndpoint> endpoints)
        {
            BindingParameterCollection parameters = new BindingParameterCollection();
            parameters.Add(new ThreadSafeMessageFilterTable<EndpointAddress>());

            foreach (IServiceBehavior behavior in serviceHost.Description.Behaviors)
            {
                behavior.AddBindingParameters(serviceHost.Description, serviceHost, endpoints, parameters);
            }

            foreach (ServiceEndpoint endpoint in endpoints)
            {
                DispatcherBuilder.SecurityContractInformationEndpointBehavior.ServerInstance.AddBindingParameters(endpoint, parameters);
                DispatcherBuilder.AddBindingParameters(endpoint, parameters);
            }

            return parameters;
        }

        internal static ListenUriInfo GetListenUriInfoForEndpoint(ServiceHostBase host, ServiceEndpoint endpoint)
        {
            Uri listenUri = EnsureListenUri(host, endpoint);
            return new ListenUriInfo(listenUri, endpoint.ListenUriMode);
        }

        public void InitializeServiceHost(ServiceDescription description, ServiceHostBase serviceHost)
        {
            if (serviceHost.ImplementedContracts != null && serviceHost.ImplementedContracts.Count > 0)
            {
                EnsureThereAreApplicationEndpoints(description);
            }
            ValidateDescription(description, serviceHost);

            AspNetEnvironment.Current.AddHostingBehavior(serviceHost, description);

            ServiceBehaviorAttribute instanceSettings = description.Behaviors.Find<ServiceBehaviorAttribute>();
            InitializeServicePerformanceCounters(serviceHost);

            Dictionary<ListenUriInfo, StuffPerListenUriInfo> stuffPerListenUriInfo
                = new Dictionary<ListenUriInfo, StuffPerListenUriInfo>();
            Dictionary<EndpointAddress, Collection<EndpointInfo>> endpointInfosPerEndpointAddress
                = new Dictionary<EndpointAddress, Collection<EndpointInfo>>();

            // Ensure ListenUri and group endpoints per ListenUri
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                //Ensure ReceiveContextSettings before building channel
                bool requiresReceiveContext = false; //at least one operation had ReceiveContextEnabledAttribute
                ServiceEndpoint endpoint = description.Endpoints[i];

                foreach (OperationDescription operation in endpoint.Contract.Operations)
                {
                    if (operation.Behaviors.Find<ReceiveContextEnabledAttribute>() != null)
                    {
                        requiresReceiveContext = true;
                        break;
                    }
                }

                if (requiresReceiveContext)
                {
                    IReceiveContextSettings receiveContextSettings = endpoint.Binding.GetProperty<IReceiveContextSettings>(new BindingParameterCollection());

                    if (receiveContextSettings == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR.GetString(SR.SFxReceiveContextSettingsPropertyMissing,
                            endpoint.Contract.Name,
                            typeof(ReceiveContextEnabledAttribute).Name,
                            endpoint.Address.Uri.AbsoluteUri,
                            typeof(IReceiveContextSettings).Name)));
                    }
                    //Enable ReceiveContext on the binding.
                    receiveContextSettings.Enabled = true;
                }

                ListenUriInfo listenUriInfo = GetListenUriInfoForEndpoint(serviceHost, endpoint);
                if (!stuffPerListenUriInfo.ContainsKey(listenUriInfo))
                {
                    stuffPerListenUriInfo.Add(listenUriInfo, new StuffPerListenUriInfo());
                }
                stuffPerListenUriInfo[listenUriInfo].Endpoints.Add(endpoint);
            }

            foreach (KeyValuePair<ListenUriInfo, StuffPerListenUriInfo> stuff in stuffPerListenUriInfo)
            {
                Uri listenUri = stuff.Key.ListenUri;
                ListenUriMode listenUriMode = stuff.Key.ListenUriMode;
                BindingParameterCollection parameters = stuff.Value.Parameters;
                Binding binding = stuff.Value.Endpoints[0].Binding;
                EndpointIdentity identity = stuff.Value.Endpoints[0].Address.Identity;
                // same EndpointAddressTable instance must be shared between channelDispatcher and parameters
                ThreadSafeMessageFilterTable<EndpointAddress> endpointAddressTable = new ThreadSafeMessageFilterTable<EndpointAddress>();
                parameters.Add(endpointAddressTable);

                bool supportContextSession = false;
                // add service-level binding parameters
                foreach (IServiceBehavior behavior in description.Behaviors)
                {
                    if (behavior is IContextSessionProvider)
                    {
                        supportContextSession = true;
                    }
                    behavior.AddBindingParameters(description, serviceHost, stuff.Value.Endpoints, parameters);
                }
                for (int i = 0; i < stuff.Value.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = stuff.Value.Endpoints[i];
                    string viaString = listenUri.AbsoluteUri;

                    // ensure all endpoints with this ListenUriInfo have same binding
                    if (endpoint.Binding != binding)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ABindingInstanceHasAlreadyBeenAssociatedTo1, viaString)));
                    }

                    // ensure all endpoints with this ListenUriInfo have same identity
                    if (!object.Equals(endpoint.Address.Identity, identity))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                                                                      SR.GetString(SR.SFxWhenMultipleEndpointsShareAListenUriTheyMustHaveSameIdentity, viaString)));
                    }

                    // add binding parameters (endpoint scope and below)
                    AddMsmqIntegrationContractInformation(endpoint);
                    SecurityContractInformationEndpointBehavior.ServerInstance.AddBindingParameters(endpoint, parameters);
                    AddBindingParameters(endpoint, parameters);
                }

                // build IChannelListener and ChannelDispatcher
                IChannelListener listener;
                Type channelType = this.BuildChannelListener(stuff.Value,
                                                             serviceHost,
                                                             listenUri,
                                                             listenUriMode,
                                                             supportContextSession,
                                                             out listener);

                XmlQualifiedName bindingQname = new XmlQualifiedName(binding.Name, binding.Namespace);
                ChannelDispatcher channelDispatcher = new ChannelDispatcher(listener, bindingQname.ToString(), binding);
                channelDispatcher.SetEndpointAddressTable(endpointAddressTable);
                stuff.Value.ChannelDispatcher = channelDispatcher;

                bool canReceiveInTransaction = false;   // at least one operation is TransactionScopeRequired
                int transactedBatchSize = int.MaxValue;

                for (int i = 0; i < stuff.Value.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = stuff.Value.Endpoints[i];
                    string viaString = listenUri.AbsoluteUri;

                    EndpointFilterProvider provider = new EndpointFilterProvider();
                    EndpointDispatcher dispatcher = DispatcherBuilder.BuildDispatcher(serviceHost, description, endpoint, endpoint.Contract, provider);

                    for (int j = 0; j < endpoint.Contract.Operations.Count; j++)
                    {
                        OperationDescription operation = endpoint.Contract.Operations[j];
                        OperationBehaviorAttribute operationBehavior = operation.Behaviors.Find<OperationBehaviorAttribute>();
                        if (null != operationBehavior && operationBehavior.TransactionScopeRequired)
                        {
                            canReceiveInTransaction = true;
                            break;
                        }
                    }

                    if (!endpointInfosPerEndpointAddress.ContainsKey(endpoint.Address))
                    {
                        endpointInfosPerEndpointAddress.Add(endpoint.Address, new Collection<EndpointInfo>());
                    }
                    endpointInfosPerEndpointAddress[endpoint.Address].Add(new EndpointInfo(endpoint, dispatcher, provider));

                    channelDispatcher.Endpoints.Add(dispatcher);

                    TransactedBatchingBehavior batchBehavior = endpoint.Behaviors.Find<TransactedBatchingBehavior>();
                    if (batchBehavior == null)
                    {
                        transactedBatchSize = 0;
                    }
                    else
                    {
                        if (!canReceiveInTransaction)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqBatchRequiresTransactionScope)));
                        transactedBatchSize = System.Math.Min(transactedBatchSize, batchBehavior.MaxBatchSize);
                    }
                    if (PerformanceCounters.PerformanceCountersEnabled || PerformanceCounters.MinimalPerformanceCountersEnabled)
                    {
                        PerformanceCounters.AddPerformanceCountersForEndpoint(serviceHost, endpoint.Contract, dispatcher);
                    }
                } // end foreach "endpoint"

                if (canReceiveInTransaction)
                {
                    BindingElementCollection bindingElements = binding.CreateBindingElements();
                    foreach (BindingElement bindingElement in bindingElements)
                    {
                        ITransactedBindingElement txElement = bindingElement as ITransactedBindingElement;
                        if (null != txElement && txElement.TransactedReceiveEnabled)
                        {
                            channelDispatcher.IsTransactedReceive = true;
                            channelDispatcher.MaxTransactedBatchSize = transactedBatchSize;
                            break;
                        }
                    }
                }

                //Set the mode of operation for ChannelDispatcher based on binding Settings.
                IReceiveContextSettings receiveContextSettings = binding.GetProperty<IReceiveContextSettings>(new BindingParameterCollection());

                if (receiveContextSettings != null)
                {
                    channelDispatcher.ReceiveContextEnabled = receiveContextSettings.Enabled;
                }
                serviceHost.ChannelDispatchers.Add(channelDispatcher);
            } // end foreach "ListenUri/ChannelDispatcher" group

            // run service behaviors
            for (int i = 0; i < description.Behaviors.Count; i++)
            {
                IServiceBehavior serviceBehavior = description.Behaviors[i];
                serviceBehavior.ApplyDispatchBehavior(description, serviceHost);
            }

            foreach (KeyValuePair<ListenUriInfo, StuffPerListenUriInfo> stuff in stuffPerListenUriInfo)
            {
                for (int i = 0; i < stuff.Value.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = stuff.Value.Endpoints[i];
                    // rediscover which dispatcher goes with this endpoint
                    Collection<EndpointInfo> infos = endpointInfosPerEndpointAddress[endpoint.Address];
                    EndpointInfo info = null;
                    foreach (EndpointInfo ei in infos)
                    {
                        if (ei.Endpoint == endpoint)
                        {
                            info = ei;
                            break;
                        }
                    }
                    EndpointDispatcher dispatcher = info.EndpointDispatcher;
                    // run contract behaviors
                    for (int k = 0; k < endpoint.Contract.Behaviors.Count; k++)
                    {
                        IContractBehavior behavior = endpoint.Contract.Behaviors[k];
                        behavior.ApplyDispatchBehavior(endpoint.Contract, endpoint, dispatcher.DispatchRuntime);
                    }
                    // run endpoint behaviors
                    BindingInformationEndpointBehavior.Instance.ApplyDispatchBehavior(endpoint, dispatcher);
                    TransactionContractInformationEndpointBehavior.Instance.ApplyDispatchBehavior(endpoint, dispatcher);
                    for (int j = 0; j < endpoint.Behaviors.Count; j++)
                    {
                        IEndpointBehavior eb = endpoint.Behaviors[j];
                        eb.ApplyDispatchBehavior(endpoint, dispatcher);
                    }
                    // run operation behaviors
                    DispatcherBuilder.BindOperations(endpoint.Contract, null, dispatcher.DispatchRuntime);
                }
            }

            this.EnsureRequiredRuntimeProperties(endpointInfosPerEndpointAddress);

            // Warn about obvious demux conflicts
            foreach (Collection<EndpointInfo> endpointInfos in endpointInfosPerEndpointAddress.Values)
            {
                // all elements of endpointInfos share the same Address (and thus EndpointListener.AddressFilter)
                if (endpointInfos.Count > 1)
                {
                    for (int i = 0; i < endpointInfos.Count; i++)
                    {
                        for (int j = i + 1; j < endpointInfos.Count; j++)
                        {
                            // if not same ListenUri, won't conflict
                            // if not same ChannelType, may not conflict (some transports demux based on this)
                            // if they share a ChannelDispatcher, this means same ListenUri and same ChannelType
                            if (endpointInfos[i].EndpointDispatcher.ChannelDispatcher ==
                                endpointInfos[j].EndpointDispatcher.ChannelDispatcher)
                            {
                                EndpointFilterProvider iProvider = endpointInfos[i].FilterProvider;
                                EndpointFilterProvider jProvider = endpointInfos[j].FilterProvider;
                                // if not default EndpointFilterProvider, we won't try to throw, you're on your own
                                string commonAction;
                                if (iProvider != null && jProvider != null
                                    && HaveCommonInitiatingActions(iProvider, jProvider, out commonAction))
                                {
                                    // you will definitely get a MultipleFiltersMatchedException at runtime,
                                    // so let's go ahead and throw now
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new InvalidOperationException(
                                            SR.GetString(SR.SFxDuplicateInitiatingActionAtSameVia,
                                                         endpointInfos[i].Endpoint.ListenUri, commonAction)));
                                }
                            }
                        }
                    }
                }
            }
        }

        void EnsureRequiredRuntimeProperties(Dictionary<EndpointAddress, Collection<EndpointInfo>> endpointInfosPerEndpointAddress)
        {
            foreach (Collection<EndpointInfo> endpointInfos in endpointInfosPerEndpointAddress.Values)
            {
                for (int i = 0; i < endpointInfos.Count; i++)
                {
                    DispatchRuntime dispatch = endpointInfos[i].EndpointDispatcher.DispatchRuntime;

                    if (dispatch.InstanceContextProvider == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxRequiredRuntimePropertyMissing, "InstanceContextProvider")));
                    }
                }
            }
        }

        static EndpointDispatcher BuildDispatcher(ServiceHostBase service,
                                                  ServiceDescription serviceDescription,
                                                  ServiceEndpoint endpoint,
                                                  ContractDescription contractDescription,
                                                  EndpointFilterProvider provider)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractDescription");
            }

            EndpointAddress address = endpoint.Address;
            EndpointDispatcher dispatcher = new EndpointDispatcher(address, contractDescription.Name, contractDescription.Namespace, endpoint.Id, endpoint.InternalIsSystemEndpoint(serviceDescription));

            DispatchRuntime dispatch = dispatcher.DispatchRuntime;
            if (contractDescription.CallbackContractType != null)
            {
                dispatch.CallbackClientRuntime.CallbackClientType = contractDescription.CallbackContractType;
                dispatch.CallbackClientRuntime.ContractClientType = contractDescription.ContractType;
            }

            for (int i = 0; i < contractDescription.Operations.Count; i++)
            {
                OperationDescription operation = contractDescription.Operations[i];

                if (!operation.IsServerInitiated())
                {
                    DispatcherBuilder.BuildDispatchOperation(operation, dispatch, provider);
                }
                else
                {
                    DispatcherBuilder.BuildProxyOperation(operation, dispatch.CallbackClientRuntime);
                }
            }

            //dispatcher.SetSupportedChannels(DispatcherBuilder.GetSupportedChannelTypes(contractDescription));
            int filterPriority = 0;
            dispatcher.ContractFilter = provider.CreateFilter(out filterPriority);
            dispatcher.FilterPriority = filterPriority;

            return dispatcher;
        }

        static void BuildProxyOperation(OperationDescription operation, ClientRuntime parent)
        {
            ClientOperation child;
            if (operation.Messages.Count == 1)
            {
                child = new ClientOperation(parent, operation.Name, operation.Messages[0].Action);
            }
            else
            {
                child = new ClientOperation(parent, operation.Name, operation.Messages[0].Action,
                                            operation.Messages[1].Action);
            }
            child.TaskMethod = operation.TaskMethod;
            child.TaskTResult = operation.TaskTResult;
            child.SyncMethod = operation.SyncMethod;
            child.BeginMethod = operation.BeginMethod;
            child.EndMethod = operation.EndMethod;
            child.IsOneWay = operation.IsOneWay;
            child.IsTerminating = operation.IsTerminating;
            child.IsInitiating = operation.IsInitiating;
            child.IsSessionOpenNotificationEnabled = operation.IsSessionOpenNotificationEnabled;
            for (int i = 0; i < operation.Faults.Count; i++)
            {
                FaultDescription fault = operation.Faults[i];
                child.FaultContractInfos.Add(new FaultContractInfo(fault.Action, fault.DetailType, fault.ElementName, fault.Namespace, operation.KnownTypes));
            }

            parent.Operations.Add(child);
        }

        static void BuildDispatchOperation(OperationDescription operation, DispatchRuntime parent, EndpointFilterProvider provider)
        {
            string requestAction = operation.Messages[0].Action;
            DispatchOperation child = null;
            if (operation.IsOneWay)
            {
                child = new DispatchOperation(parent, operation.Name, requestAction);
            }
            else
            {
                string replyAction = operation.Messages[1].Action;
                child = new DispatchOperation(parent, operation.Name, requestAction, replyAction);
            }

            child.HasNoDisposableParameters = operation.HasNoDisposableParameters;

            child.IsTerminating = operation.IsTerminating;
            child.IsSessionOpenNotificationEnabled = operation.IsSessionOpenNotificationEnabled;
            for (int i = 0; i < operation.Faults.Count; i++)
            {
                FaultDescription fault = operation.Faults[i];
                child.FaultContractInfos.Add(new FaultContractInfo(fault.Action, fault.DetailType, fault.ElementName, fault.Namespace, operation.KnownTypes));
            }

            child.IsInsideTransactedReceiveScope = operation.IsInsideTransactedReceiveScope;

            if (provider != null)
            {
                if (operation.IsInitiating)
                {
                    provider.InitiatingActions.Add(requestAction);
                }
            }

            if (requestAction != MessageHeaders.WildcardAction)
            {
                parent.Operations.Add(child);
            }
            else
            {
                if (parent.HasMatchAllOperation)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMultipleContractStarOperations0)));
                }

                parent.UnhandledDispatchOperation = child;
            }

        }

        static void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            // contract behaviors
            ContractDescription contractDescription = serviceEndpoint.Contract;
            for (int i = 0; i < contractDescription.Behaviors.Count; i++)
            {
                IContractBehavior behavior = contractDescription.Behaviors[i];
                behavior.ApplyClientBehavior(contractDescription, serviceEndpoint, clientRuntime);
            }
            // endpoint behaviors
            BindingInformationEndpointBehavior.Instance.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            TransactionContractInformationEndpointBehavior.Instance.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            for (int i = 0; i < serviceEndpoint.Behaviors.Count; i++)
            {
                IEndpointBehavior behavior = serviceEndpoint.Behaviors[i];
                behavior.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            }
            // operation behaviors
            DispatcherBuilder.BindOperations(contractDescription, clientRuntime, null);
        }

        static void BindOperations(ContractDescription contract, ClientRuntime proxy, DispatchRuntime dispatch)
        {
            if (!(((proxy == null) != (dispatch == null))))
            {
                throw Fx.AssertAndThrowFatal("DispatcherBuilder.BindOperations: ((proxy == null) != (dispatch == null))");
            }

            MessageDirection local = (proxy == null) ? MessageDirection.Input : MessageDirection.Output;

            for (int i = 0; i < contract.Operations.Count; i++)
            {
                OperationDescription operation = contract.Operations[i];
                MessageDescription first = operation.Messages[0];

                if (first.Direction != local)
                {
                    if (proxy == null)
                    {
                        proxy = dispatch.CallbackClientRuntime;
                    }

                    ClientOperation proxyOperation = proxy.Operations[operation.Name];
                    Fx.Assert(proxyOperation != null, "");

                    for (int j = 0; j < operation.Behaviors.Count; j++)
                    {
                        IOperationBehavior behavior = operation.Behaviors[j];
                        behavior.ApplyClientBehavior(operation, proxyOperation);
                    }
                }
                else
                {
                    if (dispatch == null)
                    {
                        dispatch = proxy.CallbackDispatchRuntime;
                    }

                    DispatchOperation dispatchOperation = null;
                    if (dispatch.Operations.Contains(operation.Name))
                    {
                        dispatchOperation = dispatch.Operations[operation.Name];
                    }
                    if (dispatchOperation == null && dispatch.UnhandledDispatchOperation != null && dispatch.UnhandledDispatchOperation.Name == operation.Name)
                    {
                        dispatchOperation = dispatch.UnhandledDispatchOperation;
                    }

                    if (dispatchOperation != null)
                    {
                        for (int j = 0; j < operation.Behaviors.Count; j++)
                        {
                            IOperationBehavior behavior = operation.Behaviors[j];
                            behavior.ApplyDispatchBehavior(operation, dispatchOperation);
                        }
                    }
                }
            }
        }

        internal static Type[] GetSupportedChannelTypes(ContractDescription contractDescription)
        {
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractDescription"));
            }

            ChannelRequirements reqs;
            ChannelRequirements.ComputeContractRequirements(contractDescription, out reqs);
            Type[] supportedChannels = ChannelRequirements.ComputeRequiredChannels(ref reqs);
            // supportedChannels is client-side, need to make server-side
            for (int i = 0; i < supportedChannels.Length; i++)
            {
                if (supportedChannels[i] == typeof(IRequestChannel))
                {
                    supportedChannels[i] = typeof(IReplyChannel);
                }
                else if (supportedChannels[i] == typeof(IRequestSessionChannel))
                {
                    supportedChannels[i] = typeof(IReplySessionChannel);
                }
                else if (supportedChannels[i] == typeof(IOutputChannel))
                {
                    supportedChannels[i] = typeof(IInputChannel);
                }
                else if (supportedChannels[i] == typeof(IOutputSessionChannel))
                {
                    supportedChannels[i] = typeof(IInputSessionChannel);
                }
                else if (supportedChannels[i] == typeof(IDuplexChannel))
                {
                    // no-op; duplex is its own dual
                }
                else if (supportedChannels[i] == typeof(IDuplexSessionChannel))
                {
                    // no-op; duplex is its own dual
                }
                else
                {
                    throw Fx.AssertAndThrowFatal("DispatcherBuilder.GetSupportedChannelTypes: Unexpected channel type");
                }
            }

            return supportedChannels;
        }

        static bool HaveCommonInitiatingActions(EndpointFilterProvider x, EndpointFilterProvider y, out string commonAction)
        {
            commonAction = null;
            foreach (string action in x.InitiatingActions)
            {
                if (y.InitiatingActions.Contains(action))
                {
                    commonAction = action;
                    return true;
                }
            }
            return false;
        }

        class BindingInformationEndpointBehavior : IEndpointBehavior
        {
            static BindingInformationEndpointBehavior instance;
            public static BindingInformationEndpointBehavior Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new BindingInformationEndpointBehavior();
                    }
                    return instance;
                }
            }
            public void Validate(ServiceEndpoint serviceEndpoint) { }
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters) { }
            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
                behavior.ManualAddressing = this.IsManualAddressing(serviceEndpoint.Binding);
                behavior.EnableFaults = !this.IsMulticast(serviceEndpoint.Binding);
                if (serviceEndpoint.Contract.IsDuplex())
                {
                    behavior.CallbackDispatchRuntime.ChannelDispatcher.MessageVersion = serviceEndpoint.Binding.MessageVersion;
                }
            }
            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
                IBindingRuntimePreferences runtimePreferences = serviceEndpoint.Binding as IBindingRuntimePreferences;
                if (runtimePreferences != null)
                {
                    // it is ok to go up to the ChannelDispatcher here, since
                    // all endpoints that share a ChannelDispatcher also share the same binding
                    endpointDispatcher.ChannelDispatcher.ReceiveSynchronously = runtimePreferences.ReceiveSynchronously;
                }

                endpointDispatcher.ChannelDispatcher.ManualAddressing = this.IsManualAddressing(serviceEndpoint.Binding);
                endpointDispatcher.ChannelDispatcher.EnableFaults = !this.IsMulticast(serviceEndpoint.Binding);
                endpointDispatcher.ChannelDispatcher.MessageVersion = serviceEndpoint.Binding.MessageVersion;
            }

            bool IsManualAddressing(Binding binding)
            {
                TransportBindingElement transport = binding.CreateBindingElements().Find<TransportBindingElement>();
                if (transport == null)
                {
                    string text = SR.GetString(SR.SFxBindingMustContainTransport2, binding.Name, binding.Namespace);
                    Exception error = new InvalidOperationException(text);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                }
                return transport.ManualAddressing;
            }

            bool IsMulticast(Binding binding)
            {
                IBindingMulticastCapabilities multicast = binding.GetProperty<IBindingMulticastCapabilities>(new BindingParameterCollection());
                return (multicast != null) && multicast.IsMulticast;
            }
        }
        class TransactionContractInformationEndpointBehavior : IEndpointBehavior
        {
            static TransactionContractInformationEndpointBehavior instance;
            public static TransactionContractInformationEndpointBehavior Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TransactionContractInformationEndpointBehavior();
                    }
                    return instance;
                }
            }
            public void Validate(ServiceEndpoint serviceEndpoint) { }
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters) { }
            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
                behavior.AddTransactionFlowProperties = UsesTransactionFlowProperties(serviceEndpoint.Binding.CreateBindingElements(),
                                                                                      serviceEndpoint.Contract);
            }
            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.DispatchRuntime.IgnoreTransactionMessageProperty = !UsesTransactionFlowProperties(
                    serviceEndpoint.Binding.CreateBindingElements(), serviceEndpoint.Contract);
            }
            static bool UsesTransactionFlowProperties(BindingElementCollection bindingElements, ContractDescription contract)
            {
                BindingElementCollection bindingElementCollection = new BindingElementCollection(bindingElements);
                TransactionFlowBindingElement txBE = bindingElementCollection.Find<TransactionFlowBindingElement>();
                if (txBE == null)
                {
                    return false;
                }
                return txBE.IsFlowEnabled(contract);
            }
        }

        class SecurityContractInformationEndpointBehavior : IEndpointBehavior
        {
            bool isForClient;
            SecurityContractInformationEndpointBehavior(bool isForClient)
            {
                this.isForClient = isForClient;
            }
            static SecurityContractInformationEndpointBehavior serverInstance;
            public static SecurityContractInformationEndpointBehavior ServerInstance
            {
                get
                {
                    if (serverInstance == null)
                    {
                        serverInstance = new SecurityContractInformationEndpointBehavior(false);
                    }
                    return serverInstance;
                }
            }
            static SecurityContractInformationEndpointBehavior clientInstance;
            public static SecurityContractInformationEndpointBehavior ClientInstance
            {
                get
                {
                    if (clientInstance == null)
                    {
                        clientInstance = new SecurityContractInformationEndpointBehavior(true);
                    }
                    return clientInstance;
                }
            }
            public void Validate(ServiceEndpoint serviceEndpoint) { }
            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher) { }
            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior) { }
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
            {
                // get Contract info security needs, and put in BindingParameterCollection
                ISecurityCapabilities isc = null;
                BindingElementCollection elements = endpoint.Binding.CreateBindingElements();
                for (int i = 0; i < elements.Count; ++i)
                {
                    if (!(elements[i] is ITransportTokenAssertionProvider))
                    {
                        ISecurityCapabilities tmp = elements[i].GetIndividualProperty<ISecurityCapabilities>();
                        if (tmp != null)
                        {
                            isc = tmp;
                            break;
                        }
                    }
                }
                if (isc != null)
                {
                    // ensure existence of binding parameter
                    ChannelProtectionRequirements requirements = parameters.Find<ChannelProtectionRequirements>();
                    if (requirements == null)
                    {
                        requirements = new ChannelProtectionRequirements();
                        parameters.Add(requirements);
                    }

                    MessageEncodingBindingElement encoding = elements.Find<MessageEncodingBindingElement>();
                    // use endpoint.Binding.Version
                    if (encoding != null && encoding.MessageVersion.Addressing == AddressingVersion.None)
                    {
                        // This binding does not support response actions, so...
                        requirements.Add(ChannelProtectionRequirements.CreateFromContractAndUnionResponseProtectionRequirements(endpoint.Contract, isc, isForClient));
                    }
                    else
                    {
                        requirements.Add(ChannelProtectionRequirements.CreateFromContract(endpoint.Contract, isc, isForClient));
                    }
                }
            }
        }
    }
}
