using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Net
{
    internal sealed class WinHttpWebProxyBuilder : WebProxyDataBuilder
    {
        protected override void BuildInternal()
        {
            GlobalLog.Enter("WinHttpWebProxyBuilder#" + ValidationHelper.HashString(this) + "::BuildInternal()");

            UnsafeNclNativeMethods.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG ieProxyConfig =
                new UnsafeNclNativeMethods.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG();

            // Make sure the native strings get freed, even if some unexpected exception occurs.
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (UnsafeNclNativeMethods.WinHttp.WinHttpGetIEProxyConfigForCurrentUser(ref ieProxyConfig))
                {
                    string proxy = null;
                    string proxyByPass = null;
                    string autoConfigUrl = null;

                    proxy = Marshal.PtrToStringUni(ieProxyConfig.Proxy);
                    proxyByPass = Marshal.PtrToStringUni(ieProxyConfig.ProxyBypass);
                    autoConfigUrl = Marshal.PtrToStringUni(ieProxyConfig.AutoConfigUrl);

                    // note that ieProxyConfig.Proxy will be null if "use a proxy server" flag is turned off, even if
                    // the user specified a proxy address. When we read directly from the Registry we need to check
                    // for ProxyTypeFlags.PROXY_TYPE_PROXY. WinHttp does this for us and if the flag is not set, 
                    // ieProxyConfig.Proxy will be null.
                    SetProxyAndBypassList(proxy, proxyByPass);

                    SetAutoDetectSettings(ieProxyConfig.AutoDetect);

                    // similar to comment above: ieProxyConfig.AutoConfigUrl will only be set if "automatically detect
                    // settings" flag is set. We don't need to check ProxyTypeFlags.PROXY_TYPE_AUTO_PROXY_URL; WinHttp
                    // takes care of it and sets AutoConfigUrl to null if the flag is not set, regardless of the actual
                    // config script string.
                    SetAutoProxyUrl(autoConfigUrl);
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();

                    if (errorCode == Microsoft.Win32.NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
                    {
                        throw new OutOfMemoryException();
                    }

                    // if API call fails, rely on automatic detection
                    SetAutoDetectSettings(true);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ieProxyConfig.Proxy);
                Marshal.FreeHGlobal(ieProxyConfig.ProxyBypass);
                Marshal.FreeHGlobal(ieProxyConfig.AutoConfigUrl);
            }

            GlobalLog.Leave("WinHttpWebProxyBuilder#" + ValidationHelper.HashString(this) + "::BuildInternal()");
        }
    }
}
