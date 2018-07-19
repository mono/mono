// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Internal;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper for information about the various signatures that can be applied to a manifest
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class ManifestSignatureInformation {
        private ManifestKinds m_manifest;
        private StrongNameSignatureInformation m_strongNameSignature;
        private AuthenticodeSignatureInformation m_authenticodeSignature;

        internal ManifestSignatureInformation(ManifestKinds manifest,
                                              StrongNameSignatureInformation strongNameSignature,
                                              AuthenticodeSignatureInformation authenticodeSignature) {
            Debug.Assert(manifest == ManifestKinds.Application || manifest == ManifestKinds.Deployment, "Invalid manifest for signature information");

            m_manifest = manifest;
            m_strongNameSignature = strongNameSignature;
            m_authenticodeSignature = authenticodeSignature;
        }

        /// <summary>
        ///     Authenticode signature of the manifest
        /// </summary>
        public AuthenticodeSignatureInformation AuthenticodeSignature {
            get { return m_authenticodeSignature; }
        }

        /// <summary>
        ///     Manifest the signature information is for
        /// </summary>
        public ManifestKinds Manifest {
            get { return m_manifest; }
        }

        /// <summary>
        ///     Details about the strong name signature of the manifest
        /// </summary>
        public StrongNameSignatureInformation StrongNameSignature {
            get { return m_strongNameSignature; }
        }

        /// <summary>
        ///     Load the XML from the specified manifest into an XmlDocument
        /// </summary>
        // SafeCritical - If you've got access to the ActivationContext, we don't consider the manifests 
        //                themselves to be protected resources - you had to have access to them to get the
        //                context anyway.  The IsolationLib interop is an implementation detail here and safe.
        [SecuritySafeCritical]
        private static XmlDocument GetManifestXml(ActivationContext application, ManifestKinds manifest) {
            Debug.Assert(application != null, "application != null");

            IStream manifestStream = null;
            if (manifest == ManifestKinds.Application) {
                manifestStream = InternalActivationContextHelper.GetApplicationComponentManifest(application) as IStream;
            }
            else if (manifest == ManifestKinds.Deployment) {
                manifestStream = InternalActivationContextHelper.GetDeploymentComponentManifest(application) as IStream;
            }
            Debug.Assert(manifestStream != null, "Cannot get stream for manifest");

            using (MemoryStream manifestContent = new MemoryStream()) {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                do {
                    unsafe {
                        manifestStream.Read(buffer, buffer.Length, new IntPtr(&bytesRead));
                    }

                    manifestContent.Write(buffer, 0, bytesRead);
                }
                while (bytesRead == buffer.Length);
                manifestContent.Position = 0;

                XmlDocument manifestXml = new XmlDocument();
                manifestXml.PreserveWhitespace = true;
                manifestXml.Load(manifestContent);

                return manifestXml;
            }
        }

        /// <summary>
        ///     Verify and gather information about the signatures of the specified manifests
        /// </summary>
        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application) {
            return VerifySignature(application, ManifestKinds.ApplicationAndDeployment);
        }

        /// <summary>
        ///     Verify and gather information about the signatures of the specified manifests
        /// </summary>
        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application,
                                                                             ManifestKinds manifests) {
            return VerifySignature(application,
                                   manifests,
                                   X509RevocationFlag.ExcludeRoot,
                                   X509RevocationMode.Online);
        }

        /// <summary>
        ///     Verify and gather information about the signatures of the specified manifests
        /// </summary>
        [SecuritySafeCritical]
        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application,
                                                                             ManifestKinds manifests,
                                                                             X509RevocationFlag revocationFlag,
                                                                             X509RevocationMode revocationMode) {
            if (application == null) {
                throw new ArgumentNullException("application");
            }
            if (revocationFlag < X509RevocationFlag.EndCertificateOnly || X509RevocationFlag.ExcludeRoot < revocationFlag) {
                throw new ArgumentOutOfRangeException("revocationFlag");
            }
            if (revocationMode < X509RevocationMode.NoCheck || X509RevocationMode.Offline < revocationMode) {
                throw new ArgumentOutOfRangeException("revocationMode");
            }

            List<ManifestSignatureInformation> signatures = new List<ManifestSignatureInformation>();
            if ((manifests & ManifestKinds.Deployment) == ManifestKinds.Deployment) {
                XmlDocument deploymentManifest = GetManifestXml(application, ManifestKinds.Deployment);
                ManifestSignedXml deploymentSignature = new ManifestSignedXml(deploymentManifest,
                                                                              ManifestKinds.Deployment);
                signatures.Add(deploymentSignature.VerifySignature(revocationFlag, revocationMode));
            }
            if ((manifests & ManifestKinds.Application) == ManifestKinds.Application) {
                XmlDocument applicationManifest = GetManifestXml(application, ManifestKinds.Application);
                ManifestSignedXml applicationSignature = new ManifestSignedXml(applicationManifest,
                                                                               ManifestKinds.Application);
                signatures.Add(applicationSignature.VerifySignature(revocationFlag, revocationMode));
            }

            return new ManifestSignatureInformationCollection(signatures);
        }
    }

    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class ManifestSignatureInformationCollection : ReadOnlyCollection<ManifestSignatureInformation> {
        internal ManifestSignatureInformationCollection(IList<ManifestSignatureInformation> signatureInformation) : base(signatureInformation) {
            return;
        }
    }
}
