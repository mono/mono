//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    class OpenCollectionAsyncResult : AsyncResult
    {
        bool completedSynchronously;
        Exception exception;
        static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(Callback));
        int count;
        TimeoutHelper timeoutHelper;

        public OpenCollectionAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, IList<ICommunicationObject> collection)
            : base(otherCallback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            completedSynchronously = true;

            count = collection.Count;
            if (count == 0)
            {
                Complete(true);
                return;
            }

            for (int index = 0; index < collection.Count; index++)
            {
                // Throw exception if there was a failure calling EndOpen in the callback (skips remaining items)
                if (this.exception != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                CallbackState callbackState = new CallbackState(this, collection[index]);
                IAsyncResult result = collection[index].BeginOpen(this.timeoutHelper.RemainingTime(), nestedCallback, callbackState);
                if (result.CompletedSynchronously)
                {
                    collection[index].EndOpen(result);
                    Decrement(true);
                }
            }
        }

        static void Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            CallbackState callbackState = (CallbackState)result.AsyncState;
            try
            {
                callbackState.Instance.EndOpen(result);
                callbackState.Result.Decrement(false);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                callbackState.Result.Decrement(false, e);
            }
        }

        void Decrement(bool completedSynchronously)
        {
            if (completedSynchronously == false)
                this.completedSynchronously = false;
            if (Interlocked.Decrement(ref count) == 0)
            {
                if (this.exception != null)
                    Complete(this.completedSynchronously, this.exception);
                else
                    Complete(this.completedSynchronously);
            }
        }

        void Decrement(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Decrement(completedSynchronously);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<OpenCollectionAsyncResult>(result);
        }

        class CallbackState
        {
            ICommunicationObject instance;
            OpenCollectionAsyncResult result;

            public CallbackState(OpenCollectionAsyncResult result, ICommunicationObject instance)
            {
                this.result = result;
                this.instance = instance;
            }

            public ICommunicationObject Instance
            {
                get { return instance; }
            }

            public OpenCollectionAsyncResult Result
            {
                get { return result; }
            }
        }
    }
}
