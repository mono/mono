//------------------------------------------------------------------------------
// <copyright file="SqlAes256CbcAlgorithm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    /// <summary>
    /// This class implements AES_256_CBC algorithm.
    /// </summary>
    internal class SqlAes256CbcAlgorithm : SqlAeadAes256CbcHmac256Algorithm
    {
        /// <summary>
        /// Algorithm Name
        /// </summary>
        internal new const string AlgorithmName = @"AES_256_CBC";

        /// <summary>
        /// Initializes a new instance of SqlAes256CbcAlgorithm algorithm with a given key and encryption type
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
        internal SqlAes256CbcAlgorithm(SqlAeadAes256CbcHmac256EncryptionKey encryptionKey, SqlClientEncryptionType encryptionType, byte algorithmVersion)
            :base(encryptionKey, encryptionType, algorithmVersion)
        { }

        /// <summary>
        /// Encryption Algorithm
        /// Simply call the base class, indicating we don't need an authentication tag.
        /// </summary>
        /// <param name="plainText">Plaintext data to be encrypted</param>
        /// <returns>Returns the ciphertext corresponding to the plaintext.</returns>
        internal override byte[] EncryptData(byte[] plainText) {
            return EncryptData(plainText, hasAuthenticationTag: false);
        }

        /// <summary>
        /// Decryption Algorithm
        /// Simply call the base class, indicating we don't have an authentication tag.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        internal override byte[] DecryptData(byte[] cipherText) {
            return base.DecryptData(cipherText, hasAuthenticationTag: false);
        }
    }
}
