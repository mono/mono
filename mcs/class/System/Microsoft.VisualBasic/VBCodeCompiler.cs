//
// Microsoft VisualBasic VBCodeCompiler Class implementation
//
// Authors:
// 	Jochen Wezel (jwezel@compumaster.de)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Jochen Wezel
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
				fileNames [i] = GetTempFileNameWithExtension ("vb");
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

		public CompilerResults CompileAssemblyFromFile (CompilerParameters options,string fileName)
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
			mbas.StartInfo.FileName = "mbas";
			mbas.StartInfo.Arguments = BuildArgs(options,fileNames);
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
				fileNames [i] = GetTempFileNameWithExtension ("vb");
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

			if (options.OutputAssembly == null)
				options.OutputAssembly = GetTempFileNameWithExtension ("dll");

			args.AppendFormat ("/out:\"{0}\" ", options.OutputAssembly);
			if (null != options.ReferencedAssemblies) {
				foreach (string import in options.ReferencedAssemblies)
					args.AppendFormat ("/r:\"{0}\" ", import);
			}

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
						@"(?<level>\w+)\s*(?<number>.*):\s(?<message>.*)",
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

			if (match.Result ("${level}") ==" warning")
				error.IsWarning = true;

			error.ErrorNumber = match.Result ("${number}");
			error.ErrorText = match.Result ("${message}");
			return error;
		}

		static string GetTempFileNameWithExtension (string extension)
		{
			Exception exc;
			string extFile;

			do {
				string tmpFile = Path.GetTempFileName ();
				FileInfo fileInfo = new FileInfo (tmpFile);
				extFile = Path.ChangeExtension (tmpFile, extension);
				try {
					fileInfo.MoveTo (extFile);
					exc = null;
				} catch (Exception e) {
					exc = e;
				}
			} while (exc != null);

			return extFile;
		}
	}
}

