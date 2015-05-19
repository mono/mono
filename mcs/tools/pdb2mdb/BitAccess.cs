//-----------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//-----------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace Microsoft.Cci.Pdb {
  internal class BitAccess {

    internal BitAccess(int capacity) {
      this.buffer = new byte[capacity];
      this.offset = 0;
    }

    internal byte[] Buffer {
      get { return buffer; }
    }
    private byte[] buffer;

    internal void FillBuffer(Stream stream, int capacity) {
      MinCapacity(capacity);
      stream.Read(buffer, 0, capacity);
      offset = 0;
    }

    internal int Position {
      get { return offset; }
      set { offset = value; }
    }
    private int offset;

    internal void WriteBuffer(Stream stream, int count) {
      stream.Write(buffer, 0, count);
    }

    internal void MinCapacity(int capacity) {
      if (buffer.Length < capacity) {
        buffer = new byte[capacity];
      }
      offset = 0;
    }

    internal void Align(int alignment) {
      while ((offset % alignment) != 0) {
        offset++;
      }
    }

    internal void WriteInt32(int value) {
      buffer[offset + 0] = (byte)value;
      buffer[offset + 1] = (byte)(value >> 8);
      buffer[offset + 2] = (byte)(value >> 16);
      buffer[offset + 3] = (byte)(value >> 24);
      offset += 4;
    }

    internal void WriteInt32(int[] values) {
      for (int i = 0; i < values.Length; i++) {
        WriteInt32(values[i]);
      }
    }

    internal void WriteBytes(byte[] bytes) {
      for (int i = 0; i < bytes.Length; i++) {
        buffer[offset++] = bytes[i];
      }
    }

    internal void ReadInt16(out short value) {
      value = (short)((buffer[offset + 0] & 0xFF) |
                            (buffer[offset + 1] << 8));
      offset += 2;
    }

    internal void ReadInt32(out int value) {
      value = (int)((buffer[offset + 0] & 0xFF) |
                          (buffer[offset + 1] << 8) |
                          (buffer[offset + 2] << 16) |
                          (buffer[offset + 3] << 24));
      offset += 4;
    }

    internal void ReadInt64(out long value) {
      value = (long)((buffer[offset + 0] & 0xFF) |
                           (buffer[offset + 1] << 8) |
                           (buffer[offset + 2] << 16) |
                           (buffer[offset + 3] << 24) |
                           (buffer[offset + 4] << 32) |
                           (buffer[offset + 5] << 40) |
                           (buffer[offset + 6] << 48) |
                           (buffer[offset + 7] << 56));
      offset += 8;
    }

    internal void ReadUInt16(out ushort value) {
      value = (ushort)((buffer[offset + 0] & 0xFF) |
                             (buffer[offset + 1] << 8));
      offset += 2;
    }

    internal void ReadUInt8(out byte value) {
      value = (byte)((buffer[offset + 0] & 0xFF));
      offset += 1;
    }

    internal void ReadUInt32(out uint value) {
      value = (uint)((buffer[offset + 0] & 0xFF) |
                           (buffer[offset + 1] << 8) |
                           (buffer[offset + 2] << 16) |
                           (buffer[offset + 3] << 24));
      offset += 4;
    }

    internal void ReadUInt64(out ulong value) {
      value = (ulong)((buffer[offset + 0] & 0xFF) |
                           (buffer[offset + 1] << 8) |
                           (buffer[offset + 2] << 16) |
                           (buffer[offset + 3] << 24) |
                           (buffer[offset + 4] << 32) |
                           (buffer[offset + 5] << 40) |
                           (buffer[offset + 6] << 48) |
                           (buffer[offset + 7] << 56));
      offset += 8;
    }

    internal void ReadInt32(int[] values) {
      for (int i = 0; i < values.Length; i++) {
        ReadInt32(out values[i]);
      }
    }

    internal void ReadUInt32(uint[] values) {
      for (int i = 0; i < values.Length; i++) {
        ReadUInt32(out values[i]);
      }
    }

    internal void ReadBytes(byte[] bytes) {
      for (int i = 0; i < bytes.Length; i++) {
        bytes[i] = buffer[offset++];
      }
    }

    internal float ReadFloat() {
      float result = BitConverter.ToSingle(buffer, offset);
      offset += 4;
      return result;
    }

    internal double ReadDouble() {
      double result = BitConverter.ToDouble(buffer, offset);
      offset += 8;
      return result;
    }

    internal decimal ReadDecimal() {
      int[] bits = new int[4];
      this.ReadInt32 (bits);
      try {
        bool sign = (bits[3] & 0x80000000) != 0;
        byte scale = (byte)((bits[3] >> 16) & 0x7F);
        return new decimal (bits[0], bits[1], bits[2], sign, scale);
      } catch (ArgumentException) {
        return new decimal ();
      }
    }

    internal void ReadBString(out string value) {
      ushort len;
      this.ReadUInt16(out len);
      value = Encoding.UTF8.GetString(buffer, offset, len);
      offset += len;
    }

    internal void ReadCString(out string value) {
      int len = 0;
      while (offset + len < buffer.Length && buffer[offset + len] != 0) {
        len++;
      }
      value = Encoding.UTF8.GetString(buffer, offset, len);
      offset += len + 1;
    }

    internal void SkipCString(out string value) {
      int len = 0;
      while (offset + len < buffer.Length && buffer[offset + len] != 0) {
        len++;
      }
      offset += len + 1;
      value= null;
    }

    internal void ReadGuid(out Guid guid) {
      uint a;
      ushort b;
      ushort c;
      byte d;
      byte e;
      byte f;
      byte g;
      byte h;
      byte i;
      byte j;
      byte k;

      ReadUInt32(out a);
      ReadUInt16(out b);
      ReadUInt16(out c);
      ReadUInt8(out d);
      ReadUInt8(out e);
      ReadUInt8(out f);
      ReadUInt8(out g);
      ReadUInt8(out h);
      ReadUInt8(out i);
      ReadUInt8(out j);
      ReadUInt8(out k);

      guid = new Guid(a, b, c, d, e, f, g, h, i, j, k);
    }

    internal string ReadString() {
      int len = 0;
      while (offset + len < buffer.Length && buffer[offset + len] != 0) {
        len+=2;
      }
      string result = Encoding.Unicode.GetString(buffer, offset, len);
      offset += len + 2;
      return result;
    }

  }
}
