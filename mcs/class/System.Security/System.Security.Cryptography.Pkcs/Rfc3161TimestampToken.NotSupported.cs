using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {
    public sealed class Rfc3161TimestampToken {
        public Rfc3161TimestampTokenInfo TokenInfo {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        internal Rfc3161TimestampToken () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets a Signed Cryptographic Message Syntax (CMS) representation of the RFC3161 timestamp token.</summary>
        /// <returns>The <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> representation of the <see cref="T:System.Security.Cryptography.Pkcs.Rfc3161TimestampToken" />.</returns>
        public SignedCms AsSignedCms () {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="encodedBytes" />
        /// <param name="token" />
        /// <param name="bytesConsumed" />
        public static bool TryDecode (ReadOnlyMemory<byte> encodedBytes, out Rfc3161TimestampToken token, out int bytesConsumed) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="data" />
        /// <param name="signerCertificate" />
        /// <param name="extraCandidates" />
        public bool VerifySignatureForData (ReadOnlySpan<byte> data, out X509Certificate2 signerCertificate, X509Certificate2Collection extraCandidates = null) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="hash" />
        /// <param name="hashAlgorithm" />
        /// <param name="signerCertificate" />
        /// <param name="extraCandidates" />
        public bool VerifySignatureForHash (ReadOnlySpan<byte> hash, HashAlgorithmName hashAlgorithm, out X509Certificate2 signerCertificate, X509Certificate2Collection extraCandidates = null) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="hash" />
        /// <param name="hashAlgorithmId" />
        /// <param name="signerCertificate" />
        /// <param name="extraCandidates" />
        public bool VerifySignatureForHash (ReadOnlySpan<byte> hash, Oid hashAlgorithmId, out X509Certificate2 signerCertificate, X509Certificate2Collection extraCandidates = null) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="signerInfo" />
        /// <param name="signerCertificate" />
        /// <param name="extraCandidates" />
        public bool VerifySignatureForSignerInfo (SignerInfo signerInfo, out X509Certificate2 signerCertificate, X509Certificate2Collection extraCandidates = null) {
            throw new PlatformNotSupportedException ();
        }
    }
}
