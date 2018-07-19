// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper around the CAPI implementation of the SHA-256 hashing algorithm
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class SHA256CryptoServiceProvider : SHA256 {
        private CapiHashAlgorithm m_hashAlgorithm;

        public SHA256CryptoServiceProvider() {
            Contract.Ensures(m_hashAlgorithm != null);

            m_hashAlgorithm = new CapiHashAlgorithm(CapiNative.ProviderNames.MicrosoftEnhancedRsaAes,
                                                    CapiNative.ProviderType.RsaAes,
                                                    CapiNative.AlgorithmId.Sha256);
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    m_hashAlgorithm.Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Reset the hash algorithm to begin hashing a new set of data
        /// </summary>
        public override void Initialize() {
            Contract.Assert(m_hashAlgorithm != null);
            m_hashAlgorithm.Initialize();
        }

        /// <summary>
        ///     Hash a block of data
        /// </summary>
        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            Contract.Assert(m_hashAlgorithm != null);
            m_hashAlgorithm.HashCore(array, ibStart, cbSize);
        }

        /// <summary>
        ///     Complete the hash, returning its value
        /// </summary>
        protected override byte[] HashFinal() {
            Contract.Assert(m_hashAlgorithm != null);
            return m_hashAlgorithm.HashFinal();
        }
    }
}
