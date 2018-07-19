//------------------------------------------------------------------------------
// <copyright file="MachineKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * MachineKey
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Security {
    using System;
    using System.Linq;
    using System.Web.Configuration;
    using System.Web.Security.Cryptography;
    using System.Web.Util;

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    public enum MachineKeyProtection {
        All,
        Encryption,
        Validation
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    public static class MachineKey {
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [Obsolete("This method is obsolete and is only provided for compatibility with existing code. It is recommended that new code use the Protect and Unprotect methods instead.")]
        public static string Encode(byte[] data, MachineKeyProtection protectionOption) {
            if (data == null)
                throw new ArgumentNullException("data");

            //////////////////////////////////////////////////////////////////////
            // Step 1: Get the MAC and add to the blob
            if (protectionOption == MachineKeyProtection.All || protectionOption == MachineKeyProtection.Validation) {
                byte[] bHash = MachineKeySection.HashData(data, null, 0, data.Length);
                byte[] bAll = new byte[bHash.Length + data.Length];
                Buffer.BlockCopy(data, 0, bAll, 0, data.Length);
                Buffer.BlockCopy(bHash, 0, bAll, data.Length, bHash.Length);
                data = bAll;
            }

            if (protectionOption == MachineKeyProtection.All || protectionOption == MachineKeyProtection.Encryption) {
                //////////////////////////////////////////////////////////////////////
                // Step 2: Encryption
                data = MachineKeySection.EncryptOrDecryptData(true, data, null, 0, data.Length, false, false, IVType.Random, !AppSettings.UseLegacyMachineKeyEncryption);
            }

            //////////////////////////////////////////////////////////////////////
            // Step 3: Covert the buffer to HEX string and return it
            return CryptoUtil.BinaryToHex(data);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [Obsolete("This method is obsolete and is only provided for compatibility with existing code. It is recommended that new code use the Protect and Unprotect methods instead.")]
        public static byte[] Decode(string encodedData, MachineKeyProtection protectionOption) {
            if (encodedData == null)
                throw new ArgumentNullException("encodedData");

            if ((encodedData.Length % 2) != 0)
                throw new ArgumentException(null, "encodedData");

            byte[] data = null;
            try {
                //////////////////////////////////////////////////////////////////////
                // Step 1: Covert the HEX string to byte array
                data = CryptoUtil.HexToBinary(encodedData);
            }
            catch {
                throw new ArgumentException(null, "encodedData");
            }

            if (data == null || data.Length < 1)
                throw new ArgumentException(null, "encodedData");

            if (protectionOption == MachineKeyProtection.All || protectionOption == MachineKeyProtection.Encryption) {
                //////////////////////////////////////////////////////////////////
                // Step 2: Decrypt the data
                data = MachineKeySection.EncryptOrDecryptData(false, data, null, 0, data.Length, false, false, IVType.Random, !AppSettings.UseLegacyMachineKeyEncryption);
                if (data == null)
                    return null;
            }

            if (protectionOption == MachineKeyProtection.All || protectionOption == MachineKeyProtection.Validation) {
                //////////////////////////////////////////////////////////////////
                // Step 3a: Remove the hash from the end of the data
                if (data.Length < MachineKeySection.HashSize)
                    return null;
                byte[] originalData = data;
                data = new byte[originalData.Length - MachineKeySection.HashSize];
                Buffer.BlockCopy(originalData, 0, data, 0, data.Length);

                //////////////////////////////////////////////////////////////////
                // Step 3b: Calculate the hash and make sure it matches
                byte[] bHash = MachineKeySection.HashData(data, null, 0, data.Length);
                if (bHash == null || bHash.Length != MachineKeySection.HashSize)
                    return null; // Sizes don't match
                for (int iter = 0; iter < bHash.Length; iter++) {
                    if (bHash[iter] != originalData[data.Length + iter])
                        return null; // Mis-match found
                }
            }

            return data;
        }

        /// <summary>
        /// Cryptographically protects and tamper-proofs the specified data.
        /// </summary>
        /// <param name="userData">The plaintext data that needs to be protected.</param>
        /// <param name="purposes">(optional) A list of purposes that describe what the data is meant for.
        /// If this value is specified, the same list must be passed to the Unprotect method in order
        /// to decipher the returned ciphertext.</param>
        /// <returns>The ciphertext data. To decipher the data, call the Unprotect method, passing this
        /// value as the 'protectedData' parameter.</returns>
        /// <remarks>
        /// This method supercedes the Encode method, which required the caller to know whether he wanted
        /// the plaintext data to be encrypted, signed, or both. In contrast, the Protect method just
        /// does the right thing and securely protects the data. Ciphertext data produced by this method
        /// can only be deciphered by the Unprotect method.
        /// 
        /// The 'purposes' parameter is an optional list of reason strings that can lock the ciphertext
        /// to a specific purpose. The intent of this parameter is that different subsystems within
        /// an application may depend on cryptographic operations, and a malicious client should not be
        /// able to get the result of one subsystem's Protect method and feed it as input to another
        /// subsystem's Unprotect method, which could have undesirable or insecure behavior. In essence,
        /// the 'purposes' parameter helps ensure that some protected data can be consumed only by the
        /// component that originally generated it. Applications should take care to ensure that each
        /// subsystem uses a unique 'purposes' list.
        ///
        /// For example, to protect or unprotect an authentication token, the application could call:
        /// MachineKey.Protect(..., "Authentication token");
        /// MachineKey.Unprotect(..., "Authentication token");
        /// 
        /// Applications may dynamically generate the 'purposes' parameter if desired. If an application
        /// does this, user-supplied values like usernames should never directly be passed for the 'purposes'
        /// parameter. They should instead be prefixed with something (like "Username: " + username) to
        /// minimize the risk of a malicious client crafting input that collides with a token in use by some
        /// other part of the system. Any dynamically-generated tokens should come after non-dynamically
        /// generated tokens.
        /// 
        /// For example, to protect or unprotect a private message that is tied to a specific user, the
        /// application could call:
        /// MachineKey.Protect(..., "Private message", "Recipient: " + username);
        /// MachineKey.Unprotect(..., "Private message", "Recipient: " + username);
        /// 
        /// In both of the above examples, is it important that the caller of the Unprotect method be able to
        /// resurrect the original 'purposes' list. Otherwise the operation will fail with a CryptographicException.
        /// </remarks>
        public static byte[] Protect(byte[] userData, params string[] purposes) {
            if (userData == null) {
                throw new ArgumentNullException("userData");
            }

            // Technically we don't care if the purposes array contains whitespace-only entries,
            // but the DataProtector class does, so we'll just block them right here.
            if (purposes != null && purposes.Any(String.IsNullOrWhiteSpace)) {
                throw new ArgumentException(SR.GetString(SR.MachineKey_InvalidPurpose), "purposes");
            }

            return Protect(AspNetCryptoServiceProvider.Instance, userData, purposes);
        }

        // Internal method for unit testing.
        internal static byte[] Protect(ICryptoServiceProvider cryptoServiceProvider, byte[] userData, string[] purposes) {
            // If the user is calling this method, we want to use the ICryptoServiceProvider
            // regardless of whether or not it's the default provider.

            Purpose derivedPurpose = Purpose.User_MachineKey_Protect.AppendSpecificPurposes(purposes);
            ICryptoService cryptoService = cryptoServiceProvider.GetCryptoService(derivedPurpose);
            return cryptoService.Protect(userData);
        }

        /// <summary>
        /// Verifies the integrity of and deciphers the given ciphertext.
        /// </summary>
        /// <param name="protectedData">Ciphertext data that was produced by the Protect method.</param>
        /// <param name="purposes">(optional) A list of purposes that describe what the data is meant for.</param>
        /// <returns>The plaintext data.</returns>
        /// <exception>Throws a CryptographicException if decryption fails. This can occur if the 'protectedData' has
        /// been tampered with, if an incorrect 'purposes' parameter is specified, or if an application is deployed
        /// to more than one server (as in a farm scenario) but is using auto-generated encryption keys.</exception>
        /// <remarks>See documentation on the Protect method for more information.</remarks>
        public static byte[] Unprotect(byte[] protectedData, params string[] purposes) {
            if (protectedData == null) {
                throw new ArgumentNullException("protectedData");
            }

            // Technically we don't care if the purposes array contains whitespace-only entries,
            // but the DataProtector class does, so we'll just block them right here.
            if (purposes != null && purposes.Any(String.IsNullOrWhiteSpace)) {
                throw new ArgumentException(SR.GetString(SR.MachineKey_InvalidPurpose), "purposes");
            }

            return Unprotect(AspNetCryptoServiceProvider.Instance, protectedData, purposes);
        }

        // Internal method for unit testing.
        internal static byte[] Unprotect(ICryptoServiceProvider cryptoServiceProvider, byte[] protectedData, string[] purposes) {
            // If the user is calling this method, we want to use the ICryptoServiceProvider
            // regardless of whether or not it's the default provider.

            Purpose derivedPurpose = Purpose.User_MachineKey_Protect.AppendSpecificPurposes(purposes);
            ICryptoService cryptoService = cryptoServiceProvider.GetCryptoService(derivedPurpose);
            return cryptoService.Unprotect(protectedData);
        }

    }
}
