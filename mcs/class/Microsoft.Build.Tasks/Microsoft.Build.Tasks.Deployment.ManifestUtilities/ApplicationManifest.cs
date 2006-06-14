//
// ApplicationManifest.cs
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

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public sealed class ApplicationManifest : AssemblyManifest {
	
		string			configFile;
		AssemblyReference	entryPoint;
		string			iconFile;
		bool			isClickOnceManifest;
		int			maxTargetPath;
		string			osDescription;
		string			osSupportUrl;
		string			osVersion;
		TrustInfo		trustInfo;
		string			xmlConfigFile;
		AssemblyIdentity	xmlEntryPointIdentity;
		string			xmlEntryPointParameters;
		string			xmlEntryPointPath;
		string			xmlIconFile;
		string			xmlIsClickOnceManifest;
		string			xmlOSBuild;
		string			xmlOSDescription;
		string			xmlOSMajor;
		string			xmlOSMinor;
		string			xmlOSRevision;
		string			xmlOSSupportUrl;
		
		[MonoTODO]
		public ApplicationManifest ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ConfigFile {
			get { return configFile; }
			set { configFile = value; }
		}
		
		[MonoTODO]
		public override AssemblyReference EntryPoint {
			get { return entryPoint; }
			set { entryPoint = value; }
		}
		
		[MonoTODO]
		public string IconFile {
			get { return iconFile; }
			set { iconFile = value; }
		}
		
		[MonoTODO]
		public bool IsClickOnceManifest {
			get { return isClickOnceManifest; }
			set { isClickOnceManifest = value; }
		}
		
		[MonoTODO]
		public int MaxTargetPath {
			get { return maxTargetPath; }
			set { maxTargetPath = value; }
		}
		
		[MonoTODO]
		public string OSDescription {
			get { return osDescription; }
			set { osDescription = value; }
		}
		
		[MonoTODO]
		public string OSSupportUrl {
			get { return osSupportUrl; }
			set { osSupportUrl = value; }
		}
		
		[MonoTODO]
		public string OSVersion {
			get { return osVersion; }
			set { osVersion = value; }
		}
		
		[MonoTODO]
		public TrustInfo TrustInfo {
			get { return trustInfo; }
			set { trustInfo = value; }
		}
		
		[MonoTODO]
		public string XmlConfigFile {
			get { return xmlConfigFile; }
			set { xmlConfigFile = value; }
		}
		
		[MonoTODO]
		public AssemblyIdentity XmlEntryPointIdentity {
			get { return xmlEntryPointIdentity; }
			set { xmlEntryPointIdentity = value; }
		}
		
		[MonoTODO]
		public string XmlEntryPointParameters {
			get { return xmlEntryPointParameters; }
			set { xmlEntryPointParameters = value; }
		}
		
		[MonoTODO]
		public string XmlEntryPointPath {
			get { return xmlEntryPointPath; }
			set { xmlEntryPointPath = value; }
		}
		
		[MonoTODO]
		public string XmlIconFile {
			get { return xmlIconFile; }
			set { xmlIconFile = value; }
		}
		
		[MonoTODO]
		public string XmlIsClickOnceManifest {
			get { return xmlIsClickOnceManifest; }
			set { xmlIsClickOnceManifest = value; }
		}
		
		[MonoTODO]
		public string XmlOSBuild {
			get { return xmlOSBuild; }
			set { xmlOSBuild = value; }
		}
		
		[MonoTODO]
		public string XmlOSDescription {
			get { return xmlOSDescription; }
			set { xmlOSDescription = value; }
		}
		
		[MonoTODO]
		public string XmlOSMajor {
			get { return xmlOSMajor; }
			set { xmlOSMajor = value; }
		}
		
		[MonoTODO]
		public string XmlOSMinor {
			get { return xmlOSMinor; }
			set { xmlOSMinor = value; }
		}
		
		[MonoTODO]
		public string XmlOSRevision {
			get { return xmlOSRevision; }
			set { xmlOSRevision = value; }
		}
		
		[MonoTODO]
		public string XmlOSSupportUrl {
			get { return xmlOSSupportUrl; }
			set { xmlOSSupportUrl = value; }
		}
	}
}

#endif
