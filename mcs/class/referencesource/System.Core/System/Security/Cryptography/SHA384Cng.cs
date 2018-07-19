// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper around the BCrypt implementation of the SHA-384 hashing algorithm
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class SHA384Cng : SHA384 {
        private BCryptHashAlgorithm m_hashAlgorithm;

        public SHA384Cng() {
            Contract.Ensures(m_hashAlgorithm != null);

            m_hashAlgorithm = new BCryptHashAlgorithm(CngAlgorithm.Sha384,
                                                      BCryptNative.ProviderName.MicrosoftPrimitiveProvider);
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

        public override void Initialize() {
            Contract.Assert(m_hashAlgorithm != null);
            m_hashAlgorithm.Initialize();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            Contract.Assert(m_hashAlgorithm != null);
            m_hashAlgorithm.HashCore(array, ibStart, cbSize);
        }

        protected override byte[] HashFinal() {
            Contract.Assert(m_hashAlgorithm != null);
            return m_hashAlgorithm.HashFinal();
        }
    }
}
