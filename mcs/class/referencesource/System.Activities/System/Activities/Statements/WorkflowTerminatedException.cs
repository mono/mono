//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Runtime.Serialization;
    
    [Serializable]
    public class WorkflowTerminatedException : Exception
    {
        public WorkflowTerminatedException()
            : base(SR.WorkflowTerminatedExceptionDefaultMessage)
        {
        }

        public WorkflowTerminatedException(string message)
            : base(message)
        {
        }

        public WorkflowTerminatedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WorkflowTerminatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
