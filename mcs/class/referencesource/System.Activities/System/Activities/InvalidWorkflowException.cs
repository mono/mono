//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Security;

    [Serializable]
    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException()
            : base(SR.DefaultInvalidWorkflowExceptionMessage)
        {
        }

        public InvalidWorkflowException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidWorkflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
