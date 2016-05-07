//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;

namespace System.ServiceModel.Transactions
{
    [Flags]
    enum IsolationFlags
    {
        RetainCommitDC      = 0x00000001,
        RetainCommit        = 0x00000002,
        RetainCommitNo      = 0x00000003,
        RetainAbortDC       = 0x00000004,
        RetainAbort         = 0x00000008,
        RetainAbortNo       = 0x0000000C,
        RetainDoNotCare     = IsolationFlags.RetainCommitDC | IsolationFlags.RetainAbortDC,
        RetainBoth          = IsolationFlags.RetainCommit   | IsolationFlags.RetainAbort,
        RetainNone          = IsolationFlags.RetainCommitNo | IsolationFlags.RetainAbortNo,
        Optimistic          = 0x00000010,
        ReadOnly            = 0x00000020
    }
}