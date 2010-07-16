//
// Respack.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Moonlight.Build.Tasks {
	public class Respack : ToolTask
	{
		public override bool Execute ()
		{
			if (!ValidateParameters ()) {
				// not generating any resource file
				OutputFile = null;
				return true;
			}

			return BuildRequired () ? base.Execute () : true;
		}

		bool BuildRequired ()
		{
			if (!File.Exists (OutputFile.ItemSpec))
				return true;

			DateTime outputFileTime = File.GetLastWriteTime (OutputFile.ItemSpec);
			foreach (var res in Resources) {
				string file = res.ItemSpec;
				if (File.Exists (file) && File.GetLastWriteTime (file) > outputFileTime)
					return true;
			}

			return false;
		}

		void AddCommandLineCommands (CommandLineBuilderExtension commandLine)
		{
			if (Resources.Length == 0)
				return;

			commandLine.AppendFileNameIfNotNull (OutputFile);

			commandLine.AppendFileNamesIfNotNull (Resources, " ");
		}

		protected override string GenerateCommandLineCommands ()
		{
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();
			AddCommandLineCommands (clbe);
			return clbe.ToString ();
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}

		protected override bool ValidateParameters()
		{
			return Resources.Length > 0;
		}

		[Required]
		[Output]
		public ITaskItem OutputFile {
			get; set;
		}

		[Required]
		public ITaskItem[] Resources {
			get; set;
		}

		protected override string ToolName {
			get {
				return RunningOnWindows ? "respack.bat" : "respack";
			}
		}

                static bool RunningOnWindows {
                        get {
                                // Code from Mono.GetOptions/Options.cs
                                // check for non-Unix platforms - see FAQ for more details
                                // http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
                                int platform = (int) Environment.OSVersion.Platform;
                                return ((platform != 4) && (platform != 128));
                        }

                }
	}
}
