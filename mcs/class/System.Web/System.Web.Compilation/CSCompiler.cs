//
// System.Web.Compilation.CSCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.CSharp;

namespace System.Web.Compilation
{
	class CSCompiler
	{
		static CodeDomProvider provider;
		static ICodeCompiler compiler;
		string filename;
		ArrayList assemblies;
		CompilerParameters options;

		static CSCompiler ()
		{
			provider = new CSharpCodeProvider ();
			compiler = provider.CreateCompiler ();
		}

		private CSCompiler (string filename, ArrayList assemblies)
		{
			this.filename = filename;
			this.assemblies = assemblies;
			options = new CompilerParameters ();
			if (assemblies != null) {
				StringCollection coll = new StringCollection ();
				foreach (string str in assemblies)
					coll.Add (str);

				options.ReferencedAssemblies = coll;
			}
		}

		public Assembly GetCompiledAssembly ()
		{
			CompilerResults results = compiler.CompileAssemblyFromFile (options, filename);
			if (results.NativeCompilerReturnValue != 0) {
				StreamReader reader = new StreamReader (filename);
				throw new CompilationException (filename, results.Errors, reader.ReadToEnd ());
			}

			return results.CompiledAssembly;
		}

		static public Assembly CompileCSFile (string file, ArrayList assemblies)
		{
			CSCompiler compiler = new CSCompiler (file, assemblies);
			return compiler.GetCompiledAssembly ();
		}

		static public CodeDomProvider Provider {
			get { return provider; }
		}

		static public ICodeCompiler Compiler {
			get { return compiler; }
		}
	}
}

