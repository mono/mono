//
// System.Web.Compilation.CachingCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	//TODO: caching should be done using System.Web.Caching, but that namespace still need some work.
	internal class CompilationCacheItem
	{
		CompilationResult result;
		DateTime reference;
		bool invalidated;

		public CompilationCacheItem (CompilationResult result)
		{
			this.result = result;
			try {
				this.reference = File.GetLastWriteTime (result.OutputFile);
			} catch (FileNotFoundException fnf){
				throw new FileNotFoundException (String.Format ("File: {0} at {1}", result.OutputFile, System.Environment.CurrentDirectory), fnf.FileName);
			}
		}

		public bool CheckDependencies ()
		{
			if (invalidated || result.Dependencies == null)
				return false;

			if (!File.Exists (result.OutputFile))
				return false;

			foreach (string s in result.Dependencies) {
				if (!File.Exists (s) || File.GetLastWriteTime (s) > reference) {
					invalidated = true;
					return false;
				}
			}
			
			return true;
		}

		public CompilationResult Result
		{
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

		public CompilationCacheItem this [string key]
		{
			get {
				return cache [key] as CompilationCacheItem;
			}

			set {
				cache [key] = value;
			}
		}
	}
	
	internal class CachingCompiler
	{
		static CompilationCache cache = CompilationCache.GetInstance ();
		string key;
		BaseCompiler compiler;

		public CachingCompiler (BaseCompiler compiler)
		{
			this.compiler = compiler;
			this.key = compiler.Key;
		}

		public static CompilationCacheItem GetCached (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			CompilationCacheItem item = cache [key];
			if (item != null && item.CheckDependencies ())
				return item;

			return null;
		}

		static object compilationLock = new object ();
		public bool Compile (CompilationResult result)
		{
			if (compiler.SourceFile == null)
				throw new ArgumentException ("No source to compile!");

			result.Reset ();
			CompilationCacheItem item;

			item = GetCached (key);
			if (item != null) {
				result.CopyFrom (item.Result);
				return true;
			}
			
			lock (compilationLock) {
				item = GetCached (key);
				if (item != null) {
					result.CopyFrom (item.Result);
					return true;
				}

				RealCompile (result);
				cache [key] = new CompilationCacheItem (result);
			}

			return (result.ExitCode == 0);
		}

		void RealCompile (CompilationResult result)
		{
			StringBuilder options = new StringBuilder ("/target:library ");
			if (compiler.CompilerOptions != null)
				options.Append (compiler.CompilerOptions + ' ');

			options.AppendFormat ("/out:\"{0}\" ", compiler.TargetFile);
			options.Append ('"' + compiler.SourceFile + '"');

			//Console.WriteLine ("mcs {0}", options);
			Process proc = new Process ();
			if (Path.DirectorySeparatorChar == '\\')
				proc.StartInfo.FileName = "mcs.bat";
			else
				proc.StartInfo.FileName = "mcs";

			proc.StartInfo.Arguments = options.ToString ();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;

			WebTrace.WriteLine ("{0} {1}", proc.StartInfo.FileName, options.ToString ());
			try {
				proc.Start ();
			} catch (Exception e) {
				if (Path.DirectorySeparatorChar == '\\') {
					proc.StartInfo.FileName = "mcs";
					proc.Start ();
				} else {
					throw e;
				}
			}

			string poutput = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit ();
			result.ExitCode = proc.ExitCode;
			proc.Close ();
			proc = null;

			result.CompilerOutput = poutput;
			//Console.WriteLine ("output: {0}\n", poutput);
			result.OutputFile = compiler.TargetFile;
		}
	}
}

