//
// System.Web.UI.ApplicationFileParser.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
//
using System;
using System.Collections;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	sealed class ApplicationFileParser : TemplateParser
	{
		public ApplicationFileParser (string fname, HttpContext context)
		{
			InputFile = fname;
			Context = context;
		}
		
		protected override Type CompileIntoType ()
		{
			return GlobalAsaxCompiler.CompileApplicationType (this);
		}

		internal static Type GetCompiledApplicationType (string inputFile, HttpContext context)
		{
			ApplicationFileParser parser = new ApplicationFileParser (inputFile, context);
			return GlobalAsaxCompiler.CompileApplicationType (parser);
		}

		internal override void AddDirective (string directive, Hashtable atts)
		{
			if (String.Compare (directive, "application", true) != 0 &&
			    String.Compare (directive, "Import", true) != 0 &&
			    String.Compare (directive, "Assembly", true) != 0)
				ThrowParseException ("Invalid directive: " + directive);

			base.AddDirective (directive, atts);
		}

		internal override Type DefaultBaseType {
			get { return typeof (HttpApplication); }
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.HttpApplication"; }
		}

		internal override string DefaultDirectiveName {
			get { return "application"; }
		}

		internal override string BaseVirtualDir {
			get { return Context.Request.ApplicationPath; }
		}
	}

}

