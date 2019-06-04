//
// ToolTask.cs: Base class for command line tool tasks. 
//
// Authors:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
// Copyright 2014 Xamarin Inc
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
using System.Threading;
using System.Collections.Generic;

using SCS = System.Collections.Specialized;

namespace Microsoft.Build.Utilities
{
	public abstract class ToolTask : Task
		, ICancelableTask
	{
		int			exitCode;
		int			timeout;
		string			toolPath, toolExe;
		Encoding		responseFileEncoding;
		MessageImportance	standardErrorLoggingImportance;
		MessageImportance	standardOutputLoggingImportance;
		StringBuilder toolOutput;
		bool typeLoadException;
		ManualResetEvent canceled;

		/* dummy getter/setter for msbuild compability */
		public bool YieldDuringToolExecution
		{
			get;
			set;
		}

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
			this.responseFileEncoding = Encoding.UTF8;
			this.timeout = Int32.MaxValue;
			canceled = new ManualResetEvent (false);
		}

		[MonoTODO]
		protected virtual bool CallHostObjectToExecute ()
		{
			return true;
		}

		string CreateToolPath ()
		{
			string tp;
			if (string.IsNullOrEmpty (ToolPath)) {
				tp = GenerateFullPathToTool ();
				if (string.IsNullOrEmpty (tp))
					return null;

				//
				// GenerateFullPathToTool can return path including tool name
				//
				if (string.IsNullOrEmpty (ToolExe))
					return tp;

				tp = Path.GetDirectoryName (tp);
			} else {
				tp = ToolPath;
			}

			var	path = Path.Combine (tp, ToolExe);
			if (!File.Exists (path)) {
				if (Log != null)
					Log.LogError ("Tool executable '{0}' could not be found", path);
				return null;
			}

			return path;
		}

		public override bool Execute ()
		{
			if (SkipTaskExecution ())
				return true;

			var tool_path = CreateToolPath ();
			if (tool_path == null)
				return false;

			exitCode = ExecuteTool (tool_path, GenerateResponseFileCommands (),
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

			string responseFileName;
			responseFileName = null;
			toolOutput = new StringBuilder ();

			try {
				string responseFileSwitch = String.Empty;
				if (!String.IsNullOrEmpty (responseFileCommands)) {
					responseFileName = Path.GetTempFileName ();
					File.WriteAllText (responseFileName, responseFileCommands);
					responseFileSwitch = GetResponseFileSwitch (responseFileName);
				}

				var pinfo = GetProcessStartInfo (pathToTool, commandLineCommands, responseFileSwitch);
				LogToolCommand (String.Format ("Tool {0} execution started with arguments: {1} {2}",
						pinfo.FileName, commandLineCommands, responseFileCommands));

				var pendingLineFragmentOutput = new StringBuilder ();
				var pendingLineFragmentError = new StringBuilder ();
				var environmentOverride = GetAndLogEnvironmentVariables ();
				try {
					// When StartProcess returns, the process has already .Start()'ed
					// If we subscribe to the events after that, then for processes that
					// finish executing before we can subscribe, we won't get the output/err
					// events at all!
					ProcessWrapper pw = ProcessService.StartProcess (pinfo,
							(_, msg) => ProcessLine (pendingLineFragmentOutput, msg, StandardOutputLoggingImportance),
							(_, msg) => ProcessLine (pendingLineFragmentError, msg, StandardErrorLoggingImportance),
							null,
							environmentOverride);

					pw.WaitForOutput (timeout == Int32.MaxValue ? Int32.MaxValue : timeout);

					// Process any remaining line
					ProcessLine (pendingLineFragmentOutput, StandardOutputLoggingImportance, true);
					ProcessLine (pendingLineFragmentError, StandardErrorLoggingImportance, true);

					exitCode = pw.ExitCode;
					pw.Dispose ();
				} catch (System.ComponentModel.Win32Exception e) {
					Log.LogError ("Error executing tool '{0}': {1}", pathToTool, e.Message);
					return -1;
				}

				if (typeLoadException)
					ProcessTypeLoadException ();

				pendingLineFragmentOutput.Length = 0;
				pendingLineFragmentError.Length = 0;

				Log.LogMessage (MessageImportance.Low, "Tool {0} execution finished.", pathToTool);
				return exitCode;
			} finally {
				DeleteTempFile (responseFileName);
			}
		}

		void ProcessTypeLoadException ()
		{
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

		void ProcessLine (StringBuilder outputBuilder, MessageImportance importance, bool isLastLine)
		{
			if (outputBuilder.Length == 0)
				return;

			if (isLastLine && !outputBuilder.ToString ().EndsWith (Environment.NewLine))
				// last line, but w/o an trailing newline, so add that
				outputBuilder.Append (Environment.NewLine);

			ProcessLine (outputBuilder, null, importance);
		}

		void ProcessLine (StringBuilder outputBuilder, string line, MessageImportance importance)
		{
			// Add to any line fragment from previous call
			if (line != null)
				outputBuilder.Append (line);

			// Don't remove empty lines!
			var lines = outputBuilder.ToString ().Split (new string [] {Environment.NewLine}, StringSplitOptions.None);

			// Clear the builder. If any incomplete line is found,
			// then it will get added back
			outputBuilder.Length = 0;
			for (int i = 0; i < lines.Length; i ++) {
				string singleLine = lines [i];
				if (i == lines.Length - 1 && !singleLine.EndsWith (Environment.NewLine)) {
					// Last line doesn't end in newline, could be part of
					// a bigger line. Save for later processing
					outputBuilder.Append (singleLine);
					continue;
				}

				toolOutput.AppendLine (singleLine);

				// in case of typeLoadException, collect all the output
				// and then handle in ProcessTypeLoadException
				if (!typeLoadException)
					LogEventsFromTextOutput (singleLine, importance);
			}
		}

		protected virtual void LogEventsFromTextOutput (string singleLine, MessageImportance messageImportance)
		{
			if (singleLine.Length == 0) {
				Log.LogMessage (singleLine, messageImportance);
				return;
			}

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

			var result = MSBuildErrorParser.TryParseLine (singleLine);
			if (result == null) {
				Log.LogMessage (messageImportance, singleLine);
				return;
			}

			string filename = result.Origin ?? GetType ().Name.ToUpper ();

			if (result.IsError) {
				Log.LogError (
					result.Subcategory,
					result.Code,
					null,
					filename,
					result.Line,
					result.Column,
					result.EndLine,
					result.EndColumn,
					result.Message
				);
			} else {
				Log.LogWarning (
					result.Subcategory,
					result.Code,
					null,
					filename,
					result.Line,
					result.Column,
					result.EndLine,
					result.EndColumn,
					result.Message
				);
			}
		}

		protected virtual string GenerateCommandLineCommands ()
		{
			return "";
		}

		protected abstract string GenerateFullPathToTool ();

		protected virtual string GenerateResponseFileCommands ()
		{
			return "";
		}

		protected virtual string GetResponseFileSwitch (string responseFilePath)
		{
			return String.Format ("@{0}", responseFilePath);
		}

		protected ProcessStartInfo GetProcessStartInfo (string pathToTool, string commandLineCommands, string responseFileSwitch)
		{
			var pinfo = new ProcessStartInfo (pathToTool, String.Format ("{0} {1}", commandLineCommands, responseFileSwitch));

			pinfo.WorkingDirectory = GetWorkingDirectory () ?? Environment.CurrentDirectory;
			pinfo.UseShellExecute = false;
			pinfo.CreateNoWindow = true;
			pinfo.RedirectStandardOutput = true;
			pinfo.RedirectStandardError = true;

			return pinfo;
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

		// If EnvironmentVariables is defined, then merge EnvironmentOverride
		// EnvironmentOverride is Obsolete'd in 4.0
		//
		// Returns the final set of environment variables and logs them
		Dictionary<string, string> GetAndLogEnvironmentVariables ()
		{
			var env_vars = GetEnvironmentVariables ();
			if (env_vars == null)
				return env_vars;

			Log.LogMessage (MessageImportance.Low, "Environment variables being passed to the tool:");
			foreach (var entry in env_vars)
				Log.LogMessage (MessageImportance.Low, "\t{0}={1}", (string)entry.Key, (string)entry.Value);

			return env_vars;
		}

		Dictionary<string, string> GetEnvironmentVariables ()
		{
			var env_vars = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);

			if (EnvironmentVariables != null) {
				foreach (string pair in EnvironmentVariables) {
					string [] key_value = pair.Split ('=');
					if (!String.IsNullOrEmpty (key_value [0]))
						env_vars [key_value [0]] = key_value.Length > 1 ? key_value [1] : String.Empty;
				}
			}

			if (EnvironmentOverride != null)
				foreach (DictionaryEntry entry in EnvironmentOverride)
					env_vars [(string)entry.Key] = (string)entry.Value;

			return env_vars;
		}

		protected virtual StringDictionary EnvironmentOverride
		{
			get { return null; }
		}

		// Ignore EnvironmentOverride if this is set
		public string[] EnvironmentVariables { get; set; }

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
				if (string.IsNullOrEmpty (toolExe))
					return ToolName;
				else
					return toolExe;
			}
			set { toolExe = value; }
		}

		protected abstract string ToolName
		{
			get;
		}

		public string ToolPath
		{
			get { return toolPath; }
			set { toolPath  = value; }
		}

		protected ManualResetEvent ToolCanceled {
			get {
				return canceled;
			}
		}

		public virtual void Cancel ()
		{
			canceled.Set ();
		}

		protected MessageImportance StandardErrorImportanceToUse {
			get {
				return MessageImportance.Normal;
			}
		}

		protected MessageImportance StandardOutputImportanceToUse {
			get {
				return MessageImportance.Low;
			}
		}

		public bool LogStandardErrorAsError { get; set; }
		public string StandardOutputImportance { get; set; }
	}
}
