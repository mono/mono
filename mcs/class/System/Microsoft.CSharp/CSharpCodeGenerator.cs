//
// CSharpCodeGenerator:
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

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CSharp
{
	partial class CSharpCodeGenerator
	{
		private CompilerResults FromFileBatch (CompilerParameters options, string[] fileNames)
		{
			if (options == null)
				throw new ArgumentNullException (nameof(options));

			if (fileNames == null)
				throw new ArgumentNullException (nameof(fileNames));

			CompilerResults results=new CompilerResults(options.TempFiles);
			Process mcs=new Process();

			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				mcs.StartInfo.FileName = MonoToolsLocator.Mono;
				mcs.StartInfo.Arguments = "\"" + MonoToolsLocator.McsCSharpCompiler + "\" ";
			} else {
				mcs.StartInfo.FileName = MonoToolsLocator.McsCSharpCompiler;
			}

			mcs.StartInfo.Arguments += BuildArgs (options, fileNames, _provOptions);

			var stderr_completed = new ManualResetEvent (false);
			var stdout_completed = new ManualResetEvent (false);
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
			mcs.StartInfo.EnvironmentVariables.Remove ("MONO_GC_PARAMS");

#if XAMMAC_4_5
			/*/
			 * reset MONO_CFG_DIR - we don't want to propagate the current config to another mono
			 * since it's specific to the XM application and won't work on system mono.
			 */
			mcs.StartInfo.EnvironmentVariables.Remove ("MONO_CFG_DIR");
#endif

			mcs.StartInfo.CreateNoWindow=true;
			mcs.StartInfo.UseShellExecute=false;
			mcs.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			mcs.StartInfo.RedirectStandardOutput=true;
			mcs.StartInfo.RedirectStandardError=true;
			mcs.ErrorDataReceived += new DataReceivedEventHandler ((sender, args) => {
				if (args.Data != null)
					results.Output.Add (args.Data);
				else
					stderr_completed.Set ();
			});
			mcs.OutputDataReceived += new DataReceivedEventHandler ((sender, args) => {
					if (args.Data == null)
						stdout_completed.Set ();
				});

			// Use same text decoder as mcs and not user set values in Console
			mcs.StartInfo.StandardOutputEncoding =
			mcs.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			
			try {
				mcs.Start();
			} catch (Exception e) {
				Win32Exception exc = e as Win32Exception;
				if (exc != null) {
					throw new SystemException (String.Format ("Error running {0}: {1}", mcs.StartInfo.FileName,
									Win32Exception.GetErrorMessage (exc.NativeErrorCode)));
				}
				throw;
			}

			try {
				mcs.BeginOutputReadLine ();
				mcs.BeginErrorReadLine ();
				mcs.WaitForExit();
				
				results.NativeCompilerReturnValue = mcs.ExitCode;
			} finally {
				stderr_completed.WaitOne (TimeSpan.FromSeconds (30));
				stdout_completed.WaitOne (TimeSpan.FromSeconds (30));
				mcs.Close();
			}

 			bool loadIt=true;
			foreach (string error_line in results.Output) {
				CompilerError error = CreateErrorFromString (error_line);
				if (error != null) {
					results.Errors.Add (error);
					if (!error.IsWarning)
						loadIt = false;
				}
			}
			
			if (results.Output.Count > 0) {
				results.Output.Insert (0, mcs.StartInfo.FileName + " " + mcs.StartInfo.Arguments + Environment.NewLine);
			}

			if (loadIt) {
				if (!File.Exists (options.OutputAssembly)) {
					StringBuilder sb = new StringBuilder ();
					foreach (string s in results.Output)
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

		private static string BuildArgs (CompilerParameters options, string[] fileNames, IDictionary<string, string> providerOptions)
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

			args.Append ("/noconfig ");

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

		static readonly Regex RelatedSymbolsRegex = new Regex(
			@"
            \(Location\ of\ the\ symbol\ related\ to\ previous\ (warning|error)\)
			",
			RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

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
				match = RelatedSymbolsRegex.Match (error_string);
				if (!match.Success) {
					// We had some sort of runtime crash
					error.ErrorText = error_string;
					error.IsWarning = false;
					error.ErrorNumber = "";
					return error;
				} else {
					// This line is a continuation of previous warning of error
					return null;
				}
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
	}
}
