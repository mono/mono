//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class NativeActivityAbortContext : ActivityContext
    {
        Exception reason;

        internal NativeActivityAbortContext(ActivityInstance instance, ActivityExecutor executor, Exception reason)
            : base(instance, executor)
        {
            this.reason = reason;
        }

        public Exception Reason
        {
            get
            {
                ThrowIfDisposed();

                return this.reason;
            }
        }
    }
}
