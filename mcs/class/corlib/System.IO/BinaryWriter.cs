//
// System.IO.BinaryWriter
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

using System;
using System.Text;

namespace System.IO {
	public class BinaryWriter : IDisposable {
		public static readonly BinaryWriter Null;

		protected Stream OutStream;
		Encoding m_encoding;
		
		protected BinaryWriter() {
			m_encoding = Encoding.UTF8;
		}

		public BinaryWriter(Stream output) : this(output, Encoding.UTF8) {
		}

		public BinaryWriter(Stream output, Encoding encoding) {
			if (output == null || encoding == null) 
				throw new ArgumentNullException();
			if (!output.CanWrite)
				throw new ArgumentException();

			OutStream = output;
			m_encoding = encoding;
		}

		public virtual Stream BaseStream {
			get {
				return OutStream;
			}
		}

		public virtual void Close() {
			Dispose();
		}

		public virtual void Dispose() {
			OutStream.Close();
			OutStream.Dispose();			
		}

		public virtual void Flush() {
			OutStream.Flush();
		}

		public virtual long Seek(int offset, SeekOrigin origin) {
			return OutStream.Seek(offset, origin);
		}

		public virtual void Write(bool value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 1);
		}

		public virtual void Write(byte value) {
			OutStream.WriteByte(value);
		}

		public virtual void Write(byte[] value) {
			OutStream.Write(value, 0, value.Length);
		}

		public virtual void Write(byte[] value, int offset, int length) {
			OutStream.Write(value, offset, length);
		}

		public virtual void Write(char value) {
			char[] dec = new char[1];
			dec[0] = value;
			byte[] enc = m_encoding.GetBytes(dec, 0, 1);
			OutStream.Write(enc, 0, enc.Length);
		}
		
		public virtual void Write(char[] value) {
			byte[] enc = m_encoding.GetBytes(value, 0, value.Length);
			OutStream.Write(enc, 0, enc.Length);
		}

		public virtual void Write(char[] value, int offset, int length) {
			byte[] enc = m_encoding.GetBytes(value, offset, length);
			OutStream.Write(enc, 0, enc.Length);
		}

		unsafe public virtual void Write(decimal value) {
			byte[] to_write = new byte[16];
			byte* value_ptr = (byte *)&value;
			for (int i = 0; i < 16; i++) {
				to_write[i] = value_ptr[i];
			}

			OutStream.Write(to_write, 0, 16);
		}

		public virtual void Write(double value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 8);
		}
		
		public virtual void Write(short value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 2);
		}
		
		public virtual void Write(int value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 4);
		}

		public virtual void Write(long value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 8);
		}

		[CLSCompliant(false)]
		unsafe public virtual void Write(sbyte value) {
			byte[] to_write = new byte[1];

			to_write[0] = *(byte *)&value;
			OutStream.Write(to_write, 0, 1);
		}

		public virtual void Write(float value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 4);
		}

		public virtual void Write(string value) {
			Write7BitEncodedInt(value.Length);
			byte[] enc = m_encoding.GetBytes(value);
			OutStream.Write(enc, 0, enc.Length);
		}

		[CLSCompliant(false)]
		public virtual void Write(ushort value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 2);
		}

		[CLSCompliant(false)]
		public virtual void Write(uint value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 4);
		}

		[CLSCompliant(false)]
		public virtual void Write(ulong value) {
			OutStream.Write(BitConverter.GetBytes(value), 0, 8);
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
