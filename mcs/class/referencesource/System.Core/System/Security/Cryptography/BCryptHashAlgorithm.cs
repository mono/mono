// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {

    /// <summar>
    /// Implementation of SafeBCryptAlgorithmHandle Cache.
    /// </summary>
    internal sealed class BCryptAlgorithmHandleCache {
        [SecurityCritical]
        private Dictionary<string, WeakReference> m_algorithmHandles;

        [SecurityCritical]
        public BCryptAlgorithmHandleCache()
        {
            m_algorithmHandles = new Dictionary<string, WeakReference>();
        }

        [SecuritySafeCritical]
        public SafeBCryptAlgorithmHandle GetCachedAlgorithmHandle(string algorithm, string implementation) {
            string handleKey = algorithm + implementation;
            SafeBCryptAlgorithmHandle algorithmHandle = null;

            if (m_algorithmHandles.ContainsKey(handleKey))
            {
                algorithmHandle = m_algorithmHandles[handleKey].Target as SafeBCryptAlgorithmHandle;
                if (algorithmHandle != null)
                {
                    return algorithmHandle;
                }
            }
            
            algorithmHandle = BCryptNative.OpenAlgorithm(algorithm, implementation);
            m_algorithmHandles[handleKey] = new WeakReference(algorithmHandle);
            return algorithmHandle;
        }
    }

    /// <summary>
    ///     Implementation of a generic BCrypt hashing algorithm, concrete HashAlgorithm classes
    ///     implemented by BCrypt can contain an instance of this class and delegate the work to it.
    /// </summary>
    internal sealed class BCryptHashAlgorithm : IDisposable {
        [ThreadStatic]
        [SecurityCritical]
        private static BCryptAlgorithmHandleCache _algorithmCache;
        [SecurityCritical]
        private SafeBCryptAlgorithmHandle m_algorithmHandle;
        [SecurityCritical]
        private SafeBCryptHashHandle m_hashHandle;

        // SafeCritical - we're not exposing out anything that we want to prevent untrusted code from getting at
        [SecuritySafeCritical]
        public BCryptHashAlgorithm(CngAlgorithm algorithm, string implementation) {
            Contract.Requires(algorithm != null);
            Contract.Requires(!String.IsNullOrEmpty(implementation));
            Contract.Ensures(m_algorithmHandle != null && !m_algorithmHandle.IsInvalid && !m_algorithmHandle.IsClosed);
            Contract.Ensures(m_hashHandle != null && !m_hashHandle.IsInvalid && !m_hashHandle.IsClosed);

            // Make sure CNG is supported on this platform
            if (!BCryptNative.BCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            if (_algorithmCache == null)
            {
                _algorithmCache = new BCryptAlgorithmHandleCache();
            }

            m_algorithmHandle = _algorithmCache.GetCachedAlgorithmHandle(algorithm.Algorithm, implementation);

            Initialize();
        }

        /// <summary>
        ///     Clean up the hash algorithm
        /// </summary>
        [SecuritySafeCritical]
        public void Dispose() {
            Contract.Ensures(m_hashHandle == null || m_hashHandle.IsClosed);
            Contract.Ensures(m_algorithmHandle == null || m_algorithmHandle.IsClosed);

            if (m_hashHandle != null) {
                m_hashHandle.Dispose();
            }

            if (m_algorithmHandle != null) {
                m_algorithmHandle = null;
            }
        }

        /// <summary>
        ///     Reset the hash algorithm to begin hashing a new set of data
        /// </summary>
        // SafeCritical - we're not exposing out anything that we want to prevent untrusted code from getting
        //                at.  We've also made sure not to leak any native resources out to partial trust code
        //                and we control all native inputs.
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public void Initialize() {
            Contract.Ensures(m_hashHandle != null && !m_hashHandle.IsInvalid && !m_hashHandle.IsClosed);
            Contract.Assert(m_algorithmHandle != null);

            // Try to create a new hash algorithm to use
            SafeBCryptHashHandle newHashAlgorithm = null;
            IntPtr hashObjectBuffer = IntPtr.Zero;

            // Creating a BCRYPT_HASH_HANDLE requires providing a buffer to hold the hash object in, which
            // is tied to the lifetime of the hash handle. Wrap this in a CER so we can tie the lifetimes together
            // safely.
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                
                int hashObjectSize = BCryptNative.GetInt32Property(m_algorithmHandle,
                                                                   BCryptNative.ObjectPropertyName.ObjectLength);
                Debug.Assert(hashObjectSize > 0, "hashObjectSize > 0");

                // Allocate in a CER because we could fail between the alloc and the assignment
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally {
                    hashObjectBuffer = Marshal.AllocCoTaskMem(hashObjectSize);
                }

                BCryptNative.ErrorCode error = BCryptNative.UnsafeNativeMethods.BCryptCreateHash(m_algorithmHandle,
                                                                                                 out newHashAlgorithm,
                                                                                                 hashObjectBuffer,
                                                                                                 hashObjectSize,
                                                                                                 IntPtr.Zero,
                                                                                                 0,
                                                                                                 0);

                if (error != BCryptNative.ErrorCode.Success) {
                    throw new CryptographicException((int)error);
                }
            }
            finally {
                // Make sure we've successfully transfered ownership of the hash object buffer to the safe handle
                if (hashObjectBuffer != IntPtr.Zero) {
                    // If we created the safe handle, it needs to own the buffer and free it in release
                    if (newHashAlgorithm != null) {
                        newHashAlgorithm.HashObject = hashObjectBuffer;
                    }
                    else {
                        Marshal.FreeCoTaskMem(hashObjectBuffer);
                    }

                }

            }

            // If we could create it, dispose of any old hash handle we had and replace it with the new one
            if (m_hashHandle != null) {
                m_hashHandle.Dispose();
            }
            m_hashHandle = newHashAlgorithm;
        }

        /// <summary>
        ///     Hash a block of data
        /// </summary>
        [SecuritySafeCritical]
        public void HashCore(byte[] array, int ibStart, int cbSize) {
            Contract.Assert(m_hashHandle != null);

            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (ibStart < 0 || ibStart > array.Length - cbSize) {
                throw new ArgumentOutOfRangeException("ibStart");
            }
            if (cbSize < 0 || cbSize > array.Length) {
                throw new ArgumentOutOfRangeException("cbSize");
            }

            byte[] hashData = new byte[cbSize];
            Buffer.BlockCopy(array, ibStart, hashData, 0, cbSize);

            BCryptNative.ErrorCode error = BCryptNative.UnsafeNativeMethods.BCryptHashData(m_hashHandle,
                                                                                           hashData,
                                                                                           hashData.Length,
                                                                                           0);

            if (error != BCryptNative.ErrorCode.Success) {
                throw new CryptographicException((int)error);
            }
        }

        /// <summary>
        ///     Complete the hash, returning its value
        /// </summary>
        [SecuritySafeCritical]
        public byte[] HashFinal() {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Assert(m_hashHandle != null);

            int hashSize = BCryptNative.GetInt32Property(m_hashHandle, BCryptNative.HashPropertyName.HashLength);

            byte[] hashValue = new byte[hashSize];
            BCryptNative.ErrorCode error = BCryptNative.UnsafeNativeMethods.BCryptFinishHash(m_hashHandle,
                                                                                             hashValue,
                                                                                             hashValue.Length,
                                                                                             0);

            if (error != BCryptNative.ErrorCode.Success) {
                throw new CryptographicException((int)error);
            }

            return hashValue;
        }

        [SecuritySafeCritical]
        public void HashStream(Stream stream) {
            Contract.Requires(stream != null);

            // Read the data 4KB at a time, providing similar read characteristics to a standard HashAlgorithm
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            do {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0) {
                    HashCore(buffer, 0, bytesRead);
                }
            } while (bytesRead > 0);
        }
    }
}
