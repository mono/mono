//
// Main.cs: Main program file of command line utility.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Framework;

namespace Mono.XBuild.CommandLine {
	public class MainClass {
		
		Parameters	parameters;
		string[]	args;
		string		defaultSchema;
		
		Engine		engine;
		Project		project;
		ConsoleReportPrinter printer;
		
#pragma warning disable 169
		// this does nothing but adds strong reference to Microsoft.Build.Tasks*.dll that we need to load consistently.
		Microsoft.Build.Tasks.Copy dummy;
#pragma warning restore
		public static void Main (string[] args)
		{
			MainClass mc = new MainClass ();
			mc.args = args;
			mc.Execute ();
		}
		
		public MainClass ()
		{
			string binPath = ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20);
			defaultSchema = Path.Combine (binPath, "Microsoft.Build.xsd");
			parameters = new Parameters ();
		}

		public void Execute ()
		{
			bool result = false;
			bool show_stacktrace = false;
			
			try {
				try {
					parameters.ParseArguments (args);
				} catch {
					ShowDeprecationNotice ();
					throw;
				}

				show_stacktrace = (parameters.LoggerVerbosity == LoggerVerbosity.Detailed ||
					parameters.LoggerVerbosity == LoggerVerbosity.Diagnostic);
				
				if (!parameters.NoLogo) {
					ShowDeprecationNotice ();
					ErrorUtilities.ShowVersion (false);
				}
				
				engine  = Engine.GlobalEngine;
				if (!String.IsNullOrEmpty (parameters.ToolsVersion)) {
					if (engine.Toolsets [parameters.ToolsVersion] == null)
						ErrorUtilities.ReportError (0, new UnknownToolsVersionException (parameters.ToolsVersion).Message);

					engine.DefaultToolsVersion = parameters.ToolsVersion;
				}
				
				engine.GlobalProperties = this.parameters.Properties;
				
				if (!parameters.NoConsoleLogger) {
					printer = new ConsoleReportPrinter ();
					ConsoleLogger cl = new ConsoleLogger (parameters.LoggerVerbosity,
							printer.Print, printer.SetForeground, printer.ResetColor);

					cl.Parameters = parameters.ConsoleLoggerParameters;
					cl.Verbosity = parameters.LoggerVerbosity; 
					engine.RegisterLogger (cl);
				}

				if (parameters.FileLoggerParameters != null) {
					for (int i = 0; i < parameters.FileLoggerParameters.Length; i ++) {
						string fl_params = parameters.FileLoggerParameters [i];
						if (fl_params == null)
							continue;

						var fl = new FileLogger ();
						if (fl_params.Length == 0 && i > 0)
							fl.Parameters = String.Format ("LogFile=msbuild{0}.log", i);
						else
							fl.Parameters = fl_params;
						engine.RegisterLogger (fl);
					}
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

				result = engine.BuildProjectFile (projectFile, parameters.Targets, null, null, BuildSettings.None, parameters.ToolsVersion);
			}
			
			catch (InvalidProjectFileException ipfe) {
				ErrorUtilities.ReportError (0, show_stacktrace ? ipfe.ToString () : ipfe.Message);
			}

			catch (InternalLoggerException ile) {
				ErrorUtilities.ReportError (0, show_stacktrace ? ile.ToString () : ile.Message);
			}

			catch (CommandLineException cle) {
				ErrorUtilities.ReportError(cle.ErrorCode, show_stacktrace ? cle.ToString() : cle.Message);
			}
			finally {
				if (engine != null)
					engine.UnregisterAllLoggers ();

				Environment.Exit (result ? 0 : 1);
			}

		}

		void ShowDeprecationNotice ()
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine ();
			Console.WriteLine (">>>> xbuild tool is deprecated and will be removed in future updates, use msbuild instead <<<<");
			Console.WriteLine ();
			Console.ResetColor ();
		}
	}

	// code from mcs/report.cs
	class ConsoleReportPrinter
	{
		string prefix, postfix;
		bool color_supported;
		TextWriter writer;
		string [] colorPrefixes;

		public ConsoleReportPrinter ()
			: this (Console.Out)
		{
		}

		public ConsoleReportPrinter (TextWriter writer)
		{
			this.writer = writer;

			string term = Environment.GetEnvironmentVariable ("TERM");
			bool xterm_colors = false;

			color_supported = false;
			switch (term){
			case "xterm":
			case "rxvt":
			case "rxvt-unicode":
				if (Environment.GetEnvironmentVariable ("COLORTERM") != null){
					xterm_colors = true;
				}
				break;

			case "xterm-color":
			case "xterm-256color":
				xterm_colors = true;
				break;
			}
			if (!xterm_colors)
				return;

			if (!(UnixUtils.isatty (1) && UnixUtils.isatty (2)))
				return;

			color_supported = true;
			PopulateColorPrefixes ();
			postfix = "\x001b[0m";
		}

		void PopulateColorPrefixes ()
		{
			colorPrefixes = new string [16];

			colorPrefixes [(int)ConsoleColor.Black] = GetForeground ("black");
			colorPrefixes [(int)ConsoleColor.DarkBlue] = GetForeground ("blue");
			colorPrefixes [(int)ConsoleColor.DarkGreen] = GetForeground ("green");
			colorPrefixes [(int)ConsoleColor.DarkCyan] = GetForeground ("cyan");
			colorPrefixes [(int)ConsoleColor.DarkRed] = GetForeground ("red");
			colorPrefixes [(int)ConsoleColor.DarkMagenta] = GetForeground ("magenta");
			colorPrefixes [(int)ConsoleColor.DarkYellow] = GetForeground ("yellow");
			colorPrefixes [(int)ConsoleColor.DarkGray] = GetForeground ("grey");

			colorPrefixes [(int)ConsoleColor.Gray] = GetForeground ("brightgrey");
			colorPrefixes [(int)ConsoleColor.Blue] = GetForeground ("brightblue");
			colorPrefixes [(int)ConsoleColor.Green] = GetForeground ("brightgreen");
			colorPrefixes [(int)ConsoleColor.Cyan] = GetForeground ("brightcyan");
			colorPrefixes [(int)ConsoleColor.Red] = GetForeground ("brightred");
			colorPrefixes [(int)ConsoleColor.Magenta] = GetForeground ("brightmagenta");
			colorPrefixes [(int)ConsoleColor.Yellow] = GetForeground ("brightyellow");

			colorPrefixes [(int)ConsoleColor.White] = GetForeground ("brightwhite");
		}

		public void SetForeground (ConsoleColor color)
		{
			if (color_supported)
				prefix = colorPrefixes [(int)color];
		}

		public void ResetColor ()
		{
			prefix = "\x001b[0m";
		}

		static int NameToCode (string s)
		{
			switch (s) {
			case "black":
				return 0;
			case "red":
				return 1;
			case "green":
				return 2;
			case "yellow":
				return 3;
			case "blue":
				return 4;
			case "magenta":
				return 5;
			case "cyan":
				return 6;
			case "grey":
			case "white":
				return 7;
			}
			return 7;
		}

		//
		// maps a color name to its xterm color code
		//
		static string GetForeground (string s)
		{
			string highcode;

			if (s.StartsWith ("bright")) {
				highcode = "1;";
				s = s.Substring (6);
			} else
				highcode = "";

			return "\x001b[" + highcode + (30 + NameToCode (s)).ToString () + "m";
		}

		static string GetBackground (string s)
		{
			return "\x001b[" + (40 + NameToCode (s)).ToString () + "m";
		}

		string FormatText (string txt)
		{
			if (prefix != null && color_supported)
				return prefix + txt + postfix;

			return txt;
		}

		public void Print (string message)
		{
			writer.WriteLine (FormatText (message));
		}

	}

	class UnixUtils {
		[System.Runtime.InteropServices.DllImport ("libc", EntryPoint="isatty")]
		extern static int _isatty (int fd);

		public static bool isatty (int fd)
		{
			try {
				return _isatty (fd) == 1;
			} catch {
				return false;
			}
		}
	}

}

