//
// System.Web.Compilation.WebServiceCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Web.UI;

namespace System.Web.Compilation
{
	class WebServiceCompiler
	{
		private WebServiceCompiler ()
		{
		}

		public static Type CompileIntoType (SimpleWebHandlerParser wService)
		{
			string sourceFile = GenerateSourceFile (wService);
			Type type = TemplateFactory.GetTypeFromSource (wService.PhysicalPath, sourceFile);
			if (type.FullName != wService.ClassName)
				throw new ApplicationException (String.Format (
								"Class={0}, but the class compiled is {1}",
								wService.ClassName,
								type.FullName));
								
			return type;
		}

		private static string GenerateSourceFile (SimpleWebHandlerParser wService)
		{
			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName ();
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (wService.Program);
			output.Close ();
			return csName;
		}
	}
}

