//------------------------------------------------------------------------------
// <copyright file="SystemWebSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Configuration;
    using System.Security.Permissions;

    public sealed class SystemWebSectionGroup : ConfigurationSectionGroup {

        public SystemWebSectionGroup() {
        }

        // public properties
        [ConfigurationProperty("anonymousIdentification")]
        public AnonymousIdentificationSection AnonymousIdentification {
            get {
                return (AnonymousIdentificationSection) Sections["anonymousIdentification"];
            }
        }

        [ConfigurationProperty("authentication")]
        public AuthenticationSection Authentication {
            get {
                return (AuthenticationSection) Sections["authentication"];
            }
        }

        [ConfigurationProperty("authorization")]
        public AuthorizationSection Authorization {
            get {
                return (AuthorizationSection) Sections["authorization"];
            }
        }

        [ConfigurationProperty("browserCaps")]
        public DefaultSection BrowserCaps {
            get {
                return (DefaultSection) Sections["browserCaps"];
            }
        }

        [ConfigurationProperty("clientTarget")]
        public ClientTargetSection ClientTarget {
            get {
                return (ClientTargetSection) Sections["clientTarget"];
            }
        }

        [ConfigurationProperty("compilation")]
        public CompilationSection Compilation {
            get {
                return (CompilationSection) Sections["compilation"];
            }
        }

        [ConfigurationProperty("customErrors")]
        public CustomErrorsSection CustomErrors {
            get {
                return (CustomErrorsSection) Sections["customErrors"];
            }
        }

        [ConfigurationProperty("deployment")]
        public DeploymentSection Deployment {
            get {
                return (DeploymentSection) Sections["deployment"];
            }
        }

        [ConfigurationProperty("deviceFilters")]
        public DefaultSection DeviceFilters {
            get {
                return (DefaultSection) Sections["deviceFilters"];
            }
        }

        [ConfigurationProperty("fullTrustAssemblies")]
        public FullTrustAssembliesSection FullTrustAssemblies {
            get {
                return (FullTrustAssembliesSection)Sections["fullTrustAssemblies"];
            }
        }

        [ConfigurationProperty("globalization")]
        public GlobalizationSection Globalization {
            get {
                return (GlobalizationSection) Sections["globalization"];
            }
        }

        [ConfigurationProperty("healthMonitoring")]
        public HealthMonitoringSection HealthMonitoring {
            get {
                return (HealthMonitoringSection) Sections["healthMonitoring"];
            }
        }

        [ConfigurationProperty("hostingEnvironment")]
        public HostingEnvironmentSection HostingEnvironment {
            get {
                return (HostingEnvironmentSection) Sections["hostingEnvironment"];
            }
        }

        [ConfigurationProperty("httpCookies")]
        public HttpCookiesSection HttpCookies {
            get {
                return (HttpCookiesSection) Sections["httpCookies"];
            }
        }

        [ConfigurationProperty("httpHandlers")]
        public HttpHandlersSection HttpHandlers {
            get {
                return (HttpHandlersSection) Sections["httpHandlers"];
            }
        }

        [ConfigurationProperty("httpModules")]
        public HttpModulesSection HttpModules {
            get {
                return (HttpModulesSection) Sections["httpModules"];
            }
        }

        [ConfigurationProperty("httpRuntime")]
        public HttpRuntimeSection HttpRuntime {
            get {
                return (HttpRuntimeSection) Sections["httpRuntime"];
            }
        }

        [ConfigurationProperty("identity")]
        public  IdentitySection Identity {
            get {
                return (IdentitySection) Sections["identity"];
            }
        }

        [ConfigurationProperty("machineKey")]
        public MachineKeySection MachineKey {
            get {
                return (MachineKeySection) Sections["machineKey"];
            }
        }

        [ConfigurationProperty("membership")]
        public MembershipSection Membership {
            get {
                return (MembershipSection) Sections["membership"];
            }
        }

        // Note that the return type is ConfigurationSection, not MobileControlsSection.
        // The reason is that we don't want to link to System.Web.UI.MobileControls just
        // to return the correct type of this property.
        [ConfigurationProperty("mobileControls")]
        [Obsolete("System.Web.Mobile.dll is obsolete.")]
        public ConfigurationSection MobileControls {
            get {
                return (ConfigurationSection) Sections["mobileControls"];
            }
        }

        [ConfigurationProperty("pages")]
        public PagesSection Pages {
            get {
                return (PagesSection) Sections["pages"];
            }
        }

        [ConfigurationProperty("partialTrustVisibleAssemblies")]
        public PartialTrustVisibleAssembliesSection PartialTrustVisibleAssemblies {
            get
            {
                return (PartialTrustVisibleAssembliesSection)Sections["partialTrustVisibleAssemblies"];
            }
        }

        [ConfigurationProperty("processModel")]
        public ProcessModelSection ProcessModel {
            get {
                return (ProcessModelSection) Sections["processModel"];
            }
        }

        [ConfigurationProperty("profile")]
        public ProfileSection Profile {
            get {
                return (ProfileSection)Sections["profile"];
            }
        }

        [ConfigurationProperty("protocols")]
        public DefaultSection Protocols {
            get {
                return (DefaultSection)Sections["protocols"];
            }
        }

        [ConfigurationProperty("roleManager")]
        public RoleManagerSection RoleManager {
            get {
                return (RoleManagerSection) Sections["roleManager"];
            }
        }

        [ConfigurationProperty("securityPolicy")]
        public SecurityPolicySection SecurityPolicy {
            get {
                return (SecurityPolicySection) Sections["securityPolicy"];
            }
        }

        [ConfigurationProperty("sessionState")]
        public SessionStateSection SessionState {
            get {
                return (SessionStateSection) Sections["sessionState"];
            }
        }

        [ConfigurationProperty("siteMap")]
        public SiteMapSection SiteMap {
            get {
                return (SiteMapSection) Sections["siteMap"];
            }
        }

        [ConfigurationProperty("trace")]
        public TraceSection Trace {
            get {
                return (TraceSection) Sections["trace"];
            }
        }

        [ConfigurationProperty("trust")]
        public TrustSection Trust {
            get {
                return (TrustSection) Sections["trust"];
            }
        }

        [ConfigurationProperty("urlMappings")]
        public UrlMappingsSection UrlMappings {
            get {
                return (UrlMappingsSection) Sections["urlMappings"];
            }
        }

        [ConfigurationProperty("webControls")]
        public WebControlsSection WebControls {
            get {
                return (WebControlsSection) Sections["webControls"];
            }
        }

        [ConfigurationProperty("webParts")]
        public WebPartsSection WebParts {
            get {
                return (WebPartsSection) Sections["WebParts"];
            }
        }

        [ConfigurationProperty("webServices")]
        public System.Web.Services.Configuration.WebServicesSection WebServices {
            get {
                return (System.Web.Services.Configuration.WebServicesSection) Sections["webServices"];
            }
        }

        [ConfigurationProperty("xhtmlConformance")]
        public XhtmlConformanceSection XhtmlConformance {
            get {
                return (XhtmlConformanceSection) Sections["xhtmlConformance"];
            }
        }
    }
}
