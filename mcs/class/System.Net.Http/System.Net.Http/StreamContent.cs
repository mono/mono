//
// StreamContent.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
	public class StreamContent : HttpContent
	{
		readonly Stream content;
		readonly int bufferSize;
		readonly CancellationToken cancellationToken;
		readonly long startPosition;
		bool contentCopied;

		public StreamContent (Stream content)
			: this (content, 16 * 1024)
		{
		}

		public StreamContent (Stream content, int bufferSize)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize");

			this.content = content;
			this.bufferSize = bufferSize;

			if (content.CanSeek) {
				startPosition = content.Position;
			}
		}

		//
		// Workarounds for poor .NET API
		// Instead of having SerializeToStreamAsync with CancellationToken as public API. Only LoadIntoBufferAsync
		// called internally from the send worker can be cancelled and user cannot see/do it
		//
		internal StreamContent (Stream content, CancellationToken cancellationToken)
			: this (content)
		{
			// We don't own the token so don't worry about disposing it
			this.cancellationToken = cancellationToken;
		}

		protected override Task<Stream> CreateContentReadStreamAsync ()
		{
			return Task.FromResult (content);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				content.Dispose ();
			}

			base.Dispose (disposing);
		}

		protected override Task SerializeToStreamAsync (Stream stream, TransportContext context)
		{
			if (contentCopied) {
				if (!content.CanSeek) {
					throw new InvalidOperationException ("The stream was already consumed. It cannot be read again.");
				}

				content.Seek (startPosition, SeekOrigin.Begin);
			} else {
				contentCopied = true;
			}

			return content.CopyToAsync (stream, bufferSize, cancellationToken);
		}

		protected internal override bool TryComputeLength (out long length)
		{
			if (!content.CanSeek) {
				length = 0;
				return false;
			}
			length = content.Length - startPosition;
			return true;
		}
	}
}
