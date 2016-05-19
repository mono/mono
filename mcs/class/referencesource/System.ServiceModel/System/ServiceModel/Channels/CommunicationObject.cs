//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    public abstract class CommunicationObject : ICommunicationObject
    {
        bool aborted;
        bool closeCalled;
#if DEBUG
        StackTrace closeStack;
        StackTrace faultedStack;
#endif
        ExceptionQueue exceptionQueue;
        object mutex;
        bool onClosingCalled;
        bool onClosedCalled;
        bool onOpeningCalled;
        bool onOpenedCalled;
        bool raisedClosed;
        bool raisedClosing;
        bool raisedFaulted;
        bool traceOpenAndClose;
        object eventSender;
        CommunicationState state;

        protected CommunicationObject()
            : this(new object())
        {
        }

        protected CommunicationObject(object mutex)
        {
            this.mutex = mutex;
            this.eventSender = this;
            this.state = CommunicationState.Created;
        }

        internal CommunicationObject(object mutex, object eventSender)
        {
            this.mutex = mutex;
            this.eventSender = eventSender;
            this.state = CommunicationState.Created;
        }

        internal bool Aborted
        {
            get { return this.aborted; }
        }

        internal object EventSender
        {
            get { return this.eventSender; }
            set { eventSender = value; }
        }

        protected bool IsDisposed
        {
            get { return this.state == CommunicationState.Closed; }
        }

        public CommunicationState State
        {
            get { return this.state; }
        }

        protected object ThisLock
        {
            get { return this.mutex; }
        }

        protected abstract TimeSpan DefaultCloseTimeout { get; }
        protected abstract TimeSpan DefaultOpenTimeout { get; }

        internal TimeSpan InternalCloseTimeout
        {
            get { return this.DefaultCloseTimeout; }
        }

        internal TimeSpan InternalOpenTimeout
        {
            get { return this.DefaultOpenTimeout; }
        }

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;

        public void Abort()
        {
            lock (ThisLock)
            {
                if (this.aborted || this.state == CommunicationState.Closed)
                    return;
                this.aborted = true;
#if DEBUG
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif

                this.state = CommunicationState.Closing;
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.CommunicationObjectAborted, SR.GetString(SR.TraceCodeCommunicationObjectAborted, TraceUtility.CreateSourceString(this)), this);
            }

            bool throwing = true;

            try
            {
                OnClosing();
                if (!this.onClosingCalled)
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);

                OnAbort();

                OnClosed();
                if (!this.onClosedCalled)
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosed"), Guid.Empty, this);

                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectAbortFailed, SR.GetString(SR.TraceCodeCommunicationObjectAbortFailed, this.GetCommunicationObjectType().ToString()), this);
                }
            }
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.BeginClose(this.DefaultCloseTimeout, callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", SR.GetString(SR.SFxTimeoutOutOfRange0)));

            using (DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose ? this.CreateCloseActivity() : null)
            {
                CommunicationState originalState;
                lock (ThisLock)
                {
                    originalState = this.state;
#if DEBUG
                    if (closeStack == null)
                        closeStack = new StackTrace();
#endif
                    if (originalState != CommunicationState.Closed)
                        this.state = CommunicationState.Closing;

                    this.closeCalled = true;
                }

                switch (originalState)
                {
                    case CommunicationState.Created:
                    case CommunicationState.Opening:
                    case CommunicationState.Faulted:
                        this.Abort();
                        if (originalState == CommunicationState.Faulted)
                        {
                            throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
                        }
                        return new AlreadyClosedAsyncResult(callback, state);

                    case CommunicationState.Opened:
                        {
                            bool throwing = true;
                            try
                            {
                                OnClosing();
                                if (!this.onClosingCalled)
                                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);

                                IAsyncResult result = new CloseAsyncResult(this, timeout, callback, state);
                                throwing = false;
                                return result;
                            }
                            finally
                            {
                                if (throwing)
                                {
                                    if (DiagnosticUtility.ShouldTraceWarning)
                                    {
                                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectCloseFailed, SR.GetString(SR.TraceCodeCommunicationObjectCloseFailed, this.GetCommunicationObjectType().ToString()), this);
                                    }

                                    Abort();
                                }
                            }
                        }

                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                        return new AlreadyClosedAsyncResult(callback, state);

                    default:
                        throw Fx.AssertAndThrow("CommunicationObject.BeginClose: Unknown CommunicationState");
                }
            }
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.BeginOpen(this.DefaultOpenTimeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", SR.GetString(SR.SFxTimeoutOutOfRange0)));

            lock (ThisLock)
            {
                ThrowIfDisposedOrImmutable();
                this.state = CommunicationState.Opening;
            }

            bool throwing = true;
            try
            {
                OnOpening();
                if (!this.onOpeningCalled)
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpening"), Guid.Empty, this);

                IAsyncResult result = new OpenAsyncResult(this, timeout, callback, state);
                throwing = false;
                return result;
            }
            finally
            {
                if (throwing)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectOpenFailed, SR.GetString(SR.TraceCodeCommunicationObjectOpenFailed, this.GetCommunicationObjectType().ToString()), this);
                    }

                    Fault();
                }
            }
        }

        public void Close()
        {
            this.Close(this.DefaultCloseTimeout);
        }

        public void Close(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", SR.GetString(SR.SFxTimeoutOutOfRange0)));

            using (DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose ? this.CreateCloseActivity() : null)
            {

                CommunicationState originalState;
                lock (ThisLock)
                {
                    originalState = this.state;
#if DEBUG
                    if (closeStack == null)
                        closeStack = new StackTrace();
#endif
                    if (originalState != CommunicationState.Closed)
                        this.state = CommunicationState.Closing;

                    this.closeCalled = true;
                }

                switch (originalState)
                {
                    case CommunicationState.Created:
                    case CommunicationState.Opening:
                    case CommunicationState.Faulted:
                        this.Abort();
                        if (originalState == CommunicationState.Faulted)
                        {
                            throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
                        }
                        break;

                    case CommunicationState.Opened:
                        {
                            bool throwing = true;
                            try
                            {
                                TimeoutHelper actualTimeout = new TimeoutHelper(timeout);

                                OnClosing();
                                if (!this.onClosingCalled)
                                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);

                                OnClose(actualTimeout.RemainingTime());

                                OnClosed();
                                if (!this.onClosedCalled)
                                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosed"), Guid.Empty, this);

                                throwing = false;
                            }
                            finally
                            {
                                if (throwing)
                                {
                                    if (DiagnosticUtility.ShouldTraceWarning)
                                    {
                                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectCloseFailed, SR.GetString(SR.TraceCodeCommunicationObjectCloseFailed, this.GetCommunicationObjectType().ToString()), this);
                                    }

                                    Abort();
                                }
                            }
                            break;
                        }

                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                        break;

                    default:
                        throw Fx.AssertAndThrow("CommunicationObject.BeginClose: Unknown CommunicationState");
                }
            }
        }

        Exception CreateNotOpenException()
        {
            return new InvalidOperationException(SR.GetString(SR.CommunicationObjectCannotBeUsed, this.GetCommunicationObjectType().ToString(), this.state.ToString()));
        }

        Exception CreateImmutableException()
        {
            return new InvalidOperationException(SR.GetString(SR.CommunicationObjectCannotBeModifiedInState, this.GetCommunicationObjectType().ToString(), this.state.ToString()));
        }

        Exception CreateBaseClassMethodNotCalledException(string method)
        {
            return new InvalidOperationException(SR.GetString(SR.CommunicationObjectBaseClassMethodNotCalled, this.GetCommunicationObjectType().ToString(), method));
        }

        internal Exception CreateClosedException()
        {
            if (!this.closeCalled)
            {
                return CreateAbortedException();
            }
            else
            {
#if DEBUG
                string originalStack = closeStack.ToString().Replace("\r\n", "\r\n    ");
                return new ObjectDisposedException(this.GetCommunicationObjectType().ToString() + ", Object already closed:\r\n    " + originalStack);
#else
                return new ObjectDisposedException(this.GetCommunicationObjectType().ToString());
#endif
            }
        }

        internal Exception CreateFaultedException()
        {
#if DEBUG
            string originalStack = faultedStack.ToString().Replace("\r\n", "\r\n    ");
            string message = SR.GetString(SR.CommunicationObjectFaultedStack2, this.GetCommunicationObjectType().ToString(), originalStack);
#else
            string message = SR.GetString(SR.CommunicationObjectFaulted1, this.GetCommunicationObjectType().ToString());
#endif
            return new CommunicationObjectFaultedException(message);
        }

        internal Exception CreateAbortedException()
        {
#if DEBUG
            string originalStack = closeStack.ToString().Replace("\r\n", "\r\n    ");
            return new CommunicationObjectAbortedException(SR.GetString(SR.CommunicationObjectAbortedStack2, this.GetCommunicationObjectType().ToString(), originalStack));
#else
            return new CommunicationObjectAbortedException(SR.GetString(SR.CommunicationObjectAborted1, this.GetCommunicationObjectType().ToString()));
#endif
        }

        internal virtual string CloseActivityName
        {
            get { return SR.GetString(SR.ActivityClose, this.GetType().FullName); }
        }

        internal virtual string OpenActivityName
        {
            get { return SR.GetString(SR.ActivityOpen, this.GetType().FullName); }
        }

        internal virtual ActivityType OpenActivityType
        {
            get { return ActivityType.Open; }
        }

        ServiceModelActivity CreateCloseActivity()
        {
            ServiceModelActivity retval = null;
            retval = ServiceModelActivity.CreateBoundedActivity();
            if (DiagnosticUtility.ShouldUseActivity)
            {
                ServiceModelActivity.Start(retval, this.CloseActivityName, ActivityType.Close);
            }

            return retval;
        }

        internal bool DoneReceivingInCurrentState()
        {
            this.ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    return false;

                case CommunicationState.Closing:
                    return true;

                case CommunicationState.Closed:
                    return true;

                case CommunicationState.Faulted:
                    return true;

                default:
                    throw Fx.AssertAndThrow("DoneReceivingInCurrentState: Unknown CommunicationObject.state");
            }
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is AlreadyClosedAsyncResult)
                AlreadyClosedAsyncResult.End(result);
            else
                CloseAsyncResult.End(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected void Fault()
        {
            lock (ThisLock)
            {
                if (this.state == CommunicationState.Closed || this.state == CommunicationState.Closing)
                    return;

                if (this.state == CommunicationState.Faulted)
                    return;
#if DEBUG
                if (faultedStack == null)
                    faultedStack = new StackTrace();
#endif
                this.state = CommunicationState.Faulted;
            }

            OnFaulted();
        }

        internal void Fault(Exception exception)
        {
            lock (this.ThisLock)
            {
                if (this.exceptionQueue == null)
                    this.exceptionQueue = new ExceptionQueue(this.ThisLock);
            }

            if (exception != null && DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.CommunicationObjectFaultReason,
                    SR.GetString(SR.TraceCodeCommunicationObjectFaultReason), exception, null);
            }

            this.exceptionQueue.AddException(exception);
            this.Fault();
        }

        internal void AddPendingException(Exception exception)
        {
            lock (this.ThisLock)
            {
                if (this.exceptionQueue == null)
                    this.exceptionQueue = new ExceptionQueue(this.ThisLock);
            }

            this.exceptionQueue.AddException(exception);
        }

        internal Exception GetPendingException()
        {
            CommunicationState currentState = this.state;

            Fx.Assert(currentState == CommunicationState.Closing || currentState == CommunicationState.Closed || currentState == CommunicationState.Faulted,
                "CommunicationObject.GetPendingException(currentState == CommunicationState.Closing || currentState == CommunicationState.Closed || currentState == CommunicationState.Faulted)");

            ExceptionQueue queue = this.exceptionQueue;
            if (queue != null)
            {
                return queue.GetException();
            }
            else
            {
                return null;
            }
        }

        // Terminal is loosely defined as an interruption to close or a fault.
        internal Exception GetTerminalException()
        {
            Exception exception = this.GetPendingException();

            if (exception != null)
            {
                return exception;
            }

            switch (this.state)
            {
                case CommunicationState.Closing:
                case CommunicationState.Closed:
                    return new CommunicationException(SR.GetString(SR.CommunicationObjectCloseInterrupted1, this.GetCommunicationObjectType().ToString()));

                case CommunicationState.Faulted:
                    return this.CreateFaultedException();

                default:
                    throw Fx.AssertAndThrow("GetTerminalException: Invalid CommunicationObject.state");
            }
        }

        public void Open()
        {
            this.Open(this.DefaultOpenTimeout);
        }

        public void Open(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", SR.GetString(SR.SFxTimeoutOutOfRange0)));

            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, this.OpenActivityName, this.OpenActivityType);
                }
                lock (ThisLock)
                {
                    ThrowIfDisposedOrImmutable();
                    this.state = CommunicationState.Opening;
                }

                bool throwing = true;
                try
                {
                    TimeoutHelper actualTimeout = new TimeoutHelper(timeout);

                    OnOpening();
                    if (!this.onOpeningCalled)
                        throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpening"), Guid.Empty, this);

                    OnOpen(actualTimeout.RemainingTime());

                    OnOpened();
                    if (!this.onOpenedCalled)
                        throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpened"), Guid.Empty, this);

                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectOpenFailed, SR.GetString(SR.TraceCodeCommunicationObjectOpenFailed, this.GetCommunicationObjectType().ToString()), this);
                        }

                        Fault();
                    }
                }
            }
        }

        protected virtual void OnClosed()
        {
            this.onClosedCalled = true;

            lock (ThisLock)
            {
                if (this.raisedClosed)
                    return;
                this.raisedClosed = true;
                this.state = CommunicationState.Closed;
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.CommunicationObjectClosed, SR.GetString(SR.TraceCodeCommunicationObjectClosed, TraceUtility.CreateSourceString(this)), this);
            }

            EventHandler handler = Closed;
            if (handler != null)
            {
                try
                {
                    handler(eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnClosing()
        {
            this.onClosingCalled = true;

            lock (ThisLock)
            {
                if (this.raisedClosing)
                    return;
                this.raisedClosing = true;
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.CommunicationObjectClosing, SR.GetString(SR.TraceCodeCommunicationObjectClosing, TraceUtility.CreateSourceString(this)), this);
            }
            EventHandler handler = Closing;
            if (handler != null)
            {
                try
                {
                    handler(eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnFaulted()
        {
            lock (ThisLock)
            {
                if (this.raisedFaulted)
                    return;
                this.raisedFaulted = true;
            }

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectFaulted, SR.GetString(SR.TraceCodeCommunicationObjectFaulted, this.GetCommunicationObjectType().ToString()), this);
            }

            EventHandler handler = Faulted;
            if (handler != null)
            {
                try
                {
                    handler(eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnOpened()
        {
            this.onOpenedCalled = true;

            lock (ThisLock)
            {
                if (this.aborted || this.state != CommunicationState.Opening)
                    return;
                this.state = CommunicationState.Opened;
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.CommunicationObjectOpened, SR.GetString(SR.TraceCodeCommunicationObjectOpened, TraceUtility.CreateSourceString(this)), this);

            EventHandler handler = Opened;
            if (handler != null)
            {
                try
                {
                    handler(eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnOpening()
        {
            this.onOpeningCalled = true;

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.CommunicationObjectOpening, SR.GetString(SR.TraceCodeCommunicationObjectOpening, TraceUtility.CreateSourceString(this)), this);
            }

            EventHandler handler = Opening;
            if (handler != null)
            {
                try
                {
                    handler(eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        internal void ThrowIfFaulted()
        {
            this.ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    break;

                case CommunicationState.Opening:
                    break;

                case CommunicationState.Opened:
                    break;

                case CommunicationState.Closing:
                    break;

                case CommunicationState.Closed:
                    break;

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfFaulted: Unknown CommunicationObject.state");
            }
        }

        internal void ThrowIfAborted()
        {
            if (this.aborted && !this.closeCalled)
            {
                throw TraceUtility.ThrowHelperError(CreateAbortedException(), Guid.Empty, this);
            }
        }

        internal bool TraceOpenAndClose
        {
            get
            {
                return this.traceOpenAndClose;
            }
            set
            {
                this.traceOpenAndClose = value && DiagnosticUtility.ShouldUseActivity;
            }
        }

        internal void ThrowIfClosed()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    break;

                case CommunicationState.Opening:
                    break;

                case CommunicationState.Opened:
                    break;

                case CommunicationState.Closing:
                    break;

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfClosed: Unknown CommunicationObject.state");
            }
        }

        protected virtual Type GetCommunicationObjectType()
        {
            return this.GetType();
        }

        protected internal void ThrowIfDisposed()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    break;

                case CommunicationState.Opening:
                    break;

                case CommunicationState.Opened:
                    break;

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfDisposed: Unknown CommunicationObject.state");
            }
        }

        internal void ThrowIfClosedOrOpened()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    break;

                case CommunicationState.Opening:
                    break;

                case CommunicationState.Opened:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfClosedOrOpened: Unknown CommunicationObject.state");
            }
        }

        protected internal void ThrowIfDisposedOrImmutable()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    break;

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfDisposedOrImmutable: Unknown CommunicationObject.state");
            }
        }

        protected internal void ThrowIfDisposedOrNotOpen()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    break;

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfDisposedOrNotOpen: Unknown CommunicationObject.state");
            }
        }

        internal void ThrowIfNotOpened()
        {
            if (this.state == CommunicationState.Created || this.state == CommunicationState.Opening)
                throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);
        }

        internal void ThrowIfClosedOrNotOpen()
        {
            ThrowPending();

            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    break;

                case CommunicationState.Closing:
                    break;

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);

                default:
                    throw Fx.AssertAndThrow("ThrowIfClosedOrNotOpen: Unknown CommunicationObject.state");
            }
        }

        internal void ThrowPending()
        {
            ExceptionQueue queue = this.exceptionQueue;

            if (queue != null)
            {
                Exception exception = queue.GetException();

                if (exception != null)
                {
                    throw TraceUtility.ThrowHelperError(exception, Guid.Empty, this);
                }
            }
        }

        //
        // State callbacks
        //

        protected abstract void OnAbort();

        protected abstract void OnClose(TimeSpan timeout);
        protected abstract void OnEndClose(IAsyncResult result);
        protected abstract IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state);

        protected abstract void OnOpen(TimeSpan timeout);
        protected abstract IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnEndOpen(IAsyncResult result);

        class AlreadyClosedAsyncResult : CompletedAsyncResult
        {
            public AlreadyClosedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }
        }

        class ExceptionQueue
        {
            Queue<Exception> exceptions = new Queue<Exception>();
            object thisLock;

            internal ExceptionQueue(object thisLock)
            {
                this.thisLock = thisLock;
            }

            object ThisLock
            {
                get { return this.thisLock; }
            }

            public void AddException(Exception exception)
            {
                if (exception == null)
                {
                    return;
                }

                lock (this.ThisLock)
                {
                    this.exceptions.Enqueue(exception);
                }
            }

            public Exception GetException()
            {
                lock (this.ThisLock)
                {
                    if (this.exceptions.Count > 0)
                    {
                        return this.exceptions.Dequeue();
                    }
                }

                return null;
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            static AsyncCompletion onOpenCompletion = new AsyncCompletion(OnOpenCompletion);

            CommunicationObject communicationObject;
            TimeoutHelper timeout;

            public OpenAsyncResult(CommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;
                this.timeout = new TimeoutHelper(timeout);

                base.OnCompleting = new Action<AsyncResult, Exception>(OnOpenCompleted);

                if (InvokeOpen())
                {
                    this.Complete(true);
                }
            }

            bool InvokeOpen()
            {
                IAsyncResult result = this.communicationObject.OnBeginOpen(this.timeout.RemainingTime(),
                    base.PrepareAsyncCompletion(onOpenCompletion), this);
                if (result.CompletedSynchronously)
                {
                    return OnOpenCompletion(result);
                }
                else
                {
                    return false;
                }
            }

            void NotifyOpened()
            {
                this.communicationObject.OnOpened();
                if (!this.communicationObject.onOpenedCalled)
                {
                    throw TraceUtility.ThrowHelperError(
                        this.communicationObject.CreateBaseClassMethodNotCalledException("OnOpened"),
                        Guid.Empty, this.communicationObject);
                }
            }

            void OnOpenCompleted(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectOpenFailed,
                            SR.GetString(SR.TraceCodeCommunicationObjectOpenFailed, this.communicationObject.GetCommunicationObjectType().ToString()),
                            this, exception);
                    }

                    this.communicationObject.Fault();
                }
            }

            static bool OnOpenCompletion(IAsyncResult result)
            {
                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                thisPtr.communicationObject.OnEndOpen(result);
                thisPtr.NotifyOpened();
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        class CloseAsyncResult : TraceAsyncResult
        {
            static AsyncCompletion onCloseCompletion = new AsyncCompletion(OnCloseCompletion);

            CommunicationObject communicationObject;
            TimeoutHelper timeout;

            public CloseAsyncResult(CommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;
                this.timeout = new TimeoutHelper(timeout);

                base.OnCompleting = new Action<AsyncResult, Exception>(OnCloseCompleted);
                if (InvokeClose())
                {
                    this.Complete(true);
                }
            }

            bool InvokeClose()
            {
                IAsyncResult result = this.communicationObject.OnBeginClose(this.timeout.RemainingTime(),
                    base.PrepareAsyncCompletion(onCloseCompletion), this);
                if (result.CompletedSynchronously)
                {
                    return OnCloseCompletion(result);
                }
                else
                {
                    return false;
                }
            }

            void NotifyClosed()
            {
                this.communicationObject.OnClosed();
                if (!this.communicationObject.onClosedCalled)
                {
                    throw TraceUtility.ThrowHelperError(
                        this.communicationObject.CreateBaseClassMethodNotCalledException("OnClosed"),
                        Guid.Empty, this.communicationObject);
                }
            }

            void OnCloseCompleted(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.CommunicationObjectCloseFailed,
                            SR.GetString(SR.TraceCodeCommunicationObjectCloseFailed, this.communicationObject.GetCommunicationObjectType().ToString()),
                            this, exception);
                    }

                    this.communicationObject.Abort();
                }
            }

            static bool OnCloseCompletion(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                thisPtr.communicationObject.OnEndClose(result);
                thisPtr.NotifyClosed();
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }
}
