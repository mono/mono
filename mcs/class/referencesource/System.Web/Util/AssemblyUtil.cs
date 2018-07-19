//------------------------------------------------------------------------------
// <copyright file="AssemblyUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

namespace System.Web.Util {
    internal static class AssemblyUtil {
        private const string _emptyFileVersion = "0.0.0.0";

        public static string GetAssemblyFileVersion(Assembly assembly) {
            AssemblyFileVersionAttribute[] attributes =
                (AssemblyFileVersionAttribute[])assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

            string version;
            if (attributes.Length > 0) {
                version = attributes[0].Version;
                if (String.IsNullOrEmpty(version)) {
                    version = _emptyFileVersion;
                }
            }
            else {
                version = _emptyFileVersion;
            }
            return version;
        }
    }
}
