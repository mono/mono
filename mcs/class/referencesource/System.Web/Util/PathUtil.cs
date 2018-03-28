//------------------------------------------------------------------------------
// <copyright file="PathUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.IO;
    using System.Security.Permissions;

    internal static class PathUtil {

        private static string _system32Path = GetSystem32Path();

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)] // we don't leak this path anywhere
        private static string GetSystem32Path() {
            return Environment.GetFolderPath(Environment.SpecialFolder.System);
        }

        // Gets the full path to a file in the SYSTEM32 folder (which is correct for both 32-bit and 64-bit architectures).
        // Example: "foo.dll" -> "C:\Windows\System32\foo.dll"
        internal static string GetSystemDllFullPath(string filename) {
            return Path.Combine(_system32Path, filename);
        }

    }
}
