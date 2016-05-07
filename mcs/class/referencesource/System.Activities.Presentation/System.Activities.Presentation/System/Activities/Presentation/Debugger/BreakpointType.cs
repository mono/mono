//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Activities.Presentation.Debug
{
    using System;
    [Flags]
    public enum BreakpointTypes
    {
        None    = 0x00,
        Enabled = 0x01,
        Bounded = 0x02,
        Conditional = 0x04
    }
}
