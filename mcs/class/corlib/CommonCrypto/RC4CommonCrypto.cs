// A CommonCrypto-based implementation of RC4(tm)
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.Security.Cryptography;

using Crimson.CommonCrypto;

#if MONOTOUCH || XAMMAC
using Mono.Security.Cryptography;

namespace Mono.Security.Cryptography {

#if !INSIDE_CORLIB
	public
#endif
	sealed partial class ARC4Managed : RC4, ICryptoTransform {
		
		IntPtr handle;
		
		public ARC4Managed ()
		{
		}
		
		~ARC4Managed ()
		{
			Dispose (false);
		}
#else
namespace Crimson.Security.Cryptography {

	public abstract class RC4 : SymmetricAlgorithm {

		private static KeySizes[] s_legalBlockSizes = {
			new KeySizes (64, 64, 0)
		};

		private static KeySizes[] s_legalKeySizes = {
			new KeySizes (40, 512, 8)  
		};
	
		public RC4 () 
		{
			KeySizeValue = 128;
			BlockSizeValue = 64;
			FeedbackSizeValue = BlockSizeValue;
			LegalBlockSizesValue = s_legalBlockSizes;
			LegalKeySizesValue = s_legalKeySizes;
		}

		// required for compatibility with .NET 2.0
		public override byte[] IV {
			get { return new byte [0]; }
			set { ; }
		}

		new static public RC4 Create() 
		{
			return Create ("RC4");
		}

		new static public RC4 Create (string algName) 
		{
			object o = CryptoConfig.CreateFromName (algName);
			return (RC4) o ?? new RC4CommonCrypto ();
		}
	}

	public sealed class RC4CommonCrypto : RC4, ICryptoTransform {
		
		IntPtr handle;
		
		public RC4CommonCrypto ()
		{
		}
		
		~RC4CommonCrypto ()
		{
			Dispose (false);
		}
#endif
		
		public bool CanReuseTransform {
			get { return false; }
		}

		public bool CanTransformMultipleBlocks {
			get { return true; }
		}
		
		public int InputBlockSize {
			get { return 1; }
		}

		public int OutputBlockSize {
			get { return 1; }
		}
		
		public override byte[] Key {
			get {
				return base.Key;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Key");

				int length = (value.Length << 3);
				KeySizeValue = length;
				KeyValue = (byte[]) value.Clone ();
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				Cryptor.CCCryptorRelease (handle);
				handle = IntPtr.Zero;
			}
			base.Dispose (disposing);
			GC.SuppressFinalize (this);
		}
		
		public override void GenerateIV ()
		{
			// not used for a stream cipher
			IVValue = new byte [0];
		}
		
		public override void GenerateKey ()
		{
			KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			KeyValue = rgbKey;
			IVValue = rgbIV;
			return this;
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			KeyValue = rgbKey;
			IVValue = rgbIV;
			return this;
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

		public unsafe int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			CheckInput (inputBuffer, inputOffset, inputCount);
			if (inputCount == 0)
				return 0;

			// check output parameters
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
			if (outputOffset < 0)
				throw new ArgumentOutOfRangeException ("outputOffset", "< 0");
			// ordered to avoid possible integer overflow
			if (outputOffset > outputBuffer.Length - inputCount)
				throw new ArgumentException ("outputBuffer", "Overflow");
			if (outputBuffer.Length == 0)
				throw new CryptographicException ("output buffer too small");
			
			if (handle == IntPtr.Zero)
				handle = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.RC4, CCOptions.None, KeyValue, IVValue);

			IntPtr len = IntPtr.Zero;
			IntPtr in_len = (IntPtr) (inputBuffer.Length - inputOffset);
			IntPtr out_len = (IntPtr) (outputBuffer.Length - outputOffset);
			fixed (byte* input = &inputBuffer [0])
			fixed (byte* output = &outputBuffer [0]) {
				CCCryptorStatus s = Cryptor.CCCryptorUpdate (handle, (IntPtr) (input + inputOffset), in_len, (IntPtr) (output + outputOffset), out_len, ref len);
				if ((len != out_len) || (s != CCCryptorStatus.Success))
					throw new CryptographicUnexpectedOperationException (s.ToString ());
			}
			return (int) out_len;
		}
		
		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			CheckInput (inputBuffer, inputOffset, inputCount);
			try {
				byte[] output = new byte [inputCount];
				TransformBlock (inputBuffer, inputOffset, inputCount, output, 0);
				return output;
			}
			finally {
				Cryptor.CCCryptorRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}
}