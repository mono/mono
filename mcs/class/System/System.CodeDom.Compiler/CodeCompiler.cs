//
// System.CodeDom.Compiler.CodeCompiler.cs
//
// Authors:
//   Jackson Harper (Jackson@LatitudeGeo.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Jackson Harper, All rights reserved
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Permissions;

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

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		private CompilerResults Compile (CompilerParameters options, string[] fileNames, bool keepFiles)
		{
			if (null == options)
				throw new ArgumentNullException ("options");
			if (null == fileNames)
				throw new ArgumentNullException ("fileNames");

			options.TempFiles = new TempFileCollection ();
			foreach (string file in fileNames) {
				options.TempFiles.AddFile (file, keepFiles);
			}
			options.TempFiles.KeepFiles = keepFiles;

			string std_output = String.Empty;
			string err_output = String.Empty;
			string cmd = String.Concat (CompilerName, " ", CmdArgsFromParameters (options));

			CompilerResults results = new CompilerResults (new TempFileCollection ());
			results.NativeCompilerReturnValue = Executor.ExecWaitWithCapture (cmd,
				options.TempFiles, ref std_output, ref err_output);

			string[] compiler_output_lines = std_output.Split (Environment.NewLine.ToCharArray ());
			foreach (string error_line in compiler_output_lines)
				ProcessCompilerOutputLine (results, error_line);

			if (results.Errors.Count == 0)
				results.PathToAssembly = options.OutputAssembly;
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
			int length = sa.Length;
			if (length > 1) {
				for (int i=0; i < length - 1; i++) {
					sb.Append ("\"");
					sb.Append (sa [i]);
					sb.Append ("\"");
					sb.Append (separator);
				}
			}
			if (length > 0) {
				sb.Append ("\"");
				sb.Append (sa [length - 1]);
				sb.Append ("\"");
			}
			return sb.ToString ();
		}

		protected abstract void ProcessCompilerOutputLine (CompilerResults results, string line);

	}
}

