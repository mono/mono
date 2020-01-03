using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Enables the inspection of and creation of PKCS#8 PrivateKeyInfo and EncryptedPrivateKeyInfo values. This class cannot be inherited.</summary>
    public sealed class Pkcs8PrivateKeyInfo {
        /// <summary>Gets the Object Identifier (OID) value identifying the algorithm this key is for.</summary>
        /// <returns>The Object Identifier (OID) value identifying the algorithm this key is for.</returns>
        public Oid AlgorithmId {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets a memory value containing the BER-encoded algorithm parameters associated with this key.</summary>
        /// <returns>A memory value containing the BER-encoded algorithm parameters associated with this key, or <see langword="null" /> if no parameters were present.</returns>
        public ReadOnlyMemory<byte>? AlgorithmParameters {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets the modifiable collection of attributes for this private key.</summary>
        /// <returns>The modifiable collection of attributes to encode with the private key.</returns>
        public CryptographicAttributeObjectCollection Attributes {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets a memory value that represents the algorithm-specific encoded private key.</summary>
        /// <returns>A memory value that represents the algorithm-specific encoded private key.</returns>
        public ReadOnlyMemory<byte> PrivateKeyBytes {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs8PrivateKeyInfo" /> class.</summary>
        /// <param name="algorithmId">The Object Identifier (OID) identifying the asymmetric algorithm this key is for.</param>
        /// <param name="algorithmParameters">The BER-encoded algorithm parameters associated with this key, or <see langword="null" /> to omit algorithm parameters when encoding.</param>
        /// <param name="privateKey">The algorithm-specific encoded private key.</param>
        /// <param name="skipCopies">
        ///   <see langword="true" /> to store <paramref name="algorithmParameters" /> and <paramref name="privateKey" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="algorithmId" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="algorithmParameters" /> parameter is not <see langword="null" />, empty, or a single BER-encoded value.</exception>
        public Pkcs8PrivateKeyInfo (Oid algorithmId, ReadOnlyMemory<byte>? algorithmParameters, ReadOnlyMemory<byte> privateKey, bool skipCopies = false) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Exports a specified key as a PKCS#8 PrivateKeyInfo and returns its decoded interpretation.</summary>
        /// <param name="privateKey">The private key to represent in a PKCS#8 PrivateKeyInfo.</param>
        /// <returns>The decoded interpretation of the exported PKCS#8 PrivateKeyInfo.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="privateKey" /> parameter is <see langword="null" />.</exception>
        public static Pkcs8PrivateKeyInfo Create (AsymmetricAlgorithm privateKey) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Reads the provided data as a PKCS#8 PrivateKeyInfo and returns an object view of the contents.</summary>
        /// <param name="source">The data to interpret as a PKCS#8 PrivateKeyInfo value.</param>
        /// <param name="bytesRead">When this method returns, contains a value that indicates the number of bytes read from <paramref name="source" />. This parameter is treated as uninitialized.</param>
        /// <param name="skipCopy">
        ///   <see langword="true" /> to store <paramref name="source" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <returns>An object view of the contents decoded as a PKCS#8 PrivateKeyInfo.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The contents of the <paramref name="source" /> parameter were not successfully decoded as a PKCS#8 PrivateKeyInfo.</exception>
        public static Pkcs8PrivateKeyInfo Decode (ReadOnlyMemory<byte> source, out int bytesRead, bool skipCopy = false) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the provided data using the provided byte-based password and decodes the output into an object view of the PKCS#8 PrivateKeyInfo.</summary>
        /// <param name="passwordBytes">The bytes to use as a password when decrypting the key material.</param>
        /// <param name="source">The data to read as a PKCS#8 EncryptedPrivateKeyInfo structure in the ASN.1-BER encoding.</param>
        /// <param name="bytesRead">When this method returns, contains a value that indicates the number of bytes read from <paramref name="source" />. This parameter is treated as uninitialized.</param>
        /// <returns>An object view of the contents decrypted decoded as a PKCS#8 PrivateKeyInfo.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The password is incorrect.
        /// -or-
        /// The contents of <paramref name="source" /> indicate the Key Derivation Function (KDF) to apply is the legacy PKCS#12 KDF, which requires <see cref="T:System.Char" />-based passwords.
        /// -or-
        /// The contents of <paramref name="source" /> do not represent an ASN.1-BER-encoded PKCS#8 EncryptedPrivateKeyInfo structure.</exception>
        public static Pkcs8PrivateKeyInfo DecryptAndDecode (ReadOnlySpan<byte> passwordBytes, ReadOnlyMemory<byte> source, out int bytesRead) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the provided data using the provided character-based password and decodes the output into an object view of the PKCS#8 PrivateKeyInfo.</summary>
        /// <param name="password">The password to use when decrypting the key material.</param>
        /// <param name="source">The bytes of a PKCS#8 EncryptedPrivateKeyInfo structure in the ASN.1-BER encoding.</param>
        /// <param name="bytesRead">When this method returns, contains a value that indicates the number of bytes read from <paramref name="source" />. This parameter is treated as uninitialized.</param>
        /// <returns>An object view of the contents decrypted decoded as a PKCS#8 PrivateKeyInfo.</returns>
        public static Pkcs8PrivateKeyInfo DecryptAndDecode (ReadOnlySpan<char> password, ReadOnlyMemory<byte> source, out int bytesRead) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Encodes the property data of this instance as a PKCS#8 PrivateKeyInfo and returns the encoding as a byte array.</summary>
        /// <returns>A byte array representing the encoded form of the PKCS#8 PrivateKeyInfo.</returns>
        public byte[] Encode () {
            throw new PlatformNotSupportedException();
        }

        /// <summary>Produces a PKCS#8 EncryptedPrivateKeyInfo from the property contents of this object after encrypting with the specified byte-based password and encryption parameters.</summary>
        /// <param name="passwordBytes">The bytes to use as a password when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>A byte array containing the encoded form of the PKCS#8 EncryptedPrivateKeyInfo.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        ///   <paramref name="pbeParameters" /> indicates that <see cref="F:System.Security.Cryptography.PbeEncryptionAlgorithm.TripleDes3KeyPkcs12" /> should be used, which requires <see cref="T:System.Char" />-based passwords.</exception>
        public byte[] Encrypt (ReadOnlySpan<byte> passwordBytes, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Produces a PKCS#8 EncryptedPrivateKeyInfo from the property contents of this object after encrypting with the specified character-based password and encryption parameters.</summary>
        /// <param name="password">The password to use when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>A byte array containing the encoded form of the PKCS#8 EncryptedPrivateKeyInfo.</returns>
        public byte[] Encrypt (ReadOnlySpan<char> password, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to encode the property data of this instance as a PKCS#8 PrivateKeyInfo, writing the results into a provided buffer.</summary>
        /// <param name="destination">The byte span to receive the PKCS#8 PrivateKeyInfo data.</param>
        /// <param name="bytesWritten">When this method returns, contains a value that indicates the number of bytes written to <paramref name="destination" />. This parameter is treated as uninitialized.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="destination" /> is big enough to receive the output; otherwise, <see langword="false" />.</returns>
        public bool TryEncode (Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to produce a PKCS#8 EncryptedPrivateKeyInfo from the property contents of this object after encrypting with the specified byte-based password and encryption parameters, writing the results into a provided buffer.</summary>
        /// <param name="passwordBytes">The bytes to use as a password when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <param name="destination">The byte span to receive the PKCS#8 EncryptedPrivateKeyInfo data.</param>
        /// <param name="bytesWritten">When this method returns, contains a value that indicates the number of bytes written to <paramref name="destination" />. This parameter is treated as uninitialized.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="destination" /> is big enough to receive the output; otherwise, <see langword="false" />.</returns>
        public bool TryEncrypt (ReadOnlySpan<byte> passwordBytes, PbeParameters pbeParameters, Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to produce a PKCS#8 EncryptedPrivateKeyInfo from the property contents of this object after encrypting with the specified character-based password and encryption parameters, writing the result into a provided buffer.</summary>
        /// <param name="password">The password to use when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <param name="destination">The byte span to receive the PKCS#8 EncryptedPrivateKeyInfo data.</param>
        /// <param name="bytesWritten">When this method returns, contains a value that indicates the number of bytes written to <paramref name="destination" />. This parameter is treated as uninitialized.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="destination" /> is big enough to receive the output; otherwise, <see langword="false" />.</returns>
        public bool TryEncrypt (ReadOnlySpan<char> password, PbeParameters pbeParameters, Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }
    }
}
