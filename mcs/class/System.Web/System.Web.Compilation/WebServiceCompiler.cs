//
// System.Web.Compilation.WebServiceCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2040 Novell, Inc. (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
			Type type = CachingCompiler.GetTypeFromCache (parser.PhysicalPath);
			if (type != null)
				return type;

			if (parser.Program.Trim () == "") {
				type = parser.GetTypeFromBin (parser.ClassName);
				CachingCompiler.InsertType (type, parser.PhysicalPath);
				return type;
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

			string dllfilename = Path.GetFileName (tempcoll.AddExtension ("dll", true));
			if (!Directory.Exists (dynamicBase))
				Directory.CreateDirectory (dynamicBase);

			compilerParameters.OutputAssembly = Path.Combine (dynamicBase, dllfilename);

			CompilerResults results = CachingCompiler.Compile (this);
			CheckCompilerErrors (results);
			if (results.CompiledAssembly == null)
				throw new CompilationException (inputFile, results.Errors,
					"No assembly returned after compilation!?");

			results.TempFiles.Delete ();
			type = results.CompiledAssembly.GetType (parser.ClassName, true);
			CachingCompiler.InsertType (type, parser.PhysicalPath);
			return type;
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

