using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents a PKCS#12 SafeContents value. This class cannot be inherited.</summary>
    public sealed class Pkcs12SafeContents {
        /// <summary>Gets a value that indicates the type of encryption applied to the contents.</summary>
        /// <returns>One of the enumeration values that indicates the type of encryption applied to the contents. The default value is <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12ConfidentialityMode.None" />.</returns>
        public Pkcs12ConfidentialityMode ConfidentialityMode {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets a value that indicates whether this instance in a read-only state.</summary>
        /// <returns>
        ///   <see langword="true" /> if this value is in a read-only state; otherwise, <see langword="false" />. The default value is <see langword="false" />.</returns>
        public bool IsReadOnly {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Adds a certificate to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12CertBag" /> and returns the newly created bag instance.</summary>
        /// <param name="certificate">The certificate to add.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="certificate" /> parameter is in an invalid state.</exception>
        public Pkcs12CertBag AddCertificate (X509Certificate2 certificate) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an asymmetric private key to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12KeyBag" /> and returns the newly created bag instance.</summary>
        /// <param name="key">The asymmetric private key to add.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The key export failed.</exception>
        public Pkcs12KeyBag AddKeyUnencrypted (AsymmetricAlgorithm key) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds a nested SafeContents to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12SafeContentsBag" /> and returns the newly created bag instance.</summary>
        /// <param name="safeContents">The nested contents to add to the SafeContents.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeContents" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="safeContents" /> parameter is encrypted.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        public Pkcs12SafeContentsBag AddNestedContents (Pkcs12SafeContents safeContents) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds a SafeBag to the SafeContents.</summary>
        /// <param name="safeBag">The SafeBag value to add.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="safeBag" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        public void AddSafeBag (Pkcs12SafeBag safeBag) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an ASN.1 BER-encoded value with a specified type identifier to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12SecretBag" /> and returns the newly created bag instance.</summary>
        /// <param name="secretType">The Object Identifier (OID) which identifies the data type of the secret value.</param>
        /// <param name="secretValue">The BER-encoded value representing the secret to add.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="secretType" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="secretValue" /> parameter does not represent a single ASN.1 BER-encoded value.</exception>
        public Pkcs12SecretBag AddSecret (Oid secretType, ReadOnlyMemory<byte> secretValue) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an encrypted asymmetric private key to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12ShroudedKeyBag" /> from a byte-based password in an array and returns the newly created bag instance.</summary>
        /// <param name="key">The asymmetric private key to add.</param>
        /// <param name="passwordBytes">The bytes to use as a password when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The key export failed.</exception>
        public Pkcs12ShroudedKeyBag AddShroudedKey (AsymmetricAlgorithm key, byte[] passwordBytes, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an encrypted asymmetric private key to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12ShroudedKeyBag" /> from a byte-based password in a span and returns the newly created bag instance.</summary>
        /// <param name="key">The asymmetric private key to add.</param>
        /// <param name="passwordBytes">The bytes to use as a password when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The key export failed.</exception>
        public Pkcs12ShroudedKeyBag AddShroudedKey (AsymmetricAlgorithm key, ReadOnlySpan<byte> passwordBytes, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an encrypted asymmetric private key to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12ShroudedKeyBag" /> from a character-based password in a span and returns the newly created bag instance.</summary>
        /// <param name="key">The asymmetric private key to add.</param>
        /// <param name="password">The password to use when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The key export failed.</exception>
        public Pkcs12ShroudedKeyBag AddShroudedKey (AsymmetricAlgorithm key, ReadOnlySpan<char> password, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Adds an encrypted asymmetric private key to the SafeContents via a new <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12ShroudedKeyBag" /> from a character-based password in a string and returns the newly created bag instance.</summary>
        /// <param name="key">The asymmetric private key to add.</param>
        /// <param name="password">The password to use when encrypting the key material.</param>
        /// <param name="pbeParameters">The password-based encryption (PBE) parameters to use when encrypting the key material.</param>
        /// <returns>The bag instance which was added to the SafeContents.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">This instance is read-only.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The key export failed.</exception>
        public Pkcs12ShroudedKeyBag AddShroudedKey (AsymmetricAlgorithm key, string password, PbeParameters pbeParameters) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the contents of this SafeContents value using a byte-based password from an array.</summary>
        /// <param name="passwordBytes">The bytes to use as a password for decrypting the encrypted contents.</param>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12SafeContents.ConfidentialityMode" /> property is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12ConfidentialityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The password is incorrect.
        /// -or-
        /// The contents were not successfully decrypted.</exception>
        public void Decrypt (byte[] passwordBytes) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the contents of this SafeContents value using a byte-based password from a span.</summary>
        /// <param name="passwordBytes">The bytes to use as a password for decrypting the encrypted contents.</param>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12SafeContents.ConfidentialityMode" /> property is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12ConfidentialityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The password is incorrect.
        /// -or-
        /// The contents were not successfully decrypted.</exception>
        public void Decrypt (ReadOnlySpan<byte> passwordBytes) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the contents of this SafeContents value using a character-based password from a span.</summary>
        /// <param name="password">The password to use for decrypting the encrypted contents.</param>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12SafeContents.ConfidentialityMode" /> property is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12ConfidentialityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The password is incorrect.
        /// -or-
        /// The contents were not successfully decrypted.</exception>
        public void Decrypt (ReadOnlySpan<char> password) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Decrypts the contents of this SafeContents value using a character-based password from a string.</summary>
        /// <param name="password">The password to use for decrypting the encrypted contents.</param>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12SafeContents.ConfidentialityMode" /> property is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12ConfidentialityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The password is incorrect.
        /// -or-
        /// The contents were not successfully decrypted.</exception>
        public void Decrypt (string password) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets an enumerable representation of the SafeBag values contained within the SafeContents.</summary>
        /// <returns>An enumerable representation of the SafeBag values contained within the SafeContents.</returns>
        /// <exception cref="T:System.InvalidOperationException">The contents are encrypted.</exception>
        public IEnumerable<Pkcs12SafeBag> GetBags () {
            throw new PlatformNotSupportedException ();
        }
    }
}
