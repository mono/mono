//
// Mono.CSharp CSharpCodeCompiler Class implementation
//
// Authors:
//	Sean Kasun (seank@users.sf.net)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) Novell, Inc. (http://www.novell.com)
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

namespace Mono.CSharp
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.ComponentModel;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Collections.Generic;
	using System.Globalization;
	
	internal class CSharpCodeCompiler : CSharpCodeGenerator, ICodeCompiler
	{
		static string windowsMcsPath;
		static string windowsMonoPath;

		Mutex mcsOutMutex;
		StringCollection mcsOutput;
		
		static CSharpCodeCompiler ()
		{
			if (Path.DirectorySeparatorChar == '\\') {
				PropertyInfo gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static|BindingFlags.NonPublic);
				MethodInfo get_gac = gac.GetGetMethod (true);
				string p = Path.GetDirectoryName (
					(string) get_gac.Invoke (null, null));
				windowsMonoPath = Path.Combine (
					Path.GetDirectoryName (
						Path.GetDirectoryName (p)),
					"bin\\mono.bat");
				if (!File.Exists (windowsMonoPath))
					windowsMonoPath = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (p)),
						"bin\\mono.exe");
				if (!File.Exists (windowsMonoPath))
					windowsMonoPath = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (
								Path.GetDirectoryName (p))),
						"mono\\mono\\mini\\mono.exe");
				if (!File.Exists (windowsMonoPath))
					throw new FileNotFoundException ("Windows mono path not found: " + windowsMonoPath);

				windowsMcsPath = Path.Combine (p, "4.5\\mcs.exe");
				if (!File.Exists (windowsMcsPath))
					windowsMcsPath = Path.Combine(Path.GetDirectoryName (p), "lib\\build\\mcs.exe");
				
				if (!File.Exists (windowsMcsPath))
					throw new FileNotFoundException ("Windows mcs path not found: " + windowsMcsPath);
			}
		}

		//
		// Constructors
		//
		public CSharpCodeCompiler()
		{
		}

		public CSharpCodeCompiler (IDictionary <string, string> providerOptions) :
			base (providerOptions)
		{
		}
		
		//
		// Methods
		//
		public CompilerResults CompileAssemblyFromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return CompileAssemblyFromDomBatch (options, new CodeCompileUnit[] { e });
		}

		public CompilerResults CompileAssemblyFromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromDomBatch (options, ea);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromFile (CompilerParameters options, string fileName)
		{
			return CompileAssemblyFromFileBatch (options, new string[] { fileName });
		}

		public CompilerResults CompileAssemblyFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromFileBatch (options, fileNames);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromSource (CompilerParameters options, string source)
		{
			return CompileAssemblyFromSourceBatch (options, new string[] { source });
		}

		public CompilerResults CompileAssemblyFromSourceBatch (CompilerParameters options, string[] sources)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromSourceBatch (options, sources);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		private CompilerResults CompileFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			if (null == options)
				throw new ArgumentNullException("options");
			if (null == fileNames)
				throw new ArgumentNullException("fileNames");

			CompilerResults results=new CompilerResults(options.TempFiles);
			Process mcs=new Process();

			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				mcs.StartInfo.FileName = windowsMonoPath;
				mcs.StartInfo.Arguments = "\"" + windowsMcsPath + "\" " +
					BuildArgs (options, fileNames, ProviderOptions);
			} else {
				mcs.StartInfo.FileName="mcs";
				mcs.StartInfo.Arguments=BuildArgs(options, fileNames, ProviderOptions);
			}

			mcsOutput = new StringCollection ();
			mcsOutMutex = new Mutex ();
/*		       
			string monoPath = Environment.GetEnvironmentVariable ("MONO_PATH");
			if (monoPath != null)
				monoPath = String.Empty;

			string privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
			if (privateBinPath != null && privateBinPath.Length > 0)
				monoPath = String.Format ("{0}:{1}", privateBinPath, monoPath);

			if (monoPath.Length > 0) {
				StringDictionary dict = mcs.StartInfo.EnvironmentVariables;
				if (dict.ContainsKey ("MONO_PATH"))
					dict ["MONO_PATH"] = monoPath;
				else
					dict.Add ("MONO_PATH", monoPath);
			}
*/
			/*
			 * reset MONO_GC_PARAMS - we are invoking compiler possibly with another GC that
			 * may not handle some of the options causing compilation failure
			 */
			mcs.StartInfo.EnvironmentVariables ["MONO_GC_PARAMS"] = String.Empty;

			mcs.StartInfo.CreateNoWindow=true;
			mcs.StartInfo.UseShellExecute=false;
			mcs.StartInfo.RedirectStandardOutput=true;
			mcs.StartInfo.RedirectStandardError=true;
			mcs.ErrorDataReceived += new DataReceivedEventHandler (McsStderrDataReceived);
			
			try {
				mcs.Start();
			} catch (Exception e) {
				Win32Exception exc = e as Win32Exception;
				if (exc != null) {
					throw new SystemException (String.Format ("Error running {0}: {1}", mcs.StartInfo.FileName,
									Win32Exception.W32ErrorMessage (exc.NativeErrorCode)));
				}
				throw;
			}

			try {
				mcs.BeginOutputReadLine ();
				mcs.BeginErrorReadLine ();
				mcs.WaitForExit();
				
				results.NativeCompilerReturnValue = mcs.ExitCode;
			} finally {
				mcs.CancelErrorRead ();
				mcs.CancelOutputRead ();
				mcs.Close();
			}

			StringCollection sc = mcsOutput;
		       
 			bool loadIt=true;
			foreach (string error_line in mcsOutput) {
				CompilerError error = CreateErrorFromString (error_line);
				if (error != null) {
					results.Errors.Add (error);
					if (!error.IsWarning)
						loadIt = false;
				}
			}
			
			if (sc.Count > 0) {
				sc.Insert (0, mcs.StartInfo.FileName + " " + mcs.StartInfo.Arguments + Environment.NewLine);
				results.Output = sc;
			}

			if (loadIt) {
				if (!File.Exists (options.OutputAssembly)) {
					StringBuilder sb = new StringBuilder ();
					foreach (string s in sc)
						sb.Append (s + Environment.NewLine);
					
					throw new Exception ("Compiler failed to produce the assembly. Output: '" + sb.ToString () + "'");
				}
				
				if (options.GenerateInMemory) {
					using (FileStream fs = File.OpenRead(options.OutputAssembly)) {
						byte[] buffer = new byte[fs.Length];
						fs.Read(buffer, 0, buffer.Length);
						results.CompiledAssembly = Assembly.Load(buffer, null);
						fs.Close();
					}
				} else {
					// Avoid setting CompiledAssembly right now since the output might be a netmodule
					results.PathToAssembly = options.OutputAssembly;
				}
			} else {
				results.CompiledAssembly = null;
			}
			
			return results;
		}

		void McsStderrDataReceived (object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null) {
				mcsOutMutex.WaitOne ();
				mcsOutput.Add (args.Data);
				mcsOutMutex.ReleaseMutex ();
			}
		}		

		private static string BuildArgs(CompilerParameters options,string[] fileNames, IDictionary <string, string> providerOptions)
		{
			StringBuilder args=new StringBuilder();
			if (options.GenerateExecutable)
				args.Append("/target:exe ");
			else
				args.Append("/target:library ");

			string privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
			if (privateBinPath != null && privateBinPath.Length > 0)
				args.AppendFormat ("/lib:\"{0}\" ", privateBinPath);
			
			if (options.Win32Resource != null)
				args.AppendFormat("/win32res:\"{0}\" ",
					options.Win32Resource);

			if (options.IncludeDebugInformation)
				args.Append("/debug+ /optimize- ");
			else
				args.Append("/debug- /optimize+ ");

			if (options.TreatWarningsAsErrors)
				args.Append("/warnaserror ");

			if (options.WarningLevel >= 0)
				args.AppendFormat ("/warn:{0} ", options.WarningLevel);

			if (options.OutputAssembly == null || options.OutputAssembly.Length == 0) {
				string extension = (options.GenerateExecutable ? "exe" : "dll");
				options.OutputAssembly = GetTempFileNameWithExtension (options.TempFiles, extension,
					!options.GenerateInMemory);
			}
			args.AppendFormat("/out:\"{0}\" ",options.OutputAssembly);

			foreach (string import in options.ReferencedAssemblies) {
				if (import == null || import.Length == 0)
					continue;

				args.AppendFormat("/r:\"{0}\" ",import);
			}

			if (options.CompilerOptions != null) {
				args.Append (options.CompilerOptions);
				args.Append (" ");
			}

			foreach (string embeddedResource in options.EmbeddedResources) {
				args.AppendFormat("/resource:\"{0}\" ", embeddedResource);
			}

			foreach (string linkedResource in options.LinkedResources) {
				args.AppendFormat("/linkresource:\"{0}\" ", linkedResource);
			}
			
			if (providerOptions != null && providerOptions.Count > 0) {
				string langver;

				if (!providerOptions.TryGetValue ("CompilerVersion", out langver))
					langver = "3.5";

				if (langver.Length >= 1 && langver [0] == 'v')
					langver = langver.Substring (1);

				switch (langver) {
					case "2.0":
						args.Append ("/langversion:ISO-2 ");
						break;

					case "3.5":
						// current default, omit the switch
						break;
				}
			}

			args.Append("/sdk:4.5");

			args.Append (" -- ");
			foreach (string source in fileNames)
				args.AppendFormat("\"{0}\" ",source);
			return args.ToString();
		}

		// Keep in sync with mcs/class/Microsoft.Build.Utilities/Microsoft.Build.Utilities/ToolTask.cs
		const string ErrorRegexPattern = @"
			^
			(\s*(?<file>[^\(]+)                         # filename (optional)
			 (\((?<line>\d*)(,(?<column>\d*[\+]*))?\))? # line+column (optional)
			 :\s+)?
			(?<level>\w+)                               # error|warning
			\s+
			(?<number>[^:]*\d)                          # CS1234
			:
			\s*
			(?<message>.*)$";

		private static CompilerError CreateErrorFromString(string error_string)
		{
			if (error_string.StartsWith ("BETA"))
				return null;

			if (error_string == null || error_string == "")
				return null;

			CompilerError error=new CompilerError();
			Regex reg = new Regex (ErrorRegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
			Match match=reg.Match(error_string);
			if (!match.Success) {
				// We had some sort of runtime crash
				error.ErrorText = error_string;
				error.IsWarning = false;
				error.ErrorNumber = "";
				return error;
			}
			if (String.Empty != match.Result("${file}"))
				error.FileName=match.Result("${file}");
			if (String.Empty != match.Result("${line}"))
				error.Line=Int32.Parse(match.Result("${line}"));
			if (String.Empty != match.Result("${column}"))
				error.Column=Int32.Parse(match.Result("${column}").Trim('+'));

			string level = match.Result ("${level}");
			if (level == "warning")
				error.IsWarning = true;
			else if (level != "error")
				return null; // error CS8028 will confuse the regex.

			error.ErrorNumber=match.Result("${number}");
			error.ErrorText=match.Result("${message}");
			return error;
		}

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension, bool keepFile)
		{
			return temp_files.AddExtension (extension, keepFile);
		}

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension)
		{
			return temp_files.AddExtension (extension);
		}

		private CompilerResults CompileFromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			if (ea == null) {
				throw new ArgumentNullException ("ea");
			}

			string[] fileNames = new string[ea.Length];
			StringCollection assemblies = options.ReferencedAssemblies;

			for (int i = 0; i < ea.Length; i++) {
				CodeCompileUnit compileUnit = ea[i];
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".cs");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f, Encoding.UTF8);
				if (compileUnit.ReferencedAssemblies != null) {
					foreach (string str in compileUnit.ReferencedAssemblies) {
						if (!assemblies.Contains (str))
							assemblies.Add (str);
					}
				}

				((ICodeGenerator) this).GenerateCodeFromCompileUnit (compileUnit, s, new CodeGeneratorOptions ());
				s.Close ();
				f.Close ();
			}
			return CompileAssemblyFromFileBatch (options, fileNames);
		}

		private CompilerResults CompileFromSourceBatch (CompilerParameters options, string[] sources)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			if (sources == null) {
				throw new ArgumentNullException ("sources");
			}

			string[] fileNames = new string[sources.Length];

			for (int i = 0; i < sources.Length; i++) {
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".cs");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				using (StreamWriter s = new StreamWriter (f, Encoding.UTF8)) {
					s.Write (sources[i]);
					s.Close ();
				}
				f.Close ();
			}
			return CompileFromFileBatch (options, fileNames);
		}
	}
}
