//
// ARC4Managed.cs: Alleged RC4(tm) compatible symmetric stream cipher
//	RC4 is a trademark of RSA Security
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	// References:
	// a.	Usenet 1994 - RC4 Algorithm revealed
	//	http://www.qrst.de/html/dsds/rc4.htm

	public class ARC4Managed : RC4, ICryptoTransform {

		private byte[] key;
		private byte[] state;
		private byte x;
		private byte y;
		private bool m_disposed;

		public ARC4Managed () : base () 
		{
			state = new byte [256];
			m_disposed = false;
		}

		~ARC4Managed () 
		{
			Dispose (true);
		}
	        
		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				x = 0;
				y = 0;
				if (key != null) {
					Array.Clear (key, 0, key.Length);
					key = null;
				}
				Array.Clear (state, 0, state.Length);
				state = null;
				GC.SuppressFinalize (this);
				m_disposed = true;
			}
		}

		public override byte[] Key {
			get { return (byte[]) key.Clone (); }
			set { 
				key = (byte[]) value.Clone ();
				KeySetup (key);
			}
		}

		public bool CanReuseTransform {
			get { return false; }
		}

		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgvIV)
		{
			Key = rgbKey;
			return (ICryptoTransform) this;
		}

		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgvIV) 
		{
			Key = rgbKey;
			return CreateEncryptor ();
		}

		public override void GenerateIV () 
		{
			// not used for a stream cipher
			IV = new byte [0];
		}

		public override void GenerateKey () 
		{
			byte[] key = new byte [KeySizeValue >> 3];
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			rng.GetBytes (key);
			Key = key;
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

		private void KeySetup (byte[] key) 
		{
			byte index1 = 0;
			byte index2 = 0;

			for (int counter = 0; counter < 256; counter++)
				state [counter] = (byte) counter;    
			x = 0;
			y = 0;
			for (int counter = 0; counter < 256; counter++) {
				index2 = (byte) ((key [index1] + state [counter] + index2) % 256);
				// swap byte
				byte tmp = state [counter];
				state [counter] = state [index2];
				state [index2] = tmp;
				index1 = (byte) ((index1 + 1) % key.Length);
			}
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			byte xorIndex;
			for (int counter = 0; counter < inputCount; counter ++) {               
				x = (byte) ((x + 1) % 256);
				y = (byte) ((state [x] + y) % 256);
				// swap byte
				byte tmp = state [x];
				state [x] = state [y];
				state [y] = tmp;

				xorIndex = (byte) (state [x] + (state [y]) % 256);
				outputBuffer [outputOffset + counter] = (byte) (inputBuffer [inputOffset + counter] ^ state [xorIndex]);
			}
			return inputCount;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			byte[] output = new byte [inputCount];
			TransformBlock (inputBuffer, inputOffset, inputCount, output, 0);
			return output;
		}
	}
}
