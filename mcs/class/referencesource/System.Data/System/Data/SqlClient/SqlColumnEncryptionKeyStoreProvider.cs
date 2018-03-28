//------------------------------------------------------------------------------
// <copyright file="SqlException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    using System;

    /// <summary>
    /// Abstract base class for all column encryption Key Store providers. It exposes two functions
    ///		1. DecryptColumnEncryptionKey - This is the function used by SqlClient under the covers to decrypt encrypted column encryption key blob.
    ///		2. EncryptColumnEncryptionKey - This will be used by client tools that generate DDL for customers
    /// </summary>
    public abstract class SqlColumnEncryptionKeyStoreProvider
    {
        /// <summary>
        /// This function must be implemented by the corresponding Key Store providers. This function should use an asymmetric key identified by the key path
        /// and decrypt an encrypted column encryption key with a given encryption algorithm.
        /// </summary>
        /// <param name="masterKeyPath">Complete path of an asymmetric key. Path format is specific to a key store provider.</param>
        /// <param name="encryptionAlgorithm">Asymmetric Key Encryption Algorithm</param>
        /// <param name="encryptedColumnEncryptionKey">Encrypted Column Encryption Key</param>
        /// <returns>Plain text column encryption key</returns>
        public abstract byte[] DecryptColumnEncryptionKey(string masterKeyPath, string encryptionAlgorithm, byte[] encryptedColumnEncryptionKey);

        /// <summary>
        /// This function must be implemented by the corresponding Key Store providers. This function should use an asymmetric key identified by a key path
        /// and encrypt a plain text column encryption key with a given asymmetric key encryption algorithm.
        /// </summary>
        /// <param name="keyPath">Complete path of an asymmetric key. Path format is specific to a key store provider.</param>
        /// <param name="encryptionAlgorithm">Asymmetric Key Encryption Algorithm</param>
        /// <param name="columnEncryptionKey">Plain text column encryption key to be encrypted</param>
        /// <returns>Encrypted column encryption key</returns>
        public abstract byte[] EncryptColumnEncryptionKey(string masterKeyPath, string encryptionAlgorithm, byte[] columnEncryptionKey);
    }
}