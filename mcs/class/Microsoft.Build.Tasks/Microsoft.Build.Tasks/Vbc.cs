//
// UpdateManifest.cs
//
// Author:
//      Leszek Ciesielski  <skolima@gmail.com>
//      Marek Sieradzki  <marek.sieradzki@gmail.com>
//
// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
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
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {

	public class Vbc : ManagedCompiler {

		public Vbc ()
		{
		}

		[MonoTODO]
		protected internal override void AddResponseFileCommands (
				CommandLineBuilderExtension commandLine )
		{
			base.AddResponseFileCommands (commandLine);

			commandLine.AppendSwitchIfNotNull ("/libpath:", AdditionalLibPaths, ",");

			commandLine.AppendSwitchIfNotNull ("/baseaddress:", BaseAddress);

			if (DefineConstants != null)
				commandLine.AppendSwitchUnquotedIfNotNull ("/define:",
						String.Format ("\"{0}\"", EscapeDoubleQuotes (DefineConstants)));

			// DisabledWarnings

			commandLine.AppendSwitchIfNotNull ("/doc:", DocumentationFile);

			// ErrorReport
			
			// GenerateDocumentation
			
			if (Imports != null)
				foreach (ITaskItem item in Imports)
					commandLine.AppendSwitchIfNotNull ("/imports:", item.ItemSpec);
			
			commandLine.AppendSwitchIfNotNull ("/main:", MainEntryPoint);

			// NoStandardLib
			
			if (NoWarnings)
				commandLine.AppendSwitch ("/nowarn");

			commandLine.AppendSwitchIfNotNull ("/optioncompare:", OptionCompare);

			if (Bag ["OptionExplicit"] != null)
				if (OptionExplicit)
					commandLine.AppendSwitch ("/optionexplicit+");
				else
					commandLine.AppendSwitch ("/optionexplicit-");

			if (Bag ["OptionStrict"] != null)
				if (OptionStrict)
					commandLine.AppendSwitch ("/optionstrict+");
				else
					commandLine.AppendSwitch ("/optionstrict-");

			// OptionStrictType
			
			// Platform
			
			if (References != null)
				foreach (ITaskItem item in References)
					commandLine.AppendSwitchIfNotNull ("/reference:", item.ItemSpec);
	
			if (Bag ["RemoveIntegerChecks"] != null)
				if (RemoveIntegerChecks)
					commandLine.AppendSwitch ("/removeintchecks+");
				else
					commandLine.AppendSwitch ("/removeintchecks-");

			if (ResponseFiles != null)
				foreach (ITaskItem item in ResponseFiles)
					commandLine.AppendFileNameIfNotNull (String.Format ("@{0}", item.ItemSpec));

			commandLine.AppendSwitchIfNotNull ("/rootnamespace:", RootNamespace);

			commandLine.AppendSwitchIfNotNull ("/sdkpath:", SdkPath);

			// TargetCompactFramework
			
			// Verbosity

			// WarningsAsErrors

			// WarningsNotAsErrors

		}

		string EscapeDoubleQuotes (string text)
		{
			if (text == null)
				return null;

			return text.Replace ("\"", "\\\"");
		}
		
		[MonoTODO]
		protected override bool CallHostObjectToExecute ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}
		
		[MonoTODO]
		protected override HostObjectInitializationStatus InitializeHostObject ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string BaseAddress {
			get { return (string) Bag ["BaseAddress"]; }
			set { Bag ["BaseAddress"] = value; }
		}
		
		[MonoTODO]
		public string DisabledWarnings  {
			get { return (string) Bag ["DisabledWarnings"]; }
			set { Bag ["DisabledWarnings"] = value; }
		}
		
		[MonoTODO]
		public string DocumentationFile {
			get { return (string) Bag ["DocumentationFile"]; }
			set { Bag ["DocumentationFile"] = value; }
		}
		
		[MonoTODO]
		public string ErrorReport {
			get { return (string) Bag ["ErrorReport"]; }
			set { Bag ["ErrorReport"] = value; }
		}
		
		[MonoTODO]
		public bool GenerateDocumentation {
			get { return GetBoolParameterWithDefault ("GenerateDocumentation", false); }
			set { Bag ["GenerateDocumentation"] = value; }
		}
		
		[MonoTODO]
		public ITaskItem [] Imports {
			get { return (ITaskItem []) Bag ["Imports"]; }
			set { Bag ["Imports"] = value; }
		}
		
		[MonoTODO]
		public bool NoStandardLib {
			get { return GetBoolParameterWithDefault ("NoStandardLib", false); }
			set { Bag ["NoStandardLib"] = value; }
		}
		
		[MonoTODO]
		public bool NoWarnings {
			get { return GetBoolParameterWithDefault ("NoWarnings", false); }
			set { Bag ["NoWarnings"] = value; }
		}
		
		[MonoTODO]
		public string OptionCompare {
			get { return (string) Bag ["OptionCompare"]; }
			set { Bag ["OptionCompare"] = value; }
		}
		
		[MonoTODO]
		public bool OptionExplicit {
			get { return GetBoolParameterWithDefault ("OptionExplicit", false); }
			set { Bag ["OpExplicit"] = value; }
		}
		
		[MonoTODO]
		public bool OptionStrict {
			get { return GetBoolParameterWithDefault ("OptionStrict", false); }
			set { Bag ["OptionStrict"] = value; }
		}
		
		[MonoTODO]
		public string OptionStrictType {
			get { return (string) Bag ["OptionStrictType"]; }
			set { Bag ["OptionStrictType"] = value; }
		}
		
		[MonoTODO]
		public string Platform {
			get { return (string) Bag ["Platfrom"]; }
			set { Bag ["Platform"] = value; }
		}
		
		[MonoTODO]
		public bool RemoveIntegerChecks {
			get { return GetBoolParameterWithDefault ("RemoveIntegerChecks", false); }
			set { Bag ["RemoveIntegerChecks"] = value; }
		}

		[MonoTODO]
		public string RootNamespace {
			get { return (string) Bag ["RootNamespace"]; }
			set { Bag ["RootNamespace"] = value; }
		}

		[MonoTODO]
		public string SdkPath {
			get { return (string) Bag ["SdkPath"]; }
			set { Bag ["SdkPath"] = value; }
		}

		[MonoTODO]
		public bool TargetCompactFramework {
			get { return (bool) Bag ["TargetCompactFramework"]; }
			set { Bag ["TargetCompactFramework"] = value; }
		}

		[MonoTODO]
		protected override string ToolName {
			get { return Utilities.RunningOnWindows ? "vbnc.bat" : "vbnc"; }
		}

		[MonoTODO]
		public bool UseHostCompilerIfAvailable {
			get { return (bool) Bag ["UseHostCompilerIfAvailable"]; }
			set { Bag ["UseHostCompilerIfAvailable"] = value; }
		}

		[MonoTODO]
		public string Verbosity {
			get { return (string) Bag ["Verbosity"]; }
			set { Bag ["Verbosity"] = value; }
		}

		[MonoTODO]
		public string WarningsAsErrors {
			get { return (string) Bag ["WarningsAsErrors"]; }
			set { Bag ["WarningsAsErrors"] = value; }
		}
		
		[MonoTODO]
		public string WarningsNotAsErrors {
			get { return (string) Bag ["WarningsNotAsErrors"]; }
			set { Bag ["WarningsNotAsErrors"] = value; }
		}
	}
}

#endif
