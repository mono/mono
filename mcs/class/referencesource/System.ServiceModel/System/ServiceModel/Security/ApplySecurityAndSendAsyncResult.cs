//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    abstract class ApplySecurityAndSendAsyncResult<MessageSenderType> : AsyncResult
        where MessageSenderType : class
    {
        readonly MessageSenderType channel;
        readonly SecurityProtocol binding;
        volatile bool secureOutgoingMessageDone;
        static AsyncCallback sharedCallback = Fx.ThunkCallback(new AsyncCallback(SharedCallback));
        SecurityProtocolCorrelationState newCorrelationState;
        TimeoutHelper timeoutHelper;

        public ApplySecurityAndSendAsyncResult(SecurityProtocol binding, MessageSenderType channel, TimeSpan timeout,
            AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.binding = binding;
            this.channel = channel;
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        protected SecurityProtocolCorrelationState CorrelationState
        {
            get { return newCorrelationState; }
        }
        
        protected SecurityProtocol SecurityProtocol
        {
            get { return this.binding; }
        }

        protected void Begin(Message message, SecurityProtocolCorrelationState correlationState)
        {
            IAsyncResult result = this.binding.BeginSecureOutgoingMessage(message, timeoutHelper.RemainingTime(), correlationState, sharedCallback, this);
            if (result.CompletedSynchronously)
            {
                this.binding.EndSecureOutgoingMessage(result, out message, out newCorrelationState);
                bool completedSynchronously = this.OnSecureOutgoingMessageComplete(message);
                if (completedSynchronously)
                {
                    Complete(true);
                }
            }
        }

        protected static void OnEnd(ApplySecurityAndSendAsyncResult<MessageSenderType> self)
        {
            AsyncResult.End<ApplySecurityAndSendAsyncResult<MessageSenderType>>(self);
        }

        bool OnSecureOutgoingMessageComplete(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            this.secureOutgoingMessageDone = true;
            IAsyncResult result = BeginSendCore(this.channel, message, timeoutHelper.RemainingTime(), sharedCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            EndSendCore(this.channel, result);
            return this.OnSendComplete();
        }

        protected abstract IAsyncResult BeginSendCore(MessageSenderType channel, Message message, TimeSpan timeout, AsyncCallback callback, object state);

        protected abstract void EndSendCore(MessageSenderType channel, IAsyncResult result);

        bool OnSendComplete()
        {
            OnSendCompleteCore(timeoutHelper.RemainingTime());
            return true;
        }

        protected abstract void OnSendCompleteCore(TimeSpan timeout);

        static void SharedCallback(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("result"));
            }
            if (result.CompletedSynchronously)
            {
                return;
            }
            ApplySecurityAndSendAsyncResult<MessageSenderType> self = result.AsyncState as ApplySecurityAndSendAsyncResult<MessageSenderType>;
            if (self == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidAsyncResult), "result"));
            }

            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                if (!self.secureOutgoingMessageDone)
                {
                    Message message;
                    self.binding.EndSecureOutgoingMessage(result, out message, out self.newCorrelationState);
                    completeSelf = self.OnSecureOutgoingMessageComplete(message);
                }
                else
                {
                    self.EndSendCore(self.channel, result);
                    completeSelf = self.OnSendComplete();
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
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
                self.Complete(false, completionException);
            }
        }
    }
}

