//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Transactions;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime.Diagnostics;
    using System.Security;

    class ImmutableDispatchRuntime
    {
        readonly AuthenticationBehavior authenticationBehavior;
        readonly AuthorizationBehavior authorizationBehavior;
        readonly int correlationCount;
        readonly ConcurrencyBehavior concurrency;
        readonly IDemuxer demuxer;
        readonly ErrorBehavior error;
        readonly bool enableFaults;
        readonly bool ignoreTransactionFlow;
        readonly bool impersonateOnSerializingReply;
        readonly IInputSessionShutdown[] inputSessionShutdownHandlers;
        readonly InstanceBehavior instance;
        readonly bool isOnServer;
        readonly bool manualAddressing;
        readonly IDispatchMessageInspector[] messageInspectors;
        readonly int parameterInspectorCorrelationOffset;
        readonly IRequestReplyCorrelator requestReplyCorrelator;
        readonly SecurityImpersonationBehavior securityImpersonation;
        readonly TerminatingOperationBehavior terminate;
        readonly ThreadBehavior thread;
        readonly TransactionBehavior transaction;
        readonly bool validateMustUnderstand;
        readonly bool receiveContextEnabledChannel;
        readonly bool sendAsynchronously;
        readonly bool requireClaimsPrincipalOnOperationContext;

        readonly MessageRpcProcessor processMessage1;
        readonly MessageRpcProcessor processMessage11;
        readonly MessageRpcProcessor processMessage2;
        readonly MessageRpcProcessor processMessage3;
        readonly MessageRpcProcessor processMessage31;
        readonly MessageRpcProcessor processMessage4;
        readonly MessageRpcProcessor processMessage41;
        readonly MessageRpcProcessor processMessage5;
        readonly MessageRpcProcessor processMessage6;
        readonly MessageRpcProcessor processMessage7;
        readonly MessageRpcProcessor processMessage8;
        readonly MessageRpcProcessor processMessage9;
        readonly MessageRpcProcessor processMessageCleanup;
        readonly MessageRpcProcessor processMessageCleanupError;

        static AsyncCallback onFinalizeCorrelationCompleted =
            Fx.ThunkCallback(new AsyncCallback(OnFinalizeCorrelationCompletedCallback));
        static AsyncCallback onReplyCompleted =
            Fx.ThunkCallback(new AsyncCallback(OnReplyCompletedCallback));

        bool didTraceProcessMessage1 = false;
        bool didTraceProcessMessage2 = false;
        bool didTraceProcessMessage3 = false;
        bool didTraceProcessMessage31 = false;
        bool didTraceProcessMessage4 = false;
        bool didTraceProcessMessage41 = false;

        internal ImmutableDispatchRuntime(DispatchRuntime dispatch)
        {
            this.authenticationBehavior = AuthenticationBehavior.TryCreate(dispatch);
            this.authorizationBehavior = AuthorizationBehavior.TryCreate(dispatch);
            this.concurrency = new ConcurrencyBehavior(dispatch);
            this.error = new ErrorBehavior(dispatch.ChannelDispatcher);
            this.enableFaults = dispatch.EnableFaults;
            this.inputSessionShutdownHandlers = EmptyArray<IInputSessionShutdown>.ToArray(dispatch.InputSessionShutdownHandlers);
            this.instance = new InstanceBehavior(dispatch, this);
            this.isOnServer = dispatch.IsOnServer;
            this.manualAddressing = dispatch.ManualAddressing;
            this.messageInspectors = EmptyArray<IDispatchMessageInspector>.ToArray(dispatch.MessageInspectors);
            this.requestReplyCorrelator = new RequestReplyCorrelator();
            this.securityImpersonation = SecurityImpersonationBehavior.CreateIfNecessary(dispatch);
            this.requireClaimsPrincipalOnOperationContext = dispatch.RequireClaimsPrincipalOnOperationContext;
            this.impersonateOnSerializingReply = dispatch.ImpersonateOnSerializingReply;
            this.terminate = TerminatingOperationBehavior.CreateIfNecessary(dispatch);
            this.thread = new ThreadBehavior(dispatch);
            this.validateMustUnderstand = dispatch.ValidateMustUnderstand;
            this.ignoreTransactionFlow = dispatch.IgnoreTransactionMessageProperty;
            this.transaction = TransactionBehavior.CreateIfNeeded(dispatch);
            this.receiveContextEnabledChannel = dispatch.ChannelDispatcher.ReceiveContextEnabled;
            this.sendAsynchronously = dispatch.ChannelDispatcher.SendAsynchronously;
            this.parameterInspectorCorrelationOffset = (dispatch.MessageInspectors.Count +
                dispatch.MaxCallContextInitializers);
            this.correlationCount = this.parameterInspectorCorrelationOffset + dispatch.MaxParameterInspectors;

            DispatchOperationRuntime unhandled = new DispatchOperationRuntime(dispatch.UnhandledDispatchOperation, this);

            if (dispatch.OperationSelector == null)
            {
                ActionDemuxer demuxer = new ActionDemuxer();
                for (int i = 0; i < dispatch.Operations.Count; i++)
                {
                    DispatchOperation operation = dispatch.Operations[i];
                    DispatchOperationRuntime operationRuntime = new DispatchOperationRuntime(operation, this);
                    demuxer.Add(operation.Action, operationRuntime);
                }

                demuxer.SetUnhandled(unhandled);
                this.demuxer = demuxer;
            }
            else
            {
                CustomDemuxer demuxer = new CustomDemuxer(dispatch.OperationSelector);
                for (int i = 0; i < dispatch.Operations.Count; i++)
                {
                    DispatchOperation operation = dispatch.Operations[i];
                    DispatchOperationRuntime operationRuntime = new DispatchOperationRuntime(operation, this);
                    demuxer.Add(operation.Name, operationRuntime);
                }

                demuxer.SetUnhandled(unhandled);
                this.demuxer = demuxer;
            }

            this.processMessage1 = new MessageRpcProcessor(this.ProcessMessage1);
            this.processMessage11 = new MessageRpcProcessor(this.ProcessMessage11);
            this.processMessage2 = new MessageRpcProcessor(this.ProcessMessage2);
            this.processMessage3 = new MessageRpcProcessor(this.ProcessMessage3);
            this.processMessage31 = new MessageRpcProcessor(this.ProcessMessage31);
            this.processMessage4 = new MessageRpcProcessor(this.ProcessMessage4);
            this.processMessage41 = new MessageRpcProcessor(this.ProcessMessage41);
            this.processMessage5 = new MessageRpcProcessor(this.ProcessMessage5);
            this.processMessage6 = new MessageRpcProcessor(this.ProcessMessage6);
            this.processMessage7 = new MessageRpcProcessor(this.ProcessMessage7);
            this.processMessage8 = new MessageRpcProcessor(this.ProcessMessage8);
            this.processMessage9 = new MessageRpcProcessor(this.ProcessMessage9);
            this.processMessageCleanup = new MessageRpcProcessor(this.ProcessMessageCleanup);
            this.processMessageCleanupError = new MessageRpcProcessor(this.ProcessMessageCleanupError);
        }

        internal int CallContextCorrelationOffset
        {
            get { return this.messageInspectors.Length; }
        }

        internal int CorrelationCount
        {
            get { return this.correlationCount; }
        }

        internal bool EnableFaults
        {
            get { return this.enableFaults; }
        }

        internal InstanceBehavior InstanceBehavior
        {
            get { return this.instance; }
        }

        internal bool IsImpersonationEnabledOnSerializingReply
        {
            get { return this.impersonateOnSerializingReply; }
        }

        internal bool RequireClaimsPrincipalOnOperationContext
        {
            get { return this.requireClaimsPrincipalOnOperationContext; }
        }

        internal bool ManualAddressing
        {
            get { return this.manualAddressing; }
        }

        internal int MessageInspectorCorrelationOffset
        {
            get { return 0; }
        }

        internal int ParameterInspectorCorrelationOffset
        {
            get { return this.parameterInspectorCorrelationOffset; }
        }

        internal IRequestReplyCorrelator RequestReplyCorrelator
        {
            get { return this.requestReplyCorrelator; }
        }

        internal SecurityImpersonationBehavior SecurityImpersonation
        {
            get { return this.securityImpersonation; }
        }

        internal bool ValidateMustUnderstand
        {
            get { return validateMustUnderstand; }
        }

        internal ErrorBehavior ErrorBehavior
        {
            get { return this.error; }
        }

        bool AcquireDynamicInstanceContext(ref MessageRpc rpc)
        {
            if (rpc.InstanceContext.QuotaThrottle != null)
            {
                return AcquireDynamicInstanceContextCore(ref rpc);
            }
            else
            {
                return true;
            }
        }

        bool AcquireDynamicInstanceContextCore(ref MessageRpc rpc)
        {
            bool success = rpc.InstanceContext.QuotaThrottle.Acquire(rpc.Pause());

            if (success)
            {
                rpc.UnPause();
            }

            return success;
        }

        internal void AfterReceiveRequest(ref MessageRpc rpc)
        {
            if (this.messageInspectors.Length > 0)
            {
                AfterReceiveRequestCore(ref rpc);
            }
        }
        internal void AfterReceiveRequestCore(ref MessageRpc rpc)
        {
            int offset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    rpc.Correlation[offset + i] = this.messageInspectors[i].AfterReceiveRequest(ref rpc.Request, (IClientChannel)rpc.Channel.Proxy, rpc.InstanceContext);
                    if (TD.MessageInspectorAfterReceiveInvokedIsEnabled())
                    {
                        TD.MessageInspectorAfterReceiveInvoked(rpc.EventTraceActivity, this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        void BeforeSendReply(ref MessageRpc rpc, ref Exception exception, ref bool thereIsAnUnhandledException)
        {
            if (this.messageInspectors.Length > 0)
            {
                BeforeSendReplyCore(ref rpc, ref exception, ref thereIsAnUnhandledException);
            }
        }

        internal void BeforeSendReplyCore(ref MessageRpc rpc, ref Exception exception, ref bool thereIsAnUnhandledException)
        {
            int offset = this.MessageInspectorCorrelationOffset;
            for (int i = 0; i < this.messageInspectors.Length; i++)
            {
                try
                {
                    Message originalReply = rpc.Reply;
                    Message reply = originalReply;

                    this.messageInspectors[i].BeforeSendReply(ref reply, rpc.Correlation[offset + i]);
                    if (TD.MessageInspectorBeforeSendInvokedIsEnabled())
                    {
                        TD.MessageInspectorBeforeSendInvoked(rpc.EventTraceActivity, this.messageInspectors[i].GetType().FullName);
                    }

                    if ((reply == null) && (originalReply != null))
                    {
                        string message = SR.GetString(SR.SFxNullReplyFromExtension2, this.messageInspectors[i].GetType().ToString(), (rpc.Operation.Name ?? ""));
                        ErrorBehavior.ThrowAndCatch(new InvalidOperationException(message));
                    }
                    rpc.Reply = reply;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (!ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                    }

                    if (exception == null)
                    {
                        exception = e;
                    }
                    thereIsAnUnhandledException = (!this.error.HandleError(e)) || thereIsAnUnhandledException;
                }
            }
        }

        void FinalizeCorrelation(ref MessageRpc rpc)
        {
            Message reply = rpc.Reply;

            if (reply != null && rpc.Error == null)
            {
                if (rpc.transaction != null && rpc.transaction.Current != null &&
                    rpc.transaction.Current.TransactionInformation.Status != TransactionStatus.Active)
                {
                    return;
                }

                CorrelationCallbackMessageProperty callback;

                if (CorrelationCallbackMessageProperty.TryGet(reply, out callback))
                {
                    if (callback.IsFullyDefined)
                    {
                        try
                        {
                            rpc.RequestContextThrewOnReply = true;
                            rpc.CorrelationCallback = callback;

                            rpc.Reply = rpc.CorrelationCallback.FinalizeCorrelation(reply,
                                rpc.ReplyTimeoutHelper.RemainingTime());
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            if (!this.error.HandleError(e))
                            {
                                rpc.CorrelationCallback = null;
                                rpc.CanSendReply = false;
                            }
                        }
                    }
                    else
                    {
                        rpc.CorrelationCallback = new RpcCorrelationCallbackMessageProperty(callback, this, ref rpc);
                        reply.Properties[CorrelationCallbackMessageProperty.Name] = rpc.CorrelationCallback;
                    }
                }
            }
        }

        void BeginFinalizeCorrelation(ref MessageRpc rpc)
        {
            Message reply = rpc.Reply;

            if (reply != null && rpc.Error == null)
            {
                if (rpc.transaction != null && rpc.transaction.Current != null &&
                    rpc.transaction.Current.TransactionInformation.Status != TransactionStatus.Active)
                {
                    return;
                }

                CorrelationCallbackMessageProperty callback;

                if (CorrelationCallbackMessageProperty.TryGet(reply, out callback))
                {
                    if (callback.IsFullyDefined)
                    {
                        bool success = false;

                        try
                        {
                            rpc.RequestContextThrewOnReply = true;
                            rpc.CorrelationCallback = callback;

                            IResumeMessageRpc resume = rpc.Pause();
                            rpc.AsyncResult = rpc.CorrelationCallback.BeginFinalizeCorrelation(reply,
                                rpc.ReplyTimeoutHelper.RemainingTime(), onFinalizeCorrelationCompleted, resume);
                            success = true;

                            if (rpc.AsyncResult.CompletedSynchronously)
                            {
                                rpc.UnPause();
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            if (!this.error.HandleError(e))
                            {
                                rpc.CorrelationCallback = null;
                                rpc.CanSendReply = false;
                            }
                        }
                        finally
                        {
                            if (!success)
                            {
                                rpc.UnPause();
                            }
                        }
                    }
                    else
                    {
                        rpc.CorrelationCallback = new RpcCorrelationCallbackMessageProperty(callback, this, ref rpc);
                        reply.Properties[CorrelationCallbackMessageProperty.Name] = rpc.CorrelationCallback;
                    }
                }
            }
        }

        void Reply(ref MessageRpc rpc)
        {
            rpc.RequestContextThrewOnReply = true;
            rpc.SuccessfullySendReply = false;

            try
            {
                rpc.RequestContext.Reply(rpc.Reply, rpc.ReplyTimeoutHelper.RemainingTime());
                rpc.RequestContextThrewOnReply = false;
                rpc.SuccessfullySendReply = true;

                if (TD.DispatchMessageStopIsEnabled())
                {
                    TD.DispatchMessageStop(rpc.EventTraceActivity);
                }
            }
            catch (CommunicationException e)
            {
                this.error.HandleError(e);
            }
            catch (TimeoutException e)
            {
                this.error.HandleError(e);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.ServiceOperationExceptionOnReply,
                        SR.GetString(SR.TraceCodeServiceOperationExceptionOnReply),
                        this, e);
                }

                if (!this.error.HandleError(e))
                {
                    rpc.RequestContextThrewOnReply = true;
                    rpc.CanSendReply = false;
                }
            }
        }

        void BeginReply(ref MessageRpc rpc)
        {
            bool success = false;

            try
            {
                IResumeMessageRpc resume = rpc.Pause();

                rpc.AsyncResult = rpc.RequestContext.BeginReply(rpc.Reply, rpc.ReplyTimeoutHelper.RemainingTime(),
                    onReplyCompleted, resume);
                success = true;

                if (rpc.AsyncResult.CompletedSynchronously)
                {
                    rpc.UnPause();
                }
            }
            catch (CommunicationException e)
            {
                this.error.HandleError(e);
            }
            catch (TimeoutException e)
            {
                this.error.HandleError(e);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Error,
                        TraceCode.ServiceOperationExceptionOnReply,
                        SR.GetString(SR.TraceCodeServiceOperationExceptionOnReply),
                        this, e);
                }

                if (!this.error.HandleError(e))
                {
                    rpc.RequestContextThrewOnReply = true;
                    rpc.CanSendReply = false;
                }
            }
            finally
            {
                if (!success)
                {
                    rpc.UnPause();
                }
            }
        }

        internal bool Dispatch(ref MessageRpc rpc, bool isOperationContextSet)
        {
            rpc.ErrorProcessor = this.processMessage8;
            rpc.NextProcessor = this.processMessage1;
            return rpc.Process(isOperationContextSet);
        }

        void EndFinalizeCorrelation(ref MessageRpc rpc)
        {
            try
            {
                rpc.Reply = rpc.CorrelationCallback.EndFinalizeCorrelation(rpc.AsyncResult);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (!this.error.HandleError(e))
                {
                    rpc.CanSendReply = false;
                }
            }
        }

        bool EndReply(ref MessageRpc rpc)
        {
            bool success = false;

            try
            {
                rpc.RequestContext.EndReply(rpc.AsyncResult);
                rpc.RequestContextThrewOnReply = false;
                success = true;

                if (TD.DispatchMessageStopIsEnabled())
                {
                    TD.DispatchMessageStop(rpc.EventTraceActivity);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.error.HandleError(e);
            }

            return success;
        }

        internal void InputSessionDoneReceiving(ServiceChannel channel)
        {
            if (this.inputSessionShutdownHandlers.Length > 0)
            {
                this.InputSessionDoneReceivingCore(channel);
            }
        }

        void InputSessionDoneReceivingCore(ServiceChannel channel)
        {
            IDuplexContextChannel proxy = channel.Proxy as IDuplexContextChannel;

            if (proxy != null)
            {
                IInputSessionShutdown[] handlers = this.inputSessionShutdownHandlers;
                try
                {
                    for (int i = 0; i < handlers.Length; i++)
                    {
                        handlers[i].DoneReceiving(proxy);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (!this.error.HandleError(e))
                    {
                        proxy.Abort();
                    }
                }
            }
        }

        internal bool IsConcurrent(ref MessageRpc rpc)
        {
            return this.concurrency.IsConcurrent(ref rpc);
        }

        internal void InputSessionFaulted(ServiceChannel channel)
        {
            if (this.inputSessionShutdownHandlers.Length > 0)
            {
                this.InputSessionFaultedCore(channel);
            }
        }

        void InputSessionFaultedCore(ServiceChannel channel)
        {
            IDuplexContextChannel proxy = channel.Proxy as IDuplexContextChannel;

            if (proxy != null)
            {
                IInputSessionShutdown[] handlers = this.inputSessionShutdownHandlers;
                try
                {
                    for (int i = 0; i < handlers.Length; i++)
                    {
                        handlers[i].ChannelFaulted(proxy);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (!this.error.HandleError(e))
                    {
                        proxy.Abort();
                    }
                }
            }
        }

        static internal void GotDynamicInstanceContext(object state)
        {
            bool alreadyResumedNoLock;
            ((IResumeMessageRpc)state).Resume(out alreadyResumedNoLock);

            if (alreadyResumedNoLock)
            {
                Fx.Assert("GotDynamicInstanceContext more than once for same call.");
            }
        }

        void AddMessageProperties(Message message, OperationContext context, ServiceChannel replyChannel)
        {
            if (context.InternalServiceChannel == replyChannel)
            {
                if (context.HasOutgoingMessageHeaders)
                {
                    message.Headers.CopyHeadersFrom(context.OutgoingMessageHeaders);
                }

                if (context.HasOutgoingMessageProperties)
                {
                    message.Properties.MergeProperties(context.OutgoingMessageProperties);
                }
            }
        }

        static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IResumeMessageRpc resume = result.AsyncState as IResumeMessageRpc;

            if (resume == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxInvalidAsyncResultState0));
            }

            resume.Resume(result);
        }

        static void OnReplyCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IResumeMessageRpc resume = result.AsyncState as IResumeMessageRpc;

            if (resume == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxInvalidAsyncResultState0));
            }

            resume.Resume(result);
        }

        void PrepareReply(ref MessageRpc rpc)
        {
            RequestContext context = rpc.OperationContext.RequestContext;
            Exception exception = null;
            bool thereIsAnUnhandledException = false;

            if (!rpc.Operation.IsOneWay)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    // If a service both returns null and sets RequestContext null, that
                    // means they handled it (either by calling Close or Reply manually).
                    // These traces catch accidents, where you accidentally return null,
                    // or you accidentally close the context so we can't return your message.
                    if ((rpc.Reply == null) && (context != null))
                    {
                        TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                            TraceCode.ServiceOperationMissingReply,
                            SR.GetString(SR.TraceCodeServiceOperationMissingReply, rpc.Operation.Name ?? String.Empty),
                            null, null);
                    }
                    else if ((context == null) && (rpc.Reply != null))
                    {
                        TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                            TraceCode.ServiceOperationMissingReplyContext,
                            SR.GetString(SR.TraceCodeServiceOperationMissingReplyContext, rpc.Operation.Name ?? String.Empty),
                            null, null);
                    }
                }

                if ((context != null) && (rpc.Reply != null))
                {
                    try
                    {
                        rpc.CanSendReply = PrepareAndAddressReply(ref rpc);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        thereIsAnUnhandledException = (!this.error.HandleError(e)) || thereIsAnUnhandledException;
                        exception = e;
                    }
                }
            }

            this.BeforeSendReply(ref rpc, ref exception, ref thereIsAnUnhandledException);

            if (rpc.Operation.IsOneWay)
            {
                rpc.CanSendReply = false;
            }

            if (!rpc.Operation.IsOneWay && (context != null) && (rpc.Reply != null))
            {
                if (exception != null)
                {
                    // We don't call ProvideFault again, since we have already passed the
                    // point where SFx addresses the reply, and it is reasonable for
                    // ProvideFault to expect that SFx will address the reply.  Instead
                    // we always just do 'internal server error' processing.
                    rpc.Error = exception;
                    this.error.ProvideOnlyFaultOfLastResort(ref rpc);

                    try
                    {
                        rpc.CanSendReply = PrepareAndAddressReply(ref rpc);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }
            }
            else if ((exception != null) && thereIsAnUnhandledException)
            {
                rpc.Abort();
            }
        }

        bool PrepareAndAddressReply(ref MessageRpc rpc)
        {
            bool canSendReply = true;

            if (!this.manualAddressing)
            {
                if (!object.ReferenceEquals(rpc.RequestID, null))
                {
                    System.ServiceModel.Channels.RequestReplyCorrelator.PrepareReply(rpc.Reply, rpc.RequestID);
                }

                if (!rpc.Channel.HasSession)
                {
                    canSendReply = System.ServiceModel.Channels.RequestReplyCorrelator.AddressReply(rpc.Reply, rpc.ReplyToInfo);
                }
            }

            AddMessageProperties(rpc.Reply, rpc.OperationContext, rpc.Channel);
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled && rpc.EventTraceActivity != null)
            {
                rpc.Reply.Properties[EventTraceActivity.Name] = rpc.EventTraceActivity;
            }

            return canSendReply;
        }

        internal DispatchOperationRuntime GetOperation(ref Message message)
        {
            return this.demuxer.GetOperation(ref message);
        }

        internal void ProcessMessage1(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage11;

            if (this.receiveContextEnabledChannel)
            {
                ReceiveContextRPCFacet.CreateIfRequired(this, ref rpc);
            }

            if (!rpc.IsPaused)
            {
                this.ProcessMessage11(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage1)
            {
                this.didTraceProcessMessage1 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage31Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }
        }

        internal void ProcessMessage11(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage2;

            if (rpc.Operation.IsOneWay)
            {
                rpc.RequestContext.Reply(null);
                rpc.OperationContext.RequestContext = null;
            }
            else
            {
                if (!rpc.Channel.IsReplyChannel &&
                    ((object)rpc.RequestID == null) &&
                    (rpc.Operation.Action != MessageHeaders.WildcardAction))
                {
                    CommunicationException error = new CommunicationException(SR.GetString(SR.SFxOneWayMessageToTwoWayMethod0));
                    throw TraceUtility.ThrowHelperError(error, rpc.Request);
                }

                if (!this.manualAddressing)
                {
                    EndpointAddress replyTo = rpc.ReplyToInfo.ReplyTo;
                    if (replyTo != null && replyTo.IsNone && rpc.Channel.IsReplyChannel)
                    {
                        CommunicationException error = new CommunicationException(SR.GetString(SR.SFxRequestReplyNone));
                        throw TraceUtility.ThrowHelperError(error, rpc.Request);
                    }

                    if (this.isOnServer)
                    {
                        EndpointAddress remoteAddress = rpc.Channel.RemoteAddress;
                        if ((remoteAddress != null) && !remoteAddress.IsAnonymous)
                        {
                            MessageHeaders headers = rpc.Request.Headers;
                            Uri remoteUri = remoteAddress.Uri;

                            if ((replyTo != null) && !replyTo.IsAnonymous && (remoteUri != replyTo.Uri))
                            {
                                string text = SR.GetString(SR.SFxRequestHasInvalidReplyToOnServer, replyTo.Uri, remoteUri);
                                Exception error = new InvalidOperationException(text);
                                throw TraceUtility.ThrowHelperError(error, rpc.Request);
                            }

                            EndpointAddress faultTo = headers.FaultTo;
                            if ((faultTo != null) && !faultTo.IsAnonymous && (remoteUri != faultTo.Uri))
                            {
                                string text = SR.GetString(SR.SFxRequestHasInvalidFaultToOnServer, faultTo.Uri, remoteUri);
                                Exception error = new InvalidOperationException(text);
                                throw TraceUtility.ThrowHelperError(error, rpc.Request);
                            }

                            if (rpc.RequestVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                            {
                                EndpointAddress from = headers.From;
                                if ((from != null) && !from.IsAnonymous && (remoteUri != from.Uri))
                                {
                                    string text = SR.GetString(SR.SFxRequestHasInvalidFromOnServer, from.Uri, remoteUri);
                                    Exception error = new InvalidOperationException(text);
                                    throw TraceUtility.ThrowHelperError(error, rpc.Request);
                                }
                            }
                        }
                    }
                }
            }

            if (this.concurrency.IsConcurrent(ref rpc))
            {
                rpc.Channel.IncrementActivity();
                rpc.SuccessfullyIncrementedActivity = true;
            }

            if (this.authenticationBehavior != null)
            {
                this.authenticationBehavior.Authenticate(ref rpc);
            }

            if (this.authorizationBehavior != null)
            {
                this.authorizationBehavior.Authorize(ref rpc);
            }

            this.instance.EnsureInstanceContext(ref rpc);
            this.TransferChannelFromPendingList(ref rpc);

            this.AcquireDynamicInstanceContext(ref rpc);

            if (!rpc.IsPaused)
            {
                this.ProcessMessage2(ref rpc);
            }
        }

        void ProcessMessage2(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage3;

            this.AfterReceiveRequest(ref rpc);

            if (!this.ignoreTransactionFlow)
            {
                // Transactions need to have the context in the message
                rpc.TransactionMessageProperty = TransactionMessageProperty.TryGet(rpc.Request);
            }

            this.concurrency.LockInstance(ref rpc);

            if (!rpc.IsPaused)
            {
                this.ProcessMessage3(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage2)
            {
                this.didTraceProcessMessage2 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage2Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }
        }

        void ProcessMessage3(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage31;

            rpc.SuccessfullyLockedInstance = true;

            // This also needs to happen after LockInstance, in case
            // we are using an AutoComplete=false transaction.
            if (this.transaction != null)
            {
                this.transaction.ResolveTransaction(ref rpc);
                if (rpc.Operation.TransactionRequired)
                {
                    this.transaction.SetCurrent(ref rpc);
                }
            }

            if (!rpc.IsPaused)
            {
                this.ProcessMessage31(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage3)
            {
                this.didTraceProcessMessage3 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage3Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }
        }

        void ProcessMessage31(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage4;

            if (this.transaction != null)
            {
                if (rpc.Operation.TransactionRequired)
                {
                    ReceiveContextRPCFacet receiveContext = rpc.ReceiveContext;

                    if (receiveContext != null)
                    {
                        rpc.ReceiveContext = null;
                        receiveContext.Complete(this, ref rpc, TimeSpan.MaxValue, rpc.Transaction.Current);
                    }
                }
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage4(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage31)
            {
                this.didTraceProcessMessage31 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage31Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }
        }

        void ProcessMessage4(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage41;

            try
            {
                this.thread.BindThread(ref rpc);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(e.Message, e);
            }

            if (!rpc.IsPaused)
            {
                this.ProcessMessage41(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage4)
            {
                this.didTraceProcessMessage4 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage4Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }

        }

        void ProcessMessage41(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage5;

            // This needs to happen after LockInstance--LockInstance guarantees
            // in-order delivery, so we can't receive the next message until we
            // have acquired the lock.
            //
            // This also needs to happen after BindThread, since EricZ believes
            // that running on UI thread should guarantee in-order delivery if
            // the SynchronizationContext is single threaded.
            // Note: for IManualConcurrencyOperationInvoker, the invoke assumes full control over pumping.
            if (this.concurrency.IsConcurrent(ref rpc) && !(rpc.Operation.Invoker is IManualConcurrencyOperationInvoker))
            {
                rpc.EnsureReceive();
            }

            this.instance.EnsureServiceInstance(ref rpc);

            if (!rpc.IsPaused)
            {
                this.ProcessMessage5(ref rpc);
            }
            else if (this.isOnServer && DiagnosticUtility.ShouldTraceInformation && !this.didTraceProcessMessage41)
            {
                this.didTraceProcessMessage41 = true;

                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.MessageProcessingPaused,
                    SR.GetString(SR.TraceCodeProcessMessage4Paused,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName,
                    rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress));
            }
        }

        void ProcessMessage5(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage6;

            try
            {
                bool success = false;
                try
                {
                    if (!rpc.Operation.IsSynchronous)
                    {
                        // If async call completes in sync, it tells us through the gate below
                        rpc.PrepareInvokeContinueGate();
                    }

                    if (this.transaction != null)
                    {
                        this.transaction.InitializeCallContext(ref rpc);
                    }

                    SetActivityIdOnThread(ref rpc);

                    rpc.Operation.InvokeBegin(ref rpc);
                    success = true;
                }
                finally
                {
                    try
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.ClearCallContext(ref rpc);
                            }
                        }
                        finally
                        {
                            if (!rpc.Operation.IsSynchronous && rpc.IsPaused)
                            {
                                // Check if the callback produced the async result and set it back on the RPC on this stack 
                                // and proceed only if the gate was signaled by the callback and completed synchronously
                                if (rpc.UnlockInvokeContinueGate(out rpc.AsyncResult))
                                {
                                    rpc.UnPause();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (success && (rpc.Operation.IsSynchronous || !rpc.IsPaused))
                        {
                            throw;
                        }

                        this.error.HandleError(e);
                    }
                }
            }
            catch
            {
                // This catch clause forces ClearCallContext to run prior to stackwalks exiting this frame.
                throw;
            }

            // Proceed if rpc is unpaused and invoke begin was successful.
            if (!rpc.IsPaused)
            {
                this.ProcessMessage6(ref rpc);
            }
        }

        void ProcessMessage6(ref MessageRpc rpc)
        {
            rpc.NextProcessor = (rpc.Operation.IsSynchronous) ?
                this.processMessage8 :
                this.processMessage7;

            try
            {
                this.thread.BindEndThread(ref rpc);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(e.Message, e);
            }

            if (!rpc.IsPaused)
            {
                if (rpc.Operation.IsSynchronous)
                {
                    this.ProcessMessage8(ref rpc);
                }
                else
                {
                    this.ProcessMessage7(ref rpc);
                }
            }
        }

        void ProcessMessage7(ref MessageRpc rpc)
        {
            rpc.NextProcessor = null;

            try
            {
                bool success = false;
                try
                {
                    if (this.transaction != null)
                    {
                        this.transaction.InitializeCallContext(ref rpc);
                    }
                    rpc.Operation.InvokeEnd(ref rpc);
                    success = true;
                }
                finally
                {
                    try
                    {
                        if (this.transaction != null)
                        {
                            this.transaction.ClearCallContext(ref rpc);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        if (success)
                        {
                            // Throw the transaction.ClearContextException only if
                            // there isn't an exception on the thread already.
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }
            }
            catch
            {
                // Make sure user Exception filters are not run with bad call context
                throw;
            }

            // this never pauses
            this.ProcessMessage8(ref rpc);
        }

        void ProcessMessage8(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage9;

            try
            {
                this.error.ProvideMessageFault(ref rpc);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.error.HandleError(e);
            }

            this.PrepareReply(ref rpc);

            if (rpc.CanSendReply)
            {
                rpc.ReplyTimeoutHelper = new TimeoutHelper(rpc.Channel.OperationTimeout);

                if (this.sendAsynchronously)
                {
                    this.BeginFinalizeCorrelation(ref rpc);
                }
                else
                {
                    this.FinalizeCorrelation(ref rpc);
                }
            }

            if (!rpc.IsPaused)
            {
                this.ProcessMessage9(ref rpc);
            }
        }

        void ProcessMessage9(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessageCleanup;

            if (rpc.FinalizeCorrelationImplicitly && this.sendAsynchronously)
            {
                this.EndFinalizeCorrelation(ref rpc);
            }

            if (rpc.CorrelationCallback == null || rpc.FinalizeCorrelationImplicitly)
            {
                this.ResolveTransactionOutcome(ref rpc);
            }

            if (rpc.CanSendReply)
            {
                if (rpc.Reply != null)
                {
                    TraceUtility.MessageFlowAtMessageSent(rpc.Reply, rpc.EventTraceActivity);
                }

                if (this.sendAsynchronously)
                {
                    this.BeginReply(ref rpc);
                }
                else
                {
                    this.Reply(ref rpc);
                }
            }

            if (!rpc.IsPaused)
            {
                this.ProcessMessageCleanup(ref rpc);
            }
        }

        // Logic for knowing when to close stuff:
        //
        // ASSUMPTIONS:
        //   Closing a stream over a message also closes the message.
        //   Closing a message over a stream does not close the stream.
        //     (OperationStreamProvider.ReleaseStream is no-op)
        //
        // This is a table of what should be disposed in what cases.
        // The rows represent the type of parameter to the method and
        // whether we are disposing parameters or not.  The columns
        // are for the inputs vs. the outputs.  The cells contain the
        // values that need to be Disposed.  M^P means that exactly
        // one of the message and parameter needs to be disposed,
        // since they refer to the same object.
        //
        //                               Request           Reply
        //               Message   |     M or P      |     M or P
        //     Dispose   Stream    |     P           |     M and P
        //               Params    |     M and P     |     M and P
        //                         |                 |
        //               Message   |     none        |     none
        //   NoDispose   Stream    |     none        |     M
        //               Params    |     M           |     M
        //
        // By choosing to dispose the parameter in both of the "M or P"
        // cases, the logic needed to generate this table is:
        //
        // CloseRequestMessage = IsParams
        // CloseRequestParams  = rpc.Operation.DisposeParameters
        // CloseReplyMessage   = rpc.Operation.SerializeReply
        // CloseReplyParams    = rpc.Operation.DisposeParameters
        //
        // IsParams can be calculated based on whether the request
        // message was consumed after deserializing but before calling
        // the user.  This is stored as rpc.DidDeserializeRequestBody.
        //
        void ProcessMessageCleanup(ref MessageRpc rpc)
        {
            Fx.Assert(
                !object.ReferenceEquals(rpc.ErrorProcessor, this.processMessageCleanupError),
                "ProcessMessageCleanup run twice on the same MessageRpc!");
            rpc.ErrorProcessor = this.processMessageCleanupError;

            bool replyWasSent = false;

            if (rpc.CanSendReply)
            {
                if (this.sendAsynchronously)
                {
                    replyWasSent = this.EndReply(ref rpc);
                }
                else
                {
                    replyWasSent = rpc.SuccessfullySendReply;
                }
            }

            try
            {
                try
                {
                    if (rpc.DidDeserializeRequestBody)
                    {
                        rpc.Request.Close();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.error.HandleError(e);
                }

                if (rpc.HostingProperty != null)
                {
                    try
                    {
                        rpc.HostingProperty.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(e.Message, e);
                    }
                }

                // for wf, wf owns the lifetime of the request message. So in that case, we should not dispose the inputs
                IManualConcurrencyOperationInvoker manualInvoker = rpc.Operation.Invoker as IManualConcurrencyOperationInvoker;
                rpc.DisposeParameters(manualInvoker != null && manualInvoker.OwnsFormatter); //Dispose all input/output/return parameters

                if (rpc.FaultInfo.IsConsideredUnhandled)
                {
                    if (!replyWasSent)
                    {
                        rpc.AbortRequestContext();
                        rpc.AbortChannel();
                    }
                    else
                    {
                        rpc.CloseRequestContext();
                        rpc.CloseChannel();
                    }
                    rpc.AbortInstanceContext();
                }
                else
                {
                    if (rpc.RequestContextThrewOnReply)
                    {
                        rpc.AbortRequestContext();
                    }
                    else
                    {
                        rpc.CloseRequestContext();
                    }
                }


                if ((rpc.Reply != null) && (rpc.Reply != rpc.ReturnParameter))
                {
                    try
                    {
                        rpc.Reply.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }

                if ((rpc.FaultInfo.Fault != null) && (rpc.FaultInfo.Fault.State != MessageState.Closed))
                {
                    // maybe ProvideFault gave a Message, but then BeforeSendReply replaced it
                    // in that case, we need to close the one from ProvideFault
                    try
                    {
                        rpc.FaultInfo.Fault.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }

                try
                {
                    rpc.OperationContext.FireOperationCompleted();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                }

                this.instance.AfterReply(ref rpc, this.error);

                if (rpc.SuccessfullyLockedInstance)
                {
                    try
                    {
                        this.concurrency.UnlockInstance(ref rpc);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        Fx.Assert("Exceptions should be caught by callee");
                        rpc.InstanceContext.FaultInternal();
                        this.error.HandleError(e);
                    }
                }

                if (this.terminate != null)
                {
                    try
                    {
                        this.terminate.AfterReply(ref rpc);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }

                if (rpc.SuccessfullyIncrementedActivity)
                {
                    try
                    {
                        rpc.Channel.DecrementActivity();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.error.HandleError(e);
                    }
                }
            }
            finally
            {
                if (rpc.MessageRpcOwnsInstanceContextThrottle && rpc.channelHandler.InstanceContextServiceThrottle != null)
                {
                    rpc.channelHandler.InstanceContextServiceThrottle.DeactivateInstanceContext();
                }

                if (rpc.Activity != null && DiagnosticUtility.ShouldUseActivity)
                {
                    rpc.Activity.Stop();
                }
            }

            this.error.HandleError(ref rpc);
        }

        void ProcessMessageCleanupError(ref MessageRpc rpc)
        {
            this.error.HandleError(ref rpc);
        }

        void ResolveTransactionOutcome(ref MessageRpc rpc)
        {
            if (this.transaction != null)
            {
                try
                {
                    bool hadError = (rpc.Error != null);
                    try
                    {
                        this.transaction.ResolveOutcome(ref rpc);
                    }
                    catch (FaultException e)
                    {
                        if (rpc.Error == null)
                        {
                            rpc.Error = e;
                        }
                    }
                    finally
                    {
                        if (!hadError && rpc.Error != null)
                        {
                            this.error.ProvideMessageFault(ref rpc);
                            this.PrepareAndAddressReply(ref rpc);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.error.HandleError(e);
                }

            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls security critical method to set the ActivityId on the thread",
            Safe = "Set the ActivityId only when MessageRpc is available")]
        [SecuritySafeCritical]
        void SetActivityIdOnThread(ref MessageRpc rpc)
        {
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled && rpc.EventTraceActivity != null)
            {
                // Propogate the ActivityId to the service operation
                EventTraceActivityHelper.SetOnThread(rpc.EventTraceActivity);
            }
        }

        void TransferChannelFromPendingList(ref MessageRpc rpc)
        {
            if (rpc.Channel.IsPending)
            {
                rpc.Channel.IsPending = false;

                ChannelDispatcher channelDispatcher = rpc.Channel.ChannelDispatcher;
                IInstanceContextProvider provider = this.instance.InstanceContextProvider;

                if (!InstanceContextProviderBase.IsProviderSessionful(provider) &&
                    !InstanceContextProviderBase.IsProviderSingleton(provider))
                {
                    IChannel proxy = rpc.Channel.Proxy as IChannel;
                    if (!rpc.InstanceContext.IncomingChannels.Contains(proxy))
                    {
                        channelDispatcher.Channels.Add(proxy);
                    }
                }

                channelDispatcher.PendingChannels.Remove(rpc.Channel.Binder.Channel);
            }
        }

        interface IDemuxer
        {
            DispatchOperationRuntime GetOperation(ref Message request);
        }

        class ActionDemuxer : IDemuxer
        {
            HybridDictionary map;
            DispatchOperationRuntime unhandled;

            internal ActionDemuxer()
            {
                this.map = new HybridDictionary();
            }

            internal void Add(string action, DispatchOperationRuntime operation)
            {
                if (map.Contains(action))
                {
                    DispatchOperationRuntime existingOperation = (DispatchOperationRuntime)map[action];
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxActionDemuxerDuplicate, existingOperation.Name, operation.Name, action)));
                }
                this.map.Add(action, operation);
            }

            internal void SetUnhandled(DispatchOperationRuntime operation)
            {
                this.unhandled = operation;
            }

            public DispatchOperationRuntime GetOperation(ref Message request)
            {
                string action = request.Headers.Action;
                if (action == null)
                {
                    action = MessageHeaders.WildcardAction;
                }
                DispatchOperationRuntime operation = (DispatchOperationRuntime)this.map[action];
                if (operation != null)
                {
                    return operation;
                }

                return this.unhandled;
            }
        }

        class CustomDemuxer : IDemuxer
        {
            Dictionary<string, DispatchOperationRuntime> map;
            IDispatchOperationSelector selector;
            DispatchOperationRuntime unhandled;

            internal CustomDemuxer(IDispatchOperationSelector selector)
            {
                this.selector = selector;
                this.map = new Dictionary<string, DispatchOperationRuntime>();
            }

            internal void Add(string name, DispatchOperationRuntime operation)
            {
                this.map.Add(name, operation);
            }

            internal void SetUnhandled(DispatchOperationRuntime operation)
            {
                this.unhandled = operation;
            }

            public DispatchOperationRuntime GetOperation(ref Message request)
            {
                string operationName = this.selector.SelectOperation(ref request);
                DispatchOperationRuntime operation = null;
                if (this.map.TryGetValue(operationName, out operation))
                {
                    return operation;
                }
                else
                {
                    return this.unhandled;
                }
            }
        }

        class RpcCorrelationCallbackMessageProperty : CorrelationCallbackMessageProperty
        {
            CorrelationCallbackMessageProperty innerCallback;
            MessageRpc rpc;
            ImmutableDispatchRuntime runtime;
            TransactionScope scope;

            // This constructor should be used when creating the RPCCorrelationMessageproperty the first time
            // Here we copy the data & the needed data from the original callback
            public RpcCorrelationCallbackMessageProperty(CorrelationCallbackMessageProperty innerCallback,
                ImmutableDispatchRuntime runtime, ref MessageRpc rpc)
                : base(innerCallback)
            {
                this.innerCallback = innerCallback;
                this.runtime = runtime;
                this.rpc = rpc;
            }

            // This constructor should be used when we are making a copy from the already initialized RPCCorrelationCallbackMessageProperty
            public RpcCorrelationCallbackMessageProperty(RpcCorrelationCallbackMessageProperty rpcCallbackMessageProperty)
                : base(rpcCallbackMessageProperty)
            {
                this.innerCallback = rpcCallbackMessageProperty.innerCallback;
                this.runtime = rpcCallbackMessageProperty.runtime;
                this.rpc = rpcCallbackMessageProperty.rpc;
            }

            public override IMessageProperty CreateCopy()
            {
                return new RpcCorrelationCallbackMessageProperty(this);
            }

            protected override IAsyncResult OnBeginFinalizeCorrelation(Message message, TimeSpan timeout,
                AsyncCallback callback, object state)
            {
                bool success = false;

                this.Enter();

                try
                {
                    IAsyncResult result = this.innerCallback.BeginFinalizeCorrelation(message, timeout, callback, state);
                    success = true;
                    return result;
                }
                finally
                {
                    this.Leave(success);
                }
            }

            protected override Message OnEndFinalizeCorrelation(IAsyncResult result)
            {
                bool success = false;

                this.Enter();

                try
                {
                    Message message = this.innerCallback.EndFinalizeCorrelation(result);
                    success = true;
                    return message;
                }
                finally
                {
                    this.Leave(success);
                    this.CompleteTransaction();
                }
            }

            protected override Message OnFinalizeCorrelation(Message message, TimeSpan timeout)
            {
                bool success = false;

                this.Enter();

                try
                {
                    Message newMessage = this.innerCallback.FinalizeCorrelation(message, timeout);
                    success = true;
                    return newMessage;
                }
                finally
                {
                    this.Leave(success);
                    this.CompleteTransaction();
                }
            }

            void CompleteTransaction()
            {
                this.runtime.ResolveTransactionOutcome(ref this.rpc);
            }

            void Enter()
            {
                if (this.rpc.transaction != null && this.rpc.transaction.Current != null)
                {
                    this.scope = new TransactionScope(this.rpc.transaction.Current);
                }
            }

            void Leave(bool complete)
            {
                if (this.scope != null)
                {
                    if (complete)
                    {
                        scope.Complete();
                    }

                    scope.Dispose();
                    this.scope = null;
                }
            }
        }
    }
}
