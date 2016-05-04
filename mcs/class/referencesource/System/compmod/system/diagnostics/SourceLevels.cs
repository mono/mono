//------------------------------------------------------------------------------
// <copyright file="SourceLevels.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace System.Diagnostics {

    [Flags]
    public enum SourceLevels {
        Off         = 0,
        Critical    = 0x01,
        Error       = 0x03,
        Warning     = 0x07,
        Information = 0x0F,
        Verbose     = 0x1F,

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ActivityTracing = 0xFF00,
        All             = unchecked ((int) 0xFFFFFFFF),
    }
}

