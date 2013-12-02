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

		static string[] version = {
			String.Format ("XBuild Engine Version {0}", Consts.MonoVersion),
			String.Format ("Mono, Version {0}", Consts.MonoVersion),
			"Copyright (C) Marek Sieradzki 2005-2008, Novell 2008-2011.",
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
			Display (version);
			Console.WriteLine ("xbuild [options] [project-file]");
			Console.WriteLine (
				"    /version		Show the xbuild version\n" +
				"    /noconsolelogger	Disable the default console logger\n" +
				"    /target:T1[,TN]	List of targets to build\n" +
				"    /property:Name=Value\n" +
				"			Set or override project properties\n" +
				"    /logger:<logger>	Custom logger to log events\n" +
				"    /verbosity:<level>	Logger verbosity level : quiet, minimal, normal, detailed, diagnostic\n" +
				"    /validate		Validate the project file against the schema\n" +
				"    /validate:<schema>	Validate the project file against the specified schema\n" +
				"    /consoleloggerparameters:<params>\n" +
				"    /clp:<params>\n" +
				"			Parameters for the console logger\n" +
				"    /fileloggerparameters[n]:<params>\n" +
				"    /flp[n]:<params>\n" +
				"		        Parameters for the file logger, eg. LogFile=foo.log\n" +
				"    /nologo		Don't show the initial banner\n" +
				"    /help		Show this help\n"
				);
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
