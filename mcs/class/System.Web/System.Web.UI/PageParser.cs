//
// System.Web.UI.PageParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	public sealed class PageParser : TemplateControlParser
	{
		public static IHttpHandler GetCompiledPageInstance (string virtualPath,
								    string inputFile, 
								    HttpContext context)
		{
			PageParser pp = new PageParser ();
			return (IHttpHandler) pp.GetCompiledInstance (virtualPath, inputFile, context);
		}

		protected override Type CompileIntoType ()
		{
			return PageCompiler.CompilePageType (this);
		}

		protected override Type DefaultBaseType
		{
			get {
				return typeof (Page);
			}
		}

		protected override string DefaultDirectiveName
		{
			get {
				return "page";
			}
		}
	}
}

