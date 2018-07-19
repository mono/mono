//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Remoting.Messaging;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics.Application;
    using System.Transactions;

    class ImmutableClientRuntime
    {
        int correlationCount;
        bool addTransactionFlowProperties;
        IInteractiveChannelInitializer[] interactiveChannelInitializers;
        IClientOperationSelector operationSelector;
        IChannelInitializer[] channelInitializers;
        IClientMessageInspector[] messageInspectors;
        Dictionary<string, ProxyOperationRuntime> operations;
        ProxyOperationRuntime unhandled;
        bool useSynchronizationContext;
        bool validateMustUnderstand;

        internal ImmutableClientRuntime(ClientRuntime behavior)
        {
            this.channelInitializers = EmptyArray<IChannelInitializer>.ToArray(behavior.ChannelInitializers);
            this.interactiveChannelInitializers = EmptyArray<IInteractiveChannelInitializer>.ToArray(behavior.InteractiveChannelInitializers);
            this.messageInspectors = EmptyArray<IClientMessageInspector>.ToArray(behavior.MessageInspectors);

            this.operationSelector = behavior.OperationSelector;
            this.useSynchronizationContext = behavior.UseSynchronizationContext;
            this.validateMustUnderstand = behavior.ValidateMustUnderstand;

            this.unhandled = new ProxyOperationRuntime(behavior.UnhandledClientOperation, this);

            this.addTransactionFlowProperties = behavior.AddTransactionFlowProperties;

            this.operations = new Dictionary<string, ProxyOperationRuntime>();

            for (int i = 0; i < behavior.Operations.Count; i++)
            {
                ClientOperation operation = behavior.Operations[i];
                ProxyOperationRuntime operationRuntime = new ProxyOperationRuntime(operation, this);
                this.operations.Add(operation.Name, operationRuntime);
            }

            this.correlationCount = this.messageInspectors.Length + behavior.MaxParameterInspectors;
        }

        internal int MessageInspectorCorrelationOffset
        {
            get { return 0; }
        }

        internal int ParameterInspectorCorrelationOffset
        {
            get { return this.messageInspectors.Length; }
        }

        internal int CorrelationCount
        {
            get { return this.correlationCount; }
        }

        internal IClientOperationSelector OperationSelector
        {
            get { return this.operationSelector; }
        }

        internal ProxyOperationRuntime UnhandledProxyOperation
        {
            get { return this.unhandled; }
        }

        internal bool UseSynchronizationContext
        {
            get { return this.useSynchronizationContext; }
        }

        internal bool ValidateMustUnderstand
        {
            get { return validateMustUnderstand; }
            set { validateMustUnderstand = value; }
        }

        internal void AfterReceiveReply(ref ProxyRpc rpc)
        {
            int offset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    this.messageInspectors[i].AfterReceiveReply(ref rpc.Reply, rpc.Correlation[offset + i]);
                    if (TD.ClientMessageInspectorAfterReceiveInvokedIsEnabled())
                    {
                        TD.ClientMessageInspectorAfterReceiveInvoked(rpc.EventTraceActivity, this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        internal void BeforeSendRequest(ref ProxyRpc rpc)
        {
            int offset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    rpc.Correlation[offset + i] = this.messageInspectors[i].BeforeSendRequest(ref rpc.Request, (IClientChannel)rpc.Channel.Proxy);
                    if (TD.ClientMessageInspectorBeforeSendInvokedIsEnabled())
                    {
                        TD.ClientMessageInspectorBeforeSendInvoked(rpc.EventTraceActivity, this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }

            if (this.addTransactionFlowProperties)
            {
                SendTransaction(ref rpc);
            }
        }

        internal void DisplayInitializationUI(ServiceChannel channel)
        {
            EndDisplayInitializationUI(BeginDisplayInitializationUI(channel, null, null));
        }

        internal IAsyncResult BeginDisplayInitializationUI(ServiceChannel channel, AsyncCallback callback, object state)
        {
            return new DisplayInitializationUIAsyncResult(channel, this.interactiveChannelInitializers, callback, state);
        }

        internal void EndDisplayInitializationUI(IAsyncResult result)
        {
            DisplayInitializationUIAsyncResult.End(result);
        }

        // this should not be inlined, since we want to JIT the reference to System.Transactions
        // only if transactions are being flowed.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void SendTransaction(ref ProxyRpc rpc)
        {
            System.ServiceModel.Channels.TransactionFlowProperty.Set(Transaction.Current, rpc.Request);
        }

        internal void InitializeChannel(IClientChannel channel)
        {
            try
            {
                for (int i = 0; i < this.channelInitializers.Length; ++i)
                {
                    this.channelInitializers[i].Initialize(channel);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        internal ProxyOperationRuntime GetOperation(MethodBase methodBase, object[] args, out bool canCacheResult)
        {
            if (this.operationSelector == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException
                                                        (SR.GetString(SR.SFxNeedProxyBehaviorOperationSelector2,
                                                                      methodBase.Name,
                                                                      methodBase.DeclaringType.Name)));
            }

            try
            {
                if (operationSelector.AreParametersRequiredForSelection)
                {
                    canCacheResult = false;
                }
                else
                {
                    args = null;
                    canCacheResult = true;
                }
                string operationName = operationSelector.SelectOperation(methodBase, args);
                ProxyOperationRuntime operation;
                if ((operationName != null) && this.operations.TryGetValue(operationName, out operation))
                {
                    return operation;
                }
                else
                {
                    // did not find the right operation, will not know how 
                    // to invoke the method.
                    return null;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        internal ProxyOperationRuntime GetOperationByName(string operationName)
        {
            ProxyOperationRuntime operation = null;
            if (this.operations.TryGetValue(operationName, out operation))
                return operation;
            else
                return null;
        }

        class DisplayInitializationUIAsyncResult : System.Runtime.AsyncResult
        {
            ServiceChannel channel;
            int index = -1;
            IInteractiveChannelInitializer[] initializers;
            IClientChannel proxy;

            static AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(DisplayInitializationUIAsyncResult.Callback));

            internal DisplayInitializationUIAsyncResult(ServiceChannel channel,
                                                        IInteractiveChannelInitializer[] initializers,
                                                        AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.initializers = initializers;
                this.proxy = channel.Proxy as IClientChannel;
                this.CallBegin(true);
            }

            void CallBegin(bool completedSynchronously)
            {
                while (++this.index < initializers.Length)
                {
                    IAsyncResult result = null;
                    Exception exception = null;

                    try
                    {
                        result = this.initializers[this.index].BeginDisplayInitializationUI(
                            this.proxy,
                            DisplayInitializationUIAsyncResult.callback,
                            this
                        );
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        exception = e;
                    }

                    if (exception == null)
                    {
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }

                        this.CallEnd(result, out exception);
                    }

                    if (exception != null)
                    {
                        this.CallComplete(completedSynchronously, exception);
                        return;
                    }
                }

                this.CallComplete(completedSynchronously, null);
            }

            static void Callback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                DisplayInitializationUIAsyncResult outer = (DisplayInitializationUIAsyncResult)result.AsyncState;
                Exception exception = null;

                outer.CallEnd(result, out exception);

                if (exception != null)
                {
                    outer.CallComplete(false, exception);
                    return;
                }

                outer.CallBegin(false);
            }

            void CallEnd(IAsyncResult result, out Exception exception)
            {
                try
                {
                    this.initializers[this.index].EndDisplayInitializationUI(result);
                    exception = null;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exception = e;
                }
            }

            void CallComplete(bool completedSynchronously, Exception exception)
            {
                this.Complete(completedSynchronously, exception);
            }

            internal static void End(IAsyncResult result)
            {
                System.Runtime.AsyncResult.End<DisplayInitializationUIAsyncResult>(result);
            }
        }
    }
}
