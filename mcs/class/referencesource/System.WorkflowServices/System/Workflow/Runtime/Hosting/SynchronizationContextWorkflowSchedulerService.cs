//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Runtime.Hosting
{
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Diagnostics;

    class SynchronizationContextWorkflowSchedulerService : DefaultWorkflowSchedulerService
    {
        SynchronizationContext syncContext;

        public SynchronizationContextWorkflowSchedulerService()
        {
        }

        protected internal override void Cancel(Guid timerId)
        {
            base.Cancel(timerId);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.TraceCodeSyncContextSchedulerServiceTimerCancelled, timerId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.SyncContextSchedulerServiceTimerCancelled, traceText,
                    new StringTraceRecord("TimerDetail", traceText),
                    this, null);
            }
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            base.Schedule(callback, workflowInstanceId, whenUtc, timerId);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.TraceCodeSyncContextSchedulerServiceTimerCreated, timerId, workflowInstanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.SyncContextSchedulerServiceTimerCreated, traceText,
                    new StringTraceRecord("TimerDetail", traceText),
                    this, null);
            }
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId)
        {
            if (callback == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callback");
            }

            WorkflowDispatchContext currentDispatchContext = WorkflowDispatchContext.Current;

            if (currentDispatchContext != null && currentDispatchContext.IsSynchronous)
            {
                callback(workflowInstanceId);
            }
            else
            {
                if (this.syncContext != null)
                {
                    SynchronizationContextPostHelper.Post(this.syncContext, Fx.ThunkCallback(new SendOrPostCallback(callback)), workflowInstanceId);
                }
                else
                {
                    base.Schedule(callback, workflowInstanceId);
                }
            }
        }

        internal void SetSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            this.syncContext = synchronizationContext;
        }

        internal static class SynchronizationContextPostHelper
        {
            static SendOrPostCallback wrapperCallback = 
                Fx.ThunkCallback(new SendOrPostCallback(SynchronizationContextPostHelper.Callback));

            public static void Post(SynchronizationContext synchronizationContext, SendOrPostCallback callback, object state)
            {
                Fx.Assert(synchronizationContext != null, "Null Sync Context");
                Fx.Assert(callback != null, "Null Callback");

                synchronizationContext.OperationStarted();
                synchronizationContext.Post(wrapperCallback, new PostCallbackState(synchronizationContext, callback, state));
            }

            static void Callback(object state)
            {
                if (state == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("state");
                }

                PostCallbackState postCallbackState = state as PostCallbackState;
                Fx.Assert(postCallbackState != null, "Invalid state object passed to callback function");
                try
                {
                    postCallbackState.Callback(postCallbackState.State);
                }
                finally
                {
                    postCallbackState.SynchronizationContext.OperationCompleted();
                }
            }

            class PostCallbackState
            {
                SendOrPostCallback callback;
                object callbackState;
                SynchronizationContext synchronizationContext;

                public PostCallbackState(SynchronizationContext synchronizationContext, SendOrPostCallback callback, object callbackState)
                {
                    this.synchronizationContext = synchronizationContext;
                    this.callback = callback;
                    this.callbackState = callbackState;
                }

                public SendOrPostCallback Callback
                {
                    get
                    {
                        return this.callback;
                    }
                }

                public object State
                {
                    get
                    {
                        return this.callbackState;
                    }
                }

                public SynchronizationContext SynchronizationContext
                {
                    get
                    {
                        return this.synchronizationContext;
                    }
                }
            }
        }
    }
}
