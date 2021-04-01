using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the PKCS#12 CertBag. This class cannot be inherited.</summary>
    public sealed class Pkcs12CertBag : Pkcs12SafeBag {
        /// <summary>Gets the uninterpreted certificate contents of the CertSafeBag.</summary>
        /// <returns>The uninterpreted certificate contents of the CertSafeBag.</returns>
        public ReadOnlyMemory<byte> EncodedCertificate {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets a value indicating whether the content type of the encoded certificate value is the X.509 public key certificate content type.</summary>
        /// <returns>
        ///   <see langword="true" /> if the content type is the X.509 public key certificate content type (1.2.840.113549.1.9.22.1); otherwise, <see langword="false" />.</returns>
        public bool IsX509Certificate {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs12CertBag" /> class using the specified certificate type and encoding.</summary>
        /// <param name="certificateType">The Object Identifier (OID) for the certificate type.</param>
        /// <param name="encodedCertificate">The encoded certificate value.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="certificateType" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="encodedCertificate" /> parameter does not represent a single ASN.1 BER-encoded value.</exception>
        public Pkcs12CertBag (Oid certificateType, ReadOnlyMemory<byte> encodedCertificate)
            : base (null, default(ReadOnlyMemory<byte>)) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets the contents of the CertBag interpreted as an X.509 public key certificate.</summary>
        /// <returns>A certificate decoded from the contents of the CertBag.</returns>
        /// <exception cref="">The content type is not the X.509 public key certificate content type.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The contents were not valid for the X.509 certificate content type.</exception>
        public X509Certificate2 GetCertificate () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Gets the Object Identifier (OID) which identifies the content type of the encoded certificte value.</summary>
        /// <returns>The Object Identifier (OID) which identifies the content type of the encoded certificate value.</returns>
        public Oid GetCertificateType () {
            throw new PlatformNotSupportedException ();
        }
    }
}