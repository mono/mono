// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    PlatformID
**
**
** Purpose: Defines IDs for supported platforms
**
**
===========================================================*/
namespace System {

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum PlatformID
    {
        Win32S        = 0,
        Win32Windows  = 1,
        Win32NT       = 2,
        WinCE         = 3,      
        Unix          = 4,
        Xbox          = 5,
#if !FEATURE_LEGACYNETCF
        MacOSX        = 6
#else // FEATURE_LEGACYNETCF
        NokiaS60      = 6
#endif // FEATURE_LEGACYNETCF
    }

}
