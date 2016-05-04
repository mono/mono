using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Text;
using System.Net.Cache;
using System.Globalization;
using System.Net.Configuration;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;

namespace System.Net
{
    // This WebProxyFinder implementation has the following purpose:
    // - use WinHttp APIs to determine the location of the PAC file
    // - use System.Net classes (WebRequest) to download the PAC file
    // - use Microsoft.JScript to compile and execute the JavaScript in the PAC file.
    internal sealed class NetWebProxyFinder : BaseWebProxyFinder
    {
        private static readonly char[] splitChars = new char[] { ';' };
        private static TimerThread.Queue timerQueue;
        private static readonly TimerThread.Callback timerCallback = new TimerThread.Callback(RequestTimeoutCallback);
        private static readonly WaitCallback abortWrapper = new WaitCallback(AbortWrapper);

        private RequestCache backupCache;
        private AutoWebProxyScriptWrapper scriptInstance;
        private Uri engineScriptLocation;
        private Uri scriptLocation;
        private bool scriptDetectionFailed;
        private object lockObject;
        // Keep the following fields volatile, since we're accessing them outside of lock blocks
        private volatile WebRequest request;
        private volatile bool aborted;

        public NetWebProxyFinder(AutoWebProxyScriptEngine engine)
            : base(engine)
        {
            backupCache = new SingleItemRequestCache(RequestCacheManager.IsCachingEnabled);
            lockObject = new object();
        }

        public override bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            try
            {
                proxyList = null;

                EnsureEngineAvailable();

                // after EnsureEngineAvailable we expect State to be CompilationSuccess, otherwise return.
                if (State != AutoWebProxyState.Completed)
                {
                    // the script can't run, say we're not ready and bypass
                    return false;
                }

                bool result = false;
                try
                {
                    string proxyListString = scriptInstance.FindProxyForURL(destination.ToString(), destination.Host);
                    GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::GetProxies() calling ExecuteFindProxyForURL() for destination:" + ValidationHelper.ToString(destination) + " returned scriptReturn:" + ValidationHelper.ToString(proxyList));

                    proxyList = ParseScriptResult(proxyListString);

                    result = true;
                }
                catch (Exception exception)
                {
                    if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_script_execution_error, exception));
                }

                return result;
            }
            finally
            {
                // Reset state of 'aborted', since next call to GetProxies() must not use previous aborted state.
                aborted = false;
            }
        }

        public override void Abort()
        {
            // All we abort is a running WebRequest. The following lock (and the one in DownloadAndCompile)
            // is used to "atomically" access the two fields 'aborted' and 'request': If Abort() gets
            // called before 'request' is set, the 'aborted' field will signal to DownloadAndCompile, that
            // it should not bother creating a request and just throw. If 'request' was already created
            // by DownloadAndCompile, the following code will make sure the request gets aborted.
            lock (lockObject)
            {
                aborted = true;

                if (request != null)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(abortWrapper, request);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (scriptInstance != null)
                {
                    scriptInstance.Close();
                }
            }
        }

        // Ensures that (if state is AutoWebProxyState.CompilationSuccess) there is an engine available to execute script.
        // Figures out the script location (might discover if needed).
        // Calls DownloadAndCompile().
        private void EnsureEngineAvailable()
        {
            GlobalLog.Enter("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable");

            if (State == AutoWebProxyState.Uninitialized || engineScriptLocation == null)
            {
#if !FEATURE_PAL
                if (Engine.AutomaticallyDetectSettings)
                {
                    GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() Attempting auto-detection.");
                    DetectScriptLocation();
                    if (scriptLocation != null)
                    {
                        //
                        // Successfully detected or user has flipped the automaticallyDetectSettings bit.
                        // Attempt a non conclusive DownloadAndCompile() so we can fallback
                        //
                        GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() discovered:" + ValidationHelper.ToString(scriptLocation) + " engineScriptLocation:" + ValidationHelper.ToString(engineScriptLocation));
                        if (scriptLocation.Equals(engineScriptLocation))
                        {
                            State = AutoWebProxyState.Completed;
                            GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                            return;
                        }
                        AutoWebProxyState newState = DownloadAndCompile(scriptLocation);
                        if (newState == AutoWebProxyState.Completed)
                        {
                            State = AutoWebProxyState.Completed;
                            engineScriptLocation = scriptLocation;
                            GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                            return;
                        }
                    }
                }
#endif // !FEATURE_PAL

                // Either Auto-Detect wasn't enabled or something failed with it.  Try the manual script location.
                if ((Engine.AutomaticConfigurationScript != null) && !aborted)
                {
                    GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() using automaticConfigurationScript:" + ValidationHelper.ToString(Engine.AutomaticConfigurationScript) + " engineScriptLocation:" + ValidationHelper.ToString(engineScriptLocation));
                    if (Engine.AutomaticConfigurationScript.Equals(engineScriptLocation))
                    {
                        State = AutoWebProxyState.Completed;
                        GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                        return;
                    }
                    State = DownloadAndCompile(Engine.AutomaticConfigurationScript);
                    if (State == AutoWebProxyState.Completed)
                    {
                        engineScriptLocation = Engine.AutomaticConfigurationScript;
                        GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                        return;
                    }
                }
            }
            else
            {
                // We always want to call DownloadAndCompile to check the expiration.
                GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() State:" + State + " engineScriptLocation:" + ValidationHelper.ToString(engineScriptLocation));
                State = DownloadAndCompile(engineScriptLocation);
                if (State == AutoWebProxyState.Completed)
                {
                    GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                    return;
                }

                // There's still an opportunity to fail over to the automaticConfigurationScript.
                if (!engineScriptLocation.Equals(Engine.AutomaticConfigurationScript) && !aborted)
                {
                    GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() Update failed.  Falling back to automaticConfigurationScript:" + ValidationHelper.ToString(Engine.AutomaticConfigurationScript));
                    State = DownloadAndCompile(Engine.AutomaticConfigurationScript);
                    if (State == AutoWebProxyState.Completed)
                    {
                        engineScriptLocation = Engine.AutomaticConfigurationScript;
                        GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
                        return;
                    }
                }
            }

            // Everything failed.  Set this instance to mostly-dead.  It will wake up again if there's a reg/connectoid change.
            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable() All failed.");
            State = AutoWebProxyState.DiscoveryFailure;

            if (scriptInstance != null)
            {
                scriptInstance.Close();
                scriptInstance = null;
            }

            engineScriptLocation = null;

            GlobalLog.Leave("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::EnsureEngineAvailable", ValidationHelper.ToString(State));
        }


        // Downloads and compiles the script from a given Uri.
        // This code can be called by config for a downloaded control, we need to assert.
        // This code is called holding the lock.
        private AutoWebProxyState DownloadAndCompile(Uri location)
        {
            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() location:" + ValidationHelper.ToString(location));
            AutoWebProxyState newState = AutoWebProxyState.DownloadFailure;
            WebResponse response = null;
            TimerThread.Timer timer = null;
            AutoWebProxyScriptWrapper newScriptInstance = null;

            // Can't assert this in declarative form (DCR?). This Assert() is needed to be able to create the request to download the proxy script.
            ExceptionHelper.WebPermissionUnrestricted.Assert();
            try
            {
                lock (lockObject)
                {
                    if (aborted)
                    {
                        throw new WebException(NetRes.GetWebStatusString("net_requestaborted",
                            WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    }

                    request = WebRequest.Create(location);
                }

                request.Timeout = Timeout.Infinite;
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                request.ConnectionGroupName = "__WebProxyScript";

                // We have an opportunity here, if caching is disabled AppDomain-wide, to override it with a
                // custom, trivial cache-provider to get a similar semantic.
                //
                // We also want to have a backup caching key in the case when IE has locked an expired script response
                //
                if (request.CacheProtocol != null)
                {
                    GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() Using backup caching.");
                    request.CacheProtocol = new RequestCacheProtocol(backupCache, request.CacheProtocol.Validator);
                }

                HttpWebRequest httpWebRequest = request as HttpWebRequest;
                if (httpWebRequest != null)
                {
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.UserAgent = this.GetType().FullName + "/" + Environment.Version;
                    httpWebRequest.KeepAlive = false;
                    httpWebRequest.Pipelined = false;
                    httpWebRequest.InternalConnectionGroup = true;
                }
                else
                {
                    FtpWebRequest ftpWebRequest = request as FtpWebRequest;
                    if (ftpWebRequest != null)
                    {
                        ftpWebRequest.KeepAlive = false;
                    }
                }

                // Use no proxy, default cache - initiate the download.
                request.Proxy = null;
                request.Credentials = Engine.Credentials;

                // Use our own timeout timer so that it can encompass the whole request, not just the headers.
                if (timerQueue == null)
                {
                    timerQueue = TimerThread.GetOrCreateQueue(SettingsSectionInternal.Section.DownloadTimeout);
                }
                timer = timerQueue.CreateTimer(timerCallback, request);
                response = request.GetResponse();

                // Check Last Modified.
                DateTime lastModified = DateTime.MinValue;
                HttpWebResponse httpResponse = response as HttpWebResponse;
                if (httpResponse != null)
                {
                    lastModified = httpResponse.LastModified;
                }
                else
                {
                    FtpWebResponse ftpResponse = response as FtpWebResponse;
                    if (ftpResponse != null)
                    {
                        lastModified = ftpResponse.LastModified;
                    }
                }
                GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() lastModified:" + lastModified.ToString() + " (script):" + (scriptInstance == null ? "(null)" : scriptInstance.LastModified.ToString()));
                if (scriptInstance != null && lastModified != DateTime.MinValue && scriptInstance.LastModified == lastModified)
                {
                    newScriptInstance = scriptInstance;
                    newState = AutoWebProxyState.Completed;
                }
                else
                {
                    string scriptBody = null;
                    byte[] scriptBuffer = null;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        SingleItemRequestCache.ReadOnlyStream ros = responseStream as SingleItemRequestCache.ReadOnlyStream;
                        if (ros != null)
                        {
                            scriptBuffer = ros.Buffer;
                        }
                        if (scriptInstance != null && scriptBuffer != null && scriptBuffer == scriptInstance.Buffer)
                        {
                            scriptInstance.LastModified = lastModified;
                            newScriptInstance = scriptInstance;
                            newState = AutoWebProxyState.Completed;
                            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() Buffer matched - reusing Engine.");
                        }
                        else
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream))
                            {
                                scriptBody = streamReader.ReadToEnd();
                            }
                        }
                    }

                    WebResponse tempResponse = response;
                    response = null;
                    tempResponse.Close();
                    timer.Cancel();
                    timer = null;

                    if (newState != AutoWebProxyState.Completed)
                    {
                        GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() IsFromCache:" + tempResponse.IsFromCache.ToString() + " scriptInstance:" + ValidationHelper.HashString(scriptInstance));
                        if (scriptInstance != null && scriptBody == scriptInstance.ScriptBody)
                        {
                            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() Script matched - using existing Engine.");
                            scriptInstance.LastModified = lastModified;
                            if (scriptBuffer != null)
                            {
                                scriptInstance.Buffer = scriptBuffer;
                            }
                            newScriptInstance = scriptInstance;
                            newState = AutoWebProxyState.Completed;
                        }
                        else
                        {
                            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() Creating AutoWebProxyScriptWrapper.");
                            newScriptInstance = new AutoWebProxyScriptWrapper();
                            newScriptInstance.LastModified = lastModified;

                            if (newScriptInstance.Compile(location, scriptBody, scriptBuffer))
                            {
                                newState = AutoWebProxyState.Completed;
                            }
                            else
                            {
                                newState = AutoWebProxyState.CompilationFailure;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_script_download_compile_error, exception));
                GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() Download() threw:" + ValidationHelper.ToString(exception));
            }
            finally
            {
                if (timer != null)
                {
                    timer.Cancel();
                }

                // 
                try
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                finally
                {
                    WebPermission.RevertAssert();

                    // The request is not needed anymore. Set it to null, so if Abort() gets called,
                    // after this point, it will result in a no-op.
                    request = null;
                }
            }

            if ((newState == AutoWebProxyState.Completed) && (scriptInstance != newScriptInstance))
            {
                if (scriptInstance != null)
                {
                    scriptInstance.Close();
                }

                scriptInstance = newScriptInstance;
            }

            GlobalLog.Print("NetWebProxyFinder#" + ValidationHelper.HashString(this) + "::DownloadAndCompile() retuning newState:" + ValidationHelper.ToString(newState));
            return newState;
        }

        private static IList<string> ParseScriptResult(string scriptReturn)
        {
            IList<string> result = new List<string>();

            if (scriptReturn == null)
            {
                return result;
            }

            string[] proxyListStrings = scriptReturn.Split(splitChars);
            string proxyAuthority;
            foreach (string s in proxyListStrings)
            {
                string proxyString = s.Trim(' ');
                if (!proxyString.StartsWith("PROXY ", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Compare("DIRECT", proxyString, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        proxyAuthority = null;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    // remove prefix "PROXY " (6 chars) from the string and trim additional leading spaces.
                    proxyAuthority = proxyString.Substring(6).TrimStart(' ');
                    Uri uri = null;
                    bool tryParse = Uri.TryCreate("http://" + proxyAuthority, UriKind.Absolute, out uri);
                    if (!tryParse || uri.UserInfo.Length > 0 || uri.HostNameType == UriHostNameType.Basic || uri.AbsolutePath.Length != 1 || proxyAuthority[proxyAuthority.Length - 1] == '/' || proxyAuthority[proxyAuthority.Length - 1] == '#' || proxyAuthority[proxyAuthority.Length - 1] == '?')
                    {
                        continue;
                    }
                }
                result.Add(proxyAuthority);
            }

            return result;
        }

        private void DetectScriptLocation()
        {
            if (scriptDetectionFailed || scriptLocation != null)
            {
                return;
            }

            GlobalLog.Print("NetWebProxyFinder::DetectScriptLocation() Attempting discovery PROXY_AUTO_DETECT_TYPE_DHCP.");
            scriptLocation = SafeDetectAutoProxyUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType.Dhcp);

            if (scriptLocation == null)
            {
                GlobalLog.Print("NetWebProxyFinder::DetectScriptLocation() Attempting discovery AUTO_DETECT_TYPE_DNS_A.");
                scriptLocation = SafeDetectAutoProxyUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType.DnsA);
            }

            if (scriptLocation == null)
            {
                GlobalLog.Print("NetWebProxyFinder::DetectScriptLocation() Discovery failed.");
                scriptDetectionFailed = true;
            }
        }

        // from wininet.h
        //
        //  #define INTERNET_MAX_PATH_LENGTH        2048
        //  #define INTERNET_MAX_PROTOCOL_NAME      "gopher"    // longest protocol name
        //  #define INTERNET_MAX_URL_LENGTH         ((sizeof(INTERNET_MAX_PROTOCOL_NAME) - 1) \
        //                                          + sizeof("://") \
        //                                          + INTERNET_MAX_PATH_LENGTH)
        //
        private const int MaximumProxyStringLength = 2058;

        /// <devdoc>
        ///     <para>
        ///         Called to discover script location. This performs
        ///         autodetection using the method specified in the detectFlags.
        ///     </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Reliability","CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="Implementation requires DangerousGetHandle")]
        private static unsafe Uri SafeDetectAutoProxyUrl(
            UnsafeNclNativeMethods.WinHttp.AutoDetectType discoveryMethod)
        {
            Uri autoProxy = null;

#if !FEATURE_PAL
            string url = null;
            
            GlobalLog.Print("NetWebProxyFinder::SafeDetectAutoProxyUrl() Using WinHttp.");
            SafeGlobalFree autoProxyUrl;
            bool success = UnsafeNclNativeMethods.WinHttp.WinHttpDetectAutoProxyConfigUrl(discoveryMethod, out autoProxyUrl);
            if (!success)
            {
                if (autoProxyUrl != null)
                {
                    autoProxyUrl.SetHandleAsInvalid();
                }
            }
            else
            {
                url = new string((char*)autoProxyUrl.DangerousGetHandle());
                autoProxyUrl.Close();
            }
            
            if (url != null)
            {
                bool parsed = Uri.TryCreate(url, UriKind.Absolute, out autoProxy);
                if (!parsed)
                {
                    if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_autodetect_script_location_parse_error, ValidationHelper.ToString(url)));
                    GlobalLog.Print("NetWebProxyFinder::SafeDetectAutoProxyUrl() Uri.TryParse() failed url:" + ValidationHelper.ToString(url));
                }
            }
            else
            {
                if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_autodetect_failed));
                GlobalLog.Print("NetWebProxyFinder::SafeDetectAutoProxyUrl() DetectAutoProxyUrl() returned false");
            }
#endif // !FEATURE_PAL

            return autoProxy;
        }

        // RequestTimeoutCallback - Called by the TimerThread to abort a request.  This just posts ThreadPool work item - Abort() does too
        // much to be done on the timer thread (timer thread should never block or call user code).
        private static void RequestTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ThreadPool.UnsafeQueueUserWorkItem(abortWrapper, context);
        }

        private static void AbortWrapper(object context)
        {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.Worker);
            using (GlobalLog.SetThreadKind(ThreadKinds.System))
            {
#endif
                if (context != null)
                {
                    ((WebRequest)context).Abort();
                }
#if DEBUG
            }
#endif
        }
    }
}
