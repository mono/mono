using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Net
{
    // This class behaves the same as WinHttpWebProxyFinder. The only difference is that in cases where
    // the script location has a scheme != HTTP, it falls back to NetWebProxyFinder which supports
    // also other schemes like FILE and FTP.
    // The mid-term goal for WinHttp is to support at least FILE scheme since it was already requested
    // by customers. The long term goal for System.Net is to use WinHttp only and remove this class
    // as well as NetWebProxyFinder.
    internal sealed class HybridWebProxyFinder : IWebProxyFinder
    {
        private const string allowFallbackKey = @"SOFTWARE\Microsoft\.NETFramework";
        private const string allowFallbackKeyPath = @"HKEY_LOCAL_MACHINE\" + allowFallbackKey;
        private const string allowFallbackValueName = "LegacyWPADSupport";

        private static bool allowFallback;

        private NetWebProxyFinder netFinder;
        private WinHttpWebProxyFinder winHttpFinder;
        private BaseWebProxyFinder currentFinder;
        private AutoWebProxyScriptEngine engine;

        static HybridWebProxyFinder()
        {
            InitializeFallbackSettings();
        }

        public HybridWebProxyFinder(AutoWebProxyScriptEngine engine)
        {
            this.engine = engine;            
            this.winHttpFinder = new WinHttpWebProxyFinder(engine);
            this.currentFinder = winHttpFinder;
        }

        public bool IsValid
        {
            get { return currentFinder.IsValid; }
        }

        public bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            if (currentFinder.GetProxies(destination, out proxyList))
            {
                return true;
            }

            if (allowFallback && currentFinder.IsUnrecognizedScheme && (currentFinder == winHttpFinder))
            {
                // If WinHttpWebProxyFinder failed because the script location has a != HTTP scheme,
                // fall back to NetWebProxyFinder which supports also other schemes.
                if (netFinder == null)
                {
                    netFinder = new NetWebProxyFinder(engine);
                }
                currentFinder = netFinder;
                return currentFinder.GetProxies(destination, out proxyList);
            }

            return false;
        }

        public void Abort()
        {
            // Abort only the current finder. There is no need to abort the other one (which is either
            // uninitialized, i.e. not used yet, or we have an unrecognized-scheme state, which should
            // not be changed).
            currentFinder.Abort();
        }

        public void Reset()
        {
            winHttpFinder.Reset();

            if (netFinder != null)
            {
                netFinder.Reset();
            }

            // Some settings changed, so let's reset the current finder to WinHttpWebProxyFinder, since 
            // now it may work (if it didn't already before).
            currentFinder = winHttpFinder;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                winHttpFinder.Dispose();

                if (netFinder != null)
                {
                    netFinder.Dispose();
                }
            }
        }

        [RegistryPermission(SecurityAction.Assert, Read = allowFallbackKeyPath)]
        private static void InitializeFallbackSettings()
        {
            allowFallback = false;

            try
            {
                // We read reflected keys on WOW64.
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(allowFallbackKey))
                {
                    try
                    {
                        object fallbackKeyValue = key.GetValue(allowFallbackValueName, null);

                        // Setting the value to 1 will enable fallback. All other values are ignored (i.e. no fallback).
                        if ((fallbackKeyValue != null) && (key.GetValueKind(allowFallbackValueName) == RegistryValueKind.DWord))
                        {
                            allowFallback = ((int)fallbackKeyValue) == 1;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
