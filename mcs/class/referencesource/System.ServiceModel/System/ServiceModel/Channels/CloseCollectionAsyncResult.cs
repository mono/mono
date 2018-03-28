//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    class CloseCollectionAsyncResult : AsyncResult
    {
        bool completedSynchronously;
        Exception exception;
        static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(Callback));
        int count;

        public CloseCollectionAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, IList<ICommunicationObject> collection)
            : base(otherCallback, state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            completedSynchronously = true;

            count = collection.Count;
            if (count == 0)
            {
                Complete(true);
                return;
            }

            for (int index = 0; index < collection.Count; index++)
            {
                CallbackState callbackState = new CallbackState(this, collection[index]);
                IAsyncResult result;
                try
                {
                    result = collection[index].BeginClose(timeoutHelper.RemainingTime(), nestedCallback, callbackState);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    Decrement(true, e);
                    collection[index].Abort();
                    continue;
                }

                if (result.CompletedSynchronously)
                {
                    CompleteClose(collection[index], result);
                }
            }
        }

        void CompleteClose(ICommunicationObject communicationObject, IAsyncResult result)
        {
            Exception closeException = null;
            try
            {
                communicationObject.EndClose(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                closeException = e;
                communicationObject.Abort();
            }

            Decrement(result.CompletedSynchronously, closeException);
        }

        static void Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            CallbackState callbackState = (CallbackState)result.AsyncState;
            callbackState.Result.CompleteClose(callbackState.Instance, result);
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
            AsyncResult.End<CloseCollectionAsyncResult>(result);
        }

        class CallbackState
        {
            ICommunicationObject instance;
            CloseCollectionAsyncResult result;

            public CallbackState(CloseCollectionAsyncResult result, ICommunicationObject instance)
            {
                this.result = result;
                this.instance = instance;
            }

            public ICommunicationObject Instance
            {
                get { return instance; }
            }

            public CloseCollectionAsyncResult Result
            {
                get { return result; }
            }
        }
    }
}
