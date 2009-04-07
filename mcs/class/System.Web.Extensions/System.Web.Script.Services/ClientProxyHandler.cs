//
// ScriptHandlerFactory.cs
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

namespace System.Web.Script.Services
{
	sealed class ClientProxyHandler : IHttpHandler
	{
		readonly LogicalTypeInfo _logicalTypeInfo;
		readonly Type _type;
		
		public ClientProxyHandler (Type type, string filePath)
		{
			_type = type;
			_logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (type, filePath);
		}
		#region IHttpHandler Members

		public bool IsReusable {
			get { return false; }
		}

		public void ProcessRequest (HttpContext context)
		{
			HttpResponse response = context.Response;
			object[] attributes = _type.GetCustomAttributes (typeof (ScriptServiceAttribute), true);
			if (attributes.Length == 0) {
				response.ContentType = "text/html";
				throw new InvalidOperationException ("Only Web services with a [ScriptService] attribute on the class definition can be called from script.");
			}

			response.ContentType = "application/x-javascript";
			response.Cache.SetExpires (DateTime.UtcNow.AddYears (1));
			response.Cache.SetValidUntilExpires (true);
			response.Cache.SetCacheability (HttpCacheability.Public);
			response.Output.Write (_logicalTypeInfo.Proxy);
		}

		#endregion
	}
}
