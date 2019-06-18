//
// BufferedReadStream.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net
{
	class BufferedReadStream : WebReadStream
	{
		readonly BufferOffsetSize readBuffer;

		public BufferedReadStream (WebOperation operation, Stream innerStream,
		                           BufferOffsetSize readBuffer)
			: base (operation, innerStream)
		{
			this.readBuffer = readBuffer;
		}

		protected override async Task<int> ProcessReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			var remaining = readBuffer?.Size ?? 0;
			if (remaining > 0) {
				int copy = (remaining > size) ? size : remaining;
				Buffer.BlockCopy (readBuffer.Buffer, readBuffer.Offset, buffer, offset, copy);
				readBuffer.Offset += copy;
				readBuffer.Size -= copy;
				offset += copy;
				size -= copy;
				return copy;
			}

			if (InnerStream == null)
				return 0;

			return await InnerStream.ReadAsync (
				buffer, offset, size, cancellationToken).ConfigureAwait (false);
		}

		internal bool TryReadFromBuffer (byte[] buffer, int offset, int size, out int result)
		{
			var remaining = readBuffer?.Size ?? 0;
			if (remaining <= 0) {
				result = 0;
				return InnerStream == null;
			}

			int copy = (remaining > size) ? size : remaining;
			Buffer.BlockCopy (readBuffer.Buffer, readBuffer.Offset, buffer, offset, copy);
			readBuffer.Offset += copy;
			readBuffer.Size -= copy;
			offset += copy;
			size -= copy;
			result = copy;
			return true;
		}
	}
}
