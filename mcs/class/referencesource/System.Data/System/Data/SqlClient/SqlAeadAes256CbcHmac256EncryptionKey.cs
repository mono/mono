//------------------------------------------------------------------------------
// <copyright file="SqlAeadAes256CbcHmac256EncryptionKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient 
{
    using System;
    using System.Data.SqlClient;
    using System.Text;

    /// <summary>
    /// Encryption key class containing 4 keys. This class is used by SqlAeadAes256CbcHmac256Algorithm and SqlAes256CbcAlgorithm
    /// 1) root key - Main key that is used to derive the keys used in the encryption algorithm
    /// 2) encryption key - A derived key that is used to encrypt the plain text and generate cipher text
    /// 3) mac_key - A derived key that is used to compute HMAC of the cipher text
    /// 4) iv_key - A derived key that is used to generate a synthetic IV from plain text data.
    /// </summary>
    internal class SqlAeadAes256CbcHmac256EncryptionKey : SqlClientSymmetricKey 
    {
        /// <summary>
        /// Key size in bits
        /// </summary>
        internal const int KeySize = 256;

        /// <summary>
        /// Encryption Key Salt format. This is used to derive the encryption key from the root key.
        /// </summary>
        private const string _encryptionKeySaltFormat = @"Microsoft SQL Server cell encryption key with encryption algorithm:{0} and key length:{1}";

        /// <summary>
        /// MAC Key Salt format. This is used to derive the MAC key from the root key.
        /// </summary>
        private const string _macKeySaltFormat = @"Microsoft SQL Server cell MAC key with encryption algorithm:{0} and key length:{1}";

        /// <summary>
        /// IV Key Salt format. This is used to derive the IV key from the root key. This is only used for Deterministic encryption.
        /// </summary>
        private const string _ivKeySaltFormat = @"Microsoft SQL Server cell IV key with encryption algorithm:{0} and key length:{1}";

        /// <summary>
        /// Encryption Key
        /// </summary>
        private readonly SqlClientSymmetricKey _encryptionKey;

        /// <summary>
        /// MAC key
        /// </summary>
        private readonly SqlClientSymmetricKey _macKey;

        /// <summary>
        /// IV Key
        /// </summary>
        private readonly SqlClientSymmetricKey _ivKey;

        /// <summary>
        /// The name of the algorithm this key will be used with.
        /// </summary>
        private readonly string _algorithmName;

        /// <summary>
        /// Derives all the required keys from the given root key
        /// </summary>
        /// <param name="rootKey">Root key used to derive all the required derived keys</param>
        internal SqlAeadAes256CbcHmac256EncryptionKey(byte[] rootKey, string algorithmName): base(rootKey) 
        {
            _algorithmName = algorithmName;

            int keySizeInBytes = KeySize / 8;

            // Key validation
            if (rootKey.Length != keySizeInBytes) 
            {
                throw SQL.InvalidKeySize(_algorithmName,
                                         rootKey.Length,
                                         keySizeInBytes);
            }

            // Derive keys from the root key
            //
            // Derive encryption key
            string encryptionKeySalt = string.Format(_encryptionKeySaltFormat,
                                                    _algorithmName,
                                                    KeySize);
            byte[] buff1 = new byte[keySizeInBytes];
            SqlSecurityUtility.GetHMACWithSHA256(Encoding.Unicode.GetBytes(encryptionKeySalt), RootKey, buff1);
            _encryptionKey = new SqlClientSymmetricKey(buff1);

            // Derive mac key
            string macKeySalt = string.Format(_macKeySaltFormat, _algorithmName, KeySize);
            byte[] buff2 = new byte[keySizeInBytes];
            SqlSecurityUtility.GetHMACWithSHA256(Encoding.Unicode.GetBytes(macKeySalt),RootKey,buff2);
            _macKey = new SqlClientSymmetricKey(buff2);

            // Derive iv key
            string ivKeySalt = string.Format(_ivKeySaltFormat, _algorithmName, KeySize);
            byte[] buff3 = new byte[keySizeInBytes];
            SqlSecurityUtility.GetHMACWithSHA256(Encoding.Unicode.GetBytes(ivKeySalt),RootKey,buff3);
            _ivKey = new SqlClientSymmetricKey(buff3);
        }

        /// <summary>
        /// Encryption key should be used for encryption and decryption
        /// </summary>
        internal byte[] EncryptionKey 
        {
            get { return _encryptionKey.RootKey; }
        }

        /// <summary>
        /// MAC key should be used to compute and validate HMAC
        /// </summary>
        internal byte[] MACKey 
        {
            get { return _macKey.RootKey; }
        }

        /// <summary>
        /// IV key should be used to compute synthetic IV from a given plain text
        /// </summary>
        internal byte[] IVKey 
        {
            get { return _ivKey.RootKey; }
        }
    }
}
