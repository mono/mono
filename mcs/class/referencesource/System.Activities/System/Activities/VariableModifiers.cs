//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    [Flags]
    public enum VariableModifiers
    {
        None = 0X00,
        ReadOnly = 0X01,
        Mapped = 0X02        
    }
}
