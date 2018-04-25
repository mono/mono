// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
#if FEATURE_CORESYSTEM
using System.Core;
#endif
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Flag to indicate if we're doing encryption or decryption
    /// </summary>
    internal enum EncryptionMode {
        Encrypt,
        Decrypt
    }

    /// <summary>
    ///     Implementation of a generic CAPI symmetric encryption algorithm. Concrete SymmetricAlgorithm classes
    ///     which wrap CAPI implementations can use this class to perform the actual encryption work.
    /// </summary>
    internal sealed class CapiSymmetricAlgorithm : ICryptoTransform {
        private int m_blockSize;
        private byte[] m_depadBuffer;
        private EncryptionMode m_encryptionMode;
        [SecurityCritical]
        private SafeCapiKeyHandle m_key;
        private PaddingMode m_paddingMode;
        [SecurityCritical]
        private SafeCspHandle m_provider;

        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public CapiSymmetricAlgorithm(int blockSize,
                                      int feedbackSize,
                                      SafeCspHandle provider,
                                      SafeCapiKeyHandle key,
                                      byte[] iv,
                                      CipherMode cipherMode,
                                      PaddingMode paddingMode,
                                      EncryptionMode encryptionMode) {
            Contract.Requires(0 < blockSize && blockSize % 8 == 0);
            Contract.Requires(0 <= feedbackSize);
            Contract.Requires(provider != null && !provider.IsInvalid && !provider.IsClosed);
            Contract.Requires(key != null && !key.IsInvalid && !key.IsClosed);
            Contract.Ensures(m_provider != null && !m_provider.IsInvalid && !m_provider.IsClosed);
            
            m_blockSize = blockSize;
            m_encryptionMode = encryptionMode;
            m_paddingMode = paddingMode;
            m_provider = provider.Duplicate();
            m_key = SetupKey(key, ProcessIV(iv, blockSize, cipherMode), cipherMode, feedbackSize);
        }

        public bool CanReuseTransform {
            get { return true; }
        }

        public bool CanTransformMultipleBlocks {
            get { return true; }
        }

        //
        // Note: both input and output block size are in bytes rather than bits
        //

        public int InputBlockSize {
            [Pure]
            get { return m_blockSize / 8; }
        }

        public int OutputBlockSize {
            get { return m_blockSize / 8; }
        }

        [SecuritySafeCritical]
        public void Dispose() {
            Contract.Ensures(m_key == null || m_key.IsClosed);
            Contract.Ensures(m_provider == null || m_provider.IsClosed);
            Contract.Ensures(m_depadBuffer == null);

            if (m_key != null) {
                m_key.Dispose();
            }

            if (m_provider != null) {
                m_provider.Dispose();
            }

            if (m_depadBuffer != null) {
                Array.Clear(m_depadBuffer, 0, m_depadBuffer.Length);
            }

            return;
        }

        [SecuritySafeCritical]
        private int DecryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            Contract.Requires(m_key != null);
            Contract.Requires(inputBuffer != null && inputCount <= inputBuffer.Length - inputOffset);
            Contract.Requires(inputOffset >= 0);
            Contract.Requires(inputCount > 0 && inputCount % InputBlockSize == 0);
            Contract.Requires(outputBuffer != null && inputCount <= outputBuffer.Length - outputOffset);
            Contract.Requires(inputOffset >= 0);
            Contract.Requires(m_depadBuffer == null || (m_paddingMode != PaddingMode.None && m_paddingMode != PaddingMode.Zeros));
            Contract.Ensures(Contract.Result<int>() >= 0);

            //
            // If we're decrypting, it's possible to be called with the last blocks of the data, and then
            // have TransformFinalBlock called with an empty array. Since we don't know if this is the case,
            // we won't decrypt the last block of the input until either TransformBlock or
            // TransformFinalBlock is next called.
            //
            // We don't need to do this for PaddingMode.None because there is no padding to strip, and
            // we also don't do this for PaddingMode.Zeros since there is no way for us to tell if the
            // zeros at the end of a block are part of the plaintext or the padding.
            //

            int decryptedBytes = 0;
            if (m_paddingMode != PaddingMode.None && m_paddingMode != PaddingMode.Zeros) {
                // If we have data saved from a previous call, decrypt that into the output first
                if (m_depadBuffer != null) {
                    int depadDecryptLength = RawDecryptBlocks(m_depadBuffer, 0, m_depadBuffer.Length);
                    Buffer.BlockCopy(m_depadBuffer, 0, outputBuffer, outputOffset, depadDecryptLength);
                    Array.Clear(m_depadBuffer, 0, m_depadBuffer.Length);
                    outputOffset += depadDecryptLength;
                    decryptedBytes += depadDecryptLength;
                }
                else {
                    m_depadBuffer = new byte[InputBlockSize];
                }

                // Copy the last block of the input buffer into the depad buffer
                Debug.Assert(inputCount >= m_depadBuffer.Length, "inputCount >= m_depadBuffer.Length");
                Buffer.BlockCopy(inputBuffer,
                                 inputOffset + inputCount - m_depadBuffer.Length,
                                 m_depadBuffer,
                                 0,
                                 m_depadBuffer.Length);
                inputCount -= m_depadBuffer.Length;
                Debug.Assert(inputCount % InputBlockSize == 0, "Did not remove whole blocks for depadding");
            }

            // CryptDecrypt operates in place, so if after reserving the depad buffer there's still data to decrypt,
            // make a copy of that in the output buffer to work on.
            if (inputCount > 0) {
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                decryptedBytes += RawDecryptBlocks(outputBuffer, outputOffset, inputCount);
            }

            return decryptedBytes;
        }

        /// <summary>
        ///     Remove the padding from the last blocks being decrypted
        /// </summary>
        private byte[] DepadBlock(byte[] block, int offset, int count) {
            Contract.Requires(block != null && count >= block.Length - offset);
            Contract.Requires(0 <= offset);
            Contract.Requires(0 <= count);
            Contract.Ensures(Contract.Result<byte[]>() != null && Contract.Result<byte[]>().Length <= block.Length);

            int padBytes = 0;

            // See code:System.Security.Cryptography.CapiSymmetricAlgorithm.PadBlock for a description of the
            // padding modes.
            switch (m_paddingMode) {
                case PaddingMode.ANSIX923:
                    padBytes = block[offset + count - 1];

                    // Verify the amount of padding is reasonable
                    if (padBytes <= 0 || padBytes > InputBlockSize) {
                        throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidPadding));
                    }

                    // Verify that all the padding bytes are 0s
                    for (int i = offset + count - padBytes; i < offset + count - 1; i++) {
                        if (block[i] != 0) {
                            throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidPadding));
                        }
                    }

                    break;

                case PaddingMode.ISO10126:
                    padBytes = block[offset + count - 1];

                    // Verify the amount of padding is reasonable
                    if (padBytes <= 0 || padBytes > InputBlockSize) {
                        throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidPadding));
                    }

                    // Since the padding consists of random bytes, we cannot verify the actual pad bytes themselves
                    break;

                case PaddingMode.PKCS7:
                    padBytes = block[offset + count - 1];

                    // Verify the amount of padding is reasonable
                    if (padBytes <= 0 || padBytes > InputBlockSize) {
                        throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidPadding));
                    }

                    // Verify all the padding bytes match the amount of padding
                    for (int i = offset + count - padBytes; i < offset + count; i++) {
                        if (block[i] != padBytes) {
                            throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidPadding));
                        }
                    }

                    break;

                    // We cannot remove Zeros padding because we don't know if the zeros at the end of the block
                    // belong to the padding or the plaintext itself.
                case PaddingMode.Zeros:
                case PaddingMode.None:
                    padBytes = 0;
                    break;

                default:
                    throw new CryptographicException(SR.GetString(SR.Cryptography_UnknownPaddingMode));
            }

            // Copy everything but the padding to the output
            byte[] depadded = new byte[count - padBytes];
            Buffer.BlockCopy(block, offset, depadded, 0, depadded.Length);
            return depadded;
        }

        /// <summary>
        ///     Encrypt blocks of plaintext
        /// </summary>
        [SecurityCritical]
        private int EncryptBlocks(byte[] buffer, int offset, int count) {
            Contract.Requires(m_key != null);
            Contract.Requires(buffer != null && count <= buffer.Length - offset);
            Contract.Requires(offset >= 0);
            Contract.Requires(count > 0 && count % InputBlockSize == 0);
            Contract.Ensures(Contract.Result<int>() >= 0);

            //
            // Do the encryption. Note that CapiSymmetricAlgorithm will do all padding itself since the CLR
            // supports padding modes that CAPI does not, so we will always tell CAPI that we are not working
            // with the final block.
            //

            int dataLength = count;
            unsafe {
                fixed (byte* pData = &buffer[offset]) {
                    if (!CapiNative.UnsafeNativeMethods.CryptEncrypt(m_key,
                                                                     SafeCapiHashHandle.InvalidHandle,
                                                                     false,
                                                                     0,
                                                                     new IntPtr(pData),
                                                                     ref dataLength,
                                                                     buffer.Length - offset)) {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }

            return dataLength;
        }

        /// <summary>
        ///     Calculate the padding for a block of data
        /// </summary>
        [SecuritySafeCritical]
        private byte[] PadBlock(byte[] block, int offset, int count) {
            Contract.Requires(m_provider != null);
            Contract.Requires(block != null && count <= block.Length - offset);
            Contract.Requires(0 <= offset);
            Contract.Requires(0 <= count);
            Contract.Ensures(Contract.Result<byte[]>() != null && Contract.Result<byte[]>().Length % InputBlockSize == 0);

            byte[] result = null;
            int padBytes = InputBlockSize - (count % InputBlockSize);
            
            switch (m_paddingMode) {
                    // ANSI padding fills the blocks with zeros and adds the total number of padding bytes as
                    // the last pad byte, adding an extra block if the last block is complete.
                    //
                    // x 00 00 00 00 00 00 07
                case PaddingMode.ANSIX923:
                    result = new byte[count + padBytes];
                    Buffer.BlockCopy(block, 0, result, 0, count);
                    result[result.Length - 1] = (byte)padBytes;
                    break;

                    // ISO padding fills the blocks up with random bytes and adds the total number of padding
                    // bytes as the last pad byte, adding an extra block if the last block is complete.
                    //
                    // xx rr rr rr rr rr rr 07
                case PaddingMode.ISO10126:
                    result = new byte[count + padBytes];
                    
                    CapiNative.UnsafeNativeMethods.CryptGenRandom(m_provider, result.Length - 1, result);
                    Buffer.BlockCopy(block, 0, result, 0, count);
                    result[result.Length - 1] = (byte)padBytes;
                    break;

                    // No padding requires that the input already be a multiple of the block size
                case PaddingMode.None:
                    if (count % InputBlockSize != 0) {
                        throw new CryptographicException(SR.GetString(SR.Cryptography_PartialBlock));
                    }

                    result = new byte[count];
                    Buffer.BlockCopy(block, offset, result, 0, result.Length);
                    break;

                    // PKCS padding fills the blocks up with bytes containing the total number of padding bytes
                    // used, adding an extra block if the last block is complete.
                    //
                    // xx xx 06 06 06 06 06 06
                case PaddingMode.PKCS7:
                    result = new byte[count + padBytes];
                    Buffer.BlockCopy(block, offset, result, 0, count);

                    for (int i = count; i < result.Length; i++) {
                        result[i] = (byte)padBytes;
                    }
                    break;

                    // Zeros padding fills the last partial block with zeros, and does not add a new block to
                    // the end if the last block is already complete.
                    //
                    //  xx 00 00 00 00 00 00 00
                case PaddingMode.Zeros:
                    if (padBytes == InputBlockSize) {
                        padBytes = 0;
                    }

                    result = new byte[count + padBytes];
                    Buffer.BlockCopy(block, offset, result, 0, count);
                    break;

                default:
                    throw new CryptographicException(SR.GetString(SR.Cryptography_UnknownPaddingMode));
            }

            return result;
        }

        /// <summary>
        ///     Validate and transform the user's IV into one that we will pass on to CAPI
        ///
        ///     If we have an IV, make a copy of it so that it doesn't get modified while we're using it. If
        ///     not, and we're not in ECB mode then throw an error, since we cannot decrypt without the IV, and
        ///     generating a random IV to encrypt with would lead to data which is not decryptable.
        ///
        ///     For compatibility with v1.x, we accept IVs which are longer than the block size, and truncate
        ///     them back.  We will reject an IV which is smaller than the block size however.
        /// </summary>
        private static byte[] ProcessIV(byte[] iv, int blockSize, CipherMode cipherMode) {
            Contract.Requires(blockSize % 8 == 0);
            Contract.Ensures(cipherMode == CipherMode.ECB ||
                             (Contract.Result<byte[]>() != null && Contract.Result<byte[]>().Length == blockSize / 8));

            byte[] realIV = null;

            if (iv != null) {
                if (blockSize / 8 <= iv.Length) {
                    realIV = new byte[blockSize / 8];
                    Buffer.BlockCopy(iv, 0, realIV, 0, realIV.Length);
                }
                else {
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidIVSize));
                }
            }
            else if (cipherMode != CipherMode.ECB) {
                throw new CryptographicException(SR.GetString(SR.Cryptography_MissingIV));
            }

            return realIV;
        }

        /// <summary>
        ///     Do a direct decryption of the ciphertext blocks. This method should not be called from anywhere
        ///     but DecryptBlocks or TransformFinalBlock since it does not account for the depadding buffer and
        ///     direct use could lead to incorrect decryption values.
        /// </summary>
        [SecurityCritical]
        private int RawDecryptBlocks(byte[] buffer, int offset, int count) {
            Contract.Requires(m_key != null);
            Contract.Requires(buffer != null && count <= buffer.Length - offset);
            Contract.Requires(offset >= 0);
            Contract.Requires(count > 0 && count % InputBlockSize == 0);
            Contract.Ensures(Contract.Result<int>() >= 0);

            //
            // Do the decryption. Note that CapiSymmetricAlgorithm will do all padding itself since the CLR
            // supports padding modes that CAPI does not, so we will always tell CAPI that we are not working
            // with the final block.
            //

            int dataLength = count;
            unsafe {
                fixed (byte* pData = &buffer[offset]) {
                    if (!CapiNative.UnsafeNativeMethods.CryptDecrypt(m_key,
                                                                     SafeCapiHashHandle.InvalidHandle,
                                                                     false,
                                                                     0,
                                                                     new IntPtr(pData),
                                                                     ref dataLength)) {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }

            return dataLength;
        }

        /// <summary>
        ///     Reset the state of the algorithm so that it can begin processing a new message
        /// </summary>
        [SecuritySafeCritical]
        private void Reset() {
            Contract.Requires(m_key != null);
            Contract.Ensures(m_depadBuffer == null);

            //
            // CryptEncrypt / CryptDecrypt must be called with the Final parameter set to true so that
            // their internal state is reset. Since we do all padding by hand, this isn't done by
            // TransformFinalBlock so is done on an empty buffer here.
            //

            byte[] buffer = new byte[OutputBlockSize];
            int resetSize = 0;
            unsafe {
                fixed (byte* pBuffer = buffer) {
                    if (m_encryptionMode == EncryptionMode.Encrypt) {
                        CapiNative.UnsafeNativeMethods.CryptEncrypt(m_key,
                                                                    SafeCapiHashHandle.InvalidHandle,
                                                                    true,
                                                                    0,
                                                                    new IntPtr(pBuffer),
                                                                    ref resetSize,
                                                                    buffer.Length);
                    }
                    else {
                        if (!LocalAppContextSwitches.AesCryptoServiceProviderDontCorrectlyResetDecryptor) {
                            resetSize = buffer.Length;
                        }
                        CapiNative.UnsafeNativeMethods.CryptDecrypt(m_key,
                                                                    SafeCapiHashHandle.InvalidHandle,
                                                                    true,
                                                                    0,
                                                                    new IntPtr(pBuffer),
                                                                    ref resetSize);
                    }
                }
            }

            // Also erase the depadding buffer so we don't cross data from the previous message into this one
            if (m_depadBuffer != null) {
                Array.Clear(m_depadBuffer, 0, m_depadBuffer.Length);
                m_depadBuffer = null;
            }
        }

        /// <summary>
        ///     Encrypt or decrypt a single block of data
        /// </summary>
        [SecuritySafeCritical]
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            Contract.Ensures(Contract.Result<int>() >= 0);

            if (inputBuffer == null) {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0) {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (inputCount <= 0) {
                throw new ArgumentOutOfRangeException("inputCount");
            }
            if (inputCount % InputBlockSize != 0) {
                throw new ArgumentOutOfRangeException("inputCount", SR.GetString(SR.Cryptography_MustTransformWholeBlock));
            }
            if (inputCount > inputBuffer.Length - inputOffset) {
                throw new ArgumentOutOfRangeException("inputCount", SR.GetString(SR.Cryptography_TransformBeyondEndOfBuffer));
            }
            if (outputBuffer == null) {
                throw new ArgumentNullException("outputBuffer");
            }
            if (inputCount > outputBuffer.Length - outputOffset) {
                throw new ArgumentOutOfRangeException("outputOffset", SR.GetString(SR.Cryptography_TransformBeyondEndOfBuffer));
            }

            if (m_encryptionMode == EncryptionMode.Encrypt) {
                // CryptEncrypt operates in place, so make a copy of the original data in the output buffer for
                // it to work on.
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                return EncryptBlocks(outputBuffer, outputOffset, inputCount);
            }
            else {
                return DecryptBlocks(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }
        }

        /// <summary>
        ///     Encrypt or decrypt the last block of data in the current message
        /// </summary>
        [SecuritySafeCritical]
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (inputBuffer == null) {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0) {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (inputCount < 0) {
                throw new ArgumentOutOfRangeException("inputCount");
            }
            if (inputCount > inputBuffer.Length - inputOffset) {
                throw new ArgumentOutOfRangeException("inputCount", SR.GetString(SR.Cryptography_TransformBeyondEndOfBuffer));
            }

            byte[] outputData = null;

            if (m_encryptionMode == EncryptionMode.Encrypt) {
                // If we're encrypting, we need to pad the last block before encrypting it
                outputData = PadBlock(inputBuffer, inputOffset, inputCount);
                if (outputData.Length > 0) {
                    EncryptBlocks(outputData, 0, outputData.Length);
                }
            }
            else {
                // We can't complete decryption on a partial block
                if (inputCount % InputBlockSize != 0) {
                    throw new CryptographicException(SR.GetString(SR.Cryptography_PartialBlock));
                }

                //
                // If we have a depad buffer, copy that into the decryption buffer followed by the input data.
                // Otherwise the decryption buffer is just the input data.
                //

                byte[] ciphertext = null;

                if (m_depadBuffer == null) {
                    ciphertext = new byte[inputCount];
                    Buffer.BlockCopy(inputBuffer, inputOffset, ciphertext, 0, inputCount);
                }
                else {
                    ciphertext = new byte[m_depadBuffer.Length + inputCount];
                    Buffer.BlockCopy(m_depadBuffer, 0, ciphertext, 0, m_depadBuffer.Length);
                    Buffer.BlockCopy(inputBuffer, inputOffset, ciphertext, m_depadBuffer.Length, inputCount);
                }

                // Decrypt the data, then strip the padding to get the final decrypted data.
                if (ciphertext.Length > 0) {
                    int decryptedBytes = RawDecryptBlocks(ciphertext, 0, ciphertext.Length);
                    outputData = DepadBlock(ciphertext, 0, decryptedBytes);
                }
                else {
                    outputData = new byte[0];
                }
            }

            Reset();            
            return outputData;
        }

        /// <summary>
        ///     Prepare the cryptographic key for use in the encryption / decryption operation
        /// </summary>
        [System.Security.SecurityCritical]
        private static SafeCapiKeyHandle SetupKey(SafeCapiKeyHandle key, byte[] iv, CipherMode cipherMode, int feedbackSize) {
            Contract.Requires(key != null);
            Contract.Requires(cipherMode == CipherMode.ECB || iv != null);
            Contract.Requires(0 <= feedbackSize);
            Contract.Ensures(Contract.Result<SafeCapiKeyHandle>() != null &&
                             !Contract.Result<SafeCapiKeyHandle>().IsInvalid &&
                             !Contract.Result<SafeCapiKeyHandle>().IsClosed);

            // Make a copy of the key so that we don't modify the properties of the caller's copy
            SafeCapiKeyHandle encryptionKey = key.Duplicate();

            // Setup the cipher mode first
            CapiNative.SetKeyParameter(encryptionKey, CapiNative.KeyParameter.Mode, (int)cipherMode);

            // If we're not in ECB mode then setup the IV
            if (cipherMode != CipherMode.ECB) {
                CapiNative.SetKeyParameter(encryptionKey, CapiNative.KeyParameter.IV, iv);
            }

            // OFB and CFB require a feedback loop size
            if (cipherMode == CipherMode.CFB || cipherMode == CipherMode.OFB) {
                CapiNative.SetKeyParameter(encryptionKey, CapiNative.KeyParameter.ModeBits, feedbackSize);
            }

            return encryptionKey;
        }
    }
}
