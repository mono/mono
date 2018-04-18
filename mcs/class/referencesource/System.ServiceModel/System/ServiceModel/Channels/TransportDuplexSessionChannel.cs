// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;

    abstract class TransportDuplexSessionChannel : TransportOutputChannel, IDuplexSessionChannel
    {
        BufferManager bufferManager;
        IDuplexSession duplexSession;
        bool isInputSessionClosed;
        bool isOutputSessionClosed;
        MessageEncoder messageEncoder;
        SynchronizedMessageSource messageSource;
        SecurityMessageProperty remoteSecurity;
        EndpointAddress localAddress;
        ThreadNeutralSemaphore sendLock;
        Uri localVia;
        ChannelBinding channelBindingToken;

        protected TransportDuplexSessionChannel(
                  ChannelManagerBase manager, 
                  ITransportFactorySettings settings,
                  EndpointAddress localAddress, 
                  Uri localVia, 
                  EndpointAddress remoteAddresss, 
                  Uri via)
                : base(manager, remoteAddresss, via, settings.ManualAddressing, settings.MessageVersion)
        {
            this.localAddress = localAddress;
            this.localVia = localVia;
            this.bufferManager = settings.BufferManager;
            this.sendLock = new ThreadNeutralSemaphore(1);
            this.messageEncoder = settings.MessageEncoderFactory.CreateSessionEncoder();
            this.Session = new ConnectionDuplexSession(this);
        }

        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        public SecurityMessageProperty RemoteSecurity
        {
            get { return this.remoteSecurity; }
            protected set { this.remoteSecurity = value; }
        }

        public IDuplexSession Session
        {
            get { return this.duplexSession; }
            protected set { this.duplexSession = value; }
        }

        public ThreadNeutralSemaphore SendLock
        {
            get { return this.sendLock; }
        }

        protected ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        protected BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        protected Uri LocalVia
        {
            get { return this.localVia; }
        }

        protected MessageEncoder MessageEncoder
        {
            get { return this.messageEncoder; }
            set { this.messageEncoder = value; }
        }

        protected SynchronizedMessageSource MessageSource
        {
            get { return this.messageSource; }
        }

        protected abstract bool IsStreamedOutput { get; }
        
        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = null;
            if (DoneReceivingInCurrentState())
            {
                return null;
            }

            bool shouldFault = true;
            try
            {
                message = this.messageSource.Receive(timeout);
                this.OnReceiveMessage(message);
                shouldFault = false;
                return message;
            }
            finally
            {
                if (shouldFault)
                {
                    if (message != null)
                    {
                        message.Close();
                        message = null;
                    }

                    this.Fault();
                }
            }
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }

            bool shouldFault = true;
            try
            {
                IAsyncResult result = this.messageSource.BeginReceive(timeout, callback, state);
                shouldFault = false;
                return result;
            }
            finally
            {
                if (shouldFault)
                {
                    this.Fault();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability106",
                            Justification = "This is an old method from previous release.")]
        public Message EndReceive(IAsyncResult result)
        {
            this.ThrowIfNotOpened(); // we can't be in Created or Opening
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            DoneReceivingAsyncResult doneReceivingResult = result as DoneReceivingAsyncResult;
            if (doneReceivingResult != null)
            {
                DoneReceivingAsyncResult.End(doneReceivingResult);
                return null;
            }

            bool shouldFault = true;
            Message message = null;
            try
            {
                message = this.messageSource.EndReceive(result);
                this.OnReceiveMessage(message);
                shouldFault = false;
                return message;
            }
            finally
            {
                if (shouldFault)
                {
                    if (message != null)
                    {
                        message.Close();
                        message = null;
                    }

                    this.Fault();
                }
            }
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return TryReceiveAsyncResult.End(result, out message);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            try
            {
                message = this.Receive(timeout);
                return true;
            }
            catch (TimeoutException e)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                message = null;
                return false;
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            if (DoneReceivingInCurrentState())
            {
                return true;
            }

            bool shouldFault = true;
            try
            {
                bool success = this.messageSource.WaitForMessage(timeout);
                shouldFault = !success; // need to fault if we've timed out because we're now toast
                return success;
            }
            finally
            {
                if (shouldFault)
                {
                    this.Fault();
                }
            }
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }

            bool shouldFault = true;
            try
            {
                IAsyncResult result = this.messageSource.BeginWaitForMessage(timeout, callback, state);
                shouldFault = false;
                return result;
            }
            finally
            {
                if (shouldFault)
                {
                    this.Fault();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability106",
                            Justification = "This is an old method from previous release.")]
        public bool EndWaitForMessage(IAsyncResult result)
        {
            this.ThrowIfNotOpened(); // we can't be in Created or Opening
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
            if (doneRecevingResult != null)
            {
                return DoneReceivingAsyncResult.End(doneRecevingResult);
            }

            bool shouldFault = true;
            try
            {
                bool success = this.messageSource.EndWaitForMessage(result);
                shouldFault = !success; // need to fault if we've timed out because we're now toast
                return success;
            }
            finally
            {
                if (shouldFault)
                {
                    this.Fault();
                }
            }
        }

        protected void SetChannelBinding(ChannelBinding channelBinding)
        {
            Fx.Assert(this.channelBindingToken == null, "ChannelBinding token can only be set once.");
            this.channelBindingToken = channelBinding;
        }

        protected void SetMessageSource(IMessageSource messageSource)
        {
            this.messageSource = new SynchronizedMessageSource(messageSource);
        }

        protected IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseOutputSessionAsyncResult(this, timeout, callback, state);
        }

        protected void EndCloseOutputSession(IAsyncResult result)
        {
            CloseOutputSessionAsyncResult.End(result);
        }
        
        protected abstract void CloseOutputSessionCore(TimeSpan timeout);

        protected void CloseOutputSession(TimeSpan timeout)
        {
            ThrowIfNotOpened();
            ThrowIfFaulted();
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.sendLock.TryEnter(timeoutHelper.RemainingTime()))
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(SR.GetString(SR.CloseTimedOut, timeout));
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(
                                                SR.GetString(SR.CloseTimedOut, timeout),
                                                ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }

            try
            {
                // check again in case the previous send faulted while we were waiting for the lock
                ThrowIfFaulted();

                // we're synchronized by sendLock here
                if (this.isOutputSessionClosed)
                {
                    return;
                }

                this.isOutputSessionClosed = true;
                bool shouldFault = true;
                try
                {
                    this.CloseOutputSessionCore(timeout);
                    this.OnOutputSessionClosed(ref timeoutHelper);
                    shouldFault = false;
                }
                finally
                {
                    if (shouldFault)
                    {
                        this.Fault();
                    }
                }
            }
            finally
            {
                this.sendLock.Exit();
            }
        }

        // used to return cached connection to the pool/reader pool
        protected abstract void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout);

        protected override void OnAbort()
        {
            this.ReturnConnectionIfNecessary(true, TimeSpan.Zero);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.ReturnConnectionIfNecessary(true, TimeSpan.Zero);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.CloseOutputSession(timeoutHelper.RemainingTime());

            // close input session if necessary
            if (!this.isInputSessionClosed)
            {
                this.EnsureInputClosed(timeoutHelper.RemainingTime());
                this.OnInputSessionClosed();
            }

            this.CompleteClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            // clean up the CBT after transitioning to the closed state
            ChannelBindingUtility.Dispose(ref this.channelBindingToken);
        }

        protected virtual void OnReceiveMessage(Message message)
        {
            if (message == null)
            {
                this.OnInputSessionClosed();
            }
            else
            {
                this.PrepareMessage(message);
            }
        }

        protected void ApplyChannelBinding(Message message)
        {
            ChannelBindingUtility.TryAddToMessage(this.channelBindingToken, message, false);
        }

        protected virtual void PrepareMessage(Message message)
        {
            message.Properties.Via = this.localVia;

            this.ApplyChannelBinding(message);

            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                Guid relatedActivityId = EventTraceActivity.GetActivityIdFromThread();
                if (eventTraceActivity == null)
                {
                    eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                    EventTraceActivityHelper.TryAttachActivity(message, eventTraceActivity);
                }

                if (TD.MessageReceivedByTransportIsEnabled())
                {
                    TD.MessageReceivedByTransport(
                        eventTraceActivity,
                        this.LocalAddress != null && this.LocalAddress.Uri != null ? this.LocalAddress.Uri.AbsoluteUri : string.Empty,
                        relatedActivityId);
                }
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                             TraceEventType.Information, 
                             TraceCode.MessageReceived, 
                             SR.GetString(SR.TraceCodeMessageReceived),
                             MessageTransmitTraceRecord.CreateReceiveTraceRecord(message, this.LocalAddress), 
                             this, 
                             null, 
                             message);
            }
        }

        protected abstract AsyncCompletionResult StartWritingBufferedMessage(Message message, ArraySegment<byte> messageData, bool allowOutputBatching, TimeSpan timeout, WaitCallback callback, object state);

        protected abstract AsyncCompletionResult BeginCloseOutput(TimeSpan timeout, WaitCallback callback, object state);

        protected virtual void FinishWritingMessage()
        { 
        }

        protected abstract ArraySegment<byte> EncodeMessage(Message message);

        protected abstract void OnSendCore(Message message, TimeSpan timeout);

        protected abstract AsyncCompletionResult StartWritingStreamedMessage(Message message, TimeSpan timeout, WaitCallback callback, object state);        

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            this.ThrowIfDisposedOrNotOpen();

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.sendLock.TryEnter(timeoutHelper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(
                                            SR.GetString(SR.SendToViaTimedOut, Via, timeout),
                                            ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }

            try
            {
                // check again in case the previous send faulted while we were waiting for the lock
                this.ThrowIfDisposedOrNotOpen();
                this.ThrowIfOutputSessionClosed();

                bool success = false;
                try
                {
                    this.ApplyChannelBinding(message);

                    this.OnSendCore(message, timeoutHelper.RemainingTime());
                    success = true;
                    if (TD.MessageSentByTransportIsEnabled())
                    {
                        EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                        TD.MessageSentByTransport(eventTraceActivity, this.RemoteAddress.Uri.AbsoluteUri);
                    }
                }
                finally
                {
                    if (!success)
                    {
                        this.Fault();
                    }
                }
            }
            finally
            {
                this.sendLock.Exit();
            }
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposedOrNotOpen();
            return new SendAsyncResult(this, message, timeout, this.IsStreamedOutput, callback, state);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            SendAsyncResult.End(result);
        }

        // cleanup after the framing handshake has completed
        protected abstract void CompleteClose(TimeSpan timeout);

        // must be called under sendLock 
        void ThrowIfOutputSessionClosed()
        {
            if (this.isOutputSessionClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SendCannotBeCalledAfterCloseOutputSession)));
            }
        }

        void EnsureInputClosed(TimeSpan timeout)
        {
            Message message = this.MessageSource.Receive(timeout);
            if (message != null)
            {
                using (message)
                {
                    ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                    throw TraceUtility.ThrowHelperError(error, message);
                }
            }
        }

        void OnInputSessionClosed()
        {
            lock (ThisLock)
            {
                if (this.isInputSessionClosed)
                {
                    return;
                }

                this.isInputSessionClosed = true;
            }
        }

        void OnOutputSessionClosed(ref TimeoutHelper timeoutHelper)
        {
            bool releaseConnection = false;
            lock (ThisLock)
            {
                if (this.isInputSessionClosed)
                { 
                    // we're all done, release the connection
                    releaseConnection = true;
                }
            }

            if (releaseConnection)
            {
                this.ReturnConnectionIfNecessary(false, timeoutHelper.RemainingTime());
            }
        }

        internal class ConnectionDuplexSession : IDuplexSession
        {
            static UriGenerator uriGenerator;
            TransportDuplexSessionChannel channel;
            string id;

            public ConnectionDuplexSession(TransportDuplexSessionChannel channel)
                : base()
            {
                this.channel = channel;
            }

            public string Id
            {
                get
                {
                    if (this.id == null)
                    {
                        lock (this.channel)
                        {
                            if (this.id == null)
                            {
                                this.id = UriGenerator.Next();
                            }
                        }
                    }

                    return this.id;
                }
            }

            public TransportDuplexSessionChannel Channel
            {
                get { return this.channel; }
            }

            static UriGenerator UriGenerator
            {
                get
                {
                    if (uriGenerator == null)
                    {
                        uriGenerator = new UriGenerator();
                    }

                    return uriGenerator;
                }
            }

            public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
            {
                return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginCloseOutputSession(timeout, callback, state);
            }

            public void EndCloseOutputSession(IAsyncResult result)
            {
                this.channel.EndCloseOutputSession(result);
            }

            public void CloseOutputSession()
            {
                this.CloseOutputSession(this.channel.DefaultCloseTimeout);
            }

            public void CloseOutputSession(TimeSpan timeout)
            {
                this.channel.CloseOutputSession(timeout);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static AsyncCallback onCloseOutputSession = Fx.ThunkCallback(new AsyncCallback(OnCloseOutputSession));
            static AsyncCallback onCloseInputSession = Fx.ThunkCallback(new AsyncCallback(OnCloseInputSession));
            static Action<object> onCompleteCloseScheduled;
            TransportDuplexSessionChannel channel;
            TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TransportDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                 : base(callback, state)
            {
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                IAsyncResult result =
                    this.channel.BeginCloseOutputSession(this.timeoutHelper.RemainingTime(), onCloseOutputSession, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (!this.HandleCloseOutputSession(result, true))
                {
                    return;
                }

                this.Complete(true);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            static void OnCloseOutputSession(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.HandleCloseOutputSession(result, false);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnCloseInputSession(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.HandleCloseInputSession(result, false);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnCompleteCloseScheduled(object state)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)state;
                Exception completionException = null;
                try
                {
                    thisPtr.OnCompleteCloseScheduled();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }

            bool HandleCloseOutputSession(IAsyncResult result, bool isStillSynchronous)
            {
                this.channel.EndCloseOutputSession(result);

                if (this.channel.isInputSessionClosed)
                {
                    return this.ScheduleCompleteClose(isStillSynchronous);
                }
                else
                {
                    IAsyncResult closeInputSessionResult =
                        this.channel.messageSource.BeginReceive(this.timeoutHelper.RemainingTime(), onCloseInputSession, this);

                    if (!closeInputSessionResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    return this.HandleCloseInputSession(closeInputSessionResult, isStillSynchronous);
                }
            }

            bool HandleCloseInputSession(IAsyncResult result, bool isStillSynchronous)
            {
                Message message = this.channel.messageSource.EndReceive(result);
                if (message != null)
                {
                    using (message)
                    {
                        ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                        throw TraceUtility.ThrowHelperError(error, message);
                    }
                }

                this.channel.OnInputSessionClosed();
                return this.ScheduleCompleteClose(isStillSynchronous);
            }

            bool ScheduleCompleteClose(bool isStillSynchronous)
            {
                if (isStillSynchronous)
                {
                    if (onCompleteCloseScheduled == null)
                    {
                        onCompleteCloseScheduled = new Action<object>(OnCompleteCloseScheduled);
                    }

                    ActionItem.Schedule(onCompleteCloseScheduled, this);
                    return false;
                }
                else
                {
                    this.OnCompleteCloseScheduled();
                    return true;
                }
            }

            void OnCompleteCloseScheduled()
            {
                this.channel.CompleteClose(this.timeoutHelper.RemainingTime());
            }
        }

        class CloseOutputSessionAsyncResult : AsyncResult
        {
            static WaitCallback onWriteComplete = Fx.ThunkCallback(new WaitCallback(OnWriteComplete));
            static FastAsyncCallback onEnterComplete = new FastAsyncCallback(OnEnterComplete);
            TransportDuplexSessionChannel channel;
            TimeoutHelper timeoutHelper;            

            public CloseOutputSessionAsyncResult(TransportDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                 : base(callback, state)
            {
                channel.ThrowIfNotOpened();
                channel.ThrowIfFaulted();

                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;

                if (!channel.sendLock.EnterAsync(this.timeoutHelper.RemainingTime(), onEnterComplete, this))
                {
                    return;
                }

                bool completeSelf = false;
                bool writeSuccess = false;

                try
                {
                    completeSelf = this.WriteEndBytes();
                    writeSuccess = true;
                }
                finally
                {
                    if (!writeSuccess)
                    {
                        this.Cleanup(false, true);
                    }
                }

                if (completeSelf)
                {
                    this.Cleanup(true, true);
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseOutputSessionAsyncResult>(result);
            }

            static void OnEnterComplete(object state, Exception asyncException)
            {
                CloseOutputSessionAsyncResult thisPtr = (CloseOutputSessionAsyncResult)state;
                bool completeSelf = false;
                Exception completionException = asyncException;
                if (completionException != null)
                {
                    completeSelf = true;
                }
                else
                {
                    try
                    {
                        completeSelf = thisPtr.WriteEndBytes();
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Cleanup(completionException == null, asyncException == null);
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteComplete(object asyncState)
            {
                CloseOutputSessionAsyncResult thisPtr = (CloseOutputSessionAsyncResult)asyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.HandleWriteEndBytesComplete();
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Cleanup(completionException == null, true);
                thisPtr.Complete(false, completionException);
            }

            bool WriteEndBytes()
            {
                // check again in case we faulted while we were waiting for the lock
                this.channel.ThrowIfFaulted();

                // we're synchronized by sendLock here
                if (this.channel.isOutputSessionClosed)
                {
                    return true;
                }

                this.channel.isOutputSessionClosed = true;

                AsyncCompletionResult completionResult = this.channel.BeginCloseOutput(this.timeoutHelper.RemainingTime(), onWriteComplete, this);

                if (completionResult == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                this.HandleWriteEndBytesComplete();
                return true;
            }

            void HandleWriteEndBytesComplete()
            {
                this.channel.FinishWritingMessage();
                this.channel.OnOutputSessionClosed(ref this.timeoutHelper);
            }

            void Cleanup(bool success, bool lockTaken)
            {
                try
                {
                    if (!success)
                    {
                        this.channel.Fault();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.channel.sendLock.Exit();
                    }
                }
            }
        }

        class SendAsyncResult : TraceAsyncResult
        {
            static WaitCallback onWriteComplete = Fx.ThunkCallback(new WaitCallback(OnWriteComplete));
            static FastAsyncCallback onEnterComplete = new FastAsyncCallback(OnEnterComplete);
            TransportDuplexSessionChannel channel;
            Message message;
            byte[] buffer;
            TimeoutHelper timeoutHelper;
            bool streamedOutput;
            EventTraceActivity eventTraceActivity;

            public SendAsyncResult(TransportDuplexSessionChannel channel, Message message, TimeSpan timeout, bool streamedOutput, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
                this.message = message;
                this.streamedOutput = streamedOutput;

                if (!channel.sendLock.EnterAsync(this.timeoutHelper.RemainingTime(), onEnterComplete, this))
                {
                    return;
                }

                bool completeSelf = false;
                bool writeSuccess = false;

                try
                {
                    completeSelf = this.WriteCore();
                    writeSuccess = true;
                }
                finally
                {
                    if (!writeSuccess)
                    {
                        this.Cleanup(false, true);
                    }
                }

                if (completeSelf)
                {
                    this.Cleanup(true, true);
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                if (TD.MessageSentByTransportIsEnabled())
                {
                    SendAsyncResult thisPtr = result as SendAsyncResult;
                    if (thisPtr != null)
                    {
                        TD.MessageSentByTransport(thisPtr.eventTraceActivity, thisPtr.channel.RemoteAddress.Uri.AbsoluteUri);
                    }
                }

                AsyncResult.End<SendAsyncResult>(result);
            }

            static void OnEnterComplete(object state, Exception asyncException)
            {
                SendAsyncResult thisPtr = (SendAsyncResult)state;
                bool completeSelf = false;
                Exception completionException = asyncException;
                if (completionException != null)
                {
                    completeSelf = true;
                }
                else
                {
                    try
                    {
                        completeSelf = thisPtr.WriteCore();
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Cleanup(completionException == null, asyncException == null);
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteComplete(object asyncState)
            {
                SendAsyncResult thisPtr = (SendAsyncResult)asyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.channel.FinishWritingMessage();
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Cleanup(completionException == null, true);
                thisPtr.Complete(false, completionException);
            }

            bool WriteCore()
            {
                // check again in case the previous send faulted while we were waiting for the lock
                this.channel.ThrowIfDisposedOrNotOpen();
                this.channel.ThrowIfOutputSessionClosed();

                this.channel.ApplyChannelBinding(this.message);
                
                Message message = this.message;
                this.message = null;

                // Because we nullify the message, we need to save its trace activity, for logging events later on.
                if (TD.MessageSentByTransportIsEnabled())
                {
                    this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                }

                AsyncCompletionResult completionResult;
                if (this.streamedOutput)
                {
                    completionResult = this.channel.StartWritingStreamedMessage(message, this.timeoutHelper.RemainingTime(), onWriteComplete, this);
                }
                else
                {
                    bool allowOutputBatching;
                    ArraySegment<byte> messageData;
                    allowOutputBatching = message.Properties.AllowOutputBatching;
                    messageData = this.channel.EncodeMessage(message);

                    this.buffer = messageData.Array;
                    completionResult = this.channel.StartWritingBufferedMessage(
                                                                          message,
                                                                          messageData,
                                                                          allowOutputBatching,
                                                                          this.timeoutHelper.RemainingTime(),
                                                                          onWriteComplete,
                                                                          this);
                }

                if (completionResult == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                this.channel.FinishWritingMessage();
                return true;
            }

            void Cleanup(bool success, bool lockTaken)
            {
                try
                {
                    if (!success)
                    {
                        this.channel.Fault();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.channel.sendLock.Exit();
                    }
                }

                if (this.buffer != null)
                {
                    this.channel.bufferManager.ReturnBuffer(this.buffer);
                    this.buffer = null;
                }
            }
        }

        class TryReceiveAsyncResult : AsyncResult
        {
            static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
            TransportDuplexSessionChannel channel;
            bool receiveSuccess;
            Message message;

            public TryReceiveAsyncResult(TransportDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;

                bool completeSelf = false;
                try
                {
                    IAsyncResult result = this.channel.BeginReceive(timeout, onReceive, this);
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteReceive(result);
                        completeSelf = true;
                    }
                }
                catch (TimeoutException e)
                {
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(e.Message);
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                    completeSelf = true;
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                TryReceiveAsyncResult thisPtr = AsyncResult.End<TryReceiveAsyncResult>(result);
                message = thisPtr.message;
                return thisPtr.receiveSuccess;
            }
            
            static void OnReceive(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                TryReceiveAsyncResult thisPtr = (TryReceiveAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.CompleteReceive(result);
                }
                catch (TimeoutException e)
                {
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(e.Message);
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }

            void CompleteReceive(IAsyncResult result)
            {
                this.message = this.channel.EndReceive(result);
                this.receiveSuccess = true;
            }
        }
    }
}
