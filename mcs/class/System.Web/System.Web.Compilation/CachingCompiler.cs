//
// System.Web.Compilation.CachingCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (c) Copyright Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.Caching;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	class CachingCompiler
	{
		static object compilationLock = new object ();
		const string cachePrefix = "@@Assembly";
		const string cacheTypePrefix = "@@@Type";

		public static void InsertType (Type type, string filename)
		{
			string [] cacheKeys = new string [] { cachePrefix + filename };
			CacheDependency dep = new CacheDependency (null, cacheKeys);
			HttpRuntime.Cache.Insert (cacheTypePrefix + filename, type, dep);
		}

		public static Type GetTypeFromCache (string filename)
		{
			return (Type) HttpRuntime.Cache [cacheTypePrefix + filename];
		}

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
				ArrayList realdeps = new ArrayList (assemblies.Count + 1);
				realdeps.Add (file);
				for (int i = assemblies.Count - 1; i >= 0; i--) {
					string current = (string) assemblies [i];
					if (Path.IsPathRooted (current))
						realdeps.Add (current);
				}

				string [] deps = (string []) realdeps.ToArray (typeof (string));
				cache.Insert (cachePrefix + key, results, new CacheDependency (deps));
			}

			return results;
		}

		public static Type CompileAndGetType (string typename, string language, string key,
						string file, ArrayList assemblies)
		{
			CompilerResults result = CachingCompiler.Compile (language, key, file, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				StreamReader reader = new StreamReader (file);
				throw new CompilationException (file, result.Errors, reader.ReadToEnd ());
			}

			Assembly assembly = result.CompiledAssembly;
			if (assembly == null) {
				StreamReader reader = new StreamReader (file);
				throw new CompilationException (file, result.Errors, reader.ReadToEnd ());
			}
		
			Type type = assembly.GetType (typename, true);
			InsertType (type, file);
			return type;
		}
	}
}

