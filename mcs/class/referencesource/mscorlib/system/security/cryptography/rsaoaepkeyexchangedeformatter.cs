using System.Diagnostics.Contracts;
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {
        private RSA _rsaKey; // RSA Key value to do decrypt operation
        private bool?  _rsaOverridesDecrypt;

        //
        // public constructors
        //

        public RSAOAEPKeyExchangeDeformatter() {}
        public RSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
        }

        //
        // public properties
        //

        public override String Parameters {
            get { return null; }
            set { ; }
        }

        //
        // public methods
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override byte[] DecryptKeyExchange(byte[] rgbData) {
            if (_rsaKey == null)
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));

            if (OverridesDecrypt) {
                return _rsaKey.Decrypt(rgbData, RSAEncryptionPadding.OaepSHA1);
            } else {
                return Utils.RsaOaepDecrypt(_rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), rgbData);
            }
        }

        public override void SetKey(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
            _rsaOverridesDecrypt = default(bool?);
        }

        private bool OverridesDecrypt {
            get {
                if (!_rsaOverridesDecrypt.HasValue) {
                    _rsaOverridesDecrypt = Utils.DoesRsaKeyOverride(_rsaKey, "Decrypt", new Type[] { typeof(byte[]), typeof(RSAEncryptionPadding) });
                }
                return _rsaOverridesDecrypt.Value;
            }
        }
    }
}
