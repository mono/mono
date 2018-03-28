//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    abstract class ContextOutputChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel : class, IOutputChannel
    {           
        protected ContextOutputChannelBase(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {                       
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.InnerChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return this.InnerChannel.Via; }
        }

        protected abstract ContextProtocol ContextProtocol
        {
            get;
        }

        protected abstract bool IsClient
        {
            get;            
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SendAsyncResult(message, this, this.ContextProtocol, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }        

        public void EndSend(IAsyncResult result)
        {
            SendAsyncResult.End(result);
        }        

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IContextManager))
            {
                return (T)(object)this.ContextProtocol;
            }
            else
            {
                return base.GetProperty<T>();
            }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            CorrelationCallbackMessageProperty callback = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Message sendMessage = message;

            if (message != null)
            {
                this.ContextProtocol.OnOutgoingMessage(message, null);
                if (CorrelationCallbackMessageProperty.TryGet(message, out callback))
                {
                    ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(callback, message, this.IsClient);
                    if (callback.IsFullyDefined)
                    {
                        sendMessage = callback.FinalizeCorrelation(message, timeoutHelper.RemainingTime());
                    }
                }
            }

            try
            {
                this.InnerChannel.Send(sendMessage, timeoutHelper.RemainingTime());
            }
            finally
            {
                if (message != null && !object.ReferenceEquals(message, sendMessage))
                {
                    sendMessage.Close();
                }
            }
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }        

        class SendAsyncResult : AsyncResult
        {
            static AsyncCallback onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(OnFinalizeCorrelationCompletedCallback));
            static AsyncCallback onSend = Fx.ThunkCallback(new AsyncCallback(OnSendCompletedCallback));
            ContextOutputChannelBase<TChannel> channel;
            CorrelationCallbackMessageProperty correlationCallback;
            Message message;
            Message sendMessage;
            TimeoutHelper timeoutHelper;

            public SendAsyncResult(Message message, ContextOutputChannelBase<TChannel> channel, ContextProtocol contextProtocol,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.message = this.sendMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool shouldSend = true;

                if (message != null)
                {
                    contextProtocol.OnOutgoingMessage(message, null);
                    if (CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                    {
                        ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, this.channel.IsClient);

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

                            shouldSend = false;
                        }                        
                    }
                }

                if (shouldSend)
                {
                    IAsyncResult result = this.channel.InnerChannel.BeginSend(
                        this.message, this.timeoutHelper.RemainingTime(), onSend, this);
                    if (result.CompletedSynchronously)
                    {
                        OnSendCompleted(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                SendAsyncResult thisPtr = AsyncResult.End<SendAsyncResult>(result);
            }

            static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

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

            static void OnSendCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.OnSendCompleted(result);
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
                this.sendMessage = this.correlationCallback.EndFinalizeCorrelation(result);

                bool throwing = true;
                IAsyncResult sendResult;
                try
                {
                    sendResult = this.channel.InnerChannel.BeginSend(
                        this.sendMessage, this.timeoutHelper.RemainingTime(), onSend, this);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        if (this.message != null && !object.ReferenceEquals(this.message, this.sendMessage))
                        {
                            this.sendMessage.Close();
                        }
                    }
                }

                if (sendResult.CompletedSynchronously)
                {
                    OnSendCompleted(sendResult);
                    return true;
                }

                return false;
            }

            void OnSendCompleted(IAsyncResult result)
            {
                try
                {
                    this.channel.InnerChannel.EndSend(result);
                }
                finally
                {
                    if (this.message != null && !object.ReferenceEquals(this.message, this.sendMessage))
                    {
                        this.sendMessage.Close();
                    }
                }
            }
        }
    }
}
