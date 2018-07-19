//------------------------------------------------------------------------------
// <copyright file="SqlAeadAes256CbcHmac256Algorithm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    /// <summary>
    /// This class implements authenticated encryption algorithm with associated data as described in 
    /// http://tools.ietf.org/html/draft-mcgrew-aead-aes-cbc-hmac-sha2-05. More specifically this implements
    /// AEAD_AES_256_CBC_HMAC_SHA256 algorithm.
    /// </summary>
    internal class SqlAeadAes256CbcHmac256Algorithm : SqlClientEncryptionAlgorithm
    {
        /// <summary>
        /// Algorithm Name
        /// </summary>
        internal const string AlgorithmName = @"AEAD_AES_256_CBC_HMAC_SHA256";

        /// <summary>
        /// Key size in bytes
        /// </summary>
        private const int _KeySizeInBytes = SqlAeadAes256CbcHmac256EncryptionKey.KeySize / 8;

        /// <summary>
        /// Block size in bytes. AES uses 16 byte blocks.
        /// </summary>
        private const int _BlockSizeInBytes = 16;

        /// <summary>
        /// Minimum Length of cipherText without authentication tag. This value is 1 (version byte) + 16 (IV) + 16 (minimum of 1 block of cipher Text)
        /// </summary>
        private const int _MinimumCipherTextLengthInBytesNoAuthenticationTag = sizeof(byte) + _BlockSizeInBytes + _BlockSizeInBytes;

        /// <summary>
        /// Minimum Length of cipherText. This value is 1 (version byte) + 32 (authentication tag) + 16 (IV) + 16 (minimum of 1 block of cipher Text)
        /// </summary>
        private const int _MinimumCipherTextLengthInBytesWithAuthenticationTag = _MinimumCipherTextLengthInBytesNoAuthenticationTag + _KeySizeInBytes;

        /// <summary>
        /// Cipher Mode. For this algorithm, we only use CBC mode.
        /// </summary>
        private const CipherMode _cipherMode = CipherMode.CBC;

        /// <summary>
        /// Padding mode. This algorithm uses PKCS7.
        /// </summary>
        private const PaddingMode _paddingMode = PaddingMode.PKCS7;

        /// <summary>
        /// Variable indicating whether this algorithm should work in Deterministic mode or Randomized mode.
        /// For deterministic encryption, we derive an IV from the plaintext data.
        /// For randomized encryption, we generate a cryptographically random IV.
        /// </summary>
        private readonly bool _isDeterministic;

        /// <summary>
        /// Algorithm Version.
        /// </summary>
        private readonly byte _algorithmVersion;

        /// <summary>
        /// Column Encryption Key. This has a root key and three derived keys.
        /// </summary>
        private readonly SqlAeadAes256CbcHmac256EncryptionKey _columnEncryptionKey;

        /// <summary>
        /// The pool of crypto providers to use for encrypt/decrypt operations.
        /// </summary>
        private readonly ConcurrentQueue<AesCryptoServiceProvider> _cryptoProviderPool;

        /// <summary>
        /// Byte array with algorithm version used for authentication tag computation.
        /// </summary>
        private static readonly byte[] _version = new byte[] {0x01};

        /// <summary>
        /// Byte array with algorithm version size used for authentication tag computation.
        /// </summary>
        private static readonly byte[] _versionSize = new byte[] {sizeof(byte)};

        /// <summary>
        /// Initializes a new instance of SqlAeadAes256CbcHmac256Algorithm algorithm with a given key and encryption type
        /// </summary>
        /// <param name="encryptionKey">
        /// Root encryption key from which three other keys will be derived
        /// </param>
        /// <param name="encryptionType">Encryption Type, accepted values are Deterministic and Randomized. 
        /// For Deterministic encryption, a synthetic IV will be genenrated during encryption
        /// For Randomized encryption, a random IV will be generated during encryption.
        /// </param>
        /// <param name="algorithmVersion">
        /// Algorithm version
        /// </param>
        internal SqlAeadAes256CbcHmac256Algorithm(SqlAeadAes256CbcHmac256EncryptionKey encryptionKey, SqlClientEncryptionType encryptionType, byte algorithmVersion) {
            _columnEncryptionKey = encryptionKey;
            _algorithmVersion = algorithmVersion;
            _version[0] = algorithmVersion;

            Debug.Assert (null != encryptionKey, "Null encryption key detected in AeadAes256CbcHmac256 algorithm");
            Debug.Assert (0x01 == algorithmVersion, "Unknown algorithm version passed to AeadAes256CbcHmac256");

            // Validate encryption type for this algorithm
            // This algorithm can only provide randomized or deterministic encryption types.
            if (encryptionType == SqlClientEncryptionType.Deterministic) {
                _isDeterministic = true;
            }
            else {
                Debug.Assert (SqlClientEncryptionType.Randomized == encryptionType, "Invalid Encryption Type detected in SqlAeadAes256CbcHmac256Algorithm, this should've been caught in factory class");
            }

            _cryptoProviderPool = new ConcurrentQueue<AesCryptoServiceProvider>();
        }

        /// <summary>
        /// Encryption Algorithm
        /// cell_iv = HMAC_SHA-2-256(iv_key, cell_data) truncated to 128 bits
        /// cell_ciphertext = AES-CBC-256(enc_key, cell_iv, cell_data) with PKCS7 padding.
        /// cell_tag = HMAC_SHA-2-256(mac_key, versionbyte + cell_iv + cell_ciphertext + versionbyte_length)
        /// cell_blob = versionbyte + cell_tag + cell_iv + cell_ciphertext
        /// </summary>
        /// <param name="plainText">Plaintext data to be encrypted</param>
        /// <returns>Returns the ciphertext corresponding to the plaintext.</returns>
        internal override byte[] EncryptData(byte[] plainText) {
            return EncryptData(plainText, hasAuthenticationTag: true);
        }

        /// <summary>
        /// Encryption Algorithm
        /// cell_iv = HMAC_SHA-2-256(iv_key, cell_data) truncated to 128 bits
        /// cell_ciphertext = AES-CBC-256(enc_key, cell_iv, cell_data) with PKCS7 padding.
        /// (optional) cell_tag = HMAC_SHA-2-256(mac_key, versionbyte + cell_iv + cell_ciphertext + versionbyte_length)
        /// cell_blob = versionbyte + [cell_tag] + cell_iv + cell_ciphertext
        /// </summary>
        /// <param name="plainText">Plaintext data to be encrypted</param>
        /// <param name="hasAuthenticationTag">Does the algorithm require authentication tag.</param>
        /// <returns>Returns the ciphertext corresponding to the plaintext.</returns>
        protected byte[] EncryptData(byte[] plainText, bool hasAuthenticationTag) {
            // Empty values get encrypted and decrypted properly for both Deterministic and Randomized encryptions.
            Debug.Assert(plainText != null);

            byte[] iv = new byte[_BlockSizeInBytes];

            // Prepare IV
            // Should be 1 single block (16 bytes)
            if (_isDeterministic) {
                SqlSecurityUtility.GetHMACWithSHA256(plainText, _columnEncryptionKey.IVKey, iv);
            }
            else {
                SqlSecurityUtility.GenerateRandomBytes(iv);
            }

            int numBlocks = plainText.Length / _BlockSizeInBytes + 1;

            // Final blob we return = version + HMAC + iv + cipherText
            const int hmacStartIndex = 1;
            int authenticationTagLen = hasAuthenticationTag ? _KeySizeInBytes : 0;
            int ivStartIndex = hmacStartIndex + authenticationTagLen;
            int cipherStartIndex = ivStartIndex + _BlockSizeInBytes; // this is where hmac starts.

            // Output buffer size = size of VersionByte + Authentication Tag + IV + cipher Text blocks.
            int outputBufSize = sizeof(byte) + authenticationTagLen + iv.Length + (numBlocks*_BlockSizeInBytes);
            byte[] outBuffer = new byte[outputBufSize];

            // Store the version and IV rightaway
            outBuffer[0] = _algorithmVersion;
            Buffer.BlockCopy(iv, 0, outBuffer, ivStartIndex, iv.Length);

            AesCryptoServiceProvider aesAlg;

            // Try to get a provider from the pool.
            // If no provider is available, create a new one.
            if (!_cryptoProviderPool.TryDequeue(out aesAlg)) {
                aesAlg = new AesCryptoServiceProvider();

                try {
                    // Set various algorithm properties
                    aesAlg.Key = _columnEncryptionKey.EncryptionKey;
                    aesAlg.Mode = _cipherMode;
                    aesAlg.Padding = _paddingMode;
                }
                catch (Exception) {
                    if (aesAlg != null) {
                        aesAlg.Dispose();
                    }

                    throw;
                }
            }

            try {
                // Always set the IV since it changes from cell to cell.
                aesAlg.IV = iv;

                // Compute CipherText and authentication tag in a single pass
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor()) {
                    Debug.Assert(encryptor.CanTransformMultipleBlocks, "AES Encryptor can transform multiple blocks");
                    int count = 0;
                    int cipherIndex = cipherStartIndex; // this is where cipherText starts
                    if (numBlocks > 1) {
                        count = (numBlocks - 1) * _BlockSizeInBytes;
                        cipherIndex += encryptor.TransformBlock(plainText, 0, count, outBuffer, cipherIndex);
                    }

                    byte[] buffTmp = encryptor.TransformFinalBlock(plainText, count, plainText.Length - count); // done encrypting
                    Buffer.BlockCopy(buffTmp, 0, outBuffer, cipherIndex, buffTmp.Length);
                    cipherIndex += buffTmp.Length;
                }

                if (hasAuthenticationTag) {
                    using (HMACSHA256 hmac = new HMACSHA256(_columnEncryptionKey.MACKey)) {
                        Debug.Assert(hmac.CanTransformMultipleBlocks, "HMAC can't transform multiple blocks");
                        hmac.TransformBlock(_version, 0, _version.Length, _version, 0);
                        hmac.TransformBlock(iv, 0, iv.Length, iv, 0);

                        // Compute HMAC on final block
                        hmac.TransformBlock(outBuffer, cipherStartIndex, numBlocks * _BlockSizeInBytes, outBuffer, cipherStartIndex);
                        hmac.TransformFinalBlock(_versionSize, 0, _versionSize.Length);
                        byte[] hash = hmac.Hash;
                        Debug.Assert(hash.Length >= authenticationTagLen, "Unexpected hash size");
                        Buffer.BlockCopy(hash, 0, outBuffer, hmacStartIndex, authenticationTagLen);
                    }
                }
            }
            finally {
                // Return the provider to the pool.
                _cryptoProviderPool.Enqueue(aesAlg);
            }

            return outBuffer;
        }

        /// <summary>
        /// Decryption steps
        /// 1. Validate version byte
        /// 2. Validate Authentication tag
        /// 3. Decrypt the message
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        internal override byte[] DecryptData(byte[] cipherText) {
            return DecryptData(cipherText, hasAuthenticationTag: true);
        }

        /// <summary>
        /// Decryption steps
        /// 1. Validate version byte
        /// 2. (optional) Validate Authentication tag
        /// 3. Decrypt the message
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="hasAuthenticationTag"></param>
        /// <returns></returns>
        protected byte[] DecryptData(byte[] cipherText, bool hasAuthenticationTag) {
            Debug.Assert(cipherText != null);

            byte[] iv = new byte[_BlockSizeInBytes];

            int minimumCipherTextLength = hasAuthenticationTag ? _MinimumCipherTextLengthInBytesWithAuthenticationTag : _MinimumCipherTextLengthInBytesNoAuthenticationTag;
            if (cipherText.Length < minimumCipherTextLength) {
                throw SQL.InvalidCipherTextSize(cipherText.Length, minimumCipherTextLength);
            }

            // Validate the version byte
            int startIndex = 0;
            if (cipherText[startIndex] != _algorithmVersion) {
                // Cipher text was computed with a different algorithm version than this.
                throw SQL.InvalidAlgorithmVersion(cipherText[startIndex], _algorithmVersion);
            }

            startIndex += 1;
            int authenticationTagOffset = 0;

            // Read authentication tag
            if (hasAuthenticationTag) {
                authenticationTagOffset = startIndex;
                startIndex += _KeySizeInBytes; // authentication tag size is _KeySizeInBytes
            }

            // Read cell IV
            Buffer.BlockCopy(cipherText, startIndex, iv, 0, iv.Length);
            startIndex += iv.Length;

            // Read encrypted text
            int cipherTextOffset = startIndex;
            int cipherTextCount = cipherText.Length - startIndex;

            if (hasAuthenticationTag) {
                // Compute authentication tag
                byte[] authenticationTag = PrepareAuthenticationTag(iv, cipherText, cipherTextOffset, cipherTextCount);
                if (!SqlSecurityUtility.CompareBytes(authenticationTag, cipherText, authenticationTagOffset, authenticationTag.Length)) {
                    // Potentially tampered data, throw an exception
                    throw SQL.InvalidAuthenticationTag();
                }
            }

            // Decrypt the text and return
            return DecryptData(iv, cipherText, cipherTextOffset, cipherTextCount);
        }

        /// <summary>
        /// Decrypts plain text data using AES in CBC mode
        /// </summary>
        /// <param name="plainText"> cipher text data to be decrypted</param>
        /// <param name="iv">IV to be used for decryption</param>
        /// <returns>Returns decrypted plain text data</returns>
        private byte[] DecryptData(byte[] iv, byte[] cipherText, int offset, int count) {
            Debug.Assert((iv != null) && (cipherText != null));
            Debug.Assert (offset > -1 && count > -1);
            Debug.Assert ((count+offset) <= cipherText.Length);

            byte[] plainText;
            AesCryptoServiceProvider aesAlg;

            // Try to get a provider from the pool.
            // If no provider is available, create a new one.
            if (!_cryptoProviderPool.TryDequeue(out aesAlg)) {
                aesAlg = new AesCryptoServiceProvider();

                try {
                    // Set various algorithm properties
                    aesAlg.Key = _columnEncryptionKey.EncryptionKey;
                    aesAlg.Mode = _cipherMode;
                    aesAlg.Padding = _paddingMode;
                }
                catch (Exception) {
                    if (aesAlg != null) {
                        aesAlg.Dispose();
                    }

                    throw;
                }
            }

            try {
                // Always set the IV since it changes from cell to cell.
                aesAlg.IV = iv;

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream()) {
                    // Create an encryptor to perform the stream transform.
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor()) {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write)) {
                            // Decrypt the secret message and get the plain text data
                            csDecrypt.Write(cipherText, offset, count);
                            csDecrypt.FlushFinalBlock();
                            plainText = msDecrypt.ToArray();
                        }
                    }
                }
            }
            finally {
                // Return the provider to the pool.
                _cryptoProviderPool.Enqueue(aesAlg);
            }

            return plainText;
        }

        /// <summary>
        /// Prepares an authentication tag.
        /// Authentication Tag = HMAC_SHA-2-256(mac_key, versionbyte + cell_iv + cell_ciphertext + versionbyte_length)
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        private byte[] PrepareAuthenticationTag(byte[] iv, byte[] cipherText, int offset, int length) {
            Debug.Assert(cipherText != null);

            byte[] computedHash;
            byte[] authenticationTag = new byte[_KeySizeInBytes];

            // Raw Tag Length:
            //              1 for the version byte
            //              1 block for IV (16 bytes)
            //              cipherText.Length
            //              1 byte for version byte length

            using (HMACSHA256 hmac = new HMACSHA256(_columnEncryptionKey.MACKey)) {
                int retVal = 0;
                retVal = hmac.TransformBlock(_version, 0, _version.Length, _version, 0);
                Debug.Assert(retVal == _version.Length);
                retVal = hmac.TransformBlock(iv, 0, iv.Length, iv, 0);
                Debug.Assert(retVal == iv.Length);
                retVal = hmac.TransformBlock(cipherText, offset, length, cipherText, offset);
                Debug.Assert(retVal == length);
                hmac.TransformFinalBlock(_versionSize, 0, _versionSize.Length);
                computedHash = hmac.Hash;
            }

            Debug.Assert (computedHash.Length >= authenticationTag.Length);
            Buffer.BlockCopy (computedHash, 0, authenticationTag, 0, authenticationTag.Length);
            return authenticationTag;
        }
    }
}
