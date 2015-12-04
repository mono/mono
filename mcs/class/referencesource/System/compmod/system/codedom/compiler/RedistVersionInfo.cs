//------------------------------------------------------------------------------
// <copyright file="RedistVersionInfo.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Collections.Generic;

    using Microsoft.Win32;

    // The default compiler is the one corresponding to the current-running version of the runtime.
    // Customers can choose to use a different one by setting a provider option.
    internal static class RedistVersionInfo {

        // Version identifier added for Dev10.  Takes the full path, doesn't depend on registry key
        internal const String DirectoryPath = "CompilerDirectoryPath";  // location

        // Version identifier added for Orcas.  Depends on registry key.
        internal const String NameTag = "CompilerVersion";    // name of the tag for specifying the version

        internal const String DefaultVersion = InPlaceVersion;      // should match one of the versions below
        internal const String InPlaceVersion = "v4.0";        // Default
        internal const String RedistVersion  = "v3.5";        // May change with servicing 
        internal const String RedistVersion20= "v2.0";

        private const string MSBuildToolsPath = "MSBuildToolsPath";
        private const string dotNetFrameworkRegistryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions\\";

        public static string GetCompilerPath(IDictionary<string, string> provOptions, string compilerExecutable) {

            // Get the location of the runtime, the usual answer
            string compPath = Executor.GetRuntimeInstallDirectory();

            // if provOptions is provided check to see if it alters what version we should bind to.
            // provOptions can be null if someone does new VB/CSCodeProvider(), in which case
            // they get the default behavior.
            if (provOptions != null) {

                string directoryPath;
                bool directoryPathPresent = provOptions.TryGetValue(DirectoryPath, out directoryPath);
                string versionVal;
                bool versionValPresent = provOptions.TryGetValue(NameTag, out versionVal);
                
                if(directoryPathPresent && versionValPresent)
                {
                    throw new InvalidOperationException(SR.GetString(SR.Cannot_Specify_Both_Compiler_Path_And_Version, DirectoryPath, NameTag));
                }
                
                // If they have an explicit path, use it.  Otherwise, look it up from the registry.
                if (directoryPathPresent) {
                    return directoryPath;
                }
                
                // If they have specified a version number in providerOptions, use it.
                if (versionValPresent) {
                    switch (versionVal) {

                        case RedistVersionInfo.InPlaceVersion:
                            // Use the RuntimeInstallDirectory, already obtained
                            break;

                        case RedistVersionInfo.RedistVersion:
                            // lock to the Orcas version, if it's not available throw (we'll throw at compile time)
                            compPath = GetCompilerPathFromRegistry(versionVal);
                            break;

                        case RedistVersionInfo.RedistVersion20:
                            //look up 2.0 compiler path from registry
                            compPath = GetCompilerPathFromRegistry(versionVal);
                            break;

                        default:
                            compPath = null;
                            break;
                    }
                }
            }

            if (compPath == null)
                throw new InvalidOperationException(SR.GetString(SR.CompilerNotFound, compilerExecutable));

            return compPath;
        }

        /// this method returns the location of the Orcas compilers, but will return whatever 
        /// version is requested via the COMPlus_ environment variables first
        private static string GetCompilerPathFromRegistry(String versionVal) {
            string dir = null;

            // if this is running in a private running environment such as Razzle, we would use the path
            // based on the environment variables: COMPLUS_InstallRoot and COMPLUS_Version.
            string comPlus_InstallRoot = Environment.GetEnvironmentVariable("COMPLUS_InstallRoot");
            string comPlus_Version = Environment.GetEnvironmentVariable("COMPLUS_Version");

            if (!string.IsNullOrEmpty(comPlus_InstallRoot) && !string.IsNullOrEmpty(comPlus_Version))
            {
                dir = Path.Combine(comPlus_InstallRoot, comPlus_Version);
                if (Directory.Exists(dir))
                    return dir;
            }

            String versionWithoutV = versionVal.Substring(1);
            String registryPath = dotNetFrameworkRegistryPath + versionWithoutV; 
            dir = Registry.GetValue(registryPath, MSBuildToolsPath, null) as string;

            if (dir != null && Directory.Exists(dir)) {
                return dir;
            }
            return null;
        }
    }
}

