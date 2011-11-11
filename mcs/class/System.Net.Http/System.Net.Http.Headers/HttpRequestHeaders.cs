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

		//public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept { get; }
		//public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset { get; }
		//public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding { get; }
		//public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage { get; }
		//public AuthenticationHeaderValue Authorization { get; set; }
		//public CacheControlHeaderValue CacheControl { get; set; }
		public HttpHeaderValueCollection<string> Connection {
			get {
				return Connection.GetValue<HttpHeaderValueCollection<string>> ("Connection");
			}
		}

		public bool? ConnectionClose {
			get {
				return Connection.GetValue<bool?> ("close");
			}
			set {
				return Connection.SetValue ("close", value);
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
		//public HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect { get; }
		//public bool? ExpectContinue { get; set; }
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

		//public HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch { get; }
		public DateTimeOffset? IfModifiedSince { get; set; }
		//public HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch { get; }
		//public RangeConditionHeaderValue IfRange { get; set; }
		//public DateTimeOffset? IfUnmodifiedSince { get; set; }
		public int? MaxForwards {
			get {
				return GetValue<int?> ("Max-Forwards");
			}
			set {
				SetValue ("Max-Forwards", value);
			}
		}

		//public HttpHeaderValueCollection<NameValueHeaderValue> Pragma { get; }
		//public AuthenticationHeaderValue ProxyAuthorization { get; set; }
		//public RangeHeaderValue Range { get; set; }

		public Uri Referrer {
			get {
				return GetValue<Uri> ("Referer");
			}
			set {
				SetValue ("Referer", value);
			}
		}

		//public HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE {
		//    get {
		//        return GetValue<HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue>> ("TE");
		//    }
		//}
		public HttpHeaderValueCollection<string> Trailer {
			get {
				return GetValue<HttpHeaderValueCollection<string>> ("Trailer");
			}
		}
		//public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding { get; }
		//public bool? TransferEncodingChunked { get; set; }
		//public HttpHeaderValueCollection<ProductHeaderValue> Upgrade { get; }
		//public HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent { get; }
		//public HttpHeaderValueCollection<ViaHeaderValue> Via { get; }
		//public HttpHeaderValueCollection<WarningHeaderValue> Warning { get; }
	}
}
