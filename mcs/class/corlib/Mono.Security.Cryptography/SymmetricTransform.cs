//
// Mono.Security.Cryptography.SymmetricTransform implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
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
		private int FeedBackByte;
		private int FeedBackIter;
		private bool m_disposed = false;

		public SymmetricTransform (SymmetricAlgorithm symmAlgo, bool encryption, byte[] rgbIV) 
		{
			algo = symmAlgo;
			encrypt = encryption;
			BlockSizeByte = (algo.BlockSize >> 3);
			// mode buffers
			temp = new byte [BlockSizeByte];
			Array.Copy (rgbIV, 0, temp, 0, BlockSizeByte);
			temp2 = new byte [BlockSizeByte];
			FeedBackByte = (algo.FeedbackSize >> 3);
			FeedBackIter = (int) BlockSizeByte / FeedBackByte;
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
		protected void Dispose (bool disposing) 
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

		public bool CanReuseTransform {
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
		protected void Transform (byte[] input, byte[] output) 
		{
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
				Array.Copy (output, 0, temp, 0, BlockSizeByte);
			}
			else {
				Array.Copy (input, 0, temp2, 0, BlockSizeByte);
				ECB (input, output);
				for (int i = 0; i < BlockSizeByte; i++)
					output[i] ^= temp[i];
				Array.Copy (temp2, 0, temp, 0, BlockSizeByte);
			}
		}

		// Cipher-FeedBack (CFB)
		protected virtual void CFB (byte[] input, byte[] output) 
		{
			if (encrypt) {
				for (int x = 0; x < FeedBackIter; x++) {
					// temp is first initialized with the IV
					ECB (temp, temp2);

					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
					Array.Copy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Array.Copy (output, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
				}
			}
			else {
				for (int x = 0; x < FeedBackIter; x++) {
					// we do not really decrypt this data!
					encrypt = true;
					// temp is first initialized with the IV
					ECB (temp, temp2);
					encrypt = false;

					Array.Copy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Array.Copy (input, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
				}
			}
		}

		// Output-FeedBack (OFB)
		protected virtual void OFB (byte[] input, byte[] output) 
		{
			throw new NotImplementedException ("OFB not yet supported");
		}

		// Cipher Text Stealing (CTS)
		protected virtual void CTS (byte[] input, byte[] output) 
		{
			throw new NotImplementedException ("CTS not yet supported");
		}

		// this method may get called MANY times so this is the one to optimize
		public virtual int TransformBlock (byte [] inputBuffer, int inputOffset, int inputCount, byte [] outputBuffer, int outputOffset) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");

			if (outputOffset + inputCount > outputBuffer.Length)
				throw new CryptographicException ("Insufficient output buffer size.");

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

			int total = 0;
			for (int i = 0; i < full; i++) {
				Array.Copy (inputBuffer, offs, workBuff, 0, BlockSizeByte);
				Transform (workBuff, workout);
				Array.Copy (workout, 0, outputBuffer, outputOffset, BlockSizeByte);
				offs += BlockSizeByte;
				outputOffset += BlockSizeByte;
				total += BlockSizeByte;
			}

			return total;
		}

		private byte[] FinalEncrypt (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			// are there still full block to process ?
			int full = (inputCount / BlockSizeByte) * BlockSizeByte;
			int rem = inputCount - full;
			int total = full;

			if (algo.Padding != PaddingMode.PKCS7) {
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
			}
			else {
				// we need to add an extra block for padding
				total += BlockSizeByte;
			}

			byte[] res = new byte [total];

			// process all blocks except the last (final) block
			while (total > BlockSizeByte) {
				TransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, inputOffset);
				inputOffset += BlockSizeByte;
				total -= BlockSizeByte;
			}

			// now we only have a single last block to encrypt
			if (algo.Padding == PaddingMode.PKCS7) {
				byte padding = (byte) (BlockSizeByte - rem);
				for (int i = res.Length; --i >= (res.Length - padding);) 
					res [i] = padding;
				Array.Copy (inputBuffer, inputOffset, res, full, rem);
				// the last padded block will be transformed in-place
				TransformBlock (res, full, BlockSizeByte, res, full);
			}
			else
				TransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, inputOffset);

			return res;
		}

		private byte[] FinalDecrypt (byte [] inputBuffer, int inputOffset, int inputCount) 
		{
			if ((inputCount % BlockSizeByte) > 0)
				throw new CryptographicException ("Invalid input block size.");

			int total = inputCount;
			byte[] res = new byte [total];
			while (inputCount > 0) {
				TransformBlock (inputBuffer, inputOffset, BlockSizeByte, res, inputOffset);
				inputOffset += BlockSizeByte;
				inputCount -= BlockSizeByte;
			}

			switch (algo.Padding) {
				case PaddingMode.None:	// nothing to do - it's a multiple of block size
				case PaddingMode.Zeros:	// nothing to do - user must unpad himself
					break;
				case PaddingMode.PKCS7:
					total -= res [total - 1];
					break;
			}

			// return output without padding
			if (total > 0) {
				byte[] data = new byte [total];
				Array.Copy (res, 0, data, 0, total);
				// zeroize decrypted data (copy with padding)
				Array.Clear (res, 0, res.Length);
				return data;
			}
			else
				return new byte [0];
		}

		public virtual byte [] TransformFinalBlock (byte [] inputBuffer, int inputOffset, int inputCount) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");

			if (encrypt)
				return FinalEncrypt (inputBuffer, inputOffset, inputCount);
			else
				return FinalDecrypt (inputBuffer, inputOffset, inputCount);
		}
	}
}