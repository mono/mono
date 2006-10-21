//
// Microsoft.JScript JScriptCodeCompiler Class implementation
//
// Authors:
//  akiramei (mei@work.email.ne.jp)
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

namespace Microsoft.JScript
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	internal class JScriptCodeCompiler : JScriptCodeGenerator, ICodeCompiler
	{
		static string windowsMjsPath;
		static string windowsMonoPath;

		static JScriptCodeCompiler ()
		{
			if (Path.DirectorySeparatorChar == '\\') {
				PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", 
										    BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo get_gac = gac.GetGetMethod (true);

				string p = Path.GetDirectoryName ((string) get_gac.Invoke (null, null));

				windowsMonoPath = Path.Combine (Path.GetDirectoryName (Path.GetDirectoryName (p)), "bin\\mono.bat");

				if (!File.Exists (windowsMonoPath))
					windowsMonoPath = Path.Combine (Path.GetDirectoryName (Path.GetDirectoryName (p)),
								"bin\\mono.exe");

				if (!File.Exists (windowsMonoPath))
					windowsMonoPath = Path.Combine (Path.GetDirectoryName (Path.GetDirectoryName (
												      Path.GetDirectoryName (p))),
								"mono\\mono\\mini\\mono.exe");

				if (!File.Exists (windowsMonoPath))
					throw new FileNotFoundException ("Windows mono path not found: " + windowsMonoPath);

				windowsMjsPath = Path.Combine (p, "1.0\\mjs.exe");

				if (!File.Exists (windowsMjsPath))
					windowsMjsPath = Path.Combine(Path.GetDirectoryName (p), "lib\\default\\mjs.exe");

				if (!File.Exists (windowsMjsPath))
					throw new FileNotFoundException ("Windows mjs path not found: " + windowsMjsPath);
			}
		}

		//
		// Constructors
		//
		public JScriptCodeCompiler()
		{
		}

		//
		// Methods
		//
		public CompilerResults CompileAssemblyFromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return CompileAssemblyFromDomBatch (options, new CodeCompileUnit [] { e });
		}

		public CompilerResults CompileAssemblyFromDomBatch (CompilerParameters options, CodeCompileUnit [] ea)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			try {
				return CompileFromDomBatch (options, ea);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromFile (CompilerParameters options, string fileName)
		{
			return CompileAssemblyFromFileBatch (options, new string [] { fileName });
		}

		public CompilerResults CompileAssemblyFromFileBatch (CompilerParameters options, string [] fileNames)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			try {
				return CompileFromFileBatch (options, fileNames);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromSource (CompilerParameters options, string source)
		{
			return CompileAssemblyFromSourceBatch (options, new string [] { source });
		}

		public CompilerResults CompileAssemblyFromSourceBatch (CompilerParameters options, string [] sources)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			try {
				return CompileFromSourceBatch (options, sources);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		private CompilerResults CompileFromFileBatch (CompilerParameters options, string [] fileNames)
		{
			if (null == options)
				throw new ArgumentNullException ("options");

			if (null == fileNames)
				throw new ArgumentNullException ("fileNames");

			CompilerResults results = new CompilerResults (options.TempFiles);
			Process mjs = new Process();

			string mjs_output;
			string mjs_stdout;
			string[] mjs_output_lines;

			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				mjs.StartInfo.FileName = windowsMonoPath;
				mjs.StartInfo.Arguments = "\"" + windowsMjsPath + "\" " + BuildArgs (options, fileNames);
			} else {
				mjs.StartInfo.FileName= "mjs";
				mjs.StartInfo.Arguments= BuildArgs (options,fileNames);
			}

			mjs.StartInfo.CreateNoWindow = true;
			mjs.StartInfo.UseShellExecute = false;
			mjs.StartInfo.RedirectStandardOutput = true;
			mjs.StartInfo.RedirectStandardError = true;

			try {
				mjs.Start ();

				// If there are a few kB in stdout, we might lock
				mjs_output = mjs.StandardError.ReadToEnd ();
				mjs_stdout = mjs.StandardOutput.ReadToEnd ();
				mjs.WaitForExit ();
				results.NativeCompilerReturnValue = mjs.ExitCode;
			} finally {
				mjs.Close ();
			}
			mjs_output_lines = mjs_output.Split (System.Environment.NewLine.ToCharArray ());
			bool loadIt = true;

			foreach (string error_line in mjs_output_lines) {
				CompilerError error = CreateErrorFromString (error_line);

				if (null!=error) {
					results.Errors.Add (error);

					if (!error.IsWarning)
						loadIt = false;
				}
			}

			if (loadIt) {
				if (!File.Exists (options.OutputAssembly)) {
					throw new Exception ("Compiler failed to produce the assembly. Stderr='"
						+ mjs_output + "', Stdout='" + mjs_stdout + "'");
				}

				if (options.GenerateInMemory) {
					using (FileStream fs = File.OpenRead (options.OutputAssembly)) {
						byte [] buffer = new byte [fs.Length];
						fs.Read (buffer, 0, buffer.Length);
						results.CompiledAssembly = Assembly.Load (buffer, null, options.Evidence);
						fs.Close ();
					}
				} else {
					results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
					results.PathToAssembly = options.OutputAssembly;
				}
			} else
				results.CompiledAssembly = null;
			return results;
		}

		private static string BuildArgs (CompilerParameters options, string [] fileNames)
		{
			StringBuilder args = new StringBuilder ();

#if false
			if (options.GenerateExecutable)
				; // args.Append ("/target:exe ");
			else
				; //  args.Append ("/target:library ");

			if (options.Win32Resource != null)
				; //  args.AppendFormat ("/win32res:\"{0}\" ", options.Win32Resource);

			if (options.IncludeDebugInformation)
				; // args.Append ("/debug+ /optimize- ");
			else
				; //  args.Append ("/debug- /optimize+ ");

			if (options.TreatWarningsAsErrors)
				; //  args.Append ("/warnaserror ");

			if (options.WarningLevel >= 0)
				; //  args.AppendFormat ("/warn:{0} ", options.WarningLevel);
#endif

			if (options.OutputAssembly == null)
				options.OutputAssembly = GetTempFileNameWithExtension (options.TempFiles, "dll", 
									       !options.GenerateInMemory);
			args.AppendFormat ("/out:\"{0}\" ",options.OutputAssembly);

			if (null != options.ReferencedAssemblies) {
				foreach (string import in options.ReferencedAssemblies) {
					if (import == null || import.Length == 0)
						continue;

					args.AppendFormat("/r:\"{0}\" ",import);
				}
			}

			if (options.CompilerOptions != null) {
				args.Append (options.CompilerOptions);
				args.Append (" ");
			}
			
			args.Append (" -- ");

			foreach (string source in fileNames)
				args.AppendFormat("\"{0}\" ", source);

			return args.ToString ();
		}

		private static CompilerError CreateErrorFromString (string error_string)
		{
			if (error_string == null || error_string == "")
				return null;

			CompilerError error = new CompilerError ();
			Regex reg = new Regex (@"^(\s*(?<file>.*)\((?<line>\d*)(,(?<column>\d*))?\)(:)?\s+)*(?<level>\w+)\s*(?<number>.*):\s(?<message>.*)",
				RegexOptions.Compiled | RegexOptions.ExplicitCapture);

			Match match = reg.Match (error_string);

			if (!match.Success)
				return null;

			if (String.Empty != match.Result ("${file}"))
				error.FileName = match.Result ("${file}");

			if (String.Empty != match.Result ("${line}"))
				error.Line = Int32.Parse (match.Result ("${line}"));

			if (String.Empty != match.Result ("${column}"))
				error.Column = Int32.Parse (match.Result ("${column}"));

			string level = match.Result ("${level}");

			if (level == "warning")
				error.IsWarning = true;
			else if (level != "error")
				return null; // error CS8028 will confuse the regex.

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

		private CompilerResults CompileFromDomBatch (CompilerParameters options, CodeCompileUnit [] ea)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (ea == null)
				throw new ArgumentNullException ("ea");

			string [] fileNames = new string [ea.Length];
			StringCollection assemblies = options.ReferencedAssemblies;

			for (int i = 0; i < ea.Length; i++) {
				CodeCompileUnit compileUnit = ea [i];
				fileNames [i] = GetTempFileNameWithExtension (options.TempFiles, i + ".js");
				FileStream f = new FileStream (fileNames [i], FileMode.OpenOrCreate);
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

		private CompilerResults CompileFromSourceBatch (CompilerParameters options, string [] sources)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (sources == null)
				throw new ArgumentNullException ("sources");

			string [] fileNames = new string [sources.Length];

			for (int i = 0; i < sources.Length; i++) {
				fileNames [i] = GetTempFileNameWithExtension (options.TempFiles, i + ".js");
				FileStream f = new FileStream (fileNames [i], FileMode.OpenOrCreate);
				using (StreamWriter s = new StreamWriter (f, Encoding.UTF8)) {
					s.Write (sources [i]);
					s.Close ();
				}
				f.Close ();
			}
			return CompileFromFileBatch (options, fileNames);
		}
	}
}
