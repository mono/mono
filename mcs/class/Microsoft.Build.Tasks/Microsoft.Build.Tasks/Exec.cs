//
// Exec.cs: Task that executes commands.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class Exec : ToolTaskExtension {
	
		string		command;
		bool		ignoreExitCode;
		ITaskItem[]	outputs;
		string		stdErrEncoding;
		string		stdOutEncoding;
		string		workingDirectory;
		
		//Process		process;
		//int		executionTime;
		
		public Exec ()
		{
			ignoreExitCode = false;
		}
		
		// FIXME: what does that method do?
		[MonoTODO]
		protected internal override void AddCommandLineCommands (CommandLineBuilderExtension commandLine)
		{
			base.AddCommandLineCommands (commandLine);
		}
		
		/*public override bool Execute ()
		{
			StringCollection temporaryOutputs = new StringCollection ();
			string line = null;
			string[] commandTable = command.Split (null, 2);
			string filename = commandTable [0];
			string arguments = "";
			if (commandTable.Length == 2)
				arguments = commandTable [1];
		
			if (workingDirectory != null)
				process.StartInfo.WorkingDirectory = workingDirectory;
			process.StartInfo.FileName = filename;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;
			
			try {
				process.Start ();
				process.WaitForExit ();

				//exitCode = process.ExitCode;
				while ((line = process.StandardOutput.ReadLine ()) != null)
					temporaryOutputs.Add (line);
				outputs = new ITaskItem [temporaryOutputs.Count];
				int i  = 0;
				foreach (string s in temporaryOutputs)
					outputs [i++] = new TaskItem (s);
			}
			catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}
			
			if (exitCode != 0 && ignoreExitCode == false)
				return false;
			else
				return true;
		}*/
		
		// FIXME: we need to write another ExecuteTool and RealExecute that will collect std output and
		// make it available through Outputs property
		
		[MonoTODO]
		protected override int ExecuteTool (string pathToTool,
						    string responseFileCommands,
						    string commandLineCommands)
		{
			return base.ExecuteTool (GenerateFullPathToTool (), String.Empty, String.Empty);
		}
		
		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			return command;
		}
		
		[MonoTODO]
		protected override string GetWorkingDirectory ()
		{
			return workingDirectory;
		}
		
		[MonoTODO]
		protected override bool HandleTaskExecutionErrors ()
		{
			return true;
		}
		
		[MonoTODO]
		protected override void LogPathToTool (string toolName,
						       string pathToTool)
		{
		}
		
		[MonoTODO]
		protected override void LogToolCommand (string message)
		{
		}
		
		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			return true;
		}
		
		[Required]
		public string Command {
			get { return command; }
			set { command = value; }
		}

		public bool IgnoreExitCode {
			get { return ignoreExitCode; }
			set { ignoreExitCode = value; }
		}

		[Output]
		public ITaskItem[] Outputs {
			get { return outputs; }
			set { outputs = value; }
		}

		protected override Encoding StandardErrorEncoding {
			get { return base.StandardErrorEncoding; }
		}
		
		protected override MessageImportance StandardErrorLoggingImportance {
			get { return base.StandardErrorLoggingImportance; }
		}

		protected override Encoding StandardOutputEncoding {
			get { return base.StandardOutputEncoding; }
		}
		
		protected override MessageImportance StandardOutputLoggingImportance {
			get { return base.StandardOutputLoggingImportance; }
		}
		
		[MonoTODO]
		[Output]
		public string StdOutEncoding {
			get { return stdOutEncoding; }
			set { stdOutEncoding = value; }
		}
		
		[MonoTODO]
		[Output]
		public string StdErrEncoding {
			get { return stdErrEncoding; }
			set { stdErrEncoding = value; }
		}
		
		[MonoTODO]
		protected override string ToolName {
			get { return String.Empty; }
		}

		public string WorkingDirectory {
			get { return workingDirectory; }
			set { workingDirectory = value; }
		}
	}
}

#endif
