//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    enum ReceiveContextAcknowledgementMode
    {
        AutoAcknowledgeOnReceive = 0,
        AutoAcknowledgeOnRPCComplete = 1,
        ManualAcknowledgement = 2
    }
}
