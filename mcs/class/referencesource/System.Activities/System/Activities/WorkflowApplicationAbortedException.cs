//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationAbortedException : WorkflowApplicationException
    {
        public WorkflowApplicationAbortedException()
        {
        }

        public WorkflowApplicationAbortedException(string message)
            : base(message)
        {
        }

        public WorkflowApplicationAbortedException(string message, Guid instanceId)
            : base(message, instanceId)
        {
        }

        public WorkflowApplicationAbortedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowApplicationAbortedException(string message, Guid instanceId, Exception innerException)
            : base(message, instanceId, innerException)
        {
        }

        protected WorkflowApplicationAbortedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
