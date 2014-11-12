// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace System.Security.Cryptography.X509Certificates {
    /// <summary>
    ///     Details about the timestamp applied to a manifest's Authenticode signature
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class TimestampInformation {
        private CapiNative.AlgorithmId m_hashAlgorithmId;
        private DateTime m_timestamp;
        private X509Chain m_timestampChain;
        private SignatureVerificationResult m_verificationResult;

        private X509Certificate2 m_timestamper;

        [System.Security.SecurityCritical]
        internal TimestampInformation(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO timestamper) {
            m_hashAlgorithmId = timestamper.algHash;
            m_verificationResult = (SignatureVerificationResult)timestamper.dwError;

            ulong filetime = ((ulong)((uint)timestamper.ftTimestamp.dwHighDateTime) << 32)  |
                              (ulong)((uint)timestamper.ftTimestamp.dwLowDateTime);
            m_timestamp = DateTime.FromFileTimeUtc((long)filetime);

            if (timestamper.pChainContext != IntPtr.Zero) {
                m_timestampChain = new X509Chain(timestamper.pChainContext);
            }
        }

        internal TimestampInformation(SignatureVerificationResult error) {
            Debug.Assert(error != SignatureVerificationResult.Valid, "error != SignatureVerificationResult.Valid");
            m_verificationResult = error;
        }

        /// <summary>
        ///     Hash algorithm the timestamp signature was calculated with
        /// </summary>
        public string HashAlgorithm {
            get { return CapiNative.GetAlgorithmName(m_hashAlgorithmId); }
        }

        /// <summary>
        ///     HRESULT from verifying the timestamp
        /// </summary>
        public int HResult {
            get { return CapiNative.HResultForVerificationResult(m_verificationResult); }
        }

        /// <summary>
        ///     Is the signature of the timestamp valid
        /// </summary>
        public bool IsValid {
            get {
                // Timestamp signatures are valid only if they were created by a trusted chain
                return VerificationResult == SignatureVerificationResult.Valid ||
                       VerificationResult == SignatureVerificationResult.CertificateNotExplicitlyTrusted;
            }
        }

        /// <summary>
        ///     Chain of certificates used to verify the timestamp
        /// </summary>
        public X509Chain SignatureChain {
            [StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
            [SecuritySafeCritical]
            get { return m_timestampChain; }
        }

        /// <summary>
        ///     Certificate that signed the timestamp
        /// </summary>
        public X509Certificate2 SigningCertificate {
            [StorePermission(SecurityAction.Demand, OpenStore = true, EnumerateCertificates = true)]
            [SecuritySafeCritical]
            get {
                if (m_timestamper == null && SignatureChain != null) {
                    m_timestamper = SignatureChain.ChainElements[0].Certificate;
                }

                return m_timestamper;
            }
        }

        /// <summary>
        ///     When the timestamp was applied, expressed in local time
        /// </summary>
        public DateTime Timestamp {
            get {
                return m_timestamp.ToLocalTime();
            }
        }

        /// <summary>
        ///     Result of verifying the timestamp signature
        /// </summary>
        public SignatureVerificationResult VerificationResult {
            get { return m_verificationResult; }
        }
    }
}
