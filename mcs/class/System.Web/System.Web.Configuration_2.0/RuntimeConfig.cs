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
		static internal RuntimeConfig GetConfig() {
			return new RuntimeConfig();
		}

		static internal RuntimeConfig GetConfig(HttpContext context) {
			// we currently always use WebConfigurationManager directly
			return GetConfig();
		}

		static internal RuntimeConfig GetAppConfig() {
			// we currently always use WebConfigurationManager directly
			return GetConfig();
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

		// unimplemented
		//internal ProtocolsSection Protocols {
		//    get {
		//        return (ProtocolsSection) GetSection("system.web/protocols", typeof(ProtocolsSection));
		//    }
		//}

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

		// unimplemented
		//internal FullTrustAssembliesSection FullTrustAssemblies {
		//    get {
		//        return (FullTrustAssembliesSection)GetSection("system.web/fullTrustAssemblies", typeof(FullTrustAssembliesSection));
		//    }
		//}

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

		// unimplemented
		//internal PartialTrustVisibleAssembliesSection PartialTrustVisibleAssemblies {
		//    get
		//    {
		//        return (PartialTrustVisibleAssembliesSection)GetSection("system.web/partialTrustVisibleAssemblies", typeof(PartialTrustVisibleAssembliesSection));
		//    }
		//}

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

		protected RuntimeConfig() {
			s_unevaluatedResult = new object();

			// initialize results cache
			_results = new object[(int)ResultsIndex.SIZE];
			for (int i = 0; i < _results.Length; i++) {
				_results[i] = s_unevaluatedResult;
			}
		}

		// Get the config object for a section
		[ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
		protected virtual object GetSectionObject(string sectionName) {
			return WebConfigurationManager.GetSection(sectionName);
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
				throw new ConfigurationErrorsException(SR.GetString(SR.Config_unable_to_get_section, sectionName));
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

	}
}
