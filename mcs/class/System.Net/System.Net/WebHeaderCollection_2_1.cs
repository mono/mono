//
// System.Net.WebHeaderCollection (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_1

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Net {

	public class WebHeaderCollection : IEnumerable {

		Dictionary<string, string> headers = new Dictionary<string, string> ();

		public WebHeaderCollection ()
		{
		}

		public int Count {
			get { return headers.Count; }
		}

		public string [] AllKeys {
			get {
				var keys = new string [headers.Count];
				headers.Keys.CopyTo (keys, 0);
				return keys;
			}
		}

		public string this [string header] {
			get {
				if (header == null)
					throw new ArgumentNullException ("header");

				string value;
				if (headers.TryGetValue (header, out value))
					return value;

				return null;
			}
			set {
				if (header == null)
					throw new ArgumentNullException ("header");
				if (header.Length == 0)
					throw new ArgumentException ("header");

				headers [header] = value;
			}
		}

		public string this [HttpRequestHeader header] {
			get { return this [HttpRequestHeaderToString (header)]; }
			set { this [HttpRequestHeaderToString (header)] = value; }
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return headers.GetEnumerator ();
		}

		static string HttpResponseHeaderToString (HttpResponseHeader header)
		{
			switch (header) {
			case HttpResponseHeader.CacheControl:		return "cache-control";
			case HttpResponseHeader.Connection:			return "connection";
			case HttpResponseHeader.Date:				return "date";
			case HttpResponseHeader.KeepAlive:			return "keep-alive";
			case HttpResponseHeader.Pragma:				return "pragma";
			case HttpResponseHeader.Trailer:			return "trailer";
			case HttpResponseHeader.TransferEncoding:	return "transfer-encoding";
			case HttpResponseHeader.Upgrade:			return "upgrade";
			case HttpResponseHeader.Via:				return "via";
			case HttpResponseHeader.Warning:			return "warning";
			case HttpResponseHeader.Allow:				return "allow";
			case HttpResponseHeader.ContentLength:		return "content-length";
			case HttpResponseHeader.ContentType:		return "content-type";
			case HttpResponseHeader.ContentEncoding:	return "content-encoding";
			case HttpResponseHeader.ContentLanguage:	return "content-language";
			case HttpResponseHeader.ContentLocation:	return "content-location";
			case HttpResponseHeader.ContentMd5:			return "content-md5";
			case HttpResponseHeader.ContentRange:		return "content-range";
			case HttpResponseHeader.Expires:			return "expires";
			case HttpResponseHeader.LastModified:		return "last-modified";
			case HttpResponseHeader.AcceptRanges:		return "accept-ranges";
			case HttpResponseHeader.Age:				return "age";
			case HttpResponseHeader.ETag:				return "etag";
			case HttpResponseHeader.Location:			return "location";
			case HttpResponseHeader.ProxyAuthenticate:	return "proxy-authenticate";
			case HttpResponseHeader.RetryAfter:			return "retry-after";
			case HttpResponseHeader.Server:				return "server";
			case HttpResponseHeader.SetCookie:			return "set-cookie";
			case HttpResponseHeader.Vary:				return "vary";
			case HttpResponseHeader.WwwAuthenticate:	return "www-authenticate";
			default:
				throw new IndexOutOfRangeException ();
			}
		}

		static string HttpRequestHeaderToString (HttpRequestHeader header)
		{
			switch (header) {
			case HttpRequestHeader.CacheControl:		return "cache-control";
			case HttpRequestHeader.Connection:			return "connection";
			case HttpRequestHeader.Date:				return "date";
			case HttpRequestHeader.KeepAlive:			return "keep-alive";
			case HttpRequestHeader.Pragma:				return "pragma";
			case HttpRequestHeader.Trailer:				return "trailer";
			case HttpRequestHeader.TransferEncoding:	return "transfer-encoding";
			case HttpRequestHeader.Upgrade:				return "upgrade";
			case HttpRequestHeader.Via:					return "via";
			case HttpRequestHeader.Warning:				return "warning";
			case HttpRequestHeader.Allow:				return "allow";
			case HttpRequestHeader.ContentLength:		return "content-length";
			case HttpRequestHeader.ContentType:			return "content-type";
			case HttpRequestHeader.ContentEncoding:		return "content-encoding";
			case HttpRequestHeader.ContentLanguage:		return "content-language";
			case HttpRequestHeader.ContentLocation:		return "content-location";
			case HttpRequestHeader.ContentMd5:			return "content-md5";
			case HttpRequestHeader.ContentRange:		return "content-range";
			case HttpRequestHeader.Expires:				return "expires";
			case HttpRequestHeader.LastModified:		return "last-modified";
			case HttpRequestHeader.Accept:				return "accept";
			case HttpRequestHeader.AcceptCharset:		return "accept-charset";
			case HttpRequestHeader.AcceptEncoding:		return "accept-encoding";
			case HttpRequestHeader.AcceptLanguage:		return "accept-language";
			case HttpRequestHeader.Authorization:		return "authorization";
			case HttpRequestHeader.Cookie:				return "cookie";
			case HttpRequestHeader.Expect:				return "expect";
			case HttpRequestHeader.From:				return "from";
			case HttpRequestHeader.Host:				return "host";
			case HttpRequestHeader.IfMatch:				return "if-match";
			case HttpRequestHeader.IfModifiedSince:		return "if-modified-since";
			case HttpRequestHeader.IfNoneMatch:			return "if-none-match";
			case HttpRequestHeader.IfRange:				return "if-range";
			case HttpRequestHeader.IfUnmodifiedSince:	return "if-unmodified-since";
			case HttpRequestHeader.MaxForwards:			return "max-forwards";
			case HttpRequestHeader.ProxyAuthorization:	return "proxy-authorization";
			case HttpRequestHeader.Referer:				return "referer";
			case HttpRequestHeader.Range:				return "range";
			case HttpRequestHeader.Te:					return "te";
			case HttpRequestHeader.Translate:			return "translate";
			case HttpRequestHeader.UserAgent:			return "user-agent";
			default:
				throw new IndexOutOfRangeException ();
			}
		}
	}
}

#endif
