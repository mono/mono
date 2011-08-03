//
// Exec.cs: Task that executes commands.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.Text.RegularExpressions;
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
		string scriptFile;

#if NET_4_0
		Func<string, bool> errorMatcher, warningMatcher;
#endif
		
		public Exec ()
		{
			ignoreExitCode = false;
		}
		
		protected internal override void AddCommandLineCommands (CommandLineBuilderExtension commandLine)
		{
			if (IsRunningOnWindows)
				commandLine.AppendSwitch ("/q /c");

			if (!String.IsNullOrEmpty (command)) {
				scriptFile = Path.GetTempFileName ();
				if (IsRunningOnWindows)
					scriptFile = scriptFile + ".bat";
				using (StreamWriter sw = new StreamWriter (scriptFile)) {
					sw.Write (command);
				}
				commandLine.AppendFileNameIfNotNull (scriptFile);
			}
			base.AddCommandLineCommands (commandLine);
		}

		protected override int ExecuteTool (string pathToTool,
						    string responseFileCommands,
						    string commandLineCommands)
		{
			try {
#if NET_4_0
				errorMatcher = GetTryMatchRegexFunc (CustomErrorRegularExpression, true);
				warningMatcher = GetTryMatchRegexFunc (CustomWarningRegularExpression, false);
#endif
				return base.ExecuteTool (pathToTool, responseFileCommands, commandLineCommands);
			} finally {
				if (scriptFile != null)
					DeleteTempFile (scriptFile);
			}
		}

		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			return IsRunningOnWindows ? "cmd.exe" : "sh";
		}
		
		protected override string GetWorkingDirectory ()
		{
			return workingDirectory;
		}
		
		protected override bool HandleTaskExecutionErrors ()
		{
			if (ExitCode != 0)
				Log.LogError ("Command '{0}' exited with code: {1}.", Command, ExitCode);

			return ExitCode == 0 || ignoreExitCode;
		}
		
		[MonoTODO]
		protected override void LogPathToTool (string toolName,
						       string pathToTool)
		{
		}
		
		[MonoTODO]
		protected override void LogToolCommand (string message)
		{
			Log.LogMessage (MessageImportance.Normal, "Executing: " + command);
		}
		
		protected override void LogEventsFromTextOutput (string singleLine, MessageImportance importance)
		{
#if NET_4_0
			if (IgnoreStandardErrorWarningFormat ||
				(!errorMatcher (singleLine) && !warningMatcher (singleLine)))
#endif
				Log.LogMessage (importance, singleLine);
		}

#if NET_4_0
		// @is_error_type - log as errors, else warnings
		Func<string, bool> GetTryMatchRegexFunc (string regex_str, bool is_error_type)
		{
			bool is_bad = false;
			Regex regex = null;
			return (singleLine) => {
				if (String.IsNullOrEmpty (regex_str) || is_bad)
					return false;

				try {
					if (regex == null)
						regex = new Regex (regex_str, RegexOptions.Compiled);
				} catch (ArgumentException ae) {
					Log.LogError ("The regular expression specified for '{0}' is invalid : {1}",
							is_error_type ? "errors" : "warnings", ae.Message);
					Log.LogMessage (MessageImportance.Low, "The regular expression specified for '{0}' is invalid : {1}",
							is_error_type ? "errors" : "warnings", ae.ToString ());

					is_bad = true;
					return false;
				}

				if (!regex.Match (singleLine).Success)
					return false;

				if (is_error_type)
					Log.LogError (singleLine);
				else
					Log.LogWarning (singleLine);
				return true;
			};
		}
#endif

		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			return true;
		}
		
		[Required]
		public string Command {
			get { return command; }
			set {
				command = value;
				if (Path.DirectorySeparatorChar == '/')
					command = command.Replace ("\r\n", "\n");
			}
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

#if NET_4_0
		public bool IgnoreStandardErrorWarningFormat { get; set; }

		public string CustomErrorRegularExpression { get; set; }

		public string CustomWarningRegularExpression { get; set; }
#endif
		
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

		static bool IsRunningOnWindows {
			get {
				PlatformID pid = Environment.OSVersion.Platform;
				return ((int) pid != 128 && (int) pid != 4 && (int) pid != 6);
			}
		}

	}
}

#endif
