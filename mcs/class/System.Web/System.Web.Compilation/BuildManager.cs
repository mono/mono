//
// System.Web.Compilation.BuildManager
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2006-2008 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

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

namespace System.Web.Compilation {
	public sealed class BuildManager {
		sealed class BuildItem
		{
			public BuildProvider buildProvider;
			public AssemblyBuilder assemblyBuilder;
			public Type codeDomProviderType;
			public bool codeGenerated;
			public Assembly compiledAssembly;

			public Exception ProcessingException {
				get;
				private set;
			}
			
			public bool ProcessedFine {
				get;
				private set;
			}
			
			public CompilerParameters CompilerOptions {
				get {
					if (buildProvider == null)
						throw new HttpException ("No build provider.");

					try {
						return buildProvider.CodeCompilerType.CompilerParameters;
					} catch (Exception ex) {
						ProcessedFine = false;
						ProcessingException = ex;
						return null;
					}
				}
			}

			public string VirtualPath {
				get {
					if (buildProvider == null)
						throw new HttpException ("No build provider.");
					try {
						return buildProvider.VirtualPath;
					} catch (Exception ex) {
						ProcessedFine = false;
						ProcessingException = ex;
						return null;
					}
				}
			}

			public CodeCompileUnit CodeUnit {
				get {
					if (buildProvider == null)
						throw new HttpException ("No build provider.");
					try {
						return buildProvider.CodeUnit;
					} catch (Exception ex) {
						ProcessedFine = false;
						ProcessingException = ex;
						return null;
					}
				}
			}
			
			public BuildItem (BuildProvider provider)
			{
				this.buildProvider = provider;
				try {
					if (provider != null)
						codeDomProviderType = GetCodeDomProviderType (provider);
					ProcessedFine = true;
					ProcessingException = null;
				} catch (ParseException ex) {
					ProcessedFine = false;
					ProcessingException = ex;
				}
			}

			public void SetCompiledAssembly (AssemblyBuilder assemblyBuilder, Assembly compiledAssembly)
			{
				if (this.compiledAssembly != null || this.assemblyBuilder == null || this.assemblyBuilder != assemblyBuilder)
					return;

				this.compiledAssembly = compiledAssembly;
			}
			
			public CodeDomProvider CreateCodeDomProvider ()
			{
				if (codeDomProviderType == null)
					throw new HttpException ("Unable to create compilation provider, no provider type given.");
				
				CodeDomProvider ret;

				try {
					ret = Activator.CreateInstance (codeDomProviderType) as CodeDomProvider;
				} catch (Exception ex) {
					throw new HttpException ("Failed to create compilation provider.", ex);
				}

				if (ret == null)
					throw new HttpException ("Unable to instantiate code DOM provider '" + codeDomProviderType + "'.");

				return ret;
			}

			public void GenerateCode ()
			{
				if (buildProvider == null)
					throw new HttpException ("Cannot generate code - missing build provider.");

				try {
					buildProvider.GenerateCode ();
				} catch (Exception ex) {
					ProcessedFine = false;
					ProcessingException = ex;
				}
				
				codeGenerated = true;
			}

			public void StoreCodeUnit ()
			{
				if (buildProvider == null)
					throw new HttpException ("Cannot generate code - missing build provider.");
				if (assemblyBuilder == null)
					throw new HttpException ("Cannot generate code - missing assembly builder.");

				try {
					buildProvider.GenerateCode (assemblyBuilder);
				} catch (Exception ex) {
					ProcessedFine = false;
					ProcessingException = ex;
				}
			}

			public override string ToString ()
			{
				string ret = "BuildItem [";
				string virtualPath = VirtualPath;
				
				if (!String.IsNullOrEmpty (virtualPath))
					ret += virtualPath;

				ret += "]";

				return ret;
			}
		}

		sealed class BuildCacheItem
		{
			public string compiledCustomString;
			public Assembly assembly;
			public Type type;
			public string virtualPath;

			public BuildCacheItem ()
			{
			}
			
			public BuildCacheItem (Assembly assembly, BuildProvider bp, CompilerResults results)
			{
				this.assembly = assembly;
				this.compiledCustomString = bp.GetCustomString (results);
				this.type = bp.GetGeneratedType (results);
				this.virtualPath = bp.VirtualPath;
			}
			
			public override string ToString ()
			{
				StringBuilder sb = new StringBuilder ("BuildCacheItem [");
				bool first = true;
				
				if (!String.IsNullOrEmpty (compiledCustomString)) {
					sb.Append ("compiledCustomString: " + compiledCustomString);
					first = false;
				}
				
				if (assembly != null) {
					sb.Append ((first ? "" : "; ") + "assembly: " + assembly.ToString ());
					first = false;
				}

				if (type != null) {
					sb.Append ((first ? "" : "; ") + "type: " + type.ToString ());
					first = false;
				}

				if (!String.IsNullOrEmpty (virtualPath)) {
					sb.Append ((first ? "" : "; ") + "virtualPath: " + virtualPath);
					first = false;
				}

				sb.Append ("]");
				
				return sb.ToString ();
			}
		}
		
		enum BuildKind {
			Unknown,
			Pages,
			NonPages,
			Application,
			Theme,
			Fake
		};

		internal const string FAKE_VIRTUAL_PATH_PREFIX = "/@@MonoFakeVirtualPath@@";
		const string BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX = "Build_Manager";
		static int BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX_LENGTH = BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX.Length;
		static bool hosted;
		static IEqualityComparer <string> comparer;

		
		static object buildCacheLock = new object ();

		static Stack <BuildKind> recursiveBuilds = new Stack <BuildKind> ();
		
		//
		// Disabled - see comment at the end of BuildAssembly below
		//
		// static object buildCountLock = new object ();
		// static int buildCount = 0;
		static bool is_precompiled;
		static Dictionary<string, PreCompilationData> precompiled;
		
		static List<Assembly> AppCode_Assemblies = new List<Assembly>();
		static List<Assembly> TopLevel_Assemblies = new List<Assembly>();
		static bool haveResources;

		// The build cache which maps a virtual path to a build item with all the necessary
		// bits. 
		static Dictionary <string, BuildCacheItem> buildCache;

		// Maps the virtual path of a non-page build to the assembly that contains the
		// compiled type.
		static Dictionary <string, Assembly> nonPagesCache;

		// Keeps the cache dependencies of each item currently stored in the
		// HttpRuntime.InternalCache. It is used to build a hierarchy of dependencies (for
		// nested controls)
		static Dictionary <string, List <string>> dependencyCache;
		
		static List <Assembly> referencedAssemblies = new List <Assembly> ();
		
		static Dictionary <string, object> compilationTickets;

		static Assembly globalAsaxAssembly;
		
		static Dictionary <string, BuildKind> knownFileTypes = new Dictionary <string, BuildKind> (StringComparer.OrdinalIgnoreCase) {
			{".aspx", BuildKind.Pages},
			{".asax", BuildKind.Application},
			{".ashx", BuildKind.NonPages},
			{".asmx", BuildKind.NonPages},
			{".ascx", BuildKind.NonPages},
			{".master", BuildKind.NonPages}
		};

		static Dictionary <string, bool> virtualPathsToIgnore;
		static bool haveVirtualPathsToIgnore;

		static EventHandlerList events = new EventHandlerList ();
		static object buildManagerRemoveEntryEvent = new object ();
		
		internal static event BuildManagerRemoveEntryEventHandler RemoveEntry {
			add { events.AddHandler (buildManagerRemoveEntryEvent, value); }
			remove { events.RemoveHandler (buildManagerRemoveEntryEvent, value); }
		}
		
		static BuildManager ()
		{
			if (HttpRuntime.CaseInsensitive)
				comparer = StringComparer.CurrentCultureIgnoreCase;
			else
				comparer = StringComparer.CurrentCulture;

			buildCache = new Dictionary <string, BuildCacheItem> (comparer);
			nonPagesCache = new Dictionary <string, Assembly> (comparer);
			dependencyCache = new Dictionary <string, List <string>> (comparer);
			compilationTickets = new Dictionary <string, object> (comparer);
			hosted = (AppDomain.CurrentDomain.GetData (ApplicationHost.MonoHostedDataKey) as string) == "yes";

			is_precompiled = File.Exists (Path.Combine (HttpRuntime.AppDomainAppPath, "PrecompiledApp.config"));
			if (is_precompiled) {
				LoadPrecompilationInfo ();
			}
		}
		
		internal static void ThrowNoProviderException (string extension)
		{
			string msg = "No registered provider for extension '{0}'.";
			throw new HttpException (String.Format (msg, extension));
		}
		
		public static object CreateInstanceFromVirtualPath (string virtualPath, Type requiredBaseType)
		{
			// virtualPath + Exists done in GetCompiledType()
			if (requiredBaseType == null)
				throw new NullReferenceException (); // This is what MS does, but from somewhere else.

			// Get the Type.
			Type type = GetCompiledType (virtualPath);
			if (type == null)
				//throw new HttpException ("Instance creation failed for virtual
				//path '" + virtualPath + "'.");
				return null;
			
			if (!requiredBaseType.IsAssignableFrom (type)) {
				string msg = String.Format ("Type '{0}' does not inherit from '{1}'.",
								type.FullName, requiredBaseType.FullName);
				throw new HttpException (500, msg);
			}

			return Activator.CreateInstance (type, null);
		}

		public static ICollection GetReferencedAssemblies ()
		{
			List <Assembly> al = new List <Assembly> ();
			
			CompilationSection compConfig = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
                        if (compConfig == null)
				return al;
			
                        bool addAssembliesInBin = false;
                        foreach (AssemblyInfo info in compConfig.Assemblies) {
                                if (info.Assembly == "*")
                                        addAssembliesInBin = is_precompiled ? false : true;
                                else
                                        LoadAssembly (info, al);
                        }

			foreach (Assembly topLevelAssembly in TopLevel_Assemblies)
				al.Add (topLevelAssembly);

			foreach (string assLocation in WebConfigurationManager.ExtraAssemblies)
				LoadAssembly (assLocation, al);


			// Precompiled sites unconditionally load all assemblies from bin/ (fix for
			// bug #502016)
			if (is_precompiled || addAssembliesInBin) {
				foreach (string s in HttpApplication.BinDirectoryAssemblies) {
					try {
						LoadAssembly (s, al);
					} catch (BadImageFormatException) {
						// ignore silently
					}
				}
			}

			lock (buildCacheLock) {
				foreach (Assembly asm in referencedAssemblies)
					if (!al.Contains (asm))
						al.Add (asm);
				
				if (globalAsaxAssembly != null)
					al.Add (globalAsaxAssembly);
			}
			
			return al;
		}

		static void LoadAssembly (string path, List <Assembly> al)
		{
			AddAssembly (Assembly.LoadFrom (path), al);
		}

		static void LoadAssembly (AssemblyInfo info, List <Assembly> al)
		{
			AddAssembly (Assembly.Load (info.Assembly), al);
		}

		// Deal with precompiled sites deployed in a different virtual path
		static void FixVirtualPaths ()
 		{
			string [] parts;
			int skip = -1;
			foreach (string vpath in precompiled.Keys) {
				parts = vpath.Split ('/');
				for (int i = 0; i < parts.Length; i++) {
					string test_path = String.Join ("/", parts, i, parts.Length - i);
					VirtualPath result = GetAbsoluteVirtualPath (test_path);
					if (result != null && File.Exists (result.PhysicalPath)) {
						skip = i;
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

		static void LoadPrecompilationInfo ()
		{
			string [] compiled = Directory.GetFiles (HttpRuntime.BinDirectory, "*.compiled");
			foreach (string str in compiled) {
				LoadCompiled (str);
			}
			FixVirtualPaths ();
		}

		static void LoadCompiled (string filename)
		{
			using (XmlTextReader reader = new XmlTextReader (filename)) {
				reader.MoveToContent ();
				if (reader.Name == "preserve" && reader.HasAttributes) {
					reader.MoveToNextAttribute ();
					string val = reader.Value;
					// 2 -> ashx
					// 3 -> ascx, aspx
					// 6 -> app_code - nothing to do here
					// 8 -> global.asax
					// 9 -> App_GlobalResources - nothing to do?
					if (reader.Name == "resultType" && (val == "2" || val == "3" || val == "8"))
						LoadPageData (reader);
				}
			}
		}

		class PreCompilationData {
			public string VirtualPath;
			public string AssemblyFileName;
			public string TypeName;
			public Type Type;
		}

		static void LoadPageData (XmlTextReader reader)
		{
			PreCompilationData pc_data = new PreCompilationData ();

			while (reader.MoveToNextAttribute ()) {
				string name = reader.Name;
				if (name == "virtualPath")
					pc_data.VirtualPath = reader.Value;
				else if (name == "assembly")
					pc_data.AssemblyFileName = reader.Value;
				else if (name == "type")
					pc_data.TypeName = reader.Value;
			}
			if (precompiled == null)
				precompiled = new Dictionary<string, PreCompilationData> (comparer);
			precompiled.Add (pc_data.VirtualPath, pc_data);
		}

		static void AddAssembly (Assembly asm, List <Assembly> al)
		{
			if (al.Contains (asm))
				return;

			al.Add (asm);
		}
		
		[MonoTODO ("Not implemented, always returns null")]
		public static BuildDependencySet GetCachedBuildDependencySet (HttpContext context, string virtualPath)
		{
			return null; // null is ok here until we store the dependency set in the Cache.
		}

		internal static BuildProvider GetBuildProviderForPath (VirtualPath virtualPath, bool throwOnMissing)
		{
			return GetBuildProviderForPath (virtualPath, null, throwOnMissing);
		}
		
		internal static BuildProvider GetBuildProviderForPath (VirtualPath virtualPath, CompilationSection section, bool throwOnMissing)
		{
			string extension = virtualPath.Extension;
			CompilationSection c = section;

			if (c == null)
				c = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			
			if (c == null)
				if (throwOnMissing)
					ThrowNoProviderException (extension);
				else
					return null;
			
			BuildProviderCollection coll = c.BuildProviders;
			if (coll == null || coll.Count == 0)
				ThrowNoProviderException (extension);
			
			BuildProvider provider = coll.GetProviderForExtension (extension);
			if (provider == null)
				if (throwOnMissing)
					ThrowNoProviderException (extension);
				else
					return null;

			provider.SetVirtualPath (virtualPath);
			return provider;
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
		
		static BuildCacheItem GetCachedItem (VirtualPath virtualPath)
		{
			BuildCacheItem ret;
			
			lock (buildCacheLock) {
				if (buildCache.TryGetValue (virtualPath.Absolute, out ret))
					return ret;
			}

			return null;
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
			//Console.WriteLine ("VPath not precompiled: {0}", virtualPath);
			return null;
		}

		internal static Type GetPrecompiledApplicationType ()
		{
			if (!is_precompiled)
				return null;

			Type apptype = GetPrecompiledType (VirtualPathUtility.Combine (HttpRuntime.AppDomainAppVirtualPath, "Global.asax"));
			if (apptype == null)
				apptype = GetPrecompiledType (VirtualPathUtility.Combine (HttpRuntime.AppDomainAppVirtualPath, "global.asax"));
			return apptype;
		}

		public static Assembly GetCompiledAssembly (string virtualPath)
		{
			VirtualPath vp = GetAbsoluteVirtualPath (virtualPath);
			if (is_precompiled) {
				Type type = GetPrecompiledType (vp.Absolute);
				if (type != null)
					return type.Assembly;
			}
			BuildCacheItem ret = GetCachedItem (vp);
			if (ret != null)
				return ret.assembly;
			
			BuildAssembly (vp);
			ret = GetCachedItem (vp);
			if (ret != null)
				return ret.assembly;

			return null;
		}

		public static Type GetCompiledType (string virtualPath)
		{
			VirtualPath vp = GetAbsoluteVirtualPath (virtualPath);
			if (is_precompiled) {
				Type type = GetPrecompiledType (vp.Absolute);
				if (type != null)
					return type;
				//TODO: What do we do here? Throw?
			}
			BuildCacheItem ret = GetCachedItem (vp);
			
			if (ret != null)
				return ret.type;
				
			BuildAssembly (vp);
			ret = GetCachedItem (vp);
			if (ret != null)
				return ret.type;

			return null;
		}

		
		public static string GetCompiledCustomString (string virtualPath)
		{
			VirtualPath vp = GetAbsoluteVirtualPath (virtualPath);
			BuildCacheItem ret = GetCachedItem (vp);
			if (ret != null)
				return ret.compiledCustomString;

			BuildAssembly (vp);
			ret = GetCachedItem (vp);
			if (ret != null)
				return ret.compiledCustomString;

			return null;
		}

		static List <VirtualFile> GetFilesForBuild (VirtualPath virtualPath, out BuildKind kind)
		{
			string extension = virtualPath.Extension;
			var ret = new List <VirtualFile> ();
			
			if (virtualPath.IsFake) {
				kind = BuildKind.Fake;
				return ret;
			}
			
			if (!knownFileTypes.TryGetValue (extension, out kind)) {
				if (StrUtils.StartsWith (virtualPath.AppRelative, "~/App_Themes/"))
					kind = BuildKind.Theme;
				else
					kind = BuildKind.Unknown;
			}

			if (kind == BuildKind.Theme || kind == BuildKind.Application)
				return ret;
			
			bool doBatch = BatchMode;

			lock (buildCacheLock) {
				if (recursiveBuilds.Count > 0 && recursiveBuilds.Peek () == kind)
					doBatch = false;
				recursiveBuilds.Push (kind);
			}

			string vpAbsolute = virtualPath.Absolute;
			VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
			VirtualDirectory dir = vpp.GetDirectory (vpAbsolute);
			
			if (doBatch && HostingEnvironment.HaveCustomVPP && dir != null && dir is DefaultVirtualDirectory)
				doBatch = false;
			
			if (doBatch) {
				BuildKind fileKind;
				if (dir == null) {
					VirtualFile vf = vpp.GetFile (vpAbsolute);
					if (vf == null)
						throw new HttpException (404, "Virtual directory '" + virtualPath.Directory + "' does not exist. Virtual file '" + vpAbsolute + "' does not exist.");
					if (knownFileTypes.TryGetValue (VirtualPathUtility.GetExtension (vpAbsolute), out fileKind) && fileKind == fileKind)
						ret.Add (vf);
				} else {
					foreach (VirtualFile file in dir.Files) {
						if (!knownFileTypes.TryGetValue (VirtualPathUtility.GetExtension (file.Name), out fileKind))
							continue;

						if (kind == fileKind)
							ret.Add (file);
					}
				}
			} else {
				VirtualFile vf = vpp.GetFile (vpAbsolute);
				if (vf == null)
					throw new HttpException (404, "Virtual file '" + virtualPath + "' does not exist.");
				ret.Add (vf);
			}
			
			return ret;
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
		
		internal static CompilerType GetDefaultCompilerTypeForLanguage (string language, CompilationSection configSection)
		{
			// MS throws when accesing a Hashtable, we do here.
			if (language == null || language == "")
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

			throw new HttpException (String.Concat ("No compiler for language '", language, "'."));
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

		static List <BuildItem> LoadBuildProviders (VirtualPath virtualPath, string virtualDir, Dictionary <string, bool> vpCache,
							    out BuildKind kind, out string assemblyBaseName)
		{
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;

			if (req == null)
				throw new HttpException ("No context available, cannot build.");

			CompilationSection section = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			List <VirtualFile> files;
			
			try {
				files = GetFilesForBuild (virtualPath, out kind);
			} catch (Exception ex) {
				throw new HttpException ("Error loading build providers for path '" + virtualDir + "'.", ex);
			}

			List <BuildItem> ret = new List <BuildItem> ();
			BuildProvider provider = null;
			
			switch (kind) {
				case BuildKind.Theme:
					assemblyBaseName = "App_Theme_";
					provider = new ThemeDirectoryBuildProvider ();
					provider.SetVirtualPath (virtualPath);
					break;

				case BuildKind.Application:
					assemblyBaseName = "App_global.asax.";
					provider = new ApplicationFileBuildProvider ();
					provider.SetVirtualPath (virtualPath);
					break;

				case BuildKind.Fake:
					provider = GetBuildProviderForPath (virtualPath, section, false);
					assemblyBaseName = null;
					break;
					
				default:
					assemblyBaseName = null;
					break;
			}

			if (provider != null) {
				ret.Add (new BuildItem (provider));
				return ret;
			}
			
			string fileVirtualPath;
			lock (buildCacheLock) {
				foreach (VirtualFile f in files) {
					fileVirtualPath = f.VirtualPath;
					if (IgnoreVirtualPath (fileVirtualPath))
						continue;
					
					if (buildCache.ContainsKey (fileVirtualPath) || vpCache.ContainsKey (fileVirtualPath))
						continue;
					
					vpCache.Add (fileVirtualPath, true);
					provider = GetBuildProviderForPath (new VirtualPath (fileVirtualPath), section, false);
					if (provider == null)
						continue;

					ret.Add (new BuildItem (provider));
				}
			}

			return ret;
		}		

		static bool IgnoreVirtualPath (string virtualPath)
		{
			if (!haveVirtualPathsToIgnore)
				return false;
			
			if (virtualPathsToIgnore.ContainsKey (virtualPath))
				return true;
			
			return false;
		}

		static void AddPathToIgnore (string vp)
		{
			if (virtualPathsToIgnore == null)
				virtualPathsToIgnore = new Dictionary <string, bool> ();
			
			VirtualPath path = GetAbsoluteVirtualPath (vp);
			string vpAbsolute = path.Absolute;
			if (virtualPathsToIgnore.ContainsKey (vpAbsolute))
				return;

			virtualPathsToIgnore.Add (vpAbsolute, true);
			haveVirtualPathsToIgnore = true;
		}

		static char[] virtualPathsToIgnoreSplitChars = {','};
		static void LoadVirtualPathsToIgnore ()
		{
			if (virtualPathsToIgnore != null)
				return;
			
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
		
		static AssemblyBuilder CreateAssemblyBuilder (string assemblyBaseName, VirtualPath virtualPath, BuildItem buildItem)
		{
			buildItem.assemblyBuilder = new AssemblyBuilder (virtualPath, buildItem.CreateCodeDomProvider (), assemblyBaseName);
			buildItem.assemblyBuilder.CompilerOptions = buildItem.CompilerOptions;
			
			return buildItem.assemblyBuilder;
		}

		static Dictionary <string, CompileUnitPartialType> GetUnitPartialTypes (CodeCompileUnit unit)
		{
			Dictionary <string, CompileUnitPartialType> ret = null;

			CompileUnitPartialType pt;
			foreach (CodeNamespace ns in unit.Namespaces) {
				foreach (CodeTypeDeclaration type in ns.Types) {
					if (type.IsPartial) {
						pt = new CompileUnitPartialType (unit, ns, type);

						if (ret == null)
							ret = new Dictionary <string, CompileUnitPartialType> ();
						
						ret.Add (pt.TypeName, pt);
					}
				}
			}

			return ret;
		}

		static bool TypeHasConflictingMember (CodeTypeDeclaration type, CodeMemberMethod member)
		{
			if (type == null || member == null)
				return false;

			CodeMemberMethod method;
			string methodName = member.Name;
			int count;
			
			foreach (CodeTypeMember m in type.Members) {
				if (m.Name != methodName)
					continue;
				
				method = m as CodeMemberMethod;
				if (method == null)
					continue;
			
				if ((count = method.Parameters.Count) != member.Parameters.Count)
					continue;

				CodeParameterDeclarationExpressionCollection methodA = method.Parameters;
				CodeParameterDeclarationExpressionCollection methodB = member.Parameters;
			
				for (int i = 0; i < count; i++)
					if (methodA [i].Type != methodB [i].Type)
						continue;

				return true;
			}
			
			return false;
		}

		static bool TypeHasConflictingMember (CodeTypeDeclaration type, CodeMemberField member)
		{
			if (type == null || member == null)
				return false;

			CodeMemberField field = FindMemberByName (type, member.Name) as CodeMemberField;
			if (field == null)
				return false;

			if (field.Type == member.Type)
				return false; // This will get "flattened" by AssemblyBuilder
			
			return true;
		}
		
		static bool TypeHasConflictingMember (CodeTypeDeclaration type, CodeTypeMember member)
		{
			if (type == null || member == null)
				return false;

			return (FindMemberByName (type, member.Name) != null);
		}

		static CodeTypeMember FindMemberByName (CodeTypeDeclaration type, string name)
		{
			foreach (CodeTypeMember m in type.Members) {
				if (m == null || m.Name != name)
					continue;
				return m;
			}

			return null;
		}
		
		static bool PartialTypesConflict (CodeTypeDeclaration typeA, CodeTypeDeclaration typeB)
		{
			bool conflict;
			Type type;
			
			foreach (CodeTypeMember member in typeB.Members) {
				conflict = false;
				type = member.GetType ();
				if (type == typeof (CodeMemberMethod))
					conflict = TypeHasConflictingMember (typeA, (CodeMemberMethod) member);
				else if (type == typeof (CodeMemberField))
					conflict = TypeHasConflictingMember (typeA, (CodeMemberField) member);
				else
					conflict = TypeHasConflictingMember (typeA, member);
				
				if (conflict)
					return true;
			}
			
			return false;
		}
		
		static bool CanAcceptCode (AssemblyBuilder assemblyBuilder, BuildItem buildItem)
		{
			CodeCompileUnit newUnit = buildItem.CodeUnit;
			if (newUnit == null)
				return true;
			
			Dictionary <string, CompileUnitPartialType> unitPartialTypes = GetUnitPartialTypes (newUnit);

			if (unitPartialTypes == null)
				return true;

			if (assemblyBuilder.Units.Count > CompilationConfig.MaxBatchSize)
				return false;
			
			CompileUnitPartialType pt;			
			foreach (List <CompileUnitPartialType> partialTypes in assemblyBuilder.PartialTypes.Values)
				foreach (CompileUnitPartialType cupt in partialTypes)
					if (unitPartialTypes.TryGetValue (cupt.TypeName, out pt) && PartialTypesConflict (cupt.PartialType, pt.PartialType))
						return false;
			
			return true;
		}
		
		static void AssignToAssemblyBuilder (string assemblyBaseName, VirtualPath virtualPath, BuildItem buildItem,
						     Dictionary <Type, List <AssemblyBuilder>> assemblyBuilders, bool forceOwnAssembly)
		{
			if (!buildItem.codeGenerated) {
				buildItem.GenerateCode ();
				if (!buildItem.ProcessedFine)
					return;
			}
			
			List <AssemblyBuilder> builders;

			if (!assemblyBuilders.TryGetValue (buildItem.codeDomProviderType, out builders)) {
				builders = new List <AssemblyBuilder> ();
				assemblyBuilders.Add (buildItem.codeDomProviderType, builders);
			}

			if (!forceOwnAssembly) {
				// Put it in the first assembly builder that doesn't have conflicting
				// partial types
				foreach (AssemblyBuilder assemblyBuilder in builders) {
					if (CanAcceptCode (assemblyBuilder, buildItem)) {
						buildItem.assemblyBuilder = assemblyBuilder;
						buildItem.StoreCodeUnit ();
						return;
					}
				}
			}
			
			// None of the existing builders can accept this unit, get it a new builder
			AssemblyBuilder builder = CreateAssemblyBuilder (assemblyBaseName, virtualPath, buildItem);
			if (!forceOwnAssembly)
				builders.Add (builder);
			buildItem.StoreCodeUnit ();
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

		const int ticketLockTimeout = 20000;
		const int ticketLockAttempts = 3;
		static void BuildAssembly (VirtualPath virtualPath)
		{
			AssertVirtualPathExists (virtualPath);
			LoadVirtualPathsToIgnore ();
			
			object ticket;
			bool acquired;
			bool locked = false;
			string virtualDir = virtualPath.Directory;
			BuildKind buildKind = BuildKind.Unknown;
			bool kindPushed = false;
			string vpAbsolute = virtualPath.Absolute;
			
			acquired = AcquireCompilationTicket (virtualDir, out ticket);
			try {
				int attempts = ticketLockAttempts;
				while (attempts-- > 0) {
					if (Monitor.TryEnter (ticket, ticketLockTimeout)) {
						locked = true;
						break;
					}
				}
				if (!locked)
					throw new HttpException (500, "Failed to acquire compilation lock for virtual path '" + virtualPath + "'.");
				
				lock (buildCacheLock) {
					if (buildCache.ContainsKey (vpAbsolute))
						return;
				}
				
				string assemblyBaseName;
				Dictionary <string, bool> vpCache = new Dictionary <string, bool> ();
				List <BuildItem> buildItems = LoadBuildProviders (virtualPath, virtualDir, vpCache, out buildKind, out assemblyBaseName);
				kindPushed = true;

				if (buildItems.Count == 0)
					return;
				
				Dictionary <Type, List <AssemblyBuilder>> assemblyBuilders = new Dictionary <Type, List <AssemblyBuilder>> ();
				bool checkForRecursion = buildKind == BuildKind.NonPages || buildKind == BuildKind.Pages;
				string buildItemVp;
				BuildItem requestBuildItem = null;
				AssemblyBuilder originalRequestAssemblyBuilder = null;
				bool isRequestAssemblyBuilder = false;
				var skippedItemsAssemblies = new List <Assembly> ();
				
				foreach (BuildItem buildItem in buildItems) {
					buildItemVp = buildItem.VirtualPath;
					
					if (buildItemVp == vpAbsolute) {
						if (!buildItem.ProcessedFine)
							throw buildItem.ProcessingException;
						requestBuildItem = buildItem;
						isRequestAssemblyBuilder = true;
					} else if (!buildItem.ProcessedFine)
						continue;

					if (checkForRecursion) {
						// Expensive but, alas, necessary - the builder in
						// our list might've been put into a different
						// assembly in a recursive call.
						lock (buildCacheLock) {
							BuildCacheItem bci;
							
							if (buildCache.TryGetValue (buildItem.VirtualPath, out bci)) {
								Assembly asm = bci.assembly;
								if (asm != null && !skippedItemsAssemblies.Contains (asm))
									skippedItemsAssemblies.Add (asm);
								continue;
							}
						}
					}

					if (buildItem.assemblyBuilder == null)
						AssignToAssemblyBuilder (assemblyBaseName, virtualPath, buildItem, assemblyBuilders, false);
					
					if (buildItem.assemblyBuilder == null && buildItemVp == vpAbsolute) {
						Exception ex = buildItem.ProcessingException;
						if (ex is HttpException)
							throw buildItem.ProcessingException;
						else
							throw new HttpException ("Error processing file at virtual path '" + virtualPath.Original + "'", ex);
					}

					if (isRequestAssemblyBuilder) {
						isRequestAssemblyBuilder = false;
						originalRequestAssemblyBuilder = buildItem.assemblyBuilder;
					}
				}

				bool needToBuildRequestItemAlone = false;
				foreach (List <AssemblyBuilder> abuilders in assemblyBuilders.Values) {
					foreach (AssemblyBuilder abuilder in abuilders) {
						abuilder.AddAssemblyReference (skippedItemsAssemblies);
						
						try {
							GenerateAssembly (abuilder, buildItems, virtualPath, buildKind);
						} catch (Exception ex) {
							if (requestBuildItem == null || abuilder != originalRequestAssemblyBuilder) {
								if (ex.GetType () == typeof (HttpException))
									throw;
								
								throw new HttpException ("Compilation failed.", ex);
							}
							
							// There will be another assembly containing
							// just the requested virtual path, let's
							// give it a chance
							needToBuildRequestItemAlone = true;
						}

						if (needToBuildRequestItemAlone) {
							AssignToAssemblyBuilder (assemblyBaseName, virtualPath, requestBuildItem, assemblyBuilders, true);
							try {
								GenerateAssembly (requestBuildItem.assemblyBuilder, buildItems, virtualPath, buildKind);
							} catch (Exception ex) {
								if (ex.GetType () == typeof (HttpException))
									throw;
								
								throw new HttpException ("Compilation failed.", ex);
							}
							
							needToBuildRequestItemAlone = false;
						}
					}
				}

				// WARNING: enabling this code breaks the test suite - it stays
				// disabled until I figure out what to do about it.
				// See http://support.microsoft.com/kb/319947
// 				lock (buildCountLock) {
// 					buildCount++;
// 					if (buildCount > CompilationConfig.NumRecompilesBeforeAppRestart)
// 						HttpRuntime.UnloadAppDomain ();
// 				}
			} finally {
				if (kindPushed && buildKind == BuildKind.Pages || buildKind == BuildKind.NonPages) {
					lock (buildCacheLock) {
						recursiveBuilds.Pop ();
					}
				}

				if (locked) {
					Monitor.Exit (ticket);
					if (acquired)
						ReleaseCompilationTicket (virtualDir);
				}
			}
		}

		static void GenerateAssembly (AssemblyBuilder abuilder, List <BuildItem> buildItems, VirtualPath virtualPath, BuildKind buildKind)
		{
			CompilerResults results;
			Assembly compiledAssembly;
			string vp;
			BuildProvider bp;
			
			abuilder.AddAssemblyReference (GetReferencedAssemblies () as List <Assembly>);
			results = abuilder.BuildAssembly (virtualPath);
						
			// No results is not an error - it is possible that the assembly builder contained only .asmx and
			// .ashx files which had no body, just the directive. In such case, no code unit or code file is added
			// to the assembly builder and, in effect, no assembly is produced but there are STILL types that need
			// to be added to the cache.
			compiledAssembly = results != null ? results.CompiledAssembly : null;
						
			lock (buildCacheLock) {
				switch (buildKind) {
					case BuildKind.NonPages:
						if (compiledAssembly != null)
							AddToReferencedAssemblies (compiledAssembly);
						break;

					case BuildKind.Application:
						globalAsaxAssembly = compiledAssembly;
						break;
				}

				foreach (BuildItem buildItem in buildItems) {
					if (!buildItem.ProcessedFine || buildItem.assemblyBuilder != abuilder)
						continue;
					
					vp = buildItem.VirtualPath;
					bp = buildItem.buildProvider;
					buildItem.SetCompiledAssembly (abuilder, compiledAssembly);
								
					if (!buildCache.ContainsKey (vp)) {
						AddToCache (vp, bp);
						buildCache.Add (vp, new BuildCacheItem (compiledAssembly, bp, results));
					}

					if (compiledAssembly != null && !nonPagesCache.ContainsKey (vp))
						nonPagesCache.Add (vp, compiledAssembly);
				}
			}
		}
		
		internal static void AddToReferencedAssemblies (Assembly asm)
		{
			lock (buildCacheLock) {
				if (referencedAssemblies.Contains (asm))
					return;
				
				referencedAssemblies.Add (asm);
			}
		}
		
		internal static void AddToCache (string virtualPath, BuildProvider bp)
		{
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;

			if (req == null)
				throw new HttpException ("No current context.");
			
			CacheItemRemovedCallback cb = new CacheItemRemovedCallback (OnVirtualPathChanged);
			CacheDependency dep;
			ICollection col = bp.VirtualPathDependencies;
			int count;
			
			if (col != null && (count = col.Count) > 0) {
				List <string> files = new List <string> (), innerDeps;
				string file;
				
				foreach (object o in col) {
					file = o as string;
					if (String.IsNullOrEmpty (file))
						continue;
					
					files.Add (req.MapPath (file));
					if (dependencyCache.TryGetValue (file, out innerDeps)) {
						foreach (string f in innerDeps)
							if (!files.Contains (f))
								files.Add (f);
					}
				}

				dep = new CacheDependency (files.ToArray ());
				if (dependencyCache.ContainsKey (virtualPath))
					dependencyCache [virtualPath] = files;
				else
					dependencyCache.Add (virtualPath, files);
			} else
				dep = null;
			
			HttpRuntime.InternalCache.Add (BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX + virtualPath,
						       true,
						       dep,
						       Cache.NoAbsoluteExpiration,
						       Cache.NoSlidingExpiration,
						       CacheItemPriority.High,
						       cb);
						       
		}

		static void OnEntryRemoved (string vp)
		{
			BuildManagerRemoveEntryEventHandler eh = events [buildManagerRemoveEntryEvent] as BuildManagerRemoveEntryEventHandler;

			if (eh != null)
				eh (new BuildManagerRemoveEntryEventArgs (vp, HttpContext.Current));
		}
		
		static int RemoveVirtualPathFromCaches (VirtualPath virtualPath)
		{
			lock (buildCacheLock) {
				// This is expensive, but we must do it - we must not leave
				// the assembly in which the invalidated type lived. At the same
				// time, we must remove the virtual paths which were in that
				// assembly from the other caches, so that they get recompiled.
				BuildCacheItem item = GetCachedItem (virtualPath);
				if (item == null)
					return 0;

				string vpAbsolute = virtualPath.Absolute;
				if (buildCache.ContainsKey (vpAbsolute)) {
					buildCache.Remove (vpAbsolute);
					OnEntryRemoved (vpAbsolute);
				}
				
				if (dependencyCache.ContainsKey (vpAbsolute))
					dependencyCache.Remove (vpAbsolute);
				
				Assembly asm;
				
				if (nonPagesCache.TryGetValue (vpAbsolute, out asm)) {
					nonPagesCache.Remove (vpAbsolute);
					if (referencedAssemblies.Contains (asm))
						referencedAssemblies.Remove (asm);

					ArrayList extraAssemblies = WebConfigurationManager.ExtraAssemblies;
					if (extraAssemblies != null && extraAssemblies.Contains (asm.Location))
						extraAssemblies.Remove (asm.Location);
					
					List <string> keysToRemove = new List <string> ();
					foreach (KeyValuePair <string, Assembly> kvp in nonPagesCache)
						if (kvp.Value == asm)
							keysToRemove.Add (kvp.Key);
					
					foreach (string key in keysToRemove) {
						nonPagesCache.Remove (key);

						if (buildCache.ContainsKey (key)) {
							buildCache.Remove (key);
							OnEntryRemoved (key);
						}
						
						if (dependencyCache.ContainsKey (key))
							dependencyCache.Remove (key);
					}
				}

				return 1;
			}
		}
		
		static void OnVirtualPathChanged (string key, object value, CacheItemRemovedReason removedReason)
		{
			string virtualPath;

			if (StrUtils.StartsWith (key, BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX))
				virtualPath = key.Substring (BUILD_MANAGER_VIRTUAL_PATH_CACHE_PREFIX_LENGTH);
			else
				return;

			RemoveVirtualPathFromCaches (new VirtualPath (virtualPath));
		}
		
		static bool AcquireCompilationTicket (string key, out object ticket)
                {
                        lock (((ICollection)compilationTickets).SyncRoot) {
                                if (!compilationTickets.TryGetValue (key, out ticket)) {
                                        ticket = new Mutex ();
                                        compilationTickets.Add (key, ticket);
                                        return true;
                                }
                        }
			
                        return false;
                }

                static void ReleaseCompilationTicket (string key)
                {
                        lock (((ICollection)compilationTickets).SyncRoot) {
				if (compilationTickets.ContainsKey (key))
					compilationTickets.Remove (key);
                        }
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

		internal static ICollection GetVirtualPathDependencies (string virtualPath, BuildProvider bprovider)
		{
			BuildProvider provider = bprovider;
			if (provider == null)
				provider = GetBuildProviderForPath (new VirtualPath (virtualPath), false);
			if (provider == null)
				return null;
			return provider.VirtualPathDependencies;
		}
		
		public static ICollection GetVirtualPathDependencies (string virtualPath)
		{
			return GetVirtualPathDependencies (virtualPath, null);
		}
		
		// Assemblies built from the App_Code directory
		public static IList CodeAssemblies {
			get { return AppCode_Assemblies; }
		}

		internal static IList TopLevelAssemblies {
			get { return TopLevel_Assemblies; }
		}

		internal static bool HaveResources {
			get { return haveResources; }
			set { haveResources = value; }
		}
		
		internal static bool BatchMode {
			get {
				if (!hosted)
					return false; // Fix for bug #380985
				
				return CompilationConfig.Batch;
			}
		}

		internal static CompilationSection CompilationConfig {
			get { return WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection; }
		}
			
	}
}

#endif

