//
// System.Web.Compilation.CachingCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (c) Copyright Novell, Inc. (http://www.novell.com)
//
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.Caching;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	class CachingCompiler
	{
		static object compilationLock = new object ();
		const string cachePrefix = "@@Assembly";

		public static CompilerResults Compile (BaseCompiler compiler)
		{
			Cache cache = HttpRuntime.Cache;
			string key = cachePrefix + compiler.Parser.InputFile;
			CompilerResults results = (CompilerResults) cache [key];
			if (results != null)
				return results;

			lock (compilationLock) {
				results = (CompilerResults) cache [key];
				if (results != null)
					return results;

				ICodeCompiler comp = compiler.Compiler;
				results = comp.CompileAssemblyFromDom (compiler.CompilerParameters, compiler.Unit);
				string [] deps = (string []) compiler.Parser.Dependencies.ToArray (typeof (string));
				cache.Insert (key, results, new CacheDependency (deps));
			}

			return results;
		}

		public static CompilerResults Compile (WebServiceCompiler compiler)
		{
			string key = cachePrefix + compiler.Parser.PhysicalPath;
			Cache cache = HttpRuntime.Cache;
			CompilerResults results = (CompilerResults) cache [key];
			if (results != null)
				return results;

			lock (compilationLock) {
				results = (CompilerResults) cache [key];
				if (results != null)
					return results;

				SimpleWebHandlerParser parser = compiler.Parser;
				CompilerParameters options = compiler.CompilerOptions;
				options.IncludeDebugInformation = parser.Debug;
				results = compiler.Compiler.CompileAssemblyFromFile (options, compiler.InputFile);
				string [] deps = (string []) parser.Dependencies.ToArray (typeof (string));
				cache.Insert (key, results, new CacheDependency (deps));
			}

			return results;
		}

		internal static CompilerParameters GetOptions (ICollection assemblies)
		{
			CompilerParameters options = new CompilerParameters ();
			if (assemblies != null) {
				StringCollection coll = options.ReferencedAssemblies;
				foreach (string str in assemblies)
					coll.Add (str);
			}

			return options;
		}

		public static CompilerResults Compile (string language, string key, string file,
							ArrayList assemblies)
		{
			Cache cache = HttpRuntime.Cache;
			CompilerResults results = (CompilerResults) cache [cachePrefix + key];
			if (results != null)
				return results;

			lock (compilationLock) {
				results = (CompilerResults) cache [cachePrefix + key];
				if (results != null)
					return results;
 
				CompilationConfiguration config;
				config = CompilationConfiguration.GetInstance (HttpContext.Current);
				CodeDomProvider provider = config.GetProvider (language);
				if (provider == null)
					throw new HttpException ("Configuration error. Language not supported: " +
								  language, 500);

				ICodeCompiler compiler = provider.CreateCompiler ();
				CompilerParameters options = GetOptions (assemblies);
				results = compiler.CompileAssemblyFromFile (options, file);
				string [] deps = (string []) assemblies.ToArray (typeof (string));
				cache.Insert (cachePrefix + key, results, new CacheDependency (deps));
			}

			return results;
		}
	}
}

