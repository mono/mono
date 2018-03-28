//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    abstract class RequestContextBase : RequestContext
    {
        TimeSpan defaultSendTimeout;
        TimeSpan defaultCloseTimeout;
        CommunicationState state = CommunicationState.Opened;
        Message requestMessage;
        Exception requestMessageException;
        bool replySent;
        bool replyInitiated;
        bool aborted;
        object thisLock = new object();

        protected RequestContextBase(Message requestMessage, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            this.defaultSendTimeout = defaultSendTimeout;
            this.defaultCloseTimeout = defaultCloseTimeout;
            this.requestMessage = requestMessage;
        }

        public void ReInitialize(Message requestMessage)
        {
            this.state = CommunicationState.Opened;
            this.requestMessageException = null;
            this.replySent = false;
            this.replyInitiated = false;
            this.aborted = false;
            this.requestMessage = requestMessage;
        }

        public override Message RequestMessage
        {
            get
            {
                if (this.requestMessageException != null)
                {
#pragma warning suppress 56503 // Microsoft, see outcome of DCR 50092
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.requestMessageException);
                }

                return requestMessage;
            }
        }

        protected void SetRequestMessage(Message requestMessage)
        {
            Fx.Assert(this.requestMessageException == null, "Cannot have both a requestMessage and a requestException.");
            this.requestMessage = requestMessage;
        }

        protected void SetRequestMessage(Exception requestMessageException)
        {
            Fx.Assert(this.requestMessage == null, "Cannot have both a requestMessage and a requestException.");
            this.requestMessageException = requestMessageException;
        }

        protected bool ReplyInitiated
        {
            get { return this.replyInitiated; }
        }

        protected object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        public bool Aborted
        {
            get
            {
                return this.aborted;
            }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get { return this.defaultCloseTimeout; }
        }

        public TimeSpan DefaultSendTimeout
        {
            get { return this.defaultSendTimeout; }
        }

        public override void Abort()
        {
            lock (ThisLock)
            {
                if (state == CommunicationState.Closed)
                    return;

                state = CommunicationState.Closing;

                this.aborted = true;
            }

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.RequestContextAbort,
                    SR.GetString(SR.TraceCodeRequestContextAbort), this);
            }

            try
            {
                this.OnAbort();
            }
            finally
            {
                state = CommunicationState.Closed;
            }
        }

        public override void Close()
        {
            this.Close(this.defaultCloseTimeout);
        }

        public override void Close(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout,
                    SR.GetString(SR.ValueMustBeNonNegative)));
            }

            bool sendAck = false;
            lock (ThisLock)
            {
                if (state != CommunicationState.Opened)
                    return;

                if (TryInitiateReply())
                {
                    sendAck = true;
                }

                state = CommunicationState.Closing;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            bool throwing = true;

            try
            {
                if (sendAck)
                {
                    OnReply(null, timeoutHelper.RemainingTime());
                }

                OnClose(timeoutHelper.RemainingTime());
                state = CommunicationState.Closed;
                throwing = false;
            }
            finally
            {
                if (throwing)
                    this.Abort();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            if (this.replySent)
            {
                this.Close();
            }
            else
            {
                this.Abort();
            }
        }

        protected abstract void OnAbort();
        protected abstract void OnClose(TimeSpan timeout);
        protected abstract void OnReply(Message message, TimeSpan timeout);
        protected abstract IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnEndReply(IAsyncResult result);

        protected void ThrowIfInvalidReply()
        {
            if (state == CommunicationState.Closed || state == CommunicationState.Closing)
            {
                if (aborted)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(SR.GetString(SR.RequestContextAborted)));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }

            if (this.replyInitiated)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ReplyAlreadySent)));
        }

        /// <summary>
        /// Attempts to initiate the reply. If a reply is not initiated already (and the object is opened), 
        /// then it initiates the reply and returns true. Otherwise, it returns false.
        /// </summary>
        protected bool TryInitiateReply()
        {
            lock (this.thisLock)
            {
                if ((this.state != CommunicationState.Opened) || this.replyInitiated)
                {
                    return false;
                }
                else
                {
                    this.replyInitiated = true;
                    return true;
                }
            }
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.BeginReply(message, this.defaultSendTimeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            // "null" is a valid reply (signals a 202-style "ack"), so we don't have a null-check here
            lock (this.thisLock)
            {
                this.ThrowIfInvalidReply();
                this.replyInitiated = true;
            }

            return OnBeginReply(message, timeout, callback, state);
        }

        public override void EndReply(IAsyncResult result)
        {
            OnEndReply(result);
            this.replySent = true;
        }

        public override void Reply(Message message)
        {
            this.Reply(message, this.defaultSendTimeout);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            // "null" is a valid reply (signals a 202-style "ack"), so we don't have a null-check here
            lock (this.thisLock)
            {
                this.ThrowIfInvalidReply();
                this.replyInitiated = true;
            }

            this.OnReply(message, timeout);
            this.replySent = true;
        }

        // This method is designed for WebSocket only, and will only be used once the WebSocket response was sent.
        // For WebSocket, we never call HttpRequestContext.Reply to send the response back. 
        // Instead we call AcceptWebSocket directly. So we need to set the replyInitiated and 
        // replySent boolean to be true once the response was sent successfully. Otherwise when we 
        // are disposing the HttpRequestContext, we will see a bunch of warnings in trace log.
        protected void SetReplySent()
        {
            lock (this.thisLock)
            {
                this.ThrowIfInvalidReply();
                this.replyInitiated = true;
            }

            this.replySent = true;
        }
    }

    class RequestContextMessageProperty : IDisposable
    {
        RequestContext context;
        object thisLock = new object();

        public RequestContextMessageProperty(RequestContext context)
        {
            this.context = context;
        }

        public static string Name
        {
            get { return "requestContext"; }
        }

        void IDisposable.Dispose()
        {
            bool success = false;
            RequestContext thisContext;

            lock (this.thisLock)
            {
                if (this.context == null)
                    return;
                thisContext = this.context;
                this.context = null;
            }

            try
            {
                thisContext.Close();
                success = true;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!success)
                {
                    thisContext.Abort();
                }
            }
        }
    }
}
