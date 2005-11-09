//
// System.Web.Configuration.SystemWebSectionGroup
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class SystemWebSectionGroup : ConfigurationSectionGroup
	{
		[ConfigurationProperty ("anonymousIdentification")]
		public AnonymousIdentificationSection AnonymousIdentification {
			get { return (AnonymousIdentificationSection)Sections ["anonymousIdentification"]; }
		}

		[ConfigurationProperty ("authentication")]
		public AuthenticationSection Authentication {
			get { return (AuthenticationSection)Sections ["authentication"]; }
		}

#if notyet
		[ConfigurationProperty ("authorization")]
		public AuthorizationSection Authorization {
			get { return (AuthorizationSection)Sections ["authorization"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("browserCaps")]
		public DefaultSection BrowserCaps {
			get { return (DefaultSection)Sections ["browserCaps"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("clientTarget")]
		public ClientTargetSection ClientTarget {
			get { return (ClientTargetSection)Sections ["clientTarget"]; }
		}
#endif

		[ConfigurationProperty ("compilation")]
		public CompilationSection Compilation {
			get { return (CompilationSection)Sections ["compilation"]; }
		}

#if notyet
		[ConfigurationProperty ("customErrors")]
		public CustomErrorsSection CustomErrors {
			get { return (CustomErrorsSection)Sections ["customErrors"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("deployment")]
		public DeploymentSection Deployment {
			get { return (DeploymentSection)Sections ["deployment"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("deviceFilters")]
		public DefaultSection DeviceFilters {
			get { return (DefaultSection)Sections ["deviceFilters"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("globalization")]
		public GlobalizationSection Globalization {
			get { return (GlobalizationSection)Sections ["globalization"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("healthMonitoring")]
		public HealthMonitoringSection HealthMonitoring {
			get { return (HealthMonitoringSection)Sections ["healthMonitoring"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("hostingEnvironment")]
		public HostingEnvironmentSection HostingEnvironment {
			get { return (HostingEnvironment)Sections ["hostingEnvironment"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("httpCookies")]
		public HttpCookiesSection HttpCookies {
			get { return (HttpCookiesSection)Sections ["httpCookies"]; }
		}
#endif

		[ConfigurationProperty ("httpHandlers")]
		public HttpHandlersSection HttpHandlers {
			get { return (HttpHandlersSection)Sections ["httpHandlers"]; }
		}

		[ConfigurationProperty ("httpModules")]
		public HttpModulesSection HttpModules {
			get { return (HttpModulesSection)Sections ["httpModules"]; }
		}

#if notyet
		[ConfigurationProperty ("httpRuntime")]
		public HttpRuntimeSection HttpRuntime {
			get { return (HttpRuntimeSection)Sections ["httpRuntime"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("identity")]
		public IdentitySection Identity {
			get { return (IdentitySection)Sections ["identity"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("machineKey")]
		public MachineKeySection MachineKey {
			get { return (MachineKeySection)Sections ["machineKey"]; }
		}
#endif

		[ConfigurationProperty ("membership")]
		public MembershipSection Membership {
			get { return (MembershipSection)Sections ["membership"]; }
		}

		[ConfigurationProperty ("mobileControls")]
		public ConfigurationSection MobileControls {
			get { return Sections ["MobileControls"]; }
		}

		[ConfigurationProperty ("pages")]
		public PagesSection Pages {
			get { return (PagesSection)Sections ["pages"]; }
		}

#if notyet
		public ProcessModelSection ProcessModel {
			get { return (ProcessModelSection)Sections ["processModel"]; }
		}
#endif

		[ConfigurationProperty ("profile")]
		public ProfileSection Profile {
			get { return (ProfileSection)Sections ["profile"]; }
		}

#if notyet
		[ConfigurationProperty ("protocols")]
		public DefaultSection Protocols {
			get { return (DefaultSection)Sections ["protocols"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("roleManager")]
		public RoleManagerSection RoleManager {
			get { return (RoleManagerSection)Sections ["roleManager"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("securityPolicy")]
		public SecurityPolicySection SecurityPolicy {
			get { return (SecurityPolicySection)Sections ["securityPolicy"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("sessionState")]
		public SessionStateSection SessionState {
			get { return (SessionStateSection)Sections ["sessionState"]; }
		}
#endif

		[ConfigurationProperty ("siteMap")]
		public SiteMapSection SiteMap {
			get { return (SiteMapSection)Sections ["siteMap"]; }
		}

#if notyet
		[ConfigurationProperty ("trace")]
		public TraceSection Trace {
			get { return (TraceSection)Sections ["trace"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("trust")]
		public TrustSection Trust {
			get { return (TrustSection)Sections ["trust"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("urlMappings")]
		public UrlMappingsSection UrlMappings {
			get { return (UrlMappingsSection)Sections ["urlMappings"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("webControls")]
		public WebControlsSection WebControls {
			get { return (WebControlsSection)Sections ["webControls"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("webParts")]
		public WebPartsSection WebParts {
			get { return (WebPartsSection)Sections ["webParts"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("webServices")]
		public WebServicesSection WebServices {
			get { return (WebServicesSection)Sections ["webServices"]; }
		}
#endif

#if notyet
		[ConfigurationProperty ("xhtmlConformance")]
		public XhtmlConformanceSection XhtmlConformance {
			get { return (XhtmlConformanceSection)Sections ["xhtmlConformance"]; }
		}
#endif
	}
}

#endif
