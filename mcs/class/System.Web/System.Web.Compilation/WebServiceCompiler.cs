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
using System.Reflection;
using System.Web.UI;

namespace System.Web.Compilation
{
	class WebServiceCompiler : BaseCompiler
	{
		SimpleWebHandlerParser wService;
		string sourceFile;

		private WebServiceCompiler (SimpleWebHandlerParser wService)
		{
			this.wService = wService;
		}

		public static Type CompileIntoType (SimpleWebHandlerParser wService)
		{
			CompilationCacheItem item = CachingCompiler.GetCached (wService.PhysicalPath);
			if (item != null && item.Result != null) {
				if (item.Result != null)
					return item.Result.Data as Type;

				throw new CompilationException (item.Result);
			}

			WebServiceCompiler wsc = new WebServiceCompiler (wService);
			return wsc.GetCompiledType ();
		}

		static string GenerateSourceFile (SimpleWebHandlerParser wService)
		{
			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName ();
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (wService.Program);
			output.Close ();
			return csName;
		}

		public override Type GetCompiledType ()
		{
			sourceFile = GenerateSourceFile (wService);

			CachingCompiler compiler = new CachingCompiler (this);
			CompilationResult result = new CompilationResult ();
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
				
			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			Type type = types [0];
			if (type.FullName != wService.ClassName)
				throw new ApplicationException (String.Format (
								"Class={0}, but the class compiled is {1}",
								wService.ClassName,
								type.FullName));
								
			result.Data = type;
			return type;
		}

		public override string Key {
			get {
				return wService.PhysicalPath;
			}
		}

		public override string SourceFile {
			get {
				return sourceFile;
			}
		}
	}
}

