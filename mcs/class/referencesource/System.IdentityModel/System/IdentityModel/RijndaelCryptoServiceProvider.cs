//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    class RijndaelCryptoServiceProvider : Rijndael
    {
        public RijndaelCryptoServiceProvider()
        {
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbKey");
            if (rgbIV == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbIV");
            if (this.ModeValue != CipherMode.CBC)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AESCipherModeNotSupported, this.ModeValue)));

            return new RijndaelCryptoTransform(rgbKey, rgbIV, this.PaddingValue, this.BlockSizeValue, true);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbKey");
            if (rgbIV == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbIV");
            if (this.ModeValue != CipherMode.CBC)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AESCipherModeNotSupported, this.ModeValue)));

            return new RijndaelCryptoTransform(rgbKey, rgbIV, this.PaddingValue, this.BlockSizeValue, false);
        }

        public override void GenerateKey()
        {
            this.KeyValue = new byte[this.KeySizeValue / 8];
            CryptoHelper.RandomNumberGenerator.GetBytes(this.KeyValue);
        }

        public override void GenerateIV()
        {
            // IV is always 16 bytes/128 bits because block size is always 128 bits
            this.IVValue = new byte[this.BlockSizeValue / 8];
            CryptoHelper.RandomNumberGenerator.GetBytes(this.IVValue);
        }

        class RijndaelCryptoTransform : ICryptoTransform
        {
            SafeProvHandle provHandle = SafeProvHandle.InvalidHandle;
            SafeKeyHandle keyHandle = SafeKeyHandle.InvalidHandle;
            PaddingMode paddingMode;
            byte[] depadBuffer = null;
            int blockSize;
            bool encrypt;

            public unsafe RijndaelCryptoTransform(byte[] rgbKey, byte[] rgbIV, PaddingMode paddingMode, int blockSizeBits, bool encrypt)
            {
                if (rgbKey.Length != 16 && rgbKey.Length != 24 && rgbKey.Length != 32)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AESKeyLengthNotSupported, rgbKey.Length * 8)));
                if (rgbIV.Length != 16)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AESIVLengthNotSupported, rgbIV.Length * 8)));
                if (paddingMode != PaddingMode.PKCS7 && paddingMode != PaddingMode.ISO10126)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AESPaddingModeNotSupported, paddingMode)));

                this.paddingMode = paddingMode;
                DiagnosticUtility.DebugAssert((blockSizeBits % 8) == 0, "Bits must be byte aligned.");
                this.blockSize = blockSizeBits / 8;
                this.encrypt = encrypt;

                SafeProvHandle provHandle = null;
                SafeKeyHandle keyHandle = null;
                try
                {
#pragma warning suppress 56523
                    ThrowIfFalse(SR.AESCryptAcquireContextFailed, NativeMethods.CryptAcquireContextW(out provHandle, null, null, NativeMethods.PROV_RSA_AES, NativeMethods.CRYPT_VERIFYCONTEXT));

                    // (BLOBHEADER + keyLen) + Key
                    int cbData = PLAINTEXTKEYBLOBHEADER.SizeOf + rgbKey.Length;
                    byte[] pbData = new byte[cbData];
                    Buffer.BlockCopy(rgbKey, 0, pbData, PLAINTEXTKEYBLOBHEADER.SizeOf, rgbKey.Length);
                    fixed (void* pbDataPtr = &pbData[0])
                    {
                        PLAINTEXTKEYBLOBHEADER* pbhdr = (PLAINTEXTKEYBLOBHEADER*)pbDataPtr;
                        pbhdr->bType = NativeMethods.PLAINTEXTKEYBLOB;
                        pbhdr->bVersion = NativeMethods.CUR_BLOB_VERSION;
                        pbhdr->reserved = 0;
                        if (rgbKey.Length == 16)
                            pbhdr->aiKeyAlg = NativeMethods.CALG_AES_128;
                        else if (rgbKey.Length == 24)
                            pbhdr->aiKeyAlg = NativeMethods.CALG_AES_192;
                        else
                            pbhdr->aiKeyAlg = NativeMethods.CALG_AES_256;
                        pbhdr->keyLength = rgbKey.Length;

                        keyHandle = SafeKeyHandle.SafeCryptImportKey(provHandle, pbDataPtr, cbData);
                    }
#if DEBUG
                    uint ivLen = 0;
#pragma warning suppress 56523 // win32 error checked in ThrowIfFalse() method
                    ThrowIfFalse(SR.AESCryptGetKeyParamFailed, NativeMethods.CryptGetKeyParam(keyHandle, NativeMethods.KP_IV, IntPtr.Zero, ref ivLen, 0));
                    DiagnosticUtility.DebugAssert(rgbIV.Length == ivLen, "Mismatch iv size");
#endif
                    fixed (void* pbIVPtr = &rgbIV[0])
                    {
#pragma warning suppress 56523
                        ThrowIfFalse(SR.AESCryptSetKeyParamFailed, NativeMethods.CryptSetKeyParam(keyHandle, NativeMethods.KP_IV, pbIVPtr, 0));
                    }

                    // Save
                    this.keyHandle = keyHandle;
                    this.provHandle = provHandle;
                    keyHandle = null;
                    provHandle = null;
                }
                finally
                {
                    if (keyHandle != null)
                        keyHandle.Close();
                    if (provHandle != null)
                        provHandle.Close();
                }
            }

            public bool CanReuseTransform  
            { 
                get { return true; } 
            }

            public bool CanTransformMultipleBlocks 
            { 
                get { return true; } 
            }

            public int InputBlockSize 
            { 
                get { return this.blockSize; } 
            }

            public int OutputBlockSize
            { 
                get { return this.blockSize; } 
            }

            public void Dispose()
            {
                try
                {
                    this.keyHandle.Close();
                }
                finally
                {
                    this.provHandle.Close();
                }
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (inputBuffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputBuffer");
                if (outputBuffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputBuffer");
                if (inputOffset < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (inputCount <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputCount", SR.GetString(SR.ValueMustBeGreaterThanZero)));
                if (outputOffset < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputOffset", SR.GetString(SR.ValueMustBeNonNegative)));
                if ((inputCount % this.blockSize) != 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.AESInvalidInputBlockSize, inputCount, this.blockSize)));
                if ((inputBuffer.Length - inputCount) < inputOffset)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", SR.GetString(SR.ValueMustBeInRange, 0, inputBuffer.Length - inputCount - 1)));
                if (outputBuffer.Length < outputOffset)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputOffset", SR.GetString(SR.ValueMustBeInRange, 0, outputBuffer.Length - 1)));

                if (this.encrypt)
                {
                    return EncryptData(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset, false);
                }
                else
                {
                    if (this.paddingMode == PaddingMode.PKCS7)
                    {
                        return DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset, false);
                    }
                    else
                    {
                        // OK, now we're in the special case.  Check to see if this is the *first* block we've seen
                        // If so, buffer it and return null zero bytes
                        if (this.depadBuffer == null)
                        {
                            this.depadBuffer = new byte[this.blockSize];
                            // copy the last InputBlockSize bytes to m_depadBuffer everything else gets processed and returned
                            int inputToProcess = inputCount - this.blockSize;
                            Buffer.BlockCopy(inputBuffer, inputOffset + inputToProcess, this.depadBuffer, 0, this.blockSize);
                            return ((inputToProcess <= 0) ? 0 : DecryptData(inputBuffer, inputOffset, inputToProcess, outputBuffer, outputOffset, false));
                        }
                        else
                        {
                            // we already have a depad buffer, so we need to decrypt that info first & copy it out
                            int dwCount = DecryptData(this.depadBuffer, 0, this.depadBuffer.Length, outputBuffer, outputOffset, false);
                            outputOffset += dwCount;
                            int inputToProcess = inputCount - this.blockSize;
                            Buffer.BlockCopy(inputBuffer, inputOffset + inputToProcess, this.depadBuffer, 0, this.blockSize);
                            return dwCount + ((inputToProcess <= 0) ? 0 : DecryptData(inputBuffer, inputOffset, inputToProcess, outputBuffer, outputOffset, false));
                        }
                    }
                }
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputBuffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputBuffer");
                if (inputOffset < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (inputCount < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputCount", SR.GetString(SR.ValueMustBeNonNegative)));
                if ((inputBuffer.Length - inputCount) < inputOffset)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", SR.GetString(SR.ValueMustBeInRange, 0, inputBuffer.Length - inputCount - 1)));

                if (this.encrypt)
                {
                    int padding = this.blockSize - (inputCount % this.blockSize);
                    int outputCount = inputCount + padding;
                    if (this.paddingMode == PaddingMode.ISO10126)
                        outputCount += this.blockSize;

                    byte[] outputBuffer = new byte[outputCount];
                    int dwCount = EncryptData(inputBuffer, inputOffset, inputCount, outputBuffer, 0, true);
                    return TruncateBuffer(outputBuffer, dwCount);
                }
                else
                {
                    if (this.paddingMode == PaddingMode.PKCS7)
                    {
                        byte[] outputBuffer = new byte[inputCount];
                        int dwCount = DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, 0, true);
                        return TruncateBuffer(outputBuffer, dwCount);
                    }
                    else
                    {
                        // OK, now we're in the special case.  Check to see if this is the *first* block we've seen
                        // If so, buffer it and return null zero bytes
                        if (this.depadBuffer == null)
                        {
                            byte[] outputBuffer = new byte[inputCount];
                            int dwCount = DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, 0, true);
                            return TruncateBuffer(outputBuffer, dwCount);
                        }
                        else
                        {
                            byte[] outputBuffer = new byte[this.depadBuffer.Length + inputCount];
                            // we already have a depad buffer, so we need to decrypt that info first & copy it out
                            int dwCount = DecryptData(this.depadBuffer, 0, this.depadBuffer.Length, outputBuffer, 0, false);
                            dwCount += DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, dwCount, true);
                            return TruncateBuffer(outputBuffer, dwCount);
                        }
                    }
                }
            }

            unsafe int EncryptData(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, bool final)
            {
                if ((outputBuffer.Length - outputOffset) < inputCount)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputBuffer", SR.GetString(SR.AESInsufficientOutputBuffer, outputBuffer.Length - outputOffset, inputCount)));

                bool doPadding = final && (this.paddingMode == PaddingMode.ISO10126);
                byte[] tempBuffer = outputBuffer;
                int tempOffset = outputOffset;
                int dwCount = inputCount;
                bool throwing = true;
                Buffer.BlockCopy(inputBuffer, inputOffset, tempBuffer, tempOffset, inputCount);
                try
                {
                    if (doPadding)
                        DoPadding(ref tempBuffer, ref tempOffset, ref dwCount);

                    fixed (void* tempBufferPtr = &tempBuffer[tempOffset])
                    {
#pragma warning suppress 56523
                        ThrowIfFalse(SR.AESCryptEncryptFailed, NativeMethods.CryptEncrypt(keyHandle, IntPtr.Zero, final, 0, tempBufferPtr, ref dwCount, tempBuffer.Length - tempOffset));
                    }
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                        Array.Clear(tempBuffer, tempOffset, inputCount);
                }

                // Chop off native padding.
                if (doPadding)
                    dwCount -= this.blockSize;
                if (tempBuffer != outputBuffer)
                    Buffer.BlockCopy(tempBuffer, tempOffset, outputBuffer, outputOffset, dwCount);

                return dwCount;
            }

            unsafe int DecryptData(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, bool final)
            {
                bool bFinal = final && (this.paddingMode == PaddingMode.PKCS7);
                int dwCount = inputCount;
                if (dwCount > 0)
                {
                    Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                    fixed (void* outputBufferPtr = &outputBuffer[outputOffset])
                    {
#pragma warning suppress 56523
                        ThrowIfFalse(SR.AESCryptDecryptFailed, NativeMethods.CryptDecrypt(keyHandle, IntPtr.Zero, bFinal, 0, outputBufferPtr, ref dwCount));
                    }
                }

                if (!bFinal && final)
                {
                    byte padSize = outputBuffer[outputOffset + dwCount - 1];
                    DiagnosticUtility.DebugAssert(padSize <= this.blockSize, "Invalid padding size.");
                    dwCount -= padSize;
                }
                return dwCount;
            }

            // Since the CSP only provides PKCS7 padding. For other padding, we do it manually.
            void DoPadding(ref byte[] tempBuffer, ref int tempOffset, ref int dwCount)
            {
                int lonelyBytes = dwCount % this.blockSize;
                int padSize = this.blockSize - lonelyBytes;

                // Random with last byte indicating padSize
                byte[] padBytes = new byte[padSize];
                CryptoHelper.RandomNumberGenerator.GetBytes(padBytes);
                padBytes[padSize - 1] = (byte)padSize;

                // inline if can hold manual padding and native padding (1 block)
                int requiredSize = dwCount + padSize + this.blockSize;
                if (tempBuffer.Length >= (tempOffset + requiredSize))
                {
                    Buffer.BlockCopy(padBytes, 0, tempBuffer, tempOffset + dwCount, padSize);
                }
                else
                {
                    byte[] ret = new byte[requiredSize];
                    Buffer.BlockCopy(tempBuffer, tempOffset, ret, 0, dwCount);
                    Buffer.BlockCopy(padBytes, 0, ret, dwCount, padSize);
                    Array.Clear(tempBuffer, tempOffset, dwCount);
                    tempBuffer = ret;
                    tempOffset = 0;
                }
                dwCount += padSize;
            }

            byte[] TruncateBuffer(byte[] buffer, int len)
            {
                if (len == buffer.Length)
                    return buffer;

                // Truncate
                byte[] tempBuffer = new byte[len];
                Buffer.BlockCopy(buffer, 0, tempBuffer, 0, len);
                if (!this.encrypt)
                    Array.Clear(buffer, 0, buffer.Length);
                return tempBuffer;
            }

            static void ThrowIfFalse(string sr, bool ret)
            {
                if (!ret)
                {
                    int err = Marshal.GetLastWin32Error();
                    string reason = (err != 0) ? new Win32Exception(err).Message : String.Empty;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(sr, reason)));
                }
            }
        }
    }
}
