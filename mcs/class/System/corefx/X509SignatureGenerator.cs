// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Security.Cryptography.X509Certificates
{
    public abstract partial class X509SignatureGenerator
    {
        protected X509SignatureGenerator() => throw new PlatformNotSupportedException();
        public PublicKey PublicKey => throw new PlatformNotSupportedException();
        protected abstract PublicKey BuildPublicKey();
        public static X509SignatureGenerator CreateForECDsa(ECDsa key) => throw new PlatformNotSupportedException();
        public static X509SignatureGenerator CreateForRSA(RSA key, RSASignaturePadding signaturePadding) => throw new PlatformNotSupportedException();
        public abstract byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName hashAlgorithm);
        public abstract byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm);
    }
}