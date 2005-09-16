//
// Microsoft.Web.Services.ScriptHandlerFactory
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
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

#if NET_2_0

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services
{
	class JSProxyGenerator : IHttpHandler
	{
		Type type;
		string virtualPath;

		public JSProxyGenerator (Type type, string virtualPath)
		{
			this.type = type;
			this.virtualPath = virtualPath;
		}

		public bool IsReusable {
			get {
				return false;
			}
		}

		public void ProcessRequest (HttpContext context)
		{
			TextWriter output = context.Response.Output;

			output.Write (String.Format ("Type.registerNamespace('{0}'); ", type.Namespace));
			output.Write (String.Format (@"{0} = {{ path: ""{1}""", type.FullName, virtualPath));

			foreach (MethodInfo m in type.GetMethods()) {
				object[] attrs = m.GetCustomAttributes (typeof (WebMethodAttribute), false);
				if (m.IsPublic && attrs != null && attrs.Length > 0) {
					/* it's a webmethod, output it */
					output.Write (", {0}:function (", m.Name);
					foreach (ParameterInfo p in m.GetParameters()) {
						output.Write (p.Name + ",");
					}
					output.Write (String.Format (" onMethodComplete, onMethodTimeout) {{ return Web.Net.ServiceMethodRequest.callMethod(this.path, \"{0}\", {{", m.Name));
					foreach (ParameterInfo p in m.GetParameters()) {
						output.Write ("{0}:{0}", p.Name);
					}
					output.Write ("}, onMethodComplete,onMethodTimeout); } }");
				}
			}
		}
	}

	public class ScriptHandlerFactory : IHttpHandlerFactory
	{
		WebServiceHandlerFactory fallback;

		public ScriptHandlerFactory ()
		{
			fallback = new WebServiceHandlerFactory();
		}

		public virtual IHttpHandler GetHandler (HttpContext context, string requestType, string virtualPath, string path)
		{
			if (context.Request.PathInfo == "/js") {
				Type type = WebServiceParser.GetCompiledType (path, context);

				return new JSProxyGenerator (type, virtualPath);
			}
			else {
				return fallback.GetHandler (context, requestType, virtualPath, path);
			}

		}

		public virtual void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}

#endif
