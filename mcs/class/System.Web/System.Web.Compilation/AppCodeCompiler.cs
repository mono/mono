//
// System.Web.Compilation.AppCodeCompiler: A compiler for the App_Code folder
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
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation
{
	internal class AppCodeAssembly
	{
		private List<string> files;
		private string name;
		private string path;
		private bool validAssembly;
		
		public bool IsValid
		{
			get { return validAssembly; }
		}

		public string SourcePath
		{
			get { return path; }
		}

// temporary
		public string Name
		{
			get { return name; }
		}
		
		public List<string> Files
		{
			get { return files; }
		}
// temporary
		
		public AppCodeAssembly (string name, string path)
		{
			this.files = new List<string>();
			this.validAssembly = true;
			this.name = name;
			this.path = path;
		}

		public void AddFile (string path)
		{
			files.Add (path);
		}

		object OnCreateTemporaryAssemblyFile (string path)
		{
			FileStream f = new FileStream (path, FileMode.CreateNew);
			f.Close ();
			return path;
		}
		
		// Build and add the assembly to the BuildManager's
		// CodeAssemblies collection
		public void Build ()
		{
			Type compilerProvider = null;
			CompilerInfo compilerInfo = null, cit;
			string extension, language, cpfile = null;
			List<string> knownfiles = new List<string>();
			List<string> unknownfiles = new List<string>();
			
			// First make sure all the files are in the same
			// language
			bool known;
			foreach (string f in files) {
				known = true;
				language = null;
				
				extension = Path.GetExtension (f);
				if (!CodeDomProvider.IsDefinedExtension (extension))
					known = false;
				if (known) {
					language = CodeDomProvider.GetLanguageFromExtension(extension);
					if (!CodeDomProvider.IsDefinedLanguage (language))
						known = false;
				}
				if (!known || language == null) {
					unknownfiles.Add (f);
					continue;
				}
				
				cit = CodeDomProvider.GetCompilerInfo (language);
				if (cit == null || !cit.IsCodeDomProviderTypeValid)
					continue;
				if (compilerProvider == null) {
					cpfile = f;
					compilerProvider = cit.CodeDomProviderType;
					compilerInfo = cit;
				} else if (compilerProvider != cit.CodeDomProviderType)
					throw new HttpException (
						String.Format (
							"Files {0} and {1} are in different languages - they cannot be compiled into the same assembly",
							Path.GetFileName (cpfile),
							Path.GetFileName (f)));
				knownfiles.Add (f);
			}

			CodeDomProvider provider = null;
			if (compilerInfo == null) {
				CompilationSection config = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
				if (config == null || !CodeDomProvider.IsDefinedLanguage (config.DefaultLanguage))
					throw new HttpException ("Failed to retrieve default source language");
				compilerInfo = CodeDomProvider.GetCompilerInfo (config.DefaultLanguage);
				if (compilerInfo == null || !compilerInfo.IsCodeDomProviderTypeValid)
					throw new HttpException ("Internal error while initializing application");
				provider = compilerInfo.CreateProvider ();
				if (provider == null)
					throw new HttpException ("A code provider error occurred while initializing application.");
			}

			provider = compilerInfo.CreateProvider ();
			if (provider == null)
				throw new HttpException ("A code provider error occurred while initializing application.");

			AssemblyBuilder abuilder = new AssemblyBuilder (provider);
			foreach (string file in knownfiles)
				abuilder.AddCodeFile (file);
			
			BuildProvider bprovider;
			CompilationSection compilationSection = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
			if (compilationSection != null) {
				BuildProviderCollection buildProviders = compilationSection.BuildProviders;
				
				foreach (string file in unknownfiles) {
					bprovider = GetBuildProviderFor (file, buildProviders);
					if (bprovider == null)
						continue;
					bprovider.GenerateCode (abuilder);
				}
			}
			
			string assemblyName = (string)FileUtils.CreateTemporaryFile (
				AppDomain.CurrentDomain.SetupInformation.DynamicBase,
				name, "dll", OnCreateTemporaryAssemblyFile);
			CompilerParameters parameters = compilerInfo.CreateDefaultCompilerParameters ();
			parameters.OutputAssembly = assemblyName;
			CompilerResults results = abuilder.BuildAssembly (parameters);
			if (results.Errors.Count == 0) {
				BuildManager.CodeAssemblies.Add (results.PathToAssembly);
				BuildManager.TopLevelAssemblies.Add (results.CompiledAssembly);
			} else {
				if (HttpContext.Current.IsCustomErrorEnabled)
					throw new HttpException ("An error occurred while initializing application.");
				throw new CompilationException (null, results.Errors, null);
			}
		}

		private BuildProvider GetBuildProviderFor (string file, BuildProviderCollection buildProviders)
		{
			if (file == null || file.Length == 0 || buildProviders == null || buildProviders.Count == 0)
				return null;

			foreach (BuildProvider bp in buildProviders)
				if (IsCorrectBuilderType (bp))
					return bp;

			return null;
		}

		private bool IsCorrectBuilderType (BuildProvider bp)
		{
			if (bp == null)
				return false;
			Type type;
			object[] attrs;

			type = bp.GetType ();
			attrs = type.GetCustomAttributes (true);
			if (attrs == null)
				return false;
			
			BuildProviderAppliesToAttribute bpAppliesTo;
			bool attributeFound = false;
			foreach (object attr in attrs) {
				bpAppliesTo = attr as BuildProviderAppliesToAttribute;
				if (bpAppliesTo == null)
					continue;
				attributeFound = true;
				if ((bpAppliesTo.AppliesTo & BuildProviderAppliesTo.All) == BuildProviderAppliesTo.All ||
				    (bpAppliesTo.AppliesTo & BuildProviderAppliesTo.Code) == BuildProviderAppliesTo.Code)
					return true;
			}

			if (attributeFound)
				return false;
			return true;
		}
		
	}
	
	internal class AppCodeCompiler
	{
		// A dictionary that contains an entry per an assembly that will
		// be produced by compiling App_Code. There's one main assembly
		// and an optional number of assemblies as defined by the
		// codeSubDirectories sub-element of the compilation element in
		// the system.web section of the app's config file.
		// Each entry's value is an AppCodeAssembly instance.
		//
		// Assemblies are named as follows:
		//
		//  1. main assembly: App_Code.{HASH}
		//  2. subdir assemblies: App_SubCode_{DirName}.{HASH}
		//
		// If any of the assemblies contains files that would be
		// compiled with different compilers, a System.Web.HttpException
		// is thrown.
		//
		// Files for which there is no explicit builder are ignored
		// silently
		//
		// Files for which exist BuildProviders but which have no
		// unambiguous language assigned to them (e.g. .wsdl files), are
		// built using the default website compiler.
		private List<AppCodeAssembly> assemblies;
		
		public AppCodeCompiler ()
		{
			assemblies = new List<AppCodeAssembly>();
		}

		public void Compile ()
		{
			string appCode = Path.Combine (HttpRuntime.AppDomainAppPath, "App_Code");
			if (!Directory.Exists (appCode))
				return;
			
			// First process the codeSubDirectories
			CompilationSection cs = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
			
			if (cs != null) {
				string aname;
				for (int i = 0; i < cs.CodeSubDirectories.Count; i++) {
					aname = String.Format ("App_SubCode_{0}", cs.CodeSubDirectories[i].DirectoryName);
					assemblies.Add (new AppCodeAssembly (
								aname,
								Path.Combine (appCode, cs.CodeSubDirectories[i].DirectoryName)));
				}
			}
			AppCodeAssembly defasm = new AppCodeAssembly ("App_Code", appCode);
			assemblies.Add (defasm);
			CollectFiles (appCode, defasm);

			foreach (AppCodeAssembly aca in assemblies)
				aca.Build ();
		}

		private void CollectFiles (string dir, AppCodeAssembly aca)
		{
			AppCodeAssembly curaca = aca;
			foreach (string f in Directory.GetFiles (dir))
				aca.AddFile (f);
			foreach (string d in Directory.GetDirectories (dir)) {
				foreach (AppCodeAssembly a in assemblies)
					if (a.SourcePath == d) {
						curaca = a;
						break;
					}
				CollectFiles (d, curaca);
				curaca = aca;
			}
		}
	}
}
#endif
