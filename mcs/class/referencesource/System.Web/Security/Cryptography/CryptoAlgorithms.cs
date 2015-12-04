//------------------------------------------------------------------------------
// <copyright file="CryptoAlgorithms.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;

    // Utility class to provide the "one true way" of getting instances of
    // cryptographic algorithms, like SymmetricAlgorithm and HashAlgorithm.

    // From discussions with [....] and the crypto board, we should prefer
    // the CNG implementations of algorithms, then the CAPI implementations,
    // then finally managed implementations if there are no CNG / CAPI
    // implementations. The CNG / CAPI implementations are preferred for
    // expandability, FIPS-compliance, and performance.
    //
    // .NET Framework 4.5 allows us to make two core assumptions:
    // - The built-in HMAC classes have been updated for FIPS compliance.
    // - Since .NET 4.5 requires Windows Server 2008 or greater, we can
    //   assume that CNG is available on the box.
    //
    // Note that some algorithms (MD5, DES, etc.) aren't FIPS-compliant
    // under any circumstance. Calling these methods when the OS is
    // configured to allow only FIPS-compliant algorithms will result
    // in an exception being thrown.
    //
    // The .NET Framework's built-in algorithms don't need to be created
    // under the application impersonation context since they don't depend
    // on the impersonated identity.

    internal static class CryptoAlgorithms {

        internal static Aes CreateAes() {
            return new AesCryptoServiceProvider();
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5351:DESCannotBeUsed", Justification = @"This is only used by legacy code; new features do not use this algorithm.")]
        [Obsolete("DES is deprecated and MUST NOT be used by new features. Consider using AES instead.")]
        internal static DES CreateDES() {
            return new DESCryptoServiceProvider();
        }

        internal static HMACSHA1 CreateHMACSHA1() {
            return new HMACSHA1();
        }

        internal static HMACSHA256 CreateHMACSHA256() {
            return new HMACSHA256();
        }

        internal static HMACSHA384 CreateHMACSHA384() {
            return new HMACSHA384();
        }

        internal static HMACSHA512 CreateHMACSHA512() {
            return new HMACSHA512();
        }

        internal static HMACSHA512 CreateHMACSHA512(byte[] key) {
            return new HMACSHA512(key);
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", Justification = @"This is only used by legacy code; new features do not use this algorithm.")]
        [Obsolete("MD5 is deprecated and MUST NOT be used by new features. Consider using a SHA-2 algorithm instead.")]
        internal static MD5 CreateMD5() {
            return new MD5Cng();
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5354:SHA1CannotBeUsed", Justification = @"This is only used by legacy code; new features do not use this algorithm.")]
        [Obsolete("SHA1 is deprecated and MUST NOT be used by new features. Consider using a SHA-2 algorithm instead.")]
        internal static SHA1 CreateSHA1() {
            return new SHA1Cng();
        }

        internal static SHA256 CreateSHA256() {
            return new SHA256Cng();
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5353:TripleDESCannotBeUsed", Justification = @"This is only used by legacy code; new features do not use this algorithm.")]
        [Obsolete("3DES is deprecated and MUST NOT be used by new features. Consider using AES instead.")]
        internal static TripleDES CreateTripleDES() {
            return new TripleDESCryptoServiceProvider();
        }

    }
}
