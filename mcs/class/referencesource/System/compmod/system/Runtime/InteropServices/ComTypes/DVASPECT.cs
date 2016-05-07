//------------------------------------------------------------------------------
// <copyright file="DVASPECT.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;

    /// <devdoc>
    /// </devdoc>
    [Flags]
    public enum DVASPECT {
        DVASPECT_CONTENT    = 1,
        DVASPECT_THUMBNAIL  = 2,
        DVASPECT_ICON       = 4,
        DVASPECT_DOCPRINT   = 8
    }
}


