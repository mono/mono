//
// System.Web.UI.WebServiceParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	public class WebServiceParser : SimpleWebHandlerParser
	{
		private WebServiceParser (HttpContext context, string virtualPath, string physicalPath)
			: base (context, virtualPath, physicalPath)
		{
		}

		public static Type GetCompiledType (string inputFile, HttpContext context)
		{
			WebServiceParser parser = new WebServiceParser (context, null, inputFile);
			return WebServiceCompiler.CompileIntoType (parser);
		}

		protected override string DefaultDirectiveName {
			get {
				return "webservice";
			}
		}
	}
}

