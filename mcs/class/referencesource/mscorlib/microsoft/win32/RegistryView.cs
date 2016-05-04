// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
//
// File: RegistryOptions.cs
//
// <OWNER>JFREE</OWNER>
//
// Implements Microsoft.Win32.RegistryView
//
// ======================================================================================
#if !FEATURE_PAL
namespace Microsoft.Win32 {
    using System;
    
    public enum RegistryView {
        Default      = 0,                           // 0x0000 operate on the default registry view
        Registry64   = Win32Native.KEY_WOW64_64KEY, // 0x0100 operate on the 64-bit registry view
        Registry32   = Win32Native.KEY_WOW64_32KEY, // 0x0200 operate on the 32-bit registry view
    };
}
#endif // !FEATURE_PAL