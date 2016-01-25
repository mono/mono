//
// BufferOffsetSize.cs
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
	public class BufferOffsetSize : SecretParameters, IBufferOffsetSize
	{
		public byte[] Buffer {
			get;
			private set;
		}

		public int Offset {
			get;
			internal set;
		}

		public int Size {
			get { return EndOffset - Offset; }
		}

		public int EndOffset {
			get;
			internal set;
		}

		public BufferOffsetSize (byte[] buffer, int offset, int size)
		{
			Buffer = buffer;
			Offset = offset;
			EndOffset = offset + size;
		}

		public BufferOffsetSize (byte[] buffer)
			: this (buffer, 0, buffer.Length)
		{
		}

		public BufferOffsetSize (int size)
			: this (new byte [size])
		{
		}

		public byte[] GetBuffer ()
		{
			var copy = new byte [Size];
			Array.Copy (Buffer, Offset, copy, 0, Size);
			return copy;
		}

		public void TruncateTo (int newSize)
		{
			if (newSize > Size)
				throw new ArgumentException ("newSize");
			EndOffset = Offset + newSize;
		}

		protected void SetBuffer (byte[] buffer, int offset, int size)
		{
			Buffer = buffer;
			Offset = offset;
			EndOffset = offset + size;
		}

		protected override void Clear ()
		{
			Buffer = null;
			Offset = EndOffset = 0;
		}
	}
}

