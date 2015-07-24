//
// HttpRequestMessage.cs
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

using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http
{
	public class HttpRequestMessage : IDisposable
	{
		HttpRequestHeaders headers;
		HttpMethod method;
		Version version;
		Dictionary<string, object> properties;
		Uri uri;
		bool is_used;
		bool disposed;

		public HttpRequestMessage ()
		{
			this.method = HttpMethod.Get;
		}

		public HttpRequestMessage (HttpMethod method, string requestUri)
			: this (method, string.IsNullOrEmpty (requestUri) ? (Uri) null : new Uri (requestUri, System.UriKind.RelativeOrAbsolute))
		{
		}

		public HttpRequestMessage (HttpMethod method, Uri requestUri)
		{
			Method = method;
			RequestUri = requestUri;
		}

		public HttpContent Content { get; set; }

		public HttpRequestHeaders Headers {
			get {
				return headers ?? (headers = new HttpRequestHeaders ());
			}
		}

		public HttpMethod Method {
			get {
				return method;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("method");

				method = value;
			}
		}

		public IDictionary<string, object> Properties {
			get {
				return properties ?? (properties = new Dictionary<string, object> ());
			}
		}

		public Uri RequestUri {
			get {
				return uri;
			}
			set {
				if (value != null && value.IsAbsoluteUri && !IsAllowedAbsoluteUri (value))
					throw new ArgumentException ("Only http or https scheme is allowed");

				uri = value;
			}
		}

		static bool IsAllowedAbsoluteUri (Uri uri)
		{
			if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				return true;

			// Mono URI handling which does not distinguish between file and url absolute paths without scheme
			if (uri.Scheme == Uri.UriSchemeFile && uri.OriginalString.StartsWith ("/", StringComparison.Ordinal))
				return true;

			return false;
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

		internal bool SetIsUsed ()
		{
			if (is_used)
				return true;

			is_used = true;
			return false;
		}
		
		public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.Append ("Method: ").Append (method);
			sb.Append (", RequestUri: '").Append (RequestUri != null ? RequestUri.ToString () : "<null>");
			sb.Append ("', Version: ").Append (Version);
			sb.Append (", Content: ").Append (Content != null ? Content.ToString () : "<null>");
			sb.Append (", Headers:\r\n{\r\n").Append (Headers);
			if (Content != null)
				sb.Append (Content.Headers);
			sb.Append ("}");
			
			return sb.ToString ();
		}
	}
}
