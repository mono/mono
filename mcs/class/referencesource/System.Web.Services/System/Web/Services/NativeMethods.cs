#if false
//------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {
    using System.Runtime.InteropServices;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;

    [System.Runtime.InteropServices.ComVisible(false)]
    internal class NativeMethods {

        internal const string Kernel32Name = "kernel32.dll";

        [DllImport(Kernel32Name, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern int GetModuleFileName(int hModule, StringBuilder buffer, int length);
        [DllImport(Kernel32Name, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern int GetModuleHandle(String modName);
    }
}
#endif
