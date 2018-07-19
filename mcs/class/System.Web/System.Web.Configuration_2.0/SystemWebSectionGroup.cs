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


using System;
using System.Configuration;
using System.Web.Services.Configuration;

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

		[ConfigurationProperty ("authorization")]
		public AuthorizationSection Authorization {
			get { return (AuthorizationSection)Sections ["authorization"]; }
		}

		[ConfigurationProperty ("browserCaps")]
		public DefaultSection BrowserCaps {
			get { return (DefaultSection)Sections ["browserCaps"]; }
		}

		[ConfigurationProperty ("clientTarget")]
		public ClientTargetSection ClientTarget {
			get { return (ClientTargetSection)Sections ["clientTarget"]; }
		}

		[ConfigurationProperty ("compilation")]
		public CompilationSection Compilation {
			get { return (CompilationSection)Sections ["compilation"]; }
		}

		[ConfigurationProperty ("customErrors")]
		public CustomErrorsSection CustomErrors {
			get { return (CustomErrorsSection)Sections ["customErrors"]; }
		}

		[ConfigurationProperty ("deployment")]
		public DeploymentSection Deployment {
			get { return (DeploymentSection)Sections ["deployment"]; }
		}

		[ConfigurationProperty ("deviceFilters")]
		public DefaultSection DeviceFilters {
			get { return (DefaultSection)Sections ["deviceFilters"]; }
		}

		[ConfigurationProperty ("globalization")]
		public GlobalizationSection Globalization {
			get { return (GlobalizationSection)Sections ["globalization"]; }
		}

		[ConfigurationProperty ("healthMonitoring")]
		public HealthMonitoringSection HealthMonitoring {
			get { return (HealthMonitoringSection)Sections ["healthMonitoring"]; }
		}

		[ConfigurationProperty ("hostingEnvironment")]
		public HostingEnvironmentSection HostingEnvironment {
			get { return (HostingEnvironmentSection)Sections ["hostingEnvironment"]; }
		}

		[ConfigurationProperty ("httpCookies")]
		public HttpCookiesSection HttpCookies {
			get { return (HttpCookiesSection)Sections ["httpCookies"]; }
		}

		[ConfigurationProperty ("httpHandlers")]
		public HttpHandlersSection HttpHandlers {
			get { return (HttpHandlersSection)Sections ["httpHandlers"]; }
		}

		[ConfigurationProperty ("httpModules")]
		public HttpModulesSection HttpModules {
			get { return (HttpModulesSection)Sections ["httpModules"]; }
		}

		[ConfigurationProperty ("httpRuntime")]
		public HttpRuntimeSection HttpRuntime {
			get { return (HttpRuntimeSection)Sections ["httpRuntime"]; }
		}

		[ConfigurationProperty ("identity")]
		public IdentitySection Identity {
			get { return (IdentitySection)Sections ["identity"]; }
		}

		[ConfigurationProperty ("machineKey")]
		public MachineKeySection MachineKey {
			get { return (MachineKeySection)Sections ["machineKey"]; }
		}

		[ConfigurationProperty ("membership")]
		public MembershipSection Membership {
			get { return (MembershipSection)Sections ["membership"]; }
		}

		[ConfigurationProperty ("mobileControls")]
		[Obsolete ("System.Web.Mobile.dll is obsolete.")]
		public ConfigurationSection MobileControls {
			get { return Sections ["MobileControls"]; }
		}

		[ConfigurationProperty ("pages")]
		public PagesSection Pages {
			get { return (PagesSection)Sections ["pages"]; }
		}

		[ConfigurationProperty ("processModel")]
		public ProcessModelSection ProcessModel {
			get { return (ProcessModelSection)Sections ["processModel"]; }
		}

		[ConfigurationProperty ("profile")]
		public ProfileSection Profile {
			get { return (ProfileSection)Sections ["profile"]; }
		}

		[ConfigurationProperty ("protocols")]
		public DefaultSection Protocols {
			get { return (DefaultSection)Sections ["protocols"]; }
		}

		[ConfigurationProperty ("roleManager")]
		public RoleManagerSection RoleManager {
			get { return (RoleManagerSection)Sections ["roleManager"]; }
		}

		[ConfigurationProperty ("securityPolicy")]
		public SecurityPolicySection SecurityPolicy {
			get { return (SecurityPolicySection)Sections ["securityPolicy"]; }
		}

		[ConfigurationProperty ("sessionState")]
		public SessionStateSection SessionState {
			get { return (SessionStateSection)Sections ["sessionState"]; }
		}

		[ConfigurationProperty ("siteMap")]
		public SiteMapSection SiteMap {
			get { return (SiteMapSection)Sections ["siteMap"]; }
		}

		[ConfigurationProperty ("trace")]
		public TraceSection Trace {
			get { return (TraceSection)Sections ["trace"]; }
		}

		[ConfigurationProperty ("trust")]
		public TrustSection Trust {
			get { return (TrustSection)Sections ["trust"]; }
		}

		[ConfigurationProperty ("urlMappings")]
		public UrlMappingsSection UrlMappings {
			get { return (UrlMappingsSection)Sections ["urlMappings"]; }
		}

		[ConfigurationProperty ("webControls")]
		public WebControlsSection WebControls {
			get { return (WebControlsSection)Sections ["webControls"]; }
		}

		[ConfigurationProperty ("webParts")]
		public WebPartsSection WebParts {
			get { return (WebPartsSection)Sections ["webParts"]; }
		}

		[ConfigurationProperty ("webServices")]
		public WebServicesSection WebServices {
			get { return (WebServicesSection)Sections ["webServices"]; }
		}

		[ConfigurationProperty ("xhtmlConformance")]
		public XhtmlConformanceSection XhtmlConformance {
			get { return (XhtmlConformanceSection)Sections ["xhtmlConformance"]; }
		}
	}
}

