//
// System.Web.Compilation.AppResourceFilesCollection
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2006 Marek Habersack
// (C) 2007-2009 Novell, Inc http://novell.com/
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
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation 
{
	class AppResourcesCompiler
	{
		// This class fixes bug #466059
		class TypeResolutionService : ITypeResolutionService
		{
			List <Assembly> referencedAssemblies;
			Dictionary <string, Type> mappedTypes;

			public Assembly GetAssembly (AssemblyName name)
			{
				return GetAssembly (name, false);
			}
			
			public Assembly GetAssembly (AssemblyName name, bool throwOnError)
			{
				try {
					return Assembly.Load (name);
				} catch {
					if (throwOnError)
						throw;
				}

				return null;
			}

			public void ReferenceAssembly (AssemblyName name)
			{
				if (referencedAssemblies == null)
					referencedAssemblies = new List <Assembly> ();

				Assembly asm = GetAssembly (name, false);
				if (asm == null)
					return;
				
				if (referencedAssemblies.Contains (asm))
					return;
				
				referencedAssemblies.Add (asm);
			}

			public string GetPathOfAssembly (AssemblyName name)
			{
				if (name == null)
					return null;

				Assembly asm = GetAssembly (name, false);
				if (asm == null)
					return null;
				
				return asm.Location;
			}

			public Type GetType (string name)
			{
				return GetType (name, false, false);
			}

			public Type GetType (string name, bool throwOnError)
			{
				return GetType (name, throwOnError, false);
			}

			public Type GetType (string name, bool throwOnError, bool ignoreCase)
			{
				if (String.IsNullOrEmpty (name)) {
					if (throwOnError)
						throw new ArgumentNullException ("name");
					else
						return null;
				}

				int idx = name.IndexOf (',');
				Type type = null;
				if (idx == -1) {
					type = MapType (name, false);
					if (type != null)
						return type;
					
					type = FindInAssemblies (name, ignoreCase);
					if (type == null) {
						if (throwOnError)
							throw new InvalidOperationException ("Type '" + name + "' is not fully qualified and there are no referenced assemblies.");
						else
							return null;
					}

					return type;
				}

				type = MapType (name, true);
				if (type != null)
					return type;
				
				return Type.GetType (name, throwOnError, ignoreCase);
			}

			Type MapType (string name, bool full)
			{
				if (mappedTypes == null)
					mappedTypes = new Dictionary <string, Type> (StringComparer.Ordinal);

				Type ret;
				if (mappedTypes.TryGetValue (name, out ret))
					return ret;

				if (!full) {
					if (String.Compare (name, "ResXDataNode", StringComparison.Ordinal) == 0)
						return AddMappedType (name, typeof (ResXDataNode));
					if (String.Compare (name, "ResXFileRef", StringComparison.Ordinal) == 0)
						return AddMappedType (name, typeof (ResXFileRef));
					if (String.Compare (name, "ResXNullRef", StringComparison.Ordinal) == 0)
						return AddMappedType (name, typeof (ResXNullRef));
					if (String.Compare (name, "ResXResourceReader", StringComparison.Ordinal) == 0)
						return AddMappedType (name, typeof (ResXResourceReader));
					if (String.Compare (name, "ResXResourceWriter", StringComparison.Ordinal) == 0)
						return AddMappedType (name, typeof (ResXResourceWriter));

					return null;
				}

				if (name.IndexOf ("System.Windows.Forms") == -1)
					return null;

				if (name.IndexOf ("ResXDataNode", StringComparison.Ordinal) != -1)
					return AddMappedType (name, typeof (ResXDataNode));
				if (name.IndexOf ("ResXFileRef", StringComparison.Ordinal) != -1)
					return AddMappedType (name, typeof (ResXFileRef));
				if (name.IndexOf ("ResXNullRef", StringComparison.Ordinal) != -1)
					return AddMappedType (name, typeof (ResXNullRef));
				if (name.IndexOf ("ResXResourceReader", StringComparison.Ordinal) != -1)
					return AddMappedType (name, typeof (ResXResourceReader));
				if (name.IndexOf ("ResXResourceWriter", StringComparison.Ordinal) != -1)
					return AddMappedType (name, typeof (ResXResourceWriter));

				return null;
			}

			Type AddMappedType (string name, Type type)
			{
				mappedTypes.Add (name, type);
				return type;
			}
			
			Type FindInAssemblies (string name, bool ignoreCase)
			{
				Type ret = Type.GetType (name, false);
				if (ret != null)
					return ret;

				if (referencedAssemblies == null || referencedAssemblies.Count == 0)
					return null;

				foreach (Assembly asm in referencedAssemblies) {
					ret = asm.GetType (name, false, ignoreCase);
					if (ret != null)
						return ret;
				}

				return null;
			}
		}
		
		const string cachePrefix = "@@LocalResourcesAssemblies";
		
		bool isGlobal;
		AppResourceFilesCollection files;
		string tempDirectory;
		string virtualPath;
		Dictionary <string, List <string>> cultureFiles;
		List <string> defaultCultureFiles;
		
		string TempDirectory {
			get {
				if (tempDirectory != null)
					return tempDirectory;
				return (tempDirectory = AppDomain.CurrentDomain.SetupInformation.DynamicBase);
			}
		}

		public Dictionary <string, List <string>> CultureFiles {
			get { return cultureFiles; }
		}

		public List <string> DefaultCultureFiles {
			get { return defaultCultureFiles; }
		}
		
		static AppResourcesCompiler ()
		{
			if (!BuildManager.IsPrecompiled)
				return;

			string[] binDirAssemblies = HttpApplication.BinDirectoryAssemblies;
			if (binDirAssemblies == null || binDirAssemblies.Length == 0)
				return;

			string name;
			Assembly asm;
			foreach (string asmPath in binDirAssemblies) {
				if (String.IsNullOrEmpty (asmPath))
					continue;
				
				name = Path.GetFileName (asmPath);
				if (name.StartsWith ("App_LocalResources.", StringComparison.OrdinalIgnoreCase)) {
					string virtualPath = GetPrecompiledVirtualPath (asmPath);
					if (String.IsNullOrEmpty (virtualPath))
						continue;

					asm = LoadAssembly (asmPath);
					if (asm == null)
						continue;
					
					AddAssemblyToCache (virtualPath, asm);
					continue;
				}

				if (String.Compare (name, "App_GlobalResources.dll", StringComparison.OrdinalIgnoreCase) != 0)
					continue;

				asm = LoadAssembly (asmPath);
				if (asm == null)
					continue;

				HttpContext.AppGlobalResourcesAssembly = asm;
			}
		}
		
		public AppResourcesCompiler (HttpContext context)
		{
			this.isGlobal = true;
			this.files = new AppResourceFilesCollection (context);
			this.cultureFiles = new Dictionary <string, List <string>> (StringComparer.OrdinalIgnoreCase);
		}

		public AppResourcesCompiler (string virtualPath)
		{

			this.virtualPath = virtualPath;
			this.isGlobal = false;
			this.files = new AppResourceFilesCollection (HttpContext.Current.Request.MapPath (virtualPath));
			this.cultureFiles = new Dictionary <string, List <string>> (StringComparer.OrdinalIgnoreCase);
		}

		static Assembly LoadAssembly (string asmPath)
		{
			try {
				return Assembly.LoadFrom (asmPath);
			} catch (BadImageFormatException) {
				// ignore
				return null;
			}
		}
		
		static string GetPrecompiledVirtualPath (string asmPath)
		{
			string compiledFile = Path.ChangeExtension (asmPath, ".compiled");
			
			if (!File.Exists (compiledFile))
				return null;

			var pfile = new PreservationFile (compiledFile);
			string virtualPath = pfile.VirtualPath;
			if (String.IsNullOrEmpty (virtualPath))
				return "/";

			if (virtualPath.EndsWith ("/App_LocalResources/", StringComparison.OrdinalIgnoreCase))
				virtualPath = virtualPath.Substring (0, virtualPath.Length - 19);
			
			return virtualPath;
		}
		
		public Assembly Compile ()
		{
			files.Collect ();
			if (!files.HasFiles)
				return null;
			if (isGlobal)
				return CompileGlobal ();
			else
				return CompileLocal ();
		}

		Assembly CompileGlobal ()
		{
			string assemblyPath = FileUtils.CreateTemporaryFile (TempDirectory,
									     "App_GlobalResources",
									     "dll",
									     OnCreateRandomFile) as string;

			if (assemblyPath == null)
				throw new ApplicationException ("Failed to create global resources assembly");
			
			List <string>[] fileGroups = GroupGlobalFiles ();
			if (fileGroups == null || fileGroups.Length == 0)
				return null;
			
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace ns = new CodeNamespace (null);
			ns.Imports.Add (new CodeNamespaceImport ("System"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Globalization"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Reflection"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Resources"));
			unit.Namespaces.Add (ns);

			AppResourcesAssemblyBuilder builder = new AppResourcesAssemblyBuilder ("App_GlobalResources", assemblyPath,
											       this);
			CodeDomProvider provider = builder.Provider;
			
			Dictionary <string,bool> assemblies = new Dictionary<string,bool> ();
			foreach (List<string> ls in fileGroups)
				DomFromResource (ls [0], unit, assemblies, provider);
			
			foreach (KeyValuePair<string,bool> de in assemblies)
				unit.ReferencedAssemblies.Add (de.Key);
			
			builder.Build (unit);
			HttpContext.AppGlobalResourcesAssembly = builder.MainAssembly;
			
			return builder.MainAssembly;
		}

		Assembly CompileLocal ()
		{
			if (String.IsNullOrEmpty (virtualPath))
				return null;
			
			Assembly cached = GetCachedLocalResourcesAssembly (virtualPath);
			if (cached != null)
				return cached;
			
			string prefix;
			if (virtualPath == "/")
				prefix = "App_LocalResources.root";
			else
				prefix = "App_LocalResources" + virtualPath.Replace ('/', '.');
			
			string assemblyPath = FileUtils.CreateTemporaryFile (TempDirectory,
									     prefix,
									     "dll",
									     OnCreateRandomFile) as string;
			if (assemblyPath == null)
				throw new ApplicationException ("Failed to create local resources assembly");

			List<AppResourceFileInfo> files = this.files.Files;
			foreach (AppResourceFileInfo arfi in files)
				GetResourceFile (arfi, true);

			AppResourcesAssemblyBuilder builder = new AppResourcesAssemblyBuilder ("App_LocalResources", assemblyPath,
											       this);
			builder.Build ();
			Assembly ret = builder.MainAssembly;
			
			if (ret != null)
				AddAssemblyToCache (virtualPath, ret);

			return ret;
		}
		
		internal static Assembly GetCachedLocalResourcesAssembly (string path)
		{
			Dictionary <string, Assembly> cache;

			cache = HttpRuntime.InternalCache[cachePrefix] as Dictionary <string, Assembly>;
			if (cache == null || !cache.ContainsKey (path))
				return null;
			return cache [path];
		}
		
		static void AddAssemblyToCache (string path, Assembly asm)
		{
			Cache runtimeCache = HttpRuntime.InternalCache;
			Dictionary <string, Assembly> cache;
			
			cache = runtimeCache[cachePrefix] as Dictionary <string, Assembly>;
			if (cache == null)
				cache = new Dictionary <string, Assembly> ();
			cache [path] = asm;
			runtimeCache.Insert (cachePrefix, cache);
		}
		
		uint CountChars (char c, string s)
		{
			uint ret = 0;
			foreach (char ch in s) {
				if (ch == c)
					ret++;
			}
			return ret;
		}

		string IsFileCultureValid (string fileName)
                {
                    string tmp = Path.GetFileNameWithoutExtension (fileName);
                    tmp = Path.GetExtension (tmp);
                    if (tmp != null && tmp.Length > 0) {
                              tmp = tmp.Substring (1);
                            try {
                                CultureInfo.GetCultureInfo (tmp);
                                return tmp;
                            } catch {
                                return null;
                            }
                    } 
                    return null;
                }
		
		string GetResourceFile (AppResourceFileInfo arfi, bool local)
		{
			string resfile;
			if (arfi.Kind == AppResourceFileKind.ResX)
				resfile = CompileResource (arfi, local);
			else
				resfile = arfi.Info.FullName;
			if (!String.IsNullOrEmpty (resfile)) {
				string culture = IsFileCultureValid (resfile);
				List <string> cfiles;
				if (culture != null) {
					if (cultureFiles.ContainsKey (culture))
						cfiles = cultureFiles [culture];
					else {
						cfiles = new List <string> (1);
						cultureFiles [culture] = cfiles;
					}
				} else {
					if (defaultCultureFiles == null)
						defaultCultureFiles = new List <string> ();
					cfiles = defaultCultureFiles;
				}
				
				cfiles.Add (resfile);
			}
				
			return resfile;
		}
		
		List <string>[] GroupGlobalFiles ()
		{
			List<AppResourceFileInfo> files = this.files.Files;
			List<List<string>> groups = new List<List<string>> ();
			AppResourcesLengthComparer<List<string>> lcList = new AppResourcesLengthComparer<List<string>> ();
			
			string tmp, s, basename;
			uint basedots, filedots;
			AppResourceFileInfo defaultFile;
			
			foreach (AppResourceFileInfo arfi in files) {
				if (arfi.Kind != AppResourceFileKind.ResX && arfi.Kind != AppResourceFileKind.Resource)
					continue;

				s = arfi.Info.FullName;
				basename = Path.GetFileNameWithoutExtension (s);
				basedots = CountChars ('.', basename);
				defaultFile = null;
				
				// If there are any files that start with this baseName, we have a default file
				foreach (AppResourceFileInfo fi in files) {
					if (fi.Seen)
						continue;
					
					string s2 = fi.Info.FullName;
					if (s2 == null || s == s2)
						continue;
					tmp = Path.GetFileNameWithoutExtension (s2);
					filedots = CountChars ('.', tmp);

					if (filedots == basedots + 1 && tmp.StartsWith (basename)) {
						if (IsFileCultureValid (s2) != null) {
							// A valid translated file for this name
							defaultFile = arfi;
							break;
						} else {
							// This file shares the base name, but the culture is invalid - we must
							// ignore it since the name of the generated strongly typed class for this
							// resource will clash with the one generated from the default file with
							// the given basename.
							fi.Seen = true;
						}
					}
				}
				if (defaultFile != null) {
					List<string> al = new List<string> ();
					al.Add (GetResourceFile (arfi, false));
					arfi.Seen = true;
					groups.Add (al);
					
				}
			}
			groups.Sort (lcList);

			string tmp2;
			// Now find their translated counterparts
			foreach (List<string> al in groups) {
				s = al [0];
				tmp = Path.GetFileNameWithoutExtension (s);
				if (tmp.StartsWith ("Resources."))
					tmp = tmp.Substring (10);
				foreach (AppResourceFileInfo arfi in files) {
					if (arfi.Seen)
						continue;
					s = arfi.Info.FullName;
					if (s == null)
						continue;
					tmp2 = arfi.Info.Name;
					if (tmp2.StartsWith (tmp)) {
						al.Add (GetResourceFile (arfi, false));
						arfi.Seen = true;
					}
				}
			}

			// Anything that's left here might be orphans or lone default files.
			// For those files we check the part following the last dot
			// before the .resx/.resource extensions and test whether it's a registered
			// culture or not. If it is not a culture, then we have a
			// default file that doesn't have any translations. Otherwise,
			// the file is ignored (it's the same thing MS.NET does)
			foreach (AppResourceFileInfo arfi in files) {
				if (arfi.Seen)
					continue;

				if (IsFileCultureValid (arfi.Info.FullName) != null)
					continue; // Culture found, we reject the file

				// A single default file, create a group
				List<string> al = new List<string> ();
				al.Add (GetResourceFile (arfi, false));
				groups.Add (al);
			}
			groups.Sort (lcList);
			return groups.ToArray ();
		}

		// CodeDOM generation
		void DomFromResource (string resfile, CodeCompileUnit unit, Dictionary <string,bool> assemblies,
				      CodeDomProvider provider)
		{
			if (String.IsNullOrEmpty (resfile))
				return;

			string fname, nsname, classname;

			fname = Path.GetFileNameWithoutExtension (resfile);
			nsname = Path.GetFileNameWithoutExtension (fname);
			classname = Path.GetExtension (fname);
			if (classname == null || classname.Length == 0) {
				classname = nsname;
				nsname = "Resources";
			} else {
				if (!nsname.StartsWith ("Resources", StringComparison.InvariantCulture))
					nsname = String.Concat ("Resources.", nsname);
				classname = classname.Substring(1);
			}

			if (!String.IsNullOrEmpty (classname))
				classname = classname.Replace ('.', '_');
			if (!String.IsNullOrEmpty (nsname))
				nsname = nsname.Replace ('.', '_');
			
			if (!provider.IsValidIdentifier (nsname) || !provider.IsValidIdentifier (classname))
				throw new ApplicationException ("Invalid resource file name.");

			ResourceReader res;
			try {
				res = new ResourceReader (resfile);
			} catch (ArgumentException) {
				// invalid stream, probably empty - ignore silently and abort
				return;
			}
			
			CodeNamespace ns = new CodeNamespace (nsname);
			CodeTypeDeclaration cls = new CodeTypeDeclaration (classname);
			cls.IsClass = true;
			cls.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

			CodeMemberField cmf = new CodeMemberField (typeof(CultureInfo), "_culture");
			cmf.InitExpression = new CodePrimitiveExpression (null);
			cmf.Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static;
			cls.Members.Add (cmf);

			cmf = new CodeMemberField (typeof(ResourceManager), "_resourceManager");
			cmf.InitExpression = new CodePrimitiveExpression (null);
			cmf.Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static;
			cls.Members.Add (cmf);
			
			// Property: ResourceManager
			CodeMemberProperty cmp = new CodeMemberProperty ();
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
			cmp.Name = "ResourceManager";
			cmp.HasGet = true;
			cmp.Type = new CodeTypeReference (typeof(ResourceManager));
			CodePropertyResourceManagerGet (cmp.GetStatements, resfile, classname);
			cls.Members.Add (cmp);

			// Property: Culture
			cmp = new CodeMemberProperty ();
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
			cmp.Name = "Culture";
			cmp.HasGet = true;
			cmp.HasSet = true;
			cmp.Type = new CodeTypeReference (typeof(CultureInfo));
			CodePropertyGenericGet (cmp.GetStatements, "_culture", classname);
			CodePropertyGenericSet (cmp.SetStatements, "_culture", classname);
			cls.Members.Add (cmp);

			// Add the resource properties
			Dictionary<string,bool> imports = new Dictionary<string,bool> ();
			try {
				foreach (DictionaryEntry de in res) {
					Type type = de.Value.GetType ();

					if (!imports.ContainsKey (type.Namespace))
						imports [type.Namespace] = true;

					string asname = new AssemblyName (type.Assembly.FullName).Name;
					if (!assemblies.ContainsKey (asname))
						assemblies [asname] = true;
					
					cmp = new CodeMemberProperty ();
					cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
					cmp.Name = SanitizeResourceName (provider, (string)de.Key);
					cmp.HasGet = true;
					CodePropertyResourceGet (cmp.GetStatements, (string)de.Key, type, classname);
					cmp.Type = new CodeTypeReference (type);
					cls.Members.Add (cmp);
				}
			} catch (Exception ex) {
				throw new ApplicationException ("Failed to compile global resources.", ex);
			}
			foreach (KeyValuePair<string,bool> de in imports)
				ns.Imports.Add (new CodeNamespaceImport(de.Key));
			
			ns.Types.Add (cls);
			unit.Namespaces.Add (ns);
		}

		static bool is_identifier_start_character (int c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || Char.IsLetter ((char)c);
		}

		static bool is_identifier_part_character (char c)
		{
			if (c >= 'a' && c <= 'z')
				return true;

			if (c >= 'A' && c <= 'Z')
				return true;

			if (c == '_' || (c >= '0' && c <= '9'))
				return true;

			if (c < 0x80)
				return false;

			return Char.IsLetter (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation;
		}
		
		string SanitizeResourceName (CodeDomProvider provider, string name)
		{
			if (provider.IsValidIdentifier (name))
				return provider.CreateEscapedIdentifier (name);

			var sb = new StringBuilder ();
			char ch = name [0];
			if (is_identifier_start_character (ch))
				sb.Append (ch);
			else {
				sb.Append ('_');
				if (ch >= '0' && ch <= '9')
					sb.Append (ch);
			}
			
			for (int i = 1; i < name.Length; i++) {
				ch = name [i];
				if (is_identifier_part_character (ch))
					sb.Append (ch);
				else
					sb.Append ('_');
			}
			
			return provider.CreateEscapedIdentifier (sb.ToString ());
		}
		
		CodeObjectCreateExpression NewResourceManager (string name, string typename)
		{
			CodeExpression resname = new CodePrimitiveExpression (name);
			CodePropertyReferenceExpression asm = new CodePropertyReferenceExpression (
				new CodeTypeOfExpression (new CodeTypeReference (typename)),
				"Assembly");
			
			return new CodeObjectCreateExpression ("System.Resources.ResourceManager",
							       new CodeExpression [] {resname, asm});
		}
		
		void CodePropertyResourceManagerGet (CodeStatementCollection csc, string resfile, string typename)
		{
			string name = Path.GetFileNameWithoutExtension (resfile);
			CodeStatement st;
			CodeExpression exp;

			exp = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typename), "_resourceManager");
			st = new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					exp,
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression (null)),
				new CodeStatement [] { new CodeMethodReturnStatement (exp) });
			csc.Add (st);

			st = new CodeAssignStatement (exp, NewResourceManager (name, typename));
			csc.Add (st);
			csc.Add (new CodeMethodReturnStatement (exp));
		}

		void CodePropertyResourceGet (CodeStatementCollection csc, string resname, Type restype, string typename)
		{
			CodeStatement st = new CodeVariableDeclarationStatement (
				typeof (ResourceManager),
				"rm",
				new CodePropertyReferenceExpression (
					new CodeTypeReferenceExpression (typename), "ResourceManager"));
			csc.Add (st);

			st = new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					new CodeVariableReferenceExpression ("rm"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression (null)),
				new CodeStatement [] { new CodeMethodReturnStatement (new CodePrimitiveExpression (null)) });
			csc.Add (st);

			bool gotstr = (restype == typeof (string));
			CodeExpression exp = new CodeMethodInvokeExpression (
				new CodeVariableReferenceExpression ("rm"),
				gotstr ? "GetString" : "GetObject",
				new CodeExpression [] { new CodePrimitiveExpression (resname),
							new CodeFieldReferenceExpression (
								new CodeTypeReferenceExpression (typename), "_culture") });
			st = new CodeVariableDeclarationStatement (
				restype,
				"obj",
				gotstr ? exp : new CodeCastExpression (restype, exp));
			csc.Add (st);
			csc.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("obj")));
		}
		
		void CodePropertyGenericGet (CodeStatementCollection csc, string field, string typename)
		{
			csc.Add(new CodeMethodReturnStatement (
					new CodeFieldReferenceExpression (
						new CodeTypeReferenceExpression (typename), field)));
		}

		void CodePropertyGenericSet (CodeStatementCollection csc, string field, string typename)
		{
			csc.Add(new CodeAssignStatement (
					new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typename), field),
					new CodeVariableReferenceExpression ("value")));
		}
		
		string CompileResource (AppResourceFileInfo arfi, bool local)
		{
			string path = arfi.Info.FullName;
			string rname = Path.GetFileNameWithoutExtension (path) + ".resources";
			if (!local)
				rname = "Resources." + rname;
			
			string resource = Path.Combine (TempDirectory, rname);
			FileStream source = null, destination = null;
			IResourceReader reader = null;
			ResourceWriter writer = null;

			try {
				source = new FileStream (path, FileMode.Open, FileAccess.Read);
				destination = new FileStream (resource, FileMode.Create, FileAccess.Write);
				reader = GetReaderForKind (arfi.Kind, source, path);
				writer = new ResourceWriter (destination);
				foreach (DictionaryEntry de in reader) {
					object val = de.Value;
					if (val is string)
						writer.AddResource ((string)de.Key, (string)val);
					else
						writer.AddResource ((string)de.Key, val);
				}
			} catch (Exception ex) {
				throw new HttpException ("Failed to compile resource file", ex);
			} finally {
				if (reader != null)
					reader.Dispose ();
				if (source != null)
					source.Dispose ();
				if (writer != null)
					writer.Dispose ();
				if (destination != null)
					destination.Dispose ();
			}
			
			return resource;
		}

		IResourceReader GetReaderForKind (AppResourceFileKind kind, Stream stream, string path)
		{
			switch (kind) {
				case AppResourceFileKind.ResX:
					var ret = new ResXResourceReader (stream, new TypeResolutionService ());
					if (!String.IsNullOrEmpty (path))
						ret.BasePath = Path.GetDirectoryName (path);
					return ret;

				case AppResourceFileKind.Resource:
					return new ResourceReader (stream);

				default:
					return null;
			}
		}
		
							       
		object OnCreateRandomFile (string path)
		{
			FileStream f = new FileStream (path, FileMode.CreateNew);
			f.Close ();
			return path;
		}
	};
};
#endif
