//
// AL.cs: Task for assembly linker
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
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Tasks {
	public class AL : ToolTaskExtension {
	
		public AL ()
		{
		}
		
		[MonoTODO]
		protected internal override void AddResponseFileCommands (
						 CommandLineBuilderExtension commandLine)
		{
			commandLine.AppendSwitchIfNotNull ("/algid:", AlgorithmId);
			commandLine.AppendSwitchIfNotNull ("/baseaddress:", BaseAddress);
			commandLine.AppendSwitchIfNotNull ("/company:", CompanyName);
			commandLine.AppendSwitchIfNotNull ("/configuration:", Configuration);
			commandLine.AppendSwitchIfNotNull ("/culture:", Culture);
			commandLine.AppendSwitchIfNotNull ("/copyright:", Copyright);
			if (Bag ["DelaySign"] != null)
				if (DelaySign)
					commandLine.AppendSwitch ("/delaysign+");
				else
					commandLine.AppendSwitch ("/delaysign-");
			commandLine.AppendSwitchIfNotNull ("/description:", Description);
			if (EmbedResources != null)
				foreach (ITaskItem item in EmbedResources)
					commandLine.AppendSwitchIfNotNull ("/embed:", item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/evidence:", EvidenceFile);
			commandLine.AppendSwitchIfNotNull ("/fileversion:", FileVersion);
			commandLine.AppendSwitchIfNotNull ("/flags:", Flags);
			if (GenerateFullPaths)
				commandLine.AppendSwitch ("/fullpaths");
			commandLine.AppendSwitchIfNotNull ("/keyname:", KeyContainer);
			commandLine.AppendSwitchIfNotNull ("/keyfile:", KeyFile);
			if (LinkResources != null)
				foreach (ITaskItem item in LinkResources)
					commandLine.AppendSwitchIfNotNull ("/link:", item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/main:", MainEntryPoint);
			if (OutputAssembly != null)
				commandLine.AppendSwitchIfNotNull ("/out:", OutputAssembly.ItemSpec);
			//platform
			commandLine.AppendSwitchIfNotNull ("/product:", ProductName);
			commandLine.AppendSwitchIfNotNull ("/productversion:", ProductVersion);
			if (ResponseFiles != null)
				foreach (string s in ResponseFiles)
					commandLine.AppendFileNameIfNotNull (String.Format ("@{0}", s));
			if (SourceModules != null)
				foreach (ITaskItem item in SourceModules)
					commandLine.AppendFileNameIfNotNull (item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/target:", TargetType);
			commandLine.AppendSwitchIfNotNull ("/template:", TemplateFile);
			commandLine.AppendSwitchIfNotNull ("/title:", Title);
			commandLine.AppendSwitchIfNotNull ("/trademark:", Trademark);
			commandLine.AppendSwitchIfNotNull ("/version:", Version);
			commandLine.AppendSwitchIfNotNull ("/win32icon:", Win32Icon);
			commandLine.AppendSwitchIfNotNull ("/win32res:", Win32Resource);
		}
		
		public override bool Execute ()
		{
			return base.Execute ();
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}

		public string AlgorithmId {
			get { return (string) Bag ["AlgorithmId"]; }
			set { Bag ["AlgorithmId"] = value; }
		}

		public string BaseAddress {
			get { return (string) Bag ["BaseAddress"]; }
			set { Bag ["BaseAddress"] = value; }
		}

		public string CompanyName {
			get { return (string) Bag ["CompanyName"]; }
			set { Bag ["CompanyName"] = value; }
		}

		public string Configuration {
			get { return (string) Bag ["Configuration"]; }
			set { Bag ["Configuration"] = value; }
		}

		public string Copyright {
			get { return (string) Bag ["Copyright"]; }
			set { Bag ["Copyright"] = value; }
		}

		public string Culture {
			get { return (string) Bag ["Culture"]; }
			set { Bag ["Culture"] = value; }
		}

		public bool DelaySign {
			get { return GetBoolParameterWithDefault ("DelaySign", false); }
			set { Bag ["DelaySign"] = value; }
		}

		public string Description {
			get { return (string) Bag ["Description"]; }
			set { Bag ["Description"] = value; }
		}

		public ITaskItem[] EmbedResources {
			get { return (ITaskItem[]) Bag ["EmbedResources"]; }
			set { Bag ["EmbedResources"] = value; }
		}

		public string EvidenceFile {
			get { return (string) Bag ["EvidenceFile"]; }
			set { Bag ["EvidenceFile"] = value; }
		}

		public string FileVersion {
			get { return (string) Bag ["FileVersion"]; }
			set { Bag ["FileVersion"] = value; }
		}

		public string Flags {
			get { return (string) Bag ["Flags"]; }
			set { Bag ["Flags"] = value; }
		}

		public bool GenerateFullPaths {
			get { return GetBoolParameterWithDefault ("GenerateFullPaths", false); }
			set { Bag ["GenerateFullPaths"] = value; }
		}

		public string KeyContainer {
			get { return (string) Bag ["KeyContainer"]; }
			set { Bag ["KeyContainer"] = value; }
		}

		public string KeyFile {
			get { return (string) Bag ["KeyFile"]; }
			set { Bag ["KeyFile"] = value; }
		}

		public ITaskItem[] LinkResources {
			get { return (ITaskItem[]) Bag ["LinkResources"]; }
			set { Bag ["LinkResources"] = value; }
		}

		public string MainEntryPoint {
			get { return (string) Bag ["MainEntryPoint"]; }
			set { Bag ["MainEntryPoint"] = value; }
		}

		[Required]
		[Output]
		public ITaskItem OutputAssembly {
			get { return (ITaskItem) Bag ["OutputAssembly"]; }
			set { Bag ["OutputAssembly"] = value; }
		}

		public string Platform {
			get { return (string) Bag ["Platform"]; }
			set { Bag ["Platform"] = value; }
		}

		public string ProductName {
			get { return (string) Bag ["ProductName"]; }
			set { Bag ["ProductName"] = value; }
		}

		public string ProductVersion {
			get { return (string) Bag ["ProductVersion"]; }
			set { Bag ["ProductVersion"] = value; }
		}

		public string[] ResponseFiles {
			get { return (string[]) Bag ["ResponseFiles"]; }
			set { Bag ["ResponseFiles"] = value; }
		}

		public ITaskItem[] SourceModules {
			get { return (ITaskItem[]) Bag ["SourceModules"]; }
			set { Bag ["SourceModules"] = value; }
		}

		public string TargetType {
			get { return (string) Bag ["TargetType"]; }
			set { Bag ["TargetType"] = value; }
		}

		public string TemplateFile {
			get { return (string) Bag ["TemplateFile"]; }
			set { Bag ["TemplateFile"] = value; }
		}

		public string Title {
			get { return (string) Bag ["Title"]; }
			set { Bag ["Title"] = value; }
		}

		protected override string ToolName {
			get {
				return MSBuildUtils.RunningOnWindows ? "al.bat" : "al";
			}
		}

		public string Trademark {
			get { return (string) Bag ["Trademark"]; }
			set { Bag ["Trademark"] = value; }
		}

		public string Version {
			get { return (string) Bag ["Version"]; }
			set { Bag ["Version"] = value; }
		}

		public string Win32Icon {
			get { return (string) Bag ["Win32Icon"]; }
			set { Bag ["Win32Icon"] = value; }
		}

		public string Win32Resource {
			get { return (string) Bag ["Win32Resource"]; }
			set { Bag ["Win32Resource"] = value; }
		}
	}
}

#endif
