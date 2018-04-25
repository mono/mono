//------------------------------------------------------------------------------
// <copyright file="SqlClientEncryptionAlgorithmFactoryList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">krishnib</owner>
// <owner current="true" primary="false">balnee</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Diagnostics;
    using System.Collections.Concurrent;
    using System.Text;

    /// <summary>
    /// <para> Implements a global directory of all the encryption algorithms registered with client.</para>
    /// </summary>
    sealed internal class SqlClientEncryptionAlgorithmFactoryList {
        private readonly ConcurrentDictionary<string, SqlClientEncryptionAlgorithmFactory> _encryptionAlgoFactoryList;
        private static readonly SqlClientEncryptionAlgorithmFactoryList _singletonInstance = new SqlClientEncryptionAlgorithmFactoryList();

        private SqlClientEncryptionAlgorithmFactoryList () {
            _encryptionAlgoFactoryList = new ConcurrentDictionary<string, SqlClientEncryptionAlgorithmFactory>(concurrencyLevel: 4 * Environment.ProcessorCount /* default value in ConcurrentDictionary*/, capacity: 2);

            // Add wellknown algorithms
            _encryptionAlgoFactoryList.TryAdd(SqlAeadAes256CbcHmac256Algorithm.AlgorithmName, new SqlAeadAes256CbcHmac256Factory());
            _encryptionAlgoFactoryList.TryAdd(SqlAes256CbcAlgorithm.AlgorithmName, new SqlAes256CbcFactory());
        }

        internal static SqlClientEncryptionAlgorithmFactoryList GetInstance () {
            return _singletonInstance;
        }

        /// <summary>
        /// Get the registered list of algorithms as a comma seperated list with algorithm names
        /// wrapped in single quotes.
        /// <summary>
        internal string GetRegisteredCipherAlgorithmNames () {
            StringBuilder builder = new StringBuilder();
            bool firstElem = true;
            foreach (string key in _encryptionAlgoFactoryList.Keys) {
                if (firstElem) {
                    builder.Append("'");
                    firstElem = false;
                }
                else {
                    builder.Append(", '");
                }
                builder.Append (key);
                builder.Append ("'");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the algorithm handle instance for a given algorithm and instantiates it using the provided key and the encryption type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="algorithmName"></param>
        /// <param name="encryptionAlgorithm"></param>
        internal void GetAlgorithm(SqlClientSymmetricKey key, byte type, string algorithmName, out SqlClientEncryptionAlgorithm encryptionAlgorithm) {
            encryptionAlgorithm = null;

            SqlClientEncryptionAlgorithmFactory factory = null;
            if (!_encryptionAlgoFactoryList.TryGetValue (algorithmName, out factory)) {
                throw SQL.UnknownColumnEncryptionAlgorithm(algorithmName, 
                        SqlClientEncryptionAlgorithmFactoryList.GetInstance().GetRegisteredCipherAlgorithmNames());
            }

            Debug.Assert (null != factory, "Null Algorithm Factory class detected");

            // If the factory exists, following method will Create an algorithm object. If this fails,
            // it will raise an exception.
            encryptionAlgorithm = factory.Create(key, (SqlClientEncryptionType)type, algorithmName);
        }
    }
}
