//
// System.Web.Compilation.WebServiceCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web.UI;

namespace System.Web.Compilation
{
	class WebServiceCompiler : BaseCompiler
	{
		SimpleWebHandlerParser wService;

		[MonoTODO]
		public WebServiceCompiler (SimpleWebHandlerParser wService)
			: base (null)
		{
			this.wService = wService;
		}

		[MonoTODO]
		public static Type CompileIntoType (SimpleWebHandlerParser wService)
		{
			return null;
		}
	}
}

