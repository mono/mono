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
using System.Web.Compilation;
using System.Web.Services.Protocols;
using System.Web.UI;

namespace System.Web.Script.Services
{
	sealed class ScriptHandlerFactory : IHttpHandlerFactory
	{
		readonly WebServiceHandlerFactory _wsFactory;
		public ScriptHandlerFactory () {
			_wsFactory = new WebServiceHandlerFactory ();
		}
		#region IHttpHandlerFactory Members

		public IHttpHandler GetHandler (HttpContext context, string requestType, string url, string pathTranslated) {
			HttpRequest request = context.Request;
			string contentType = request.ContentType;
			if (!String.IsNullOrEmpty (contentType) && contentType.StartsWith ("application/json", StringComparison.OrdinalIgnoreCase)) {
				Type handlerType = null;
				if (url.EndsWith (ProfileService.DefaultWebServicePath, StringComparison.Ordinal))
					handlerType = typeof (ProfileService);
				else
				if (url.EndsWith (AuthenticationService.DefaultWebServicePath, StringComparison.Ordinal))
					handlerType = typeof(AuthenticationService);
				else {
#if !TARGET_JVM
					handlerType = BuildManager.GetCompiledType (url);
#endif
					if (handlerType == null)
						handlerType = WebServiceParser.GetCompiledType (url, context);
				}

				return RestHandler.GetHandler (context, handlerType, url);
			}
			if (request.PathInfo.StartsWith ("/js", StringComparison.OrdinalIgnoreCase))
				return new ClientProxyHandler (WebServiceParser.GetCompiledType (url, context), url);

			return _wsFactory.GetHandler (context, requestType, url, pathTranslated);
		}

		public void ReleaseHandler (IHttpHandler handler) {

		}

		#endregion
	}
}
