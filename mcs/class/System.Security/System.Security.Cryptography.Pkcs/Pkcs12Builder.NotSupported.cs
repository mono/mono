using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Enables the creation of PKCS#12 PFX data values. This class cannot be inherited.</summary>
    public sealed class Pkcs12Builder {
        /// <summary>Gets a value that indicates whether the PFX data has been sealed.</summary>
        /// <returns>A value that indicates whether the PFX data has been sealed.</returns>
        public bool IsSealed {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Add contents to the PFX in an bundle encrypted with a byte-based password from a byte array.</summary>
        /// <param name="safeContents">The contents to add to the PFX.</param>
        /// <param name="passwordBytes">The byte array to use as a password when encrypting the contents.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the contents.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> or <paramref name="pbeParameters" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="safeContents" /> parameter value is already encrypted.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        ///   <paramref name="pbeParameters" /> indicates that <see cref="F:System.Security.Cryptography.PbeEncryptionAlgorithm.TripleDes3KeyPkcs12" /> should be used, which requires <see cref="T:System.Char" />-based passwords.</exception>
        public void AddSafeContentsEncrypted (Pkcs12SafeContents safeContents, byte[] passwordBytes, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Add contents to the PFX in an bundle encrypted with a byte-based password from a span.</summary>
        /// <param name="safeContents">The contents to add to the PFX.</param>
        /// <param name="passwordBytes">The byte span to use as a password when encrypting the contents.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the contents.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> or <paramref name="pbeParameters" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="safeContents" /> parameter value is already encrypted.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        ///   <paramref name="pbeParameters" /> indicates that <see cref="F:System.Security.Cryptography.PbeEncryptionAlgorithm.TripleDes3KeyPkcs12" /> should be used, which requires <see cref="T:System.Char" />-based passwords.</exception>
        public void AddSafeContentsEncrypted (Pkcs12SafeContents safeContents, ReadOnlySpan<byte> passwordBytes, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Add contents to the PFX in an bundle encrypted with a char-based password from a span.</summary>
        /// <param name="safeContents">The contents to add to the PFX.</param>
        /// <param name="password">The span to use as a password when encrypting the contents.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the contents.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> or <paramref name="pbeParameters" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="safeContents" /> parameter value is already encrypted.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void AddSafeContentsEncrypted (Pkcs12SafeContents safeContents, ReadOnlySpan<char> password, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Add contents to the PFX in an bundle encrypted with a char-based password from a string.</summary>
        /// <param name="safeContents">The contents to add to the PFX.</param>
        /// <param name="password">The string to use as a password when encrypting the contents.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the contents.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> or <paramref name="pbeParameters" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="safeContents" /> parameter value is already encrypted.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void AddSafeContentsEncrypted (Pkcs12SafeContents safeContents, string password, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Add contents to the PFX without encrypting them.</summary>
        /// <param name="safeContents">The contents to add to the PFX.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void AddSafeContentsUnencrypted (Pkcs12SafeContents safeContents) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Encodes the contents of a sealed PFX and returns it as a byte array.</summary>
        /// <returns>A byte array representing the encoded form of the PFX.</returns>
        /// <exception cref="T:System.InvalidOperationException">The PFX is not sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="false" />).</exception>
        public byte[] Encode () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Seals the PFX against further changes by applying a password-based Message Authentication Code (MAC) over the contents with a password from a span.</summary>
        /// <param name="password">The password to use as a key for computing the MAC.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use when computing the MAC.</param>
        /// <param name="iterationCount">The iteration count for the Key Derivation Function (KDF) used in computing the MAC.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="iterationCount" /> parameter is less than or equal to 0.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void SealWithMac (ReadOnlySpan<char> password, HashAlgorithmName hashAlgorithm, int iterationCount) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Seals the PFX against further changes by applying a password-based Message Authentication Code (MAC) over the contents with a password from a string.</summary>
        /// <param name="password">The password to use as a key for computing the MAC.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use when computing the MAC.</param>
        /// <param name="iterationCount">The iteration count for the Key Derivation Function (KDF) used in computing the MAC.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="iterationCount" /> parameter is less than or equal to 0.</exception>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void SealWithMac (string password, HashAlgorithmName hashAlgorithm, int iterationCount) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Seals the PFX from further changes without applying tamper-protection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The PFX is already sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="true" />).</exception>
        public void SealWithoutIntegrity () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to encode the contents of a sealed PFX into a provided buffer.</summary>
        /// <param name="destination">The byte span to receive the PKCS#12 PFX data.</param>
        /// <param name="bytesWritten">When this method returns, contains a value that indicates the number of bytes written to <paramref name="destination" />. This parameter is treated as uninitialized.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="destination" /> is big enough to receive the output; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.InvalidOperationException">The PFX is not sealed (<see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Builder.IsSealed" /> is <see langword="false" />).</exception>
        public bool TryEncode (Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }
    }
}
