namespace System.Security.Cryptography.X509Certificates
{
    public sealed partial class CertificateRequest
    {
        public CertificateRequest(System.Security.Cryptography.X509Certificates.X500DistinguishedName subjectName, System.Security.Cryptography.ECDsa key, System.Security.Cryptography.HashAlgorithmName hashAlgorithm) { }
        public CertificateRequest(System.Security.Cryptography.X509Certificates.X500DistinguishedName subjectName, System.Security.Cryptography.RSA key, System.Security.Cryptography.HashAlgorithmName hashAlgorithm, System.Security.Cryptography.RSASignaturePadding padding) { }
        public CertificateRequest(System.Security.Cryptography.X509Certificates.X500DistinguishedName subjectName, System.Security.Cryptography.X509Certificates.PublicKey publicKey, System.Security.Cryptography.HashAlgorithmName hashAlgorithm) { }
        public CertificateRequest(string subjectName, System.Security.Cryptography.ECDsa key, System.Security.Cryptography.HashAlgorithmName hashAlgorithm) { }
        public CertificateRequest(string subjectName, System.Security.Cryptography.RSA key, System.Security.Cryptography.HashAlgorithmName hashAlgorithm, System.Security.Cryptography.RSASignaturePadding padding) { }
        public System.Collections.ObjectModel.Collection<System.Security.Cryptography.X509Certificates.X509Extension> CertificateExtensions { get { throw null; } }
        public System.Security.Cryptography.HashAlgorithmName HashAlgorithm { get { throw null; } }
        public System.Security.Cryptography.X509Certificates.PublicKey PublicKey { get { throw null; } }
        public System.Security.Cryptography.X509Certificates.X500DistinguishedName SubjectName { get { throw null; } }
        public System.Security.Cryptography.X509Certificates.X509Certificate2 Create(System.Security.Cryptography.X509Certificates.X500DistinguishedName issuerName, System.Security.Cryptography.X509Certificates.X509SignatureGenerator generator, System.DateTimeOffset notBefore, System.DateTimeOffset notAfter, byte[] serialNumber) { throw null; }
        public System.Security.Cryptography.X509Certificates.X509Certificate2 Create(System.Security.Cryptography.X509Certificates.X509Certificate2 issuerCertificate, System.DateTimeOffset notBefore, System.DateTimeOffset notAfter, byte[] serialNumber) { throw null; }
        public System.Security.Cryptography.X509Certificates.X509Certificate2 CreateSelfSigned(System.DateTimeOffset notBefore, System.DateTimeOffset notAfter) { throw null; }
        public byte[] CreateSigningRequest() { throw null; }
        public byte[] CreateSigningRequest(System.Security.Cryptography.X509Certificates.X509SignatureGenerator signatureGenerator) { throw null; }
    }

    public abstract partial class X509SignatureGenerator
    {
        protected X509SignatureGenerator() { }
        public System.Security.Cryptography.X509Certificates.PublicKey PublicKey { get { throw null; } }
        protected abstract System.Security.Cryptography.X509Certificates.PublicKey BuildPublicKey();
        public static System.Security.Cryptography.X509Certificates.X509SignatureGenerator CreateForECDsa(System.Security.Cryptography.ECDsa key) { throw null; }
        public static System.Security.Cryptography.X509Certificates.X509SignatureGenerator CreateForRSA(System.Security.Cryptography.RSA key, System.Security.Cryptography.RSASignaturePadding signaturePadding) { throw null; }
        public abstract byte[] GetSignatureAlgorithmIdentifier(System.Security.Cryptography.HashAlgorithmName hashAlgorithm);
        public abstract byte[] SignData(byte[] data, System.Security.Cryptography.HashAlgorithmName hashAlgorithm);
    }
}