//
// VBCodeGenerator.cs:
//
// Authors:
//	Jochen Wezel (jwezel@compumaster.de)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Jochen Wezel (http://www.compumaster.de)
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.VisualBasic
{
	partial class VBCodeGenerator
	{
		protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			if (fileNames == null)
				throw new ArgumentNullException(nameof(fileNames));

			CompilerResults results = new CompilerResults (options.TempFiles);
			Process vbc = new Process ();

			string vbnc_output = "";
			string[] vbc_output_lines;
			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				vbc.StartInfo.FileName = MonoToolsLocator.Mono;
				vbc.StartInfo.Arguments = MonoToolsLocator.VBCompiler + ' ' + BuildArgs (options, fileNames);
			} else {
				vbc.StartInfo.FileName = MonoToolsLocator.VBCompiler;
				vbc.StartInfo.Arguments = BuildArgs (options, fileNames);
			}
			//Console.WriteLine (vbnc.StartInfo.Arguments);
			vbc.StartInfo.CreateNoWindow = true;
			vbc.StartInfo.UseShellExecute = false;
			vbc.StartInfo.RedirectStandardOutput = true;
			try {
				vbc.Start ();
			} catch (Exception e) {
				Win32Exception exc = e as Win32Exception;
				if (exc != null) {
					throw new SystemException (String.Format ("Error running {0}: {1}", vbc.StartInfo.FileName,
											Win32Exception.GetErrorMessage (exc.NativeErrorCode)));
				}
				throw;
			}

			try {
				vbc_output = vbc.StandardOutput.ReadToEnd ();
				vbc.WaitForExit ();
			} finally {
				results.NativeCompilerReturnValue = vbc.ExitCode;
				vbc.Close ();
			}

			bool loadIt = true;
			if (results.NativeCompilerReturnValue == 1) {
				loadIt = false;
				vbc_output_lines = vbc_output.Split (Environment.NewLine.ToCharArray ());
				foreach (string error_line in vbc_output_lines) {
					CompilerError error = CreateErrorFromString (error_line);
					if (null != error) {
						results.Errors.Add (error);
					}
				}
			}
			
			if ((loadIt == false && !results.Errors.HasErrors) // Failed, but no errors? Probably couldn't parse the compiler output correctly. 
				|| (results.NativeCompilerReturnValue != 0 && results.NativeCompilerReturnValue != 1)) // Neither success (0), nor failure (1), so it crashed. 
			{
				// Show the entire output as one big error message.
				loadIt = false;
				CompilerError error = new CompilerError (string.Empty, 0, 0, "VBCCRASH", vbncoutput);
				results.Errors.Add (error);
			};

			if (loadIt) {
				if (options.GenerateInMemory) {
					using (FileStream fs = File.OpenRead (options.OutputAssembly)) {
						byte[] buffer = new byte[fs.Length];
						fs.Read (buffer, 0, buffer.Length);
						results.CompiledAssembly = Assembly.Load (buffer, null);
						fs.Close ();
					}
				} else {
					results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
					results.PathToAssembly = options.OutputAssembly;
				}
			} else {
				results.CompiledAssembly = null;
			}

			return results;
		}

		static string BuildArgs (CompilerParameters options, string[] fileNames)
		{
			StringBuilder args = new StringBuilder ();
			args.Append ("/quiet ");
			if (options.GenerateExecutable)
				args.Append ("/target:exe ");
			else
				args.Append ("/target:library ");

			/* Disabled. It causes problems now. -- Gonzalo
			if (options.IncludeDebugInformation)
				args.AppendFormat("/debug ");
			*/

			if (options.TreatWarningsAsErrors)
				args.Append ("/warnaserror ");

			/* Disabled. vbnc does not support warninglevels.
			if (options.WarningLevel != -1)
				args.AppendFormat ("/wlevel:{0} ", options.WarningLevel);
			*/

			if (options.OutputAssembly == null || options.OutputAssembly.Length == 0) {
				string ext = (options.GenerateExecutable ? "exe" : "dll");
				options.OutputAssembly = GetTempFileNameWithExtension (options.TempFiles, ext, !options.GenerateInMemory);
			}

			args.AppendFormat ("/out:\"{0}\" ", options.OutputAssembly);

			bool Reference2MSVBFound;
			Reference2MSVBFound = false;
			if (null != options.ReferencedAssemblies) {
				foreach (string import in options.ReferencedAssemblies) {
					if (string.Compare (import, "Microsoft.VisualBasic", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
						Reference2MSVBFound = true;
					args.AppendFormat ("/r:\"{0}\" ", import);
				}
			}
			
			// add standard import to Microsoft.VisualBasic if missing
			if (!Reference2MSVBFound)
				args.Append ("/r:\"Microsoft.VisualBasic.dll\" ");

			if (options.CompilerOptions != null) {
				args.Append (options.CompilerOptions);
				args.Append (" ");
			}
			/* Disabled, vbc does not support this.
			args.Append (" -- "); // makes vbc not try to process filenames as options
			*/
			foreach (string source in fileNames)
				args.AppendFormat (" \"{0}\" ", source);

			return args.ToString ();
		}

		static CompilerError CreateErrorFromString (string error_string)
		{
			CompilerError error = new CompilerError ();
			Regex reg = new Regex (@"^(\s*(?<file>.*)?\((?<line>\d*)(,(?<column>\d*))?\)\s+)?:\s*" +
						@"(?<level>Error|Warning)?\s*(?<number>.*):\s(?<message>.*)",
						RegexOptions.Compiled | RegexOptions.ExplicitCapture);

			Match match = reg.Match (error_string);
			if (!match.Success) {
				return null;
			}

			if (String.Empty != match.Result ("${file}"))
				error.FileName = match.Result ("${file}").Trim ();

			if (String.Empty != match.Result ("${line}"))
				error.Line = Int32.Parse (match.Result ("${line}"));

			if (String.Empty != match.Result ("${column}"))
				error.Column = Int32.Parse (match.Result ("${column}"));

			if (match.Result ("${level}").Trim () == "Warning")
				error.IsWarning = true;

			error.ErrorNumber = match.Result ("${number}");
			error.ErrorText = match.Result ("${message}");
			
			return error;
		}

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension, bool keepFile)
		{
			return temp_files.AddExtension (extension, keepFile);
		}
	}
}
