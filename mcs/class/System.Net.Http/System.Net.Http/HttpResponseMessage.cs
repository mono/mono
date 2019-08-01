//
// HttpResponseMessage.cs
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
using System.Text;

namespace System.Net.Http
{
	public class HttpResponseMessage : IDisposable
	{
		HttpResponseHeaders headers;
		HttpResponseHeaders trailingHeaders;
		string reasonPhrase;
		HttpStatusCode statusCode;
		Version version;
		bool disposed;

		public HttpResponseMessage ()
			: this (HttpStatusCode.OK)
		{
		}

		public HttpResponseMessage (HttpStatusCode statusCode)
		{
			StatusCode = statusCode;
		}

		public HttpContent Content { get; set; }

		public HttpResponseHeaders Headers {
			get {
				return headers ?? (headers = new HttpResponseHeaders ());
			}
		}

		public bool IsSuccessStatusCode {
			get {
				// Successful codes are 2xx
				return statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices;
			}
		}

		public string ReasonPhrase {
			get {
				return reasonPhrase ?? HttpStatusDescription.Get (statusCode);
			}
			set {
				reasonPhrase = value;
			}
		}

		public HttpRequestMessage RequestMessage { get; set; }

		public HttpStatusCode StatusCode {
			get {
				return statusCode;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				statusCode = value;
			}
		}

		public Version Version {
			get {
				return version ?? HttpVersion.Version11;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Version");

				version = value;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;

				if (Content != null)
					Content.Dispose ();
			}
		}

		public HttpResponseMessage EnsureSuccessStatusCode ()
		{
			if (IsSuccessStatusCode)
				return this;

			throw new HttpRequestException (string.Format ("{0} ({1})", (int) statusCode, ReasonPhrase));
		}
		
		public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.Append ("StatusCode: ").Append ((int)StatusCode);
			sb.Append (", ReasonPhrase: '").Append (ReasonPhrase ?? "<null>");
			sb.Append ("', Version: ").Append (Version);
			sb.Append (", Content: ").Append (Content != null ? Content.ToString () : "<null>");
			sb.Append (", Headers:\r\n{\r\n").Append (Headers);
			if (Content != null)
				sb.Append (Content.Headers);
			
			sb.Append ("}");
			
			return sb.ToString ();
		}

		public HttpResponseHeaders TrailingHeaders {
			get {
				if (trailingHeaders == null)
					trailingHeaders = new HttpResponseHeaders ();

				return trailingHeaders;
			}
		}
	}
}
