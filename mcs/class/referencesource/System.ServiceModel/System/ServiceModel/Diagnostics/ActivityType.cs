//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    enum ActivityType
    {
        Unknown,
        Close,
        Construct,
        ExecuteUserCode,
        ListenAt,
        Open,
        OpenClient,
        ProcessMessage,
        ProcessAction,
        ReceiveBytes,
        SecuritySetup,
        TransferToComPlus,
        WmiGetObject,
        WmiPutInstance,
        NumItems, // leave this item at the end of the list.
    }
}
