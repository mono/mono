//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.DynamicUpdate;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;

    public abstract class AsyncCodeActivity : Activity, IAsyncCodeActivity
    {
        static AsyncCallback onExecuteComplete;

        protected AsyncCodeActivity()
        {
        }

        protected internal sealed override Version ImplementationVersion
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException());
                }
            }
        }

        [IgnoreDataMember]
        [Fx.Tag.KnownXamlExternal]
        protected sealed override Func<Activity> Implementation
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException());
                }
            }
        }

        internal static AsyncCallback OnExecuteComplete
        {
            get
            {
                if (onExecuteComplete == null)
                {
                    onExecuteComplete = Fx.ThunkCallback(new AsyncCallback(CompleteAsynchronousExecution));
                }

                return onExecuteComplete;
            }
        }

        internal override bool InternalCanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected abstract IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state);
        protected abstract void EndExecute(AsyncCodeActivityContext context, IAsyncResult result);

        // called on the Cancel and Abort paths to allow cleanup of outstanding async work
        protected virtual void Cancel(AsyncCodeActivityContext context)
        {
        }

        sealed internal override void InternalExecute(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            // first set up an async context
            AsyncOperationContext asyncContext = executor.SetupAsyncOperationBlock(instance);
            instance.IncrementBusyCount();

            AsyncCodeActivityContext context = new AsyncCodeActivityContext(asyncContext, instance, executor);
            bool success = false;
            try
            {
                IAsyncResult result = BeginExecute(context, AsyncCodeActivity.OnExecuteComplete, asyncContext);

                if (result == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BeginExecuteMustNotReturnANullAsyncResult));
                }

                if (!object.ReferenceEquals(result.AsyncState, asyncContext))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BeginExecuteMustUseProvidedStateAsAsyncResultState));
                }

                if (result.CompletedSynchronously)
                {
                    EndExecute(context, result);
                    asyncContext.CompleteOperation();
                }
                success = true;
            }
            finally
            {
                context.Dispose();
                if (!success)
                {
                    asyncContext.CancelOperation();
                }
            }
        }

        void IAsyncCodeActivity.FinishExecution(AsyncCodeActivityContext context, IAsyncResult result)
        {
            this.EndExecute(context, result);
        }

        internal static void CompleteAsynchronousExecution(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            AsyncOperationContext asyncContext = result.AsyncState as AsyncOperationContext;

            // User code may not have correctly passed the AsyncOperationContext thru as the "state" parameter for
            // BeginInvoke. If is null, don't bother going any further. We would have thrown an exception out of the
            // workflow from InternalExecute. In that case, AsyncOperationContext.CancelOperation will be called in
            // InternalExecute.
            if (asyncContext != null)
            {
                asyncContext.CompleteAsyncCodeActivity(new CompleteAsyncCodeActivityData(asyncContext, result));
            }
        }

        sealed internal override void InternalCancel(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            AsyncOperationContext asyncContext;
            if (executor.TryGetPendingOperation(instance, out asyncContext))
            {
                AsyncCodeActivityContext context = new AsyncCodeActivityContext(asyncContext, instance, executor);
                try
                {
                    asyncContext.HasCalledAsyncCodeActivityCancel = true;
                    Cancel(context);
                }
                finally
                {
                    context.Dispose();
                }
            }
        }

        sealed internal override void InternalAbort(ActivityInstance instance, ActivityExecutor executor, Exception terminationReason)
        {
            AsyncOperationContext asyncContext;
            if (executor.TryGetPendingOperation(instance, out asyncContext))
            {
                try
                {
                    if (!asyncContext.HasCalledAsyncCodeActivityCancel)
                    {
                        asyncContext.IsAborting = true;
                        InternalCancel(instance, executor, null);
                    }
                }
                finally
                {
                    // we should always abort outstanding contexts
                    if (asyncContext.IsStillActive)
                    {
                        asyncContext.CancelOperation();
                    }
                }
            }
        }

        sealed internal override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            CodeActivityMetadata metadata = new CodeActivityMetadata(this, this.GetParentEnvironment(), createEmptyBindings);
            CacheMetadata(metadata);
            metadata.Dispose();
        }

        internal sealed override void OnInternalCreateDynamicUpdateMap(DynamicUpdateMapBuilder.Finalizer finalizer,
            DynamicUpdateMapBuilder.IDefinitionMatcher matcher, Activity originalActivity)
        {
        }

        protected sealed override void OnCreateDynamicUpdateMap(UpdateMapMetadata metadata, Activity originalActivity)
        {
            // NO OP
        }

        protected sealed override void CacheMetadata(ActivityMetadata metadata)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WrongCacheMetadataForCodeActivity));
        }

        protected virtual void CacheMetadata(CodeActivityMetadata metadata)
        {
            // We bypass the metadata call to avoid the null checks
            SetArgumentsCollection(ReflectedInformation.GetArguments(this), metadata.CreateEmptyBindings);
        }

        class CompleteAsyncCodeActivityData : AsyncOperationContext.CompleteData
        {
            IAsyncResult result;

            public CompleteAsyncCodeActivityData(AsyncOperationContext context, IAsyncResult result)
                : base(context, false)
            {
                this.result = result;
            }

            protected override void OnCallExecutor()
            {
                this.Executor.CompleteOperation(new CompleteAsyncCodeActivityWorkItem(this.AsyncContext, this.Instance, this.result));
            }

            // not [DataContract] since this workitem will never happen when persistable
            class CompleteAsyncCodeActivityWorkItem : ActivityExecutionWorkItem
            {
                IAsyncResult result;
                AsyncOperationContext asyncContext;

                public CompleteAsyncCodeActivityWorkItem(AsyncOperationContext asyncContext, ActivityInstance instance, IAsyncResult result)
                    : base(instance)
                {
                    this.result = result;
                    this.asyncContext = asyncContext;
                    this.ExitNoPersistRequired = true;
                }

                public override void TraceCompleted()
                {
                    if (TD.CompleteBookmarkWorkItemIsEnabled())
                    {
                        TD.CompleteBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }

                public override void TraceScheduled()
                {
                    if (TD.ScheduleBookmarkWorkItemIsEnabled())
                    {
                        TD.ScheduleBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }

                public override void TraceStarting()
                {
                    if (TD.StartBookmarkWorkItemIsEnabled())
                    {
                        TD.StartBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }

                public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
                {
                    AsyncCodeActivityContext context = null;

                    try
                    {
                        context = new AsyncCodeActivityContext(this.asyncContext, this.ActivityInstance, executor);
                        IAsyncCodeActivity owner = (IAsyncCodeActivity)this.ActivityInstance.Activity;
                        owner.FinishExecution(context, this.result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        this.ExceptionToPropagate = e;
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.Dispose();
                        }
                    }

                    return true;
                }
            }
        }
    }

    public abstract class AsyncCodeActivity<TResult> : Activity<TResult>, IAsyncCodeActivity
    {
        protected AsyncCodeActivity()
        {
        }

        protected internal sealed override Version ImplementationVersion
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException());
                }
            }
        }

        [IgnoreDataMember]
        [Fx.Tag.KnownXamlExternal]
        protected sealed override Func<Activity> Implementation
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException());
                }
            }
        }

        internal override bool InternalCanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected abstract IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state);
        protected abstract TResult EndExecute(AsyncCodeActivityContext context, IAsyncResult result);

        // called on the Cancel and Abort paths to allow cleanup of outstanding async work
        protected virtual void Cancel(AsyncCodeActivityContext context)
        {
        }

        sealed internal override void InternalExecute(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            // first set up an async context
            AsyncOperationContext asyncContext = executor.SetupAsyncOperationBlock(instance);
            instance.IncrementBusyCount();

            AsyncCodeActivityContext context = new AsyncCodeActivityContext(asyncContext, instance, executor);
            bool success = false;
            try
            {
                IAsyncResult result = BeginExecute(context, AsyncCodeActivity.OnExecuteComplete, asyncContext);

                if (result == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BeginExecuteMustNotReturnANullAsyncResult));
                }

                if (!object.ReferenceEquals(result.AsyncState, asyncContext))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BeginExecuteMustUseProvidedStateAsAsyncResultState));
                }

                if (result.CompletedSynchronously)
                {
                    ((IAsyncCodeActivity)this).FinishExecution(context, result);
                    asyncContext.CompleteOperation();
                }
                success = true;
            }
            finally
            {
                context.Dispose();
                if (!success)
                {
                    asyncContext.CancelOperation();
                }
            }
        }

        void IAsyncCodeActivity.FinishExecution(AsyncCodeActivityContext context, IAsyncResult result)
        {
            TResult executionResult = this.EndExecute(context, result);
            this.Result.Set(context, executionResult);
        }

        sealed internal override void InternalCancel(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            AsyncOperationContext asyncContext;
            if (executor.TryGetPendingOperation(instance, out asyncContext))
            {
                AsyncCodeActivityContext context = new AsyncCodeActivityContext(asyncContext, instance, executor);
                try
                {
                    asyncContext.HasCalledAsyncCodeActivityCancel = true;
                    Cancel(context);
                }
                finally
                {
                    context.Dispose();
                }
            }
        }

        sealed internal override void InternalAbort(ActivityInstance instance, ActivityExecutor executor, Exception terminationReason)
        {
            AsyncOperationContext asyncContext;
            if (executor.TryGetPendingOperation(instance, out asyncContext))
            {
                try
                {
                    if (!asyncContext.HasCalledAsyncCodeActivityCancel)
                    {
                        asyncContext.IsAborting = true;
                        InternalCancel(instance, executor, null);
                    }
                }
                finally
                {
                    // we should always abort outstanding contexts
                    if (asyncContext.IsStillActive)
                    {
                        asyncContext.CancelOperation();
                    }
                }
            }
        }

        sealed internal override void OnInternalCacheMetadataExceptResult(bool createEmptyBindings)
        {
            CodeActivityMetadata metadata = new CodeActivityMetadata(this, this.GetParentEnvironment(), createEmptyBindings);
            CacheMetadata(metadata);
            metadata.Dispose();
        }

        internal sealed override void OnInternalCreateDynamicUpdateMap(DynamicUpdateMapBuilder.Finalizer finalizer, 
            DynamicUpdateMapBuilder.IDefinitionMatcher matcher, Activity originalActivity)
        {
        }

        protected sealed override void OnCreateDynamicUpdateMap(UpdateMapMetadata metadata, Activity originalActivity)
        {
            // NO OP
        }

        protected sealed override void CacheMetadata(ActivityMetadata metadata)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WrongCacheMetadataForCodeActivity));
        }

        protected virtual void CacheMetadata(CodeActivityMetadata metadata)
        {
            // We bypass the metadata call to avoid the null checks
            SetArgumentsCollection(ReflectedInformation.GetArguments(this), metadata.CreateEmptyBindings);
        }
    }
}
