//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Transactions;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    static class ClientFactory
    {
        public static IRoutingClient Create(RoutingEndpointTrait endpointTrait, RoutingService service, bool impersonating)
        {
            Type contractType = endpointTrait.RouterContract;
            IRoutingClient client;
            if (contractType == typeof(ISimplexDatagramRouter))
            {
                client = new SimplexDatagramClient(endpointTrait, service.RoutingConfig, impersonating);
            }
            else if (contractType == typeof(IRequestReplyRouter))
            {
                client = new RequestReplyClient(endpointTrait, service.RoutingConfig, impersonating);
            }
            else if (contractType == typeof(ISimplexSessionRouter))
            {
                client = new SimplexSessionClient(endpointTrait, service.RoutingConfig, impersonating);
            }
            else //if (contractType == typeof(IDuplexSessionRouter))
            {
                Fx.Assert(contractType == typeof(IDuplexSessionRouter), "Only one contract type remaining.");
                client = new DuplexSessionClient(service, endpointTrait, impersonating);
            }            

            return client;
        }

        abstract class RoutingClientBase<TChannel> : ClientBase<TChannel>, IRoutingClient
            where TChannel : class
        {
            bool openCompleted;
            object thisLock;
            Queue<OperationAsyncResult> waiters;

            protected RoutingClientBase(RoutingEndpointTrait endpointTrait, RoutingConfiguration routingConfig, bool impersonating)
                : base(endpointTrait.Endpoint.Binding, endpointTrait.Endpoint.Address)
            {
                Initialize(endpointTrait, routingConfig, impersonating);
            }

            protected RoutingClientBase(RoutingEndpointTrait endpointTrait, RoutingConfiguration routingConfig, object callbackInstance, bool impersonating)
                : base(new InstanceContext(callbackInstance), endpointTrait.Endpoint.Binding, endpointTrait.Endpoint.Address)
            {
                Initialize(endpointTrait, routingConfig, impersonating);
            }

            public RoutingEndpointTrait Key
            {
                get;
                private set;
            }

            public event EventHandler Faulted;

            static void ConfigureImpersonation(ServiceEndpoint endpoint, bool impersonating)
            {
                // Used for both impersonation and ASP.NET Compatibilty Mode.  Both currently require
                // everything to be synchronous.
                if (impersonating)
                {
                    CustomBinding binding = endpoint.Binding as CustomBinding;
                    if (binding == null)
                    {
                        binding = new CustomBinding(endpoint.Binding);
                    }

                    SynchronousSendBindingElement syncSend = binding.Elements.Find<SynchronousSendBindingElement>();
                    if (syncSend == null)
                    {
                        binding.Elements.Insert(0, new SynchronousSendBindingElement());
                        endpoint.Binding = binding;
                    }
                }
            }

            static void ConfigureTransactionFlow(ServiceEndpoint endpoint)
            {
                CustomBinding binding = endpoint.Binding as CustomBinding;
                if (binding == null)
                {
                    binding = new CustomBinding(endpoint.Binding);
                }
                TransactionFlowBindingElement transactionFlow = binding.Elements.Find<TransactionFlowBindingElement>();
                if (transactionFlow != null)
                {
                    transactionFlow.AllowWildcardAction = true;
                    endpoint.Binding = binding;
                }
            }

            void Initialize(RoutingEndpointTrait endpointTrait, RoutingConfiguration routingConfig, bool impersonating)
            {
                this.thisLock = new object();
                this.Key = endpointTrait;
                if (TD.RoutingServiceCreatingClientForEndpointIsEnabled())
                {
                    TD.RoutingServiceCreatingClientForEndpoint(this.Key.ToString());
                }
                ServiceEndpoint clientEndpoint = endpointTrait.Endpoint;
                ServiceEndpoint endpoint = this.Endpoint;
                KeyedByTypeCollection<IEndpointBehavior> behaviors = endpoint.Behaviors;
                endpoint.ListenUri = clientEndpoint.ListenUri;
                endpoint.ListenUriMode = clientEndpoint.ListenUriMode;
                endpoint.Name = clientEndpoint.Name;
                foreach (IEndpointBehavior behavior in clientEndpoint.Behaviors)
                {
                    // Remove if present, ok to call if not there (will simply return false)
                    behaviors.Remove(behavior.GetType());
                    behaviors.Add(behavior);
                }

                // If the configuration doesn't explicitly add MustUnderstandBehavior (to override us)
                // add it here, with mustunderstand = false.
                if (behaviors.Find<MustUnderstandBehavior>() == null)
                {
                    behaviors.Add(new MustUnderstandBehavior(false));
                }

                // If the configuration doesn't explicitly turn off marshaling we add it here.
                if (routingConfig.SoapProcessingEnabled && behaviors.Find<SoapProcessingBehavior>() == null)
                {
                    behaviors.Add(new SoapProcessingBehavior());
                }

                ConfigureTransactionFlow(endpoint);
                ConfigureImpersonation(endpoint, impersonating);
            }

            protected override TChannel CreateChannel()
            {
                TChannel channel = base.CreateChannel();
                ((ICommunicationObject)channel).Faulted += this.InnerChannelFaulted;
                return channel;
            }

            public IAsyncResult BeginOperation(Message message, Transaction transaction, AsyncCallback callback, object state)
            {
                return new OperationAsyncResult(this, message, transaction, callback, state);
            }

            public Message EndOperation(IAsyncResult result)
            {
                return OperationAsyncResult.End(result);
            }

            protected abstract IAsyncResult OnBeginOperation(Message message, AsyncCallback callback, object state);
            protected abstract Message OnEndOperation(IAsyncResult asyncResult);

            void InnerChannelFaulted(object sender, EventArgs args)
            {
                EventHandler handlers = this.Faulted;
                if (handlers != null)
                {
                    handlers(this, args);
                }
            }

            class OperationAsyncResult : TransactedAsyncResult
            {
                static AsyncCompletion openComplete = OpenComplete;
                static AsyncCompletion operationComplete = OperationComplete;
                static Action<object> signalWaiter;

                RoutingClientBase<TChannel> parent;
                Message replyMessage;
                Message requestMessage;
                Transaction transaction;

                public OperationAsyncResult(RoutingClientBase<TChannel> parent, Message requestMessage, Transaction transaction, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;
                    this.requestMessage = requestMessage;
                    this.transaction = transaction;

                    bool shouldOpen = false;

                    if (!this.parent.openCompleted)
                    {
                        lock (this.parent.thisLock)
                        {
                            if (!this.parent.openCompleted)
                            {
                                //The first to open initializes the waiters queue others add themselves to it.
                                if (this.parent.waiters == null)
                                {
                                    //it's our job to open the proxy
                                    this.parent.waiters = new Queue<OperationAsyncResult>();
                                    shouldOpen = true;
                                }
                                else
                                {
                                    //Someone beat us to it, just join the list of waiters.
                                    this.parent.waiters.Enqueue(this);
                                    return;
                                }
                            }
                        }
                    }

                    if (shouldOpen)
                    {
                        //we are the first so we need to open this channel
                        IAsyncResult asyncResult;
                        using (this.PrepareTransactionalCall(this.transaction))
                        {
                            //This will use the binding's OpenTimeout.
                            asyncResult = ((ICommunicationObject)this.parent).BeginOpen(this.PrepareAsyncCompletion(openComplete), this);
                        }
                        if (this.SyncContinue(asyncResult))
                        {
                            this.Complete(true);
                        }
                    }
                    else
                    {
                        if (this.CallOperation())
                        {
                            this.Complete(true);
                        }
                    }
                }

                public static Message End(IAsyncResult result)
                {
                    OperationAsyncResult thisPtr = AsyncResult.End<OperationAsyncResult>(result);
                    return thisPtr.replyMessage;
                }

                static bool OpenComplete(IAsyncResult openResult)
                {
                    OperationAsyncResult thisPtr = (OperationAsyncResult)openResult.AsyncState;
                    try
                    {
                        ((ICommunicationObject)thisPtr.parent).EndOpen(openResult);
                    }
                    finally
                    {
                        Queue<OperationAsyncResult> localWaiters = null;

                        lock (thisPtr.parent.thisLock)
                        {
                            localWaiters = thisPtr.parent.waiters;
                            thisPtr.parent.waiters = null;
                            thisPtr.parent.openCompleted = true;
                        }

                        if (localWaiters != null && localWaiters.Count > 0)
                        {
                            if (signalWaiter == null)
                            {
                                signalWaiter = new Action<object>(SignalWaiter);
                            }

                            //foreach over Queue<T> goes FIFO
                            foreach (OperationAsyncResult waiter in localWaiters)
                            {
                                ActionItem.Schedule(signalWaiter, waiter);
                            }
                        }
                    }
                    return thisPtr.CallOperation();
                }

                bool CallOperation()
                {
                    IAsyncResult asyncResult;
                    using (this.PrepareTransactionalCall(this.transaction))
                    {
                        asyncResult = this.parent.OnBeginOperation(this.requestMessage, this.PrepareAsyncCompletion(operationComplete), this);
                    }
                    return this.SyncContinue(asyncResult);
                }

                static bool OperationComplete(IAsyncResult result)
                {
                    OperationAsyncResult thisPtr = (OperationAsyncResult)result.AsyncState;
                    thisPtr.replyMessage = thisPtr.parent.OnEndOperation(result);
                    return true;
                }

                static void SignalWaiter(object state)
                {
                    OperationAsyncResult waiter = (OperationAsyncResult)state;
                    try
                    {
                        if (waiter.CallOperation())
                        {
                            waiter.Complete(false);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        waiter.Complete(false, exception);
                    }
                }
            }
        }

        class SimplexDatagramClient : RoutingClientBase<ISimplexDatagramRouter>
        {
            public SimplexDatagramClient(RoutingEndpointTrait endpointTrait, RoutingConfiguration routingConfig, bool impersonating)
                : base(endpointTrait, routingConfig, impersonating)
            {
            }

            protected override IAsyncResult OnBeginOperation(Message message, AsyncCallback callback, object state)
            {
                return this.Channel.BeginProcessMessage(message, callback, state);
            }

            protected override Message OnEndOperation(IAsyncResult result)
            {
                this.Channel.EndProcessMessage(result);
                return null;
            }
        }

        class SimplexSessionClient : RoutingClientBase<ISimplexSessionRouter>
        {
            public SimplexSessionClient(RoutingEndpointTrait endointTrait, RoutingConfiguration routingConfig, bool impersonating)
                : base(endointTrait, routingConfig, impersonating)
            {
            }

            protected override IAsyncResult OnBeginOperation(Message message, AsyncCallback callback, object state)
            {
                return this.Channel.BeginProcessMessage(message, callback, state);
            }

            protected override Message OnEndOperation(IAsyncResult result)
            {
                this.Channel.EndProcessMessage(result);
                return null;
            }
        }

        class DuplexSessionClient : RoutingClientBase<IDuplexSessionRouter>
        {
            public DuplexSessionClient(RoutingService service, RoutingEndpointTrait endpointTrait, bool impersonating)
                : base(endpointTrait, service.RoutingConfig, new DuplexCallbackProxy(service.ChannelExtension.ActivityID, endpointTrait.CallbackInstance), impersonating)
            {
            }

            protected override IAsyncResult OnBeginOperation(Message message, AsyncCallback callback, object state)
            {
                return this.Channel.BeginProcessMessage(message, callback, state);
            }

            protected override Message OnEndOperation(IAsyncResult result)
            {
                this.Channel.EndProcessMessage(result);
                return null;
            }

            class DuplexCallbackProxy : IDuplexRouterCallback
            {
                Guid activityID;
                IDuplexRouterCallback callbackInstance;
                EventTraceActivity eventTraceActivity;

                public DuplexCallbackProxy(Guid activityID, IDuplexRouterCallback callbackInstance)
                {
                    this.activityID = activityID;
                    this.callbackInstance = callbackInstance;
                    if (Fx.Trace.IsEtwProviderEnabled)
                    {
                        this.eventTraceActivity = new EventTraceActivity(activityID);
                    }
                }

                IAsyncResult IDuplexRouterCallback.BeginProcessMessage(Message message, AsyncCallback callback, object state)
                {
                    FxTrace.Trace.SetAndTraceTransfer(this.activityID, true);
                    try
                    {
                        return new CallbackAsyncResult(this.callbackInstance, message, callback, state);
                    }
                    catch (Exception e)
                    {
                        if (TD.RoutingServiceDuplexCallbackExceptionIsEnabled())
                        {
                            TD.RoutingServiceDuplexCallbackException(this.eventTraceActivity, "DuplexCallbackProxy.BeginProcessMessage", e);
                        }
                        throw;
                    }
                }

                void IDuplexRouterCallback.EndProcessMessage(IAsyncResult result)
                {
                    FxTrace.Trace.SetAndTraceTransfer(this.activityID, true);
                    try
                    {
                        CallbackAsyncResult.End(result);
                    }
                    catch (Exception e)
                    {
                        if (TD.RoutingServiceDuplexCallbackExceptionIsEnabled())
                        {
                            TD.RoutingServiceDuplexCallbackException(this.eventTraceActivity, "DuplexCallbackProxy.EndProcessMessage", e);
                        }
                        throw;
                    }
                }

                // We have to have an AsyncResult implementation here in order to handle the 
                // TransactionScope appropriately (use PrepareTransactionalCall, SyncContinue, etc...)
                class CallbackAsyncResult : TransactedAsyncResult
                {
                    static AsyncCompletion processCallback = ProcessCallback;
                    IDuplexRouterCallback callbackInstance;

                    public CallbackAsyncResult(IDuplexRouterCallback callbackInstance, Message message, AsyncCallback callback, object state)
                        : base(callback, state)
                    {
                        this.callbackInstance = callbackInstance;

                        IAsyncResult result;
                        using (this.PrepareTransactionalCall(TransactionMessageProperty.TryGetTransaction(message)))
                        {
                            result = this.callbackInstance.BeginProcessMessage(message,
                                this.PrepareAsyncCompletion(processCallback), this);
                        }

                        if (this.SyncContinue(result))
                        {
                            this.Complete(true);
                        }
                    }

                    public static void End(IAsyncResult result)
                    {
                        AsyncResult.End<CallbackAsyncResult>(result);
                    }

                    static bool ProcessCallback(IAsyncResult result)
                    {
                        CallbackAsyncResult thisPtr = (CallbackAsyncResult)result.AsyncState;
                        thisPtr.callbackInstance.EndProcessMessage(result);
                        return true;
                    }
                }
            }
        }

        class RequestReplyClient : RoutingClientBase<IRequestReplyRouter>
        {
            public RequestReplyClient(RoutingEndpointTrait endpointTrait, RoutingConfiguration routingConfig, bool impersonating)
                : base(endpointTrait, routingConfig, impersonating)
            {
            }

            protected override IAsyncResult OnBeginOperation(Message message, AsyncCallback callback, object state)
            {
                return this.Channel.BeginProcessRequest(message, callback, state);
            }

            protected override Message OnEndOperation(IAsyncResult result)
            {
                return this.Channel.EndProcessRequest(result);
            }
        }
    }
}
