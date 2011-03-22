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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.UI;
using System.Web.Caching;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	class CachingCompiler
	{
		static string dynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
		static Hashtable compilationTickets = new Hashtable ();
		const string cachePrefix = "@@Assembly";
		const string cacheTypePrefix = "@@@Type";
		static Hashtable assemblyCache = new Hashtable ();
		
		public static void InsertTypeFileDep (Type type, string filename)
		{
			CacheDependency dep = new CacheDependency (filename);
			HttpRuntime.InternalCache.Insert (cacheTypePrefix + filename, type, dep);
		}

 		public static void InsertType (Type type, string filename)
 		{
 			string [] cacheKeys = new string [] { cachePrefix + filename };
			CacheDependency dep = new CacheDependency (null, cacheKeys);
			HttpRuntime.InternalCache.Insert (cacheTypePrefix + filename, type, dep);
		}

		public static Type GetTypeFromCache (string filename)
		{
			return (Type) HttpRuntime.InternalCache [cacheTypePrefix + filename];
		}

		public static CompilerResults Compile (BaseCompiler compiler)
		{
			Cache cache = HttpRuntime.InternalCache;
			string key = cachePrefix + compiler.Parser.InputFile;
			CompilerResults results = (CompilerResults) cache [key];

			if (!compiler.IsRebuildingPartial)
				if (results != null)
					return results;

			object ticket;
			bool acquired = AcquireCompilationTicket (key, out ticket);

			try {
				Monitor.Enter (ticket);
				results = (CompilerResults) cache [key];
				if (!compiler.IsRebuildingPartial)
					if (results != null)
						return results;

				CodeDomProvider comp = compiler.Provider;
				CompilerParameters options = compiler.CompilerParameters;
				GetExtraAssemblies (options);
				results = comp.CompileAssemblyFromDom (options, compiler.CompileUnit);
				List <string> dependencies = compiler.Parser.Dependencies;
				if (dependencies != null && dependencies.Count > 0) {
					string [] deps = dependencies.ToArray ();
					HttpContext ctx = HttpContext.Current;
					HttpRequest req = ctx != null ? ctx.Request : null;

					if (req == null)
						throw new HttpException ("No current context, cannot compile.");
				
					for (int i = 0; i < deps.Length; i++)
						deps [i] = req.MapPath (deps [i]);
				
					cache.Insert (key, results, new CacheDependency (deps));
				}
			} finally {
				Monitor.Exit (ticket);
				if (acquired)
					ReleaseCompilationTicket (key);
			}

			return results;
		}

		public static CompilerResults Compile (WebServiceCompiler compiler)
		{
			string key = cachePrefix + compiler.Parser.PhysicalPath;
			Cache cache = HttpRuntime.InternalCache;
			CompilerResults results = (CompilerResults) cache [key];
			if (results != null)
				return results;

			object ticket;
			bool acquired = AcquireCompilationTicket (key, out ticket);

			try {
				Monitor.Enter (ticket);
				results = (CompilerResults) cache [key];
				if (results != null)
					return results;

				CodeDomProvider comp = compiler.Provider;
				CompilerParameters options = compiler.CompilerParameters;

				GetExtraAssemblies (options);
				
				results = comp.CompileAssemblyFromFile (options, compiler.InputFile);
				string [] deps = (string []) compiler.Parser.Dependencies.ToArray (typeof (string));
				cache.Insert (key, results, new CacheDependency (deps));
			} finally {
				Monitor.Exit (ticket);
				if (acquired)
					ReleaseCompilationTicket (key);
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
			GetExtraAssemblies (options);
			return options;
		}

		public static CompilerResults Compile (string language, string key, string file, ArrayList assemblies)
		{
			return Compile (language, key, file, assemblies, false);
		}
		
		public static CompilerResults Compile (string language, string key, string file, ArrayList assemblies, bool debug)
		{
			Cache cache = HttpRuntime.InternalCache;
			CompilerResults results = (CompilerResults) cache [cachePrefix + key];
			if (results != null)
				return results;

			if (!Directory.Exists (dynamicBase))
				Directory.CreateDirectory (dynamicBase);

			object ticket;
			bool acquired = AcquireCompilationTicket (cachePrefix + key, out ticket);

			try {
				Monitor.Enter (ticket);
				results = (CompilerResults) cache [cachePrefix + key];
				if (results != null)
					return results;

				CodeDomProvider provider = null;
				int warningLevel;
				string compilerOptions;
				string tempdir;

				provider = BaseCompiler.CreateProvider (language, out compilerOptions, out warningLevel, out tempdir);
				if (provider == null)
					throw new HttpException ("Configuration error. Language not supported: " +
								  language, 500);
				CodeDomProvider compiler = provider;
				CompilerParameters options = GetOptions (assemblies);
				options.IncludeDebugInformation = debug;
				options.WarningLevel = warningLevel;
				options.CompilerOptions = compilerOptions;
				
				TempFileCollection tempcoll = new TempFileCollection (tempdir, true);
				string dllfilename = Path.GetFileName (tempcoll.AddExtension ("dll", true));
				options.OutputAssembly = Path.Combine (dynamicBase, dllfilename);
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
			} finally {
				Monitor.Exit (ticket);
				if (acquired)
					ReleaseCompilationTicket (cachePrefix + key);
			}

			return results;
		}

		public static Type CompileAndGetType (string typename, string language, string key,
						string file, ArrayList assemblies)
		{
			CompilerResults result = CachingCompiler.Compile (language, key, file, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				using (StreamReader reader = new StreamReader (file)) {
					throw new CompilationException (file, result.Errors, reader.ReadToEnd ());
				}
			}

			Assembly assembly = result.CompiledAssembly;
			if (assembly == null) {
				using (StreamReader reader = new StreamReader (file)) {
					throw new CompilationException (file, result.Errors, reader.ReadToEnd ());
				}
			}
		
			Type type = assembly.GetType (typename, true);
			InsertType (type, file);
			return type;
		}

		static void GetExtraAssemblies (CompilerParameters options)
		{
			StringCollection refAsm = options.ReferencedAssemblies;
			string asmLocation;
			string asmName;
			ArrayList al = WebConfigurationManager.ExtraAssemblies;
			
			if (al != null && al.Count > 0) {
				foreach (object o in al) {
					asmName = o as string;
					if (asmName != null && !refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}

			Assembly asm;
			IList list = BuildManager.CodeAssemblies;
			if (list != null && list.Count > 0) {
				foreach (object o in list) {
					asm = o as Assembly;
					if (asm == null)
						continue;
					asmName = asm.Location;
					if (asmName != null && !refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}
			
			list = BuildManager.TopLevelAssemblies;
			if (list != null && list.Count > 0) {
				foreach (object o in list) {
					asm = o as Assembly;
					if (o == null)
						continue;
					asmName = asm.Location;
					if (!refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}

			CompilationSection cfg = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			AssemblyCollection asmcoll = cfg != null ? cfg.Assemblies : null;

			if (asmcoll == null)
				return;

			foreach (AssemblyInfo ai in asmcoll) {
				asmLocation = GetAssemblyLocationFromName (ai.Assembly);
				
				if (asmLocation == null || refAsm.Contains (asmLocation))
					continue;
				refAsm.Add (asmLocation);
			}
		}

		static string GetAssemblyLocationFromName (string name)
		{
			Assembly asm = assemblyCache [name] as Assembly;
			if (asm != null)
				return asm.Location;

			try {
				asm = Assembly.Load (name);
			} catch {
			}

			if (asm == null)
				return null;

			assemblyCache [name] = asm;
			return asm.Location;
		}
		
		static bool AcquireCompilationTicket (string key, out object ticket)
		{
			lock (compilationTickets.SyncRoot) {
				ticket = compilationTickets [key];
				if (ticket == null) {
					ticket = new object ();
					compilationTickets [key] = ticket;
					return true;
				}
			}
			return false;
		}

		static void ReleaseCompilationTicket (string key)
		{
			lock (compilationTickets.SyncRoot) {
				compilationTickets.Remove (key);
			}
		}
	}
}

