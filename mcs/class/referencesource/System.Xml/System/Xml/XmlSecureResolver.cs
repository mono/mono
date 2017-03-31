//------------------------------------------------------------------------------
// <copyright file="XmlSecureResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System.Net;
    using System.Security;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Runtime.Versioning;

    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public partial class XmlSecureResolver : XmlResolver {
        XmlResolver resolver;
#if MONO_FEATURE_CAS
        PermissionSet permissionSet;
#endif

#if MONO_FEATURE_CAS
        public XmlSecureResolver(XmlResolver resolver, string securityUrl) : this(resolver, CreateEvidenceForUrl(securityUrl)) {}

        public XmlSecureResolver(XmlResolver resolver, Evidence evidence) : this(resolver, SecurityManager.GetStandardSandbox(evidence)) {}
#else
        public XmlSecureResolver(XmlResolver resolver, string securityUrl) : this(resolver, (PermissionSet) null) {}

        public XmlSecureResolver(XmlResolver resolver, Evidence evidence) : this(resolver, (PermissionSet) null) {}
#endif

        public XmlSecureResolver(XmlResolver resolver, PermissionSet permissionSet) {
            this.resolver = resolver;
#if MONO_FEATURE_CAS
            this.permissionSet = permissionSet;
#endif
        }

        public override ICredentials Credentials {
            set { resolver.Credentials = value; }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
#if MONO_FEATURE_CAS
            permissionSet.PermitOnly();
#endif
            return resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            return resolver.ResolveUri(baseUri, relativeUri);
        }

        public static Evidence CreateEvidenceForUrl(string securityUrl) {
#if MONO_FEATURE_CAS
            Evidence evidence = new Evidence();
            if (securityUrl != null && securityUrl.Length > 0) {
                evidence.AddHostEvidence(new Url(securityUrl));
                evidence.AddHostEvidence(Zone.CreateFromUrl(securityUrl));
                Uri uri = new Uri(securityUrl, UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri && !uri.IsFile) {
                    evidence.AddHostEvidence(Site.CreateFromUrl(securityUrl));
                }

                // Allow same directory access for UNCs (SQLBUDT 394535)
                if (uri.IsAbsoluteUri && uri.IsUnc) {
                    string uncDir = System.IO.Path.GetDirectoryName(uri.LocalPath);
                    if (uncDir != null && uncDir.Length != 0) {
                        evidence.AddHostEvidence(new UncDirectory(uncDir));
                    }
                }
            }

            return evidence;
#else
            return null;
#endif
        }

#if MONO_FEATURE_CAS
        [Serializable]
        private class UncDirectory : EvidenceBase, IIdentityPermissionFactory {
            private string uncDir;

            public UncDirectory(string uncDirectory) {
                this.uncDir = uncDirectory;
            }

            public IPermission CreateIdentityPermission(Evidence evidence) {
                return new FileIOPermission(FileIOPermissionAccess.Read, uncDir);
            }

            public override EvidenceBase Clone()
            {
                return new UncDirectory(uncDir);
            }

            private SecurityElement ToXml() {
                SecurityElement root = new SecurityElement("System.Xml.XmlSecureResolver");
                root.AddAttribute("version", "1");
                root.AddChild(new SecurityElement("UncDirectory", uncDir));
                return root;
            }

            public override string ToString() {
                return ToXml().ToString();
            }
        }
#endif
    }
}
