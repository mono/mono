//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;

    public interface IReceiveMessageCallback
    {
        void OnReceiveMessage(OperationContext operationContext, ExecutionProperties activityExecutionProperties);
    }
}
