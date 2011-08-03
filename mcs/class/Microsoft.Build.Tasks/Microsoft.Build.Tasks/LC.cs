//
// LC.cs: Task for license compiler
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
// NONINFRINGEMENT. IN NO EVENT SHLCL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DELCINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Tasks {
	public class LC : ToolTaskExtension
	{
		public LC ()
		{
		}

		protected internal override void AddCommandLineCommands (
						CommandLineBuilderExtension commandLine)
		{
			if (Sources.Length == 0)
				return;

			foreach (ITaskItem item in Sources)
				commandLine.AppendSwitchIfNotNull ("--complist=", item.ItemSpec);

			commandLine.AppendSwitchIfNotNull ("--target=", LicenseTarget);

			if (ReferencedAssemblies != null)
				foreach (ITaskItem reference in ReferencedAssemblies)
					commandLine.AppendSwitchIfNotNull ("--load=", reference.ItemSpec);

			string outdir;
			if (Bag ["OutputDirectory"] != null)
				outdir = OutputDirectory;
			else
				outdir = ".";

			commandLine.AppendSwitchIfNotNull ("--outdir=", outdir);

			if (Bag ["NoLogo"] != null && NoLogo)
				commandLine.AppendSwitch ("--nologo");

			OutputLicense = new TaskItem (Path.Combine (OutputDirectory, LicenseTarget.ItemSpec + ".licenses"));
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}

		protected override bool ValidateParameters()
		{
			return Sources.Length > 0;
		}

		[Required]
		public ITaskItem LicenseTarget {
			get { return (ITaskItem) Bag ["LicenseTarget"]; }
			set { Bag ["LicenseTarget"] = value; }
		}

		public bool NoLogo {
			get { return GetBoolParameterWithDefault ("NoLogo", false); }
			set { Bag ["NoLogo"] = value; }
		}

		public string OutputDirectory {
			get { return (string) Bag ["OutputDirectory"]; }
			set { Bag ["OutputDirectory"] = value; }
		}

		[Output]
		public ITaskItem OutputLicense {
			get { return (ITaskItem) Bag ["OutputLicense"]; }
			set { Bag ["OutputLicense"] = value; }
		}

		public ITaskItem[] ReferencedAssemblies {
			get { return (ITaskItem[]) Bag ["ReferencedAssemblies"]; }
			set { Bag ["ReferencedAssemblies"] = value; }
		}

		[Required]
		public ITaskItem[] Sources {
			get { return (ITaskItem[]) Bag ["Sources"]; }
			set { Bag ["Sources"] = value; }
		}

		protected override string ToolName {
			get {
				return MSBuildUtils.RunningOnWindows ? "lc.bat" : "lc";
			}
		}
	}
}

#endif
