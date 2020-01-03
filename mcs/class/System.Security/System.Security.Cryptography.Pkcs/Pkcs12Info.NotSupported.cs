using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the data from PKCS#12 PFX contents. This class cannot be inherited.</summary>
    public sealed class Pkcs12Info {
        /// <summary>Gets a read-only collection of the SafeContents values present in the PFX AuthenticatedSafe.</summary>
        /// <returns>A read-only collection of the SafeContents values present in the PFX AuthenticatedSafe.</returns>
        public ReadOnlyCollection<Pkcs12SafeContents> AuthenticatedSafe {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        /// <summary>Gets a value that indicates the type of tamper protection provided for the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents.</summary>
        /// <returns>One of the enumeration members that indicates the type of tamper protection provided for the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents.</returns>
        public Pkcs12IntegrityMode IntegrityMode {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        internal Pkcs12Info () {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Reads the provided data as a PKCS#12 PFX and returns an object view of the contents.</summary>
        /// <param name="encodedBytes">The data to interpret as a PKCS#12 PFX.</param>
        /// <param name="bytesConsumed">When this method returns, contains a value that indicates the number of bytes from <paramref name="encodedBytes" /> which were read by this method. This parameter is treated as uninitialized.</param>
        /// <param name="skipCopy">
        ///   <see langword="true" /> to store <paramref name="encodedBytes" /> without making a defensive copy; otherwise, <see langword="false" />. The default is <see langword="false" />.</param>
        /// <returns>An object view of the PKCS#12 PFX decoded from the input.</returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The contents of the <paramref name="encodedBytes" /> parameter were not successfully decoded as a PKCS#12 PFX.</exception>
        public static Pkcs12Info Decode (ReadOnlyMemory<byte> encodedBytes, out int bytesConsumed, bool skipCopy = false) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to verify the integrity of the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents with a password represented by a <see cref="System.ReadOnlySpan{System.Char}" />.</summary>
        /// <param name="password">The password to use to attempt to verify integrity.</param>
        /// <returns>
        ///   <see langword="true" /> if the password successfully verifies the integrity of the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents; <see langword="false" /> if the password is not correct or the contents have been altered.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.IntegrityMode" /> value is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12IntegrityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash algorithm option specified by the PKCS#12 PFX contents could not be identified or is not supported by this platform.</exception>
        public bool VerifyMac (ReadOnlySpan<char> password) {
            throw new PlatformNotSupportedException ();
        }

        /// <summary>Attempts to verify the integrity of the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents with a password represented by a <see cref="T:System.String" />.</summary>
        /// <param name="password">The password to use to attempt to verify integrity.</param>
        /// <returns>
        ///   <see langword="true" /> if the password successfully verifies the integrity of the <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.AuthenticatedSafe" /> contents; <see langword="false" /> if the password is not correct or the contents have been altered.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs12Info.IntegrityMode" /> value is not <see cref="F:System.Security.Cryptography.Pkcs.Pkcs12IntegrityMode.Password" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash algorithm option specified by the PKCS#12 PFX contents could not be identified or is not supported by this platform.</exception>
        public bool VerifyMac (string password) {
            throw new PlatformNotSupportedException ();
        }
    }
}
