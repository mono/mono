// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper for public key material passed between parties during Diffie-Hellman key material generation
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class ECDiffieHellmanPublicKey : IDisposable {
        private byte[] m_keyBlob;

        protected ECDiffieHellmanPublicKey(byte[] keyBlob) {
            Contract.Ensures(m_keyBlob != null);

            if (keyBlob == null) {
                throw new ArgumentNullException("keyBlob");
            }

            m_keyBlob = keyBlob.Clone() as byte[];
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            return;
        }

        public virtual byte[] ToByteArray() {
            Contract.Assert(m_keyBlob != null);
            return m_keyBlob.Clone() as byte[];
        }

        public abstract string ToXmlString();
    }
}
