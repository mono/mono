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

        protected ECDiffieHellmanPublicKey() {
            m_keyBlob = new byte[0];
        }

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

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual string ToXmlString() {
            throw new NotImplementedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        /// <summary>
        /// When overridden in a derived class, exports the named or explicit ECParameters for an ECCurve.
        /// If the curve has a name, the Curve property will contain named curve parameters, otherwise it
        /// will contain explicit parameters.
        /// </summary>
        /// <returns>The ECParameters representing the point on the curve for this key.</returns>
        public virtual ECParameters ExportParameters() {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        /// <summary>
        /// When overridden in a derived class, exports the explicit ECParameters for an ECCurve.
        /// </summary>
        /// <returns>The ECParameters representing the point on the curve for this key, using the explicit curve format.</returns>
        public virtual ECParameters ExportExplicitParameters() {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }
    }
}
