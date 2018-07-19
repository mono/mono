//------------------------------------------------------------------------------
// <copyright file="SqlException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    /// <summary>
    /// Abstract base class for all TCE encryption algorithm factory classes. Factory classes create instances of an encryption algorithm
    /// with a given key. At runtime when we determine a particular column is marked for TCE, based on the encryption algorithm we invoke 
    /// the corresponding factory class and retrieve an object to an encryption algorithm.
    /// </summary>
    internal abstract class SqlClientEncryptionAlgorithmFactory
    {
        /// <summary>
        /// Creates an encrytion algorithm with a given key.
        /// </summary>
        /// <param name="rootKey">encryption key that should be passed to the encryption algorithm to be created</param>
        /// <param name="encryptionType">Encryption Type, some algorithms will need this</param>
        /// <param name="encryptionAlgorithm">Encryption algorithm name. Needed for extracting version bits</param>
        /// <returns>Return a newly created SqlClientEncryptionAlgorithm instance</returns>
        internal abstract SqlClientEncryptionAlgorithm Create(SqlClientSymmetricKey encryptionKey, SqlClientEncryptionType encryptionType, string encryptionAlgorithm);
    }
}
