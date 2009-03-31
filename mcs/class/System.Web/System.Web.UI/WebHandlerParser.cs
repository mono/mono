//
// System.Web.UI.WebHandlerParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2008 Novell, Inc (http://novell.com/)

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

using System.Web;
using System.Web.Compilation;
using System.IO;

namespace System.Web.UI
{
	class WebHandlerParser : SimpleWebHandlerParser
	{
		WebHandlerParser (HttpContext context, string virtualPath, string physicalPath)
			: base (context, virtualPath, physicalPath)
		{
		}

#if NET_2_0
		internal WebHandlerParser (HttpContext context, VirtualPath virtualPath, TextReader reader)
			: this (context, virtualPath, null, reader)
		{
		}
		
		internal WebHandlerParser (HttpContext context, VirtualPath virtualPath, string physicalPath, TextReader reader)
			: base (context, virtualPath.Original, physicalPath, reader)
		{
		}
#endif

		public static Type GetCompiledType (HttpContext context, string virtualPath, string physicalPath)
		{
			WebHandlerParser parser = new WebHandlerParser (context, virtualPath, physicalPath);
			Type type = parser.GetCompiledTypeFromCache ();
			if (type != null)
				return type;

			return WebServiceCompiler.CompileIntoType (parser);
		}

		protected override string DefaultDirectiveName {
			get {
				return "webhandler";
			}
		}
	}
}

