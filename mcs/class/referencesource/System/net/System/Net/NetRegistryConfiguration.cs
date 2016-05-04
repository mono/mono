//------------------------------------------------------------------------------
// <copyright file="NetRegistryConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using Microsoft.Win32;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// Reads configuration from registry.
    /// 
    /// Settings convention:
    ///     * Global (all apps): HKLM\SOFTWARE\Microsoft\.NETFramework\vdotNetVersion
    ///                          configName         REG_DWORD | REG_SZ     value
    ///                          
    ///     * App specific: HKLM\SOFTWARE\Microsoft\.NETFramework\vdotNetVersion\configName
    ///                          fullAppExePath     REG_DWORD | REG_SZ     value
    /// </summary>
    internal static class RegistryConfiguration
    {
        private const string netFrameworkPath = @"SOFTWARE\Microsoft\.NETFramework";
        private const string netFrameworkVersionedPath = netFrameworkPath + @"\v{0}";
        private const string netFrameworkFullPath = @"HKEY_LOCAL_MACHINE\" + netFrameworkPath;
               
        /// <summary>
        /// Reads global configuration (REG_DWORD) from registry.
        /// </summary>
        /// <param name="configVariable">Configuration variable</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>The value within registry if it exists and is accessible; else the defaultValue.</returns>
        [RegistryPermission(SecurityAction.Assert, Read = netFrameworkFullPath)]
        public static int GlobalConfigReadInt(string configVariable, int defaultValue)
        {
            object value = ReadConfig(GetNetFrameworkVersionedPath(), configVariable, RegistryValueKind.DWord);
            if (value != null)
            {
                return (int)value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Reads global configuration (REG_SZ) from registry.
        /// </summary>
        /// <param name="configVariable">Configuration variable</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>The value within registry if it exists and is accessible; else the defaultValue.</returns>
        [RegistryPermission(SecurityAction.Assert, Read = netFrameworkFullPath)]
        public static string GlobalConfigReadString(string configVariable, string defaultValue)
        {
            object value = ReadConfig(GetNetFrameworkVersionedPath(), configVariable, RegistryValueKind.String);
            if (value != null)
            {
                return (string)value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Reads app-specific configuration (REG_DWORD) from registry.
        /// </summary>
        /// <param name="configVariable">Configuration variable</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>
        /// The value within registry if it exists for this application and is accessible; else the defaultValue.
        /// </returns>
        [RegistryPermission(SecurityAction.Assert, Read = netFrameworkFullPath)]
        public static int AppConfigReadInt(string configVariable, int defaultValue)
        {
            object value = ReadConfig(GetAppConfigPath(configVariable), GetAppConfigValueName(), RegistryValueKind.DWord);
            if (value != null)
            {
                return (int)value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Reads app-specific configuration (REG_SZ) from registry.
        /// </summary>
        /// <param name="configVariable">Configuration variable</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>
        /// The value within registry if it exists for this application and is accessible; else the defaultValue.
        /// </returns>
        [RegistryPermission(SecurityAction.Assert, Read = netFrameworkFullPath)]
        public static string AppConfigReadString(string configVariable, string defaultValue)
        {
            object value = ReadConfig(GetAppConfigPath(configVariable), GetAppConfigValueName(), RegistryValueKind.String);
            if (value != null)
            {
                return (string)value;
            }

            return defaultValue;
        }

        private static object ReadConfig(string path, string valueName, RegistryValueKind kind)
        {
            object ret = null;

            Debug.Assert(!String.IsNullOrEmpty(path), "Registry path should not be null.");
            Debug.Assert(!String.IsNullOrEmpty(valueName), "valueName should not be null.");

            try
            {
                // We read reflected keys on WOW64.
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key == null)
                    {
                        return ret;
                    }

                    try
                    {
                        object value = key.GetValue(valueName, null);

                        if ((value != null) && (key.GetValueKind(valueName) == kind))
                        {
                            ret = value;
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (IOException) { }
                }
            }
            catch (SecurityException) { }
            catch (ObjectDisposedException) { }

            return ret;
        }
        
        private static string GetNetFrameworkVersionedPath()
        {
            string versionedKeyPath = String.Format(
                CultureInfo.InvariantCulture,
                netFrameworkVersionedPath,
                Environment.Version.ToString(3));

            Debug.Assert(!String.IsNullOrEmpty(versionedKeyPath), ".Net Version should not be null.");

            return versionedKeyPath;
        }

        private static string GetAppConfigPath(string valueName)
        {
            Debug.Assert(!String.IsNullOrEmpty(valueName), "valueName should not be null.");
            return String.Format(CultureInfo.InvariantCulture, @"{0}\{1}", GetNetFrameworkVersionedPath(), valueName);
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        private static string GetAppConfigValueName()
        {
            string appExePath = "Unknown";

            Process currentProcess = Process.GetCurrentProcess();
            Debug.Assert(currentProcess != null);

            try
            {
                ProcessModule module = currentProcess.MainModule;
                // File-name can be truncated.
                appExePath = module.FileName;
            }
            catch (NotSupportedException) { }
            catch (Win32Exception) { }
            catch (InvalidOperationException) { }

            Debug.Assert(appExePath != null);

            // Get the full path in cases where the process was started using 8.3 named folders.
            // GetCurrentProcess will return the long exe name regardless of how it was started.
            try
            {
                appExePath = Path.GetFullPath(appExePath);
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return appExePath;
        }
    }
}
