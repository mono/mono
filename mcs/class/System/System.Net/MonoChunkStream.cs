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

		protected override async Task<int> ProcessReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

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

		internal override async Task FinishReading (CancellationToken cancellationToken)
		{
			await base.FinishReading (cancellationToken).ConfigureAwait (false);

			cancellationToken.ThrowIfCancellationRequested ();

			/*
			 * We are expecting the chunk trailer, but no more data.
			 */
			if (Decoder.DataAvailable)
				ThrowExpectingChunkTrailer ();

			/*
			 * Need to loop here since there might be header fields after
			 * the chunk trailer.
			 */
			while (Decoder.WantMore) {
				var buffer = new byte[256];
				int ret = await InnerStream.ReadAsync (
					buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
				if (ret <= 0)
					ThrowExpectingChunkTrailer ();

				Decoder.Write (buffer, 0, ret);
				ret = Decoder.Read (buffer, 0, 1);
				if (ret != 0)
					ThrowExpectingChunkTrailer ();
			}
		}

		static void ThrowExpectingChunkTrailer ()
		{
			throw new WebException (
				"Expecting chunk trailer.", null,
				WebExceptionStatus.ServerProtocolViolation, null);
		}
	}
}
