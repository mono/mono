//
// System.Security.Cryptography.ToBase64Transform
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//
// (C) 2004 Novell (http://www.novell.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public class ToBase64Transform : ICryptoTransform {

		private const int inputBlockSize = 3;
		private const int outputBlockSize = 4;
		private bool m_disposed;

		public ToBase64Transform ()
		{
		}

		~ToBase64Transform ()
		{
			Dispose (false);
		}

		public bool CanTransformMultipleBlocks {
			get { return false; }
		}

		public virtual bool CanReuseTransform {
			get { return true; }
		}

		public int InputBlockSize {
			get { return inputBlockSize; }
		}

		public int OutputBlockSize {
			get { return outputBlockSize; }
		}

		public void Clear() 
		{
			Dispose (true);
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose () 
#endif
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		protected virtual void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// dispose unmanaged objects
				if (disposing) {
					// dispose managed objects
				}
				m_disposed = true;
			}
		}

		// LAMESPEC: It's not clear from docs what should be happening 
		// here if inputCount > InputBlockSize. It just "Converts the 
		// specified region of the specified byte array" and that's all.
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("TransformBlock");
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
			if (inputCount < 0)
				throw new ArgumentException ("inputCount", "< 0");
			if (inputCount > inputBuffer.Length)
				throw new ArgumentException ("inputCount", Locale.GetText ("Overflow"));
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException ("inputOffset", "< 0");
			// ordered to avoid possible integer overflow
			if (inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentException ("inputOffset", Locale.GetText ("Overflow"));
			// ordered to avoid possible integer overflow
			if (outputOffset < 0)
				throw new ArgumentOutOfRangeException ("outputOffset", "< 0");
			if (outputOffset > outputBuffer.Length - inputCount)
				throw new ArgumentException ("outputOffset", Locale.GetText ("Overflow"));
/// To match MS implementation
//			if (inputCount != this.InputBlockSize)
//				throw new CryptographicException (Locale.GetText ("Invalid input length"));

			InternalTransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
			return this.OutputBlockSize;
		}

		internal static void InternalTransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			byte[] lookup = Base64Constants.EncodeTable;

			int b1 = inputBuffer [inputOffset];
			int b2 = inputBuffer [inputOffset + 1];
			int b3 = inputBuffer [inputOffset + 2];

			outputBuffer [outputOffset] = lookup [b1 >> 2];
			outputBuffer [outputOffset+1] = lookup [((b1 << 4) & 0x30) | (b2 >> 4)];
			outputBuffer [outputOffset+2] = lookup [((b2 << 2) & 0x3c) | (b3 >> 6)];
			outputBuffer [outputOffset+3] = lookup [b3 & 0x3f];
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("TransformFinalBlock");
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputCount < 0)
				throw new ArgumentException ("inputCount", "< 0");
			if (inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentException ("inputCount", Locale.GetText ("Overflow"));
			if (inputCount > this.InputBlockSize)
				throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid input length"));
			
			return InternalTransformFinalBlock (inputBuffer, inputOffset, inputCount);
		}
		
		// Mono System.Convert depends on the ability to process multiple blocks		
		internal static byte[] InternalTransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			int blockLen = inputBlockSize;
			int outLen = outputBlockSize;
			int fullBlocks = inputCount / blockLen;
			int tail = inputCount % blockLen;

			byte[] res = new byte [(inputCount != 0)
			                        ? ((inputCount + 2) / blockLen) * outLen
			                        : 0];

			int outputOffset = 0;

			for (int i = 0; i < fullBlocks; i++) {
				InternalTransformBlock (inputBuffer, inputOffset, blockLen, res, outputOffset);
				inputOffset += blockLen;
				outputOffset += outLen;
			}

			byte[] lookup = Base64Constants.EncodeTable;
			int b1,b2;

			// When fewer than 24 input bits are available
			// in an input group, zero bits are added
			// (on the right) to form an integral number of
			// 6-bit groups.
			switch (tail) {
			case 0:
				break;
			case 1:
				b1 = inputBuffer [inputOffset];
				res [outputOffset] = lookup [b1 >> 2];
				res [outputOffset+1] = lookup [(b1 << 4) & 0x30];

				// padding
				res [outputOffset+2] = (byte)'=';
				res [outputOffset+3] = (byte)'=';
				break;

			case 2:
				b1 = inputBuffer [inputOffset];
				b2 = inputBuffer [inputOffset + 1];
				res [outputOffset] = lookup [b1 >> 2];
				res [outputOffset+1] = lookup [((b1 << 4) & 0x30) | (b2 >> 4)];
				res [outputOffset+2] = lookup [(b2 << 2) & 0x3c];

				// one-byte padding
				res [outputOffset+3] = (byte)'=';
				break;
			}

			return res;
		}
	}
}
