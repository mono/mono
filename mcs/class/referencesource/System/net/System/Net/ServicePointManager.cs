//------------------------------------------------------------------------------
// <copyright file="ServicePointManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net.Configuration;
    using System.Net.Sockets;
    using System.Net.Security;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Win32;
    
    // This turned to be a legacy type name that is simply forwarded to System.Security.Authentication.SslProtocols defined values.
#if !FEATURE_PAL
    [Flags]
    public enum SecurityProtocolType
    {
        Ssl3          = System.Security.Authentication.SslProtocols.Ssl3,
        Tls           = System.Security.Authentication.SslProtocols.Tls,
        Tls11         = System.Security.Authentication.SslProtocols.Tls11,
        Tls12         = System.Security.Authentication.SslProtocols.Tls12,
    }

    internal class CertPolicyValidationCallback
    {
        readonly ICertificatePolicy  m_CertificatePolicy;
        readonly ExecutionContext    m_Context;

        internal CertPolicyValidationCallback()
        {
            m_CertificatePolicy = new DefaultCertPolicy();
            m_Context = null;
        }

        internal CertPolicyValidationCallback(ICertificatePolicy certificatePolicy)
        {
            m_CertificatePolicy = certificatePolicy;
            m_Context = ExecutionContext.Capture();
        }

        internal ICertificatePolicy CertificatePolicy {
            get { return m_CertificatePolicy;}
        }

        internal bool UsesDefault {
            get { return m_Context == null;}
        }

        internal void Callback(object state)
        {
            CallbackContext context = (CallbackContext) state;
            context.result = context.policyWrapper.CheckErrors(context.hostName,
                                                               context.certificate,
                                                               context.chain,
                                                               context.sslPolicyErrors);
        }

        internal bool Invoke(string hostName,
                             ServicePoint servicePoint,
                             X509Certificate certificate,
                             WebRequest request,
                             X509Chain chain,
                             SslPolicyErrors sslPolicyErrors)
        {
            PolicyWrapper policyWrapper = new PolicyWrapper(m_CertificatePolicy,
                                                            servicePoint,
                                                            (WebRequest) request);

            if (m_Context == null)
            {
                return policyWrapper.CheckErrors(hostName,
                                                 certificate,
                                                 chain,
                                                 sslPolicyErrors);
            }
            else
            {
                ExecutionContext execContext = m_Context.CreateCopy();
                CallbackContext callbackContext = new CallbackContext(policyWrapper,
                                                                      hostName,
                                                                      certificate,
                                                                      chain,
                                                                      sslPolicyErrors);
                ExecutionContext.Run(execContext, Callback, callbackContext);
                return callbackContext.result;
            }
        }

        private class CallbackContext
        {
            internal CallbackContext(PolicyWrapper policyWrapper,
                                     string hostName,
                                     X509Certificate certificate,
                                     X509Chain chain,
                                     SslPolicyErrors sslPolicyErrors)
            {
                this.policyWrapper = policyWrapper;
                this.hostName = hostName;
                this.certificate = certificate;
                this.chain = chain;
                this.sslPolicyErrors = sslPolicyErrors;
            }

            internal readonly PolicyWrapper   policyWrapper;
            internal readonly string          hostName;
            internal readonly X509Certificate certificate;
            internal readonly X509Chain       chain;
            internal readonly SslPolicyErrors sslPolicyErrors;

            internal bool result;
        }
    }

    internal class ServerCertValidationCallback
    {
        readonly RemoteCertificateValidationCallback m_ValidationCallback;
        readonly ExecutionContext                    m_Context;

        internal ServerCertValidationCallback(RemoteCertificateValidationCallback validationCallback)
        {
            m_ValidationCallback = validationCallback;
            m_Context = ExecutionContext.Capture();
        }

        internal RemoteCertificateValidationCallback ValidationCallback {
            get { return m_ValidationCallback;}
        }

        internal void Callback(object state)
        {
            CallbackContext context = (CallbackContext) state;
            context.result = m_ValidationCallback(context.request,
                                                  context.certificate,
                                                  context.chain,
                                                  context.sslPolicyErrors);
        }

        internal bool Invoke(object request,
                             X509Certificate certificate,
                             X509Chain chain,
                             SslPolicyErrors sslPolicyErrors)
        {
            if (m_Context == null)
            {
                return m_ValidationCallback(request, certificate, chain, sslPolicyErrors);
            }
            else
            {
                ExecutionContext execContext = m_Context.CreateCopy();
                CallbackContext callbackContext = new CallbackContext(request,
                                                                      certificate,
                                                                      chain,
                                                                      sslPolicyErrors);
                ExecutionContext.Run(execContext, Callback, callbackContext);
                return callbackContext.result;
            }
        }

        private class CallbackContext
        {
            internal readonly Object request;
            internal readonly X509Certificate certificate;
            internal readonly X509Chain chain;
            internal readonly SslPolicyErrors sslPolicyErrors;

            internal bool result;

            internal CallbackContext(Object request,
                                     X509Certificate certificate,
                                     X509Chain chain,
                                     SslPolicyErrors sslPolicyErrors)
            {
                this.request = request;
                this.certificate = certificate;
                this.chain = chain;
                this.sslPolicyErrors = sslPolicyErrors;
            }
        }
    }
#endif // !FEATURE_PAL

    //
    // The ServicePointManager class hands out ServicePoints (may exist or be created
    // as needed) and makes sure they are garbage collected when they expire.
    // The ServicePointManager runs in its own thread so that it never
    //

    /// <devdoc>
    /// <para>Manages the collection of <see cref='System.Net.ServicePoint'/> instances.</para>
    /// </devdoc>
    ///
    public class ServicePointManager {

        /// <devdoc>
        ///    <para>
        ///       The number of non-persistent connections allowed on a <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public const int DefaultNonPersistentConnectionLimit = 4;
        /// <devdoc>
        ///    <para>
        ///       The default number of persistent connections allowed on a <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public const int DefaultPersistentConnectionLimit = 2;

        /// <devdoc>
        ///    <para>
        ///       The default number of persistent connections when running under ASP+.
        ///    </para>
        /// </devdoc>
        private const int DefaultAspPersistentConnectionLimit = 10;


        internal static readonly string SpecialConnectGroupName = "/.NET/NetClasses/HttpWebRequest/CONNECT__Group$$/";
        internal static readonly TimerThread.Callback s_IdleServicePointTimeoutDelegate = new TimerThread.Callback(IdleServicePointTimeoutCallback);

        //
        // data  - only statics used
        //

        //
        // s_ServicePointTable - Uri of ServicePoint is the hash key
        // We provide our own comparer function that knows about Uris
        //

        //also used as a lock object
        private static Hashtable s_ServicePointTable = new Hashtable(10);

        // IIS6 has 120 sec for an idle connection timeout, we should have a little bit less.
        private static volatile TimerThread.Queue s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(100 * 1000);
        private static int s_MaxServicePoints = 0;
#if !FEATURE_PAL
        private static volatile CertPolicyValidationCallback s_CertPolicyValidationCallback = new CertPolicyValidationCallback();
        private static volatile ServerCertValidationCallback s_ServerCertValidationCallback = null;

        private const string strongCryptoKeyUnversioned = @"SOFTWARE\Microsoft\.NETFramework";
        private const string strongCryptoKeyVersionedPattern = strongCryptoKeyUnversioned + @"\v{0}";
        private const string strongCryptoKeyPath = @"HKEY_LOCAL_MACHINE\" + strongCryptoKeyUnversioned;
        private const string strongCryptoValueName = "SchUseStrongCrypto";
        private static string secureProtocolAppSetting = "System.Net.ServicePointManager.SecurityProtocol";

        private static object disableStrongCryptoLock = new object();
        private static volatile bool disableStrongCryptoInitialized = false;
        private static bool disableStrongCrypto = false;

        private static SecurityProtocolType s_SecurityProtocolType = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
#endif // !FEATURE_PAL
        private static volatile Hashtable s_ConfigTable = null;
        private static volatile int s_ConnectionLimit = PersistentConnectionLimit;

        internal static volatile bool s_UseTcpKeepAlive = false;
        internal static volatile int s_TcpKeepAliveTime;
        internal static volatile int s_TcpKeepAliveInterval;

        //
        // InternalConnectionLimit -
        //  set/get Connection Limit on demand, checking config beforehand
        //

        private static volatile bool s_UserChangedLimit;
        private static int InternalConnectionLimit {
            get {
                if (s_ConfigTable == null) {
                    // init config
                    s_ConfigTable = ConfigTable;
                }
                return s_ConnectionLimit;
            }
            set {
                if (s_ConfigTable == null) {
                    // init config
                    s_ConfigTable = ConfigTable;
                }
                s_UserChangedLimit = true;
                s_ConnectionLimit = value;
            }
        }

        //
        // PersistentConnectionLimit -
        //  Determines the correct connection limit based on whether with running with ASP+
        //  The following order is followed generally for figuring what ConnectionLimit size to use
        //    1.    If ServicePoint.ConnectionLimit is set, then take that value
        //    2.    If ServicePoint has a specific config setting, then take that value
        //    3.    If ServicePoint.DefaultConnectionLimit is set, then take that value
        //    4.    If ServicePoint is localhost, then set to infinite (TO Should we change this value?)
        //    5.    If ServicePointManager has a default config connection limit setting, then take that value
        //    6.    If ServicePoint is running under ASP+, then set value to 10, else set it to 2
        //
        private static int PersistentConnectionLimit {
            get {
#if !FEATURE_PAL
                if (ComNetOS.IsAspNetServer) {
                    return DefaultAspPersistentConnectionLimit;
                } else
#endif
                {
                    return DefaultPersistentConnectionLimit;
                }
            }
        }

        /* Consider Removing
        //
        // InternalServicePointCount -
        //  Gets the active number of ServicePoints being used
        //
        internal static int InternalServicePointCount {
            get {
                return s_ServicePointTable.Count;
            }
        }
        */

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugMembers(int requestHash) {
            try {
                foreach (WeakReference servicePointReference in  s_ServicePointTable) {
                    ServicePoint servicePoint;
                    if (servicePointReference != null && servicePointReference.IsAlive) {
                        servicePoint = (ServicePoint)servicePointReference.Target;
                    }
                    else {
                        servicePoint = null;
                    }
                    if (servicePoint!=null) {
                        servicePoint.DebugMembers(requestHash);
                    }
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
            }
        }

        //
        // ConfigTable -
        // read ConfigTable from Config, or create
        //  a default on failure
        //

        private static Hashtable ConfigTable {
            get {
                if (s_ConfigTable == null) {
                    lock(s_ServicePointTable) {
                        if (s_ConfigTable == null) {
                            ConnectionManagementSectionInternal configSection 
                                = ConnectionManagementSectionInternal.GetSection();
                            Hashtable configTable = null;
                            if (configSection != null)
                            {
                                configTable = configSection.ConnectionManagement;
                            }

                            if (configTable == null) {
                                configTable = new Hashtable();
                            }

                            // we piggy back loading the ConnectionLimit here
                            if (configTable.ContainsKey("*") ) {
                                int connectionLimit  = (int) configTable["*"];
                                if ( connectionLimit < 1 ) {
                                    connectionLimit = PersistentConnectionLimit;
                                }
                                s_ConnectionLimit = connectionLimit;
                            }
                            s_ConfigTable = configTable;
                        }
                    }
                }
                return s_ConfigTable;
            }
        }


        internal static TimerThread.Callback IdleServicePointTimeoutDelegate
        {
            get
            {
                return s_IdleServicePointTimeoutDelegate;
            }
        }

        private static void IdleServicePointTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ServicePoint servicePoint = (ServicePoint) context;

            if (Logging.On) Logging.PrintInfo(Logging.Web, SR.GetString(SR.net_log_closed_idle,
                "ServicePoint", servicePoint.GetHashCode()));

            lock (s_ServicePointTable)
            {
                s_ServicePointTable.Remove(servicePoint.LookupString);
            }

            servicePoint.ReleaseAllConnectionGroups();
        }


        //
        // constructors
        //

        private ServicePointManager() {
        }

#if !FEATURE_PAL
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
        public static SecurityProtocolType SecurityProtocol {
            get {
                EnsureStrongCryptoSettingsInitialized();
                return s_SecurityProtocolType;
            }
            set {
                EnsureStrongCryptoSettingsInitialized();
                ValidateSecurityProtocol(value);
                s_SecurityProtocolType = value;
            }
        }

        private static void ValidateSecurityProtocol(SecurityProtocolType value)
        {
            // Do not allow Ssl2 (and others) as explicit SSL version request
            SecurityProtocolType allowed = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            if ((value & ~allowed) != 0)
            {
                throw new NotSupportedException(SR.GetString(SR.net_securityprotocolnotsupported));
            }
        }
#endif // !FEATURE_PAL

        //
        // accessors
        //

        /// <devdoc>
        /// <para>Gets or sets the maximum number of <see cref='System.Net.ServicePoint'/> instances that should be maintained at any
        ///    time.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
        public static int MaxServicePoints {
            get {
                return s_MaxServicePoints;
            }
            set {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (!ValidationHelper.ValidateRange(value, 0, Int32.MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                s_MaxServicePoints = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static int DefaultConnectionLimit {
            get {
                return InternalConnectionLimit;
            }
            set {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (value > 0) {
                    InternalConnectionLimit = value;

                }
                else {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_toosmall));
                }
            }
        }



        /// <devdoc>
        /// <para>Gets or sets the maximum idle time in seconds of a <see cref='System.Net.ServicePoint'/>.</para>
        /// </devdoc>
        public static int MaxServicePointIdleTime {
            get {
                return s_ServicePointIdlingQueue.Duration;
            }
            set {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if ( !ValidationHelper.ValidateRange(value, Timeout.Infinite, Int32.MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (s_ServicePointIdlingQueue.Duration != value)
                {
                    s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(value);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets indication whether use of the Nagling algorithm is desired.
        ///       Changing this value does not affect existing <see cref='System.Net.ServicePoint'/> instances but only to new ones that are created from that moment on.
        ///    </para>
        /// </devdoc>
        public static bool UseNagleAlgorithm {
            get {
                return SettingsSectionInternal.Section.UseNagleAlgorithm;
            }
            set {
                SettingsSectionInternal.Section.UseNagleAlgorithm = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets indication whether 100-continue behaviour is desired.
        ///       Changing this value does not affect existing <see cref='System.Net.ServicePoint'/> instances but only to new ones that are created from that moment on.
        ///    </para>
        /// </devdoc>
        public static bool Expect100Continue {
            get {
                return SettingsSectionInternal.Section.Expect100Continue;
            }
            set {
                SettingsSectionInternal.Section.Expect100Continue = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///         Enables the use of DNS round robin access, meaning a different IP
        ///         address may be used on each connection, when more than one IP is availble
        ///    </para>
        /// </devdoc>
        public static bool EnableDnsRoundRobin {
            get {
                return SettingsSectionInternal.Section.EnableDnsRoundRobin;
            }
            set {
                SettingsSectionInternal.Section.EnableDnsRoundRobin = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Causes us to go back and reresolve addresses through DNS, even when
        ///       there were no recorded failures.  -1 is infinite.  Time should be in ms
        ///    </para>
        /// </devdoc>
        public static int DnsRefreshTimeout {
            get {
                return SettingsSectionInternal.Section.DnsRefreshTimeout;
            }
            set {
                if(value < -1){
                    SettingsSectionInternal.Section.DnsRefreshTimeout = -1;
                }
                else{
                    SettingsSectionInternal.Section.DnsRefreshTimeout = value;
                }
            }
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Defines the s_Policy for how to deal with server certificates.
        ///    </para>
        /// </devdoc>


        [Obsolete("CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static ICertificatePolicy CertificatePolicy {
            get {
                return GetLegacyCertificatePolicy();
            }
            set {
                //Prevent for an applet to override default Certificate Policy
                ExceptionHelper.UnmanagedPermission.Demand();
                s_CertPolicyValidationCallback = new CertPolicyValidationCallback(value);
            }
        }

        internal static ICertificatePolicy GetLegacyCertificatePolicy(){
            if (s_CertPolicyValidationCallback == null)
                return null;
            else
                return s_CertPolicyValidationCallback.CertificatePolicy;
        }

        internal static CertPolicyValidationCallback CertPolicyValidationCallback {
            get {
                return s_CertPolicyValidationCallback;
            }
        }


        public static RemoteCertificateValidationCallback ServerCertificateValidationCallback {
            get {
                if (s_ServerCertValidationCallback == null)
                    return null;
                else
                    return s_ServerCertValidationCallback.ValidationCallback;
            }
            set {
                // Prevent an applet from overriding the default Certificate Policy
                ExceptionHelper.InfrastructurePermission.Demand();
                if (value == null)
                {
                    s_ServerCertValidationCallback = null;
                }
                else
                {
                    s_ServerCertValidationCallback = new ServerCertValidationCallback(value);
                }
            }
        }

        internal static ServerCertValidationCallback ServerCertValidationCallback {
            get {
                return s_ServerCertValidationCallback;
            }
        }

        internal static bool DisableStrongCrypto {
            get {
                EnsureStrongCryptoSettingsInitialized();
                return (bool)disableStrongCrypto; 
            }
        }

        [RegistryPermission(SecurityAction.Assert, Read = strongCryptoKeyPath)]
        private static void EnsureStrongCryptoSettingsInitialized() {
            
            if (disableStrongCryptoInitialized) {
                return;
            }

            lock (disableStrongCryptoLock) {
                if (disableStrongCryptoInitialized) {
                    return;
                }

                bool disableStrongCryptoInternal = true;

                try {
                    string strongCryptoKey = String.Format(CultureInfo.InvariantCulture, strongCryptoKeyVersionedPattern, Environment.Version.ToString(3));

                    // We read reflected keys on WOW64.
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(strongCryptoKey)) {
                        try {
                            object schUseStrongCryptoKeyValue =
                                key.GetValue(strongCryptoValueName, null);

                            // Setting the value to 1 will enable the MSRC behavior. 
                            // All other values are ignored.
                            if ((schUseStrongCryptoKeyValue != null)
                                && (key.GetValueKind(strongCryptoValueName) == RegistryValueKind.DWord)) {
                                disableStrongCryptoInternal = ((int)schUseStrongCryptoKeyValue) != 1;
                            }
                        }
                        catch (UnauthorizedAccessException) { }
                        catch (IOException) { }
                    }
                }
                catch (SecurityException) { }
                catch (ObjectDisposedException) { }

                if (disableStrongCryptoInternal) {
                    // Revert the SecurityProtocol selection to the legacy combination.
                    s_SecurityProtocolType = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                }
                else {
                    s_SecurityProtocolType = 
                        SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    // Attempt to read this from the AppSettings config section
                    string appSetting = ConfigurationManager.AppSettings[secureProtocolAppSetting];
                    SecurityProtocolType value;
                    try {
                        value = (SecurityProtocolType)Enum.Parse(typeof(SecurityProtocolType), appSetting);
                        ValidateSecurityProtocol(value);
                        s_SecurityProtocolType = value;
                    }
                    catch (ArgumentNullException) { }
                    catch (ArgumentException) { }
                    catch (NotSupportedException) { }
                }

                disableStrongCrypto = disableStrongCryptoInternal;
                disableStrongCryptoInitialized = true;
            }
        }
#endif // !FEATURE_PAL

        public static bool CheckCertificateRevocationList {
            get {
                return SettingsSectionInternal.Section.CheckCertificateRevocationList;
            }
            set {
                //Prevent an applet to override default certificate checking
                ExceptionHelper.UnmanagedPermission.Demand();
                SettingsSectionInternal.Section.CheckCertificateRevocationList = value;
            }
        }

        public static EncryptionPolicy EncryptionPolicy {
            get {
                return SettingsSectionInternal.Section.EncryptionPolicy;
            }
        }
        
        internal static bool CheckCertificateName {
            get {
                return SettingsSectionInternal.Section.CheckCertificateName;
            }
        }


        //
        // class methods
        //

        //
        // MakeQueryString - Just a short macro to handle creating the query
        //  string that we search for host ports in the host list
        //
        internal static string MakeQueryString(Uri address) {
            if (address.IsDefaultPort)
                return address.Scheme + "://" + address.DnsSafeHost;
            else
                return address.Scheme + "://" + address.DnsSafeHost + ":" + address.Port.ToString();
        }

        internal static string MakeQueryString(Uri address1, bool isProxy) {
           if (isProxy) {
               return MakeQueryString(address1) + "://proxy";
           }
           else {
               return MakeQueryString(address1);
           }
        }

        //
        // FindServicePoint - Query using an Uri string for a given ServerPoint Object
        //

        /// <devdoc>
        /// <para>Finds an existing <see cref='System.Net.ServicePoint'/> or creates a new <see cref='System.Net.ServicePoint'/> to manage communications to the
        ///    specified Uniform Resource Identifier.</para>
        /// </devdoc>
        public static ServicePoint FindServicePoint(Uri address) {
            return FindServicePoint(address, null);
        }


        /// <devdoc>
        /// <para>Finds an existing <see cref='System.Net.ServicePoint'/> or creates a new <see cref='System.Net.ServicePoint'/> to manage communications to the
        ///    specified Uniform Resource Identifier.</para>
        /// </devdoc>
        public static ServicePoint FindServicePoint(string uriString, IWebProxy proxy) {
            Uri uri = new Uri(uriString);
            return FindServicePoint(uri, proxy);
        }


        //
        // FindServicePoint - Query using an Uri for a given server point
        //

        /// <devdoc>
        /// <para>Findes an existing <see cref='System.Net.ServicePoint'/> or creates a new <see cref='System.Net.ServicePoint'/> to manage communications to the specified <see cref='System.Uri'/>
        /// instance.</para>
        /// </devdoc>
        public static ServicePoint FindServicePoint(Uri address, IWebProxy proxy) {
            ProxyChain chain;
            HttpAbortDelegate abortDelegate = null;
            int abortState = 0;
            return FindServicePoint(address, proxy, out chain, ref abortDelegate, ref abortState);
        }

        // If abortState becomes non-zero, the attempt to find a service point has been aborted.
        internal static ServicePoint FindServicePoint(Uri address, IWebProxy proxy, out ProxyChain chain, ref HttpAbortDelegate abortDelegate, ref int abortState)
        {
            if (address==null) {
                throw new ArgumentNullException("address");
            }
            GlobalLog.Enter("ServicePointManager::FindServicePoint() address:" + address.ToString());

            bool isProxyServicePoint = false;
            chain = null;

            //
            // find proxy info, and then switch on proxy
            //
            Uri proxyAddress = null;
            if (proxy!=null  && !address.IsLoopback) {
                IAutoWebProxy autoProxy = proxy as IAutoWebProxy;
                if (autoProxy != null)
                {
                    chain = autoProxy.GetProxies(address);

                    // Set up our ability to abort this MoveNext call.  Note that the current implementations of ProxyChain will only
                    // take time on the first call, so this is the only place we do this.  If a new ProxyChain takes time in later
                    // calls, this logic should be copied to other places MoveNext is called.
                    GlobalLog.Assert(abortDelegate == null, "ServicePointManager::FindServicePoint()|AbortDelegate already set.");
                    abortDelegate = chain.HttpAbortDelegate;
                    try
                    {
                        Thread.MemoryBarrier();
                        if (abortState != 0)
                        {
                            Exception exception = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                            GlobalLog.LeaveException("ServicePointManager::FindServicePoint() Request aborted before proxy lookup.", exception);
                            throw exception;
                        }

                        if (!chain.Enumerator.MoveNext())
                        {
                            GlobalLog.Assert("ServicePointManager::FindServicePoint()|GetProxies() returned zero proxies.");
/*
                            Exception exception = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.RequestProhibitedByProxy), WebExceptionStatus.RequestProhibitedByProxy);
                            GlobalLog.LeaveException("ServicePointManager::FindServicePoint() Proxy prevented request.", exception);
                            throw exception;
*/
                        }
                        proxyAddress = chain.Enumerator.Current;
                    }
                    finally
                    {
                        abortDelegate = null;
                    }
                }
                else if (!proxy.IsBypassed(address))
                {
                    // use proxy support
                    // rework address
                    proxyAddress = proxy.GetProxy(address);
                }

                // null means DIRECT
                if (proxyAddress!=null) {
                    address = proxyAddress;
                    isProxyServicePoint = true;
                }
            }

            ServicePoint servicePoint = FindServicePointHelper(address, isProxyServicePoint);
            GlobalLog.Leave("ServicePointManager::FindServicePoint() servicePoint#" + ValidationHelper.HashString(servicePoint));
            return servicePoint;
        }

        // Returns null if we get to the end of the chain.
        internal static ServicePoint FindServicePoint(ProxyChain chain)
        {
            GlobalLog.Print("ServicePointManager::FindServicePoint() Calling chained version.");
            if (!chain.Enumerator.MoveNext())
            {
                return null;
            }

            Uri proxyAddress = chain.Enumerator.Current;
            return FindServicePointHelper(proxyAddress == null ? chain.Destination : proxyAddress, proxyAddress != null);
        }

        private static ServicePoint FindServicePointHelper(Uri address, bool isProxyServicePoint)
        {
            GlobalLog.Enter("ServicePointManager::FindServicePointHelper() address:" + address.ToString());

            if (isProxyServicePoint)
            {
                if (address.Scheme != Uri.UriSchemeHttp)
                {
                    // <



                    Exception exception = new NotSupportedException(SR.GetString(SR.net_proxyschemenotsupported, address.Scheme));
                    GlobalLog.LeaveException("ServicePointManager::FindServicePointHelper() proxy has unsupported scheme:" + address.Scheme.ToString(), exception);
                    throw exception;
                }
            }

            //
            // Search for the correct proxy host,
            //  then match its acutal host by using ConnectionGroups
            //  which are located on the actual ServicePoint.
            //
            string tempEntry = MakeQueryString(address, isProxyServicePoint);

            // lookup service point in the table
            ServicePoint servicePoint = null;
            GlobalLog.Print("ServicePointManager::FindServicePointHelper() locking and looking up tempEntry:[" + tempEntry.ToString() + "]");
            lock (s_ServicePointTable) {
                // once we grab the lock, check if it wasn't already added
                WeakReference servicePointReference =  s_ServicePointTable[tempEntry] as WeakReference;
                GlobalLog.Print("ServicePointManager::FindServicePointHelper() lookup returned WeakReference#" + ValidationHelper.HashString(servicePointReference));
                if ( servicePointReference != null ) {
                    servicePoint = (ServicePoint)servicePointReference.Target;
                    GlobalLog.Print("ServicePointManager::FindServicePointHelper() successful lookup returned ServicePoint#" + ValidationHelper.HashString(servicePoint));
                }
                if (servicePoint==null) {
                    // lookup failure or timeout, we need to create a new ServicePoint
                    if (s_MaxServicePoints<=0 || s_ServicePointTable.Count<s_MaxServicePoints) {
                        // Determine Connection Limit
                        int connectionLimit = InternalConnectionLimit;
                        string schemeHostPort = MakeQueryString(address);
                        bool userDefined = s_UserChangedLimit;
                        if (ConfigTable.ContainsKey(schemeHostPort) ) {
                            connectionLimit = (int) ConfigTable[schemeHostPort];
                            userDefined = true;
                        }
                        servicePoint = new ServicePoint(address, s_ServicePointIdlingQueue, connectionLimit, tempEntry, userDefined, isProxyServicePoint);
                        GlobalLog.Print("ServicePointManager::FindServicePointHelper() created ServicePoint#" + ValidationHelper.HashString(servicePoint));
                        servicePointReference = new WeakReference(servicePoint);
                        s_ServicePointTable[tempEntry] = servicePointReference;
                        GlobalLog.Print("ServicePointManager::FindServicePointHelper() adding entry WeakReference#" + ValidationHelper.HashString(servicePointReference) + " key:[" + tempEntry + "]");
                    }
                    else {
                        Exception exception = new InvalidOperationException(SR.GetString(SR.net_maxsrvpoints));
                        GlobalLog.LeaveException("ServicePointManager::FindServicePointHelper() reached the limit count:" + s_ServicePointTable.Count.ToString() + " limit:" + s_MaxServicePoints.ToString(), exception);
                        throw exception;
                    }
                }
            }

            GlobalLog.Leave("ServicePointManager::FindServicePointHelper() servicePoint#" + ValidationHelper.HashString(servicePoint));
            return servicePoint;
        }

        //
        // FindServicePoint - Query using an Uri for a given server point
        //

        /// <devdoc>
        /// <para>Findes an existing <see cref='System.Net.ServicePoint'/> or creates a new <see cref='System.Net.ServicePoint'/> to manage communications to the specified <see cref='System.Uri'/>
        /// instance.</para>
        /// </devdoc>
        internal static ServicePoint FindServicePoint(string host, int port) {
            if (host==null) {
                throw new ArgumentNullException("address");
            }
            GlobalLog.Enter("ServicePointManager::FindServicePoint() host:" + host.ToString());

            string tempEntry = null;
            bool isProxyServicePoint = false;


            //
            // Search for the correct proxy host,
            //  then match its acutal host by using ConnectionGroups
            //  which are located on the actual ServicePoint.
            //
            tempEntry = "ByHost:"+host+":"+port.ToString(CultureInfo.InvariantCulture);
            // lookup service point in the table
            ServicePoint servicePoint = null;
            GlobalLog.Print("ServicePointManager::FindServicePoint() locking and looking up tempEntry:[" + tempEntry.ToString() + "]");
            lock (s_ServicePointTable) {
                // once we grab the lock, check if it wasn't already added
                WeakReference servicePointReference =  s_ServicePointTable[tempEntry] as WeakReference;
                GlobalLog.Print("ServicePointManager::FindServicePoint() lookup returned WeakReference#" + ValidationHelper.HashString(servicePointReference));
                if ( servicePointReference != null ) {
                    servicePoint = (ServicePoint)servicePointReference.Target;
                    GlobalLog.Print("ServicePointManager::FindServicePoint() successfull lookup returned ServicePoint#" + ValidationHelper.HashString(servicePoint));
                }
                if (servicePoint==null) {
                    // lookup failure or timeout, we need to create a new ServicePoint
                    if (s_MaxServicePoints<=0 || s_ServicePointTable.Count<s_MaxServicePoints) {
                        // Determine Connection Limit
                        int connectionLimit = InternalConnectionLimit;
                        bool userDefined = s_UserChangedLimit;
                        string schemeHostPort =host+":"+port.ToString(CultureInfo.InvariantCulture);

                        if (ConfigTable.ContainsKey(schemeHostPort) ) {
                            connectionLimit = (int) ConfigTable[schemeHostPort];
                            userDefined = true;
                        }
                        servicePoint = new ServicePoint(host, port, s_ServicePointIdlingQueue, connectionLimit, tempEntry, userDefined, isProxyServicePoint);
                        GlobalLog.Print("ServicePointManager::FindServicePoint() created ServicePoint#" + ValidationHelper.HashString(servicePoint));
                        servicePointReference = new WeakReference(servicePoint);
                        s_ServicePointTable[tempEntry] = servicePointReference;
                        GlobalLog.Print("ServicePointManager::FindServicePoint() adding entry WeakReference#" + ValidationHelper.HashString(servicePointReference) + " key:[" + tempEntry + "]");
                    }
                    else {
                        Exception exception = new InvalidOperationException(SR.GetString(SR.net_maxsrvpoints));
                        GlobalLog.LeaveException("ServicePointManager::FindServicePoint() reached the limit count:" + s_ServicePointTable.Count.ToString() + " limit:" + s_MaxServicePoints.ToString(), exception);
                        throw exception;
                    }
                }
            }

            GlobalLog.Leave("ServicePointManager::FindServicePoint() servicePoint#" + ValidationHelper.HashString(servicePoint));
            return servicePoint;
        }

        [FriendAccessAllowed]
        internal static void CloseConnectionGroups(string connectionGroupName) {
            // This method iterates through all service points and closes connection groups with the provided name.
            ServicePoint servicePoint = null;
            lock (s_ServicePointTable) {
                foreach (DictionaryEntry item in s_ServicePointTable) {
                    WeakReference servicePointReference = item.Value as WeakReference;
                    if (servicePointReference != null) {
                        servicePoint = (ServicePoint)servicePointReference.Target;                    
                        if (servicePoint != null) {
                            // We found a service point. Ask the service point to close all internal connection groups 
                            // with name 'connectionGroupName'.
                            servicePoint.CloseConnectionGroupInternal(connectionGroupName);
                        }
                    }
                }
            }
        }

        //
        // SetTcpKeepAlive 
        //
        // Enable/Disable the use of TCP keepalive option on ServicePoint
        // connections. This method does not affect existing ServicePoints.
        // When a ServicePoint is constructed it will inherit the current 
        // settings.
        //
        // Parameters:
        //
        // enabled - if true enables the use of the TCP keepalive option 
        // for ServicePoint connections.
        //
        // keepAliveTime - specifies the timeout, in milliseconds, with no
        // activity until the first keep-alive packet is sent. Ignored if 
        // enabled parameter is false.
        //
        // keepAliveInterval - specifies the interval, in milliseconds, between
        // when successive keep-alive packets are sent if no acknowledgement is
        // received. Ignored if enabled parameter is false.
        //
        public static void SetTcpKeepAlive(
                            bool enabled, 
                            int keepAliveTime, 
                            int keepAliveInterval) {
        
            GlobalLog.Enter(
                "ServicePointManager::SetTcpKeepAlive()" + 
                " enabled: " + enabled.ToString() +
                " keepAliveTime: " + keepAliveTime.ToString() +
                " keepAliveInterval: " + keepAliveInterval.ToString()
            );
            if (enabled) {
                s_UseTcpKeepAlive = true;
                if (keepAliveTime <= 0) {
                    throw new ArgumentOutOfRangeException("keepAliveTime");
                }
                if (keepAliveInterval <= 0) {
                    throw new ArgumentOutOfRangeException("keepAliveInterval");
                }
                s_TcpKeepAliveTime = keepAliveTime;
                s_TcpKeepAliveInterval = keepAliveInterval;
            } else {
                s_UseTcpKeepAlive = false;
                s_TcpKeepAliveTime = 0;
                s_TcpKeepAliveInterval =0;
            }
            GlobalLog.Leave("ServicePointManager::SetTcpKeepAlive()");
        }
    }
}
