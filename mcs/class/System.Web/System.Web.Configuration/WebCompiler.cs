//
// System.Web.Configuration.WebCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.CodeDom.Compiler;

namespace System.Web.Configuration
{
	class WebCompiler
	{
		public string Languages;
		public string Extension;
		public string Type;
		public int WarningLevel;
		public string CompilerOptions;
		public CodeDomProvider Provider;

		public override string ToString ()
		{
			return "Languages: " + Languages + "\n" +
				"Extension: " + Extension + "\n" +
				"Type: " + Type + "\n" +
				"WarningLevel: " + WarningLevel + "\n" +
				"CompilerOptions: " + CompilerOptions + "\n";
		}
	}
}

