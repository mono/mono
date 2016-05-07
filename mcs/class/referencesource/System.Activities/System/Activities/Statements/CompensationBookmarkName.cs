//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;

    enum CompensationBookmarkName
    {
        Confirmed = 0,
        Canceled = 1,
        Compensated = 2,
        OnConfirmation = 3,
        OnCompensation = 4,
        OnCancellation = 5,
        OnSecondaryRootScheduled = 6,
    }
}
