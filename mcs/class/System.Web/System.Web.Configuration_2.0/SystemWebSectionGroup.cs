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
#if notyet
		public AnonymousIdentificationSection AnonymousIdentification {
			get { return (AnonymousIdenficationSection)Sections ["anonymousIdentification"]; }
		}
#endif

		[ConfigurationProperty ("authentication")]
		public AuthenticationSection Authentication {
			get { return (AuthenticationSection)Sections ["authentication"]; }
		}

#if notyet
		public AuthorizationSection Authorization {
			get { return (AuthorizationSection)Sections ["authorization"]; }
		}
#endif

#if notyet
		public DefaultSection BrowserCaps {
			get { return (DefaultSection)Sections ["browserCaps"]; }
		}
#endif

#if notyet
		public ClientTargetSection ClientTarget {
			get { return (ClientTargetSection)Sections ["clientTarget"]; }
		}
#endif

		[ConfigurationProperty ("compilation")]
		public CompilationSection Compilation {
			get { return (CompilationSection)Sections ["compilation"]; }
		}

#if notyet
		public CustomErrorsSection CustomErrors {
			get { return (CustomErrorsSection)Sections ["customErrors"]; }
		}
#endif

#if notyet
		public DeploymentSection Deployment {
			get { return (DeploymentSection)Sections ["deployment"]; }
		}
#endif

#if notyet
		public DefaultSection DeviceFilters {
			get { return (DefaultSection)Sections ["deviceFilters"]; }
		}
#endif

#if notyet
		public GlobalizationSection Globalization {
			get { return (GlobalizationSection)Sections ["globalization"]; }
		}
#endif

#if notyet
		public HealthMonitoringSection HealthMonitoring {
			get { return (HealthMonitoringSection)Sections ["healthMonitoring"]; }
		}
#endif

#if notyet
		public HostingEnvironmentSection HostingEnvironment {
			get { return (HostingEnvironment)Sections ["hostingEnvironment"]; }
		}
#endif

#if notyet
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
		public HttpRuntimeSection HttpRuntime {
			get { return (HttpRuntimeSection)Sections ["httpRuntime"]; }
		}
#endif

#if notyet
		public IdentitySection Identity {
			get { return (IdentitySection)Sections ["identity"]; }
		}
#endif

#if notyet
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

#if notyet
		public ProfileSection Profile {
			get { return (ProfileSection)Sections ["profile"]; }
		}
#endif

#if notyet
		public DefaultSection Protocols {
			get { return (DefaultSection)Sections ["protocols"]; }
		}
#endif

#if notyet
		public RoleManagerSection RoleManager {
			get { return (RoleManagerSection)Sections ["roleManager"]; }
		}
#endif

#if notyet
		public SecurityPolicySection SecurityPolicy {
			get { return (SecurityPolicySection)Sections ["securityPolicy"]; }
		}
#endif

#if notyet
		public SessionStateSection SessionState {
			get { return (SessionStateSection)Sections ["sessionState"]; }
		}
#endif

		[ConfigurationProperty ("siteMap")]
		public SiteMapSection SiteMap {
			get { return (SiteMapSection)Sections ["siteMap"]; }
		}

#if notyet
		public TraceSection Trace {
			get { return (TraceSection)Sections ["trace"]; }
		}
#endif

#if notyet
		public TrustSection Trust {
			get { return (TrustSection)Sections ["trust"]; }
		}
#endif

#if notyet
		public UrlMappingsSection UrlMappings {
			get { return (UrlMappingsSection)Sections ["urlMappings"]; }
		}
#endif

#if notyet
		public WebControlsSection WebControls {
			get { return (WebControlsSection)Sections ["webControls"]; }
		}
#endif

#if notyet
		public WebPartsSection WebParts {
			get { return (WebPartsSection)Sections ["webParts"]; }
		}
#endif

#if notyet
		public WebServicesSection WebServices {
			get { return (WebServicesSection)Sections ["webServices"]; }
		}
#endif

#if notyet
		public XhtmlConformanceSection XhtmlConformance {
			get { return (XhtmlConformanceSection)Sections ["xhtmlConformance"]; }
		}
#endif
	}
}

#endif
