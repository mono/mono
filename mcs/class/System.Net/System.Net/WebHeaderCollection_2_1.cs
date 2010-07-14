//
// System.Net.WebHeaderCollection (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) 2007, 2009 Novell, Inc. (http://www.novell.com)
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

	public sealed class WebHeaderCollection : IEnumerable {

		Dictionary<string, string> headers;
		bool validate;

		public WebHeaderCollection ()
			: this (false)
		{
		}

		internal WebHeaderCollection (bool restrict)
		{
			validate = restrict;
			headers = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
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

				string value = null;
				headers.TryGetValue (header, out value);
				return value;
			}
			set {
				if (header == null)
					throw new ArgumentNullException ("header");
				if (header.Length == 0)
					throw new ArgumentException ("header");

				if (validate)
					ValidateHeader (header);
				headers [header] = value;
			}
		}

		public string this [HttpRequestHeader header] {
			get { return this [HttpRequestHeaderToString (header)]; }
			set {
				string h = HttpRequestHeaderToString (header);
				if (validate)
					ValidateHeader (h);
				headers [h] = value;
			}
		}

		// some headers cannot be set using the "this" property but by using
		// the right property of the Web[Request|Response]. However the value 
		// does end up in the collection (and can be read safely from there)
		internal void SetHeader (string header, string value)
		{
			if (String.IsNullOrEmpty (value))
				headers.Remove (header);
			else
				headers [header] = value;
		}

		internal void Clear ()
		{
			headers.Clear ();
		}

		internal bool ContainsKey (string key)
		{
			return headers.ContainsKey (key);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return headers.Keys.GetEnumerator ();
		}

		static string HttpResponseHeaderToString (HttpResponseHeader header)
		{
			switch (header) {
			case HttpResponseHeader.CacheControl:		return "Cache-Control";
			case HttpResponseHeader.Connection:		return "Connection";
			case HttpResponseHeader.Date:			return "Date";
			case HttpResponseHeader.KeepAlive:		return "Keep-Alive";
			case HttpResponseHeader.Pragma:			return "Pragma";
			case HttpResponseHeader.Trailer:		return "Trailer";
			case HttpResponseHeader.TransferEncoding:	return "Transfer-Encoding";
			case HttpResponseHeader.Upgrade:		return "Upgrade";
			case HttpResponseHeader.Via:			return "Via";
			case HttpResponseHeader.Warning:		return "Warning";
			case HttpResponseHeader.Allow:			return "Allow";
			case HttpResponseHeader.ContentLength:		return "Content-Length";
			case HttpResponseHeader.ContentType:		return "Content-Type";
			case HttpResponseHeader.ContentEncoding:	return "Content-Encoding";
			case HttpResponseHeader.ContentLanguage:	return "Content-Language";
			case HttpResponseHeader.ContentLocation:	return "Content-Location";
			case HttpResponseHeader.ContentMd5:		return "Content-MD5";
			case HttpResponseHeader.ContentRange:		return "Content-Range";
			case HttpResponseHeader.Expires:		return "Expires";
			case HttpResponseHeader.LastModified:		return "Last-Modified";
			case HttpResponseHeader.AcceptRanges:		return "Accept-Ranges";
			case HttpResponseHeader.Age:			return "Age";
			case HttpResponseHeader.ETag:			return "ETag";
			case HttpResponseHeader.Location:		return "Location";
			case HttpResponseHeader.ProxyAuthenticate:	return "Proxy-Authenticate";
			case HttpResponseHeader.RetryAfter:		return "Retry-After";
			case HttpResponseHeader.Server:			return "Server";
			case HttpResponseHeader.SetCookie:		return "Set-Cookie";
			case HttpResponseHeader.Vary:			return "Vary";
			case HttpResponseHeader.WwwAuthenticate:	return "WWW-Authenticate";
			default:
				throw new IndexOutOfRangeException ();
			}
		}

		static string HttpRequestHeaderToString (HttpRequestHeader header)
		{
			switch (header) {
			case HttpRequestHeader.CacheControl:		return "Cache-Control";
			case HttpRequestHeader.Connection:		return "Connection";
			case HttpRequestHeader.Date:			return "Date";
			case HttpRequestHeader.KeepAlive:		return "Keep-Alive";
			case HttpRequestHeader.Pragma:			return "Pragma";
			case HttpRequestHeader.Trailer:			return "Trailer";
			case HttpRequestHeader.TransferEncoding:	return "Transfer-Encoding";
			case HttpRequestHeader.Upgrade:			return "Upgrade";
			case HttpRequestHeader.Via:			return "Via";
			case HttpRequestHeader.Warning:			return "Warning";
			case HttpRequestHeader.Allow:			return "Allow";
			case HttpRequestHeader.ContentLength:		return "Content-Length";
			case HttpRequestHeader.ContentType:		return "Content-Type";
			case HttpRequestHeader.ContentEncoding:		return "Content-Encoding";
			case HttpRequestHeader.ContentLanguage:		return "Content-Language";
			case HttpRequestHeader.ContentLocation:		return "Content-Location";
			case HttpRequestHeader.ContentMd5:		return "Content-MD5";
			case HttpRequestHeader.ContentRange:		return "Content-Range";
			case HttpRequestHeader.Expires:			return "Expires";
			case HttpRequestHeader.LastModified:		return "Last-Modified";
			case HttpRequestHeader.Accept:			return "Accept";
			case HttpRequestHeader.AcceptCharset:		return "Accept-Charset";
			case HttpRequestHeader.AcceptEncoding:		return "Accept-Encoding";
			case HttpRequestHeader.AcceptLanguage:		return "Accept-Language";
			case HttpRequestHeader.Authorization:		return "Authorization";
			case HttpRequestHeader.Cookie:			return "Cookie";
			case HttpRequestHeader.Expect:			return "Expect";
			case HttpRequestHeader.From:			return "From";
			case HttpRequestHeader.Host:			return "Host";
			case HttpRequestHeader.IfMatch:			return "If-Match";
			case HttpRequestHeader.IfModifiedSince:		return "If-Modified-Since";
			case HttpRequestHeader.IfNoneMatch:		return "If-None-Match";
			case HttpRequestHeader.IfRange:			return "If-Range";
			case HttpRequestHeader.IfUnmodifiedSince:	return "If-Unmodified-Since";
			case HttpRequestHeader.MaxForwards:		return "Max-Forwards";
			case HttpRequestHeader.ProxyAuthorization:	return "Proxy-Authorization";
			case HttpRequestHeader.Referer:			return "Referer";
			case HttpRequestHeader.Range:			return "Range";
			case HttpRequestHeader.Te:			return "TE";
			case HttpRequestHeader.Translate:		return "Translate";
			case HttpRequestHeader.UserAgent:		return "User-Agent";
			default:
				throw new IndexOutOfRangeException ();
			}
		}

		internal static void ValidateHeader (string header)
		{
			switch (header.ToLowerInvariant ()) {
			case "connection":
			case "date":
			case "keep-alive":
			case "trailer":
			case "transfer-encoding":
			case "upgrade":
			case "via":
			case "warning":
			case "allow":
			case "content-length":
			case "content-type":
			case "content-location":
			case "content-range":
			case "last-modified":
			case "accept":
			case "accept-charset":
			case "accept-encoding":
			case "accept-language":
			case "cookie":
			case "expect":
			case "host":
			case "if-modified-since":
			case "max-forwards":
			case "referer":
			case "te":
			case "user-agent":
			// extra (not HttpRequestHeader defined) headers that are not accepted by SL2
			// note: the HttpResponseHeader enum is not available in SL2
			case "accept-ranges":
			case "age":
			case "allowed":
			case "connect":
			case "content-transfer-encoding":
			case "delete":
			case "etag":
			case "get":
			case "head":
			case "location":
			case "options":
			case "post":
			case "proxy-authenticate":
			case "proxy-connection":
			case "public":
			case "put":
			case "request-range":
			case "retry-after":
			case "server":
			case "sec-headertest":
			case "sec-":
			case "trace":
			case "uri":
			case "vary":
			case "www-authenticate":
			case "x-flash-version":
				throw new ArgumentException ("header");
			default:
				return;
			}
		}
	}
}

#endif
