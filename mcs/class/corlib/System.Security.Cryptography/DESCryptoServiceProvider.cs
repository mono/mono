//
// System.Security.Cryptography.DESCryptoServiceProvider
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Security.Cryptography;


namespace System.Security.Cryptography {


	internal class DESTransformBase : ICryptoTransform {

		internal enum Mode : int {
			ENCRYPTOR = 0,
			DECRYPTOR = 1
		}

		protected delegate void Filter (byte [] workBuff);

		private DESCore core;

		private DESCore.DESCall cryptFn;
		private Filter preprocess;
		private Filter postprocess;

		private byte [] iv;
		private byte [] tmpBlock;

		protected DESTransformBase (Mode mode, byte [] key, byte [] iv)
		{
			core = new DESCore ();

			if (mode == Mode.ENCRYPTOR) {
				cryptFn = new DESCore.DESCall (core.Encrypt);
				preprocess = new Filter (this.EncPreprocess);
				postprocess = new Filter (this.EncPostprocess);
			} else {
				cryptFn = new DESCore.DESCall (core.Decrypt);
				preprocess = new Filter (this.DecPreprocess);
				postprocess = new Filter (this.DecPostprocess);
			}

			core.SetKey (key);
			this.iv = new byte [DESCore.BLOCK_BYTE_SIZE];
			Array.Copy (iv, 0, this.iv, 0, DESCore.BLOCK_BYTE_SIZE);

			tmpBlock = new byte [DESCore.BLOCK_BYTE_SIZE];
		}


		public virtual bool CanTransformMultipleBlocks {
			get {
				return true;
			}
		}


		public virtual int InputBlockSize {
			get {
				return DESCore.BLOCK_BYTE_SIZE;
			}
		}

		public virtual int OutputBlockSize {
			get {
				return DESCore.BLOCK_BYTE_SIZE;
			}
		}

		private void EncPreprocess (byte [] workBuff)
		{
			byte [] iv = this.iv;
			for (int i = 0; i < DESCore.BLOCK_BYTE_SIZE; i++) {
				workBuff [i] ^= iv [i];
			}
		}

		private void EncPostprocess (byte [] workBuff)
		{
			Array.Copy (workBuff, 0, iv, 0, DESCore.BLOCK_BYTE_SIZE);
		}


		private void DecPreprocess (byte [] workBuff)
		{
			Array.Copy (workBuff, 0, tmpBlock, 0, DESCore.BLOCK_BYTE_SIZE);
		}

		private void DecPostprocess (byte [] workBuff)
		{
			EncPreprocess (workBuff);
			Array.Copy (tmpBlock, 0, iv, 0, DESCore.BLOCK_BYTE_SIZE);
		}



		private void Transform (byte [] workBuff)
		{
			preprocess (workBuff);
			cryptFn (workBuff, null);
			postprocess (workBuff);
		}


		public virtual int TransformBlock (byte [] inputBuffer, int inputOffset, int inputCount, byte [] outputBuffer, int outputOffset)
		{
			if ((inputCount & (DESCore.BLOCK_BYTE_SIZE-1)) != 0)
				throw new CryptographicException ("Invalid input block size.");

			if (outputOffset + inputCount > outputBuffer.Length)
				throw new CryptographicException ("Insufficient output buffer size.");

			int step = InputBlockSize;
			int offs = inputOffset;
			int full = inputCount / step;

			byte [] workBuff = new byte [step];

			for (int i = 0; i < full; i++) {
				Array.Copy (inputBuffer, offs, workBuff, 0, step);
				Transform (workBuff);
				Array.Copy (workBuff, 0, outputBuffer, outputOffset, step);
				offs += step;
				outputOffset += step;
			}

			return (full * step);
		}


		public virtual byte [] TransformFinalBlock (byte [] inputBuffer, int inputOffset, int inputCount)
		{
			// TODO: add decryption support

			int num = (inputCount + DESCore.BLOCK_BYTE_SIZE) & (~(DESCore.BLOCK_BYTE_SIZE-1));
			byte [] res = new byte [num];
			int full = num - DESCore.BLOCK_BYTE_SIZE;

			TransformBlock (inputBuffer, inputOffset, full, res, 0);

			int rem = inputCount & (DESCore.BLOCK_BYTE_SIZE-1);

			// PKCS-5 padding
			for (int i = num; --i >= (num - rem);) {
				res [i] = (byte) rem;
			}

			Array.Copy (inputBuffer, inputOffset + full, res, full, rem);

			// the last padded block will be transformed in-place
			TransformBlock (res, full, DESCore.BLOCK_BYTE_SIZE, res, full);

			/*
			byte [] workBuff = new byte [DESCore.BLOCK_BYTE_SIZE];
			Array.Copy (res, full, workBuff, 0, DESCore.BLOCK_BYTE_SIZE);
			preprocess (workBuff);
			cryptFn (workBuff, null);
			Array.Copy (workBuff, 0, res, full, DESCore.BLOCK_BYTE_SIZE);
			*/

			return res;
		}


	} // DESTransformBase


	internal sealed class DESEncryptor : DESTransformBase {
		internal DESEncryptor (byte [] key, byte [] iv)
		: base (DESTransformBase.Mode.ENCRYPTOR, key, iv)
		{
		}
	} // DESEncryptor


	internal sealed class DESDecryptor : DESTransformBase {
		internal DESDecryptor (byte [] key, byte [] iv)
		: base (DESTransformBase.Mode.DECRYPTOR, key, iv)
		{
		}
	} // DESDecryptor


	public class DESCryptoServiceProvider {
		private byte [] iv;
		private byte [] key;

		public DESCryptoServiceProvider ()
		{
		}


		//
		// Factories
		//

		public virtual ICryptoTransform CreateEncryptor()
		{
			return new DESEncryptor (key, iv);
		}

		public virtual ICryptoTransform CreateDecryptor()
		{
			return new DESDecryptor (key, iv);
		}



		// FIXME: Ought to be in DES.cs

		public /*override*/ byte[] Key {
			get {
				return this.key;
			}
			set {
				this.key = new byte [DESCore.KEY_BYTE_SIZE];
				Array.Copy (value, 0, this.key, 0, DESCore.KEY_BYTE_SIZE);
			}
		}

		public virtual byte[] IV {
			get {
				return this.iv;
			}
			set {
				this.iv = new byte [DESCore.KEY_BYTE_SIZE];
				Array.Copy (value, 0, this.iv, 0, DESCore.KEY_BYTE_SIZE);
			}
		}





		public override string ToString ()
		{
			return "mono::System.Security.Cryptography.DESCryptoServiceProvider";
		}

	} // DESCryptoServiceProvider

} // System.Security.Cryptography
