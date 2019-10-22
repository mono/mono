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
		sealed class FixedMemoryStream : MemoryStream
		{
			readonly long maxSize;
			
			public FixedMemoryStream (long maxSize)
				: base ()
			{
				this.maxSize = maxSize;
			}
			
			void CheckOverflow (int count)
			{
				if (Length + count > maxSize)
					throw new HttpRequestException (string.Format ("Cannot write more bytes to the buffer than the configured maximum buffer size: {0}", maxSize));
			}
			
			public override void WriteByte (byte value)
			{
				CheckOverflow (1);
				base.WriteByte (value);
			}
			
			public override void Write (byte[] buffer, int offset, int count)
			{
				CheckOverflow (count);
				base.Write (buffer, offset, count);
			}
		}
		
		FixedMemoryStream buffer;
		Stream stream;
		bool disposed;
		HttpContentHeaders headers;

		public HttpContentHeaders Headers {
			get {
				return headers ?? (headers = new HttpContentHeaders (this));
			}
		}

		internal long? LoadedBufferLength {
			get {
				return buffer == null ? (long?)null : buffer.Length;
			}
		}

		// Only used by HttpWebRequest internals which is not async friendly
		internal void CopyTo (Stream stream)
		{
			CopyToAsync (stream).Wait ();
		}

		public Task CopyToAsync (Stream stream)
		{
			return CopyToAsync (stream, null);
		}

		public Task CopyToAsync (Stream stream, TransportContext context)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (buffer != null)
				return buffer.CopyToAsync (stream);

			return SerializeToStreamAsync (stream, context);
		}

		protected async virtual Task<Stream> CreateContentReadStreamAsync ()
		{
			await LoadIntoBufferAsync ().ConfigureAwait (false);
			return buffer;
		}
		
		static FixedMemoryStream CreateFixedMemoryStream (long maxBufferSize)
		{
			return new FixedMemoryStream (maxBufferSize);
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
			return LoadIntoBufferAsync (int.MaxValue);
		}

		public async Task LoadIntoBufferAsync (long maxBufferSize)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer != null)
				return;

			buffer = CreateFixedMemoryStream (maxBufferSize);
			await SerializeToStreamAsync (buffer, null).ConfigureAwait (false);
			buffer.Seek (0, SeekOrigin.Begin);
		}
		
		public async Task<Stream> ReadAsStreamAsync ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer != null)
				return new MemoryStream (buffer.GetBuffer (), 0, (int)buffer.Length, false);

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
			await LoadIntoBufferAsync ().ConfigureAwait (false);
			if (buffer.Length == 0)
				return string.Empty;

			var buf = buffer.GetBuffer ();
			var buf_length = (int) buffer.Length;
			int preambleLength = 0;
			Encoding encoding;

			if (headers != null && headers.ContentType != null && headers.ContentType.CharSet != null) {
				encoding = Encoding.GetEncoding (headers.ContentType.CharSet);
				preambleLength = StartsWith (buf, buf_length, encoding.GetPreamble ());
			} else {
				encoding = GetEncodingFromBuffer (buf, buf_length, ref preambleLength) ?? Encoding.UTF8;
			}

			return encoding.GetString (buf, preambleLength, buf_length - preambleLength);
		}

		static Encoding GetEncodingFromBuffer (byte[] buffer, int length, ref int preambleLength)
		{
			var encodings_with_preamble = new [] { Encoding.UTF8, Encoding.UTF32, Encoding.Unicode };
			foreach (var enc in encodings_with_preamble) {
				if ((preambleLength = StartsWith (buffer, length, enc.GetPreamble ())) != 0)
					return enc;
			}

			return null;
		}

		static int StartsWith (byte[] array, int length, byte[] value)
		{
			if (length < value.Length)
				return 0;

			for (int i = 0; i < value.Length; ++i) {
				if (array [i] != value [i])
					return 0;
			}

			return value.Length;
		}

		internal Task SerializeToStreamAsync_internal (Stream stream, TransportContext context)
		{
			return SerializeToStreamAsync (stream, context);
		}

		protected abstract Task SerializeToStreamAsync (Stream stream, TransportContext context);
		protected internal abstract bool TryComputeLength (out long length);
	}
}
