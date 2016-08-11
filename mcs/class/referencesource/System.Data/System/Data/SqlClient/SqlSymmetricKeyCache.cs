//------------------------------------------------------------------------------
// <copyright file="SqlSymmetricKeyCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">krishnib</owner>
// <owner current="true" primary="false">balnee</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Text;

    /// <summary>
    /// <para> Implements a cache of Symmetric Keys (once they are decrypted).Useful for rapidly decrypting multiple data values.</para>
    /// </summary>
    sealed internal class SqlSymmetricKeyCache {
        private readonly MemoryCache _cache;
        private static readonly SqlSymmetricKeyCache _singletonInstance = new SqlSymmetricKeyCache();


        private SqlSymmetricKeyCache () {
            _cache = new MemoryCache("ColumnEncryptionKeyCache");
        }

        internal static SqlSymmetricKeyCache GetInstance () {
            return _singletonInstance;
        }

        /// <summary>
        /// <para> Retrieves Symmetric Key (in plaintext) given the encryption material.</para>
        /// </summary>
        internal bool GetKey (SqlEncryptionKeyInfo keyInfo, string serverName, out SqlClientSymmetricKey encryptionKey) {
            Debug.Assert(serverName != null, @"serverName should not be null.");

            StringBuilder cacheLookupKeyBuilder = new StringBuilder(serverName, capacity: serverName.Length + SqlSecurityUtility.GetBase64LengthFromByteLength(keyInfo.encryptedKey.Length) + keyInfo.keyStoreName.Length + 2/*separators*/);

#if DEBUG
            int capacity = cacheLookupKeyBuilder.Capacity;
#endif //DEBUG

            cacheLookupKeyBuilder.Append(":");
            cacheLookupKeyBuilder.Append(Convert.ToBase64String(keyInfo.encryptedKey));
            cacheLookupKeyBuilder.Append(":");
            cacheLookupKeyBuilder.Append(keyInfo.keyStoreName);

            string cacheLookupKey = cacheLookupKeyBuilder.ToString();

#if DEBUG
            Debug.Assert(cacheLookupKey.Length <= capacity, "We needed to allocate a larger array");
#endif //DEBUG

            // Lookup the key in cache
            encryptionKey = _cache.Get(cacheLookupKey) as SqlClientSymmetricKey;
            
            if (encryptionKey == null) {
                Debug.Assert(SqlConnection.ColumnEncryptionTrustedMasterKeyPaths != null, @"SqlConnection.ColumnEncryptionTrustedMasterKeyPaths should not be null");

                // Check against the trusted key paths
                //
                // Get the List corresponding to the connected server
                IList<string> trustedKeyPaths;
                if (SqlConnection.ColumnEncryptionTrustedMasterKeyPaths.TryGetValue(serverName, out trustedKeyPaths)) {
                    // If the list is null or is empty or if the keyPath doesn't exist in the trusted key paths, then throw an exception.
                    if ((trustedKeyPaths == null) || (trustedKeyPaths.Count() == 0) ||
                        // (trustedKeyPaths.Where(s => s.Equals(keyInfo.keyPath, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)) {
                        (trustedKeyPaths.Any(s => s.Equals(keyInfo.keyPath, StringComparison.InvariantCultureIgnoreCase)) == false)) {
                        // throw an exception since the key path is not in the trusted key paths list for this server
                        throw SQL.UntrustedKeyPath(keyInfo.keyPath, serverName);
                    }
                }

                // Key Not found, attempt to look up the provider and decrypt CEK
                SqlColumnEncryptionKeyStoreProvider provider;
                if (!SqlConnection.TryGetColumnEncryptionKeyStoreProvider(keyInfo.keyStoreName, out provider)) {
                    throw SQL.UnrecognizedKeyStoreProviderName(keyInfo.keyStoreName, 
                            SqlConnection.GetColumnEncryptionSystemKeyStoreProviders(),
                            SqlConnection.GetColumnEncryptionCustomKeyStoreProviders());
                }

                // Decrypt the CEK
                // We will simply bubble up the exception from the DecryptColumnEncryptionKey function.
                byte[] plaintextKey;
                try {
                    plaintextKey = provider.DecryptColumnEncryptionKey(keyInfo.keyPath, keyInfo.algorithmName, keyInfo.encryptedKey);
                }
                catch (Exception e) {
                    // Generate a new exception and throw.
                    string keyHex = SqlSecurityUtility.GetBytesAsString(keyInfo.encryptedKey, fLast:true, countOfBytes:10);
                    throw SQL.KeyDecryptionFailed (keyInfo.keyStoreName, keyHex, e);
                }

                encryptionKey = new SqlClientSymmetricKey (plaintextKey);

                // If the cache TTL is zero, don't even bother inserting to the cache.
                if (SqlConnection.ColumnEncryptionKeyCacheTtl != TimeSpan.Zero) {
                    // In case multiple threads reach here at the same time, the first one wins.
                    // The allocated memory will be reclaimed by Garbage Collector.
                    DateTimeOffset expirationTime = DateTimeOffset.UtcNow.Add(SqlConnection.ColumnEncryptionKeyCacheTtl);
                    _cache.Add(cacheLookupKey, encryptionKey, expirationTime);
                }
            }

            return true;
        }
    }
}
