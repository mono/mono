//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Transactions;
    using System.Xml;
    using SessionIdleManager = System.ServiceModel.Channels.ServiceChannel.SessionIdleManager;

    class ChannelHandler
    {
        public static readonly TimeSpan CloseAfterFaultTimeout = TimeSpan.FromSeconds(10);
        public const string MessageBufferPropertyName = "_RequestMessageBuffer_";

        readonly IChannelBinder binder;
        readonly DuplexChannelBinder duplexBinder;
        readonly ServiceHostBase host;
        readonly bool incrementedActivityCountInConstructor;
        readonly bool isCallback;
        readonly ListenerHandler listener;
        readonly ServiceThrottle throttle;
        readonly bool wasChannelThrottled;
        readonly SessionIdleManager idleManager;
        readonly bool sendAsynchronously;

        static AsyncCallback onAsyncReplyComplete = Fx.ThunkCallback(new AsyncCallback(ChannelHandler.OnAsyncReplyComplete));
        static AsyncCallback onAsyncReceiveComplete = Fx.ThunkCallback(new AsyncCallback(ChannelHandler.OnAsyncReceiveComplete));
        static Action<object> onContinueAsyncReceive = new Action<object>(ChannelHandler.OnContinueAsyncReceive);
        static Action<object> onStartSyncMessagePump = new Action<object>(ChannelHandler.OnStartSyncMessagePump);
        static Action<object> onStartAsyncMessagePump = new Action<object>(ChannelHandler.OnStartAsyncMessagePump);
        static Action<object> onStartSingleTransactedBatch = new Action<object>(ChannelHandler.OnStartSingleTransactedBatch);
        static Action<object> openAndEnsurePump = new Action<object>(ChannelHandler.OpenAndEnsurePump);

        RequestInfo requestInfo;
        ServiceChannel channel;
        bool doneReceiving;
        bool hasRegisterBeenCalled;
        bool hasSession;
        int isPumpAcquired;
        bool isChannelTerminated;
        bool isConcurrent;
        bool isManualAddressing;
        MessageVersion messageVersion;
        ErrorHandlingReceiver receiver;
        bool receiveSynchronously;
        bool receiveWithTransaction;
        RequestContext replied;
        RequestContext requestWaitingForThrottle;
        WrappedTransaction acceptTransaction;
        ServiceThrottle instanceContextThrottle;
        SharedTransactedBatchContext sharedTransactedBatchContext;
        TransactedBatchContext transactedBatchContext;
        bool isMainTransactedBatchHandler;
        EventTraceActivity eventTraceActivity;
        SessionOpenNotification sessionOpenNotification;
        bool needToCreateSessionOpenNotificationMessage;
        bool shouldRejectMessageWithOnOpenActionHeader;

        internal ChannelHandler(MessageVersion messageVersion, IChannelBinder binder, ServiceChannel channel)
        {
            ClientRuntime clientRuntime = channel.ClientRuntime;

            this.messageVersion = messageVersion;
            this.isManualAddressing = clientRuntime.ManualAddressing;
            this.binder = binder;
            this.channel = channel;

            this.isConcurrent = true;
            this.duplexBinder = binder as DuplexChannelBinder;
            this.hasSession = binder.HasSession;
            this.isCallback = true;

            DispatchRuntime dispatchRuntime = clientRuntime.DispatchRuntime;
            if (dispatchRuntime == null)
            {
                this.receiver = new ErrorHandlingReceiver(binder, null);
            }
            else
            {
                this.receiver = new ErrorHandlingReceiver(binder, dispatchRuntime.ChannelDispatcher);
            }
            this.requestInfo = new RequestInfo(this);

        }

        internal ChannelHandler(MessageVersion messageVersion, IChannelBinder binder, ServiceThrottle throttle,
            ListenerHandler listener, bool wasChannelThrottled, WrappedTransaction acceptTransaction, SessionIdleManager idleManager)
        {
            ChannelDispatcher channelDispatcher = listener.ChannelDispatcher;

            this.messageVersion = messageVersion;
            this.isManualAddressing = channelDispatcher.ManualAddressing;
            this.binder = binder;
            this.throttle = throttle;
            this.listener = listener;
            this.wasChannelThrottled = wasChannelThrottled;

            this.host = listener.Host;
            this.receiveSynchronously = channelDispatcher.ReceiveSynchronously;
            this.sendAsynchronously = channelDispatcher.SendAsynchronously;
            this.duplexBinder = binder as DuplexChannelBinder;
            this.hasSession = binder.HasSession;
            this.isConcurrent = ConcurrencyBehavior.IsConcurrent(channelDispatcher, this.hasSession);

            if (channelDispatcher.MaxPendingReceives > 1)
            {
                // We need to preserve order if the ChannelHandler is not concurrent.
                this.binder = new MultipleReceiveBinder(
                    this.binder,
                    channelDispatcher.MaxPendingReceives,
                    !this.isConcurrent);
            }

            if (channelDispatcher.BufferedReceiveEnabled)
            {
                this.binder = new BufferedReceiveBinder(this.binder);
            }

            this.receiver = new ErrorHandlingReceiver(this.binder, channelDispatcher);
            this.idleManager = idleManager;
            Fx.Assert((this.idleManager != null) == (this.binder.HasSession && this.listener.ChannelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout != TimeSpan.MaxValue), "idle manager is present only when there is a session with a finite receive timeout");

            if (channelDispatcher.IsTransactedReceive && !channelDispatcher.ReceiveContextEnabled)
            {
                receiveSynchronously = true;
                receiveWithTransaction = true;

                if (channelDispatcher.MaxTransactedBatchSize > 0)
                {
                    int maxConcurrentBatches = 1;
                    if (null != throttle && throttle.MaxConcurrentCalls > 1)
                    {
                        maxConcurrentBatches = throttle.MaxConcurrentCalls;
                        foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                        {
                            if (ConcurrencyMode.Multiple != endpointDispatcher.DispatchRuntime.ConcurrencyMode)
                            {
                                maxConcurrentBatches = 1;
                                break;
                            }
                        }
                    }

                    this.sharedTransactedBatchContext = new SharedTransactedBatchContext(this, channelDispatcher, maxConcurrentBatches);
                    this.isMainTransactedBatchHandler = true;
                    this.throttle = null;
                }
            }
            else if (channelDispatcher.IsTransactedReceive && channelDispatcher.ReceiveContextEnabled && channelDispatcher.MaxTransactedBatchSize > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IncompatibleBehaviors)));
            }

            if (this.binder.HasSession)
            {
                this.sessionOpenNotification = this.binder.Channel.GetProperty<SessionOpenNotification>();
                this.needToCreateSessionOpenNotificationMessage = this.sessionOpenNotification != null && this.sessionOpenNotification.IsEnabled;
            }

            this.acceptTransaction = acceptTransaction;
            this.requestInfo = new RequestInfo(this);

            if (this.listener.State == CommunicationState.Opened)
            {
                this.listener.ChannelDispatcher.Channels.IncrementActivityCount();
                this.incrementedActivityCountInConstructor = true;
            }
        }


        internal ChannelHandler(ChannelHandler handler, TransactedBatchContext context)
        {
            this.messageVersion = handler.messageVersion;
            this.isManualAddressing = handler.isManualAddressing;
            this.binder = handler.binder;
            this.listener = handler.listener;
            this.wasChannelThrottled = handler.wasChannelThrottled;

            this.host = handler.host;
            this.receiveSynchronously = true;
            this.receiveWithTransaction = true;
            this.duplexBinder = handler.duplexBinder;
            this.hasSession = handler.hasSession;
            this.isConcurrent = handler.isConcurrent;
            this.receiver = handler.receiver;

            this.sharedTransactedBatchContext = context.Shared;
            this.transactedBatchContext = context;
            this.requestInfo = new RequestInfo(this);

            this.sendAsynchronously = handler.sendAsynchronously;
            this.sessionOpenNotification = handler.sessionOpenNotification;
            this.needToCreateSessionOpenNotificationMessage = handler.needToCreateSessionOpenNotificationMessage;
            this.shouldRejectMessageWithOnOpenActionHeader = handler.shouldRejectMessageWithOnOpenActionHeader;
        }

        internal IChannelBinder Binder
        {
            get { return this.binder; }
        }

        internal ServiceChannel Channel
        {
            get { return this.channel; }
        }

        internal bool HasRegisterBeenCalled
        {
            get { return this.hasRegisterBeenCalled; }
        }

        internal InstanceContext InstanceContext
        {
            get { return (this.channel != null) ? this.channel.InstanceContext : null; }
        }

        internal ServiceThrottle InstanceContextServiceThrottle
        {
            get
            {
                return this.instanceContextThrottle;
            }
            set
            {
                this.instanceContextThrottle = value;
            }
        }

        bool IsOpen
        {
            get { return this.binder.Channel.State == CommunicationState.Opened; }
        }

        EndpointAddress LocalAddress
        {
            get
            {
                if (this.binder != null)
                {
                    IInputChannel input = this.binder.Channel as IInputChannel;
                    if (input != null)
                    {
                        return input.LocalAddress;
                    }

                    IReplyChannel reply = this.binder.Channel as IReplyChannel;
                    if (reply != null)
                    {
                        return reply.LocalAddress;
                    }
                }

                return null;
            }
        }

        object ThisLock
        {
            get { return this; }
        }

        EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = new EventTraceActivity();
                }
                return this.eventTraceActivity;
            }
        }

        internal static void Register(ChannelHandler handler)
        {
            handler.Register();
        }

        internal static void Register(ChannelHandler handler, RequestContext request)
        {
            BufferedReceiveBinder bufferedBinder = handler.Binder as BufferedReceiveBinder;
            Fx.Assert(bufferedBinder != null, "ChannelHandler.Binder is not a BufferedReceiveBinder");

            bufferedBinder.InjectRequest(request);
            handler.Register();
        }

        void Register()
        {
            this.hasRegisterBeenCalled = true;
            if (this.binder.Channel.State == CommunicationState.Created)
            {
                ActionItem.Schedule(openAndEnsurePump, this);
            }
            else
            {
                this.EnsurePump();
            }
        }

        void AsyncMessagePump()
        {
            IAsyncResult result = this.BeginTryReceive();

            if ((result != null) && result.CompletedSynchronously)
            {
                this.AsyncMessagePump(result);
            }
        }

        void AsyncMessagePump(IAsyncResult result)
        {
            if (TD.ChannelReceiveStopIsEnabled())
            {
                TD.ChannelReceiveStop(this.EventTraceActivity, this.GetHashCode());
            }

            for (;;)
            {
                RequestContext request;

                while (!this.EndTryReceive(result, out request))
                {
                    result = this.BeginTryReceive();

                    if ((result == null) || !result.CompletedSynchronously)
                    {
                        return;
                    }
                }

                if (!HandleRequest(request, null))
                {
                    break;
                }

                if (!TryAcquirePump())
                {
                    break;
                }

                result = this.BeginTryReceive();

                if (result == null || !result.CompletedSynchronously)
                {
                    break;
                }
            }
        }

        IAsyncResult BeginTryReceive()
        {
            this.requestInfo.Cleanup();

            if (TD.ChannelReceiveStartIsEnabled())
            {
                TD.ChannelReceiveStart(this.EventTraceActivity, this.GetHashCode());
            }

            this.shouldRejectMessageWithOnOpenActionHeader = !this.needToCreateSessionOpenNotificationMessage;
            if (this.needToCreateSessionOpenNotificationMessage)
            {
                return new CompletedAsyncResult(ChannelHandler.onAsyncReceiveComplete, this);
            }

            return this.receiver.BeginTryReceive(TimeSpan.MaxValue, ChannelHandler.onAsyncReceiveComplete, this);
        }

        bool DispatchAndReleasePump(RequestContext request, bool cleanThread, OperationContext currentOperationContext)
        {
            ServiceChannel channel = this.requestInfo.Channel;
            EndpointDispatcher endpoint = this.requestInfo.Endpoint;
            bool releasedPump = false;

            try
            {
                DispatchRuntime dispatchBehavior = this.requestInfo.DispatchRuntime;

                if (channel == null || dispatchBehavior == null)
                {
                    Fx.Assert("System.ServiceModel.Dispatcher.ChannelHandler.Dispatch(): (channel == null || dispatchBehavior == null)");
                    return true;
                }

                MessageBuffer buffer = null;
                Message message;

                EventTraceActivity eventTraceActivity = TraceDispatchMessageStart(request.RequestMessage);
                AspNetEnvironment.Current.PrepareMessageForDispatch(request.RequestMessage);
                if (dispatchBehavior.PreserveMessage)
                {
                    object previousBuffer = null;
                    if (request.RequestMessage.Properties.TryGetValue(MessageBufferPropertyName, out previousBuffer))
                    {
                        buffer = (MessageBuffer)previousBuffer;
                        message = buffer.CreateMessage();
                    }
                    else
                    {
                        // 
                        buffer = request.RequestMessage.CreateBufferedCopy(int.MaxValue);
                        message = buffer.CreateMessage();
                    }
                }
                else
                {
                    message = request.RequestMessage;
                }

                DispatchOperationRuntime operation = dispatchBehavior.GetOperation(ref message);
                if (operation == null)
                {
                    Fx.Assert("ChannelHandler.Dispatch (operation == null)");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "No DispatchOperationRuntime found to process message.")));
                }

                if (this.shouldRejectMessageWithOnOpenActionHeader && message.Headers.Action == OperationDescription.SessionOpenedAction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoEndpointMatchingAddressForConnectionOpeningMessage, message.Headers.Action, "Open")));
                }

                if (MessageLogger.LoggingEnabled)
                {
                    MessageLogger.LogMessage(ref message, (operation.IsOneWay ? MessageLoggingSource.ServiceLevelReceiveDatagram : MessageLoggingSource.ServiceLevelReceiveRequest) | MessageLoggingSource.LastChance);
                }

                if (operation.IsTerminating && this.hasSession)
                {
                    this.isChannelTerminated = true;
                }

                bool hasOperationContextBeenSet;
                if (currentOperationContext != null)
                {
                    hasOperationContextBeenSet = true;
                    currentOperationContext.ReInit(request, message, channel);
                }
                else
                {
                    hasOperationContextBeenSet = false;
                    currentOperationContext = new OperationContext(request, message, channel, this.host);
                }

                if (dispatchBehavior.PreserveMessage)
                {
                    currentOperationContext.IncomingMessageProperties.Add(MessageBufferPropertyName, buffer);
                }

                if (currentOperationContext.EndpointDispatcher == null && this.listener != null)
                {
                    currentOperationContext.EndpointDispatcher = endpoint;
                }

                MessageRpc rpc = new MessageRpc(request, message, operation, channel, this.host,
                    this, cleanThread, currentOperationContext, this.requestInfo.ExistingInstanceContext, eventTraceActivity);

                TraceUtility.MessageFlowAtMessageReceived(message, currentOperationContext, eventTraceActivity, true);

                rpc.TransactedBatchContext = this.transactedBatchContext;

                // passing responsibility for call throttle to MessageRpc
                // (MessageRpc implicitly owns this throttle once it's created)
                this.requestInfo.ChannelHandlerOwnsCallThrottle = false;
                // explicitly passing responsibility for instance throttle to MessageRpc
                rpc.MessageRpcOwnsInstanceContextThrottle = this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle;
                this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = false;

                // These need to happen before Dispatch but after accessing any ChannelHandler
                // state, because we go multi-threaded after this until we reacquire pump mutex.
                this.ReleasePump();
                releasedPump = true;

                return operation.Parent.Dispatch(ref rpc, hasOperationContextBeenSet);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                return this.HandleError(e, request, channel);
            }
            finally
            {
                if (!releasedPump)
                {
                    this.ReleasePump();
                }
            }
        }

        internal void DispatchDone()
        {
            if (this.throttle != null)
            {
                this.throttle.DeactivateCall();
            }
        }

        RequestContext GetSessionOpenNotificationRequestContext()
        {
            Fx.Assert(this.sessionOpenNotification != null, "this.sessionOpenNotification should not be null.");
            Message message = Message.CreateMessage(this.Binder.Channel.GetProperty<MessageVersion>(), OperationDescription.SessionOpenedAction);
            Fx.Assert(this.LocalAddress != null, "this.LocalAddress should not be null.");
            message.Headers.To = this.LocalAddress.Uri;
            this.sessionOpenNotification.UpdateMessageProperties(message.Properties);
            return this.Binder.CreateRequestContext(message);
        }

        bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            bool valid;
            if (this.needToCreateSessionOpenNotificationMessage)
            {
                this.needToCreateSessionOpenNotificationMessage = false;
                Fx.Assert(result is CompletedAsyncResult, "result must be CompletedAsyncResult");
                CompletedAsyncResult.End(result);
                requestContext = this.GetSessionOpenNotificationRequestContext();
                valid = true;
            }
            else
            {
                valid = this.receiver.EndTryReceive(result, out requestContext);
            }

            if (valid)
            {
                this.HandleReceiveComplete(requestContext);
            }

            return valid;
        }

        void EnsureChannelAndEndpoint(RequestContext request)
        {
            this.requestInfo.Channel = this.channel;

            if (this.requestInfo.Channel == null)
            {
                bool addressMatched;
                if (this.hasSession)
                {
                    this.requestInfo.Channel = this.GetSessionChannel(request.RequestMessage, out this.requestInfo.Endpoint, out addressMatched);
                }
                else
                {
                    this.requestInfo.Channel = this.GetDatagramChannel(request.RequestMessage, out this.requestInfo.Endpoint, out addressMatched);
                }

                if (this.requestInfo.Channel == null)
                {
                    this.host.RaiseUnknownMessageReceived(request.RequestMessage);
                    if (addressMatched)
                    {
                        this.ReplyContractFilterDidNotMatch(request);
                    }
                    else
                    {
                        this.ReplyAddressFilterDidNotMatch(request);
                    }
                }
            }
            else
            {
                this.requestInfo.Endpoint = this.requestInfo.Channel.EndpointDispatcher;

                //For sessionful contracts, the InstanceContext throttle is not copied over to the channel
                //as we create the channel before acquiring the lock
                if (this.InstanceContextServiceThrottle != null && this.requestInfo.Channel.InstanceContextServiceThrottle == null)
                {
                    this.requestInfo.Channel.InstanceContextServiceThrottle = this.InstanceContextServiceThrottle;
                }
            }

            this.requestInfo.EndpointLookupDone = true;

            if (this.requestInfo.Channel == null)
            {
                // SFx drops a message here
                TraceUtility.TraceDroppedMessage(request.RequestMessage, this.requestInfo.Endpoint);
                request.Close();
                return;
            }

            if (this.requestInfo.Channel.HasSession || this.isCallback)
            {
                this.requestInfo.DispatchRuntime = this.requestInfo.Channel.DispatchRuntime;
            }
            else
            {
                this.requestInfo.DispatchRuntime = this.requestInfo.Endpoint.DispatchRuntime;
            }
        }

        void EnsurePump()
        {
            if (null == this.sharedTransactedBatchContext || this.isMainTransactedBatchHandler)
            {
                if (TryAcquirePump())
                {
                    if (this.receiveSynchronously)
                    {
                        ActionItem.Schedule(ChannelHandler.onStartSyncMessagePump, this);
                    }
                    else
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                        {
                            IAsyncResult result = this.BeginTryReceive();
                            if ((result != null) && result.CompletedSynchronously)
                            {
                                ActionItem.Schedule(ChannelHandler.onContinueAsyncReceive, result);
                            }
                        }
                        else
                        {
                            // Since this is not a threadpool thread, we don't know if this thread will exit 
                            // while the IO is still pending (which would cancel the IO), so we have to get 
                            // over to a threadpool thread which we know will not exit while there is pending IO.
                            ActionItem.Schedule(ChannelHandler.onStartAsyncMessagePump, this);
                        }
                    }
                }
            }
            else
            {
                ActionItem.Schedule(ChannelHandler.onStartSingleTransactedBatch, this);
            }
        }

        ServiceChannel GetDatagramChannel(Message message, out EndpointDispatcher endpoint, out bool addressMatched)
        {
            addressMatched = false;
            endpoint = this.GetEndpointDispatcher(message, out addressMatched);

            if (endpoint == null)
            {
                return null;
            }

            if (endpoint.DatagramChannel == null)
            {
                lock (this.listener.ThisLock)
                {
                    if (endpoint.DatagramChannel == null)
                    {
                        endpoint.DatagramChannel = new ServiceChannel(this.binder, endpoint, this.listener.ChannelDispatcher, this.idleManager);
                        this.InitializeServiceChannel(endpoint.DatagramChannel);
                    }
                }
            }

            return endpoint.DatagramChannel;
        }

        EndpointDispatcher GetEndpointDispatcher(Message message, out bool addressMatched)
        {
            return this.listener.Endpoints.Lookup(message, out addressMatched);
        }

        ServiceChannel GetSessionChannel(Message message, out EndpointDispatcher endpoint, out bool addressMatched)
        {
            addressMatched = false;

            if (this.channel == null)
            {
                lock (this.ThisLock)
                {
                    if (this.channel == null)
                    {
                        endpoint = this.GetEndpointDispatcher(message, out addressMatched);
                        if (endpoint != null)
                        {
                            this.channel = new ServiceChannel(this.binder, endpoint, this.listener.ChannelDispatcher, this.idleManager);
                            this.InitializeServiceChannel(this.channel);
                        }
                    }
                }
            }

            if (this.channel == null)
            {
                endpoint = null;
            }
            else
            {
                endpoint = this.channel.EndpointDispatcher;
            }
            return this.channel;
        }

        void InitializeServiceChannel(ServiceChannel channel)
        {
            if (this.wasChannelThrottled)
            {
                // TFS#500703, when the idle timeout was hit, the constructor of ServiceChannel will abort itself directly. So
                // the session throttle will not be released and thus lead to a service unavailablity.
                // Note that if the channel is already aborted, the next line "channel.ServiceThrottle = this.throttle;" will throw an exception,
                // so we are not going to do any more work inside this method. 
                // Ideally we should do a thorough refactoring work for this throttling issue. However, it's too risky as a QFE. We should consider
                // this in a whole release.
                // Note that the "wasChannelThrottled" boolean will only be true if we aquired the session throttle. So we don't have to check HasSession
                // again here.
                if (channel.Aborted && this.throttle != null)
                {
                    // This line will release the "session" throttle.
                    this.throttle.DeactivateChannel();
                }

                channel.ServiceThrottle = this.throttle;
            }

            if (this.InstanceContextServiceThrottle != null)
            {
                channel.InstanceContextServiceThrottle = this.InstanceContextServiceThrottle;
            }

            ClientRuntime clientRuntime = channel.ClientRuntime;
            if (clientRuntime != null)
            {
                Type contractType = clientRuntime.ContractClientType;
                Type callbackType = clientRuntime.CallbackClientType;

                if (contractType != null)
                {
                    channel.Proxy = ServiceChannelFactory.CreateProxy(contractType, callbackType, MessageDirection.Output, channel);
                }
            }

            if (this.listener != null)
            {
                this.listener.ChannelDispatcher.InitializeChannel((IClientChannel)channel.Proxy);
            }

            ((IChannel)channel).Open();
        }

        void ProvideFault(Exception e, ref ErrorHandlerFaultInfo faultInfo)
        {
            if (this.listener != null)
            {
                this.listener.ChannelDispatcher.ProvideFault(e, this.requestInfo.Channel == null ? this.binder.Channel.GetProperty<FaultConverter>() : this.requestInfo.Channel.GetProperty<FaultConverter>(), ref faultInfo);
            }
            else if (this.channel != null)
            {
                DispatchRuntime dispatchBehavior = this.channel.ClientRuntime.CallbackDispatchRuntime;
                dispatchBehavior.ChannelDispatcher.ProvideFault(e, this.channel.GetProperty<FaultConverter>(), ref faultInfo);
            }
        }

        internal bool HandleError(Exception e)
        {
            ErrorHandlerFaultInfo dummy = new ErrorHandlerFaultInfo();
            return this.HandleError(e, ref dummy);
        }

        bool HandleError(Exception e, ref ErrorHandlerFaultInfo faultInfo)
        {
            if (e == null)
            {
                Fx.Assert(SR.GetString(SR.GetString(SR.SFxNonExceptionThrown)));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.GetString(SR.SFxNonExceptionThrown))));
            }
            if (this.listener != null)
            {
                return listener.ChannelDispatcher.HandleError(e, ref faultInfo);
            }
            else if (this.channel != null)
            {
                return this.channel.ClientRuntime.CallbackDispatchRuntime.ChannelDispatcher.HandleError(e, ref faultInfo);
            }
            else
            {
                return false;
            }
        }

        bool HandleError(Exception e, RequestContext request, ServiceChannel channel)
        {
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo(this.messageVersion.Addressing.DefaultFaultAction);
            bool replied, replySentAsync;
            ProvideFaultAndReplyFailure(request, e, ref faultInfo, out replied, out replySentAsync);

            if (!replySentAsync)
            {
                return this.HandleErrorContinuation(e, request, channel, ref faultInfo, replied);
            }
            else
            {
                return false;
            }
        }

        bool HandleErrorContinuation(Exception e, RequestContext request, ServiceChannel channel, ref ErrorHandlerFaultInfo faultInfo, bool replied)
        {
            if (replied)
            {
                try
                {
                    request.Close();
                }
                catch (Exception e1)
                {
                    if (Fx.IsFatal(e1))
                    {
                        throw;
                    }
                    this.HandleError(e1);
                }
            }
            else
            {
                request.Abort();
            }
            if (!this.HandleError(e, ref faultInfo) && this.hasSession)
            {
                if (channel != null)
                {
                    if (replied)
                    {
                        TimeoutHelper timeoutHelper = new TimeoutHelper(CloseAfterFaultTimeout);
                        try
                        {
                            channel.Close(timeoutHelper.RemainingTime());
                        }
                        catch (Exception e2)
                        {
                            if (Fx.IsFatal(e2))
                            {
                                throw;
                            }
                            this.HandleError(e2);
                        }
                        try
                        {
                            this.binder.CloseAfterFault(timeoutHelper.RemainingTime());
                        }
                        catch (Exception e3)
                        {
                            if (Fx.IsFatal(e3))
                            {
                                throw;
                            }
                            this.HandleError(e3);
                        }
                    }
                    else
                    {
                        channel.Abort();
                        this.binder.Abort();
                    }
                }
                else
                {
                    if (replied)
                    {
                        try
                        {
                            this.binder.CloseAfterFault(CloseAfterFaultTimeout);
                        }
                        catch (Exception e4)
                        {
                            if (Fx.IsFatal(e4))
                            {
                                throw;
                            }
                            this.HandleError(e4);
                        }
                    }
                    else
                    {
                        this.binder.Abort();
                    }
                }
            }

            return true;
        }

        void HandleReceiveComplete(RequestContext context)
        {
            try
            {
                if (this.channel != null)
                {
                    this.channel.HandleReceiveComplete(context);
                }
                else
                {
                    if (context == null && this.hasSession)
                    {
                        bool close;
                        lock (this.ThisLock)
                        {
                            close = !this.doneReceiving;
                            this.doneReceiving = true;
                        }

                        if (close)
                        {
                            this.receiver.Close();

                            if (this.idleManager != null)
                            {
                                this.idleManager.CancelTimer();
                            }

                            ServiceThrottle throttle = this.throttle;
                            if (throttle != null)
                            {
                                throttle.DeactivateChannel();
                            }
                        }
                    }
                }
            }
            finally
            {
                if ((context == null) && this.incrementedActivityCountInConstructor)
                {
                    this.listener.ChannelDispatcher.Channels.DecrementActivityCount();
                }
            }
        }

        bool HandleRequest(RequestContext request, OperationContext currentOperationContext)
        {
            if (request == null)
            {
                // channel EOF, stop receiving
                return false;
            }

            ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(request) : null;

            using (ServiceModelActivity.BoundOperation(activity))
            {
                if (this.HandleRequestAsReply(request))
                {
                    this.ReleasePump();
                    return true;
                }

                if (this.isChannelTerminated)
                {
                    this.ReleasePump();
                    this.ReplyChannelTerminated(request);
                    return true;
                }

                if (this.requestInfo.RequestContext != null)
                {
                    Fx.Assert("ChannelHandler.HandleRequest: this.requestInfo.RequestContext != null");
                }

                this.requestInfo.RequestContext = request;

                if (!this.TryAcquireCallThrottle(request))
                {
                    // this.ThrottleAcquiredForCall will be called to continue
                    return false;
                }

                // NOTE: from here on down, ensure that this code is the same as ThrottleAcquiredForCall (see 55460)
                if (this.requestInfo.ChannelHandlerOwnsCallThrottle)
                {
                    Fx.Assert("ChannelHandler.HandleRequest: this.requestInfo.ChannelHandlerOwnsCallThrottle");
                }
                this.requestInfo.ChannelHandlerOwnsCallThrottle = true;

                if (!this.TryRetrievingInstanceContext(request))
                {
                    //Would have replied and close the request.
                    return true;
                }

                this.requestInfo.Channel.CompletedIOOperation();

                //Only acquire InstanceContext throttle if one doesnt already exist.
                if (!this.TryAcquireThrottle(request, (this.requestInfo.ExistingInstanceContext == null)))
                {
                    // this.ThrottleAcquired will be called to continue
                    return false;
                }
                if (this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle)
                {
                    Fx.Assert("ChannelHandler.HandleRequest: this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle");
                }
                this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = (this.requestInfo.ExistingInstanceContext == null);

                if (!this.DispatchAndReleasePump(request, true, currentOperationContext))
                {
                    // this.DispatchDone will be called to continue
                    return false;
                }
            }
            return true;
        }

        bool HandleRequestAsReply(RequestContext request)
        {
            if (this.duplexBinder != null)
            {
                if (this.duplexBinder.HandleRequestAsReply(request.RequestMessage))
                {
                    return true;
                }
            }
            return false;
        }

        static void OnStartAsyncMessagePump(object state)
        {
            ((ChannelHandler)state).AsyncMessagePump();
        }

        static void OnStartSyncMessagePump(object state)
        {
            ChannelHandler handler = state as ChannelHandler;

            if (TD.ChannelReceiveStopIsEnabled())
            {
                TD.ChannelReceiveStop(handler.EventTraceActivity, state.GetHashCode());
            }

            if (handler.receiveWithTransaction)
            {
                handler.SyncTransactionalMessagePump();
            }
            else
            {
                handler.SyncMessagePump();
            }
        }

        static void OnStartSingleTransactedBatch(object state)
        {
            ChannelHandler handler = state as ChannelHandler;
            handler.TransactedBatchLoop();
        }

        static void OnAsyncReceiveComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((ChannelHandler)result.AsyncState).AsyncMessagePump(result);
            }
        }

        static void OnContinueAsyncReceive(object state)
        {
            IAsyncResult result = (IAsyncResult)state;
            ((ChannelHandler)result.AsyncState).AsyncMessagePump(result);
        }

        static void OpenAndEnsurePump(object state)
        {
            ((ChannelHandler)state).OpenAndEnsurePump();
        }

        void OpenAndEnsurePump()
        {
            Exception exception = null;
            try
            {
                this.binder.Channel.Open();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                exception = e;
            }

            if (exception != null)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                        TraceCode.FailedToOpenIncomingChannel,
                        SR.GetString(SR.TraceCodeFailedToOpenIncomingChannel));
                }
                SessionIdleManager idleManager = this.idleManager;
                if (idleManager != null)
                {
                    idleManager.CancelTimer();
                }
                if ((this.throttle != null) && this.hasSession)
                {
                    this.throttle.DeactivateChannel();
                }

                bool errorHandled = this.HandleError(exception);

                if (this.incrementedActivityCountInConstructor)
                {
                    this.listener.ChannelDispatcher.Channels.DecrementActivityCount();
                }

                if (!errorHandled)
                {
                    this.binder.Channel.Abort();
                }
            }
            else
            {
                this.EnsurePump();
            }
        }

        bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            this.shouldRejectMessageWithOnOpenActionHeader = !this.needToCreateSessionOpenNotificationMessage;

            bool valid;
            if (this.needToCreateSessionOpenNotificationMessage)
            {
                this.needToCreateSessionOpenNotificationMessage = false;
                requestContext = this.GetSessionOpenNotificationRequestContext();
                valid = true;
            }
            else
            {
                valid = this.receiver.TryReceive(timeout, out requestContext);
            }

            if (valid)
            {
                this.HandleReceiveComplete(requestContext);
            }

            return valid;
        }

        void ReplyAddressFilterDidNotMatch(RequestContext request)
        {
            FaultCode code = FaultCode.CreateSenderFaultCode(AddressingStrings.DestinationUnreachable,
                this.messageVersion.Addressing.Namespace);
            string reason = SR.GetString(SR.SFxNoEndpointMatchingAddress, request.RequestMessage.Headers.To);

            ReplyFailure(request, code, reason);
        }

        void ReplyContractFilterDidNotMatch(RequestContext request)
        {
            // By default, the contract filter is just a filter over the set of initiating actions in 
            // the contract, so we do error messages accordingly
            AddressingVersion addressingVersion = this.messageVersion.Addressing;
            if (addressingVersion != AddressingVersion.None && request.RequestMessage.Headers.Action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MessageHeaderException(
                    SR.GetString(SR.SFxMissingActionHeader, addressingVersion.Namespace), AddressingStrings.Action, addressingVersion.Namespace));
            }
            else
            {
                // some of this code is duplicated in DispatchRuntime.UnhandledActionInvoker
                // ideally both places would use FaultConverter and ActionNotSupportedException
                FaultCode code = FaultCode.CreateSenderFaultCode(AddressingStrings.ActionNotSupported,
                    this.messageVersion.Addressing.Namespace);
                string reason = SR.GetString(SR.SFxNoEndpointMatchingContract, request.RequestMessage.Headers.Action);
                ReplyFailure(request, code, reason, this.messageVersion.Addressing.FaultAction);
            }
        }

        void ReplyChannelTerminated(RequestContext request)
        {
            FaultCode code = FaultCode.CreateSenderFaultCode(FaultCodeConstants.Codes.SessionTerminated,
                FaultCodeConstants.Namespaces.NetDispatch);
            string reason = SR.GetString(SR.SFxChannelTerminated0);
            string action = FaultCodeConstants.Actions.NetDispatcher;
            Message fault = Message.CreateMessage(this.messageVersion, code, reason, action);
            ReplyFailure(request, fault, action, reason, code);
        }

        void ReplyFailure(RequestContext request, FaultCode code, string reason)
        {
            string action = this.messageVersion.Addressing.DefaultFaultAction;
            ReplyFailure(request, code, reason, action);
        }

        void ReplyFailure(RequestContext request, FaultCode code, string reason, string action)
        {
            Message fault = Message.CreateMessage(this.messageVersion, code, reason, action);
            ReplyFailure(request, fault, action, reason, code);
        }

        void ReplyFailure(RequestContext request, Message fault, string action, string reason, FaultCode code)
        {
            FaultException exception = new FaultException(reason, code);
            ErrorBehavior.ThrowAndCatch(exception);
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo(action);
            faultInfo.Fault = fault;
            bool replied, replySentAsync;
            ProvideFaultAndReplyFailure(request, exception, ref faultInfo, out replied, out replySentAsync);
            this.HandleError(exception, ref faultInfo);
        }

        void ProvideFaultAndReplyFailure(RequestContext request, Exception exception, ref ErrorHandlerFaultInfo faultInfo, out bool replied, out bool replySentAsync)
        {
            replied = false;
            replySentAsync = false;
            bool requestMessageIsFault = false;
            try
            {
                requestMessageIsFault = request.RequestMessage.IsFault;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                // ---- it
            }

            bool enableFaults = false;
            if (this.listener != null)
            {
                enableFaults = this.listener.ChannelDispatcher.EnableFaults;
            }
            else if (this.channel != null && this.channel.IsClient)
            {
                enableFaults = this.channel.ClientRuntime.EnableFaults;
            }

            if ((!requestMessageIsFault) && enableFaults)
            {
                this.ProvideFault(exception, ref faultInfo);
                if (faultInfo.Fault != null)
                {
                    Message reply = faultInfo.Fault;
                    try
                    {
                        try
                        {
                            if (this.PrepareReply(request, reply))
                            {
                                if (this.sendAsynchronously)
                                {
                                    var state = new ContinuationState { ChannelHandler = this, Channel = channel, Exception = exception, FaultInfo = faultInfo, Request = request, Reply = reply };
                                    var result = request.BeginReply(reply, ChannelHandler.onAsyncReplyComplete, state);
                                    if (result.CompletedSynchronously)
                                    {
                                        ChannelHandler.AsyncReplyComplete(result, state);
                                        replied = true;
                                    }
                                    else
                                    {
                                        replySentAsync = true;
                                    }
                                }
                                else
                                {
                                    request.Reply(reply);
                                    replied = true;
                                }
                            }
                        }
                        finally
                        {
                            if (!replySentAsync)
                            {
                                reply.Close();
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.HandleError(e);
                    }
                }
            }
        }

        /// <summary>
        /// Prepares a reply that can either be sent asynchronously or synchronously depending on the value of 
        /// sendAsynchronously
        /// </summary>
        /// <param name="request">The request context to prepare</param>
        /// <param name="reply">The reply to prepare</param>
        /// <returns>True if channel is open and prepared reply should be sent; otherwise false.</returns>
        bool PrepareReply(RequestContext request, Message reply)
        {
            // Ensure we only reply once (we may hit the same error multiple times)
            if (this.replied == request)
            {
                return false;
            }
            this.replied = request;

            bool canSendReply = true;

            Message requestMessage = null;
            try
            {
                requestMessage = request.RequestMessage;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                // ---- it
            }
            if (!object.ReferenceEquals(requestMessage, null))
            {
                UniqueId requestID = null;
                try
                {
                    requestID = requestMessage.Headers.MessageId;
                }
                catch (MessageHeaderException)
                {
                    // ---- it - we don't need to correlate the reply if the MessageId header is bad
                }
                if (!object.ReferenceEquals(requestID, null) && !this.isManualAddressing)
                {
                    System.ServiceModel.Channels.RequestReplyCorrelator.PrepareReply(reply, requestID);
                }
                if (!this.hasSession && !this.isManualAddressing)
                {
                    try
                    {
                        canSendReply = System.ServiceModel.Channels.RequestReplyCorrelator.AddressReply(reply, requestMessage);
                    }
                    catch (MessageHeaderException)
                    {
                        // ---- it - we don't need to address the reply if the FaultTo header is bad
                    }
                }
            }

            // ObjectDisposeException can happen
            // if the channel is closed in a different
            // thread. 99% this check will avoid false
            // exceptions.
            return this.IsOpen && canSendReply;
        }

        static void AsyncReplyComplete(IAsyncResult result, ContinuationState state)
        {
            try
            {
                state.Request.EndReply(result);
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Error);

                if (Fx.IsFatal(e))
                {
                    throw;
                }
                
                state.ChannelHandler.HandleError(e);
            }

            try
            {
                state.Reply.Close();
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Error);

                if (Fx.IsFatal(e))
                {
                    throw;
                }

                state.ChannelHandler.HandleError(e);
            }

            try
            {
                state.ChannelHandler.HandleErrorContinuation(state.Exception, state.Request, state.Channel, ref state.FaultInfo, true);
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Error);

                if (Fx.IsFatal(e))
                {
                    throw;
                }

                state.ChannelHandler.HandleError(e);
            }

            state.ChannelHandler.EnsurePump();
        }

        static void OnAsyncReplyComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            try
            {
                var state = (ContinuationState)result.AsyncState;
                ChannelHandler.AsyncReplyComplete(result, state);
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Error);

                if (Fx.IsFatal(e))
                {
                    throw;
                }
            }
        }

        void ReleasePump()
        {
            if (this.isConcurrent)
            {
                Interlocked.Exchange(ref this.isPumpAcquired, 0);
            }
        }

        void SyncMessagePump()
        {
            OperationContext existingOperationContext = OperationContext.Current;
            try
            {
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;

                for (;;)
                {
                    RequestContext request;

                    this.requestInfo.Cleanup();

                    while (!TryReceive(TimeSpan.MaxValue, out request))
                    {
                    }

                    if (!HandleRequest(request, currentOperationContext))
                    {
                        break;
                    }

                    if (!TryAcquirePump())
                    {
                        break;
                    }

                    currentOperationContext.Recycle();
                }
            }
            finally
            {
                OperationContext.Current = existingOperationContext;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void SyncTransactionalMessagePump()
        {
            for (;;)
            {
                bool completedSynchronously;
                if (null == sharedTransactedBatchContext)
                {
                    completedSynchronously = TransactedLoop();
                }
                else
                {
                    completedSynchronously = TransactedBatchLoop();
                }

                if (!completedSynchronously)
                {
                    return;
                }
            }
        }

        bool TransactedLoop()
        {
            try
            {
                this.receiver.WaitForMessage();
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                if (!this.HandleError(ex))
                {
                    throw;
                }
            }

            RequestContext request;
            Transaction tx = CreateOrGetAttachedTransaction();
            OperationContext existingOperationContext = OperationContext.Current;

            try
            {
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;

                for (;;)
                {
                    this.requestInfo.Cleanup();

                    bool received = TryTransactionalReceive(tx, out request);

                    if (!received)
                    {
                        return IsOpen;
                    }

                    if (null == request)
                    {
                        return false;
                    }

                    TransactionMessageProperty.Set(tx, request.RequestMessage);

                    if (!HandleRequest(request, currentOperationContext))
                    {
                        return false;
                    }

                    if (!TryAcquirePump())
                    {
                        return false;
                    }

                    tx = CreateOrGetAttachedTransaction();
                    currentOperationContext.Recycle();
                }
            }
            finally
            {
                OperationContext.Current = existingOperationContext;
            }
        }

        bool TransactedBatchLoop()
        {
            if (null != this.transactedBatchContext)
            {
                if (this.transactedBatchContext.InDispatch)
                {
                    this.transactedBatchContext.ForceRollback();
                    this.transactedBatchContext.InDispatch = false;
                }
                if (!this.transactedBatchContext.IsActive)
                {
                    if (!this.isMainTransactedBatchHandler)
                    {
                        return false;
                    }
                    this.transactedBatchContext = null;
                }
            }

            if (null == this.transactedBatchContext)
            {
                try
                {
                    this.receiver.WaitForMessage();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    if (!this.HandleError(ex))
                    {
                        throw;
                    }
                }
                this.transactedBatchContext = this.sharedTransactedBatchContext.CreateTransactedBatchContext();
            }

            OperationContext existingOperationContext = OperationContext.Current;

            try
            {
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;

                RequestContext request;

                while (this.transactedBatchContext.IsActive)
                {
                    this.requestInfo.Cleanup();

                    bool valid = TryTransactionalReceive(this.transactedBatchContext.Transaction, out request);

                    if (!valid)
                    {
                        if (this.IsOpen)
                        {
                            this.transactedBatchContext.ForceCommit();
                            return true;
                        }
                        else
                        {
                            this.transactedBatchContext.ForceRollback();
                            return false;
                        }
                    }

                    if (null == request)
                    {
                        this.transactedBatchContext.ForceRollback();
                        return false;
                    }

                    TransactionMessageProperty.Set(this.transactedBatchContext.Transaction, request.RequestMessage);

                    this.transactedBatchContext.InDispatch = true;
                    if (!HandleRequest(request, currentOperationContext))
                    {
                        return false;
                    }

                    if (this.transactedBatchContext.InDispatch)
                    {
                        this.transactedBatchContext.ForceRollback();
                        this.transactedBatchContext.InDispatch = false;
                        return true;
                    }

                    if (!TryAcquirePump())
                    {
                        Fx.Assert("System.ServiceModel.Dispatcher.ChannelHandler.TransactedBatchLoop(): (TryAcquiredPump returned false)");
                        return false;
                    }

                    currentOperationContext.Recycle();
                }
            }
            finally
            {
                OperationContext.Current = existingOperationContext;
            }
            return true;
        }

        Transaction CreateOrGetAttachedTransaction()
        {
            if (null != this.acceptTransaction)
            {
                lock (ThisLock)
                {
                    if (null != this.acceptTransaction)
                    {
                        Transaction tx = this.acceptTransaction.Transaction;
                        this.acceptTransaction = null;
                        return tx;
                    }
                }
            }

            if (null != this.InstanceContext && this.InstanceContext.HasTransaction)
            {
                return InstanceContext.Transaction.Attached;
            }
            else
            {
                return TransactionBehavior.CreateTransaction(
                    this.listener.ChannelDispatcher.TransactionIsolationLevel,
                    TransactionBehavior.NormalizeTimeout(this.listener.ChannelDispatcher.TransactionTimeout));
            }
        }

        // calls receive on the channel; returns false if no message during the "short timeout"
        bool TryTransactionalReceive(Transaction tx, out RequestContext request)
        {
            request = null;
            bool received = false;

            try
            {
                using (TransactionScope scope = new TransactionScope(tx))
                {
                    if (null != this.sharedTransactedBatchContext)
                    {
                        lock (this.sharedTransactedBatchContext.ReceiveLock)
                        {
                            if (this.transactedBatchContext.AboutToExpire)
                            {
                                return false;
                            }

                            received = this.receiver.TryReceive(TimeSpan.Zero, out request);
                        }
                    }
                    else
                    {
                        TimeSpan receiveTimeout = TimeoutHelper.Min(this.listener.ChannelDispatcher.TransactionTimeout, this.listener.ChannelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                        received = this.receiver.TryReceive(TransactionBehavior.NormalizeTimeout(receiveTimeout), out request);
                    }
                    scope.Complete();
                }

                if (received)
                {
                    this.HandleReceiveComplete(request);
                }
            }
            catch (ObjectDisposedException ex) // thrown from the transaction
            {
                this.HandleError(ex);
                request = null;
                return false;
            }
            catch (TransactionException ex)
            {
                this.HandleError(ex);
                request = null;
                return false;
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                if (!this.HandleError(ex))
                {
                    throw;
                }
            }

            return received;
        }

        // This callback always occurs async and always on a dirty thread
        internal void ThrottleAcquiredForCall()
        {
            RequestContext request = this.requestWaitingForThrottle;
            this.requestWaitingForThrottle = null;
            if (this.requestInfo.ChannelHandlerOwnsCallThrottle)
            {
                Fx.Assert("ChannelHandler.ThrottleAcquiredForCall: this.requestInfo.ChannelHandlerOwnsCallThrottle");
            }
            this.requestInfo.ChannelHandlerOwnsCallThrottle = true;

            if (!this.TryRetrievingInstanceContext(request))
            {
                //Should reply/close request and also close the pump                
                this.EnsurePump();
                return;
            }

            this.requestInfo.Channel.CompletedIOOperation();

            if (this.TryAcquireThrottle(request, (this.requestInfo.ExistingInstanceContext == null)))
            {
                if (this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle)
                {
                    Fx.Assert("ChannelHandler.ThrottleAcquiredForCall: this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle");
                }
                this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = (this.requestInfo.ExistingInstanceContext == null);

                if (this.DispatchAndReleasePump(request, false, null))
                {
                    this.EnsurePump();
                }
            }
        }

        bool TryRetrievingInstanceContext(RequestContext request)
        {
            try
            {
                return TryRetrievingInstanceContextCore(request);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(ex, TraceEventType.Error);

                try
                {
                    request.Close();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    request.Abort();
                }

                return false;
            }
        }

        //Return: False denotes failure, Caller should discard the request.
        //      : True denotes operation is sucessful.
        bool TryRetrievingInstanceContextCore(RequestContext request)
        {
            bool releasePump = true;
            try
            {
                if (!this.requestInfo.EndpointLookupDone)
                {
                    this.EnsureChannelAndEndpoint(request);
                }

                if (this.requestInfo.Channel == null)
                {
                    return false;
                }

                if (this.requestInfo.DispatchRuntime != null)
                {
                    IContextChannel transparentProxy = this.requestInfo.Channel.Proxy as IContextChannel;
                    try
                    {
                        this.requestInfo.ExistingInstanceContext = this.requestInfo.DispatchRuntime.InstanceContextProvider.GetExistingInstanceContext(request.RequestMessage, transparentProxy);
                        releasePump = false;
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.requestInfo.Channel = null;
                        this.HandleError(e, request, channel);
                        return false;
                    }
                }
                else
                {
                    // This can happen if we are pumping for an async client,
                    // and we receive a bogus reply.  In that case, there is no
                    // DispatchRuntime, because we are only expecting replies.
                    //
                    // One possible fix for this would be in DuplexChannelBinder
                    // to drop all messages with a RelatesTo that do not match a
                    // pending request.
                    //
                    // However, that would not fix:
                    // (a) we could get a valid request message with a
                    // RelatesTo that we should try to process.
                    // (b) we could get a reply message that does not have
                    // a RelatesTo.
                    //
                    // So we do the null check here.
                    //
                    // SFx drops a message here
                    TraceUtility.TraceDroppedMessage(request.RequestMessage, this.requestInfo.Endpoint);
                    request.Close();
                    return false;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.HandleError(e, request, channel);
                
                return false;
            }
            finally
            {
                if (releasePump)
                {
                    this.ReleasePump();
                }
            }
            return true;
        }

        // This callback always occurs async and always on a dirty thread
        internal void ThrottleAcquired()
        {
            RequestContext request = this.requestWaitingForThrottle;
            this.requestWaitingForThrottle = null;
            if (this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle)
            {
                Fx.Assert("ChannelHandler.ThrottleAcquired: this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle");
            }
            this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = (this.requestInfo.ExistingInstanceContext == null);

            if (this.DispatchAndReleasePump(request, false, null))
            {
                this.EnsurePump();
            }
        }

        bool TryAcquireThrottle(RequestContext request, bool acquireInstanceContextThrottle)
        {
            ServiceThrottle throttle = this.throttle;
            if ((throttle != null) && (throttle.IsActive))
            {
                this.requestWaitingForThrottle = request;

                if (throttle.AcquireInstanceContextAndDynamic(this, acquireInstanceContextThrottle))
                {
                    this.requestWaitingForThrottle = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        bool TryAcquireCallThrottle(RequestContext request)
        {
            ServiceThrottle throttle = this.throttle;
            if ((throttle != null) && (throttle.IsActive))
            {
                this.requestWaitingForThrottle = request;

                if (throttle.AcquireCall(this))
                {
                    this.requestWaitingForThrottle = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        bool TryAcquirePump()
        {
            if (this.isConcurrent)
            {
                return Interlocked.CompareExchange(ref this.isPumpAcquired, 1, 0) == 0;
            }

            return true;
        }

        struct RequestInfo
        {
            public EndpointDispatcher Endpoint;
            public InstanceContext ExistingInstanceContext;
            public ServiceChannel Channel;
            public bool EndpointLookupDone;
            public DispatchRuntime DispatchRuntime;
            public RequestContext RequestContext;
            public ChannelHandler ChannelHandler;
            public bool ChannelHandlerOwnsCallThrottle; // if true, we are responsible for call throttle
            public bool ChannelHandlerOwnsInstanceContextThrottle; // if true, we are responsible for instance/dynamic throttle

            public RequestInfo(ChannelHandler channelHandler)
            {
                this.Endpoint = null;
                this.ExistingInstanceContext = null;
                this.Channel = null;
                this.EndpointLookupDone = false;
                this.DispatchRuntime = null;
                this.RequestContext = null;
                this.ChannelHandler = channelHandler;
                this.ChannelHandlerOwnsCallThrottle = false;
                this.ChannelHandlerOwnsInstanceContextThrottle = false;
            }

            public void Cleanup()
            {
                if (this.ChannelHandlerOwnsInstanceContextThrottle)
                {
                    this.ChannelHandler.throttle.DeactivateInstanceContext();
                    this.ChannelHandlerOwnsInstanceContextThrottle = false;
                }

                this.Endpoint = null;
                this.ExistingInstanceContext = null;
                this.Channel = null;
                this.EndpointLookupDone = false;
                this.RequestContext = null;
                if (this.ChannelHandlerOwnsCallThrottle)
                {
                    this.ChannelHandler.DispatchDone();
                    this.ChannelHandlerOwnsCallThrottle = false;
                }
            }
        }

        EventTraceActivity TraceDispatchMessageStart(Message message)
        {
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled && message != null)
            {
                EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                if (TD.DispatchMessageStartIsEnabled())
                {
                    TD.DispatchMessageStart(eventTraceActivity);
                }
                return eventTraceActivity;
            }

            return null;
        }

        /// <summary>
        /// Data structure used to carry state for asynchronous replies
        /// </summary>
        struct ContinuationState
        {
            public ChannelHandler ChannelHandler;
            public Exception Exception;
            public RequestContext Request;
            public Message Reply;
            public ServiceChannel Channel;
            public ErrorHandlerFaultInfo FaultInfo;
        }
    }
}
