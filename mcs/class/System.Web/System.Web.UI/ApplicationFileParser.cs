//
// System.Web.UI.ApplicationfileParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	sealed class ApplicationFileParser : TemplateParser
	{
		public ApplicationFileParser (string fname)
		{
			InputFile = fname;
		}
		
		protected override Type CompileIntoType ()
		{
			GlobalAsaxCompiler compiler = new GlobalAsaxCompiler (this);
			return compiler.GetCompiledType ();
		}

		internal static Type GetCompiledApplicationType (string inputFile, HttpContext context)
		{
			ApplicationFileParser parser = new ApplicationFileParser (inputFile);
			parser.Context = context;
			return parser.CompileIntoType ();
		}

		protected override Type DefaultBaseType {
			get { return typeof (HttpApplication); }
		}

		protected internal override string DefaultDirectiveName {
			get { return "application"; }
		}
	}

}

