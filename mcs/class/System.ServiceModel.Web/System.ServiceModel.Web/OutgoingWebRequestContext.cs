//
// OutgoingWebRequestContext.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Net;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Web
{
	public class OutgoingWebRequestContext
	{
		internal OutgoingWebRequestContext ()
		{
		}

		public string Accept { get; set; }

		public long ContentLength { get; set; }

		public string ContentType { get; set; }

		public WebHeaderCollection Headers { get; private set; }

		public string IfMatch { get; set; }

		public string IfModifiedSince { get; set; }

		public string IfNoneMatch { get; set; }

		public string IfUnmodifiedSince { get; set; }

		public string Method { get; set; }

		public bool SuppressEntityBody { get; set; }

		public string UserAgent { get; set; }

		internal void Apply (HttpRequestMessageProperty hp)
		{
			if (Headers != null)
				foreach (var key in Headers.AllKeys)
					hp.Headers [key] = Headers [key];
			if (Accept != null)
				hp.Headers ["Accept"] = Accept;
			if (ContentLength > 0)
				hp.Headers ["Content-Length"] = ContentLength.ToString (NumberFormatInfo.InvariantInfo);
			if (ContentType != null)
				hp.Headers ["Content-Type"] = ContentType;
			if (IfMatch != null)
				hp.Headers ["If-Match"] = IfMatch;
			if (IfModifiedSince != null)
				hp.Headers ["If-Modified-Since"] = IfModifiedSince;
			if (IfNoneMatch != null)
				hp.Headers ["If-None-Match"] = IfNoneMatch;
			if (IfUnmodifiedSince != null)
				hp.Headers ["If-Unmodified-Since"] = IfUnmodifiedSince;
			if (Method != null)
				hp.Method = Method;
			if (SuppressEntityBody)
				hp.SuppressEntityBody = true;
			if (UserAgent != null)
				hp.Headers ["User-Agent"] = UserAgent;
		}
	}
}
