//------------------------------------------------------------------------------
// <copyright file="SettingsSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Configuration;
    using System.Threading; 
    using System.Net.Security;
    using System.Net.Sockets;

    public sealed class SettingsSection : ConfigurationSection
    {

        // This gets called by the configuration system when the app.config file is located at an
        // http:// Uri.  It forces the System.Net config to be loaded based on only the machine.config
        // because the configuration system uses System.Net to download the app.config file.
        static internal void EnsureConfigLoaded() {
            try {
                //AuthenticationModules section
                System.Net.AuthenticationManager.EnsureConfigLoaded();
                //Requestcachingsection section
                object o = System.Net.Cache.RequestCacheManager.IsCachingEnabled;
                //ConnectionManagement section
                o = System.Net.ServicePointManager.DefaultConnectionLimit;
                //Settings section
                o = System.Net.ServicePointManager.Expect100Continue;
                //webrequestmodules section
                o = System.Net.WebRequest.PrefixList;
                //DefaultProxy section
                o = System.Net.WebRequest.InternalDefaultWebProxy;
            }
            catch {
            }
        }


        public SettingsSection() 
        {
            this.properties.Add(this.httpWebRequest);
            this.properties.Add(this.ipv6);
            this.properties.Add(this.servicePointManager);
            this.properties.Add(this.socket);
            this.properties.Add(this.webProxyScript);
            this.properties.Add(this.performanceCounters);
            this.properties.Add(this.httpListener);
            this.properties.Add(this.webUtility);
        }


        [ConfigurationProperty(ConfigurationStrings.HttpWebRequest)]
        public HttpWebRequestElement HttpWebRequest
        {
            get { return (HttpWebRequestElement)this[this.httpWebRequest]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Ipv6)]
        public Ipv6Element Ipv6
        {
            get { return (Ipv6Element)this[this.ipv6]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServicePointManager)]
        public ServicePointManagerElement ServicePointManager
        {
            get { return (ServicePointManagerElement)this[this.servicePointManager]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Socket)]
        public SocketElement Socket
        {
            get { return (SocketElement)this[this.socket]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WebProxyScript)]
        public WebProxyScriptElement WebProxyScript
        {
            get { return (WebProxyScriptElement) this[this.webProxyScript]; }
        }


        [ConfigurationProperty(ConfigurationStrings.PerformanceCounters)]
        public PerformanceCountersElement PerformanceCounters
        {
            get { return (PerformanceCountersElement) this[this.performanceCounters]; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpListener)]
        public HttpListenerElement HttpListener
        {
            get { return (HttpListenerElement)this[this.httpListener]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WebUtility)]
        public WebUtilityElement WebUtility
        {
            get { return (WebUtilityElement)this[this.webUtility]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty httpWebRequest =
            new ConfigurationProperty(ConfigurationStrings.HttpWebRequest, typeof(HttpWebRequestElement), null,
                    ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty ipv6 =
            new ConfigurationProperty(ConfigurationStrings.Ipv6, typeof(Ipv6Element), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty servicePointManager =
            new ConfigurationProperty(ConfigurationStrings.ServicePointManager, typeof(ServicePointManagerElement), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty socket =
            new ConfigurationProperty(ConfigurationStrings.Socket, typeof(SocketElement), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty webProxyScript =
            new ConfigurationProperty(ConfigurationStrings.WebProxyScript, typeof(WebProxyScriptElement), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty performanceCounters =
            new ConfigurationProperty(ConfigurationStrings.PerformanceCounters, typeof(PerformanceCountersElement), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty httpListener =
            new ConfigurationProperty(ConfigurationStrings.HttpListener, typeof(HttpListenerElement), null,
                ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty webUtility =
            new ConfigurationProperty(ConfigurationStrings.WebUtility, typeof(WebUtilityElement), null,
                ConfigurationPropertyOptions.None);
    }

    internal sealed class SettingsSectionInternal
    {
        private static object s_InternalSyncObject = null;
        internal SettingsSectionInternal(SettingsSection section)
        {
            TimeSpan ts;

            if (section == null)
                section = new SettingsSection();

            this.alwaysUseCompletionPortsForConnect = section.Socket.AlwaysUseCompletionPortsForConnect;
            this.alwaysUseCompletionPortsForAccept = section.Socket.AlwaysUseCompletionPortsForAccept;
            this.checkCertificateName = section.ServicePointManager.CheckCertificateName;
            this.checkCertificateRevocationList = section.ServicePointManager.CheckCertificateRevocationList;
            this.dnsRefreshTimeout = section.ServicePointManager.DnsRefreshTimeout;
            this.ipProtectionLevel = section.Socket.IPProtectionLevel;
            this.ipv6Enabled = section.Ipv6.Enabled;
            this.enableDnsRoundRobin = section.ServicePointManager.EnableDnsRoundRobin;
            this.encryptionPolicy = section.ServicePointManager.EncryptionPolicy;
            this.expect100Continue = section.ServicePointManager.Expect100Continue;
            this.maximumUnauthorizedUploadLength = section.HttpWebRequest.MaximumUnauthorizedUploadLength;
            this.maximumResponseHeadersLength = section.HttpWebRequest.MaximumResponseHeadersLength;
            this.maximumErrorResponseLength = section.HttpWebRequest.MaximumErrorResponseLength;
            this.useUnsafeHeaderParsing = section.HttpWebRequest.UseUnsafeHeaderParsing;
            this.useNagleAlgorithm = section.ServicePointManager.UseNagleAlgorithm;
            ts = section.WebProxyScript.DownloadTimeout;
            this.downloadTimeout = (ts == TimeSpan.MaxValue || ts == TimeSpan.Zero) ? Timeout.Infinite : (int) ts.TotalMilliseconds;
            this.performanceCountersEnabled = section.PerformanceCounters.Enabled;
            this.httpListenerUnescapeRequestUrl = section.HttpListener.UnescapeRequestUrl;
            this.httpListenerTimeouts = section.HttpListener.Timeouts.GetTimeouts();

            // <webUtility> element
            WebUtilityElement webUtilityElement = section.WebUtility;
            this.WebUtilityUnicodeDecodingConformance = webUtilityElement.UnicodeDecodingConformance;
            this.WebUtilityUnicodeEncodingConformance = webUtilityElement.UnicodeEncodingConformance;
        }


        internal static SettingsSectionInternal Section
        {
            get
            {

                if (s_settings == null) {
                    lock(InternalSyncObject) {
                        if (s_settings == null) {
                            s_settings = new SettingsSectionInternal((SettingsSection) PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
                        }
                    }
                }
                return s_settings;
            }
        }


        private static object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    object o = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }


        // This method is required - it gets called through reflection, matching all the other XxxSectionInternal classes.
        // This one gets it fresh for each call.  Generally it shouldn't be used.
        internal static SettingsSectionInternal GetSection()
        {
            return new SettingsSectionInternal((SettingsSection) PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
        }


        internal bool AlwaysUseCompletionPortsForAccept
        {
            get { return this.alwaysUseCompletionPortsForAccept; }
        }

        internal bool AlwaysUseCompletionPortsForConnect
        {
            get { return this.alwaysUseCompletionPortsForConnect; }
        }

        internal bool CheckCertificateName
        {
            get { return this.checkCertificateName; }
        }

        internal bool CheckCertificateRevocationList
        {
            get { return this.checkCertificateRevocationList; }
            set { this.checkCertificateRevocationList = value; }
        }

        internal int DnsRefreshTimeout
        {
            get { return this.dnsRefreshTimeout; }
            set { this.dnsRefreshTimeout = value; }
        }

        internal int DownloadTimeout
        {
            get { return this.downloadTimeout; }
        }

        internal bool EnableDnsRoundRobin
        {
            get { return this.enableDnsRoundRobin; }
            set { this.enableDnsRoundRobin = value; }
        }

        internal EncryptionPolicy EncryptionPolicy
        {
            get { return this.encryptionPolicy; }
        }

        internal bool Expect100Continue
        {
            get { return this.expect100Continue; }
            set { this.expect100Continue = value; }
        }

        internal IPProtectionLevel IPProtectionLevel
        {
            get { return this.ipProtectionLevel; }
        }

        internal bool Ipv6Enabled
        {
            get { return this.ipv6Enabled; }
        }

        internal int MaximumResponseHeadersLength
        {
            get { return this.maximumResponseHeadersLength; }
            set { this.maximumResponseHeadersLength = value; }
        }

        internal int MaximumUnauthorizedUploadLength
        {
            get { return this.maximumUnauthorizedUploadLength; }
        }
        
        internal int MaximumErrorResponseLength
        {
            get { return this.maximumErrorResponseLength; }
            set { this.maximumErrorResponseLength = value; }
        }
        
        internal bool UseUnsafeHeaderParsing
        {
            get { return this.useUnsafeHeaderParsing; }
        }

        internal bool UseNagleAlgorithm
        {
            get { return this.useNagleAlgorithm; }
            set { this.useNagleAlgorithm = value; }
        }

        internal bool PerformanceCountersEnabled
        {
            get { return this.performanceCountersEnabled; }
        }

        internal bool HttpListenerUnescapeRequestUrl
        {
            get { return this.httpListenerUnescapeRequestUrl; }
        }

        internal long[] HttpListenerTimeouts
        {
            get { return this.httpListenerTimeouts; }
        }

        internal UnicodeDecodingConformance WebUtilityUnicodeDecodingConformance
        {
            get;
            private set;
        }

        internal UnicodeEncodingConformance WebUtilityUnicodeEncodingConformance
        {
            get;
            private set;
        }

        private static volatile SettingsSectionInternal s_settings;
        bool alwaysUseCompletionPortsForAccept;
        bool alwaysUseCompletionPortsForConnect;
        bool checkCertificateName;
        bool checkCertificateRevocationList;
        int downloadTimeout;
        int dnsRefreshTimeout;
        bool enableDnsRoundRobin;
        EncryptionPolicy encryptionPolicy;
        bool expect100Continue;
        IPProtectionLevel ipProtectionLevel;
        bool ipv6Enabled;
        int maximumResponseHeadersLength;
        int maximumErrorResponseLength;
        int maximumUnauthorizedUploadLength;
        bool useUnsafeHeaderParsing;
        bool useNagleAlgorithm;
        bool performanceCountersEnabled;
        bool httpListenerUnescapeRequestUrl;
        long[] httpListenerTimeouts;
    }
}
