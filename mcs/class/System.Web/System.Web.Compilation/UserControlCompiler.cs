//
// System.Web.Compilation.UserControlCompiler
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
	class UserControlCompiler : TemplateControlCompiler
	{
		UserControlParser parser;

		public UserControlCompiler (UserControlParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		public static Type CompileUserControlType (UserControlParser userControlParser)
		{
			UserControlCompiler pc = new UserControlCompiler (userControlParser);
			return pc.GetCompiledType ();
		}
	}
}

