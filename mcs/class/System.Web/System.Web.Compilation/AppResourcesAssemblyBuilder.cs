//
// System.Web.Compilation.AppResourceAseemblyBuilder
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	internal class AppResourcesAssemblyBuilder
	{
		CompilationSection config;
		CompilerInfo ci;
		CodeDomProvider _provider;
		string baseAssemblyPath;
		string baseAssemblyDirectory;
		string canonicAssemblyName;
		Assembly mainAssembly;
		AppResourcesCompiler appResourcesCompiler;
		
		public CodeDomProvider Provider {
			get {
				if (_provider == null)
					_provider = ci.CreateProvider ();
				else
					return _provider;
				
				if (_provider == null)
					throw new ApplicationException ("Failed to instantiate the default compiler.");
				return _provider;
			}
		}
		
		public Assembly MainAssembly {
			get { return mainAssembly; }
		}
		
		public AppResourcesAssemblyBuilder (string canonicAssemblyName, string baseAssemblyPath, AppResourcesCompiler appres)
		{
			this.appResourcesCompiler = appres;
			this.baseAssemblyPath = baseAssemblyPath;
			this.baseAssemblyDirectory = Path.GetDirectoryName (baseAssemblyPath);
			this.canonicAssemblyName = canonicAssemblyName;
			
			config = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			if (config == null || !CodeDomProvider.IsDefinedLanguage (config.DefaultLanguage))
				throw new ApplicationException ("Could not get the default compiler.");
			ci = CodeDomProvider.GetCompilerInfo (config.DefaultLanguage);
			if (ci == null || !ci.IsCodeDomProviderTypeValid)
				throw new ApplicationException ("Failed to obtain the default compiler information.");
		}

		public void Build ()
		{
			Build (null);
		}
		
		public void Build (CodeCompileUnit unit)
		{
			Dictionary <string, List <string>> cultures = appResourcesCompiler.CultureFiles;
			string defaultAssemblyKey = AppResourcesCompiler.DefaultCultureKey;
			
			foreach (KeyValuePair <string, List <string>> kvp in cultures)
				BuildAssembly (kvp.Key, kvp.Value, defaultAssemblyKey, unit);
		}

		void BuildAssembly (string cultureName, List <string> files, string defaultAssemblyKey, CodeCompileUnit unit)
		{
			bool defaultAssembly = cultureName == defaultAssemblyKey;			
			AssemblyBuilder abuilder = new AssemblyBuilder (Provider);
			if (unit != null && defaultAssembly)
				abuilder.AddCodeCompileUnit (unit);
			
			string assemblyPath = defaultAssembly ? baseAssemblyPath : BuildAssemblyPath (cultureName, abuilder);
			CompilerParameters cp = ci.CreateDefaultCompilerParameters ();
			cp.OutputAssembly = assemblyPath;
			cp.GenerateExecutable = false;
			cp.TreatWarningsAsErrors = true;
			cp.IncludeDebugInformation = config.Debug;

			foreach (string f in files)
				cp.EmbeddedResources.Add (f);
			
			CompilerResults results = abuilder.BuildAssembly (cp);
			if (results == null)
				return;
			
			Assembly ret = null;
			
			if (results.NativeCompilerReturnValue == 0) {
				ret = results.CompiledAssembly;
				if (defaultAssembly) {
					BuildManager.TopLevelAssemblies.Add (ret);
					mainAssembly = ret;
				}
			} else {
				if (HttpContext.Current.IsCustomErrorEnabled)
					throw new ApplicationException ("An error occurred while compiling global resources.");
				throw new CompilationException (null, results.Errors, null);
			}
			
			if (defaultAssembly) {
				HttpRuntime.WritePreservationFile (ret, canonicAssemblyName);
				HttpRuntime.EnableAssemblyMapping (true);
			}
		}

		string BuildAssemblyPath (string cultureName, AssemblyBuilder abuilder)
		{
			string baseDir = Path.Combine (baseAssemblyDirectory, cultureName);
			if (!Directory.Exists (baseDir))
				Directory.CreateDirectory (baseDir);
			
			string baseFileName = Path.GetFileNameWithoutExtension (baseAssemblyPath);
			string fileName = String.Concat (baseFileName, ".resources.dll");
			fileName = Path.Combine (baseDir, fileName);

			CodeCompileUnit assemblyInfo = GenerateAssemblyInfo (cultureName);
			if (assemblyInfo != null)
				abuilder.AddCodeCompileUnit (assemblyInfo);

			return fileName;
		}

		CodeCompileUnit GenerateAssemblyInfo (string cultureName)
		{
			CodeAttributeArgument[] args = new CodeAttributeArgument [1];
			args [0] = new CodeAttributeArgument (new CodePrimitiveExpression (cultureName));

			CodeCompileUnit unit = new CodeCompileUnit ();
			unit.AssemblyCustomAttributes.Add (
				new CodeAttributeDeclaration (
					new CodeTypeReference ("System.Reflection.AssemblyCultureAttribute"),
					args));

			args = new CodeAttributeArgument [2];
			args [0] = new CodeAttributeArgument (new CodePrimitiveExpression ("ASP.NET"));
			args [1] = new CodeAttributeArgument (new CodePrimitiveExpression (Environment.Version.ToString ()));
			unit.AssemblyCustomAttributes.Add (
				new CodeAttributeDeclaration (
					new CodeTypeReference ("System.CodeDom.Compiler.GeneratedCodeAttribute"),
					args));
			
			return unit;
		}
	}
}
#endif
