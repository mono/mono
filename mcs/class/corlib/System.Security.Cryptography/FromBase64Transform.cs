//
// System.Security.Cryptography.FromBase64Transform.cs
//
// Authors:
//	Sergey Chaban (serge@wildwestsoftware.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

	[Serializable]
	[ComVisible (true)]
	public enum FromBase64TransformMode : int {
		IgnoreWhiteSpaces,
		DoNotIgnoreWhiteSpaces
	}

	[ComVisible (true)]
	public class FromBase64Transform : ICryptoTransform {

		private FromBase64TransformMode mode;
		private byte[] accumulator;
		private int accPtr;
		private bool m_disposed;

		private const byte TerminatorByte = ((byte) '=');

		public FromBase64Transform ()
			: this (FromBase64TransformMode.IgnoreWhiteSpaces)
		{
		}

		public FromBase64Transform (FromBase64TransformMode whitespaces)
		{
			this.mode = whitespaces;
			accumulator = new byte [4];
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
				// zeroize data
				if (accumulator != null)
					Array.Clear (accumulator, 0, accumulator.Length);

				// dispose unmanaged objects
				if (disposing) {
					// dispose managed objects
					accumulator = null;
				}
				m_disposed = true;
			}
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

		private int ProcessBlock (byte[] output, int offset)
		{
			int rem = 0;
			if (accumulator [3] == TerminatorByte)
				rem++;
			if (accumulator [2] == TerminatorByte)
				rem++;

			lookupTable = Base64Constants.DecodeTable;
			int b0,b1,b2,b3;

			switch (rem) {
			case 0:
				b0 = lookup (accumulator [0]);
				b1 = lookup (accumulator [1]);
				b2 = lookup (accumulator [2]);
				b3 = lookup (accumulator [3]);
				output [offset++] = (byte) ((b0 << 2) | (b1 >> 4));
				output [offset++] = (byte) ((b1 << 4) | (b2 >> 2));
				output [offset] = (byte) ((b2 << 6) | b3);
				break;
			case 1:
				b0 = lookup (accumulator [0]);
				b1 = lookup (accumulator [1]);
				b2 = lookup (accumulator [2]);
				output [offset++] = (byte) ((b0 << 2) | (b1 >> 4));
				output [offset] = (byte) ((b1 << 4) | (b2 >> 2));
				break;
			case 2:
				b0 = lookup (accumulator [0]);
				b1 = lookup (accumulator [1]);
				output [offset] = (byte) ((b0 << 2) | (b1 >> 4));
				break;
			}

			return (3 - rem);
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
			if ((outputBuffer == null) || (outputOffset < 0))
				throw new FormatException ("outputBuffer");

			int res = 0;

			while (inputCount > 0) {
				if (accPtr < 4) {
					byte b = inputBuffer [inputOffset++];
					if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
						if (!Char.IsWhiteSpace ((char) b))
							accumulator [accPtr++] = b;
					} else {
						// don't ignore, we'll fail if bad data is provided
						accumulator [accPtr++] = b;
					}
				}
				if (accPtr == 4) {
					res += ProcessBlock (outputBuffer, outputOffset);
					outputOffset += 3;
					accPtr = 0;
				}
				inputCount--;
			}

			return res;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("FromBase64Transform");
			// LAMESPEC: undocumented exceptions
			CheckInputParameters (inputBuffer, inputOffset, inputCount);

			int ws = 0;
			int terminator = 0;
			if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
				// count whitespace inside string
				for (int i=inputOffset, j=0; j < inputCount; i++, j++) {
					if (Char.IsWhiteSpace ((char)inputBuffer [i]))
						ws++;
				}
				// no more (useful) data
				if (ws == inputCount)
					return new byte [0];
				// there may be whitespace after the terminator
				int k = inputOffset + inputCount - 1;
				int n = Math.Min (2, inputCount);
				while (n > 0) {
					char c = (char) inputBuffer [k--];
					if (c == '=') {
						terminator++;
						n--;
					} else if (Char.IsWhiteSpace (c)) {
						continue;
					} else {
						break;
					}
				}						
			} else {
				if (inputBuffer [inputOffset + inputCount - 1] == TerminatorByte)
					terminator++;
				if (inputBuffer [inputOffset + inputCount - 2] == TerminatorByte)
					terminator++;
			}
			// some terminators could already be in the accumulator
			if ((inputCount < 4) && (terminator < 2)) {
				if ((accPtr > 2) && (accumulator [3] == TerminatorByte))
					terminator++;
				if ((accPtr > 1) && (accumulator [2] == TerminatorByte))
					terminator++;
			}

			int count = ((accPtr + inputCount - ws) >> 2) * 3 - terminator;
			if (count <= 0)
				return new byte [0];

			// allocate the "right" ammount (to avoid multiple allocation/copy)
			byte[] result = new byte [count];
			TransformBlock (inputBuffer, inputOffset, inputCount, result, 0);
			return result;
		}
	}
}
