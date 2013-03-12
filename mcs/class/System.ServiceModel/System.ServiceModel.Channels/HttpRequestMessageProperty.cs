//
// HttpRequestMessageProperty.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Net;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public sealed class HttpRequestMessageProperty
#if NET_4_5
		: IMessageProperty
#endif
	{
		public static string Name {
			get { return "httpRequest"; }
		}

		WebHeaderCollection headers = new WebHeaderCollection ();
		string method = "POST", query_string = String.Empty;
		bool suppress_entity;

		public HttpRequestMessageProperty ()
		{
		}

		public WebHeaderCollection Headers {
			get { return headers; }
		}

		public string Method {
			get { return method; }
			set { method = value; }
		}

		public string QueryString {
			get { return query_string; }
			set { query_string = value; }
		}

		public bool SuppressEntityBody {
			get { return suppress_entity; }
			set { suppress_entity = value; }
		}
		
		
#if NET_4_5
		IMessageProperty IMessageProperty.CreateCopy ()
		{
			var copy = new HttpRequestMessageProperty ();
			// FIXME: Clone headers?
			copy.headers = headers;
			copy.method = method;
			copy.query_string = query_string;
			copy.suppress_entity = suppress_entity;
			return copy;
		}
#endif
	}
}
