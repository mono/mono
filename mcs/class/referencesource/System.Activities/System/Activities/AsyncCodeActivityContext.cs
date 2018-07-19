//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Runtime;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class AsyncCodeActivityContext : CodeActivityContext
    {
        AsyncOperationContext asyncContext;

        internal AsyncCodeActivityContext(AsyncOperationContext asyncContext, ActivityInstance instance, ActivityExecutor executor)
            : base(instance, executor)
        {
            this.asyncContext = asyncContext;
        }

        public bool IsCancellationRequested
        {
            get
            {
                ThrowIfDisposed();
                return this.CurrentInstance.IsCancellationRequested;
            }
        }

        public object UserState
        {
            get
            {
                ThrowIfDisposed();
                return this.asyncContext.UserState;
            }
            set
            {
                ThrowIfDisposed();
                this.asyncContext.UserState = value;
            }
        }

        public void MarkCanceled()
        {
            ThrowIfDisposed();

            // This is valid to be called while aborting or while canceling
            if (!this.CurrentInstance.IsCancellationRequested && !this.asyncContext.IsAborting)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MarkCanceledOnlyCallableIfCancelRequested));
            }

            this.CurrentInstance.MarkCanceled();
        }
    }
}
