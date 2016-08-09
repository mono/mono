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
    using System.Data.SqlClient;
    using System.Security.Cryptography;

    /// <summary>
    /// Base class containing raw key bytes for symmetric key algorithms. Some encryption algorithms can use the key directly while others derive sub keys from this.
    /// If an algorithm needs to derive more keys, have a derived class from this and use it in the corresponding encryption algorithm.
    /// </summary>
    internal class SqlClientSymmetricKey {
        /// <summary>
        /// The underlying key material
        /// </summary>
        protected readonly byte[] _rootKey;

        /// <summary>
        /// Constructor that initializes the root key.
        /// </summary>
        /// <param name="rootKey">root key</param>
        internal SqlClientSymmetricKey(byte[] rootKey) {
            // Key validation
            if (rootKey == null || rootKey.Length == 0) {
                throw SQL.NullColumnEncryptionKeySysErr();
            }

            _rootKey = rootKey;
        }

        /// <summary>
        /// Destructor that cleans up the key material.
        /// This is a best effort approach since there are no guarantees around GC.
        /// </summary>
        ~SqlClientSymmetricKey() {
            if (_rootKey != null) {
                for (int i = 0; i < _rootKey.Length; i++) {
                    _rootKey[i] = 0;
                }
            }
        }

        /// <summary>
        /// Returns a copy of the plain text key
        /// This is needed for actual encryption/decryption.
        /// </summary>
        internal virtual byte[] RootKey {
            get {
                return _rootKey; 
            }
        }

        /// <summary>
        /// Computes SHA256 value of the plain text key bytes
        /// </summary>
        /// <returns>A string containing SHA256 hash of the root key</returns>
        internal virtual string GetKeyHash() {
            return SqlSecurityUtility.GetSHA256Hash(RootKey);
        }

        /// <summary>
        /// Gets the length of the root key
        /// </summary>
        /// <returns>
        /// Returns the length of the root key
        /// </returns>
        internal virtual int Length() {
            return _rootKey.Length;
        }
    }
}
