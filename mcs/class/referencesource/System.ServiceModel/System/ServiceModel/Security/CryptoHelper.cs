//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Reflection;
    using System.Threading;
    using System.IO;

    using System.Runtime.InteropServices;
    using System.IdentityModel.Tokens;
    using System.Text;
    using System.Xml;
    using System.Diagnostics;
    using System.Security.Cryptography;

    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;
    using CryptoAlgorithms = System.IdentityModel.CryptoHelper;

    static class CryptoHelper
    {
        static byte[] emptyBuffer;
        static readonly RandomNumberGenerator random = new RNGCryptoServiceProvider();

        enum CryptoAlgorithmType
        {
            Unknown,
            Symmetric,
            Asymmetric
        }

        internal static byte[] EmptyBuffer
        {
            get
            {
                if (emptyBuffer == null)
                {
                    byte[] tmp = new byte[0];
                    emptyBuffer = tmp;
                }
                return emptyBuffer;
            }
        }

        internal static HashAlgorithm NewSha1HashAlgorithm()
        {
            return CryptoHelper.CreateHashAlgorithm(SecurityAlgorithms.Sha1Digest);
        }

        internal static HashAlgorithm NewSha256HashAlgorithm()
        {
            return CryptoHelper.CreateHashAlgorithm(SecurityAlgorithms.Sha256Digest);
        }

        internal static HashAlgorithm CreateHashAlgorithm(string digestMethod)
        {
            object algorithmObject = CryptoAlgorithms.GetAlgorithmFromConfig(digestMethod);
            if (algorithmObject != null)
            {
                HashAlgorithm hashAlgorithm = algorithmObject as HashAlgorithm;
                if (hashAlgorithm != null)
                    return hashAlgorithm;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidHashAlgorithm, digestMethod)));
            }

            switch (digestMethod)
            {
                case SecurityAlgorithms.Sha1Digest:
                    if (SecurityUtilsEx.RequiresFipsCompliance)
                        return new SHA1CryptoServiceProvider();
                    else
                        return new SHA1Managed();
                case SecurityAlgorithms.Sha256Digest:
                    if (SecurityUtilsEx.RequiresFipsCompliance)
                        return new SHA256CryptoServiceProvider();
                    else
                        return new SHA256Managed();
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.UnsupportedCryptoAlgorithm, digestMethod)));

            }
        }

        internal static HashAlgorithm CreateHashForAsymmetricSignature(string signatureMethod)
        {
            object algorithmObject = CryptoAlgorithms.GetAlgorithmFromConfig(signatureMethod);
            if (algorithmObject != null)
            {
                HashAlgorithm hashAlgorithm;
                SignatureDescription signatureDescription = algorithmObject as SignatureDescription;

                if (signatureDescription != null)
                {
                    hashAlgorithm = signatureDescription.CreateDigest();
                    if (hashAlgorithm != null)
                        return hashAlgorithm;
                }

                hashAlgorithm = algorithmObject as HashAlgorithm;
                if (hashAlgorithm != null)
                    return hashAlgorithm;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidAsymmetricSignature, signatureMethod)));
            }

            switch (signatureMethod)
            {
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.DsaSha1Signature:
                    if (SecurityUtilsEx.RequiresFipsCompliance)
                        return new SHA1CryptoServiceProvider();
                    else
                        return new SHA1Managed();

                case SecurityAlgorithms.RsaSha256Signature:
                    if (SecurityUtilsEx.RequiresFipsCompliance)
                        return new SHA256CryptoServiceProvider();
                    else
                        return new SHA256Managed();

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.UnsupportedCryptoAlgorithm, signatureMethod)));
            }
        }

        internal static byte[] ExtractIVAndDecrypt(SymmetricAlgorithm algorithm, byte[] cipherText, int offset, int count)
        {
            if (cipherText == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cipherText");
            }
            if (count < 0 || count > cipherText.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeInRange, 0, cipherText.Length)));

            }
            if (offset < 0 || offset > cipherText.Length - count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeInRange, 0, cipherText.Length - count)));
            }

            int ivSize = algorithm.BlockSize / 8;
            byte[] iv = new byte[ivSize];
            Buffer.BlockCopy(cipherText, offset, iv, 0, iv.Length);
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Mode = CipherMode.CBC;
            try
            {
                using (ICryptoTransform decrTransform = algorithm.CreateDecryptor(algorithm.Key, iv))
                {
                    return decrTransform.TransformFinalBlock(cipherText, offset + iv.Length, count - iv.Length);
                }
            }
            catch (CryptographicException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.DecryptionFailed), ex));
            }
        }

        internal static void FillRandomBytes(byte[] buffer)
        {
            random.GetBytes(buffer);
        }

        static CryptoAlgorithmType GetAlgorithmType(string algorithm)
        {
            object algorithmObject = null;

            try
            {
                algorithmObject = CryptoAlgorithms.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmObject = null;
                // We ---- the exception and continue.
            }
            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                KeyedHashAlgorithm keyedHashAlgorithm = algorithmObject as KeyedHashAlgorithm;
                if (symmetricAlgorithm != null || keyedHashAlgorithm != null)
                    return CryptoAlgorithmType.Symmetric;

                // NOTE: A KeyedHashAlgorithm is symmetric in nature.

                AsymmetricAlgorithm asymmetricAlgorithm = algorithmObject as AsymmetricAlgorithm;
                SignatureDescription signatureDescription = algorithmObject as SignatureDescription;
                if (asymmetricAlgorithm != null || signatureDescription != null)
                    return CryptoAlgorithmType.Asymmetric;

                return CryptoAlgorithmType.Unknown;
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return CryptoAlgorithmType.Asymmetric;
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
                    return CryptoAlgorithmType.Symmetric;
                default:
                    return CryptoAlgorithmType.Unknown;
            }
        }

        internal static byte[] GenerateIVAndEncrypt(SymmetricAlgorithm algorithm, byte[] plainText, int offset, int count)
        {
            byte[] iv;
            byte[] cipherText;
            GenerateIVAndEncrypt(algorithm, new ArraySegment<byte>(plainText, offset, count), out iv, out cipherText);
            byte[] output = DiagnosticUtility.Utility.AllocateByteArray(checked(iv.Length + cipherText.Length));
            Buffer.BlockCopy(iv, 0, output, 0, iv.Length);
            Buffer.BlockCopy(cipherText, 0, output, iv.Length, cipherText.Length);
            return output;
        }

        internal static void GenerateIVAndEncrypt(SymmetricAlgorithm algorithm, ArraySegment<byte> plainText, out byte[] iv, out byte[] cipherText)
        {
            int ivSize = algorithm.BlockSize / 8;
            iv = new byte[ivSize];
            FillRandomBytes(iv);
            algorithm.Padding = PaddingMode.PKCS7;
            algorithm.Mode = CipherMode.CBC;
            using (ICryptoTransform encrTransform = algorithm.CreateEncryptor(algorithm.Key, iv))
            {
                cipherText = encrTransform.TransformFinalBlock(plainText.Array, plainText.Offset, plainText.Count);
            }
        }

        internal static bool IsEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            return GetAlgorithmType(algorithm) == CryptoAlgorithmType.Symmetric;
        }

        internal static bool IsSymmetricSupportedAlgorithm(string algorithm, int keySize)
        {
            bool found = false;
            object algorithmObject = null;

            try
            {
                algorithmObject = CryptoAlgorithms.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmObject = null;
                // We ---- the exception and continue.
            }
            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                KeyedHashAlgorithm keyedHashAlgorithm = algorithmObject as KeyedHashAlgorithm;
                if (symmetricAlgorithm != null || keyedHashAlgorithm != null)
                    found = true;
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return false;
                case SecurityAlgorithms.HmacSha1Signature:
                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.Psha1KeyDerivation:
                case SecurityAlgorithms.Psha1KeyDerivationDec2005:
                    return true;
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                    return keySize == 128;
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes192KeyWrap:
                    return keySize == 192;
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.Aes256KeyWrap:
                    return keySize == 256;
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.TripleDesKeyWrap:
                    return keySize == 128 || keySize == 192;
                default:
                    if (found)
                        return true;
                    return false;
            }
        }

        internal static void ValidateBufferBounds(Array buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (count < 0 || count > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeInRange, 0, buffer.Length)));
            }
            if (offset < 0 || offset > buffer.Length - count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeInRange, 0, buffer.Length - count)));
            }
        }

        internal static void ValidateSymmetricKeyLength(int keyLength, SecurityAlgorithmSuite algorithmSuite)
        {
            if (!algorithmSuite.IsSymmetricKeyLengthSupported(keyLength))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("algorithmSuite",
                   SR.GetString(SR.UnsupportedKeyLength, keyLength, algorithmSuite.ToString())));
            }
            if (keyLength % 8 != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("algorithmSuite",
                   SR.GetString(SR.KeyLengthMustBeMultipleOfEight, keyLength)));
            }
        }

    }
}
