//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Tracking;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;
    using System.Xml.Linq;
    using SR2 = System.ServiceModel.Activities.SR;
    using System.Runtime.DurableInstancing;
    using System.Security;
    using System.ServiceModel.Description;
    using System.Xml;


    sealed class InternalReceiveMessage : NativeActivity
    {
        const string OperationNamePropertyName = "OperationName";
        const string ServiceContractNamePropertyName = "ServiceContractName";
        const string WSContextInstanceIdName = "wsc-instanceId";
        const string InstanceIdKey = ContextMessageProperty.InstanceIdKey;

        static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;

        Collection<CorrelationInitializer> correlationInitializers;
        BookmarkCallback onMessageBookmarkCallback;
        ServiceDescriptionData additionalData;

        string operationBookmarkName;

        Variable<VolatileReceiveMessageInstance> receiveMessageInstance;
        WaitForReply waitForReply;
        CompletionCallback onClientReceiveMessageComplete;
        Variable<Bookmark> extensionReceiveBookmark;
        
        public InternalReceiveMessage()
        {
            this.CorrelatesWith = new InArgument<CorrelationHandle>(context => (CorrelationHandle)null);

            this.receiveMessageInstance = new Variable<VolatileReceiveMessageInstance>();
            this.waitForReply = new WaitForReply { Instance = this.receiveMessageInstance };
            this.onClientReceiveMessageComplete = new CompletionCallback(ClientScheduleOnReceiveMessageCallback);
            this.extensionReceiveBookmark = new Variable<Bookmark>();
        }

        public string Action
        {
            get;
            set;
        }

        public bool CanCreateInstance
        {
            get;
            set;
        }

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

        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }

        public OutArgument<Message> Message
        {
            get;
            set;
        }

        public InArgument<NoPersistHandle> NoPersistHandle
        {
            get;
            set;
        }

        public string OperationName
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

        internal bool IsOneWay
        {
            get;
            set;
        }

        // Added becuase there isn't a good way to distinguish
        // between the Receive and ReceiveReply modes of execution of this activity.
        // The Execute method distinguishes between those modes by predicating 
        // on followingCorrelation not being null and being able to 
        // acquire the RequestContext off the handle. We do not use RequestContext
        // for the SendReceiveExtension based code-paths hence need
        // another mechanism.
        internal bool IsReceiveReply
        {
            get;
            set;
        }

        internal ServiceDescriptionData AdditionalData
        {
            get
            {
                if (this.additionalData == null)
                {
                    this.additionalData = new ServiceDescriptionData();
                }

                return this.additionalData;
            }
        }

        public XName ServiceContractName
        {
            get;
            set;
        }

        // Used by CreateProtocolBookmark and WorkflowOperationBehavior
        internal string OperationBookmarkName
        {
            get
            {
                if (this.operationBookmarkName == null)
                {
                    this.operationBookmarkName = BookmarkNameHelper.CreateBookmarkName(this.OperationName, this.ServiceContractName);
                }

                return this.operationBookmarkName;
            }
        }

        internal string OwnerDisplayName { get; set; }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            InternalReceiveMessage originalInternalReceive = (InternalReceiveMessage)originalActivity;

            if (this.ServiceContractName != originalInternalReceive.ServiceContractName)
            {
                metadata.SaveOriginalValue(ServiceContractNamePropertyName, originalInternalReceive.ServiceContractName);
            }

            if (this.OperationName != originalInternalReceive.OperationName)
            {
                metadata.SaveOriginalValue(OperationNamePropertyName, originalInternalReceive.OperationName);
            }
        }

        protected override void UpdateInstance(NativeActivityUpdateContext updateContext)
        {
            // we only care about the server-side Receive since the client-side Receive is not persistable.
            // only valid instance update condition is when a bookmark with OperationBookmarkName is found.

            CorrelationHandle followingCorrelation = (this.CorrelatesWith == null) ? null : (CorrelationHandle)updateContext.GetValue(this.CorrelatesWith);
            if (followingCorrelation == null)
            {
                followingCorrelation = updateContext.FindExecutionProperty(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
            }

            BookmarkScope bookmarkScope;
            if (followingCorrelation != null && followingCorrelation.Scope != null)
            {
                bookmarkScope = followingCorrelation.Scope;
            }
            else
            {
                bookmarkScope = updateContext.DefaultBookmarkScope;
            }

            string savedOriginalOperationName = (string)updateContext.GetSavedOriginalValue(OperationNamePropertyName);
            XName savedOriginalServiceContractName = (XName)updateContext.GetSavedOriginalValue(ServiceContractNamePropertyName);
            if ((savedOriginalOperationName == null && savedOriginalServiceContractName == null) || (savedOriginalOperationName == this.OperationName && savedOriginalServiceContractName == this.ServiceContractName))
            {
                // neither ServiceContractName nor OperationName have changed
                // nothing to do, so exit early.
                return;
            }

            string originalOperationBookmarkName = BookmarkNameHelper.CreateBookmarkName(savedOriginalOperationName ?? this.OperationName, savedOriginalServiceContractName ?? this.ServiceContractName);
            if (updateContext.RemoveBookmark(originalOperationBookmarkName, bookmarkScope))
            {
                // if we are here, it means Receive is on the server-side and waiting for a request message to arrive
                updateContext.CreateBookmark(this.OperationBookmarkName, new BookmarkCallback(this.OnMessage), bookmarkScope);
            }
            else
            {
                // this means Receive is in a state DU is not allowed.
                updateContext.DisallowUpdate(SR.InvalidReceiveStateForDU);
            }
        }

        ReceiveSettings GetReceiveSettings()
        {
            string actionName = null;

            if (!string.IsNullOrWhiteSpace(this.Action))
            {
                actionName = this.Action;
            }
            else
            {
                // These values are null in the ReceiveReply configuration
                if (this.ServiceContractName != null && !string.IsNullOrWhiteSpace(this.OperationName))
                {
                    actionName = NamingHelper.GetMessageAction(new XmlQualifiedName(this.ServiceContractName.ToString()), this.OperationName, null, false);
                }
            }

            ReceiveSettings receiveSettings = new ReceiveSettings
            {
                Action = actionName,
                CanCreateInstance = this.CanCreateInstance,
                OwnerDisplayName = this.OwnerDisplayName
            };

            return receiveSettings;
        }

        protected override void Abort(NativeActivityAbortContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                Bookmark pendingBookmark = this.extensionReceiveBookmark.Get(context);
                if (pendingBookmark != null)
                {
                    sendReceiveExtension.Cancel(pendingBookmark);
                }
            }
            base.Abort(context);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                Bookmark pendingBookmark = this.extensionReceiveBookmark.Get(context);
                if (pendingBookmark != null)
                {
                    sendReceiveExtension.Cancel(pendingBookmark);
                    context.RemoveBookmark(pendingBookmark);
                }
            }
            base.Cancel(context);
        }

        // Activity Entry point: Phase 1: Execute
        // A separate code-path for extension based execution least impacts 
        // the existing workflow hosts. In the future we will add an extension from 
        // workflowservicehost and always use the extension.
        protected override void Execute(NativeActivityContext executionContext)
        {
            SendReceiveExtension sendReceiveExtension = executionContext.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                this.ExecuteUsingExtension(sendReceiveExtension, executionContext);
            }
            else
            {

                // this activity's runtime DU particpation(UpdateInstance) is dependent on
                // the following server side logic for resolving CorrelationHandle and creating a protocol bookmark.

                CorrelationHandle followingCorrelation = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(executionContext);
                bool triedAmbientCorrelation = false;
                CorrelationHandle ambientCorrelation = null;

                if (followingCorrelation == null)
                {
                    ambientCorrelation = executionContext.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                    triedAmbientCorrelation = true;
                    if (ambientCorrelation != null)
                    {
                        followingCorrelation = ambientCorrelation;
                    }
                }

                CorrelationRequestContext requestContext;
                if (followingCorrelation != null && followingCorrelation.TryAcquireRequestContext(executionContext, out requestContext))
                {
                    // Client receive that is following a send.
                    ReceiveMessageInstanceData instance = new ReceiveMessageInstanceData(requestContext);

                    // for perf, cache the ambient correlation information
                    if (triedAmbientCorrelation)
                    {
                        instance.SetAmbientCorrelation(ambientCorrelation);
                    }

                    ClientScheduleOnReceivedMessage(executionContext, instance);
                }
                else
                {
                    // Server side receive

                    // Validation of correlatesWithHandle
                    if (ambientCorrelation == null)
                    {
                        ambientCorrelation = executionContext.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                    }
                    if (!this.IsOneWay && ambientCorrelation == null)
                    {
                        CorrelationHandle channelCorrelationHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(executionContext, this.correlationInitializers);
                        if (channelCorrelationHandle == null)
                        {
                            // With a two-way contract, we require a request/reply correlation handle
                            throw FxTrace.Exception.AsError(new InvalidOperationException(
                                SR2.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
                        }
                    }

                    BookmarkScope bookmarkScope = (followingCorrelation != null) ? followingCorrelation.EnsureBookmarkScope(executionContext) : executionContext.DefaultBookmarkScope;

                    if (this.onMessageBookmarkCallback == null)
                    {
                        this.onMessageBookmarkCallback = new BookmarkCallback(this.OnMessage);
                    }

                    executionContext.CreateBookmark(this.OperationBookmarkName, this.onMessageBookmarkCallback, bookmarkScope);
                }
            }
        }        

        // Phase 2a: server side message has arrived and resumed the protocol bookmark
        void OnMessage(NativeActivityContext executionContext, Bookmark bookmark, object state)
        {
            WorkflowOperationContext workflowContext = state as WorkflowOperationContext;

            if (workflowContext == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.WorkflowMustBeHosted));
            }

            ReceiveMessageInstanceData instance = new ReceiveMessageInstanceData(
                new CorrelationResponseContext
                {
                    WorkflowOperationContext = workflowContext,
                });

            SetupTransaction(executionContext, instance);
        }

        // Phase 3: Setup Transaction for server receive case.
        // 
        void SetupTransaction(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            WorkflowOperationContext workflowContext = instance.CorrelationResponseContext.WorkflowOperationContext;
            if (workflowContext.CurrentTransaction != null)
            {
                //get the RuntimeTransactionHandle from the ambient
                RuntimeTransactionHandle handle = null;
                handle = executionContext.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
                if (handle != null)
                {
                    //You are probably inside a TransactedReceiveScope
                    //TransactedReceiveData is used to pass information about the Initiating Transaction to the TransactedReceiveScope 
                    //so that it can subsequently call Complete or Commit on it at the end of the scope
                    TransactedReceiveData transactedReceiveData = executionContext.Properties.Find(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName) as TransactedReceiveData;
                    if (transactedReceiveData != null)
                    {
                        if (this.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree)
                        {
                            Fx.Assert(workflowContext.OperationContext != null, "InternalReceiveMessage.SetupTransaction - Operation Context was null");
                            Fx.Assert(workflowContext.OperationContext.TransactionFacet != null, "InternalReceiveMessage.SetupTransaction - Transaction Facet was null");
                            transactedReceiveData.InitiatingTransaction = workflowContext.OperationContext.TransactionFacet.Current;
                        }
                    }

                    Transaction currentTransaction = handle.GetCurrentTransaction(executionContext);
                    if (currentTransaction != null) 
                    {
                        if (!currentTransaction.Equals(workflowContext.CurrentTransaction))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.FlowedTransactionDifferentFromAmbient));
                        }
                        else
                        {
                            ServerScheduleOnReceivedMessage(executionContext, instance);
                            return;
                        }
                    }

                    ReceiveMessageState receiveMessageState = new ReceiveMessageState
                    {
                        CurrentTransaction = workflowContext.CurrentTransaction.Clone(),
                        Instance = instance
                    };

                    handle.RequireTransactionContext(executionContext, RequireContextCallback, receiveMessageState);

                    return;
                }
                else
                {
                    //Receive was probably not used within a TransactionFlowScope since no ambient transaction handle was found
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.ReceiveNotWithinATransactedReceiveScope));
                }
            }

            ServerScheduleOnReceivedMessage(executionContext, instance);
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

        void ProcessReceiveMessageTrace(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                if (TraceUtility.ActivityTracing)
                {
                    instance.AmbientActivityId = InternalReceiveMessage.TraceCorrelationActivityId;
                }

                Guid receivedActivityId = Guid.Empty;
                if (instance.CorrelationRequestContext != null)
                {
                    //client side reply
                    receivedActivityId = TraceUtility.GetReceivedActivityId(instance.CorrelationRequestContext.OperationContext);
                }
                else if (instance.CorrelationResponseContext != null)
                {
                    //server side receive
                    receivedActivityId = instance.CorrelationResponseContext.WorkflowOperationContext.E2EActivityId;
                }

                ProcessReceiveMessageTrace(executionContext, receivedActivityId);
            }
        }

        void ProcessReceiveMessageTrace(NativeActivityContext executionContext, Guid receivedActivityId)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                try
                {
                    // 
                    ReceiveMessageRecord messageFlowTrackingRecord = new ReceiveMessageRecord(MessagingActivityHelper.MessageCorrelationReceiveRecord)
                    {
                        E2EActivityId = receivedActivityId
                    };
                    executionContext.Track(messageFlowTrackingRecord);

                    if (receivedActivityId != Guid.Empty && DiagnosticTraceBase.ActivityId != receivedActivityId)
                    {
                        DiagnosticTraceBase.ActivityId = receivedActivityId;
                    }

                    FxTrace.Trace.SetAndTraceTransfer(executionContext.WorkflowInstanceId, true);

                    if (TraceUtility.ActivityTracing)
                    {
                        if (TD.StartSignpostEventIsEnabled())
                        {
                            TD.StartSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                    { MessagingActivityHelper.ActivityName, this.DisplayName },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                    { MessagingActivityHelper.ActivityInstanceId, executionContext.ActivityInstanceId }
                        }));
                        }
                    }
                    else if (TD.WfMessageReceivedIsEnabled())
                    {
                        TD.WfMessageReceived(new EventTraceActivity(receivedActivityId), executionContext.WorkflowInstanceId);
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

        void RequireContextCallback(NativeActivityTransactionContext transactionContext, object state)
        {
            Fx.Assert(transactionContext != null, "TransactionContext is null");

            ReceiveMessageState receiveMessageState = state as ReceiveMessageState;
            Fx.Assert(receiveMessageState != null, "ReceiveMessageState is null");

            transactionContext.SetRuntimeTransaction(receiveMessageState.CurrentTransaction);

            NativeActivityContext executionContext = transactionContext as NativeActivityContext;
            Fx.Assert(executionContext != null, "Failed to cast ActivityTransactionContext to NativeActivityContext");
            ServerScheduleOnReceivedMessage(executionContext, receiveMessageState.Instance);
        }

        // Phase 4: Set up the Message as OutArgument and invoke the OnReceivedMessage activity action
        void ServerScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            Fx.Assert(instance.CorrelationResponseContext != null, "Server side receive must have CorrelationResponseContext");

            // if we infer the contract as Message the first input parameter will be the requestMessage from the client
            Message request = instance.CorrelationResponseContext.WorkflowOperationContext.Inputs[0] as Message;
            Fx.Assert(request != null, "WorkflowOperationContext.Inputs[0] must be of type Message");
            Fx.Assert(request.State == MessageState.Created, "The request message must be in Created state");
            this.Message.Set(executionContext, request);

            // update instance->CorrelationResponseContext with the MessageVersion information, this is later used by 
            // ToReply formatter to construct the reply message
            instance.CorrelationResponseContext.MessageVersion = ((Message)instance.CorrelationResponseContext.WorkflowOperationContext.Inputs[0]).Version;

            // initialize the relevant correlation handle(s) with the 'anonymous' response context
            CorrelationHandle ambientHandle = instance.GetAmbientCorrelation(executionContext);
            CorrelationHandle correlatesWithHandle = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(executionContext);

            // populate instance keys first
            MessagingActivityHelper.InitializeCorrelationHandles(executionContext, correlatesWithHandle, ambientHandle, this.correlationInitializers,
                instance.CorrelationResponseContext.WorkflowOperationContext.OperationContext.IncomingMessageProperties);

            // for the request/reply handle
            // then store the response context in the designated correlation handle
            // first check for an explicit association
            CorrelationHandle channelCorrelationHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(executionContext, this.correlationInitializers);

            
            if (this.IsOneWay)
            {
                // this is one way, verify that the channelHandle is null
                if (channelCorrelationHandle != null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.RequestReplyHandleShouldNotBePresentForOneWay));
                }

                // we need to enter the nopersistzone using the NoPersistHandle and exit it in the formatter
                if (this.NoPersistHandle != null)
                {
                    NoPersistHandle noPersistHandle = this.NoPersistHandle.Get(executionContext);
                    if (noPersistHandle != null)
                    {
                        noPersistHandle.Enter(executionContext);
                    }
                }
            }
            else 
            {
                // first check for an explicit association
                if (channelCorrelationHandle != null)
                {
                    if (!channelCorrelationHandle.TryRegisterResponseContext(executionContext, instance.CorrelationResponseContext))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.TryRegisterRequestContextFailed));
                    }
                }
                else// if that fails, use ambient handle. we should never initialize CorrelatesWith with response context
                {
                    Fx.Assert(ambientHandle != null, "Ambient handle should not be null for two-way server side receive/sendReply");
                    if (!ambientHandle.TryRegisterResponseContext(executionContext, instance.CorrelationResponseContext))
                    {
                        // With a two-way contract, the request context must be initialized
                        throw FxTrace.Exception.AsError(new InvalidOperationException(
                            SR2.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
                    }
                }

                // validate that NoPersistHandle is null, we should have nulled it out in Receive->SetIsOneWay during ContractInference
                Fx.Assert(this.NoPersistHandle == null, "NoPersistHandle should be null in case of two-way");
            }
        
            // for the duplex handle: we want to save the callback context in the correlation handle
            if (instance.CorrelationCallbackContext != null)
            {
                // Pass the CorrelationCallbackContext to correlation handle.
                CorrelationHandle callbackHandle = CorrelationHandle.GetExplicitCallbackCorrelation(executionContext, this.correlationInitializers);

                // if that is not set, then try the ambientHandle, we will not use the CorrelatesWith handle  to store callback context
                if (callbackHandle == null)
                {
                    callbackHandle = ambientHandle;
                }
                if (callbackHandle != null)
                {
                    callbackHandle.CallbackContext = instance.CorrelationCallbackContext;
                }
            }

            FinalizeScheduleOnReceivedMessage(executionContext, instance);
        }

        void ClientScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            Fx.Assert(instance.CorrelationRequestContext != null, "Client side receive must have CorrelationRequestContext");

            // client side: retrieve the reply from the request context
            if (instance.CorrelationRequestContext.TryGetReply())
            {
                // Reply has already come back because one of the following happened:
                // (1) Receive reply completed synchronously
                // (2) Async receive reply completed very quickly and channel callback already happened by now
                ClientScheduleOnReceiveMessageCore(executionContext, instance);
                FinalizeScheduleOnReceivedMessage(executionContext, instance);
            }
            else
            {
                // Async path: wait for reply to come back
                VolatileReceiveMessageInstance volatileInstance = new VolatileReceiveMessageInstance { Instance = instance };
                this.receiveMessageInstance.Set(executionContext, volatileInstance);

                if (onClientReceiveMessageComplete == null)
                {
                    onClientReceiveMessageComplete = new CompletionCallback(ClientScheduleOnReceiveMessageCallback);
                }

                executionContext.ScheduleActivity(this.waitForReply, onClientReceiveMessageComplete);
            }
        }

        void ClientScheduleOnReceiveMessageCallback(NativeActivityContext executionContext, ActivityInstance completedInstance)
        {
            VolatileReceiveMessageInstance volatileInstance = this.receiveMessageInstance.Get(executionContext);
            ReceiveMessageInstanceData instance = volatileInstance.Instance;

            if (instance.CorrelationRequestContext.TryGetReply())
            {
                ClientScheduleOnReceiveMessageCore(executionContext, instance);
            }
            FinalizeScheduleOnReceivedMessage(executionContext, instance);
        }

        void ClientScheduleOnReceiveMessageCore(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            Fx.Assert(instance.CorrelationRequestContext.Reply != null, "Reply message cannot be null!");

            // Initialize CorrelationContext and CorrelationCallbackContext
            instance.InitializeContextAndCallbackContext();

            CorrelationHandle ambientHandle = instance.GetAmbientCorrelation(executionContext);

            if (instance.CorrelationRequestContext.CorrelationKeyCalculator != null)
            {
                // Client side reply do not use CorrelatesWith to initialize correlation
                instance.CorrelationRequestContext.Reply = MessagingActivityHelper.InitializeCorrelationHandles(executionContext,
                    null, ambientHandle, this.correlationInitializers,
                    instance.CorrelationRequestContext.CorrelationKeyCalculator, instance.CorrelationRequestContext.Reply);
            }

            // for the duplex-case 
            // we would receive the Server Context in the Request-Reply message, we have to save the Server Context so that subsequent sends from the client to
            // the server can use this context to reach the correct Server instance
            if (instance.CorrelationContext != null)
            {
                // Pass the CorrelationContext to correlation handle.
                // Correlation handle will have to be in the correlation Initializers collection
                CorrelationHandle contextHandle = CorrelationHandle.GetExplicitContextCorrelation(executionContext, this.correlationInitializers);

                // if that is not set, then try the ambient handle
                if (contextHandle == null)
                {
                    // get the cached ambient handle, we only use explicit handle or ambient handle to store the context
                    contextHandle = ambientHandle;
                }
                if (contextHandle != null)
                {
                    contextHandle.Context = instance.CorrelationContext;
                }
            }

            // set the Message with what is in the correlationRequestContext 
            // this Message needs to be closed later by the formatter
            Message request = instance.CorrelationRequestContext.Reply;
            this.Message.Set(executionContext, request);
        }

        void FinalizeScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            ProcessReceiveMessageTrace(executionContext, instance);

            IList<IReceiveMessageCallback> receiveMessageCallbacks = MessagingActivityHelper.GetCallbacks<IReceiveMessageCallback>(executionContext.Properties);
            if (receiveMessageCallbacks != null && receiveMessageCallbacks.Count > 0)
            {
                OperationContext operationContext = instance.GetOperationContext();
                // invoke the callback that user might have added in the AEC in the previous activity 
                // e.g. distributed compensation activity will add this so that they can convert a message back to
                // an execution property
                foreach (IReceiveMessageCallback receiveMessageCallback in receiveMessageCallbacks)
                {
                    receiveMessageCallback.OnReceiveMessage(operationContext, executionContext.Properties);
                }
            }

            // call this method with or without callback
            this.FinalizeReceiveMessageCore(executionContext, instance);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
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

            RuntimeArgument receiveMessageArgument = new RuntimeArgument(Constants.Message, Constants.MessageType, ArgumentDirection.Out);
            if (this.Message == null)
            {
                this.Message = new OutArgument<Message>();
            }
            metadata.Bind(this.Message, receiveMessageArgument);
            metadata.AddArgument(receiveMessageArgument);

            RuntimeArgument noPersistHandleArgument = new RuntimeArgument(Constants.NoPersistHandle, Constants.NoPersistHandleType, ArgumentDirection.In);
            if (this.NoPersistHandle == null)
            {
                this.NoPersistHandle = new InArgument<NoPersistHandle>();
            }
            metadata.Bind(this.NoPersistHandle, noPersistHandleArgument);
            metadata.AddArgument(noPersistHandleArgument);

            metadata.AddImplementationVariable(this.receiveMessageInstance);
            metadata.AddImplementationVariable(this.extensionReceiveBookmark);

            metadata.AddImplementationChild(this.waitForReply);
        }

        // Phase 5: Useful for the both client and server side receive. It passes down the response context if it is two way or 
        // throw the exception right back to the workflow if it is not expected. 
        void FinalizeReceiveMessageCore(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            if (instance != null)
            {
                if (instance.CorrelationRequestContext != null && instance.CorrelationRequestContext.Reply != null)
                {
                    // This should be closed by the formatter after desrializing the message
                    // clean this reply message up for a following receive
                    //instance.CorrelationRequestContext.Reply.Close();
                }
                else if (instance.CorrelationResponseContext != null)
                {
                    // this is only for the server side
                    if (this.IsOneWay)
                    {
                        // mark this workflow service operation as complete
                        instance.CorrelationResponseContext.WorkflowOperationContext.SetOperationCompleted();

                        if (instance.CorrelationResponseContext.Exception != null)
                        {
                            // We got an unexpected exception while running the OnReceivedMessage action
                            throw FxTrace.Exception.AsError(instance.CorrelationResponseContext.Exception);
                        }
                    }
                }

                //reset the trace
                this.ResetTrace(executionContext, instance);
            }
        }

        void ResetTrace(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            this.ResetTrace(executionContext, instance.AmbientActivityId);

            if (TraceUtility.ActivityTracing)
            {
                instance.AmbientActivityId = Guid.Empty;
            }
        }

        void ResetTrace(NativeActivityContext executionContext, Guid ambientActivityId)
        {
            if (TraceUtility.ActivityTracing)
            {
                if (TD.StopSignpostEventIsEnabled())
                {
                    TD.StopSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(3) {
                                                { MessagingActivityHelper.ActivityName, this.DisplayName },
                                                { MessagingActivityHelper.ActivityType, MessagingActivityHelper.MessagingActivityTypeActivityExecution },
                                                { MessagingActivityHelper.ActivityInstanceId, executionContext.ActivityInstanceId }
                        }));
                }
                FxTrace.Trace.SetAndTraceTransfer(ambientActivityId, true);
            }
            else if (TD.WfMessageReceivedIsEnabled())
            {
                TD.WfMessageReceived(new EventTraceActivity(executionContext.WorkflowInstanceId), ambientActivityId);
            }
        }

        void ExecuteUsingExtension(SendReceiveExtension sendReceiveExtension, NativeActivityContext executionContext)
        {
            Fx.Assert(sendReceiveExtension != null, "SendReceiveExtension should be available here.");

            CorrelationHandle followingCorrelation = null;
            if (!this.TryGetCorrelatesWithHandle(executionContext, out followingCorrelation))
            {
                followingCorrelation = CorrelationHandle.GetAmbientCorrelation(executionContext);
                if (followingCorrelation == null)
                {
                    if (!this.IsOneWay)
                    {
                        if (!this.correlationInitializers.TryGetRequestReplyCorrelationHandle(executionContext, out followingCorrelation))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(
                                SR2.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
                        }
                    }
                }
            }

            Bookmark bookmark = executionContext.CreateBookmark(this.OnReceiveMessageFromExtension);
            this.extensionReceiveBookmark.Set(executionContext, bookmark);

            InstanceKey correlatesWithValue = null;
            if (followingCorrelation != null)
            {
                if (this.IsReceiveReply && followingCorrelation.TransientInstanceKey != null)
                {
                    correlatesWithValue = followingCorrelation.TransientInstanceKey;
                }
                else
                {
                    correlatesWithValue = followingCorrelation.InstanceKey;
                }
            }
            sendReceiveExtension.RegisterReceive(this.GetReceiveSettings(), correlatesWithValue, bookmark);
        }

        void OnReceiveMessageFromExtension(NativeActivityContext executionContext, Bookmark bookmark, object state)
        {
            SendReceiveExtension sendReceiveExtension = executionContext.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.SendReceiveExtensionNotFound));
            }

            // Now that the bookmark has been resumed, clear out the workflow variable holding its value.
            this.extensionReceiveBookmark.Set(executionContext, null);

            MessageContext messageContext = state as MessageContext;
            if (messageContext == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.InvalidDataFromReceiveBookmarkState(this.OperationName)));
            }

            this.Message.Set(executionContext, messageContext.Message);
            this.ProcessReceiveMessageTrace(executionContext, messageContext.EndToEndTracingId);
            this.InitializeCorrelationHandles(executionContext, messageContext.Message.Properties, messageContext.EndToEndTracingId);
            this.ResetTrace(executionContext, InternalReceiveMessage.TraceCorrelationActivityId);
        }

        void InitializeCorrelationHandles(NativeActivityContext executionContext, MessageProperties messageProperties, Guid e2eTracingId)
        {
            CorrelationHandle ambientHandle = CorrelationHandle.GetAmbientCorrelation(executionContext);
            HostSettings hostSettings = executionContext.GetExtension<SendReceiveExtension>().HostSettings;

            if (this.IsReceiveReply)
            {
                // Client side ReceiveReply.

                MessagingActivityHelper.InitializeCorrelationHandles(executionContext, null, ambientHandle, this.correlationInitializers, messageProperties);

                // Set InstanceKey on ContextCorrelation/Ambient handle.
                InstanceKey contextCorrelationInstanceKey;
                if (this.TryGetContextCorrelationInstanceKey(hostSettings, messageProperties, out contextCorrelationInstanceKey))
                {
                    CorrelationHandle contextCorrelationHandle = CorrelationHandle.GetExplicitContextCorrelation(executionContext, this.correlationInitializers);
                    MessagingActivityHelper.InitializeCorrelationHandles(executionContext, contextCorrelationHandle, ambientHandle, null, contextCorrelationInstanceKey, null);
                }

                // ensure we clear the transient correlation handle so that it can be reused by subsequent request-reply pairs
                if (ambientHandle != null)
                {
                    ambientHandle.TransientInstanceKey = null;
                }
            }
            else
            {
                // Server side receive. Can be a one-way Receive or a Receive-SendReply.
                CorrelationHandle requestReplyHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(executionContext, this.correlationInitializers);

                if (requestReplyHandle == null && ambientHandle == null && !this.IsOneWay)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        SR2.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
                }

                if (requestReplyHandle != null && this.IsOneWay)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.RequestReplyHandleShouldNotBePresentForOneWay));
                }

                CorrelationHandle correlatesWithHandle;
                this.TryGetCorrelatesWithHandle(executionContext, out correlatesWithHandle);

                MessagingActivityHelper.InitializeCorrelationHandles(executionContext, correlatesWithHandle, ambientHandle, this.correlationInitializers, messageProperties);

                if (!this.IsOneWay)
                {
                    InstanceKey requestReplyCorrelationInstanceKey;
                    if (this.TryGetRequestReplyCorrelationInstanceKey(messageProperties, out requestReplyCorrelationInstanceKey))
                    {
                        MessagingActivityHelper.InitializeCorrelationHandles(executionContext, requestReplyHandle, ambientHandle, null, requestReplyCorrelationInstanceKey, null);
                    }
                    else
                    {
                        if (requestReplyHandle != null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.FailedToInitializeRequestReplyCorrelationHandle(this.OperationName)));
                        }
                    }
                }

                this.UpdateE2ETracingId(e2eTracingId, correlatesWithHandle, ambientHandle, requestReplyHandle);

                // Set InstanceKey on CallbackCorrelation/Ambient handle.
                InstanceKey callbackContextCorrelationInstanceKey;
                if (this.TryGetCallbackContextCorrelationInstanceKey(hostSettings, messageProperties, out callbackContextCorrelationInstanceKey))
                {
                    CorrelationHandle callbackContextCorrelationHandle = CorrelationHandle.GetExplicitCallbackCorrelation(executionContext, this.correlationInitializers);
                    MessagingActivityHelper.InitializeCorrelationHandles(executionContext, callbackContextCorrelationHandle, ambientHandle, null, callbackContextCorrelationInstanceKey, null);
                }
            }
        }

        void UpdateE2ETracingId(Guid e2eTracingId, CorrelationHandle correlatesWith, CorrelationHandle ambientHandle, CorrelationHandle requestReplyHandle)
        {
            if (correlatesWith != null)
            {
                correlatesWith.E2ETraceId = e2eTracingId;
            }
            else if (ambientHandle != null)
            {
                ambientHandle.E2ETraceId = e2eTracingId;
            }
            else if (requestReplyHandle != null)
            {
                requestReplyHandle.E2ETraceId = e2eTracingId;
            }
        }

        bool TryGetCallbackContextCorrelationInstanceKey(HostSettings hostSettings, MessageProperties messageProperties, out InstanceKey callbackContextCorrelationInstanceKey)
        {
            callbackContextCorrelationInstanceKey = null;
            CallbackContextMessageProperty callbackContext;
            if (CallbackContextMessageProperty.TryGet(messageProperties, out callbackContext))
            {
                if (callbackContext.Context != null)
                {
                    string instanceId = null;
                    if (callbackContext.Context.TryGetValue(InstanceIdKey, out instanceId))
                    {
                        IDictionary<string, string> keyData = new Dictionary<string, string>(1)
                        {
                            { WSContextInstanceIdName, instanceId }
                        };

                        callbackContextCorrelationInstanceKey = new CorrelationKey(keyData, hostSettings.ScopeName, null);
                    }
                }
            }

            return callbackContextCorrelationInstanceKey != null;
        }

        bool TryGetContextCorrelationInstanceKey(HostSettings hostSettings, MessageProperties messageProperties, out InstanceKey correlationContextInstanceKey)
        {
            correlationContextInstanceKey = null;

            ContextMessageProperty contextProperties = null;
            if (ContextMessageProperty.TryGet(messageProperties, out contextProperties))
            {
                if (contextProperties.Context != null)
                {
                    string instanceId = null;
                    if (contextProperties.Context.TryGetValue(InstanceIdKey, out instanceId))
                    {
                        IDictionary<string, string> keyData = new Dictionary<string, string>(1)
                        {
                            { WSContextInstanceIdName, instanceId }
                        };

                        correlationContextInstanceKey = new CorrelationKey(keyData, hostSettings.ScopeName, null);
                    }
                }
            }

            return correlationContextInstanceKey != null;
        }

        bool TryGetRequestReplyCorrelationInstanceKey(MessageProperties messageProperties, out InstanceKey instanceKey)
        {
            instanceKey = null;
            CorrelationMessageProperty correlationMessageProperty;
            if (messageProperties.TryGetValue<CorrelationMessageProperty>(CorrelationMessageProperty.Name, out correlationMessageProperty))
            {
                foreach (InstanceKey key in correlationMessageProperty.TransientCorrelations)
                {
                    InstanceValue value;
                    if (key.Metadata.TryGetValue(WorkflowServiceNamespace.RequestReplyCorrelation, out value))
                    {
                        instanceKey = key;
                        break;
                    }
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

        [DataContract]
        internal class VolatileReceiveMessageInstance
        {
            public VolatileReceiveMessageInstance()
            {
            }

            // Note that we do not mark this DataMember since we don’t want it to be serialized
            public ReceiveMessageInstanceData Instance { get; set; }
        }

        // This class defines the instance data that is saved in a variable. This will be initialized with null, and only be
        // used to pass data around during the execution. It is not intended to be persisted, thus it is not marked with 
        // DataContract and DataMemeber.
        internal class ReceiveMessageInstanceData
        {
            bool triedAmbientCorrelation;
            CorrelationHandle ambientCorrelation;

            public ReceiveMessageInstanceData(CorrelationRequestContext requestContext)
            {
                Fx.Assert(requestContext != null, "requestContext is a required parameter");
                this.CorrelationRequestContext = requestContext;
            }

            public ReceiveMessageInstanceData(CorrelationResponseContext responseContext)
            {
                Fx.Assert(responseContext != null, "responseContext is a required parameter");
                this.CorrelationResponseContext = responseContext;
                this.CorrelationCallbackContext =
                    MessagingActivityHelper.CreateCorrelationCallbackContext(responseContext.WorkflowOperationContext.OperationContext.IncomingMessageProperties);
            }

            // For the client-receive case. Saves the context retrieved from the handle
            public CorrelationRequestContext CorrelationRequestContext
            {
                get;
                private set;
            }

            // For the server-receive case. The context that will be used to by the following send.
            public CorrelationResponseContext CorrelationResponseContext
            {
                get;
                private set;
            }

            public CorrelationCallbackContext CorrelationCallbackContext
            {
                get;
                private set;
            }

            public CorrelationContext CorrelationContext
            {
                get;
                private set;
            }

            public Guid AmbientActivityId
            {
                get;
                set;
            }

            public CorrelationHandle GetAmbientCorrelation(NativeActivityContext context)
            {
                if (this.triedAmbientCorrelation)
                {
                    return this.ambientCorrelation;
                }

                this.triedAmbientCorrelation = true;
                this.ambientCorrelation = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                return this.ambientCorrelation;
            }

            public void SetAmbientCorrelation(CorrelationHandle ambientCorrelation)
            {
                Fx.Assert(!this.triedAmbientCorrelation, "can only set ambient correlation once");
                this.ambientCorrelation = ambientCorrelation;
                this.triedAmbientCorrelation = true;
            }

            internal OperationContext GetOperationContext()
            {
                if (this.CorrelationRequestContext != null)
                {
                    return this.CorrelationRequestContext.OperationContext;
                }
                else if (this.CorrelationResponseContext != null)
                {
                    return this.CorrelationResponseContext.WorkflowOperationContext.OperationContext;
                }

                return null;

            }

            public void InitializeContextAndCallbackContext()
            {
                Fx.Assert(this.CorrelationRequestContext.Reply != null, "Reply message cannot be null for context and callback!");
                
                this.CorrelationCallbackContext =
                    MessagingActivityHelper.CreateCorrelationCallbackContext(this.CorrelationRequestContext.Reply.Properties);
                // this is the context that the server must have send back in the initial hand-shake
                this.CorrelationContext =
                    MessagingActivityHelper.CreateCorrelationContext(this.CorrelationRequestContext.Reply.Properties);
            }
        }

        class ReceiveMessageState
        {
            public Transaction CurrentTransaction
            {
                get;
                set;
            }

            public ReceiveMessageInstanceData Instance
            {
                get;
                set;
            }
        }

        class WaitForReply : AsyncCodeActivity
        {
            public WaitForReply()
            {
            }

            public InArgument<VolatileReceiveMessageInstance> Instance
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument instanceArgument = new RuntimeArgument("Instance", typeof(VolatileReceiveMessageInstance), ArgumentDirection.In);
                if (this.Instance == null)
                {
                    this.Instance = new InArgument<VolatileReceiveMessageInstance>();
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
                VolatileReceiveMessageInstance volatileInstance = this.Instance.Get(context);

                return new WaitForReplyAsyncResult(volatileInstance.Instance, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                WaitForReplyAsyncResult.End(result);
            }

            protected override void Cancel(AsyncCodeActivityContext context)
            {
                VolatileReceiveMessageInstance volatileInstance = this.Instance.Get(context);
                volatileInstance.Instance.CorrelationRequestContext.Cancel();

                base.Cancel(context);
            }

            class WaitForReplyAsyncResult : AsyncResult
            {
                static Action<object, TimeoutException> onReceiveReply;

                public WaitForReplyAsyncResult(ReceiveMessageInstanceData instance, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    if (onReceiveReply == null)
                    {
                        onReceiveReply = new Action<object, TimeoutException>(OnReceiveReply);
                    }

                    if (instance.CorrelationRequestContext.WaitForReplyAsync(onReceiveReply, this))
                    {
                        Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WaitForReplyAsyncResult>(result);
                }

                static void OnReceiveReply(object state, TimeoutException timeoutException)
                {
                    WaitForReplyAsyncResult thisPtr = (WaitForReplyAsyncResult)state;
                    thisPtr.Complete(false);
                }
            }
        }
    }
}
