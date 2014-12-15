//------------------------------------------------------------------------------
// <copyright file="ConfigurationStrings.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", ConfigurationStrings.SectionGroupName, sectionName);
        }

        static string GetSectionPath(string sectionName, string subSectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"{0}/{1}/{2}", ConfigurationStrings.SectionGroupName, sectionName, subSectionName);
        }

        static internal string AuthenticationModulesSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.AuthenticationModulesSectionName); }
        }

        static internal string ConnectionManagementSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.ConnectionManagementSectionName); }
        }

        static internal string DefaultProxySectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.DefaultProxySectionName); }
        }

        static internal string SmtpSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.MailSettingsSectionName, ConfigurationStrings.SmtpSectionName); }
        }

        static internal string RequestCachingSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.RequestCachingSectionName); }
        }

        static internal string SettingsSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.SettingsSectionName); }
        }

        static internal string WebRequestModulesSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.WebRequestModulesSectionName); }
        }

        internal const string Address = "address";
        internal const string AutoDetect = "autoDetect";
        internal const string AlwaysUseCompletionPortsForAccept = "alwaysUseCompletionPortsForAccept";
        internal const string AlwaysUseCompletionPortsForConnect = "alwaysUseCompletionPortsForConnect";
        internal const string AuthenticationModulesSectionName = "authenticationModules";
        internal const string BypassList = "bypasslist";
        internal const string BypassOnLocal = "bypassonlocal";
        internal const string CheckCertificateName = "checkCertificateName";
        internal const string CheckCertificateRevocationList = "checkCertificateRevocationList";
        internal const string ClientDomain = "clientDomain";
        internal const string ConnectionManagementSectionName = "connectionManagement";
        internal const string DefaultCredentials = "defaultCredentials";
        internal const string DefaultHttpCachePolicy = "defaultHttpCachePolicy";
        internal const string DefaultFtpCachePolicy = "defaultFtpCachePolicy";
        internal const string DefaultPolicyLevel = "defaultPolicyLevel";
        internal const string DefaultProxySectionName = "defaultProxy";
        internal const string DeliveryMethod = "deliveryMethod";
        internal const string DeliveryFormat = "deliveryFormat";
        internal const string DisableAllCaching = "disableAllCaching";
        internal const string DnsRefreshTimeout = "dnsRefreshTimeout";
        internal const string DownloadTimeout = "downloadTimeout";
        internal const string Enabled = "enabled";
        internal const string EnableDnsRoundRobin = "enableDnsRoundRobin";
        internal const string EnableSsl = "enableSsl";
        internal const string EncryptionPolicy = "encryptionPolicy";
        internal const string Expect100Continue = "expect100Continue";
        internal const string File = "file:";
        internal const string From = "from";
        internal const string Ftp = "ftp:";
        internal const string Host = "host";
        internal const string HttpWebRequest = "httpWebRequest";
        internal const string HttpListener = "httpListener";
        internal const string Http = "http:";
        internal const string Https = "https:";
        internal const string Ipv6 = "ipv6";
        internal const string IsPrivateCache = "isPrivateCache";
        internal const string IPProtectionLevel = "ipProtectionLevel";
        internal const string MailSettingsSectionName = "mailSettings";
        internal const string MaxConnection = "maxconnection";
        internal const string MaximumAge = "maximumAge";
        internal const string MaximumStale = "maximumStale";
        internal const string MaximumResponseHeadersLength = "maximumResponseHeadersLength";
        internal const string MaximumErrorResponseLength = "maximumErrorResponseLength";
        internal const string MinimumFresh = "minimumFresh";
        internal const string Module = "module";
        internal const string Name = "name";
        internal const string Network = "network";
        internal const string Password = "password";
        internal const string PerformanceCounters = "performanceCounters";
        internal const string PickupDirectoryFromIis = "pickupDirectoryFromIis";
        internal const string PickupDirectoryLocation = "pickupDirectoryLocation";
        internal const string PolicyLevel = "policyLevel";
        internal const string Port = "port";
        internal const string Prefix = "prefix";
        internal const string Proxy = "proxy";
        internal const string ProxyAddress = "proxyaddress";
        internal const string RequestCachingSectionName = "requestCaching";
        internal const string ScriptLocation = "scriptLocation";
        internal const string SectionGroupName = "system.net";
        internal const string ServicePointManager = "servicePointManager";
        internal const string SettingsSectionName = "settings";
        internal const string SmtpSectionName = "smtp";
        internal const string Socket = "socket";
        internal const string SpecifiedPickupDirectory = "specifiedPickupDirectory";
        internal const string TargetName = "targetName";
        internal const string Type = "type";
        internal const string UnicodeDecodingConformance = "unicodeDecodingConformance";
        internal const string UnicodeEncodingConformance = "unicodeEncodingConformance";
        internal const string UnspecifiedMaximumAge = "unspecifiedMaximumAge";
        internal const string UseDefaultCredentials = "useDefaultCredentials";
        internal const string UseNagleAlgorithm = "useNagleAlgorithm";
        internal const string UseSystemDefault = "usesystemdefault";
        internal const string UseUnsafeHeaderParsing = "useUnsafeHeaderParsing";
        internal const string UserName = "userName";
        internal const string WebProxyScript = "webProxyScript";
        internal const string WebRequestModulesSectionName = "webRequestModules";
        internal const string WebUtility = "webUtility";
    	internal const string maximumUnauthorizedUploadLength = "maximumUnauthorizedUploadLength";
        internal const string UnescapeRequestUrl = "unescapeRequestUrl";
        internal const string Timeouts = "timeouts";
        internal const string EntityBody = "entityBody";
        internal const string DrainEntityBody = "drainEntityBody";
        internal const string RequestQueue = "requestQueue";
        internal const string IdleConnection = "idleConnection";
        internal const string HeaderWait = "headerWait";
        internal const string MinSendBytesPerSecond = "minSendBytesPerSecond";
    }
}
namespace System.Configuration
{
    using System;
    using System.Globalization;

    internal static class CommonConfigurationStrings
    {
        internal const string UriSectionName = "uri";
        internal const string IriParsing = "iriParsing";
        internal const string Idn = "idn";
        internal const string Enabled = "enabled";
        internal const string SchemeSettings = "schemeSettings";
        internal const string SchemeName = "name";
        internal const string GenericUriParserOptions = "genericUriParserOptions";
    }
}
