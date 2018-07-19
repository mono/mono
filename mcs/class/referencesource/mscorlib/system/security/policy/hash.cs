// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// Hash
//
// Evidence corresponding to a hash of the assembly bits.
//

using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Util;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Policy
{
    [Serializable]
    [ComVisible(true)]
    public sealed class Hash : EvidenceBase, ISerializable
    {
        private RuntimeAssembly m_assembly;
        private Dictionary<Type, byte[]> m_hashes;
        private WeakReference m_rawData;

        /// <summary>
        ///     Deserialize a serialized hash evidence object
        /// </summary>
        [SecurityCritical]
        internal Hash(SerializationInfo info, StreamingContext context)
        {
            //
            // We have three serialization formats that we might be deserializing, the Whidbey format which
            // contains hash values directly, the Whidbey format which contains a pointer to a PEImage, and
            // the v4 format which contains a dictionary of calculated hashes.
            // 
            // If we have the Whidbey version that has built in hash values, we can convert that, but we
            // cannot do anything with the PEImage format since that is a serialized pointer into another
            // runtime's VM.
            // 

            Dictionary<Type, byte[]> hashes = info.GetValueNoThrow("Hashes", typeof(Dictionary<Type, byte[]>)) as Dictionary<Type, byte[]>;
            if (hashes != null)
            {
                m_hashes = hashes;
            }
            else
            {
                // If there is no hash value dictionary, then check to see if we have the Whidbey multiple
                // hashes version of the evidence.
                m_hashes = new Dictionary<Type, byte[]>();

                byte[] md5 = info.GetValueNoThrow("Md5", typeof(byte[])) as byte[];
                if (md5 != null)
                {
                    m_hashes[typeof(MD5)] = md5;
                }

                byte[] sha1 = info.GetValueNoThrow("Sha1", typeof(byte[])) as byte[];
                if (sha1 != null)
                {
                    m_hashes[typeof(SHA1)] = sha1;
                }

                byte[] rawData = info.GetValueNoThrow("RawData", typeof(byte[])) as byte[];
                if (rawData != null)
                {
                    GenerateDefaultHashes(rawData);
                }
            }
        }

        /// <summary>
        ///     Create hash evidence for the specified assembly
        /// </summary>
        public Hash(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();
            if (assembly.IsDynamic)
                throw new ArgumentException(Environment.GetResourceString("Security_CannotGenerateHash"), "assembly");

            m_hashes = new Dictionary<Type, byte[]>();
            m_assembly = assembly as RuntimeAssembly;

            if (m_assembly == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");
        }

        /// <summary>
        ///     Create a copy of some hash evidence
        /// </summary>
        private Hash(Hash hash)
        {
            Contract.Assert(hash != null);

            m_assembly = hash.m_assembly;
            m_rawData = hash.m_rawData;
            m_hashes = new Dictionary<Type, byte[]>(hash.m_hashes);
        }

        /// <summary>
        ///     Create a hash evidence prepopulated with a specific hash value
        /// </summary>
        private Hash(Type hashType, byte[] hashValue)
        {
            Contract.Assert(hashType != null);
            Contract.Assert(hashValue != null);

            m_hashes = new Dictionary<Type, byte[]>();

            byte[] hashClone = new byte[hashValue.Length];
            Array.Copy(hashValue, hashClone, hashClone.Length);

            m_hashes[hashType] = hashValue;
        }

        /// <summary>
        ///     Build a Hash evidence that contains the specific SHA-1 hash.  The input hash is not validated,
        ///     and the resulting Hash object cannot calculate additional hash values.
        /// </summary>
        public static Hash CreateSHA1(byte[] sha1)
        {
            if (sha1 == null)
                throw new ArgumentNullException("sha1");
            Contract.EndContractBlock();

            return new Hash(typeof(SHA1), sha1);
        }

        /// <summary>
        ///     Build a Hash evidence that contains the specific SHA-256 hash.  The input hash is not
        ///     validated, and the resulting Hash object cannot calculate additional hash values.
        /// </summary>
        public static Hash CreateSHA256(byte[] sha256)
        {
            if (sha256 == null)
                throw new ArgumentNullException("sha256");
            Contract.EndContractBlock();

            return new Hash(typeof(SHA256), sha256);
        }

        /// <summary>
        ///     Build a Hash evidence that contains the specific MD5 hash.  The input hash is not validated,
        ///     and the resulting Hash object cannot calculate additional hash values.
        /// </summary>
        public static Hash CreateMD5(byte[] md5)
        {
            if (md5 == null)
                throw new ArgumentNullException("md5");
            Contract.EndContractBlock();

            return new Hash(typeof(MD5), md5);
        }

        /// <summary>
        ///     Make a copy of this evidence object
        /// </summary>
        public override EvidenceBase Clone()
        {
            return new Hash(this);
        }

        /// <summary>
        ///     Prepare the hash evidence for serialization
        /// </summary>
        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            GenerateDefaultHashes();
        }

        /// <summary>
        ///     Serialize the hash evidence
        /// </summary>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GenerateDefaultHashes();

            //
            // Backwards compatibility with Whidbey
            //

            byte[] sha1Hash;
            byte[] md5Hash;

            // Whidbey expects the MD5 and SHA1 hashes stored separately.
            if (m_hashes.TryGetValue(typeof(MD5), out md5Hash))
            {
                info.AddValue("Md5", md5Hash);
            }
            if (m_hashes.TryGetValue(typeof(SHA1), out sha1Hash))
            {
                info.AddValue("Sha1", sha1Hash);
            }
            
            // For perf, don't serialize the assembly binary content.
            // This has the side-effect that the Whidbey runtime will not be able to compute any 
            // hashes besides the provided MD5 and SHA1.
            info.AddValue("RawData", null);
            // It doesn't make sense to serialize a memory pointer cross-runtime.
            info.AddValue("PEFile", IntPtr.Zero);

            //
            // Current implementation
            //

            // Add all the computed hashes. While this can duplicate the MD5 and SHA1 hashes, 
            // it allows for a clean separation between legacy support and the current implementation.
            info.AddValue("Hashes", m_hashes);
        }

        /// <summary>
        ///     Get the SHA-1 hash value of the assembly
        /// </summary>
        public byte[] SHA1
        {
            get
            {
                byte[] sha1 = null;
                if (!m_hashes.TryGetValue(typeof(SHA1), out sha1))
                {
                    sha1 = GenerateHash(GetDefaultHashImplementationOrFallback(typeof(SHA1), typeof(SHA1)));
                }

                byte[] returnHash = new byte[sha1.Length];
                Array.Copy(sha1, returnHash, returnHash.Length);
                return returnHash;
            }
        }

        /// <summary>
        ///     Get the SHA-256 hash value of the assembly
        /// </summary>
        public byte[] SHA256
        {
            get
            {
                byte[] sha256 = null;
                if (!m_hashes.TryGetValue(typeof(SHA256), out sha256))
                {
                    sha256 = GenerateHash(GetDefaultHashImplementationOrFallback(typeof(SHA256), typeof(SHA256)));
                }

                byte[] returnHash = new byte[sha256.Length];
                Array.Copy(sha256, returnHash, returnHash.Length);
                return returnHash;
            }
        }

        /// <summary>
        ///     Get the MD5 hash value of the assembly
        /// </summary>
        public byte[] MD5
        {
            get
            {
                byte[] md5 = null;
                if (!m_hashes.TryGetValue(typeof(MD5), out md5))
                {
                    md5 = GenerateHash(GetDefaultHashImplementationOrFallback(typeof(MD5), typeof(MD5)));
                }

                byte[] returnHash = new byte[md5.Length];
                Array.Copy(md5, returnHash, returnHash.Length);
                return returnHash;
            }
        }

        /// <summary>
        ///     Get the hash value of the assembly when hashed with a specific algorithm.  The actual hash
        ///     algorithm object is not used, however the same type of object will be used.
        /// </summary>
        public byte[] GenerateHash(HashAlgorithm hashAlg)
        {
            if (hashAlg == null)
                throw new ArgumentNullException("hashAlg");
            Contract.EndContractBlock();

            byte[] hashValue = GenerateHash(hashAlg.GetType());

            byte[] returnHash = new byte[hashValue.Length];
            Array.Copy(hashValue, returnHash, returnHash.Length);
            return returnHash;
        }

        /// <summary>
        ///     Generate the hash value of an assembly when hashed with the specified algorithm. The result
        ///     may be a direct reference to our internal table of hashes, so it should be copied before
        ///     returning it to user code.
        /// </summary>
        private byte[] GenerateHash(Type hashType)
        {
            Contract.Assert(hashType != null && typeof(HashAlgorithm).IsAssignableFrom(hashType), "Expected a hash algorithm");

            Type indexType = GetHashIndexType(hashType);
            byte[] hashValue = null;
            if (!m_hashes.TryGetValue(indexType, out hashValue))
            {
                // If we're not attached to an assembly, then we cannot generate hashes on demand
                if (m_assembly == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Security_CannotGenerateHash"));
                }

                hashValue = GenerateHash(hashType, GetRawData());
                m_hashes[indexType] = hashValue;
            }

            return hashValue;
        }

        /// <summary>
        ///     Generate a hash of the given type for the assembly data
        /// </summary>
        private static byte[] GenerateHash(Type hashType, byte[] assemblyBytes)
        {
            Contract.Assert(hashType != null && typeof(HashAlgorithm).IsAssignableFrom(hashType), "Expected a hash algorithm");
            Contract.Assert(assemblyBytes != null);

            using (HashAlgorithm hash = HashAlgorithm.Create(hashType.FullName))
            {
                return hash.ComputeHash(assemblyBytes);
            }
        }


        /// <summary>
        ///     Build the default set of hash values that will be available for all assemblies
        /// </summary>
        private void GenerateDefaultHashes()
        {
            // We can't generate any hash values that we don't already have if there isn't an attached
            // assembly to get the hash value of.
            if (m_assembly != null)
            {
                GenerateDefaultHashes(GetRawData());
            }
        }

        /// <summary>
        ///     Build the default set of hash values that will be available for all assemblies given the raw
        ///     assembly data to hash.
        /// </summary>
        private void GenerateDefaultHashes(byte[] assemblyBytes)
        {
            Contract.Assert(assemblyBytes != null);

            Type[] defaultHashTypes = new Type[]
            {
                GetHashIndexType(typeof(SHA1)),
                GetHashIndexType(typeof(SHA256)),
                GetHashIndexType(typeof(MD5))
            };

            foreach (Type defaultHashType in defaultHashTypes)
            {
                Type hashImplementationType = GetDefaultHashImplementation(defaultHashType);
                if (hashImplementationType != null)
                {
                    if (!m_hashes.ContainsKey(defaultHashType))
                    {
                        m_hashes[defaultHashType] = GenerateHash(hashImplementationType, assemblyBytes);
                    }
                }
            }
        }

        /// <summary>
        ///     Map a hash algorithm to the default implementation of that algorithm, falling back to a given
        ///     implementation if no suitable default can be found.  This option may be used for situations
        ///     where it is better to throw an informative exception when trying to use an unsuitable hash
        ///     algorithm than to just return null.  (For instance, throwing a FIPS not supported exception
        ///     when trying to get the MD5 hash evidence).
        /// </summary>
        private static Type GetDefaultHashImplementationOrFallback(Type hashAlgorithm,
                                                                   Type fallbackImplementation)
        {
            Contract.Assert(hashAlgorithm != null && typeof(HashAlgorithm).IsAssignableFrom(hashAlgorithm));
            Contract.Assert(fallbackImplementation != null && GetHashIndexType(hashAlgorithm).IsAssignableFrom(fallbackImplementation));

            Type defaultImplementation = GetDefaultHashImplementation(hashAlgorithm);
            return defaultImplementation != null ? defaultImplementation : fallbackImplementation;
        }

        /// <summary>
        ///     Map a hash algorithm to the default implementation of that algorithm to use for Hash
        ///     evidence, taking into account things such as FIPS support.  If there is no suitable
        ///     implementation for the algorithm, GetDefaultHashImplementation returns null.
        /// </summary>
        private static Type GetDefaultHashImplementation(Type hashAlgorithm)
        {
            Contract.Assert(hashAlgorithm != null && typeof(HashAlgorithm).IsAssignableFrom(hashAlgorithm));

            if (hashAlgorithm.IsAssignableFrom(typeof(MD5)))
            {
                // MD5 is not a FIPS compliant algorithm, so if we need to allow only FIPS implementations,
                // we have no way to create an MD5 hash.  Otherwise, we can just use the standard CAPI
                // implementation since that is available on all operating systems we support.
                if (!CryptoConfig.AllowOnlyFipsAlgorithms)
                {
                    return typeof(MD5CryptoServiceProvider);
                }
                else
                {
                    return null;
                }
            }
            else if (hashAlgorithm.IsAssignableFrom(typeof(SHA256)))
            {
                // The managed SHA256 implementation is not a FIPS certified implementation, however on
                // we have a FIPS alternative.
                return Type.GetType("System.Security.Cryptography.SHA256CryptoServiceProvider, " + AssemblyRef.SystemCore);
            }
            else
            {
                // Otherwise we don't have a better suggestion for the algorithm, so we can just fallback to
                // the input algorithm.
                return hashAlgorithm;
            }
        }

        /// <summary>
        ///     Get the type used to index into the saved hash value dictionary.  We want this to be the
        ///     class which immediately derives from HashAlgorithm so that we can reuse the same hash value
        ///     if we're asked for (e.g.) SHA256Managed and SHA256CryptoServiceProvider
        /// </summary>
        private static Type GetHashIndexType(Type hashType)
        {
            Contract.Assert(hashType != null && typeof(HashAlgorithm).IsAssignableFrom(hashType));

            Type currentType = hashType;

            // Walk up the inheritence hierarchy looking for the first class that derives from HashAlgorithm
            while (currentType != null && currentType.BaseType != typeof(HashAlgorithm))
            {
                currentType = currentType.BaseType;
            }

            // If this is the degenerate case where we started out with HashAlgorithm, we won't find it
            // further up our inheritence tree.
            if (currentType == null)
            {
                BCLDebug.Assert(hashType == typeof(HashAlgorithm), "hashType == typeof(HashAlgorithm)");
                currentType = typeof(HashAlgorithm);
            }

            return currentType;
        }

        /// <summary>
        ///     Raw bytes of the assembly being hashed
        /// </summary>
        private byte[] GetRawData()
        {
            byte[] rawData = null;

            // We can only generate hashes on demand if we're associated with an assembly
            if (m_assembly != null)
            {
                // See if we still hold a reference to the assembly data
                if (m_rawData != null)
                {
                    rawData = m_rawData.Target as byte[];
                }

                // If not, load the raw bytes up
                if (rawData == null)
                {
                    rawData = m_assembly.GetRawBytes();
                    m_rawData = new WeakReference(rawData);
                }
            }

            return rawData;
        }

        private SecurityElement ToXml()
        {
            GenerateDefaultHashes();

            SecurityElement root = new SecurityElement("System.Security.Policy.Hash");
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            BCLDebug.Assert(this.GetType().FullName.Equals("System.Security.Policy.Hash"), "Class name changed!");

            root.AddAttribute("version", "2");
            foreach (KeyValuePair<Type, byte[]> hashValue in m_hashes)
            {
                SecurityElement hashElement = new SecurityElement("hash");
                hashElement.AddAttribute("algorithm", hashValue.Key.Name);
                hashElement.AddAttribute("value", Hex.EncodeHexString(hashValue.Value));

                root.AddChild(hashElement);
            }

            return root;
        }

        public override String ToString()
        {
            return ToXml().ToString();
        }
    }
}
