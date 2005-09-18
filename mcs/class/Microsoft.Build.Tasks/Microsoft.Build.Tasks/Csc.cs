//
// Csc.cs: Task that runs C# compiler
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

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks.Hosting;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class Csc : ManagedCompiler {
	
		public Csc ()
		{
		}

		// FIXME: move some commands to managedcompiler
		protected internal override void AddResponseFileCommands (CommandLineBuilderExtension commandLine)
		{
			base.AddResponseFileCommands (commandLine);
			
			commandLine.AppendSwitchIfNotNull ("/lib:", AdditionalLibPaths, ",");
			commandLine.AppendSwitchIfNotNull ("/addmodule:", AddModules, ",");
			if (AllowUnsafeBlocks == true)
				commandLine.AppendSwitch ("/unsafe");
			//baseAddress
			//checkForOverflowUnderflow
			//commandLine.AppendSwitchIfNotNull ("/codepage:", CodePage.ToString ());
			//debugType
			commandLine.AppendSwitchIfNotNull ("/define:", DefineConstants);
			//delaySign
			commandLine.AppendSwitchIfNotNull ("/nowarn:", DisabledWarnings);
			commandLine.AppendSwitchIfNotNull ("/doc:", DocumentationFile);
			if (EmitDebugInformation)
				commandLine.AppendSwitch ("/debug");
			//errorReport
			//fileAlignment
			commandLine.AppendSwitchIfNotNull ("/keycontainer:", KeyContainer);
			commandLine.AppendSwitchIfNotNull ("/keyfile:", KeyFile);
			commandLine.AppendSwitchIfNotNull ("/langversion:", LangVersion);
			// FIXME: add ids from metadata
			if (LinkResources != null) {
				foreach (ITaskItem item in LinkResources) {
					if (GenerateFullPaths)
						commandLine.AppendSwitchIfNotNull ("/linkresource:", item.GetMetadata ("FullPath"));
					else
						commandLine.AppendSwitchIfNotNull ("/linkresource:", item.ItemSpec);
				}
			}
			commandLine.AppendSwitchIfNotNull ("/main:", MainEntryPoint);
			//moduleAssemblyName
			if (NoConfig)
				commandLine.AppendSwitch ("/noconfig");
			if (NoStandardLib)
				commandLine.AppendSwitch ("/nostdlib");
			if (Optimize)
				commandLine.AppendSwitch ("/optimize");
			commandLine.AppendSwitchIfNotNull ("/out:", OutputAssembly.ItemSpec);
			//platform
			if (References != null) {
				foreach (ITaskItem item in References) {
					commandLine.AppendSwitchIfNotNull ("/reference:", item.ItemSpec);
				}
			}
			if (Resources != null) {
				foreach (ITaskItem item in Resources) {
					if (GenerateFullPaths)
						commandLine.AppendSwitchIfNotNull ("/resource:", item.GetMetadata ("FullPath"));
					else
						commandLine.AppendSwitchIfNotNull ("/resource:", item.ItemSpec);
				}
			}
			if (ResponseFiles != null) {
				foreach (ITaskItem item in ResponseFiles) {
					if (GenerateFullPaths)
						commandLine.AppendFileNameIfNotNull (String.Format ("@{0}",item.GetMetadata ("FullPath")));
					else
						commandLine.AppendFileNameIfNotNull (String.Format ("@{0}",item.ItemSpec));
				}
			}
			if (Sources != null) {
				foreach (ITaskItem item in Sources) {
					if (GenerateFullPaths)
						commandLine.AppendFileNameIfNotNull (item.GetMetadata ("FullPath"));
					else
						commandLine.AppendFileNameIfNotNull (item.ItemSpec);
				}
			}
			commandLine.AppendSwitchIfNotNull ("/target:", TargetType);
			if (TreatWarningsAsErrors)
				commandLine.AppendSwitch ("/warnaserror");
			commandLine.AppendSwitchIfNotNull ("/warn:", WarningLevel.ToString ());
			//warningsAsErrors
			//warningNotAsErrors
			commandLine.AppendSwitchIfNotNull ("/win32icon:", Win32Icon);
			commandLine.AppendSwitchIfNotNull ("/win32res:", Win32Resource);
		}

		protected override bool CallHostObjectToExecute ()
		{
			return true;
		}

		protected override string GenerateFullPathToTool ()
		{
			return "/usr/local/bin/mcs";
		}

		protected override bool InitializeHostObject (out bool appropriateHostObjectExists,
							      out bool continueBuild)
		{
			appropriateHostObjectExists = true;
			continueBuild = true;
			return true;
		}

		public bool AllowUnsafeBlocks {
			get { return GetBoolParameterWithDefault ("AllowUnsafeBlocks", false); }
			set { Bag ["AllowUnsafeBlocks"] = value; }
		}

		public string BaseAddress {
			get { return (string) Bag ["BaseAddress"]; }
			set { Bag ["BaseAddress"] = value; }
		}

		public bool CheckForOverflowUnderFlow {
			get { return GetBoolParameterWithDefault ("CheckForOverflowUnderFlow", false); }
			set { Bag ["CheckForOverflowUnderFlow"] = value; }
		}

		public string DisabledWarnings {
			get { return (string) Bag ["DisabledWarnings"]; }
			set { Bag ["DisabledWarnings"] = value; }
		}

		public string DocumentationFile {
			get { return (string) Bag ["DocumentationFile"]; }
			set { Bag ["DocumentationFile"] = value; }
		}

		public string ErrorReport {
			get { return (string) Bag ["ErrorReport"]; }
			set { Bag ["ErrorReport"] = value; }
		}

		public bool GenerateFullPaths {
			get { return GetBoolParameterWithDefault ("GenerateFullPaths", false); }
			set { Bag ["GenerateFullPaths"] = value; }
		}

		public string LangVersion {
			get { return (string) Bag ["LangVersion"]; }
			set { Bag ["LangVersion"] = value; }
		}

		public string ModuleAssemblyName {
			get { return (string) Bag ["ModuleAssemblyName"]; }
			set { Bag ["ModuleAssemblyName"] = value; }
		}

		public bool NoStandardLib {
			get { return GetBoolParameterWithDefault ("NoStandardLib", false); }
			set { Bag ["NoStandardLib"] = value; }
		}

		public string Platform {
			get { return (string) Bag ["Platform"]; }
			set { Bag ["Platform"] = value; }
		}

		protected override string ToolName {
			get { return "mcs"; }
		}

		public bool UseHostCompilerIfAvailable {
			get { return GetBoolParameterWithDefault ("UseHostCompilerIfAvailable", false); }
			set { Bag ["UseHostCompilerIfAvailable"] = value; }
		}

		public int WarningLevel {
			get { return GetIntParameterWithDefault ("WarningLevel", 2); }
			set { Bag ["WarningLevel"] = value; }
		}

		public string WarningsAsErrors {
			get { return (string) Bag ["WarningsAsErrors"]; }
			set { Bag ["WarningsAsErrors"] = value; }
		}

		public string WarningsNotAsErrors {
			get { return (string) Bag ["WarningsNotAsErrors"]; }
			set { Bag ["WarningsNotAsErrors"] = value; }
		}
	}
}
