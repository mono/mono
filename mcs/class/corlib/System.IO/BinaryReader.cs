//
// System.IO.BinaryReader
//
// Author:
//   Matt Kimball (matt@kimball.net)
//   Dick Porter (dick@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
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
	[ComVisible (true)]
	public class BinaryReader : IDisposable {
		Stream m_stream;
		Encoding m_encoding;

		byte[] m_buffer;

		Decoder decoder;
		char[] charBuffer;
		byte[] charByteBuffer;
		
		//
		// 128 chars should cover most strings in one grab.
		//
		const int MaxBufferSize = 128;

		
		private bool m_disposed;

		public BinaryReader(Stream input) : this(input, Encoding.UTF8UnmarkedUnsafe) {
		}

		public BinaryReader(Stream input, Encoding encoding) {
			if (input == null || encoding == null) 
				throw new ArgumentNullException(Locale.GetText ("Input or Encoding is a null reference."));
			if (!input.CanRead)
				throw new ArgumentException(Locale.GetText ("The stream doesn't support reading."));

			m_stream = input;
			m_encoding = encoding;
			decoder = encoding.GetDecoder ();
			
			// internal buffer size is documented to be between 16 and the value
			// returned by GetMaxByteCount for the specified encoding
			m_buffer = new byte [Math.Max (16, encoding.GetMaxByteCount (1))];
		}

		public virtual Stream BaseStream {
			get {
				return m_stream;
			}
		}

		public virtual void Close() {
			Dispose (true);
			m_disposed = true;
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && m_stream != null)
				m_stream.Close ();

			m_disposed = true;
			m_buffer = null;
			m_encoding = null;
			m_stream = null;
			charBuffer = null;
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose() 
#endif
		{
			Dispose (true);
		}

		protected virtual void FillBuffer (int numBytes)
		{
			if (numBytes > m_buffer.Length)
				throw new ArgumentOutOfRangeException ("numBytes");
			if (m_disposed)
				throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");
			if (m_stream==null)
				throw new IOException("Stream is invalid");
			
			/* Cope with partial reads */
			int pos=0;

			while(pos<numBytes) {
				int n=m_stream.Read(m_buffer, pos, numBytes-pos);
				if(n==0) {
					throw new EndOfStreamException();
				}

				pos+=n;
			}
		}

		public virtual int PeekChar() {
			if(m_stream==null) {
				
				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}

			if ( !m_stream.CanSeek )
			{
				return -1;
			}

			char[] result = new char[1];
			int bcount;

			int ccount = ReadCharBytes (result, 0, 1, out bcount);

			// Reposition the stream
			m_stream.Position -= bcount;

			// If we read 0 characters then return -1
			if (ccount == 0) 
			{
				return -1;
			}
			
			// Return the single character we read
			return result[0];
		}

		public virtual int Read() {
			if (charBuffer == null)
				charBuffer = new char [MaxBufferSize];

			int count = Read (charBuffer, 0, 1);
			if(count == 0) {
				/* No chars available */
				return -1;
			}
			
			return charBuffer [0];
		}

		public virtual int Read(byte[] buffer, int index, int count) {
			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (buffer == null) {
				throw new ArgumentNullException("buffer is null");
			}
			if (index < 0) {
				throw new ArgumentOutOfRangeException("index is less than 0");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException("buffer is too small");
			}

			int bytes_read=m_stream.Read(buffer, index, count);

			return(bytes_read);
		}

		public virtual int Read(char[] buffer, int index, int count) {

			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (buffer == null) {
				throw new ArgumentNullException("buffer is null");
			}
			if (index < 0) {
				throw new ArgumentOutOfRangeException("index is less than 0");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException("buffer is too small");
			}

			int bytes_read;
			return ReadCharBytes (buffer, index, count, out bytes_read);
		}

		private int ReadCharBytes (char[] buffer, int index, int count, out int bytes_read) 
		{
			int chars_read = 0;
			bytes_read = 0;
			
			while (chars_read < count) {
				int pos = 0;
				while (true) {
					CheckBuffer (pos + 1);

					int read_byte = m_stream.ReadByte ();

					if (read_byte == -1)
						/* EOF */
						return chars_read;

					m_buffer [pos ++] = (byte)read_byte;
					bytes_read ++;

					int n = m_encoding.GetChars (m_buffer, 0, pos, buffer, index + chars_read);
					if (n > 0)
						break;
				}
				chars_read ++;
			}

			return chars_read;
		}

		protected int Read7BitEncodedInt() {
			int ret = 0;
			int shift = 0;
			int len;
			byte b;

			for (len = 0; len < 5; ++len) {
				b = ReadByte();
				
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
				if ((b & 0x80) == 0)
					break;
			}

			if (len < 5)
				return ret;
			else
				throw new FormatException ("Too many bytes in what should have been a 7 bit encoded Int32.");
		}

		public virtual bool ReadBoolean() {
			// Return value:
			//  true if the byte is non-zero; otherwise false.
			return ReadByte() != 0;
		}

		public virtual byte ReadByte() {
			if (m_stream == null) {
				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException ("Stream is invalid");
			}
			
			int val = m_stream.ReadByte ();
			if (val != -1)
				return (byte) val;
			
			throw new EndOfStreamException ();
		}

		public virtual byte[] ReadBytes(int count) {
			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			/* Can't use FillBuffer() here, because it's OK to
			 * return fewer bytes than were requested
			 */

			byte[] buf = new byte[count];
			int pos=0;
			
			while(pos < count) 
			{
				int n=m_stream.Read(buf, pos, count-pos);
				if(n==0) {
					/* EOF */
					break;
				}

				pos+=n;
			}
				
			if (pos!=count) {
				byte[] new_buffer=new byte[pos];
				Buffer.BlockCopyInternal (buf, 0, new_buffer, 0, pos);
				return(new_buffer);
			}
			
			return(buf);
		}

		public virtual char ReadChar() {
			int ch=Read();

			if(ch==-1) {
				throw new EndOfStreamException();
			}

			return((char)ch);
		}

		public virtual char[] ReadChars(int count) {
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			if (count == 0)
				return new char [0];
					
			char[] full = new char[count];
			int chars = Read(full, 0, count);
			
			if (chars == 0) {
				throw new EndOfStreamException();
			} else if (chars != full.Length) {
				char[] ret = new char[chars];
				Array.Copy(full, 0, ret, 0, chars);
				return ret;
			} else {
				return full;
			}
		}

		unsafe public virtual decimal ReadDecimal() {
			FillBuffer(16);

			decimal ret;
			byte* ret_ptr = (byte *)&ret;
			
			/*
			 * internal representation of decimal is 
			 * ss32, hi32, lo32, mi32, 
			 * but in stream it is 
			 * lo32, mi32, hi32, ss32
			 * So we have to rerange this int32 values
			 */			  
		  
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < 16; i++) {
					if (i < 4) {
					        // lo 8 - 12			  
					        ret_ptr [i + 8] = m_buffer [i];
					} else if (i < 8) {
					        // mid 12 - 16
					        ret_ptr [i + 8] = m_buffer [i];
					} else if (i < 12) {
					        // hi 4 - 8
					        ret_ptr [i - 4] = m_buffer [i];
					} else if (i < 16) {
					        // ss 0 - 4
					        ret_ptr [i - 12] = m_buffer [i];
					}				
				}
			} else {
				for (int i = 0; i < 16; i++) {
					if (i < 4) {
					        // lo 8 - 12			  
					        ret_ptr [11 - i] = m_buffer [i];
					} else if (i < 8) {
					        // mid 12 - 16
					        ret_ptr [19 - i] = m_buffer [i];
					} else if (i < 12) {
					        // hi 4 - 8
					        ret_ptr [15 - i] = m_buffer [i];
					} else if (i < 16) {
					        // ss 0 - 4
					        ret_ptr [15 - i] = m_buffer [i];
					}				
				}
			}

			return ret;
		}

		public virtual double ReadDouble() {
			FillBuffer(8);

			return(BitConverterLE.ToDouble(m_buffer, 0));
		}

		public virtual short ReadInt16() {
			FillBuffer(2);

			return((short) (m_buffer[0] | (m_buffer[1] << 8)));
		}

		public virtual int ReadInt32() {
			FillBuffer(4);

			return(m_buffer[0] | (m_buffer[1] << 8) |
			       (m_buffer[2] << 16) | (m_buffer[3] << 24));
		}

		public virtual long ReadInt64() {
			FillBuffer(8);

			uint ret_low  = (uint) (m_buffer[0]            |
			                       (m_buffer[1] << 8)  |
			                       (m_buffer[2] << 16) |
			                       (m_buffer[3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[4]        |
			                       (m_buffer[5] << 8)  |
			                       (m_buffer[6] << 16) |
			                       (m_buffer[7] << 24)
			                       );
			return (long) ((((ulong) ret_high) << 32) | ret_low);
		}

		[CLSCompliant(false)]
		public virtual sbyte ReadSByte() {
			return (sbyte) ReadByte ();
		}

		public virtual string ReadString() {
			/* Inspection of BinaryWriter-written files
			 * shows that the length is given in bytes,
			 * not chars
			 */
			int len = Read7BitEncodedInt();
			
			if (len < 0)
				throw new IOException ("Invalid binary file (string len < 0)");

			if (len == 0)
				return String.Empty;
			
			if (charByteBuffer == null) {
				charBuffer = new char [m_encoding.GetMaxByteCount (MaxBufferSize)];
				charByteBuffer = new byte [MaxBufferSize];
			}

			//
			// We read the string here in small chunks. Also, we
			// Attempt to optimize the common case of short strings.
			//
			StringBuilder sb = null;
			do {
				int readLen = Math.Min (MaxBufferSize, len);
				
				int n = m_stream.Read (charByteBuffer, 0, readLen);
				if (n == 0)
					throw new EndOfStreamException();
				
				int cch = decoder.GetChars (charByteBuffer, 0, n, charBuffer, 0);

				if (sb == null && readLen == len) // ok, we got out the easy way, dont bother with the sb
					return new String (charBuffer, 0, cch);

				if (sb == null)
					// Len is a fairly good estimate of the number of chars in a string
					// Most of the time 1 byte == 1 char
					sb = new StringBuilder (len);
				
				sb.Append (charBuffer, 0, cch);
				len -= readLen;
			} while (len > 0);

			return sb.ToString();
		}

		public virtual float ReadSingle() {
			FillBuffer(4);

			return(BitConverterLE.ToSingle(m_buffer, 0));
		}

		[CLSCompliant(false)]
		public virtual ushort ReadUInt16() {
			FillBuffer(2);

			return((ushort) (m_buffer[0] | (m_buffer[1] << 8)));
		}

		[CLSCompliant(false)]
		public virtual uint ReadUInt32() {
			FillBuffer(4);
				

			return((uint) (m_buffer[0] |
				       (m_buffer[1] << 8) |
				       (m_buffer[2] << 16) |
				       (m_buffer[3] << 24)));
		}

		[CLSCompliant(false)]
		public virtual ulong ReadUInt64() {
			FillBuffer(8);

			uint ret_low  = (uint) (m_buffer[0]            |
			                       (m_buffer[1] << 8)  |
			                       (m_buffer[2] << 16) |
			                       (m_buffer[3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[4]        |
			                       (m_buffer[5] << 8)  |
			                       (m_buffer[6] << 16) |
			                       (m_buffer[7] << 24)
			                       );
			return (((ulong) ret_high) << 32) | ret_low;
		}

		/* Ensures that m_buffer is at least length bytes
		 * long, growing it if necessary
		 */
		private void CheckBuffer(int length)
		{
			if(m_buffer.Length <= length) {
				byte[] new_buffer=new byte[length];
				Buffer.BlockCopyInternal (m_buffer, 0, new_buffer, 0, m_buffer.Length);
				m_buffer=new_buffer;
			}
		}
	}
}
