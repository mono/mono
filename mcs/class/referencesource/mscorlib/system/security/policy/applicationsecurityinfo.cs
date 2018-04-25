// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// ApplicationSecurityInfo.cs
//
// The application security info holds all the security related information pertinent
// to the application. In some sense, it is the CLR public representation of the security
// information held in the manifest.
//

namespace System.Security.Policy {
    using System.Collections;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Threading;
    using System.Runtime.Versioning;
    using System.Runtime.Hosting;
    using System.Diagnostics.Contracts;
    
    [System.Security.SecurityCritical]  // auto-generated
    [SecurityPermissionAttribute(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class ApplicationSecurityInfo {
        private ActivationContext m_context;
        private object m_appId;
        private object m_deployId;
        private object m_defaultRequest;
        private object m_appEvidence;

        internal ApplicationSecurityInfo () {}

        //
        // Public.
        //

        public ApplicationSecurityInfo (ActivationContext activationContext) {
            if (activationContext == null)
                throw new ArgumentNullException("activationContext");
            Contract.EndContractBlock();
            m_context = activationContext;
        }

        public ApplicationId ApplicationId {
            get {
                if (m_appId == null && m_context != null) {
                    ICMS appManifest = m_context.ApplicationComponentManifest;
                    ApplicationId appId = ParseApplicationId(appManifest);
                    Interlocked.CompareExchange(ref m_appId, appId, null);
                }
                return m_appId as ApplicationId;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                m_appId = value;
            }
        }

        public ApplicationId DeploymentId {
            get {
                if (m_deployId == null && m_context != null) {
                    ICMS deplManifest = m_context.DeploymentComponentManifest;
                    ApplicationId deplId = ParseApplicationId(deplManifest);
                    Interlocked.CompareExchange(ref m_deployId, deplId, null);
                }
                return m_deployId as ApplicationId;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                m_deployId = value;
            }
        }

        public PermissionSet DefaultRequestSet {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (m_defaultRequest == null) {
                    PermissionSet defaultRequest = new PermissionSet(PermissionState.None);
                    if (m_context != null) {
                        // read the default request from the app manifest.
                        ICMS appManifest = m_context.ApplicationComponentManifest;
                        string defaultPSetId = ((IMetadataSectionEntry) appManifest.MetadataSectionEntry).defaultPermissionSetID;
                        object permissionSetObj = null;
                        if (defaultPSetId != null && defaultPSetId.Length > 0) {
                            ((ISectionWithStringKey) appManifest.PermissionSetSection).Lookup(defaultPSetId, out permissionSetObj);
                            IPermissionSetEntry defaultPSet = permissionSetObj as IPermissionSetEntry;
                            if (defaultPSet != null) {
                                SecurityElement seDefaultPS = SecurityElement.FromString(defaultPSet.AllData.XmlSegment);
                                string unrestricted = seDefaultPS.Attribute("temp:Unrestricted");
                                if (unrestricted != null)
                                    seDefaultPS.AddAttribute("Unrestricted", unrestricted);

                                // Look for "SameSite" request.
                                string sameSite = seDefaultPS.Attribute("SameSite");
                                if (String.Compare(sameSite, "Site", StringComparison.OrdinalIgnoreCase) == 0) {
                                    Url url = new Url(m_context.Identity.CodeBase);
                                    URLString urlString = url.GetURLString();

                                    // Create a same site web permission for HTTP deployed applications. We'll
                                    // always use a v2.0 WebPermission for this because this XML is loadable
                                    // on all versions of the framework that support ClickOnce.  This allows
                                    // newer versions of the framework to create ApplicationSecurityInfo objects
                                    // that may eventually be used by applications running against older versions
                                    // of the framework.
                                    NetCodeGroup netCodeGroup = new NetCodeGroup(new AllMembershipCondition());
                                    SecurityElement webPermission =
                                        netCodeGroup.CreateWebPermission(urlString.Host,
                                                                         urlString.Scheme,
                                                                         urlString.Port,
                                                                         "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=" + AssemblyRef.EcmaPublicKeyToken);

                                    if (webPermission != null) {
                                        seDefaultPS.AddChild(webPermission);
                                    }

                                    if (String.Compare("file:", 0, m_context.Identity.CodeBase, 0, 5, StringComparison.OrdinalIgnoreCase) == 0) {
                                        FileCodeGroup fileCodeGroup = new FileCodeGroup(new AllMembershipCondition(), FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery);
                                        PolicyStatement ps = fileCodeGroup.CalculatePolicy(url);
                                        if (ps != null) {
                                            PermissionSet filePermissionSet = ps.PermissionSet;
                                            if (filePermissionSet != null) {
                                                seDefaultPS.AddChild(filePermissionSet.GetPermission(typeof(FileIOPermission)).ToXml());
                                            }
                                        }
                                    }
                                }

                                // We need to use a ReadOnlyPermissionSet to ensure that any permissions in
                                // the manifest which were created on a previous runtime are stored back to
                                // the application store in a format that the previous runtime can understand.
                                defaultRequest = new ReadOnlyPermissionSet(seDefaultPS);
                            }
                        }
                    }
                    Interlocked.CompareExchange(ref m_defaultRequest, defaultRequest, null);
                }
                return m_defaultRequest as PermissionSet;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                m_defaultRequest = value;
            }
        }

        public Evidence ApplicationEvidence {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (m_appEvidence == null) {
                    Evidence appEvidence = new Evidence();
                    if (m_context != null) {
                        appEvidence = new Evidence();
                        Url deploymentUrl = new Url(m_context.Identity.CodeBase);
                        appEvidence.AddHostEvidence(deploymentUrl);
                        appEvidence.AddHostEvidence(Zone.CreateFromUrl(m_context.Identity.CodeBase));
                        if (String.Compare("file:", 0, m_context.Identity.CodeBase, 0, 5, StringComparison.OrdinalIgnoreCase) != 0) {
                            appEvidence.AddHostEvidence(Site.CreateFromUrl(m_context.Identity.CodeBase));
                        }
                        appEvidence.AddHostEvidence(new StrongName(new StrongNamePublicKeyBlob(DeploymentId.m_publicKeyToken),
                                                                   DeploymentId.Name,
                                                                   DeploymentId.Version));
                        appEvidence.AddHostEvidence(new ActivationArguments(m_context));
                    }
                    Interlocked.CompareExchange(ref m_appEvidence, appEvidence, null);
                }
                return m_appEvidence as Evidence;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                m_appEvidence = value;
            }
        }

        //
        // Internal.
        //

        private static ApplicationId ParseApplicationId (ICMS manifest) {
            if (manifest.Identity == null)
                return null;

            return new ApplicationId(Hex.DecodeHexString(manifest.Identity.GetAttribute("", "publicKeyToken")),
                                     manifest.Identity.GetAttribute("", "name"),
                                     new Version(manifest.Identity.GetAttribute("", "version")),
                                     manifest.Identity.GetAttribute("", "processorArchitecture"),
                                     manifest.Identity.GetAttribute("", "culture"));
        }
    }
}
