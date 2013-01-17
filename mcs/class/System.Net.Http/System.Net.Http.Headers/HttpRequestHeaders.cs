//
// HttpRequestHeaders.cs
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
	public sealed class HttpRequestHeaders : HttpHeaders
	{
		bool? expectContinue;

		internal HttpRequestHeaders ()
			: base (HttpHeaderKind.Request)
		{
		}

		public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept {
			get {
				return GetValues<MediaTypeWithQualityHeaderValue> ("Accept");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset {
			get {
				return GetValues<StringWithQualityHeaderValue> ("Accept-Charset");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding {
			get {
				return GetValues<StringWithQualityHeaderValue> ("Accept-Encoding");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage {
			get {
				return GetValues<StringWithQualityHeaderValue> ("Accept-Language");
			}
		}

		public AuthenticationHeaderValue Authorization {
			get {
				return GetValue<AuthenticationHeaderValue> ("Authorization");
			}
			set {
				AddOrRemove ("Authorization", value);
			}
		}

		public CacheControlHeaderValue CacheControl {
			get {
				return GetValue<CacheControlHeaderValue> ("Cache-Control");
			}
			set {
				AddOrRemove ("Cache-Control", value);
			}
		}

		public HttpHeaderValueCollection<string> Connection {
			get {
				return GetValues<string> ("Connection");
			}
		}

		public bool? ConnectionClose {
			get {
				if (connectionclose == true || Connection.Find (l => string.Equals (l, "close", StringComparison.OrdinalIgnoreCase)) != null)
					return true;

				return connectionclose;
			}
			set {
				if (connectionclose == value)
					return;

				Connection.Remove ("close");
				if (value == true)
					Connection.Add ("close");

				connectionclose = value;
			}
		}

		internal bool ConnectionKeepAlive {
			get {
				return Connection.Find (l => string.Equals (l, "Keep-Alive", StringComparison.OrdinalIgnoreCase)) != null;
			}
		}

		public DateTimeOffset? Date {
			get {
				return GetValue<DateTimeOffset?> ("Date");
			}
			set {
				AddOrRemove ("Date", value, Parser.DateTime.ToString);
			}
		}

		public HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect {
			get {
				return GetValues<NameValueWithParametersHeaderValue> ("Expect");
			}
		}

		public bool? ExpectContinue { 
			get {
				if (expectContinue.HasValue)
					return expectContinue;

				var found = TransferEncoding.Find (l => string.Equals (l.Value, "100-continue", StringComparison.OrdinalIgnoreCase));
				return found != null ? true : (bool?) null;
			}
			set {
				if (expectContinue == value)
					return;

				Expect.Remove (l => l.Name == "100-continue");

				if (value == true)
					Expect.Add (new NameValueWithParametersHeaderValue ("100-continue"));

				expectContinue = value;
			}
		}

		public string From {
			get {
				return GetValue<string> ("From");
			}
			set {
				if (!string.IsNullOrEmpty (value) && !Parser.EmailAddress.TryParse (value, out value))
					throw new FormatException ();

				AddOrRemove ("From", value);
			}
		}

		public string Host {
			get {
				return GetValue<string> ("Host");
			}
			set {
				AddOrRemove ("Host", value);
			}
		}

		public HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch {
			get {
				return GetValues<EntityTagHeaderValue> ("If-Match");
			}
		}

		public DateTimeOffset? IfModifiedSince {
			get {
				return GetValue<DateTimeOffset?> ("If-Modified-Since");
			}
			set {
				AddOrRemove ("If-Modified-Since", value, Parser.DateTime.ToString);
			}
		}

		public HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch {
			get {
				return GetValues<EntityTagHeaderValue> ("If-None-Match");
			}
		}

		public RangeConditionHeaderValue IfRange {
			get {
				return GetValue<RangeConditionHeaderValue> ("If-Range");
			}
			set {
				AddOrRemove ("If-Range", value);
			}
		}

		public DateTimeOffset? IfUnmodifiedSince {
			get {
				return GetValue<DateTimeOffset?> ("If-Unmodified-Since");
			}
			set {
				AddOrRemove ("If-Unmodified-Since", value, Parser.DateTime.ToString);
			}
		}

		public int? MaxForwards {
			get {
				return GetValue<int?> ("Max-Forwards");
			}
			set {
				AddOrRemove ("Max-Forwards", value);
			}
		}

		public HttpHeaderValueCollection<NameValueHeaderValue> Pragma {
			get {
				return GetValues<NameValueHeaderValue> ("Pragma");
			}
		}

		public AuthenticationHeaderValue ProxyAuthorization {
			get {
				return GetValue<AuthenticationHeaderValue> ("Proxy-Authorization");
			}
			set {
				AddOrRemove ("Proxy-Authorization", value);
			}
		}

		public RangeHeaderValue Range {
			get {
				return GetValue<RangeHeaderValue> ("Range");
			}
			set {
				AddOrRemove ("Range", value);
			}
		}

		public Uri Referrer {
			get {
				return GetValue<Uri> ("Referer");
			}
			set {
				AddOrRemove ("Referer", value);
			}
		}

		public HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE {
		    get {
		        return GetValues<TransferCodingWithQualityHeaderValue> ("TE");
		    }
		}

		public HttpHeaderValueCollection<string> Trailer {
			get {
				return GetValues<string> ("Trailer");
			}
		}

		public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding {
			get {
				return GetValues<TransferCodingHeaderValue> ("Transfer-Encoding");
			}
		}

		public bool? TransferEncodingChunked {
			get {
				if (transferEncodingChunked.HasValue)
					return transferEncodingChunked;

				var found = TransferEncoding.Find (l => string.Equals (l.Value, "chunked", StringComparison.OrdinalIgnoreCase));
				return found != null ? true : (bool?) null;
			}
			set {
				if (value == transferEncodingChunked)
					return;

				TransferEncoding.Remove (l => l.Value == "chunked");
				if (value == true)
					TransferEncoding.Add (new TransferCodingHeaderValue ("chunked"));

				transferEncodingChunked = value;
			}
		}

		public HttpHeaderValueCollection<ProductHeaderValue> Upgrade {
			get {
				return GetValues<ProductHeaderValue> ("Upgrade");
			}
		}

		public HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent {
			get {
				return GetValues<ProductInfoHeaderValue> ("User-Agent");
			}
		}

		public HttpHeaderValueCollection<ViaHeaderValue> Via {
			get {
				return GetValues<ViaHeaderValue> ("Via");
			}
		}

		public HttpHeaderValueCollection<WarningHeaderValue> Warning {
			get {
				return GetValues<WarningHeaderValue> ("Warning");
			}
		}

		internal void AddHeaders (HttpRequestHeaders headers)
		{
			foreach (var header in headers) {
				TryAddWithoutValidation (header.Key, header.Value);
			}
		}
	}
}
