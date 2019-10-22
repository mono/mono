//
// ByteArrayContent.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Threading.Tasks;

namespace System.Net.Http
{
	public class ByteArrayContent: HttpContent
	{
		readonly byte[] content;
		readonly int offset, count;

		public ByteArrayContent (byte[] content)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			this.content = content;
			this.count = content.Length;
		}

		public ByteArrayContent (byte[] content, int offset, int count)
			: this (content)
		{
			if (offset < 0 || offset > this.count)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (this.count - offset))
				throw new ArgumentOutOfRangeException ("count");

			this.offset = offset;
			this.count = count;
		}

		protected override Task<Stream> CreateContentReadStreamAsync ()
		{
			return Task.FromResult<Stream> (new MemoryStream (content, offset, count));
		}

		protected override Task SerializeToStreamAsync (Stream stream, TransportContext context)
		{
			return stream.WriteAsync (content, offset, count);
		}

		protected internal override bool TryComputeLength (out long length)
		{
			length = count;
			return true;
		}
	}
}
