using System;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the LocalKeyId attribute from PKCS#9.</summary>
    public sealed class Pkcs9LocalKeyId : Pkcs9AttributeObject {
        /// <summary>Gets a memory value containing the key identifier from this attribute.</summary>
        /// <returns>A memory value containing the key identifier from this attribute.</returns>
        public ReadOnlyMemory<byte> KeyId {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9LocalKeyId" /> class with an empty key identifier value.</summary>
        public Pkcs9LocalKeyId () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9LocalKeyId" /> class with a key identifier specified by a byte array.</summary>
        /// <param name="keyId">A byte array containing the key identifier.</param>
        public Pkcs9LocalKeyId (byte[] keyId) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9LocalKeyId" /> class with a key identifier specified by a byte span.</summary>
        /// <param name="keyId">A byte array containing the key identifier.</param>
        public Pkcs9LocalKeyId (ReadOnlySpan<byte> keyId) {
            throw new PlatformNotSupportedException ();
        }
    }
}
