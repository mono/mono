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

namespace Microsoft.VisualBasic
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

	internal class VBCodeCompiler: VBCodeGenerator, ICodeCompiler
	{
		static string windowsMonoPath;
		static string windowsMbasPath;
		static VBCodeCompiler ()
		{
			if (Path.DirectorySeparatorChar == '\\') {
				// FIXME: right now we use "fixed" version 1.0
				// mcs at any time.
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
				windowsMbasPath =
					Path.Combine (p, "1.0\\mbas.exe");
			}
		}

		//
		// Constructors
		//
		public VBCodeCompiler()
		{
		}

		//
		// Methods
		//
		[MonoTODO]
		public CompilerResults CompileAssemblyFromDom (CompilerParameters options,CodeCompileUnit e)
		{
			return CompileAssemblyFromDomBatch (options, new CodeCompileUnit []{e});
		}

		public CompilerResults CompileAssemblyFromDomBatch (CompilerParameters options,
								    CodeCompileUnit [] ea)
		{
			string [] fileNames = new string [ea.Length];
			int i = 0;
			if (options == null)
			options = new CompilerParameters ();

			StringCollection assemblies = options.ReferencedAssemblies;

			foreach (CodeCompileUnit e in ea) {
				fileNames [i] = GetTempFileNameWithExtension (options.TempFiles, "vb");
				FileStream f = new FileStream (fileNames [i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f);
				if (e.ReferencedAssemblies != null) {
					foreach (string str in e.ReferencedAssemblies) {
						if (!assemblies.Contains (str))
							assemblies.Add (str);
					}
				}

				((ICodeGenerator)this).GenerateCodeFromCompileUnit (e, s, new CodeGeneratorOptions());
				s.Close();
				f.Close();
				i++;
			}
			return CompileAssemblyFromFileBatch (options, fileNames);
		}

		public CompilerResults CompileAssemblyFromFile (CompilerParameters options, string fileName)
		{
			return CompileAssemblyFromFileBatch (options, new string []{fileName});
		}

		public CompilerResults CompileAssemblyFromFileBatch (CompilerParameters options,
								     string [] fileNames)
		{
			if (null == options)
				throw new ArgumentNullException ("options");

			if (null == fileNames)
				throw new ArgumentNullException ("fileNames");

			CompilerResults results = new CompilerResults (options.TempFiles);
			Process mbas = new Process ();

			string mbas_output;
			string [] mbas_output_lines;
			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				mbas.StartInfo.FileName = windowsMonoPath;
				mbas.StartInfo.Arguments = windowsMbasPath + ' ' + BuildArgs (options, fileNames);
			}
			else {
				mbas.StartInfo.FileName = "mbas";
				mbas.StartInfo.Arguments = BuildArgs (options,fileNames);
			}
			mbas.StartInfo.CreateNoWindow = true;
			mbas.StartInfo.UseShellExecute = false;
			mbas.StartInfo.RedirectStandardOutput = true;
			try {
				mbas.Start();
				mbas_output = mbas.StandardOutput.ReadToEnd ();
				mbas.WaitForExit();
			} finally {
				results.NativeCompilerReturnValue = mbas.ExitCode;
				mbas.Close ();
			}

			mbas_output_lines = mbas_output.Split(Environment.NewLine.ToCharArray());
			bool loadIt=true;
			foreach (string error_line in mbas_output_lines) {
				CompilerError error = CreateErrorFromString (error_line);
				if (null != error) {
					results.Errors.Add (error);
					if (!error.IsWarning)
						loadIt = false;
				}
			}

			if (loadIt)
				results.CompiledAssembly=Assembly.LoadFrom(options.OutputAssembly);
			else
				results.CompiledAssembly=null;

			return results;
		}

		public CompilerResults CompileAssemblyFromSource (CompilerParameters options,
								  string source)
		{
			return CompileAssemblyFromSourceBatch (options, new string [] {source});
		}

		public CompilerResults CompileAssemblyFromSourceBatch (CompilerParameters options,
									string [] sources)
		{
			string [] fileNames = new string [sources.Length];
			int i = 0;
			foreach (string source in sources) {
				fileNames [i] = GetTempFileNameWithExtension (options.TempFiles, "vb");
				FileStream f = new FileStream (fileNames [i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f);
				s.Write (source);
				s.Close ();
				f.Close ();
				i++;
			}
			return CompileAssemblyFromFileBatch(options,fileNames);
		}

		static string BuildArgs (CompilerParameters options, string [] fileNames)
		{
			StringBuilder args = new StringBuilder ();
			args.AppendFormat ("/quiet ");
			if (options.GenerateExecutable)
				args.AppendFormat("/target:exe ");
			else
				args.AppendFormat("/target:library ");

			/* Disabled. It causes problems now. -- Gonzalo
			if (options.IncludeDebugInformation)
				args.AppendFormat("/debug ");
			*/

			if (options.TreatWarningsAsErrors)
				args.AppendFormat ("/warnaserror ");

			if (options.WarningLevel != -1)
				args.AppendFormat ("/wlevel:{0} ", options.WarningLevel);

			if (options.OutputAssembly == null) {
				string ext = (options.GenerateExecutable ? "exe" : "dll");
				options.OutputAssembly = GetTempFileNameWithExtension (options.TempFiles, ext);
			}

			args.AppendFormat ("/out:\"{0}\" ", options.OutputAssembly);

			bool Reference2MSVBFound;
			Reference2MSVBFound = false;
			if (null != options.ReferencedAssemblies) 
			{
				foreach (string import in options.ReferencedAssemblies)
				{
					if (string.Compare (import, "Microsoft.VisualBasic", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
						Reference2MSVBFound = true;
					args.AppendFormat ("/r:\"{0}\" ", import);
				}
			}
			// add standard import to Microsoft.VisualBasic if missing
			if (Reference2MSVBFound == false)
				args.AppendFormat ("/r:\"{0}\" ", "Microsoft.VisualBasic");

			args.AppendFormat(" -- "); // makes mbas not try to process filenames as options

			foreach (string source in fileNames)
				args.AppendFormat("\"{0}\" ",source);

			return args.ToString();
		}

		static CompilerError CreateErrorFromString (string error_string)
		{
			// When IncludeDebugInformation is true, prevents the debug symbols stats from braeking this.
			if (error_string.StartsWith ("WROTE SYMFILE") || error_string.StartsWith ("OffsetTable"))
				return null;

			CompilerError error = new CompilerError ();
			Regex reg = new Regex (@"^(\s*(?<file>.*)\((?<line>\d*)(,(?<column>\d*))?\)\s+)*" +
						@"(?<level>error|warning)\s*(?<number>.*):\s(?<message>.*)",
						RegexOptions.Compiled | RegexOptions.ExplicitCapture);

			Match match = reg.Match (error_string);
			if (!match.Success)
				return null;

			if (String.Empty != match.Result("${file}"))
				error.FileName = match.Result ("${file}");

			if (String.Empty != match.Result ("${line}"))
				error.Line = Int32.Parse (match.Result ("${line}"));

			if (String.Empty != match.Result( "${column}"))
				error.Column = Int32.Parse (match.Result ("${column}"));

			if (match.Result ("${level}") =="warning")
				error.IsWarning = true;

			error.ErrorNumber = match.Result ("${number}");
			error.ErrorText = match.Result ("${message}");
			return error;
		}

		static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension)
		{
			return temp_files.AddExtension (extension);
		}
	}
}

