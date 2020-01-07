using System;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the ShroudedKeyBag from PKCS#12, a container whose contents are a PKCS#8 EncryptedPrivateKeyInfo. This class cannot be inherited.</summary>
    public sealed class Pkcs12ShroudedKeyBag : Pkcs12SafeBag {
        /// <summary>Gets a memory value containing the PKCS#8 EncryptedPrivateKeyInfo value transported by this bag.</summary>
        /// <returns>A memory value containing the PKCS#8 EncryptedPrivateKeyInfo value transported by this bag.</returns>
        public ReadOnlyMemory<byte> EncryptedPkcs8PrivateKey {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12ShroudedKeyBag" /> from an existing encoded PKCS#8 EncryptedPrivateKeyInfo value.</summary>
        /// <param name="encryptedPkcs8PrivateKey">A BER-encoded PKCS#8 EncryptedPrivateKeyInfo value.</param>
        /// <param name="skipCopy">
        ///   <see langword="true" /> to store <paramref name="encryptedPkcs8PrivateKey" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="encryptedPkcs8privateKey" /> parameter does not represent a single ASN.1 BER-encoded value.</exception>
        public Pkcs12ShroudedKeyBag (ReadOnlyMemory<byte> encryptedPkcs8PrivateKey, bool skipCopy = false)
            : base (null, default(ReadOnlyMemory<byte>)) {
            throw new PlatformNotSupportedException ();
        }
    }
}
