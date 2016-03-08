//
// TlsBuffer.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014-2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Mono.Security.Interface
{
	public class TlsBuffer : SecretParameters
	{
		public int Position {
			get; set;
		}

		public int Remaining {
			get { return Size - (Position - Offset); }
		}

		public byte[] Buffer {
			get { return innerBuffer.Buffer; }
		}

		public int Offset {
			get { return innerBuffer.Offset; }
		}

		public int Size {
			get { return innerBuffer.Size; }
		}

		public int EndOffset {
			get { return Offset + Size; }
		}

		IBufferOffsetSize innerBuffer;

		protected TlsBuffer ()
			: this (null, 0, 0)
		{
		}

		public TlsBuffer (IBufferOffsetSize bos)
		{
			innerBuffer = bos;
			Position = bos.Offset;
		}

		public TlsBuffer (byte[] buffer, int offset, int size)
			: this (new BufferOffsetSize (buffer, offset, size))
		{
		}

		public TlsBuffer (byte[] buffer)
			: this (buffer, 0, buffer.Length)
		{
		}

		public TlsBuffer (int size)
			: this (new byte [size], 0, size)
		{
		}

		public byte ReadByte ()
		{
			if (Position >= EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			return Buffer [Position++];
		}

		public short ReadInt16 ()
		{
			if (Position + 1 >= EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			var retval = (short)(Buffer [Position] << 8 | Buffer [Position + 1]);
			Position += 2;
			return retval;
		}

		public int ReadInt24 ()
		{
			if (Position + 2 >= EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			var retval = ((Buffer [Position] << 16) | (Buffer [Position+1] << 8) | Buffer [Position+2]);
			Position += 3;
			return retval;
		}

		public int ReadInt32 ()
		{
			if (Position + 3 >= EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			var retval = ((Buffer [Position] << 24) | (Buffer [Position+1] << 16) | (Buffer [Position+2] << 8) | Buffer [Position+3]);
			Position += 4;
			return retval;
		}

		public TlsBuffer ReadBuffer (int length)
		{
			if (Position + length > EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			var retval = new TlsBuffer (Buffer, Position, length);
			Position += length;
			return retval;
		}

		public IBufferOffsetSize GetRemaining ()
		{
			return new BufferOffsetSize (Buffer, Position, Remaining);
		}

		protected virtual void MakeRoomInternal (int size)
		{
			if (Position + size > EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
		}

		public void Write (byte value)
		{
			MakeRoomInternal (1);
			Buffer [Position++] = value;
		}

		public void Write (short value)
		{
			MakeRoomInternal (2);
			WriteInt16 (Buffer, Position, value);
			Position += 2;
		}

		public static void WriteInt16 (byte[] buffer, int offset, short value)
		{
			buffer[offset] = ((byte)(value >> 8));
			buffer[offset+1] = ((byte)value);
		}

		public void Write (int value)
		{
			MakeRoomInternal (4);
			WriteInt32 (Buffer, Position, value);
			Position += 4;
		}

		public void WriteInt24 (int value)
		{
			MakeRoomInternal (3);
			WriteInt24 (Buffer, Position, value);
			Position += 3;
		}

		#pragma warning disable 3001
		public void Write (ulong value)
		#pragma warning restore 3001
		{
			MakeRoomInternal (8);
			WriteInt64 (Buffer, Position, value);
			Position += 8;
		}

		public static void WriteInt24 (byte[] buffer, int offset, int value)
		{
			buffer[offset] = ((byte)(value >> 16));
			buffer[offset+1] = ((byte)(value >> 8));
			buffer[offset+2] = ((byte)value);
		}

		public static void WriteInt32 (byte[] buffer, int offset, int value)
		{
			buffer[offset] = ((byte)(value >> 24));
			buffer[offset+1] = ((byte)(value >> 16));
			buffer[offset+2] = ((byte)(value >> 8));
			buffer[offset+3] = ((byte)value);
		}

		#pragma warning disable 3001
		public static void WriteInt64 (byte[] buffer, int offset, ulong value)
		#pragma warning restore 3001
		{
			buffer[offset] = (byte) (value >> 56);
			buffer[offset+1] = (byte) (value >> 48);
			buffer[offset+2] = (byte) (value >> 40);
			buffer[offset+3] = (byte) (value >> 32);
			buffer[offset+4] = (byte) (value >> 24);
			buffer[offset+5] = (byte) (value >> 16);
			buffer[offset+6] = (byte) (value >> 8);
			buffer[offset+7] = (byte) value;
		}

		public void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		public void Write (byte[] buffer, int offset, int size)
		{
			MakeRoomInternal (size);
			Array.Copy (buffer, offset, Buffer, Position, size);
			Position += size;
		}

		public void Write (IBufferOffsetSize buffer)
		{
			Write (buffer.Buffer, buffer.Offset, buffer.Size);
		}

		public SecureBuffer ReadSecureBuffer (int count)
		{
			return new SecureBuffer (ReadBytes (count));
		}

		public byte[] ReadBytes (int count)
		{
			if (Position + count > EndOffset)
				throw new TlsException (AlertDescription.DecodeError, "Buffer overflow");
			var retval = new byte [count];
			Array.Copy (Buffer, Position, retval, 0, count);
			Position += count;
			return retval;
		}

		internal static bool Compare (SecureBuffer buffer1, SecureBuffer buffer2)
		{
			if (buffer1 == null || buffer2 == null)
				return false;

			if (buffer1.Size != buffer2.Size)
				return false;

			for (int i = 0; i < buffer1.Size; i++) {
				if (buffer1.Buffer [i] != buffer2.Buffer [i])
					return false;
			}
			return true;
		}

		public static bool Compare (IBufferOffsetSize buffer1, IBufferOffsetSize buffer2)
		{
			if (buffer1 == null || buffer2 == null)
				return false;

			if (buffer1.Size != buffer2.Size)
				return false;

			for (int i = 0; i < buffer1.Size; i++) {
				if (buffer1.Buffer [buffer1.Offset + i] != buffer2.Buffer [buffer2.Offset + i])
					return false;
			}
			return true;
		}

		public static bool Compare (byte[] buffer1, byte[] buffer2)
		{
			if (buffer1 == null || buffer2 == null)
				return false;

			return Compare (buffer1, 0, buffer1.Length, buffer2, 0, buffer2.Length);
		}

		public static bool Compare (byte[] buffer1, int offset1, int size1, byte[] buffer2, int offset2, int size2)
		{
			if (buffer1 == null || buffer2 == null)
				return false;

			if (size1 != size2)
				return false;

			for (int i = 0; i < size1; i++) {
				if (buffer1 [offset1 + i] != buffer2 [offset2 + i])
					return false;
			}
			return true;

		}

		public static int ConstantTimeCompare (byte[] buffer1, int offset1, int size1, byte[] buffer2, int offset2, int size2)
		{
			int status = 0;
			int effectiveSize;
			if (size1 < size2) {
				status--;
				effectiveSize = size1;
			} else if (size2 < size1) {
				status--;
				effectiveSize = size2;
			} else {
				effectiveSize = size1;
			}

			for (int i = 0; i < effectiveSize; i++) {
				if (buffer1 [offset1 + i] != buffer2 [offset2 + i])
					status--;
			}

			return status;
		}

		protected void SetBuffer (byte[] buffer, int offset, int size)
		{
			innerBuffer = new BufferOffsetSize (buffer, offset, size);
		}

		protected override void Clear ()
		{
			var disposable = innerBuffer as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			innerBuffer = null;
			Position = 0;
		}

		public static readonly byte[] EmptyArray = new byte [0];
	}
}

