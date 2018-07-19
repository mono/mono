//------------------------------------------------------------------------------
// <copyright file="RuntimeConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Configuration;
using System.Configuration.Internal;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Util;
using System.Web.Hosting;
using System.Web.Configuration;

namespace System.Web.Configuration {

    //
    // Internal, read-only access to configuration settings.
    //
    internal class RuntimeConfig {
        //
        // GetConfig() - get configuration appropriate for the current thread.
        //
        // Looks up the HttpContext on the current thread if it is available,
        // otherwise it uses the config at the app path.
        //
        // Use GetConfig(context) if a context is available, as it will avoid
        // the lookup for contxt on the current thread.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetConfig() {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            HttpContext context = HttpContext.Current;
            if (context != null) {
                return GetConfig(context);
            }
            else {
                return GetAppConfig();
            }
        }

        //
        // GetConfig(context) - gets configuration appropriate for the HttpContext.
        // The most efficient way to get config.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetConfig(HttpContext context) {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            return context.GetRuntimeConfig();
        }

        //
        // GetConfig(context, path) - returns the config at 'path'.
        //
        // This method is more efficient than not using context, as
        // the config cached in the context is used if it matches the
        // context path.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetConfig(HttpContext context, VirtualPath path) {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            return context.GetRuntimeConfig(path);
        }

        //
        // GetConfig(path) - returns the config at 'path'. 
        // 
        // If 'path' is null, or is outside of the application path, then it
        // returns the application config.
        //
        // For efficientcy, use GetConfig(context) instead of this method 
        // where possible.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetConfig(string path) {
            return GetConfig(VirtualPath.CreateNonRelativeAllowNull(path));
        }

        static internal RuntimeConfig GetConfig(VirtualPath path) {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem) {
                return GetClientRuntimeConfig();
            }

            return CachedPathData.GetVirtualPathData(path, true).RuntimeConfig;
        }

        //
        // GetAppConfig() - returns the application config.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetAppConfig() {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            return CachedPathData.GetApplicationPathData().RuntimeConfig;
        }

        //
        // GetRootWebConfig() - returns the root web configuration.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetRootWebConfig() {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            return CachedPathData.GetRootWebPathData().RuntimeConfig;
        }

        //
        // GetMachineConfig() - returns the machine configuration.
        //
        // For config derived from ConfigurationSection, this will either
        // return a non-null object or throw an exception.
        //
        // For config implemented with IConfigurationSectionHandler, this 
        // may return null, non-null, or throw an exception.
        //
        static internal RuntimeConfig GetMachineConfig() {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return GetClientRuntimeConfig();
            }

            return CachedPathData.GetMachinePathData().RuntimeConfig;
        }

        //
        // GetLKGConfig(context) - gets the nearest configuration available.
        //
        // This method is to be used in the few instances where we
        // cannot throw an exception if a config file has an error.
        //
        // This method will never throw an exception. If no config
        // is available, a request for a section will return null.
        //
        static internal RuntimeConfig GetLKGConfig(HttpContext context) {
            RuntimeConfig config = null;
            bool success = false;
            try {
                config = GetConfig(context);
                success = true;
            }
            catch {
            }

            if (!success) {
                config = GetLKGRuntimeConfig(context.Request.FilePathObject);
            }

            return config.RuntimeConfigLKG;
        }

        //
        // GetAppLKGConfig(path) - gets the nearest configuration available,
        // starting from the application path.
        //
        // This method is to be used in the few instances where we
        // cannot throw an exception if a config file has an error.
        //
        // This method will never throw an exception. If no config
        // is available, a request for a section will return null.
        //
        static internal RuntimeConfig GetAppLKGConfig() {
            RuntimeConfig config = null;
            bool success = false;
            try {
                config = GetAppConfig();
                success = true;
            }
            catch {
            }


            if (!success) {
                config = GetLKGRuntimeConfig(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPathObject);
            }

            return config.RuntimeConfigLKG;
        }

        //
        // WHIDBEY sections
        //

        internal ConnectionStringsSection ConnectionStrings {
            get {
                return (ConnectionStringsSection) GetSection("connectionStrings", typeof(ConnectionStringsSection), ResultsIndex.ConnectionStrings);
            }
        }

        internal System.Net.Configuration.SmtpSection Smtp {
            get {
                return (System.Net.Configuration.SmtpSection) GetSection("system.net/mailSettings/smtp", typeof(System.Net.Configuration.SmtpSection));
            }
        }

        internal AnonymousIdentificationSection AnonymousIdentification {
            get {
                return (AnonymousIdentificationSection) GetSection("system.web/anonymousIdentification", typeof(AnonymousIdentificationSection));
            }
        }

        internal ProtocolsSection Protocols {
            get {
                return (ProtocolsSection) GetSection("system.web/protocols", typeof(ProtocolsSection));
            }
        }

        internal AuthenticationSection Authentication {
            get {
                return (AuthenticationSection) GetSection("system.web/authentication", typeof(AuthenticationSection), ResultsIndex.Authentication);
            }
        }

        internal AuthorizationSection Authorization {
            get {
                return (AuthorizationSection) GetSection("system.web/authorization", typeof(AuthorizationSection), ResultsIndex.Authorization);
            }
        }

        // may return null
        internal HttpCapabilitiesDefaultProvider BrowserCaps {
            get {
                return (HttpCapabilitiesDefaultProvider) GetHandlerSection("system.web/browserCaps", typeof(HttpCapabilitiesDefaultProvider), ResultsIndex.BrowserCaps);
            }
        }

        internal ClientTargetSection ClientTarget {
            get {
                return (ClientTargetSection) GetSection("system.web/clientTarget", typeof(ClientTargetSection), ResultsIndex.ClientTarget);
            }
        }

        internal CompilationSection Compilation {
            get {
                return (CompilationSection) GetSection("system.web/compilation", typeof(CompilationSection), ResultsIndex.Compilation);
            }
        }

        internal CustomErrorsSection CustomErrors {
            get {
                return (CustomErrorsSection) GetSection("system.web/customErrors", typeof(CustomErrorsSection));
            }
        }

        internal GlobalizationSection Globalization {
            get {
                return (GlobalizationSection) GetSection("system.web/globalization", typeof(GlobalizationSection), ResultsIndex.Globalization);
            }
        }

        internal DeploymentSection Deployment {
            get {
                return (DeploymentSection) GetSection("system.web/deployment", typeof(DeploymentSection));
            }
        }

        internal FullTrustAssembliesSection FullTrustAssemblies {
            get {
                return (FullTrustAssembliesSection)GetSection("system.web/fullTrustAssemblies", typeof(FullTrustAssembliesSection));
            }
        }

        internal HealthMonitoringSection HealthMonitoring {
            get {
                return (HealthMonitoringSection) GetSection("system.web/healthMonitoring", typeof(HealthMonitoringSection));
            }
        }

        internal HostingEnvironmentSection HostingEnvironment {
            get {
                return (HostingEnvironmentSection) GetSection("system.web/hostingEnvironment", typeof(HostingEnvironmentSection));
            }
        }

        internal HttpCookiesSection HttpCookies {
            get {
                return (HttpCookiesSection) GetSection("system.web/httpCookies", typeof(HttpCookiesSection), ResultsIndex.HttpCookies);
            }
        }

        internal HttpHandlersSection HttpHandlers {
            get {
                return (HttpHandlersSection) GetSection("system.web/httpHandlers", typeof(HttpHandlersSection), ResultsIndex.HttpHandlers);
            }
        }

        internal HttpModulesSection HttpModules {
            get {
                return (HttpModulesSection) GetSection("system.web/httpModules", typeof(HttpModulesSection), ResultsIndex.HttpModules);
            }
        }

        internal HttpRuntimeSection HttpRuntime {
            get {
                return (HttpRuntimeSection) GetSection("system.web/httpRuntime", typeof(HttpRuntimeSection), ResultsIndex.HttpRuntime);
            }
        }

        internal IdentitySection Identity {
            get {
                return (IdentitySection) GetSection("system.web/identity", typeof(IdentitySection), ResultsIndex.Identity);
            }
        }

        internal MachineKeySection MachineKey {
            get {
                return (MachineKeySection) GetSection("system.web/machineKey", typeof(MachineKeySection), ResultsIndex.MachineKey);
            }
        }

        internal MembershipSection Membership {
            get {
                return (MembershipSection) GetSection("system.web/membership", typeof(MembershipSection), ResultsIndex.Membership);
            }
        }

        internal PagesSection Pages {
            get {
                return (PagesSection) GetSection("system.web/pages", typeof(PagesSection), ResultsIndex.Pages);
            }
        }

        internal PartialTrustVisibleAssembliesSection PartialTrustVisibleAssemblies {
            get
            {
                return (PartialTrustVisibleAssembliesSection)GetSection("system.web/partialTrustVisibleAssemblies", typeof(PartialTrustVisibleAssembliesSection));
            }
        }

        internal ProcessModelSection ProcessModel {
            get {
                return (ProcessModelSection) GetSection("system.web/processModel", typeof(ProcessModelSection));
            }
        }

        internal ProfileSection Profile {
            get {
                return (ProfileSection) GetSection("system.web/profile", typeof(ProfileSection), ResultsIndex.Profile);
            }
        }

        internal RoleManagerSection RoleManager {
            get {
                return (RoleManagerSection) GetSection("system.web/roleManager", typeof(RoleManagerSection));
            }
        }

        internal SecurityPolicySection SecurityPolicy {
            get {
                return (SecurityPolicySection) GetSection("system.web/securityPolicy", typeof(SecurityPolicySection));
            }
        }

        internal SessionPageStateSection SessionPageState {
            get {
                return (SessionPageStateSection) GetSection("system.web/sessionPageState", typeof(SessionPageStateSection), ResultsIndex.SessionPageState);
            }
        }

        internal SessionStateSection SessionState {
            get {
                return (SessionStateSection) GetSection("system.web/sessionState", typeof(SessionStateSection));
            }
        }

        internal SiteMapSection SiteMap {
            get {
                return (SiteMapSection) GetSection("system.web/siteMap", typeof(SiteMapSection));
            }
        }

        internal TraceSection Trace {
            get {
                return (TraceSection) GetSection("system.web/trace", typeof(TraceSection));
            }
        }

        internal TrustSection Trust {
            get {
                return (TrustSection) GetSection("system.web/trust", typeof(TrustSection));
            }
        }

        internal UrlMappingsSection UrlMappings {
            get {
                return (UrlMappingsSection) GetSection("system.web/urlMappings", typeof(UrlMappingsSection), ResultsIndex.UrlMappings);
            }
        }

        internal Hashtable WebControls {
            get {
                return (Hashtable)GetSection("system.web/webControls", typeof(Hashtable), ResultsIndex.WebControls);
            }
        }

        internal WebPartsSection WebParts {
            get {
                return (WebPartsSection) GetSection("system.web/webParts", typeof(WebPartsSection), ResultsIndex.WebParts);
            }
        }

        internal XhtmlConformanceSection XhtmlConformance {
            get {
                return (XhtmlConformanceSection) GetSection("system.web/xhtmlConformance", typeof(XhtmlConformanceSection), ResultsIndex.XhtmlConformance);
            }
        }

        internal CacheSection Cache {
            get {
                return (CacheSection) GetSection("system.web/caching/cache", typeof(CacheSection));
            }
        }

        internal OutputCacheSection OutputCache {
            get {
                return (OutputCacheSection) GetSection("system.web/caching/outputCache", typeof(OutputCacheSection), ResultsIndex.OutputCache);
            }
        }

        internal OutputCacheSettingsSection OutputCacheSettings {
            get {
                return (OutputCacheSettingsSection) GetSection("system.web/caching/outputCacheSettings", typeof(OutputCacheSettingsSection), ResultsIndex.OutputCacheSettings);
            }
        }

        internal SqlCacheDependencySection SqlCacheDependency {
            get {
                return (SqlCacheDependencySection) GetSection("system.web/caching/sqlCacheDependency", typeof(SqlCacheDependencySection));
            }
        }
     

        //////////////////////////////
        //
        // IMPLEMENTATION
        //
        //////////////////////////////

        // Wraps calls to RuntimeConfig.GetConfig() when 
        // the web config system is not being used.
        private static RuntimeConfig    s_clientRuntimeConfig;

        // Wraps calls to RuntimeConfig.GetConfig() when 
        // we must return null.
        private static RuntimeConfig    s_nullRuntimeConfig;

        // Wraps calls to RuntimeConfig.GetConfig() when 
        // we must return an error because there was an
        // unrecoverable error creating the config record.
        private static RuntimeConfig    s_errorRuntimeConfig;

        // object used to indicate that result has not been evaluated
        private static object           s_unevaluatedResult;

        // Commonly used results on every request. We cache these by index
        // into an array so we don't need to do hash table lookups,
        // type comparisons, and handle a demand for ConfigurationPermission
        // to retreive the config.
        internal enum ResultsIndex {
            // a valid index into the results array that is always unevaluated
            UNUSED = 0,             

            Authentication,
            Authorization,
            BrowserCaps,
            ClientTarget,
            Compilation,
            ConnectionStrings,
            Globalization,
            HttpCookies,
            HttpHandlers,
            HttpModules,
            HttpRuntime,
            Identity,
            MachineKey,
            Membership,
            OutputCache,
            OutputCacheSettings,
            Pages,
            Profile,
            SessionPageState,
            WebControls,
            WebParts,
            UrlMappings,
            XhtmlConformance,

            // size of the results array, must be last in list
            SIZE
        };

        // cached results
        // Per-path caching for perf reason.  Available only to internal components.
        private object[]                _results;   

        // LKG config 
        private RuntimeConfigLKG        _runtimeConfigLKG;

        // for http configuration, the ConfigurationRecord on which we call GetConfig
        protected IInternalConfigRecord _configRecord;

        // classes implementing LKG may return null from GetSectionObject
        private bool                    _permitNull;

        static RuntimeConfig() {
            s_unevaluatedResult = new object();

            // Ensure that we have an error config record available if we
            // get an unrecoverable error situation.
            GetErrorRuntimeConfig();
        }

        // ctor used by CachedPathData to wrap the ConfigurationRecord
        internal RuntimeConfig(IInternalConfigRecord configRecord) : this(configRecord, false) {}

        protected RuntimeConfig(IInternalConfigRecord configRecord, bool permitNull) {
            _configRecord = configRecord;
            _permitNull = permitNull;

            // initialize results cache
            _results = new object[(int)ResultsIndex.SIZE];
            for (int i = 0; i < _results.Length; i++) {
                _results[i] = s_unevaluatedResult;
            }
        }

        private RuntimeConfigLKG RuntimeConfigLKG {
            get {
                if (_runtimeConfigLKG == null) {
                    lock (this) {
                        if (_runtimeConfigLKG == null) {
                            _runtimeConfigLKG = new RuntimeConfigLKG(_configRecord);
                        }
                    }
                }
                
                return _runtimeConfigLKG;
            }
        }

        internal IInternalConfigRecord ConfigRecord {
            get {
                return _configRecord;
            }
        }

        // Create the single instance of the wrapper for ConfigurationManager configuration.
        static RuntimeConfig GetClientRuntimeConfig() {
            if (s_clientRuntimeConfig == null) {
                s_clientRuntimeConfig = new ClientRuntimeConfig();
            }

            return s_clientRuntimeConfig;
        }

        // Create the single instance of the wrapper for null configuration.
        static RuntimeConfig GetNullRuntimeConfig() {
            if (s_nullRuntimeConfig == null) {
                s_nullRuntimeConfig = new NullRuntimeConfig();
            }

            return s_nullRuntimeConfig;
        }

        // Create the single instance of the wrapper for error configuration.
        static internal RuntimeConfig GetErrorRuntimeConfig() {
            if (s_errorRuntimeConfig == null) {
                s_errorRuntimeConfig = new ErrorRuntimeConfig();
            }

            return s_errorRuntimeConfig;
        }

        // Get the config object for a section
        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        protected virtual object GetSectionObject(string sectionName) {
            return _configRecord.GetSection(sectionName);
        }


        //
        // Return a config implemented by IConfigurationHandler, 
        // and use the runtime cache to store it for quick retreival without
        // having to hit a config record and a demand for ConfigurationPermission.
        //
        private object GetHandlerSection(string sectionName, Type type, ResultsIndex index) {
            // check the results cache
            object result = _results[(int)index];
            if (result != s_unevaluatedResult) {
                return result;
            }

            // Get the configuration object.
            //
            // Note that it is legal for an IConfigurationSectionHandler implementation 
            // to return null.
            result = GetSectionObject(sectionName);

            // verify the object is of the expected type
            if (result != null && result.GetType() != type) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_unable_to_get_section, sectionName));
            }

            // store into results cache
            if (index != ResultsIndex.UNUSED) {
                _results[(int)index] = result;
            }

            return result;
        }

        //
        // Return a configuration section without checking the runtime cache.
        //
        private object GetSection(string sectionName, Type type) {
            return GetSection(sectionName, type, ResultsIndex.UNUSED);
        }

        //
        // Return a configuration section, and use the runtime cache to store it for
        // quick retreival without having to hit a config record and a demand for
        // ConfigurationPermission.
        //
        private object GetSection(string sectionName, Type type, ResultsIndex index) {
            // check the results cache
            object result = _results[(int)index];
            if (result != s_unevaluatedResult) {
                return result;
            }

            // get the configuration object
            result = GetSectionObject(sectionName);
            if (result == null) {
                // A section implemented by ConfigurationSection may not return null,
                // but various error handling subclasses of RuntimeConfig may need it.
                // Throw an error if null is not permitted.
                if (!_permitNull) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_unable_to_get_section, sectionName));
                }
            }
            else {
                // verify the object is of the expected type
                if (result.GetType() != type) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_unable_to_get_section, sectionName));
                }
            }

            // store into results cache
            if (index != ResultsIndex.UNUSED) {
                _results[(int)index] = result;
            }

            return result;
        }

        //
        // There are extreme cases where we cannot even retreive the CachedPathData
        // for a path - such as when MapPath deems the path to be suspicious.
        // In these cases, walk the hierarchy upwards until we are able to retreive
        // a CachedPathData and its associated RuntimeConfig.
        //
        static private RuntimeConfig GetLKGRuntimeConfig(VirtualPath path) {
            try {
                // Start with the parent of the path. 
                path = path.Parent;
            }
            catch {
                path = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPathObject;
            }

            // Walk the path hierarchy until we can succesfully get a RuntimeConfig.
            while (path != null) {
                try {
                    return GetConfig(path);
                }
                catch {
                    path = path.Parent;
                }
            }
 
            try {
                return GetRootWebConfig();
            }
            catch {
            }

            try {
                return GetMachineConfig();
            }
            catch {
            }

            return GetNullRuntimeConfig();
        }
    }
}
