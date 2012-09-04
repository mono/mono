//
// HttpContentHeaders.cs
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

namespace System.Net.Http.Headers
{
	public sealed class HttpContentHeaders : HttpHeaders
	{
		readonly HttpContent content;

		internal HttpContentHeaders (HttpContent content)
			: base (HttpHeaderKind.Content)
		{
			this.content = content;
		}
		
		public ICollection<string> Allow {
			get {
				return GetValues<string> ("Allow");
			}
		}

		public ICollection<string> ContentEncoding {
			get {
				return GetValues<string> ("Content-Encoding");
			}
		}
		
		public ContentDispositionHeaderValue ContentDisposition {
			get {
				return GetValue<ContentDispositionHeaderValue> ("Content-Disposition");
			}
			set {
				AddOrRemove ("Content-Disposition", value);
			}
		}

		public ICollection<string> ContentLanguage {
			get {
				return GetValues<string> ("Content-Language");
			}
		}

		public long? ContentLength {
			get {
				long? v = GetValue<long?> ("Content-Length");
				if (v != null)
					return v;

				long l;
				if (content.TryComputeLength (out l))
					return l;

				return null;
			}
			set {
				AddOrRemove ("Content-Length", value);
			}
		}

		public Uri ContentLocation {
			get {
				return GetValue<Uri> ("Content-Location");
			}
			set {
				AddOrRemove ("Content-Location", value);
			}
		}

		public byte[] ContentMD5 {
			get {
				return GetValue<byte[]> ("Content-MD5");
			}
			set {
				AddOrRemove ("Content-MD5", value);
			}
		}

		public ContentRangeHeaderValue ContentRange {
			get {
				return GetValue<ContentRangeHeaderValue> ("Content-Range");
			}
			set {
				AddOrRemove ("Content-Range", value);
			}
		}

		public MediaTypeHeaderValue ContentType {
			get {
				return GetValue<MediaTypeHeaderValue> ("Content-Type");
			}
			set {
				AddOrRemove ("Content-Type", value);
			}
		}

		public DateTimeOffset? Expires {
			get {
				return GetValue<DateTimeOffset?> ("Expires");
			}
			set {
				AddOrRemove ("Expires", value, Parser.DateTime.ToString);
			}
		}

		public DateTimeOffset? LastModified {
			get {
				return GetValue<DateTimeOffset?> ("Last-Modified");
			}
			set {
				AddOrRemove ("Last-Modified", value, Parser.DateTime.ToString);
			}
		}
	}
}
