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
    /// Abstract base class for all TCE encryption algorithms. It exposes two functions
    ///		1. Encrypt - This function is used by SqlClient under the covers to transparently encrypt TCE enabled column data.
    ///		2. Decrypt - This function is used by SqlClient under the covers to transparently decrypt TCE enabled column data.
    /// </summary>
    internal abstract class SqlClientEncryptionAlgorithm
    {
        /// <summary>
        /// Encrypts the plainText with a column encryption key
        /// </summary>
        /// <param name="plainText">Plain text value to be encrypted</param>
        /// <returns></returns>
        internal abstract byte[] EncryptData(byte[] plainText);

        /// <summary>
        /// Decrypts the cipherText with a column encryption key
        /// </summary>
        /// <param name="cipherText">Ciphertext value to be decrypted</param>
        /// <returns></returns>
        internal abstract byte[] DecryptData(byte[] cipherText);
    }
}
