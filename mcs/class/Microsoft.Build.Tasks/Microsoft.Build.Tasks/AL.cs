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

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class AL : ToolTaskExtension {
	
		// FIXME: replace all variables with Bag
		string		algorithmId;
		string		baseAddress;
		string		companyName;
		string		configuration;
		string		copyright;
		string		culture;
		bool		delaySign;
		string		description;
		ITaskItem[]	embedResources;
		string		evidenceFile;
		string		fileVersion;
		string		flags;
		bool		generateFullPaths;
		string		keyContainer;
		string		keyFile;
		ITaskItem[]	linkResources;
		string		mainEntryPoint;
		ITaskItem	outputAssembly;
		string		platform;
		string		productName;
		string		productVersion;
		string[]	responseFiles;
		ITaskItem[]	sourceModules;
		string		targetType;
		string		templateFile;
		string		title;
		string		trademark;
		string		version;
		string		win32Icon;
		string		win32Resource;

		Process	alProcess;
	
		public AL ()
		{
		}
		
		protected internal override void AddResponseFileCommands (
						 CommandLineBuilderExtension commandLine)
		{
			commandLine.AppendSwitchIfNotNull ("/algid:", algorithmId);
			commandLine.AppendSwitchIfNotNull ("/baseaddress:", baseAddress);
			commandLine.AppendSwitchIfNotNull ("/company:", companyName);
			commandLine.AppendSwitchIfNotNull ("/configuration:", configuration);
			commandLine.AppendSwitchIfNotNull ("/copyright:", copyright);
			commandLine.AppendSwitchIfNotNull ("/culture:", culture);
			if (delaySign == true)
				commandLine.AppendSwitch ("/delaysign");
			foreach (ITaskItem item in embedResources)
				commandLine.AppendSwitchIfNotNull ("/embedresource:", item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/evidence:", evidenceFile);
			commandLine.AppendSwitchIfNotNull ("/fileversion:", fileVersion);
			commandLine.AppendSwitchIfNotNull ("/flags:", flags);
			if (generateFullPaths == true)
				commandLine.AppendSwitch ("/fullpaths");
			commandLine.AppendSwitchIfNotNull ("/keyname:", keyContainer);
			commandLine.AppendSwitchIfNotNull ("/keyfile:", keyFile);
			foreach (ITaskItem item in linkResources)
				commandLine.AppendSwitchIfNotNull ("/linkresource:", item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/main:", mainEntryPoint);
			commandLine.AppendSwitchIfNotNull ("/out:", outputAssembly.ItemSpec);
			//platform
			commandLine.AppendSwitchIfNotNull ("/product:", productName);
			commandLine.AppendSwitchIfNotNull ("/productversion:", productVersion);
			foreach (string s in responseFiles)
				commandLine.AppendFileNameIfNotNull (String.Format ("@{0}", s));
			foreach (ITaskItem item in sourceModules)
				commandLine.AppendFileNameIfNotNull (item.ItemSpec);
			commandLine.AppendSwitchIfNotNull ("/target:", targetType);
			commandLine.AppendSwitchIfNotNull ("/template:", templateFile);
			commandLine.AppendSwitchIfNotNull ("/title:", title);
			commandLine.AppendSwitchIfNotNull ("/trademark:", trademark);
			commandLine.AppendSwitchIfNotNull ("/version:", version);
			commandLine.AppendSwitchIfNotNull ("/win32icon:", win32Icon);
			commandLine.AppendSwitchIfNotNull ("/win32res:", win32Resource);
		}
		
		public override bool Execute ()
		{
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();
			AddResponseFileCommands (clbe);
			
			alProcess = new Process ();
			alProcess.StartInfo.Arguments = clbe.ToString ();
			alProcess.StartInfo.FileName = GenerateFullPathToTool ();
			alProcess.Start ();
			alProcess.WaitForExit ();
			return true;
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolName);
		}

		public string AlgorithmId {
			get { return algorithmId; }
			set { algorithmId = value; }
		}

		public string BaseAddress {
			get { return baseAddress; }
			set { baseAddress = value; }
		}

		public string CompanyName {
			get { return companyName; }
			set { companyName = value; }
		}

		public string Configuration {
			get { return configuration; }
			set { configuration = value; }
		}

		public string Copyright {
			get { return copyright; }
			set { copyright = value; }
		}

		public string Culture {
			get { return culture; }
			set { culture = value; }
		}

		public bool DelaySign {
			get { return delaySign; }
			set { delaySign = value; }
		}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public ITaskItem[] EmbedResources {
			get { return embedResources; }
			set { embedResources = value; }
		}

		public string EvidenceFile {
			get { return evidenceFile; }
			set { evidenceFile = value; }
		}

		public string FileVersion {
			get { return fileVersion; }
			set { fileVersion = value; }
		}

		public string Flags {
			get { return flags; }
			set { flags = value; }
		}

		public bool GenerateFullPaths {
			get { return generateFullPaths; }
			set { generateFullPaths = value; }
		}

		public string KeyContainer {
			get { return keyContainer; }
			set { keyContainer = value; }
		}

		public string KeyFile {
			get { return keyFile; }
			set { keyFile = value; }
		}

		public ITaskItem[] LinkResources {
			get { return linkResources; }
			set { linkResources = value; }
		}

		public string MainEntryPoint {
			get { return mainEntryPoint; }
			set { mainEntryPoint = value; }
		}

		[Required]
		public ITaskItem OutputAssembly {
			get { return outputAssembly; }
			set { outputAssembly = value; }
		}

		public string Platform {
			get { return platform; }
			set { platform = value; }
		}

		public string ProductName {
			get { return productName; }
			set { productName = value; }
		}

		public string ProductVersion {
			get { return productVersion; }
			set { productVersion = value; }
		}

		public string[] ResponseFiles {
			get { return responseFiles; }
			set { responseFiles = value; }
		}

		public ITaskItem[] SourceModules {
			get { return sourceModules; }
			set { sourceModules = value; }
		}

		public string TargetType {
			get { return targetType; }
			set { targetType = value; }
		}

		public string TemplateFile {
			get { return templateFile; }
			set { templateFile = value; }
		}

		public string Title {
			get { return title; }
			set { title = value; }
		}

		protected override string ToolName {
			get {
				return "al";
			}
		}

		public string Trademark {
			get { return trademark; }
			set { trademark = value; }
		}

		public string Version {
			get { return version; }
			set { version = value; }
		}

		public string Win32Icon {
			get { return win32Icon; }
			set { win32Icon = value; }
		}

		public string Win32Resource {
			get { return win32Resource; }
			set { win32Resource = value; }
		}
	}
}
