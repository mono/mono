//------------------------------------------------------------------------------
// <copyright file="ConfigPathUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

#if CONFIGPATHUTILITY_SYSTEMWEB
    using Debug=System.Web.Util.Debug;
#endif

    internal static class ConfigPathUtility {
        private const char SeparatorChar = '/';

        //
        // A configPath is valid if
        //  * It does not start or end with a '/'
        //  * It is not null or empty, except in the case of the root configuration record
        //  * It does not contain '\'
        //  * It does not contain a path component equal to "." or ".."
        //
        // The checks for '\', ".", and ".." are not strictly necessary, but their presence
        // could lead to problems for configuration hosts.
        // 
        static internal bool IsValid(string configPath) {
            if (String.IsNullOrEmpty(configPath)) {
                return false;
            }

            int start = -1;
            for (int examine = 0; examine <= configPath.Length; examine++) {
                char ch;

                if (examine < configPath.Length) {
                    ch = configPath[examine];
                }
                else {
                    ch = SeparatorChar;
                }

                // backslash disallowed
                if (ch == '\\') {
                    return false;
                }

                if (ch == SeparatorChar) {
                    // double slash disallowed
                    // note this check also purposefully catches starting and ending slash
                    if (examine == start + 1) {
                        return false;
                    }

                    // "." disallowed
                    if (examine == start + 2 && configPath[start + 1] == '.') {
                        return false;
                    }

                    // ".." disallowed
                    if (examine == start + 3 && configPath[start + 1] == '.' && configPath[start + 2] == '.') {
                        return false;
                    }

                    start = examine;
                }
            }

            return true;
        }

#if !CONFIGPATHUTILITY_SYSTEMWEB
        static internal string Combine(string parentConfigPath, string childConfigPath) {
            Debug.Assert(String.IsNullOrEmpty(parentConfigPath) || IsValid(parentConfigPath), "String.IsNullOrEmpty(parentConfigPath) || IsValid(parentConfigPath)");
            Debug.Assert(String.IsNullOrEmpty(childConfigPath) || IsValid(childConfigPath), "String.IsNullOrEmpty(childConfigPath) || IsValid(childConfigPath)");

            if (String.IsNullOrEmpty(parentConfigPath)) {
                return childConfigPath;
            }

            if (String.IsNullOrEmpty(childConfigPath)) {
                return parentConfigPath;
            }

            return parentConfigPath + "/" + childConfigPath;
        }

        static internal string[] GetParts(string configPath) {
            Debug.Assert(IsValid(configPath), "IsValid(configPath)");

            string[] parts = configPath.Split(SeparatorChar);
            return parts;
        }

        //
        // Return the last part of a config path, e.g.
        //   GetName("MACHINE/WEBROOT/Default Web Site/app") == "app"
        //
        static internal string GetName(string configPath) {
            Debug.Assert(String.IsNullOrEmpty(configPath) || IsValid(configPath), "String.IsNullOrEmpty(configPath) || IsValid(configPath)");

            if (String.IsNullOrEmpty(configPath)) {
                return configPath;
            }

            int index = configPath.LastIndexOf('/');
            if (index == -1) {
                return configPath;
            }

            Debug.Assert(index != configPath.Length - 1);
            return configPath.Substring(index + 1);
        }
#endif

// Avoid unused code warning in System.Configuration by including functions in assembly-specific #defines
#if CONFIGPATHUTILITY_SYSTEMWEB
        static internal string GetParent(string configPath) {
            Debug.Assert(String.IsNullOrEmpty(configPath) || IsValid(configPath), "String.IsNullOrEmpty(configPath) || IsValid(configPath)");

            if (String.IsNullOrEmpty(configPath)) {
                return null;
            }

            string parentConfigPath;
            int lastSlash = configPath.LastIndexOf(SeparatorChar);
            if (lastSlash == -1) {
                parentConfigPath = null;
            }
            else {
                parentConfigPath = configPath.Substring(0, lastSlash);
            }

            return parentConfigPath;
        }
#endif
    }
}
