// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;

namespace System.Security.Cryptography.X509Certificates
{
    public sealed partial class CertificateRequest
    {
        public CertificateRequest(X500DistinguishedName subjectName, ECDsa key, HashAlgorithmName hashAlgorithm) => throw new PlatformNotSupportedException();
        public CertificateRequest(X500DistinguishedName subjectName, RSA key, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new PlatformNotSupportedException();
        public CertificateRequest(X500DistinguishedName subjectName, PublicKey publicKey, HashAlgorithmName hashAlgorithm) => throw new PlatformNotSupportedException();
        public CertificateRequest(string subjectName, ECDsa key, HashAlgorithmName hashAlgorithm) => throw new PlatformNotSupportedException();
        public CertificateRequest(string subjectName, RSA key, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new PlatformNotSupportedException();
        public Collection<X509Extension> CertificateExtensions => throw new PlatformNotSupportedException();
        public HashAlgorithmName HashAlgorithm => throw new PlatformNotSupportedException();
        public PublicKey PublicKey => throw new PlatformNotSupportedException();
        public X500DistinguishedName SubjectName => throw new PlatformNotSupportedException();
        public X509Certificate2 Create(X500DistinguishedName issuerName, X509SignatureGenerator generator, System.DateTimeOffset notBefore, System.DateTimeOffset notAfter, byte[] serialNumber) => throw new PlatformNotSupportedException();
        public X509Certificate2 Create(X509Certificate2 issuerCertificate, System.DateTimeOffset notBefore, System.DateTimeOffset notAfter, byte[] serialNumber) => throw new PlatformNotSupportedException();
        public X509Certificate2 CreateSelfSigned(System.DateTimeOffset notBefore, System.DateTimeOffset notAfter) => throw new PlatformNotSupportedException();
        public byte[] CreateSigningRequest() => throw new PlatformNotSupportedException();
        public byte[] CreateSigningRequest(X509SignatureGenerator signatureGenerator) => throw new PlatformNotSupportedException();
    }
}