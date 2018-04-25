// Fast, multi-block, ICryptoTransform implementation on top of CommonCrypto
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.

using System;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Crimson.CommonCrypto {

	unsafe class FastCryptorTransform : ICryptoTransform {
		
		IntPtr handle;
		bool encrypt;
		int BlockSizeByte;
		byte[] workBuff;
		bool lastBlock;
		PaddingMode padding;
		
		public FastCryptorTransform (IntPtr cryptor, SymmetricAlgorithm algo, bool encryption, byte[] iv)
		{
			BlockSizeByte = (algo.BlockSize >> 3);
			
			if (iv == null) {
				iv = KeyBuilder.IV (BlockSizeByte);
			} else if (iv.Length < BlockSizeByte) {
				string msg = String.Format ("IV is too small ({0} bytes), it should be {1} bytes long.",
					iv.Length, BlockSizeByte);
				throw new CryptographicException (msg);
			}
			
			handle = cryptor;
			encrypt = encryption;
			padding = algo.Padding;
			// transform buffer
			workBuff = new byte [BlockSizeByte];
		}
		
		~FastCryptorTransform ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				Cryptor.CCCryptorRelease (handle);
				handle = IntPtr.Zero;
			}
			GC.SuppressFinalize (this);
		}

		public virtual bool CanTransformMultipleBlocks {
			get { return true; }
		}

		public virtual bool CanReuseTransform {
			get { return false; }
		}

		public virtual int InputBlockSize {
			get { return BlockSizeByte; }
		}

		public virtual int OutputBlockSize {
			get { return BlockSizeByte; }
		}

		int Transform (byte[] input, int inputOffset, byte[] output, int outputOffset, int length)
		{
			IntPtr len = IntPtr.Zero;
			IntPtr in_len = (IntPtr) length;
			IntPtr out_len = (IntPtr) (output.Length - outputOffset);
			fixed (byte* inputBuffer = &input [0])
			fixed (byte* outputBuffer = &output [0]) {
				CCCryptorStatus s = Cryptor.CCCryptorUpdate (handle, (IntPtr) (inputBuffer + inputOffset), in_len, (IntPtr) (outputBuffer + outputOffset), out_len, ref len);
				if (s != CCCryptorStatus.Success)
					throw new CryptographicException (s.ToString ());
			}
			return (int) len;
		}

		private void CheckInput (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException ("inputOffset", "< 0");
			if (inputCount < 0)
				throw new ArgumentOutOfRangeException ("inputCount", "< 0");
			// ordered to avoid possible integer overflow
			if (inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentException ("inputBuffer", "Overflow");
		}

		// this method may get called MANY times so this is the one to optimize
		public virtual int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			CheckInput (inputBuffer, inputOffset, inputCount);
			// check output parameters
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
			if (outputOffset < 0)
				throw new ArgumentOutOfRangeException ("outputOffset", "< 0");

			// ordered to avoid possible integer overflow
			int len = outputBuffer.Length - inputCount - outputOffset;
			if (!encrypt && (0 > len) && ((padding == PaddingMode.None) || (padding == PaddingMode.Zeros))) {
				throw new CryptographicException ("outputBuffer", "Overflow");
			} else if (KeepLastBlock) {
				if (0 > len + BlockSizeByte) {
					throw new CryptographicException ("outputBuffer", "Overflow");
				}
			} else {
				if (0 > len) {
					// there's a special case if this is the end of the decryption process
					if (inputBuffer.Length - inputOffset - outputBuffer.Length == BlockSizeByte)
						inputCount = outputBuffer.Length - outputOffset;
					else
						throw new CryptographicException ("outputBuffer", "Overflow");
				}
			}
			return InternalTransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
		}

		private bool KeepLastBlock {
			get {
				return ((!encrypt) && (padding != PaddingMode.None) && (padding != PaddingMode.Zeros));
			}
		}

		private int InternalTransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			int offs = inputOffset;
			int full;

			// this way we don't do a modulo every time we're called
			// and we may save a division
			if (inputCount != BlockSizeByte) {
				if ((inputCount % BlockSizeByte) != 0)
					throw new CryptographicException ("Invalid input block size.");

				full = inputCount / BlockSizeByte;
			} else
				full = 1;

			if (KeepLastBlock)
				full--;

			int total = 0;

			if (lastBlock) {
				Transform (workBuff, 0, outputBuffer, outputOffset, BlockSizeByte);
				outputOffset += BlockSizeByte;
				total += BlockSizeByte;
				lastBlock = false;
			}

			if (full > 0) {
				int length = full * BlockSizeByte;
				Transform (inputBuffer, offs, outputBuffer, outputOffset, length);
				offs += length;
				outputOffset += length;
				total += length;
			}

			if (KeepLastBlock) {
				Buffer.BlockCopy (inputBuffer, offs, workBuff, 0, BlockSizeByte);
				lastBlock = true;
			}

			return total;
		}

		private void Random (byte[] buffer, int start, int length)
		{
			byte[] random = new byte [length];
			Cryptor.GetRandom (random);
			Buffer.BlockCopy (random, 0, buffer, start, length);
		}

		private void ThrowBadPaddingException (PaddingMode padding, int length, int position)
		{
			string msg = String.Format ("Bad {0} padding.", padding);
			if (length >= 0)
				msg += String.Format (" Invalid length {0}.", length);
			if (position >= 0)
				msg += String.Format (" Error found at position {0}.", position);
			throw new CryptographicException (msg);
		}

		private byte[] FinalEncrypt (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			// are there still full block to process ?
			int full = (inputCount / BlockSizeByte) * BlockSizeByte;
			int rem = inputCount - full;
			int total = full;

			switch (padding) {
			case PaddingMode.ANSIX923:
			case PaddingMode.ISO10126:
			case PaddingMode.PKCS7:
				// we need to add an extra block for padding
				total += BlockSizeByte;
				break;
			default:
				if (inputCount == 0)
					return new byte [0];
				if (rem != 0) {
					if (padding == PaddingMode.None)
						throw new CryptographicException ("invalid block length");
					// zero padding the input (by adding a block for the partial data)
					byte[] paddedInput = new byte [full + BlockSizeByte];
					Buffer.BlockCopy (inputBuffer, inputOffset, paddedInput, 0, inputCount);
					inputBuffer = paddedInput;
					inputOffset = 0;
					inputCount = paddedInput.Length;
					total = inputCount;
				}
				break;
			}

			byte[] res = new byte [total];
			int outputOffset = 0;

			// process all blocks except the last (final) block
			if (total > BlockSizeByte) {
				outputOffset = InternalTransformBlock (inputBuffer, inputOffset, total - BlockSizeByte, res, 0);
				inputOffset += outputOffset;
			}

			// now we only have a single last block to encrypt
			byte pad = (byte) (BlockSizeByte - rem);
			switch (padding) {
			case PaddingMode.ANSIX923:
				// XX 00 00 00 00 00 00 07 (zero + padding length)
				res [res.Length - 1] = pad;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			case PaddingMode.ISO10126:
				// XX 3F 52 2A 81 AB F7 07 (random + padding length)
				Random (res, res.Length - pad, pad - 1);
				res [res.Length - 1] = pad;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			case PaddingMode.PKCS7:
				// XX 07 07 07 07 07 07 07 (padding length)
				for (int i = res.Length; --i >= (res.Length - pad);) 
					res [i] = pad;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			default:
				InternalTransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
				break;
			}
			return res;
		}

		private byte[] FinalDecrypt (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			if ((inputCount % BlockSizeByte) > 0)
				throw new CryptographicException ("Invalid input block size.");

			int total = inputCount;
			if (lastBlock)
				total += BlockSizeByte;

			byte[] res = new byte [total];
			int outputOffset = 0;

			if (inputCount > 0)
				outputOffset = InternalTransformBlock (inputBuffer, inputOffset, inputCount, res, 0);

			if (lastBlock) {
				Transform (workBuff, 0, res, outputOffset, BlockSizeByte);
				outputOffset += BlockSizeByte;
				lastBlock = false;
			}

			// total may be 0 (e.g. PaddingMode.None)
			byte pad = ((total > 0) ? res [total - 1] : (byte) 0);
			switch (padding) {
			case PaddingMode.ANSIX923:
				if ((pad == 0) || (pad > BlockSizeByte))
					ThrowBadPaddingException (padding, pad, -1);
				for (int i = pad - 1; i > 0; i--) {
					if (res [total - 1 - i] != 0x00)
						ThrowBadPaddingException (padding, -1, i);
				}
				total -= pad;
				break;
			case PaddingMode.ISO10126:
				if ((pad == 0) || (pad > BlockSizeByte))
					ThrowBadPaddingException (padding, pad, -1);
				total -= pad;
				break;
			case PaddingMode.PKCS7:
				if ((pad == 0) || (pad > BlockSizeByte))
					ThrowBadPaddingException (padding, pad, -1);
				for (int i = pad - 1; i > 0; i--) {
					if (res [total - 1 - i] != pad)
						ThrowBadPaddingException (padding, -1, i);
				}
				total -= pad;
				break;
			case PaddingMode.None:	// nothing to do - it's a multiple of block size
			case PaddingMode.Zeros:	// nothing to do - user must unpad himself
				break;
			}

			// return output without padding
			if (total > 0) {
				byte[] data = new byte [total];
				Buffer.BlockCopy (res, 0, data, 0, total);
				// zeroize decrypted data (copy with padding)
				Array.Clear (res, 0, res.Length);
				return data;
			}
			else
				return new byte [0];
		}

		public virtual byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			CheckInput (inputBuffer, inputOffset, inputCount);

			if (encrypt)
				return FinalEncrypt (inputBuffer, inputOffset, inputCount);
			else
				return FinalDecrypt (inputBuffer, inputOffset, inputCount);
		}
	}
}