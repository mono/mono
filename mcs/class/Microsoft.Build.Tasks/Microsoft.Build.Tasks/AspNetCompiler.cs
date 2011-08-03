//
// AspNetCompiler.cs: Task for ASP .NET compiler
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
using System.IO;
using Microsoft.Build.Framework;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Tasks {
	public class AspNetCompiler : ToolTaskExtension {
	
		bool	allowPartiallyTrustedCallers;
		bool	clean;
		bool	debug;
		bool	delaySign;
		bool	fixedNames;
		bool	force;
		string	keyContainer;
		string	keyFile;
		string	metabasePath;
		string	physicalPath;
		string	targetPath;
		bool	updateable;
		string	virtualPath;
	
		public AspNetCompiler ()
		{
		}
		
		[MonoTODO]
		public override bool Execute ()
		{
			return false;
		}
		
		[MonoTODO]
		protected internal override void AddCommandLineCommands (CommandLineBuilderExtension commandLine)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			throw new NotImplementedException ();
		}
		
		public bool AllowPartiallyTrustedCallers {
			get { return allowPartiallyTrustedCallers; }
			set { allowPartiallyTrustedCallers = value; }
		}
		
		public bool Clean {
			get { return clean; }
			set { clean = value; }
		}
		
		public bool Debug {
			get { return debug; }
			set { debug = value; }
		}
		
		public bool DelaySign {
			get { return delaySign; }
			set { delaySign = value; }
		}
		
		public bool FixedNames {
			get { return fixedNames; }
			set { fixedNames = value; }
		}
		
		public bool Force {
			get { return force; }
			set { force = value; }
		}
		
		public string KeyContainer {
			get { return keyContainer; }
			set { keyContainer = value; }
		}
		
		public string KeyFile {
			get { return keyFile; }
			set { keyFile = value; }
		}
		
		public string MetabasePath {
			get { return metabasePath; }
			set { metabasePath = value; }
		}
		
		public string PhysicalPath {
			get { return physicalPath; }
			set { physicalPath = value; }
		}
		
		public string TargetPath {
			get { return targetPath; }
			set { targetPath = value; }
		}
		
		public bool Updateable {
			get { return updateable; }
			set { updateable = value; }
		}
		
		public string VirtualPath {
			get { return virtualPath; }
			set { virtualPath = value; }
		}
		
		protected override string ToolName {
			get { return MSBuildUtils.RunningOnWindows ? "aspnet_compiler.bat" : "aspnet_compiler"; }
		}
	}
}

#endif
