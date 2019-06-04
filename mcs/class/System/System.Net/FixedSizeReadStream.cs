//
// FixedSizeReadStream.cs
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
	class FixedSizeReadStream : WebReadStream
	{
		public long ContentLength {
			get;
		}

		long position;

		public FixedSizeReadStream (WebOperation operation, Stream innerStream,
		                            long contentLength)
			: base (operation, innerStream)
		{
			ContentLength = contentLength;
		}

		protected override async Task<int> ProcessReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			var remaining = ContentLength - position;
			WebConnection.Debug ($"{ME} READ: position={position} length={ContentLength} size={size} remaining={remaining}");

			if (remaining == 0)
				return 0;

			var readSize = (int)Math.Min (remaining, size);
			WebConnection.Debug ($"{ME} READ #1: readSize={readSize}");
			var ret = await InnerStream.ReadAsync (
				buffer, offset, readSize, cancellationToken).ConfigureAwait (false);
			WebConnection.Debug ($"{ME} READ #2: ret={ret}");
			if (ret <= 0)
				return ret;

			position += ret;
			return ret;
		}
	}
}

