//
// System.Web.UI.WebServiceParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

using System.IO;
using System.Security.Permissions;
using System.Web.Compilation;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class WebServiceParser : SimpleWebHandlerParser
	{
		WebServiceParser (HttpContext context, string virtualPath, string physicalPath)
			: base (context, virtualPath, physicalPath)
		{
		}

#if NET_2_0
		internal WebServiceParser (HttpContext context, VirtualPath virtualPath, TextReader reader)
			: this (context, virtualPath, null, reader)
		{
		}
		
		internal WebServiceParser (HttpContext context, VirtualPath virtualPath, string physicalPath, TextReader reader)
			: base (context, virtualPath.Original, physicalPath, reader)
		{
		}
#endif

		public static Type GetCompiledType (string inputFile, HttpContext context)
		{
#if NET_2_0
			return BuildManager.GetCompiledType (inputFile);
#else
			string physPath;
			HttpRequest req = context != null ? context.Request : null;
			
			if (req != null)
				physPath = req.MapPath (inputFile);
			else // likely to fail
				physPath = inputFile;
			
			WebServiceParser parser = new WebServiceParser (context, inputFile, physPath);
			Type type = parser.GetCompiledTypeFromCache ();
			if (type != null)
				return type;

			return WebServiceCompiler.CompileIntoType (parser);
#endif
		}

		protected override string DefaultDirectiveName {
			get {
				return "webservice";
			}
		}
	}
}

