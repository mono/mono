//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime;
    using System.Diagnostics;

    abstract class TraceAsyncResult : AsyncResult
    {
        static Action<AsyncCallback, IAsyncResult> waitResultCallback = new Action<AsyncCallback, IAsyncResult>(DoCallback);

        protected TraceAsyncResult(AsyncCallback callback, object state) :
            base(callback, state)
        {
            if (TraceUtility.MessageFlowTracingOnly)
            {
                this.CallbackActivity = ServiceModelActivity.CreateLightWeightAsyncActivity(Trace.CorrelationManager.ActivityId);
                base.VirtualCallback = waitResultCallback;
            } 
            else if (DiagnosticUtility.ShouldUseActivity)
            {
                this.CallbackActivity = ServiceModelActivity.Current;
                if (this.CallbackActivity != null)
                {
                    base.VirtualCallback = waitResultCallback;
                }
            }
        }

        public ServiceModelActivity CallbackActivity
        {
            get;
            private set;
        }

        static void DoCallback(AsyncCallback callback, IAsyncResult result)
        {
            if (result is TraceAsyncResult)
            {
                TraceAsyncResult thisPtr = result as TraceAsyncResult;
                Fx.Assert(thisPtr.CallbackActivity != null, "this shouldn't be hooked up if we don't have a CallbackActivity");

                if (TraceUtility.MessageFlowTracingOnly)
                {
                    Trace.CorrelationManager.ActivityId = thisPtr.CallbackActivity.Id;
                    thisPtr.CallbackActivity = null;
                }

                using (ServiceModelActivity.BoundOperation(thisPtr.CallbackActivity))
                {
                    callback(result);
                }
            }
        }
    }
}
