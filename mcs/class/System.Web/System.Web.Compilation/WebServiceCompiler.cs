//
// System.Web.Compilation.WebServiceCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2040 Novell, Inc. (http://www.novell.com)
//
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Web.Configuration;
using System.Web.UI;
using System.Reflection;

namespace System.Web.Compilation
{
	class WebServiceCompiler : BaseCompiler
	{
		SimpleWebHandlerParser parser;
		ICodeCompiler compiler;
		CodeDomProvider provider;
		CompilerParameters compilerParameters;
		string inputFile;

		public WebServiceCompiler (SimpleWebHandlerParser wService)
			: base (null)
		{
			this.parser = wService;
		}

		public static Type CompileIntoType (SimpleWebHandlerParser wService)
		{
			WebServiceCompiler wsc = new WebServiceCompiler (wService);
			return wsc.GetCompiledType ();
		}

		public override Type GetCompiledType ()
		{
			if (parser.Program.Trim () == "")
				return parser.GetTypeFromBin (parser.ClassName);

			CompilerResults results = (CompilerResults) HttpRuntime.Cache [parser.PhysicalPath];
			if (results != null) {
				Assembly a = results.CompiledAssembly;
				if (a != null)
					return a.GetType (parser.ClassName, true);
			}

			string lang = parser.Language;
			CompilationConfiguration config;
			config = CompilationConfiguration.GetInstance (parser.Context);
			provider = config.GetProvider (lang);
			if (provider == null)
				throw new HttpException ("Configuration error. Language not supported: " +
							  lang, 500);

			compiler = provider.CreateCompiler ();

			compilerParameters = CachingCompiler.GetOptions (parser.Assemblies);
			compilerParameters.IncludeDebugInformation = parser.Debug;
			compilerParameters.CompilerOptions = config.GetCompilerOptions (lang);
			compilerParameters.WarningLevel = config.GetWarningLevel (lang);

			bool keepFiles = (Environment.GetEnvironmentVariable ("MONO_ASPNET_NODELETE") != null);

			TempFileCollection tempcoll;
			tempcoll = new TempFileCollection (config.TempDirectory, keepFiles);
			compilerParameters.TempFiles = tempcoll;

			inputFile = tempcoll.AddExtension (provider.FileExtension);
			Stream st = File.OpenWrite (inputFile);
			StreamWriter sw = new StreamWriter (st);
			sw.WriteLine (parser.Program);
			sw.Close ();

			string dllfilename = tempcoll.AddExtension ("dll", true);
			if (!Directory.Exists (dynamicBase))
				Directory.CreateDirectory (dynamicBase);

			compilerParameters.OutputAssembly = Path.Combine (dynamicBase, dllfilename);

			results = CachingCompiler.Compile (this);
			CheckCompilerErrors (results);
			if (results.CompiledAssembly == null)
				throw new CompilationException (inputFile, results.Errors,
					"No assembly returned after compilation!?");

			return results.CompiledAssembly.GetType (parser.ClassName, true);
		}

		void CheckCompilerErrors (CompilerResults results)
		{
			if (results.NativeCompilerReturnValue == 0)
				return;
 
			throw new CompilationException (parser.PhysicalPath, results.Errors, parser.Program);
		}

		internal new SimpleWebHandlerParser Parser {
			get { return parser; }
		}

		internal override ICodeCompiler Compiler {
			get { return compiler; }
		}

		internal CompilerParameters CompilerOptions {
			get { return compilerParameters; }
		}

		internal string InputFile {
			get { return inputFile; }
		}
	}
}

