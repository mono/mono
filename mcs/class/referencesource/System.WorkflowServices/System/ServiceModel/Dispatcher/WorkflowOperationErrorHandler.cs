//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;

    class WorkflowOperationErrorHandler : DurableErrorHandler
    {
        public WorkflowOperationErrorHandler(bool includeDebugInfo)
            : base(includeDebugInfo)
        {
        }

        public static Exception CreateUnhandledException(Exception innerException)
        {
            if (innerException == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerException");
            }
            return new WorkflowOperationUnhandledException(innerException);
        }

        protected override Exception GetExceptionToTrace(Exception error)
        {
            return error.InnerException;
        }

        protected override bool IsUserCodeException(Exception error)
        {
            return error is WorkflowOperationUnhandledException;
        }

        class WorkflowOperationUnhandledException : Exception
        {
            public WorkflowOperationUnhandledException(Exception innerException)
                : base(SR2.GetString(SR2.WorkflowOperationUnhandledException), innerException)
            {

            }
        }
    }
}
