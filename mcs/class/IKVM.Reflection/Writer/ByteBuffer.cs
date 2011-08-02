/*
  Copyright (C) 2008 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	sealed class ByteBuffer
	{
		private byte[] buffer;
		private int pos;
		private int __length;	// __length is only valid if > pos, otherwise pos is the current length

		internal ByteBuffer(int initialCapacity)
		{
			buffer = new byte[initialCapacity];
		}

		private ByteBuffer(byte[] wrap, int length)
		{
			this.buffer = wrap;
			this.pos = length;
		}

		internal int Position
		{
			get { return pos; }
			set
			{
				if (value > this.Length || value > buffer.Length)
					throw new ArgumentOutOfRangeException();
				__length = Math.Max(__length, pos);
				pos = value;
			}
		}

		internal int Length
		{
			get { return Math.Max(pos, __length); }
		}

		private void Grow(int minGrow)
		{
			byte[] newbuf = new byte[Math.Max(buffer.Length + minGrow, buffer.Length * 2)];
			Buffer.BlockCopy(buffer, 0, newbuf, 0, buffer.Length);
			buffer = newbuf;
		}

		// NOTE this does not advance the position
		internal int GetInt32AtCurrentPosition()
		{
			return buffer[pos]
				+ (buffer[pos + 1] << 8)
				+ (buffer[pos + 2] << 16)
				+ (buffer[pos + 3] << 24);
		}

		// NOTE this does not advance the position
		internal byte GetByteAtCurrentPosition()
		{
			return buffer[pos];
		}

		internal void Write(byte[] value)
		{
			if (pos + value.Length > buffer.Length)
				Grow(value.Length);
			Buffer.BlockCopy(value, 0, buffer, pos, value.Length);
			pos += value.Length;
		}

		internal void Write(byte value)
		{
			if (pos == buffer.Length)
				Grow(1);
			buffer[pos++] = value;
		}

		internal void Write(sbyte value)
		{
			Write((byte)value);
		}

		internal void Write(ushort value)
		{
			Write((short)value);
		}

		internal void Write(short value)
		{
			if (pos + 2 > buffer.Length)
				Grow(2);
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
		}

		internal void Write(uint value)
		{
			Write((int)value);
		}
	
		internal void Write(int value)
		{
			if (pos + 4 > buffer.Length)
				Grow(4);
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
			buffer[pos++] = (byte)(value >> 16);
			buffer[pos++] = (byte)(value >> 24);
		}

		internal void Write(ulong value)
		{
			Write((long)value);
		}

		internal void Write(long value)
		{
			if (pos + 8 > buffer.Length)
				Grow(8);
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 8);
			buffer[pos++] = (byte)(value >> 16);
			buffer[pos++] = (byte)(value >> 24);
			buffer[pos++] = (byte)(value >> 32);
			buffer[pos++] = (byte)(value >> 40);
			buffer[pos++] = (byte)(value >> 48);
			buffer[pos++] = (byte)(value >> 56);
		}

		internal void Write(float value)
		{
			Write(SingleConverter.SingleToInt32Bits(value));
		}

		internal void Write(double value)
		{
			Write(BitConverter.DoubleToInt64Bits(value));
		}

		internal void Write(string str)
		{
			if (str == null)
			{
				Write((byte)0xFF);
			}
			else
			{
				byte[] buf = Encoding.UTF8.GetBytes(str);
				WriteCompressedInt(buf.Length);
				Write(buf);
			}
		}

		internal void WriteCompressedInt(int value)
		{
			if (value <= 0x7F)
			{
				Write((byte)value);
			}
			else if (value <= 0x3FFF)
			{
				Write((byte)(0x80 | (value >> 8)));
				Write((byte)value);
			}
			else
			{
				Write((byte)(0xC0 | (value >> 24)));
				Write((byte)(value >> 16));
				Write((byte)(value >> 8));
				Write((byte)value);
			}
		}

		internal void Write(ByteBuffer bb)
		{
			if (pos + bb.Length > buffer.Length)
				Grow(bb.Length);
			Buffer.BlockCopy(bb.buffer, 0, buffer, pos, bb.Length);
			pos += bb.Length;
		}

		internal void WriteTo(System.IO.Stream stream)
		{
			stream.Write(buffer, 0, this.Length);
		}

		internal void Clear()
		{
			pos = 0;
			__length = 0;
		}

		internal void Align(int alignment)
		{
			if (pos + alignment > buffer.Length)
				Grow(alignment);
			int newpos = (pos + alignment - 1) & ~(alignment - 1);
			while (pos < newpos)
				buffer[pos++] = 0;
		}

		internal void WriteTypeDefOrRefEncoded(int token)
		{
			switch (token >> 24)
			{
				case TypeDefTable.Index:
					WriteCompressedInt((token & 0xFFFFFF) << 2 | 0);
					break;
				case TypeRefTable.Index:
					WriteCompressedInt((token & 0xFFFFFF) << 2 | 1);
					break;
				case TypeSpecTable.Index:
					WriteCompressedInt((token & 0xFFFFFF) << 2 | 2);
					break;
				default:
					throw new InvalidOperationException();
			}
		}

		internal void Write(System.IO.Stream stream)
		{
			const int chunkSize = 8192;
			for (; ; )
			{
				if (pos + chunkSize > buffer.Length)
					Grow(chunkSize);
				int read = stream.Read(buffer, pos, chunkSize);
				if (read <= 0)
				{
					break;
				}
				pos += read;
			}
		}

		internal byte[] ToArray()
		{
			byte[] buf = new byte[pos];
			Buffer.BlockCopy(buffer, 0, buf, 0, pos);
			return buf;
		}

		internal static ByteBuffer Wrap(byte[] buf)
		{
			return new ByteBuffer(buf, buf.Length);
		}

		internal static ByteBuffer Wrap(byte[] buf, int length)
		{
			return new ByteBuffer(buf, length);
		}

		internal bool Match(int pos, ByteBuffer bb2, int pos2, int len)
		{
			for (int i = 0; i < len; i++)
			{
				if (buffer[pos + i] != bb2.buffer[pos2 + i])
				{
					return false;
				}
			}
			return true;
		}

		internal int Hash()
		{
			int hash = 0;
			int len = this.Length;
			for (int i = 0; i < len; i++)
			{
				hash *= 37;
				hash ^= buffer[i];
			}
			return hash;
		}

		internal IKVM.Reflection.Reader.ByteReader GetBlob(int offset)
		{
			return IKVM.Reflection.Reader.ByteReader.FromBlob(buffer, offset);
		}

		internal void Patch(int offset, byte b)
		{
			buffer[offset] = b;
		}
	}
}
