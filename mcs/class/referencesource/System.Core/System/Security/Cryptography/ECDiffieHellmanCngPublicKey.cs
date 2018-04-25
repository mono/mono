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
        private CngKeyBlobFormat m_format;
        [OptionalField] private string m_curveName;

        /// <summary>
        ///     Wrap a CNG key
        /// </summary>
        [SecuritySafeCritical]
        internal ECDiffieHellmanCngPublicKey(byte[] keyBlob, string curveName, CngKeyBlobFormat format) : base(keyBlob) {
            Contract.Requires(format != null);
            Contract.Ensures(m_format != null);

            m_format = format;
            // Can be null for P256, P384, P521, or an explicit blob
            m_curveName = curveName;
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
            base.Dispose(disposing);
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

            // Verify that the key can import successfully, because we did in the past.
            using (CngKey imported = CngKey.Import(publicKeyBlob, format)) {
                if (imported.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey));
                }

                return new ECDiffieHellmanCngPublicKey(publicKeyBlob, null, format);
            }
        }

        internal static ECDiffieHellmanCngPublicKey FromKey(CngKey key) {
            Contract.Requires(key != null && key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);
            Contract.Ensures(Contract.Result<ECDiffieHellmanCngPublicKey>() != null);

            CngKeyBlobFormat format;
            string curveName;
            byte[] blob = ECCng.ExportKeyBlob(key, false, out format, out curveName);
            return new ECDiffieHellmanCngPublicKey(blob, curveName, format);
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

            bool isEcdh;
            ECParameters parameters = Rfc4050KeyFormatter.FromXml(xml, out isEcdh);

            if (!isEcdh) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey), "xml");
            }

            CngKeyBlobFormat format;
            string curveName;
            byte[] blob = ECCng.EcdhParametersToBlob(ref parameters, out format, out curveName);
            return new ECDiffieHellmanCngPublicKey(blob, curveName, format);
        }

        /// <summary>
        ///     Import the public key into CNG
        /// </summary>
        /// <returns></returns>
        public CngKey Import() {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            Contract.Assert(m_format != null);

            return CngKey.Import(ToByteArray(), m_curveName, BlobFormat);
        }

        /// <summary>
        ///     Convert the key blob to XML
        /// 
        ///     See code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information
        ///     about the XML format used.
        /// </summary>
        public override string ToXmlString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            ECParameters ecParams = ExportParameters();
            return Rfc4050KeyFormatter.ToXml(ecParams, isEcdh: true);
        }

        /// <summary>
        ///  Exports the key and explicit curve parameters used by the ECC object into an <see cref="ECParameters"/> object.
        /// </summary>
        /// <exception cref="CryptographicException">
        ///  if there was an issue obtaining the curve values.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        ///  if explicit export is not supported by this platform. Windows 10 or higher is required.
        /// </exception>
        /// <returns>The key and explicit curve parameters used by the ECC object.</returns>
        public override ECParameters ExportExplicitParameters() {
            using (CngKey key = Import()) {
                return ECCng.ExportExplicitParameters(key, includePrivateParameters: false);
            }
        }

        /// <summary>
        ///  Exports the key used by the ECC object into an <see cref="ECParameters"/> object.
        ///  If the key was created as a named curve, the Curve property will contain named curve parameters
        ///  otherwise it will contain explicit parameters.
        /// </summary>
        /// <exception cref="CryptographicException">
        ///  if there was an issue obtaining the curve values.
        /// </exception>
        /// <returns>The key and named curve parameters used by the ECC object.</returns>
        public override ECParameters ExportParameters() {
            using (CngKey key = Import()) {
                return ECCng.ExportParameters(key, includePrivateParameters: false);
            }
        }
    }
}
