//------------------------------------------------------------------------------
// <copyright file="SqlAeadAes256CbcHmac256Factory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// This is a factory class for AES_256_CBC.
    /// </summary>
    internal class SqlAes256CbcFactory : SqlAeadAes256CbcHmac256Factory
    {
        /// <summary>
        /// Factory classes caches the SqlAeadAes256CbcHmac256EncryptionKey objects to avoid computation of the derived keys
        /// </summary>
        private readonly ConcurrentDictionary<string, SqlAes256CbcAlgorithm> _encryptionAlgorithms =
            new ConcurrentDictionary<string, SqlAes256CbcAlgorithm>(concurrencyLevel: 4 * Environment.ProcessorCount /* default value in ConcurrentDictionary*/, capacity: 2);

        /// <summary>
        /// Creates an instance of SqlAes256CbcAlgorithm class with a given key
        /// </summary>
        /// <param name="encryptionKey">Root key</param>
        /// <param name="encryptionType">Encryption Type. Expected values are either Determinitic or Randomized.</param>
        /// <param name="encryptionAlgorithm">Encryption Algorithm.</param>
        /// <returns></returns>
        internal override SqlClientEncryptionAlgorithm Create(SqlClientSymmetricKey encryptionKey, SqlClientEncryptionType encryptionType, string encryptionAlgorithm)
        {
            // Callers should have validated the encryption algorithm and the encryption key
            Debug.Assert(encryptionKey != null);
            Debug.Assert(string.Equals(encryptionAlgorithm, SqlAes256CbcAlgorithm.AlgorithmName, StringComparison.OrdinalIgnoreCase) == true);

            // Validate encryption type
            if (!((encryptionType == SqlClientEncryptionType.Deterministic) || (encryptionType == SqlClientEncryptionType.Randomized)))
            {
                throw SQL.InvalidEncryptionType(SqlAes256CbcAlgorithm.AlgorithmName,
                                                encryptionType,
                                                SqlClientEncryptionType.Deterministic,
                                                SqlClientEncryptionType.Randomized);
            }

            // Get the cached encryption algorithm if one exists or create a new one, add it to cache and use it
            //
            // For now, we only have one version. In future, we may need to parse the algorithm names to derive the version byte.
            const byte algorithmVersion = 0x1;

            StringBuilder algorithmKeyBuilder = new StringBuilder(Convert.ToBase64String(encryptionKey.RootKey), SqlSecurityUtility.GetBase64LengthFromByteLength(encryptionKey.RootKey.Length) + 4/*separators, type and version*/);

#if DEBUG
            int capacity = algorithmKeyBuilder.Capacity;
#endif //DEBUG

            algorithmKeyBuilder.Append(":");
            algorithmKeyBuilder.Append((int)encryptionType);
            algorithmKeyBuilder.Append(":");
            algorithmKeyBuilder.Append(algorithmVersion);

            string algorithmKey = algorithmKeyBuilder.ToString();

#if DEBUG
            Debug.Assert(algorithmKey.Length <= capacity, "We needed to allocate a larger array");
#endif //DEBUG

            SqlAes256CbcAlgorithm aesAlgorithm;
            if (!_encryptionAlgorithms.TryGetValue(algorithmKey, out aesAlgorithm))
            {
                SqlAeadAes256CbcHmac256EncryptionKey encryptedKey = new SqlAeadAes256CbcHmac256EncryptionKey(encryptionKey.RootKey, SqlAes256CbcAlgorithm.AlgorithmName);
                aesAlgorithm = new SqlAes256CbcAlgorithm(encryptedKey, encryptionType, algorithmVersion);

                // In case multiple threads reach here at the same time, the first one adds the value
                // the second one will be a no-op, the allocated memory will be claimed by Garbage Collector.
                _encryptionAlgorithms.TryAdd(algorithmKey, aesAlgorithm);
            }

            return aesAlgorithm;
        }
    }
}
