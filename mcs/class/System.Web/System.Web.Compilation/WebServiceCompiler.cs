//
// System.Web.Compilation.WebServiceCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Web.UI;
using System.CodeDom.Compiler;
//temp:
using Microsoft.CSharp;

namespace System.Web.Compilation
{
	class WebServiceCompiler : BaseCompiler
	{
		SimpleWebHandlerParser wService;
		ICodeCompiler compiler;

		public WebServiceCompiler (SimpleWebHandlerParser wService)
			: base (null)
		{
			this.wService = wService;
		}

		public static Type CompileIntoType (SimpleWebHandlerParser wService)
		{
			WebServiceCompiler wsc = new WebServiceCompiler (wService);
			return wsc.GetCompiledType ();
		}

		public override Type GetCompiledType ()
		{
			if (wService.Program.Trim () == "")
				return wService.GetTypeFromBin (wService.ClassName);

			//FIXME: update when we support other languages
			string fname = Path.ChangeExtension (Path.GetTempFileName (), ".cs");
			StreamWriter sw = new StreamWriter (File.OpenWrite (fname));
			sw.WriteLine (wService.Program);
			sw.Close ();

			//TODO: get the compiler and default options from system.web/compileroptions
			CompilerResults results = CachingCompiler.Compile (this, fname);
			CheckCompilerErrors (results);

			return results.CompiledAssembly.GetType (wService.ClassName, true);
		}

		void CheckCompilerErrors (CompilerResults results)
		{
			if (results.NativeCompilerReturnValue == 0)
				return;

			throw new CompilationException (wService.PhysicalPath, results.Errors, wService.Program);
		}

		internal new SimpleWebHandlerParser Parser {
			get { return wService; }
		}

		internal override ICodeCompiler Compiler {
			get {
				if (compiler == null) {
					//TODO: get the compiler and default options from system.web/compileroptions
					compiler = new CSharpCodeProvider ().CreateCompiler ();
				}

				return compiler;
			}
		}
	}
}

