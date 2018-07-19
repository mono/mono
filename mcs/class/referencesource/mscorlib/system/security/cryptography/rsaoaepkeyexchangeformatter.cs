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
    public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter {
        private byte[] ParameterValue;
        private RSA _rsaKey;
        private bool?  _rsaOverridesEncrypt;
        private RandomNumberGenerator RngValue;

        //
        // public constructors
        //

        public RSAOAEPKeyExchangeFormatter() {}
        public RSAOAEPKeyExchangeFormatter(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
        }

        //
        // public properties
        //

        /// <internalonly/>
        public byte[] Parameter {
            get {
                if (ParameterValue != null)
                    return (byte[]) ParameterValue.Clone(); 
                return null;
            }
            set {
                if (value != null)
                    ParameterValue = (byte[]) value.Clone();
                else 
                    ParameterValue = null;
            }
        }

        /// <internalonly/>
        public override String Parameters {
            get { return null; }
        }

        public RandomNumberGenerator Rng {
            get { return RngValue; }
            set { RngValue = value; }
        }

        //
        // public methods
        //

        public override void SetKey(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
            _rsaOverridesEncrypt = default(bool?);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override byte[] CreateKeyExchange(byte[] rgbData) {
            if (_rsaKey == null)
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));

            if (OverridesEncrypt) {
                return _rsaKey.Encrypt(rgbData, RSAEncryptionPadding.OaepSHA1);
            } else {
                return Utils.RsaOaepEncrypt(_rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), RandomNumberGenerator.Create(), rgbData);
            }
        }

        public override byte[] CreateKeyExchange(byte[] rgbData, Type symAlgType) {
            return CreateKeyExchange(rgbData);
        }

        private bool OverridesEncrypt {
            get {
                if (!_rsaOverridesEncrypt.HasValue) {
                    _rsaOverridesEncrypt = Utils.DoesRsaKeyOverride(_rsaKey, "Encrypt", new Type[] { typeof(byte[]), typeof(RSAEncryptionPadding) });
                }
                return _rsaOverridesEncrypt.Value;
            }
        }
    }
}
