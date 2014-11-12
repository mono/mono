//------------------------------------------------------------------------------
// <copyright file="SyntaxCheck.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <internalonly/>
    /// <devdoc>
    ///     SyntaxCheck
    ///     Helper class to check for path and machine name syntax.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public static class SyntaxCheck {
        /// <devdoc>
        ///     Checks the syntax of the machine name (no "\" anywhere in it).
        /// </devdoc>
        /// <internalonly/>
        public static bool CheckMachineName(string value) {
            if (value == null) 
                return false;
            
            value = value.Trim();                
            if (value.Equals(String.Empty))
                return false;
                
            // Machine names shouldn't contain any "\"
            return (value.IndexOf('\\') == -1);
        }

        /// <devdoc>
        ///     Checks the syntax of the path (must start with "\\").
        /// </devdoc>
        /// <internalonly/>
        public static bool CheckPath(string value) {
            if (value == null) 
                return false;
            
            value = value.Trim();                
            if (value.Equals(String.Empty))
                return false;

            // Path names should start with "\\"
            return value.StartsWith("\\\\");
        }

        /// <devdoc>
        ///     Checks the syntax of the path (must start with "\" or drive letter "C:").
        ///     NOTE:  These denote a file or directory path!!
        ///     
        /// </devdoc>
        /// <internalonly/>
        public static bool CheckRootedPath(string value) {
            if (value == null) 
                return false;
            
            value = value.Trim();                
            if (value.Equals(String.Empty))
                return false;

            // Is it rooted?
            return Path.IsPathRooted(value);
        }
    }
}
