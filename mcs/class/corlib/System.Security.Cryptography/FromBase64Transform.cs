//
// System.Security.Cryptography.FromBase64Transform
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

namespace System.Security.Cryptography {

	[Serializable]
	public enum FromBase64TransformMode : int {
		IgnoreWhiteSpaces,
		DoNotIgnoreWhiteSpaces
	}

	public class FromBase64Transform : ICryptoTransform {

		private FromBase64TransformMode mode;
		private byte[] accumulator;
		private byte[] filterBuffer;
		private int accPtr;
		private bool m_disposed;

		public FromBase64Transform ()
			: this (FromBase64TransformMode.IgnoreWhiteSpaces)
		{
		}

		public FromBase64Transform (FromBase64TransformMode mode)
		{
			this.mode = mode;
			accumulator = new byte [4];
			filterBuffer = new byte [4];
			accPtr = 0;
			m_disposed = false;
		}

		~FromBase64Transform () 
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
			get { return 1; }
		}

		public int OutputBlockSize {
			get { return 3; }
		}

		public void Clear() 
		{
			Dispose (true);
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		protected virtual void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// zeroize data
				if (accumulator != null)
					Array.Clear (accumulator, 0, accumulator.Length);
				if (filterBuffer != null)
					Array.Clear (filterBuffer, 0, filterBuffer.Length);

				// dispose unmanaged objects
				if (disposing) {
					// dispose managed objects
					accumulator = null;
					filterBuffer = null;
				}
				m_disposed = true;
			}
		}

		private int Filter (byte [] buffer, int offset, int count)
		{
			int end = offset + count;
			int len = filterBuffer.Length;
			int ptr = 0;
			byte[] filter = this.filterBuffer;

			for (int i = offset; i < end; i++) {
				byte b = buffer [i];
				if (!Char.IsWhiteSpace ((char) b)) {
					if (ptr >= len) {
						len <<= 1;
						this.filterBuffer = new byte [len];
						Buffer.BlockCopy (filter, 0, this.filterBuffer, 0, len >> 1);
						filter = this.filterBuffer;
					}
					filter [ptr++] = b;
				}
			}

			return ptr;
		}

		private byte[] lookupTable; 

		private byte lookup (byte input)
		{
			if (input >= lookupTable.Length) {
				throw new FormatException (
					Locale.GetText ("Invalid character in a Base-64 string."));
			}

			byte ret = lookupTable [input];
			if (ret == Byte.MaxValue) {
				throw new FormatException (
					Locale.GetText ("Invalid character in a Base-64 string."));
			}

			return ret;
		}

		private int DoTransform (byte [] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			int full = inputCount >> 2;
			if (full == 0)
				return 0;

			int rem = 0;

			if (inputBuffer[inputCount - 1] == (byte)'=') {
				++rem;
				--full;
			}

			if (inputBuffer[inputCount - 2] == (byte)'=')
				++rem;

			lookupTable = Base64Constants.DecodeTable;
			int b0,b1,b2,b3;

			for (int i = 0; i < full; i++) {
				b0 = lookup (inputBuffer [inputOffset++]);
				b1 = lookup (inputBuffer [inputOffset++]);
				b2 = lookup (inputBuffer [inputOffset++]);
				b3 = lookup (inputBuffer [inputOffset++]);

				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				outputBuffer [outputOffset++] = (byte) ((b1 << 4) | (b2 >> 2));
				outputBuffer [outputOffset++] = (byte) ((b2 << 6) | b3);
			}

			int res = full * 3;

			switch (rem) {
			case 0:
				break;
			case 1:
				b0 = lookup (inputBuffer [inputOffset++]);
				b1 = lookup (inputBuffer [inputOffset++]);
				b2 = lookup (inputBuffer [inputOffset++]);
				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				outputBuffer [outputOffset++] = (byte) ((b1 << 4) | (b2 >> 2));
				res += 2;
				break;
			case 2:
				b0 = lookup (inputBuffer [inputOffset++]);
				b1 = lookup (inputBuffer [inputOffset++]);
				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				++res;
				break;
			}

			return res;
		}

		private void CheckInputParameters (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException ("inputOffset", "< 0");
			if (inputCount > inputBuffer.Length)
				throw new OutOfMemoryException ("inputCount " + Locale.GetText ("Overflow"));
			if (inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentException ("inputOffset", Locale.GetText ("Overflow"));
			if (inputCount < 0)
				throw new OverflowException ("inputCount < 0");
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("FromBase64Transform");
			// LAMESPEC: undocumented exceptions
			CheckInputParameters (inputBuffer, inputOffset, inputCount);
			if ((outputBuffer == null) || (outputOffset < 0) || (inputCount > outputBuffer.Length - outputOffset))
				throw new FormatException ("outputBuffer");

			int n;
			byte [] src;
			int srcOff;
			int res = 0;

			if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
				n = Filter (inputBuffer, inputOffset, inputCount);
				src = filterBuffer;
				srcOff = 0;
			}
			else {
				n = inputCount;
				src = inputBuffer;
				srcOff = inputOffset;
			}

			int count = accPtr + n;

			if (count < 4) {
				Buffer.BlockCopy (src, srcOff, accumulator, accPtr, n);
				accPtr = count;
			}
			else {
				byte[] tmpBuff = new byte [count];
				Buffer.BlockCopy (accumulator, 0, tmpBuff, 0, accPtr);
				Buffer.BlockCopy (src, srcOff, tmpBuff, accPtr, n);
				accPtr = count & 3;
				Buffer.BlockCopy (src, srcOff + (n - accPtr), accumulator, 0, accPtr);
				res = DoTransform (tmpBuff, 0, count & (~3), outputBuffer, outputOffset) ;
			}

			return res;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("FromBase64Transform");
			// LAMESPEC: undocumented exceptions
			CheckInputParameters (inputBuffer, inputOffset, inputCount);

			byte[] src;
			int srcOff;
			int n;

			if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
				n = Filter (inputBuffer, inputOffset, inputCount);
				src = filterBuffer;
				srcOff = 0;
			}
			else {
				n = inputCount;
				src = inputBuffer;
				srcOff = inputOffset;
			}

			int dataLen = accPtr + n;
			byte[] tmpBuf = new byte [dataLen];

			int resLen = ((dataLen) >> 2) * 3;
			byte[] res = new byte [resLen];

			Buffer.BlockCopy (accumulator, 0, tmpBuf, 0, accPtr);
			Buffer.BlockCopy (src, srcOff, tmpBuf, accPtr, n);
			
			int actLen = DoTransform (tmpBuf, 0, dataLen, res, 0);
			accPtr = 0;

			if (actLen < resLen) {
				byte[] newres = new byte [actLen];

				Buffer.BlockCopy (res, 0, newres, 0, actLen);
				return newres;
			} 
			else
				return res;
		}
	}
}
