//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Web;
    using System.Web.Hosting;
    using Microsoft.Web.Administration;

    static class MetabaseSettingsIis7Constants
    {
        internal const string SitesSectionName = "system.applicationHost/sites";
        internal const string ClientCertMapAuthenticationName = "system.webServer/security/authentication/clientCertificateMappingAuthentication";
        internal const string IisClientCertMapAuthenticationName = "system.webServer/security/authentication/iisClientCertificateMappingAuthentication";
        internal const string AnonymousAuthenticationSectionName = "system.webServer/security/authentication/anonymousAuthentication";
        internal const string BasicAuthenticationSectionName = "system.webServer/security/authentication/basicAuthentication";
        internal const string DigestAuthenticationSectionName = "system.webServer/security/authentication/digestAuthentication";
        internal const string WindowsAuthenticationSectionName = "system.webServer/security/authentication/windowsAuthentication";
        internal const string SecurityAccessSectionName = "system.webServer/security/access";

        internal const string EnabledAttributeName = "enabled";
        internal const string RealmAttributeName = "realm";
        internal const string ValueAttributeName = "value";
        internal const string SslFlagsAttributeName = "sslFlags";
        internal const string ProviderElementName = "providers";
        internal const string BindingsElementName = "bindings";
        internal const string ProtocolAttributeName = "protocol";
        internal const string BindingInfoAttributeName = "bindingInformation";
        internal const string PathAttributeName = "path";
        internal const string EnabledProtocolsAttributeName = "enabledProtocols";
        internal const string NameAttributeName = "name";
        internal const string ExtendedProtectionElementName = "extendedProtection";
        internal const string TokenCheckingAttributeName = "tokenChecking";
        internal const string FlagsAttributeName = "flags";


        internal const string CommaSeparator = ",";

        internal const string WebConfigGetSectionMethodName = "GetSection";
    }

    // MetabaseSettingsIis7 use ServerManager class to get Metabase settings. ServerManager 
    // does not work in Longhorn Sever/SP1 builds.
    class MetabaseSettingsIis7 : MetabaseSettingsIis
    {
        internal MetabaseSettingsIis7()
            : base()
        {
            if (!Iis7Helper.IsIis7)
            {
                DiagnosticUtility.DebugAssert("MetabaseSettingsIis7 constructor must not be called when running outside of IIS7");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInternal(true);
            }

            PopulateSiteProperties();
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "called by MetabaseSettingsIis7 constructor")]
        void PopulateSiteProperties()
        {
            Site site = ServerManagerWrapper.GetSite(HostingEnvironment.SiteName);
            DiagnosticUtility.DebugAssert(site != null, "Unable to find site.");

            //
            // Build up the binding table.
            //
            IDictionary<string, List<string>> bindingList = ServerManagerWrapper.GetProtocolBindingTable(site);

            // Convert to string arrays
            foreach (KeyValuePair<string, List<string>> entry in bindingList)
            {
                this.Bindings.Add(entry.Key, entry.Value.ToArray());
                entry.Value.Clear();
            }

            // Clear the temporary buffer
            bindingList.Clear();

            //
            // Build up the protocol list.
            //
            string[] protocols = ServerManagerWrapper.GetEnabledProtocols(site).Split(MetabaseSettingsIis7Constants.CommaSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string protocolValue in protocols)
            {
                string protocol = protocolValue.Trim();
                protocol = protocol.ToLowerInvariant();

                if (string.IsNullOrEmpty(protocol) || this.Protocols.Contains(protocol))
                {
                    // Ignore duplicates and empty protocols
                    continue;
                }
                else if (string.Compare(protocol, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0 ||
                         string.Compare(protocol, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Special casing HTTPS. If HTTP is enabled, it means that
                    // both HTTP and HTTPS are enabled.
                    if (this.Bindings.ContainsKey(Uri.UriSchemeHttp))
                    {
                        this.Protocols.Add(Uri.UriSchemeHttp);
                    }

                    if (this.Bindings.ContainsKey(Uri.UriSchemeHttps))
                    {
                        this.Protocols.Add(Uri.UriSchemeHttps);
                    }
                }
                else if (this.Bindings.ContainsKey(protocol))
                {
                    // We only take the protocols that have bindings.
                    this.Protocols.Add(protocol);
                }
            }
        }

        protected override HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath)
        {
            Debug.Print("MetabaseSettingsIis7.CreateTransportSettings() calling ServerManager.GetWebConfiguration() virtualPath: " + relativeVirtualPath);

            string absolutePath = VirtualPathUtility.ToAbsolute(relativeVirtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);

            Configuration config =
                    ServerManagerWrapper.GetWebConfiguration(
                    HostingEnvironment.SiteName,
                    absolutePath);

            HostedServiceTransportSettings transportSettings = new HostedServiceTransportSettings();

            ProcessAnonymousAuthentication(config, ref transportSettings);
            ProcessBasicAuthentication(config, ref transportSettings);
            ProcessWindowsAuthentication(config, ref transportSettings);
            ProcessDigestAuthentication(config, ref transportSettings);
            ProcessSecurityAccess(config, ref transportSettings);

            return transportSettings;
        }

        void ProcessAnonymousAuthentication(Configuration config, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.AnonymousAuthenticationSectionName);

            if ((section != null) &&
                ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthAnonymous;
            }
        }

        void ProcessBasicAuthentication(Configuration config, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.BasicAuthenticationSectionName);

            if ((section != null) &&
                ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthBasic;
                transportSettings.Realm = (string)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.RealmAttributeName);
            }
        }

        void ProcessWindowsAuthentication(Configuration config, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.WindowsAuthenticationSectionName);

            if ((section != null) &&
                ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthNTLM;

                List<string> providerList = ServerManagerWrapper.GetProviders(section, MetabaseSettingsIis7Constants.ProviderElementName,
                    MetabaseSettingsIis7Constants.ValueAttributeName);

                if (providerList.Count != 0)
                {
                    transportSettings.AuthProviders = providerList.ToArray();
                }
            }
            try
            {

                ConfigurationElement element = section.GetChildElement(MetabaseSettingsIis7Constants.ExtendedProtectionElementName);
                if (element != null)
                {
                    ExtendedProtectionTokenChecking tokenChecking;
                    ExtendedProtectionFlags flags;
                    List<string> spnList;
                    ServerManagerWrapper.ReadIisExtendedProtectionPolicy(element, out tokenChecking, out flags, out spnList);
                    transportSettings.IisExtendedProtectionPolicy = BuildExtendedProtectionPolicy(tokenChecking, flags, spnList);
                }
            }
            catch (COMException e)
            {
                // hit this exception only when IIS does not support CBT
                // safe for us to igore this COMException so that services not using CBT still can be activated
                // if a service does use CBT in binding, channel listener will catch it when comparing IIS setting against WCF (on CBT) and throw exception 
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.WebHostNoCBTSupport,
                              SR.TraceCodeWebHostNoCBTSupport, this, e);
                }
            }
        }

        void ProcessDigestAuthentication(Configuration config, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.DigestAuthenticationSectionName);

            if ((section != null) &&
                ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthMD5;
            }
        }

        void ProcessSecurityAccess(Configuration config, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.SecurityAccessSectionName);

            // Check SSL Flags.
            if (section != null)
            {
                int sslFlags = (int)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.SslFlagsAttributeName);
                transportSettings.AccessSslFlags = (HttpAccessSslFlags)sslFlags;

                // Clear SslMapCert field, which should not contain any useful data now.
                transportSettings.AccessSslFlags &= ~(HttpAccessSslFlags.SslMapCert);
            }

            // Check whether IIS client certificate mapping is enabled.
            section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.IisClientCertMapAuthenticationName);
            if ((section != null) &&
               ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AccessSslFlags |= HttpAccessSslFlags.SslMapCert;
            }
            else
            {
                // Check whether Active Directory client certification mapping is enabled.
                section = ServerManagerWrapper.GetSection(config, MetabaseSettingsIis7Constants.ClientCertMapAuthenticationName);
                if ((section != null) &&
                   ((bool)ServerManagerWrapper.GetAttributeValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                    )
                {
                    transportSettings.AccessSslFlags |= HttpAccessSslFlags.SslMapCert;
                }
            }
        }

        protected override IEnumerable<string> GetSiteApplicationPaths()
        {
            return ServerManagerWrapper.GetApplicationPaths();
        }

        // wraps calls to ServerManager with Asserts as necessary to support partial trust scenarios
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static class ServerManagerWrapper
        {
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7.PopulateSiteProperties")]
            static internal Site GetSite(string name)
            {
                return new ServerManager().Sites[name];
            }

            static internal Configuration GetWebConfiguration(string siteName, string absolutePath)
            {
                return new ServerManager().GetWebConfiguration(siteName, absolutePath);
            }

            static internal ConfigurationSection GetSection(Configuration config, string sectionName)
            {
                return config.GetSection(sectionName);
            }

            static internal List<string> GetProviders(ConfigurationSection section, string providerElementName, string valueAttributeName)
            {
                List<string> providerList = new List<string>();
                foreach (ConfigurationElement element in section.GetCollection(providerElementName))
                {
                    providerList.Add((string)ServerManagerWrapper.GetAttributeValue(element, valueAttributeName));
                }
                return providerList;
            }

            static internal object GetAttributeValue(ConfigurationSection section, string attributeName)
            {
                return section.GetAttribute(attributeName).Value;
            }

            static internal object GetAttributeValue(ConfigurationElement element, string attributeName)
            {
                return element.GetAttribute(attributeName).Value;
            }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7.PopulateSiteProperties")]
            static internal IDictionary<string, List<string>> GetProtocolBindingTable(Site site)
            {
                IDictionary<string, List<string>> bindingList = new Dictionary<string, List<string>>();
                foreach (Microsoft.Web.Administration.Binding binding in site.Bindings)
                {
                    string protocol = binding.Protocol.ToLowerInvariant();
                    string bindingInformation = binding.BindingInformation;
                    Debug.Print("MetabaseSettingsIis7.ctor() adding Protocol: " + protocol + " BindingInformation: " + bindingInformation);

                    if (!bindingList.ContainsKey(protocol))
                    {
                        bindingList.Add(protocol, new List<string>());
                    }
                    bindingList[protocol].Add(bindingInformation);
                }
                return bindingList;
            }
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7.PopulateSiteProperties")]
            static internal string GetEnabledProtocols(Site site)
            {
                Application application = site.Applications[HostingEnvironmentWrapper.ApplicationVirtualPath];
                DiagnosticUtility.DebugAssert(application != null, "Unable to find application.");

                return application.EnabledProtocols;
            }

            static internal void ReadIisExtendedProtectionPolicy(ConfigurationElement element, out ExtendedProtectionTokenChecking tokenChecking,
                out ExtendedProtectionFlags flags, out List<string> spnList)
            {
                tokenChecking = (ExtendedProtectionTokenChecking)element.GetAttributeValue(MetabaseSettingsIis7Constants.TokenCheckingAttributeName);
                flags = (ExtendedProtectionFlags)element.GetAttributeValue(MetabaseSettingsIis7Constants.FlagsAttributeName);
                spnList = new List<string>();
                foreach (ConfigurationElement configElement in element.GetCollection())
                {
                    spnList.Add((string)configElement[MetabaseSettingsIis7Constants.NameAttributeName]);
                }
            }

            static internal IEnumerable<string> GetApplicationPaths()
            {
                //Get the site ourselves instead of calling GetSite() because we should dispose of the ServerManager
                using (ServerManager serverManager = new ServerManager())
                {
                    List<string> appPaths = new List<string>();
                    Site site = serverManager.Sites[HostingEnvironment.SiteName];
                    foreach (Application app in site.Applications)
                    {
                        appPaths.Add(app.Path);
                    }
                    return appPaths;
                }
            }
        }
    }

    // MetabaseSettingsIis7V2 use WebConfigurationManager to get Metabase settings, we depend on 
    // some methods which only availble in Longhorn Server/SP1 build. 
    class MetabaseSettingsIis7V2 : MetabaseSettingsIis
    {
        static MethodInfo getSectionMethod;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Called by MetabaseSettingsIis7Factory.CreateMetabaseSettings")]
        internal MetabaseSettingsIis7V2()
            : base()
        {
            if (!Iis7Helper.IsIis7)
            {
                DiagnosticUtility.DebugAssert("MetabaseSettingsIis7V2 constructor must not be called when running outside of IIS7");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInternal(true);
            }

            PopulateSiteProperties();
        }

        static internal MethodInfo GetSectionMethod
        {
            get
            {
                if (getSectionMethod == null)
                {
                    Type type = typeof(WebConfigurationManager);

                    getSectionMethod = type.GetMethod(
                        MetabaseSettingsIis7Constants.WebConfigGetSectionMethodName,
                        new Type[3] { typeof(string), typeof(string), typeof(string) }
                        );
                }
                return getSectionMethod;
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Called by MetabaseSettingsIis7V2 constructor")]
        void PopulateSiteProperties()
        {
            ConfigurationElement site = WebConfigurationManagerWrapper.GetSite(HostingEnvironment.SiteName);
            //
            // Build up the binding table.
            //
            IDictionary<string, List<string>> bindingList = WebConfigurationManagerWrapper.GetProtocolBindingTable(site);

            // Convert to string arrays
            foreach (KeyValuePair<string, List<string>> entry in bindingList)
            {
                this.Bindings.Add(entry.Key, entry.Value.ToArray());
                entry.Value.Clear();
            }

            // Clear the temporary buffer
            bindingList.Clear();

            //
            // Build up the protocol list.
            //

            string[] protocols = WebConfigurationManagerWrapper.GetEnabledProtocols(site).Split(MetabaseSettingsIis7Constants.CommaSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string protocolValue in protocols)
            {
                string protocol = protocolValue.Trim();
                protocol = protocol.ToLowerInvariant();

                if (string.IsNullOrEmpty(protocol) || this.Protocols.Contains(protocol))
                {
                    // Ignore duplicates and empty protocols
                    continue;
                }
                else if (string.Compare(protocol, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0 ||
                         string.Compare(protocol, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Special casing HTTPS. If HTTP is enabled, it means that
                    // both HTTP and HTTPS are enabled.
                    if (this.Bindings.ContainsKey(Uri.UriSchemeHttp))
                    {
                        this.Protocols.Add(Uri.UriSchemeHttp);
                    }

                    if (this.Bindings.ContainsKey(Uri.UriSchemeHttps))
                    {
                        this.Protocols.Add(Uri.UriSchemeHttps);
                    }
                }
                else if (this.Bindings.ContainsKey(protocol))
                {
                    // We only take the protocols that have bindings.
                    this.Protocols.Add(protocol);
                }
            }
        }

        protected override HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath)
        {
            Debug.Print("MetabaseSettingsIis7.CreateTransportSettings() calling ServerManager.GetWebConfiguration() virtualPath: " + relativeVirtualPath);

            string absolutePath = VirtualPathUtility.ToAbsolute(relativeVirtualPath, HostingEnvironment.ApplicationVirtualPath);

            HostedServiceTransportSettings transportSettings = new HostedServiceTransportSettings();
            string siteName = HostingEnvironment.SiteName;

            ProcessAnonymousAuthentication(siteName, absolutePath, ref transportSettings);
            ProcessBasicAuthentication(siteName, absolutePath, ref transportSettings);
            ProcessWindowsAuthentication(siteName, absolutePath, ref transportSettings);
            ProcessDigestAuthentication(siteName, absolutePath, ref transportSettings);
            ProcessSecurityAccess(siteName, absolutePath, ref transportSettings);

            return transportSettings;
        }

        void ProcessAnonymousAuthentication(string siteName, string virtualPath, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.AnonymousAuthenticationSectionName);

            if ((section != null) &&
                ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthAnonymous;
            }
        }

        void ProcessBasicAuthentication(string siteName, string virtualPath, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.BasicAuthenticationSectionName);

            if ((section != null) &&
                ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthBasic;
                transportSettings.Realm = (string)section.GetAttribute(MetabaseSettingsIis7Constants.RealmAttributeName).Value;
            }
        }

        void ProcessWindowsAuthentication(string siteName, string virtualPath, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.WindowsAuthenticationSectionName);

            if ((section != null) &&
                ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthNTLM;

                List<string> providerList = WebConfigurationManagerWrapper.GetProviderList(section);

                if (providerList.Count != 0)
                {
                    transportSettings.AuthProviders = providerList.ToArray();
                }

                // Check the CBT configuration
                try
                {
                    ConfigurationElement element = section.GetChildElement(MetabaseSettingsIis7Constants.ExtendedProtectionElementName);
                    if (element != null)
                    {
                        ExtendedProtectionTokenChecking tokenChecking;
                        ExtendedProtectionFlags flags;
                        List<string> spnList;
                        WebConfigurationManagerWrapper.ReadIisExtendedProtectionPolicy(element, out tokenChecking, out flags, out spnList);
                        transportSettings.IisExtendedProtectionPolicy = BuildExtendedProtectionPolicy(tokenChecking, flags, spnList);
                    }
                }
                catch (COMException e)
                {
                    // hit this exception only when IIS does not support CBT
                    // safe for us to igore this COMException so that services not using CBT still can be activated
                    // if a service does use CBT in binding, channel listener will catch it when comparing IIS setting against WCF (on CBT) and throw exception 
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.WebHostNoCBTSupport,
                            SR.TraceCodeWebHostNoCBTSupport, this, e);
                    }
                }
            }
        }

        void ProcessDigestAuthentication(string siteName, string virtualPath, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.DigestAuthenticationSectionName);

            if ((section != null) &&
                ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AuthFlags = transportSettings.AuthFlags | AuthFlags.AuthMD5;
            }
        }

        void ProcessSecurityAccess(string siteName, string virtualPath, ref HostedServiceTransportSettings transportSettings)
        {
            ConfigurationSection section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.SecurityAccessSectionName);

            // Check SSL Flags.
            if (section != null)
            {
                int sslFlags = (int)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.SslFlagsAttributeName);
                transportSettings.AccessSslFlags = (HttpAccessSslFlags)sslFlags;

                // Clear SslMapCert field, which should not contain any useful data now.
                transportSettings.AccessSslFlags &= ~(HttpAccessSslFlags.SslMapCert);
            }

            // Check whether IIS client certificate mapping is enabled.
            section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.IisClientCertMapAuthenticationName);
            if ((section != null) &&
               ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                )
            {
                transportSettings.AccessSslFlags |= HttpAccessSslFlags.SslMapCert;
            }
            else
            {
                // Check whether Active Directory client certification mapping is enabled.
                section = WebConfigurationManagerWrapper.WebConfigGetSection(siteName, virtualPath, MetabaseSettingsIis7Constants.ClientCertMapAuthenticationName);
                if ((section != null) &&
                   ((bool)WebConfigurationManagerWrapper.GetValue(section, MetabaseSettingsIis7Constants.EnabledAttributeName))
                    )
                {
                    transportSettings.AccessSslFlags |= HttpAccessSslFlags.SslMapCert;
                }
            }
        }

        protected override IEnumerable<string> GetSiteApplicationPaths()
        {
            ConfigurationElement site = WebConfigurationManagerWrapper.GetSite(HostingEnvironment.SiteName);
            return WebConfigurationManagerWrapper.GetApplicationPaths(site);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static class WebConfigurationManagerWrapper
        {
            // Helper Method to get a site configuration
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7V2.PopulateSiteProperties")]
            static internal ConfigurationElement GetSite(string siteName)
            {
                ConfigurationSection sitesSection = WebConfigGetSection(null, null, MetabaseSettingsIis7Constants.SitesSectionName);
                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();

                return FindElement(sitesCollection, MetabaseSettingsIis7Constants.NameAttributeName, siteName);
            }

            // Helper method to find element based on an string attribute.
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by GetSite")]
            static internal ConfigurationElement FindElement(ConfigurationElementCollection collection, string attributeName, string value)
            {
                foreach (ConfigurationElement element in collection)
                {
                    if (String.Equals((string)element[attributeName], value, StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }

                return null;
            }

            static internal ConfigurationSection WebConfigGetSection(string siteName, string virtualPath, string sectionName)
            {
                return (ConfigurationSection)GetSectionMethod.Invoke(null, new object[] { siteName, virtualPath, sectionName });
            }

            static internal object GetValue(ConfigurationSection section, string name)
            {
                return section[name];
            }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7V2.PopulateSiteProperties")]
            static internal IDictionary<string, List<string>> GetProtocolBindingTable(ConfigurationElement site)
            {
                IDictionary<string, List<string>> bindingList = new Dictionary<string, List<string>>();
                foreach (ConfigurationElement binding in site.GetCollection(MetabaseSettingsIis7Constants.BindingsElementName))
                {
                    string protocol = ((string)binding[MetabaseSettingsIis7Constants.ProtocolAttributeName]).ToLowerInvariant();
                    string bindingInformation = (string)binding[MetabaseSettingsIis7Constants.BindingInfoAttributeName];
                    Debug.Print("MetabaseSettingsIis7V2.ctor() adding Protocol: " + protocol + " BindingInformation: " + bindingInformation);

                    if (!bindingList.ContainsKey(protocol))
                    {
                        bindingList.Add(protocol, new List<string>());
                    }
                    bindingList[protocol].Add(bindingInformation);
                }
                return bindingList;
            }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by MetabaseSettingsIis7V2.PopulateSiteProperties")]
            static internal string GetEnabledProtocols(ConfigurationElement site)
            {
                ConfigurationElement application = FindElement(
                    site.GetCollection(),
                    MetabaseSettingsIis7Constants.PathAttributeName,
                    HostingEnvironment.ApplicationVirtualPath
                    );
                DiagnosticUtility.DebugAssert(application != null, "Unable to find application.");

                return (string)application[MetabaseSettingsIis7Constants.EnabledProtocolsAttributeName];
            }

            static internal List<string> GetProviderList(ConfigurationElement section)
            {
                List<string> providerList = new List<string>();
                foreach (ConfigurationElement element in section.GetCollection(MetabaseSettingsIis7Constants.ProviderElementName))
                {
                    providerList.Add((string)element[MetabaseSettingsIis7Constants.ValueAttributeName]);
                }
                return providerList;
            }

            // translate IIS setting on extended protection to NCL object
            static internal void ReadIisExtendedProtectionPolicy(ConfigurationElement element, out ExtendedProtectionTokenChecking tokenChecking,
                out ExtendedProtectionFlags flags, out List<string> spnList)
            {
                tokenChecking = (ExtendedProtectionTokenChecking)element.GetAttributeValue(MetabaseSettingsIis7Constants.TokenCheckingAttributeName);
                flags = (ExtendedProtectionFlags)element.GetAttributeValue(MetabaseSettingsIis7Constants.FlagsAttributeName);
                spnList = new List<string>();
                foreach (ConfigurationElement configElement in element.GetCollection())
                {
                    spnList.Add((string)configElement[MetabaseSettingsIis7Constants.NameAttributeName]);
                }
            }

            static internal IEnumerable<string> GetApplicationPaths(ConfigurationElement site)
            {
                List<string> appPaths = new List<string>();
                ConfigurationElementCollection applications = site.GetCollection();
                foreach (ConfigurationElement application in applications)
                {
                    string appPath = (string)application["path"];
                    appPaths.Add(appPath);
                }
                return appPaths;
            }

        }
    }

    // Note: There is a dependency on this class name and CreateMetabaseSettings 
    // method name from System.ServiceModel.dll
    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
                Justification = "Instantiated by System.ServiceModel")]
    internal class MetabaseSettingsIis7Factory
    {
        internal static MetabaseSettings CreateMetabaseSettings()
        {
            MethodInfo method = MetabaseSettingsIis7V2.GetSectionMethod;
            if (method != null)
            {
                return new MetabaseSettingsIis7V2();
            }

            return new MetabaseSettingsIis7();
        }
    }
}
