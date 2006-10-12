//
// BuildSettings.cs
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
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.Bootstrapper {

	[ClassInterface (ClassInterfaceType.None)]
	[Guid ("5D13802C-C830-4b41-8E7A-F69D9DD6A095")]
	[ComVisible (true)]
	public class BuildSettings : IBuildSettings {
		
		string	applicationFile;
		string	applicationName;
		string	applicationUrl;
		ComponentsLocation	componentsLocation;
		string	componentsUrl;
		bool	copyComponents;
		int	fallbackLCID;
		int	lcid;
		string	outputPath;
		ProductBuilderCollection	productBuilders;
		string	supportUrl;
		bool	validate;
		
		public BuildSettings ()
		{
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
		
		public ComponentsLocation ComponentsLocation {
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
		
		public int FallbackLCID {
			get { return fallbackLCID; }
			set { fallbackLCID = value; }
		}
		
		public int LCID {
			get { return lcid; }
			set { lcid = value; }
		}
		
		public string OutputPath {
			get { return outputPath; }
			set { outputPath = value; }
		}
		
		public ProductBuilderCollection ProductBuilders {
			get { return productBuilders; }
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
