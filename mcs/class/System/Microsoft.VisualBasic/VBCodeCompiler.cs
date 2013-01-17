//
// Microsoft VisualBasic VBCodeCompiler Class implementation
//
// Authors:
//	Jochen Wezel (jwezel@compumaster.de)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Jochen Wezel (http://www.compumaster.de)
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//
// Modifications:
// 2003-11-28 JW: create reference to Microsoft.VisualBasic if not explicitly done

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
	internal class VBCodeCompiler : VBCodeGenerator, ICodeCompiler
	{
		static string windowsMonoPath;
		static string windowsvbncPath;

		static VBCodeCompiler ()
		{
			if (Path.DirectorySeparatorChar == '\\') {
				PropertyInfo gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);
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
				windowsvbncPath =
					Path.Combine (p, "2.0\\vbnc.exe");
			}
		}

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
			/* Disabled, vbnc does not support this.
			args.Append (" -- "); // makes vbnc not try to process filenames as options
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

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension)
		{
			return temp_files.AddExtension (extension);
		}

		private CompilerResults CompileFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			if (fileNames == null) {
				throw new ArgumentNullException ("fileNames");
			}

			CompilerResults results = new CompilerResults (options.TempFiles);
			Process vbnc = new Process ();

			string vbnc_output = "";
			string[] vbnc_output_lines;
			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				vbnc.StartInfo.FileName = windowsMonoPath;
				vbnc.StartInfo.Arguments = windowsvbncPath + ' ' + BuildArgs (options, fileNames);
			} else {
				vbnc.StartInfo.FileName = "vbnc";
				vbnc.StartInfo.Arguments = BuildArgs (options, fileNames);
			}
			//Console.WriteLine (vbnc.StartInfo.Arguments);
			vbnc.StartInfo.CreateNoWindow = true;
			vbnc.StartInfo.UseShellExecute = false;
			vbnc.StartInfo.RedirectStandardOutput = true;
			try {
				vbnc.Start ();
			} catch (Exception e) {
				Win32Exception exc = e as Win32Exception;
				if (exc != null) {
					throw new SystemException (String.Format ("Error running {0}: {1}", vbnc.StartInfo.FileName,
											Win32Exception.W32ErrorMessage (exc.NativeErrorCode)));
				}
				throw;
			}

			try {
				vbnc_output = vbnc.StandardOutput.ReadToEnd ();
				vbnc.WaitForExit ();
			} finally {
				results.NativeCompilerReturnValue = vbnc.ExitCode;
				vbnc.Close ();
			}

			bool loadIt = true;
			if (results.NativeCompilerReturnValue == 1) {
				loadIt = false;
				vbnc_output_lines = vbnc_output.Split (Environment.NewLine.ToCharArray ());
				foreach (string error_line in vbnc_output_lines) {
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
				CompilerError error = new CompilerError (string.Empty, 0, 0, "VBNC_CRASH", vbnc_output);
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
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".vb");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f);
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
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".vb");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (sources[i]);
					s.Close ();
				}
				f.Close ();
			}
			return CompileFromFileBatch (options, fileNames);
		}
	}
}

