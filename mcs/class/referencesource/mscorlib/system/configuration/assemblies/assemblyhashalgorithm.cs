// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    AssemblyHashAlgorithm
**
**
** Purpose: 
**
**
===========================================================*/
using System.Runtime.InteropServices;

namespace System.Configuration.Assemblies {
    
    using System;
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum AssemblyHashAlgorithm
    {
        None = 0,
        MD5 = 0x8003,
        SHA1 = 0x8004,
        [ComVisible(false)]
        SHA256 = 0x800c,
        [ComVisible(false)]
        SHA384 = 0x800d,
        [ComVisible(false)]
        SHA512 = 0x800e,
    }
}
