//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;
    using System.Xaml;
    using System.Xml.Linq;
    using System.Runtime.DurableInstancing;
    using System.Security;

    // InternalSendMessage encapsulates both the server and client send.  For the server
    // send it provides the ability to persist after correlations have been initialized
    // but before the send has actually been completed by the channel stack.  This is not
    // supported by client send.

    class InternalSendMessage : NativeActivity
    {
        static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;

        // Explicit correlation OM
        Collection<CorrelationInitializer> correlationInitializers;
        Collection<CorrelationQuery> replyCorrelationQueries;

        ICollection<CorrelationQuery> correlationQueries;

        MessageVersion messageVersion;

        ContractDescription cachedContract;
        ServiceEndpoint cachedServiceEndpoint;
        AddressHeaderCollection cachedEndpointHeaderCollection;
        FactoryCacheKey cachedFactoryCacheKey;
        bool isConfigSettingsSecure;
        bool configVerified;

        KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache> lastUsedFactoryCacheItem;


        // this will be scheduled if ShouldPersistBeforeSend is set to true
        Activity persist;

        WaitOnChannelCorrelation channelCorrelationCompletionWaiter;
        Variable<VolatileSendMessageInstance> sendMessageInstance;
        Variable<NoPersistHandle> noPersistHandle;
        Variable<Bookmark> extensionSendCompleteBookmark;
        Variable<Guid> e2eActivityId;

        OpenChannelFactory openChannelFactory;
        OpenChannelAndSendMessage openChannelAndSendMessage;

        FaultCallback onSendFailure;

        public InternalSendMessage()
        {
            this.TokenImpersonationLevel = TokenImpersonationLevel.Identification;

            this.sendMessageInstance = new Variable<VolatileSendMessageInstance>();
            this.channelCorrelationCompletionWaiter = new WaitOnChannelCorrelation { Instance = this.sendMessageInstance };

            this.noPersistHandle = new Variable<NoPersistHandle>();
            this.extensionSendCompleteBookmark = new Variable<Bookmark>();
            this.e2eActivityId = new Variable<Guid>();

            this.openChannelFactory = new OpenChannelFactory { Instance = this.sendMessageInstance };
            this.openChannelAndSendMessage = new OpenChannelAndSendMessage { Instance = this.sendMessageInstance, InternalSendMessage = this, };
        }

        public TokenImpersonationLevel TokenImpersonationLevel
        {
            get;
            set;
        }

        // Endpoint defines the service to talk to, and endpointAddress is used to set 
        // the Uri at the runtime, such as the duplex scenario.
        public Endpoint Endpoint
        {
            get;
            set;
        }

        public string EndpointConfigurationName
        {
            get;
            set;
        }

        // This is needed for the callback case
        public InArgument<Uri> EndpointAddress
        {
            get;
            set;
        }

        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }
        
        public string OperationName
        {
            get;
            set;
        }

        public string Action
        {
            get;
            set;
        }

        // cache for internal implementation. This should be set by the Send<T>
        // Should only be used in initating send. 
        // Should use this instead of OperationContract.IsOneWay
        public bool IsOneWay
        {
            get;
            set;
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        // this flag is for Send/SendReply to indicate if we are client-side send or receive-side sendreply
        // 
        internal bool IsSendReply
        {
            get;
            set;
        }

        // Used for cleaning up the Message variable
        internal OutArgument<Message> MessageOut
        {
            get;
            set;
        }

        // should be used to decide whether persist before sending the message
        internal bool ShouldPersistBeforeSend { get; set; }

        internal string OwnerDisplayName { get; set; }

        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        // This will be passed in from the parent Send activity
        public CorrelationQuery CorrelationQuery
        {
            get;
            set;
        }

        // This needs to be set by the ReceiveReply, we assume that this is unique
        internal ICollection<CorrelationQuery> ReplyCorrelationQueries
        {
            get
            {
                if (this.replyCorrelationQueries == null)
                {
                    this.replyCorrelationQueries = new Collection<CorrelationQuery>();
                }

                return this.replyCorrelationQueries;
            }
        }

        // on the serverside, the ContractName is set during ContractInference and is used for retrieving the
        // correct CorrelationQueryBehavior. ContractName on the Serverside can thus be different from what is
        // set on the OM
        public XName ServiceContractName
        {
            get;
            set;
        }

        public InArgument<Message> Message
        {
            get;
            set;
        }

        internal Send Parent
        {
            get;
            set;
        }

        internal static Guid TraceCorrelationActivityId
        {
            [Fx.Tag.SecurityNote(Critical = "Critical because Trace.CorrelationManager has a Link demand for UnmanagedCode.",
                Safe = "Safe because we aren't leaking a critical resource.")]
            [SecuritySafeCritical]
            get
            {
                return Trace.CorrelationManager.ActivityId;
            }
        }

        // we cache the ServiceEndpoint for perf reasons so that we can retrieve endpointaddress, contract etc without
        // creating a new ServiceEndpoint each time
        // Note that we should not pass the cachedServiceEndpoint to the ChannelFactory, as we need to have a 
        // distinct instance per-Factory.
        ServiceEndpoint GetCachedServiceEndpoint()
        {
            if (this.cachedServiceEndpoint == null)
            {
                this.cachedServiceEndpoint = CreateServiceEndpoint();
            }
            return this.cachedServiceEndpoint;
        }

        AddressHeaderCollection GetCachedEndpointHeaders()
        {
            Fx.Assert(this.Endpoint != null, "Endpoint should not be null");
            if (this.cachedEndpointHeaderCollection == null)
            {
                this.cachedEndpointHeaderCollection = new AddressHeaderCollection(this.Endpoint.Headers);
            }
            return this.cachedEndpointHeaderCollection;
        }

        void InitializeEndpoint(ref ServiceEndpoint serviceEndpoint, string configurationName)
        {
            ServiceEndpoint serviceEndpointFromConfig = null;

            if (configurationName != null)
            {
                // load the standard endpoint from the config
                serviceEndpointFromConfig = ConfigLoader.LookupEndpoint(configurationName, null, serviceEndpoint.Contract);
            }

            if (serviceEndpointFromConfig != null)
            {
                // standard endpoint case: it can completely override the endpoint
                serviceEndpoint = serviceEndpointFromConfig;
            }
            else
            {
                // normal endpoint case
                if (!serviceEndpoint.IsFullyConfigured)
                {
                    new ConfigLoader().LoadChannelBehaviors(serviceEndpoint, configurationName);
                }
            }
        }

        // used to create ChannelFactoryReference instances. We don't cache the serviceEndpoint 
        // directly, as we need to have a distinct instance per-Factory. So it's cached behind the 
        // scenes as part of the ChannelFactoryReference
        ServiceEndpoint CreateServiceEndpoint()
        {
            ContractDescription contract = null;
            bool ensureTransactionFlow = false;
            if (this.cachedContract == null)
            {
                contract = this.GetContractDescription();
                ensureTransactionFlow = true;
            }
            else
            {
                contract = this.cachedContract;
            }
            ServiceEndpoint result = new ServiceEndpoint(contract);
            if (this.Endpoint != null)
            {
                result.Binding = this.Endpoint.Binding;
                if (this.Endpoint.AddressUri != null)
                {
                    result.Address = new EndpointAddress(this.Endpoint.AddressUri, this.Endpoint.Identity, this.GetCachedEndpointHeaders());
                }
            }
            // Get ServiceEndpoint will be called only on the client side, hence if endpoint is null, we will try to load the config with 
            // endpointConfigurationName. 
            // endpointConfigurationName = null will be translated to endpointConfigurationName = String.Empty
            else
            {
                // we are loading the binding & the behaviors from config
                if (this.ServiceContractName != null)
                {
                    result.Contract.ConfigurationName = this.ServiceContractName.LocalName;
                }
                InitializeEndpoint(ref result, this.EndpointConfigurationName ?? string.Empty);
            }

            // if the cachedContract is null, verify if TransactionFlow is accounted for in the contract
            // if cachedContract is not null, we can skip this since the contract should be fixed for the workflow definition 
            if (ensureTransactionFlow)
            {
                EnsureTransactionFlowOnContract(ref result);
                this.cachedContract = result.Contract;
            }
            EnsureCorrelationQueryBehavior(result);

            return result;
        }

        void EnsureCorrelationQueryBehavior(ServiceEndpoint serviceEndpoint)
        {
            CorrelationQueryBehavior correlationQueryBehavior = serviceEndpoint.Behaviors.Find<CorrelationQueryBehavior>();
            if (correlationQueryBehavior == null)
            {
                // Add CorrelationQueryBehavior if either Binding has queries or if either Send or ReceiveReplies 
                // have correlation query associated with them
                if (CorrelationQueryBehavior.BindingHasDefaultQueries(serviceEndpoint.Binding)
                    || this.CorrelationQuery != null
                    || this.ReplyCorrelationQueries.Count > 0)
                {
                    correlationQueryBehavior = new CorrelationQueryBehavior(new Collection<CorrelationQuery>());
                    serviceEndpoint.Behaviors.Add(correlationQueryBehavior);
                }
            }
            if (correlationQueryBehavior != null)
            {
                // add CorrelationQuery from Send
                if (this.CorrelationQuery != null && !correlationQueryBehavior.CorrelationQueries.Contains(this.CorrelationQuery))
                {
                    correlationQueryBehavior.CorrelationQueries.Add(this.CorrelationQuery);
                }

                //add ReplyCorrelationQueries from ReceiveReply (there could be multiple ReceiveReplies for a Send and hence the collection
                foreach (CorrelationQuery query in this.ReplyCorrelationQueries)
                {
                    // Filter out duplicate CorrelationQueries in the collection.
                    // Currently, we only do reference comparison and Where message filter comparison.
                    if (!correlationQueryBehavior.CorrelationQueries.Contains(query))
                    {
                        correlationQueryBehavior.CorrelationQueries.Add(query);
                    }
                    else
                    {
                        if (TD.DuplicateCorrelationQueryIsEnabled())
                        {
                            TD.DuplicateCorrelationQuery(query.Where.ToString());
                        }
                    }
                }

                this.correlationQueries = correlationQueryBehavior.CorrelationQueries;
            }
        }

        static void EnsureCorrelationBehaviorScopeName(ActivityContext context, CorrelationQueryBehavior correlationBehavior)
        {
            Fx.Assert(correlationBehavior != null, "caller must verify");
            if (correlationBehavior.ScopeName == null)
            {
                CorrelationExtension extension = context.GetExtension<CorrelationExtension>();
                if (extension != null)
                {
                    correlationBehavior.ScopeName = extension.ScopeName;
                }
            }
        }

        void EnsureTransactionFlowOnContract(ref ServiceEndpoint serviceEndpoint)
        {
            if (!this.IsOneWay)
            {
                BindingElementCollection elementCollection = serviceEndpoint.Binding.CreateBindingElements();
                TransactionFlowBindingElement bindingElement = elementCollection.Find<TransactionFlowBindingElement>();
                bool flowTransaction = ((bindingElement != null) && (bindingElement.Transactions));
                if (flowTransaction)
                {
                    ContractInferenceHelper.EnsureTransactionFlowOnContract(ref serviceEndpoint,
                        this.ServiceContractName, this.OperationName, this.Action, this.Parent.ProtectionLevel);
                }
            }
        }

        internal MessageVersion GetMessageVersion()
        {
            if (this.messageVersion == null)
            {
                ServiceEndpoint endpoint = this.GetCachedServiceEndpoint();
                this.messageVersion = (endpoint != null && endpoint.Binding != null) ? endpoint.Binding.MessageVersion : null;
            }
            return this.messageVersion;
        }

        ContractDescription GetContractDescription()
        {
            ContractDescription cd;

            // When channel cache is disabled or when operation uses message contract,
            // we use the fully inferred description; otherwise, we use a fixed description to increase channel cache hits

            if (!this.Parent.ChannelCacheEnabled || this.Parent.OperationUsesMessageContract)
            {
                // If this is one-way send untyped message, this.OperationDescription would still be null
                if (this.Parent.OperationDescription == null)
                {
                    Fx.Assert(this.IsOneWay, "We can only reach here when we are one-way send Message!");
                    this.Parent.OperationDescription = ContractInferenceHelper.CreateOneWayOperationDescription(this.Parent);
                }

                cd = ContractInferenceHelper.CreateContractFromOperation(this.ServiceContractName, this.Parent.OperationDescription);
            }
            else
            {
                // Create ContractDescription using Fixed MessageIn/MessageOut contract
                // If IOutputChannel, we create a Contract with name IOutputChannel and OperationDescription "Send"
                // else, Contract name is IRequestChannel with OperationDescription "Request"

                if (this.IsOneWay)
                {
                    cd = ContractInferenceHelper.CreateOutputChannelContractDescription(this.ServiceContractName, this.Parent.ProtectionLevel);
                }
                else
                {
                    cd = ContractInferenceHelper.CreateRequestChannelContractDescription(this.ServiceContractName, this.Parent.ProtectionLevel);
                }
            }

            if (this.ServiceContractName != null)
            {
                cd.ConfigurationName = this.ServiceContractName.LocalName;
            }
            return cd;
        }

        EndpointAddress CreateEndpointAddress(NativeActivityContext context)
        {
            ServiceEndpoint endpoint = this.GetCachedServiceEndpoint();
            Uri endpointAddressUri = (this.EndpointAddress != null) ? this.EndpointAddress.Get(context) : null;

            if (endpoint != null && endpoint.Address != null)
            {
                return endpointAddressUri == null ?
                    endpoint.Address :
                    (new EndpointAddressBuilder(endpoint.Address) { Uri = endpointAddressUri }).ToEndpointAddress();
            }
            else if (this.Endpoint != null)
            {
                return endpointAddressUri == null ?
                    this.Endpoint.GetAddress() :
                    new EndpointAddress(endpointAddressUri, this.Endpoint.Identity, this.GetCachedEndpointHeaders());
            }
            else
            {
                return null;
            }
        }

        EndpointAddress CreateEndpointAddressFromCallback(EndpointAddress CallbackAddress)
        {
            Fx.Assert(CallbackAddress != null, "CallbackAddress cannot be null");

            EndpointIdentity endpointIdentity = null;
            AddressHeaderCollection headers = null;
            EndpointAddress endpointAddress;

            if (this.Endpoint != null)
            {
                // we honor Identity and Headers on the Endpoint OM even when the AddressUri is null
                endpointIdentity = this.Endpoint.Identity;
                headers = this.GetCachedEndpointHeaders();
            }
            else
            {
                // this could be from config
                ServiceEndpoint endpoint = this.GetCachedServiceEndpoint();
                Fx.Assert(endpoint != null, " endpoint cannot be null");
                if (endpoint.Address != null)
                {
                    endpointIdentity = endpoint.Address.Identity;
                    headers = endpoint.Address.Headers;
                }
            }

            if (endpointIdentity != null || headers != null)
            {
                Uri callbackUri = CallbackAddress.Uri;
                endpointAddress = new EndpointAddress(callbackUri, endpointIdentity, headers);
            }
            else
            {
                endpointAddress = CallbackAddress;
            }
            return endpointAddress;
        }


        bool IsEndpointSettingsSafeForCache()
        {
            if (!this.configVerified)
            {

                // let's set isConfigSettingsSecure flag to false if we use endpointConfiguration, 
                // this is used to decide if we cache factory or not

                this.isConfigSettingsSecure = this.Endpoint != null ? true : false;
                this.configVerified = true;
            }
            return this.isConfigSettingsSecure;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (ShouldPersistBeforeSend)
            {
                if (this.persist == null)
                {
                    this.persist = new Persist();
                }
                metadata.AddImplementationChild(this.persist);
            }

            RuntimeArgument endpointAddressArgument = new RuntimeArgument(Constants.EndpointAddress, Constants.UriType, ArgumentDirection.In);
            if (this.EndpointAddress == null)
            {
                this.EndpointAddress = new InArgument<Uri>();
            }
            metadata.Bind(this.EndpointAddress, endpointAddressArgument);
            metadata.AddArgument(endpointAddressArgument);

            RuntimeArgument correlatesWithArgument = new RuntimeArgument(Constants.CorrelatesWith, Constants.CorrelationHandleType, ArgumentDirection.In);
            if (this.CorrelatesWith == null)
            {
                this.CorrelatesWith = new InArgument<CorrelationHandle>();
            }
            metadata.Bind(this.CorrelatesWith, correlatesWithArgument);
            metadata.AddArgument(correlatesWithArgument);
            
            if (this.correlationInitializers != null)
            {
                int count = 0;
                foreach (CorrelationInitializer correlation in this.correlationInitializers)
                {
                    if (correlation.CorrelationHandle != null)
                    {
                        RuntimeArgument argument = new RuntimeArgument(Constants.Parameter + count,
                            correlation.CorrelationHandle.ArgumentType, correlation.CorrelationHandle.Direction, true);
                        metadata.Bind(correlation.CorrelationHandle, argument);
                        metadata.AddArgument(argument);
                        count++;
                    }
                }
            }

            RuntimeArgument requestMessageArgument = new RuntimeArgument(Constants.RequestMessage, Constants.MessageType, ArgumentDirection.In);
            if (this.Message == null)
            {
                this.Message = new InArgument<Message>();
            }
            metadata.Bind(this.Message, requestMessageArgument);
            metadata.AddArgument(requestMessageArgument);

            if (this.MessageOut != null)
            {
                RuntimeArgument requestMessageReference = new RuntimeArgument("MessageReference", Constants.MessageType, ArgumentDirection.Out);
                metadata.Bind(this.MessageOut, requestMessageReference);
                metadata.AddArgument(requestMessageReference);
            }

            metadata.AddImplementationVariable(this.sendMessageInstance);
            metadata.AddImplementationVariable(this.noPersistHandle);
            metadata.AddImplementationVariable(this.extensionSendCompleteBookmark);
            metadata.AddImplementationVariable(this.e2eActivityId);

            metadata.AddImplementationChild(this.channelCorrelationCompletionWaiter);
            metadata.AddImplementationChild(this.openChannelFactory);
            metadata.AddImplementationChild(this.openChannelAndSendMessage);

            metadata.AddDefaultExtensionProvider(SendMessageChannelCache.DefaultExtensionProvider);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                Bookmark pendingBookmark = this.extensionSendCompleteBookmark.Get(context);
                if (pendingBookmark != null)
                {
                    sendReceiveExtension.Cancel(pendingBookmark);
                    context.RemoveBookmark(pendingBookmark);
                }
                context.MarkCanceled();
            }
            else
            {
                // Do nothing.  InternalSendMessage cannot be canceled since
                // the individual parts of the process cannot be canceled.
            }
        }

        protected override void Abort(NativeActivityAbortContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                Bookmark pendingBookmark = this.extensionSendCompleteBookmark.Get(context);
                if (pendingBookmark != null)
                {
                    sendReceiveExtension.Cancel(pendingBookmark);
                }
                base.Abort(context);
            }
            else
            {

                VolatileSendMessageInstance volatileInstance = this.sendMessageInstance.Get(context);

                if (volatileInstance != null)
                {
                    CleanupResources(volatileInstance.Instance);
                }
            }
        }

        void CleanupResources(SendMessageInstance instance)
        {
            if (instance != null)
            {
                instance.Dispose();
            }
        }

        // A separate code-path for extension based execution least impacts 
        // the existing workflow hosts. In the future we will add an extension from 
        // workflowservicehost and always use the extension.
        protected override void Execute(NativeActivityContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                this.ExecuteUsingExtension(sendReceiveExtension, context);
            }
            else
            {
                // 



                // The entire InternalSendMessage runs in a no persist zone
                NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
                noPersistHandle.Enter(context);

                // Set up the SendMessageInstance, which will 
                // setup an AsyncOperationBlock under the hood and thus block persistence 
                // until the message has been sent and we return to the workflow thread
                SendMessageInstance instance = new SendMessageInstance(this, context);
                SetSendMessageInstance(context, instance);

                if (instance.RequestContext != null)
                {
                    ExecuteClientRequest(context, instance);
                }
                else
                {
                    ExecuteServerResponse(context, instance);
                }
            }
        }

        void ExecuteUsingExtension(SendReceiveExtension sendReceiveExtension, NativeActivityContext context)
        {
            CorrelationHandle correlatesWith = null;
            if (this.TryGetCorrelatesWithHandle(context, out correlatesWith) && !correlatesWith.IsInitalized())
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR.SendWithUninitializedCorrelatesWith(this.OperationName ?? string.Empty)));
            }

            CorrelationHandle ambientHandle = CorrelationHandle.GetAmbientCorrelation(context);
            if (correlatesWith == null)
            {
                correlatesWith = ambientHandle;
            }

            Guid e2eTracingId;
            SendSettings sendSettings;

            if (this.IsSendReply)
            {
                if (correlatesWith == null || !correlatesWith.IsInitalized())
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.SendWithUninitializedCorrelatesWith(this.OperationName ?? string.Empty)));
                }

                e2eTracingId = correlatesWith.E2ETraceId;
                sendSettings = GetSettingsForSendReply();
            }
            else
            {
                CorrelationHandle requestReplyCorrelationHandle;
                this.correlationInitializers.TryGetRequestReplyCorrelationHandle(context, out requestReplyCorrelationHandle);

                // validate correlation configuration
                if (this.IsOneWay)
                {
                    if (requestReplyCorrelationHandle != null)
                    {
                        // this is a one-way send , we should not have a RequestReply Correlation initializer
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RequestReplyHandleShouldNotBePresentForOneWay));
                    }
                }
                else
                {
                    if (requestReplyCorrelationHandle == null && ambientHandle == null)
                    {
                        // we neither have a requestReply nor an ambientHandle
                        throw FxTrace.Exception.AsError(new InvalidOperationException(
                            SR.SendMessageNeedsToPairWithReceiveMessageForTwoWayContract(this.OperationName ?? string.Empty)));
                    }
                }

                e2eTracingId = InternalSendMessage.TraceCorrelationActivityId;
                if (e2eTracingId == Guid.Empty)
                {
                    e2eTracingId = Guid.NewGuid();
                }
                sendSettings = GetSettingsForSend(context);
            }

            this.SendToExtension(sendReceiveExtension, context, sendSettings, e2eTracingId, correlatesWith);
        }

        void SendToExtension(SendReceiveExtension sendReceiveExtension, NativeActivityContext context, SendSettings sendSettings, Guid e2eTracingId, CorrelationHandle correlatesWith)
        {
            Message message = this.Message.Get(context);

            // add a transient correlation if necessary
            if (!IsOneWay && !IsSendReply)
            {
                CorrelationMessageProperty correlationMessageProperty;
                if (!message.Properties.TryGetValue(CorrelationMessageProperty.Name, out correlationMessageProperty))
                {
                    InstanceKey requestReplyCorrelationKey = new InstanceKey(Guid.NewGuid(),
                            new Dictionary<XName, InstanceValue>
                            {
                                { WorkflowServiceNamespace.RequestReplyCorrelation, new InstanceValue(true) }
                            });

                    List<InstanceKey> transientCorrelations = new List<InstanceKey>();
                    transientCorrelations.Add(requestReplyCorrelationKey);
                    correlationMessageProperty = new CorrelationMessageProperty(InstanceKey.InvalidKey, new List<InstanceKey>(0), transientCorrelations);
                    message.Properties[CorrelationMessageProperty.Name] = correlationMessageProperty;
                }
                else
                {
                    InstanceKey requestReplyCorrelationKey;
                    // if requestReplyCorrelationKey does not exist, clone correlationMessageProperty and
                    // replace it in the message with one that has the key.
                    if (!this.TryGetRequestReplyCorrelationInstanceKey(correlationMessageProperty, out requestReplyCorrelationKey))
                    {
                        requestReplyCorrelationKey = new InstanceKey(Guid.NewGuid(),
                            new Dictionary<XName, InstanceValue>
                            {
                                { WorkflowServiceNamespace.RequestReplyCorrelation, new InstanceValue(true) }
                            });
                        List<InstanceKey> transientCorrelations = new List<InstanceKey>(correlationMessageProperty.TransientCorrelations);
                        transientCorrelations.Add(requestReplyCorrelationKey);
                        CorrelationMessageProperty newProperty = new CorrelationMessageProperty(
                                correlationMessageProperty.CorrelationKey,
                                correlationMessageProperty.AdditionalKeys,
                                transientCorrelations);
                        message.Properties[CorrelationMessageProperty.Name] = newProperty;
                    }
                }
            }

            MessageContext messageContext = new MessageContext(message) { EndToEndTracingId = e2eTracingId };
            Bookmark sendCompleteBookmark = context.CreateBookmark(SendCompleteOnExtension);
            this.extensionSendCompleteBookmark.Set(context, sendCompleteBookmark);
            this.e2eActivityId.Set(context, e2eTracingId);
            this.ProcessSendMessageTrace(context, e2eTracingId, true);
            sendReceiveExtension.Send(
                messageContext, 
                sendSettings, 
                (correlatesWith == null) ? null : correlatesWith.InstanceKey, 
                sendCompleteBookmark);

            if (this.MessageOut != null)
            {
                this.MessageOut.Set(context, null);
            }

            this.Message.Set(context, null);
        }

        SendSettings GetSettingsForSendReply()
        {
            return new SendSettings
            {
                RequirePersistBeforeSend = this.ShouldPersistBeforeSend,
                OwnerDisplayName = this.OwnerDisplayName
            };
        }

        SendSettings GetSettingsForSend(NativeActivityContext context)
        {
            SendSettings settings = new SendSettings
            {
                IsOneWay = this.IsOneWay,
                EndpointConfigurationName = this.EndpointConfigurationName,
                TokenImpersonationLevel = this.TokenImpersonationLevel,
                ProtectionLevel = this.Parent.ProtectionLevel,
                OwnerDisplayName = this.OwnerDisplayName
            };

            if (this.EndpointAddress != null)
            {
                settings.EndpointAddress = this.EndpointAddress.Get(context);
            }

            if (this.Endpoint != null)
            {
                settings.Endpoint = XamlServices.Parse(XamlServices.Save(this.Endpoint)) as Endpoint;
            }

            return settings;
        }

        void SendCompleteOnExtension(NativeActivityContext context, Bookmark bookmark, object state)
        {
            // Now that the bookmark has been resumed, clear out the workflow variable holding 
            // its value.
            this.extensionSendCompleteBookmark.Set(context, null);

            Exception fault = state as Exception;
            if (fault != null)
            {
                throw FxTrace.Exception.AsError(fault);
            }

            CorrelationMessageProperty correlationMessageProperty = state as CorrelationMessageProperty;

            if (state != null && correlationMessageProperty == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidDataFromSendBookmarkState(this.OperationName ?? string.Empty)));
            }

            if (correlationMessageProperty != null)
            {
                this.InitializeCorrelationHandles(context, correlationMessageProperty);
            }

            Guid e2eActivityId = this.e2eActivityId.Get(context);
            this.ProcessSendMessageCompleteTrace(context, e2eActivityId);
        }

        void InitializeCorrelationHandles(NativeActivityContext context, CorrelationMessageProperty correlationMessageProperty)
        {
            CorrelationHandle ambientCorrelationHandle = CorrelationHandle.GetAmbientCorrelation(context);

            if (this.IsSendReply)
            {
                // Check for ContextCorrelationInitializer handle
                CorrelationHandle contextCorrelationHandle = CorrelationHandle.GetExplicitContextCorrelation(context, this.correlationInitializers);
                MessagingActivityHelper.InitializeCorrelationHandles(context, contextCorrelationHandle, ambientCorrelationHandle, this.correlationInitializers, correlationMessageProperty.CorrelationKey, correlationMessageProperty.AdditionalKeys);
            }
            else
            {
                // Check for CallbackCorrelationInitializer handle
                CorrelationHandle callbackCorrelationHandle = CorrelationHandle.GetExplicitCallbackCorrelation(context, this.correlationInitializers);
                MessagingActivityHelper.InitializeCorrelationHandles(context, callbackCorrelationHandle, ambientCorrelationHandle, this.correlationInitializers, correlationMessageProperty.CorrelationKey, correlationMessageProperty.AdditionalKeys);

                InstanceKey requestReplyInstanceKey;
                if (this.TryGetRequestReplyCorrelationInstanceKey(correlationMessageProperty, out requestReplyInstanceKey))
                {
                    CorrelationHandle requestReplyCorrelationHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(context, this.correlationInitializers);
                    if (requestReplyCorrelationHandle != null)
                    {
                        requestReplyCorrelationHandle.TransientInstanceKey = requestReplyInstanceKey;
                    }
                    else if (ambientCorrelationHandle != null)
                    {
                        ambientCorrelationHandle.TransientInstanceKey = requestReplyInstanceKey;
                    }
                }
            }
        }

        bool TryGetRequestReplyCorrelationInstanceKey(CorrelationMessageProperty correlationMessageProperty, out InstanceKey instanceKey)
        {
            instanceKey = null;

            foreach (InstanceKey key in correlationMessageProperty.TransientCorrelations)
            {
                InstanceValue value;
                if (key.Metadata.TryGetValue(WorkflowServiceNamespace.RequestReplyCorrelation, out value))
                {
                    instanceKey = key;
                    break;
                }
            }

            return instanceKey != null;
        }

        bool TryGetCorrelatesWithHandle(NativeActivityContext context, out CorrelationHandle correlationHandle)
        {
            correlationHandle = null;
            if (this.CorrelatesWith != null)
            {
                correlationHandle = this.CorrelatesWith.Get(context);
            }

            return correlationHandle != null;
        }

        void SetSendMessageInstance(NativeActivityContext context, SendMessageInstance instance)
        {
            VolatileSendMessageInstance volatileInstance = new VolatileSendMessageInstance { Instance = instance };
            this.sendMessageInstance.Set(context, volatileInstance);
        }

        SendMessageInstance GetSendMessageInstance(ActivityContext context)
        {
            VolatileSendMessageInstance volatileInstance = this.sendMessageInstance.Get(context);

            Fx.Assert(volatileInstance != null, "This should never be null.");

            return volatileInstance.Instance;
        }

        // Used for server-side send (replies). We don't have any async code here since the
        // Dispatcher handles any completions
        void ExecuteServerResponse(NativeActivityContext context, SendMessageInstance instance)
        {
            Fx.Assert(instance.ResponseContext != null, "only valid for responses");
            Fx.Assert(instance.ResponseContext.WorkflowOperationContext != null, "The WorkflowOperationContext is required on the CorrelationResponseContext");
            instance.OperationContext = instance.ResponseContext.WorkflowOperationContext.OperationContext;

            // now that we have our op-context, invoke the callback that user might have added in the AEC in the previous activity 
            // e.g. distributed compensation activity will add this so that they can convert an execution property 
            // to an message properties, as will Transaction Flow
            instance.ProcessMessagePropertyCallbacks();

            ProcessSendMessageTrace(context, instance, false);

            // retrieve the correct CorrelationQueryBehavior from the ChannelExtensions collection
            CorrelationQueryBehavior correlationBehavior = null;
            Collection<CorrelationQueryBehavior> correlationQueryBehaviors = instance.OperationContext.Channel.Extensions.FindAll<CorrelationQueryBehavior>();
            foreach (CorrelationQueryBehavior cqb in correlationQueryBehaviors)
            {
                if (cqb.ServiceContractName == this.ServiceContractName)
                {
                    correlationBehavior = cqb;
                    break;
                }
            }

            //set the reply
            instance.RequestOrReply = this.Message.Get(context);

            if (correlationBehavior != null)
            {
                EnsureCorrelationBehaviorScopeName(context, correlationBehavior);
                instance.RegisterCorrelationBehavior(correlationBehavior);

                if (instance.CorrelationKeyCalculator != null)
                {
                    if (correlationBehavior.SendNames != null && correlationBehavior.SendNames.Count > 0)
                    {
                        if (correlationBehavior.SendNames.Count == 1 && (correlationBehavior.SendNames.Contains(ContextExchangeCorrelationHelper.CorrelationName)))
                        {
                            // Contextchannel is the only channel participating in correlation
                            // Since we already have the instance id, we don't have to wait for the context channel to call us back to initialize 
                            // the correlation - InstanceId can be retrieved directly from ContextMessageProperty through Operation context.
                            ContextMessageProperty contextProperties = null;
                            if (ContextMessageProperty.TryGet(instance.OperationContext.OutgoingMessageProperties, out contextProperties))
                            {
                                // 

                                CorrelationDataMessageProperty.AddData(instance.RequestOrReply, ContextExchangeCorrelationHelper.CorrelationName, () => ContextExchangeCorrelationHelper.GetContextCorrelationData(instance.OperationContext));
                            }
                            // Initialize correlations right away without waiting for the context channel to call us back
                            InitializeCorrelations(context, instance);
                        }
                        else
                        {
                            // Initialize correlations through channel callback
                            instance.OperationContext.OutgoingMessageProperties.Add(CorrelationCallbackMessageProperty.Name,
                                new MessageCorrelationCallbackMessageProperty(correlationBehavior.SendNames ?? new string[0], instance));
                            instance.CorrelationSynchronizer = new CorrelationSynchronizer();
                        }
                    }
                    else
                    {
                        // there are no channel based queries, we can initialize correlations right away
                        InitializeCorrelations(context, instance);
                    }
                }
            }

            // For exception case: Always call WorkflowOperationContext.SendFault to either send back the fault in the request/reply case 
            // or make sure the error handler extension gets a chance to handle this fault;
            if (instance.ResponseContext.Exception != null)
            {
                try
                {
                    instance.ResponseContext.WorkflowOperationContext.SendFault(instance.ResponseContext.Exception);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    instance.ResponseContext.Exception = e;
                }
            }
            else
            {
                try
                {
                    instance.ResponseContext.WorkflowOperationContext.SendReply(instance.RequestOrReply);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    instance.ResponseContext.Exception = e;
                }
            }

            if (TraceUtility.ActivityTracing)
            {
                if (instance.AmbientActivityId != InternalSendMessage.TraceCorrelationActivityId)
                {
                    if (TD.StopSignpostEventIsEnabled())
                    {
                        TD.StopSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                    { MessagingActivityHelper.ActivityName, this.DisplayName },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                    { MessagingActivityHelper.ActivityInstanceId, context.ActivityInstanceId }
                            }));
                    }
                    FxTrace.Trace.SetAndTraceTransfer(instance.AmbientActivityId, true);
                    instance.AmbientActivityId = Guid.Empty;
                }
            }

            if (instance.CorrelationSynchronizer == null)
            {
                // We aren't doing any correlation work so we just
                // finalize the send.
                context.SetValue(this.Message, null);
                context.SetValue(this.MessageOut, null);

                if (ShouldPersistBeforeSend)
                {
                    // Need to allow persistence.
                    NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
                    noPersistHandle.Exit(context);

                    // 
                    context.ScheduleActivity(this.persist, new CompletionCallback(OnPersistCompleted));
                }
                else
                {
                    FinalizeSendMessageCore(instance);
                }
            }
            else
            {
                // We're doing correlation.  Either the work is already
                // done or we need to synchronize with the channel stack.
                if (instance.CorrelationSynchronizer.IsChannelWorkComplete)
                {
                    // No need to schedule our completion waiter
                    OnChannelCorrelationCompleteCore(context, instance);
                }
                else
                {
                    context.ScheduleActivity(this.channelCorrelationCompletionWaiter, OnChannelCorrelationComplete, null);
                }

                // We notify that we're done with the send.  If the
                // correlation processing has already completed then
                // we'll finalize the send.
                if (instance.CorrelationSynchronizer.NotifySendComplete())
                {
                    FinalizeSendMessageCore(instance);
                }
            }
        }

        void ProcessSendMessageTrace(NativeActivityContext context, SendMessageInstance instance, bool isClient)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                if (TraceUtility.ActivityTracing)
                {
                    instance.AmbientActivityId = InternalSendMessage.TraceCorrelationActivityId;
                }

                if (isClient)
                {
                    //We need to emit a transfer from WF instance ID to the id set in the TLS
                    instance.E2EActivityId = InternalSendMessage.TraceCorrelationActivityId;
                    if (instance.E2EActivityId == Guid.Empty)
                    {
                        instance.E2EActivityId = Guid.NewGuid();
                    }
                }
                else
                {
                    instance.E2EActivityId = instance.ResponseContext.WorkflowOperationContext.E2EActivityId;
                }

                this.ProcessSendMessageTrace(context, instance.E2EActivityId, isClient);
            }
        }

        void ProcessSendMessageTrace(NativeActivityContext context, Guid e2eActivityId, bool isClient)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                try
                {
                    if (isClient)
                    {
                        if (context.WorkflowInstanceId != e2eActivityId)
                        {
                            DiagnosticTraceBase.ActivityId = context.WorkflowInstanceId;
                            FxTrace.Trace.SetAndTraceTransfer(e2eActivityId, true);
                        }
                    }
                    else
                    {
                        DiagnosticTraceBase.ActivityId = context.WorkflowInstanceId;
                    }

                    context.Track(
                        new SendMessageRecord(MessagingActivityHelper.MessageCorrelationSendRecord)
                        {
                            E2EActivityId = e2eActivityId
                        });

                    if (TraceUtility.ActivityTracing)
                    {
                        if (TD.StartSignpostEventIsEnabled())
                        {
                            TD.StartSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                    { MessagingActivityHelper.ActivityName, this.DisplayName },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                    { MessagingActivityHelper.ActivityInstanceId, context.ActivityInstanceId }
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    FxTrace.Exception.AsInformation(ex);
                }
            }
        }

        void ProcessSendMessageCompleteTrace(NativeActivityContext context, Guid e2eActivityId)
        {
            Guid ambientActivityId = InternalSendMessage.TraceCorrelationActivityId;
            if (TraceUtility.ActivityTracing)
            {
                if (TD.StopSignpostEventIsEnabled())
                {
                    TD.StopSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                    { MessagingActivityHelper.ActivityName, this.DisplayName },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                    { MessagingActivityHelper.ActivityInstanceId, context.ActivityInstanceId }
                                }));
                }
                FxTrace.Trace.SetAndTraceTransfer(ambientActivityId, true);
            }
            if (TD.WfMessageSentIsEnabled())
            {
                // 
                EventTraceActivity eta = new EventTraceActivity();
                if (e2eActivityId != Guid.Empty)
                {
                    eta.SetActivityId(e2eActivityId);
                }
                TD.WfMessageSent(eta, ambientActivityId);
            }
        }

        void OnChannelCorrelationComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            SendMessageInstance instance = GetSendMessageInstance(context);
            Fx.Assert(instance != null, "The instance cannot be null here.");

            OnChannelCorrelationCompleteCore(context, instance);
        }

        void OnChannelCorrelationCompleteCore(NativeActivityContext context, SendMessageInstance instance)
        {
            Message message = InitializeCorrelations(context, instance);
            instance.CorrelationSynchronizer.NotifyMessageUpdatedByWorkflow(message);

            context.SetValue(this.Message, null);
            context.SetValue(this.MessageOut, null);

            if (this.ShouldPersistBeforeSend && instance.RequestContext == null)
            {
                // Need to allow persistence.
                NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
                noPersistHandle.Exit(context);

                // 
                context.ScheduleActivity(this.persist, new CompletionCallback(OnPersistCompleted));
            }
            else
            {
                // Create a bookmark to complete the callback, this is to ensure that the InstanceKey does get saved in the PPD 
                // by the time the bookmark is resumed. The instancekey is not getting  saved in the PPD  till workflow gets to 
                // the next idle state. 
                // 
                Bookmark completeCorrelationBookmark = context.CreateBookmark(CompleteCorrelationCallback, BookmarkOptions.NonBlocking);
                context.ResumeBookmark(completeCorrelationBookmark, null);
            }
        }

        void CompleteCorrelationCallback(NativeActivityContext context, Bookmark bookmark, object value)
        {
            SendMessageInstance instance = GetSendMessageInstance(context);
            Fx.Assert(instance != null, "The instance cannot be null here.");

            if (instance.CorrelationSynchronizer.NotifyWorkflowCorrelationProcessingComplete())
            {
                // The send complete notification has already occurred
                // so it is up to us to finalize the send.
                FinalizeSendMessageCore(instance);
            }
        }

        void OnPersistCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // We can reenter no persist now
            NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
            noPersistHandle.Enter(context);

            // We might get back a null here because we've allowed persistence.
            // If that is the case we'll just ignore it ... we don't have any more
            // meaningful work to do.
            SendMessageInstance instance = GetSendMessageInstance(context);

            if (instance != null)
            {
                // Do it with or without correlation
                if (instance.CorrelationSynchronizer == null || instance.CorrelationSynchronizer.NotifyWorkflowCorrelationProcessingComplete())
                {
                    // The send complete notification has already occurred
                    // so it is up to us to finalize the send.
                    FinalizeSendMessageCore(instance);
                }
            }
        }

        void ExecuteClientRequest(NativeActivityContext context, SendMessageInstance instance)
        {
            // This is the client send request: we need to figure out the channel and request message first

            // Get the Extension for the ChannelSettings
            instance.CacheExtension = context.GetExtension<SendMessageChannelCache>();
            Fx.Assert(instance.CacheExtension != null, "channelCacheExtension must exist.");


            // Send.ChannelCacheEnabled must be set before we call CreateEndpointAddress
            // because CreateEndpointAddress will cache description and description resolution depends on the value of ChannelCacheEnabled
            this.Parent.InitializeChannelCacheEnabledSetting(instance.CacheExtension);

            // if there is a correlatesWith handle with callbackcontext(Durable Duplex case), use the callback address and context from
            // there. The handle could be an explicit 'CorrelatesWith' handle or an ambient handle.
            if (instance.CorrelatesWith != null)
            {
                if (instance.CorrelatesWith.CallbackContext != null)
                {
                    instance.CorrelationCallbackContext = instance.CorrelatesWith.CallbackContext;

                    // construct EndpointAdress based on the ListenAddress from callback and the identity and headers from Endpoint or from Config
                    instance.EndpointAddress = CreateEndpointAddressFromCallback(instance.CorrelationCallbackContext.ListenAddress.ToEndpointAddress());
                }

                if (instance.CorrelatesWith.Context != null)
                {
                    instance.CorrelationContext = instance.CorrelatesWith.Context;
                }
            }
            // Request  is always of Type Message. Message Argument will be set by Send<T> using the appropriate formatter
            instance.RequestOrReply = this.Message.Get(context);

            if (instance.EndpointAddress == null)
            {
                //try to get it from endpoint or config
                instance.EndpointAddress = CreateEndpointAddress(context);
            }

            if (instance.EndpointAddress == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR.EndpointAddressNotSetInEndpoint(this.OperationName)));
            }

            // Configname to be used for the FactoryCacheKey, 
            // if endpoint is defined, we use the settings from endpoint and ignore the endpointConfigurationName
            // if endpoint is not defined we use the endpointConfigurationName
            string configName = (this.Endpoint != null) ? null : this.EndpointConfigurationName;

            ProcessSendMessageTrace(context, instance, true);

            // Get ChannelFactory from the cache
            ObjectCache<FactoryCacheKey, ChannelFactoryReference> channelFactoryCache = null;
            ObjectCacheItem<ChannelFactoryReference> cacheItem = null;
            ChannelCacheSettings channelCacheSettings;                        
            
            // retrieve the FactoryCacheKey and cache it so that we could use it later.  
            if (this.cachedFactoryCacheKey == null)
            {
                ServiceEndpoint targetEndpoint = this.GetCachedServiceEndpoint();
                this.cachedFactoryCacheKey = new FactoryCacheKey(this.Endpoint, configName, this.IsOneWay, this.TokenImpersonationLevel,
                    targetEndpoint.Contract, this.correlationQueries);
            }
            
            // let's decide if we can share the cache from the extension
            // cache can be share if AllowUnsafeSharing is true or it is safe to share
            if (instance.CacheExtension.AllowUnsafeCaching || this.IsEndpointSettingsSafeForCache())
            {
                channelFactoryCache = instance.CacheExtension.GetFactoryCache();
                Fx.Assert(channelFactoryCache != null, "factory cache should be initialized either from the extension or from the globalcache");

                channelCacheSettings = instance.CacheExtension.ChannelSettings;

                // Get a ChannelFactoryReference (either cached or brand new)
                KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache> localLastUsedCacheItem = this.lastUsedFactoryCacheItem;
                if (object.ReferenceEquals(localLastUsedCacheItem.Value, instance.CacheExtension))
                {
                    if (localLastUsedCacheItem.Key != null && localLastUsedCacheItem.Key.TryAddReference())
                    {
                        cacheItem = localLastUsedCacheItem.Key;
                    }
                    else
                    {
                        // the item is invalid
                        this.lastUsedFactoryCacheItem = new KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache>(null, null);
                    }
                }

                if (cacheItem == null)
                {
                    // try retrieving the factoryreference directly from the factory cache 
                    cacheItem = channelFactoryCache.Take(this.cachedFactoryCacheKey);
                }
                if (cacheItem == null && TD.SendMessageChannelCacheMissIsEnabled())
                {
                    TD.SendMessageChannelCacheMiss();
                }
            }
            else
            {
                // not safe to share cache, do not cache anything
                channelCacheSettings = ChannelCacheSettings.EmptyCacheSettings;
            }

            ChannelFactoryReference newFactoryReference = null;
            if (cacheItem == null)
            {
                // nothing in our cache, we'll have to setup a new factory reference, which ClientSendAsyncResult will open asynchronously
                ServiceEndpoint targetEndpoint = this.CreateServiceEndpoint();
                // create a new ChannelFactoryReference that holds the channelfactory and a cache for its channels, 
                // cache settings are based on the channelcachesettings provided through the extension
                newFactoryReference = new ChannelFactoryReference(this.cachedFactoryCacheKey, targetEndpoint, channelCacheSettings);
            }

            instance.SetupFactoryReference(cacheItem, newFactoryReference, channelFactoryCache);

            if (this.onSendFailure == null)
            {
                this.onSendFailure = new FaultCallback(OnSendFailure);
            }

            if (instance.FactoryReference.NeedsOpen)
            {
                context.ScheduleActivity(this.openChannelFactory, OnChannelFactoryOpened, this.onSendFailure);
            }
            else
            {
                OnChannelFactoryOpenedCore(context, instance);
            }
        }

        void OnSendFailure(NativeActivityFaultContext context, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            // We throw the exception because we want this activity to abort
            // as well.  The abort path will take care of performing resource
            // clean-up (see Abort(NativeActivityAbortContext)).
            throw FxTrace.Exception.AsError(propagatedException);
        }

        void OnChannelFactoryOpened(NativeActivityContext context, ActivityInstance completedInstance)
        {
            SendMessageInstance instance = GetSendMessageInstance(context);
            Fx.Assert(instance != null, "Must have a SendMessageInstance here.");

            OnChannelFactoryOpenedCore(context, instance);
        }

        void OnChannelFactoryOpenedCore(NativeActivityContext context, SendMessageInstance instance)
        {
            // now that we know the factory is open, setup our client channel and pool reference
            instance.PopulateClientChannel();

            IContextChannel contextChannel = instance.ClientSendChannel as IContextChannel;
            instance.OperationContext = (contextChannel == null) ? null : new OperationContext(contextChannel);
            
            // Retrieve the CorrelationQueryBehavior from the serviceEndpoint that we used for ChannelFactoryCreation
            // we later look for CorrelationQueryBehavior.SendNames which actually gets initialized during ChannelFactory creation
            // 
            CorrelationQueryBehavior correlationQueryBehavior = instance.FactoryReference.CorrelationQueryBehavior;

            if (correlationQueryBehavior != null)
            {
                EnsureCorrelationBehaviorScopeName(context, correlationQueryBehavior);
                instance.RegisterCorrelationBehavior(correlationQueryBehavior);
            }

            // now that we have our op-context, invoke the callback that user might have added in the AEC in the previous activity 
            // e.g. distributed compensation activity will add this so that they can convert an execution property 
            // to an message properties, as will Transaction Flow
            instance.ProcessMessagePropertyCallbacks();

            // Add the ContextMessage Property if either CallBackContextMessageProperty or ContextMessageProperty is set
            // if both are set validate that the context is the same in both of them
            ContextMessageProperty contextMessageProperty = null;
            if (instance.CorrelationCallbackContext != null && instance.CorrelationContext != null)
            {
                // validate if the context is equivalent
                if (MessagingActivityHelper.CompareContextEquality(instance.CorrelationCallbackContext.Context, instance.CorrelationContext.Context))
                {
                    contextMessageProperty = new ContextMessageProperty(instance.CorrelationCallbackContext.Context);
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ContextMismatchInContextAndCallBackContext));
                }
            }
            else if (instance.CorrelationCallbackContext != null)
            {
                contextMessageProperty = new ContextMessageProperty(instance.CorrelationCallbackContext.Context);
            }
            else if (instance.CorrelationContext != null)
            {
                contextMessageProperty = new ContextMessageProperty(instance.CorrelationContext.Context);
            }

            if (contextMessageProperty != null)
            {
                contextMessageProperty.AddOrReplaceInMessage(instance.RequestOrReply);
            }

            // Add callback context Message property with instance id.
            // If binding contains ContextBindingElement with listenaddress set, the callback context message property will flow on the wire

            // Pull the instanceId from the CorrelationHandle, if it is already initialized, else create a new GUID.
            // we want to send the callback context only for the first message and when there is a FollowingContextCorrelation defined ( i.e., we are expecting a 
            // receive message back) or when there is an ambienthandle and the handle is not initalized. We will never use CorrelatesWith handle to initialize 
            // FollowingContext, since CorrelatesWith on the client side should always be used for a following correlation
            String contextValue;
            CorrelationHandle followingContextHandle = instance.ContextBasedCorrelationHandle != null ? instance.ContextBasedCorrelationHandle : instance.AmbientHandle;

            if (followingContextHandle != null && (followingContextHandle.Scope == null || followingContextHandle.Scope.IsInitialized == false))
            {
                // we are creating a new GUID for the context. As a practice,we don't want to send the WorkflowInstanceId over the wire
                contextValue = Guid.NewGuid().ToString();
                Dictionary<string, string> contextValues = new Dictionary<string, string>(1)
                    {
                        { ContextMessageProperty.InstanceIdKey, contextValue }
                    };
                new CallbackContextMessageProperty(contextValues).AddOrReplaceInMessage(instance.RequestOrReply);
            }

            // verify if we can complete Correlation intialization now
            if (instance.CorrelationSendNames != null)
            {
                // we're going to initialize request correlations later
                instance.RequestOrReply.Properties.Add(CorrelationCallbackMessageProperty.Name,
                    new MessageCorrelationCallbackMessageProperty(instance.CorrelationSendNames, instance));

                instance.CorrelationSynchronizer = new CorrelationSynchronizer();
            }
            else
            {
                InitializeCorrelations(context, instance);
            }

            if (instance.CorrelationSynchronizer != null)
            {
                context.ScheduleActivity(this.channelCorrelationCompletionWaiter, OnChannelCorrelationComplete, this.onSendFailure);
            }

            context.ScheduleActivity(this.openChannelAndSendMessage, OnClientSendComplete, this.onSendFailure);
        }

        void OnClientSendComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            SendMessageInstance instance = GetSendMessageInstance(context);

            if (instance.CorrelationSynchronizer == null || instance.CorrelationSynchronizer.NotifySendComplete())
            {
                // Either there was no correlation or the send completed
                // after the correlation processing so we need to do the
                // finalize
                FinalizeSendMessageCore(instance);
            }
        }

        Message InitializeCorrelations(NativeActivityContext context, SendMessageInstance instance)
        {
            if (instance.CorrelationKeyCalculator != null)
            {
                // first setup the key-based correlations, pass in the Correlation Initialiers and the AmbientHandle 
                // for associating the keys. 
                // For content based correlation, we will never initalize correlation with a selectHandle.It has to be either specified in a CorrelationInitalizer 
                // or should be an ambient handle
                // For contextbased correlation, selecthandle will be callbackHandle in case of Send and contextHandle in case of sendReply
                instance.RequestOrReply = MessagingActivityHelper.InitializeCorrelationHandles(context,
                    instance.ContextBasedCorrelationHandle, instance.AmbientHandle, this.correlationInitializers,
                    instance.CorrelationKeyCalculator, instance.RequestOrReply);
            }

            // then setup any channel based correlations as necessary
            // 
            if (instance.RequestContext != null)
            {
                // first check for an explicit association
                CorrelationHandle requestReplyCorrelationHandle = instance.GetExplicitRequestReplyCorrelationHandle(context, this.correlationInitializers);
                if (requestReplyCorrelationHandle != null)
                {
                    if (!requestReplyCorrelationHandle.TryRegisterRequestContext(context, instance.RequestContext))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TryRegisterRequestContextFailed));
                    }
                }
                else // if that fails, use the ambient handle. We do not use the CorrelatesWith handle for RequestReply correlation
                {
                    if (!this.IsOneWay)
                    {
                        // we have already validated this in SendMessageInstanceConstructor, just assert here
                        Fx.Assert(instance.AmbientHandle != null, "For two way send we need to have either a RequestReply correlation handle or an ambient handle");
                        if (!instance.AmbientHandle.TryRegisterRequestContext(context, instance.RequestContext))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TryRegisterRequestContextFailed));
                        }
                    }
                }
            }

            return instance.RequestOrReply;
        }

        void FinalizeSendMessageCore(SendMessageInstance instance)
        {
            Exception completionException = instance.GetCompletionException();

            if (completionException != null)
            {
                throw FxTrace.Exception.AsError(completionException);
            }
        }

        class OpenChannelFactory : AsyncCodeActivity
        {
            public OpenChannelFactory()
            {
            }

            public InArgument<VolatileSendMessageInstance> Instance
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument instanceArgument = new RuntimeArgument("Instance", typeof(VolatileSendMessageInstance), ArgumentDirection.In);
                if (this.Instance == null)
                {
                    this.Instance = new InArgument<VolatileSendMessageInstance>();
                }
                metadata.Bind(this.Instance, instanceArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    instanceArgument
                });
            }

            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                VolatileSendMessageInstance volatileInstance = this.Instance.Get(context);

                return new OpenChannelFactoryAsyncResult(volatileInstance.Instance, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                OpenChannelFactoryAsyncResult.End(result);
            }

            class OpenChannelFactoryAsyncResult : AsyncResult
            {
                static AsyncCompletion channelFactoryOpenCompletion = new AsyncCompletion(ChannelFactoryOpenCompletion);

                SendMessageInstance instance;

                public OpenChannelFactoryAsyncResult(SendMessageInstance instance, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.instance = instance;
                    bool completeSelf = false;

                    if (this.instance.FactoryReference.NeedsOpen)
                    {
                        IAsyncResult result = this.instance.FactoryReference.BeginOpen(PrepareAsyncCompletion(channelFactoryOpenCompletion), this);
                        if (result.CompletedSynchronously)
                        {
                            completeSelf = OnNewChannelFactoryOpened(result);
                        }
                    }
                    else
                    {
                        completeSelf = true;
                    }

                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenChannelFactoryAsyncResult>(result);
                }

                static bool ChannelFactoryOpenCompletion(IAsyncResult result)
                {
                    OpenChannelFactoryAsyncResult thisPtr = (OpenChannelFactoryAsyncResult)result.AsyncState;
                    return thisPtr.OnNewChannelFactoryOpened(result);
                }

                bool OnNewChannelFactoryOpened(IAsyncResult result)
                {
                    ObjectCacheItem<ChannelFactoryReference> newCacheItem =
                        this.instance.FactoryReference.EndOpen(result, this.instance.FactoryCache);
                    this.instance.RegisterNewCacheItem(newCacheItem);

                    return true;
                }

            }
        }

        class OpenChannelAndSendMessage : AsyncCodeActivity
        {
            public OpenChannelAndSendMessage()
            {
            }

            public InArgument<VolatileSendMessageInstance> Instance
            {
                get;
                set;
            }

            public InternalSendMessage InternalSendMessage
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument instanceArgument = new RuntimeArgument("Instance", typeof(VolatileSendMessageInstance), ArgumentDirection.In);
                if (this.Instance == null)
                {
                    this.Instance = new InArgument<VolatileSendMessageInstance>();
                }
                metadata.Bind(this.Instance, instanceArgument);
                metadata.AddArgument(instanceArgument);
            }

            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                VolatileSendMessageInstance volatileInstance = this.Instance.Get(context);
                Transaction transaction = null;

                RuntimeTransactionHandle handle = context.GetProperty<RuntimeTransactionHandle>();
                if (handle != null)
                {
                    transaction = handle.GetCurrentTransaction(context);
                }

                return new OpenChannelAndSendMessageAsyncResult(InternalSendMessage, volatileInstance.Instance, transaction, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                OpenChannelAndSendMessageAsyncResult.End(result);
            }

            class OpenChannelAndSendMessageAsyncResult : TransactedAsyncResult
            {
                static AsyncCompletion onChannelOpened = new AsyncCompletion(OnChannelOpened);
                static AsyncCompletion onChannelSendComplete = new AsyncCompletion(OnChannelSendComplete);
                static AsyncCallback onChannelReceiveReplyCompleted = Fx.ThunkCallback(OnChannelReceiveReplyComplete);

                SendMessageInstance instance;
                InternalSendMessage internalSendMessage;
                IChannel channel;
                Transaction currentTransactionContext;
                Guid ambientActivityId;

                //This is used to create a blocking dependent clone to synchronize the transaction commit processing with the completion of the aborting clone
                //that is created in this async result.
                DependentTransaction dependentClone;

                public OpenChannelAndSendMessageAsyncResult(InternalSendMessage internalSendMessage, SendMessageInstance instance, Transaction currentTransactionContext, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.internalSendMessage = internalSendMessage;
                    this.instance = instance;
                    this.channel = this.instance.ClientSendChannel;
                    this.currentTransactionContext = currentTransactionContext;
                    
                    bool completeSelf = false;

                    //channel is still in created state, we need to open it
                    if (this.channel.State == CommunicationState.Created)
                    {
                        // Disable ContextManager before channel is opened
                        IContextManager contextManager = this.channel.GetProperty<IContextManager>();
                        if (contextManager != null)
                        {
                            contextManager.Enabled = false;
                        }

                        IAsyncResult result = this.channel.BeginOpen(PrepareAsyncCompletion(onChannelOpened), this);
                        if (result.CompletedSynchronously)
                        {
                            completeSelf = OnChannelOpened(result);
                        }
                    }
                    else
                    {
                        // channel already opened & retrieved from cache
                        // we don't have to do anything with ChannelOpen
                        completeSelf = BeginSendMessage();
                    }

                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenChannelAndSendMessageAsyncResult>(result);
                }

                static bool OnChannelOpened(IAsyncResult result)
                {
                    OpenChannelAndSendMessageAsyncResult thisPtr = (OpenChannelAndSendMessageAsyncResult)result.AsyncState;
                    thisPtr.channel.EndOpen(result);
                    return thisPtr.BeginSendMessage();
                }

                bool BeginSendMessage()
                {
                    IAsyncResult result = null;
                    bool requestSucceeded = false;
                    OperationContext oldContext = OperationContext.Current;
                    bool asyncSend = !this.internalSendMessage.IsOneWay;

                    try
                    {
                        OperationContext.Current = this.instance.OperationContext;

                        if (TraceUtility.MessageFlowTracingOnly)
                        {
                            //set the E2E activity ID
                            DiagnosticTraceBase.ActivityId = this.instance.E2EActivityId;
                        }

                        using (PrepareTransactionalCall(this.currentTransactionContext))
                        {
                            if (asyncSend)
                            {
                                //If there is a transaction that we could be flowing out then we create this blocking clone to sync with the commit processing.
                                if (this.currentTransactionContext != null)
                                {
                                    this.dependentClone = this.currentTransactionContext.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                                }

                                this.instance.RequestContext.EnsureAsyncWaitHandle();

                                result = ((IRequestChannel)this.channel).BeginRequest(this.instance.RequestOrReply, onChannelReceiveReplyCompleted, this);
                                if (result.CompletedSynchronously)
                                {
                                    Message reply = ((IRequestChannel)this.channel).EndRequest(result);
                                    this.instance.RequestContext.ReceiveReply(this.instance.OperationContext, reply);
                                }
                            }
                            else
                            {
                                result = ((IOutputChannel)this.channel).BeginSend(this.instance.RequestOrReply, PrepareAsyncCompletion(onChannelSendComplete), this);
                                if (result.CompletedSynchronously)
                                {
                                    ((IOutputChannel)this.channel).EndSend(result);
                                }
                            }

                            requestSucceeded = true;
                        }
                    }
                    finally
                    {
                        OperationContext.Current = oldContext;

                        if (!requestSucceeded)
                        {
                            //if we did not succeed, complete the blocking clone anyway if we created it
                            if (this.dependentClone != null)
                            {
                                this.dependentClone.Complete();
                                this.dependentClone = null;
                            }
                            this.channel.Abort();
                        }

                        if (result != null && result.CompletedSynchronously)
                        {
                            //if we are done synchronously, we need to complete a blocking dependent clone if we created one (asyncSend case)
                            if (this.dependentClone != null)
                            {
                                this.dependentClone.Complete();
                                this.dependentClone = null;
                            }
                            this.internalSendMessage.CleanupResources(this.instance);
                        }
                    }

                    if (asyncSend)
                    {
                        return true;
                    }
                    else
                    {
                        return SyncContinue(result);
                    }
                }

                static void OnChannelReceiveReplyComplete(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    OpenChannelAndSendMessageAsyncResult thisPtr = (OpenChannelAndSendMessageAsyncResult)result.AsyncState;

                    OperationContext oldContext = OperationContext.Current;

                    Message reply = null;
                    bool requestSucceeded = false;

                    try
                    {
                        OperationContext.Current = thisPtr.instance.OperationContext;

                        thisPtr.TraceActivityData();

                        System.Transactions.TransactionScope scope = TransactionHelper.CreateTransactionScope(thisPtr.currentTransactionContext);
                        try
                        {
                            Fx.Assert(thisPtr.channel is IRequestChannel, "Channel must be of IRequestChannel type!");

                            reply = ((IRequestChannel)thisPtr.channel).EndRequest(result);

                            //
                            thisPtr.instance.RequestContext.ReceiveAsyncReply(thisPtr.instance.OperationContext, reply, null);

                            requestSucceeded = true;
                        }
                        finally
                        {
                            TransactionHelper.CompleteTransactionScope(ref scope);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        thisPtr.instance.RequestContext.ReceiveAsyncReply(thisPtr.instance.OperationContext, null, exception);
                    }
                    finally
                    {
                        //Complete the blocking dependent clone created before the async call was made.
                        if (thisPtr.dependentClone != null)
                        {
                            thisPtr.dependentClone.Complete();
                            thisPtr.dependentClone = null;
                        }

                        OperationContext.Current = oldContext;

                        if (!requestSucceeded)
                        {
                            thisPtr.channel.Abort();
                        }

                        thisPtr.internalSendMessage.CleanupResources(thisPtr.instance);
                    }
                }

                static bool OnChannelSendComplete(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return true;
                    }

                    OpenChannelAndSendMessageAsyncResult thisPtr = (OpenChannelAndSendMessageAsyncResult)result.AsyncState;

                    OperationContext oldContext = OperationContext.Current;

                    try
                    {
                        OperationContext.Current = thisPtr.instance.OperationContext;

                        thisPtr.TraceActivityData();

                        System.Transactions.TransactionScope scope = TransactionHelper.CreateTransactionScope(thisPtr.currentTransactionContext);
                        try
                        {
                            Fx.Assert(thisPtr.channel is IOutputChannel, "Channel must be of IOutputChannel type!");

                            ((IOutputChannel)thisPtr.channel).EndSend(result);
                        }
                        finally
                        {
                            TransactionHelper.CompleteTransactionScope(ref scope);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        // stash away the exception to be retrieved in FinalizeSendMessageCore
                        thisPtr.instance.RequestContext.Exception = exception;
                    }
                    finally
                    {
                        OperationContext.Current = oldContext;
                        thisPtr.internalSendMessage.CleanupResources(thisPtr.instance);
                    }

                    return true;
                }

                void TraceActivityData()
                {
                    if (TraceUtility.ActivityTracing)
                    {
                        if (TD.StopSignpostEventIsEnabled())
                        {
                            TD.StopSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                    { MessagingActivityHelper.ActivityName, this.instance.Activity.DisplayName },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                    { MessagingActivityHelper.ActivityInstanceId, this.instance.ActivityInstanceId }
                                }));
                        }
                        FxTrace.Trace.SetAndTraceTransfer(this.ambientActivityId, true);
                        this.ambientActivityId = Guid.Empty;
                    }
                    if (TD.WfMessageSentIsEnabled())
                    {
                        // 
                        EventTraceActivity eta = new EventTraceActivity();
                        if (this.instance.E2EActivityId != Guid.Empty)
                        {
                            eta.SetActivityId(this.instance.E2EActivityId);
                        }
                        TD.WfMessageSent(eta, this.ambientActivityId);
                    }
                }
            }
        }

        class WaitOnChannelCorrelation : AsyncCodeActivity
        {
            public WaitOnChannelCorrelation()
            {
            }

            public InArgument<VolatileSendMessageInstance> Instance
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument instanceArgument = new RuntimeArgument("Instance", typeof(VolatileSendMessageInstance), ArgumentDirection.In);
                if (this.Instance == null)
                {
                    this.Instance = new InArgument<VolatileSendMessageInstance>();
                }
                metadata.Bind(this.Instance, instanceArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    instanceArgument
                });
            }

            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                VolatileSendMessageInstance volatileInstance = this.Instance.Get(context);

                Fx.Assert(volatileInstance.Instance != null, "This should not have gone through a persistence episode yet.");

                return new WaitOnChannelCorrelationAsyncResult(volatileInstance.Instance.CorrelationSynchronizer, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                WaitOnChannelCorrelationAsyncResult.End(result);
            }

            class WaitOnChannelCorrelationAsyncResult : AsyncResult
            {
                CorrelationSynchronizer synchronizer;

                public WaitOnChannelCorrelationAsyncResult(CorrelationSynchronizer synchronizer, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.synchronizer = synchronizer;

                    if (synchronizer.IsChannelWorkComplete)
                    {
                        Complete(true);
                    }
                    else
                    {
                        if (synchronizer.SetWorkflowNotificationCallback(new Action(OnChannelCorrelationComplete)))
                        {
                            // The bool flipped just before we set the action so
                            // we're actually complete.  The contract is that the
                            // action will never be raised if Set returns true.
                            Complete(true);
                        }
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WaitOnChannelCorrelationAsyncResult>(result);
                }

                void OnChannelCorrelationComplete()
                {
                    Complete(false);
                }
            }
        }

       internal class CorrelationSynchronizer
        {
            Action onRequestSetByChannel;
            Action<Message> onWorkflowCorrelationProcessingComplete;
            object thisLock;
            Completion completion;

            public CorrelationSynchronizer()
            {
                this.thisLock = new object();
            }

            public bool IsChannelWorkComplete
            {
                get;
                private set;
            }

            public Message UpdatedMessage
            {
                get;
                private set;
            }

            public void NotifyRequestSetByChannel(Action<Message> onWorkflowCorrelationProcessingComplete)
            {
                Fx.Assert(onWorkflowCorrelationProcessingComplete != null, "Must have a non-null callback.");
                Action toCall = null;

                lock (this.thisLock)
                {
                    this.IsChannelWorkComplete = true;
                    this.onWorkflowCorrelationProcessingComplete = onWorkflowCorrelationProcessingComplete;

                    toCall = this.onRequestSetByChannel;
                }

                if (toCall != null)
                {
                    toCall();
                }
            }

            public void NotifyMessageUpdatedByWorkflow(Message message)
            {
                this.UpdatedMessage = message;
            }

            public bool NotifyWorkflowCorrelationProcessingComplete()
            {
                Fx.Assert(this.onWorkflowCorrelationProcessingComplete != null, "This must be set before this can be called.");

                bool result = false;

                lock (this.thisLock)
                {
                    if (this.completion == Completion.SendComplete)
                    {
                        // The send has already completed so we are responsible for
                        // making sure FinalizeSendMessage is called.
                        result = true;
                    }
                    else
                    {
                        Fx.Assert(this.completion == Completion.None, "We should be the first one to complete.");

                        this.completion = Completion.CorrelationComplete;
                    }
                }

                this.onWorkflowCorrelationProcessingComplete(this.UpdatedMessage);

                return result;
            }

            public bool NotifySendComplete()
            {
                bool result = false;
                lock (this.thisLock)
                {
                    if (this.completion == Completion.CorrelationComplete)
                    {
                        // The correlation has already finished so we are responsible for
                        // making sure that FinalizeSendMessage is called.
                        result = true;
                    }
                    else
                    {
                        Fx.Assert(this.completion == Completion.None, "We should be the first one to complete.");

                        this.completion = Completion.SendComplete;
                    }
                }

                return result;
            }

            // Returns true if the channel work is actually done.  If this
            // returns true then the passed in Action will never be called.
            public bool SetWorkflowNotificationCallback(Action onRequestSetByChannel)
            {
                Fx.Assert(onRequestSetByChannel != null, "Must have a non-null callback.");

                bool result = false;
                lock (this.thisLock)
                {
                    result = this.IsChannelWorkComplete;
                    this.onRequestSetByChannel = onRequestSetByChannel;
                }

                return result;
            }

            // This three state enum allows us to determine whether
            // we are the first or second code path.  The second
            // code path needs finalize the send.
            enum Completion
            {
                None,
                SendComplete,
                CorrelationComplete
            }
        }

        // This class defines the instance data that used to store intermediate states
        // during the volatile async operation of sending a message.
        internal class SendMessageInstance
        {
            CorrelationHandle explicitChannelCorrelationHandle;
            IList<ISendMessageCallback> sendMessageCallbacks;
            ChannelFactoryReference factoryReference;
            ObjectCacheItem<ChannelFactoryReference> cacheItem;
            ObjectCache<FactoryCacheKey, ChannelFactoryReference> factoryCache;
            
            readonly InternalSendMessage parent;
            bool isUsingCacheFromExtension;

            // needed so that we can return our ClientSendChannel to the pool under Dispose
            ObjectCacheItem<Pool<IChannel>> clientChannelPool;

            public SendMessageInstance(InternalSendMessage parent, NativeActivityContext context)
            {
                this.parent = parent;

                // setup both our following state as well as any anonymous response information
                CorrelationHandle correlatesWith = (parent.CorrelatesWith == null) ? null : parent.CorrelatesWith.Get(context);
                if (correlatesWith != null && !correlatesWith.IsInitalized())
                {
                    // if send or sendReply has a correlatesWith, it should always be initialized with either content or with callbackcontext, context or 
                    // ResponseContext
                    throw FxTrace.Exception.AsError(new ValidationException(SR.SendWithUninitializedCorrelatesWith(this.parent.OperationName ?? string.Empty)));
                }

                if (correlatesWith == null)
                {
                    this.AmbientHandle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                    correlatesWith = this.AmbientHandle;
                }

                this.CorrelatesWith = correlatesWith;

                if (!parent.IsSendReply)
                {
                    // we're a client-side request

                    // Validate correlation handle
                    CorrelationHandle requestReplyCorrelationHandle = GetExplicitRequestReplyCorrelationHandle(context, parent.correlationInitializers);
                    if (parent.IsOneWay)
                    {
                        if (requestReplyCorrelationHandle != null)
                        {
                            // this is a one-way send , we should not have a RequestReply Correlation initializer
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RequestReplyHandleShouldNotBePresentForOneWay));

                        }
                    }
                    else // two-way send
                    {
                        if (requestReplyCorrelationHandle == null && this.AmbientHandle == null)
                        {
                            this.AmbientHandle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                            if (this.AmbientHandle == null)
                            {
                                // we neither have a channelHandle nor an ambientHandle
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                    SR.SendMessageNeedsToPairWithReceiveMessageForTwoWayContract(parent.OperationName ?? string.Empty)));
                            }
                        }
                    }

                    // Formatter and OperationContract should be  removed from CorrelationRequestContext
                    // This will be done when SendMessage/ReceiveMessage is completely removed from the code base
                    this.RequestContext = new CorrelationRequestContext();

                    // callback correlationHandle is used for initalizing context based correlation 
                    this.ContextBasedCorrelationHandle = CorrelationHandle.GetExplicitCallbackCorrelation(context, parent.correlationInitializers);

                    // by default we use the channel factory cache from the extension
                    isUsingCacheFromExtension = true;
                }
                else
                {
                    // we are a server-side following send
                    CorrelationResponseContext responseContext;
                    if (correlatesWith == null || !correlatesWith.TryAcquireResponseContext(context, out responseContext))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CorrelatedContextRequiredForAnonymousSend));
                    }

                    // Contract inference logic should validate that the Receive and Following send do not have conflicting data(e.g., OperationName)

                    this.ResponseContext = responseContext;

                    // in case of Context based correlation, we use context handle to initialize correlation
                    this.ContextBasedCorrelationHandle = CorrelationHandle.GetExplicitContextCorrelation(context, parent.correlationInitializers);
                }

                this.sendMessageCallbacks = MessagingActivityHelper.GetCallbacks<ISendMessageCallback>(context.Properties);

                if (TraceUtility.MessageFlowTracing)
                {
                    this.ActivityInstanceId = context.ActivityInstanceId;
                }
            }

            public InternalSendMessage Activity
            {
                get
                {
                    return this.parent;
                }
            }

            public CorrelationHandle CorrelatesWith
            {
                get;
                private set;
            }

            public CorrelationHandle AmbientHandle
            {
                get;
                private set;
            }

            public CorrelationHandle ContextBasedCorrelationHandle
            {
                get;
                private set;
            }

            public EndpointAddress EndpointAddress
            {
                get;
                set;
            }

            public IChannel ClientSendChannel
            {
                get;
                private set;
            }

            public CorrelationSynchronizer CorrelationSynchronizer
            {
                get;
                set;
            }

            public Message RequestOrReply
            {
                get;
                set;
            }

            public OperationContext OperationContext
            {
                get;
                set;
            }

            public CorrelationRequestContext RequestContext
            {
                get;
                private set;
            }

            // This is required for setting adding the ChannelFactory to the cache once it is opened
            public ObjectCache<FactoryCacheKey, ChannelFactoryReference> FactoryCache
            {
                get
                {
                    return this.factoryCache;
                }
            }

            // This is required for setting adding the ChannelFactory to the cache once it is opened
            public SendMessageChannelCache CacheExtension
            {
                get;
                set;
            }

            //This is required for returning it to the cache after use
            public ChannelFactoryReference FactoryReference
            {
                get
                {
                    return this.factoryReference;
                }
            }

            public CorrelationResponseContext ResponseContext
            {
                get;
                private set;
            }

            public CorrelationKeyCalculator CorrelationKeyCalculator
            {
                get;
                private set;
            }

            public CorrelationCallbackContext CorrelationCallbackContext
            {
                get;
                set;
            }

            public CorrelationContext CorrelationContext
            {
                get;
                set;
            }

            public Guid AmbientActivityId
            {
                get;
                set;
            }

            public ICollection<string> CorrelationSendNames
            {
                get;
                private set;
            }

            public Guid E2EActivityId
            {
                get;
                set;
            }

            public string ActivityInstanceId
            {
                get;
                private set;
            }

            public bool IsCorrelationInitialized
            {
                get;
                set;
            }

            public void SetupFactoryReference(ObjectCacheItem<ChannelFactoryReference> cacheItem, ChannelFactoryReference newFactoryReference, ObjectCache<FactoryCacheKey, ChannelFactoryReference> factoryCache)
            {
                this.factoryCache = factoryCache;
                if (this.factoryCache == null)
                {
                    isUsingCacheFromExtension = false;
                }
                if (cacheItem != null)
                {
                    // we found the item in our cache
                    Fx.Assert(newFactoryReference == null, "need one of cacheItem or newFactoryReference");
                    Fx.Assert(cacheItem.Value != null, "should have valid value");
                    this.cacheItem = cacheItem;
                    this.factoryReference = cacheItem.Value;
                }
                else
                {
                    Fx.Assert(newFactoryReference != null, "need one of cacheItem or newFactoryReference");
                    this.factoryReference = newFactoryReference;
                }
            }

            public void RegisterNewCacheItem(ObjectCacheItem<ChannelFactoryReference> newCacheItem)
            {
                Fx.Assert(this.cacheItem == null, "should only be called for new cache items");
                this.cacheItem = newCacheItem;
            }

            public CorrelationHandle GetExplicitRequestReplyCorrelationHandle(NativeActivityContext context, Collection<CorrelationInitializer> additionalCorrelations)
            {
                if (this.explicitChannelCorrelationHandle == null)
                {
                    this.explicitChannelCorrelationHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(context, additionalCorrelations);
                }
                return this.explicitChannelCorrelationHandle;
            }

            public void RegisterCorrelationBehavior(CorrelationQueryBehavior correlationBehavior)
            {
                Fx.Assert(correlationBehavior != null, "caller must verify");
                if (correlationBehavior.ScopeName != null)
                {
                    CorrelationKeyCalculator keyCalculator = correlationBehavior.GetKeyCalculator();
                    if (keyCalculator != null)
                    {
                        this.CorrelationKeyCalculator = keyCalculator;
                        if (this.RequestContext != null)
                        {
                            this.RequestContext.CorrelationKeyCalculator = keyCalculator;
                            // for requests, determine if we should be using the correlation callback
                            if (correlationBehavior.SendNames != null && correlationBehavior.SendNames.Count > 0)
                            {
                                this.CorrelationSendNames = correlationBehavior.SendNames;
                            }
                        }
                    }
                }
            }

            public void ProcessMessagePropertyCallbacks()
            {
                if (this.sendMessageCallbacks != null)
                {
                    foreach (ISendMessageCallback sendMessageCallback in this.sendMessageCallbacks)
                    {
                        sendMessageCallback.OnSendMessage(this.OperationContext);
                    }
                }
            }

            public void PopulateClientChannel()
            {
                Fx.Assert(this.ClientSendChannel == null && this.clientChannelPool == null, "should only be called once per instance");
                this.ClientSendChannel = this.FactoryReference.TakeChannel(this.EndpointAddress, out this.clientChannelPool);
            }

            public void Dispose()
            {
                if (this.ClientSendChannel != null)
                {
                    Fx.Assert(this.FactoryReference != null, "Must have a factory reference.");
                    this.FactoryReference.ReturnChannel(this.ClientSendChannel, this.clientChannelPool);
                    this.ClientSendChannel = null;
                    this.clientChannelPool = null;
                }

                if (this.cacheItem != null)
                {
                    this.cacheItem.ReleaseReference();

                    // if we are using the FactoryCache from the extension, store the last used cacheItem and extension
                    if (this.isUsingCacheFromExtension)
                    {
                        this.parent.lastUsedFactoryCacheItem = new KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache>(this.cacheItem, this.CacheExtension);
                    }
                    this.cacheItem = null;
                }
            }

            public Exception GetCompletionException()
            {
                if (this.RequestContext != null)
                {
                    // We got an exception trying to send message or receive a reply
                    // Scenario: ContractFilterMismatch at serverside if the message action is not matched correctly
                    return this.RequestContext.Exception;
                }
                else
                {
                    return this.ResponseContext.Exception;
                }
            }
        }

        class MessageCorrelationCallbackMessageProperty : CorrelationCallbackMessageProperty
        {
            public MessageCorrelationCallbackMessageProperty(ICollection<string> neededData, SendMessageInstance instance)
                : base(neededData)
            {
                this.Instance = instance;
            }

            protected MessageCorrelationCallbackMessageProperty(MessageCorrelationCallbackMessageProperty callback)
                : base(callback)
            {
                this.Instance = callback.Instance;
            }

            public SendMessageInstance Instance
            {
                get;
                private set;
            }

            public override IMessageProperty CreateCopy()
            {
                return new MessageCorrelationCallbackMessageProperty(this);
            }

            protected override IAsyncResult OnBeginFinalizeCorrelation(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new FinalizeCorrelationAsyncResult(this, message, callback, state);
            }

            protected override Message OnEndFinalizeCorrelation(IAsyncResult result)
            {
                return FinalizeCorrelationAsyncResult.End(result);
            }

            protected override Message OnFinalizeCorrelation(Message message, TimeSpan timeout)
            {
                return OnEndFinalizeCorrelation(OnBeginFinalizeCorrelation(message, timeout, null, null));
            }

            class FinalizeCorrelationAsyncResult : AsyncResult
            {
                Message message;
                Completion completion;

                object thisLock;

                public FinalizeCorrelationAsyncResult(MessageCorrelationCallbackMessageProperty property, Message message,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    bool completeSelf = false;
                    if (property.Instance.IsCorrelationInitialized)
                    {
                        // we do not modify the message since correlation is not calculated again
                        this.message = message;
                        completeSelf = true;
                    }
                    else
                    {
                        property.Instance.IsCorrelationInitialized = true;
                        this.thisLock = new object();

                        property.Instance.RequestOrReply = message;

                        property.Instance.CorrelationSynchronizer.NotifyRequestSetByChannel(new Action<Message>(OnWorkflowCorrelationProcessingComplete));

                        // We have to do this dance with the lock because
                        // we aren't sure if we've been running sync or not.
                        // NOTE: It is possible for us to go async and
                        // still decide we're completing sync.  This is fine
                        // as it does not violate the async pattern since
                        // the work is done by the time Begin completes.
                        completeSelf = false;

                        lock (this.thisLock)
                        {
                            if (completion == Completion.WorkflowCorrelationProcessingComplete)
                            {
                                completeSelf = true;
                            }
                            else
                            {
                                Fx.Assert(this.completion == Completion.None, "We must be not ready then.");

                                this.completion = Completion.ConstructorComplete;
                            }
                        }
                    }
                    if (completeSelf)
                    {
                        Complete(true);
                    }

                }

                void OnWorkflowCorrelationProcessingComplete(Message updatedMessage)
                {
                    this.message = updatedMessage;

                    // We have to do this dance with the lock because
                    // we aren't sure if we've been running sync or not.
                    // NOTE: It is possible for us to go async and
                    // still decide we're completing sync.  This is fine
                    // as it does not violate the async pattern since
                    // the work is done by the time Begin completes.
                    bool completeSelf = false;

                    lock (this.thisLock)
                    {
                        if (this.completion == Completion.ConstructorComplete)
                        {
                            completeSelf = true;
                        }
                        else
                        {
                            Fx.Assert(this.completion == Completion.None, "We must be not ready then.");

                            this.completion = Completion.WorkflowCorrelationProcessingComplete;
                        }
                    }

                    if (completeSelf)
                    {
                        Complete(false);
                    }
                }

                public static Message End(IAsyncResult result)
                {
                    FinalizeCorrelationAsyncResult thisPtr = AsyncResult.End<FinalizeCorrelationAsyncResult>(result);
                    return thisPtr.message;
                }

                // This three state enum allows us to determine whether
                // we are the first or second code path.  The second
                // code path needs to complete the async result.
                enum Completion
                {
                    None,
                    ConstructorComplete,
                    WorkflowCorrelationProcessingComplete
                }
            }
        }

        [DataContract]
        internal class VolatileSendMessageInstance
        {
            public VolatileSendMessageInstance()
            {
            }

            // Note that we do not mark this DataMember since we don’t want it to be serialized
            public SendMessageInstance Instance { get; set; }
        }

        // Represents an item in our object cache. Stores a ChannelFactory and an associated pool of channels
        internal sealed class ChannelFactoryReference : IDisposable
        {
            static AsyncCallback onDisposeCommunicationObject = Fx.ThunkCallback(new AsyncCallback(OnDisposeCommunicationObject));
            Action<Pool<IChannel>> disposeChannelPool;
            readonly FactoryCacheKey factoryKey;
            readonly ServiceEndpoint targetEndpoint;
            ChannelFactory channelFactory;
            ObjectCache<EndpointAddress, Pool<IChannel>> channelCache;
            CorrelationQueryBehavior correlationQueryBehavior;
            Func<Pool<IChannel>> createChannelCacheItem;

            // Aborting a channel that is in the middle of closing can cause an ObjectDisposedException in the Close.
            // We need to prevent DisposeCommunicationObject(ChannelFactory) from racing with a call to 
            // DisposeCommunicationObject()on an individual channel.
            // This lock will be used to synchronize calls into DisposeCommunicationObject method.
            object disposeLock = new object();

            public ChannelFactoryReference(FactoryCacheKey factoryKey, ServiceEndpoint targetEndpoint, ChannelCacheSettings channelCacheSettings)
            {
                Fx.Assert(channelCacheSettings != null, " channelCacheSettings should not be null");
                Fx.Assert(factoryKey != null, " factoryKey should not be null");
                Fx.Assert(targetEndpoint != null, " targetEndpoint should not be null");

                this.factoryKey = factoryKey;
                this.targetEndpoint = targetEndpoint;
                                
                if (factoryKey.IsOperationContractOneWay)
                {
                    this.channelFactory = new ChannelFactory<IOutputChannel>(targetEndpoint);
                }
                else
                {
                    this.channelFactory = new ChannelFactory<IRequestChannel>(targetEndpoint);
                }

                this.channelFactory.UseActiveAutoClose = true;
                this.channelFactory.Credentials.Windows.AllowedImpersonationLevel = factoryKey.TokenImpersonationLevel;

                ObjectCacheSettings channelSettings = new ObjectCacheSettings
                {
                    CacheLimit = channelCacheSettings.MaxItemsInCache,
                    IdleTimeout = channelCacheSettings.IdleTimeout,
                    LeaseTimeout = channelCacheSettings.LeaseTimeout
                };

                this.disposeChannelPool = new Action<Pool<IChannel>>(this.DisposeChannelPool);

                // our channel cache is keyed solely on endpoint since we don't allow the via to be dynamic
                // for a ChannelFactoryReference
                this.channelCache = new ObjectCache<EndpointAddress, Pool<IChannel>>(channelSettings)
                {
                    DisposeItemCallback = this.disposeChannelPool
                };
                this.createChannelCacheItem = () => new Pool<IChannel>(channelCacheSettings.MaxItemsInCache);
            }

            public CorrelationQueryBehavior CorrelationQueryBehavior
            {
                get
                {
                    if (this.correlationQueryBehavior == null)
                    {
                        this.correlationQueryBehavior = this.targetEndpoint.Behaviors.Find<CorrelationQueryBehavior>();
                    }

                    return this.correlationQueryBehavior;
                }
            }
            
            // As a perf optimization, we provide this property to avoid async result/callback creations
            public bool NeedsOpen
            {
                get
                {
                    return this.channelFactory.State == CommunicationState.Created;
                }
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                Fx.Assert(NeedsOpen, "caller should check NeedsOpen first");
                return this.channelFactory.BeginOpen(callback, state);
            }

            // after open we should be added to a cache if one is provided
            public ObjectCacheItem<ChannelFactoryReference> EndOpen(IAsyncResult result, ObjectCache<FactoryCacheKey, ChannelFactoryReference> factoryCache)
            {
                this.channelFactory.EndOpen(result);

                ObjectCacheItem<ChannelFactoryReference> cacheItem = null;
                if (factoryCache != null)
                {
                    cacheItem = factoryCache.Add(this.factoryKey, this);
                }

                return cacheItem;
            }

            [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DisposableFieldsShouldBeDisposed,
                Justification = "disposable field is being disposed using DisposeCommunicationObject")]
            public void Dispose()
            {
                lock (this.disposeLock)
                {
                    DisposeCommunicationObject(this.channelFactory);
                }
            }

            public IChannel TakeChannel(EndpointAddress endpointAddress, out ObjectCacheItem<Pool<IChannel>> channelPool)
            {
                channelPool = this.channelCache.Take(endpointAddress, this.createChannelCacheItem);
                Fx.Assert(channelPool != null, "Take with delegate should always return a valid Item");

                IChannel result = null;

                lock (channelPool.Value)
                {
                    result = channelPool.Value.Take();
                }

                // make an effort to kill stale channels
                ServiceChannel serviceChannel = result as ServiceChannel;
                if (result != null && (result.State != CommunicationState.Opened || (serviceChannel != null && serviceChannel.Binder.Channel.State != CommunicationState.Opened)))
                {
                    result.Abort();
                    result = null;
                }

                if (result == null)
                {
                    Uri via = null;

                    // service endpoint always sets the ListenUri, which will break default callback-context behavior
                    if (this.targetEndpoint.Address != null && this.targetEndpoint.Address.Uri != this.targetEndpoint.ListenUri)
                    {
                        via = this.targetEndpoint.ListenUri;
                    }

                    if (this.factoryKey.IsOperationContractOneWay)
                    {
                        result = ((ChannelFactory<IOutputChannel>)this.channelFactory).CreateChannel(endpointAddress, via);
                    }
                    else
                    {
                        result = ((ChannelFactory<IRequestChannel>)this.channelFactory).CreateChannel(endpointAddress, via);
                    }
                }

                if (!(result is ServiceChannel))
                {
                    result = ServiceChannelFactory.GetServiceChannel(result);
                }

                return result;
            }

            public void ReturnChannel(IChannel channel, ObjectCacheItem<Pool<IChannel>> channelPool)
            {
                bool shouldDispose = channel.State != CommunicationState.Opened;

                // channel is in open state, try returning it to the pool
                if (!shouldDispose)
                {
                    lock (channelPool.Value)
                    {
                        shouldDispose = !channelPool.Value.Return(channel);
                    }
                }

                if (shouldDispose)
                {
                    lock (this.disposeLock)
                    {
                        if (this.channelFactory.State != CommunicationState.Closed &&
                            this.channelFactory.State != CommunicationState.Closing)
                        {
                            // not caching the channel, so we need to close it
                            DisposeCommunicationObject(channel);
                        }
                    }
                }

                // and return our cache item
                channelPool.ReleaseReference();
            }

            public void DisposeChannelPool(Pool<IChannel> channelPool)
            {
                IChannel channel;

                // we don't need to lock the Take from the Pool here since no one will be accessing this anymore
                // Dispose will be called under a lock from the ObjectCacheItem
                while ((channel = channelPool.Take()) != null)
                {
                    lock (this.disposeLock)
                    {
                        if (this.channelFactory.State != CommunicationState.Closed &&
                            this.channelFactory.State != CommunicationState.Closing)
                        {
                            DisposeCommunicationObject(channel);
                        }
                    }
                }
            }

            static void DisposeCommunicationObject(ICommunicationObject communicationObject)
            {
                bool success = false;
                try
                {
                    if (communicationObject.State == CommunicationState.Opened)
                    {
                        IAsyncResult result = communicationObject.BeginClose(ServiceDefaults.CloseTimeout, onDisposeCommunicationObject, communicationObject);
                        if (result.CompletedSynchronously)
                        {
                            communicationObject.EndClose(result);
                        }
                        success = true;
                    }
                }
                catch (CommunicationException)
                {
                    // expected, we'll abort
                }
                catch (TimeoutException)
                {
                    // expected, we'll abort
                }
                finally
                {
                    if (!success)
                    {
                        communicationObject.Abort();
                    }
                }
            }

            static void OnDisposeCommunicationObject(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                ICommunicationObject communicationObject = (ICommunicationObject)result.AsyncState;

                bool success = false;
                try
                {
                    communicationObject.EndClose(result);
                    success = true;
                }
                catch (CommunicationException)
                {
                    // expected, we'll abort
                }
                catch (TimeoutException)
                {
                    // expected, we'll abort
                }
                catch (ObjectDisposedException)
                {
                    // expected,
                    // ObjectDisposedException may be thrown if you try to abort ClientSecurityDuplexSessionChannel that is in the middle of closing.
                    // we'll abort
                }
                finally
                {
                    if (!success)
                    {
                        communicationObject.Abort();
                    }
                }
            }
        }

        internal class FactoryCacheKey : IEquatable<FactoryCacheKey>
        {
            Endpoint endpoint;
            bool isOperationContractOneWay;
            
            TokenImpersonationLevel tokenImpersonationLevel;
            ContractDescription contract;
            Collection<CorrelationQuery> correlationQueries;
            string endpointConfigurationName;

            public FactoryCacheKey(Endpoint endpoint, string endpointConfigurationName, bool isOperationOneway,
                TokenImpersonationLevel tokenImpersonationLevel, ContractDescription contractDescription,
                ICollection<CorrelationQuery> correlationQueries)
            {
                this.endpoint = endpoint;
                this.endpointConfigurationName = endpointConfigurationName;
                this.isOperationContractOneWay = isOperationOneway;
                this.tokenImpersonationLevel = tokenImpersonationLevel;
                this.contract = contractDescription;

                if (correlationQueries != null)
                {
                    this.correlationQueries = new Collection<CorrelationQuery>();
                    foreach (CorrelationQuery query in correlationQueries)
                    {
                        this.correlationQueries.Add(query);
                    }
                }
            }

            public bool IsOperationContractOneWay
            {
                get
                {
                    return this.isOperationContractOneWay;
                }
            }

            public TokenImpersonationLevel TokenImpersonationLevel
            {
                get
                {
                    return this.tokenImpersonationLevel;
                }
            }

            public bool Equals(FactoryCacheKey other)
            {
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other == null)
                {
                    // this means only one of them is null
                    return false;
                }

                // 1) Compare Endpoint/EndpointConfigurationName
                if ((this.endpoint == null && other.endpoint != null) ||
                    (other.endpoint == null && this.endpoint != null))
                {
                    return false;
                }

                // if endpoint is not null we compare the endpoint, else we compare the endpointconfiguration
                if (this.endpoint != null)
                {
                    if (!object.ReferenceEquals(this.endpoint, other.endpoint))
                    {
                        // Binding -
                        // We are comparing by ref here, can we compare binding elements instead
                        if (this.endpoint.Binding != other.endpoint.Binding)
                        {
                            return false;
                        }
                    }
                }
                else if (this.endpointConfigurationName != other.endpointConfigurationName)
                {
                    return false;
                }

                // (2) TokenImpersonationlevel
                if (this.TokenImpersonationLevel != other.TokenImpersonationLevel)
                {
                    return false;
                }

                // (3) OperationContract.IsOneWay to decide if the ChannelFactory needs to be of type RequestChannel or OutputChannel
                if (this.IsOperationContractOneWay != other.IsOperationContractOneWay)
                {
                    return false;
                }

                // (4) Verify if the ContractDescriptions are equivalent
                if (!ContractDescriptionComparerHelper.IsContractDescriptionEquivalent(this.contract, other.contract))
                {
                    return false;
                }

                // (5) Verify the correlationquery collection
                //  For now, we verify each query by ref, so that loop scenarios would work
                //  Can we do a value comparison here?  
                if (!ContractDescriptionComparerHelper.EqualsUnordered(this.correlationQueries, other.correlationQueries))
                {
                    return false;
                }
                
                return true;
            }
            
            public override int GetHashCode()
            {
                int hashCode = 0;

                if (this.contract != null && this.contract.Name != null)
                {
                    //using ContractName as the hashcode
                    hashCode ^= this.contract.Name.GetHashCode();
                }

                if (this.endpoint != null && this.endpoint.Binding != null)
                {
                    //we compare binding by ref
                    hashCode ^= this.endpoint.Binding.GetHashCode();
                }

                return hashCode;
            }
        }
        
        static class ContractDescriptionComparerHelper
        {
            public static bool EqualsUnordered<T>(Collection<T> left, Collection<T> right) where T : class
            {
                return EqualsUnordered(left, right, (t1, t2) => t1 == t2);
            }

            public static bool IsContractDescriptionEquivalent(ContractDescription c1, ContractDescription c2)
            {
                if (c1 == c2)
                {
                    return true;
                }

                // if the contract is not one of the default contracts that we use, we only do a byref comparison
                // fully inferred contracts always have null ContractType
                if (c1.ContractType == null || c2.ContractType == null)
                {
                    return false;
                }

                //compare contractname
                return (c1 != null &&
                        c2 != null &&
                        c1.Name == c2.Name &&
                        c1.Namespace == c2.Namespace &&
                        c1.ConfigurationName == c2.ConfigurationName &&
                        c1.ProtectionLevel == c2.ProtectionLevel &&
                        c1.SessionMode == c2.SessionMode &&
                        c1.ContractType == c2.ContractType &&
                        c1.Behaviors.Count == c2.Behaviors.Count && //we have no way to verify each one
                        EqualsUnordered<OperationDescription>(c1.Operations, c2.Operations, (o1, o2) => IsOperationDescriptionEquivalent(o1, o2)));
            }

            static bool EqualsOrdered<T>(IList<T> left, IList<T> right, Func<T, T, bool> equals)
            {
                if (left == null)
                {
                    return (right == null || right.Count == 0);
                }
                else if (right == null)
                {
                    return left.Count == 0;
                }
                if (left.Count != right.Count)
                {
                    return false;
                }
                for (int i = 0; i < left.Count; i++)
                {
                    if (!equals(left[i], right[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            static bool EqualsUnordered<T>(Collection<T> left, Collection<T> right, Func<T, T, bool> equals)
            {
                if (left == null)
                {
                    return (right == null || right.Count == 0);
                }
                else if (right == null)
                {
                    return left.Count == 0;
                }
                // This check ensures that the lists have the same contents, but does not verify that they have the same
                // quantity of each item if they are duplicates.
                return left.Count == right.Count &&
                    left.All(leftItem => right.Any(rightItem => equals(leftItem, rightItem))) &&
                    right.All(rightItem => left.Any(leftItem => equals(leftItem, rightItem)));
            }

            static bool IsOperationDescriptionEquivalent(OperationDescription o1, OperationDescription o2)
            {
                if (o1 == o2)
                {
                    return true;
                }

                return (o1.Name == o2.Name &&
                        o1.ProtectionLevel == o2.ProtectionLevel &&
                        o1.IsOneWay == o2.IsOneWay &&
                        IsTransactionBehaviorEquivalent(o1, o2) && //we are verifying only the TransactionFlowBehavior
                        EqualsOrdered(o1.Messages, o2.Messages, (m1, m2) => IsMessageDescriptionEquivalent(m1, m2)));
            }

            static bool IsMessageDescriptionEquivalent(MessageDescription m1, MessageDescription m2)
            {
                if (m1 == m2)
                {
                    return true;
                }

                //we are comparing only action and direction
                return (m1.Action == m2.Action && m1.Direction == m2.Direction);
            }

            static bool IsTransactionBehaviorEquivalent(OperationDescription o1, OperationDescription o2)
            {
                if ((o1 == null || o2 == null) && o1 == o2)
                {
                    return true;
                }
                if (o1.Behaviors.Count == o2.Behaviors.Count)
                {
                    //we are only going to check the TransactionFlowAttribute
                    TransactionFlowAttribute t1 = o1.Behaviors.Find<TransactionFlowAttribute>();
                    TransactionFlowAttribute t2 = o2.Behaviors.Find<TransactionFlowAttribute>();
                    if ((t1 == null && t2 != null) || (t2 == null && t1 != null))
                    {
                        return false;

                    }
                    //verify if both have the same value for TransactionFlowOption
                    if ((t1 != null) && (t1.Transactions != t2.Transactions))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
