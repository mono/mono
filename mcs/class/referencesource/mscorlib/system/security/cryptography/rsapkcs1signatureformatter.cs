// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// RSAPKCS1SignatureFormatter.cs
//

using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter {
        private RSA    _rsaKey;
        private String _strOID;
        private bool?  _rsaOverridesSignHash;

        //
        // public constructors
        //

        public RSAPKCS1SignatureFormatter() {}

        public RSAPKCS1SignatureFormatter(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
        }

        //
        // public methods
        //

        public override void SetKey(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
            _rsaOverridesSignHash = default(bool?);
        }

        public override void SetHashAlgorithm(String strName) {
            _strOID = CryptoConfig.MapNameToOID(strName, OidGroup.HashAlgorithm);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override byte[] CreateSignature(byte[] rgbHash) {
            if (rgbHash == null)
                throw new ArgumentNullException("rgbHash");
            Contract.EndContractBlock();

            if (_strOID == null)
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingOID"));
            if (_rsaKey == null)
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));

            // Two cases here -- if we are talking to the CSP version or if we are talking to some other RSA provider.
            if (_rsaKey is RSACryptoServiceProvider) {
                // This path is kept around for desktop compat: in case someone is using this with a hash algorithm that's known to GetAlgIdFromOid but
                // not from OidToHashAlgorithmName.
                int calgHash = X509Utils.GetAlgIdFromOid(_strOID, OidGroup.HashAlgorithm);
                return ((RSACryptoServiceProvider)_rsaKey).SignHash(rgbHash, calgHash);
            }
            else if (OverridesSignHash) {
                HashAlgorithmName hashAlgorithmName = Utils.OidToHashAlgorithmName(_strOID);
                return _rsaKey.SignHash(rgbHash, hashAlgorithmName, RSASignaturePadding.Pkcs1);
            }
            else {
                // Fallback compat path for 3rd-party RSA classes that don't override SignHash()

                byte[] pad = Utils.RsaPkcs1Padding(_rsaKey, CryptoConfig.EncodeOID(_strOID), rgbHash);
                // Create the signature by applying the private key to the padded buffer we just created.
                return _rsaKey.DecryptValue(pad);
            }
        }

        private bool OverridesSignHash {
            get {
                if (!_rsaOverridesSignHash.HasValue) {
                    _rsaOverridesSignHash = Utils.DoesRsaKeyOverride(_rsaKey, "SignHash", new Type[] { typeof(byte[]), typeof(HashAlgorithmName), typeof(RSASignaturePadding) });
                }
                return _rsaOverridesSignHash.Value;
            }
        }
    }
}
