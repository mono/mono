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
    using System.Security.Cryptography.Xml;
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
    //  The InfoCard remoted version of a SymmetricAlgorithm.  Allows limited access to a symmetric key owned by
    //  the infocard service.
    //
    internal class InfoCardSymmetricAlgorithm : SymmetricAlgorithm, IDisposable
    {

        //
        // Used to generate intialization vectors.
        //
        static readonly RandomNumberGenerator random = new RNGCryptoServiceProvider();

        SymmetricCryptoHandle m_cryptoHandle;
        RpcSymmetricCryptoParameters m_parameters;

        //
        // Summary:
        //  Constructs an InfoCardSymmetricAlgorithm
        //
        // Parameters:
        //  cryptoHandle  - A handle to the symmetric key to base the symmetric algorithm on.
        //
        public InfoCardSymmetricAlgorithm(SymmetricCryptoHandle cryptoHandle)
            : base()
        {
            m_cryptoHandle = (SymmetricCryptoHandle)cryptoHandle.Duplicate();

            try
            {
                m_parameters = (RpcSymmetricCryptoParameters)m_cryptoHandle.Parameters;

                KeySizeValue = m_parameters.keySize;
                BlockSizeValue = m_parameters.blockSize;
                FeedbackSizeValue = m_parameters.feedbackSize;
                LegalBlockSizesValue = new KeySizes[] { new KeySizes(BlockSizeValue, BlockSizeValue, 0) };
                LegalKeySizesValue = new KeySizes[] { new KeySizes(KeySizeValue, KeySizeValue, 0) };
            }
            catch
            {
                m_cryptoHandle.Dispose();
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
#pragma warning disable 56503       // do not throw from property getters.
        public override byte[] Key
        {
            get
            {
                throw IDT.ThrowHelperError(new NotImplementedException());
            }
            set
            {
                throw IDT.ThrowHelperError(new NotImplementedException());
            }
        }
#pragma warning restore 56503

        //
        // public methods
        //

        public override ICryptoTransform CreateEncryptor()
        {
            return new CryptoTransform(this, CryptoTransform.Direction.Encrypt);
        }

        //
        // We don't allow specifying a key so this is not supported.
        //
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return new CryptoTransform(this, CryptoTransform.Direction.Decrypt);
        }

        //
        // We don't allow specifying a key so this is not supported.
        //
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        public override void GenerateKey()
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        public override void GenerateIV()
        {
            byte[] ivvalue = new byte[BlockSize / 8];

            random.GetBytes(ivvalue);

            IVValue = ivvalue;
        }

        //
        // Summary:
        //  Implements the ICryptoTransform interface based on an instance of an InfoCardSymmetricAlgorithm.
        //
        private class CryptoTransform : ICryptoTransform
        {
            public enum Direction
            {
                Encrypt = 1,
                Decrypt = 2
            };

            TransformCryptoHandle m_transCryptoHandle;
            RpcTransformCryptoParameters m_param;

            //
            // Parameters:
            //  symAlgo  - the algorithm being requested.
            //  cryptoDirection - determines whether the transform will encrypt or decrypt.
            //
            public CryptoTransform(InfoCardSymmetricAlgorithm symAlgo, Direction cryptoDirection)
            {
                InternalRefCountedHandle nativeHandle = null;
                byte[] iv = symAlgo.IV;
                using (HGlobalSafeHandle pIV = HGlobalSafeHandle.Construct(iv.Length))
                {
                    //
                    // Marshal the initialization vector.
                    //
                    Marshal.Copy(iv, 0, pIV.DangerousGetHandle(), iv.Length);

                    //
                    // Call native method to get a handle to a native transform.
                    //
                    int status = CardSpaceSelector.GetShim().m_csShimGetCryptoTransform(symAlgo.m_cryptoHandle.InternalHandle,
                                                                   (int)symAlgo.Mode,
                                                                   (int)symAlgo.Padding,
                                                                   symAlgo.FeedbackSize,
                                                                   (int)cryptoDirection,
                                                                   iv.Length,
                                                                   pIV,
                                                                   out nativeHandle);

                    if (0 != status)
                    {
                        IDT.CloseInvalidOutSafeHandle(nativeHandle);
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }

                    m_transCryptoHandle = (TransformCryptoHandle)CryptoHandle.Create(nativeHandle);

                    m_param = (RpcTransformCryptoParameters)m_transCryptoHandle.Parameters;
                }

            }

            public int InputBlockSize
            {
                get { return m_param.inputBlockSize; }
            }

            public int OutputBlockSize
            {
                get { return m_param.outputBlockSize; }
            }

            public bool CanTransformMultipleBlocks
            {
                get { return m_param.canTransformMultipleBlocks; }
            }

            public bool CanReuseTransform
            {
                get { return m_param.canReuseTransform; }
            }

            //
            // Summary:
            //  The return value of TransformBlock is the number of bytes returned to outputBuffer and is
            //  always <= OutputBlockSize.  If CanTransformMultipleBlocks is true, then inputCount may be
            //  any positive multiple of InputBlockSize
            //
            // Parameters:
            //  inputBuffer - The input for which to compute the transform.
            //  inputOffset - The offset into the input byte array from which to begin using data.
            //  outputBuffer - The output to which to write the transform.
            //  outputOffset - The offset into the output byte array from which to begin writing data.
            //
            // Returns:
            //  The number of bytes written.
            //
            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                GlobalAllocSafeHandle pOutData = null;
                int cbOutData = 0;
                IDT.DebugAssert(null != inputBuffer && 0 != inputBuffer.Length, "null input buffer");
                IDT.DebugAssert(0 != inputCount, "0 input count");
                using (HGlobalSafeHandle pInData = HGlobalSafeHandle.Construct(inputCount))
                {
                    Marshal.Copy(inputBuffer, inputOffset, pInData.DangerousGetHandle(), inputCount);
                    int status = CardSpaceSelector.GetShim().m_csShimTransformBlock(m_transCryptoHandle.InternalHandle,
                                                               inputCount,
                                                               pInData,
                                                               out cbOutData,
                                                               out pOutData);

                    if (0 != status)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }
                    pOutData.Length = cbOutData;
                    using (pOutData)
                    {
                        Marshal.Copy(pOutData.DangerousGetHandle(), outputBuffer, outputOffset, pOutData.Length);
                    }
                }

                return cbOutData;
            }

            //
            // Summary:
            //  Special function for transforming the last block or partial block in the stream.  The
            //  return value is an array containting the remaining transformed bytes.
            //  We return a new array here because the amount of information we send back at the end could
            //  be larger than a single block once padding is accounted for.
            //
            // Parameters:
            //  inputBuffer  - The input for which to compute the transform.
            //  inputOffset  - The offset into the byte array from which to begin using data.
            //  inputCount   - The number of bytes in the byte array to use as data.
            //
            // Returns:
            //  The computed transform.
            //
            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                IDT.DebugAssert(null != inputBuffer && 0 != inputBuffer.Length, "null input buffer");
                IDT.DebugAssert(0 != inputCount, "0 input count");
                GlobalAllocSafeHandle pOutData = null;
                int cbOutData = 0;
                byte[] outData;

                using (HGlobalSafeHandle pInData = HGlobalSafeHandle.Construct(inputCount))
                {
                    Marshal.Copy(inputBuffer, inputOffset, pInData.DangerousGetHandle(), inputCount);

                    int status = CardSpaceSelector.GetShim().m_csShimTransformFinalBlock(m_transCryptoHandle.InternalHandle,
                                                                    inputCount,
                                                                    pInData,
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

                return outData;
            }

            public void Dispose()
            {
                if (null != m_transCryptoHandle)
                {
                    m_transCryptoHandle.Dispose();
                    m_transCryptoHandle = null;
                }
            }
        }
    }
}
