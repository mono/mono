//
// OutgoingWebResponseContext.cs
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
	public class OutgoingWebResponseContext
	{
		WebHeaderCollection headers;
		long content_length;
		string content_type, etag, location, status_desc;
		DateTime last_modified;
		HttpStatusCode status_code;
		bool suppress_body;

		internal OutgoingWebResponseContext ()
		{
		}

		internal void Apply (HttpResponseMessageProperty hp)
		{
			if (headers != null)
				hp.Headers.Add (headers);
			if (content_length != 0)
				hp.Headers ["Content-Length"] = content_length.ToString (NumberFormatInfo.InvariantInfo);
			if (content_type != null)
				hp.Headers ["Content-Type"] = content_type;
			if (etag != null)
				hp.Headers ["ETag"] = etag;
			if (location != null)
				hp.Headers ["Location"] = location;
			if (last_modified != default (DateTime))
				hp.Headers ["Last-Modified"] = last_modified.ToString ("R");
			if (status_code != default (HttpStatusCode))
				hp.StatusCode = status_code;
			if (status_desc != null)
				hp.StatusDescription = status_desc;
			hp.SuppressEntityBody = suppress_body;
		}

		public long ContentLength {
			get { return content_length; }
			set { content_length = value; }
		}

		public string ContentType {
			get { return content_type; }
			set { content_type = value; }
		}

		public string ETag {
			get { return etag; }
			set { etag = value; }
		}

		public WebHeaderCollection Headers {
			get {
				if (headers == null)
					headers = new WebHeaderCollection ();
				return headers;
			}
		}

		public DateTime LastModified {
			get { return last_modified; }
			set { last_modified = value; }
		}

		public string Location {
			get { return location; }
			set { location = value; }
		}

		public HttpStatusCode StatusCode {
			get { return status_code; }
			set { status_code = value; }
		}

		public string StatusDescription {
			get { return status_desc; }
			set { status_desc = value; }
		}

		public bool SuppressEntityBody {
			get { return suppress_body; }
			set { suppress_body = value; }
		}

		public void SetStatusAsCreated (Uri locationUri)
		{
			StatusCode = HttpStatusCode.Created;
			Location = locationUri.AbsoluteUri;
		}

		public void SetStatusAsNotFound ()
		{
			StatusCode = HttpStatusCode.NotFound;
		}

		public void SetStatusAsNotFound (string description)
		{
			StatusCode = HttpStatusCode.NotFound;
			StatusDescription = description;
		}
	}
}
