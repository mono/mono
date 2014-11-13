//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;

    internal delegate IAsyncResult ChainedBeginHandler(TimeSpan timeout, AsyncCallback asyncCallback, object state);
    internal delegate void ChainedEndHandler(IAsyncResult result);

    class ChainedAsyncResult : AsyncResult
    {
        ChainedBeginHandler begin2;
        ChainedEndHandler end1;
        ChainedEndHandler end2;
        TimeoutHelper timeoutHelper;
        static AsyncCallback begin1Callback = Fx.ThunkCallback(new AsyncCallback(Begin1Callback));
        static AsyncCallback begin2Callback = Fx.ThunkCallback(new AsyncCallback(Begin2Callback));

        protected ChainedAsyncResult(TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        public ChainedAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, ChainedBeginHandler begin2, ChainedEndHandler end2)
            : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            Begin(begin1, end1, begin2, end2);
        }

        protected void Begin(ChainedBeginHandler begin1, ChainedEndHandler end1, ChainedBeginHandler begin2, ChainedEndHandler end2)
        {
            this.end1 = end1;
            this.begin2 = begin2;
            this.end2 = end2;

            IAsyncResult result = begin1(this.timeoutHelper.RemainingTime(), begin1Callback, this);
            if (!result.CompletedSynchronously)
                return;

            if (Begin1Completed(result))
            {
                this.Complete(true);
            }
        }

        static void Begin1Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;

            ChainedAsyncResult thisPtr = (ChainedAsyncResult)result.AsyncState;

            bool completeSelf = false;
            Exception completeException = null;

            try
            {
                completeSelf = thisPtr.Begin1Completed(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                completeSelf = true;
                completeException = exception;
            }

            if (completeSelf)
            {
                thisPtr.Complete(false, completeException);
            }
        }

        bool Begin1Completed(IAsyncResult result)
        {
            end1(result);

            result = begin2(this.timeoutHelper.RemainingTime(), begin2Callback, this);
            if (!result.CompletedSynchronously)
                return false;

            end2(result);
            return true;
        }

        static void Begin2Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;

            ChainedAsyncResult thisPtr = (ChainedAsyncResult)result.AsyncState;

            Exception completeException = null;

            try
            {
                thisPtr.end2(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                completeException = exception;
            }

            thisPtr.Complete(false, completeException);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ChainedAsyncResult>(result);
        }
    }

    internal class ChainedCloseAsyncResult : ChainedAsyncResult
    {
        IList<ICommunicationObject> collection;

        public ChainedCloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, IList<ICommunicationObject> collection)
            : base(timeout, callback, state)
        {
            this.collection = collection;

            Begin(BeginClose, EndClose, begin1, end1);
        }

        public ChainedCloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, params ICommunicationObject[] objs)
            : base(timeout, callback, state)
        {
            collection = new List<ICommunicationObject>();
            if (objs != null)
                for (int index = 0; index < objs.Length; index++)
                    if (objs[index] != null)
                        collection.Add(objs[index]);

            Begin(BeginClose, EndClose, begin1, end1);
        }

        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCollectionAsyncResult(timeout, callback, state, collection);
        }

        void EndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End((CloseCollectionAsyncResult)result);
        }
    }

    internal class ChainedOpenAsyncResult : ChainedAsyncResult
    {
        IList<ICommunicationObject> collection;

        public ChainedOpenAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, IList<ICommunicationObject> collection)
            : base(timeout, callback, state)
        {
            this.collection = collection;

            Begin(begin1, end1, BeginOpen, EndOpen);
        }

        public ChainedOpenAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, params ICommunicationObject[] objs)
            : base(timeout, callback, state)
        {
            collection = new List<ICommunicationObject>();

            for (int index = 0; index < objs.Length; index++)
                if (objs[index] != null)
                    collection.Add(objs[index]);

            Begin(begin1, end1, BeginOpen, EndOpen);
        }

        IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCollectionAsyncResult(timeout, callback, state, collection);
        }

        void EndOpen(IAsyncResult result)
        {
            OpenCollectionAsyncResult.End((OpenCollectionAsyncResult)result);
        }
    }
}
