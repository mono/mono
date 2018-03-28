//------------------------------------------------------------------------------
// <OWNER>petes</OWNER>
// 
// <copyright file="FieldDirection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [
        ComVisible(true),
        Serializable,
    ]
    public enum CodeRegionMode {
        None = 0,
        Start = 1,
        End = 2,
    }
}
