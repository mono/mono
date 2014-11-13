//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Runtime;
    using System.ServiceModel.Activities.Dispatcher;
    using System.Threading;

    [Fx.Tag.XamlVisible(false)]
    public sealed class WorkflowHostingResponseContext
    {
        AsyncWaitHandle responseWaitHandle;
        WorkflowOperationContext context;
        object returnValue;
        object[] outputs;

        // Used by Creation Endpoint
        internal WorkflowHostingResponseContext()
        {
            this.responseWaitHandle = new AsyncWaitHandle(EventResetMode.AutoReset);
        }

        // Used by BookmarkResumption Endpoint
        internal WorkflowHostingResponseContext(WorkflowOperationContext context)
        {
            this.context = context;
        }

        public void SendResponse(object returnValue, object[] outputs)
        {
            this.returnValue = returnValue;
            this.outputs = outputs ?? EmptyArray.Allocate(0);

            if (this.responseWaitHandle != null)
            {
                this.responseWaitHandle.Set();
            }
            else
            {
                Fx.Assert(this.context != null, "context must not be null!");
                if (this.returnValue is Exception)
                {
                    this.context.SendFault((Exception)this.returnValue);
                }
                else
                {
                    this.context.SendReply(this.returnValue, this.outputs);
                }
            }
        }

        object GetResponse(out object[] outputs)
        {
            if (this.returnValue is Exception)
            {
                throw FxTrace.Exception.AsError((Exception)this.returnValue);
            }
            outputs = this.outputs;
            return this.returnValue;
        }

        internal IAsyncResult BeginGetResponse(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(this.responseWaitHandle != null, "this.responseWaitHandle must not be null!");
            return GetResponseAsyncResult.Create(this, timeout, callback, state);
        }

        internal object EndGetResponse(IAsyncResult result, out object[] outputs)
        {
            return GetResponseAsyncResult.End(result, out outputs);
        }

        class GetResponseAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> handleEndWait = new Action<object, TimeoutException>(HandleEndWait);

            WorkflowHostingResponseContext context;

            GetResponseAsyncResult(WorkflowHostingResponseContext context, TimeSpan timeout, AsyncCallback callback, object state)
               : base(callback, state)
            {
                this.context = context;
                if (context.responseWaitHandle.WaitAsync(handleEndWait, this, timeout))
                {
                    Complete(true);
                }
            }

            public static GetResponseAsyncResult Create(WorkflowHostingResponseContext context, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new GetResponseAsyncResult(context, timeout, callback, state);
            }

            public static object End(IAsyncResult result, out object[] outputs)
            {
                GetResponseAsyncResult thisPtr = AsyncResult.End<GetResponseAsyncResult>(result);
                return thisPtr.context.GetResponse(out outputs);
            }

            static void HandleEndWait(object state, TimeoutException e)
            {
                GetResponseAsyncResult thisPtr = (GetResponseAsyncResult)state;
                thisPtr.Complete(false, e);
            }
        }
    }
}
