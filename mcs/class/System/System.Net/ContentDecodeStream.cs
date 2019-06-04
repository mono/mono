//
// ContentDecodeStream.cs
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
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net
{
	class ContentDecodeStream : WebReadStream
	{
		internal enum Mode
		{
			GZip,
			Deflate
		}

		public static ContentDecodeStream Create (
			WebOperation operation, Stream innerStream, Mode mode)
		{
			Stream decodeStream;
			if (mode == Mode.GZip)
				decodeStream = new GZipStream (innerStream, CompressionMode.Decompress);
			else
				decodeStream = new DeflateStream (innerStream, CompressionMode.Decompress);
			return new ContentDecodeStream (operation, decodeStream, innerStream);
		}

		Stream OriginalInnerStream {
			get;
		}

		ContentDecodeStream (WebOperation operation, Stream decodeStream,
		                     Stream originalInnerStream)
			: base (operation, decodeStream)
		{
			/*
			 * We pass the GZipStream/DeflateStream to the base .ctor,
			 * so it can Dispose() it when we're done.
			 * 
			 * Save the original inner stream here for FinishReading().
			 */
			OriginalInnerStream = originalInnerStream;
		}

		protected override Task<int> ProcessReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken)
		{
			return InnerStream.ReadAsync (buffer, offset, size, cancellationToken);
		}

		internal override Task FinishReading (CancellationToken cancellationToken)
		{
			/*
			 * Call FinishReading() on the original inner stream.
			 * 
			 * Since GZipStream/DeflateStream knows the exact number of
			 * bytes that it wants to receive from it, it may not have
			 * read the chunk trailer yet.
			 */
			if (OriginalInnerStream is WebReadStream innerReadStream)
				return innerReadStream.FinishReading (cancellationToken);
			return Task.CompletedTask;
		}
	}
}

