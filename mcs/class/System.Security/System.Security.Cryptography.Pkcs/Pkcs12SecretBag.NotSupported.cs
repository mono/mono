using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the SecretBag from PKCS#12, a container whose contents are arbitrary data with a type identifier. This class cannot be inherited.</summary>
    public sealed class Pkcs12SecretBag : Pkcs12SafeBag {
        /// <summary>Gets a memory value containing the BER-encoded contents of the bag.</summary>
        /// <returns>A memory value containing the BER-encoded contents of the bag.</returns>
        public ReadOnlyMemory<byte> SecretValue {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        internal Pkcs12SecretBag ()
            : base (null, default(ReadOnlyMemory<byte>)) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets the Object Identifier (OID) which identifies the data type of the secret value.</summary>
        /// <returns>The Object Identifier (OID) which identifies the data type of the secret value.</returns>
        public Oid GetSecretType () {
            throw new PlatformNotSupportedException ();
        }
    }
}
