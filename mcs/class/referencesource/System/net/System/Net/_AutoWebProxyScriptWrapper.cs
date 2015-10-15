//------------------------------------------------------------------------------
// <copyright fieldInfole="_AutoWebProxyScriptWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


// Two implementations of AutoWebProxyScriptWrapper live in this file.  The first uses jscript.dll via COM, the second
// uses Microsoft.JScript using an external AppDomain.

#pragma warning disable 618

#define AUTOPROXY_MANAGED_JSCRIPT
#if !AUTOPROXY_MANAGED_JSCRIPT

namespace System.Net
{
    using System.Net.ComImports;
    using System.Runtime.InteropServices;
    using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;
    using System.Threading;
    using System.Reflection;
    using System.Net.Configuration;
    using System.Globalization;

    internal class AutoWebProxyScriptWrapper
    {
        const string c_ScriptHelperName = "__AutoWebProxyScriptHelper";

        private static TimerThread.Queue s_TimerQueue = TimerThread.CreateQueue(SettingsSectionInternal.Section.ExecutionTimeout);
        private static TimerThread.Callback s_InterruptCallback = new TimerThread.Callback(InterruptCallback);


        //
        // Methods called by the AutoWebProxyScriptEngine
        //

        internal static AutoWebProxyScriptWrapper CreateInstance()
        {
            return new AutoWebProxyScriptWrapper();
        }

        internal void Close()
        {
            if (Interlocked.Increment(ref closed) != 1)
            {
                return;
            }

            GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Close() Closing engine.");

            // Time out any running thread.
            TimerThread.Timer timer = activeTimer;
            if (timer != null)
            {
                if (timer.Cancel())
                {
                    InterruptCallback(timer, 0, this);
                }
                activeTimer = null;
            }

            jscript.Close();
            jscriptObject = null;
            jscript = null;
            host = null;
            jscriptParser = null;
            dispatch = null;
            script = null;
            scriptText = null;
            lastModified = DateTime.MinValue;
        }

        internal string FindProxyForURL(string url, string host)
        {
            if (url == null || host == null)
            {
                throw new ArgumentNullException(url == null ? "url" : "host");
            }
            if (closed != 0)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            EXCEPINFO exceptionInfo = new EXCEPINFO();
            object result = null;
            jscript.GetCurrentScriptThreadID(out interruptThreadId);
            TimerThread.Timer timer = s_TimerQueue.CreateTimer(s_InterruptCallback, this);
            activeTimer = timer;
            try
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::FindProxyForURL() Calling url:" + url + " host:" + host);
                result = script.FindProxyForURL(url, host);
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;
                if (exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                COMException comException = exception as COMException;
                if (comException == null || comException.ErrorCode != (int) HRESULT.SCRIPT_E_REPORTED)
                {
                    throw;
                }
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::FindProxyForURL() Script error:[" + this.host.ExceptionMessage == null ? "" : this.host.ExceptionMessage + "]");
            }
            catch {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::FindProxyForURL() Script error:[Non-CLS Compliant Exception]");
                throw;
            }
            finally
            {
                activeTimer = null;
                timer.Cancel();
            }

            string proxy = result as string;
            if (proxy != null)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::FindProxyForURL() found:" + proxy);
                return proxy;
            }

            GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::FindProxyForURL() Returning null. result:" + ValidationHelper.ToString(exceptionInfo.bstrDescription) + " result:" + ValidationHelper.ToString(result) + " error:" + ValidationHelper.ToString(exceptionInfo.bstrDescription));
            return null;
        }

        internal AutoWebProxyState Compile(Uri engineScriptLocation, string scriptBody, byte[] buffer)
        {
            if (closed != 0)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (jscriptObject != null)
            {
                jscript.Close();
            }

            scriptText = null;
            scriptBytes = null;
            jscriptObject = new JScriptEngine();
            jscript = (IActiveScript) jscriptObject;
            host = new ScriptHost();
            
            GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() Binding to ScriptHost#" + ValidationHelper.HashString(this));
            
            jscriptParser = new ActiveScriptParseWrapper(jscriptObject);
            jscriptParser.InitNew();

            jscript.SetScriptSite(host);
            jscript.SetScriptState(ScriptState.Initialized);

            //
            // Inform the script engine that this host implements the IInternetHostSecurityManager interface, which
            // is used to prevent the script code from using any ActiveX objects.
            //
            IObjectSafety objSafety = jscript as IObjectSafety;
            if (objSafety != null)
            {
                Guid guid = Guid.Empty;
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() Setting up IInternetHostSecurityManager");
                objSafety.SetInterfaceSafetyOptions(ref guid, ComConstants.INTERFACE_USES_SECURITY_MANAGER, ComConstants.INTERFACE_USES_SECURITY_MANAGER);
                objSafety = null;
            }

            EXCEPINFO exceptionInfo = new EXCEPINFO();
            object result = null;
            try
            {
                jscriptParser.ParseScriptText(scriptBody, null, null, null, IntPtr.Zero, 0, ScriptText.IsPersistent | ScriptText.IsVisible, out result, out exceptionInfo);
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() ParseScriptText() success:" + ValidationHelper.ToString(exceptionInfo.bstrDescription) + " result:" + ValidationHelper.ToString(result));
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;
                if (exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                COMException comException = exception as COMException;
                if (comException == null || comException.ErrorCode != (int) HRESULT.SCRIPT_E_REPORTED)
                {
                    throw;
                }
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() Script load error:[" + host.ExceptionMessage == null ? "" : host.ExceptionMessage + "]");
                throw new COMException(SR.GetString(SR.net_jscript_load, host.ExceptionMessage), comException.ErrorCode);
            }
            catch {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() Script load error:[Non-CLS Compliant Exception]");
                throw;
            }

            jscript.AddNamedItem(c_ScriptHelperName, ScriptItem.GlobalMembers | ScriptItem.IsPersistent | ScriptItem.IsVisible);

            // This part can run global code - time it out if necessary.
            jscript.GetCurrentScriptThreadID(out interruptThreadId);
            TimerThread.Timer timer = s_TimerQueue.CreateTimer(s_InterruptCallback, this);
            activeTimer = timer;
            try
            {
                jscript.SetScriptState(ScriptState.Started);
                jscript.SetScriptState(ScriptState.Connected);
            }
            finally
            {
                activeTimer = null;
                timer.Cancel();
            }

            jscript.GetScriptDispatch(null, out script);
            GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(this) + "::Compile() Got IDispatch:" + ValidationHelper.ToString(dispatch));

            scriptText = scriptBody;
            scriptBytes = buffer;

            return AutoWebProxyState.CompilationSuccess;
        }

        internal string ScriptBody
        {
            get
            {
                return scriptText;
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return scriptBytes;
            }

            set
            {
                scriptBytes = value;
            }
        }

        internal DateTime LastModified
        {
            get
            {
                return lastModified;
            }

            set
            {
                lastModified = value;
            }
        }

        private static void InterruptCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            AutoWebProxyScriptWrapper pThis = (AutoWebProxyScriptWrapper)context;
            GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(pThis) + "::InterruptCallback()");

            if (!object.ReferenceEquals(timer, pThis.activeTimer))
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(pThis) + "::InterruptCallback() Spurious - returning.");
                return;
            }

            EXCEPINFO exceptionInfo;
            try
            {
                pThis.jscript.InterruptScriptThread(pThis.interruptThreadId, out exceptionInfo, 0);
            }
            catch (Exception ex)
            {
                if (NclUtilities.IsFatal(ex)) throw;
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(pThis) + "::InterruptCallback() InterruptScriptThread() threw:" + ValidationHelper.ToString(ex));
            }
            catch {
                GlobalLog.Print("AutoWebProxyScriptWrapper#" + ValidationHelper.HashString(pThis) + "::InterruptCallback() InterruptScriptThread() threw: Non-CLS Compliant Exception");
            }
        }


        //
        // Privates
        //

        private JScriptEngine jscriptObject;
        private IActiveScript jscript;
        private ActiveScriptParseWrapper jscriptParser;
        private ScriptHost host;
        private object dispatch;
        private IScript script;
        private string scriptText;
        private byte[] scriptBytes;
        private DateTime lastModified;

        // 'activeTimer' is used to protect the engine from spurious callbacks and when the engine is closed.
        private TimerThread.Timer activeTimer;
        private uint interruptThreadId;

        private int closed;


        // IActiveScriptSite implementation
        private class ScriptHost : IActiveScriptSite, IInternetHostSecurityManager, IOleServiceProvider
        {
            private WebProxyScriptHelper helper = new WebProxyScriptHelper();
            private string exceptionMessage;

            internal string ExceptionMessage
            {
                get
                {
                    return exceptionMessage;
                }
            }

            //
            // IActiveScriptSite
            //

            public void GetLCID(out int lcid)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetLCID()");
                lcid = Thread.CurrentThread.CurrentCulture.LCID;
            }

            public void GetItemInfo(
                string name,
                ScriptInfo returnMask,
                [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] object[] item,
                [Out] [MarshalAs(UnmanagedType.LPArray)] IntPtr[] typeInfo)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetItemInfo() name:" + ValidationHelper.ToString(name));
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                if (name != c_ScriptHelperName)
                {
                    throw new COMException(null, (int) HRESULT.TYPE_E_ELEMENTNOTFOUND);
                }

                if ((returnMask & ScriptInfo.IUnknown) != 0)
                {
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetItemInfo() Setting item.");
                    item[0] = helper;
                }

                if ((returnMask & ScriptInfo.ITypeInfo) != 0)
                {
                    if (typeInfo == null)
                    {
                        throw new ArgumentNullException("typeInfo");
                    }

                    typeInfo[0] = IntPtr.Zero;
                }

                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetItemInfo() Done.");
            }

            public void GetDocVersionString(out string version)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetDocVersionString()");
                throw new NotImplementedException();
            }

            public void OnScriptTerminate(object result, EXCEPINFO exceptionInfo)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::OnScriptTerminate() result:" + ValidationHelper.ToString(result) + " error:" + ValidationHelper.ToString(exceptionInfo.bstrDescription));
            }

            public void OnStateChange(ScriptState scriptState)
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::OnStateChange() state:" + ValidationHelper.ToString(scriptState));
                if (scriptState == ScriptState.Closed)
                {
                    helper = null;
                }
            }

            public void OnScriptError(IActiveScriptError scriptError)
            {
                EXCEPINFO exceptionInfo;
                uint dummy;
                uint line;
                int pos;
                scriptError.GetExceptionInfo(out exceptionInfo);
                scriptError.GetSourcePosition(out dummy, out line, out pos);
                exceptionMessage = exceptionInfo.bstrDescription + " (" + line + "," + pos + ")";
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::OnScriptError() error:" + ValidationHelper.ToString(exceptionInfo.bstrDescription) + " line:" + line + " pos:" + pos);
            }

            public void OnEnterScript()
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::OnEnterScript()");
            }

            public void OnLeaveScript()
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::OnLeaveScript()");
            }

            // IOleServiceProvider methods

            public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject) {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::QueryService(" + guidService.ToString() + ")");
                
                int hr = (int)HRESULT.E_NOINTERFACE;
                ppvObject = IntPtr.Zero;
                
                if (guidService == typeof(IInternetHostSecurityManager).GUID) {
                    IntPtr ppObj = Marshal.GetIUnknownForObject(this);
                    try {
                        hr = Marshal.QueryInterface(ppObj, ref riid, out ppvObject);
                    }
                    finally {
                        Marshal.Release(ppObj);
                    }
                }

                return hr;
            }

            // IInternetHostSecurityManager methods.
            // Implementation based on inetcore\wininet\autoconf\cscpsite.cpp
            // The current implementation disallows all ActiveX control activation from script.

            public int GetSecurityId(byte[] pbSecurityId, ref IntPtr pcbSecurityId, IntPtr dwReserved) {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::GetSecurityId()");
                return (int)HRESULT.E_NOTIMPL;
            }

            public int ProcessUrlAction(int dwAction, int[] pPolicy, int cbPolicy, byte[] pContext, int cbContext, int dwFlags, int dwReserved) {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::ProcessUrlAction()");
                if (pPolicy != null && cbPolicy >= Marshal.SizeOf(typeof(int))) {
                    pPolicy[0] = (int)UrlPolicy.DisAllow;
                }

                return (int)HRESULT.S_FALSE; // S_FALSE means the policy != URLPOLICY_ALLOW.
            }

            public int QueryCustomPolicy(Guid guidKey, out byte[] ppPolicy, out int pcbPolicy, byte[] pContext, int cbContext, int dwReserved) {
                GlobalLog.Print("AutoWebProxyScriptWrapper.ScriptHost#" + ValidationHelper.HashString(this) + "::QueryCustomPolicy()");
                ppPolicy = null;
                pcbPolicy = 0;
                return (int) HRESULT.E_NOTIMPL;
            }
        }
    }
}

#else

namespace System.Net 
{
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Runtime.Remoting;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using System.Threading;

    // This interface is useless to users.  We need it to interact with our Microsoft.JScript helper class.
    public interface IWebProxyScript
    {
        bool Load(Uri scriptLocation, string script, Type helperType);
        string Run(string url, string host);
        void Close();
    }

    internal class AutoWebProxyScriptWrapper
    {
        private const string c_appDomainName = "WebProxyScript";

        // The index is required for the hashtable because calling GetHashCode() on an unloaded AppDomain throws.
        private int appDomainIndex;
        private AppDomain scriptDomain;
        private IWebProxyScript site;

        // s_ExcessAppDomain is a holding spot for the most recently created AppDomain.  Until we guarantee it gets
        // into s_AppDomains (or is unloaded), no additional AppDomains can be created, to avoid leaking them.
        private static volatile AppDomain s_ExcessAppDomain;
        private static Hashtable s_AppDomains = new Hashtable();
        private static bool s_CleanedUp;
        private static int s_NextAppDomainIndex;
        private static AppDomainSetup s_AppDomainInfo;
        private static volatile Type s_ProxyScriptHelperType;
        private static volatile Exception s_ProxyScriptHelperLoadError;
        private static object s_ProxyScriptHelperLock = new object();

        static AutoWebProxyScriptWrapper()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.TypeInformation)]
        internal AutoWebProxyScriptWrapper()
        {
            GlobalLog.Print("AutoWebProxyScriptWrapper::.ctor() Creating AppDomain: " + c_appDomainName);

            Exception exception = null;
            if (s_ProxyScriptHelperLoadError == null && s_ProxyScriptHelperType == null)
            {
                lock (s_ProxyScriptHelperLock)
                {
                    if (s_ProxyScriptHelperLoadError == null && s_ProxyScriptHelperType == null)
                    {
                        // Try to load the type late-bound out of Microsoft.JScript.
                        try
                        {
                            s_ProxyScriptHelperType = Type.GetType("System.Net.VsaWebProxyScript, Microsoft.JScript, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }

                        if (s_ProxyScriptHelperType == null)
                        {
                            s_ProxyScriptHelperLoadError = exception == null ? new InternalException() : exception;
                        }
                    }
                }
            }

            if (s_ProxyScriptHelperLoadError != null)
            {
                throw new TypeLoadException(SR.GetString(SR.net_cannot_load_proxy_helper), s_ProxyScriptHelperLoadError is InternalException ? null : s_ProxyScriptHelperLoadError);
            }

            CreateAppDomain();

            exception = null;
            try
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper::CreateInstance() Creating Object. type.Assembly.FullName: [" + s_ProxyScriptHelperType.Assembly.FullName + "] type.FullName: [" + s_ProxyScriptHelperType.FullName + "]");
                ObjectHandle handle = Activator.CreateInstance(scriptDomain, s_ProxyScriptHelperType.Assembly.FullName, s_ProxyScriptHelperType.FullName, false,
                    BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null, null, null, null, null);
                if (handle != null)
                {
                    site = (IWebProxyScript) handle.Unwrap();
                }
                GlobalLog.Print("AutoWebProxyScriptWrapper::CreateInstance() Create script site:" + ValidationHelper.HashString(site));
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (site == null)
            {
                lock (s_ProxyScriptHelperLock)
                {
                    if (s_ProxyScriptHelperLoadError == null)
                    {
                        s_ProxyScriptHelperLoadError = exception == null ? new InternalException() : exception;
                    }
                }
                throw new TypeLoadException(SR.GetString(SR.net_cannot_load_proxy_helper), s_ProxyScriptHelperLoadError is InternalException ? null : s_ProxyScriptHelperLoadError);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private void CreateAppDomain()
        {
            // Locking s_AppDomains must happen in a CER so we don't orphan a lock that gets taken by AppDomain.DomainUnload.
            bool lockHeld = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_AppDomains.SyncRoot, ref lockHeld);

                if (s_CleanedUp)
                {
                    throw new InvalidOperationException(SR.GetString(SR.net_cant_perform_during_shutdown));
                }

                // Create singleton.
                if (s_AppDomainInfo == null)
                {
                    s_AppDomainInfo = new AppDomainSetup();
                    s_AppDomainInfo.DisallowBindingRedirects = true;
                    s_AppDomainInfo.DisallowCodeDownload = true;

                    NamedPermissionSet perms = new NamedPermissionSet("__WebProxySandbox", PermissionState.None);
                    perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    ApplicationTrust trust = new ApplicationTrust();
                    trust.DefaultGrantSet = new PolicyStatement(perms);
                    s_AppDomainInfo.ApplicationTrust = trust;
                    s_AppDomainInfo.ApplicationBase = Environment.SystemDirectory;
                }

                // If something's already in s_ExcessAppDomain, try to dislodge it again.
                AppDomain excessAppDomain = s_ExcessAppDomain;
                if (excessAppDomain != null)
                {
                    TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(CloseAppDomainCallback), excessAppDomain);
                    throw new InvalidOperationException(SR.GetString(SR.net_cant_create_environment));
                }

                appDomainIndex = s_NextAppDomainIndex++;
                try { }
                finally
                {
                    PermissionSet permissionSet = new PermissionSet(PermissionState.None);
                    permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

                    // 

                    s_ExcessAppDomain = AppDomain.CreateDomain(c_appDomainName, null, s_AppDomainInfo, permissionSet, null);

                    try
                    {
                        s_AppDomains.Add(appDomainIndex, s_ExcessAppDomain);

                        // This indicates to the finally and the finalizer that everything succeeded.
                        scriptDomain = s_ExcessAppDomain;
                    }
                    finally
                    {
                        // ReferenceEquals has a ReliabilityContract.
                        if (object.ReferenceEquals(scriptDomain, s_ExcessAppDomain))
                        {
                            s_ExcessAppDomain = null;
                        }
                        else
                        {
                            // Something failed.  Leave the domain in s_ExcessAppDomain until we can get rid of it.  No
                            // more AppDomains can be created until we do.  In the mean time, keep attempting to get the
                            // TimerThread to remove it.  Also, might as well remove it from the hash if it made it in.
                            try
                            {
                                s_AppDomains.Remove(appDomainIndex);
                            }
                            finally
                            {
                                // Can't call AppDomain.Unload from a user thread (or in a lock).
                                TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(CloseAppDomainCallback), s_ExcessAppDomain);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (lockHeld)
                {
                    Monitor.Exit(s_AppDomains.SyncRoot);
                }
            }
        }

        internal void Close()
        {
            site.Close();

            // Can't call AppDomain.Unload() from a user thread.
            TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(CloseAppDomainCallback), appDomainIndex);
            GC.SuppressFinalize(this);
        }

        // 








        ~AutoWebProxyScriptWrapper()
        {
            if (!NclUtilities.HasShutdownStarted && scriptDomain != null)
            {
                // Can't call AppDomain.Unload() from the finalizer thread.
                TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(CloseAppDomainCallback), appDomainIndex);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private static void CloseAppDomainCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            try
            {
                AppDomain domain = context as AppDomain;
                if (domain == null)
                {
                    CloseAppDomain((int) context);
                }
                else
                {
                    if (object.ReferenceEquals(domain, s_ExcessAppDomain))
                    {
                        try
                        {
                            AppDomain.Unload(domain);
                        }
                        catch (AppDomainUnloadedException) { }
                        s_ExcessAppDomain = null;
                    }
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private static void CloseAppDomain(int index)
        {
            AppDomain appDomain;

            // Locking s_AppDomains must happen in a CER so we don't orphan a lock that gets taken by AppDomain.DomainUnload.
            bool lockHeld = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_AppDomains.SyncRoot, ref lockHeld);

                if (s_CleanedUp)
                {
                    return;
                }
                appDomain = (AppDomain) s_AppDomains[index];
            }
            finally
            {
                if (lockHeld)
                {
                    Monitor.Exit(s_AppDomains.SyncRoot);
                    lockHeld = false;
                }
            }

            try
            {
                // Cannot call Unload() in a lock shared by OnDomainUnload() - deadlock with ADUnload thread.
                // So we may try to unload the same domain twice.
                AppDomain.Unload(appDomain);
            }
            catch (AppDomainUnloadedException) { }
            finally
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(s_AppDomains.SyncRoot, ref lockHeld);
                    s_AppDomains.Remove(index);
                }
                finally
                {
                    if (lockHeld)
                    {
                        Monitor.Exit(s_AppDomains.SyncRoot);
                    }
                }
            }
        }

        [ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private static void OnDomainUnload(object sender, EventArgs e)
        {
            lock (s_AppDomains.SyncRoot)
            {
                if (!s_CleanedUp)
                {
                    s_CleanedUp = true;
                    foreach (AppDomain domain in s_AppDomains.Values)
                    {
                        try
                        {
                            AppDomain.Unload(domain);
                        }
                        catch { }
                    }
                    s_AppDomains.Clear();
                    AppDomain excessAppDomain = s_ExcessAppDomain;
                    if (excessAppDomain != null)
                    {
                        try
                        {
                            AppDomain.Unload(excessAppDomain);
                        }
                        catch { }
                        s_ExcessAppDomain = null;
                    }
                }
            }
        }

        internal string ScriptBody
        {
            get
            {
                return scriptText;
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return scriptBytes;
            }

            set
            {
                scriptBytes = value;
            }
        }

        internal DateTime LastModified
        {
            get
            {
                return lastModified;
            }

            set
            {
                lastModified = value;
            }
        }

        private string scriptText;
        private byte[] scriptBytes;
        private DateTime lastModified;

        // Forward these to the site.
        internal string FindProxyForURL(string url, string host)
        {
            GlobalLog.Print("AutoWebProxyScriptWrapper::FindProxyForURL() Calling JScript for url:" + url.ToString() + " host:" + host.ToString());
            return site.Run(url, host);
        }

        internal bool Compile(Uri engineScriptLocation, string scriptBody, byte[] buffer)
        {
            if (site.Load(engineScriptLocation, scriptBody, typeof(WebProxyScriptHelper)))
            {
                GlobalLog.Print("AutoWebProxyScriptWrapper::Compile() Compilation succeeded for engineScriptLocation:" + engineScriptLocation.ToString());
                scriptText = scriptBody;
                scriptBytes = buffer;
                return true;
            }
            GlobalLog.Print("AutoWebProxyScriptWrapper::Compile() Compilation failed for engineScriptLocation:" + engineScriptLocation.ToString());
            return false;
        }
    }
}
#endif
