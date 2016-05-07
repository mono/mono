//------------------------------------------------------------------------------
// <copyright file="FORMATETC.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;
    using System.Runtime.InteropServices;

    /// <devdoc>
    /// </devdoc>
    public struct FORMATETC {
        [MarshalAs(UnmanagedType.U2)]
        public short    cfFormat;
        public IntPtr   ptd;
        [MarshalAs(UnmanagedType.U4)]
        public DVASPECT dwAspect;
        public int      lindex;
        [MarshalAs(UnmanagedType.U4)]
        public TYMED    tymed;
    }
}


