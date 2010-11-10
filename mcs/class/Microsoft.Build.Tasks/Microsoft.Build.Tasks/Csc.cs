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

#if NET_2_0

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

		protected internal override void AddResponseFileCommands (CommandLineBuilderExtension commandLine)
		{
			base.AddResponseFileCommands (commandLine);

			if (AdditionalLibPaths != null && AdditionalLibPaths.Length > 0)
				commandLine.AppendSwitchIfNotNull ("/lib:", AdditionalLibPaths, ",");

			if (Bag ["AllowUnsafeBlocks"] != null)
				if (AllowUnsafeBlocks)
					commandLine.AppendSwitch ("/unsafe+");
				else
					commandLine.AppendSwitch ("/unsafe-");

			//baseAddress
			
			if (Bag ["CheckForOverflowUnderflow"] != null)
				if (CheckForOverflowUnderflow)
					commandLine.AppendSwitch ("/checked+");
				else
					commandLine.AppendSwitch ("/checked-");

			if (!String.IsNullOrEmpty (DefineConstants)) {
				string [] defines = DefineConstants.Split (new char [] {';', ' '},
						StringSplitOptions.RemoveEmptyEntries);
				if (defines.Length > 0)
					commandLine.AppendSwitchIfNotNull ("/define:",
							String.Join (";", defines));
			}

			commandLine.AppendSwitchIfNotNull ("/nowarn:", DisabledWarnings);

			commandLine.AppendSwitchIfNotNull ("/doc:", DocumentationFile);

			//errorReport

			if (GenerateFullPaths)
				commandLine.AppendSwitch ("/fullpaths");

			commandLine.AppendSwitchIfNotNull ("/langversion:", LangVersion);

			commandLine.AppendSwitchIfNotNull ("/main:", MainEntryPoint);

			//moduleAssemblyName
			
			if (NoStandardLib)
				commandLine.AppendSwitch ("/nostdlib");

			//platform
			commandLine.AppendSwitchIfNotNull ("/platform:", Platform);
			//
			if (References != null)
				foreach (ITaskItem item in References)
					commandLine.AppendSwitchIfNotNull ("/reference:", item.ItemSpec);

			if (ResponseFiles != null)
				foreach (ITaskItem item in ResponseFiles) 
					commandLine.AppendSwitchIfNotNull ("@", item.ItemSpec);

			if (Bag ["WarningLevel"] != null)
				commandLine.AppendSwitchIfNotNull ("/warn:", WarningLevel.ToString ());

			commandLine.AppendSwitchIfNotNull ("/warnaserror+:", WarningsAsErrors);

			commandLine.AppendSwitchIfNotNull ("/warnaserror-:", WarningsNotAsErrors);

			if (Win32Resource != null)
				commandLine.AppendSwitchIfNotNull ("/win32res:", Win32Resource);
		}

		[MonoTODO]
		protected override bool CallHostObjectToExecute ()
		{
			throw new NotImplementedException ();
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}

		[MonoTODO]
		protected override HostObjectInitializationStatus InitializeHostObject ()
		{
			return HostObjectInitializationStatus.NoActionReturnSuccess;
		}

		public bool AllowUnsafeBlocks {
			get { return GetBoolParameterWithDefault ("AllowUnsafeBlocks", false); }
			set { Bag ["AllowUnsafeBlocks"] = value; }
		}

		public string BaseAddress {
			get { return (string) Bag ["BaseAddress"]; }
			set { Bag ["BaseAddress"] = value; }
		}

		public bool CheckForOverflowUnderflow {
			get { return GetBoolParameterWithDefault ("CheckForOverflowUnderflow", false); }
			set { Bag ["CheckForOverflowUnderflow"] = value; }
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
		
		public string PdbFile {
			get { return (string) Bag ["PdbFile"]; }
			set { Bag ["PdbFile"] = value; }
		}

		public string Platform {
			get { return (string) Bag ["Platform"]; }
			set { Bag ["Platform"] = value; }
		}

		protected override string ToolName {
			get {
#if NET_4_0
				return Utilities.RunningOnWindows ? "dmcs.bat" : "dmcs";
#else
				return Utilities.RunningOnWindows ? "gmcs.bat" : "gmcs";
#endif
			}
		}

		public bool UseHostCompilerIfAvailable {
			get { return GetBoolParameterWithDefault ("UseHostCompilerIfAvailable", false); }
			set { Bag ["UseHostCompilerIfAvailable"] = value; }
		}

		public int WarningLevel {
			get { return GetIntParameterWithDefault ("WarningLevel", 4); }
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

#endif
