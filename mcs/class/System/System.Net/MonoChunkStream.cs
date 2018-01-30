//
// MonoChunkStream.cs
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
	class MonoChunkStream : WebReadStream
	{
		protected WebHeaderCollection Headers {
			get;
		}

		protected MonoChunkParser Decoder {
			get;
		}

		public MonoChunkStream (WebOperation operation, Stream innerStream,
		                         WebHeaderCollection headers)
			: base (operation, innerStream)
		{
			Headers = headers;

			Decoder = new MonoChunkParser (headers);
		}

		public override async Task<int> ReadAsync (byte[] buffer, int offset, int size,
		                                           CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			if (Decoder.DataAvailable)
				return Decoder.Read (buffer, offset, size);

			int ret = 0;
			byte[] moreBytes = null;
			while (ret == 0 && Decoder.WantMore) {
				int localSize = Decoder.ChunkLeft;
				if (localSize <= 0) // not read chunk size yet
					localSize = 1024;
				else if (localSize > 16384)
					localSize = 16384;

				if (moreBytes == null || moreBytes.Length < localSize)
					moreBytes = new byte[localSize];

				ret = await InnerStream.ReadAsync (
					moreBytes, 0, localSize, cancellationToken).ConfigureAwait (false);

				if (ret <= 0)
					return ret;

				Decoder.Write (moreBytes, 0, ret);
				ret = Decoder.Read (buffer, offset, size);
			}

			return ret;
		}
	}
}

