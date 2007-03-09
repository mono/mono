//
// System.Web.Compilation.AppResourceFilesCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation 
{
	internal class AppResourcesCompiler
	{
		const string cachePrefix = "@@LocalResourcesAssemblies";
		
		bool isGlobal;
		HttpContext context;
		AppResourceFilesCollection files;
		string tempDirectory;
		
		string TempDirectory {
			get {
				if (tempDirectory != null)
					return tempDirectory;
				return (tempDirectory = AppDomain.CurrentDomain.SetupInformation.DynamicBase);
			}
		}
		
		public AppResourcesCompiler (HttpContext context, bool isGlobal)
		{
			this.context = context;
			this.isGlobal = isGlobal;
			this.files = new AppResourceFilesCollection (context, isGlobal);
		}

		
		public void Compile ()
		{
			files.Collect ();
			if (!files.HasFiles)
				return;
			if (isGlobal)
				CompileGlobal ();
			else
				CompileLocal ();
		}

		void CompileGlobal ()
		{
			string assemblyPath = FileUtils.CreateTemporaryFile (TempDirectory,
									     "App_GlobalResources",
									     "dll",
									     OnCreateRandomFile) as string;

			if (assemblyPath == null)
				throw new ApplicationException ("Failed to create global resources assembly");
			
			CompilationSection config = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
			if (config == null || !CodeDomProvider.IsDefinedLanguage (config.DefaultLanguage))
				throw new ApplicationException ("Could not get the default compiler.");
			CompilerInfo ci = CodeDomProvider.GetCompilerInfo (config.DefaultLanguage);
			if (ci == null || !ci.IsCodeDomProviderTypeValid)
				throw new ApplicationException ("Failed to obtain the default compiler information.");

			CompilerParameters cp = ci.CreateDefaultCompilerParameters ();
			cp.OutputAssembly = assemblyPath;
			cp.GenerateExecutable = false;
			cp.TreatWarningsAsErrors = true;
			cp.IncludeDebugInformation = config.Debug;
			
			List <string>[] fileGroups = GroupGlobalFiles (cp);
			if (fileGroups == null || fileGroups.Length == 0)
				return;

			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace ns = new CodeNamespace (null);
			ns.Imports.Add (new CodeNamespaceImport ("System"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Globalization"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Reflection"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Resources"));
			unit.Namespaces.Add (ns);

			CodeDomProvider provider;
			provider = ci.CreateProvider ();
			if (provider == null)
				throw new ApplicationException ("Failed to instantiate the default compiler.");
			
			Dictionary <string,bool> assemblies = new Dictionary<string,bool> ();
			foreach (List<string> ls in fileGroups)
				DomFromResource (ls [0], unit, assemblies, provider);
			foreach (KeyValuePair<string,bool> de in assemblies)
				unit.ReferencedAssemblies.Add (de.Key);
			
			AssemblyBuilder abuilder = new AssemblyBuilder (provider);
			abuilder.AddCodeCompileUnit (unit);

			CompilerResults results = abuilder.BuildAssembly (cp);
			if (results.Errors.Count == 0) {
				BuildManager.TopLevelAssemblies.Add (results.CompiledAssembly);
				HttpContext.AppGlobalResourcesAssembly = results.CompiledAssembly;
			} else {
				if (context.IsCustomErrorEnabled)
					throw new ApplicationException ("An error occurred while compiling global resources.");
				throw new CompilationException (null, results.Errors, null);
			}
			HttpRuntime.WritePreservationFile (results.CompiledAssembly, "App_GlobalResources");
			HttpRuntime.EnableAssemblyMapping (true);
		}

		void CompileLocal ()
		{
			string path = Path.GetDirectoryName (VirtualPathUtility.ToAbsolute (context.Request.CurrentExecutionFilePath));
			
			if (String.IsNullOrEmpty (path))
				throw new ApplicationException ("Unable to determine the request virtual path.");

			Assembly cached = GetCachedLocalResourcesAssembly (path);
			if (cached != null)
				return;
			
			string prefix;
			if (path == "/")
				prefix = "App_LocalResources.root";
			else
				prefix = "App_LocalResources" + path.Replace ('/', '.');
			
			string assemblyPath = FileUtils.CreateTemporaryFile (TempDirectory,
									     prefix,
									     "dll",
									     OnCreateRandomFile) as string;
			if (assemblyPath == null)
				throw new ApplicationException ("Failed to create global resources assembly");
			
			CompilationSection config = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
			if (config == null || !CodeDomProvider.IsDefinedLanguage (config.DefaultLanguage))
				throw new ApplicationException ("Could not get the default compiler.");
			CompilerInfo ci = CodeDomProvider.GetCompilerInfo (config.DefaultLanguage);
			if (ci == null || !ci.IsCodeDomProviderTypeValid)
				throw new ApplicationException ("Failed to obtain the default compiler information.");

			CompilerParameters cp = ci.CreateDefaultCompilerParameters ();
			cp.OutputAssembly = assemblyPath;
			cp.GenerateExecutable = false;
			cp.TreatWarningsAsErrors = true;
			cp.IncludeDebugInformation = config.Debug;

			List<AppResourceFileInfo> files = this.files.Files;
			foreach (AppResourceFileInfo arfi in files)
				GetResourceFile (arfi, cp);

			CodeDomProvider provider;
			provider = ci.CreateProvider ();
			if (provider == null)
				throw new ApplicationException ("Failed to instantiate the default compiler.");
			
			AssemblyBuilder abuilder = new AssemblyBuilder (provider);
			CompilerResults results = abuilder.BuildAssembly (cp);
			if (results.Errors.Count == 0) {
				AddAssemblyToCache (path, results.CompiledAssembly);
			} else {
				if (context.IsCustomErrorEnabled)
					throw new ApplicationException ("An error occurred while compiling global resources.");
				throw new CompilationException (null, results.Errors, null);
			}
		}

		internal static Assembly GetCachedLocalResourcesAssembly (string path)
		{
			Dictionary <string, Assembly> cache;

			cache = HttpRuntime.Cache[cachePrefix] as Dictionary <string, Assembly>;
			if (cache == null || !cache.ContainsKey (path))
				return null;
			return cache [path];
		}

		void AddAssemblyToCache (string path, Assembly asm)
		{
			Cache runtimeCache = HttpRuntime.Cache;
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

		bool IsFileCultureValid (string fileName)
                {
                    string tmp = Path.GetFileNameWithoutExtension (fileName);
                    tmp = Path.GetExtension (tmp);
                    if (tmp != null && tmp.Length > 0) {
                              tmp = tmp.Substring (1);
                            try {
                                CultureInfo.GetCultureInfo (tmp);
                                return true;
                            } catch {
                                return false;
                            }
                    } 
                    return false;
                }

		string GetResourceFile (AppResourceFileInfo arfi, CompilerParameters cp)
		{
			string resfile;
			if (arfi.Kind == AppResourceFileKind.ResX)
				resfile = CompileResource (arfi);
			else
				resfile = arfi.Info.FullName;
			if (!String.IsNullOrEmpty (resfile))
				cp.EmbeddedResources.Add (resfile);
			return resfile;
		}
		
		List <string>[] GroupGlobalFiles (CompilerParameters cp)
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
						if (IsFileCultureValid (s2)) {
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
					al.Add (GetResourceFile (arfi, cp));
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
				foreach (AppResourceFileInfo arfi in files) {
					if (arfi.Seen)
						continue;
					
					s = arfi.Info.FullName;
					if (s == null)
						continue;
					tmp2 = arfi.Info.Name;
					if (tmp2.StartsWith (tmp)) {
						al.Add (GetResourceFile (arfi, cp));
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

				if (IsFileCultureValid (arfi.Info.FullName))
					continue; // Culture found, we reject the file

				// A single default file, create a group
				List<string> al = new List<string> ();
				al.Add (GetResourceFile (arfi, cp));
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
					nsname = String.Format ("Resources.{0}", nsname);
				classname = classname.Substring(1);
			}
			
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

			CodeMemberField cmf = new CodeMemberField (typeof(CultureInfo), "culture");
			cmf.InitExpression = new CodePrimitiveExpression (null);
			cmf.Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static;
			cls.Members.Add (cmf);

			cmf = new CodeMemberField (typeof(ResourceManager), "resourceManager");
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
			CodePropertyGenericGet (cmp.GetStatements, "culture", classname);
			CodePropertyGenericSet (cmp.SetStatements, "culture", classname);
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
					cmp.Name = SanitizeResourceName ((string)de.Key);
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

		string SanitizeResourceName (string name)
		{
			return name.Replace (' ', '_').Replace ('-', '_').Replace ('.', '_');
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

			exp = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typename), "resourceManager");
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
								new CodeTypeReferenceExpression (typename), "culture") });
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
		
		string CompileResource (AppResourceFileInfo arfi)
		{
			string path = arfi.Info.FullName;
			string resource = Path.Combine (TempDirectory,
							"Resources." + Path.GetFileNameWithoutExtension (path) + ".resources");
			FileStream source = null, destination = null;
			IResourceReader reader = null;
			ResourceWriter writer = null;

			try {
				source = new FileStream (path, FileMode.Open, FileAccess.Read);
				destination = new FileStream (resource, FileMode.Create, FileAccess.Write);
				reader = GetReaderForKind (arfi.Kind, source);
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
					reader.Close ();
				else if (source != null)
					source.Close ();
				if (writer != null)
					writer.Close ();
				else if (destination != null)
					destination.Close ();
			}
			
			return resource;
		}

		IResourceReader GetReaderForKind (AppResourceFileKind kind, Stream stream)
		{
			switch (kind) {
				case AppResourceFileKind.ResX:
					return new ResXResourceReader (stream);

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
