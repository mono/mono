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
			string sourceFile = GenerateSourceFile (pageParser.InputFile);
			return TemplateFactory.GetTypeFromSource (sourceFile);
		}

		private static string GenerateSourceFile (string inputFile)
		{
			Stream input = File.OpenRead (inputFile);
			AspParser parser = new AspParser (inputFile, input);
			parser.parse ();
			AspGenerator generator = new Generator (args [i], parser.Elements);
			//FIXME: set properties here -> base type, interfaces,...
			generator.ProcessElements ();
			string code = generator.GetCode ().ReadToEnd ();
			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName ();
			StreamWriter output = File.OpenWrite (csName);
			output.Write (code);
			output.Close ();
			return csName;
		}
	}
}
