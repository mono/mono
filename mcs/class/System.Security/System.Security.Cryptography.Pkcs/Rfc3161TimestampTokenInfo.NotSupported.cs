using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {
    public sealed class Rfc3161TimestampTokenInfo {
        public long? AccuracyInMicroseconds {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

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

        public bool IsOrdering {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public Oid PolicyId {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public DateTimeOffset Timestamp {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        public int Version {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <param name="policyId" />
        /// <param name="hashAlgorithmId" />
        /// <param name="messageHash" />
        /// <param name="serialNumber" />
        /// <param name="timestamp" />
        /// <param name="accuracyInMicroseconds" />
        /// <param name="isOrdering" />
        /// <param name="nonce" />
        /// <param name="timestampAuthorityName" />
        /// <param name="extensions" />
        public Rfc3161TimestampTokenInfo (Oid policyId, Oid hashAlgorithmId, ReadOnlyMemory<byte> messageHash, ReadOnlyMemory<byte> serialNumber, DateTimeOffset timestamp, long? accuracyInMicroseconds = default(long?), bool isOrdering = false, ReadOnlyMemory<byte>? nonce = default(ReadOnlyMemory<byte>?), ReadOnlyMemory<byte>? timestampAuthorityName = default(ReadOnlyMemory<byte>?), X509ExtensionCollection extensions = null) {
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

        public ReadOnlyMemory<byte> GetSerialNumber () {
            throw new PlatformNotSupportedException ();
        }

        public ReadOnlyMemory<byte>? GetTimestampAuthorityName () {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="encodedBytes" />
        /// <param name="timestampTokenInfo" />
        /// <param name="bytesConsumed" />
        public static bool TryDecode (ReadOnlyMemory<byte> encodedBytes, out Rfc3161TimestampTokenInfo timestampTokenInfo, out int bytesConsumed) {
            throw new PlatformNotSupportedException ();
        }

        /// <param name="destination" />
        /// <param name="bytesWritten" />
        public bool TryEncode (Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }
    }
}
