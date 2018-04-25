//------------------------------------------------------------------------------
// <copyright file="TYMED.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;

    /// <devdoc>
    /// </devdoc>
    [Flags]
    public enum TYMED {
        TYMED_HGLOBAL	= 1,
        TYMED_FILE	    = 2,
        TYMED_ISTREAM	= 4,
        TYMED_ISTORAGE	= 8,
        TYMED_GDI	    = 16,
        TYMED_MFPICT	= 32,
        TYMED_ENHMF	    = 64,
        TYMED_NULL	    = 0
    }
}


