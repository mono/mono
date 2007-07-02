//
// RestHandler.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.IO;

namespace System.Web.Script.Services
{
	class RestHandler : IHttpHandler
	{
		sealed class NameValueCollectionDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly NameValueCollection _nmc;
			public NameValueCollectionDictionary (NameValueCollection nmc) {
				_nmc = nmc;
			}
			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				for (int i = 0, max = _nmc.Count; i < max; i++)
					yield return new KeyValuePair<string, object> (_nmc.GetKey (i), _nmc.Get (i));
			}
		}

		readonly LogicalTypeInfo _logicalTypeInfo;
		public RestHandler (Type type, string filePath) {
			_logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (type, filePath);
		}
		#region IHttpHandler Members

		public bool IsReusable {
			get { return false; }
		}

		public void ProcessRequest (HttpContext context) {
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			response.ContentType = "application/json";
			string method = request.PathInfo.Substring(1);
			if ("GET".Equals (request.RequestType, StringComparison.OrdinalIgnoreCase))
				_logicalTypeInfo.Invoke (method,
					new NameValueCollectionDictionary (request.QueryString),
					response.Output);
			else
				_logicalTypeInfo.Invoke(method,
					new StreamReader(request.InputStream, request.ContentEncoding),
					response.Output);
		}
		/*
		static Encoding GetContentEncoding (string cts, out string content_type) {
			string encoding;

			if (cts == null)
				cts = "";

			encoding = "utf-8";
			int start = 0;
			int idx = cts.IndexOf (';');
			if (idx == -1)
				content_type = cts;
			else
				content_type = cts.Substring (0, idx);

			content_type = content_type.Trim ();
			for (start = idx + 1; idx != -1; ) {
				idx = cts.IndexOf (';', start);
				string body;
				if (idx == -1)
					body = cts.Substring (start);
				else {
					body = cts.Substring (start, idx - start);
					start = idx + 1;
				}
				body = body.Trim ();
				if (String.CompareOrdinal (body, 0, "charset=", 0, 8) == 0) {
					encoding = body.Substring (8);
					encoding = encoding.TrimStart (trimChars).TrimEnd (trimChars);
				}
			}

			return Encoding.GetEncoding (encoding);
		}*/

		static readonly char [] trimChars = { '"', '\'' };

		#endregion
	}
}
