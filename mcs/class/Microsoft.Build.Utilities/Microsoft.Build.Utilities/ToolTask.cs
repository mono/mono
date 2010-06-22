//
// ToolTask.cs: Base class for command line tool tasks. 
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
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Mono.XBuild.Utilities;

using SCS = System.Collections.Specialized;

namespace Microsoft.Build.Utilities
{
	public abstract class ToolTask : Task
	{
		SCS.ProcessStringDictionary	environmentOverride;
		int			exitCode;
		int			timeout;
		string			toolPath, toolExe;
		Encoding		responseFileEncoding;
		MessageImportance	standardErrorLoggingImportance;
		MessageImportance	standardOutputLoggingImportance;
		StringBuilder toolOutput;
		bool typeLoadException;

		protected ToolTask ()
			: this (null, null)
		{
			this.standardErrorLoggingImportance = MessageImportance.High;
			this.standardOutputLoggingImportance = MessageImportance.Normal;
		}

		protected ToolTask (ResourceManager taskResources)
			: this (taskResources, null)
		{
		}

		protected ToolTask (ResourceManager taskResources,
				   string helpKeywordPrefix)
		{
			this.TaskResources = taskResources;
			this.HelpKeywordPrefix = helpKeywordPrefix;
			this.toolPath = MonoLocationHelper.GetBinDir ();
			this.responseFileEncoding = Encoding.UTF8;
			this.timeout = Int32.MaxValue;
			this.environmentOverride = new SCS.ProcessStringDictionary ();
		}

		[MonoTODO]
		protected virtual bool CallHostObjectToExecute ()
		{
			return true;
		}

		public override bool Execute ()
		{
			if (SkipTaskExecution ())
				return true;

			exitCode = ExecuteTool (GenerateFullPathToTool (), GenerateResponseFileCommands (),
				GenerateCommandLineCommands ());

			// HandleTaskExecutionErrors is called only if exitCode != 0
			return exitCode == 0 || HandleTaskExecutionErrors ();
		}
		
		[MonoTODO]
		protected virtual string GetWorkingDirectory ()
		{
			return null;
		}
		
		protected virtual int ExecuteTool (string pathToTool,
						   string responseFileCommands,
						   string commandLineCommands)

		{
			if (pathToTool == null)
				throw new ArgumentNullException ("pathToTool");

			string output, error, responseFileName;
			StreamWriter outwr, errwr;

			outwr = errwr = null;
			responseFileName = output = error = null;
			toolOutput = new StringBuilder ();

			try {
				string arguments = commandLineCommands;
				if (!String.IsNullOrEmpty (responseFileCommands)) {
					responseFileName = Path.GetTempFileName ();
					File.WriteAllText (responseFileName, responseFileCommands);
					arguments = arguments + " " + GetResponseFileSwitch (responseFileName);
				}

				LogToolCommand (String.Format ("Tool {0} execution started with arguments: {1} {2}",
						pathToTool, commandLineCommands, responseFileCommands));

				output = Path.GetTempFileName ();
				error = Path.GetTempFileName ();
				outwr = new StreamWriter (output);
				errwr = new StreamWriter (error);

				ProcessStartInfo pinfo = new ProcessStartInfo (pathToTool, arguments);
				pinfo.WorkingDirectory = GetWorkingDirectory () ?? Environment.CurrentDirectory;

				pinfo.UseShellExecute = false;
				pinfo.RedirectStandardOutput = true;
				pinfo.RedirectStandardError = true;

				try {
					ProcessWrapper pw = ProcessService.StartProcess (pinfo, outwr, errwr, null, environmentOverride);
					pw.WaitForOutput (timeout == Int32.MaxValue ? -1 : timeout);
					exitCode = pw.ExitCode;
					outwr.Close();
					errwr.Close();
					pw.Dispose ();
				} catch (System.ComponentModel.Win32Exception e) {
					Log.LogError ("Error executing tool '{0}': {1}", pathToTool, e.Message);
					return -1;
				}

				ProcessOutputFile (output, StandardOutputLoggingImportance);
				ProcessOutputFile (error, StandardErrorLoggingImportance);

				Log.LogMessage (MessageImportance.Low, "Tool {0} execution finished.", pathToTool);
				return exitCode;
			} finally {
				DeleteTempFile (responseFileName);
				if (outwr != null)
					outwr.Dispose ();
				if (errwr != null)
					errwr.Dispose ();

				DeleteTempFile (output);
				DeleteTempFile (error);
			}
		}

		void ProcessOutputFile (string filename, MessageImportance importance)
		{
			using (StreamReader sr = File.OpenText (filename)) {
				string line;
				while ((line = sr.ReadLine ()) != null) {
					if (typeLoadException) {
						toolOutput.Append (sr.ReadToEnd ());
						string output_str = toolOutput.ToString ();
						Regex reg  = new Regex (@".*WARNING.*used in (mscorlib|System),.*",
								RegexOptions.Multiline);

						if (reg.Match (output_str).Success)
							Log.LogError (
								"Error: A referenced assembly may be built with an incompatible " + 
								"CLR version. See the compilation output for more details.");
						else
							Log.LogError (
								"Error: A dependency of a referenced assembly may be missing, or " +
								"you may be referencing an assembly created with a newer CLR " +
								"version. See the compilation output for more details.");

						Log.LogError (output_str);
					}

					toolOutput.AppendLine (line);
					LogEventsFromTextOutput (line, importance);
				}
			}
		}

		protected virtual void LogEventsFromTextOutput (string singleLine, MessageImportance importance)
		{
			singleLine = singleLine.Trim ();
			if (singleLine.Length == 0)
				return;

			if (singleLine.StartsWith ("Unhandled Exception: System.TypeLoadException") ||
			    singleLine.StartsWith ("Unhandled Exception: System.IO.FileNotFoundException")) {
				typeLoadException = true;
			}

			// When IncludeDebugInformation is true, prevents the debug symbols stats from braeking this.
			if (singleLine.StartsWith ("WROTE SYMFILE") ||
				singleLine.StartsWith ("OffsetTable") ||
				singleLine.StartsWith ("Compilation succeeded") ||
				singleLine.StartsWith ("Compilation failed"))
				return;

			Match match = CscErrorRegex.Match (singleLine);
			if (!match.Success) {
				Log.LogMessage (importance, singleLine);
				return;
			}

			string filename = match.Result ("${file}") ?? "";
			string line = match.Result ("${line}");
			int lineNumber = !string.IsNullOrEmpty (line) ? Int32.Parse (line) : 0;

			string col = match.Result ("${column}");
			int columnNumber = 0;
			if (!string.IsNullOrEmpty (col))
				columnNumber = col == "255+" ? -1 : Int32.Parse (col);

			string category = match.Result ("${level}");
			string code = match.Result ("${number}");
			string text = match.Result ("${message}");

			if (String.Compare (category, "warning", StringComparison.OrdinalIgnoreCase) == 0) {
				Log.LogWarning (null, code, null, filename, lineNumber, columnNumber, -1,
					-1, text, null);
			} else if (String.Compare (category, "error", StringComparison.OrdinalIgnoreCase) == 0) {
				Log.LogError (null, code, null, filename, lineNumber, columnNumber, -1,
					-1, text, null);
			} else {
				Log.LogMessage (importance, singleLine);
			}
		}

		protected virtual string GenerateCommandLineCommands ()
		{
			return null;
		}

		protected abstract string GenerateFullPathToTool ();

		protected virtual string GenerateResponseFileCommands ()
		{
			return null;
		}

		protected virtual string GetResponseFileSwitch (string responseFilePath)
		{
			return String.Format ("@{0}", responseFilePath);
		}

		protected virtual bool HandleTaskExecutionErrors ()
		{
			if (!Log.HasLoggedErrors && exitCode != 0)
				Log.LogError ("Tool exited with code: {0}. Output: {1}", exitCode,
						toolOutput != null ? toolOutput.ToString () : String.Empty);
			toolOutput = null;

			return ExitCode == 0 && !Log.HasLoggedErrors;
		}

		protected virtual HostObjectInitializationStatus InitializeHostObject ()
		{
			return HostObjectInitializationStatus.NoActionReturnSuccess;
		}

		protected virtual void LogToolCommand (string message)
		{
			Log.LogMessage (MessageImportance.Normal, message);
		}
		
		[MonoTODO]
		protected virtual void LogPathToTool (string toolName,
						      string pathToTool)
		{
		}

		protected virtual bool SkipTaskExecution()
		{
			return !ValidateParameters ();
		}

		protected virtual bool ValidateParameters()
		{
			return true;
		}

		protected void DeleteTempFile (string fileName)
		{
			if (String.IsNullOrEmpty (fileName))
				return;

			try {
				File.Delete (fileName);
			} catch (IOException ioe) {
				Log.LogWarning ("Unable to delete temporary file '{0}' : {1}", ioe.Message);
			} catch (UnauthorizedAccessException uae) {
				Log.LogWarning ("Unable to delete temporary file '{0}' : {1}", uae.Message);
			}
		}

		protected virtual StringDictionary EnvironmentOverride
		{
			get { return environmentOverride; }
		}
		
		[Output]
		public int ExitCode {
			get { return exitCode; }
		}

		protected virtual Encoding ResponseFileEncoding
		{
			get { return responseFileEncoding; }
		}

		protected virtual Encoding StandardErrorEncoding
		{
			get { return Console.Error.Encoding; }
		}

		protected virtual MessageImportance StandardErrorLoggingImportance {
			get { return standardErrorLoggingImportance; }
		}

		protected virtual Encoding StandardOutputEncoding
		{
			get { return Console.Out.Encoding; }
		}

		protected virtual MessageImportance StandardOutputLoggingImportance {
			get { return standardOutputLoggingImportance; }
		}

		protected virtual bool HasLoggedErrors {
			get { return Log.HasLoggedErrors; }
		}

		public virtual int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		public virtual string ToolExe
		{
			get {
				if (toolExe == null)
					return ToolName;
				else
					return toolExe;
			}
			set {
				if (!String.IsNullOrEmpty (value))
					toolExe = value;
			}
		}

		protected abstract string ToolName
		{
			get;
		}

		public string ToolPath
		{
			get { return toolPath; }
			set {
				if (!String.IsNullOrEmpty (value))
					toolPath  = value;
			}
		}

		// Snatched from our codedom code, with some changes to make it compatible with csc
		// (the line+column group is optional is csc)
		static Regex errorRegex;
		static Regex CscErrorRegex {
			get {
				if (errorRegex == null)
					errorRegex = new Regex (@"^(\s*(?<file>[^\(]+)(\((?<line>\d*)(,(?<column>\d*[\+]*))?\))?:\s+)*(?<level>\w+)\s+(?<number>.*\d):\s*(?<message>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
				return errorRegex;
			}
		}
	}
}

#endif
