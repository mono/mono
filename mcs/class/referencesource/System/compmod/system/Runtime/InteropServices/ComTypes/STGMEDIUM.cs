//------------------------------------------------------------------------------
// <copyright file="STGMEDIUM.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;

    /// <devdoc>
    /// </devdoc>
    public struct STGMEDIUM {
        public TYMED  tymed;
        public IntPtr unionmember;
        [MarshalAs(UnmanagedType.IUnknown)] 
        public object pUnkForRelease;
    }
}


