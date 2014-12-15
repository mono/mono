using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Permissions;
using Microsoft.Win32;
using System.IO;
using System.Security;

namespace System.Net
{
    internal static class HttpSysSettings
    {
        private const string httpSysParametersKey = @"System\CurrentControlSet\Services\HTTP\Parameters";
        private const bool enableNonUtf8Default = true;
        private const bool favorUtf8Default = true;
        private const string enableNonUtf8Name = "EnableNonUtf8";
        private const string favorUtf8Name = "FavorUtf8";

        private static volatile bool enableNonUtf8;
        private static volatile bool favorUtf8;

        static HttpSysSettings()
        {
            enableNonUtf8 = enableNonUtf8Default;
            favorUtf8 = favorUtf8Default;
            ReadHttpSysRegistrySettings();
        }

        public static bool EnableNonUtf8
        {
            get { return enableNonUtf8; }
        }

        public static bool FavorUtf8
        {
            get { return favorUtf8; }
        }

        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + httpSysParametersKey)]
        private static void ReadHttpSysRegistrySettings()
        {
            try
            {
                RegistryKey httpSysParameters = Registry.LocalMachine.OpenSubKey(httpSysParametersKey);

                if (httpSysParameters == null)
                {
                    LogWarning("ReadHttpSysRegistrySettings", SR.net_log_listener_httpsys_registry_null,
                        httpSysParametersKey);
                }
                else
                {
                    using (httpSysParameters)
                    {
                        enableNonUtf8 = ReadRegistryValue(httpSysParameters, enableNonUtf8Name, enableNonUtf8Default);
                        favorUtf8 = ReadRegistryValue(httpSysParameters, favorUtf8Name, favorUtf8Default);
                    }
                }
            }
            catch (SecurityException e)
            {
                LogRegistryException("ReadHttpSysRegistrySettings", e);
            }
            catch (ObjectDisposedException e)
            {
                LogRegistryException("ReadHttpSysRegistrySettings", e);
            }
        }

        private static bool ReadRegistryValue(RegistryKey key, string valueName, bool defaultValue)
        {
            Debug.Assert(key != null, "'key' must not be null");

            try
            {
                // This check will throw an IOException if keyName doesn't exist. That's OK, we return the
                // default value.
                if (key.GetValueKind(valueName) == RegistryValueKind.DWord)
                {
                    // At this point we know the Registry value exists and it must be valid (any DWORD value
                    // can be converted to a bool).
                    return Convert.ToBoolean(key.GetValue(valueName), CultureInfo.InvariantCulture);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                LogRegistryException("ReadRegistryValue", e);
            }
            catch (IOException e)
            {
                LogRegistryException("ReadRegistryValue", e);
            }
            catch (SecurityException e)
            {
                LogRegistryException("ReadRegistryValue", e);
            }
            catch (ObjectDisposedException e)
            {
                LogRegistryException("ReadRegistryValue", e);
            }

            return defaultValue;
        }

        private static void LogRegistryException(string methodName, Exception e)
        {
            LogWarning(methodName, SR.net_log_listener_httpsys_registry_error, httpSysParametersKey, e);
        }

        private static void LogWarning(string methodName, string message, params object[] args)
        {
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.HttpListener, typeof(HttpSysSettings), methodName,
                    SR.GetString(message, args));
            }
        }
    }
}
