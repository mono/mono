//------------------------------------------------------------------------------
// <copyright file="FipsAwareEncryptedXml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Xml;

    // 
    // Extends EncryptedXml to use FIPS-certified symmetric algorithm
    // 
    class FipsAwareEncryptedXml : EncryptedXml {

        public FipsAwareEncryptedXml(XmlDocument doc)
            : base(doc) {
        }

        // Override EncryptedXml.GetDecryptionKey to avoid calling into CryptoConfig.CreateFromName
        // When detect AES, we need to return AesCryptoServiceProvider (FIPS certified) instead of AesManaged (FIPS obsolated)
        public override SymmetricAlgorithm GetDecryptionKey(EncryptedData encryptedData, string symmetricAlgorithmUri) {
            
            // If AES is used then assume FIPS is required
            bool fipsRequired = IsAesDetected(encryptedData, symmetricAlgorithmUri);

            if (fipsRequired) {
                // Obtain the EncryptedKey
                EncryptedKey ek = null;

                foreach (var ki in encryptedData.KeyInfo) {
                    KeyInfoEncryptedKey kiEncKey = ki as KeyInfoEncryptedKey;
                    if (kiEncKey != null) {
                        ek = kiEncKey.EncryptedKey;
                        break;
                    }
                }

                // Got an EncryptedKey, decrypt it to get the AES key
                if (ek != null) {
                    byte[] key = DecryptEncryptedKey(ek);

                    // Construct FIPS-certified AES provider
                    if (key != null) {
                        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                        aes.Key = key;
                        
                        return aes;
                    }
                }
            }

            // Fallback to the base implementation
            return base.GetDecryptionKey(encryptedData, symmetricAlgorithmUri);
        }

        private static bool IsAesDetected(EncryptedData encryptedData, string symmetricAlgorithmUri) {
            if (encryptedData != null &&
                encryptedData.KeyInfo != null &&
                (symmetricAlgorithmUri != null || encryptedData.EncryptionMethod != null)) {

                if (symmetricAlgorithmUri == null) {
                    symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
                }

                // Check if the Uri matches AES256
                return string.Equals(symmetricAlgorithmUri, EncryptedXml.XmlEncAES256Url, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }
    }
}
