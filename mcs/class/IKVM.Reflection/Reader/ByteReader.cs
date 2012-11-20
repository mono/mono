/*
  Copyright (C) 2009 Jeroen Frijters

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

namespace IKVM.Reflection.Reader
{
	sealed class ByteReader
	{
		private byte[] buffer;
		private int pos;
		private int end;

		internal ByteReader(byte[] buffer, int offset, int length)
		{
			this.buffer = buffer;
			this.pos = offset;
			this.end = pos + length;
		}

		internal static ByteReader FromBlob(byte[] blobHeap, int blob)
		{
			ByteReader br = new ByteReader(blobHeap, blob, 4);
			int length = br.ReadCompressedUInt();
			br.end = br.pos + length;
			return br;
		}

		internal int Length
		{
			get { return end - pos; }
		}

		internal byte PeekByte()
		{
			if (pos == end)
				throw new BadImageFormatException();
			return buffer[pos];
		}

		internal byte ReadByte()
		{
			if (pos == end)
				throw new BadImageFormatException();
			return buffer[pos++];
		}

		internal byte[] ReadBytes(int count)
		{
			if (count < 0)
				throw new BadImageFormatException();
			if (end - pos < count)
				throw new BadImageFormatException();
			byte[] buf = new byte[count];
			Buffer.BlockCopy(buffer, pos, buf, 0, count);
			pos += count;
			return buf;
		}

		internal int ReadCompressedUInt()
		{
			byte b1 = ReadByte();
			if (b1 <= 0x7F)
			{
				return b1;
			}
			else if ((b1 & 0xC0) == 0x80)
			{
				byte b2 = ReadByte();
				return ((b1 & 0x3F) << 8) | b2;
			}
			else
			{
				byte b2 = ReadByte();
				byte b3 = ReadByte();
				byte b4 = ReadByte();
				return ((b1 & 0x3F) << 24) + (b2 << 16) + (b3 << 8) + b4;
			}
		}

		internal int ReadCompressedInt()
		{
			byte b1 = PeekByte();
			int value = ReadCompressedUInt();
			if ((value & 1) == 0)
			{
				return value >> 1;
			}
			else
			{
				switch (b1 & 0xC0)
				{
					case 0:
					case 0x40:
						return (value >> 1) - 0x40;
					case 0x80:
						return (value >> 1) - 0x2000;
					default:
						return (value >> 1) - 0x10000000;
				}
			}
		}

		internal string ReadString()
		{
			if (PeekByte() == 0xFF)
			{
				pos++;
				return null;
			}
			int length = ReadCompressedUInt();
			string str = Encoding.UTF8.GetString(buffer, pos, length);
			pos += length;
			return str;
		}

		internal char ReadChar()
		{
			return (char)ReadInt16();
		}

		internal sbyte ReadSByte()
		{
			return (sbyte)ReadByte();
		}

		internal short ReadInt16()
		{
			if (end - pos < 2)
				throw new BadImageFormatException();
			byte b1 = buffer[pos++];
			byte b2 = buffer[pos++];
			return (short)(b1 | (b2 << 8));
		}

		internal ushort ReadUInt16()
		{
			return (ushort)ReadInt16();
		}

		internal int ReadInt32()
		{
			if (end - pos < 4)
				throw new BadImageFormatException();
			byte b1 = buffer[pos++];
			byte b2 = buffer[pos++];
			byte b3 = buffer[pos++];
			byte b4 = buffer[pos++];
			return (int)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
		}

		internal uint ReadUInt32()
		{
			return (uint)ReadInt32();
		}

		internal long ReadInt64()
		{
			ulong lo = ReadUInt32();
			ulong hi = ReadUInt32();
			return (long)(lo | (hi << 32));
		}

		internal ulong ReadUInt64()
		{
			return (ulong)ReadInt64();
		}

		internal float ReadSingle()
		{
			return SingleConverter.Int32BitsToSingle(ReadInt32());
		}

		internal double ReadDouble()
		{
			return BitConverter.Int64BitsToDouble(ReadInt64());
		}

		internal ByteReader Slice(int length)
		{
			if (end - pos < length)
				throw new BadImageFormatException();
			ByteReader br = new ByteReader(buffer, pos, length);
			pos += length;
			return br;
		}

		// NOTE this method only works if the original offset was aligned and for alignments that are a power of 2
		internal void Align(int alignment)
		{
			alignment--;
			pos = (pos + alignment) & ~alignment;
		}
	}
}
