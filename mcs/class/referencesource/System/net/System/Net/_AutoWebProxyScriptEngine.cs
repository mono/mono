//------------------------------------------------------------------------------
// <copyright fieldInfole="_AutoWebProxyScriptEngine.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Text;
    using System.Net.Cache;
#if !FEATURE_PAL
    using System.Net.NetworkInformation;
    using System.Security.Principal;
#endif
    using System.Globalization;
    using System.Net.Configuration;
    using System.Security.Permissions;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    // This class (and its helper classes implementing IWebRequestFinder interface) are responsible for
    // determining the location of the PAC file, download and execute it, in order to retrieve proxy 
    // information.
    // This class also monitors the Registry and re-reads proxy configuration settings, if the corresponding
    // Registry values change.
    internal class AutoWebProxyScriptEngine
    {
        private bool automaticallyDetectSettings;
        private Uri automaticConfigurationScript;

        private WebProxy webProxy;
        private IWebProxyFinder webProxyFinder;

        // Used by abortable lock.
        private bool m_LockHeld;

        private bool m_UseRegistry;

#if !FEATURE_PAL
        // Used to get notifications of network changes and do AutoDetection (which are global).
        private int m_NetworkChangeStatus;
        private AutoDetector m_AutoDetector;

        // This has to hold on to the creating user's registry hive and impersonation context.
        private SafeRegistryHandle hkcu;
        private WindowsIdentity m_Identity;
#endif // !FEATURE_PAL

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        internal AutoWebProxyScriptEngine(WebProxy proxy, bool useRegistry)
        {
            GlobalLog.Assert(proxy != null, "'proxy' must be assigned.");
            webProxy = proxy;
            m_UseRegistry = useRegistry;

#if !FEATURE_PAL
            m_AutoDetector = AutoDetector.CurrentAutoDetector;
            m_NetworkChangeStatus = m_AutoDetector.NetworkChangeStatus;

            SafeRegistryHandle.RegOpenCurrentUser(UnsafeNclNativeMethods.RegistryHelper.KEY_READ, out hkcu);
            if (m_UseRegistry)
            {
                ListenForRegistry();

                // Keep track of the identity we used to read the registry, in case we need to read it again later.
                m_Identity = WindowsIdentity.GetCurrent();
            }

#endif // !FEATURE_PAL

            // In Win2003 winhttp added a Windows Service handling the auto-proxy discovery. In XP using winhttp
            // APIs will load, compile and execute the wpad file in-process. This will also load COM, since
            // WinHttp requires COM to compile the file. For these reasons, we don't use WinHttp on XP, but
            // only on newer OS versions where the "WinHTTP Web Proxy Auto-Discovery Service" exists.
            webProxyFinder = new HybridWebProxyFinder(this);
        }

        // AutoWebProxyScriptEngine has special abortable locking.  No one should ever lock (this) except the locking helper methods below.
        private static class SyncStatus
        {
            internal const int Unlocked = 0;
            internal const int Locking = 1;
            internal const int LockOwner = 2;
            internal const int AbortedLocked = 3;
            internal const int Aborted = 4;
        }

        private void EnterLock(ref int syncStatus)
        {
            if (syncStatus == SyncStatus.Unlocked)
            {
                lock (this)
                {
                    if (syncStatus != SyncStatus.Aborted)
                    {
                        syncStatus = SyncStatus.Locking;
                        while (true)
                        {
                            if (!m_LockHeld)
                            {
                                syncStatus = SyncStatus.LockOwner;
                                m_LockHeld = true;
                                return;
                            }
                            Monitor.Wait(this);
                            if (syncStatus == SyncStatus.Aborted)
                            {
                                Monitor.Pulse(this);  // This is to ensure that a Pulse meant to let someone take the lock isn't lost.
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void ExitLock(ref int syncStatus)
        {
            if (syncStatus != SyncStatus.Unlocked && syncStatus != SyncStatus.Aborted)
            {
                lock (this)
                {
                    m_LockHeld = false;
                    if (syncStatus == SyncStatus.AbortedLocked)
                    {
                        webProxyFinder.Reset();
                        syncStatus = SyncStatus.Aborted;
                    }
                    else
                    {
                        syncStatus = SyncStatus.Unlocked;
                    }
                    Monitor.Pulse(this);
                }
            }
        }

        internal void Abort(ref int syncStatus)
        {
            lock (this)
            {
                switch (syncStatus)
                {
                    case SyncStatus.Unlocked:
                        syncStatus = SyncStatus.Aborted;
                        break;

                    case SyncStatus.Locking:
                        syncStatus = SyncStatus.Aborted;
                        Monitor.PulseAll(this);
                        break;

                    case SyncStatus.LockOwner:
                        syncStatus = SyncStatus.AbortedLocked;
                        webProxyFinder.Abort();
                        break;
                }
            }
        }
        // End of locking helper methods.


        // The lock is always held while these three are modified.
        internal bool AutomaticallyDetectSettings
        {
            get
            {
                return automaticallyDetectSettings;
            }
            set
            {
                if (automaticallyDetectSettings != value)
                {
                    automaticallyDetectSettings = value;
                    webProxyFinder.Reset();
                }
            }
        }

        internal Uri AutomaticConfigurationScript
        {
            get
            {
                return automaticConfigurationScript;
            }
            set
            {
                if (!object.Equals(automaticConfigurationScript, value))
                {
                    automaticConfigurationScript = value;
                    webProxyFinder.Reset();
                }
            }
        }

        internal ICredentials Credentials
        {
            get
            {
                return webProxy.Credentials;
            }
        }

        internal bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            int syncStatus = SyncStatus.Unlocked;
            return GetProxies(destination, out proxyList, ref syncStatus);
        }

        internal bool GetProxies(Uri destination, out IList<string> proxyList, ref int syncStatus)
        {
            GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::GetProxies()");

            proxyList = null;

#if !FEATURE_PAL
            // See if we need to reinitialize based on registry or other changes.
#if !AUTOPROXY_SKIP_CHECK
            CheckForChanges(ref syncStatus);
#endif
#endif // !FEATURE_PAL

            if (!webProxyFinder.IsValid)
            {
                // This is to improve performance on e.g. home networks, where auto-detect will always
                // fail, but IE settings turn auto-detect ON by default. I.e. in home networks on each
                // call we would try to retrieve the PAC location.
                // The downside of this approach is that if after some time the PAC file can be downloaded,
                // we don't do it. An application restart, changes in the proxy Registry settings, or a
                // connection change (e.g. dial-up/VPN) are required in order to retry to retrieve the PAC file.
                return false;
            }

            // This whole thing has to be locked, both to prevent simultaneous downloading / compilation, and
            // because the script isn't threadsafe.
            try
            {
                EnterLock(ref syncStatus);
                if (syncStatus != SyncStatus.LockOwner)
                {
                    // This is typically because a download got aborted.
                    return false;
                }

                return webProxyFinder.GetProxies(destination, out proxyList);
            }
            finally
            {
                ExitLock(ref syncStatus);
                GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::GetProxies() proxies:" + ValidationHelper.ToString(proxyList));
            }
        }

        internal WebProxyData GetWebProxyData()
        {
            // PS DevDiv 
            WebProxyDataBuilder builder = null;

            if (ComNetOS.IsWin7orLater)
            {
                builder = new WinHttpWebProxyBuilder();
            }
            else
            {
                builder = new RegBlobWebProxyDataBuilder(m_AutoDetector.Connectoid, hkcu);
            }

            return builder.Build();
        }

        internal void Close()
        {
#if !FEATURE_PAL
            // m_AutoDetector is always set up in the constructor, use it to lock
            if (m_AutoDetector != null)
            {
                int syncStatus = SyncStatus.Unlocked;
                try
                {
                    EnterLock(ref syncStatus);
                    GlobalLog.Assert(syncStatus == SyncStatus.LockOwner, "AutoWebProxyScriptEngine#{0}::Close()|Failed to acquire lock.", ValidationHelper.HashString(this));

                    if (m_AutoDetector != null)
                    {
                        registrySuppress = true;
                        if (registryChangeEventPolicy != null)
                        {
                            registryChangeEventPolicy.Close();
                            registryChangeEventPolicy = null;
                        }
                        if (registryChangeEventLM != null)
                        {
                            registryChangeEventLM.Close();
                            registryChangeEventLM = null;
                        }
                        if (registryChangeEvent != null)
                        {
                            registryChangeEvent.Close();
                            registryChangeEvent = null;
                        }

                        if (regKeyPolicy != null && !regKeyPolicy.IsInvalid)
                        {
                            regKeyPolicy.Close();
                        }
                        if (regKeyLM != null && !regKeyLM.IsInvalid)
                        {
                            regKeyLM.Close();
                        }
                        if (regKey != null && !regKey.IsInvalid)
                        {
                            regKey.Close();
                        }

                        if (hkcu != null)
                        {
                            hkcu.RegCloseKey();
                            hkcu = null;
                        }

                        if (m_Identity != null)
                        {
                            m_Identity.Dispose();
                            m_Identity = null;
                        }

                        webProxyFinder.Dispose();

                        m_AutoDetector = null;
                    }
                }
                finally
                {
                    ExitLock(ref syncStatus);
                }
            }
#endif // !FEATURE_PAL
        }

#if !FEATURE_PAL
        private SafeRegistryHandle regKey;
        private SafeRegistryHandle regKeyLM;
        private SafeRegistryHandle regKeyPolicy;
        private AutoResetEvent registryChangeEvent;
        private AutoResetEvent registryChangeEventLM;
        private AutoResetEvent registryChangeEventPolicy;
        private bool registryChangeDeferred;
        private bool registryChangeLMDeferred;
        private bool registryChangePolicyDeferred;
        private bool needRegistryUpdate;
        private bool needConnectoidUpdate;
        private bool registrySuppress;

        internal void ListenForRegistry()
        {
            GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry()");
            if (!registrySuppress)
            {
                if (registryChangeEvent == null)
                {
                    GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() hooking HKCU.");
                    ListenForRegistryHelper(ref regKey, ref registryChangeEvent, IntPtr.Zero,
                        RegBlobWebProxyDataBuilder.ProxyKey);
                }
                if (registryChangeEventLM == null)
                {
                    GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() hooking HKLM.");
                    ListenForRegistryHelper(ref regKeyLM, ref registryChangeEventLM,
                        UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, RegBlobWebProxyDataBuilder.ProxyKey);
                }
                if (registryChangeEventPolicy == null)
                {
                    GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() hooking HKLM/Policies.");
                    ListenForRegistryHelper(ref regKeyPolicy, ref registryChangeEventPolicy,
                        UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, RegBlobWebProxyDataBuilder.PolicyKey);
                }

                // If any succeeded, we should monitor it.
                if (registryChangeEvent == null && registryChangeEventLM == null && registryChangeEventPolicy == null)
                {
                    registrySuppress = true;
                }
            }
        }

        private void ListenForRegistryHelper(ref SafeRegistryHandle key, ref AutoResetEvent changeEvent, IntPtr baseKey, string subKey)
        {
            uint errorCode = 0;

            // First time through?
            if (key == null || key.IsInvalid)
            {
                if (baseKey == IntPtr.Zero)
                {
                    // Impersonation requires extra effort.
                    GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() RegOpenCurrentUser() using hkcu:" + hkcu.DangerousGetHandle().ToString("x"));
                    if (hkcu != null)
                    {
                        errorCode = hkcu.RegOpenKeyEx(subKey, 0, UnsafeNclNativeMethods.RegistryHelper.KEY_READ, out key);
                        GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() RegOpenKeyEx() returned errorCode:" + errorCode + " key:" + key.DangerousGetHandle().ToString("x"));
                    }
                    else
                    {
                        errorCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_NOT_FOUND;
                    }
                }
                else
                {
                    errorCode = SafeRegistryHandle.RegOpenKeyEx(baseKey, subKey, 0, UnsafeNclNativeMethods.RegistryHelper.KEY_READ, out key);
                    //GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() RegOpenKeyEx() returned errorCode:" + errorCode + " key:" + key.DangerousGetHandle().ToString("x"));
                }
                if (errorCode == 0)
                {
                    changeEvent = new AutoResetEvent(false);
                }
            }
            if (errorCode == 0)
            {
                // accessing Handle is protected by a link demand, OK for System.dll
                errorCode = key.RegNotifyChangeKeyValue(true, UnsafeNclNativeMethods.RegistryHelper.REG_NOTIFY_CHANGE_LAST_SET, changeEvent.SafeWaitHandle, true);
                GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() RegNotifyChangeKeyValue() returned errorCode:" + errorCode);
            }
            if (errorCode != 0)
            {
                if (key != null && !key.IsInvalid)
                {
                    try
                    {
                        errorCode = key.RegCloseKey();
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception)) throw;
                    }
                    GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ListenForRegistry() RegCloseKey() returned errorCode:" + errorCode);
                }
                key = null;
                if (changeEvent != null)
                {
                    changeEvent.Close();
                    changeEvent = null;
                }
            }
        }

        private void RegistryChanged()
        {
            GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::RegistryChanged()");
            if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_system_setting_update));

            // always refresh settings because they might have changed
            WebProxyData webProxyData;
            using (m_Identity.Impersonate())
            {
                webProxyData = GetWebProxyData();
            }
            webProxy.Update(webProxyData);
        }

        private void ConnectoidChanged()
        {
            GlobalLog.Print("AutoWebProxyScriptEngine#" + ValidationHelper.HashString(this) + "::ConnectoidChanged()");
            if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_update_due_to_ip_config_change));

            // Get the new connectoid/detector.  Only do this after detecting a change, to avoid ----s with other people detecting changes.
            // (We don't want to end up using a detector/connectoid that doesn't match what we read from the registry.)
            m_AutoDetector = AutoDetector.CurrentAutoDetector;

            if (m_UseRegistry)
            {
                // update the engine and proxy
                WebProxyData webProxyData;
                using (m_Identity.Impersonate())
                {
                    webProxyData = GetWebProxyData();
                }
                webProxy.Update(webProxyData);
            }

            // Always uninitialized if the connectoid/address changed and we are autodetecting.
            if (automaticallyDetectSettings)
                webProxyFinder.Reset();
        }

        internal void CheckForChanges()
        {
            int syncStatus = SyncStatus.Unlocked;
            CheckForChanges(ref syncStatus);
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        private void CheckForChanges(ref int syncStatus)
        {
            // Catch ObjectDisposedException instead of synchronizing with Close().
            try
            {
                bool changed = AutoDetector.CheckForNetworkChanges(ref m_NetworkChangeStatus);
                bool ignoreRegistryChange = false;
                if (changed || needConnectoidUpdate)
                {
                    try
                    {
                        EnterLock(ref syncStatus);
                        if (changed || needConnectoidUpdate)   // Make sure no one else took care of it before we got the lock.
                        {
                            needConnectoidUpdate = syncStatus != SyncStatus.LockOwner;
                            if (!needConnectoidUpdate)
                            {
                                ConnectoidChanged();

                                // We usually get a registry change at the same time.  Since the connectoid change does more,
                                // we can skip reading the registry info twice.
                                ignoreRegistryChange = true;
                            }
                        }
                    }
                    finally
                    {
                        ExitLock(ref syncStatus);
                    }
                }

                if (!m_UseRegistry)
                {
                    return;
                }

                bool forReal = false;
                AutoResetEvent tempEvent = registryChangeEvent;
                if (registryChangeDeferred || (forReal = (tempEvent != null && tempEvent.WaitOne(0, false))))
                {
                    try
                    {
                        EnterLock(ref syncStatus);
                        if (forReal || registryChangeDeferred)  // Check if someone else handled it before I got the lock.
                        {
                            registryChangeDeferred = syncStatus != SyncStatus.LockOwner;
                            if (!registryChangeDeferred && registryChangeEvent != null)
                            {
                                try
                                {
                                    using (m_Identity.Impersonate())
                                    {
                                        ListenForRegistryHelper(ref regKey, ref registryChangeEvent, IntPtr.Zero,
                                            RegBlobWebProxyDataBuilder.ProxyKey);
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                                needRegistryUpdate = true;
                            }
                        }
                    }
                    finally
                    {
                        ExitLock(ref syncStatus);
                    }
                }

                forReal = false;
                tempEvent = registryChangeEventLM;
                if (registryChangeLMDeferred || (forReal = (tempEvent != null && tempEvent.WaitOne(0, false))))
                {
                    try
                    {
                        EnterLock(ref syncStatus);
                        if (forReal || registryChangeLMDeferred)  // Check if someone else handled it before I got the lock.
                        {
                            registryChangeLMDeferred = syncStatus != SyncStatus.LockOwner;
                            if (!registryChangeLMDeferred && registryChangeEventLM != null)
                            {
                                try
                                {
                                    using (m_Identity.Impersonate())
                                    {
                                        ListenForRegistryHelper(ref regKeyLM, ref registryChangeEventLM,
                                            UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE,
                                            RegBlobWebProxyDataBuilder.ProxyKey);
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                                needRegistryUpdate = true;
                            }
                        }
                    }
                    finally
                    {
                        ExitLock(ref syncStatus);
                    }
                }

                forReal = false;
                tempEvent = registryChangeEventPolicy;
                if (registryChangePolicyDeferred || (forReal = (tempEvent != null && tempEvent.WaitOne(0, false))))
                {
                    try
                    {
                        EnterLock(ref syncStatus);
                        if (forReal || registryChangePolicyDeferred)  // Check if someone else handled it before I got the lock.
                        {
                            registryChangePolicyDeferred = syncStatus != SyncStatus.LockOwner;
                            if (!registryChangePolicyDeferred && registryChangeEventPolicy != null)
                            {
                                try
                                {
                                    using (m_Identity.Impersonate())
                                    {
                                        ListenForRegistryHelper(ref regKeyPolicy, ref registryChangeEventPolicy,
                                            UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE,
                                            RegBlobWebProxyDataBuilder.PolicyKey);
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                                needRegistryUpdate = true;
                            }
                        }
                    }
                    finally
                    {
                        ExitLock(ref syncStatus);
                    }
                }

                if (needRegistryUpdate)
                {
                    try
                    {
                        EnterLock(ref syncStatus);
                        if (needRegistryUpdate && syncStatus == SyncStatus.LockOwner)
                        {
                            needRegistryUpdate = false;

                            // We don't need to process this now if we just did it for the connectoid.
                            if (!ignoreRegistryChange)
                            {
                                RegistryChanged();
                            }
                        }
                    }
                    finally
                    {
                        ExitLock(ref syncStatus);
                    }
                }
            }
            catch (ObjectDisposedException) { }
        }

        private class AutoDetector
        {
            private static volatile NetworkAddressChangePolled s_AddressChange;
            private static volatile UnsafeNclNativeMethods.RasHelper s_RasHelper;

            private static int s_CurrentVersion = 0;
            private volatile static AutoDetector s_CurrentAutoDetector;
            private volatile static bool s_Initialized;
            private static object s_LockObject;

            static AutoDetector()
            {
                s_LockObject = new object();
            }

            private static void Initialize()
            {
                if (!s_Initialized)
                {
                    lock (s_LockObject)
                    {
                        if (!s_Initialized)
                        {
                            s_CurrentAutoDetector = new AutoDetector(UnsafeNclNativeMethods.RasHelper.GetCurrentConnectoid(), 1);
                            if (NetworkChange.CanListenForNetworkChanges)
                            {
                                s_AddressChange = new NetworkAddressChangePolled();
                            }
                            if (UnsafeNclNativeMethods.RasHelper.RasSupported)
                            {
                                s_RasHelper = new UnsafeNclNativeMethods.RasHelper();
                            }
                            s_CurrentVersion = 1;
                            s_Initialized = true;
                        }
                    }
                }
            }

            internal static bool CheckForNetworkChanges(ref int changeStatus)
            {
                Initialize();
                CheckForChanges();
                int oldStatus = changeStatus;
                changeStatus = Volatile.Read(ref s_CurrentVersion);
                return oldStatus != changeStatus;
            }

            private static void CheckForChanges()
            {
                bool changed = false;
                if (s_RasHelper != null && s_RasHelper.HasChanged)
                {
                    s_RasHelper.Reset();
                    changed = true;
                }
                if (s_AddressChange != null && s_AddressChange.CheckAndReset())
                {
                    changed = true;
                }
                if (changed)
                {
                    int currentVersion = Interlocked.Increment(ref s_CurrentVersion);
                    s_CurrentAutoDetector = new AutoDetector(UnsafeNclNativeMethods.RasHelper.GetCurrentConnectoid(), currentVersion);
                }
            }

            internal static AutoDetector CurrentAutoDetector
            {
                get
                {
                    Initialize();
                    return s_CurrentAutoDetector;
                }
            }


            private readonly string m_Connectoid;
            private readonly int m_CurrentVersion;

            private AutoDetector(string connectoid, int currentVersion)
            {
                m_Connectoid = connectoid;
                m_CurrentVersion = currentVersion;
            }

            internal string Connectoid
            {
                get
                {
                    return m_Connectoid;
                }
            }

            internal int NetworkChangeStatus
            {
                get
                {
                    return m_CurrentVersion;
                }
            }
        }
#endif
    }
}
