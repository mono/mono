//
// System.CodeDom.Compiler.CodeCompiler.cs
//
// Authors:
//   Jackson Harper (Jackson@LatitudeGeo.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Jackson Harper, All rights reserved
// (C) 2003 Andreas Nahr
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace System.CodeDom.Compiler {

	public abstract class CodeCompiler : CodeGenerator, ICodeCompiler
	{

		protected CodeCompiler ()
		{
		}

		protected abstract string CompilerName {
			get;
		}
	
		protected abstract string FileExtension {
			get;
		}

		protected abstract string CmdArgsFromParameters (CompilerParameters options);

		protected virtual CompilerResults FromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return FromDomBatch (options, new CodeCompileUnit[]{e});
		}
	
		protected virtual CompilerResults FromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			string[] fileNames = new string[ea.Length];
			int i = 0;
			if (options == null)
				options = new CompilerParameters ();
			
			StringCollection assemblies = options.ReferencedAssemblies;

			foreach (CodeCompileUnit e in ea) {
				fileNames[i] = Path.ChangeExtension (Path.GetTempFileName(), FileExtension);
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
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
			return Compile (options, fileNames, false);
		}

		protected virtual CompilerResults FromFile (CompilerParameters options, string fileName)
		{
			return FromFileBatch (options, new string[] {fileName});
		}

		protected virtual CompilerResults FromFileBatch (CompilerParameters options, string[] fileNames)
		{
			return Compile (options, fileNames, true);
		}

		protected virtual CompilerResults FromSource (CompilerParameters options, string source)
		{
			return FromSourceBatch(options, new string[]{source});
		}

		protected virtual CompilerResults FromSourceBatch (CompilerParameters options, string[] sources)
		{
			string[] fileNames = new string[sources.Length];
			int i = 0;
			foreach (string source in sources) {
				fileNames[i] = Path.ChangeExtension (Path.GetTempFileName(), FileExtension);
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f);
				s.Write (source);
				s.Close ();
				f.Close ();
				i++;
			}
			return Compile (options, fileNames, false);
		}

		private CompilerResults Compile (CompilerParameters options, string[] fileNames, bool keepFiles)
		{
			if (null == options)
				throw new ArgumentNullException ("options");
			if (null == fileNames)
				throw new ArgumentNullException ("fileNames");

			options.TempFiles = new TempFileCollection ();
			foreach (string file in fileNames)
			{
				options.TempFiles.AddFile (file, keepFiles);
			}
			options.TempFiles.KeepFiles = keepFiles;

			CompilerResults results = new CompilerResults (new TempFileCollection());

			// FIXME this should probably be done by the System.CodeDom.Compiler.Executor class
			Process compiler = new Process();

			string compiler_output;
			string[] compiler_output_lines;
			compiler.StartInfo.FileName = CompilerName;
			compiler.StartInfo.Arguments = CmdArgsFromParameters (options);
			compiler.StartInfo.CreateNoWindow = true;
			compiler.StartInfo.UseShellExecute = false;
			compiler.StartInfo.RedirectStandardOutput = true;
			try {
				compiler.Start();
				compiler_output = compiler.StandardOutput.ReadToEnd();
				compiler.WaitForExit();
			} 
			finally {
				results.NativeCompilerReturnValue = compiler.ExitCode;
				compiler.Close();
			}

			// END FIXME

			compiler_output_lines = compiler_output.Split(
				System.Environment.NewLine.ToCharArray());
			foreach (string error_line in compiler_output_lines)
				ProcessCompilerOutputLine (results, error_line);
			if (results.Errors.Count == 0)
				results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
			else
				results.CompiledAssembly = null;
			return results;
		}

		[MonoTODO]
		protected virtual string GetResponseFileCmdArgs (CompilerParameters options, string cmdArgs)
		{
			// FIXME I'm not sure what this function should do...
			throw new NotImplementedException ();
		}

		CompilerResults ICodeCompiler.CompileAssemblyFromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return FromDom (options, e);
		}

		CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			return FromDomBatch (options, ea);
		}

		CompilerResults ICodeCompiler.CompileAssemblyFromFile (CompilerParameters options, string fileName)
		{
			return FromFile (options, fileName);
		}

		CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			return FromFileBatch (options, fileNames);
		}


		CompilerResults ICodeCompiler.CompileAssemblyFromSource (CompilerParameters options, string source)
		{
			return FromSource (options, source);
		}


		CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch (CompilerParameters options, string[] sources)
		{
			return FromSourceBatch (options, sources);
		}

		protected static string JoinStringArray (string[] sa, string separator)
		{
			StringBuilder sb = new StringBuilder ();

			foreach (string s in sa)
				sb.Append (s + separator);
			return sb.ToString ();
		}

		protected abstract void ProcessCompilerOutputLine (CompilerResults results, string line);

	}
}

