// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.InteropServices {

    using System;
    // Used for the CallingConvention named argument to the DllImport attribute
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum CallingConvention
    {
        Winapi          = 1,
        Cdecl           = 2,
        StdCall         = 3,
        ThisCall        = 4,
        FastCall        = 5,
    }
    
}
