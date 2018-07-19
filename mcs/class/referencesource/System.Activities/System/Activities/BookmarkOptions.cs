//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    [Flags]
    public enum BookmarkOptions
    {
        None = 0x00,
        MultipleResume = 0x01,
        NonBlocking = 0x02,
    }
}
