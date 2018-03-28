//------------------------------------------------------------------------------
// <copyright file="_ProxyRegBlob.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Net.Sockets;
    using System.Threading;
    using System.Runtime.InteropServices;
#if USE_WINIET_AUTODETECT_CACHE
#if !FEATURE_PAL
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
#endif // !FEATURE_PAL
#endif
    using Microsoft.Win32;
    using System.Runtime.Versioning;
    using System.Diagnostics;

    internal class RegBlobWebProxyDataBuilder : WebProxyDataBuilder
    {

#if !FEATURE_PAL
        //
        // Allows us to grob through the registry and read the
        //  IE binary format, note that this should be replaced,
        //  by code that calls Wininet directly, but it can be
        //  expensive to load wininet, in order to do this.
        //

        [Flags]
        private enum ProxyTypeFlags
        {
            PROXY_TYPE_DIRECT          = 0x00000001,   // direct to net
            PROXY_TYPE_PROXY           = 0x00000002,   // via named proxy
            PROXY_TYPE_AUTO_PROXY_URL  = 0x00000004,   // autoproxy URL
            PROXY_TYPE_AUTO_DETECT     = 0x00000008,   // use autoproxy detection
        }

        internal const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings";
        internal const string ProxyKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections";
        private const string DefaultConnectionSettings = "DefaultConnectionSettings";
        private const string ProxySettingsPerUser = "ProxySettingsPerUser";

#if USE_WINIET_AUTODETECT_CACHE
        // Get the number of MilliSeconds in 7 days and then multiply by 10 because
        // FILETIME stores data stores time in 100-nanosecond intervals.
        //
        internal static UInt64 s_lkgScriptValidTime = (UInt64)(new TimeSpan(7, 0, 0, 0).Ticks); // 7 days
#endif

        const int IE50StrucSize = 60;

        private byte[] m_RegistryBytes;
        private int m_ByteOffset;

        private string m_Connectoid;
        private SafeRegistryHandle m_Registry;

        public RegBlobWebProxyDataBuilder(string connectoid, SafeRegistryHandle registry)
        {
            Debug.Assert(registry != null);

            m_Registry = registry;
            m_Connectoid = connectoid;
        }

        // returns true - on successful read of proxy registry settings
        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\" + PolicyKey)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private bool ReadRegSettings()
        {
            SafeRegistryHandle key = null;
            RegistryKey lmKey = null;
            try {
                bool isPerUser = true;
                lmKey = Registry.LocalMachine.OpenSubKey(PolicyKey);
                if (lmKey != null)
                {
                    object perUser = lmKey.GetValue(ProxySettingsPerUser);
                    if (perUser != null && perUser.GetType() == typeof(int) && 0 == (int) perUser)
                    {
                        isPerUser = false;
                    }
                }

                uint errorCode;
                if (isPerUser)
                {
                    if (m_Registry != null)
                    {
                        errorCode = m_Registry.RegOpenKeyEx(ProxyKey, 0, UnsafeNclNativeMethods.RegistryHelper.KEY_READ, out key);
                    }
                    else
                    {
                        errorCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_NOT_FOUND;
                    }
                }
                else
                {
                    errorCode = SafeRegistryHandle.RegOpenKeyEx(UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, ProxyKey, 0, UnsafeNclNativeMethods.RegistryHelper.KEY_READ, out key);
                }
                if (errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                {
                    key = null;
                }
                if (key != null)
                {
                    // When reading settings from the registry, if connectoid key is missing, the connectoid
                    // was never configured. In this case we have no settings (this is equivalent to always go direct).
                    object data;
                    errorCode = key.QueryValue(m_Connectoid != null ? m_Connectoid : DefaultConnectionSettings, out data);
                    if (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                    {
                        m_RegistryBytes = (byte[]) data;
                    }
                }
            }
            catch (Exception exception) {
                if (NclUtilities.IsFatal(exception)) throw;
            }
            finally
            {
                if (lmKey != null)
                    lmKey.Close();

                if(key != null)
                    key.RegCloseKey();
            }
            return m_RegistryBytes != null;
        }

#if USE_WINIET_AUTODETECT_CACHE
        public FILETIME ReadFileTime() {
            FILETIME ft = new FILETIME();
            ft.dwLowDateTime = ReadInt32();
            ft.dwHighDateTime = ReadInt32();
            return ft;
        }
#endif

        //
        // Reads a string from the byte buffer, cached
        //  inside this object, and then updates the
        //  offset, NOTE: Must be in the correct offset
        //  before reading, or will error
        //
        public string ReadString() {
            string stringOut = null;
            int stringSize = ReadInt32();
            if (stringSize>0) {
                // prevent reading too much
                int actualSize = m_RegistryBytes.Length - m_ByteOffset;
                if (stringSize >= actualSize) {
                    stringSize = actualSize;
                }
                stringOut = Encoding.UTF8.GetString(m_RegistryBytes, m_ByteOffset, stringSize);
                m_ByteOffset += stringSize;
            }
            return stringOut;
        }


        //
        // Reads a DWORD into a Int32, used to read
        //  a int from the byte buffer.
        //
        internal unsafe int ReadInt32() {
            int intValue = 0;
            int actualSize = m_RegistryBytes.Length - m_ByteOffset;
            // copy bytes and increment offset
            if (actualSize>=sizeof(int)) {
                fixed (byte* pBuffer = m_RegistryBytes) {
                    if (sizeof(IntPtr)==4) {
                        intValue = *((int*)(pBuffer + m_ByteOffset));
                    }
                    else {
                        intValue = Marshal.ReadInt32((IntPtr)pBuffer, m_ByteOffset);
                    }
                }
                m_ByteOffset += sizeof(int);
            }
            // tell caller what we actually read
            return intValue;
        }
#else // !FEATURE_PAL
        private static string ReadConfigString(string ConfigName) {
            const int parameterValueLength = 255;
            StringBuilder parameterValue = new StringBuilder(parameterValueLength);
            bool rc = UnsafeNclNativeMethods.FetchConfigurationString(true, ConfigName, parameterValue, parameterValueLength);
            if (rc) {
                return parameterValue.ToString();
            }
            return "";
        }
#endif // !FEATURE_PAL

        //
        // Updates an instance of WbeProxy with the proxy settings from IE for:
        // the current user and a given connectoid.
        //
        [ResourceExposure(ResourceScope.Machine)]  // Check scoping on this SafeRegistryHandle
        [ResourceConsumption(ResourceScope.Machine)]
        protected override void BuildInternal()
        {
            GlobalLog.Enter("RegBlobWebProxyDataBuilder#" + ValidationHelper.HashString(this) + "::BuildInternal() m_Connectoid:" + ValidationHelper.ToString(m_Connectoid));

            // DON'T TOUCH THE ORDERING OF THE CALLS TO THE INSTANCE OF ProxyRegBlob
            bool success = ReadRegSettings();
            if (success) {
                success = ReadInt32() >= IE50StrucSize;
            }
            if (!success) {
                // if registry access fails rely on automatic detection
                SetAutoDetectSettings(true);
                return;
            }
            // read the rest of the items out
            ReadInt32(); // incremental version# of current settings (ignored)
            ProxyTypeFlags proxyFlags = (ProxyTypeFlags)ReadInt32(); // flags
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() proxyFlags:" + ValidationHelper.ToString(proxyFlags));

            string addressString = ReadString(); // proxy name
            string proxyBypassString = ReadString(); // proxy bypass
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() proxyAddressString:" + ValidationHelper.ToString(addressString) + " proxyBypassString:" + ValidationHelper.ToString(proxyBypassString));

            //
            // Once we verify that the flag for proxy is enabled,
            // Parse UriString that is stored, may be in the form,
            //  of "http=http://http-proxy;ftp="ftp=http://..." must
            //  handle this case along with just a URI.
            //
            if ((proxyFlags & ProxyTypeFlags.PROXY_TYPE_PROXY) != 0) {

                SetProxyAndBypassList(addressString, proxyBypassString);
            }


#if !FEATURE_PAL
            SetAutoDetectSettings((proxyFlags & ProxyTypeFlags.PROXY_TYPE_AUTO_DETECT) != 0);

            string autoConfigUrlString = ReadString(); // autoconfig url
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() scriptLocation:" + ValidationHelper.ToString(addressString));
            if ((proxyFlags & ProxyTypeFlags.PROXY_TYPE_AUTO_PROXY_URL) != 0) {
                SetAutoProxyUrl(autoConfigUrlString);
            }

            // The final straw against attempting to use the WinInet LKG script location was, it's invalid when IPs have changed even if the
            // connectoid hadn't.  Doing that validation didn't seem worth it (error-prone, expensive, unsupported).
#if USE_WINIET_AUTODETECT_CACHE
            proxyIE5Settings.ReadInt32(); // autodetect flags (ignored)

            // reuse addressString for lkgScriptLocationString
            addressString = proxyIE5Settings.ReadString(); // last known good auto-proxy url

            // read ftLastKnownDetectTime
            FILETIME ftLastKnownDetectTime = proxyIE5Settings.ReadFileTime();

            // Verify if this lkgScriptLocationString has timed out
            //
            if (IsValidTimeForLkgScriptLocation(ftLastKnownDetectTime)) {
                // reuse address for lkgScriptLocation
                GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() lkgScriptLocation:" + ValidationHelper.ToString(addressString));
                if (Uri.TryCreate(addressString, UriKind.Absolute, out address)) {
                    webProxyData.lkgScriptLocation = address;
                }
            }
            else {
#if TRAVE
                SYSTEMTIME st = new SYSTEMTIME();
                bool f = SafeNclNativeMethods.FileTimeToSystemTime(ref ftLastKnownDetectTime, ref st);
                if (f)
                    GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() ftLastKnownDetectTime:" + ValidationHelper.ToString(st));
#endif // TRAVE
                GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() Ignoring Timed out lkgScriptLocation:" + ValidationHelper.ToString(addressString));

                // Now rely on automatic detection settings set above
                // based on the proxy flags (webProxyData.automaticallyDetectSettings).
                //
            }
#endif
            /*
            // This is some of the rest of the proxy reg key blob parsing.
            //
            // Read Inte---- IPs
            int iftCount = proxyIE5Settings.ReadInt32();
            for (int ift = 0; ift < iftCount; ++ift) {
                proxyIE5Settings.ReadInt32();
            }

            // Read lpszAutoconfigSecondaryUrl
            string autoconfigSecondaryUrl = proxyIE5Settings.ReadString();

            // Read dwAutoconfigReloadDelayMins
            int autoconfigReloadDelayMins = proxyIE5Settings.ReadInt32();
            */
#endif
            GlobalLog.Leave("RegBlobWebProxyDataBuilder#" + ValidationHelper.HashString(this) + "::BuildInternal()");
        }

#if USE_WINIET_AUTODETECT_CACHE
#if !FEATURE_PAL
        internal unsafe static bool IsValidTimeForLkgScriptLocation(FILETIME ftLastKnownDetectTime) {
            // Get Current System Time.
            FILETIME ftCurrentTime = new FILETIME();
            SafeNclNativeMethods.GetSystemTimeAsFileTime(ref ftCurrentTime);

            UInt64 ftDetect = (UInt64)ftLastKnownDetectTime.dwHighDateTime;
            ftDetect <<= (sizeof(int) * 8);
            ftDetect |= (UInt64)(uint)ftLastKnownDetectTime.dwLowDateTime;

            UInt64 ftCurrent = (UInt64)ftCurrentTime.dwHighDateTime;
            ftCurrent <<= (sizeof(int) * 8);
            ftCurrent |= (UInt64)(uint)ftCurrentTime.dwLowDateTime;

            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() Detect Time:" + ValidationHelper.ToString(ftDetect));
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() Current Time:" + ValidationHelper.ToString(ftCurrent));
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() 7 days:" + ValidationHelper.ToString(s_lkgScriptValidTime));
            GlobalLog.Print("RegBlobWebProxyDataBuilder::BuildInternal() Delta Time:" + ValidationHelper.ToString((UInt64)(ftCurrent - ftDetect)));

            return (ftCurrent - ftDetect) < s_lkgScriptValidTime;
        }
#endif // !FEATURE_PAL
#endif
    }
}
