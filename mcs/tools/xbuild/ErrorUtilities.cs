//
// ErrorUtilities.cs: Functions that print out errors, warnings, help etc.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

namespace Mono.XBuild.CommandLine {
	public static class ErrorUtilities {

		static string[] usage = {
			"",
			"Syntax:              xbuild.exe [options] [project file]",
			"",
			"Description:         Builds the specified targets in the project file. If",
			"                     a project file is not specified, MSBuild searches the",
			"                     current working directory for a file that has a file",
			"                     extension that ends in \"proj\" and uses that file.",
			"",
			"Switches:",
			"",
			"  /help              Display this usage message. (Short form: /? or /h)",
			"",
			"  /nologo            Do not display the startup banner and copyright message.",
			"",
			"  /version           Display version information only. (Short form: /ver)",
			"",
			"  @<file>            Insert command-line settings from a text file. To specify",
			"                     multiple response files, specify each response file",
			"                     separately.",
			"",
			"  /noautoresponse    Do not auto-include the MSBuild.rsp file. (Short form:",
			"                     /noautorsp)",
			"",
			"  /target:<targets>  Build these targets in this project. Use a semicolon or a",
			"                     comma to separate multiple targets, or specify each",
			"                     target separately. (Short form: /t)",
			"                     Example:",
			"                       /target:Resources;Compile",
			"",
			"  /property:<n>=<v>  Set or override these project-level properties. <n> is",
			"                     the property name, and <v> is the property value. Use a",
			"                     semicolon or a comma to separate multiple properties, or",
			"                     specify each property separately. (Short form: /p)",
			"                     Example:",
			@"                       /property:WarningLevel=2;OutDir=bin\Debug\",
			"",
			"  /logger:<logger>   Use this logger to log events from MSBuild. To specify",
			"                     multiple loggers, specify each logger separately.",
			"                     The <logger> syntax is:",
			"                        [<logger class>,]<logger assembly>[;<logger parameters>]",
			"                     The <logger class> syntax is:",
			"                        [<partial or full namespace>.]<logger class name>",
			"                     The <logger assembly> syntax is:",
			"                        {<assembly name>[,<strong name>] | <assembly file>}",
			"                     The <logger parameters> are optional, and are passed",
			"                     to the logger exactly as you typed them. (Short form: /l)",
			"                     Examples:",
			"                       /logger:XMLLogger,MyLogger,Version=1.0.2,Culture=neutral",
			@"                       /logger:XMLLogger,C:\Loggers\MyLogger.dll;OutputAsHTML",
			"",
			"  /verbosity:<level> Display this amount of information in the event log.",
			"                     The available verbosity levels are: q[uiet], m[inimal],",
			"                     n[ormal], d[etailed], and diag[nostic]. (Short form: /v)",
			"                     Example:",
			"                       /verbosity:quiet",
			"",
			"  /consoleloggerparameters:<parameters>",
			"                     Parameters to console logger. (Short form: /clp)",
			"                     The available parameters are:",
			"                        PerformanceSummary--show time spent in tasks, targets",
			"                            and projects.",
			"                        NoSummary--don't show error and warning summary at the",
			"                            end.",
			"                     Example:",
			"                        /consoleloggerparameters:PerformanceSummary;NoSummary",
			"",
			"  /noconsolelogger   Disable the default console logger and do not log events",
			"                     to the console. (Short form: /noconlog)",
			"",
			"  /validate          Validate the project against the default schema. (Short",
			"                     form: /val)",
			"",
			"  /validate:<schema> Validate the project against the specified schema. (Short",
			"                     form: /val)",
			"                     Example:",
			"                       /validate:MyExtendedBuildSchema.xsd",
			"",
			"Examples:",
			"",
			"        MSBuild MyApp.sln /t:Rebuild /p:Configuration=Release",
			"        MSBuild MyApp.csproj /t:Clean /p:Configuration=Debug",

		};
		
		static string[] version = {
			"XBuild Engine Version 0.1",
			String.Format ("Mono, Version {0}", Consts.MonoVersion),
			"Copyright (C) Marek Sieradzki 2005. All rights reserved.",
		};

		
		static public void ReportError (int errorNum, string msg)
		{
			Console.WriteLine (String.Format ("MSBUILD: error MSBUILD{0:0000}: {1}", errorNum, msg));
			Environment.Exit (1);
		}

		static public void ReportWarning (int errorNum, string msg)
		{
			Console.WriteLine (String.Format ("MSBUILD: warning MSBUILD{0:0000}: {1}", errorNum, msg));
		}

                static public void ReportInvalidArgument (string option, string value)
		{
                        ReportError (1012, String.Format ("'{0}' is not a valid setting for option '{1}'", value, option));
                }

                static public void ReportMissingArgument (string option)
		{
                        ReportError (1003, String.Format ("Compiler option '{0}' must be followed by an argument", option));
                }

                static public void ReportNotImplemented (string option)
		{
                        ReportError (0, String.Format ("Compiler option '{0}' is not implemented", option));
                }
 
                static public void ReportMissingFileSpec (string option)
		{
                        ReportError (1008, String.Format ("Missing file specification for '{0}' command-line option", option));
                }

                static public void ReportMissingText (string option)
		{
                        ReportError (1010, String.Format ("Missing ':<text>' for '{0}' option", option));
                }

		static public void ShowUsage ()
		{
			Display (usage);
			Environment.Exit (0);
			
		}

		static public void ShowVersion (bool exit)
		{
			Display (version);
			if (exit)
				Environment.Exit (0);
		}

		static private void Display (string[] array)
		{
			foreach (string s in array)
				Console.WriteLine (s);
		}
	}
}

#endif
