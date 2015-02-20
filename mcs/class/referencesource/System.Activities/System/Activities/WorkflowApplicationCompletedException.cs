//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationCompletedException : WorkflowApplicationException
    {
        public WorkflowApplicationCompletedException()
        {
        }

        public WorkflowApplicationCompletedException(string message)
            : base(message)
        {
        }

        public WorkflowApplicationCompletedException(string message, Guid instanceId)
            : base(message, instanceId)
        {
        }

        public WorkflowApplicationCompletedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowApplicationCompletedException(string message, Guid instanceId, Exception innerException)
            : base(message, instanceId, innerException)
        {
        }

        protected WorkflowApplicationCompletedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
