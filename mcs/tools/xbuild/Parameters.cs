//
// Parameters.cs: Class that contains information about command line parameters
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Mono.XBuild.CommandLine {
	public class Parameters {
	
		string			consoleLoggerParameters;
		bool			displayHelp;
		IList			flatArguments;
		IList			loggers;
		LoggerVerbosity		loggerVerbosity;
		bool			noConsoleLogger;
		bool			noLogo;
		string			projectFile;
		BuildPropertyGroup	properties;
		IList			remainingArguments;
		Hashtable		responseFiles;
		string[]		targets;
		bool			validate;
		string			validationSchema;
		string			toolsVersion;
		
		string			responseFile;
	
		public Parameters ()
		{
			consoleLoggerParameters = "";
			displayHelp = false;
			loggers = new ArrayList ();
			loggerVerbosity = LoggerVerbosity.Normal;
			noConsoleLogger = false;
			noLogo = false;
			properties = new BuildPropertyGroup ();
			targets = new string [0];
			
			responseFile = Path.Combine (
					Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location),
					"xbuild.rsp");
		}
		
		public void ParseArguments (string[] args)
		{
			bool autoResponse = true;
			flatArguments = new ArrayList ();
			remainingArguments = new ArrayList ();
			responseFiles = new Hashtable ();
			foreach (string s in args) {
				if (s.StartsWith ("/noautoresponse") || s.StartsWith ("/noautorsp")) {
					autoResponse = false;
					continue;
				}
				if (s [0] != '@') {
					flatArguments.Add (s);
					continue;
				}
				string responseFilename = Path.GetFullPath (UnquoteIfNeeded (s.Substring (1)));
				if (responseFiles.ContainsKey (responseFilename))
					ReportError (1, String.Format ("We already have {0} file.", responseFilename));
				responseFiles [responseFilename] = responseFilename;
				LoadResponseFile (responseFilename);
			}
			if (autoResponse == true) {
				// FIXME: we do not allow nested auto response file
				LoadResponseFile (responseFile);
			}
			foreach (string s in flatArguments) {
				if (s [0] != '/' || !ParseFlatArgument (s))
					remainingArguments.Add (s);
			}
			if (remainingArguments.Count == 0) {
				string[] sln_files = Directory.GetFiles (Directory.GetCurrentDirectory (), "*.sln");
				string[] proj_files = Directory.GetFiles (Directory.GetCurrentDirectory (), "*proj");

				if (sln_files.Length == 0 && proj_files.Length == 0)
					ReportError (3, "Please specify the project or solution file " +
							"to build, as none was found in the current directory.");

				if (sln_files.Length == 1 && proj_files.Length > 0) {
					var projects_table = new Dictionary<string, string> ();
					foreach (string pfile in SolutionParser.GetAllProjectFileNames (sln_files [0])) {
						string full_path = Path.GetFullPath (pfile);
						projects_table [full_path] = full_path;
					}

					if (!proj_files.Any (p => !projects_table.ContainsKey (Path.GetFullPath (p))))
						// if all the project files in the cur dir, are referenced
						// from the single .sln in the cur dir, then pick the sln
						proj_files = new string [0];
				}

				if (sln_files.Length + proj_files.Length > 1)
					ReportError (5, "Please specify the project or solution file " +
							"to build, as more than one solution or project file was found " +
							"in the current directory");

				if (sln_files.Length == 1)
					projectFile = sln_files [0];
				else
					projectFile = proj_files [0];
			} else if (remainingArguments.Count == 1) {
				projectFile = (string) remainingArguments [0];
			} else {
				ReportError (4, "Too many project files specified");
			}
		}

		private string UnquoteIfNeeded(string arg)
		{
			if (arg.StartsWith("\""))
				return arg.Substring(1, arg.Length - 2);
			return arg;
		}

		void LoadResponseFile (string filename)
		{
			StreamReader sr = null;
			string line;
			try {
				sr = new StreamReader (filename);
                                StringBuilder sb = new StringBuilder ();

                                while ((line = sr.ReadLine ()) != null) {
                                        int t = line.Length;

                                        for (int i = 0; i < t; i++) {
                                                char c = line [i];

						if (c == '#')
							// comment, ignore rest of the line
							break;

                                                if (c == '"' || c == '\'') {
                                                        char end = c;

                                                        for (i++; i < t; i++) {
                                                                c = line [i];

                                                                if (c == end)
                                                                        break;
                                                                sb.Append (c);
                                                        }
                                                } else if (c == ' ') {
                                                        if (sb.Length > 0) {
                                                                flatArguments.Add (sb.ToString ());
                                                                sb.Length = 0;
                                                        }
                                                } else
                                                        sb.Append (c);
                                        }
                                        if (sb.Length > 0){
                                                flatArguments.Add (sb.ToString ());
                                                sb.Length = 0;
                                        }
                                }
                        } catch (IOException x) {
				ErrorUtilities.ReportWarning (2, String.Format (
							"Error loading response file. (Exception: {0}). Ignoring.",
							x.Message));
			} finally {
                                if (sr != null)
                                        sr.Close ();
                        }
		}
		
		private bool ParseFlatArgument (string s)
		{
			switch (s) {
			case "/help":
			case "/h":
			case "/?":
				ErrorUtilities.ShowUsage ();
				break;
			case "/nologo":
				noLogo = true;
				break;
			case "/version":
			case "/ver":
				ErrorUtilities.ShowVersion (true);
				break;
			case "/noconsolelogger":
			case "/noconlog":
				noConsoleLogger = true;
				break;
			case "/validate":
			case "/val":
				validate = true;
				break;
			default:
				if (s.StartsWith ("/target:") || s.StartsWith ("/t:")) {
					ProcessTarget (s);
				} else if (s.StartsWith ("/property:") || s.StartsWith ("/p:")) {
					if (!ProcessProperty (s))
						return false;
				} else  if (s.StartsWith ("/logger:") || s.StartsWith ("/l:")) {
					ProcessLogger (s);
				} else if (s.StartsWith ("/verbosity:") || s.StartsWith ("/v:")) {
					ProcessVerbosity (s);
				} else if (s.StartsWith ("/consoleloggerparameters:") || s.StartsWith ("/clp:")) {
					ProcessConsoleLoggerParameters (s);
				} else if (s.StartsWith ("/validate:") || s.StartsWith ("/val:")) {
					ProcessValidate (s);
				} else if (s.StartsWith ("/toolsversion:") || s.StartsWith ("/tv:")) {
					ToolsVersion = s.Split (':') [1];
				} else
					return false;
				break;
			}

			return true;
		}
		
		internal void ProcessTarget (string s)
		{
			TryProcessMultiOption (s, "Target names must be specified as /t:Target1;Target2",
						out targets);
		}
		
		internal bool ProcessProperty (string s)
		{
			string[] splitProperties;
			if (!TryProcessMultiOption (s, "Property name and value expected as /p:<prop name>=<prop value>",
						out splitProperties))
				return false;

			foreach (string st in splitProperties) {
				if (st.IndexOf ('=') < 0) {
					ReportError (5,
							"Invalid syntax. Property name and value expected as " +
							"<prop name>=[<prop value>]");
					return false;
				}
				string [] property = st.Split ('=');
				properties.SetProperty (property [0], property.Length == 2 ? property [1] : "");
			}

			return true;
		}

		bool TryProcessMultiOption (string s, string error_message, out string[] values)
		{
			values = null;
			int colon = s.IndexOf (':');
			if (colon + 1 == s.Length) {
				ReportError (5, error_message);
				return false;
			}

			values = s.Substring (colon + 1).Split (';');
			return true;
		}

		private void ReportError (int errorCode, string message)
		{
			throw new CommandLineException (message, errorCode);
		}

		private void ReportError (int errorCode, string message, Exception cause)
		{
			throw new CommandLineException (message, cause, errorCode);
		}

		internal void ProcessLogger (string s)
		{
			loggers.Add (new LoggerInfo (s));
		}
		
		internal void ProcessVerbosity (string s)
		{
			string[] temp = s.Split (':');
			switch (temp [1]) {
			case "q":
			case "quiet":
				loggerVerbosity = LoggerVerbosity.Quiet;
				break;
			case "m":
			case "minimal":
				loggerVerbosity = LoggerVerbosity.Minimal;
				break;
			case "n":
			case "normal":
				loggerVerbosity = LoggerVerbosity.Normal;
				break;
			case "d":
			case "detailed":
				loggerVerbosity = LoggerVerbosity.Detailed;
				break;
			case "diag":
			case "diagnostic":
				loggerVerbosity = LoggerVerbosity.Diagnostic;
				break;
			}
		}
		
		internal void ProcessConsoleLoggerParameters (string s)
		{
			int colon = s.IndexOf (':');
			if (colon + 1 == s.Length)
				ReportError (5, "Invalid syntax, specify parameters as /clp:parameters");

			consoleLoggerParameters = s.Substring (colon + 1);
		}
		
		internal void ProcessValidate (string s)
		{
			string[] temp;
			validate = true;
			temp = s.Split (':');
			validationSchema = temp [1];
		}
		public bool DisplayHelp {
			get { return displayHelp; }
		}
		
		public bool NoLogo {
			get { return noLogo; }
		}
		
		public string ProjectFile {
			get { return projectFile; }
		}
		
		public string[] Targets {
			get { return targets; }
		}
		
		public BuildPropertyGroup Properties {
			get { return properties; }
		}
		
		public IList Loggers {
			get { return loggers; }
		}
		
		public LoggerVerbosity LoggerVerbosity {
			get { return loggerVerbosity; }
		}
		
		public string ConsoleLoggerParameters {
			get { return consoleLoggerParameters; }
		}
		
		public bool NoConsoleLogger {
			get { return noConsoleLogger; }
		}
		
		public bool Validate {
			get { return validate; }
		}
		
		public string ValidationSchema {
			get { return validationSchema; }
		}

		public string ToolsVersion {
			get { return toolsVersion; }
			private set { toolsVersion = value; }
		}
		
	}
}

#endif
