//
// Mono.CSharp CSharpCodeCompiler Class implementation
//
// Author:
//   Sean Kasun (seank@users.sf.net)
//

namespace Mono.CSharp
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	internal class CSharpCodeCompiler : CSharpCodeGenerator, ICodeCompiler
	{
		//
		// Constructors
		//
		public CSharpCodeCompiler()
		{
		}

		//
		// Methods
		//
		[MonoTODO]
		public CompilerResults CompileAssemblyFromDom (
			CompilerParameters options,CodeCompileUnit e)
		{
			return CompileAssemblyFromDomBatch(options,new CodeCompileUnit[]{e});
		}
		public CompilerResults CompileAssemblyFromDomBatch (
			CompilerParameters options,CodeCompileUnit[] ea)
		{
			string[] fileNames=new string[ea.Length];
			int i=0;
			foreach (CodeCompileUnit e in ea)
			{
				fileNames[i]=Path.ChangeExtension(Path.GetTempFileName(),"cs");
				FileStream f=new FileStream(fileNames[i],FileMode.OpenOrCreate);
				StreamWriter s=new StreamWriter(f);

				GenerateCodeFromCompileUnit(e,s,new CodeGeneratorOptions());
				s.Close();
				f.Close();
				i++;
			}
			return CompileAssemblyFromFileBatch(options,fileNames);
		}
		public CompilerResults CompileAssemblyFromFile (
			CompilerParameters options,string fileName)
		{
			return CompileAssemblyFromFileBatch(options,new string[]{fileName});
		}
		public CompilerResults CompileAssemblyFromFileBatch (
			CompilerParameters options,string[] fileNames)
		{
			if (null == options)
				throw new ArgumentNullException("options");
			if (null == fileNames)
				throw new ArgumentNullException("fileNames");

			CompilerResults results=new CompilerResults(options.TempFiles);
			Process mcs=new Process();

			string mcs_output;
			string[] mcs_output_lines;
			mcs.StartInfo.FileName="mcs";
			mcs.StartInfo.Arguments=BuildArgs(options,fileNames);
			mcs.StartInfo.CreateNoWindow=true;
			mcs.StartInfo.UseShellExecute=false;
			mcs.StartInfo.RedirectStandardOutput=true;
			try {
				mcs.Start();
				mcs_output=mcs.StandardOutput.ReadToEnd();
				mcs.WaitForExit();
			} finally {
				mcs.Close();
			}
			mcs_output_lines=mcs_output.Split(
				System.Environment.NewLine.ToCharArray());
			bool loadIt=true;
			foreach (string error_line in mcs_output_lines)
			{
				CompilerError error=CreateErrorFromString(error_line);
				if (null!=error)
				{
					results.Errors.Add(error);
					if (!error.IsWarning) loadIt=false;
				}
			}
			if (loadIt)
				results.CompiledAssembly=Assembly.LoadFrom(options.OutputAssembly);
			else
				results.CompiledAssembly=null;
			return results;
		}
		public CompilerResults CompileAssemblyFromSource (
			CompilerParameters options,string source)
		{
			return CompileAssemblyFromSourceBatch(options,new string[]{source});
		}
		public CompilerResults CompileAssemblyFromSourceBatch (
			CompilerParameters options,string[] sources)
		{
			string[] fileNames=new string[sources.Length];
			int i=0;
			foreach (string source in sources)
			{
				fileNames[i]=Path.ChangeExtension(Path.GetTempFileName(),"cs");
				FileStream f=new FileStream(fileNames[i],FileMode.OpenOrCreate);
				StreamWriter s=new StreamWriter(f);
				s.Write(source);
				s.Close();
				f.Close();
				i++;
			}
			return CompileAssemblyFromFileBatch(options,fileNames);
		}
		private static string BuildArgs(
			CompilerParameters options,string[] fileNames)
		{
			StringBuilder args=new StringBuilder();
			if (options.GenerateExecutables)
				args.AppendFormat("/target:exe ");
			else
				args.AppendFormat("/target:library ");
			if (options.IncludeDebugInformation)
				args.AppendFormat("/debug ");
			if (options.TreatWarningsAsErrors)
				args.AppendFormat("/warnaserror ");
			args.AppendFormat("/warn:{0} ",options.WarningLevel);
			if (options.OutputAssembly==null)
				options.OutputAssembly=Path.ChangeExtension(Path.GetTempFileName(),"dll");
			args.AppendFormat("/out:{0} ",options.OutputAssembly);
			if (null != options.ReferencedAssemblies)
			{
				foreach (string import in options.ReferencedAssemblies)
					args.AppendFormat("/r:'{0}' ",import);
			}
			foreach (string source in fileNames)
				args.AppendFormat("'{0}' ",source);
			return args.ToString();
		}
		private static CompilerError CreateErrorFromString(string error_string)
		{
			CompilerError error=new CompilerError();
			Regex reg = new Regex (@"^(\s*(?<file>.*)\((?<line>\d*)(,(?<column>\d*))?\)\s+)*(?<level>\w+)\s*(?<number>.*):\s(?<message>.*)",
				RegexOptions.Compiled | RegexOptions.ExplicitCapture);
			Match match=reg.Match(error_string);
			if (!match.Success) return null;
			if (String.Empty != match.Result("${file}"))
				error.FileName=match.Result("${file}");
			if (String.Empty != match.Result("${line}"))
				error.Line=Int32.Parse(match.Result("${line}"));
			if (String.Empty != match.Result("${column}"))
				error.Column=Int32.Parse(match.Result("${column}"));
			if (match.Result("${level}")=="warning")
				error.IsWarning=true;
			error.ErrorNumber=match.Result("${number}");
			error.ErrorText=match.Result("${message}");
			return error;
		}
	}
}
