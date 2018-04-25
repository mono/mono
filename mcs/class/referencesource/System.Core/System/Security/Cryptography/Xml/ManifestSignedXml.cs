// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.Xml {
    /// <summary>
    ///     Class which verifies the signature of a single manifest and can return detailed information
    ///     about the signature.
    /// </summary>
    internal sealed class ManifestSignedXml : SignedXml {
        private ManifestKinds m_manifest;
        private XmlDocument m_manifestXml;
        private XmlNamespaceManager m_namespaceManager;

        public ManifestSignedXml(XmlDocument manifestXml, ManifestKinds manifest) : base(manifestXml) {
            Debug.Assert(manifestXml != null, "manifestXml != null");
            Debug.Assert(manifest == ManifestKinds.Application || manifest == ManifestKinds.Deployment, "Unknown manifest kind");

            m_manifest = manifest;
            m_manifestXml = manifestXml;

            m_namespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
            m_namespaceManager.AddNamespace("as", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            m_namespaceManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            m_namespaceManager.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2");
            m_namespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            m_namespaceManager.AddNamespace("msrel", "http://schemas.microsoft.com/windows/rel/2005/reldata");
            m_namespaceManager.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
        }

        /// <summary>
        ///     Convert a hex string to bytes in reverse order
        /// </summary>
        private static byte[] BackwardHexToBytes(string hex) {
            if (String.IsNullOrEmpty(hex) || hex.Length % 2 != 0) {
                return null;
            }

            byte[] bytes = new byte[hex.Length / 2];

            for (int stringIndex = hex.Length - 2, decodedIndex = 0; decodedIndex < bytes.Length;stringIndex -= 2, decodedIndex++) {
                byte? upper = HexToByte(hex[stringIndex]);
                byte? lower = HexToByte(hex[stringIndex + 1]);

                if (!upper.HasValue || !lower.HasValue) {
                    return null;
                }

                bytes[decodedIndex] = (byte)((upper.Value << 4) | lower.Value);
            }

            return bytes;
        }

        /// <summary>
        ///     Get the chain from the Authenticode signing certificate
        /// </summary>
        /// <remarks>
        ///     We want partially trusted callers to be able to verify the signature, so we'll
        ///     assert permission to access the store here; however any API which hands out the chain
        ///     or information derived from it such as the signing certificate needs to demand these
        ///     permissions before doing so.
        /// </remarks>
        [System.Security.SecurityCritical]
        [StorePermission(SecurityAction.Assert, EnumerateCertificates = true, OpenStore = true)]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        private X509Chain BuildSignatureChain(X509Native.AXL_AUTHENTICODE_SIGNER_INFO signer,
                                              XmlElement licenseNode,
                                              X509RevocationFlag revocationFlag,
                                              X509RevocationMode revocationMode) {
            Debug.Assert(licenseNode != null, "licenseNode != null");

            X509Chain signatureChain = null;

            if (signer.pChainContext != IntPtr.Zero) {
                signatureChain = new X509Chain(signer.pChainContext);
            }
            else if (signer.dwError == (int)SignatureVerificationResult.UntrustedRootCertificate) {
                // CertVerifyAuthenticodeLicense will not return the certificate chain for self signed certificates
                // so we'll need to extract the certificate from the signature ourselves.

                XmlElement x509Data = licenseNode.SelectSingleNode("r:issuer/ds:Signature/ds:KeyInfo/ds:X509Data",
                                                                   m_namespaceManager) as XmlElement;
                if (x509Data != null) {
                    XmlNodeList certificateNodes = x509Data.SelectNodes("ds:X509Certificate", m_namespaceManager);

                    // A manifest could have many X509Certificate nodes in its X509Data, which may include the
                    // signing certificate, links on the chain to a root, or certificates not used at all in
                    // the chain.  Since we don't know which certificate actually did the signing, we only
                    // process the chain if we have a single certificate.
                    if (certificateNodes.Count == 1 && certificateNodes[0] is XmlElement) {
                        byte[] rawCertificate = Convert.FromBase64String(certificateNodes[0].InnerText.Trim());
                        X509Certificate2 signingCertificate = new X509Certificate2(rawCertificate);

                        signatureChain = new X509Chain();
                        signatureChain.ChainPolicy.RevocationFlag = revocationFlag;
                        signatureChain.ChainPolicy.RevocationMode = revocationMode;

                        signatureChain.Build(signingCertificate);
                    }
                }
            }

            return signatureChain;
        }

        /// <summary>
        ///     Get the public key token specified in the manifest id
        /// </summary>
        private byte[] CalculateManifestPublicKeyToken() {
            XmlElement identityElement = m_manifestXml.SelectSingleNode("//asm:assembly/asm:assemblyIdentity",
                                                                        m_namespaceManager) as XmlElement;
            if (identityElement == null) {
                return null;
            }

            return HexStringToBytes(identityElement.GetAttribute("publicKeyToken"));
        }

        /// <summary>
        ///     Get the public key token of the strong name key used in the strong name signature
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "DangerousGetHandle is protected by a CER")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        private static byte[] CalculateSignerPublicKeyToken(AsymmetricAlgorithm key) {
            Debug.Assert(key != null, "key != null");

            ICspAsymmetricAlgorithm cspAlgorithm = key as ICspAsymmetricAlgorithm;
            if (cspAlgorithm == null) {
                return null;
            }

            byte[] publicKey = cspAlgorithm.ExportCspBlob(false);
            SafeAxlBufferHandle tokenBuffer;

            unsafe {
                fixed (byte* pPublicKey = publicKey) {
                    // Safe, since we're ensuring the CAPI buffer going in is sized correctly
                    CapiNative.CRYPTOAPI_BLOB keyBlob = new CapiNative.CRYPTOAPI_BLOB();
                    keyBlob.cbData = publicKey.Length;
                    keyBlob.pbData = new IntPtr(pPublicKey);

                    int hrToken = CapiNative.UnsafeNativeMethods._AxlPublicKeyBlobToPublicKeyToken(ref keyBlob,
                                                                                                   out tokenBuffer);
                    if (((uint)hrToken & 0x80000000) != 0) {
                        return null;
                    }
                }
            }

            bool acquired = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                tokenBuffer.DangerousAddRef(ref acquired);
                return HexStringToBytes(Marshal.PtrToStringUni(tokenBuffer.DangerousGetHandle()));
            }
            finally {
                if (acquired) {
                    tokenBuffer.DangerousRelease();
                }
            }
        }

        /// <summary>
        ///     Compare two byte arrays for equality
        /// </summary>
        /// <returns>true if both arrays are the non-null, the same length, and have the same contents</returns>
        private static bool CompareBytes(byte[] lhs, byte[] rhs) {
            if (lhs == null || rhs == null) {
                return false;
            }

            for (int i = 0; i < lhs.Length; i++) {
                if (lhs[i] != rhs[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Find the XML element being referenced by the signature
        /// </summary>
        public override XmlElement GetIdElement(XmlDocument document, string idValue) {
            // Redirect id elements back into the KeyInfo element
            if (KeyInfo != null && String.Compare(KeyInfo.Id, idValue, StringComparison.OrdinalIgnoreCase) == 0) {
                return KeyInfo.GetXml();
            }

            return null;
        }

        /// <summary>
        ///     Gether information about the timestamp of the authenticode signature, if there is one
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        private TimestampInformation GetTimestampInformation(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO timestamper,
                                                             XmlElement licenseNode) {
            Debug.Assert(licenseNode != null, "licenseNode != null");

            TimestampInformation timestamp = null;

            // If the timestamper is a trusted publisher, then CAPI has done the work for us;
            // If the leaf certificate is not explicitly a trusted publisher, CAPI will not process
            // the timestamp information so we will verify it ourselves. In any other case, we will
            // return no timestamp information.
            if (timestamper.dwError == (int)SignatureVerificationResult.Valid) {
                timestamp = new TimestampInformation(timestamper);
            }
            else if (timestamper.dwError == (int)SignatureVerificationResult.CertificateNotExplicitlyTrusted ||
                     timestamper.dwError == (int)SignatureVerificationResult.MissingSignature) {

                XmlElement timestampElement = licenseNode.SelectSingleNode("r:issuer/ds:Signature/ds:Object/as:Timestamp",
                                                                           m_namespaceManager) as XmlElement;
                if (timestampElement != null) {
                    // The timestamp is held as a parameter of a base64 encoded PKCS7 message in the signature
                    byte[] timestampBlob = Convert.FromBase64String(timestampElement.InnerText);

                    try {
                        SignedCms timestampCms = new SignedCms();
                        timestampCms.Decode(timestampBlob);
                        timestampCms.CheckSignature(true);

                        // The SignedCms class does not expose a way to read arbitrary properties from the
                        // message, nor does it expose the HCRYPTMSG to P/Invoke with. We cannot access the
                        // actual timestamp because of this, so for signatures which are not created by a
                        // trusted publisher, we will return a null timestamp. This should be corrected in
                        // v3 of the CLR, as we can extend SignedCms to have the properties we need to
                        // pull all of this information.
                        timestamp = null;
                    }
                    catch (CryptographicException e) {
                        timestamp = new TimestampInformation((SignatureVerificationResult)Marshal.GetHRForException(e));
                    }
                }
            }
            else {
                timestamp = null;
            }

            return timestamp;
        }

        /// <summary>
        ///     Convert a string of hex digits into an equivilent byte array, returning null on any error
        /// </summary>
        private static byte[] HexStringToBytes(string hex) {
            if (String.IsNullOrEmpty(hex) || hex.Length % 2 != 0) {
                return null;
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++) {
                byte? upper = HexToByte(hex[i]);
                byte? lower = HexToByte(hex[i + 1]);

                if (!upper.HasValue || !lower.HasValue) {
                    return null;
                }

                bytes[i] = (byte)((upper.Value << 4) | lower.Value);
            }

            return bytes;
        }

        /// <summary>
        ///     Convert a single hex character to a byte
        /// </summary>
        private static byte? HexToByte(char hex) {
            if (hex >= '0' && hex <= '9') {
                return (byte)(hex - '0');
            }
            else if (hex >= 'a' && hex <= 'f') {
                return (byte)(hex - 'a' + 10);
            }
            else if (hex >= 'A' && hex <= 'F') {
                return (byte)(hex - 'A' + 10);
            }
            else {
                return null;
            }
        }

        /// <summary>
        ///     Map X509 revocation flags to flags for the AXL verification APIs
        /// </summary>
        private static X509Native.AxlVerificationFlags MapRevocationFlags(X509RevocationFlag revocationFlag,
                                                                          X509RevocationMode revocationMode) {
            X509Native.AxlVerificationFlags axlFlags = X509Native.AxlVerificationFlags.None;

            switch (revocationFlag) {
                case X509RevocationFlag.EndCertificateOnly:
                    axlFlags |= X509Native.AxlVerificationFlags.RevocationCheckEndCertOnly;
                    break;

                case X509RevocationFlag.EntireChain:
                    axlFlags |= X509Native.AxlVerificationFlags.RevocationCheckEntireChain;
                    break;

                case X509RevocationFlag.ExcludeRoot:
                default:
                    axlFlags |= X509Native.AxlVerificationFlags.None;
                    break;
            }

            switch (revocationMode) {
                case X509RevocationMode.NoCheck:
                    axlFlags |= X509Native.AxlVerificationFlags.NoRevocationCheck;
                    break;

                case X509RevocationMode.Offline:
                    axlFlags |= X509Native.AxlVerificationFlags.UrlOnlyCacheRetrieval;
                    break;

                case X509RevocationMode.Online:
                default:
                    axlFlags |= X509Native.AxlVerificationFlags.None;
                    break;
            }

            return axlFlags;
        }

        /// <summary>
        ///     Verify the hash of the manifest without any signature attached is what the Authenticode
        ///     signature expects it to be
        /// </summary>
        private SignatureVerificationResult VerifyAuthenticodeExpectedHash(XmlElement licenseNode) {
            Debug.Assert(licenseNode != null, "licenseNode != null");

            // Get the expected hash value from the signature
            XmlElement manifestInformation = licenseNode.SelectSingleNode("r:grant/as:ManifestInformation",
                                                                          m_namespaceManager) as XmlElement;
            if (manifestInformation == null)
                return SignatureVerificationResult.BadSignatureFormat;

            string expectedHashString = manifestInformation.GetAttribute("Hash");
            if (String.IsNullOrEmpty(expectedHashString)) {
                return SignatureVerificationResult.BadSignatureFormat;
            }

            // The expected hash value is stored in backward order, so we cannot use a standard hex to bytes
            // routine to decode it.
            byte[] expectedHash = BackwardHexToBytes(expectedHashString);

            // Make a normalized copy of the manifest without the strong name signature attached
            XmlDocument normalizedManifest = new XmlDocument();
            normalizedManifest.PreserveWhitespace = true;

            XmlReaderSettings normalizationSettings = new XmlReaderSettings();
            normalizationSettings.DtdProcessing = DtdProcessing.Parse;

            using (TextReader manifestReader = new StringReader(m_manifestXml.OuterXml))
            using (XmlReader xmlReader = XmlReader.Create(manifestReader, normalizationSettings, m_manifestXml.BaseURI)) {
                normalizedManifest.Load(xmlReader);
            }

            XmlElement signatureNode = normalizedManifest.SelectSingleNode("//asm:assembly/ds:Signature",
                                                                           m_namespaceManager) as XmlElement;
            Debug.Assert(signatureNode != null, "signatureNode != null");

            signatureNode.ParentNode.RemoveChild(signatureNode);

            // calculate the hash value of the manifest
            XmlDsigExcC14NTransform canonicalizedXml = new XmlDsigExcC14NTransform();
            canonicalizedXml.LoadInput(normalizedManifest);

            byte[] actualHash = null;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
                actualHash = sha1.ComputeHash(canonicalizedXml.GetOutput() as MemoryStream);
            }

            if (!CompareBytes(expectedHash, actualHash)) {
                return SignatureVerificationResult.BadDigest;
            }

            return SignatureVerificationResult.Valid;
        }

        /// <summary>
        ///     Verify that the certificate which signed the manifest is the certificate the manifest expected
        ///     to be signed with
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "DangerousGetHandle is protected by a CER")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        private SignatureVerificationResult VerifyAuthenticodePublisher(X509Certificate2 publisherCertificate) {
            Debug.Assert(publisherCertificate != null, "publisherCertificate != null");

            // Get the expected name and key hash
            XmlElement publisherIdentity = m_manifestXml.SelectSingleNode("//asm:assembly/asmv2:publisherIdentity",
                                                                          m_namespaceManager) as XmlElement;
            if (publisherIdentity == null)
                return SignatureVerificationResult.BadSignatureFormat;

            string publisherName = publisherIdentity.GetAttribute("name");
            string publisherIssuerKeyHash = publisherIdentity.GetAttribute("issuerKeyHash");

            if (String.IsNullOrEmpty(publisherName) || String.IsNullOrEmpty(publisherIssuerKeyHash)) {
                return SignatureVerificationResult.BadSignatureFormat;
            }

            // Get the actual key hash
            SafeAxlBufferHandle issuerKeyBuffer = null;
            int hrHash = X509Native.UnsafeNativeMethods._AxlGetIssuerPublicKeyHash(publisherCertificate.Handle,
                                                                                   out issuerKeyBuffer);
            if (hrHash != (int)SignatureVerificationResult.Valid) {
                return (SignatureVerificationResult)hrHash;
            }

            string actualKeyHash = null;
            bool acquired = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                issuerKeyBuffer.DangerousAddRef(ref acquired);
                actualKeyHash = Marshal.PtrToStringUni(issuerKeyBuffer.DangerousGetHandle());
            }
            finally {
                if (acquired) {
                    issuerKeyBuffer.DangerousRelease();
                }
            }

            if (String.Compare(publisherName, publisherCertificate.SubjectName.Name, StringComparison.Ordinal) != 0 ||
                String.Compare(publisherIssuerKeyHash, actualKeyHash, StringComparison.Ordinal) != 0) {
                return SignatureVerificationResult.PublisherMismatch;
            }

            return SignatureVerificationResult.Valid;
        }

        /// <summary>
        ///     Verify the Authenticode signature has a valid format, applies to this manifest, and is valid
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        private AuthenticodeSignatureInformation VerifyAuthenticodeSignature(XmlElement signatureNode,
                                                                             X509RevocationFlag revocationFlag,
                                                                             X509RevocationMode revocationMode) {
            Debug.Assert(signatureNode != null, "signatureNode != null");

            // See if there is an Authenticode signature on the manifest
            XmlElement licenseNode = signatureNode.SelectSingleNode("ds:KeyInfo/msrel:RelData/r:license",
                                                                     m_namespaceManager) as XmlElement;
            if (licenseNode == null) {
                return null;
            }

            // Make sure that the signature is for this manifest
            SignatureVerificationResult identityVerification = VerifyAuthenticodeSignatureIdentity(licenseNode);
            if (identityVerification != SignatureVerificationResult.Valid) {
                return new AuthenticodeSignatureInformation(identityVerification);
            }

            SignatureVerificationResult hashVerification = VerifyAuthenticodeExpectedHash(licenseNode);
            if (hashVerification != SignatureVerificationResult.Valid) {
                return new AuthenticodeSignatureInformation(hashVerification);
            }

            // Verify the signature, extracting information about it
            AuthenticodeSignatureInformation authenticodeSignature = null;

            X509Native.AXL_AUTHENTICODE_SIGNER_INFO signer = new X509Native.AXL_AUTHENTICODE_SIGNER_INFO();
            signer.cbSize = Marshal.SizeOf(typeof(X509Native.AXL_AUTHENTICODE_SIGNER_INFO));

            X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO timestamper = new X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO();
            timestamper.cbsize = Marshal.SizeOf(typeof(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO));

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                byte[] licenseXml = Encoding.UTF8.GetBytes(licenseNode.OuterXml);
                X509Native.AxlVerificationFlags verificationFlags = MapRevocationFlags(revocationFlag,
                                                                                       revocationMode);

                unsafe {
                    fixed (byte* pLicenseXml = licenseXml) {
                        // Safe since we're verifying the size of this buffer is correct
                        CapiNative.CRYPTOAPI_BLOB xmlBlob = new CapiNative.CRYPTOAPI_BLOB();
                        xmlBlob.cbData = licenseXml.Length;
                        xmlBlob.pbData = new IntPtr(pLicenseXml);

                        int hrVerify = X509Native.UnsafeNativeMethods.CertVerifyAuthenticodeLicense(ref xmlBlob,
                                                                                                    verificationFlags,
                                                                                                    ref signer,
                                                                                                    ref timestamper);

                        if (hrVerify == (int)SignatureVerificationResult.MissingSignature) {
                            return new AuthenticodeSignatureInformation(SignatureVerificationResult.MissingSignature);
                        }
                    }
                }

                X509Chain signatureChain = BuildSignatureChain(signer,
                                                               licenseNode,
                                                               revocationFlag,
                                                               revocationMode);

                TimestampInformation timestamp = GetTimestampInformation(timestamper,
                                                                         licenseNode);

                authenticodeSignature = new AuthenticodeSignatureInformation(signer,
                                                                             signatureChain,
                                                                             timestamp);
            }
            finally {
                X509Native.UnsafeNativeMethods.CertFreeAuthenticodeSignerInfo(ref signer);
                X509Native.UnsafeNativeMethods.CertFreeAuthenticodeTimestamperInfo(ref timestamper);
            }

            // Verify the signing certificate matches the expected publisher
            Debug.Assert(authenticodeSignature != null, "authenticodeSignature != null");
            if (authenticodeSignature.SigningCertificate == null) {
                return new AuthenticodeSignatureInformation(authenticodeSignature.VerificationResult);
            }

            SignatureVerificationResult publisherMatch = VerifyAuthenticodePublisher(authenticodeSignature.SigningCertificate);
            if (publisherMatch != SignatureVerificationResult.Valid) {
                return new AuthenticodeSignatureInformation(publisherMatch);
            }

            return authenticodeSignature;
        }

        /// <summary>
        ///     Verify that the Authenticode signature expects to be attached to this manifest
        /// </summary>
        private SignatureVerificationResult VerifyAuthenticodeSignatureIdentity(XmlElement licenseNode) {
            Debug.Assert(licenseNode != null, "licenseNode != null");

            XmlElement signatureIdentity = licenseNode.SelectSingleNode("r:grant/as:ManifestInformation/as:assemblyIdentity",
                                                                        m_namespaceManager) as XmlElement;
            XmlElement assemblyIdentity = m_manifestXml.SelectSingleNode("//asm:assembly/asm:assemblyIdentity",
                                                                         m_namespaceManager) as XmlElement;

            bool validAssemblyIdentity = assemblyIdentity != null && assemblyIdentity.HasAttributes;
            bool validSignatureIdentity = signatureIdentity != null && signatureIdentity.HasAttributes;

            if (!validAssemblyIdentity ||
                !validSignatureIdentity ||
                assemblyIdentity.Attributes.Count != signatureIdentity.Attributes.Count) {
                return SignatureVerificationResult.BadSignatureFormat;
            }

            foreach (XmlAttribute identityAttribute in assemblyIdentity.Attributes) {
                string signatureValue = signatureIdentity.GetAttribute(identityAttribute.LocalName);

                if (signatureValue == null ||
                    String.Compare(identityAttribute.Value, signatureValue, StringComparison.Ordinal) != 0) {
                    return SignatureVerificationResult.AssemblyIdentityMismatch;
                }
            }

            return SignatureVerificationResult.Valid;
        }

        /// <summary>
        ///     Verify that the strong name signature has a valid id
        /// </summary>
        private static SignatureVerificationResult VerifyStrongNameSignatureId(XmlElement signatureNode) {
            Debug.Assert(signatureNode != null, "signatureNode != null");

            string signatureId = null;
            for (int i = 0; i < signatureNode.Attributes.Count && signatureId == null; i++) {
                if (String.Compare(signatureNode.Attributes[i].LocalName, "id", StringComparison.OrdinalIgnoreCase) == 0) {
                    signatureId = signatureNode.Attributes[i].Value;
                }
            }

            if (String.IsNullOrEmpty(signatureId)) {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            if (String.Compare(signatureId, "StrongNameSignature", StringComparison.Ordinal) != 0) {
                return SignatureVerificationResult.BadSignatureFormat;
            }

            return SignatureVerificationResult.Valid;
        }

        /// <summary>
        ///     Verify the transforms on a strong name signature are valid for a ClickOnce manifest.
        /// </summary>
        /// <remarks>
        ///     For a reference to the entire document, we expect both exclusive cannonicalization and
        ///     enveloped transform. For a reference to the  strong name key section, we expect only exclusive
        ///     cannonicalization. Other references are not given special meaning for a strong name signature
        ///     and are ignored. Failure to have exactly the correct set of transforms is an error with the
        ///     strong name signature.
        /// </remarks>
        private static SignatureVerificationResult VerifyStrongNameSignatureTransforms(SignedInfo signedInfo) {
            Debug.Assert(signedInfo != null, "signedInfo != null");

            int totalReferences = 0;
            foreach (Reference reference in signedInfo.References) {
                TransformChain transforms = reference.TransformChain;
                bool validTransformChain = false;

                if (String.IsNullOrEmpty(reference.Uri)) {
                    totalReferences++;
                    validTransformChain = transforms != null &&
                                          transforms.Count == 2 &&
                                          String.Compare(transforms[0].Algorithm, SignedXml.XmlDsigEnvelopedSignatureTransformUrl, StringComparison.Ordinal) == 0 &&
                                          String.Compare(transforms[1].Algorithm, SignedXml.XmlDsigExcC14NTransformUrl, StringComparison.Ordinal) == 0;
                }
                else if (String.Compare(reference.Uri, "#StrongNameKeyInfo", StringComparison.Ordinal) == 0) {
                    totalReferences++;
                    validTransformChain = transforms != null &&
                                          transforms.Count == 1 &&
                                          String.Compare(transforms[0].Algorithm, SignedXml.XmlDsigExcC14NTransformUrl, StringComparison.Ordinal) == 0;
                }
                else {
                    validTransformChain = true;
                }

                if (!validTransformChain) {
                    return SignatureVerificationResult.BadSignatureFormat;
                }
            }

            if (totalReferences == 0) {
                return SignatureVerificationResult.BadSignatureFormat;
            }

            return SignatureVerificationResult.Valid;
        }

        /// <summary>
        ///     Verify the strong name signature has a valid format and applies to this manifest
        /// </summary>
        private StrongNameSignatureInformation VerifyStrongNameSignature(XmlElement signatureNode) {
            Debug.Assert(signatureNode != null, "signatureNode != null");

            // Verify that the signature is valid
            AsymmetricAlgorithm key;
            if (!CheckSignatureReturningKey(out key)) {
                return new StrongNameSignatureInformation(SignatureVerificationResult.BadDigest);
            }

            // ensure there is an ID element, and it is the strong name id
            SignatureVerificationResult strongNameId = VerifyStrongNameSignatureId(signatureNode);
            if (strongNameId != SignatureVerificationResult.Valid) {
                return new StrongNameSignatureInformation(strongNameId);
            }

            // Verify that the transforms are the ones we expect.
            Debug.Assert(Signature != null && Signature.SignedInfo != null,
                         "XML signature must be verified before getting SN details");
            SignatureVerificationResult transformsValid = VerifyStrongNameSignatureTransforms(Signature.SignedInfo);
            if (transformsValid != SignatureVerificationResult.Valid) {
                return new StrongNameSignatureInformation(transformsValid);
            }

            // ensure the public key token in the manifest identity matches the public key token of the signing
            // strong name key
            if (!CompareBytes(CalculateManifestPublicKeyToken(), CalculateSignerPublicKeyToken(key))) {
                return new StrongNameSignatureInformation(SignatureVerificationResult.PublicKeyTokenMismatch);
            }

            return new StrongNameSignatureInformation(key);
        }

        /// <summary>
        ///     Verify the signature of the manifest
        /// </summary>
        public ManifestSignatureInformation VerifySignature(X509RevocationFlag revocationFlag,
                                                            X509RevocationMode revocationMode) {
            XmlElement signatureNode = m_manifestXml.SelectSingleNode("//ds:Signature", m_namespaceManager) as XmlElement;
            if (signatureNode == null) {
                return new ManifestSignatureInformation(m_manifest, null, null);
            }

            LoadXml(signatureNode);

            StrongNameSignatureInformation strongName = VerifyStrongNameSignature(signatureNode);

            // Since the Authenticode signature is wrapped in the strong name signature, we do not want to
            // give a valid AuthenticodeSignatureInformation object for an Authenticode signature which is
            // contained within a strong name signature with an invalid hash value.
            AuthenticodeSignatureInformation authenticode = null;
            if (strongName.VerificationResult != SignatureVerificationResult.BadDigest) {
                authenticode = VerifyAuthenticodeSignature(signatureNode, revocationFlag, revocationMode);
            }
            else {
                authenticode = new AuthenticodeSignatureInformation(SignatureVerificationResult.ContainingSignatureInvalid);
            }

            return new ManifestSignatureInformation(m_manifest, strongName, authenticode);
        }
    }
}
