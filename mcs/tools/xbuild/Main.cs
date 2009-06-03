//
// Main.cs: Main program file of command line utility.
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
using System.IO;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Framework;

namespace Mono.XBuild.CommandLine {
	public class MainClass {
		
		Parameters	parameters;
		string[]	args;
		string		binPath;
		string		defaultSchema;
		
		Engine		engine;
		Project		project;
		
		public static void Main (string[] args)
		{
			MainClass mc = new MainClass ();
			mc.args = args;
			mc.Execute ();
		}
		
		public MainClass ()
		{
			binPath = ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20);
			defaultSchema = Path.Combine (binPath, "Microsoft.Build.xsd");
			parameters = new Parameters (binPath);
		}
		
		public void Execute ()
		{
			bool result = false;
			
			try {
				parameters.ParseArguments (args);
				
				if (parameters.DisplayVersion)
					ErrorUtilities.ShowVersion (false);
				
				engine  = new Engine (binPath);
				
				engine.GlobalProperties = this.parameters.Properties;
				
				if (!parameters.NoConsoleLogger) {
					ConsoleLogger cl = new ConsoleLogger ();
					cl.Parameters = parameters.ConsoleLoggerParameters;
					cl.Verbosity = parameters.LoggerVerbosity; 
					engine.RegisterLogger (cl);
				}
				
				foreach (LoggerInfo li in parameters.Loggers) {
					Assembly assembly;
					if (li.InfoType == LoadInfoType.AssemblyFilename)
						assembly = Assembly.LoadFrom (li.Filename);
					else
						assembly = Assembly.Load (li.AssemblyName);
					ILogger logger = (ILogger)Activator.CreateInstance (assembly.GetType (li.ClassName));
					logger.Parameters = li.Parameters;
					engine.RegisterLogger (logger); 
				}
				
				project = engine.CreateNewProject ();
				
				if (parameters.Validate) {
					if (parameters.ValidationSchema == null)
						project.SchemaFile = defaultSchema;
					else
						project.SchemaFile = parameters.ValidationSchema;
				}

				string projectFile = parameters.ProjectFile;
				if (!File.Exists (projectFile)) {
					ErrorUtilities.ReportError (0, String.Format ("Project file '{0}' not found.", projectFile));
					return;
				}

				if (projectFile.EndsWith (".sln"))
					projectFile = GenerateSolutionProject (projectFile);

				project.Load (projectFile);
				
				string oldCurrentDirectory = Environment.CurrentDirectory;
				string dir = Path.GetDirectoryName (projectFile);
				if (!String.IsNullOrEmpty (dir))
					Directory.SetCurrentDirectory (dir);
				result = engine.BuildProject (project, parameters.Targets, null);
				Directory.SetCurrentDirectory (oldCurrentDirectory);
			}
			
			catch (InvalidProjectFileException ipfe) {
				ErrorUtilities.ReportError (0, ipfe.Message);
			}

			catch (InternalLoggerException ile) {
				ErrorUtilities.ReportError (0, ile.Message);
			}

			catch (Exception) {
				throw;
			}
			
			finally {
				if (engine != null)
					engine.UnregisterAllLoggers ();

				Environment.Exit (result ? 0 : 1);
			}

		}

		string GenerateSolutionProject (string solutionFile)
		{
			SolutionParser s = new SolutionParser ();
			Project p = engine.CreateNewProject ();
			s.ParseSolution (solutionFile, p);
			string projectFile = solutionFile + ".proj";
			p.Save (projectFile);

			return projectFile;
		}
	}
}

#endif
