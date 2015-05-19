
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Build.Utilities
{
	internal static class ProcessService {
		static Dictionary<string, string> globalEnvironmentVariablesOverride;

		public static Dictionary<string, string> GlobalEnvironmentVariblesOverride {
			get {
				if (globalEnvironmentVariablesOverride == null)
					globalEnvironmentVariablesOverride = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
				return globalEnvironmentVariablesOverride;
			}
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, EventHandler exited)
		{
			return StartProcess (command, arguments, workingDirectory, (ProcessEventHandler)null, (ProcessEventHandler)null, exited);
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, ProcessEventHandler outputStreamChanged, ProcessEventHandler errorStreamChanged)
		{
			return StartProcess (command, arguments, workingDirectory, outputStreamChanged, errorStreamChanged, null);
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, TextWriter outWriter, TextWriter errorWriter, EventHandler exited)
		{
			return StartProcess (command, arguments, workingDirectory, outWriter, errorWriter, exited, false);
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, TextWriter outWriter, TextWriter errorWriter, EventHandler exited, bool redirectStandardInput)
		{
			ProcessEventHandler wout = OutWriter.GetWriteHandler (outWriter);
			ProcessEventHandler werr = OutWriter.GetWriteHandler (errorWriter);
			return StartProcess (command, arguments, workingDirectory, wout, werr, exited, redirectStandardInput);
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, ProcessEventHandler outputStreamChanged, ProcessEventHandler errorStreamChanged, EventHandler exited)
		{
			return StartProcess (command, arguments, workingDirectory, outputStreamChanged, errorStreamChanged, exited, false);
		}

		public static ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, ProcessEventHandler outputStreamChanged, ProcessEventHandler errorStreamChanged, EventHandler exited, bool redirectStandardInput)
		{
			return StartProcess (CreateProcessStartInfo (command, arguments, workingDirectory, redirectStandardInput),
				outputStreamChanged, errorStreamChanged, exited, null);
		}

		public static ProcessWrapper StartProcess (ProcessStartInfo startInfo, TextWriter outWriter, TextWriter errorWriter, EventHandler exited)
		{
			return StartProcess (startInfo, outWriter, errorWriter, exited, null);
		}

		public static ProcessWrapper StartProcess (ProcessStartInfo startInfo, TextWriter outWriter, TextWriter errorWriter, EventHandler exited, Dictionary<string, string> environmentOverride)
		{
			ProcessEventHandler wout = OutWriter.GetWriteHandler (outWriter);
			ProcessEventHandler werr = OutWriter.GetWriteHandler (errorWriter);
			return StartProcess (startInfo, wout, werr, exited, environmentOverride);
		}

		// @environmentOverride overrides even the global override values
		public static ProcessWrapper StartProcess (ProcessStartInfo startInfo, ProcessEventHandler outputStreamChanged, ProcessEventHandler errorStreamChanged, EventHandler exited, Dictionary<string, string> environmentOverride)
		{
			if (startInfo == null)
				throw new ArgumentException ("startInfo");

			ProcessWrapper p = new ProcessWrapper();

			if (outputStreamChanged != null) {
				p.OutputStreamChanged += outputStreamChanged;
			}

			if (errorStreamChanged != null)
				p.ErrorStreamChanged += errorStreamChanged;

			if (exited != null)
				p.Exited += exited;

			p.StartInfo = startInfo;
			ProcessEnvironmentVariableOverrides (p.StartInfo, environmentOverride);

			// WORKAROUND for "Bug 410743 - wapi leak in System.Diagnostic.Process"
			// Process leaks when an exit event is registered
			// instead we use another thread to monitor I/O and wait for exit
			// p.EnableRaisingEvents = true;

			p.Start ();
			return p;
		}

		public static ProcessStartInfo CreateProcessStartInfo (string command, string arguments, string workingDirectory, bool redirectStandardInput)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			if (command.Length == 0)
				throw new ArgumentException("command");

			ProcessStartInfo startInfo = null;
			if(String.IsNullOrEmpty (arguments))
				startInfo = new ProcessStartInfo (command);
			else
				startInfo = new ProcessStartInfo (command, arguments);

			if(workingDirectory != null && workingDirectory.Length > 0)
				startInfo.WorkingDirectory = workingDirectory;

			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardInput = redirectStandardInput;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;

			return startInfo;
		}

		public static void ProcessEnvironmentVariableOverrides (ProcessStartInfo info, Dictionary<string, string> environmentOverride)
		{
			if (globalEnvironmentVariablesOverride != null)
				foreach (var entry in globalEnvironmentVariablesOverride)
					ProcessEnvironmentVariable (info, (string)entry.Key, (string)entry.Value);

			if (environmentOverride != null)
				foreach (var entry in environmentOverride)
					ProcessEnvironmentVariable (info, (string)entry.Key, (string)entry.Value);
                }

		static void ProcessEnvironmentVariable (ProcessStartInfo info, string name, string value)
		{
			if (value == null && info.EnvironmentVariables.ContainsKey (name))
				info.EnvironmentVariables.Remove (name);
			else
				info.EnvironmentVariables[name] = value;
		}

	}

	class OutWriter
	{
		TextWriter writer;

		public OutWriter (TextWriter writer)
		{
			this.writer = writer;
		}

		public void WriteOut (object sender, string s)
		{
			writer.Write (s);
		}

		public static ProcessEventHandler GetWriteHandler (TextWriter tw)
		{
			return tw != null ? new ProcessEventHandler(new OutWriter (tw).WriteOut) : null;
		}
	}
}

