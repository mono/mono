//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    abstract class ContextRequestChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel : class, IRequestChannel
    {
        ContextProtocol contextProtocol;

        protected ContextRequestChannelBase(ChannelManagerBase channelManager, TChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, innerChannel.Via, this, callbackAddress, contextManagementEnabled);
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.InnerChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return this.InnerChannel.Via; }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.contextProtocol.OnOutgoingMessage(message, null);
            return new RequestAsyncResult(message, this.InnerChannel, timeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message message = RequestAsyncResult.End(result);

            if (message != null)
            {
                this.contextProtocol.OnIncomingMessage(message);
            }

            return message;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IContextManager) && this.contextProtocol is IContextManager)
            {
                return (T)(object)this.contextProtocol;
            }
            else
            {
                return base.GetProperty<T>();
            }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            CorrelationCallbackMessageProperty callback = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Message requestMessage = message;

            this.contextProtocol.OnOutgoingMessage(message, null);
            if (message != null && CorrelationCallbackMessageProperty.TryGet(message, out callback))
            {
                ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(callback, message, true);
                if (callback.IsFullyDefined)
                {
                    requestMessage = callback.FinalizeCorrelation(message, timeoutHelper.RemainingTime());
                }
            }

            Message response = null;
            try
            {
                response = this.InnerChannel.Request(requestMessage, timeout);
                if (response != null)
                {
                    this.contextProtocol.OnIncomingMessage(response);
                }
            }
            finally
            {
                if (message != null && !object.ReferenceEquals(message, requestMessage))
                {
                    requestMessage.Close();
                }
            }

            return response;
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.DefaultSendTimeout);
        }

        class RequestAsyncResult : AsyncResult
        {

            static AsyncCallback onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(OnFinalizeCorrelationCompletedCallback));
            static AsyncCallback onRequest = Fx.ThunkCallback(new AsyncCallback(OnRequestCompletedCallback));
            IRequestChannel channel;
            CorrelationCallbackMessageProperty correlationCallback;
            Message message;
            Message replyMessage;
            Message requestMessage;
            TimeoutHelper timeoutHelper;

            public RequestAsyncResult(Message message, IRequestChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.message = this.requestMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool shouldRequest = true;

                if (message != null)
                {
                    if (CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                    {
                        ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, true);

                        if (this.correlationCallback.IsFullyDefined)
                        {
                            IAsyncResult result = this.correlationCallback.BeginFinalizeCorrelation(this.message, this.timeoutHelper.RemainingTime(), onFinalizeCorrelation, this);
                            if (result.CompletedSynchronously)
                            {
                                if (OnFinalizeCorrelationCompleted(result))
                                {
                                    base.Complete(true);
                                }
                            }

                            shouldRequest = false;
                        }
                    }
                }

                if (shouldRequest)
                {
                    IAsyncResult result = this.channel.BeginRequest(this.message, this.timeoutHelper.RemainingTime(), onRequest, this);
                    if (result.CompletedSynchronously)
                    {
                        OnRequestCompleted(result);
                        base.Complete(true);
                    }
                }
            }

            public static Message End(IAsyncResult result)
            {
                RequestAsyncResult thisPtr = AsyncResult.End<RequestAsyncResult>(result);
                return thisPtr.replyMessage;
            }

            static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                RequestAsyncResult thisPtr = (RequestAsyncResult)result.AsyncState;

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

            static void OnRequestCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                RequestAsyncResult thisPtr = (RequestAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.OnRequestCompleted(result);
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
                this.requestMessage = this.correlationCallback.EndFinalizeCorrelation(result);
                this.requestMessage.Properties.Remove(CorrelationCallbackMessageProperty.Name);

                bool throwing = true;
                IAsyncResult requestResult;
                try
                {
                    requestResult = this.channel.BeginRequest(this.requestMessage, this.timeoutHelper.RemainingTime(), onRequest, this);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        if (this.message != null && !object.ReferenceEquals(this.message, this.requestMessage))
                        {
                            this.requestMessage.Close();
                        }
                    }
                }

                if (requestResult.CompletedSynchronously)
                {
                    OnRequestCompleted(requestResult);
                    return true;
                }

                return false;
            }

            void OnRequestCompleted(IAsyncResult result)
            {
                try
                {
                    this.replyMessage = this.channel.EndRequest(result);
                }
                finally
                {
                    if (this.message != null && !object.ReferenceEquals(this.message, this.requestMessage))
                    {
                        this.requestMessage.Close();
                    }
                }
            }
        }
    }
}
