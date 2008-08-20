//
// CachingCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ankit Jain  (jankit@novell.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (c) Copyright Novell, Inc. (http://www.novell.com)
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
// Code taken from System.Web.Compilation.CachingCompiler
//

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.Caching;
using System.Web.Configuration;

namespace System.ServiceModel.Channels
{
	class CachingCompiler
	{
		static string dynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
		static Hashtable compilationTickets = new Hashtable ();
		public const string cachePrefix = "@@Assembly";
		public const string cacheTypePrefix = "@@@Type";

		public static void InsertTypeFileDep (Type type, string filename)
		{
			CacheDependency dep = new CacheDependency (filename);
			HttpRuntime.Cache.Insert (cacheTypePrefix + filename, type, dep);
		}

 		public static void InsertType (Type type, string filename, string key,
				CacheItemRemovedCallback removed_callback)
 		{
 			//string [] cacheKeys = new string [] { cachePrefix + filename };
			//CacheDependency dep = new CacheDependency (null, cacheKeys);
			CacheDependency dep = new CacheDependency (filename);

			HttpRuntime.Cache.Insert (cacheTypePrefix + key, type, dep,
				Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
				CacheItemPriority.Normal, removed_callback);
		}

		public static Type GetTypeFromCache (string filename)
		{
			return (Type) HttpRuntime.Cache [cacheTypePrefix + filename];
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

		public static CompilerResults Compile (string language, string key, string source,
							string filename, ArrayList assemblies)
		{
			Cache cache = HttpRuntime.Cache;
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
 
				CompilationSection config = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
				Compiler c = config.Compilers[language];
				Type t = Type.GetType (c.Type, true);
				CodeDomProvider provider = Activator.CreateInstance (t) as CodeDomProvider;

				if (provider == null)
					throw new HttpException ("Configuration error. Language not supported: " +
								  language, 500);

				CompilerParameters options = GetOptions (assemblies);
				TempFileCollection tempcoll = new TempFileCollection (config.TempDirectory, true);
				string dllfilename = Path.GetFileName (tempcoll.AddExtension ("dll", true));
				options.OutputAssembly = Path.Combine (dynamicBase, dllfilename);

				results = provider.CompileAssemblyFromSource (options, source);

				ArrayList realdeps = new ArrayList (assemblies.Count + 1);
				realdeps.Add (filename);
				for (int i = assemblies.Count - 1; i >= 0; i--) {
					string current = (string) assemblies [i];
					if (Path.IsPathRooted (current))
						realdeps.Add (current);
				}

				string [] deps = (string []) realdeps.ToArray (typeof (string));
				//cache results
				cache.Insert (cachePrefix + key, results, new CacheDependency (deps));
			} finally {
				Monitor.Exit (ticket);
				if (acquired)
					ReleaseCompilationTicket (cachePrefix + key);
			}

			return results;
		}

		public static Type CompileAndGetType (ServiceHostParser parser,
			string key, CacheItemRemovedCallback removed_callback)
		{
			CompilerResults result = CachingCompiler.Compile (parser.Language, key, parser.Program, parser.Filename, parser.Assemblies);
			if (result.NativeCompilerReturnValue != 0)
				throw new CompilationException (parser.Filename, result.Errors, parser.Program);

			Assembly assembly = result.CompiledAssembly;
			if (assembly == null)
				throw new CompilationException (parser.Filename, result.Errors, parser.Program);
		
			Type type = assembly.GetType (parser.TypeName, true);
			//cache type
			InsertType (type, parser.Filename, key, removed_callback);

			return type;
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

