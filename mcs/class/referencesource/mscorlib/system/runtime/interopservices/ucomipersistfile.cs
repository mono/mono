// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: UCOMIPersistFile
**
**
** Purpose: UCOMIPersistFile interface definition.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {

    using System;
    using DWORD = System.UInt32;

    [Obsolete("Use System.Runtime.InteropServices.ComTypes.IPersistFile instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface UCOMIPersistFile
    {
        // IPersist portion
        void GetClassID(out Guid pClassID);

        // IPersistFile portion
        [PreserveSig]
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] String pszFileName, int dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] String pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] String pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out String ppszFileName);
    }
}
