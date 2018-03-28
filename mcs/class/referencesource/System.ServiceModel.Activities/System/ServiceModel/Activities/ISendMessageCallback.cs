//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    public interface ISendMessageCallback
    {
        void OnSendMessage(OperationContext operationContext);
    }
}
