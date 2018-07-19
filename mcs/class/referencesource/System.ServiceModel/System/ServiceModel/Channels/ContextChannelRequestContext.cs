//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    class ContextChannelRequestContext : RequestContext
    {
        ContextProtocol contextProtocol;
        TimeSpan defaultSendTimeout;
        RequestContext innerContext;

        public ContextChannelRequestContext(RequestContext innerContext, ContextProtocol contextProtocol, TimeSpan defaultSendTimeout)
        {
            if (innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerContext");
            }
            if (contextProtocol == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextProtocol");
            }

            this.innerContext = innerContext;
            this.contextProtocol = contextProtocol;
            this.defaultSendTimeout = defaultSendTimeout;
        }

        public override Message RequestMessage
        {
            get { return this.innerContext.RequestMessage; }
        }

        public override void Abort()
        {
            this.innerContext.Abort();
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReplyAsyncResult(message, this, timeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.BeginReply(message, this.defaultSendTimeout, callback, state);
        }

        public override void Close(TimeSpan timeout)
        {
            this.innerContext.Close(timeout);
        }

        public override void Close()
        {
            this.innerContext.Close();
        }

        public override void EndReply(IAsyncResult result)
        {
            ReplyAsyncResult.End(result);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Message replyMessage = message;

            if (message != null)
            {
                this.contextProtocol.OnOutgoingMessage(message, this);

                CorrelationCallbackMessageProperty callback;
                if (CorrelationCallbackMessageProperty.TryGet(message, out callback))
                {
                    ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(callback, message, false);

                    if (callback.IsFullyDefined)
                    {
                        replyMessage = callback.FinalizeCorrelation(message, timeoutHelper.RemainingTime());
                        // we are done finalizing correlation, removing the messageproperty since we do not need it anymore
                        replyMessage.Properties.Remove(CorrelationCallbackMessageProperty.Name);
                    }

                }
            }

            try
            {
                this.innerContext.Reply(replyMessage, timeoutHelper.RemainingTime());
            }
            finally
            {
                if (message != null && !object.ReferenceEquals(message, replyMessage))
                {
                    replyMessage.Close();
                }
            }
        }

        public override void Reply(Message message)
        {
            this.Reply(message, this.defaultSendTimeout);
        }

        class ReplyAsyncResult : AsyncResult
        {

            static AsyncCallback onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(OnFinalizeCorrelationCompletedCallback));
            static AsyncCallback onReply = Fx.ThunkCallback(new AsyncCallback(OnReplyCompletedCallback));
            ContextChannelRequestContext context;
            CorrelationCallbackMessageProperty correlationCallback;
            Message message;
            Message replyMessage;
            TimeoutHelper timeoutHelper;

            public ReplyAsyncResult(Message message, ContextChannelRequestContext context, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;
                this.message = this.replyMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool shouldReply = true;

                if (message != null)
                {
                    this.context.contextProtocol.OnOutgoingMessage(message, this.context);

                    if (CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                    {
                        ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, false);

                        if (this.correlationCallback.IsFullyDefined)
                        {
                            IAsyncResult result = correlationCallback.BeginFinalizeCorrelation(this.message, this.timeoutHelper.RemainingTime(), onFinalizeCorrelation, this);
                            if (result.CompletedSynchronously)
                            {
                                if (OnFinalizeCorrelationCompleted(result))
                                {
                                    base.Complete(true);
                                }
                            }

                            shouldReply = false;
                        }
                    }
                }

                if (shouldReply)
                {
                    IAsyncResult result = this.context.innerContext.BeginReply(this.message, this.timeoutHelper.RemainingTime(), onReply, this);
                    if (result.CompletedSynchronously)
                    {
                        OnReplyCompleted(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReplyAsyncResult>(result);
            }

            static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ReplyAsyncResult thisPtr = (ReplyAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.OnFinalizeCorrelationCompleted(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnReplyCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ReplyAsyncResult thisPtr = (ReplyAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.OnReplyCompleted(result);
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

            bool OnFinalizeCorrelationCompleted(IAsyncResult result)
            {
                this.replyMessage = this.correlationCallback.EndFinalizeCorrelation(result);

                bool throwing = true;
                IAsyncResult replyResult;
                try
                {
                    replyResult = this.context.innerContext.BeginReply(this.replyMessage, this.timeoutHelper.RemainingTime(), onReply, this);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        if (this.message != null && !object.ReferenceEquals(this.message, this.replyMessage))
                        {
                            this.replyMessage.Close();
                        }
                    }
                }

                if (replyResult.CompletedSynchronously)
                {
                    OnReplyCompleted(replyResult);
                    return true;
                }

                return false;
            }

            void OnReplyCompleted(IAsyncResult result)
            {
                try
                {
                    this.context.innerContext.EndReply(result);
                }
                finally
                {
                    if (this.message != null && !object.ReferenceEquals(this.message, this.replyMessage))
                    {
                        this.replyMessage.Close();
                    }
                }
            }
        }
    }
}
