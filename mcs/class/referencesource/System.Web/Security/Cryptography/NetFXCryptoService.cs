//------------------------------------------------------------------------------
// <copyright file="NetFXCryptoService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /******************************************************************
    * !! WARNING !!                                                  *
    * This class contains cryptographic code. If you make changes to *
    * this class, please have it reviewed by the appropriate people. *
    ******************************************************************/

    // Uses .NET Framework classes to encrypt (SymmetricAlgorithm) and sign (KeyedHashAlgorithm) data.
    //
    // [PROTECT]
    // INPUT: clearData
    // OUTPUT: protectedData
    // ALGORITHM:
    //   protectedData := IV || Enc(Kenc, IV, clearData) || Sign(Kval, IV || Enc(Kenc, IV, clearData))
    //
    // [UNPROTECT]
    // INPUT: protectedData
    // OUTPUT: clearData
    // ALGORITHM:
    //   1) Assume protectedData := IV || Enc(Kenc, IV, clearData) || Sign(Kval, IV || Enc(Kenc, IV, clearData))
    //   2) Validate the signature over the payload and strip it from the end
    //   3) Strip off the IV from the beginning of the payload
    //   4) Decrypt what remains of the payload, and return it as clearData

    internal sealed class NetFXCryptoService : ICryptoService {

        private readonly ICryptoAlgorithmFactory _cryptoAlgorithmFactory;
        private readonly CryptographicKey _encryptionKey;
        private readonly bool _predictableIV;
        private readonly CryptographicKey _validationKey;

        public NetFXCryptoService(ICryptoAlgorithmFactory cryptoAlgorithmFactory, CryptographicKey encryptionKey, CryptographicKey validationKey, bool predictableIV = false) {
            _cryptoAlgorithmFactory = cryptoAlgorithmFactory;
            _encryptionKey = encryptionKey;
            _validationKey = validationKey;
            _predictableIV = predictableIV;
        }

        public byte[] Protect(byte[] clearData) {
            // The entire operation is wrapped in a 'checked' block because any overflows should be treated as failures.
            checked {

                // These SymmetricAlgorithm instances are single-use; we wrap it in a 'using' block.
                using (SymmetricAlgorithm encryptionAlgorithm = _cryptoAlgorithmFactory.GetEncryptionAlgorithm()) {
                    // Initialize the algorithm with the specified key and an appropriate IV
                    encryptionAlgorithm.Key = _encryptionKey.GetKeyMaterial();

                    if (_predictableIV) {
                        // The caller wanted the output to be predictable (e.g. for caching), so we'll create an
                        // appropriate IV directly from the input buffer. The IV length is equal to the block size.
                        encryptionAlgorithm.IV = CryptoUtil.CreatePredictableIV(clearData, encryptionAlgorithm.BlockSize);
                    }
                    else {
                        // If the caller didn't ask for a predictable IV, just let the algorithm itself choose one.
                        encryptionAlgorithm.GenerateIV();
                    }
                    byte[] iv = encryptionAlgorithm.IV;

                    using (MemoryStream memStream = new MemoryStream()) {
                        memStream.Write(iv, 0, iv.Length);

                        // At this point:
                        // memStream := IV

                        // Write the encrypted payload to the memory stream.
                        using (ICryptoTransform encryptor = encryptionAlgorithm.CreateEncryptor()) {
                            using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write(clearData, 0, clearData.Length);
                                cryptoStream.FlushFinalBlock();

                                // At this point:
                                // memStream := IV || Enc(Kenc, IV, clearData)

                                // These KeyedHashAlgorithm instances are single-use; we wrap it in a 'using' block.
                                using (KeyedHashAlgorithm signingAlgorithm = _cryptoAlgorithmFactory.GetValidationAlgorithm()) {
                                    // Initialize the algorithm with the specified key
                                    signingAlgorithm.Key = _validationKey.GetKeyMaterial();

                                    // Compute the signature
                                    byte[] signature = signingAlgorithm.ComputeHash(memStream.GetBuffer(), 0, (int)memStream.Length);

                                    // At this point:
                                    // memStream := IV || Enc(Kenc, IV, clearData)
                                    // signature := Sign(Kval, IV || Enc(Kenc, IV, clearData))

                                    // Append the signature to the encrypted payload
                                    memStream.Write(signature, 0, signature.Length);

                                    // At this point:
                                    // memStream := IV || Enc(Kenc, IV, clearData) || Sign(Kval, IV || Enc(Kenc, IV, clearData))

                                    // Algorithm complete
                                    byte[] protectedData = memStream.ToArray();
                                    return protectedData;
                                }
                            }
                        }
                    }
                }
            }
        }

        public byte[] Unprotect(byte[] protectedData) {
            // The entire operation is wrapped in a 'checked' block because any overflows should be treated as failures.
            checked {

                // We want to check that the input is in the form:
                // protectedData := IV || Enc(Kenc, IV, clearData) || Sign(Kval, IV || Enc(Kenc, IV, clearData))

                // Definitions used in this method:
                // encryptedPayload := Enc(Kenc, IV, clearData)
                // signature := Sign(Kval, IV || encryptedPayload)

                // These SymmetricAlgorithm instances are single-use; we wrap it in a 'using' block.
                using (SymmetricAlgorithm decryptionAlgorithm = _cryptoAlgorithmFactory.GetEncryptionAlgorithm()) {
                    decryptionAlgorithm.Key = _encryptionKey.GetKeyMaterial();

                    // These KeyedHashAlgorithm instances are single-use; we wrap it in a 'using' block.
                    using (KeyedHashAlgorithm validationAlgorithm = _cryptoAlgorithmFactory.GetValidationAlgorithm()) {
                        validationAlgorithm.Key = _validationKey.GetKeyMaterial();

                        // First, we need to verify that protectedData is even long enough to contain
                        // the required components (IV, encryptedPayload, signature).

                        int ivByteCount = decryptionAlgorithm.BlockSize / 8; // IV length is equal to the block size
                        int signatureByteCount = validationAlgorithm.HashSize / 8;
                        int encryptedPayloadByteCount = protectedData.Length - ivByteCount - signatureByteCount;
                        if (encryptedPayloadByteCount <= 0) {
                            // protectedData doesn't meet minimum length requirements
                            return null;
                        }

                        // If that check passes, we need to detect payload tampering.

                        // Compute the signature over the IV and encrypted payload
                        // computedSignature := Sign(Kval, IV || encryptedPayload)
                        byte[] computedSignature = validationAlgorithm.ComputeHash(protectedData, 0, ivByteCount + encryptedPayloadByteCount);

                        if (!CryptoUtil.BuffersAreEqual(
                            buffer1: protectedData, buffer1Offset: ivByteCount + encryptedPayloadByteCount, buffer1Count: signatureByteCount,
                            buffer2: computedSignature, buffer2Offset: 0, buffer2Count: computedSignature.Length)) {

                            // the computed signature didn't match the incoming signature, which is a sign of payload tampering
                            return null;
                        }

                        // At this point, we're certain that we generated the signature over this payload,
                        // so we can go ahead with decryption.

                        // Populate the IV from the incoming stream
                        byte[] iv = new byte[ivByteCount];
                        Buffer.BlockCopy(protectedData, 0, iv, 0, iv.Length);
                        decryptionAlgorithm.IV = iv;

                        // Write the decrypted payload to the memory stream.
                        using (MemoryStream memStream = new MemoryStream()) {
                            using (ICryptoTransform decryptor = decryptionAlgorithm.CreateDecryptor()) {
                                using (CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write)) {
                                    cryptoStream.Write(protectedData, ivByteCount, encryptedPayloadByteCount);
                                    cryptoStream.FlushFinalBlock();

                                    // At this point
                                    // memStream := clearData

                                    byte[] clearData = memStream.ToArray();
                                    return clearData;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
