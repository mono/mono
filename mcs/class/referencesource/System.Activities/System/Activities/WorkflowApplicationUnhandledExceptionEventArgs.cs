//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowApplicationUnhandledExceptionEventArgs : WorkflowApplicationEventArgs
    {
        internal WorkflowApplicationUnhandledExceptionEventArgs(WorkflowApplication application, Exception exception, Activity exceptionSource, string exceptionSourceInstanceId)
            : base(application)
        {
            this.UnhandledException = exception;
            this.ExceptionSource = exceptionSource;
            this.ExceptionSourceInstanceId = exceptionSourceInstanceId;
        }

        public Exception UnhandledException
        {
            get;
            private set;
        }

        public Activity ExceptionSource
        {
            get;
            private set;
        }

        public string ExceptionSourceInstanceId
        {
            get;
            private set;
        }
    }
}


