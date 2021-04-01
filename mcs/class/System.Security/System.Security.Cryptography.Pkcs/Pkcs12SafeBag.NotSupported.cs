using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Defines the core behavior of a SafeBag value from the PKCS#12 specification and provides a base for derived classes.</summary>
    public abstract class Pkcs12SafeBag {
        /// <summary>Gets the modifiable collection of attributes to encode with the SafeBag value.</summary>
        /// <returns>The modifiable collection of attributes to encode with the SafeBag value.</returns>
        public CryptographicAttributeObjectCollection Attributes {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets the ASN.1 BER encoding of the contents of this SafeBag.</summary>
        /// <returns>The ASN.1 BER encoding of the contents of this SafeBag.</returns>
        public ReadOnlyMemory<byte> EncodedBagValue {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Called from constructors in derived classes to initialize the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12SafeBag" /> class.</summary>
        /// <param name="bagIdValue">The Object Identifier (OID), in dotted decimal form, indicating the data type of this SafeBag.</param>
        /// <param name="encodedBagValue">The ASN.1 BER encoded value of the SafeBag contents.</param>
        /// <param name="skipCopy">
        ///   <see langword="true" /> to store <paramref name="encodedBagValue" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="bagIdValue" /> parameter is <see langword="null" /> or the empty string.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="encodedBagValue" /> parameter does not represent a single ASN.1 BER-encoded value.</exception>
        protected Pkcs12SafeBag (string bagIdValue, ReadOnlyMemory<byte> encodedBagValue, bool skipCopy = false) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Encodes the SafeBag value and returns it as a byte array.</summary>
        /// <returns>A byte array representing the encoded form of the SafeBag.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The object identifier value passed to the constructor was invalid.</exception>
        public byte[] Encode () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets the Object Identifier (OID) identifying the content type of this SafeBag.</summary>
        /// <returns>The Object Identifier (OID) identifying the content type of this SafeBag.</returns>
        public Oid GetBagId () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to encode the SafeBag value into a provided buffer.</summary>
        /// <param name="destination">The byte span to receive the encoded SafeBag value.</param>
        /// <param name="bytesWritten">When this method returns, contains a value that indicates the number of bytes written to <paramref name="destination" />. This parameter is treated as uninitialized.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="destination" /> is big enough to receive the output; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The object identifier value passed to the constructor was invalid.</exception>
        public bool TryEncode (Span<byte> destination, out int bytesWritten) {
            throw new PlatformNotSupportedException ();
        }
    }
}
