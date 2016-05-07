//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowApplicationAbortedEventArgs : WorkflowApplicationEventArgs
    {
        internal WorkflowApplicationAbortedEventArgs(WorkflowApplication application, Exception reason)
            : base(application)
        {
            this.Reason = reason;
        }

        public Exception Reason
        {
            get;
            private set;
        }
    }
}
