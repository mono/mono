//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Activities.Hosting;

    class AsyncOperationContext
    {
        static AsyncCallback onResumeAsyncCodeActivityBookmark;
     
        ActivityExecutor executor;
        ActivityInstance owningActivityInstance;
        bool hasCanceled;
        bool hasCompleted;

        internal AsyncOperationContext(ActivityExecutor executor, ActivityInstance owningActivityInstance)
        {
            this.executor = executor;
            this.owningActivityInstance = owningActivityInstance;
        }

        internal bool IsStillActive
        {
            get
            {
                return !this.hasCanceled && !this.hasCompleted;
            }
        }

        public object UserState
        {
            get;
            set;
        }

        public bool HasCalledAsyncCodeActivityCancel
        {
            get;
            set;
        }

        public bool IsAborting
        {
            get;
            set;
        }

        bool ShouldCancel()
        {
            return this.IsStillActive;
        }

        bool ShouldComplete()
        {
            if (this.hasCanceled)
            {
                return false;
            }

            if (this.hasCompleted)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.OperationAlreadyCompleted));
            }

            return true;
        }

        internal void CancelOperation()
        {
            if (ShouldCancel())
            {
                this.executor.CompleteOperation(this.owningActivityInstance);
            }

            this.hasCanceled = true;
        }

        public void CompleteOperation()
        {
            if (ShouldComplete())
            {
                this.executor.CompleteOperation(this.owningActivityInstance);

                this.hasCompleted = true;
            }
        }

        // used by AsyncCodeActivity to efficiently complete a "true" async operation
        internal void CompleteAsyncCodeActivity(CompleteData completeData)
        {
            Fx.Assert(completeData != null, "caller must validate this is not null");

            if (!this.ShouldComplete())
            {
                // nothing to do here
                return;
            }

            if (onResumeAsyncCodeActivityBookmark == null)
            {
                onResumeAsyncCodeActivityBookmark = Fx.ThunkCallback(new AsyncCallback(OnResumeAsyncCodeActivityBookmark));
            }

            try
            {
                IAsyncResult result = this.executor.BeginResumeBookmark(Bookmark.AsyncOperationCompletionBookmark,
                    completeData, TimeSpan.MaxValue, onResumeAsyncCodeActivityBookmark, this.executor);
                if (result.CompletedSynchronously)
                {
                    this.executor.EndResumeBookmark(result);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.executor.AbortWorkflowInstance(e);
            }
        }

        static void OnResumeAsyncCodeActivityBookmark(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ActivityExecutor executor = (ActivityExecutor)result.AsyncState;

            try
            {
                executor.EndResumeBookmark(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                executor.AbortWorkflowInstance(e);
            }
        }

        internal abstract class CompleteData
        {
            AsyncOperationContext context;
            bool isCancel;

            protected CompleteData(AsyncOperationContext context, bool isCancel)
            {
                Fx.Assert(context != null, "Cannot have a null context.");
                this.context = context;
                this.isCancel = isCancel;
            }

            protected ActivityExecutor Executor
            {
                get
                {
                    return this.context.executor;
                }
            }

            public ActivityInstance Instance
            {
                get
                {
                    return this.context.owningActivityInstance;
                }
            }

            protected AsyncOperationContext AsyncContext
            {
                get
                {
                    return this.context;
                }
            }

            // This method will throw if the complete/cancel is now invalid, it will return
            // true if the complete/cancel should proceed, and return false if the complete/cancel
            // should be ignored.
            bool ShouldCallExecutor()
            {
                if (this.isCancel)
                {
                    return this.context.ShouldCancel();
                }
                else
                {
                    return this.context.ShouldComplete();
                }
            }

            // This must be called from a workflow thread
            public void CompleteOperation()
            {
                if (ShouldCallExecutor())
                {
                    OnCallExecutor();

                    // We only update hasCompleted if we just did the completion work.
                    // Calling Cancel followed by Complete does not mean you've completed.
                    if (!this.isCancel)
                    {
                        this.context.hasCompleted = true;
                    }
                }

                // We update hasCanceled even if we skipped the actual work.
                // Calling Complete followed by Cancel does imply that you have canceled.
                if (this.isCancel)
                {
                    this.context.hasCanceled = true;
                }
            }

            protected abstract void OnCallExecutor();
        }
    }
}
