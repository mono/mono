//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    enum TolerateFaultsMode
    {
        Never,
        IfNotSecuritySession,
        Always
    }

    [Flags]
    enum MaskingMode
    {
        None = 0x0,
        Handled = 0x1,
        Unhandled = 0x2,
        All = Handled | Unhandled
    }

    abstract class ReliableChannelBinder<TChannel> : IReliableChannelBinder
        where TChannel : class, IChannel
    {
        bool aborted = false;
        TimeSpan defaultCloseTimeout;
        MaskingMode defaultMaskingMode;
        TimeSpan defaultSendTimeout;
        AsyncCallback onCloseChannelComplete;
        CommunicationState state = CommunicationState.Created;
        ChannelSynchronizer synchronizer;
        object thisLock = new object();

        protected ReliableChannelBinder(TChannel channel, MaskingMode maskingMode,
            TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout,
            TimeSpan defaultSendTimeout)
        {
            if ((maskingMode != MaskingMode.None) && (maskingMode != MaskingMode.All))
            {
                throw Fx.AssertAndThrow("ReliableChannelBinder was implemented with only 2 default masking modes, None and All.");
            }

            this.defaultMaskingMode = maskingMode;
            this.defaultCloseTimeout = defaultCloseTimeout;
            this.defaultSendTimeout = defaultSendTimeout;

            this.synchronizer = new ChannelSynchronizer(this, channel, faultMode);
        }

        protected abstract bool CanGetChannelForReceive
        {
            get;
        }

        public abstract bool CanSendAsynchronously
        {
            get;
        }

        public virtual ChannelParameterCollection ChannelParameters
        {
            get { return null; }
        }

        public IChannel Channel
        {
            get
            {
                return this.synchronizer.CurrentChannel;
            }
        }

        public bool Connected
        {
            get
            {
                return this.synchronizer.Connected;
            }
        }

        public MaskingMode DefaultMaskingMode
        {
            get
            {
                return this.defaultMaskingMode;
            }
        }

        public TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.defaultSendTimeout;
            }
        }

        public abstract bool HasSession
        {
            get;
        }

        public abstract EndpointAddress LocalAddress
        {
            get;
        }

        protected abstract bool MustCloseChannel
        {
            get;
        }

        protected abstract bool MustOpenChannel
        {
            get;
        }

        public abstract EndpointAddress RemoteAddress
        {
            get;
        }

        public CommunicationState State
        {
            get
            {
                return this.state;
            }
        }

        protected ChannelSynchronizer Synchronizer
        {
            get
            {
                return this.synchronizer;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        bool TolerateFaults
        {
            get
            {
                return this.synchronizer.TolerateFaults;
            }
        }

        public event EventHandler ConnectionLost;
        public event BinderExceptionHandler Faulted;
        public event BinderExceptionHandler OnException;


        public void Abort()
        {
            TChannel channel;
            lock (this.ThisLock)
            {
                this.aborted = true;

                if (this.state == CommunicationState.Closed)
                {
                    return;
                }

                this.state = CommunicationState.Closing;
                channel = this.synchronizer.StopSynchronizing(true);

                if (!this.MustCloseChannel)
                {
                    channel = null;
                }
            }

            this.synchronizer.UnblockWaiters();
            this.OnShutdown();
            this.OnAbort();

            if (channel != null)
            {
                channel.Abort();
            }

            this.TransitionToClosed();
        }

        protected virtual void AddOutputHeaders(Message message)
        {
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.BeginClose(timeout, this.defaultMaskingMode, callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, MaskingMode maskingMode,
            AsyncCallback callback, object state)
        {
            this.ThrowIfTimeoutNegative(timeout);
            TChannel channel;

            if (this.CloseCore(out channel))
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                return new CloseAsyncResult(this, channel, timeout, maskingMode, callback, state);
            }
        }

        protected virtual IAsyncResult BeginCloseChannel(TChannel channel, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return channel.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfTimeoutNegative(timeout);

            if (this.OnOpening(this.defaultMaskingMode))
            {
                try
                {
                    return this.OnBeginOpen(timeout, callback, state);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.Fault(null);

                    if (this.defaultMaskingMode == MaskingMode.None)
                    {
                        throw;
                    }
                    else
                    {
                        this.RaiseOnException(e);
                    }
                }
            }

            return new BinderCompletedAsyncResult(callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.BeginSend(message, timeout, this.defaultMaskingMode, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, MaskingMode maskingMode,
            AsyncCallback callback, object state)
        {
            SendAsyncResult result = new SendAsyncResult(this, callback, state);
            result.Start(message, timeout, maskingMode);
            return result;
        }

        // ChannelSynchronizer helper, cannot take a lock.
        protected abstract IAsyncResult BeginTryGetChannel(TimeSpan timeout,
            AsyncCallback callback, object state);

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.BeginTryReceive(timeout, this.defaultMaskingMode, callback, state);
        }

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, MaskingMode maskingMode,
            AsyncCallback callback, object state)
        {
            if (this.ValidateInputOperation(timeout))
                return new TryReceiveAsyncResult(this, timeout, maskingMode, callback, state);
            else
                return new CompletedAsyncResult(callback, state);
        }

        internal IAsyncResult BeginWaitForPendingOperations(TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return this.synchronizer.BeginWaitForPendingOperations(timeout, callback, state);
        }

        bool CloseCore(out TChannel channel)
        {
            channel = null;
            bool abort = true;
            bool abortChannel = false;

            lock (this.ThisLock)
            {
                if ((this.state == CommunicationState.Closing)
                    || (this.state == CommunicationState.Closed))
                {
                    return true;
                }

                if (this.state == CommunicationState.Opened)
                {
                    this.state = CommunicationState.Closing;
                    channel = this.synchronizer.StopSynchronizing(true);
                    abort = false;

                    if (!this.MustCloseChannel)
                    {
                        channel = null;
                    }

                    if (channel != null)
                    {
                        CommunicationState channelState = channel.State;

                        if ((channelState == CommunicationState.Created)
                            || (channelState == CommunicationState.Opening)
                            || (channelState == CommunicationState.Faulted))
                        {
                            abortChannel = true;
                        }
                        else if ((channelState == CommunicationState.Closing)
                            || (channelState == CommunicationState.Closed))
                        {
                            channel = null;
                        }
                    }
                }
            }

            this.synchronizer.UnblockWaiters();

            if (abort)
            {
                this.Abort();
                return true;
            }
            else
            {
                if (abortChannel)
                {
                    channel.Abort();
                    channel = null;
                }

                return false;
            }
        }

        public void Close(TimeSpan timeout)
        {
            this.Close(timeout, this.defaultMaskingMode);
        }

        public void Close(TimeSpan timeout, MaskingMode maskingMode)
        {
            this.ThrowIfTimeoutNegative(timeout);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            TChannel channel;

            if (this.CloseCore(out channel))
            {
                return;
            }

            try
            {
                this.OnShutdown();
                this.OnClose(timeoutHelper.RemainingTime());

                if (channel != null)
                {
                    this.CloseChannel(channel, timeoutHelper.RemainingTime());
                }

                this.TransitionToClosed();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.Abort();

                if (!this.HandleException(e, maskingMode))
                {
                    throw;
                }
            }
        }

        // The ChannelSynchronizer calls this from an operation thread so this method must not
        // block.
        void CloseChannel(TChannel channel)
        {
            if (!this.MustCloseChannel)
            {
                throw Fx.AssertAndThrow("MustCloseChannel is false when there is no receive loop and this method is called when there is a receive loop.");
            }

            if (this.onCloseChannelComplete == null)
            {
                this.onCloseChannelComplete = Fx.ThunkCallback(new AsyncCallback(this.OnCloseChannelComplete));
            }

            try
            {
                IAsyncResult result = channel.BeginClose(onCloseChannelComplete, channel);

                if (result.CompletedSynchronously)
                {
                    channel.EndClose(result);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.HandleException(e, MaskingMode.All);
            }
        }

        protected virtual void CloseChannel(TChannel channel, TimeSpan timeout)
        {
            channel.Close(timeout);
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult closeResult = result as CloseAsyncResult;

            if (closeResult != null)
            {
                closeResult.End();
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected virtual void EndCloseChannel(TChannel channel, IAsyncResult result)
        {
            channel.EndClose(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            BinderCompletedAsyncResult completedResult = result as BinderCompletedAsyncResult;

            if (completedResult != null)
            {
                completedResult.End();
            }
            else
            {
                try
                {
                    this.OnEndOpen(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.Fault(null);

                    if (this.defaultMaskingMode == MaskingMode.None)
                    {
                        throw;
                    }
                    else
                    {
                        this.RaiseOnException(e);
                        return;
                    }
                }

                this.synchronizer.StartSynchronizing();
                this.OnOpened();
            }
        }

        public void EndSend(IAsyncResult result)
        {
            SendAsyncResult.End(result);
        }

        // ChannelSynchronizer helper, cannot take a lock.
        protected abstract bool EndTryGetChannel(IAsyncResult result);

        public virtual bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            TryReceiveAsyncResult tryReceiveResult = result as TryReceiveAsyncResult;

            if (tryReceiveResult != null)
            {
                return tryReceiveResult.End(out requestContext);
            }
            else
            {
                CompletedAsyncResult.End(result);
                requestContext = null;
                return true;
            }
        }

        public void EndWaitForPendingOperations(IAsyncResult result)
        {
            this.synchronizer.EndWaitForPendingOperations(result);
        }

        protected void Fault(Exception e)
        {
            lock (this.ThisLock)
            {
                if (this.state == CommunicationState.Created)
                {
                    throw Fx.AssertAndThrow("The binder should not detect the inner channel's faults until after the binder is opened.");
                }

                if ((this.state == CommunicationState.Faulted)
                    || (this.state == CommunicationState.Closed))
                {
                    return;
                }

                this.state = CommunicationState.Faulted;
                this.synchronizer.StopSynchronizing(false);
            }

            this.synchronizer.UnblockWaiters();

            BinderExceptionHandler handler = this.Faulted;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        // ChannelSynchronizer helper, cannot take a lock.
        Exception GetClosedException(MaskingMode maskingMode)
        {
            if (ReliableChannelBinderHelper.MaskHandled(maskingMode))
            {
                return null;
            }
            else if (this.aborted)
            {
                return new CommunicationObjectAbortedException(SR.GetString(
                    SR.CommunicationObjectAborted1, this.GetType().ToString()));
            }
            else
            {
                return new ObjectDisposedException(this.GetType().ToString());
            }
        }

        // Must be called within lock (this.ThisLock)
        Exception GetClosedOrFaultedException(MaskingMode maskingMode)
        {
            if (this.state == CommunicationState.Faulted)
            {
                return this.GetFaultedException(maskingMode);
            }
            else if ((this.state == CommunicationState.Closing)
               || (this.state == CommunicationState.Closed))
            {
                return this.GetClosedException(maskingMode);
            }
            else
            {
                throw Fx.AssertAndThrow("Caller is attempting to get a terminal exception in a non-terminal state.");
            }
        }

        // ChannelSynchronizer helper, cannot take a lock.
        Exception GetFaultedException(MaskingMode maskingMode)
        {
            if (ReliableChannelBinderHelper.MaskHandled(maskingMode))
            {
                return null;
            }
            else
            {
                return new CommunicationObjectFaultedException(SR.GetString(
                    SR.CommunicationObjectFaulted1, this.GetType().ToString()));
            }
        }

        public abstract ISession GetInnerSession();

        public void HandleException(Exception e)
        {
            this.HandleException(e, MaskingMode.All);
        }

        protected bool HandleException(Exception e, MaskingMode maskingMode)
        {
            if (this.TolerateFaults && (e is CommunicationObjectFaultedException))
            {
                return true;
            }

            if (this.IsHandleable(e))
            {
                return ReliableChannelBinderHelper.MaskHandled(maskingMode);
            }

            bool maskUnhandled = ReliableChannelBinderHelper.MaskUnhandled(maskingMode);

            if (maskUnhandled)
            {
                this.RaiseOnException(e);
            }

            return maskUnhandled;
        }

        protected bool HandleException(Exception e, MaskingMode maskingMode, bool autoAborted)
        {
            if (this.TolerateFaults && autoAborted && e is CommunicationObjectAbortedException)
            {
                return true;
            }

            return this.HandleException(e, maskingMode);
        }

        // ChannelSynchronizer helper, cannot take a lock.
        protected abstract bool HasSecuritySession(TChannel channel);

        public bool IsHandleable(Exception e)
        {
            if (e is ProtocolException)
            {
                return false;
            }

            return (e is CommunicationException)
                || (e is TimeoutException);
        }

        protected abstract void OnAbort();
        protected abstract IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback,
            object state);
        protected abstract IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback,
            object state);

        protected virtual IAsyncResult OnBeginSend(TChannel channel, Message message,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("The derived class does not support the BeginSend operation.");
        }

        protected virtual IAsyncResult OnBeginTryReceive(TChannel channel, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("The derived class does not support the BeginTryReceive operation.");
        }

        protected abstract void OnClose(TimeSpan timeout);

        void OnCloseChannelComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            TChannel channel = (TChannel)result.AsyncState;

            try
            {
                channel.EndClose(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.HandleException(e, MaskingMode.All);
            }
        }

        protected abstract void OnEndClose(IAsyncResult result);
        protected abstract void OnEndOpen(IAsyncResult result);

        protected virtual void OnEndSend(TChannel channel, IAsyncResult result)
        {
            throw Fx.AssertAndThrow("The derived class does not support the EndSend operation.");
        }

        protected virtual bool OnEndTryReceive(TChannel channel, IAsyncResult result,
            out RequestContext requestContext)
        {
            throw Fx.AssertAndThrow("The derived class does not support the EndTryReceive operation.");
        }

        void OnInnerChannelFaulted()
        {
            if (!this.TolerateFaults)
                return;

            EventHandler handler = this.ConnectionLost;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected abstract void OnOpen(TimeSpan timeout);

        void OnOpened()
        {
            lock (this.ThisLock)
            {
                if (this.state == CommunicationState.Opening)
                {
                    this.state = CommunicationState.Opened;
                }
            }
        }

        bool OnOpening(MaskingMode maskingMode)
        {
            lock (this.ThisLock)
            {
                if (this.state != CommunicationState.Created)
                {
                    Exception e = null;

                    if ((this.state == CommunicationState.Opening)
                        || (this.state == CommunicationState.Opened))
                    {
                        if (!ReliableChannelBinderHelper.MaskUnhandled(maskingMode))
                        {
                            e = new InvalidOperationException(SR.GetString(
                                SR.CommunicationObjectCannotBeModifiedInState,
                                this.GetType().ToString(), this.state.ToString()));
                        }
                    }
                    else
                    {
                        e = this.GetClosedOrFaultedException(maskingMode);
                    }

                    if (e != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                    }

                    return false;
                }
                else
                {
                    this.state = CommunicationState.Opening;
                    return true;
                }
            }
        }

        protected virtual void OnShutdown()
        {
        }

        protected virtual void OnSend(TChannel channel, Message message, TimeSpan timeout)
        {
            throw Fx.AssertAndThrow("The derived class does not support the Send operation.");
        }

        protected virtual bool OnTryReceive(TChannel channel, TimeSpan timeout,
            out RequestContext requestContext)
        {
            throw Fx.AssertAndThrow("The derived class does not support the TryReceive operation.");
        }

        public void Open(TimeSpan timeout)
        {
            this.ThrowIfTimeoutNegative(timeout);

            if (!this.OnOpening(this.defaultMaskingMode))
            {
                return;
            }

            try
            {
                this.OnOpen(timeout);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.Fault(null);

                if (this.defaultMaskingMode == MaskingMode.None)
                {
                    throw;
                }
                else
                {
                    this.RaiseOnException(e);
                    return;
                }
            }

            this.synchronizer.StartSynchronizing();
            this.OnOpened();
        }

        void RaiseOnException(Exception e)
        {
            BinderExceptionHandler handler = this.OnException;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.Send(message, timeout, this.defaultMaskingMode);
        }

        public void Send(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            if (!this.ValidateOutputOperation(message, timeout, maskingMode))
            {
                return;
            }

            bool autoAborted = false;

            try
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                TChannel channel;

                if (!this.synchronizer.TryGetChannelForOutput(timeoutHelper.RemainingTime(), maskingMode,
                    out channel))
                {
                    if (!ReliableChannelBinderHelper.MaskHandled(maskingMode))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new TimeoutException(SR.GetString(SR.TimeoutOnSend, timeout)));
                    }

                    return;
                }

                if (channel == null)
                {
                    return;
                }

                this.AddOutputHeaders(message);

                try
                {
                    this.OnSend(channel, message, timeoutHelper.RemainingTime());
                }
                finally
                {
                    autoAborted = this.Synchronizer.Aborting;
                    this.synchronizer.ReturnChannel();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (!this.HandleException(e, maskingMode, autoAborted))
                {
                    throw;
                }
            }
        }

        public void SetMaskingMode(RequestContext context, MaskingMode maskingMode)
        {
            BinderRequestContext binderContext = (BinderRequestContext)context;
            binderContext.SetMaskingMode(maskingMode);
        }

        // throwDisposed indicates whether to throw in the Faulted, Closing, and Closed states.
        // returns true if in Opened state
        bool ThrowIfNotOpenedAndNotMasking(MaskingMode maskingMode, bool throwDisposed)
        {
            lock (this.ThisLock)
            {
                if (this.State == CommunicationState.Created)
                {
                    throw Fx.AssertAndThrow("Messaging operations cannot be called when the binder is in the Created state.");
                }

                if (this.State == CommunicationState.Opening)
                {
                    throw Fx.AssertAndThrow("Messaging operations cannot be called when the binder is in the Opening state.");
                }

                if (this.State == CommunicationState.Opened)
                {
                    return true;
                }

                // state is Faulted, Closing, or Closed
                if (throwDisposed)
                {
                    Exception e = this.GetClosedOrFaultedException(maskingMode);

                    if (e != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                    }
                }

                return false;
            }
        }

        void ThrowIfTimeoutNegative(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.SFxTimeoutOutOfRange0));
            }
        }

        void TransitionToClosed()
        {
            lock (this.ThisLock)
            {
                if ((this.state != CommunicationState.Closing)
                    && (this.state != CommunicationState.Closed)
                    && (this.state != CommunicationState.Faulted))
                {
                    throw Fx.AssertAndThrow("Caller cannot transition to the Closed state from a non-terminal state.");
                }

                this.state = CommunicationState.Closed;
            }
        }

        // ChannelSynchronizer helper, cannot take a lock.
        protected abstract bool TryGetChannel(TimeSpan timeout);

        public virtual bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.TryReceive(timeout, out requestContext, this.defaultMaskingMode);
        }

        public virtual bool TryReceive(TimeSpan timeout, out RequestContext requestContext, MaskingMode maskingMode)
        {
            if (maskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }

            if (!this.ValidateInputOperation(timeout))
            {
                requestContext = null;
                return true;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            while (true)
            {
                bool autoAborted = false;

                try
                {
                    TChannel channel;
                    bool success = !this.synchronizer.TryGetChannelForInput(
                        this.CanGetChannelForReceive, timeoutHelper.RemainingTime(), out channel);

                    if (channel == null)
                    {
                        requestContext = null;
                        return success;
                    }

                    try
                    {
                        success = this.OnTryReceive(channel, timeoutHelper.RemainingTime(),
                            out requestContext);

                        // timed out || got message, return immediately
                        if (!success || (requestContext != null))
                        {
                            return success;
                        }

                        // the underlying channel closed or faulted, retry
                        this.synchronizer.OnReadEof();
                    }
                    finally
                    {
                        autoAborted = this.Synchronizer.Aborting;
                        this.synchronizer.ReturnChannel();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.HandleException(e, maskingMode, autoAborted))
                    {
                        throw;
                    }
                }
            }
        }

        protected bool ValidateInputOperation(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout,
                    SR.SFxTimeoutOutOfRange0));
            }

            return this.ThrowIfNotOpenedAndNotMasking(MaskingMode.All, false);
        }

        protected bool ValidateOutputOperation(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout,
                    SR.SFxTimeoutOutOfRange0));
            }

            return this.ThrowIfNotOpenedAndNotMasking(maskingMode, true);
        }

        internal void WaitForPendingOperations(TimeSpan timeout)
        {
            this.synchronizer.WaitForPendingOperations(timeout);
        }

        protected RequestContext WrapMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }

            return new MessageRequestContext(this, message);
        }

        public RequestContext WrapRequestContext(RequestContext context)
        {
            if (context == null)
            {
                return null;
            }

            if (!this.TolerateFaults && this.defaultMaskingMode == MaskingMode.None)
            {
                return context;
            }

            return new RequestRequestContext(this, context, context.RequestMessage);
        }

        sealed class BinderCompletedAsyncResult : CompletedAsyncResult
        {
            public BinderCompletedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public void End()
            {
                CompletedAsyncResult.End(this);
            }
        }

        abstract class BinderRequestContext : RequestContextBase
        {
            ReliableChannelBinder<TChannel> binder;
            MaskingMode maskingMode;

            public BinderRequestContext(ReliableChannelBinder<TChannel> binder, Message message)
                : base(message, binder.defaultCloseTimeout, binder.defaultSendTimeout)
            {
                if (binder == null)
                {
                    Fx.Assert("Argument binder cannot be null.");
                }

                this.binder = binder;
                this.maskingMode = binder.defaultMaskingMode;
            }

            protected ReliableChannelBinder<TChannel> Binder
            {
                get
                {
                    return this.binder;
                }
            }

            protected MaskingMode MaskingMode
            {
                get
                {
                    return this.maskingMode;
                }
            }

            public void SetMaskingMode(MaskingMode maskingMode)
            {
                if (this.binder.defaultMaskingMode != MaskingMode.All)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                this.maskingMode = maskingMode;
            }
        }

        protected class ChannelSynchronizer
        {
            bool aborting; // Indicates the current channel is being aborted, not the synchronizer.
            ReliableChannelBinder<TChannel> binder;
            int count = 0;
            TChannel currentChannel;
            InterruptibleWaitObject drainEvent;
            static Action<object> asyncGetChannelCallback = new Action<object>(AsyncGetChannelCallback);
            TolerateFaultsMode faultMode;
            Queue<IWaiter> getChannelQueue;
            bool innerChannelFaulted;
            EventHandler onChannelFaulted;
            State state = State.Created;
            bool tolerateFaults = true;
            object thisLock = new object();
            Queue<IWaiter> waitQueue;

            public ChannelSynchronizer(ReliableChannelBinder<TChannel> binder, TChannel channel,
                TolerateFaultsMode faultMode)
            {
                this.binder = binder;
                this.currentChannel = channel;
                this.faultMode = faultMode;
            }

            public bool Aborting
            {
                get
                {
                    return this.aborting;
                }
            }

            public bool Connected
            {
                get
                {
                    return (this.state == State.ChannelOpened ||
                        this.state == State.ChannelOpening);
                }
            }

            public TChannel CurrentChannel
            {
                get
                {
                    return this.currentChannel;
                }
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            public bool TolerateFaults
            {
                get
                {
                    return this.tolerateFaults;
                }
            }

            // Server only API.
            public TChannel AbortCurentChannel()
            {
                lock (this.ThisLock)
                {
                    if (!this.tolerateFaults)
                    {
                        throw Fx.AssertAndThrow("It is only valid to abort the current channel when masking faults");
                    }

                    if (this.state == State.ChannelOpening)
                    {
                        this.aborting = true;
                    }
                    else if (this.state == State.ChannelOpened)
                    {
                        if (this.count == 0)
                        {
                            this.state = State.NoChannel;
                        }
                        else
                        {
                            this.aborting = true;
                            this.state = State.ChannelClosing;
                        }
                    }
                    else
                    {
                        return null;
                    }

                    return this.currentChannel;
                }
            }

            static void AsyncGetChannelCallback(object state)
            {
                AsyncWaiter waiter = (AsyncWaiter)state;
                waiter.GetChannel(false);
            }

            public IAsyncResult BeginTryGetChannelForInput(bool canGetChannel, TimeSpan timeout,
                AsyncCallback callback, object state)
            {
                return this.BeginTryGetChannel(canGetChannel, false, timeout, MaskingMode.All,
                    callback, state);
            }

            public IAsyncResult BeginTryGetChannelForOutput(TimeSpan timeout,
                MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                return this.BeginTryGetChannel(true, true, timeout, maskingMode,
                    callback, state);
            }

            IAsyncResult BeginTryGetChannel(bool canGetChannel, bool canCauseFault,
                TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                TChannel channel = null;
                AsyncWaiter waiter = null;
                bool getChannel = false;
                bool faulted = false;

                lock (this.ThisLock)
                {
                    if (!this.ThrowIfNecessary(maskingMode))
                    {
                        channel = null;
                    }
                    else if (this.state == State.ChannelOpened)
                    {
                        if (this.currentChannel == null)
                        {
                            throw Fx.AssertAndThrow("Field currentChannel cannot be null in the ChannelOpened state.");
                        }

                        this.count++;
                        channel = this.currentChannel;
                    }
                    else if (!this.tolerateFaults
                        && ((this.state == State.NoChannel)
                        || (this.state == State.ChannelClosing)))
                    {
                        if (canCauseFault)
                        {
                            faulted = true;
                        }

                        channel = null;
                    }
                    else if (!canGetChannel
                        || (this.state == State.ChannelOpening)
                        || (this.state == State.ChannelClosing))
                    {
                        waiter = new AsyncWaiter(this, canGetChannel, null, timeout, maskingMode,
                            this.binder.ChannelParameters,
                            callback, state);
                        this.GetQueue(canGetChannel).Enqueue(waiter);
                    }
                    else
                    {
                        if (this.state != State.NoChannel)
                        {
                            throw Fx.AssertAndThrow("The state must be NoChannel.");
                        }

                        waiter = new AsyncWaiter(this, canGetChannel,
                            this.GetCurrentChannelIfCreated(), timeout, maskingMode,
                            this.binder.ChannelParameters,
                            callback, state);

                        this.state = State.ChannelOpening;
                        getChannel = true;
                    }
                }

                if (faulted)
                {
                    this.binder.Fault(null);
                }

                if (waiter == null)
                {
                    return new CompletedAsyncResult<TChannel>(channel, callback, state);
                }

                if (getChannel)
                {
                    waiter.GetChannel(true);
                }
                else
                {
                    waiter.Wait();
                }

                return waiter;
            }

            public IAsyncResult BeginWaitForPendingOperations(TimeSpan timeout,
                AsyncCallback callback, object state)
            {
                lock (this.ThisLock)
                {
                    if (this.drainEvent != null)
                    {
                        throw Fx.AssertAndThrow("The WaitForPendingOperations operation may only be invoked once.");
                    }

                    if (this.count > 0)
                    {
                        this.drainEvent = new InterruptibleWaitObject(false, false);
                    }
                }

                if (this.drainEvent != null)
                {
                    return this.drainEvent.BeginWait(timeout, callback, state);
                }
                else
                {
                    return new SynchronizerCompletedAsyncResult(callback, state);
                }
            }

            bool CompleteSetChannel(IWaiter waiter, out TChannel channel)
            {
                if (waiter == null)
                {
                    throw Fx.AssertAndThrow("Argument waiter cannot be null.");
                }

                bool close = false;

                lock (this.ThisLock)
                {
                    if (this.ValidateOpened())
                    {
                        channel = this.currentChannel;
                        return true;
                    }
                    else
                    {
                        channel = null;
                        close = this.state == State.Closed;
                    }
                }

                if (close)
                {
                    waiter.Close();
                }
                else
                {
                    waiter.Fault();
                }

                return false;
            }

            public bool EndTryGetChannel(IAsyncResult result, out TChannel channel)
            {
                AsyncWaiter waiter = result as AsyncWaiter;

                if (waiter != null)
                {
                    return waiter.End(out channel);
                }
                else
                {
                    channel = CompletedAsyncResult<TChannel>.End(result);
                    return true;
                }
            }

            public void EndWaitForPendingOperations(IAsyncResult result)
            {
                SynchronizerCompletedAsyncResult completedResult =
                    result as SynchronizerCompletedAsyncResult;

                if (completedResult != null)
                {
                    completedResult.End();
                }
                else
                {
                    this.drainEvent.EndWait(result);
                }
            }

            // Client API only.
            public bool EnsureChannel()
            {
                bool fault = false;

                lock (this.ThisLock)
                {
                    if (this.ValidateOpened())
                    {
                        // This is called only during the RM CS phase. In this phase, there are 2
                        // valid states between Request calls, ChannelOpened and NoChannel.
                        if (this.state == State.ChannelOpened)
                        {
                            return true;
                        }

                        if (this.state != State.NoChannel)
                        {
                            throw Fx.AssertAndThrow("The caller may only invoke this EnsureChannel during the CreateSequence negotiation. ChannelOpening and ChannelClosing are invalid states during this phase of the negotiation.");
                        }

                        if (!this.tolerateFaults)
                        {
                            fault = true;
                        }
                        else
                        {
                            if (this.GetCurrentChannelIfCreated() != null)
                            {
                                return true;
                            }

                            if (this.binder.TryGetChannel(TimeSpan.Zero))
                            {
                                if (this.currentChannel == null)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }

                if (fault)
                {
                    this.binder.Fault(null);
                }

                return false;
            }

            IWaiter GetChannelWaiter()
            {
                if ((this.getChannelQueue == null) || (this.getChannelQueue.Count == 0))
                {
                    return null;
                }

                return this.getChannelQueue.Dequeue();
            }

            // Must be called within lock (this.ThisLock)
            TChannel GetCurrentChannelIfCreated()
            {
                if (this.state != State.NoChannel)
                {
                    throw Fx.AssertAndThrow("This method may only be called in the NoChannel state.");
                }

                if ((this.currentChannel != null)
                    && (this.currentChannel.State == CommunicationState.Created))
                {
                    return this.currentChannel;
                }
                else
                {
                    return null;
                }
            }

            Queue<IWaiter> GetQueue(bool canGetChannel)
            {
                if (canGetChannel)
                {
                    if (this.getChannelQueue == null)
                    {
                        this.getChannelQueue = new Queue<IWaiter>();
                    }

                    return this.getChannelQueue;
                }
                else
                {
                    if (this.waitQueue == null)
                    {
                        this.waitQueue = new Queue<IWaiter>();
                    }

                    return this.waitQueue;
                }
            }

            void OnChannelFaulted(object sender, EventArgs e)
            {
                TChannel faultedChannel = (TChannel)sender;
                bool faultBinder = false;
                bool raiseInnerChannelFaulted = false;

                lock (this.ThisLock)
                {
                    if (this.currentChannel != faultedChannel)
                    {
                        return;
                    }

                    // The synchronizer is already closed or aborted.
                    if (!this.ValidateOpened())
                    {
                        return;
                    }

                    if (this.state == State.ChannelOpened)
                    {
                        if (this.count == 0)
                        {
                            faultedChannel.Faulted -= this.onChannelFaulted;
                        }

                        faultBinder = !this.tolerateFaults;
                        this.state = State.ChannelClosing;
                        this.innerChannelFaulted = true;

                        if (!faultBinder && this.count == 0)
                        {
                            this.state = State.NoChannel;
                            this.aborting = false;
                            raiseInnerChannelFaulted = true;
                            this.innerChannelFaulted = false;
                        }
                    }
                }

                if (faultBinder)
                {
                    this.binder.Fault(null);
                }

                faultedChannel.Abort();

                if (raiseInnerChannelFaulted)
                {
                    this.binder.OnInnerChannelFaulted();
                }
            }

            bool OnChannelOpened(IWaiter waiter)
            {
                if (waiter == null)
                {
                    throw Fx.AssertAndThrow("Argument waiter cannot be null.");
                }

                bool close = false;
                bool fault = false;

                Queue<IWaiter> temp1 = null;
                Queue<IWaiter> temp2 = null;
                TChannel channel = null;

                lock (this.ThisLock)
                {
                    if (this.currentChannel == null)
                    {
                        throw Fx.AssertAndThrow("Caller must ensure that field currentChannel is set before opening the channel.");
                    }

                    if (this.ValidateOpened())
                    {
                        if (this.state != State.ChannelOpening)
                        {
                            throw Fx.AssertAndThrow("This method may only be called in the ChannelOpening state.");
                        }

                        this.state = State.ChannelOpened;
                        this.SetTolerateFaults();

                        this.count += 1;
                        this.count += (this.getChannelQueue == null) ? 0 : this.getChannelQueue.Count;
                        this.count += (this.waitQueue == null) ? 0 : this.waitQueue.Count;

                        temp1 = this.getChannelQueue;
                        temp2 = this.waitQueue;
                        channel = this.currentChannel;

                        this.getChannelQueue = null;
                        this.waitQueue = null;
                    }
                    else
                    {
                        close = this.state == State.Closed;
                        fault = this.state == State.Faulted;
                    }
                }

                if (close)
                {
                    waiter.Close();
                    return false;
                }
                else if (fault)
                {
                    waiter.Fault();
                    return false;
                }

                this.SetWaiters(temp1, channel);
                this.SetWaiters(temp2, channel);
                return true;
            }

            void OnGetChannelFailed()
            {
                IWaiter waiter = null;

                lock (this.ThisLock)
                {
                    if (!this.ValidateOpened())
                    {
                        return;
                    }

                    if (this.state != State.ChannelOpening)
                    {
                        throw Fx.AssertAndThrow("The state must be set to ChannelOpening before the caller attempts to open the channel.");
                    }

                    waiter = this.GetChannelWaiter();

                    if (waiter == null)
                    {
                        this.state = State.NoChannel;
                        return;
                    }
                }

                if (waiter is SyncWaiter)
                {
                    waiter.GetChannel(false);
                }
                else
                {
                    ActionItem.Schedule(asyncGetChannelCallback, waiter);
                }
            }

            public void OnReadEof()
            {
                lock (this.ThisLock)
                {
                    if (this.count <= 0)
                    {
                        throw Fx.AssertAndThrow("Caller must ensure that OnReadEof is called before ReturnChannel.");
                    }

                    if (this.ValidateOpened())
                    {
                        if ((this.state != State.ChannelOpened) && (this.state != State.ChannelClosing))
                        {
                            throw Fx.AssertAndThrow("Since count is positive, the only valid states are ChannelOpened and ChannelClosing.");
                        }

                        if (this.currentChannel.State != CommunicationState.Faulted)
                        {
                            this.state = State.ChannelClosing;
                        }
                    }
                }
            }

            bool RemoveWaiter(IWaiter waiter)
            {
                Queue<IWaiter> waiters = waiter.CanGetChannel ? this.getChannelQueue : this.waitQueue;
                bool removed = false;

                lock (this.ThisLock)
                {
                    if (!this.ValidateOpened())
                    {
                        return false;
                    }

                    for (int i = waiters.Count; i > 0; i--)
                    {
                        IWaiter temp = waiters.Dequeue();

                        if (object.ReferenceEquals(waiter, temp))
                        {
                            removed = true;
                        }
                        else
                        {
                            waiters.Enqueue(temp);
                        }
                    }
                }

                return removed;
            }

            public void ReturnChannel()
            {
                TChannel channel = null;
                IWaiter waiter = null;
                bool faultBinder = false;
                bool drained;
                bool raiseInnerChannelFaulted = false;

                lock (this.ThisLock)
                {
                    if (this.count <= 0)
                    {
                        throw Fx.AssertAndThrow("Method ReturnChannel() can only be called after TryGetChannel or EndTryGetChannel returns a channel.");
                    }

                    this.count--;
                    drained = (this.count == 0) && (this.drainEvent != null);

                    if (this.ValidateOpened())
                    {
                        if ((this.state != State.ChannelOpened) && (this.state != State.ChannelClosing))
                        {
                            throw Fx.AssertAndThrow("ChannelOpened and ChannelClosing are the only 2 valid states when count is positive.");
                        }

                        if (this.currentChannel.State == CommunicationState.Faulted)
                        {
                            faultBinder = !this.tolerateFaults;
                            this.innerChannelFaulted = true;
                            this.state = State.ChannelClosing;
                        }

                        if (!faultBinder && (this.state == State.ChannelClosing) && (this.count == 0))
                        {
                            channel = this.currentChannel;
                            raiseInnerChannelFaulted = this.innerChannelFaulted;
                            this.innerChannelFaulted = false;

                            this.state = State.NoChannel;
                            this.aborting = false;

                            waiter = this.GetChannelWaiter();

                            if (waiter != null)
                            {
                                this.state = State.ChannelOpening;
                            }
                        }
                    }
                }

                if (faultBinder)
                {
                    this.binder.Fault(null);
                }

                if (drained)
                {
                    this.drainEvent.Set();
                }

                if (channel != null)
                {
                    channel.Faulted -= this.onChannelFaulted;

                    if (channel.State == CommunicationState.Opened)
                    {
                        this.binder.CloseChannel(channel);
                    }
                    else
                    {
                        channel.Abort();
                    }

                    if (waiter != null)
                    {
                        waiter.GetChannel(false);
                    }
                }

                if (raiseInnerChannelFaulted)
                {
                    this.binder.OnInnerChannelFaulted();
                }
            }

            public bool SetChannel(TChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (this.state != State.ChannelOpening && this.state != State.NoChannel)
                    {
                        throw Fx.AssertAndThrow("SetChannel is only valid in the NoChannel and ChannelOpening states");
                    }

                    if (!this.tolerateFaults)
                    {
                        throw Fx.AssertAndThrow("SetChannel is only valid when masking faults");
                    }

                    if (this.ValidateOpened())
                    {
                        this.currentChannel = channel;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            void SetTolerateFaults()
            {
                if (this.faultMode == TolerateFaultsMode.Never)
                {
                    this.tolerateFaults = false;
                }
                else if (this.faultMode == TolerateFaultsMode.IfNotSecuritySession)
                {
                    this.tolerateFaults = !this.binder.HasSecuritySession(this.currentChannel);
                }

                if (this.onChannelFaulted == null)
                {
                    this.onChannelFaulted = new EventHandler(this.OnChannelFaulted);
                }

                this.currentChannel.Faulted += this.onChannelFaulted;
            }

            void SetWaiters(Queue<IWaiter> waiters, TChannel channel)
            {
                if ((waiters != null) && (waiters.Count > 0))
                {
                    foreach (IWaiter waiter in waiters)
                    {
                        waiter.Set(channel);
                    }
                }
            }

            public void StartSynchronizing()
            {
                lock (this.ThisLock)
                {
                    if (this.state == State.Created)
                    {
                        this.state = State.NoChannel;
                    }
                    else
                    {
                        if (this.state != State.Closed)
                        {
                            throw Fx.AssertAndThrow("Abort is the only operation that can ---- with Open.");
                        }

                        return;
                    }

                    if (this.currentChannel == null)
                    {
                        if (!this.binder.TryGetChannel(TimeSpan.Zero))
                        {
                            return;
                        }
                    }

                    if (this.currentChannel == null)
                    {
                        return;
                    }

                    if (!this.binder.MustOpenChannel)
                    {
                        // Channel is already opened.
                        this.state = State.ChannelOpened;
                        this.SetTolerateFaults();
                    }
                }
            }

            public TChannel StopSynchronizing(bool close)
            {
                lock (this.ThisLock)
                {
                    if ((this.state != State.Faulted) && (this.state != State.Closed))
                    {
                        this.state = close ? State.Closed : State.Faulted;

                        if ((this.currentChannel != null) && (this.onChannelFaulted != null))
                        {
                            this.currentChannel.Faulted -= this.onChannelFaulted;
                        }
                    }

                    return this.currentChannel;
                }
            }

            // Must be called under a lock.
            bool ThrowIfNecessary(MaskingMode maskingMode)
            {
                if (this.ValidateOpened())
                {
                    return true;
                }

                // state is Closed or Faulted.
                Exception e;

                if (this.state == State.Closed)
                {
                    e = this.binder.GetClosedException(maskingMode);
                }
                else
                {
                    e = this.binder.GetFaultedException(maskingMode);
                }

                if (e != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                }

                return false;
            }

            public bool TryGetChannelForInput(bool canGetChannel, TimeSpan timeout,
                out TChannel channel)
            {
                return this.TryGetChannel(canGetChannel, false, timeout, MaskingMode.All,
                    out channel);
            }

            public bool TryGetChannelForOutput(TimeSpan timeout, MaskingMode maskingMode,
                out TChannel channel)
            {
                return this.TryGetChannel(true, true, timeout, maskingMode, out channel);
            }

            bool TryGetChannel(bool canGetChannel, bool canCauseFault, TimeSpan timeout,
                MaskingMode maskingMode, out TChannel channel)
            {
                SyncWaiter waiter = null;
                bool faulted = false;
                bool getChannel = false;

                lock (this.ThisLock)
                {
                    if (!this.ThrowIfNecessary(maskingMode))
                    {
                        channel = null;
                        return true;
                    }

                    if (this.state == State.ChannelOpened)
                    {
                        if (this.currentChannel == null)
                        {
                            throw Fx.AssertAndThrow("Field currentChannel cannot be null in the ChannelOpened state.");
                        }

                        this.count++;
                        channel = this.currentChannel;
                        return true;
                    }

                    if (!this.tolerateFaults
                        && ((this.state == State.ChannelClosing)
                        || (this.state == State.NoChannel)))
                    {
                        if (!canCauseFault)
                        {
                            channel = null;
                            return true;
                        }

                        faulted = true;
                    }
                    else if (!canGetChannel
                        || (this.state == State.ChannelOpening)
                        || (this.state == State.ChannelClosing))
                    {
                        waiter = new SyncWaiter(this, canGetChannel, null, timeout, maskingMode, this.binder.ChannelParameters);
                        this.GetQueue(canGetChannel).Enqueue(waiter);
                    }
                    else
                    {
                        if (this.state != State.NoChannel)
                        {
                            throw Fx.AssertAndThrow("The state must be NoChannel.");
                        }

                        waiter = new SyncWaiter(this, canGetChannel,
                            this.GetCurrentChannelIfCreated(), timeout, maskingMode,
                            this.binder.ChannelParameters);

                        this.state = State.ChannelOpening;
                        getChannel = true;
                    }
                }

                if (faulted)
                {
                    this.binder.Fault(null);
                    channel = null;
                    return true;
                }

                if (getChannel)
                {
                    waiter.GetChannel(true);
                }

                return waiter.TryWait(out channel);
            }

            public void UnblockWaiters()
            {
                Queue<IWaiter> temp1;
                Queue<IWaiter> temp2;

                lock (this.ThisLock)
                {
                    temp1 = this.getChannelQueue;
                    temp2 = this.waitQueue;

                    this.getChannelQueue = null;
                    this.waitQueue = null;
                }

                bool close = this.state == State.Closed;
                this.UnblockWaiters(temp1, close);
                this.UnblockWaiters(temp2, close);
            }

            void UnblockWaiters(Queue<IWaiter> waiters, bool close)
            {
                if ((waiters != null) && (waiters.Count > 0))
                {
                    foreach (IWaiter waiter in waiters)
                    {
                        if (close)
                        {
                            waiter.Close();
                        }
                        else
                        {
                            waiter.Fault();
                        }
                    }
                }
            }

            bool ValidateOpened()
            {
                if (this.state == State.Created)
                {
                    throw Fx.AssertAndThrow("This operation expects that the synchronizer has been opened.");
                }

                return (this.state != State.Closed) && (this.state != State.Faulted);
            }

            public void WaitForPendingOperations(TimeSpan timeout)
            {
                lock (this.ThisLock)
                {
                    if (this.drainEvent != null)
                    {
                        throw Fx.AssertAndThrow("The WaitForPendingOperations operation may only be invoked once.");
                    }

                    if (this.count > 0)
                    {
                        this.drainEvent = new InterruptibleWaitObject(false, false);
                    }
                }

                if (this.drainEvent != null)
                {
                    this.drainEvent.Wait(timeout);
                }
            }

            enum State
            {
                Created,
                NoChannel,
                ChannelOpening,
                ChannelOpened,
                ChannelClosing,
                Faulted,
                Closed
            }

            public interface IWaiter
            {
                bool CanGetChannel { get; }

                void Close();
                void Fault();
                void GetChannel(bool onUserThread);
                void Set(TChannel channel);
            }

            public sealed class AsyncWaiter : AsyncResult, IWaiter
            {
                bool canGetChannel;
                TChannel channel;
                ChannelParameterCollection channelParameters;
                bool isSynchronous = true;
                MaskingMode maskingMode;
                static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnOpenComplete));
                static Action<object> onTimeoutElapsed = new Action<object>(OnTimeoutElapsed);
                static AsyncCallback onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(OnTryGetChannelComplete));
                bool timedOut = false;
                ChannelSynchronizer synchronizer;
                TimeoutHelper timeoutHelper;
                IOThreadTimer timer;
                bool timerCancelled = false;

                public AsyncWaiter(ChannelSynchronizer synchronizer, bool canGetChannel,
                    TChannel channel, TimeSpan timeout, MaskingMode maskingMode,
                    ChannelParameterCollection channelParameters,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    if (!canGetChannel)
                    {
                        if (channel != null)
                        {
                            throw Fx.AssertAndThrow("This waiter must wait for a channel thus argument channel must be null.");
                        }
                    }

                    this.synchronizer = synchronizer;
                    this.canGetChannel = canGetChannel;
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.maskingMode = maskingMode;
                    this.channelParameters = channelParameters;
                }

                public bool CanGetChannel
                {
                    get
                    {
                        return this.canGetChannel;
                    }
                }

                object ThisLock
                {
                    get
                    {
                        return this;
                    }
                }

                void CancelTimer()
                {
                    lock (this.ThisLock)
                    {
                        if (!this.timerCancelled)
                        {
                            if (this.timer != null)
                            {
                                this.timer.Cancel();
                            }

                            this.timerCancelled = true;
                        }
                    }
                }

                public void Close()
                {
                    this.CancelTimer();
                    this.channel = null;
                    this.Complete(false,
                        this.synchronizer.binder.GetClosedException(this.maskingMode));
                }

                bool CompleteOpen(IAsyncResult result)
                {
                    this.channel.EndOpen(result);
                    return this.OnChannelOpened();
                }

                bool CompleteTryGetChannel(IAsyncResult result)
                {
                    if (!this.synchronizer.binder.EndTryGetChannel(result))
                    {
                        this.timedOut = true;
                        this.OnGetChannelFailed();
                        return true;
                    }

                    if (!this.synchronizer.CompleteSetChannel(this, out this.channel))
                    {
                        if (!this.IsCompleted)
                        {
                            throw Fx.AssertAndThrow("CompleteSetChannel must complete the IWaiter if it returns false.");
                        }

                        return false;
                    }

                    return this.OpenChannel();
                }

                public bool End(out TChannel channel)
                {
                    AsyncResult.End<AsyncWaiter>(this);
                    channel = this.channel;
                    return !this.timedOut;
                }

                public void Fault()
                {
                    this.CancelTimer();
                    this.channel = null;
                    this.Complete(false,
                        this.synchronizer.binder.GetFaultedException(this.maskingMode));
                }

                bool GetChannel()
                {
                    if (this.channel != null)
                    {
                        return this.OpenChannel();
                    }
                    else
                    {
                        IAsyncResult result = this.synchronizer.binder.BeginTryGetChannel(
                            this.timeoutHelper.RemainingTime(), onTryGetChannelComplete, this);

                        if (result.CompletedSynchronously)
                        {
                            return this.CompleteTryGetChannel(result);
                        }
                    }

                    return false;
                }

                public void GetChannel(bool onUserThread)
                {
                    if (!this.CanGetChannel)
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus the caller cannot attempt to get a channel.");
                    }

                    this.isSynchronous = onUserThread;

                    if (onUserThread)
                    {
                        bool throwing = true;

                        try
                        {
                            if (this.GetChannel())
                            {
                                this.Complete(true);
                            }

                            throwing = false;
                        }
                        finally
                        {
                            if (throwing)
                            {
                                this.OnGetChannelFailed();
                            }
                        }
                    }
                    else
                    {
                        bool complete = false;
                        Exception completeException = null;

                        try
                        {
                            this.CancelTimer();
                            complete = this.GetChannel();
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            this.OnGetChannelFailed();
                            completeException = e;
                        }

                        if (complete || completeException != null)
                        {
                            this.Complete(false, completeException);
                        }
                    }
                }

                bool OnChannelOpened()
                {
                    if (this.synchronizer.OnChannelOpened(this))
                    {
                        return true;
                    }
                    else
                    {
                        if (!this.IsCompleted)
                        {
                            throw Fx.AssertAndThrow("OnChannelOpened must complete the IWaiter if it returns false.");
                        }

                        return false;
                    }
                }

                void OnGetChannelFailed()
                {
                    if (this.channel != null)
                    {
                        this.channel.Abort();
                    }

                    this.synchronizer.OnGetChannelFailed();
                }

                static void OnOpenComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        AsyncWaiter waiter = (AsyncWaiter)result.AsyncState;
                        bool complete = false;
                        Exception completeException = null;

                        waiter.isSynchronous = false;

                        try
                        {
                            complete = waiter.CompleteOpen(result);
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            completeException = e;
                        }

                        if (complete)
                        {
                            waiter.Complete(false);
                        }
                        else if (completeException != null)
                        {
                            waiter.OnGetChannelFailed();
                            waiter.Complete(false, completeException);
                        }
                    }
                }

                void OnTimeoutElapsed()
                {
                    if (this.synchronizer.RemoveWaiter(this))
                    {
                        this.timedOut = true;
                        this.Complete(this.isSynchronous, null);
                    }
                }

                static void OnTimeoutElapsed(object state)
                {
                    AsyncWaiter waiter = (AsyncWaiter)state;
                    waiter.isSynchronous = false;
                    waiter.OnTimeoutElapsed();
                }

                static void OnTryGetChannelComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        AsyncWaiter waiter = (AsyncWaiter)result.AsyncState;
                        waiter.isSynchronous = false;
                        bool complete = false;
                        Exception completeException = null;

                        try
                        {
                            complete = waiter.CompleteTryGetChannel(result);
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            completeException = e;
                        }

                        if (complete || completeException != null)
                        {
                            if (completeException != null)
                                waiter.OnGetChannelFailed();
                            waiter.Complete(waiter.isSynchronous, completeException);
                        }
                    }
                }

                bool OpenChannel()
                {
                    if (this.synchronizer.binder.MustOpenChannel)
                    {
                        if (this.channelParameters != null)
                        {
                            this.channelParameters.PropagateChannelParameters(this.channel);
                        }

                        IAsyncResult result = this.channel.BeginOpen(
                            this.timeoutHelper.RemainingTime(), onOpenComplete, this);

                        if (result.CompletedSynchronously)
                        {
                            return this.CompleteOpen(result);
                        }

                        return false;
                    }
                    else
                    {
                        return this.OnChannelOpened();
                    }
                }

                public void Set(TChannel channel)
                {
                    this.CancelTimer();
                    this.channel = channel;
                    this.Complete(false);
                }

                // Always called from the user's thread.
                public void Wait()
                {
                    lock (this.ThisLock)
                    {
                        if (this.timerCancelled)
                        {
                            return;
                        }

                        TimeSpan timeout = this.timeoutHelper.RemainingTime();

                        if (timeout > TimeSpan.Zero)
                        {
                            this.timer = new IOThreadTimer(onTimeoutElapsed, this, true);
                            this.timer.Set(this.timeoutHelper.RemainingTime());
                            return;
                        }
                    }

                    this.OnTimeoutElapsed();
                }
            }

            sealed class SynchronizerCompletedAsyncResult : CompletedAsyncResult
            {
                public SynchronizerCompletedAsyncResult(AsyncCallback callback, object state)
                    : base(callback, state)
                {
                }

                public void End()
                {
                    CompletedAsyncResult.End(this);
                }
            }

            sealed class SyncWaiter : IWaiter
            {
                bool canGetChannel;
                TChannel channel;
                ChannelParameterCollection channelParameters;
                AutoResetEvent completeEvent = new AutoResetEvent(false);
                Exception exception;
                bool getChannel = false;
                MaskingMode maskingMode;
                ChannelSynchronizer synchronizer;
                TimeoutHelper timeoutHelper;

                public SyncWaiter(ChannelSynchronizer synchronizer, bool canGetChannel,
                    TChannel channel, TimeSpan timeout, MaskingMode maskingMode,
                    ChannelParameterCollection channelParameters)
                {
                    if (!canGetChannel)
                    {
                        if (channel != null)
                        {
                            throw Fx.AssertAndThrow("This waiter must wait for a channel thus argument channel must be null.");
                        }
                    }

                    this.synchronizer = synchronizer;
                    this.canGetChannel = canGetChannel;
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.maskingMode = maskingMode;
                    this.channelParameters = channelParameters;
                }

                public bool CanGetChannel
                {
                    get
                    {
                        return this.canGetChannel;
                    }
                }

                public void Close()
                {
                    this.exception = this.synchronizer.binder.GetClosedException(this.maskingMode);
                    this.completeEvent.Set();
                }

                public void Fault()
                {
                    this.exception = this.synchronizer.binder.GetFaultedException(this.maskingMode);
                    this.completeEvent.Set();
                }

                public void GetChannel(bool onUserThread)
                {
                    if (!this.CanGetChannel)
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus the caller cannot attempt to get a channel.");
                    }

                    this.getChannel = true;
                    this.completeEvent.Set();
                }

                public void Set(TChannel channel)
                {
                    if (channel == null)
                    {
                        throw Fx.AssertAndThrow("Argument channel cannot be null. Caller must call Fault or Close instead.");
                    }

                    this.channel = channel;
                    this.completeEvent.Set();
                }

                bool TryGetChannel()
                {
                    TChannel channel;

                    if (this.channel != null)
                    {
                        channel = this.channel;
                    }
                    else if (this.synchronizer.binder.TryGetChannel(
                        this.timeoutHelper.RemainingTime()))
                    {
                        if (!this.synchronizer.CompleteSetChannel(this, out channel))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        this.synchronizer.OnGetChannelFailed();
                        return false;
                    }

                    if (this.synchronizer.binder.MustOpenChannel)
                    {
                        bool throwing = true;

                        if (this.channelParameters != null)
                        {
                            this.channelParameters.PropagateChannelParameters(channel);
                        }

                        try
                        {
                            channel.Open(this.timeoutHelper.RemainingTime());
                            throwing = false;
                        }
                        finally
                        {
                            if (throwing)
                            {
                                channel.Abort();
                                this.synchronizer.OnGetChannelFailed();
                            }
                        }
                    }

                    if (this.synchronizer.OnChannelOpened(this))
                    {
                        this.Set(channel);
                    }

                    return true;
                }

                public bool TryWait(out TChannel channel)
                {
                    if (!this.Wait())
                    {
                        channel = null;
                        return false;
                    }
                    else if (this.getChannel && !this.TryGetChannel())
                    {
                        channel = null;
                        return false;
                    }

                    this.completeEvent.Close();

                    if (this.exception != null)
                    {
                        if (this.channel != null)
                        {
                            throw Fx.AssertAndThrow("User of IWaiter called both Set and Fault or Close.");
                        }

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                    }

                    channel = this.channel;
                    return true;
                }

                bool Wait()
                {
                    if (!TimeoutHelper.WaitOne(this.completeEvent, this.timeoutHelper.RemainingTime()))
                    {
                        if (this.synchronizer.RemoveWaiter(this))
                        {
                            return false;
                        }
                        else
                        {
                            TimeoutHelper.WaitOne(this.completeEvent, TimeSpan.MaxValue);
                        }
                    }

                    return true;
                }
            }
        }

        sealed class CloseAsyncResult : AsyncResult
        {
            ReliableChannelBinder<TChannel> binder;
            TChannel channel;
            MaskingMode maskingMode;
            static AsyncCallback onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnBinderCloseComplete));
            static AsyncCallback onChannelCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnChannelCloseComplete));
            TimeoutHelper timeoutHelper;

            public CloseAsyncResult(ReliableChannelBinder<TChannel> binder, TChannel channel,
                TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.binder = binder;
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.maskingMode = maskingMode;
                bool complete = false;

                try
                {
                    this.binder.OnShutdown();
                    IAsyncResult result = this.binder.OnBeginClose(timeout, onBinderCloseComplete, this);

                    if (result.CompletedSynchronously)
                    {
                        complete = this.CompleteBinderClose(true, result);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.binder.Abort();

                    if (!this.binder.HandleException(e, this.maskingMode))
                    {
                        throw;
                    }
                    else
                    {
                        complete = true;
                    }
                }

                if (complete)
                {
                    this.Complete(true);
                }
            }

            bool CompleteBinderClose(bool synchronous, IAsyncResult result)
            {
                this.binder.OnEndClose(result);

                if (this.channel != null)
                {
                    result = this.binder.BeginCloseChannel(this.channel,
                        this.timeoutHelper.RemainingTime(), onChannelCloseComplete, this);

                    if (result.CompletedSynchronously)
                    {
                        return this.CompleteChannelClose(synchronous, result);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    this.binder.TransitionToClosed();
                    return true;
                }
            }

            bool CompleteChannelClose(bool synchronous, IAsyncResult result)
            {
                this.binder.EndCloseChannel(this.channel, result);
                this.binder.TransitionToClosed();
                return true;
            }

            public void End()
            {
                AsyncResult.End<CloseAsyncResult>(this);
            }

            Exception HandleAsyncException(Exception e)
            {
                this.binder.Abort();

                if (this.binder.HandleException(e, this.maskingMode))
                {
                    return null;
                }
                else
                {
                    return e;
                }
            }

            static void OnBinderCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    CloseAsyncResult closeResult = (CloseAsyncResult)result.AsyncState;
                    bool complete;
                    Exception completeException;

                    try
                    {
                        complete = closeResult.CompleteBinderClose(false, result);
                        completeException = null;
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        complete = true;
                        completeException = e;
                    }

                    if (complete)
                    {
                        if (completeException != null)
                        {
                            completeException = closeResult.HandleAsyncException(completeException);
                        }

                        closeResult.Complete(false, completeException);
                    }
                }
            }

            static void OnChannelCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    CloseAsyncResult closeResult = (CloseAsyncResult)result.AsyncState;
                    bool complete;
                    Exception completeException;

                    try
                    {
                        complete = closeResult.CompleteChannelClose(false, result);
                        completeException = null;
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        complete = true;
                        completeException = e;
                    }

                    if (complete)
                    {
                        if (completeException != null)
                        {
                            completeException = closeResult.HandleAsyncException(completeException);
                        }

                        closeResult.Complete(false, completeException);
                    }
                }
            }
        }

        protected abstract class InputAsyncResult<TBinder> : AsyncResult
            where TBinder : ReliableChannelBinder<TChannel>
        {
            bool autoAborted;
            TBinder binder;
            bool canGetChannel;
            TChannel channel;
            bool isSynchronous = true;
            MaskingMode maskingMode;
            static AsyncCallback onInputComplete = Fx.ThunkCallback(new AsyncCallback(OnInputCompleteStatic));
            static AsyncCallback onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(OnTryGetChannelCompleteStatic));
            bool success;
            TimeoutHelper timeoutHelper;

            public InputAsyncResult(TBinder binder, bool canGetChannel, TimeSpan timeout,
                MaskingMode maskingMode, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.binder = binder;
                this.canGetChannel = canGetChannel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.maskingMode = maskingMode;
            }

            protected abstract IAsyncResult BeginInput(TBinder binder, TChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state);

            // returns true if the caller should retry
            bool CompleteInput(IAsyncResult result)
            {
                bool complete;

                try
                {
                    this.success = this.EndInput(this.binder, this.channel, result, out complete);
                }
                finally
                {
                    this.autoAborted = this.binder.Synchronizer.Aborting;
                    this.binder.synchronizer.ReturnChannel();
                }

                return !complete;
            }

            // returns true if the caller should retry
            bool CompleteTryGetChannel(IAsyncResult result, out bool complete)
            {
                complete = false;
                this.success = this.binder.synchronizer.EndTryGetChannel(result, out this.channel);

                // the synchronizer is faulted and not reestablishing or closed, or the call timed
                // out, complete and don't retry.
                if (this.channel == null)
                {
                    complete = true;
                    return false;
                }

                bool throwing = true;
                IAsyncResult inputResult = null;

                try
                {
                    inputResult = this.BeginInput(this.binder, this.channel,
                        this.timeoutHelper.RemainingTime(), onInputComplete, this);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        this.autoAborted = this.binder.Synchronizer.Aborting;
                        this.binder.synchronizer.ReturnChannel();
                    }
                }

                if (inputResult.CompletedSynchronously)
                {
                    if (this.CompleteInput(inputResult))
                    {
                        complete = false;
                        return true;
                    }
                    else
                    {
                        complete = true;
                        return false;
                    }
                }
                else
                {
                    complete = false;
                    return false;
                }
            }

            public bool End()
            {
                AsyncResult.End<InputAsyncResult<TBinder>>(this);
                return this.success;
            }

            protected abstract bool EndInput(TBinder binder, TChannel channel,
                IAsyncResult result, out bool complete);

            void OnInputComplete(IAsyncResult result)
            {
                this.isSynchronous = false;
                bool retry;
                Exception completeException = null;

                try
                {
                    retry = this.CompleteInput(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                    {
                        completeException = e;
                        retry = false;
                    }
                    else
                    {
                        retry = true;
                    }
                }

                if (retry)
                {
                    this.StartOnNonUserThread();
                }
                else
                {
                    this.Complete(this.isSynchronous, completeException);
                }
            }

            static void OnInputCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    InputAsyncResult<TBinder> inputResult =
                        (InputAsyncResult<TBinder>)result.AsyncState;
                    inputResult.OnInputComplete(result);
                }
            }

            void OnTryGetChannelComplete(IAsyncResult result)
            {
                this.isSynchronous = false;
                bool retry = false;
                bool complete = false;
                Exception completeException = null;

                try
                {
                    retry = this.CompleteTryGetChannel(result, out complete);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                    {
                        completeException = e;
                        retry = false;
                    }
                    else
                    {
                        retry = true;
                    }
                }

                // Can't complete AND retry.
                if (complete && retry)
                {
                    throw Fx.AssertAndThrow("The derived class' implementation of CompleteTryGetChannel() cannot indicate that the asynchronous operation should complete and retry.");
                }

                if (retry)
                {
                    this.StartOnNonUserThread();
                }
                else if (complete || completeException != null)
                {
                    this.Complete(this.isSynchronous, completeException);
                }
            }

            static void OnTryGetChannelCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    InputAsyncResult<TBinder> inputResult =
                        (InputAsyncResult<TBinder>)result.AsyncState;
                    inputResult.OnTryGetChannelComplete(result);
                }
            }

            protected bool Start()
            {
                while (true)
                {
                    bool retry = false;
                    bool complete = false;

                    this.autoAborted = false;

                    try
                    {
                        IAsyncResult result = this.binder.synchronizer.BeginTryGetChannelForInput(
                            canGetChannel, this.timeoutHelper.RemainingTime(),
                            onTryGetChannelComplete, this);

                        if (result.CompletedSynchronously)
                        {
                            retry = this.CompleteTryGetChannel(result, out complete);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (!this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                        {
                            throw;
                        }
                        else
                        {
                            retry = true;
                        }
                    }

                    // Can't complete AND retry.
                    if (complete && retry)
                    {
                        throw Fx.AssertAndThrow("The derived class' implementation of CompleteTryGetChannel() cannot indicate that the asynchronous operation should complete and retry.");
                    }

                    if (!retry)
                    {
                        return complete;
                    }
                }
            }

            void StartOnNonUserThread()
            {
                bool complete = false;
                Exception completeException = null;

                try
                {
                    complete = this.Start();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete || completeException != null)
                    this.Complete(false, completeException);
            }
        }

        sealed class MessageRequestContext : BinderRequestContext
        {
            public MessageRequestContext(ReliableChannelBinder<TChannel> binder, Message message)
                : base(binder, message)
            {
            }

            protected override void OnAbort()
            {
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReplyAsyncResult(this, message, timeout, callback, state);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                ReplyAsyncResult.End(result);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                if (message != null)
                {
                    this.Binder.Send(message, timeout, this.MaskingMode);
                }
            }

            class ReplyAsyncResult : AsyncResult
            {
                static AsyncCallback onSend;
                MessageRequestContext context;

                public ReplyAsyncResult(MessageRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    if (message != null)
                    {
                        if (onSend == null)
                        {
                            onSend = Fx.ThunkCallback(new AsyncCallback(OnSend));
                        }
                        this.context = context;
                        IAsyncResult result = context.Binder.BeginSend(message, timeout, context.MaskingMode, onSend, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        context.Binder.EndSend(result);
                    }

                    base.Complete(true);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ReplyAsyncResult>(result);
                }

                static void OnSend(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    ReplyAsyncResult thisPtr = (ReplyAsyncResult)result.AsyncState;
                    try
                    {
                        thisPtr.context.Binder.EndSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }

                    thisPtr.Complete(false, completionException);
                }
            }
        }

        protected abstract class OutputAsyncResult<TBinder> : AsyncResult
            where TBinder : ReliableChannelBinder<TChannel>
        {
            bool autoAborted;
            TBinder binder;
            TChannel channel;
            bool hasChannel = false;
            MaskingMode maskingMode;
            Message message;
            static AsyncCallback onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(OnTryGetChannelCompleteStatic));
            static AsyncCallback onOutputComplete = Fx.ThunkCallback(new AsyncCallback(OnOutputCompleteStatic));
            TimeSpan timeout;
            TimeoutHelper timeoutHelper;

            public OutputAsyncResult(TBinder binder, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.binder = binder;
            }

            public MaskingMode MaskingMode
            {
                get
                {
                    return this.maskingMode;
                }
            }

            protected abstract IAsyncResult BeginOutput(TBinder binder, TChannel channel,
                Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback,
                object state);

            void Cleanup()
            {
                if (this.hasChannel)
                {
                    this.autoAborted = this.binder.Synchronizer.Aborting;
                    this.binder.synchronizer.ReturnChannel();
                }
            }

            bool CompleteOutput(IAsyncResult result)
            {
                this.EndOutput(this.binder, this.channel, this.maskingMode, result);
                this.Cleanup();
                return true;
            }

            bool CompleteTryGetChannel(IAsyncResult result)
            {
                bool timedOut = !this.binder.synchronizer.EndTryGetChannel(result,
                    out this.channel);

                if (timedOut || (this.channel == null))
                {
                    this.Cleanup();

                    if (timedOut && !ReliableChannelBinderHelper.MaskHandled(maskingMode))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(this.GetTimeoutString(this.timeout)));
                    }

                    return true;
                }

                this.hasChannel = true;

                result = this.BeginOutput(this.binder, this.channel, this.message,
                    this.timeoutHelper.RemainingTime(), this.maskingMode, onOutputComplete,
                    this);

                if (result.CompletedSynchronously)
                {
                    return this.CompleteOutput(result);
                }
                else
                {
                    return false;
                }
            }

            protected abstract void EndOutput(TBinder binder, TChannel channel,
                MaskingMode maskingMode, IAsyncResult result);

            protected abstract string GetTimeoutString(TimeSpan timeout);

            void OnOutputComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool complete = false;
                    Exception completeException = null;

                    try
                    {
                        complete = this.CompleteOutput(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        this.Cleanup();
                        complete = true;
                        if (!this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                        {
                            completeException = e;
                        }
                    }

                    if (complete)
                    {
                        this.Complete(false, completeException);
                    }
                }
            }

            static void OnOutputCompleteStatic(IAsyncResult result)
            {
                OutputAsyncResult<TBinder> outputResult =
                    (OutputAsyncResult<TBinder>)result.AsyncState;

                outputResult.OnOutputComplete(result);
            }

            void OnTryGetChannelComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool complete = false;
                    Exception completeException = null;

                    try
                    {
                        complete = this.CompleteTryGetChannel(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        this.Cleanup();
                        complete = true;
                        if (!this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                        {
                            completeException = e;
                        }
                    }

                    if (complete)
                    {
                        this.Complete(false, completeException);
                    }
                }
            }

            static void OnTryGetChannelCompleteStatic(IAsyncResult result)
            {
                OutputAsyncResult<TBinder> outputResult =
                    (OutputAsyncResult<TBinder>)result.AsyncState;

                outputResult.OnTryGetChannelComplete(result);
            }

            public void Start(Message message, TimeSpan timeout, MaskingMode maskingMode)
            {
                if (!this.binder.ValidateOutputOperation(message, timeout, maskingMode))
                {
                    this.Complete(true);
                    return;
                }

                this.message = message;
                this.timeout = timeout;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.maskingMode = maskingMode;

                bool complete = false;

                try
                {
                    IAsyncResult result = this.binder.synchronizer.BeginTryGetChannelForOutput(
                        timeoutHelper.RemainingTime(), this.maskingMode, onTryGetChannelComplete, this);

                    if (result.CompletedSynchronously)
                    {
                        complete = this.CompleteTryGetChannel(result);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.Cleanup();
                    if (this.binder.HandleException(e, this.maskingMode, this.autoAborted))
                    {
                        complete = true;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (complete)
                {
                    this.Complete(true);
                }
            }
        }

        sealed class RequestRequestContext : BinderRequestContext
        {
            RequestContext innerContext;

            public RequestRequestContext(ReliableChannelBinder<TChannel> binder,
                RequestContext innerContext, Message message)
                : base(binder, message)
            {
                if ((binder.defaultMaskingMode != MaskingMode.All) && !binder.TolerateFaults)
                {
                    throw Fx.AssertAndThrow("This request context is designed to catch exceptions. Thus it cannot be used if the caller expects no exception handling.");
                }

                if (innerContext == null)
                {
                    throw Fx.AssertAndThrow("Argument innerContext cannot be null.");
                }

                this.innerContext = innerContext;
            }

            protected override void OnAbort()
            {
                this.innerContext.Abort();
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout,
                AsyncCallback callback, object state)
            {
                try
                {
                    if (message != null)
                        this.Binder.AddOutputHeaders(message);
                    return this.innerContext.BeginReply(message, timeout, callback, state);
                }
                catch (ObjectDisposedException) { }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.Binder.HandleException(e, this.MaskingMode))
                    {
                        throw;
                    }

                    this.innerContext.Abort();
                }

                return new BinderCompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                try
                {
                    this.innerContext.Close(timeout);
                }
                catch (ObjectDisposedException) { }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.Binder.HandleException(e, this.MaskingMode))
                    {
                        throw;
                    }

                    this.innerContext.Abort();
                }
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                BinderCompletedAsyncResult completedResult = result as BinderCompletedAsyncResult;
                if (completedResult != null)
                {
                    completedResult.End();
                    return;
                }

                try
                {
                    this.innerContext.EndReply(result);
                }
                catch (ObjectDisposedException) { }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.Binder.HandleException(e, this.MaskingMode))
                    {
                        throw;
                    }

                    this.innerContext.Abort();
                }
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                try
                {
                    if (message != null)
                        this.Binder.AddOutputHeaders(message);
                    this.innerContext.Reply(message, timeout);
                }
                catch (ObjectDisposedException) { }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!this.Binder.HandleException(e, this.MaskingMode))
                    {
                        throw;
                    }

                    this.innerContext.Abort();
                }
            }
        }

        sealed class SendAsyncResult : OutputAsyncResult<ReliableChannelBinder<TChannel>>
        {
            public SendAsyncResult(ReliableChannelBinder<TChannel> binder, AsyncCallback callback,
                object state)
                : base(binder, callback, state)
            {
            }

            protected override IAsyncResult BeginOutput(ReliableChannelBinder<TChannel> binder,
                TChannel channel, Message message, TimeSpan timeout, MaskingMode maskingMode,
                AsyncCallback callback, object state)
            {
                binder.AddOutputHeaders(message);
                return binder.OnBeginSend(channel, message, timeout, callback, state);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SendAsyncResult>(result);
            }

            protected override void EndOutput(ReliableChannelBinder<TChannel> binder,
                TChannel channel, MaskingMode maskingMode, IAsyncResult result)
            {
                binder.OnEndSend(channel, result);
            }

            protected override string GetTimeoutString(TimeSpan timeout)
            {
                return SR.GetString(SR.TimeoutOnSend, timeout);
            }
        }

        sealed class TryReceiveAsyncResult : InputAsyncResult<ReliableChannelBinder<TChannel>>
        {
            RequestContext requestContext;

            public TryReceiveAsyncResult(ReliableChannelBinder<TChannel> binder, TimeSpan timeout,
                MaskingMode maskingMode, AsyncCallback callback, object state)
                : base(binder, binder.CanGetChannelForReceive, timeout, maskingMode, callback, state)
            {
                if (this.Start())
                    this.Complete(true);
            }

            protected override IAsyncResult BeginInput(ReliableChannelBinder<TChannel> binder,
                TChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return binder.OnBeginTryReceive(channel, timeout, callback, state);
            }

            public bool End(out RequestContext requestContext)
            {
                requestContext = this.requestContext;
                return this.End();
            }

            protected override bool EndInput(ReliableChannelBinder<TChannel> binder,
                TChannel channel, IAsyncResult result, out bool complete)
            {
                bool success = binder.OnEndTryReceive(channel, result, out this.requestContext);

                // timed out || got message, complete immediately
                complete = !success || (this.requestContext != null);

                if (!complete)
                {
                    // the underlying channel closed or faulted
                    binder.synchronizer.OnReadEof();
                }

                return success;
            }
        }
    }

    static class ReliableChannelBinderHelper
    {
        internal static IAsyncResult BeginCloseDuplexSessionChannel(
            ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseDuplexSessionChannelAsyncResult(binder, channel, timeout, callback,
                state);
        }

        internal static IAsyncResult BeginCloseReplySessionChannel(
            ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseReplySessionChannelAsyncResult(binder, channel, timeout, callback,
                state);
        }

        internal static void CloseDuplexSessionChannel(
            ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel,
            TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            channel.Session.CloseOutputSession(timeoutHelper.RemainingTime());
            binder.WaitForPendingOperations(timeoutHelper.RemainingTime());

            TimeSpan iterationTimeout = timeoutHelper.RemainingTime();
            bool lastIteration = (iterationTimeout == TimeSpan.Zero);

            while (true)
            {
                Message message = null;
                bool receiveThrowing = true;

                try
                {
                    bool success = channel.TryReceive(iterationTimeout, out message);

                    receiveThrowing = false;
                    if (success && message == null)
                    {
                        channel.Close(timeoutHelper.RemainingTime());
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (receiveThrowing)
                    {
                        if (!MaskHandled(binder.DefaultMaskingMode) || !binder.IsHandleable(e))
                            throw;

                        receiveThrowing = false;
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (message != null)
                        message.Close();

                    if (receiveThrowing)
                        channel.Abort();
                }

                if (lastIteration || channel.State != CommunicationState.Opened)
                    break;

                iterationTimeout = timeoutHelper.RemainingTime();
                lastIteration = (iterationTimeout == TimeSpan.Zero);
            }

            channel.Abort();
        }

        internal static void CloseReplySessionChannel(
            ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel,
            TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            binder.WaitForPendingOperations(timeoutHelper.RemainingTime());

            TimeSpan iterationTimeout = timeoutHelper.RemainingTime();
            bool lastIteration = (iterationTimeout == TimeSpan.Zero);

            while (true)
            {
                RequestContext context = null;
                bool receiveThrowing = true;

                try
                {
                    bool success = channel.TryReceiveRequest(iterationTimeout, out context);

                    receiveThrowing = false;
                    if (success && context == null)
                    {
                        channel.Close(timeoutHelper.RemainingTime());
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (receiveThrowing)
                    {
                        if (!MaskHandled(binder.DefaultMaskingMode) || !binder.IsHandleable(e))
                            throw;

                        receiveThrowing = false;
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (context != null)
                    {
                        context.RequestMessage.Close();
                        context.Close();
                    }

                    if (receiveThrowing)
                        channel.Abort();
                }

                if (lastIteration || channel.State != CommunicationState.Opened)
                    break;

                iterationTimeout = timeoutHelper.RemainingTime();
                lastIteration = (iterationTimeout == TimeSpan.Zero);
            }

            channel.Abort();
        }

        internal static void EndCloseDuplexSessionChannel(IDuplexSessionChannel channel,
                IAsyncResult result)
        {
            CloseDuplexSessionChannelAsyncResult.End(result);
        }

        internal static void EndCloseReplySessionChannel(IReplySessionChannel channel,
                IAsyncResult result)
        {
            CloseReplySessionChannelAsyncResult.End(result);
        }

        internal static bool MaskHandled(MaskingMode maskingMode)
        {
            return (maskingMode & MaskingMode.Handled) == MaskingMode.Handled;
        }

        internal static bool MaskUnhandled(MaskingMode maskingMode)
        {
            return (maskingMode & MaskingMode.Unhandled) == MaskingMode.Unhandled;
        }

        abstract class CloseInputSessionChannelAsyncResult<TChannel, TItem> : AsyncResult
            where TChannel : class, IChannel
            where TItem : class
        {
            static AsyncCallback onChannelCloseCompleteStatic =
                Fx.ThunkCallback(
                new AsyncCallback(OnChannelCloseCompleteStatic));
            static AsyncCallback onInputCompleteStatic =
                Fx.ThunkCallback(new AsyncCallback(OnInputCompleteStatic));
            static AsyncCallback onWaitForPendingOperationsCompleteStatic =
                Fx.ThunkCallback(
                new AsyncCallback(OnWaitForPendingOperationsCompleteStatic));
            ReliableChannelBinder<TChannel> binder;
            TChannel channel;
            bool lastReceive;
            TimeoutHelper timeoutHelper;

            protected CloseInputSessionChannelAsyncResult(
                ReliableChannelBinder<TChannel> binder, TChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.binder = binder;
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
            }

            protected TChannel Channel
            {
                get
                {
                    return this.channel;
                }
            }

            protected TimeSpan RemainingTime
            {
                get
                {
                    return this.timeoutHelper.RemainingTime();
                }
            }

            protected bool Begin()
            {
                bool complete = false;
                IAsyncResult result = this.binder.BeginWaitForPendingOperations(
                    this.RemainingTime, onWaitForPendingOperationsCompleteStatic,
                    this);

                if (result.CompletedSynchronously)
                    complete = this.HandleWaitForPendingOperationsComplete(result);

                return complete;
            }

            protected abstract IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback,
                object state);

            protected abstract void DisposeItem(TItem item);

            protected abstract bool EndTryInput(IAsyncResult result, out TItem item);

            void HandleChannelCloseComplete(IAsyncResult result)
            {
                this.channel.EndClose(result);
            }

            bool HandleInputComplete(IAsyncResult result, out bool gotEof)
            {
                TItem item = null;
                bool endThrowing = true;

                gotEof = false;

                try
                {
                    bool success = false;

                    success = this.EndTryInput(result, out item);
                    endThrowing = false;

                    if (!success || item != null)
                    {
                        if (this.lastReceive || this.channel.State != CommunicationState.Opened)
                        {
                            this.channel.Abort();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    gotEof = true;

                    result = this.channel.BeginClose(this.RemainingTime,
                        onChannelCloseCompleteStatic, this);
                    if (result.CompletedSynchronously)
                    {
                        this.HandleChannelCloseComplete(result);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (endThrowing)
                    {
                        if (!MaskHandled(binder.DefaultMaskingMode) || !binder.IsHandleable(e))
                            throw;

                        if (this.lastReceive || this.channel.State != CommunicationState.Opened)
                        {
                            this.channel.Abort();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    throw;
                }
                finally
                {
                    if (item != null)
                        this.DisposeItem(item);

                    if (endThrowing)
                        this.channel.Abort();
                }
            }

            bool HandleWaitForPendingOperationsComplete(IAsyncResult result)
            {
                this.binder.EndWaitForPendingOperations(result);
                return this.WaitForEof();
            }

            static void OnChannelCloseCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseInputSessionChannelAsyncResult<TChannel, TItem> closeResult =
                    (CloseInputSessionChannelAsyncResult<TChannel, TItem>)result.AsyncState;

                Exception completeException = null;

                try
                {
                    closeResult.HandleChannelCloseComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                closeResult.Complete(false, completeException);
            }

            static void OnInputCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseInputSessionChannelAsyncResult<TChannel, TItem> closeResult =
                    (CloseInputSessionChannelAsyncResult<TChannel, TItem>)result.AsyncState;

                bool complete = false;
                Exception completeException = null;

                try
                {
                    bool gotEof;

                    complete = closeResult.HandleInputComplete(result, out gotEof);
                    if (!complete && !gotEof)
                        complete = closeResult.WaitForEof();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete || completeException != null)
                    closeResult.Complete(false, completeException);
            }

            static void OnWaitForPendingOperationsCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseInputSessionChannelAsyncResult<TChannel, TItem> closeResult =
                    (CloseInputSessionChannelAsyncResult<TChannel, TItem>)result.AsyncState;

                bool complete = false;
                Exception completeException = null;

                try
                {
                    complete = closeResult.HandleWaitForPendingOperationsComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete || completeException != null)
                    closeResult.Complete(false, completeException);
            }

            bool WaitForEof()
            {
                TimeSpan iterationTimeout = this.RemainingTime;
                this.lastReceive = (iterationTimeout == TimeSpan.Zero);

                while (true)
                {
                    IAsyncResult result = null;

                    try
                    {
                        result = this.BeginTryInput(iterationTimeout, onInputCompleteStatic, this);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        if (!MaskHandled(this.binder.DefaultMaskingMode) || !this.binder.IsHandleable(e))
                            throw;
                    }

                    if (result != null)
                    {
                        if (result.CompletedSynchronously)
                        {
                            bool gotEof;
                            bool complete = this.HandleInputComplete(result, out gotEof);

                            if (complete || gotEof)
                                return complete;
                        }
                        else
                            return false;
                    }

                    if (this.lastReceive || this.channel.State != CommunicationState.Opened)
                    {
                        this.channel.Abort();
                        break;
                    }

                    iterationTimeout = this.RemainingTime;
                    this.lastReceive = (iterationTimeout == TimeSpan.Zero);
                }

                return true;
            }
        }

        sealed class CloseDuplexSessionChannelAsyncResult :
            CloseInputSessionChannelAsyncResult<IDuplexSessionChannel, Message>
        {
            static AsyncCallback onCloseOutputSessionCompleteStatic =
                Fx.ThunkCallback(
                new AsyncCallback(OnCloseOutputSessionCompleteStatic));

            public CloseDuplexSessionChannelAsyncResult(
                ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(binder, channel, timeout, callback, state)
            {
                bool complete = false;

                IAsyncResult result = this.Channel.Session.BeginCloseOutputSession(
                    this.RemainingTime, onCloseOutputSessionCompleteStatic, this);

                if (result.CompletedSynchronously)
                    complete = this.HandleCloseOutputSessionComplete(result);

                if (complete)
                    this.Complete(true);
            }

            protected override IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.Channel.BeginTryReceive(timeout, callback, state);
            }

            protected override void DisposeItem(Message item)
            {
                item.Close();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseDuplexSessionChannelAsyncResult>(result);
            }

            protected override bool EndTryInput(IAsyncResult result, out Message item)
            {
                return this.Channel.EndTryReceive(result, out item);
            }

            bool HandleCloseOutputSessionComplete(IAsyncResult result)
            {
                this.Channel.Session.EndCloseOutputSession(result);
                return this.Begin();
            }

            static void OnCloseOutputSessionCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseDuplexSessionChannelAsyncResult closeResult =
                    (CloseDuplexSessionChannelAsyncResult)result.AsyncState;

                bool complete = false;
                Exception completeException = null;

                try
                {
                    complete = closeResult.HandleCloseOutputSessionComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete || completeException != null)
                    closeResult.Complete(false, completeException);
            }
        }

        sealed class CloseReplySessionChannelAsyncResult :
            CloseInputSessionChannelAsyncResult<IReplySessionChannel, RequestContext>
        {
            public CloseReplySessionChannelAsyncResult(
                ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(binder, channel, timeout, callback, state)
            {
                if (this.Begin())
                    this.Complete(true);
            }

            protected override IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.Channel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override void DisposeItem(RequestContext item)
            {
                item.RequestMessage.Close();
                item.Close();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseReplySessionChannelAsyncResult>(result);
            }

            protected override bool EndTryInput(IAsyncResult result, out RequestContext item)
            {
                return this.Channel.EndTryReceiveRequest(result, out item);
            }
        }
    }
}
