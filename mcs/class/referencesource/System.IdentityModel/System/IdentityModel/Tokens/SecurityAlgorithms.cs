//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    public static class SecurityAlgorithms
    {
        public const string Aes128Encryption = SecurityAlgorithmStrings.Aes128Encryption;

        public const string Aes128KeyWrap = SecurityAlgorithmStrings.Aes128KeyWrap;
        public const string Aes192Encryption = SecurityAlgorithmStrings.Aes192Encryption;
        public const string Aes192KeyWrap = SecurityAlgorithmStrings.Aes192KeyWrap;
        public const string Aes256Encryption = SecurityAlgorithmStrings.Aes256Encryption;
        public const string Aes256KeyWrap = SecurityAlgorithmStrings.Aes256KeyWrap;
        public const string DesEncryption = SecurityAlgorithmStrings.DesEncryption;

        public const string DsaSha1Signature = SecurityAlgorithmStrings.DsaSha1Signature;

        public const string ExclusiveC14n = SecurityAlgorithmStrings.ExclusiveC14n;
        public const string ExclusiveC14nWithComments = SecurityAlgorithmStrings.ExclusiveC14nWithComments;
        public const string HmacSha1Signature = SecurityAlgorithmStrings.HmacSha1Signature;
        public const string HmacSha256Signature = SecurityAlgorithmStrings.HmacSha256Signature;

        public const string Psha1KeyDerivation = SecurityAlgorithmStrings.Psha1KeyDerivation;
        public const string Psha1KeyDerivationDec2005 = SecurityAlgorithmDec2005Strings.Psha1KeyDerivationDec2005;

        public const string Ripemd160Digest = SecurityAlgorithmStrings.Ripemd160Digest;
        public const string RsaOaepKeyWrap = SecurityAlgorithmStrings.RsaOaepKeyWrap;
        public const string RsaSha1Signature = SecurityAlgorithmStrings.RsaSha1Signature;
        public const string RsaSha256Signature = SecurityAlgorithmStrings.RsaSha256Signature;
        public const string RsaV15KeyWrap = SecurityAlgorithmStrings.RsaV15KeyWrap;

        public const string Sha1Digest = SecurityAlgorithmStrings.Sha1Digest;
        public const string Sha256Digest = SecurityAlgorithmStrings.Sha256Digest;
        public const string Sha512Digest = SecurityAlgorithmStrings.Sha512Digest;
        public const string StrTransform = SecurityAlgorithmStrings.StrTransform;
        public const string TripleDesEncryption = SecurityAlgorithmStrings.TripleDesEncryption;
        public const string TripleDesKeyWrap = SecurityAlgorithmStrings.TripleDesKeyWrap;

        public const string TlsSspiKeyWrap = SecurityAlgorithmStrings.TlsSspiKeyWrap;
        public const string WindowsSspiKeyWrap = SecurityAlgorithmStrings.WindowsSspiKeyWrap;

        internal const int DefaultSymmetricKeyLength = 256;
        internal const string DefaultEncryptionAlgorithm = Aes256Encryption;
        internal const string DefaultAsymmetricKeyWrapAlgorithm = RsaOaepKeyWrap;
        internal const string DefaultAsymmetricSignatureAlgorithm = RsaSha256Signature;
        internal const string DefaultDigestAlgorithm = Sha256Digest;
    }
}
