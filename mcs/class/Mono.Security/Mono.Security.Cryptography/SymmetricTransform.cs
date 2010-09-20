//
// Mono.Security.Cryptography.SymmetricTransform implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	// This class implement most of the common code required for symmetric
	// algorithm transforms, like:
	// - CipherMode: Builds CBC and CFB on top of (descendant supplied) ECB
	// - PaddingMode, transform properties, multiple blocks, reuse...
	//
	// Descendants MUST:
	// - intialize themselves (like key expansion, ...)
	// - override the ECB (Electronic Code Book) method which will only be
	//   called using BlockSize byte[] array.
	internal abstract class SymmetricTransform : ICryptoTransform {
		protected SymmetricAlgorithm algo;
		protected bool encrypt;
		private int BlockSizeByte;
		private byte[] temp;
		private byte[] temp2;
		private byte[] workBuff;
		private byte[] workout;
#if !MOONLIGHT
		// Silverlight 2.0 does not support any feedback mode
		private int FeedBackByte;
		private int FeedBackIter;
#endif
		private bool m_disposed = false;
		private bool lastBlock;

		public SymmetricTransform (SymmetricAlgorithm symmAlgo, bool encryption, byte[] rgbIV) 
		{
			algo = symmAlgo;
			encrypt = encryption;
			BlockSizeByte = (algo.BlockSize >> 3);

			if (rgbIV == null) {
				rgbIV = KeyBuilder.IV (BlockSizeByte);
			} else {
				rgbIV = (byte[]) rgbIV.Clone ();
			}
			// compare the IV length with the "currently selected" block size and *ignore* IV that are too big
			if (rgbIV.Length < BlockSizeByte) {
				string msg = Locale.GetText ("IV is too small ({0} bytes), it should be {1} bytes long.",
					rgbIV.Length, BlockSizeByte);
				throw new CryptographicException (msg);
			}

			// mode buffers
			temp = new byte [BlockSizeByte];
			Buffer.BlockCopy (rgbIV, 0, temp, 0, System.Math.Min (BlockSizeByte, rgbIV.Length));
			temp2 = new byte [BlockSizeByte];
#if !MOONLIGHT
			FeedBackByte = (algo.FeedbackSize >> 3);
			if (FeedBackByte != 0)
				FeedBackIter = (int) BlockSizeByte / FeedBackByte;
#endif
			// transform buffers
			workBuff = new byte [BlockSizeByte];
			workout =  new byte [BlockSizeByte];
		}

		~SymmetricTransform () 
		{
			Dispose (false);
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		// MUST be overriden by classes using unmanaged ressources
		// the override method must call the base class
		protected virtual void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				if (disposing) {
					// dispose managed object: zeroize and free
					Array.Clear (temp, 0, BlockSizeByte);
					temp = null;
					Array.Clear (temp2, 0, BlockSizeByte);
					temp2 = null;
				}
				m_disposed = true;
			}
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

		// note: Each block MUST be BlockSizeValue in size!!!
		// i.e. Any padding must be done before calling this method
		protected virtual void Transform (byte[] input, byte[] output) 
		{
#if MOONLIGHT
			// Silverlight 2.0 only supports CBC
			CBC (input, output);
#else
			switch (algo.Mode) {
			case CipherMode.ECB:
				ECB (input, output);
				break;
			case CipherMode.CBC:
				CBC (input, output);
				break;
			case CipherMode.CFB:
				CFB (input, output);
				break;
			case CipherMode.OFB:
				OFB (input, output);
				break;
			case CipherMode.CTS:
				CTS (input, output);
				break;
			default:
				throw new NotImplementedException ("Unkown CipherMode" + algo.Mode.ToString ());
			}
#endif
		}

		// Electronic Code Book (ECB)
		protected abstract void ECB (byte[] input, byte[] output); 

		// Cipher-Block-Chaining (CBC)
		protected virtual void CBC (byte[] input, byte[] output) 
		{
			if (encrypt) {
				for (int i = 0; i < BlockSizeByte; i++)
					temp[i] ^= input[i];
				ECB (temp, output);
				Buffer.BlockCopy (output, 0, temp, 0, BlockSizeByte);
			}
			else {
				Buffer.BlockCopy (input, 0, temp2, 0, BlockSizeByte);
				ECB (input, output);
				for (int i = 0; i < BlockSizeByte; i++)
					output[i] ^= temp[i];
				Buffer.BlockCopy (temp2, 0, temp, 0, BlockSizeByte);
			}
		}

#if !MOONLIGHT
		// Cipher-FeedBack (CFB)
		protected virtual void CFB (byte[] input, byte[] output) 
		{
			if (encrypt) {
				for (int x = 0; x < FeedBackIter; x++) {
					// temp is first initialized with the IV
					ECB (temp, temp2);

					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
					Buffer.BlockCopy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Buffer.BlockCopy (output, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
				}
			}
			else {
				for (int x = 0; x < FeedBackIter; x++) {
					// we do not really decrypt this data!
					encrypt = true;
					// temp is first initialized with the IV
					ECB (temp, temp2);
					encrypt = false;

					Buffer.BlockCopy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Buffer.BlockCopy (input, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
				}
			}
		}

		// Output-FeedBack (OFB)
		protected virtual void OFB (byte[] input, byte[] output) 
		{
			throw new CryptographicException ("OFB isn't supported by the framework");
		}

		// Cipher Text Stealing (CTS)
		protected virtual void CTS (byte[] input, byte[] output) 
		{
			throw new CryptographicException ("CTS isn't supported by the framework");
		}
#endif

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
				throw new ArgumentException ("inputBuffer", Locale.GetText ("Overflow"));
		}

		// this method may get called MANY times so this is the one to optimize
		public virtual int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");
			CheckInput (inputBuffer, inputOffset, inputCount);
			// check output parameters
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
			if (outputOffset < 0)
				throw new ArgumentOutOfRangeException ("outputOffset", "< 0");

			// ordered to avoid possible integer overflow
			int len = outputBuffer.Length - inputCount - outputOffset;
#if MOONLIGHT
			// only PKCS7 is supported Silverlight 2.0
			if (KeepLastBlock) {
#else
			if (!encrypt && (0 > len) && ((algo.Padding == PaddingMode.None) || (algo.Padding == PaddingMode.Zeros))) {
				throw new CryptographicException ("outputBuffer", Locale.GetText ("Overflow"));
			} else if (KeepLastBlock) {
#endif
				if (0 > len + BlockSizeByte) {
					throw new CryptographicException ("outputBuffer", Locale.GetText ("Overflow"));
				}
			} else {
				if (0 > len) {
					// there's a special case if this is the end of the decryption process
					if (inputBuffer.Length - inputOffset - outputBuffer.Length == BlockSizeByte)
						inputCount = outputBuffer.Length - outputOffset;
					else
						throw new CryptographicException ("outputBuffer", Locale.GetText ("Overflow"));
				}
			}
			return InternalTransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
		}

		private bool KeepLastBlock {
			get {
#if MOONLIGHT
				// only PKCS7 is supported Silverlight 2.0
				return !encrypt;
#else
				return ((!encrypt) && (algo.Padding != PaddingMode.None) && (algo.Padding != PaddingMode.Zeros));
#endif
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
			}
			else
				full = 1;

			if (KeepLastBlock)
				full--;

			int total = 0;

			if (lastBlock) {
				Transform (workBuff, workout);
				Buffer.BlockCopy (workout, 0, outputBuffer, outputOffset, BlockSizeByte);
				outputOffset += BlockSizeByte;
				total += BlockSizeByte;
				lastBlock = false;
			}

			for (int i = 0; i < full; i++) {
				Buffer.BlockCopy (inputBuffer, offs, workBuff, 0, BlockSizeByte);
				Transform (workBuff, workout);
				Buffer.BlockCopy (workout, 0, outputBuffer, outputOffset, BlockSizeByte);
				offs += BlockSizeByte;
				outputOffset += BlockSizeByte;
				total += BlockSizeByte;
			}

			if (KeepLastBlock) {
				Buffer.BlockCopy (inputBuffer, offs, workBuff, 0, BlockSizeByte);
				lastBlock = true;
			}

			return total;
		}

#if !MOONLIGHT
		RandomNumberGenerator _rng;

		private void Random (byte[] buffer, int start, int length)
		{
			if (_rng == null) {
				_rng = RandomNumberGenerator.Create ();
			}
			byte[] random = new byte [length];
			_rng.GetBytes (random);
			Buffer.BlockCopy (random, 0, buffer, start, length);
		}

		private void ThrowBadPaddingException (PaddingMode padding, int length, int position)
		{
			string msg = String.Format (Locale.GetText ("Bad {0} padding."), padding);
			if (length >= 0)
				msg += String.Format (Locale.GetText (" Invalid length {0}."), length);
			if (position >= 0)
				msg += String.Format (Locale.GetText (" Error found at position {0}."), position);
			throw new CryptographicException (msg);
		}
#endif

		private byte[] FinalEncrypt (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			// are there still full block to process ?
			int full = (inputCount / BlockSizeByte) * BlockSizeByte;
			int rem = inputCount - full;
			int total = full;

#if MOONLIGHT
			// only PKCS7 is supported Silverlight 2.0
			total += BlockSizeByte;
#else
			switch (algo.Padding) {
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
					if (algo.Padding == PaddingMode.None)
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
#endif // NET_2_1

			byte[] res = new byte [total];
			int outputOffset = 0;

			// process all blocks except the last (final) block
			while (total > BlockSizeByte) {
				InternalTransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
				inputOffset += BlockSizeByte;
				outputOffset += BlockSizeByte;
				total -= BlockSizeByte;
			}

			// now we only have a single last block to encrypt
			byte padding = (byte) (BlockSizeByte - rem);
#if MOONLIGHT
			// only PKCS7 is supported Silverlight 2.0
			for (int i = res.Length; --i >= (res.Length - padding);) 
				res [i] = padding;
			Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
			InternalTransformBlock (res, full, BlockSizeByte, res, full);
#else
			switch (algo.Padding) {
			case PaddingMode.ANSIX923:
				// XX 00 00 00 00 00 00 07 (zero + padding length)
				res [res.Length - 1] = padding;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			case PaddingMode.ISO10126:
				// XX 3F 52 2A 81 AB F7 07 (random + padding length)
				Random (res, res.Length - padding, padding - 1);
				res [res.Length - 1] = padding;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			case PaddingMode.PKCS7:
				// XX 07 07 07 07 07 07 07 (padding length)
				for (int i = res.Length; --i >= (res.Length - padding);) 
					res [i] = padding;
				Buffer.BlockCopy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				InternalTransformBlock (res, full, BlockSizeByte, res, full);
				break;
			default:
				InternalTransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
				break;
			}
#endif // NET_2_1
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

			while (inputCount > 0) {
				int len = InternalTransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
				inputOffset += BlockSizeByte;
				outputOffset += len;
				inputCount -= BlockSizeByte;
			}

			if (lastBlock) {
				Transform (workBuff, workout);
				Buffer.BlockCopy (workout, 0, res, outputOffset, BlockSizeByte);
				outputOffset += BlockSizeByte;
				lastBlock = false;
			}

			// total may be 0 (e.g. PaddingMode.None)
			byte padding = ((total > 0) ? res [total - 1] : (byte) 0);
#if MOONLIGHT
			// only PKCS7 is supported Silverlight 2.0
			if ((padding == 0) || (padding > BlockSizeByte))
				throw new CryptographicException (Locale.GetText ("Bad padding length."));
			for (int i = padding - 1; i > 0; i--) {
				if (res [total - 1 - i] != padding)
					throw new CryptographicException (Locale.GetText ("Bad padding at position {0}.", i));
			}
			total -= padding;
#else
			switch (algo.Padding) {
			case PaddingMode.ANSIX923:
				if ((padding == 0) || (padding > BlockSizeByte))
					ThrowBadPaddingException (algo.Padding, padding, -1);
				for (int i = padding - 1; i > 0; i--) {
					if (res [total - 1 - i] != 0x00)
						ThrowBadPaddingException (algo.Padding, -1, i);
				}
				total -= padding;
				break;
			case PaddingMode.ISO10126:
				if ((padding == 0) || (padding > BlockSizeByte))
					ThrowBadPaddingException (algo.Padding, padding, -1);
				total -= padding;
				break;
			case PaddingMode.PKCS7:
				if ((padding == 0) || (padding > BlockSizeByte))
					ThrowBadPaddingException (algo.Padding, padding, -1);
				for (int i = padding - 1; i > 0; i--) {
					if (res [total - 1 - i] != padding)
						ThrowBadPaddingException (algo.Padding, -1, i);
				}
				total -= padding;
				break;
			case PaddingMode.None:	// nothing to do - it's a multiple of block size
			case PaddingMode.Zeros:	// nothing to do - user must unpad himself
				break;
			}
#endif // NET_2_1

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
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");
			CheckInput (inputBuffer, inputOffset, inputCount);

			if (encrypt)
				return FinalEncrypt (inputBuffer, inputOffset, inputCount);
			else
				return FinalDecrypt (inputBuffer, inputOffset, inputCount);
		}
	}
}
