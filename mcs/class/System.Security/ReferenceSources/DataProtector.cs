//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Crypto = System.Security.Cryptography.Translation;

namespace System.Security.Cryptography
{
    // Data protectors should be derived from this class
    public abstract class DataProtector
    {
		private string m_applicationName;
        private string m_primaryPurpose;
        private IEnumerable<string> m_specificPurposes;
 
        private volatile byte[] m_hashedPurpose;
 
        // Required Constructor for DataProtector
        protected DataProtector(string applicationName,
                                string primaryPurpose,
                                string[] specificPurposes)
        {
            // We require that applicationName, primaryPurpose, and specificPurpose elements to be provided and not whitespace
            if (String.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (applicationName));
            if (String.IsNullOrWhiteSpace(primaryPurpose))
                throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (primaryPurpose));
 
            // Check each of the specific purposes if they were passed
            if (specificPurposes != null)
            {
                foreach (string purpose in specificPurposes)
                {
                    if (String.IsNullOrWhiteSpace(purpose))
                    {
                        throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (specificPurposes));
                    }
                }
            }
 
            m_applicationName = applicationName;
            m_primaryPurpose = primaryPurpose;
 
            List<string> specificPurposesList = new List<string>();
            if (specificPurposes != null)
            {
                specificPurposesList.AddRange(specificPurposes);
            }
            m_specificPurposes = specificPurposesList;
        }
 
        protected string ApplicationName
        {
            get { return m_applicationName; }
        }
 
        // We will be safe and assume that derived classes want to have the hash pre-pended to the plain text
        // before encryption, and checked and verified during decryption.  If a derived class wants to use
        // HashedPurpose on its own (e.g. as OptionalEntropy to DPAPI or for some sort of key derivation), this
        // property can be overridden and set to return false.  We will then just pass Protect/Unprotect directly
        // through to ProviderProtect/ProviderUnprotect without altering the array
        protected virtual bool PrependHashedPurposeToPlaintext
        {
            get { return true; }
        }
 
        // A hash of the full purpose passed to the constructor or factory
        protected virtual byte[] GetHashedPurpose()
        {
            if (m_hashedPurpose == null)
            {
                // Compute hash of the full purpose.  The full purpose is a concatination of all the
                // parts - applicationName, primaryPurpose,and specificPurposes[].  We prefix each part with
                // the length so we know the process is reversible
                using (HashAlgorithm sha256 = HashAlgorithm.Create("System.Security.Cryptography.Sha256Cng"))
                {
                    using (BinaryWriter stream = new BinaryWriter(new CryptoStream(new MemoryStream(), sha256, CryptoStreamMode.Write), new UTF8Encoding(false, true)))
                    {
                        // Add applicationName to the hash
                        stream.Write(ApplicationName);
 
                        // Add primaryPurpose to the hash
                        stream.Write(PrimaryPurpose);
 
                        // If they exist, add each specificPurposes element to the hash
                        foreach (string purpose in SpecificPurposes)
                        {
                            stream.Write(purpose);
                        }
                    }
 
                    // Now that the CryptoStream is closed, sha256 should have the computed hash
                    m_hashedPurpose = sha256.Hash;
                }
            }
 
            return m_hashedPurpose;
        }
 
        // Allow callers to directly request if an Update is required
        // (e.g. the key used in encryptedData blob is out of date)
        public abstract bool IsReprotectRequired(byte[] encryptedData);
 
        protected string PrimaryPurpose
        {
            get { return m_primaryPurpose; }
        }
 
        protected IEnumerable<string> SpecificPurposes
        {
            get { return m_specificPurposes; }
        }
 
        // Static factory method to create a DataProtector given a type name, a purpose, and a dictionary of parameters
        public static DataProtector Create(string providerClass,
                                           string applicationName,
                                           string primaryPurpose,
                                           params string[] specificPurposes)
        {
            // Make sure providerClass is not null - Other parameters checked in constructor
            if (null == providerClass)
                throw new ArgumentNullException("providerClass");
 
            // Create a DataProtector based on this type using CryptoConfig
            return (DataProtector)CryptoConfig.CreateFromName(providerClass, applicationName, primaryPurpose, specificPurposes);
        }
 
        // Methods for protect/unprotect
        public byte[] Protect(byte[] userData)
        {
            // Make sure we were passed some data - empty array OK
            if (userData == null)
                throw new ArgumentNullException("userData");
 
            // See if the derived class has set PrependHashedPurposeToPlainText to true.
            // If so, we have to pre-pend the hash of the purpose to the plain text before encrypting
            if (PrependHashedPurposeToPlaintext)
            {
                byte[] hashedPurpose = GetHashedPurpose();
 
                // Allocate enough space for userData and HashedPurpose
                byte[] userDataWithHashedPurpose = new byte[userData.Length + hashedPurpose.Length];
 
                // Copy HashedPurpose to the start of the new array
                Array.Copy(hashedPurpose, 0, userDataWithHashedPurpose, 0, hashedPurpose.Length);
 
                // Copy original user data after HashedPurpose
                Array.Copy(userData, 0, userDataWithHashedPurpose, hashedPurpose.Length, userData.Length);
 
                // Swap new array with original user data
                userData = userDataWithHashedPurpose;
            }
            return ProviderProtect(userData);
        }
 
        // Derived classes implement these methods
        protected abstract byte[] ProviderProtect(byte[] userData);
        protected abstract byte[] ProviderUnprotect(byte[] encryptedData);
 
        // For security reasons, we don't want the optimizer to introduce a timing attack against
        // the scenario where we are checking the HashedPurpose.  Whenever we are making decisions
        // based on the plainText bytes, make sure not to bail out early - Doing so will expose
        // information about what the plaintext bytes were.
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public byte[] Unprotect(byte[] encryptedData)
        {
            // Make sure we were given some encrypted data
            if (encryptedData == null)
                throw new ArgumentNullException("encryptedData");
 
            // See if the derived class has set PrependHashedPurposeToPlaintext to true.
            // If so, we have to verify that the first bytes of the plain text are the HashedPurpose
            // Then, return the remaining bytes as the original data.
            if (PrependHashedPurposeToPlaintext)
            {
                // Get the plainText that includes the hash of the purpose
                byte[] plainTextWithHashedPurpose = ProviderUnprotect(encryptedData);
                byte[] hashedPurpose = GetHashedPurpose();
 
                ////////////////////////////////////////////////////////////////////////////////////////
                // In this code block, we don't want any timing differences between success and failure
                // Don't touch this code block without crypto board review
                {
                    // If the length of the decrypted text is less than the HashPurpose, we know something
                    // is wrong - However, we don't want to expose this to the caller via a timing difference
                    // detectable when verifying the HashPurpose.  As a result, we'll still iterate through
                    // the 'for' loop exactly HashPurpose.Length times.
                    bool hashedPurposeOK = plainTextWithHashedPurpose.Length >= hashedPurpose.Length;
                    for (int i = 0; i < hashedPurpose.Length; i++)
                    {
                        // As a trick to handle the case where the plain text was less than the length
                        // of the hash, we modulo it with the lenght of the array - This prevents exceptions
                        // while preserving the number of iterations of this loop
                        if (hashedPurpose[i] != plainTextWithHashedPurpose[i % plainTextWithHashedPurpose.Length])
                        {
                            hashedPurposeOK = false;
                        }
                    }
 
                    if (!hashedPurposeOK)
                    {
                        throw new CryptographicException(Crypto.SR.Cryptography_DataProtector_InvalidPurpose);
                    }
                }
 
                // Now we've verified that the expected hash was at the start of the plain text.  The original
                // plain text specified by the user appears after these bytes.  Create a new array and copy
                // what the caller is expecting into this array
                byte[] plainText = new byte[plainTextWithHashedPurpose.Length - hashedPurpose.Length];
                Array.Copy(plainTextWithHashedPurpose, hashedPurpose.Length, plainText, 0, plainText.Length);
 
                return plainText;
            }
            else
            {
                return ProviderUnprotect(encryptedData);
            }
        }
    }
}
