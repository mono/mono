//
// TlsMultiBuffer.cs
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
	public class TlsMultiBuffer
	{
		MemoryChunk first, last;

		private class MemoryChunk : BufferOffsetSize
		{
			public MemoryChunk next;

			public MemoryChunk (byte[] buffer, int offset, int size)
				: base (buffer, offset, size)
			{
			}
		}

		public bool IsEmpty {
			get { return first == null; }
		}

		public bool IsSingle {
			get { return first != null && first.next == null; }
		}

		public void Add (TlsBuffer buffer)
		{
			Add (buffer.Buffer, buffer.Offset, buffer.Size);
		}

		public void Add (byte[] buffer)
		{
			Add (buffer, 0, buffer.Length);
		}

		public void Add (byte[] buffer, int offset, int size)
		{
			var chunk = new MemoryChunk (buffer, offset, size);
			if (last == null)
				first = last = chunk;
			else {
				last.next = chunk;
				last = chunk;
			}
		}

		public BufferOffsetSize[] GetBufferArray ()
		{
			int count = 0;
			for (var ptr = first; ptr != null; ptr = ptr.next)
				count++;
			var array = new BufferOffsetSize [count];
			count = 0;
			for (var ptr = first; ptr != null; ptr = ptr.next)
				array [count++] = ptr;
			return array;
		}

		public void Clear ()
		{
			for (var ptr = first; ptr != null; ptr = ptr.next)
				ptr.Dispose ();
			first = last = null;
		}

		public BufferOffsetSize GetBuffer ()
		{
			int totalSize = 0;
			for (var ptr = first; ptr != null; ptr = ptr.next)
				totalSize += ptr.Size;

			var outBuffer = new BufferOffsetSize (new byte [totalSize]);
			int offset = 0;
			for (var ptr = first; ptr != null; ptr = ptr.next) {
				Buffer.BlockCopy (ptr.Buffer, ptr.Offset, outBuffer.Buffer, offset, ptr.Size);
				offset += ptr.Size;
			}
			return outBuffer;
		}

		public BufferOffsetSize StealBuffer ()
		{
			if (IsSingle) {
				var retval = first;
				first = last = null;
				return retval;
			}

			return GetBuffer ();
		}
	}
}

