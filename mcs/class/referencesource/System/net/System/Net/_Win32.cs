//------------------------------------------------------------------------------
// <copyright file="_Win32.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    internal static class Win32 {
        internal const  int OverlappedInternalOffset     = 0;
        internal static int OverlappedInternalHighOffset = IntPtr.Size;
        internal static int OverlappedOffsetOffset       = IntPtr.Size*2;
        internal static int OverlappedOffsetHighOffset   = IntPtr.Size*2 + 4;
        internal static int OverlappedhEventOffset       = IntPtr.Size*2 + 8;
        internal static int OverlappedSize               = IntPtr.Size*3 + 8;
    }
}
