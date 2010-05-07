//
// System.Web.Compilation.BuildManager
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2006-2009 Novell, Inc (http://www.novell.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.Compilation
{
	public sealed class BuildManager
	{
		internal const string FAKE_VIRTUAL_PATH_PREFIX = "/@@MonoFakeVirtualPath@@";
		const string BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX = "@@Build_Manager@@";
		static int BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX_LENGTH = BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX.Length;

		static readonly object bigCompilationLock = new object ();
		static readonly object virtualPathsToIgnoreLock = new object ();
		static readonly char[] virtualPathsToIgnoreSplitChars = {','};
		
		static EventHandlerList events = new EventHandlerList ();
		static object buildManagerRemoveEntryEvent = new object ();
		
		static bool hosted;
		static Dictionary <string, bool> virtualPathsToIgnore;
		static bool virtualPathsToIgnoreChecked;
		static bool haveVirtualPathsToIgnore;
		static List <Assembly> AppCode_Assemblies = new List<Assembly>();
		static List <Assembly> TopLevel_Assemblies = new List<Assembly>();
		static Dictionary <Type, CodeDomProvider> codeDomProviders;
		static Dictionary <string, BuildManagerCacheItem> buildCache;
		static List <Assembly> referencedAssemblies;
		static List <Assembly> configReferencedAssemblies;
		static bool getReferencedAssembliesInvoked;
		
		static int buildCount;
		static bool is_precompiled;
		//static bool updatable; unused
		static Dictionary<string, PreCompilationData> precompiled;
		
		// This is here _only_ for the purpose of unit tests!
		internal static bool suppressDebugModeMessages;

#if SYSTEMCORE_DEP
		static ReaderWriterLockSlim buildCacheLock;
#else
		static ReaderWriterLock buildCacheLock;
#endif
		static ulong recursionDepth;

		internal static bool IsPrecompiled {
			get { return is_precompiled; }
		}
		
		internal static event BuildManagerRemoveEntryEventHandler RemoveEntry {
			add { events.AddHandler (buildManagerRemoveEntryEvent, value); }
			remove { events.RemoveHandler (buildManagerRemoveEntryEvent, value); }
		}
		
		internal static bool BatchMode {
			get {
				if (!hosted)
					return false; // Fix for bug #380985

				CompilationSection cs = CompilationConfig;
				if (cs == null)
					return true;
				
				return cs.Batch;
			}
		}

		// Assemblies built from the App_Code directory
		public static IList CodeAssemblies {
			get { return AppCode_Assemblies; }
		}
		
		internal static CompilationSection CompilationConfig {
			get { return WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection; }
		}

		internal static bool HaveResources {
			get; set;
		}
		
		internal static IList TopLevelAssemblies {
			get { return TopLevel_Assemblies; }
		}
		
		static BuildManager ()
		{
			hosted = (AppDomain.CurrentDomain.GetData (ApplicationHost.MonoHostedDataKey) as string) == "yes";
			buildCache = new Dictionary <string, BuildManagerCacheItem> (RuntimeHelpers.StringEqualityComparerCulture);
#if SYSTEMCORE_DEP
			buildCacheLock = new ReaderWriterLockSlim ();
#else
			buildCacheLock = new ReaderWriterLock ();
#endif
			referencedAssemblies = new List <Assembly> ();
			recursionDepth = 0;

			string appPath = HttpRuntime.AppDomainAppPath;
			string precomp_name = null;
			is_precompiled = String.IsNullOrEmpty (appPath) ? false : File.Exists ((precomp_name = Path.Combine (appPath, "PrecompiledApp.config")));
			if (is_precompiled)
				is_precompiled = LoadPrecompilationInfo (precomp_name);
		}

		// Deal with precompiled sites deployed in a different virtual path
		static void FixVirtualPaths ()
		{
			if (precompiled == null)
				return;
			
			string [] parts;
			int skip = -1;
			foreach (string vpath in precompiled.Keys) {
				parts = vpath.Split ('/');
				for (int i = 0; i < parts.Length; i++) {
					if (String.IsNullOrEmpty (parts [i]))
						continue;
					// The path must be rooted, otherwise PhysicalPath returned
					// below will be relative to the current request path and
					// File.Exists will return a false negative. See bug #546053
					string test_path = "/" + String.Join ("/", parts, i, parts.Length - i);
					VirtualPath result = GetAbsoluteVirtualPath (test_path);
					if (result != null && File.Exists (result.PhysicalPath)) {
						skip = i - 1;
						break;
					}
				}
			}
			
			string app_vpath = HttpRuntime.AppDomainAppVirtualPath;
			if (skip == -1 || (skip == 0 && app_vpath == "/"))
				return;

			if (!app_vpath.EndsWith ("/"))
				app_vpath = app_vpath + "/";
			Dictionary<string, PreCompilationData> copy = new Dictionary<string, PreCompilationData> (precompiled);
			precompiled.Clear ();
			foreach (KeyValuePair<string,PreCompilationData> entry in copy) {
				parts = entry.Key.Split ('/');
				string new_path;
				if (String.IsNullOrEmpty (parts [0]))
					new_path = app_vpath + String.Join ("/", parts, skip + 1, parts.Length - skip - 1);
				else
					new_path = app_vpath + String.Join ("/", parts, skip, parts.Length - skip);
				entry.Value.VirtualPath = new_path;
				precompiled.Add (new_path, entry.Value);
			}
		}

		static bool LoadPrecompilationInfo (string precomp_config)
		{
			using (XmlTextReader reader = new XmlTextReader (precomp_config)) {
				reader.MoveToContent ();
				if (reader.Name != "precompiledApp")
					return false;

				/* unused
				if (reader.HasAttributes)
					while (reader.MoveToNextAttribute ())
						if (reader.Name == "updatable") {
							updatable = (reader.Value == "true");
							break;
						}
				*/
			}

			string [] compiled = Directory.GetFiles (HttpRuntime.BinDirectory, "*.compiled");
			foreach (string str in compiled)
				LoadCompiled (str);

			FixVirtualPaths ();
			return true;
		}

		static void LoadCompiled (string filename)
		{
			using (XmlTextReader reader = new XmlTextReader (filename)) {
				reader.MoveToContent ();
				if (reader.Name == "preserve" && reader.HasAttributes) {
					reader.MoveToNextAttribute ();
					string val = reader.Value;
					// 1 -> app_code subfolder - add the assembly to CodeAssemblies
					// 2 -> ashx
					// 3 -> ascx, aspx
					// 6 -> app_code - add the assembly to CodeAssemblies
					// 8 -> global.asax
					// 9 -> App_GlobalResources - set the assembly for HttpContext
					if (reader.Name == "resultType" && (val == "2" || val == "3" || val == "8"))
						LoadPageData (reader, true);
					else if (val == "1" || val == "6") {
						PreCompilationData pd = LoadPageData (reader, false);
						CodeAssemblies.Add (Assembly.Load (pd.AssemblyFileName));
					} else if (val == "9") {
						PreCompilationData pd = LoadPageData (reader, false);
						HttpContext.AppGlobalResourcesAssembly = Assembly.Load (pd.AssemblyFileName);
					}
				}
			}
		}

		class PreCompilationData {
			public string VirtualPath;
			public string AssemblyFileName;
			public string TypeName;
			public Type Type;
		}

		static PreCompilationData LoadPageData (XmlTextReader reader, bool store)
		{
			PreCompilationData pc_data = new PreCompilationData ();

			while (reader.MoveToNextAttribute ()) {
				string name = reader.Name;
				if (name == "virtualPath")
					pc_data.VirtualPath = VirtualPathUtility.RemoveTrailingSlash (reader.Value);
				else if (name == "assembly")
					pc_data.AssemblyFileName = reader.Value;
				else if (name == "type")
					pc_data.TypeName = reader.Value;
			}
			if (store) {
				if (precompiled == null)
					precompiled = new Dictionary<string, PreCompilationData> (RuntimeHelpers.StringEqualityComparerCulture);
				precompiled.Add (pc_data.VirtualPath, pc_data);
			}
			return pc_data;
		}

		static void AddAssembly (Assembly asm, List <Assembly> al)
		{
			if (al.Contains (asm))
				return;

			al.Add (asm);
		}
		
		static void AddPathToIgnore (string vp)
		{
			if (virtualPathsToIgnore == null)
				virtualPathsToIgnore = new Dictionary <string, bool> (RuntimeHelpers.StringEqualityComparerCulture);
			
			VirtualPath path = GetAbsoluteVirtualPath (vp);
			string vpAbsolute = path.Absolute;
			if (!virtualPathsToIgnore.ContainsKey (vpAbsolute)) {
				virtualPathsToIgnore.Add (vpAbsolute, true);
				haveVirtualPathsToIgnore = true;
			}
			
			string vpRelative = path.AppRelative;
			if (!virtualPathsToIgnore.ContainsKey (vpRelative)) {
				virtualPathsToIgnore.Add (vpRelative, true);
				haveVirtualPathsToIgnore = true;
			}

			if (!virtualPathsToIgnore.ContainsKey (vp)) {
				virtualPathsToIgnore.Add (vp, true);
				haveVirtualPathsToIgnore = true;
			}
		}

		internal static void AddToReferencedAssemblies (Assembly asm)
		{
			// should not be used
		}
		
		static void AssertVirtualPathExists (VirtualPath virtualPath)
		{
			string realpath;
			bool dothrow = false;
			
			if (virtualPath.IsFake) {
				realpath = virtualPath.PhysicalPath;
				if (!File.Exists (realpath) && !Directory.Exists (realpath))
					dothrow = true;
			} else {
				VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
				string vpAbsolute = virtualPath.Absolute;
				
				if (!vpp.FileExists (vpAbsolute) && !vpp.DirectoryExists (vpAbsolute))
					dothrow = true;
			}

			if (dothrow)
				throw new HttpException (404, "The file '" + virtualPath + "' does not exist.", virtualPath.Absolute);
		}

		static void Build (VirtualPath vp)
		{
			AssertVirtualPathExists (vp);

			CompilationSection cs = CompilationConfig;
			lock (bigCompilationLock) {
				bool entryExists;
				if (HasCachedItemNoLock (vp.Absolute, out entryExists))
					return;

				if (recursionDepth == 0)
					referencedAssemblies.Clear ();

				recursionDepth++;
				try {
					BuildInner (vp, cs != null ? cs.Debug : false);
					if (entryExists && recursionDepth <= 1)
						// We count only update builds - first time a file
						// (or a batch) is built doesn't count.
						buildCount++;
				} finally {
					// See http://support.microsoft.com/kb/319947
					if (buildCount > cs.NumRecompilesBeforeAppRestart)
						HttpRuntime.UnloadAppDomain ();
					recursionDepth--;
				}
			}
		}

		// This method assumes it is being called with the big compilation lock held
		static void BuildInner (VirtualPath vp, bool debug)
		{
			var builder = new BuildManagerDirectoryBuilder (vp);
			bool recursive = recursionDepth > 1;
			List <BuildProviderGroup> builderGroups = builder.Build (IsSingleBuild (vp, recursive));
			if (builderGroups == null)
				return;

			string vpabsolute = vp.Absolute;
			int buildHash = (vpabsolute.GetHashCode () | (int)DateTime.Now.Ticks) + (int)recursionDepth;
			string assemblyBaseName;
			AssemblyBuilder abuilder;
			CompilerType ct;
			int attempts;
			bool singleBuild, needMainVpBuild;
			CompilationException compilationError;
			
			// Each group becomes a separate assembly.
			foreach (BuildProviderGroup group in builderGroups) {
				needMainVpBuild = false;
				compilationError = null;
				assemblyBaseName = null;
				
				if (group.Count == 1) {
					if (recursive || !group.Master)
						assemblyBaseName = String.Format ("{0}_{1}.{2:x}.", group.NamePrefix, VirtualPathUtility.GetFileName (group [0].VirtualPath), buildHash);
					singleBuild = true;
				} else
					singleBuild = false;
				
				if (assemblyBaseName == null)
					assemblyBaseName = group.NamePrefix + "_";
				
				ct = group.CompilerType;
				attempts = 3;
				while (attempts > 0) {
					abuilder = new AssemblyBuilder (vp, CreateDomProvider (ct), assemblyBaseName);
					abuilder.CompilerOptions = ct.CompilerParameters;
					abuilder.AddAssemblyReference (GetReferencedAssemblies () as List <Assembly>);
					try {
						GenerateAssembly (abuilder, group, vp, debug);
						attempts = 0;
					} catch (CompilationException ex) {
						attempts--;
						if (singleBuild)
							throw new HttpException ("Single file build failed.", ex);
						
						if (attempts == 0) {
							needMainVpBuild = true;
							compilationError = ex;
							break;
						}
						
						CompilerResults results = ex.Results;
						if (results == null)
							throw new HttpException ("No results returned from failed compilation.", ex);
						else
							RemoveFailedAssemblies (vpabsolute, ex, abuilder, group, results, debug);
					}
				}

				if (needMainVpBuild) {
					// One last attempt - try to build just the requested path
					// if it's not built yet or just return without throwing the
					// exception if it has already been built. 
					if (HasCachedItemNoLock (vpabsolute)) {
						if (debug)
							DescribeCompilationError ("Path '{0}' built successfully, but a compilation exception has been thrown for other files:",
										  compilationError, vpabsolute);
						return;
					};

					// This will trigger a recursive build of the requested vp,
					// which means only the vp alone will be built (or not); 
					Build (vp);
					if (HasCachedItemNoLock (vpabsolute)) {
						if (debug)
							DescribeCompilationError ("Path '{0}' built successfully, but a compilation exception has been thrown for other files:",
										  compilationError, vpabsolute);
						return;
					}

					// In theory this code is unreachable. If the recursive
					// build of the main vp failed, then it should have thrown
					// the build exception.
					throw new HttpException ("Requested virtual path build failed.", compilationError);
				}
			}
		}
		
		static CodeDomProvider CreateDomProvider (CompilerType ct)
		{
			if (codeDomProviders == null)
				codeDomProviders = new Dictionary <Type, CodeDomProvider> ();

			Type type = ct.CodeDomProviderType;
			if (type == null) {
				CompilationSection cs = CompilationConfig;
				CompilerType tmp = GetDefaultCompilerTypeForLanguage (cs.DefaultLanguage, cs);
				if (tmp != null)
					type = tmp.CodeDomProviderType;
			}

			if (type == null)
				return null;
			
			CodeDomProvider ret;
			if (codeDomProviders.TryGetValue (type, out ret))
				return ret;

			ret = Activator.CreateInstance (type) as CodeDomProvider;
			if (ret == null)
				return null;

			codeDomProviders.Add (type, ret);
			return ret;
		}
#if NET_4_0
		public static Type GetGlobalAsaxType ()
		{
			Type ret = HttpApplicationFactory.AppType;
			if (ret == null)
				throw new InvalidOperationException ("This method cannot be called during the application's pre-start initialization stage.");
			
			return ret;
		}
		
		public static Stream CreateCachedFile (string fileName)
		{
			if (fileName != null && (fileName == String.Empty || fileName.IndexOf (Path.DirectorySeparatorChar) != -1))
				throw new ArgumentException ("Value does not fall within the expected range.");

			string path = Path.Combine (HttpRuntime.CodegenDir, fileName);
			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		public static Stream ReadCachedFile (string fileName)
		{
			if (fileName != null && (fileName == String.Empty || fileName.IndexOf (Path.DirectorySeparatorChar) != -1))
				throw new ArgumentException ("Value does not fall within the expected range.");

			string path = Path.Combine (HttpRuntime.CodegenDir, fileName);
			if (!File.Exists (path))
				return null;
			
			return new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.None);
		}

		[MonoDocumentationNote ("Fully implemented but no info on application pre-init stage is available yet.")]
		public static void AddReferencedAssembly (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			Type ret = HttpApplicationFactory.AppType;
			if (ret != null)
				throw new InvalidOperationException ("This method cannot be called during the application's pre-start initialization stage.");

			if (configReferencedAssemblies == null)
				configReferencedAssemblies = new List <Assembly> ();

			if (!configReferencedAssemblies.Contains (assembly))
				configReferencedAssemblies.Add (assembly);
		}
#endif
		public static object CreateInstanceFromVirtualPath (string virtualPath, Type requiredBaseType)
		{
			return CreateInstanceFromVirtualPath (GetAbsoluteVirtualPath (virtualPath), requiredBaseType);
		}

		internal static object CreateInstanceFromVirtualPath (VirtualPath virtualPath, Type requiredBaseType)
		{
			if (requiredBaseType == null)
				throw new NullReferenceException (); // This is what MS does, but
								     // from somewhere else.
			
			Type type = GetCompiledType (virtualPath);
			if (type == null)
				return null;

			if (!requiredBaseType.IsAssignableFrom (type))
				throw new HttpException (500,
							 String.Format ("Type '{0}' does not inherit from '{1}'.", type.FullName, requiredBaseType.FullName));

			return Activator.CreateInstance (type, null);
		}
		
		static void DescribeCompilationError (string format, CompilationException ex, params object[] parms)
		{
			StringBuilder sb = new StringBuilder ();
			string newline = Environment.NewLine;
			
			if (parms != null)
				sb.AppendFormat (format + newline, parms);
			else
				sb.Append (format + newline);

			CompilerResults results = ex != null ? ex.Results : null;
			if (results == null)
				sb.Append ("No compiler error information present." + newline);
			else {
				sb.Append ("Compiler errors:" + newline);
				foreach (CompilerError error in results.Errors)
					sb.Append ("  " + error.ToString () + newline);
			}

			if (ex != null) {
				sb.Append (newline + "Exception thrown:" + newline);
				sb.Append (ex.ToString ());
			}

			ShowDebugModeMessage (sb.ToString ());
		}
		
		static BuildProvider FindBuildProviderForPhysicalPath (string path, BuildProviderGroup group, HttpRequest req)
		{
			if (req == null || String.IsNullOrEmpty (path))
				return null;

			foreach (BuildProvider bp in group) {
				if (String.Compare (path, req.MapPath (bp.VirtualPath), RuntimeHelpers.StringComparison) == 0)
					return bp;
			}
			
			return null;
		}
		
		static void GenerateAssembly (AssemblyBuilder abuilder, BuildProviderGroup group, VirtualPath vp, bool debug)
		{
			IDictionary <string, bool> deps;
			BuildManagerCacheItem bmci;
			string bvp, vpabsolute = vp.Absolute;
			StringBuilder sb;
			string newline;
			int failedCount = 0;
			
			if (debug) {
				newline = Environment.NewLine;
				sb = new StringBuilder ("Code generation for certain virtual paths in a batch failed. Those files have been removed from the batch." + newline);
				sb.Append ("Since you're running in debug mode, here's some more information about the error:" + newline);
			} else {
				newline = null;
				sb = null;
			}
			
			List <BuildProvider> failedBuildProviders = null;
			StringComparison stringComparison = RuntimeHelpers.StringComparison;
			foreach (BuildProvider bp in group) {
				bvp = bp.VirtualPath;
				if (HasCachedItemNoLock (bvp))
					continue;
				
				try {
					bp.GenerateCode (abuilder);
				} catch (Exception ex) {
					if (String.Compare (bvp, vpabsolute, stringComparison) == 0) {
						if (ex is CompilationException || ex is ParseException)
							throw;
						
						throw new HttpException ("Code generation failed.", ex);
					}
					
					if (failedBuildProviders == null)
						failedBuildProviders = new List <BuildProvider> ();
					failedBuildProviders.Add (bp);
					failedCount++;
					if (sb != null) {
						if (failedCount > 1)
							sb.Append (newline);
						
						sb.AppendFormat ("Failed file virtual path: {0}; Exception: {1}{2}{1}", bp.VirtualPath, newline, ex);
					}
					continue;
				}
				
				deps = bp.ExtractDependencies ();
				if (deps != null) {
					foreach (var dep in deps) {
						bmci = GetCachedItemNoLock (dep.Key);
						if (bmci == null || bmci.BuiltAssembly == null)
							continue;
						abuilder.AddAssemblyReference (bmci.BuiltAssembly);
					}
				}
			}

			if (sb != null && failedCount > 0)
				ShowDebugModeMessage (sb.ToString ());
			
			if (failedBuildProviders != null) {
				foreach (BuildProvider bp in failedBuildProviders)
					group.Remove (bp);
			}
			
			foreach (Assembly asm in referencedAssemblies) {
				if (asm == null)
					continue;
				
				abuilder.AddAssemblyReference (asm);
			}
			
			CompilerResults results  = abuilder.BuildAssembly (vp);
			
			// No results is not an error - it is possible that the assembly builder contained only .asmx and
			// .ashx files which had no body, just the directive. In such case, no code unit or code file is added
			// to the assembly builder and, in effect, no assembly is produced but there are STILL types that need
			// to be added to the cache.
			Assembly compiledAssembly = results != null ? results.CompiledAssembly : null;
			bool locked = false;
			try {
#if SYSTEMCORE_DEP
				buildCacheLock.EnterWriteLock ();
#else
				buildCacheLock.AcquireWriterLock (-1);
#endif
				locked = true;
				if (compiledAssembly != null)
					referencedAssemblies.Add (compiledAssembly);
				
				foreach (BuildProvider bp in group) {
					if (HasCachedItemNoLock (bp.VirtualPath))
						continue;
					
					StoreInCache (bp, compiledAssembly, results);
				}
			} finally {
				if (locked) {
#if SYSTEMCORE_DEP
					buildCacheLock.ExitWriteLock ();
#else
					buildCacheLock.ReleaseWriterLock ();
#endif
				}
			}
		}
		
		static VirtualPath GetAbsoluteVirtualPath (string virtualPath)
		{
			string vp;

			if (!VirtualPathUtility.IsRooted (virtualPath)) {
				HttpContext ctx = HttpContext.Current;
				HttpRequest req = ctx != null ? ctx.Request : null;
				
				if (req != null) {
					string fileDir = req.FilePath;
					if (!String.IsNullOrEmpty (fileDir) && String.Compare (fileDir, "/", StringComparison.Ordinal) != 0)
						fileDir = VirtualPathUtility.GetDirectory (fileDir);
					else
						fileDir = "/";

					vp = VirtualPathUtility.Combine (fileDir, virtualPath);
				} else
					throw new HttpException ("No context, cannot map paths.");
			} else
				vp = virtualPath;

			return new VirtualPath (vp);
		}

		[MonoTODO ("Not implemented, always returns null")]
		public static BuildDependencySet GetCachedBuildDependencySet (HttpContext context, string virtualPath)
		{
			return null; // null is ok here until we store the dependency set in the Cache.
		}
		
		static BuildManagerCacheItem GetCachedItem (string vp)
		{
			bool locked = false;
			
			try {
#if SYSTEMCORE_DEP
				buildCacheLock.EnterReadLock ();
#else
				buildCacheLock.AcquireReaderLock (-1);
#endif
				locked = true;
				return GetCachedItemNoLock (vp);
			} finally {
				if (locked) {
#if SYSTEMCORE_DEP
					buildCacheLock.ExitReadLock ();
#else
					buildCacheLock.ReleaseReaderLock ();
#endif
				}
			}
		}

		static BuildManagerCacheItem GetCachedItemNoLock (string vp)
		{
			BuildManagerCacheItem ret;
			if (buildCache.TryGetValue (vp, out ret))
				return ret;
			
			return null;
		}
		
		internal static Type GetCodeDomProviderType (BuildProvider provider)
		{
			CompilerType codeCompilerType;
			Type codeDomProviderType = null;

			codeCompilerType = provider.CodeCompilerType;
			if (codeCompilerType != null)
				codeDomProviderType = codeCompilerType.CodeDomProviderType;
				
			if (codeDomProviderType == null)
				throw new HttpException (String.Concat ("Provider '", provider, " 'fails to specify the compiler type."));

			return codeDomProviderType;
		}

		static Type GetPrecompiledType (string virtualPath)
		{
			PreCompilationData pc_data;
			if (precompiled != null && precompiled.TryGetValue (virtualPath, out pc_data)) {
				if (pc_data.Type == null) {
					pc_data.Type = Type.GetType (pc_data.TypeName + ", " + pc_data.AssemblyFileName, true);
				}
				return pc_data.Type;
			}
			return null;
		}

		internal static Type GetPrecompiledApplicationType ()
		{
			if (!is_precompiled)
				return null;

			Type apptype = GetPrecompiledType (VirtualPathUtility.Combine (HttpRuntime.AppDomainAppVirtualPath, "Global.asax"));
			if (apptype == null)
				apptype = GetPrecompiledType (VirtualPathUtility.Combine (HttpRuntime.AppDomainAppVirtualPath , "global.asax"));
			return apptype;
		}

		public static Assembly GetCompiledAssembly (string virtualPath)
		{
			return GetCompiledAssembly (GetAbsoluteVirtualPath (virtualPath));
		}

		internal static Assembly GetCompiledAssembly (VirtualPath virtualPath)
		{
			string vpabsolute = virtualPath.Absolute;
			if (is_precompiled) {
				Type type = GetPrecompiledType (vpabsolute);
				if (type != null)
					return type.Assembly;
			}
			BuildManagerCacheItem bmci = GetCachedItem (vpabsolute);
			if (bmci != null)
				return bmci.BuiltAssembly;

			Build (virtualPath);
			bmci = GetCachedItem (vpabsolute);
			if (bmci != null)
				return bmci.BuiltAssembly;
			
			return null;
		}
		
		public static Type GetCompiledType (string virtualPath)
		{
			return GetCompiledType (GetAbsoluteVirtualPath (virtualPath));
		}

		internal static Type GetCompiledType (VirtualPath virtualPath)
		{
			string vpabsolute = virtualPath.Absolute;
			if (is_precompiled) {
				Type type = GetPrecompiledType (vpabsolute);
				if (type != null)
					return type;
			}
			BuildManagerCacheItem bmci = GetCachedItem (vpabsolute);
			if (bmci != null) {
				ReferenceAssemblyInCompilation (bmci);
				return bmci.Type;
			}

			Build (virtualPath);
			bmci = GetCachedItem (vpabsolute);
			if (bmci != null) {
				ReferenceAssemblyInCompilation (bmci);
				return bmci.Type;
			}

			return null;
		}

		public static string GetCompiledCustomString (string virtualPath)
		{
			return GetCompiledCustomString (GetAbsoluteVirtualPath (virtualPath));
		}
	
		internal static string GetCompiledCustomString (VirtualPath virtualPath) 
		{
			string vpabsolute = virtualPath.Absolute;
			BuildManagerCacheItem bmci = GetCachedItem (vpabsolute);
			if (bmci != null)
				return bmci.CompiledCustomString;
			
			Build (virtualPath);
			bmci = GetCachedItem (vpabsolute);
			if (bmci != null)
				return bmci.CompiledCustomString;
			
			return null;
		}

		internal static CompilerType GetDefaultCompilerTypeForLanguage (string language, CompilationSection configSection)
		{
			return GetDefaultCompilerTypeForLanguage (language, configSection, true);
		}
		
		internal static CompilerType GetDefaultCompilerTypeForLanguage (string language, CompilationSection configSection, bool throwOnMissing)
		{
			// MS throws when accesing a Hashtable, we do here.
			if (language == null || language.Length == 0)
				throw new ArgumentNullException ("language");
				
			CompilationSection config;
			if (configSection == null)
				config = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			else
				config = configSection;
			
			Compiler compiler = config.Compilers.Get (language);
			CompilerParameters p;
			Type type;
			
			if (compiler != null) {
				type = HttpApplication.LoadType (compiler.Type, true);
				p = new CompilerParameters ();
				p.CompilerOptions = compiler.CompilerOptions;
				p.WarningLevel = compiler.WarningLevel;
				SetCommonParameters (config, p, type, language);
				return new CompilerType (type, p);
			}

			if (CodeDomProvider.IsDefinedLanguage (language)) {
				CompilerInfo info = CodeDomProvider.GetCompilerInfo (language);
				p = info.CreateDefaultCompilerParameters ();
				type = info.CodeDomProviderType;
				SetCommonParameters (config, p, type, language);
				return new CompilerType (type, p);
			}

			if (throwOnMissing)
				throw new HttpException (String.Concat ("No compiler for language '", language, "'."));

			return null;
		}

		public static ICollection GetReferencedAssemblies ()
		{
			if (getReferencedAssembliesInvoked)
				return configReferencedAssemblies;

			getReferencedAssembliesInvoked = true;
			if (configReferencedAssemblies == null)
				configReferencedAssemblies = new List <Assembly> ();
			
			CompilationSection compConfig = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
                        if (compConfig == null)
				return configReferencedAssemblies;
			
                        bool addAssembliesInBin = false;
                        foreach (AssemblyInfo info in compConfig.Assemblies) {
                                if (info.Assembly == "*")
                                        addAssembliesInBin = is_precompiled ? false : true;
                                else
                                        LoadAssembly (info, configReferencedAssemblies);
                        }

			foreach (Assembly topLevelAssembly in TopLevelAssemblies)
				configReferencedAssemblies.Add (topLevelAssembly);

			foreach (string assLocation in WebConfigurationManager.ExtraAssemblies)
				LoadAssembly (assLocation, configReferencedAssemblies);

			// Precompiled sites unconditionally load all assemblies from bin/ (fix for
			// bug #502016)
			if (is_precompiled || addAssembliesInBin) {
				foreach (string s in HttpApplication.BinDirectoryAssemblies) {
					try {
						LoadAssembly (s, configReferencedAssemblies);
					} catch (BadImageFormatException) {
						// ignore silently
					}
				}
			}
				
			return configReferencedAssemblies;
		}
		
		// The 2 GetType() overloads work on the global.asax, App_GlobalResources, App_WebReferences or App_Browsers
		public static Type GetType (string typeName, bool throwOnError)
		{
			return GetType (typeName, throwOnError, false);
		}

		public static Type GetType (string typeName, bool throwOnError, bool ignoreCase)
		{
			Type ret = null;
			try {
				foreach (Assembly asm in TopLevel_Assemblies) {
					ret = asm.GetType (typeName, throwOnError, ignoreCase);
					if (ret != null)
						break;
				}
			} catch (Exception ex) {
				throw new HttpException ("Failed to find the specified type.", ex);
			}
			return ret;
		}

		public static ICollection GetVirtualPathDependencies (string virtualPath)
		{
			return GetVirtualPathDependencies (virtualPath, null);
		}

		internal static ICollection GetVirtualPathDependencies (string virtualPath, BuildProvider bprovider)
		{
			BuildProvider provider = bprovider;
			if (provider == null) {
				CompilationSection cs = CompilationConfig;
				if (cs == null)
					return null;
				provider = BuildManagerDirectoryBuilder.GetBuildProvider (virtualPath, cs.BuildProviders);
			}
			
			if (provider == null)
				return null;
			
			IDictionary <string, bool> deps =  provider.ExtractDependencies ();
			if (deps == null)
				return null;

			return (ICollection)deps.Keys;
		}

		internal static bool HasCachedItemNoLock (string vp, out bool entryExists)
		{
			BuildManagerCacheItem item;
			
			if (buildCache.TryGetValue (vp, out item)) {
				entryExists = true;
				return item != null;
			}

			entryExists = false;
			return false;
		}
		
		internal static bool HasCachedItemNoLock (string vp)
		{
			bool dummy;
			return HasCachedItemNoLock (vp, out dummy);
		}
		
		internal static bool IgnoreVirtualPath (string virtualPath)
		{
			if (!virtualPathsToIgnoreChecked) {
				lock (virtualPathsToIgnoreLock) {
					if (!virtualPathsToIgnoreChecked)
						LoadVirtualPathsToIgnore ();
					virtualPathsToIgnoreChecked = true;
				}
			}
			
			if (!haveVirtualPathsToIgnore)
				return false;
			
			if (virtualPathsToIgnore.ContainsKey (virtualPath))
				return true;
			
			return false;
		}

		static bool IsSingleBuild (VirtualPath vp, bool recursive)
		{
			if (String.Compare (vp.AppRelative, "~/global.asax", StringComparison.OrdinalIgnoreCase) == 0)
				return true;

			if (!BatchMode)
				return true;
			
			return recursive;
		}
		
		static void LoadAssembly (string path, List <Assembly> al)
		{
			AddAssembly (Assembly.LoadFrom (path), al);
		}

		static void LoadAssembly (AssemblyInfo info, List <Assembly> al)
		{
			AddAssembly (Assembly.Load (info.Assembly), al);
		}
		
		static void LoadVirtualPathsToIgnore ()
		{
			NameValueCollection appSettings = WebConfigurationManager.AppSettings;
			if (appSettings == null)
				return;

			string pathsFromConfig = appSettings ["MonoAspnetBatchCompileIgnorePaths"];
			string pathsFromFile = appSettings ["MonoAspnetBatchCompileIgnoreFromFile"];

			if (!String.IsNullOrEmpty (pathsFromConfig)) {
				string[] paths = pathsFromConfig.Split (virtualPathsToIgnoreSplitChars);
				string path;
				
				foreach (string p in paths) {
					path = p.Trim ();
					if (path.Length == 0)
						continue;

					AddPathToIgnore (path);
				}
			}

			if (!String.IsNullOrEmpty (pathsFromFile)) {
				string realpath;
				HttpContext ctx = HttpContext.Current;
				HttpRequest req = ctx != null ? ctx.Request : null;

				if (req == null)
					throw new HttpException ("Missing context, cannot continue.");

				realpath = req.MapPath (pathsFromFile);
				if (!File.Exists (realpath))
					return;

				string[] paths = File.ReadAllLines (realpath);
				if (paths == null || paths.Length == 0)
					return;

				string path;
				foreach (string p in paths) {
					path = p.Trim ();
					if (path.Length == 0)
						continue;

					AddPathToIgnore (path);
				}
			}
		}

		static void OnEntryRemoved (string vp)
		{
			BuildManagerRemoveEntryEventHandler eh = events [buildManagerRemoveEntryEvent] as BuildManagerRemoveEntryEventHandler;

			if (eh != null)
				eh (new BuildManagerRemoveEntryEventArgs (vp, HttpContext.Current));
		}
		
		static void OnVirtualPathChanged (string key, object value, CacheItemRemovedReason removedReason)
		{
			string virtualPath;

			if (StrUtils.StartsWith (key, BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX))
				virtualPath = key.Substring (BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX_LENGTH);
			else
				return;
			
			bool locked = false;
			try {
#if SYSTEMCORE_DEP
				buildCacheLock.EnterWriteLock ();
#else
				buildCacheLock.AcquireWriterLock (-1);
#endif
				locked = true;

				if (HasCachedItemNoLock (virtualPath)) {
					buildCache [virtualPath] = null;
					OnEntryRemoved (virtualPath);
				}
			} finally {
				if (locked) {
#if SYSTEMCORE_DEP
					buildCacheLock.ExitWriteLock ();
#else
					buildCacheLock.ReleaseWriterLock ();
#endif
				}
			}
		}
		
		static void ReferenceAssemblyInCompilation (BuildManagerCacheItem bmci)
		{
			if (recursionDepth == 0 || referencedAssemblies.Contains (bmci.BuiltAssembly))
				return;

			referencedAssemblies.Add (bmci.BuiltAssembly);
		}
		
		static void RemoveFailedAssemblies (string requestedVirtualPath, CompilationException ex, AssemblyBuilder abuilder,
						    BuildProviderGroup group, CompilerResults results, bool debug)
		{
			StringBuilder sb;
			string newline;
			
			if (debug) {
				newline = Environment.NewLine;
				sb = new StringBuilder ("Compilation of certain files in a batch failed. Another attempt to compile the batch will be made." + newline);
				sb.Append ("Since you're running in debug mode, here's some more information about the error:" + newline);
			} else {
				newline = null;
				sb = null;
			}
			
			var failedBuildProviders = new List <BuildProvider> ();
			BuildProvider bp;
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			bool rethrow = false;
			
			foreach (CompilerError error in results.Errors) {
				if (error.IsWarning)
					continue;
				
				bp = abuilder.GetBuildProviderForPhysicalFilePath (error.FileName);
				if (bp == null) {
					bp = FindBuildProviderForPhysicalPath (error.FileName, group, req);
					if (bp == null)
						continue;
				}

				if (String.Compare (bp.VirtualPath, requestedVirtualPath, StringComparison.Ordinal) == 0)
					rethrow = true;

				if (!failedBuildProviders.Contains (bp)) {
					failedBuildProviders.Add (bp);
					if (sb != null)
						sb.AppendFormat ("\t{0}{1}", bp.VirtualPath, newline);
				}

				if (sb != null)
					sb.AppendFormat ("\t\t{0}{1}", error, newline);
			}

			foreach (BuildProvider fbp in failedBuildProviders)
				group.Remove (fbp);
			
			if (sb != null) {
				sb.AppendFormat ("{0}The following exception has been thrown for the file(s) listed above:{0}{1}",
						 newline, ex.ToString ());
				ShowDebugModeMessage (sb.ToString ());
				sb = null;
			}

			if (rethrow)
				throw new HttpException ("Compilation failed.", ex);
		}
		
		static void SetCommonParameters (CompilationSection config, CompilerParameters p, Type compilerType, string language)
		{
			p.IncludeDebugInformation = config.Debug;
			MonoSettingsSection mss = WebConfigurationManager.GetSection ("system.web/monoSettings") as MonoSettingsSection;
			if (mss == null || !mss.UseCompilersCompatibility)
				return;

			Compiler compiler = mss.CompilersCompatibility.Get (language);
			if (compiler == null)
				return;

			Type type = HttpApplication.LoadType (compiler.Type, false);
			if (type != compilerType)
				return;

			p.CompilerOptions = String.Concat (p.CompilerOptions, " ", compiler.CompilerOptions);
		}

		static void ShowDebugModeMessage (string msg)
		{
			if (suppressDebugModeMessages)
				return;
			
			Console.WriteLine ();
			Console.WriteLine ("******* DEBUG MODE MESSAGE *******");
			Console.WriteLine (msg);
			Console.WriteLine ("******* DEBUG MODE MESSAGE *******");
			Console.WriteLine ();
		}

		static void StoreInCache (BuildProvider bp, Assembly compiledAssembly, CompilerResults results)
		{
			string virtualPath = bp.VirtualPath;
			var item = new BuildManagerCacheItem (compiledAssembly, bp, results);
			
			if (buildCache.ContainsKey (virtualPath))
				buildCache [virtualPath] = item;
			else
				buildCache.Add (virtualPath, item);
			
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			CacheDependency dep;
			
			if (req != null) {
				IDictionary <string, bool> deps = bp.ExtractDependencies ();
				var files = new List <string> ();
				string physicalPath;

				physicalPath = req.MapPath (virtualPath);
				if (File.Exists (physicalPath))
					files.Add (physicalPath);
				
				if (deps != null && deps.Count > 0) {
					foreach (var d in deps) {
						physicalPath = req.MapPath (d.Key);
						if (!File.Exists (physicalPath))
							continue;
						if (!files.Contains (physicalPath))
							files.Add (physicalPath);
					}
				}

				dep = new CacheDependency (files.ToArray ());
			} else
				dep = null;

			HttpRuntime.InternalCache.Add (BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX + virtualPath,
						       true,
						       dep,
						       Cache.NoAbsoluteExpiration,
						       Cache.NoSlidingExpiration,
						       CacheItemPriority.High,
						       new CacheItemRemovedCallback (OnVirtualPathChanged));
		}
	}
}

