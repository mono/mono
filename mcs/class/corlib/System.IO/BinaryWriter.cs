//
// System.IO.BinaryWriter
//
// Author:
//   Matt Kimball (matt@kimball.net)
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
using System.Text;
using System.Globalization;
using Mono.Security;
using System.Runtime.InteropServices;

namespace System.IO {
	[Serializable]
	[ComVisible (true)]
	public class BinaryWriter : IDisposable {

		// Null is a BinaryWriter with no backing store.
		public static readonly BinaryWriter Null = new BinaryWriter ();

		protected Stream OutStream;
		private Encoding m_encoding;
		private byte [] buffer;
		private bool disposed = false;

		protected BinaryWriter() : this (Stream.Null, Encoding.UTF8UnmarkedUnsafe) {
		}

		public BinaryWriter(Stream output) : this(output, Encoding.UTF8UnmarkedUnsafe) {
		}

		public BinaryWriter(Stream output, Encoding encoding) {
			if (output == null) 
				throw new ArgumentNullException("output");
			if (encoding == null) 
				throw new ArgumentNullException("encoding");
			if (!output.CanWrite)
				throw new ArgumentException(Locale.GetText ("Stream does not support writing or already closed."));

			OutStream = output;
			m_encoding = encoding;
			buffer = new byte [16];
		}

		public virtual Stream BaseStream {
			get {
				Flush ();
				return OutStream;
			}
		}

		public virtual void Close() {
			Dispose (true);
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose() 
#endif
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && OutStream != null)
				OutStream.Close();
			
			buffer = null;
			m_encoding = null;
			disposed = true;
		}

		public virtual void Flush() {
			OutStream.Flush();
		}

		public virtual long Seek(int offset, SeekOrigin origin) {

			return OutStream.Seek(offset, origin);
		}

		public virtual void Write(bool value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) (value ? 1 : 0);
			OutStream.Write(buffer, 0, 1);
		}

		public virtual void Write(byte value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.WriteByte(value);
		}

		public virtual void Write(byte[] buffer) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (buffer == null)
				throw new ArgumentNullException("buffer");
			OutStream.Write(buffer, 0, buffer.Length);
		}

		public virtual void Write(byte[] buffer, int index, int count) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (buffer == null)
				throw new ArgumentNullException("buffer");
			OutStream.Write(buffer, index, count);
		}

		public virtual void Write(char ch) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			char[] dec = new char[1];
			dec[0] = ch;
			byte[] enc = m_encoding.GetBytes(dec, 0, 1);
			OutStream.Write(enc, 0, enc.Length);
		}
		
		public virtual void Write(char[] chars) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (chars == null)
				throw new ArgumentNullException("chars");
			byte[] enc = m_encoding.GetBytes(chars, 0, chars.Length);
			OutStream.Write(enc, 0, enc.Length);
		}

		public virtual void Write(char[] chars, int index, int count) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (chars == null)
				throw new ArgumentNullException("chars");
			byte[] enc = m_encoding.GetBytes(chars, index, count);
			OutStream.Write(enc, 0, enc.Length);
		}

		unsafe public virtual void Write(decimal value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			byte* value_ptr = (byte *)&value;
			
			/*
			 * decimal in stream is lo32, mi32, hi32, ss32
			 * but its internal structure si ss32, hi32, lo32, mi32
			 */
				 
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < 16; i++) {
					if (i < 4) 
						buffer [i + 12] = value_ptr [i];
					else if (i < 8)
						buffer [i + 4] = value_ptr [i];
					else if (i < 12)
						buffer [i - 8] = value_ptr [i];
					else 
						buffer [i - 8] = value_ptr [i];
				}
			} else {
				for (int i = 0; i < 16; i++) {
					if (i < 4) 
						buffer [15 - i] = value_ptr [i];
					else if (i < 8)
						buffer [15 - i] = value_ptr [i];
					else if (i < 12)
						buffer [11 - i] = value_ptr [i];
					else 
						buffer [19 - i] = value_ptr [i];
				}
			}

			OutStream.Write(buffer, 0, 16);
		}

		public virtual void Write(double value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.Write(BitConverterLE.GetBytes(value), 0, 8);
		}
		
		public virtual void Write(short value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			OutStream.Write(buffer, 0, 2);
		}
		
		public virtual void Write(int value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			buffer [2] = (byte) (value >> 16);
			buffer [3] = (byte) (value >> 24);
			OutStream.Write(buffer, 0, 4);
		}

		public virtual void Write(long value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			for (int i = 0, sh = 0; i < 8; i++, sh += 8)
				buffer [i] = (byte) (value >> sh);
			OutStream.Write(buffer, 0, 8);
		}

		[CLSCompliant(false)]
		public virtual void Write(sbyte value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			OutStream.Write(buffer, 0, 1);
		}

		public virtual void Write(float value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.Write(BitConverterLE.GetBytes(value), 0, 4);
		}

		byte [] stringBuffer;
		int maxCharsPerRound;
		
		public virtual void Write(string value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			int len = m_encoding.GetByteCount (value);
			Write7BitEncodedInt (len);
			
			if (stringBuffer == null) {
				stringBuffer = new byte [512];
				maxCharsPerRound = 512 / m_encoding.GetMaxByteCount (1);
			}
			
			int chpos = 0;
			int chrem = value.Length;
			while (chrem > 0) {
				int cch = (chrem > maxCharsPerRound) ? maxCharsPerRound : chrem;
				int blen = m_encoding.GetBytes (value, chpos, cch, stringBuffer, 0);
				OutStream.Write (stringBuffer, 0, blen);
				
				chpos += cch;
				chrem -= cch;
			}
		}

		[CLSCompliant(false)]
		public virtual void Write(ushort value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			OutStream.Write(buffer, 0, 2);
		}

		[CLSCompliant(false)]
		public virtual void Write(uint value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			buffer [2] = (byte) (value >> 16);
			buffer [3] = (byte) (value >> 24);
			OutStream.Write(buffer, 0, 4);
		}

		[CLSCompliant(false)]
		public virtual void Write(ulong value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			for (int i = 0, sh = 0; i < 8; i++, sh += 8)
				buffer [i] = (byte) (value >> sh);
			OutStream.Write(buffer, 0, 8);
		}

		protected void Write7BitEncodedInt(int value) {
			do {
				int high = (value >> 7) & 0x01ffffff;
				byte b = (byte)(value & 0x7f);

				if (high != 0) {
					b = (byte)(b | 0x80);
				}

				Write(b);
				value = high;
			} while(value != 0);
		}
	}
}
