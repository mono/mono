//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationUnloadedException : WorkflowApplicationException
    {
        public WorkflowApplicationUnloadedException()
        {
        }

        public WorkflowApplicationUnloadedException(string message)
            : base(message)
        {
        }

        public WorkflowApplicationUnloadedException(string message, Guid instanceId)
            : base(message, instanceId)
        {
        }

        public WorkflowApplicationUnloadedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowApplicationUnloadedException(string message, Guid instanceId, Exception innerException)
            : base(message, instanceId, innerException)
        {
        }

        protected WorkflowApplicationUnloadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
