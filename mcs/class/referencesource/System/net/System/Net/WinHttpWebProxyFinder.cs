using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Net.Configuration;

namespace System.Net
{
    // This class uses WinHttp APIs only to find, download and execute the PAC file.
    internal sealed class WinHttpWebProxyFinder : BaseWebProxyFinder
    {
        private SafeInternetHandle session;
        private bool autoDetectFailed;

        public WinHttpWebProxyFinder(AutoWebProxyScriptEngine engine)
            : base(engine)
        {
            // Don't specify a user agent and dont' specify proxy settings. This is the same behavior WinHttp
            // uses when downloading the PAC file.
            session = UnsafeNclNativeMethods.WinHttp.WinHttpOpen(null,
                UnsafeNclNativeMethods.WinHttp.AccessType.NoProxy, null, null, 0);

            // Don't throw on error, just log the error information. This is consistent with how auto-proxy
            // works: we never throw on error (discovery, download, execution errors).
            if (session == null || session.IsInvalid)
            {
                int errorCode = GetLastWin32Error();
                if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_proxy_winhttp_cant_open_session, errorCode));
            }
            else
            {
                // The default download-timeout is 1 min.
                // WinHTTP will use the sum of all four timeouts provided in WinHttpSetTimeouts as the
                // actual timeout. Setting a value to 0 means "infinite".
                // Since we don't provide the ability to specify finegrained timeouts like WinHttp does,
                // we simply apply the configured timeout to all four WinHttp timeouts.
                int timeout = SettingsSectionInternal.Section.DownloadTimeout;

                if (!UnsafeNclNativeMethods.WinHttp.WinHttpSetTimeouts(session, timeout, timeout, timeout, timeout))
                {
                    // We weren't able to set the timeouts. Just log and continue.
                    int errorCode = GetLastWin32Error();
                    if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_proxy_winhttp_timeout_error, errorCode));
                }
            }
        }

        public override bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            proxyList = null;

            if (session == null || session.IsInvalid)
            {
                return false;
            }

            if (State == AutoWebProxyState.UnrecognizedScheme)
            {
                // If a previous call already determined that we don't support the scheme of the script
                // location, then just return false.
                return false;
            }

            string proxyListString = null;
            // Set to auto-detect failed. In case auto-detect is turned off and a script-location is available
            // we'll try downloading the script from that location.
            int errorCode = (int)UnsafeNclNativeMethods.WinHttp.ErrorCodes.AudodetectionFailed;

            // If auto-detect is turned on, try to execute DHCP/DNS query to get PAC file, then run the script
            if (Engine.AutomaticallyDetectSettings && !autoDetectFailed)
            {
                errorCode = GetProxies(destination, null, out proxyListString);
                
                // Remember if auto-detect failed. If config-script works, then the next time GetProxies() is
                // called, we'll not try auto-detect but jump right to config-script.
                autoDetectFailed = IsErrorFatalForAutoDetect(errorCode);

                if (errorCode == (int)UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnrecognizedScheme)
                {
                    // DHCP returned FILE or FTP scheme for the PAC file location: We should stop here
                    // since this is not an error, but a feature WinHttp doesn't currently support. The
                    // caller may be able to handle this case by using another WebProxyFinder.
                    State = AutoWebProxyState.UnrecognizedScheme;
                    return false;
                }
            }

            // If auto-detect failed or was turned off, and a config-script location is available, download
            // the script from that location and execute it.
            if ((Engine.AutomaticConfigurationScript != null) && (IsRecoverableAutoProxyError(errorCode)))
            {
                errorCode = GetProxies(destination, Engine.AutomaticConfigurationScript,
                    out proxyListString);
            }

            State = GetStateFromErrorCode(errorCode);

            if (State == AutoWebProxyState.Completed)
            {
                if (string.IsNullOrEmpty(proxyListString))
                {
                    // In this case the PAC file execution returned "DIRECT", i.e. WinHttp returned
                    // 'true' with a 'null' proxy string. This state is represented as a list
                    // containing one element with value 'null'.
                    proxyList = new string[1] { null };
                }
                else
                {
                    // WinHttp doesn't really clear all whitespaces. It does a pretty good job with
                    // spaces, but e.g. tabs aren't removed. Therefore make sure all whitespaces get
                    // removed.
                    // Note: Even though the PAC script could use space characters as separators,
                    // WinHttp will always use ';' as separator character. E.g. for the PAC result
                    // "PROXY 192.168.0.1 PROXY 192.168.0.2" WinHttp will return "192.168.0.1;192.168.0.2".
                    // WinHttp will also remove trailing ';'.
                    proxyListString = RemoveWhitespaces(proxyListString);
                    proxyList = proxyListString.Split(';');
                }
                return true;
            }

            // We get here if something went wrong, or if neither auto-detect nor script-location
            // were turned on.
            return false;
        }

        public override void Abort()
        {
            // WinHttp doesn't support aborts. Therefore we can't do anything here.
        }

        public override void Reset()
        {
            base.Reset();
            
            // Reset auto-detect failure: If the connection changes, we may be able to do auto-detect again.
            autoDetectFailed = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (session != null && !session.IsInvalid)
                {
                    session.Close();
                }
            }
        }

        private int GetProxies(Uri destination, Uri scriptLocation, out string proxyListString)
        {
            int errorCode = 0;
            proxyListString = null;

            UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions =
                new UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS();

            // Always try to download the PAC file without authentication. If we turn auth. on, the WinHttp
            // service will create a new session for every request (performance/memory implications).
            // Therefore we only turn auto-logon on if it is really needed.
            autoProxyOptions.AutoLogonIfChallenged = false;

            if (scriptLocation == null)
            {
                // Use auto-discovery to find the script location.
                autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoDetect;
                autoProxyOptions.AutoConfigUrl = null;
                autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.Dhcp |
                    UnsafeNclNativeMethods.WinHttp.AutoDetectType.DnsA;
            }
            else
            {
                // Use the provided script location for the PAC file.
                autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoProxyConfigUrl;
                autoProxyOptions.AutoConfigUrl = scriptLocation.ToString();
                autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.None;
            }

            if (!WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions, out proxyListString))
            {
                errorCode = GetLastWin32Error();

                // If the PAC file can't be downloaded because auth. was required, we check if the
                // credentials are set; if so, then we try again using auto-logon.
                // Note that by default webProxy.Credentials will be null. The user needs to set
                // <defaultProxy useDefaultCredentials="true"> in the config file, in order for
                // webProxy.Credentials to be set to DefaultNetworkCredentials.
                if ((errorCode == (int)UnsafeNclNativeMethods.WinHttp.ErrorCodes.LoginFailure) &&
                    (Engine.Credentials != null))
                {
                    // Now we need to try again, this time by enabling auto-logon.
                    autoProxyOptions.AutoLogonIfChallenged = true;

                    if (!WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions,
                        out proxyListString))
                    {
                        errorCode = GetLastWin32Error();
                    }
                }

                if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_proxy_winhttp_getproxy_failed, destination, errorCode));
            }

            return errorCode;
        }

        private bool WinHttpGetProxyForUrl(string destination,
            ref UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions,
            out string proxyListString)
        {
            proxyListString = null;

            bool success = false;
            UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO proxyInfo =
                new UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO();

            // Make sure the strings get cleaned up in a CER (thus unexpected exceptions, like
            // ThreadAbortException will not interrupt the execution of the finally block, and we'll not
            // leak resources).
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                success = UnsafeNclNativeMethods.WinHttp.WinHttpGetProxyForUrl(session,
                    destination, ref autoProxyOptions, out proxyInfo);

                if (success)
                {
                    proxyListString = Marshal.PtrToStringUni(proxyInfo.Proxy);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(proxyInfo.Proxy);
                Marshal.FreeHGlobal(proxyInfo.ProxyBypass);
            }

            return success;
        }

        private static int GetLastWin32Error()
        {
            int errorCode = Marshal.GetLastWin32Error();

            if (errorCode == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
            {
                throw new OutOfMemoryException();
            }

            return errorCode;
        }

        private static bool IsRecoverableAutoProxyError(int errorCode)
        {
            GlobalLog.Assert(errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER,
                "WinHttpGetProxyForUrl() call: Error code 'Invalid parameter' should not be returned.");

            // According to WinHttp the following states can be considered "recoverable", i.e.
            // we should continue trying WinHttpGetProxyForUrl() with the provided script-location
            // (if available).
            switch ((UnsafeNclNativeMethods.WinHttp.ErrorCodes)errorCode)
            {
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AudodetectionFailed:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.LoginFailure:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.OperationCancelled:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.Timeout:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnableToDownloadScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnrecognizedScheme:
                    return true;
            }

            return false;
        }

        private static AutoWebProxyState GetStateFromErrorCode(int errorCode)
        {
            if (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
            {
                return AutoWebProxyState.Completed;
            }

            switch ((UnsafeNclNativeMethods.WinHttp.ErrorCodes)errorCode)
            {
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AudodetectionFailed:
                    return AutoWebProxyState.DiscoveryFailure;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnableToDownloadScript:
                    return AutoWebProxyState.DownloadFailure;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnrecognizedScheme:
                    return AutoWebProxyState.UnrecognizedScheme;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.InvalidUrl:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError:
                    // AutoProxy succeeded, but no proxy could be found for this request
                    return AutoWebProxyState.Completed; 

                default:
                    // We don't know the exact cause of the failure. Set the state to compilation failure to
                    // indicate that something went wrong.
                    return AutoWebProxyState.CompilationFailure;
            }
        }

        private static string RemoveWhitespaces(string value)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in value)
            {
                if (!char.IsWhiteSpace(c))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        // Should we ignore auto-detect from now on?
        // http://msdn.microsoft.com/en-us/library/aa384097(VS.85).aspx
        private static bool IsErrorFatalForAutoDetect(int errorCode)
        {
            switch ((UnsafeNclNativeMethods.WinHttp.ErrorCodes)errorCode)
            {
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.Success:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.InvalidUrl:
                    // Some URIs are not supported (like Unicode hosts on Win7 and lower), 
                    // but our proxy is still valid
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript:
                    // Got the script, but something went wrong in execution.  For example, 
                    // the request was for an unresolvable single label name.
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError:
                    // Returned when a proxy for the specified URL cannot be located.
                    return false;

                default:
                    return true;
            }
        }
    }
}
