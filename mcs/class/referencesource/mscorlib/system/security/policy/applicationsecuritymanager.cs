// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// ApplicationSecurityManager.cs
//

namespace System.Security.Policy {
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    [System.Runtime.InteropServices.ComVisible(true)]
    public static class ApplicationSecurityManager {
        private static volatile IApplicationTrustManager m_appTrustManager = null;

        //
        // Public static methods.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        static ApplicationSecurityManager()
        {
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [SecurityPermissionAttribute(SecurityAction.Assert, Unrestricted=true)]
        public static bool DetermineApplicationTrust (ActivationContext activationContext, TrustManagerContext context) {
            if (activationContext == null)
                throw new ArgumentNullException("activationContext");
            Contract.EndContractBlock();

            ApplicationTrust appTrust = null;
            AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
            if (domainManager != null) {
                HostSecurityManager securityManager = domainManager.HostSecurityManager;
                if ((securityManager != null) && ((securityManager.Flags & HostSecurityManagerOptions.HostDetermineApplicationTrust) == HostSecurityManagerOptions.HostDetermineApplicationTrust)) {
                    appTrust = securityManager.DetermineApplicationTrust(CmsUtils.MergeApplicationEvidence(null, activationContext.Identity, activationContext, null), null, context);
                    if (appTrust == null)
                        return false;
                    return appTrust.IsApplicationTrustedToRun;
                }
            }

            appTrust = DetermineApplicationTrustInternal(activationContext, context);
            if (appTrust == null)
                return false;
            return appTrust.IsApplicationTrustedToRun;
        }

        //
        // Public static properties.
        //

        public static ApplicationTrustCollection UserApplicationTrusts {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
            get {
                return new ApplicationTrustCollection(true);
            }
        }

        public static IApplicationTrustManager ApplicationTrustManager {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
            get {
                if (m_appTrustManager == null) {
                    m_appTrustManager = DecodeAppTrustManager();
                    if (m_appTrustManager == null)
                        throw new PolicyException(Environment.GetResourceString("Policy_NoTrustManager"));
                }
                return m_appTrustManager;
            }
        }

        //
        // Internal
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static ApplicationTrust DetermineApplicationTrustInternal (ActivationContext activationContext, TrustManagerContext context) {
            ApplicationTrust trust = null;
            ApplicationTrustCollection userTrusts = new ApplicationTrustCollection(true);

            // See if there is a persisted trust decision for this application.
            if ((context == null || !context.IgnorePersistedDecision)) {
                trust = userTrusts[activationContext.Identity.FullName];
                if (trust != null)
                    return trust;
            }

            // There is no cached trust decision so invoke the trust manager.
            trust = ApplicationTrustManager.DetermineApplicationTrust(activationContext, context);
            if (trust == null)
                trust = new ApplicationTrust(activationContext.Identity);
            // make sure the application identity is correctly set.
            trust.ApplicationIdentity = activationContext.Identity;
            if (trust.Persist)
                userTrusts.Add(trust);

            return trust;
        }

        //
        // Private.
        //

        private static string s_machineConfigFile = Config.MachineDirectory + "applicationtrust.config";

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static IApplicationTrustManager DecodeAppTrustManager () {
            if (!File.InternalExists(s_machineConfigFile))
                goto defaultTrustManager;

            // A config file exists. Decode the trust manager from its Xml.
            String configFileStr;
            using (FileStream contents = new FileStream(s_machineConfigFile, FileMode.Open, FileAccess.Read))
            {
                configFileStr = new StreamReader(contents).ReadToEnd();
            }

            SecurityElement elRoot = SecurityElement.FromString(configFileStr);
            SecurityElement elMscorlib = elRoot.SearchForChildByTag("mscorlib");
            if (elMscorlib == null)
                goto defaultTrustManager;
            SecurityElement elSecurity = elMscorlib.SearchForChildByTag("security");
            if (elSecurity == null)
                goto defaultTrustManager;
            SecurityElement elPolicy = elSecurity.SearchForChildByTag("policy");
            if (elPolicy == null)
                goto defaultTrustManager;
            SecurityElement elSecurityManager = elPolicy.SearchForChildByTag("ApplicationSecurityManager");
            if (elSecurityManager == null)
                goto defaultTrustManager;
            SecurityElement elTrustManager = elSecurityManager.SearchForChildByTag("IApplicationTrustManager");
            if (elTrustManager == null)
                goto defaultTrustManager;
            IApplicationTrustManager appTrustManager = DecodeAppTrustManagerFromElement(elTrustManager);
            if (appTrustManager == null)
                goto defaultTrustManager;
            return appTrustManager;

defaultTrustManager:
            return DecodeAppTrustManagerFromElement(CreateDefaultApplicationTrustManagerElement());
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static SecurityElement CreateDefaultApplicationTrustManagerElement() {
            SecurityElement elTrustManager = new SecurityElement("IApplicationTrustManager");
            elTrustManager.AddAttribute("class",
                                        "System.Security.Policy.TrustManager, System.Windows.Forms, Version=" + ((RuntimeAssembly)Assembly.GetExecutingAssembly()).GetVersion() + ", Culture=neutral, PublicKeyToken=" + AssemblyRef.EcmaPublicKeyToken);
            elTrustManager.AddAttribute("version", "1");
            return elTrustManager;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static IApplicationTrustManager DecodeAppTrustManagerFromElement (SecurityElement elTrustManager) {
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            string trustManagerName = elTrustManager.Attribute("class");
            Type tmClass = Type.GetType(trustManagerName, false, false);
            if (tmClass == null)
                return null;

            IApplicationTrustManager appTrustManager = Activator.CreateInstance(tmClass) as IApplicationTrustManager;
            if (appTrustManager != null)
                appTrustManager.FromXml(elTrustManager);
            return appTrustManager;
        }
    }
}
