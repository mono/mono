//
// GenerateApplicationManifest.cs
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
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;

namespace Microsoft.Build.Tasks {
	public sealed class GenerateApplicationManifest : GenerateManifestBase {
	
		string		clrVersion;
		ITaskItem	configFile;
		ITaskItem[]	dependencies;
		ITaskItem[]	files;
		ITaskItem	iconFile;
		ITaskItem[]	isolatedComReferences;
		string		manifestType;
		string		osVersion;
		ITaskItem	trustInfoFile;
	
		public GenerateApplicationManifest ()
		{
		}

		[MonoTODO]
		protected override Type GetObjectType ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool OnManifestLoaded (Manifest manifest)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool OnManifestResolved (Manifest manifest)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override bool ValidateInputs ()
		{
			throw new NotImplementedException ();
		}
		
		public string ClrVersion {
			get { return clrVersion; }
			set { clrVersion = value; }
		}
		
		public ITaskItem ConfigFile {
			get { return configFile; }
			set { configFile = value; }
		}
		
		public ITaskItem[] Dependencies {
			get { return dependencies; }
			set { dependencies = value; }
		}
		
		public ITaskItem[] Files {
			get { return files; }
			set { files = value; }
		}
		
		public ITaskItem IconFile {
			get { return iconFile; }
			set { iconFile = value; }
		}
		
		public ITaskItem[] IsolatedComReferences {
			get { return isolatedComReferences; }
			set { isolatedComReferences = value; }
		}
		
		public string ManifestType {
			get { return manifestType; }
			set { manifestType = value; }
		}
		
		public string OSVersion {
			get { return osVersion; }
			set { osVersion = value; }
		}
		
		public ITaskItem TrustInfoFile {
			get { return trustInfoFile; }
			set { trustInfoFile = value; }
		}
	}
}

#endif
