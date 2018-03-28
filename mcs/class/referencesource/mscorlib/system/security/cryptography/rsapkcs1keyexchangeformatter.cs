// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

namespace System.Security.Cryptography {
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class RSAPKCS1KeyExchangeFormatter : AsymmetricKeyExchangeFormatter {
        RandomNumberGenerator RngValue;
        RSA _rsaKey;
        bool?  _rsaOverridesEncrypt;

        //
        // public constructors
        //

        public RSAPKCS1KeyExchangeFormatter() {}

        public RSAPKCS1KeyExchangeFormatter(AsymmetricAlgorithm key) {
            if (key == null) 
                throw new ArgumentNullException("key");
            Contract.EndContractBlock();
            _rsaKey = (RSA) key;
        }

        //
        // public properties
        //

        public override String Parameters {
            get { return "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />"; }
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

        public override byte[] CreateKeyExchange(byte[] rgbData) {
#if MONO
			if (rgbData == null)
				throw new ArgumentNullException ("rgbData");
#endif

            if (_rsaKey == null)
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));

            byte[] rgbKeyEx;
            if (OverridesEncrypt) {
                rgbKeyEx = _rsaKey.Encrypt(rgbData, RSAEncryptionPadding.Pkcs1);
            }
            else {
                int cb = _rsaKey.KeySize/8;
                if ((rgbData.Length + 11) > cb)
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_EncDataTooBig", cb-11));
                byte[]  rgbInput = new byte[cb];

                //
                //  We want to pad to the following format:
                //      00 || 02 || PS || 00 || D
                //
                //      PS - pseudorandom non zero bytes
                //      D - data
                //

                if (RngValue == null) {
                    RngValue = RandomNumberGenerator.Create();
                }
                
                Rng.GetNonZeroBytes(rgbInput);
                rgbInput[0] = 0;
                rgbInput[1] = 2;
                rgbInput[cb-rgbData.Length-1] = 0;
                Buffer.InternalBlockCopy(rgbData, 0, rgbInput, cb-rgbData.Length, rgbData.Length);

                //
                //  Now encrypt the value and return it. (apply public key)
                //

                rgbKeyEx = _rsaKey.EncryptValue(rgbInput);
            }
            return rgbKeyEx;
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
