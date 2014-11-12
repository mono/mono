// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Details about a strong name signature
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class StrongNameSignatureInformation {
        private SignatureVerificationResult m_verificationResult;
        private AsymmetricAlgorithm m_publicKey;

        // All strong name signatures use SHA1 as their hash algorithm
        private static readonly string StrongNameHashAlgorithm =
            CapiNative.GetAlgorithmName(CapiNative.AlgorithmId.Sha1);

        internal StrongNameSignatureInformation(AsymmetricAlgorithm publicKey) {
            Debug.Assert(publicKey != null, "publicKey != null");

            m_verificationResult = SignatureVerificationResult.Valid;
            m_publicKey = publicKey;
        }

        internal StrongNameSignatureInformation(SignatureVerificationResult error) {
            Debug.Assert(error != SignatureVerificationResult.Valid, "error != SignatureVerificationResult.Valid");

            m_verificationResult = error;
        }

        /// <summary>
        ///     Hash algorithm used in calculating the strong name signature
        /// </summary>
        public string HashAlgorithm {
            get { return StrongNameHashAlgorithm; }
        }

        /// <summary>
        ///     HRESULT version of the result code
        /// </summary>
        public int HResult {
            get { return CapiNative.HResultForVerificationResult(m_verificationResult); }
        }

        /// <summary>
        ///     Is the strong name signature valid, or was there some form of error
        /// </summary>
        public bool IsValid {
            get { return m_verificationResult == SignatureVerificationResult.Valid; }
        }

        /// <summary>
        ///     Public key used to create the signature
        /// </summary>
        public AsymmetricAlgorithm PublicKey {
            get { return m_publicKey; }
        }

        /// <summary>
        ///     Results of verifying the strong name signature
        /// </summary>
        public SignatureVerificationResult VerificationResult {
            get { return m_verificationResult; }
        }
    }
}
