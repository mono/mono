// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
//
// File: RegistryOptions.cs
//
// <OWNER>[....]</OWNER>
//
// Implements Microsoft.Win32.RegistryOptions
//
// ======================================================================================
#if !FEATURE_PAL
namespace Microsoft.Win32 {
    using System;
    
    [Flags]
    public enum RegistryOptions {
        None                = Win32Native.REG_OPTION_NON_VOLATILE,  // 0x0000
        Volatile            = Win32Native.REG_OPTION_VOLATILE,      // 0x0001
///
/// Consider exposing more options in a future release.  Users can access this
/// functionality by calling [RegistryKey].Handle and pinvoking
///
///     CreateLink          = Win32Native.REG_OPTION_CREATE_LINK,   // 0x0002
///     BackupRestore       = Win32Native.REG_OPTION_BACKUP_RESTORE,// 0x0004
    };
}
#endif // !FEATURE_PAL
