//
// System.IO.BinaryReader
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

using System;
using System.Text;
using System.Globalization;

namespace System.IO {
	public class BinaryReader : IDisposable {
		Stream m_stream;
		Encoding m_encoding;
		int m_encoding_max_byte;

		byte[] m_buffer;
		int m_buffer_used;
		int m_buffer_pos;


		public BinaryReader(Stream input) : this(input, Encoding.UTF8) {
		}

		public BinaryReader(Stream input, Encoding encoding) {
			if (input == null || encoding == null) 
				throw new ArgumentNullException(Locale.GetText ("Input or Encoding is a null reference."));
			if (!input.CanRead)
				throw new ArgumentException(Locale.GetText ("The stream doesn't support reading."));

			m_stream = input;
			m_encoding = encoding;
			m_encoding_max_byte = m_encoding.GetMaxByteCount(1);
			m_buffer = new byte [32];
		}

		public virtual Stream BaseStream {
			get {
				return m_stream;
			}
		}

		public virtual void Close() {
			Dispose (true);
			m_stream.Close();
		}
		
		protected void Dispose (bool disposing)
		{
			if (disposing)
				m_stream.Close ();

			m_buffer = null;
			m_buffer_used = 0;
		}

		void IDisposable.Dispose() 
		{
			Dispose (true);
		}

		protected virtual void FillBuffer(int bytes) {
			if (!EnsureBuffered(m_buffer_used - m_buffer_pos + bytes)) {
				throw new EndOfStreamException();
			}
		}

		public virtual int PeekChar() {
			EnsureBuffered(m_encoding_max_byte);
			
			int i;
			for (i = 1; m_encoding.GetCharCount(m_buffer, m_buffer_pos, i) == 0; i++) {
				if (m_buffer_pos + i >= m_buffer_used) {
					return -1;
				}
			}

			char[] decode = m_encoding.GetChars(m_buffer, m_buffer_pos, i);
			return decode[0];
		}

		public virtual int Read() {
			char[] decode = new char[1];

			Read(decode, 0, 1);
			return decode[0];
		}

		public virtual int Read(byte[] buffer, int index, int count) {
			if (buffer == null) {
				throw new ArgumentNullException();
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException();
			}
			if (index < 0 || count < 0) {
				throw new ArgumentOutOfRangeException();
			}

			EnsureBuffered(count);
			
			if (m_buffer_used - m_buffer_pos < count) {
				count = m_buffer_used - m_buffer_pos;
			}

			Array.Copy(m_buffer, m_buffer_pos, buffer, index, count);

			ConsumeBuffered(count);
			return count;
		}

		public virtual int Read(char[] buffer, int index, int count) {
			if (buffer == null) {
				throw new ArgumentNullException();
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException();
			}
			if (index < 0 || count < 0) {
				throw new ArgumentOutOfRangeException();
			}

			EnsureBuffered(m_encoding_max_byte * count);

			int i;		
			for (i = 1; m_encoding.GetCharCount(m_buffer, m_buffer_pos, i) < count; i++) {
				if (m_buffer_pos + i >= m_buffer_used) {
					break;
				}
			}
			
			count = m_encoding.GetCharCount(m_buffer, m_buffer_pos, i);

			char[] dec = m_encoding.GetChars(m_buffer, m_buffer_pos, i);
			Array.Copy(dec, 0, buffer, index, count);

			ConsumeBuffered(i);
			return count;
		}

		protected int Read7BitEncodedInt() {
			int ret = 0;
			int shift = 0;
			int count = 0;
			byte b;

			do {
				if (!EnsureBuffered(++count)) {
					throw new EndOfStreamException();
				}
				b = m_buffer[m_buffer_pos + count - 1];
				
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			} while ((b & 0x80) == 0x80);

			ConsumeBuffered(count);
			return ret;
		}

		public virtual bool ReadBoolean() {
			if (!EnsureBuffered(1)) {
				throw new EndOfStreamException();
			}

			// Return value:
			//  true if the byte is non-zero; otherwise false.
			bool ret = (m_buffer[m_buffer_pos] != 0);
			ConsumeBuffered(1);
			return ret;
		}

		public virtual byte ReadByte() {
			if (!EnsureBuffered(1)) {
				throw new EndOfStreamException();
			}

			byte ret = m_buffer[m_buffer_pos];
			ConsumeBuffered(1);
			return ret;
		}

		public virtual byte[] ReadBytes(int count) {
			if (count < 0) {
				throw new ArgumentOutOfRangeException();
			}

			EnsureBuffered(count);

			if (count > m_buffer_used - m_buffer_pos) {
				count = m_buffer_used - m_buffer_pos;
			}

			if (count == 0) {
				throw new EndOfStreamException();
			}

			byte[] buf = new byte[count];
			Read(buf, 0, count);
			return buf;
		}

		public virtual char ReadChar() {
			char[] buf = ReadChars(1);
			return buf[0];
		}

		public virtual char[] ReadChars(int count) {
			if (count < 0) {
				throw new ArgumentOutOfRangeException();
			}

			char[] full = new char[count];
			count = Read(full, 0, count);
			
			if (count != full.Length) {
				char[] ret = new char[count];
				Array.Copy(full, 0, ret, 0, count);
				return ret;
			} else {
				return full;
			}
		}

		unsafe public virtual decimal ReadDecimal() {
			if (!EnsureBuffered(16)) {
				throw new EndOfStreamException();
			}

			decimal ret;
			byte* ret_ptr = (byte *)&ret;
			for (int i = 0; i < 16; i++) {
				ret_ptr[i] = m_buffer[m_buffer_pos + i];
			}

			ConsumeBuffered(16);
			return ret;
		}

		public virtual double ReadDouble() {
			if (!EnsureBuffered(8)) {
				throw new EndOfStreamException();
			}

			double ret = BitConverter.ToDouble(m_buffer, m_buffer_pos);
			ConsumeBuffered(8);
			return ret;
		}

		public virtual short ReadInt16() {
			if (!EnsureBuffered(2)) {
				throw new EndOfStreamException();
			}

			short ret = (short) (m_buffer[m_buffer_pos] | (m_buffer[m_buffer_pos + 1] << 8));
			ConsumeBuffered(2);
			return ret;
		}

		public virtual int ReadInt32() {
			if (!EnsureBuffered(4)) {
				throw new EndOfStreamException();
			}

			int ret = (m_buffer[m_buffer_pos]             |
			           (m_buffer[m_buffer_pos + 1] << 8)  |
			           (m_buffer[m_buffer_pos + 2] << 16) |
			           (m_buffer[m_buffer_pos + 3] << 24)
			          );
			ConsumeBuffered(4);
			return ret;
		}

		public virtual long ReadInt64() {
			if (!EnsureBuffered(8)) {
				throw new EndOfStreamException();
			}

			uint ret_low  = (uint) (m_buffer[m_buffer_pos]            |
			                       (m_buffer[m_buffer_pos + 1] << 8)  |
			                       (m_buffer[m_buffer_pos + 2] << 16) |
			                       (m_buffer[m_buffer_pos + 3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[m_buffer_pos + 4]        |
			                       (m_buffer[m_buffer_pos + 5] << 8)  |
			                       (m_buffer[m_buffer_pos + 6] << 16) |
			                       (m_buffer[m_buffer_pos + 7] << 24)
			                       );
			ConsumeBuffered(8);
			return (long) ((((ulong) ret_high) << 32) | ret_low);
		}

		[CLSCompliant(false)]
		unsafe public virtual sbyte ReadSByte() {
			if (!EnsureBuffered(1)) {
				throw new EndOfStreamException();
			}

			sbyte ret;
			byte* ret_ptr = (byte *)&ret;
			ret_ptr[0] = m_buffer[m_buffer_pos];

			ConsumeBuffered(1);
			return ret;
		}

		public virtual string ReadString() {
			int len = Read7BitEncodedInt();

			char[] str = ReadChars(len);
			string ret = "";
			for (int i = 0; i < str.Length; i++) {
				ret = ret + str[i];
			}

			return ret;
		}

		public virtual float ReadSingle() {
			if (!EnsureBuffered(4)) {
				throw new EndOfStreamException();
			}

			float ret = BitConverter.ToSingle(m_buffer, m_buffer_pos);
			ConsumeBuffered(4);
			return ret;
		}

		[CLSCompliant(false)]
		public virtual ushort ReadUInt16() {
			if (!EnsureBuffered(2)) {
				throw new EndOfStreamException();
			}

			ushort ret = (ushort) (m_buffer[m_buffer_pos] | (m_buffer[m_buffer_pos + 1] << 8));
			ConsumeBuffered(2);
			return ret;
		}

		[CLSCompliant(false)]
		public virtual uint ReadUInt32() {
			if (!EnsureBuffered(4)) {
				throw new EndOfStreamException();
			}

			uint ret = (uint) (m_buffer[m_buffer_pos]            |
			                  (m_buffer[m_buffer_pos + 1] << 8)  |
			                  (m_buffer[m_buffer_pos + 2] << 16) |
			                  (m_buffer[m_buffer_pos + 3] << 24)
			                  );
			ConsumeBuffered(4);
			return ret;
		}

		[CLSCompliant(false)]
		public virtual ulong ReadUInt64() {
			if (!EnsureBuffered(8)) {
				throw new EndOfStreamException();
			}

			uint ret_low  = (uint) (m_buffer[m_buffer_pos]            |
			                       (m_buffer[m_buffer_pos + 1] << 8)  |
			                       (m_buffer[m_buffer_pos + 2] << 16) |
			                       (m_buffer[m_buffer_pos + 3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[m_buffer_pos + 4]        |
			                       (m_buffer[m_buffer_pos + 5] << 8)  |
			                       (m_buffer[m_buffer_pos + 6] << 16) |
			                       (m_buffer[m_buffer_pos + 7] << 24)
			                       );
			ConsumeBuffered(8);
			return (((ulong) ret_high) << 32) | ret_low;
		}

		
		bool EnsureBuffered(int bytes) {
			int needed = bytes - (m_buffer_used - m_buffer_pos);
			if (needed < 0)
				return true;

			if (m_buffer_used + needed > m_buffer.Length) {
				byte[] old_buffer = m_buffer;
				m_buffer = new byte[m_buffer_used + needed];
				Array.Copy(old_buffer, 0, m_buffer, 0, m_buffer_used);
				m_buffer_pos = m_buffer_used;
			}

			int n = m_stream.Read(m_buffer, m_buffer_used, needed);
			if (n == 0) return false;

			m_buffer_used += n;

			return (m_buffer_used >= m_buffer_pos + bytes);
		}


		void ConsumeBuffered(int bytes) {
			m_buffer_pos += bytes;
		}
	}
}
