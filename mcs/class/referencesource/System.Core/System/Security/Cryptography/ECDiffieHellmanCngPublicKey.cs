// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Public key used to do key exchange with the ECDiffieHellmanCng algorithm
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class ECDiffieHellmanCngPublicKey : ECDiffieHellmanPublicKey {
        [NonSerialized]
        private CngKey m_key;
        private CngKeyBlobFormat m_format;

        /// <summary>
        ///     Wrap a CNG key
        /// </summary>
        [SecuritySafeCritical]
        internal ECDiffieHellmanCngPublicKey(CngKey key) : base(key.Export(CngKeyBlobFormat.EccPublicBlob)) {
            Contract.Requires(key != null && key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);
            Contract.Ensures(m_format != null);

            m_format = CngKeyBlobFormat.EccPublicBlob;

            //
            // We need to make a copy of the key to prevent the situation where the ECDiffieHellmanCng algorithm
            // object is disposed (this disposing its key) before the ECDiffieHellmanCngPublic key is disposed.
            //
            // Accessing the handle in partial trust is safe because we're not exposing it back out to user code
            //

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

            // This looks odd, but .Handle returns a duplicate, so we need to dispose it
            using (SafeNCryptKeyHandle importKey = key.Handle) {
                m_key = CngKey.Open(importKey, key.IsEphemeral ? CngKeyHandleOpenOptions.EphemeralKey : CngKeyHandleOpenOptions.None);
            }

            CodeAccessPermission.RevertAssert();
        }

        /// <summary>
        ///     Format the key blob is expressed in
        /// </summary>
        public CngKeyBlobFormat BlobFormat {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);
                Contract.Assert(m_format != null);

                return m_format;
            }
        }

        /// <summary>
        ///     Clean up the key
        /// </summary>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    if (m_key != null) {
                        m_key.Dispose();
                    }
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Hydrate a public key from a blob
        /// </summary>
        [SecuritySafeCritical]
        public static ECDiffieHellmanPublicKey FromByteArray(byte[] publicKeyBlob, CngKeyBlobFormat format) {
            if (publicKeyBlob == null) {
                throw new ArgumentNullException("publicKeyBlob");
            }
            if (format == null) {
                throw new ArgumentNullException("format");
            }

            using (CngKey imported = CngKey.Import(publicKeyBlob, format)) {
                if (imported.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey));
                }

                return new ECDiffieHellmanCngPublicKey(imported);
            }
        }

        /// <summary>
        ///     Hydrate a public key from XML
        /// 
        ///     See code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information
        ///     about the XML format used.
        /// </summary>
        [SecuritySafeCritical]
        public static ECDiffieHellmanCngPublicKey FromXmlString(string xml) {
            if (xml == null) {
                throw new ArgumentNullException("xml");
            }

            using (CngKey imported = Rfc4050KeyFormatter.FromXml(xml)) {
                if (imported.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey), "xml");
                }

                return new ECDiffieHellmanCngPublicKey(imported);
            }
        }

        /// <summary>
        ///     Import the public key into CNG
        /// </summary>
        /// <returns></returns>
        public CngKey Import() {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            Contract.Assert(m_format != null);

            return CngKey.Import(ToByteArray(), BlobFormat);
        }

        /// <summary>
        ///     Convert the key blob to XML
        /// 
        ///     See code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information
        ///     about the XML format used.
        /// </summary>
        public override string ToXmlString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (m_key == null) {
                m_key = Import();
            }

            return Rfc4050KeyFormatter.ToXml(m_key);
        }
    }
}
