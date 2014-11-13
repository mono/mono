//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    abstract class ReceiveMessageAndVerifySecurityAsyncResultBase : AsyncResult
    {
        static AsyncCallback innerTryReceiveCompletedCallback = Fx.ThunkCallback(new AsyncCallback(InnerTryReceiveCompletedCallback));
        Message message;
        bool receiveCompleted;
        TimeoutHelper timeoutHelper;
        IInputChannel innerChannel;

        protected ReceiveMessageAndVerifySecurityAsyncResultBase(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.innerChannel = innerChannel;
        }

        public void Start()
        {
            IAsyncResult asyncResult = innerChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), innerTryReceiveCompletedCallback, this);
            if (!asyncResult.CompletedSynchronously)
            {
                return;
            }
            bool innerReceiveCompleted = innerChannel.EndTryReceive(asyncResult, out this.message);
            if (!innerReceiveCompleted)
            {
                receiveCompleted = false;
            }
            else
            {
                receiveCompleted = true;
                bool completedSynchronously = this.OnInnerReceiveDone(ref this.message, this.timeoutHelper.RemainingTime());
                if (!completedSynchronously)
                {
                    return;
                }
            }
            Complete(true);

        }

        static void InnerTryReceiveCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            ReceiveMessageAndVerifySecurityAsyncResultBase thisResult = (ReceiveMessageAndVerifySecurityAsyncResultBase)result.AsyncState;
            Exception completionException = null;
            bool completeSelf = false;
            try
            {
                bool innerReceiveCompleted = thisResult.innerChannel.EndTryReceive(result, out thisResult.message);
                if (!innerReceiveCompleted)
                {
                    thisResult.receiveCompleted = false;
                    completeSelf = true;
                }
                else
                {
                    thisResult.receiveCompleted = true;
                    completeSelf = thisResult.OnInnerReceiveDone(ref thisResult.message, thisResult.timeoutHelper.RemainingTime());
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                completeSelf = true;
                completionException = e;
            }
            if (completeSelf)
            {
                thisResult.Complete(false, completionException);
            }
        }

        protected abstract bool OnInnerReceiveDone(ref Message message, TimeSpan timeout);

        public static bool End(IAsyncResult result, out Message message)
        {
            ReceiveMessageAndVerifySecurityAsyncResultBase thisResult = AsyncResult.End<ReceiveMessageAndVerifySecurityAsyncResultBase>(result);
            message = thisResult.message;
            return thisResult.receiveCompleted;
        }
    }
}
