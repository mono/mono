// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace System.Security.Cryptography.X509Certificates {
    /// <summary>
    ///     Details about the Authenticode signature of a manifest
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class AuthenticodeSignatureInformation {
        private string m_description;
        private Uri m_descriptionUrl;
        private CapiNative.AlgorithmId m_hashAlgorithmId;
        private X509Chain m_signatureChain;
        private TimestampInformation m_timestamp;
        private SignatureVerificationResult m_verificationResult;

        private X509Certificate2 m_signingCertificate;

        [SecurityCritical]
        internal AuthenticodeSignatureInformation(X509Native.AXL_AUTHENTICODE_SIGNER_INFO signer,
                                                  X509Chain signatureChain,
                                                  TimestampInformation timestamp) {
            m_verificationResult = (SignatureVerificationResult)signer.dwError;
            m_hashAlgorithmId = signer.algHash;

            if (signer.pwszDescription != IntPtr.Zero) {
                m_description = Marshal.PtrToStringUni(signer.pwszDescription);
            }
            if (signer.pwszDescriptionUrl != IntPtr.Zero) {
                string descriptionUrl = Marshal.PtrToStringUni(signer.pwszDescriptionUrl);
                Uri.TryCreate(descriptionUrl, UriKind.RelativeOrAbsolute, out m_descriptionUrl);
            }

            m_signatureChain = signatureChain;

            // If there was a timestamp, and it was not valid we need to invalidate the entire Authenticode
            // signature as well, since we cannot assume that the signature would have verified without
            // the timestamp.
            if (timestamp != null && timestamp.VerificationResult != SignatureVerificationResult.MissingSignature) {
                if (timestamp.IsValid) {
                    m_timestamp = timestamp;
                }
                else {
                    m_verificationResult = SignatureVerificationResult.InvalidTimestamp;
                }
            }
            else {
                m_timestamp = null;
            }
        }

        /// <summary>
        ///     Create an Authenticode signature information for a signature which is not valid
        /// </summary>
        internal AuthenticodeSignatureInformation(SignatureVerificationResult error) {
            Debug.Assert(error != SignatureVerificationResult.Valid);
            m_verificationResult = error;
        }

        /// <summary>
        ///     Description of the signing certificate
        /// </summary>
        public string Description {
            get { return m_description; }
        }

        /// <summary>
        ///     Description URL of the signing certificate
        /// </summary>
        public Uri DescriptionUrl {
            get { return m_descriptionUrl; }
        }

        /// <summary>
        ///     Hash algorithm the signature was computed with
        /// </summary>
        public string HashAlgorithm {
            get { return CapiNative.GetAlgorithmName(m_hashAlgorithmId); }
        }

        /// <summary>
        ///     HRESULT from verifying the signature
        /// </summary>
        public int HResult {
            get { return CapiNative.HResultForVerificationResult(m_verificationResult); }
        }

        /// <summary>
        ///     X509 chain used to verify the Authenticode signature
        /// </summary>
        public X509Chain SignatureChain {
            [StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
            [SecuritySafeCritical]
            get { return m_signatureChain; }
        }

        /// <summary>
        ///     Certificate the manifest was signed with
        /// </summary>
        public X509Certificate2 SigningCertificate {
            [StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
            [SecuritySafeCritical]
            get {
                if (m_signingCertificate == null && SignatureChain != null) {
                    Debug.Assert(SignatureChain.ChainElements.Count > 0, "SignatureChain.ChainElements.Count > 0");
                    m_signingCertificate = SignatureChain.ChainElements[0].Certificate;
                }

                return m_signingCertificate;
            }
        }

        /// <summary>
        ///     Timestamp, if any, applied to the Authenticode signature
        /// </summary>
        /// <remarks>
        ///     Note that this is only available in the trusted publisher case
        /// </remarks>
        public TimestampInformation Timestamp {
            get { return m_timestamp; }
        }

        /// <summary>
        ///     Trustworthiness of the Authenticode signature
        /// </summary>
        public TrustStatus TrustStatus {
            get {
                switch (VerificationResult) {
                    case SignatureVerificationResult.Valid:
                        return TrustStatus.Trusted;

                    case SignatureVerificationResult.CertificateNotExplicitlyTrusted:
                        return TrustStatus.KnownIdentity;

                    case SignatureVerificationResult.CertificateExplicitlyDistrusted:
                        return TrustStatus.Untrusted;

                    default:
                        return TrustStatus.UnknownIdentity;
                }
            }
        }

        /// <summary>
        ///     Result of verifying the Authenticode signature
        /// </summary>
        public SignatureVerificationResult VerificationResult {
            get { return m_verificationResult; }
        }
    }
}
