//
// GenerateBootstrapper.cs
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
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {
	public sealed class GenerateBootstrapper : TaskExtension {
	
		string		applicationFile;
		string		applicationName;
		string		applicationUrl;
		string[]	bootstrapperComponentFiles;
		ITaskItem[]	bootstrapperItems;
		string		bootstrapperKeyFile;
		string		componentsLocation;
		string		componentsUrl;
		bool		copyComponents;
		string		culture;
		string		fallbackCulture;
		string		outputPath;
		string		path;
		string		supportUrl;
		bool		validate;
	
		public GenerateBootstrapper ()
		{
		}

		public override bool Execute ()
		{
			return false;
		}
		
		public string ApplicationFile {
			get { return applicationFile; }
			set { applicationFile = value; }
		}
		
		public string ApplicationName {
			get { return applicationName; }
			set { applicationName = value; }
		}
		
		public string ApplicationUrl {
			get { return applicationUrl; }
			set { applicationUrl = value; }
		}
		
		[Output]
		public string[] BootstrapperComponentFiles {
			get { return bootstrapperComponentFiles; }
			set { bootstrapperComponentFiles = value; }
		}
		
		public ITaskItem[] BootstrapperItems {
			get { return bootstrapperItems; }
			set { bootstrapperItems = value; }
		}
		
		[Output]
		public string BootstrapperKeyFile {
			get { return bootstrapperKeyFile; }
			set { bootstrapperKeyFile = value; }
		}
		
		public string ComponentsLocation {
			get { return componentsLocation; }
			set { componentsLocation = value; }
		}
		
		public string ComponentsUrl {
			get { return componentsUrl; }
			set { componentsUrl = value; }
		}
		
		public bool CopyComponents {
			get { return copyComponents; }
			set { copyComponents = value; }
		}
		
		public string Culture {
			get { return culture; }
			set { culture = value; }
		}
		
		public string FallbackCulture {
			get { return fallbackCulture; }
			set { fallbackCulture = value; }
		}
		
		public string OutputPath {
			get { return outputPath; }
			set { outputPath = value; }
		}
		
		public string Path {
			get { return path; }
			set { path = value; }
		}
		
		public string SupportUrl {
			get { return supportUrl; }
			set { supportUrl = value; }
		}
		
		public bool Validate {
			get { return validate; }
			set { validate = value; }
		}
	}
}

#endif
