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
using System.Text;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Mono.XBuild.CommandLine {
	public class Parameters {
	
		string			consoleLoggerParameters;
		bool			displayHelp;
		bool			displayVersion;
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
		
		string			responseFile;
	
		public Parameters (string binPath)
		{
			consoleLoggerParameters = "";
			displayHelp = false;
			displayVersion = true;
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
				string responseFilename = Path.GetFullPath (s.Substring (1));
				if (responseFiles.ContainsKey (responseFilename))
					ErrorUtilities.ReportError (1, String.Format ("We already have {0} file.", responseFilename));
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
					ErrorUtilities.ReportError (3, "Please specify the project or solution file " +
							"to build, as none was found in the current directory.");

				if (sln_files.Length + proj_files.Length > 1)
					ErrorUtilities.ReportError (5, "Please specify the project or solution file " +
							"to build, as more than one solution or project file was found " +
							"in the current directory");

				if (sln_files.Length == 1)
					projectFile = sln_files [0];
				else
					projectFile = proj_files [0];
			} else if (remainingArguments.Count == 1) {
				projectFile = (string) remainingArguments [0];
			} else {
				ErrorUtilities.ReportError (4, "Too many project files specified");
			}
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
                        } catch (Exception) {
				// FIXME: we lose exception message
				ErrorUtilities.ReportError (2, "Error during loading response file.");
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
				} else
					return false;
				break;
			}

			return true;
		}
		
		internal void ProcessTarget (string s)
		{
			string[] temp = s.Split (':');
			targets = temp [1].Split (';');
		}
		
		internal bool ProcessProperty (string s)
		{
			string[] parameter, splittedProperties, property;
			parameter = s.Split (':');
			if (parameter.Length != 2) {
				ErrorUtilities.ReportError (5, "Property name and value expected as /p:<prop name>=<prop value>");
				return false;
			}

			splittedProperties = parameter [1].Split (';');
			foreach (string st in splittedProperties) {
				if (st.IndexOf ('=') < 0) {
					ErrorUtilities.ReportError (5,
							"Invalid syntax. Property name and value expected as " +
							"<prop name>=[<prop value>]");
					return false;
				}
				property = st.Split ('=');
				properties.SetProperty (property [0], property.Length == 2 ? property [1] : "");
			}

			return true;
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
			consoleLoggerParameters = s; 
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
		
		public bool DisplayVersion {
			get { return displayVersion; }
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
		
	}
}

#endif
