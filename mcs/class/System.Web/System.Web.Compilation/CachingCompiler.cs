//
// System.Web.Compilation.CachingCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web.UI;

namespace System.Web.Compilation
{
	//TODO: caching should be done using System.Web.Caching, but that namespace still need some work.
	internal class CompilationCacheItem
	{
		CompilerResults result;
		ArrayList dependencies;
		DateTime reference;

		public CompilationCacheItem (CompilerResults result, ArrayList dependencies)
		{
			this.result = result;
			this.dependencies = dependencies;
			this.reference = DateTime.Now;
		}

		public bool CheckDependencies (string key)
		{
			if (dependencies == null)
				return true; // Forever young

			foreach (string s in dependencies) {
				if (!File.Exists (s) || File.GetLastWriteTime (s) > reference)
					return false;
			}
			
			return true;
		}

		public CompilerResults Result {
			get { return result; }
		}
	}

	internal class CompilationCache
	{
		static Hashtable cache;
		static CompilationCache instance = new CompilationCache ();
		
		private CompilationCache ()
		{
		}

		static CompilationCache ()
		{
			cache = new Hashtable ();
		}

		public static CompilationCache GetInstance ()
		{
			return instance;
		}

		public bool CheckDependencies (CompilationCacheItem item, string key)
		{
			bool result = item.CheckDependencies (key);
			if (result == false)
				cache.Remove (key);

			return result;
		}

		public CompilationCacheItem this [string key] {
			get { return cache [key] as CompilationCacheItem; }
			set { cache [key] = value; }
		}
	}
	
	internal class CachingCompiler
	{
		static CompilationCache cache = CompilationCache.GetInstance ();

		private CachingCompiler () {}

		public static CompilationCacheItem GetCached (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			CompilationCacheItem item = cache [key];
			if (item != null && cache.CheckDependencies (item, key))
				return item;

			return null;
		}

		static object compilationLock = new object ();
		public static CompilerResults Compile (BaseCompiler compiler)
		{
			string key = compiler.Parser.InputFile;
			CompilationCacheItem item = GetCached (key);
			if (item != null)
				return item.Result;
			
			CompilerResults results = null;
			lock (compilationLock) {
				item = GetCached (key);
				if (item != null)
					return item.Result;

				results = compiler.Compiler.CompileAssemblyFromDom (compiler.CompilerParameters, compiler.Unit);
				cache [key] = new CompilationCacheItem (results, compiler.Parser.Dependencies);
			}

			return results;
		}

		static CompilerParameters GetOptions (ICollection assemblies)
		{
			CompilerParameters options = new CompilerParameters ();
			if (assemblies != null) {
				StringCollection coll = options.ReferencedAssemblies;
				foreach (string str in assemblies)
					coll.Add (str);
			}

			return options;
		}

		public static CompilerResults Compile (string file, ArrayList assemblies)
		{
			CompilationCacheItem item = GetCached (file);
			if (item != null)
				return item.Result;
			
			CompilerResults results = null;
			lock (compilationLock) {
				item = GetCached (file);
				if (item != null)
					return item.Result;

				CompilerParameters options = GetOptions (assemblies);
				//TODO: support for other languages
				results = CSCompiler.Compiler.CompileAssemblyFromFile (options, file);
				cache [file] = new CompilationCacheItem (results, null);
			}

			return results;
		}
	}
}

