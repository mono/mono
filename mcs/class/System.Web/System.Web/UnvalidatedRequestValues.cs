//
// UnvalidatedRequestValues.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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

using System;
using System.Collections.Specialized;
using System.Runtime;

namespace System.Web {
	public sealed class UnvalidatedRequestValues {
		private readonly HttpRequest httpRequest;
		private NameValueCollection headers;

		public NameValueCollection Form
		{
			get
			{
				return httpRequest.FormUnvalidated;
			}
		}

		public NameValueCollection QueryString
		{
			get
			{
				return httpRequest.QueryStringUnvalidated;
			}
		}

		public NameValueCollection Headers
		{
			get
			{
				return headers;
			}
		}

		public HttpCookieCollection Cookies
		{
			get
			{
				return httpRequest.CookiesNoValidation;
			}
		}

		public HttpFileCollection Files
		{
			get
			{
				return httpRequest.Files;
			}
		}

		public string RawUrl
		{
			get
			{
				return httpRequest.RawUrlUnvalidated;
			}
		}

		public string Path
		{
			get
			{
				return httpRequest.PathNoValidation;
			}
		}

		public string PathInfo
		{
			get
			{
				return httpRequest.PathInfoNoValidation;
			}
		}

		public string this [string field]
		{
			get
			{
				HttpCookie cookie;
				return
					Form [field] ??
					((cookie = Cookies [field]) != null ? cookie.Value : null) ??
					QueryString [field] ??
					httpRequest.ServerVariables [field];
			}
		}

		public Uri Url
		{
			get
			{
				return httpRequest.Url;
			}
		}

		internal UnvalidatedRequestValues (HttpRequest request)
		{
			this.httpRequest = request;
			this.headers = new HeadersCollection (httpRequest);
		}
	}
}
