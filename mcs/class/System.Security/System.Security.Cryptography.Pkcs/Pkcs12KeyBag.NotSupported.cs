using System;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the KeyBag from PKCS#12, a container whose contents are a PKCS#8 PrivateKeyInfo. This class cannot be inherited.</summary>
    public sealed class Pkcs12KeyBag : Pkcs12SafeBag {
        /// <summary>Gets a memory value containing the PKCS#8 PrivateKeyInfo value transported by this bag.</summary>
        /// <returns>A memory value containing the PKCS#8 PrivateKeyInfo value transported by this bag.</returns>
        public ReadOnlyMemory<byte> Pkcs8PrivateKey {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12KeyBag" /> from an existing encoded PKCS#8 PrivateKeyInfo value.</summary>
        /// <param name="pkcs8PrivateKey">A BER-encoded PKCS#8 PrivateKeyInfo value.</param>
        /// <param name="skipCopy">
        ///   <see langword="true" /> to store <paramref name="pkcs8PrivateKey" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="pkcs8privateKey" /> parameter does not represent a single ASN.1 BER-encoded value.</exception>
        public Pkcs12KeyBag (ReadOnlyMemory<byte> pkcs8PrivateKey, bool skipCopy = false)
            : base (null, default(ReadOnlyMemory<byte>)) {
            throw new PlatformNotSupportedException ();
        }
    }
}
