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
		internal HttpRequestHeaders ()
		{
		}

		public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept {
			get {
				return GetValue<HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue>> ("Accept");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset {
			get {
				return GetValue<HttpHeaderValueCollection<StringWithQualityHeaderValue>> ("Accept-Charset");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding {
			get {
				return GetValue<HttpHeaderValueCollection<StringWithQualityHeaderValue>> ("Accept-Encoding");
			}
		}

		public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage {
			get {
				return GetValue<HttpHeaderValueCollection<StringWithQualityHeaderValue>> ("Accept-Language");
			}
		}

		public AuthenticationHeaderValue Authorization {
			get {
				return GetValue<AuthenticationHeaderValue> ("Authorization");
			}
			set {
				// TODO:
			}
		}

		public CacheControlHeaderValue CacheControl {
			get {
				return GetValue<CacheControlHeaderValue> ("Cache-Control");
			}
			set {
				// TODO:
			}
		}

		public HttpHeaderValueCollection<string> Connection {
			get {
				return GetValue<HttpHeaderValueCollection<string>> ("Connection");
			}
		}

		public bool? ConnectionClose {
			get {
				throw new NotImplementedException ();
				//return Connection.GetValue<bool?> ("close");
			}
			set {
				// TODO: return Connection.SetValue ("Connection", "close", value);
			}
		}

		public DateTimeOffset? Date {
			get {
				return GetValue<DateTimeOffset?> ("Date");
			}
			set {
				SetValue ("Date", value);
			}
		}

		public HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect {
			get {
				return GetValue<HttpHeaderValueCollection<NameValueWithParametersHeaderValue>> ("Expect");
			}
		}

		public bool? ExpectContinue { 
			get {
				throw new NotImplementedException ();
			}
			set {
				// TODO:
			}
		}

		public string From {
			get {
				return GetValue<string> ("From");
			}
			set {
				// TODO: error checks
			}
		}

		public string Host {
			get {
				return GetValue<string> ("Host");
			}
			set {
				// TODO: error checks
			}
		}

		public HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch {
			get {
				return GetValue<HttpHeaderValueCollection<EntityTagHeaderValue>> ("If-Match");
			}
		}

		public DateTimeOffset? IfModifiedSince {
			get {
				return GetValue<DateTimeOffset?> ("If-Modified-Since");
			}
			set {
				// TODO:
			}
		}

		public HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch {
			get {
				return GetValue<HttpHeaderValueCollection<EntityTagHeaderValue>> ("If-None-Match");
			}
		}

		public RangeConditionHeaderValue IfRange {
			get
			{
				return GetValue<RangeConditionHeaderValue> ("If-Range");
			}
			set {
				// TODO:
			}
		}

		public DateTimeOffset? IfUnmodifiedSince {
			get {
				return GetValue<DateTimeOffset?> ("If-Unmodified-Since");
			}
			set {
				// TODO:
			}
		}

		public int? MaxForwards {
			get {
				return GetValue<int?> ("Max-Forwards");
			}
			set {
				SetValue ("Max-Forwards", value);
			}
		}

		public HttpHeaderValueCollection<NameValueHeaderValue> Pragma {
			get {
				return GetValue<HttpHeaderValueCollection<NameValueHeaderValue>> ("Pragma");
			}
		}

		public AuthenticationHeaderValue ProxyAuthorization {
			get {
				return GetValue<AuthenticationHeaderValue> ("Proxy-Authorization");
			}
			set {
				// TODO:
			}
		}

		public RangeHeaderValue Range {
			get {
				return GetValue<RangeHeaderValue> ("Range");
			}
			set {
				// TODO:
			}
		}

		public Uri Referrer {
			get {
				return GetValue<Uri> ("Referer");
			}
			set {
				SetValue ("Referer", value);
			}
		}

		public HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE {
		    get {
		        return GetValue<HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue>> ("TE");
		    }
		}

		public HttpHeaderValueCollection<string> Trailer {
			get {
				return GetValue<HttpHeaderValueCollection<string>> ("Trailer");
			}
		}

		public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding {
			get {
				return GetValue<HttpHeaderValueCollection<TransferCodingHeaderValue>> ("Transfer-Encoding");
			}
		}

		public bool? TransferEncodingChunked {
			get {
				throw new NotImplementedException ();
			}

			set {
				// TODO:
			}
		}

		public HttpHeaderValueCollection<ProductHeaderValue> Upgrade {
			get {
				return GetValue<HttpHeaderValueCollection<ProductHeaderValue>> ("Upgrade");
			}
		}

		public HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent {
			get {
				return GetValue<HttpHeaderValueCollection<ProductInfoHeaderValue>> ("User-Agent");
			}
		}

		public HttpHeaderValueCollection<ViaHeaderValue> Via {
			get {
				return GetValue<HttpHeaderValueCollection<ViaHeaderValue>> ("Via");
			}
		}

		public HttpHeaderValueCollection<WarningHeaderValue> Warning {
			get {
				return GetValue<HttpHeaderValueCollection<WarningHeaderValue>> ("Warning");
			}
		}
	}
}
