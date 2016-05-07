//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    delegate void OperationEndCallback(IAsyncResult result);
    delegate IAsyncResult OperationWithTimeoutBeginCallback(TimeSpan timeout, AsyncCallback asyncCallback, object asyncState);
    delegate void OperationWithTimeoutCallback(TimeSpan timeout);

    static class OperationWithTimeoutComposer
    {
        public static IAsyncResult BeginComposeAsyncOperations(
            TimeSpan timeout, OperationWithTimeoutBeginCallback[] beginOperations, OperationEndCallback[] endOperations,
            AsyncCallback callback, object state)
        {
            return new ComposedAsyncResult(timeout, beginOperations, endOperations, callback, state);
        }

        public static void EndComposeAsyncOperations(IAsyncResult result)
        {
            ComposedAsyncResult.End(result);
        }

        public static TimeSpan RemainingTime(IAsyncResult result)
        {
            return ((ComposedAsyncResult)result).RemainingTime();
        }

        class ComposedAsyncResult : AsyncResult
        {
            OperationWithTimeoutBeginCallback[] beginOperations;
            bool completedSynchronously = true;
            int currentOperation = 0;
            OperationEndCallback[] endOperations;
            TimeoutHelper timeoutHelper;
            static AsyncCallback onOperationCompleted = Fx.ThunkCallback(new AsyncCallback(OnOperationCompletedStatic));

            internal ComposedAsyncResult(
                TimeSpan timeout, OperationWithTimeoutBeginCallback[] beginOperations, OperationEndCallback[] endOperations,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.beginOperations = beginOperations;
                this.endOperations = endOperations;

                this.SkipToNextOperation();

                if (this.currentOperation < this.beginOperations.Length)
                {
                    this.beginOperations[this.currentOperation](this.RemainingTime(), onOperationCompleted, this);
                }
                else
                {
                    Complete(this.completedSynchronously);
                }
            }

            public TimeSpan RemainingTime()
            {
                return this.timeoutHelper.RemainingTime();
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<ComposedAsyncResult>(result);
            }

            void OnOperationCompleted(IAsyncResult result)
            {
                this.completedSynchronously = this.completedSynchronously && result.CompletedSynchronously;

                Exception exception = null;
                try
                {
                    this.endOperations[this.currentOperation](result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }
                if (exception != null)
                {
                    Complete(this.completedSynchronously, exception);
                    return;
                }

                this.currentOperation++;
                this.SkipToNextOperation();

                if (this.currentOperation < this.beginOperations.Length)
                {
                    try
                    {
                        this.beginOperations[this.currentOperation](this.RemainingTime(), onOperationCompleted, this);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        exception = e;
                    }
                    if (exception != null)
                    {
                        Complete(this.completedSynchronously, exception);
                        return;
                    }
                }
                else
                {
                    Complete(this.completedSynchronously);
                }
            }

            static void OnOperationCompletedStatic(IAsyncResult result)
            {
                ((ComposedAsyncResult)(result.AsyncState)).OnOperationCompleted(result);
            }

            void SkipToNextOperation()
            {
                while (this.currentOperation < this.beginOperations.Length)
                {
                    if (this.beginOperations[this.currentOperation] != default(OperationWithTimeoutBeginCallback))
                    {
                        return;
                    }

                    this.currentOperation++;
                }
            }
        }
    }

    sealed class Guard
    {
        ManualResetEvent closeEvent;
        int currentCount = 0;
        int maxCount;
        bool closed;
        object thisLock = new object();

        event WaitAsyncResult.SignaledHandler Signaled;

        public Guard()
            : this(1)
        {
        }

        public Guard(int maxCount)
        {
            this.maxCount = maxCount;
        }

        public void Abort()
        {
            this.closed = true;
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool complete = false;
            WaitAsyncResult result = null;

            lock (this.thisLock)
            {
                if (this.closed || this.currentCount == 0)
                {
                    complete = true;
                }
                else
                {
                    result = new WaitAsyncResult(timeout, true, callback, state);
                    this.Signaled += result.OnSignaled;
                }

                this.closed = true;
            }

            if (complete)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                result.Begin();
                return result;
            }
        }

        public void Close(TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                if (this.closed)
                    return;

                this.closed = true;

                if (this.currentCount > 0)
                    this.closeEvent = new ManualResetEvent(false);
            }

            if (this.closeEvent != null)
            {
                try
                {
                    if (!TimeoutHelper.WaitOne(this.closeEvent, timeout))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.TimeoutOnOperation, timeout)));
                }
                finally
                {
                    lock (this.thisLock)
                    {
                        this.closeEvent.Close();
                        this.closeEvent = null;
                    }
                }
            }
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
                CompletedAsyncResult.End(result);
            else
                WaitAsyncResult.End(result);
        }

        public bool Enter()
        {
            lock (this.thisLock)
            {
                if (this.closed)
                    return false;

                if (this.currentCount == this.maxCount)
                    return false;

                this.currentCount++;
                return true;
            }
        }

        public void Exit()
        {
            WaitAsyncResult.SignaledHandler handler = null;

            lock (this.thisLock)
            {
                this.currentCount--;

                if (this.currentCount < 0)
                {
                    throw Fx.AssertAndThrow("Exit can only be called after Enter.");
                }

                if (this.currentCount == 0)
                {
                    if (this.closeEvent != null)
                        this.closeEvent.Set();

                    handler = this.Signaled;
                }
            }

            if (handler != null)
                handler();
        }
    }

    class InterruptibleTimer
    {
        WaitCallback callback;
        bool aborted = false;
        TimeSpan defaultInterval;
        static Action<object> onTimerElapsed = new Action<object>(OnTimerElapsed);
        bool set = false;
        object state;
        object thisLock = new object();
        IOThreadTimer timer;

        public InterruptibleTimer(TimeSpan defaultInterval, WaitCallback callback, object state)
        {
            if (callback == null)
            {
                throw Fx.AssertAndThrow("Argument callback cannot be null.");
            }

            this.defaultInterval = defaultInterval;
            this.callback = callback;
            this.state = state;
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                this.aborted = true;

                if (this.set)
                {
                    this.timer.Cancel();
                    this.set = false;
                }
            }
        }

        public bool Cancel()
        {
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    return false;
                }

                if (this.set)
                {
                    this.timer.Cancel();
                    this.set = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        void OnTimerElapsed()
        {
            lock (this.ThisLock)
            {
                if (this.aborted)
                    return;

                this.set = false;
            }

            callback(state);
        }

        static void OnTimerElapsed(object state)
        {
            InterruptibleTimer interruptibleTimer = (InterruptibleTimer)state;
            interruptibleTimer.OnTimerElapsed();
        }

        public void Set()
        {
            this.Set(this.defaultInterval);
        }

        public void Set(TimeSpan interval)
        {
            this.InternalSet(interval, false);
        }

        public void SetIfNotSet()
        {
            this.InternalSet(this.defaultInterval, true);
        }

        void InternalSet(TimeSpan interval, bool ifNotSet)
        {
            lock (this.ThisLock)
            {
                if (this.aborted || (ifNotSet && this.set))
                    return;

                if (this.timer == null)
                    this.timer = new IOThreadTimer(onTimerElapsed, this, true);

                this.timer.Set(interval);
                this.set = true;
            }
        }
    }

    class InterruptibleWaitObject
    {
        bool aborted = false;
        CommunicationObject communicationObject;
        ManualResetEvent handle;
        bool set;
        int syncWaiters;
        object thisLock = new object();
        bool throwTimeoutByDefault = true;

        public InterruptibleWaitObject(bool signaled)
            : this(signaled, true)
        {
        }

        public InterruptibleWaitObject(bool signaled, bool throwTimeoutByDefault)
        {
            this.set = signaled;
            this.throwTimeoutByDefault = throwTimeoutByDefault;
        }

        event WaitAsyncResult.AbortHandler Aborted;
        event WaitAsyncResult.AbortHandler Faulted;
        event WaitAsyncResult.SignaledHandler Signaled;

        public void Abort(CommunicationObject communicationObject)
        {
            if (communicationObject == null)
            {
                throw Fx.AssertAndThrow("Argument communicationObject cannot be null.");
            }

            lock (this.thisLock)
            {
                if (this.aborted)
                    return;

                this.communicationObject = communicationObject;

                this.aborted = true;
                InternalSet();
            }

            WaitAsyncResult.AbortHandler handler = this.Aborted;

            if (handler != null)
                handler(communicationObject);
        }

        public void Fault(CommunicationObject communicationObject)
        {
            if (communicationObject == null)
            {
                throw Fx.AssertAndThrow("Argument communicationObject cannot be null.");
            }

            lock (this.thisLock)
            {
                if (this.aborted)
                    return;

                this.communicationObject = communicationObject;

                this.aborted = false;
                InternalSet();
            }

            WaitAsyncResult.AbortHandler handler = this.Faulted;

            if (handler != null)
                handler(communicationObject);
        }

        public IAsyncResult BeginWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginWait(timeout, this.throwTimeoutByDefault, callback, state);
        }

        public IAsyncResult BeginWait(TimeSpan timeout, bool throwTimeoutException, AsyncCallback callback, object state)
        {
            Exception e = null;

            lock (this.thisLock)
            {
                if (!this.set)
                {
                    WaitAsyncResult result = new WaitAsyncResult(timeout, throwTimeoutException, callback, state);

                    this.Aborted += result.OnAborted;
                    this.Faulted += result.OnFaulted;
                    this.Signaled += result.OnSignaled;
                    result.Begin();
                    return result;
                }
                else if (this.communicationObject != null)
                {
                    e = this.GetException();
                }
            }

            if (e != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        public IAsyncResult BeginTryWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginWait(timeout, false, callback, state);
        }

        public void EndWait(IAsyncResult result)
        {
            this.EndTryWait(result);
        }

        public bool EndTryWait(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
                return true;
            }
            else
            {
                return WaitAsyncResult.End(result);
            }
        }

        Exception GetException()
        {
            if (this.communicationObject == null)
            {
                Fx.Assert("Caller is attempting to retrieve an exception from a null communicationObject.");
            }

            return this.aborted
                ? this.communicationObject.CreateAbortedException()
                : this.communicationObject.GetTerminalException();
        }

        void InternalSet()
        {
            lock (this.thisLock)
            {
                set = true;

                if (this.handle != null)
                    this.handle.Set();
            }
        }

        public void Reset()
        {
            lock (this.thisLock)
            {
                communicationObject = null;
                aborted = false;
                set = false;

                if (this.handle != null)
                    this.handle.Reset();
            }
        }

        public void Set()
        {
            InternalSet();

            WaitAsyncResult.SignaledHandler handler = this.Signaled;
            if (handler != null)
                handler();
        }

        public bool Wait(TimeSpan timeout)
        {
            return this.Wait(timeout, this.throwTimeoutByDefault);
        }

        public bool Wait(TimeSpan timeout, bool throwTimeoutException)
        {
            lock (this.thisLock)
            {
                if (set)
                {
                    if (this.communicationObject != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetException());

                    return true;
                }

                if (this.handle == null)
                    this.handle = new ManualResetEvent(false);

                this.syncWaiters++;
            }

            try
            {
                if (!TimeoutHelper.WaitOne(this.handle, timeout))
                {
                    if (throwTimeoutException)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.TimeoutOnOperation, timeout)));
                    else
                        return false;
                }
            }
            finally
            {
                lock (this.thisLock)
                {
                    // Last one out turns off the light.
                    this.syncWaiters--;
                    if (this.syncWaiters == 0)
                    {
                        this.handle.Close();
                        this.handle = null;
                    }
                }
            }

            if (this.communicationObject != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetException());

            return true;
        }
    }

    abstract class FaultHelper
    {
        object thisLock = new object();

        protected FaultHelper()
        {
        }

        protected object ThisLock
        {
            get { return this.thisLock; }
        }

        public abstract void Abort();

        public static bool AddressReply(Message message, Message faultMessage)
        {
            try
            {
                RequestReplyCorrelator.PrepareReply(faultMessage, message);
            }
            catch (MessageHeaderException exception)
            {
                // ---- it - we don't need to correlate the reply if the MessageId header is bad
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }

            bool sendFault = true;
            try
            {
                sendFault = RequestReplyCorrelator.AddressReply(faultMessage, message);
            }
            catch (MessageHeaderException exception)
            {
                // ---- it - we don't need to address the reply if the addressing headers are bad
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }

            return sendFault;
        }

        public abstract IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract void Close(TimeSpan timeout);
        public abstract void EndClose(IAsyncResult result);
        public abstract void SendFaultAsync(IReliableChannelBinder binder, RequestContext requestContext, Message faultMessage);
    }

    abstract class TypedFaultHelper<TState> : FaultHelper
    {
        InterruptibleWaitObject closeHandle;
        TimeSpan defaultCloseTimeout;
        TimeSpan defaultSendTimeout;
        Dictionary<IReliableChannelBinder, TState> faultList = new Dictionary<IReliableChannelBinder, TState>();
        AsyncCallback onBinderCloseComplete;
        AsyncCallback onSendFaultComplete;
        Action<object> sendFaultCallback;

        protected TypedFaultHelper(TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout)
        {
            this.defaultSendTimeout = defaultSendTimeout;
            this.defaultCloseTimeout = defaultCloseTimeout;
        }

        public override void Abort()
        {
            Dictionary<IReliableChannelBinder, TState> tempFaultList;
            InterruptibleWaitObject tempCloseHandle;

            lock (this.ThisLock)
            {
                tempFaultList = this.faultList;
                this.faultList = null;
                tempCloseHandle = this.closeHandle;
            }

            if ((tempFaultList == null) || (tempFaultList.Count == 0))
            {
                if (tempCloseHandle != null)
                    tempCloseHandle.Set();
                return;
            }

            foreach (KeyValuePair<IReliableChannelBinder, TState> pair in tempFaultList)
            {
                this.AbortState(pair.Value, true);
                pair.Key.Abort();
            }

            if (tempCloseHandle != null)
                tempCloseHandle.Set();
        }

        void AbortBinder(IReliableChannelBinder binder)
        {
            try
            {
                binder.Abort();
            }
            finally
            {
                this.RemoveBinder(binder);
            }
        }

        void AsyncCloseBinder(IReliableChannelBinder binder)
        {
            if (this.onBinderCloseComplete == null)
                this.onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(this.OnBinderCloseComplete));

            IAsyncResult result = binder.BeginClose(this.defaultCloseTimeout, this.onBinderCloseComplete, binder);
            if (result.CompletedSynchronously)
                this.CompleteBinderClose(binder, result);
        }

        protected abstract void AbortState(TState state, bool isOnAbortThread);

        void AfterClose()
        {
            this.Abort();
        }

        bool BeforeClose()
        {
            lock (this.ThisLock)
            {
                if ((this.faultList == null) || (this.faultList.Count == 0))
                    return true;

                this.closeHandle = new InterruptibleWaitObject(false, false);
            }

            return false;
        }

        public override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.BeforeClose())
                return new AlreadyClosedAsyncResult(callback, state);
            else
                return this.closeHandle.BeginWait(timeout, callback, state);
        }

        protected abstract IAsyncResult BeginSendFault(IReliableChannelBinder binder, TState state, TimeSpan timeout,
            AsyncCallback callback, object asyncState);

        public override void Close(TimeSpan timeout)
        {
            if (this.BeforeClose())
                return;

            this.closeHandle.Wait(timeout);
            this.AfterClose();
        }

        void CompleteBinderClose(IReliableChannelBinder binder, IAsyncResult result)
        {
            try
            {
                binder.EndClose(result);
            }
            finally
            {
                this.RemoveBinder(binder);
            }
        }

        void CompleteSendFault(IReliableChannelBinder binder, TState state, IAsyncResult result)
        {
            bool throwing = true;

            try
            {
                this.EndSendFault(binder, state, result);
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    this.AbortState(state, false);
                    this.AbortBinder(binder);
                }
            }

            this.AsyncCloseBinder(binder);
        }

        public override void EndClose(IAsyncResult result)
        {
            if (result is AlreadyClosedAsyncResult)
                AlreadyClosedAsyncResult.End(result);
            else
                this.closeHandle.EndWait(result);

            this.AfterClose();
        }

        protected abstract void EndSendFault(IReliableChannelBinder binder, TState state, IAsyncResult result);
        protected abstract TState GetState(RequestContext requestContext, Message faultMessage);

        void OnBinderCloseComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IReliableChannelBinder binder = (IReliableChannelBinder)result.AsyncState;

            try
            {
                this.CompleteBinderClose(binder, result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                binder.HandleException(e);
            }
        }

        void OnSendFaultComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IReliableChannelBinder binder;
            TState state;

            lock (this.ThisLock)
            {
                if (this.faultList == null)
                    return;

                binder = (IReliableChannelBinder)result.AsyncState;
                state = this.faultList[binder];
            }

            try
            {
                this.CompleteSendFault(binder, state, result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                binder.HandleException(e);
            }
        }

        protected void RemoveBinder(IReliableChannelBinder binder)
        {
            InterruptibleWaitObject tempCloseHandle;

            lock (this.ThisLock)
            {
                if (this.faultList == null)
                    return;

                this.faultList.Remove(binder);
                if ((this.closeHandle == null) || (this.faultList.Count > 0))
                    return;

                // Close has been called.
                this.faultList = null;
                tempCloseHandle = this.closeHandle;
            }

            tempCloseHandle.Set();
        }

        protected void SendFault(IReliableChannelBinder binder, TState state)
        {
            IAsyncResult result;
            bool throwing = true;

            try
            {
                result = this.BeginSendFault(binder, state, this.defaultSendTimeout, this.onSendFaultComplete, binder);
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    this.AbortState(state, false);
                    this.AbortBinder(binder);
                }
            }

            if (result.CompletedSynchronously)
                this.CompleteSendFault(binder, state, result);
        }

        public override void SendFaultAsync(IReliableChannelBinder binder, RequestContext requestContext, Message faultMessage)
        {
            try
            {
                bool abort = true;
                TState state = this.GetState(requestContext, faultMessage);

                lock (this.ThisLock)
                {
                    if (this.faultList != null)
                    {
                        abort = false;
                        this.faultList.Add(binder, state);

                        if (this.onSendFaultComplete == null)
                            this.onSendFaultComplete = Fx.ThunkCallback(new AsyncCallback(this.OnSendFaultComplete));
                    }
                }

                if (abort)
                {
                    this.AbortState(state, false);
                    binder.Abort();
                }
                else if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    this.SendFault(binder, state);
                }
                else
                {
                    if (this.sendFaultCallback == null)
                    {
                        this.sendFaultCallback = new Action<object>(this.SendFaultCallback);
                    }
                    ActionItem.Schedule(this.sendFaultCallback, binder);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                binder.HandleException(e);
            }
        }

        void SendFaultCallback(object callbackState)
        {
            IReliableChannelBinder binder;
            TState state;

            lock (this.ThisLock)
            {
                if (this.faultList == null)
                    return;

                binder = (IReliableChannelBinder)callbackState;
                state = this.faultList[binder];
            }

            try
            {
                this.SendFault(binder, state);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                binder.HandleException(e);
            }
        }

        class AlreadyClosedAsyncResult : CompletedAsyncResult
        {
            public AlreadyClosedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }
        }
    }

    struct FaultState
    {
        Message faultMessage;
        RequestContext requestContext;

        public FaultState(RequestContext requestContext, Message faultMessage)
        {
            this.requestContext = requestContext;
            this.faultMessage = faultMessage;
        }

        public Message FaultMessage { get { return this.faultMessage; } }
        public RequestContext RequestContext { get { return this.requestContext; } }
    }

    class ReplyFaultHelper : TypedFaultHelper<FaultState>
    {
        public ReplyFaultHelper(TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout)
            : base(defaultSendTimeout, defaultCloseTimeout)
        {
        }

        protected override void AbortState(FaultState faultState, bool isOnAbortThread)
        {
            // if abort is true, the request could be in the middle of the encoding step, let the sending thread clean up.
            if (!isOnAbortThread)
            {
                faultState.FaultMessage.Close();
            }
            faultState.RequestContext.Abort();
        }

        protected override IAsyncResult BeginSendFault(IReliableChannelBinder binder, FaultState faultState,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return faultState.RequestContext.BeginReply(faultState.FaultMessage, timeout, callback, state);
        }

        protected override void EndSendFault(IReliableChannelBinder binder, FaultState faultState, IAsyncResult result)
        {
            faultState.RequestContext.EndReply(result);
            faultState.FaultMessage.Close();
        }

        protected override FaultState GetState(RequestContext requestContext, Message faultMessage)
        {
            return new FaultState(requestContext, faultMessage);
        }

    }

    class SendFaultHelper : TypedFaultHelper<Message>
    {
        public SendFaultHelper(TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout)
            : base(defaultSendTimeout, defaultCloseTimeout)
        {
        }

        protected override void AbortState(Message message, bool isOnAbortThread)
        {
            // if abort is true, the request could be in the middle of the encoding step, let the sending thread clean up.
            if (!isOnAbortThread)
            {
                message.Close();
            }
        }

        protected override IAsyncResult BeginSendFault(IReliableChannelBinder binder, Message message,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return binder.BeginSend(message, timeout, callback, state);
        }

        protected override void EndSendFault(IReliableChannelBinder binder, Message message, IAsyncResult result)
        {
            binder.EndSend(result);
            message.Close();
        }

        protected override Message GetState(RequestContext requestContext, Message faultMessage)
        {
            return faultMessage;
        }
    }

    class ReliableChannelCloseAsyncResult : AsyncResult
    {
        IReliableChannelBinder binder;
        static AsyncCallback onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnBinderCloseComplete));
        static AsyncCallback onComposeAsyncOperationsComplete = Fx.ThunkCallback(new AsyncCallback(OnComposeAsyncOperationsComplete));
        TimeoutHelper timeoutHelper;

        public ReliableChannelCloseAsyncResult(OperationWithTimeoutBeginCallback[] beginCallbacks,
            OperationEndCallback[] endCallbacks, IReliableChannelBinder binder, TimeSpan timeout,
            AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.binder = binder;
            this.timeoutHelper = new TimeoutHelper(timeout);

            IAsyncResult result = OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeoutHelper.RemainingTime(),
                beginCallbacks, endCallbacks, onComposeAsyncOperationsComplete, this);

            if (result.CompletedSynchronously)
            {
                if (this.CompleteComposeAsyncOperations(result))
                {
                    this.Complete(true);
                }
            }
        }

        bool CompleteComposeAsyncOperations(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);

            result = this.binder.BeginClose(this.timeoutHelper.RemainingTime(),
                MaskingMode.Handled, onBinderCloseComplete, this);

            if (result.CompletedSynchronously)
            {
                this.binder.EndClose(result);
                return true;
            }

            return false;
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReliableChannelCloseAsyncResult>(result);
        }

        static void OnBinderCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelCloseAsyncResult closeResult =
                    (ReliableChannelCloseAsyncResult)result.AsyncState;
                Exception completeException = null;

                try
                {
                    closeResult.binder.EndClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                closeResult.Complete(false, completeException);
            }
        }

        static void OnComposeAsyncOperationsComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelCloseAsyncResult closeResult =
                    (ReliableChannelCloseAsyncResult)result.AsyncState;

                bool complete = false;
                Exception completeException = null;

                try
                {
                    complete = closeResult.CompleteComposeAsyncOperations(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete || completeException != null)
                {
                    closeResult.Complete(false, completeException);
                }
            }
        }
    }

    class ReliableChannelOpenAsyncResult : AsyncResult
    {
        IReliableChannelBinder binder;
        static AsyncCallback onBinderOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnBinderOpenComplete));
        static AsyncCallback onSessionOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnSessionOpenComplete));
        ChannelReliableSession session;
        TimeoutHelper timeoutHelper;

        public ReliableChannelOpenAsyncResult(IReliableChannelBinder binder,
            ChannelReliableSession session, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.binder = binder;
            this.session = session;
            this.timeoutHelper = new TimeoutHelper(timeout);

            bool complete = false;
            bool throwing = true;
            Exception completeException = null;

            try
            {
                IAsyncResult result = this.binder.BeginOpen(timeoutHelper.RemainingTime(), onBinderOpenComplete, this);
                throwing = false;
                if (result.CompletedSynchronously)
                {
                    complete = this.CompleteBinderOpen(true, result);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (throwing || this.CloseBinder(completeException))
                    throw;
            }
            finally
            {
                if (throwing)
                    this.binder.Abort();
            }

            if (complete)
                this.Complete(true);
        }

        bool CloseBinder(Exception e)
        {
            IAsyncResult result = this.binder.BeginClose(this.timeoutHelper.RemainingTime(),
                Fx.ThunkCallback(new AsyncCallback(this.OnBinderCloseComplete)), e);

            if (result.CompletedSynchronously)
            {
                this.binder.EndClose(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        void CloseBinderAndComplete(Exception e)
        {
            bool complete = true;

            try
            {
                complete = this.CloseBinder(e);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                    throw;
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
            }

            if (complete)
                this.Complete(false, e);

        }

        bool CompleteBinderOpen(bool synchronous, IAsyncResult result)
        {
            this.binder.EndOpen(result);
            result = this.session.BeginOpen(this.timeoutHelper.RemainingTime(),
                onSessionOpenComplete, this);

            if (result.CompletedSynchronously)
            {
                this.session.EndOpen(result);
                return true;
            }

            return false;
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReliableChannelOpenAsyncResult>(result);
        }

        void OnBinderCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                Exception completeException = (Exception)result.AsyncState;

                try
                {
                    this.binder.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    if (DiagnosticUtility.ShouldTraceInformation)
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }

                this.Complete(false, completeException);
            }
        }

        static void OnBinderOpenComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelOpenAsyncResult openResult =
                    (ReliableChannelOpenAsyncResult)result.AsyncState;

                bool complete = false;
                Exception completeException = null;

                try
                {
                    complete = openResult.CompleteBinderOpen(false, result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (complete)
                    openResult.Complete(false, completeException);
                else if (completeException != null)
                    openResult.CloseBinderAndComplete(completeException);
            }
        }

        static void OnSessionOpenComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelOpenAsyncResult openResult =
                    (ReliableChannelOpenAsyncResult)result.AsyncState;

                Exception completeException = null;

                try
                {
                    openResult.session.EndOpen(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (completeException != null)
                    openResult.CloseBinderAndComplete(completeException);
                else
                    openResult.Complete(false);
            }
        }
    }


    static class ReliableMessagingConstants
    {
        static public TimeSpan UnknownInitiationTime = TimeSpan.FromSeconds(2);
        static public TimeSpan RequestorIterationTime = TimeSpan.FromSeconds(10);
        static public TimeSpan RequestorReceiveTime = TimeSpan.FromSeconds(10);
        static public int MaxSequenceRanges = 128;
    }

    // This class and its derivates attempt to unify 3 similar request reply patterns.
    // 1. Straightforward R/R pattern
    // 2. R/R pattern with binder and exception semantics on Open (CreateSequence)
    // 3. TerminateSequence request - TerminateSequence response for R(Request|Reply)SC
    abstract class ReliableRequestor
    {
        InterruptibleWaitObject abortHandle = new InterruptibleWaitObject(false, false);
        IReliableChannelBinder binder;
        bool isCreateSequence;
        ActionHeader messageAction;
        BodyWriter messageBody;
        WsrmMessageHeader messageHeader;
        UniqueId messageId;
        MessageVersion messageVersion;
        TimeSpan originalTimeout;
        string timeoutString1Index;

        public IReliableChannelBinder Binder
        {
            protected get { return this.binder; }
            set { this.binder = value; }
        }

        public bool IsCreateSequence
        {
            protected get { return this.isCreateSequence; }
            set { this.isCreateSequence = value; }
        }

        public ActionHeader MessageAction
        {
            set { this.messageAction = value; }
        }

        public BodyWriter MessageBody
        {
            set { this.messageBody = value; }
        }

        public UniqueId MessageId
        {
            get { return this.messageId; }
        }

        public WsrmMessageHeader MessageHeader
        {
            get { return this.messageHeader; }
            set { this.messageHeader = value; }
        }

        public MessageVersion MessageVersion
        {
            set { this.messageVersion = value; }
        }

        public string TimeoutString1Index
        {
            set { this.timeoutString1Index = value; }
        }

        public void Abort(CommunicationObject communicationObject)
        {
            this.abortHandle.Abort(communicationObject);
        }

        Message CreateRequestMessage()
        {
            Message request = Message.CreateMessage(this.messageVersion, this.messageAction, this.messageBody);
            request.Properties.AllowOutputBatching = false;

            if (this.messageHeader != null)
            {
                request.Headers.Insert(0, this.messageHeader);
            }

            if (this.messageId != null)
            {
                request.Headers.MessageId = this.messageId;
                RequestReplyCorrelator.PrepareRequest(request);

                EndpointAddress address = this.binder.LocalAddress;

                if (address == null)
                {
                    request.Headers.ReplyTo = null;
                }
                else if (this.messageVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    request.Headers.ReplyTo = address;
                }
                else if (this.messageVersion.Addressing == AddressingVersion.WSAddressing10)
                {
                    request.Headers.ReplyTo = address.IsAnonymous ? null : address;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, this.messageVersion.Addressing)));
                }
            }

            return request;
        }

        bool EnsureChannel()
        {
            if (this.IsCreateSequence)
            {
                IClientReliableChannelBinder clientBinder = (IClientReliableChannelBinder)this.binder;
                return clientBinder.EnsureChannelForRequest();
            }
            else
            {
                return true;
            }
        }

        public virtual void Fault(CommunicationObject communicationObject)
        {
            this.abortHandle.Fault(communicationObject);
        }

        public abstract WsrmMessageInfo GetInfo();

        TimeSpan GetNextRequestTimeout(TimeSpan remainingTimeout, out TimeoutHelper iterationTimeout, out bool lastIteration)
        {
            iterationTimeout = new TimeoutHelper(ReliableMessagingConstants.RequestorIterationTime);
            lastIteration = remainingTimeout <= iterationTimeout.RemainingTime();
            return remainingTimeout;
        }

        bool HandleException(Exception exception, bool lastIteration)
        {
            if (this.IsCreateSequence)
            {
                if (exception is QuotaExceededException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CommunicationException(exception.Message, exception));
                }

                if (!this.binder.IsHandleable(exception)
                    || exception is MessageSecurityException
                    || exception is SecurityNegotiationException
                    || exception is SecurityAccessDeniedException
                    || (this.binder.State != CommunicationState.Opened)
                    || lastIteration)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return this.binder.IsHandleable(exception);
            }
        }

        void ThrowTimeoutException()
        {
            if (this.timeoutString1Index != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TimeoutException(SR.GetString(this.timeoutString1Index, this.originalTimeout)));
            }
        }

        protected abstract Message OnRequest(Message request, TimeSpan timeout, bool last);
        protected abstract IAsyncResult OnBeginRequest(Message request, TimeSpan timeout,
            AsyncCallback callback, object state);
        protected abstract Message OnEndRequest(bool last, IAsyncResult result);

        public Message Request(TimeSpan timeout)
        {
            this.originalTimeout = timeout;
            TimeoutHelper timeoutHelper = new TimeoutHelper(this.originalTimeout);
            TimeoutHelper iterationTimeoutHelper;
            bool lastIteration;

            while (true)
            {
                Message request = null;
                Message reply = null;
                bool requestCompleted = false;
                TimeSpan requestTimeout = this.GetNextRequestTimeout(timeoutHelper.RemainingTime(),
                    out iterationTimeoutHelper, out lastIteration);

                try
                {
                    if (this.EnsureChannel())
                    {
                        request = this.CreateRequestMessage();
                        reply = this.OnRequest(request, requestTimeout, lastIteration);
                        requestCompleted = true;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || !this.HandleException(e, lastIteration))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (request != null)
                    {
                        request.Close();
                    }
                }

                if (requestCompleted)
                {
                    if (this.ValidateReply(reply))
                    {
                        return reply;
                    }
                }

                if (lastIteration)
                    break;

                this.abortHandle.Wait(iterationTimeoutHelper.RemainingTime());
            }

            this.ThrowTimeoutException();
            return null;
        }

        public IAsyncResult BeginRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new RequestAsyncResult(this, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return RequestAsyncResult.End(result);
        }

        public abstract void SetInfo(WsrmMessageInfo info);

        public void SetRequestResponsePattern()
        {
            if (this.messageId != null)
            {
                throw Fx.AssertAndThrow("Initialize messageId only once.");
            }

            this.messageId = new UniqueId();
        }

        bool ValidateReply(Message response)
        {
            if (this.messageId != null)
            {
                // r/r pattern requires a response
                return response != null;
            }
            else
            {
                return true;
            }
        }

        class RequestAsyncResult : AsyncResult
        {
            static AsyncCallback requestCallback = Fx.ThunkCallback(new AsyncCallback(RequestCallback));
            static AsyncCallback waitCallback = Fx.ThunkCallback(new AsyncCallback(RequestAsyncResult.WaitCallback));
            TimeoutHelper iterationTimeoutHelper;
            bool lastIteration = false;
            Message request;
            ReliableRequestor requestor;
            Message response;
            TimeoutHelper timeoutHelper;

            public RequestAsyncResult(ReliableRequestor requestor, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.requestor = requestor;
                this.requestor.originalTimeout = timeout;
                this.timeoutHelper = new TimeoutHelper(this.requestor.originalTimeout);

                if (this.Request(null))
                {
                    this.Complete(true);
                }
            }

            public static Message End(IAsyncResult result)
            {
                RequestAsyncResult requestResult = AsyncResult.End<RequestAsyncResult>(result);
                return requestResult.response;
            }

            bool Request(IAsyncResult requestResult)
            {
                while (true)
                {
                    bool requestCompleted = false;
                    bool disposeRequest = true;

                    TimeSpan requestTimeout = (requestResult == null)
                        ? this.requestor.GetNextRequestTimeout(this.timeoutHelper.RemainingTime(),
                            out this.iterationTimeoutHelper, out this.lastIteration)
                        : TimeSpan.Zero;

                    try
                    {
                        if (requestResult == null)
                        {
                            if (this.requestor.EnsureChannel())
                            {
                                this.request = this.requestor.CreateRequestMessage();
                                requestResult = this.requestor.OnBeginRequest(this.request, requestTimeout,
                                    requestCallback, this);

                                if (!requestResult.CompletedSynchronously)
                                {
                                    disposeRequest = false;
                                    return false;
                                }
                            }
                        }

                        if (requestResult != null)
                        {
                            this.response = this.requestor.OnEndRequest(this.lastIteration, requestResult);
                            requestCompleted = true;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e) || !this.requestor.HandleException(e, lastIteration))
                        {
                            throw;
                        }

                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    finally
                    {
                        if (disposeRequest && this.request != null)
                        {
                            this.request.Close();
                            this.request = null;
                        }

                        requestResult = null;
                    }

                    if (requestCompleted)
                    {
                        if (this.requestor.ValidateReply(this.response))
                        {
                            return true;
                        }
                    }

                    if (this.lastIteration)
                    {
                        break;
                    }

                    IAsyncResult waitResult = this.requestor.abortHandle.BeginWait(
                        iterationTimeoutHelper.RemainingTime(), waitCallback, this);

                    if (!waitResult.CompletedSynchronously)
                    {
                        return false;
                    }
                    else
                    {
                        this.requestor.abortHandle.EndWait(waitResult);
                    }
                }

                this.requestor.ThrowTimeoutException();
                return true;
            }

            static void RequestCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    RequestAsyncResult requestResult = (RequestAsyncResult)result.AsyncState;
                    bool complete;
                    Exception completeException;

                    try
                    {
                        complete = requestResult.Request(result);
                        completeException = null;
                    }
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
                        requestResult.Complete(false, completeException);
                    }
                }
            }

            bool EndWait(IAsyncResult result)
            {
                this.requestor.abortHandle.EndWait(result);
                return this.Request(null);
            }

            static void WaitCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    RequestAsyncResult requestResult = (RequestAsyncResult)result.AsyncState;
                    bool complete;
                    Exception completeException;

                    try
                    {
                        complete = requestResult.EndWait(result);
                        completeException = null;
                    }
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
                        requestResult.Complete(false, completeException);
                    }
                }
            }
        }
    }

    sealed class RequestReliableRequestor : ReliableRequestor
    {
        bool replied = false;
        WsrmMessageInfo replyInfo;
        object thisLock = new object();

        IClientReliableChannelBinder ClientBinder
        {
            get { return (IClientReliableChannelBinder)this.Binder; }
        }

        object ThisLock
        {
            get { return this.thisLock; }
        }

        public override WsrmMessageInfo GetInfo()
        {
            return this.replyInfo;
        }

        Message GetReply(Message reply, bool last)
        {
            lock (this.ThisLock)
            {
                if (reply != null && this.replyInfo != null)
                {
                    this.replyInfo = null;
                }
                else if (reply == null && this.replyInfo != null)
                {
                    reply = this.replyInfo.Message;
                }

                if (reply != null || last)
                {
                    this.replied = true;
                }
            }

            return reply;
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            return this.GetReply(this.ClientBinder.Request(request, timeout, MaskingMode.None), last);
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.ClientBinder.BeginRequest(request, timeout, MaskingMode.None, callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            return this.GetReply(this.ClientBinder.EndRequest(result), last);
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            lock (this.ThisLock)
            {
                if (!this.replied && this.replyInfo == null)
                {
                    this.replyInfo = info;
                }
            }
        }
    }

    sealed class SendReceiveReliableRequestor : ReliableRequestor
    {
        bool timeoutIsSafe;

        public bool TimeoutIsSafe
        {
            set { this.timeoutIsSafe = value; }
        }

        public override WsrmMessageInfo GetInfo()
        {
            throw Fx.AssertAndThrow("Not Supported.");
        }

        TimeSpan GetReceiveTimeout(TimeSpan timeoutRemaining)
        {
            if ((timeoutRemaining < ReliableMessagingConstants.RequestorReceiveTime) || !this.timeoutIsSafe)
            {
                return timeoutRemaining;
            }
            else
            {
                return ReliableMessagingConstants.RequestorReceiveTime;
            }
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            this.Binder.Send(request, timeoutHelper.RemainingTime(), MaskingMode.None);
            TimeSpan receiveTimeout = this.GetReceiveTimeout(timeoutHelper.RemainingTime());

            RequestContext requestContext;
            this.Binder.TryReceive(receiveTimeout, out requestContext, MaskingMode.None);
            return (requestContext != null) ? requestContext.RequestMessage : null;
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SendReceiveAsyncResult(this, request, timeout, callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            return SendReceiveAsyncResult.End(result);
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            throw Fx.AssertAndThrow("Not Supported.");
        }

        class SendReceiveAsyncResult : AsyncResult
        {
            static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendCallback));
            static AsyncCallback tryReceiveCallback = Fx.ThunkCallback(new AsyncCallback(TryReceiveCallback));

            Message request;
            SendReceiveReliableRequestor requestor;
            Message response;
            TimeoutHelper timeoutHelper;

            internal SendReceiveAsyncResult(SendReceiveReliableRequestor requestor, Message request, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.requestor = requestor;
                this.request = request;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (this.BeginSend())
                {
                    this.Complete(true);
                }
            }

            bool BeginSend()
            {
                IAsyncResult sendResult = this.requestor.Binder.BeginSend(this.request,
                    this.timeoutHelper.RemainingTime(), MaskingMode.None, sendCallback, this);

                if (sendResult.CompletedSynchronously)
                {
                    return this.EndSend(sendResult);
                }
                else
                {
                    return false;
                }
            }

            public static Message End(IAsyncResult result)
            {
                SendReceiveAsyncResult sendReceiveResult = AsyncResult.End<SendReceiveAsyncResult>(result);
                return sendReceiveResult.response;
            }

            bool EndSend(IAsyncResult result)
            {
                this.requestor.Binder.EndSend(result);

                TimeSpan receiveTimeout = this.requestor.GetReceiveTimeout(this.timeoutHelper.RemainingTime());
                IAsyncResult tryReceiveResult = this.requestor.Binder.BeginTryReceive(receiveTimeout,
                    MaskingMode.None, tryReceiveCallback, this);

                if (tryReceiveResult.CompletedSynchronously)
                {
                    return this.EndTryReceive(tryReceiveResult);
                }
                else
                {
                    return false;
                }
            }

            bool EndTryReceive(IAsyncResult result)
            {
                RequestContext requestContext;
                this.requestor.Binder.EndTryReceive(result, out requestContext);
                this.response = (requestContext != null) ? requestContext.RequestMessage : null;
                return true;
            }

            static void SendCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SendReceiveAsyncResult sendReceiveResult = (SendReceiveAsyncResult)result.AsyncState;
                    Exception completeException;
                    bool complete = false;

                    try
                    {
                        complete = sendReceiveResult.EndSend(result);
                        completeException = null;
                    }
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
                        sendReceiveResult.Complete(false, completeException);
                    }
                }
            }

            static void TryReceiveCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SendReceiveAsyncResult sendReceiveResult = (SendReceiveAsyncResult)result.AsyncState;
                    Exception completeException;
                    bool complete = false;

                    try
                    {
                        complete = sendReceiveResult.EndTryReceive(result);
                        completeException = null;
                    }
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
                        sendReceiveResult.Complete(false, completeException);
                    }
                }
            }
        }
    }

    sealed class SendWaitReliableRequestor : ReliableRequestor
    {
        bool replied = false;
        InterruptibleWaitObject replyHandle = new InterruptibleWaitObject(false, true);
        WsrmMessageInfo replyInfo;
        Message request;
        object thisLock = new object();

        object ThisLock
        {
            get { return this.thisLock; }
        }

        public override void Fault(CommunicationObject communicationObject)
        {
            this.replied = true;
            this.replyHandle.Fault(communicationObject);
            base.Fault(communicationObject);
        }

        public override WsrmMessageInfo GetInfo()
        {
            return this.replyInfo;
        }

        Message GetReply(bool last)
        {
            lock (this.ThisLock)
            {
                if (this.replyInfo != null)
                {
                    this.replied = true;
                    return this.replyInfo.Message;
                }
                else if (last)
                {
                    this.replied = true;
                }
            }

            return null;
        }

        TimeSpan GetWaitTimeout(TimeSpan timeoutRemaining)
        {
            if ((timeoutRemaining < ReliableMessagingConstants.RequestorReceiveTime))
            {
                return timeoutRemaining;
            }
            else
            {
                return ReliableMessagingConstants.RequestorReceiveTime;
            }
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.Binder.Send(request, timeoutHelper.RemainingTime(), MaskingMode.None);
            TimeSpan waitTimeout = this.GetWaitTimeout(timeoutHelper.RemainingTime());
            this.replyHandle.Wait(waitTimeout);
            return this.GetReply(last);
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.request = request;

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                new OperationWithTimeoutBeginCallback[] {
                    new OperationWithTimeoutBeginCallback(BeginSend),
                    new OperationWithTimeoutBeginCallback(BeginWait) },
                new OperationEndCallback[] {
                    new OperationEndCallback(EndSend),
                    new OperationEndCallback(EndWait) },
                callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            return this.GetReply(last);
        }

        IAsyncResult BeginSend(TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return this.Binder.BeginSend(this.request, timeout, MaskingMode.None, callback, state);
            }
            finally
            {
                this.request = null;
            }
        }

        void EndSend(IAsyncResult result)
        {
            this.Binder.EndSend(result);
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            lock (this.ThisLock)
            {
                if (this.replied || this.replyInfo != null)
                {
                    return;
                }

                this.replyInfo = info;
            }

            this.replyHandle.Set();
        }

        IAsyncResult BeginWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeSpan waitTimeout = this.GetWaitTimeout(timeout);
            return this.replyHandle.BeginWait(waitTimeout, callback, state);
        }

        void EndWait(IAsyncResult result)
        {
            this.replyHandle.EndWait(result);
        }
    }

    abstract class ReliableOutputAsyncResult : AsyncResult
    {
        IReliableChannelBinder binder;
        Exception handledException;
        MaskingMode maskingMode;
        MessageAttemptInfo messageAttemptInfo;
        static AsyncCallback operationCallback = Fx.ThunkCallback(new AsyncCallback(OperationCallback));
        bool saveHandledException;

        protected ReliableOutputAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public IReliableChannelBinder Binder
        {
            protected get { return this.binder; }
            set { this.binder = value; }
        }

        protected Exception HandledException
        {
            get { return this.handledException; }
        }

        public MaskingMode MaskingMode
        {
            get { return this.maskingMode; }
            set { this.maskingMode = value; }
        }

        public MessageAttemptInfo MessageAttemptInfo
        {
            get { return this.messageAttemptInfo; }
            set { this.messageAttemptInfo = value; }
        }

        public Message Message
        {
            protected get { return this.messageAttemptInfo.Message; }
            set { this.messageAttemptInfo = new MessageAttemptInfo(value, 0, 0, null); }
        }

        public bool SaveHandledException
        {
            set { this.saveHandledException = value; }
        }

        public void Begin(TimeSpan timeout)
        {
            bool complete;

            if (this.saveHandledException)
            {
                complete = this.BeginInternal(timeout);
            }
            else
            {
                try
                {
                    complete = this.BeginInternal(timeout);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e) || !this.HandleException(e))
                    {
                        throw;
                    }

                    complete = true;
                }
            }

            if (complete)
            {
                this.Complete(true);
            }
        }

        bool BeginInternal(TimeSpan timeout)
        {
            bool closeMessage = true;

            try
            {
                IAsyncResult operationResult = this.BeginOperation(timeout, operationCallback, this);

                if (operationResult.CompletedSynchronously)
                {
                    this.EndOperation(operationResult);
                    return true;
                }
                else
                {
                    closeMessage = false;
                    return false;
                }
            }
            finally
            {
                if (closeMessage)
                {
                    this.Message.Close();
                }
            }
        }

        protected abstract IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void EndOperation(IAsyncResult result);

        static void OperationCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputAsyncResult outputResult = (ReliableOutputAsyncResult)result.AsyncState;
                Exception completeException = null;

                try
                {
                    outputResult.EndOperation(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (!outputResult.HandleException(e))
                    {
                        completeException = e;
                    }
                }
                finally
                {
                    outputResult.Message.Close();
                }

                outputResult.Complete(false, completeException);
            }
        }

        bool HandleException(Exception e)
        {
            if (this.saveHandledException && this.Binder.IsHandleable(e))
            {
                this.handledException = e;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class ReliableBinderSendAsyncResult : ReliableOutputAsyncResult
    {
        public ReliableBinderSendAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            Exception handledException;
            End(result, out handledException);
        }

        public static void End(IAsyncResult result, out Exception handledException)
        {
            ReliableBinderSendAsyncResult sendResult = AsyncResult.End<ReliableBinderSendAsyncResult>(result);
            handledException = sendResult.HandledException;
        }

        protected override IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.Binder.BeginSend(this.Message, timeout, this.MaskingMode, callback, state);
        }

        protected override void EndOperation(IAsyncResult result)
        {
            this.Binder.EndSend(result);
        }
    }

    class ReliableBinderRequestAsyncResult : ReliableOutputAsyncResult
    {
        Message reply;

        public ReliableBinderRequestAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        protected IClientReliableChannelBinder ClientBinder
        {
            get { return (IClientReliableChannelBinder)this.Binder; }
        }

        protected Message Reply
        {
            get { return this.reply; }
        }

        public static Message End(IAsyncResult result)
        {
            Exception handledException;
            return End(result, out handledException);
        }

        public static Message End(IAsyncResult result, out Exception handledException)
        {
            ReliableBinderRequestAsyncResult requestResult = AsyncResult.End<ReliableBinderRequestAsyncResult>(result);
            handledException = requestResult.HandledException;
            return requestResult.reply;
        }

        protected override IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.ClientBinder.BeginRequest(this.Message, timeout, this.MaskingMode, callback, state);
        }

        protected override void EndOperation(IAsyncResult result)
        {
            this.reply = this.ClientBinder.EndRequest(result);
        }
    }

    class WaitAsyncResult : AsyncResult
    {
        bool completed;
        bool throwTimeoutException;
        bool timedOut;
        TimeSpan timeout;
        IOThreadTimer timer;
        object thisLock = new object();

        public delegate void AbortHandler(CommunicationObject communicationObject);
        public delegate void SignaledHandler();

        public WaitAsyncResult(TimeSpan timeout, bool throwTimeoutException, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.timeout = timeout;
            this.throwTimeoutException = throwTimeoutException;
        }

        public void Begin()
        {
            lock (this.thisLock)
            {
                if (this.completed)
                    return;

                if (this.timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(new Action<object>(OnTimerElapsed), null, true);
                    this.timer.Set(this.timeout);
                }
            }
        }

        public static bool End(IAsyncResult result)
        {
            return !AsyncResult.End<WaitAsyncResult>(result).timedOut;
        }

        protected virtual string GetTimeoutString(TimeSpan timeout)
        {
            return SR.GetString(SR.TimeoutOnOperation, timeout);
        }

        public void OnAborted(CommunicationObject communicationObject)
        {
            if (this.ShouldComplete(false))
                Complete(false, communicationObject.CreateClosedException());
        }

        public void OnFaulted(CommunicationObject communicationObject)
        {
            if (this.ShouldComplete(false))
                Complete(false, communicationObject.GetTerminalException());
        }

        public void OnSignaled()
        {
            if (this.ShouldComplete(false))
                Complete(false);
        }

        protected virtual void OnTimerElapsed(object state)
        {
            if (this.ShouldComplete(true))
            {
                if (this.throwTimeoutException)
                    Complete(false, new TimeoutException(this.GetTimeoutString(this.timeout)));
                else
                    Complete(false);
            }
        }

        bool ShouldComplete(bool timedOut)
        {
            lock (this.thisLock)
            {
                if (!this.completed)
                {
                    this.completed = true;
                    this.timedOut = timedOut;
                    if (!timedOut && (this.timer != null))
                    {
                        this.timer.Cancel();
                    }

                    return true;
                }
            }

            return false;
        }
    }

    abstract class WsrmIndex
    {
        static WsrmFeb2005Index wsAddressingAug2004WSReliableMessagingFeb2005;
        static WsrmFeb2005Index wsAddressing10WSReliableMessagingFeb2005;
        static Wsrm11Index wsAddressingAug2004WSReliableMessaging11;
        static Wsrm11Index wsAddressing10WSReliableMessaging11;

        internal static ActionHeader GetAckRequestedActionHeader(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, WsrmFeb2005Strings.AckRequested);
        }

        protected abstract ActionHeader GetActionHeader(string element);

        static ActionHeader GetActionHeader(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion, string element)
        {
            WsrmIndex cache = null;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    if (wsAddressingAug2004WSReliableMessagingFeb2005 == null)
                    {
                        wsAddressingAug2004WSReliableMessagingFeb2005 = new WsrmFeb2005Index(addressingVersion);
                    }

                    cache = wsAddressingAug2004WSReliableMessagingFeb2005;
                }
                else if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    if (wsAddressing10WSReliableMessagingFeb2005 == null)
                    {
                        wsAddressing10WSReliableMessagingFeb2005 = new WsrmFeb2005Index(addressingVersion);
                    }

                    cache = wsAddressing10WSReliableMessagingFeb2005;
                }
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    if (wsAddressingAug2004WSReliableMessaging11 == null)
                    {
                        wsAddressingAug2004WSReliableMessaging11 = new Wsrm11Index(addressingVersion);
                    }

                    cache = wsAddressingAug2004WSReliableMessaging11;
                }
                else if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    if (wsAddressing10WSReliableMessaging11 == null)
                    {
                        wsAddressing10WSReliableMessaging11 = new Wsrm11Index(addressingVersion);
                    }

                    cache = wsAddressing10WSReliableMessaging11;
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }

            if (cache == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, addressingVersion)));
            }

            return cache.GetActionHeader(element);
        }

        internal static ActionHeader GetCloseSequenceActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11, Wsrm11Strings.CloseSequence);
        }

        internal static ActionHeader GetCloseSequenceResponseActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11, Wsrm11Strings.CloseSequenceResponse);
        }

        internal static ActionHeader GetCreateSequenceActionHeader(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, WsrmFeb2005Strings.CreateSequence);
        }

        internal static string GetCreateSequenceActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Strings.CreateSequenceAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.CreateSequenceAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static XmlDictionaryString GetCreateSequenceResponseAction(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return XD.WsrmFeb2005Dictionary.CreateSequenceResponseAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return DXD.Wsrm11Dictionary.CreateSequenceResponseAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static string GetCreateSequenceResponseActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Strings.CreateSequenceResponseAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.CreateSequenceResponseAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static string GetFaultActionString(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return addressingVersion.DefaultFaultAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.FaultAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static XmlDictionaryString GetNamespace(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return XD.WsrmFeb2005Dictionary.Namespace;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return DXD.Wsrm11Dictionary.Namespace;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static string GetNamespaceString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Strings.Namespace;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.Namespace;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static ActionHeader GetSequenceAcknowledgementActionHeader(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, WsrmFeb2005Strings.SequenceAcknowledgement);
        }

        internal static string GetSequenceAcknowledgementActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Strings.SequenceAcknowledgementAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.SequenceAcknowledgementAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static MessagePartSpecification GetSignedReliabilityMessageParts(
            ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Index.SignedReliabilityMessageParts;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Index.SignedReliabilityMessageParts;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static ActionHeader GetTerminateSequenceActionHeader(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, WsrmFeb2005Strings.TerminateSequence);
        }

        internal static string GetTerminateSequenceActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Strings.TerminateSequenceAction;
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.TerminateSequenceAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static string GetTerminateSequenceResponseActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return Wsrm11Strings.TerminateSequenceResponseAction;
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        internal static ActionHeader GetTerminateSequenceResponseActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11,
                Wsrm11Strings.TerminateSequenceResponse);
        }
    }

    class Wsrm11Index : WsrmIndex
    {
        static MessagePartSpecification signedReliabilityMessageParts;

        ActionHeader ackRequestedActionHeader;
        AddressingVersion addressingVersion;
        ActionHeader closeSequenceActionHeader;
        ActionHeader closeSequenceResponseActionHeader;
        ActionHeader createSequenceActionHeader;
        ActionHeader sequenceAcknowledgementActionHeader;
        ActionHeader terminateSequenceActionHeader;
        ActionHeader terminateSequenceResponseActionHeader;

        internal Wsrm11Index(AddressingVersion addressingVersion)
        {
            this.addressingVersion = addressingVersion;
        }

        internal static MessagePartSpecification SignedReliabilityMessageParts
        {
            get
            {
                if (signedReliabilityMessageParts == null)
                {
                    XmlQualifiedName[] wsrmMessageHeaders = new XmlQualifiedName[]
                    {
                        new XmlQualifiedName(WsrmFeb2005Strings.Sequence, Wsrm11Strings.Namespace),
                        new XmlQualifiedName(WsrmFeb2005Strings.SequenceAcknowledgement, Wsrm11Strings.Namespace),
                        new XmlQualifiedName(WsrmFeb2005Strings.AckRequested, Wsrm11Strings.Namespace),
                        new XmlQualifiedName(Wsrm11Strings.UsesSequenceSTR, Wsrm11Strings.Namespace),
                    };

                    MessagePartSpecification s = new MessagePartSpecification(wsrmMessageHeaders);
                    s.MakeReadOnly();
                    signedReliabilityMessageParts = s;
                }

                return signedReliabilityMessageParts;
            }
        }

        protected override ActionHeader GetActionHeader(string element)
        {
            Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;
            if (element == WsrmFeb2005Strings.AckRequested)
            {
                if (ackRequestedActionHeader == null)
                {
                    ackRequestedActionHeader = ActionHeader.Create(wsrm11Dictionary.AckRequestedAction,
                        this.addressingVersion);
                }

                return ackRequestedActionHeader;
            }
            else if (element == WsrmFeb2005Strings.CreateSequence)
            {
                if (createSequenceActionHeader == null)
                {
                    createSequenceActionHeader = ActionHeader.Create(wsrm11Dictionary.CreateSequenceAction,
                        this.addressingVersion);
                }

                return createSequenceActionHeader;
            }
            else if (element == WsrmFeb2005Strings.SequenceAcknowledgement)
            {
                if (sequenceAcknowledgementActionHeader == null)
                {
                    sequenceAcknowledgementActionHeader =
                        ActionHeader.Create(wsrm11Dictionary.SequenceAcknowledgementAction,
                        this.addressingVersion);
                }

                return sequenceAcknowledgementActionHeader;
            }
            else if (element == WsrmFeb2005Strings.TerminateSequence)
            {
                if (terminateSequenceActionHeader == null)
                {
                    terminateSequenceActionHeader =
                        ActionHeader.Create(wsrm11Dictionary.TerminateSequenceAction, this.addressingVersion);
                }

                return terminateSequenceActionHeader;
            }
            else if (element == Wsrm11Strings.TerminateSequenceResponse)
            {
                if (terminateSequenceResponseActionHeader == null)
                {
                    terminateSequenceResponseActionHeader =
                        ActionHeader.Create(wsrm11Dictionary.TerminateSequenceResponseAction, this.addressingVersion);
                }

                return terminateSequenceResponseActionHeader;
            }
            else if (element == Wsrm11Strings.CloseSequence)
            {
                if (closeSequenceActionHeader == null)
                {
                    closeSequenceActionHeader =
                        ActionHeader.Create(wsrm11Dictionary.CloseSequenceAction, this.addressingVersion);
                }

                return closeSequenceActionHeader;
            }
            else if (element == Wsrm11Strings.CloseSequenceResponse)
            {
                if (closeSequenceResponseActionHeader == null)
                {
                    closeSequenceResponseActionHeader =
                        ActionHeader.Create(wsrm11Dictionary.CloseSequenceResponseAction, this.addressingVersion);
                }

                return closeSequenceResponseActionHeader;
            }
            else
            {
                throw Fx.AssertAndThrow("Element not supported.");
            }
        }
    }

    class WsrmFeb2005Index : WsrmIndex
    {
        static MessagePartSpecification signedReliabilityMessageParts;

        ActionHeader ackRequestedActionHeader;
        AddressingVersion addressingVersion;
        ActionHeader createSequenceActionHeader;
        ActionHeader sequenceAcknowledgementActionHeader;
        ActionHeader terminateSequenceActionHeader;

        internal WsrmFeb2005Index(AddressingVersion addressingVersion)
        {
            this.addressingVersion = addressingVersion;
        }

        internal static MessagePartSpecification SignedReliabilityMessageParts
        {
            get
            {
                if (signedReliabilityMessageParts == null)
                {
                    XmlQualifiedName[] wsrmMessageHeaders = new XmlQualifiedName[]
                    {
                        new XmlQualifiedName(WsrmFeb2005Strings.Sequence, WsrmFeb2005Strings.Namespace),
                        new XmlQualifiedName(WsrmFeb2005Strings.SequenceAcknowledgement, WsrmFeb2005Strings.Namespace),
                        new XmlQualifiedName(WsrmFeb2005Strings.AckRequested, WsrmFeb2005Strings.Namespace),
                    };

                    MessagePartSpecification s = new MessagePartSpecification(wsrmMessageHeaders);
                    s.MakeReadOnly();
                    signedReliabilityMessageParts = s;
                }

                return signedReliabilityMessageParts;
            }
        }

        protected override ActionHeader GetActionHeader(string element)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;

            if (element == WsrmFeb2005Strings.AckRequested)
            {
                if (ackRequestedActionHeader == null)
                {
                    ackRequestedActionHeader = ActionHeader.Create(wsrmFeb2005Dictionary.AckRequestedAction,
                        this.addressingVersion);
                }

                return ackRequestedActionHeader;
            }
            else if (element == WsrmFeb2005Strings.CreateSequence)
            {
                if (createSequenceActionHeader == null)
                {
                    createSequenceActionHeader =
                        ActionHeader.Create(wsrmFeb2005Dictionary.CreateSequenceAction, this.addressingVersion);
                }

                return createSequenceActionHeader;
            }
            else if (element == WsrmFeb2005Strings.SequenceAcknowledgement)
            {
                if (sequenceAcknowledgementActionHeader == null)
                {
                    sequenceAcknowledgementActionHeader =
                        ActionHeader.Create(wsrmFeb2005Dictionary.SequenceAcknowledgementAction,
                        this.addressingVersion);
                }

                return sequenceAcknowledgementActionHeader;
            }
            else if (element == WsrmFeb2005Strings.TerminateSequence)
            {
                if (terminateSequenceActionHeader == null)
                {
                    terminateSequenceActionHeader =
                        ActionHeader.Create(wsrmFeb2005Dictionary.TerminateSequenceAction, this.addressingVersion);
                }

                return terminateSequenceActionHeader;
            }
            else
            {
                throw Fx.AssertAndThrow("Element not supported.");
            }
        }
    }

    static class WsrmUtilities
    {
        public static TimeSpan CalculateKeepAliveInterval(TimeSpan inactivityTimeout, int maxRetryCount)
        {
            return Ticks.ToTimeSpan(Ticks.FromTimeSpan(inactivityTimeout) / 2 / maxRetryCount);
        }

        internal static UniqueId NextSequenceId()
        {
            return new UniqueId();
        }

        internal static void AddAcknowledgementHeader(ReliableMessagingVersion reliableMessagingVersion,
            Message message, UniqueId id, SequenceRangeCollection ranges, bool final)
        {
            WsrmUtilities.AddAcknowledgementHeader(reliableMessagingVersion, message, id, ranges, final, -1);
        }

        internal static void AddAcknowledgementHeader(ReliableMessagingVersion reliableMessagingVersion,
            Message message, UniqueId id, SequenceRangeCollection ranges, bool final, int bufferRemaining)
        {
            message.Headers.Insert(0,
                new WsrmAcknowledgmentHeader(reliableMessagingVersion, id, ranges, final, bufferRemaining));
        }

        internal static void AddAckRequestedHeader(ReliableMessagingVersion reliableMessagingVersion, Message message,
            UniqueId id)
        {
            message.Headers.Insert(0, new WsrmAckRequestedHeader(reliableMessagingVersion, id));
        }

        internal static void AddSequenceHeader(ReliableMessagingVersion reliableMessagingVersion, Message message,
            UniqueId id, Int64 sequenceNumber, bool isLast)
        {
            message.Headers.Insert(0,
                new WsrmSequencedMessageHeader(reliableMessagingVersion, id, sequenceNumber, isLast));
        }

        internal static void AssertWsrm11(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("WS-ReliableMessaging 1.1 required.");
            }
        }

        internal static Message CreateAcknowledgmentMessage(MessageVersion version,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId id, SequenceRangeCollection ranges, bool final,
            int bufferRemaining)
        {
            Message message = Message.CreateMessage(version,
                WsrmIndex.GetSequenceAcknowledgementActionHeader(version.Addressing, reliableMessagingVersion));

            WsrmUtilities.AddAcknowledgementHeader(reliableMessagingVersion, message, id, ranges, final,
                bufferRemaining);
            message.Properties.AllowOutputBatching = false;

            return message;
        }

        internal static Message CreateAckRequestedMessage(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId id)
        {
            Message message = Message.CreateMessage(messageVersion,
                WsrmIndex.GetAckRequestedActionHeader(messageVersion.Addressing, reliableMessagingVersion));

            WsrmUtilities.AddAckRequestedHeader(reliableMessagingVersion, message, id);
            message.Properties.AllowOutputBatching = false;

            return message;
        }

        internal static Message CreateCloseSequenceResponse(MessageVersion messageVersion, UniqueId messageId,
            UniqueId inputId)
        {
            CloseSequenceResponse response = new CloseSequenceResponse(inputId);

            Message message = Message.CreateMessage(messageVersion,
                WsrmIndex.GetCloseSequenceResponseActionHeader(messageVersion.Addressing), response);

            message.Headers.RelatesTo = messageId;
            return message;
        }

        internal static Message CreateCreateSequenceResponse(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, bool duplex, CreateSequenceInfo createSequenceInfo,
            bool ordered, UniqueId inputId, EndpointAddress acceptAcksTo)
        {
            CreateSequenceResponse response = new CreateSequenceResponse(messageVersion.Addressing, reliableMessagingVersion);
            response.Identifier = inputId;
            response.Expires = createSequenceInfo.Expires;
            response.Ordered = ordered;

            if (duplex)
                response.AcceptAcksTo = acceptAcksTo;

            Message responseMessage
                = Message.CreateMessage(messageVersion, ActionHeader.Create(
                WsrmIndex.GetCreateSequenceResponseAction(reliableMessagingVersion), messageVersion.Addressing), response);

            return responseMessage;
        }

        internal static Message CreateCSRefusedCommunicationFault(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, false, null, reason);
        }

        internal static Message CreateCSRefusedProtocolFault(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, true, null, reason);
        }

        internal static Message CreateCSRefusedServerTooBusyFault(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            FaultCode subCode = new FaultCode(WsrmFeb2005Strings.ConnectionLimitReached,
                WsrmFeb2005Strings.NETNamespace);
            subCode = new FaultCode(WsrmFeb2005Strings.CreateSequenceRefused,
                WsrmIndex.GetNamespaceString(reliableMessagingVersion), subCode);
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, false, subCode, reason);
        }

        static Message CreateCSRefusedFault(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, bool isSenderFault, FaultCode subCode, string reason)
        {
            FaultCode code;

            if (messageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                code = new FaultCode(WsrmFeb2005Strings.CreateSequenceRefused, WsrmIndex.GetNamespaceString(reliableMessagingVersion));
            }
            else if (messageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                if (subCode == null)
                    subCode = new FaultCode(WsrmFeb2005Strings.CreateSequenceRefused, WsrmIndex.GetNamespaceString(reliableMessagingVersion), subCode);

                if (isSenderFault)
                    code = FaultCode.CreateSenderFaultCode(subCode);
                else
                    code = FaultCode.CreateReceiverFaultCode(subCode);
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }

            FaultReason faultReason = new FaultReason(SR.GetString(SR.CSRefused, reason), CultureInfo.CurrentCulture);

            MessageFault fault = MessageFault.CreateFault(code, faultReason);
            string action = WsrmIndex.GetFaultActionString(messageVersion.Addressing, reliableMessagingVersion);
            return Message.CreateMessage(messageVersion, fault, action);
        }

        public static Exception CreateCSFaultException(MessageVersion version,
            ReliableMessagingVersion reliableMessagingVersion, Message message, IChannel innerChannel)
        {
            MessageFault fault = MessageFault.CreateFault(message, TransportDefaults.MaxRMFaultSize);
            FaultCode code = fault.Code;
            FaultCode subCode;

            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                subCode = code;
            }
            else if (version.Envelope == EnvelopeVersion.Soap12)
            {
                subCode = code.SubCode;
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }

            if (subCode != null)
            {
                // CreateSequenceRefused
                if ((subCode.Namespace == WsrmIndex.GetNamespaceString(reliableMessagingVersion))
                    && (subCode.Name == WsrmFeb2005Strings.CreateSequenceRefused))
                {
                    string reason = FaultException.GetSafeReasonText(fault);

                    if (version.Envelope == EnvelopeVersion.Soap12)
                    {
                        FaultCode subSubCode = subCode.SubCode;
                        if ((subSubCode != null)
                            && (subSubCode.Namespace == WsrmFeb2005Strings.NETNamespace)
                            && (subSubCode.Name == WsrmFeb2005Strings.ConnectionLimitReached))
                        {
                            return new ServerTooBusyException(reason);
                        }

                        if (code.IsSenderFault)
                        {
                            return new ProtocolException(reason);
                        }
                    }

                    return new CommunicationException(reason);
                }
                else if ((subCode.Namespace == version.Addressing.Namespace)
                    && (subCode.Name == AddressingStrings.EndpointUnavailable))
                {
                    return new EndpointNotFoundException(FaultException.GetSafeReasonText(fault));
                }
            }

            FaultConverter faultConverter = innerChannel.GetProperty<FaultConverter>();
            if (faultConverter == null)
                faultConverter = FaultConverter.GetDefaultFaultConverter(version);

            Exception exception;
            if (faultConverter.TryCreateException(message, fault, out exception))
            {
                return exception;
            }
            else
            {
                return new ProtocolException(SR.GetString(SR.UnrecognizedFaultReceivedOnOpen, fault.Code.Namespace, fault.Code.Name, FaultException.GetSafeReasonText(fault)));
            }
        }

        internal static Message CreateEndpointNotFoundFault(MessageVersion version, string reason)
        {
            FaultCode subCode = new FaultCode(AddressingStrings.EndpointUnavailable, version.Addressing.Namespace);
            FaultCode code;

            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                code = subCode;
            }
            else if (version.Envelope == EnvelopeVersion.Soap12)
            {
                code = FaultCode.CreateSenderFaultCode(subCode);
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }

            FaultReason faultReason = new FaultReason(reason, CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code, faultReason);
            return Message.CreateMessage(version, fault, version.Addressing.DefaultFaultAction);
        }

        internal static Message CreateTerminateMessage(MessageVersion version,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId id)
        {
            return CreateTerminateMessage(version, reliableMessagingVersion, id, -1);
        }

        internal static Message CreateTerminateMessage(MessageVersion version,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId id, Int64 last)
        {
            Message message = Message.CreateMessage(version,
                WsrmIndex.GetTerminateSequenceActionHeader(version.Addressing, reliableMessagingVersion),
                new TerminateSequence(reliableMessagingVersion, id, last));

            message.Properties.AllowOutputBatching = false;

            return message;
        }

        internal static Message CreateTerminateResponseMessage(MessageVersion version, UniqueId messageId, UniqueId sequenceId)
        {
            Message message = Message.CreateMessage(version,
                WsrmIndex.GetTerminateSequenceResponseActionHeader(version.Addressing),
                new TerminateSequenceResponse(sequenceId));

            message.Properties.AllowOutputBatching = false;
            message.Headers.RelatesTo = messageId;
            return message;
        }

        internal static UniqueId GetInputId(WsrmMessageInfo info)
        {
            if (info.TerminateSequenceInfo != null)
            {
                return info.TerminateSequenceInfo.Identifier;
            }

            if (info.SequencedMessageInfo != null)
            {
                return info.SequencedMessageInfo.SequenceID;
            }

            if (info.AckRequestedInfo != null)
            {
                return info.AckRequestedInfo.SequenceID;
            }

            if (info.WsrmHeaderFault != null && info.WsrmHeaderFault.FaultsInput)
            {
                return info.WsrmHeaderFault.SequenceID;
            }

            if (info.CloseSequenceInfo != null)
            {
                return info.CloseSequenceInfo.Identifier;
            }

            return null;
        }

        internal static UniqueId GetOutputId(ReliableMessagingVersion reliableMessagingVersion, WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null)
            {
                return info.AcknowledgementInfo.SequenceID;
            }

            if (info.WsrmHeaderFault != null && info.WsrmHeaderFault.FaultsOutput)
            {
                return info.WsrmHeaderFault.SequenceID;
            }

            if (info.TerminateSequenceResponseInfo != null)
            {
                return info.TerminateSequenceResponseInfo.Identifier;
            }

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (info.CloseSequenceInfo != null)
                {
                    return info.CloseSequenceInfo.Identifier;
                }

                if (info.CloseSequenceResponseInfo != null)
                {
                    return info.CloseSequenceResponseInfo.Identifier;
                }

                if (info.TerminateSequenceResponseInfo != null)
                {
                    return info.TerminateSequenceResponseInfo.Identifier;
                }
            }

            return null;
        }

        internal static bool IsWsrmAction(ReliableMessagingVersion reliableMessagingVersion, string action)
        {
            if (action == null)
                return false;
            return (action.StartsWith(WsrmIndex.GetNamespaceString(reliableMessagingVersion), StringComparison.Ordinal));
        }

        public static void ReadEmptyElement(XmlDictionaryReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.Read();
                reader.ReadEndElement();
            }
        }

        public static UniqueId ReadIdentifier(XmlDictionaryReader reader,
            ReliableMessagingVersion reliableMessagingVersion)
        {
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, WsrmIndex.GetNamespace(reliableMessagingVersion));
            UniqueId sequenceID = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            return sequenceID;
        }

        public static Int64 ReadSequenceNumber(XmlDictionaryReader reader)
        {
            return WsrmUtilities.ReadSequenceNumber(reader, false);
        }

        public static Int64 ReadSequenceNumber(XmlDictionaryReader reader, bool allowZero)
        {
            Int64 sequenceNumber = reader.ReadContentAsLong();

            if (sequenceNumber < 0 || (sequenceNumber == 0 && !allowZero))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                    SR.GetString(SR.InvalidSequenceNumber, sequenceNumber)));
            }

            return sequenceNumber;
        }

        // Caller owns message.
        public static WsrmFault ValidateCloseSequenceResponse(ChannelReliableSession session, UniqueId messageId,
            WsrmMessageInfo info, Int64 last)
        {
            string exceptionString = null;
            string faultString = null;

            if (info.CloseSequenceResponseInfo == null)
            {
                exceptionString = SR.GetString(SR.InvalidWsrmResponseSessionFaultedExceptionString,
                    Wsrm11Strings.CloseSequence, info.Action,
                    Wsrm11Strings.CloseSequenceResponseAction);
                faultString = SR.GetString(SR.InvalidWsrmResponseSessionFaultedFaultString,
                    Wsrm11Strings.CloseSequence, info.Action,
                    Wsrm11Strings.CloseSequenceResponseAction);
            }
            else if (!object.Equals(messageId, info.CloseSequenceResponseInfo.RelatesTo))
            {
                exceptionString = SR.GetString(SR.WsrmMessageWithWrongRelatesToExceptionString, Wsrm11Strings.CloseSequence);
                faultString = SR.GetString(SR.WsrmMessageWithWrongRelatesToFaultString, Wsrm11Strings.CloseSequence);
            }
            else if (info.AcknowledgementInfo == null || !info.AcknowledgementInfo.Final)
            {
                exceptionString = SR.GetString(SR.MissingFinalAckExceptionString);
                faultString = SR.GetString(SR.SequenceTerminatedMissingFinalAck);
            }
            else
            {
                return ValidateFinalAck(session, info, last);
            }

            UniqueId sequenceId = session.OutputID;
            return SequenceTerminatedFault.CreateProtocolFault(sequenceId, faultString, exceptionString);
        }

        public static bool ValidateCreateSequence<TChannel>(WsrmMessageInfo info,
            ReliableChannelListenerBase<TChannel> listener, IChannel channel, out EndpointAddress acksTo)
            where TChannel : class, IChannel
        {
            acksTo = null;
            string reason = null;

            if (info.CreateSequenceInfo.OfferIdentifier == null)
            {
                if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                    reason = SR.GetString(SR.CSRefusedDuplexNoOffer, listener.Uri);
                else if (typeof(TChannel) == typeof(IReplySessionChannel))
                    reason = SR.GetString(SR.CSRefusedReplyNoOffer, listener.Uri);
            }
            else if (listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (typeof(TChannel) == typeof(IInputSessionChannel))
                    reason = SR.GetString(SR.CSRefusedInputOffer, listener.Uri);
            }

            if (reason != null)
            {
                info.FaultReply = WsrmUtilities.CreateCSRefusedProtocolFault(listener.MessageVersion,
                    listener.ReliableMessagingVersion, reason);
                info.FaultException = new ProtocolException(SR.GetString(SR.ConflictingOffer));
                return false;
            }

            if (listener.LocalAddresses != null)
            {
                Collection<EndpointAddress> addresses = new Collection<EndpointAddress>();

                try
                {
                    listener.LocalAddresses.GetMatchingValues(info.Message, addresses);
                }
                catch (CommunicationException e)
                {
                    FaultConverter converter = channel.GetProperty<FaultConverter>();
                    if (converter == null)
                        converter = FaultConverter.GetDefaultFaultConverter(listener.MessageVersion);

                    Message faultReply;
                    if (converter.TryCreateFaultMessage(e, out faultReply))
                    {
                        info.FaultReply = faultReply;
                        info.FaultException = new ProtocolException(SR.GetString(SR.MessageExceptionOccurred), e);
                        return false;
                    }

                    throw;
                }

                if (addresses.Count > 0)
                {
                    EndpointAddress match = addresses[0];
                    acksTo = new EndpointAddress(info.CreateSequenceInfo.To, match.Identity, match.Headers);
                    return true;
                }
                else
                {
                    info.FaultReply = CreateEndpointNotFoundFault(listener.MessageVersion, SR.GetString(SR.EndpointNotFound, info.CreateSequenceInfo.To));
                    info.FaultException = new ProtocolException(SR.GetString(SR.ConflictingAddress));
                    return false;
                }
            }
            else
            {
                acksTo = new EndpointAddress(info.CreateSequenceInfo.To);
                return true;
            }
        }

        public static WsrmFault ValidateFinalAck(ChannelReliableSession session, WsrmMessageInfo info, Int64 last)
        {
            WsrmAcknowledgmentInfo ackInfo = info.AcknowledgementInfo;
            WsrmFault fault = ValidateFinalAckExists(session, ackInfo);

            if (fault != null)
            {
                return fault;
            }

            SequenceRangeCollection finalRanges = ackInfo.Ranges;

            if (last == 0)
            {
                if (finalRanges.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                if ((finalRanges.Count == 1) && (finalRanges[0].Lower == 1) && (finalRanges[0].Upper == last))
                {
                    return null;
                }
            }

            return new InvalidAcknowledgementFault(session.OutputID, ackInfo.Ranges);
        }

        public static WsrmFault ValidateFinalAckExists(ChannelReliableSession session, WsrmAcknowledgmentInfo ackInfo)
        {
            if (ackInfo == null || !ackInfo.Final)
            {
                string exceptionString = SR.GetString(SR.MissingFinalAckExceptionString);
                string faultString = SR.GetString(SR.SequenceTerminatedMissingFinalAck);
                return SequenceTerminatedFault.CreateProtocolFault(session.OutputID, faultString, exceptionString);
            }

            return null;
        }

        // Caller owns message.
        public static WsrmFault ValidateTerminateSequenceResponse(ChannelReliableSession session, UniqueId messageId,
            WsrmMessageInfo info, Int64 last)
        {
            string exceptionString = null;
            string faultString = null;

            if (info.WsrmHeaderFault is UnknownSequenceFault)
            {
                return null;
            }
            else if (info.TerminateSequenceResponseInfo == null)
            {
                exceptionString = SR.GetString(SR.InvalidWsrmResponseSessionFaultedExceptionString,
                    WsrmFeb2005Strings.TerminateSequence, info.Action,
                    Wsrm11Strings.TerminateSequenceResponseAction);
                faultString = SR.GetString(SR.InvalidWsrmResponseSessionFaultedFaultString,
                    WsrmFeb2005Strings.TerminateSequence, info.Action,
                    Wsrm11Strings.TerminateSequenceResponseAction);
            }
            else if (!object.Equals(messageId, info.TerminateSequenceResponseInfo.RelatesTo))
            {
                exceptionString = SR.GetString(SR.WsrmMessageWithWrongRelatesToExceptionString, WsrmFeb2005Strings.TerminateSequence);
                faultString = SR.GetString(SR.WsrmMessageWithWrongRelatesToFaultString, WsrmFeb2005Strings.TerminateSequence);
            }
            else
            {
                return ValidateFinalAck(session, info, last);
            }

            UniqueId sequenceId = session.OutputID;
            return SequenceTerminatedFault.CreateProtocolFault(sequenceId, faultString, exceptionString);
        }

        // Checks that ReplyTo and RemoteAddress are equivalent. Will fault the session with SequenceTerminatedFault.
        // Meant to be used for CloseSequence and TerminateSequence in Wsrm 1.1.
        public static bool ValidateWsrmRequest(ChannelReliableSession session, WsrmRequestInfo info,
            IReliableChannelBinder binder, RequestContext context)
        {
            if (!(info is CloseSequenceInfo) && !(info is TerminateSequenceInfo))
            {
                throw Fx.AssertAndThrow("Method is meant for CloseSequence or TerminateSequence only.");
            }

            if (info.ReplyTo.Uri != binder.RemoteAddress.Uri)
            {
                string faultString = SR.GetString(SR.WsrmRequestIncorrectReplyToFaultString, info.RequestName);
                string exceptionString = SR.GetString(SR.WsrmRequestIncorrectReplyToExceptionString, info.RequestName);
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(session.InputID, faultString, exceptionString);
                session.OnLocalFault(fault.CreateException(), fault, context);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void WriteIdentifier(XmlDictionaryWriter writer,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceId)
        {
            writer.WriteStartElement(WsrmFeb2005Strings.Prefix, XD.WsrmFeb2005Dictionary.Identifier,
                WsrmIndex.GetNamespace(reliableMessagingVersion));
            writer.WriteValue(sequenceId);
            writer.WriteEndElement();
        }

        // These are strings that are not actually used anywhere.
        // This method and resources strings can be deleted whenever the resource file can be changed.
        public static string UseStrings()
        {
            string s = SR.SupportedAddressingModeNotSupported;
            s = SR.SequenceTerminatedUnexpectedCloseSequence;
            s = SR.UnexpectedCloseSequence;
            s = SR.SequenceTerminatedUnsupportedTerminateSequence;
            return s;
        }
    }
}
