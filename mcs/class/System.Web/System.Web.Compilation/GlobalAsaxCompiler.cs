//
// System.Web.Compilation.GlobalAsaxCompiler
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
	class GlobalAsaxCompiler : BaseCompiler
	{
		ApplicationFileParser parser;

		public GlobalAsaxCompiler (ApplicationFileParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		public static Type CompileApplicationType (ApplicationFileParser parser)
		{
			GlobalAsaxCompiler compiler = new GlobalAsaxCompiler (parser);
			return compiler.GetCompiledType ();
		}

		[MonoTODO("Process application scope for object tags")]
		protected override void ProcessObjectTag (ObjectTagBuilder tag)
		{
			//TODO
		}
	}
}

