//
// System.Web.UI.WebHandlerParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	class WebHandlerParser : SimpleWebHandlerParser
	{
		private WebHandlerParser (HttpContext context, string virtualPath, string physicalPath)
			: base (context, virtualPath, physicalPath)
		{
		}

		public static Type GetCompiledType (HttpContext context, string virtualPath, string physicalPath)
		{
			WebHandlerParser parser = new WebHandlerParser (context, virtualPath, physicalPath);
			// WebServiceCompiler also does this job.
			return WebServiceCompiler.CompileIntoType (parser);
		}

		protected override string DefaultDirectiveName {
			get {
				return "webhandler";
			}
		}
	}
}

