//
// System.Web.Compilation.CompilationResult
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Web.Compilation
{
	internal class CompilationResult
	{
		int exitCode;
		string output;
		string outputFile;
		object data;
		
		public CompilationResult ()
		{
		}

		public void Reset ()
		{
			exitCode = 0;
			output = null;
		}

		public void CopyFrom (CompilationResult other)
		{
			exitCode = other.ExitCode;
			output = other.output;
			outputFile = other.outputFile;
			data = other.data;
		}
		
		public int ExitCode
		{
			get { return exitCode; }
			set { exitCode = exitCode; }
		}
		
		public string CompilerOutput
		{
			get { return output; }
			set { output = value; }
		}

		public string OutputFile
		{
			get { return outputFile; }
			set { outputFile = value; }
		}

		public object Data
		{
			get { return data; }
			set { data = value; }
		}

		public override string ToString ()
		{
			return String.Format ("CompilationResult: {0} {1} {2} {3}", exitCode, output, outputFile, data);
		}
	}
}

