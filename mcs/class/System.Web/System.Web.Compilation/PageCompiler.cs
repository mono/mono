//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web.UI;

namespace System.Web.Compilation
{
	class PageCompiler
	{
		private PageParser pageParser;

		internal PageCompiler (PageParser pageParser)
		{
			this.pageParser = pageParser;
		}

		public static Type CompilePageType (PageParser pageParser)
		{
			return TemplateFactory.GetTypeFromSource (pageParser.InputFile);
		}
	}
}
