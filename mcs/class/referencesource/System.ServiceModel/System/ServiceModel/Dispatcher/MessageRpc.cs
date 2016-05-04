//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;
    using System.Transactions;
    using System.ServiceModel.Diagnostics.Application;

    delegate void MessageRpcProcessor(ref MessageRpc rpc);

    struct MessageRpc
    {
        internal readonly ServiceChannel Channel;
        internal readonly ChannelHandler channelHandler;
        internal readonly object[] Correlation;
        internal readonly ServiceHostBase Host;
        internal readonly OperationContext OperationContext;
        internal ServiceModelActivity Activity;
        internal Guid ResponseActivityId;
        internal IAsyncResult AsyncResult;
        internal bool CanSendReply;
        internal bool SuccessfullySendReply;
        internal CorrelationCallbackMessageProperty CorrelationCallback;
        internal object[] InputParameters;
        internal object[] OutputParameters;
        internal object ReturnParameter;
        internal bool ParametersDisposed;
        internal bool DidDeserializeRequestBody;
        internal TransactionMessageProperty TransactionMessageProperty;
        internal TransactedBatchContext TransactedBatchContext;
        internal Exception Error;
        internal MessageRpcProcessor ErrorProcessor;
        internal ErrorHandlerFaultInfo FaultInfo;
        internal bool HasSecurityContext;
        internal object Instance;
        internal bool MessageRpcOwnsInstanceContextThrottle;
        internal MessageRpcProcessor NextProcessor;
        internal Collection<MessageHeaderInfo> NotUnderstoodHeaders;
        internal DispatchOperationRuntime Operation;
        internal Message Request;
        internal RequestContext RequestContext;
        internal bool RequestContextThrewOnReply;
        internal UniqueId RequestID;
        internal Message Reply;
        internal TimeoutHelper ReplyTimeoutHelper;
        internal RequestReplyCorrelator.ReplyToInfo ReplyToInfo;
        internal MessageVersion RequestVersion;
        internal ServiceSecurityContext SecurityContext;
        internal InstanceContext InstanceContext;
        internal bool SuccessfullyBoundInstance;
        internal bool SuccessfullyIncrementedActivity;
        internal bool SuccessfullyLockedInstance;
        internal ReceiveContextRPCFacet ReceiveContext;
        internal TransactionRpcFacet transaction;
        internal IAspNetMessageProperty HostingProperty;
        internal MessageRpcInvokeNotification InvokeNotification;
        internal EventTraceActivity EventTraceActivity;

        static AsyncCallback handleEndComplete = Fx.ThunkCallback(new AsyncCallback(HandleEndComplete));
        static AsyncCallback handleEndAbandon = Fx.ThunkCallback(new AsyncCallback(HandleEndAbandon));

        bool paused;
        bool switchedThreads;
        bool isInstanceContextSingleton;
        SignalGate<IAsyncResult> invokeContinueGate;

        internal MessageRpc(RequestContext requestContext, Message request, DispatchOperationRuntime operation,
            ServiceChannel channel, ServiceHostBase host, ChannelHandler channelHandler, bool cleanThread,
            OperationContext operationContext, InstanceContext instanceContext, EventTraceActivity eventTraceActivity)
        {
            Fx.Assert((operationContext != null), "System.ServiceModel.Dispatcher.MessageRpc.MessageRpc(), operationContext == null");
            Fx.Assert(channelHandler != null, "System.ServiceModel.Dispatcher.MessageRpc.MessageRpc(), channelHandler == null");

            this.Activity = null;
            this.EventTraceActivity = eventTraceActivity;            
            this.AsyncResult = null;
            this.CanSendReply = true;
            this.Channel = channel;
            this.channelHandler = channelHandler;
            this.Correlation = EmptyArray.Allocate(operation.Parent.CorrelationCount);
            this.CorrelationCallback = null;
            this.DidDeserializeRequestBody = false;
            this.TransactionMessageProperty = null;
            this.TransactedBatchContext = null;
            this.Error = null;
            this.ErrorProcessor = null;
            this.FaultInfo = new ErrorHandlerFaultInfo(request.Version.Addressing.DefaultFaultAction);
            this.HasSecurityContext = false;
            this.Host = host;
            this.Instance = null;
            this.MessageRpcOwnsInstanceContextThrottle = false;
            this.NextProcessor = null;
            this.NotUnderstoodHeaders = null;
            this.Operation = operation;
            this.OperationContext = operationContext;
            this.paused = false;
            this.ParametersDisposed = false;
            this.ReceiveContext = null;
            this.Request = request;
            this.RequestContext = requestContext;
            this.RequestContextThrewOnReply = false;
            this.SuccessfullySendReply = false;
            this.RequestVersion = request.Version;
            this.Reply = null;
            this.ReplyTimeoutHelper = new TimeoutHelper();
            this.SecurityContext = null;
            this.InstanceContext = instanceContext;
            this.SuccessfullyBoundInstance = false;
            this.SuccessfullyIncrementedActivity = false;
            this.SuccessfullyLockedInstance = false;
            this.switchedThreads = !cleanThread;
            this.transaction = null;
            this.InputParameters = null;
            this.OutputParameters = null;
            this.ReturnParameter = null;
            this.isInstanceContextSingleton = InstanceContextProviderBase.IsProviderSingleton(this.Channel.DispatchRuntime.InstanceContextProvider);
            this.invokeContinueGate = null;

            if (!operation.IsOneWay && !operation.Parent.ManualAddressing)
            {
                this.RequestID = request.Headers.MessageId;
                this.ReplyToInfo = new RequestReplyCorrelator.ReplyToInfo(request);
            }
            else
            {
                this.RequestID = null;
                this.ReplyToInfo = new RequestReplyCorrelator.ReplyToInfo();
            }

            this.HostingProperty = AspNetEnvironment.Current.GetHostingProperty(request, true);

            if (DiagnosticUtility.ShouldUseActivity)
            {
                this.Activity = TraceUtility.ExtractActivity(this.Request);
            }

            if (DiagnosticUtility.ShouldUseActivity || TraceUtility.ShouldPropagateActivity)
            {
                this.ResponseActivityId = ActivityIdHeader.ExtractActivityId(this.Request);
            }
            else
            {
                this.ResponseActivityId = Guid.Empty;
            }

            this.InvokeNotification = new MessageRpcInvokeNotification(this.Activity, this.channelHandler);

            if (this.EventTraceActivity == null && FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                if (this.Request != null)
                {
                    this.EventTraceActivity = EventTraceActivityHelper.TryExtractActivity(this.Request, true);
                }
            }
        }

        internal bool FinalizeCorrelationImplicitly
        {
            get { return this.CorrelationCallback != null && this.CorrelationCallback.IsFullyDefined; }
        }

        internal bool IsPaused
        {
            get { return this.paused; }
        }

        internal bool SwitchedThreads
        {
            get { return this.switchedThreads; }
        }

        internal bool IsInstanceContextSingleton
        {
            set
            {
                this.isInstanceContextSingleton = value;
            }
        }

        internal TransactionRpcFacet Transaction
        {
            get
            {
                if (this.transaction == null)
                {
                    this.transaction = new TransactionRpcFacet(ref this);
                }
                return this.transaction;
            }
        }

        internal void Abort()
        {
            this.AbortRequestContext();
            this.AbortChannel();
            this.AbortInstanceContext();
        }

        void AbortRequestContext(RequestContext requestContext)
        {
            try
            {
                requestContext.Abort();

                ReceiveContextRPCFacet receiveContext = this.ReceiveContext;

                if (receiveContext != null)
                {
                    this.ReceiveContext = null;
                    IAsyncResult result = receiveContext.BeginAbandon(
                        TimeSpan.MaxValue,
                        handleEndAbandon,
                        new CallbackState
                        {
                            ReceiveContext = receiveContext,
                            ChannelHandler = this.channelHandler
                        });

                    if (result.CompletedSynchronously)
                    {
                        receiveContext.EndAbandon(result);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.channelHandler.HandleError(e);
            }
        }

        internal void AbortRequestContext()
        {
            if (this.OperationContext.RequestContext != null)
            {
                this.AbortRequestContext(this.OperationContext.RequestContext);
            }
            if ((this.RequestContext != null) && (this.RequestContext != this.OperationContext.RequestContext))
            {
                this.AbortRequestContext(this.RequestContext);
            }
            TraceCallDurationInDispatcherIfNecessary(false);
        }

        void TraceCallDurationInDispatcherIfNecessary(bool requestContextWasClosedSuccessfully)
        {
            // only need to trace once (either for the failure or success case)
            if (TD.DispatchFailedIsEnabled())
            {
                if (requestContextWasClosedSuccessfully)
                {
                    TD.DispatchSuccessful(this.EventTraceActivity, this.Operation.Name);
                }
                else
                {
                    TD.DispatchFailed(this.EventTraceActivity, this.Operation.Name);
                }
            }
        }

        internal void CloseRequestContext()
        {
            if (this.OperationContext.RequestContext != null)
            {
                this.DisposeRequestContext(this.OperationContext.RequestContext);
            }
            if ((this.RequestContext != null) && (this.RequestContext != this.OperationContext.RequestContext))
            {
                this.DisposeRequestContext(this.RequestContext);
            }
            TraceCallDurationInDispatcherIfNecessary(true);
        }

        void DisposeRequestContext(RequestContext context)
        {
            try
            {
                context.Close();

                ReceiveContextRPCFacet receiveContext = this.ReceiveContext;

                if (receiveContext != null)
                {
                    this.ReceiveContext = null;
                    IAsyncResult result = receiveContext.BeginComplete(
                        TimeSpan.MaxValue,
                        null,
                        this.channelHandler,
                        handleEndComplete,
                        new CallbackState
                        {
                            ChannelHandler = this.channelHandler,
                            ReceiveContext = receiveContext
                        });

                    if (result.CompletedSynchronously)
                    {
                        receiveContext.EndComplete(result);
                    }

                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.AbortRequestContext(context);
                this.channelHandler.HandleError(e);
            }
        }

        static void HandleEndAbandon(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            CallbackState callbackState = (CallbackState)result.AsyncState;

            try
            {
                callbackState.ReceiveContext.EndAbandon(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                callbackState.ChannelHandler.HandleError(e);
            }
        }

        static void HandleEndComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            CallbackState callbackState = (CallbackState)result.AsyncState;

            try
            {
                callbackState.ReceiveContext.EndComplete(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                callbackState.ChannelHandler.HandleError(e);
            }
        }

        internal void AbortChannel()
        {
            if ((this.Channel != null) && this.Channel.HasSession)
            {
                try
                {
                    this.Channel.Abort();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(e);
                }
            }
        }

        internal void CloseChannel()
        {
            if ((this.Channel != null) && this.Channel.HasSession)
            {
                try
                {
                    this.Channel.Close(ChannelHandler.CloseAfterFaultTimeout);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(e);
                }
            }
        }

        internal void AbortInstanceContext()
        {
            if (this.InstanceContext != null && !this.isInstanceContextSingleton)
            {
                try
                {
                    this.InstanceContext.Abort();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(e);
                }
            }
        }

        internal void EnsureReceive()
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                ChannelHandler.Register(this.channelHandler);
            }
        }

        bool ProcessError(Exception e)
        {
            MessageRpcProcessor handler = this.ErrorProcessor;
            try
            {
                Type exceptionType = e.GetType();

                if (exceptionType.IsAssignableFrom(typeof(FaultException)))
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                else
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                }

                if (TraceUtility.MessageFlowTracingOnly)
                {
                    TraceUtility.SetActivityId(this.Request.Properties);
                    if (Guid.Empty == DiagnosticTraceBase.ActivityId)
                    {
                        Guid receivedActivityId = TraceUtility.ExtractActivityId(this.Request);
                        if (Guid.Empty != receivedActivityId)
                        {
                            DiagnosticTraceBase.ActivityId = receivedActivityId;
                        }
                    }
                }


                this.Error = e;

                if (this.ErrorProcessor != null)
                {
                    this.ErrorProcessor(ref this);
                }

                return (this.Error == null);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e2)
            {
                if (Fx.IsFatal(e2))
                {
                    throw;
                }

                return ((handler != this.ErrorProcessor) && this.ProcessError(e2));
            }
        }

        internal void DisposeParameters(bool excludeInput)
        {
            if (this.Operation.DisposeParameters)
            {
                this.DisposeParametersCore(excludeInput);
            }
        }

        internal void DisposeParametersCore(bool excludeInput)
        {
            if (!this.ParametersDisposed)
            {
                if (!excludeInput)
                {
                    this.DisposeParameterList(this.InputParameters);
                }

                this.DisposeParameterList(this.OutputParameters);

                IDisposable disposableParameter = this.ReturnParameter as IDisposable;
                if (disposableParameter != null)
                {
                    try
                    {
                        disposableParameter.Dispose();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.channelHandler.HandleError(e);
                    }
                }
                this.ParametersDisposed = true;
            }
        }

        void DisposeParameterList(object[] parameters)
        {
            IDisposable disposableParameter = null;
            if (parameters != null)
            {
                foreach (Object obj in parameters)
                {
                    disposableParameter = obj as IDisposable;
                    if (disposableParameter != null)
                    {
                        try
                        {
                            disposableParameter.Dispose();
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            this.channelHandler.HandleError(e);
                        }
                    }
                }
            }
        }

        // See notes on UnPause and Resume (mutually exclusive)
        // Pausing will Increment the BusyCount for the hosting environment
        internal IResumeMessageRpc Pause()
        {
            Wrapper wrapper = new Wrapper(ref this);
            this.paused = true;
            return wrapper;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method ApplyHostingIntegrationContextNoInline. Caller must ensure that"
            + "function is called appropriately and result is guarded and Dispose()'d correctly.")]
        [SecurityCritical]
        IDisposable ApplyHostingIntegrationContext()
        {
            if (this.HostingProperty != null)
            {
                return this.ApplyHostingIntegrationContextNoInline();
            }
            else
            {
                return null;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method HostingMessageProperty.ApplyIntegrationContext. Caller must ensure that"
            + "function is called appropriately and result is guarded and Dispose()'d correctly.")]
        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        IDisposable ApplyHostingIntegrationContextNoInline()
        {
            return this.HostingProperty.ApplyIntegrationContext();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method ApplyHostingIntegrationContext.",
            Safe = "Does call properly and calls Dispose, doesn't leak control of the IDisposable out of the function.")]
        [SecuritySafeCritical]
        internal bool Process(bool isOperationContextSet)
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                bool completed = true;

                if (this.NextProcessor != null)
                {
                    MessageRpcProcessor processor = this.NextProcessor;
                    this.NextProcessor = null;

                    OperationContext originalContext;
                    OperationContext.Holder contextHolder;
                    if (!isOperationContextSet)
                    {
                        contextHolder = OperationContext.CurrentHolder;
                        originalContext = contextHolder.Context;
                    }
                    else
                    {
                        contextHolder = null;
                        originalContext = null;
                    }
                    IncrementBusyCount();

                    IDisposable hostedIntegrationContext = this.ApplyHostingIntegrationContext();

                    try
                    {
                        if (!isOperationContextSet)
                        {
                            contextHolder.Context = this.OperationContext;
                        }

                        processor(ref this);

                        if (!this.paused)
                        {
                            this.OperationContext.SetClientReply(null, false);
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        if (!this.ProcessError(e) && this.FaultInfo.Fault == null)
                        {
                            this.Abort();
                        }
                    }
                    finally
                    {
                        try
                        {
                            DecrementBusyCount();

                            if (hostedIntegrationContext != null)
                            {
                                hostedIntegrationContext.Dispose();
                            }

                            if (!isOperationContextSet)
                            {
                                contextHolder.Context = originalContext;
                            }

                            completed = !this.paused;
                            if (completed)
                            {
                                this.channelHandler.DispatchDone();
                                this.OperationContext.ClearClientReplyNoThrow();
                            }
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(e.Message, e);
                        }
                    }
                }

                return completed;
            }
        }

        // UnPause is called on the original MessageRpc to continue work on the current thread, and the copy is ignored.
        // Since the copy is ignored, Decrement the BusyCount
        internal void UnPause()
        {
            this.paused = false;
            DecrementBusyCount();

        }

        internal bool UnlockInvokeContinueGate(out IAsyncResult result)
        {
            return this.invokeContinueGate.Unlock(out result);
        }

        internal void PrepareInvokeContinueGate()
        {
            this.invokeContinueGate = new SignalGate<IAsyncResult>();
        }

        void IncrementBusyCount()
        {
            // Only increment the counter on the service side.
            if (this.Host != null)
            {
                this.Host.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(SR.GetString(SR.ServiceBusyCountTrace, this.Operation.Action));
                }
            }
        }

        void DecrementBusyCount()
        {
            if (this.Host != null)
            {
                this.Host.DecrementBusyCount();
                if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceDecrementBusyCount(SR.GetString(SR.ServiceBusyCountTrace, this.Operation.Action));
                }
            }
        }

        class CallbackState
        {
            public ReceiveContextRPCFacet ReceiveContext
            {
                get;
                set;
            }

            public ChannelHandler ChannelHandler
            {
                get;
                set;
            }
        }

        class Wrapper : IResumeMessageRpc
        {
            MessageRpc rpc;
            bool alreadyResumed;

            internal Wrapper(ref MessageRpc rpc)
            {
                this.rpc = rpc;
                if (rpc.NextProcessor == null)
                {
                    Fx.Assert("MessageRpc.Wrapper.Wrapper: (rpc.NextProcessor != null)");
                }
                this.rpc.IncrementBusyCount();

            }

            public InstanceContext GetMessageInstanceContext()
            {
                return this.rpc.InstanceContext;
            }

            // Resume is called on the copy on some completing thread, whereupon work continues on that thread.
            // BusyCount is Decremented as the copy is now complete
            public void Resume(out bool alreadyResumedNoLock)
            {
                try
                {
                    alreadyResumedNoLock = this.alreadyResumed;
                    this.alreadyResumed = true;

                    this.rpc.switchedThreads = true;
                    if (this.rpc.Process(false) && !rpc.InvokeNotification.DidInvokerEnsurePump)
                    {
                        this.rpc.EnsureReceive();
                    }
                }
                finally
                {
                    this.rpc.DecrementBusyCount();

                }
            }

            public void Resume(IAsyncResult result)
            {
                this.rpc.AsyncResult = result;
                this.Resume();
            }

            public void Resume(object instance)
            {
                this.rpc.Instance = instance;
                this.Resume();
            }

            public void Resume()
            {
                using (ServiceModelActivity.BoundOperation(this.rpc.Activity, true))
                {
                    bool alreadyResumedNoLock;
                    this.Resume(out alreadyResumedNoLock);
                    if (alreadyResumedNoLock)
                    {
                        string text = SR.GetString(SR.SFxMultipleCallbackFromAsyncOperation, rpc.Operation.Name);
                        Exception error = new InvalidOperationException(text);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                    }
                }
            }

            public void SignalConditionalResume(IAsyncResult result)
            {
                if (this.rpc.invokeContinueGate.Signal(result))
                {
                    this.rpc.AsyncResult = result;
                    Resume();
                }
            }
        }

    }

    class MessageRpcInvokeNotification : IInvokeReceivedNotification
    {
        ServiceModelActivity activity;
        ChannelHandler handler;

        public MessageRpcInvokeNotification(ServiceModelActivity activity, ChannelHandler handler)
        {
            this.activity = activity;
            this.handler = handler;
        }

        public bool DidInvokerEnsurePump { get; set; }

        public void NotifyInvokeReceived()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ChannelHandler.Register(this.handler);
            }
            this.DidInvokerEnsurePump = true;
        }

        public void NotifyInvokeReceived(RequestContext request)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ChannelHandler.Register(this.handler, request);
            }
            this.DidInvokerEnsurePump = true;
        }
    }
}
