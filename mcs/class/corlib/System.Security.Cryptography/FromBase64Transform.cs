//
// System.Security.Cryptography.FromBase64Transform
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//

using System;

namespace System.Security.Cryptography {

	public enum FromBase64TransformMode : int {
		IgnoreWhiteSpaces,
		DoNotIgnoreWhiteSpaces
	}

	public class FromBase64Transform : ICryptoTransform {

		private FromBase64TransformMode mode;
		private byte [] accumulator;
		private byte [] filterBuffer;
		private int accPtr;
		private bool m_disposed;


		/// <summary>
		///  Creates a new instance of the decoder
		///  with the default transformation mode (IgnoreWhiteSpaces).
		/// </summary>
		public FromBase64Transform ()
			: this (FromBase64TransformMode.IgnoreWhiteSpaces)
		{
		}


		/// <summary>
		///  Creates a new instance of the decoder
		///  with the specified transformation mode.
		/// </summary>
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


		/// <summary>
		/// </summary>
		public bool CanTransformMultipleBlocks {
			get {
				return false;
			}
		}

		public bool CanReuseTransform {
			get { return false; }
		}

		/// <summary>
		///  Returns the input block size for the Base64 decoder.
		/// </summary>
		/// <remarks>
		///  The input block size for Base64 decoder is always 1 byte.
		/// </remarks>
		public int InputBlockSize {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Returns the output block size for the Base64 decoder.
		/// </summary>
		/// <remarks>
		///  The value returned by this property is always 3.
		/// </remarks>
		public int OutputBlockSize {
			get {
				return 3;
			}
		}

		public void Clear() 
		{
			Dispose (true);
		}

		public void Dispose () 
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

		private int Filter (byte [] buffer, int offset, int count)
		{
			int end = offset + count;
			int len = filterBuffer.Length;
			int ptr = 0;
			byte [] filter = this.filterBuffer;

			for (int i = offset; i < end; i++) {
				byte b = buffer [i];
				if (!Char.IsWhiteSpace ((char) b)) {
					if (ptr >= len) {
						len <<= 1;
						this.filterBuffer = new byte [len];
						Array.Copy(filter, 0, this.filterBuffer, 0, len >> 1);
						filter = this.filterBuffer;
					}
					filter [ptr++] = b;
				}
			}

			return ptr;
		}




		private int DoTransform (byte [] inputBuffer,
		                         int inputOffset,
		                         int inputCount,
		                         byte [] outputBuffer,
		                         int outputOffset)
		{
			int full = inputCount >> 2;
			if (full == 0) return 0;

			int rem = 0;

			if (inputBuffer[inputCount - 1] == (byte)'=') {
				++rem;
				--full;
			}

			if (inputBuffer[inputCount - 2] == (byte)'=') ++rem;

			byte [] lookup = Base64Table.DecodeTable;
			int b0,b1,b2,b3;

			for (int i = 0; i < full; i++) {
				b0 = lookup [inputBuffer [inputOffset++]];
				b1 = lookup [inputBuffer [inputOffset++]];
				b2 = lookup [inputBuffer [inputOffset++]];
				b3 = lookup [inputBuffer [inputOffset++]];

				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				outputBuffer [outputOffset++] = (byte) ((b1 << 4) | (b2 >> 2));
				outputBuffer [outputOffset++] = (byte) ((b2 << 6) | b3);
			}

			int res = full * 3;

			switch (rem) {
			case 0:
				break;
			case 1:
				b0 = lookup [inputBuffer [inputOffset++]];
				b1 = lookup [inputBuffer [inputOffset++]];
				b2 = lookup [inputBuffer [inputOffset++]];
				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				outputBuffer [outputOffset++] = (byte) ((b1 << 4) | (b2 >> 2));
				res += 2;
				break;
			case 2:
				b0 = lookup [inputBuffer [inputOffset++]];
				b1 = lookup [inputBuffer [inputOffset++]];
				outputBuffer [outputOffset++] = (byte) ((b0 << 2) | (b1 >> 4));
				++res;
				break;
			default:
				break;
			}

			return res;
		}


		/// <summary>
		/// </summary>
		public int TransformBlock (byte [] inputBuffer,
		                                   int inputOffset,
		                                   int inputCount,
		                                   byte [] outputBuffer,
		                                   int outputOffset)
		{
			int n;
			byte [] src;
			int srcOff;
			int res = 0;

			if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
				n = Filter (inputBuffer, inputOffset, inputCount);
				src = filterBuffer;
				srcOff = 0;
			} else {
				n = inputCount;
				src = inputBuffer;
				srcOff = inputOffset;
			}


			int count = accPtr + n;

			if (count < 4) {
				Array.Copy (src, srcOff, accumulator, accPtr, n);
				accPtr = count;
			} else {
				byte [] tmpBuff = new byte [count];
				Array.Copy (accumulator, 0, tmpBuff, 0, accPtr);
				Array.Copy (src, srcOff, tmpBuff, accPtr, n);
				accPtr = count & 3;
				Array.Copy (src, srcOff + (n - accPtr), accumulator, 0, accPtr);
				res = DoTransform (tmpBuff, 0, count & (~3), outputBuffer, outputOffset);
			}


			return res;
		}





		/// <summary>
		/// </summary>
		public byte [] TransformFinalBlock (byte [] inputBuffer,
		                                            int inputOffset,
		                                            int inputCount)
		{
			byte [] src;
			int srcOff;
			int n;

			if (mode == FromBase64TransformMode.IgnoreWhiteSpaces) {
				n = Filter (inputBuffer, inputOffset, inputCount);
				src = filterBuffer;
				srcOff = 0;
			} else {
				n = inputCount;
				src = inputBuffer;
				srcOff = inputOffset;
			}


			int dataLen = accPtr + n;
			byte [] tmpBuf = new byte [dataLen];

			int resLen = ((dataLen) >> 2) * 3;
			byte [] res = new byte [resLen];

			Array.Copy (accumulator, 0, tmpBuf, 0, accPtr);
			Array.Copy (src, srcOff, tmpBuf, accPtr, n);

			int actLen = DoTransform (tmpBuf, 0, dataLen, res, 0);

			accPtr = 0;

			if (actLen < resLen) {
				byte [] newres = new byte [actLen];

				Array.Copy (res, newres, actLen);
				return newres;
			} else
			return res;
		}

	} // FromBase64Transform

} // System.Security.Cryptography
