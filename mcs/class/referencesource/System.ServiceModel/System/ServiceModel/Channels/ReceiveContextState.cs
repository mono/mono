//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public enum ReceiveContextState
    {
        Received,
        Completing,
        Completed,
        Abandoning,
        Abandoned,
        Faulted
    }
}
