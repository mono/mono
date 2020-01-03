using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {
    public sealed class Rfc3161TimestampRequest {
        public bool HasExtensions {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public Oid HashAlgorithmId {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public Oid RequestedPolicyId {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public bool RequestSignerCertificate {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public int Version {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        internal Rfc3161TimestampRequest () {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="data" />
        /// <param name="hashAlgorithm" />
        /// <param name="requestedPolicyId" />
        /// <param name="nonce" />
        /// <param name="requestSignerCertificates" />
        /// <param name="extensions" />
        public static Rfc3161TimestampRequest CreateFromData (ReadOnlySpan<byte> data, HashAlgorithmName hashAlgorithm, Oid requestedPolicyId = null, ReadOnlyMemory<byte>? nonce = default(ReadOnlyMemory<byte>?), bool requestSignerCertificates = false, X509ExtensionCollection extensions = null) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="hash" />
        /// <param name="hashAlgorithm" />
        /// <param name="requestedPolicyId" />
        /// <param name="nonce" />
        /// <param name="requestSignerCertificates" />
        /// <param name="extensions" />
        public static Rfc3161TimestampRequest CreateFromHash (ReadOnlyMemory<byte> hash, HashAlgorithmName hashAlgorithm, Oid requestedPolicyId = null, ReadOnlyMemory<byte>? nonce = default(ReadOnlyMemory<byte>?), bool requestSignerCertificates = false, X509ExtensionCollection extensions = null) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Create a timestamp request using a pre-computed hash value.</summary>
        /// <param name="hash">The pre-computed hash value to be timestamped.</param>
        /// <param name="hashAlgorithmId">The Object Identifier (OID) for the hash algorithm that produced <paramref name="hash" />.</param>
        /// <param name="requestedPolicyId">The Object Identifier (OID) for a timestamp policy the Timestamp Authority (TSA) should use, or <see langword="null" /> to express no preference.</param>
        /// <param name="nonce">An optional nonce (number used once) to uniquely identify this request to pair it with the response. The value is interpreted as an unsigned big-endian integer and may be normalized to the encoding format.</param>
        /// <param name="requestSignerCertificates">
        ///   <see langword="true" /> to indicate the Timestamp Authority (TSA) must include the signing certificate in the issued timestamp token; otherwise, <see langword="false" />.</param>
        /// <param name="extensions">RFC3161 extensions to present with the request.</param>
        /// <returns>An <see cref="T:System.Security.Cryptography.Pkcs.Rfc3161TimestampRequest" /> representing the chosen values.</returns>
        public static Rfc3161TimestampRequest CreateFromHash (ReadOnlyMemory<byte> hash, Oid hashAlgorithmId, Oid requestedPolicyId = null, ReadOnlyMemory<byte>? nonce = default(ReadOnlyMemory<byte>?), bool requestSignerCertificates = false, X509ExtensionCollection extensions = null) {
            throw new PlatformNotSupportedException ();		
        }

        /// <param name="signerInfo" />
        /// <param name="hashAlgorithm" />
        /// <param name="requestedPolicyId" />
        /// <param name="nonce" />
        /// <param name="requestSignerCertificates" />
        /// <param name="extensions" />
        public static Rfc3161TimestampRequest CreateFromSignerInfo (SignerInfo signerInfo, HashAlgorithmName hashAlgorithm, Oid requestedPolicyId = null, ReadOnlyMemory<byte>? nonce = default(ReadOnlyMemory<byte>?), bool requestSignerCertificates = false, X509ExtensionCollection extensions = null) {
            throw new PlatformNotSupportedException ();
        }

        public byte[] Encode () {
            throw new PlatformNotSupportedException ();
        }

        public X509ExtensionCollection GetExtensions () {
            throw new PlatformNotSupportedException ();
        }

        public ReadOnlyMemory<byte> GetMessageHash () {
            throw new PlatformNotSupportedException ();
        }

        public ReadOnlyMemory<byte>? GetNonce () {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="responseBytes" />
        /// <param name="bytesConsumed" />
        public Rfc3161TimestampToken ProcessResponse (ReadOnlyMemory<byte> responseBytes, out int bytesConsumed) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="encodedBytes" />
        /// <param name="request" />
        /// <param name="bytesConsumed" />
        public static bool TryDecode (ReadOnlyMemory<byte> encodedBytes, out Rfc3161TimestampRequest request, out int bytesConsumed) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="destination" />
        /// <param name="bytesWritten" />
        public bool TryEncode (Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }
    }
}
