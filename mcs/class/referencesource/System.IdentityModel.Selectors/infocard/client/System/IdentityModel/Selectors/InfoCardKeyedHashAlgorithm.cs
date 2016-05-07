//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//
// Presharp uses the c# pragma mechanism to supress its warnings.
// These are not recognised by the base compiler so we need to explictly
// disable the following warnings. See http://winweb/cse/Tools/PREsharp/userguide/default.asp 
// for details. 
//
#pragma warning disable 1634, 1691      // unknown message, unknown pragma

namespace System.IdentityModel.Selectors
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;
    using DiagnosticUtility = Microsoft.InfoCards.Diagnostics.DiagnosticUtility;

    //
    // For common & resources
    //
    using Microsoft.InfoCards;


    //
    // Summary:
    //  Remotes a KeyedHashAlgorithm from the InfoCard service.
    //
    internal class InfoCardKeyedHashAlgorithm : KeyedHashAlgorithm
    {
        HashCryptoHandle m_cryptoHandle;
        RpcHashCryptoParameters m_param;
        byte[] m_cachedBlock;

        //
        // Summary:
        //  Creates a new InfoCardKeyedHashAlgorithm based on a SymmetricCryptoHandle.
        //
        // Parameters:
        //  cryptoHandle  - The handle to the symmetric key on which to base the keyed hash.
        //
        public InfoCardKeyedHashAlgorithm(SymmetricCryptoHandle cryptoHandle)
        {
            InternalRefCountedHandle nativeHandle = null;

            try
            {
                //
                // Call native api to get a hashCryptoHandle.
                //
                int status = CardSpaceSelector.GetShim().m_csShimGetKeyedHash(cryptoHandle.InternalHandle, out nativeHandle);

                if (0 != status)
                {
                    IDT.CloseInvalidOutSafeHandle(nativeHandle);
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw IDT.ThrowHelperError(new Win32Exception(status));
                }

                m_cryptoHandle = (HashCryptoHandle)CryptoHandle.Create(nativeHandle);

                m_param = (RpcHashCryptoParameters)m_cryptoHandle.Parameters;
            }
            catch
            {

                if (null != m_cryptoHandle)
                {
                    m_cryptoHandle.Dispose();
                }
                throw;
            }

        }
#pragma warning disable 56503 // property gets should not throw.
        public override byte[] Key
        {
            get { throw IDT.ThrowHelperError(new NotImplementedException()); }
        }

        public override int HashSize
        {
            get { return m_param.hashSize; }
        }

        public override int InputBlockSize
        {
            get { return m_param.transform.inputBlockSize; }
        }
        public override int OutputBlockSize
        {
            get { return m_param.transform.outputBlockSize; }
        }
        public override bool CanTransformMultipleBlocks
        {
            get { return m_param.transform.canTransformMultipleBlocks; }
        }

        public override bool CanReuseTransform
        {
            get { return m_param.transform.canReuseTransform; }
        }
#pragma warning restore 56503
        public override void Initialize()
        {
        }

        //
        // Summary:
        //  Implements the HashCore method of the KeyedHashAlgorithm class by calling the InfoCard native client.
        //
        // Parameters:
        //  array   - the bytes to hash.
        //  ibStart - the index in the array from which to begin hashing.
        //  cbSize  - the number of bytes after the starting index to hash.
        //
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            //
            // will cache one block and call TransformBlock on the previous block.
            //
            if (null != m_cachedBlock)
            {

                HGlobalSafeHandle pInData = null;
                try
                {
                    if (0 != m_cachedBlock.Length)
                    {
                        pInData = HGlobalSafeHandle.Construct(m_cachedBlock.Length);
                        Marshal.Copy(m_cachedBlock, 0, pInData.DangerousGetHandle(), m_cachedBlock.Length);
                    }

                    int status = CardSpaceSelector.GetShim().m_csShimHashCore(m_cryptoHandle.InternalHandle,
                                                         m_cachedBlock.Length,
                                                         null != pInData ? pInData : HGlobalSafeHandle.Construct());

                    if (0 != status)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }

                }
                finally
                {
                    if (null != pInData)
                    {
                        pInData.Dispose();
                    }
                }

            }

            //
            // Cache the current block.
            //
            if (null != m_cachedBlock)
            {
                Array.Clear(m_cachedBlock, 0, m_cachedBlock.Length);
            }
            m_cachedBlock = DiagnosticUtility.Utility.AllocateByteArray(cbSize);
            Array.Copy(array, ibStart, m_cachedBlock, 0, cbSize);

            return;
        }

        //
        // Summary:
        //  Implements the HashFinal method of the KeyedHashAlgorithm class by calling the InfoCard native client.
        //
        protected override byte[] HashFinal()
        {
            byte[] outData = null;
            int cbOutData = 0;
            IDT.DebugAssert(null != m_cachedBlock, "null cached block");

            HGlobalSafeHandle pInData = null;
            GlobalAllocSafeHandle pOutData = null;
            try
            {
                if (null != m_cachedBlock)
                {

                    if (0 != m_cachedBlock.Length)
                    {
                        pInData = HGlobalSafeHandle.Construct(m_cachedBlock.Length);
                        Marshal.Copy(m_cachedBlock, 0, pInData.DangerousGetHandle(), m_cachedBlock.Length);
                    }

                    int status = CardSpaceSelector.GetShim().m_csShimHashFinal(m_cryptoHandle.InternalHandle,
                                                         m_cachedBlock.Length,
                                                         null != pInData ? pInData : HGlobalSafeHandle.Construct(),
                                                         out cbOutData,
                                                         out pOutData);
                    if (0 != status)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }
                    pOutData.Length = cbOutData;
                    outData = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                    using (pOutData)
                    {
                        Marshal.Copy(pOutData.DangerousGetHandle(), outData, 0, pOutData.Length);
                    }
                }
            }
            finally
            {
                if (null != pInData)
                {
                    pInData.Dispose();
                }
                Array.Clear(m_cachedBlock, 0, m_cachedBlock.Length);
                m_cachedBlock = null;
            }

            return outData;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (null != m_cachedBlock)
                {
                    Array.Clear(m_cachedBlock, 0, m_cachedBlock.Length);
                }
                m_cryptoHandle.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
