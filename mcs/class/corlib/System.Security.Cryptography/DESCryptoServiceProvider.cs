//
// System.Security.Cryptography.DESCryptoServiceProvider
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//


using System;
using System.Security.Cryptography;


namespace System.Security.Cryptography {

	internal class DESTransformBase : ICryptoTransform {

		internal enum Action : int {
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
		private CipherMode mode;
		private Action action;

		protected DESTransformBase (Action action, byte [] key, byte [] iv, CipherMode mode) 
		{
			core = new DESCore ();
			this.action = action;
			this.mode = mode;

			if (action == Action.ENCRYPTOR) {
				cryptFn = new DESCore.DESCall (core.Encrypt);
				preprocess = new Filter (this.EncPreprocess);
				postprocess = new Filter (this.EncPostprocess);
			} 
			else {
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
			get { return false; }
		}

		public bool CanReuseTransform {
			get { return true; }
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

                void System.IDisposable.Dispose ()
                {
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
			int num = (inputCount + DESCore.BLOCK_BYTE_SIZE) & (~(DESCore.BLOCK_BYTE_SIZE-1));
			byte [] res = new byte [num];
			int full = num - DESCore.BLOCK_BYTE_SIZE;

			TransformBlock (inputBuffer, inputOffset, full, res, 0);

			int rem = inputCount & (DESCore.BLOCK_BYTE_SIZE-1);

			if (action == Action.ENCRYPTOR) {
				// PKCS#7 padding
				int p7Padding = DESCore.BLOCK_BYTE_SIZE - (inputCount % DESCore.BLOCK_BYTE_SIZE);
				for (int i = DESCore.BLOCK_BYTE_SIZE; --i >= (DESCore.BLOCK_BYTE_SIZE - p7Padding);) {
					res [i] = (byte) p7Padding;
				}

				Array.Copy (inputBuffer, inputOffset + full, res, full, rem);

				// the last padded block will be transformed in-place
				TransformBlock (res, full, DESCore.BLOCK_BYTE_SIZE, res, full);
			}
			else {
				// PKCS#7 padding
				byte padding = res [inputCount - 1];
				for (int i = 0; i < padding; i++) {
					if (res [inputCount - 1 - i] == padding)
						res[inputCount - 1 - i] = 0x00;
				}
			}

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
		internal DESEncryptor (byte [] key, byte [] iv, CipherMode mode)
		: base (DESTransformBase.Action.ENCRYPTOR, key, iv, mode)
		{
		}
	} // DESEncryptor


	internal sealed class DESDecryptor : DESTransformBase {
		internal DESDecryptor (byte [] key, byte [] iv, CipherMode mode)
		: base (DESTransformBase.Action.DECRYPTOR, key, iv, mode)
		{
		}
	} // DESDecryptor


	public sealed class DESCryptoServiceProvider : DES {
		private RandomNumberGenerator rng;

		public DESCryptoServiceProvider ()
		{
			// there are no constructor accepting a secret key
			// so we always need the RNG (using the default one)
			rng = RandomNumberGenerator.Create();
			// there's always a default key/iv available when 
			// creating a symmetric algorithm object
			GenerateKey();
			GenerateIV();
		}

		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			// by using Key/IV (instead of KeyValue/IVValue) we get
			// all validation done by "set"
			Key = rgbKey;
			IV = rgbIV;
			return new DESEncryptor (KeyValue, IVValue, ModeValue);
		}

		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			// by using Key/IV (instead of KeyValue/IVValue) we get
			// all validation done by "set"
			Key = rgbKey;
			IV = rgbIV;
			return new DESDecryptor (KeyValue, IVValue, ModeValue);
		}

		public override void GenerateIV () 
		{
			IVValue = new byte [DESCore.BLOCK_BYTE_SIZE];
			rng.GetBytes (IVValue);
		}

		public override void GenerateKey () 
		{
			KeyValue = new byte [DESCore.KEY_BYTE_SIZE];
			rng.GetBytes (KeyValue);
			while (IsWeakKey (KeyValue) || IsSemiWeakKey (KeyValue))
				rng.GetBytes (KeyValue);
		}

	} // DESCryptoServiceProvider

} // System.Security.Cryptography
