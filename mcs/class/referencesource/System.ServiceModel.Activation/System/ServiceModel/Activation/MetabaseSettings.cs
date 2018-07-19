//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation.Diagnostics;
    using System.Web;
    using Microsoft.Win32;

    abstract class MetabaseSettings
    {
        internal const char AboPathDelimiter = '/';
        internal const string DotDelimiter = ".";
        internal const string LocalMachine = "localhost";

        List<string> enabledProtocols;
        IDictionary<string, string[]> bindingsTable;

        protected MetabaseSettings()
        {
            enabledProtocols = new List<string>();
            bindingsTable = new Dictionary<string, string[]>();
        }

        internal abstract string GetRealm(string virtualPath);
        internal abstract HttpAccessSslFlags GetAccessSslFlags(string virtualPath);
        internal abstract AuthenticationSchemes GetAuthenticationSchemes(string virtualPath);
        internal abstract ExtendedProtectionPolicy GetExtendedProtectionPolicy(string virtualPath);
        internal abstract bool IsWithinApp(string absoluteVirtualPath);

        protected List<string> Protocols { get { return enabledProtocols; } set { enabledProtocols = value; } }
        protected IDictionary<string, string[]> Bindings { get { return bindingsTable; } set { bindingsTable = value; } }

        internal bool GetAllowSslOnly(string virtualPath)
        {
            HttpAccessSslFlags flags = this.GetAccessSslFlags(virtualPath);
            if ((flags & HttpAccessSslFlags.Ssl) != 0)
            {
                return true;
            }
            return false;
        }

        internal string[] GetProtocols()
        {
            return enabledProtocols.ToArray();
        }

        internal string[] GetBindings(string scheme)
        {
            return bindingsTable[scheme];
        }

        // build NCL ExtendedProtectionPolicy object
        // From NCL comments:
        // The NoServiceNameCheck flag can always be ignored because it has no meaning in the .NET Framework 
        // where validation against an SPN list is always required when the scenario does not require a CBT.
        protected static ExtendedProtectionPolicy BuildExtendedProtectionPolicy(
                    ExtendedProtectionTokenChecking tokenChecking,
                    ExtendedProtectionFlags flags,
                    List<string> spnList)
        {
            PolicyEnforcement enforce;
            ProtectionScenario scenario;
            ServiceNameCollection serviceNames = null;

            if (tokenChecking == ExtendedProtectionTokenChecking.None)
            {
                return new ExtendedProtectionPolicy(PolicyEnforcement.Never);
            }
            else if (tokenChecking == ExtendedProtectionTokenChecking.Allow)
            {
                enforce = PolicyEnforcement.WhenSupported;
            }
            else if (tokenChecking == ExtendedProtectionTokenChecking.Require)
            {
                enforce = PolicyEnforcement.Always;
            }
            else
            {
                throw FxTrace.Exception.Argument("tokenChecking", SR.Hosting_UnrecognizedTokenCheckingValue);
            }

            bool transportSelectedCondition1 = (flags == ExtendedProtectionFlags.None);
            bool transportSelectedCondition2 = (flags == ExtendedProtectionFlags.AllowDotlessSpn);
            bool transportSelectedCondition3 = ((flags & ExtendedProtectionFlags.Proxy) != 0) && ((flags & ExtendedProtectionFlags.ProxyCohosting) != 0);
            bool trustedProxyCondition = (flags & ExtendedProtectionFlags.Proxy) != 0;

            //only none or allowdotlessspn flag has been selected or both proxy and proxycohosting flags have been selected 
            //set scenario to TransportSelected
            if (transportSelectedCondition1 || transportSelectedCondition2 || transportSelectedCondition3)
            {
                scenario = ProtectionScenario.TransportSelected;
            }
            // proxy but no procycohosting flag has been selected, set scenario to TrustedProxy
            else if (trustedProxyCondition)
            {
                scenario = ProtectionScenario.TrustedProxy;
            }
            // other nonsupported scenarios, throw NotSupportedException
            else
            {
                throw FxTrace.Exception.Argument("flags", SR.Hosting_ExtendedProtectionFlagsNotSupport(flags));
            }

            // dotless spn check if dotlessspn is not allowed
            // spn format <service class>/<host>:<port>/<service name> per http://msdn.microsoft.com/en-us/library/ms677601(VS.85).aspx
            if (spnList != null)
            {
                if ((flags & ExtendedProtectionFlags.AllowDotlessSpn) == 0)
                {
                    foreach (string spn in spnList)
                    {
                        string[] parts = spn.Split(AboPathDelimiter);
                        if (parts.Length > 1)
                        {
                            int position = parts[1].IndexOf(DotDelimiter, StringComparison.CurrentCultureIgnoreCase);
                            if (position == -1)
                            {
                                throw FxTrace.Exception.Argument("spn", SR.Hosting_ExtendedProtectionDotlessSpnNotEnabled(spn));
                            }
                            else if (position == 0 || position == parts[1].Length - 1)
                            {
                                throw FxTrace.Exception.Argument("spn", SR.Hosting_ExtendedProtectionSpnFormatError(spn));
                            }
                        }
                        else
                        {
                            throw FxTrace.Exception.Argument("spn", SR.Hosting_ExtendedProtectionSpnFormatError(spn));
                        }
                    }
                }
                // ExtendedProtectionPolicy constructor rejects empty collection but accept null
                // in order to avoid any ambiguilty
                if (spnList.Count != 0)
                {
                    serviceNames = new ServiceNameCollection(spnList);
                }
            }
            return new ExtendedProtectionPolicy(enforce, scenario, serviceNames);
        }
    }

    class MetabaseSettingsCassini : MetabaseSettings
    {
        internal MetabaseSettingsCassini(HostedHttpRequestAsyncResult result)
            : base()
        {
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsCassini..ctor() Not a simple application host.");
            }

            // The hostName is hard-coded to "localhost" for Cassini.
            string binding = string.Format(CultureInfo.InvariantCulture, ":{0}:{1}", result.OriginalRequestUri.Port.ToString(NumberFormatInfo.InvariantInfo), MetabaseSettings.LocalMachine);
            this.Bindings.Add(result.OriginalRequestUri.Scheme, new string[] { binding });
            this.Protocols.Add(result.OriginalRequestUri.Scheme);
        }

        internal override string GetRealm(string virtualPath) { return string.Empty; }
        internal override HttpAccessSslFlags GetAccessSslFlags(string virtualPath) { return HttpAccessSslFlags.None; }
        internal override AuthenticationSchemes GetAuthenticationSchemes(string virtualPath)
        {
            // Special casing Cassini so that Ntlm is supported since the request always has the identity of the
            // logged on user.
            return AuthenticationSchemes.Anonymous | AuthenticationSchemes.Ntlm;
        }
        internal override ExtendedProtectionPolicy GetExtendedProtectionPolicy(string virtualPath)
        {   //Alwasy return null since cassini does not support Https
            return null;
        }

        internal override bool IsWithinApp(string absoluteVirtualPath)
        {
            return true;
        }

    }

    abstract class MetabaseSettingsIis : MetabaseSettings
    {
        IDictionary<string, HostedServiceTransportSettings> transportSettingsTable;
        internal const string NegotiateAuthProvider = "negotiate";
        internal const string NtlmAuthProvider = "ntlm";
        internal static string[] DefaultAuthProviders = { NegotiateAuthProvider, NtlmAuthProvider };

        protected MetabaseSettingsIis()
            : base()
        {
            if (ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsIis..ctor() Is a simple application host.");
            }

            transportSettingsTable = new Dictionary<string, HostedServiceTransportSettings>(StringComparer.OrdinalIgnoreCase);
        }

        object ThisLock { get { return this; } }

        protected abstract HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath);

        internal override string GetRealm(string virtualPath)
        {
            HostedServiceTransportSettings transportSettings = GetTransportSettings(virtualPath);
            return transportSettings.Realm;
        }

        internal override HttpAccessSslFlags GetAccessSslFlags(string virtualPath)
        {
            HostedServiceTransportSettings transportSettings = GetTransportSettings(virtualPath);
            return transportSettings.AccessSslFlags;
        }

        internal override AuthenticationSchemes GetAuthenticationSchemes(string virtualPath)
        {
            HostedServiceTransportSettings transportSettings = GetTransportSettings(virtualPath);
            return RemapAuthenticationSchemes(transportSettings.AuthFlags, transportSettings.AuthProviders);
        }

        internal override ExtendedProtectionPolicy GetExtendedProtectionPolicy(string virtualPath)
        {
            HostedServiceTransportSettings transportSettings = GetTransportSettings(virtualPath);
            return transportSettings.IisExtendedProtectionPolicy;
        }

        // IIS and NCL have different enums/values for the various settings
        // therefore we will have to remap.
        AuthenticationSchemes RemapAuthenticationSchemes(AuthFlags flags, string[] providers)
        {
            // The default value for the authetication in IIS is anonymous
            AuthenticationSchemes retValue = AuthenticationSchemes.None;
            if ((flags & AuthFlags.AuthAnonymous) != 0)
            {
                retValue = retValue | AuthenticationSchemes.Anonymous;
            }
            if ((flags & AuthFlags.AuthBasic) != 0)
            {
                retValue = retValue | AuthenticationSchemes.Basic;
            }
            if ((flags & AuthFlags.AuthMD5) != 0)
            {
                retValue = retValue | AuthenticationSchemes.Digest;
            }

            if ((flags & AuthFlags.AuthNTLM) != 0)
            {
                for (int i = 0; i < providers.Length; i++)
                {
                    if (providers[i].StartsWith(NegotiateAuthProvider, StringComparison.OrdinalIgnoreCase))
                    {
                        retValue = retValue | AuthenticationSchemes.Negotiate;
                    }
                    else if (string.Compare(providers[i], NtlmAuthProvider, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        retValue = retValue | AuthenticationSchemes.Ntlm;
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_NotSupportedAuthScheme(providers[i])));
                    }
                }
            }

            if ((flags & AuthFlags.AuthPassport) != 0)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_NotSupportedAuthScheme("Passport")));
            }
            return retValue;
        }

        HostedServiceTransportSettings GetTransportSettings(string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            //Make sure we get relative virtual path.
            string relativeVirtualPath = VirtualPathUtility.ToAppRelative(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);

            HostedServiceTransportSettings transportSettings;

            if (!transportSettingsTable.TryGetValue(relativeVirtualPath, out transportSettings))
            {
                lock (ThisLock)
                {
                    if (!transportSettingsTable.TryGetValue(relativeVirtualPath, out transportSettings))
                    {
                        transportSettings = CreateTransportSettings(relativeVirtualPath);
                        transportSettingsTable.Add(relativeVirtualPath, transportSettings);
                    }
                }
            }

            return transportSettings;
        }

        protected abstract IEnumerable<string> GetSiteApplicationPaths();

        internal override bool IsWithinApp(string absoluteVirtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            string matchedAppPath = this.FindLongestMatchingAppPath(absoluteVirtualPath);
            string curAppPath = VirtualPathUtility.AppendTrailingSlash(HostingEnvironmentWrapper.ApplicationVirtualPath);
            return (string.Compare(matchedAppPath, curAppPath, StringComparison.OrdinalIgnoreCase) == 0);
        }

        string FindLongestMatchingAppPath(string absoluteVirtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            //we need to append slashes at the end for scenarios like this:
            //   /App1 - current app
            //   /App1/App2 - nested app
            //   aboluteVirtualPath = /App1/App2.svc

            string longestMatchedAppPath = null;
            int matchLength = 0;
            IEnumerable<string> appPaths = this.GetSiteApplicationPaths();
            absoluteVirtualPath = VirtualPathUtility.AppendTrailingSlash(absoluteVirtualPath);
            foreach (string appPath in appPaths)
            {
                string childPath = VirtualPathUtility.AppendTrailingSlash(appPath);
                if (absoluteVirtualPath.StartsWith(childPath, StringComparison.OrdinalIgnoreCase)
                   && childPath.Length > matchLength)
                {
                    matchLength = childPath.Length;
                    longestMatchedAppPath = childPath;
                }
            }
            return longestMatchedAppPath;
        }
    }

    class MetabaseSettingsIis6 : MetabaseSettingsIis
    {
        static class IISConstants
        {
            internal const char AboPathDelimiter = '/';
            internal const string LMSegment = "/LM";
            internal const string RootSegment = "/Root";
            internal static char[] CommaSeparator = new char[] { ',' };
            internal const string CBTRegistryHKLMPath = @"System\CurrentControlSet\Services\W3SVC\Parameters\ExtendedProtection";
            internal const string SpnAttributeName = "spns";
            internal const string ExtendedProtectionElementName = "extendedProtection";
            internal const string TokenCheckingAttributeName = "tokenChecking";
            internal const string FlagsAttributeName = "flags";
        }

        [Fx.Tag.SecurityNote(Critical = "potentially protected data read from the IIS metabase under an elevation.")]
        [SecurityCritical]
        string siteAboPath;

        [Fx.Tag.SecurityNote(Critical = "potentially protected data read from the IIS metabase under an elevation.")]
        [SecurityCritical]
        string appAboPath;

        // Application-level settings
        [Fx.Tag.SecurityNote(Critical = "A SecurityCritical field, caller must use care.")]
        [SecurityCritical]
        HostedServiceTransportSettings appTransportSettings;

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use.")]
        [SecuritySafeCritical]
        internal MetabaseSettingsIis6()
            : base()
        {
            if (Iis7Helper.IsIis7)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsIis6 constructor must not be called when running in IIS7");
            }

            SetApplicationInfo();
            using (MetabaseReader reader = new MetabaseReader())
            {
                PopulateSiteProperties(reader);
                PopulateApplicationProperties(reader);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use, returns sanitized values (safe for read).")]
        [SecuritySafeCritical]
        protected override HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath)
        {
            HostedServiceTransportSettings transportSettings = new HostedServiceTransportSettings();
            using (MetabaseReader reader = new MetabaseReader())
            {
                transportSettings.Realm = GetRealm(reader, relativeVirtualPath);
                transportSettings.AccessSslFlags = GetAccessSslFlags(reader, relativeVirtualPath);
                transportSettings.AuthFlags = GetAuthFlags(reader, relativeVirtualPath);
                transportSettings.AuthProviders = GetAuthProviders(reader, relativeVirtualPath);
                if ((transportSettings.AuthFlags & AuthFlags.AuthNTLM) != 0)
                {
                    transportSettings.IisExtendedProtectionPolicy = GetExtendedProtectionPolicy();
                }
            }

            return transportSettings;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use, returns sanitized values (safe for read).")]
        [SecuritySafeCritical]
        string GetRealm(MetabaseReader reader, string relativeVirtualPath)
        {
            object propertyValue = FindPropertyUnderAppRoot(reader, MetabasePropertyType.Realm, relativeVirtualPath);
            if (propertyValue != null)
            {
                return (string)propertyValue;
            }

            return appTransportSettings.Realm;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use, returns sanitized values (safe for read).")]
        [SecuritySafeCritical]
        HttpAccessSslFlags GetAccessSslFlags(MetabaseReader reader, string relativeVirtualPath)
        {
            object propertyValue = FindPropertyUnderAppRoot(reader, MetabasePropertyType.AccessSslFlags, relativeVirtualPath);
            if (propertyValue != null)
            {
                return (HttpAccessSslFlags)(uint)propertyValue;
            }

            return appTransportSettings.AccessSslFlags;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use, returns sanitized values (safe for read).")]
        [SecuritySafeCritical]
        AuthFlags GetAuthFlags(MetabaseReader reader, string relativeVirtualPath)
        {
            object propertyValue = FindPropertyUnderAppRoot(reader, MetabasePropertyType.AuthFlags, relativeVirtualPath);
            if (propertyValue != null)
            {
                return (AuthFlags)(uint)propertyValue;
            }

            return appTransportSettings.AuthFlags;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use, returns sanitized values (safe for read).")]
        [SecuritySafeCritical]
        string[] GetAuthProviders(MetabaseReader reader, string relativeVirtualPath)
        {
            object propertyValue = FindPropertyUnderAppRoot(reader, MetabasePropertyType.AuthProviders, relativeVirtualPath);
            if (propertyValue != null)
            {
                string providersString = (string)propertyValue;
                string[] providers = providersString.Split(IISConstants.CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (providers != null && providers.Length > 0)
                {
                    return providers;
                }
            }

            return appTransportSettings.AuthProviders;
        }

        [Fx.Tag.SecurityNote(Critical = "Asserts registry access to get multiple values from the registry, caller should not leak value.",
            Safe = "No value passed to critical method, discards after use, returns sanitized values (safe for readonly)")]
        [SecuritySafeCritical]
        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + IISConstants.CBTRegistryHKLMPath)]
        ExtendedProtectionPolicy GetExtendedProtectionPolicy()
        {
            ExtendedProtectionPolicy extendedProtection = null;
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(IISConstants.CBTRegistryHKLMPath))
            {
                if (registryKey != null)
                {
                    object tokenCheckingObj = registryKey.GetValue(IISConstants.TokenCheckingAttributeName);
                    object flagsObj = registryKey.GetValue(IISConstants.FlagsAttributeName);
                    object spnsObj = registryKey.GetValue(IISConstants.SpnAttributeName);
                    //using the default one if the registry value is missing
                    ExtendedProtectionTokenChecking tokenChecking = (tokenCheckingObj == null) ?
                        ExtendedProtectionTokenChecking.None : (ExtendedProtectionTokenChecking)tokenCheckingObj;
                    ExtendedProtectionFlags flags = flagsObj == null ?
                        ExtendedProtectionFlags.None : (ExtendedProtectionFlags)flagsObj;
                    List<string> spns = spnsObj == null ? null : new List<string>(spnsObj as string[]);
                    extendedProtection = BuildExtendedProtectionPolicy(tokenChecking, flags, spns);
                }
                else
                {
                    // this IIS6 does not support CBT, log a warning to tracing
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.WebHostNoCBTSupport,
                            SR.TraceCodeWebHostNoCBTSupport, this, (Exception)null);
                    }
                }
            }
            return extendedProtection;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use.")]
        [SecuritySafeCritical]
        void SetApplicationInfo()
        {
            // find the first '/' after the /LM/W3SVC/<site>
            // and get the substring before that
            string applicationID = HostingEnvironmentWrapper.UnsafeApplicationID;
            int index = applicationID.IndexOf(IISConstants.AboPathDelimiter, ServiceHostingEnvironment.ISAPIApplicationIdPrefix.Length);
            siteAboPath = applicationID.Substring(IISConstants.LMSegment.Length, index - IISConstants.LMSegment.Length);

            if (HostingEnvironmentWrapper.ApplicationVirtualPath.Length > 1)
            {
                appAboPath = string.Concat(siteAboPath, IISConstants.RootSegment, HostingEnvironmentWrapper.ApplicationVirtualPath);
            }
            else
            {
                if (HostingEnvironmentWrapper.ApplicationVirtualPath.Length != 1 || HostingEnvironmentWrapper.ApplicationVirtualPath[0] != IISConstants.AboPathDelimiter)
                {
                    throw Fx.AssertAndThrowFatal("ApplicationVirtualPath must be '/'.");
                }
                appAboPath = string.Concat(siteAboPath, IISConstants.RootSegment);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use.")]
        [SecuritySafeCritical]
        void PopulateSiteProperties(MetabaseReader reader)
        {
            // 1. ServerBindings
            object propertyValue = reader.GetData(siteAboPath, MetabasePropertyType.ServerBindings);
            if (propertyValue != null)
            {
                string[] serverBindings = (string[])propertyValue;
                if (serverBindings.Length > 0)
                {
                    this.Bindings.Add(Uri.UriSchemeHttp, serverBindings);
                }
            }

            // 2. SecureBindings
            propertyValue = reader.GetData(siteAboPath, MetabasePropertyType.SecureBindings);
            if (propertyValue != null)
            {
                string[] secureBindings = (string[])propertyValue;
                if (secureBindings.Length > 0)
                {
                    this.Bindings.Add(Uri.UriSchemeHttps, secureBindings);
                }
            }

            foreach (string scheme in this.Bindings.Keys)
            {
                this.Protocols.Add(scheme);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase.",
            Safe = "Only passes MetabaseReader instance to critical methods, discards after use.")]
        [SecuritySafeCritical]
        void PopulateApplicationProperties(MetabaseReader reader)
        {
            int foundCount = 0;
            bool foundRealm = false;
            bool foundAuthFlags = false;
            bool foundAccessSslFlags = !Bindings.ContainsKey(Uri.UriSchemeHttps);
            bool foundAuthProviders = false;

            appTransportSettings = new HostedServiceTransportSettings();

            string endAboPath = appAboPath;
            object propertyValue = null;
            while (foundCount < 4 && endAboPath.Length >= siteAboPath.Length)
            {
                // Realm
                if (!foundRealm && ((propertyValue = reader.GetData(endAboPath, MetabasePropertyType.Realm))
                    != null))
                {
                    appTransportSettings.Realm = (string)propertyValue;
                    foundRealm = true;
                    foundCount++;
                }

                // AuthFlags
                if (!foundAuthFlags && ((propertyValue = reader.GetData(endAboPath, MetabasePropertyType.AuthFlags))
                    != null))
                {
                    appTransportSettings.AuthFlags = (AuthFlags)(uint)propertyValue;
                    foundAuthFlags = true;
                    foundCount++;
                }

                // AccessSslFlags
                if (!foundAccessSslFlags && ((propertyValue = reader.GetData(endAboPath, MetabasePropertyType.AccessSslFlags))
                    != null))
                {
                    appTransportSettings.AccessSslFlags = (HttpAccessSslFlags)(uint)propertyValue;
                    foundAccessSslFlags = true;
                    foundCount++;
                }

                // NTAuthProviders
                if (!foundAuthProviders && ((propertyValue = reader.GetData(endAboPath, MetabasePropertyType.AuthProviders))
                    != null))
                {
                    string providersString = (string)propertyValue;
                    appTransportSettings.AuthProviders = providersString.Split(IISConstants.CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
                    foundAuthProviders = true;
                    foundCount++;
                }

                // Continue the search in the parent path
                int index = endAboPath.LastIndexOf(IISConstants.AboPathDelimiter);
                endAboPath = endAboPath.Substring(0, index);
            }

            if (appTransportSettings.AuthProviders == null || appTransportSettings.AuthProviders.Length == 0)
            {
                appTransportSettings.AuthProviders = DefaultAuthProviders;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase." +
            "Caller must sanitize return value.")]
        [SecurityCritical]
        object FindPropertyUnderAppRoot(MetabaseReader reader, MetabasePropertyType propertyType, string relativeVirtualPath)
        {
            string matchedPath;
            return FindPropertyUnderAppRoot(reader, propertyType, relativeVirtualPath, out matchedPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase." +
            "Caller must sanitize return value.")]
        [SecurityCritical]
        object FindPropertyUnderAppRoot(MetabaseReader reader, MetabasePropertyType propertyType, string relativeVirtualPath, out string matchedPath)
        {
            string endAboPath = appAboPath + relativeVirtualPath.Substring(1);
            int index = endAboPath.IndexOf(IISConstants.AboPathDelimiter, appAboPath.Length + 1);

            string startAboPath;
            if (index == -1)
            {
                startAboPath = endAboPath;
            }
            else
            {
                startAboPath = endAboPath.Substring(0, index);
            }

            return FindHierarchicalProperty(reader, propertyType, startAboPath, endAboPath, out matchedPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Uses MetabaseReader (critical class) to read data from metabase." +
            "Caller must sanitize return value.")]
        [SecurityCritical]
        object FindHierarchicalProperty(MetabaseReader reader, MetabasePropertyType propertyType, string startAboPath, string endAboPath, out string matchedPath)
        {
            matchedPath = null;
            while (endAboPath.Length >= startAboPath.Length)
            {
                object propertyValue = reader.GetData(endAboPath, propertyType);
                if (propertyValue != null)
                {
                    matchedPath = endAboPath;
                    return propertyValue;
                }

                // Continue the search in the parent
                int index = endAboPath.LastIndexOf(IISConstants.AboPathDelimiter);
                endAboPath = endAboPath.Substring(0, index);
            }

            return null;
        }

        protected override IEnumerable<string> GetSiteApplicationPaths()
        {
            //IsWithinApp is currently only used by WAS features
            throw Fx.AssertAndThrowFatal("GetSiteApplicationPaths() not implemented for iis6.");
        }
    }

    class HostedServiceTransportSettings
    {
        public string Realm = string.Empty;
        public HttpAccessSslFlags AccessSslFlags = HttpAccessSslFlags.None;
        public AuthFlags AuthFlags = AuthFlags.None;
        public string[] AuthProviders = MetabaseSettingsIis.DefaultAuthProviders;
        public ExtendedProtectionPolicy IisExtendedProtectionPolicy { get; set; }
    }

    [Flags]
    enum AuthFlags
    {
        None = 0,
        AuthAnonymous = 1,
        AuthBasic = 2,
        AuthNTLM = 4,

        // Note: AuthMD5 means IIS AuthScheme is Digest. Not MD5 algorithm.
        AuthMD5 = 16,
        AuthPassport = 64,
    }

    [Flags]
    enum HttpAccessSslFlags
    {
        None = 0x00000000,
        Ssl = 0x00000008,
        SslNegotiateCert = 0x00000020,
        SslRequireCert = 0x00000040,
        SslMapCert = 0x00000080,
        Ssl128 = 0x00000100
    }

    enum ExtendedProtectionTokenChecking
    {
        None = 0,
        Allow = 1,
        Require = 2,
    }

    [Flags]
    enum ExtendedProtectionFlags
    {
        None = 0,
        Proxy = 1,
        NoServiceNameCheck = 2,
        AllowDotlessSpn = 4,
        ProxyCohosting = 32,
    }
}
