//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.InfoCards
{
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;

    // copied from IdentityModel\CryptoHelper.cs and they need to be kept in [....].  After V1, we need to rethink how we can have 
    // a single place to ask this question.  Perhaps even add it as an extensibility

    internal static class InfoCardCryptoHelper
    {

        internal static bool IsAsymmetricAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case SecurityAlgorithms.HmacSha1Signature:
                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                case SecurityAlgorithms.TripleDesKeyWrap:
                case SecurityAlgorithms.Psha1KeyDerivation:
                case SecurityAlgorithms.Psha1KeyDerivationDec2005:
                    return true;
                default:
                    return false;
            }
        }

    }
}
