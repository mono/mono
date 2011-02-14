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
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

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
			return true;
		}

		protected override void LogEventsFromTextOutput (string singleLine, MessageImportance importance)
		{
			singleLine = singleLine.Trim ();
			if (singleLine.Length == 0)
				return;

			// When IncludeDebugInformation is true, prevents the debug symbols stats from braeking this.
			if (singleLine.StartsWith ("WROTE SYMFILE") ||
				singleLine.StartsWith ("OffsetTable") ||
				singleLine.StartsWith ("Compilation succeeded") ||
				singleLine.StartsWith ("Compilation failed"))
				return;

			Match match = ErrorRegex.Match (singleLine);
			if (!match.Success) {
				Log.LogMessage (importance, singleLine);
				return;
			}

			string filename = match.Result ("${file}") ?? "";

			string line = match.Result ("${line}");
			int lineNumber = !string.IsNullOrEmpty (line) ? Int32.Parse (line) : 0;

			string col = match.Result ("${column}");
			int columnNumber = 0;
			if (!string.IsNullOrEmpty (col))
				columnNumber = col == "255+" ? -1 : Int32.Parse (col);

			string category = match.Result ("${level}");
			string code = match.Result ("${number}");
			string text = match.Result ("${message}");

			if (String.Compare (category, "warning", StringComparison.OrdinalIgnoreCase) == 0) {
				Log.LogWarning (null, code, null, filename, lineNumber, columnNumber, -1,
					-1, text, null);
			} else if (String.Compare (category, "error", StringComparison.OrdinalIgnoreCase) == 0) {
				Log.LogError (null, code, null, filename, lineNumber, columnNumber, -1,
					-1, text, null);
			} else {
				Log.LogMessage (importance, singleLine);
			}
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
			get { return MSBuildUtils.RunningOnWindows ? "vbnc.bat" : "vbnc"; }
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

		// from md's VBBindingCompilerServices.cs
		//matches "/home/path/Default.aspx.vb (40,31) : Error VBNC30205: Expected end of statement."
		//and "Error : VBNC99999: vbnc crashed nearby this location in the source code."
		//and "Error : VBNC99999: Unexpected error: Object reference not set to an instance of an object"
		static Regex errorRegex;
		static Regex ErrorRegex {
			get {
				if (errorRegex == null)
					errorRegex = new Regex (@"^\s*((?<file>.*)\s?\((?<line>\d*)(,(?<column>\d*))?\) : )?(?<level>\w+) :? ?(?<number>[^:]*): (?<message>.*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
				return errorRegex;
			}
		}

	}
}

#endif
