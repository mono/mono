//
// HttpContent.cs
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

using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace System.Net.Http
{
	public abstract class HttpContent : IDisposable
	{
		MemoryStream buffer;
		Stream stream;
		bool disposed;
		HttpContentHeaders headers;

		public HttpContentHeaders Headers {
			get {
				return headers ?? (headers = new HttpContentHeaders (this));
			}
		}

		public Task CopyToAsync (Stream stream)
		{
			return CopyToAsync (stream, null);
		}

		public Task CopyToAsync (Stream stream, TransportContext context)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			return SerializeToStreamAsync (stream, context);
		}

		protected async virtual Task<Stream> CreateContentReadStreamAsync ()
		{
			await LoadIntoBufferAsync ();
			return buffer;
		}

		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;

				if (buffer != null)
					buffer.Dispose ();
			}
		}

		public Task LoadIntoBufferAsync ()
		{
			return LoadIntoBufferAsync (0x2000);
		}

		public async Task LoadIntoBufferAsync (int maxBufferSize)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer != null)
				return;

			buffer = new MemoryStream ();
			await SerializeToStreamAsync (buffer, null).ConfigureAwait (false);
			buffer.Seek (0, SeekOrigin.Begin);
		}
		
		public async Task<Stream> ReadAsStreamAsync ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (stream == null)
				stream = await CreateContentReadStreamAsync ().ConfigureAwait (false);

			return stream;
		}

		public async Task<byte[]> ReadAsByteArrayAsync ()
		{
			await LoadIntoBufferAsync ().ConfigureAwait (false);
			return buffer.ToArray ();
		}

		public async Task<string> ReadAsStringAsync ()
		{
			await LoadIntoBufferAsync ();
			if (buffer.Length == 0)
				return string.Empty;

			Encoding encoding;
			if (headers != null && headers.ContentType != null && headers.ContentType.CharSet != null) {
				encoding = Encoding.GetEncoding (headers.ContentType.CharSet);
			} else {
				encoding = Encoding.UTF8;
			}

			return encoding.GetString (buffer.GetBuffer (), 0, (int) buffer.Length);
		}

		protected abstract Task SerializeToStreamAsync (Stream stream, TransportContext context);
		protected internal abstract bool TryComputeLength (out long length);
	}
}
