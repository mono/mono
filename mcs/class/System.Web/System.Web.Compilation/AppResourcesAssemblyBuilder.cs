//
// System.Web.Compilation.AppResourceAseemblyBuilder
//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2007-2009 Novell, Inc (http://novell.com/)
// (C) 2011 Xamarin, Inc (http://xamarin.com/)

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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation
{
	class AppResourcesAssemblyBuilder
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
			List <string> defaultCultureFiles = appResourcesCompiler.DefaultCultureFiles;
			
			if (defaultCultureFiles != null)
				BuildDefaultAssembly (defaultCultureFiles, unit);
			
			foreach (KeyValuePair <string, List <string>> kvp in cultures)
				BuildSatelliteAssembly (kvp.Key, kvp.Value);
		}

		void BuildDefaultAssembly (List <string> files, CodeCompileUnit unit)
		{
			AssemblyBuilder abuilder = new AssemblyBuilder (Provider);
			if (unit != null)
				abuilder.AddCodeCompileUnit (unit);
			
			CompilerParameters cp = ci.CreateDefaultCompilerParameters ();
			cp.OutputAssembly = baseAssemblyPath;
			cp.GenerateExecutable = false;
			cp.TreatWarningsAsErrors = true;
			cp.IncludeDebugInformation = config.Debug;

			foreach (string f in files)
				cp.EmbeddedResources.Add (f);
			
			CompilerResults results = abuilder.BuildAssembly (cp);
			if (results == null)
				return;
			
			if (results.NativeCompilerReturnValue == 0) {
				mainAssembly = results.CompiledAssembly;
				BuildManager.TopLevelAssemblies.Add (mainAssembly);
			} else {
				if (HttpContext.Current.IsCustomErrorEnabled)
					throw new ApplicationException ("An error occurred while compiling global resources.");
				throw new CompilationException (null, results.Errors, null);
			}
			
			HttpRuntime.WritePreservationFile (mainAssembly, canonicAssemblyName);
			HttpRuntime.EnableAssemblyMapping (true);
		}

		void BuildSatelliteAssembly (string cultureName, List <string> files)
		{
			string assemblyPath = BuildAssemblyPath (cultureName);
			var info = new ProcessStartInfo ();
			var al = new Process ();

			string arguments = SetAlPath (info);
			var sb = new StringBuilder (arguments);

			sb.Append ("/c:\"" + cultureName + "\" ");
			sb.Append ("/t:lib ");
			sb.Append ("/out:\"" + assemblyPath + "\" ");
			if (mainAssembly != null)
				sb.Append ("/template:\"" + mainAssembly.Location + "\" ");
			
			string responseFilePath = assemblyPath + ".response";
			using (FileStream fs = File.OpenWrite (responseFilePath)) {
				using (StreamWriter sw = new StreamWriter (fs)) {
					foreach (string f in files) 
						sw.WriteLine ("/embed:\"" + f + "\" ");
				}
			}
			sb.Append ("@\"" + responseFilePath + "\"");
			
			info.Arguments = sb.ToString ();
			info.CreateNoWindow = true;
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			
			al.StartInfo = info;

			var alOutput = new StringCollection ();
			var alMutex = new Mutex ();
			DataReceivedEventHandler outputHandler = (object sender, DataReceivedEventArgs args) => {
				if (args.Data != null) {
					alMutex.WaitOne ();
					alOutput.Add (args.Data);
					alMutex.ReleaseMutex ();
				}
			};
			
			al.ErrorDataReceived += outputHandler;
			al.OutputDataReceived += outputHandler;

			// TODO: consider using asynchronous processes
			try {
				al.Start ();
			} catch (Exception ex) {
				throw new HttpException (String.Format ("Error running {0}", al.StartInfo.FileName), ex);
			}

			Exception alException = null;
			int exitCode = 0;
			try {
				al.BeginOutputReadLine ();
				al.BeginErrorReadLine ();
				al.WaitForExit ();
				exitCode = al.ExitCode;
			} catch (Exception ex) {
				alException = ex;
			} finally {
				al.CancelErrorRead ();
				al.CancelOutputRead ();
				al.Close ();
			}

			if (exitCode != 0 || alException != null) {
				// TODO: consider adding a new type of compilation exception,
				// tailored for al
				CompilerErrorCollection errors = null;
				
				if (alOutput.Count != 0) {
					foreach (string line in alOutput) {
						if (!line.StartsWith ("ALINK: error ", StringComparison.Ordinal))
							continue;
						if (errors == null)
							errors = new CompilerErrorCollection ();

						int colon = line.IndexOf (':', 13);
						string errorNumber = colon != -1 ? line.Substring (13, colon - 13) : "Unknown";
						string errorText = colon != -1 ? line.Substring (colon + 1) : line.Substring (13);
						
						errors.Add (new CompilerError (Path.GetFileName (assemblyPath), 0, 0, errorNumber, errorText));
					}
				}
				
				throw new CompilationException (Path.GetFileName (assemblyPath), errors, null);
			}
		}

		string SetAlPath (ProcessStartInfo info)
		{			
			if (RuntimeHelpers.RunningOnWindows) {
				info.FileName = MonoToolsLocator.Mono;
				return MonoToolsLocator.AssemblyLinker + " ";
			} else {
				info.FileName = MonoToolsLocator.AssemblyLinker;
				return String.Empty;
			}
		}

		string BuildAssemblyPath (string cultureName)
		{
			string baseDir = Path.Combine (baseAssemblyDirectory, cultureName);
			if (!Directory.Exists (baseDir))
				Directory.CreateDirectory (baseDir);
			
			string baseFileName = Path.GetFileNameWithoutExtension (baseAssemblyPath);
			string fileName = String.Concat (baseFileName, ".resources.dll");
			fileName = Path.Combine (baseDir, fileName);

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

