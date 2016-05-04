//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class NativeActivityFaultContext : NativeActivityContext
    {
        bool isFaultHandled;
        Exception exception;
        ActivityInstanceReference source;

        internal NativeActivityFaultContext(ActivityInstance executingActivityInstance,
            ActivityExecutor executor, BookmarkManager bookmarkManager, Exception exception, ActivityInstanceReference source)
            : base(executingActivityInstance, executor, bookmarkManager)
        {
            Fx.Assert(exception != null, "There must be an exception.");
            Fx.Assert(source != null, "There must be a source.");

            this.exception = exception;
            this.source = source;
        }

        internal bool IsFaultHandled
        {
            get
            {
                return this.isFaultHandled;
            }
        }

        public void HandleFault()
        {
            ThrowIfDisposed();

            this.isFaultHandled = true;
        }

        internal FaultContext CreateFaultContext()
        {
            Fx.Assert(!this.IsDisposed, "We must not have been disposed.");

            return new FaultContext(this.exception, this.source);
        }
    }
}
