//
// AssemblyIdentity.cs
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
	public sealed class AssemblyIdentity {
	
		string	culture;
		bool	isFrameworkAssembly;
		bool	isNeutralPlatform;
		bool	isStrongName;
		string	name;
		string	processorArchitecture;
		string	publicKeyToken;
		string	type;
		string	version;
		string	xmlCulture;
		string	xmlName;
		string	xmlProcessorArchitecture;
		string	xmlPublicKeyToken;
		string	xmlType;
		string	xmlVersion;
	
		[MonoTODO]
		public AssemblyIdentity ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public AssemblyIdentity (AssemblyIdentity identity)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public AssemblyIdentity (string name)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public AssemblyIdentity (string name, string version)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public AssemblyIdentity (string name, string version,
					 string publicKeyToken, string culture)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AssemblyIdentity (string name, string version,
					 string publicKeyToken, string culture,
					 string processorArchitecture)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AssemblyIdentity (string name, string version,
					 string publicKeyToken, string culture,
					 string processorArchitecture,
					 string type)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static AssemblyIdentity FromAssemblyName (string assemblyName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static AssemblyIdentity FromFile (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static AssemblyIdentity FromManagedAssembly (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static AssemblyIdentity FromManifest (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static AssemblyIdentity FromNativeAssembly (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string GetFullName (FullNameFlags flags)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string Culture {
			get { return culture; }
			set { culture = value; }
		}
		
		[MonoTODO]
		public bool IsFrameworkAssembly {
			get { return isFrameworkAssembly; }
		}
		
		[MonoTODO]
		public bool IsNeutralPlatform {
			get { return isNeutralPlatform; }
		}
		
		[MonoTODO]
		public bool IsStrongName {
			get { return isStrongName; }
		}
		
		[MonoTODO]
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		[MonoTODO]
		public string ProcessorArchitecture {
			get { return processorArchitecture; }
			set { processorArchitecture = value; }
		}
		
		[MonoTODO]
		public string PublicKeyToken {
			get { return publicKeyToken; }
			set { publicKeyToken = value; }
		}
		
		[MonoTODO]
		public string Type {
			get { return type; }
			set { type = value; }
		}
		
		[MonoTODO]
		public string Version {
			get { return version; }
			set { version = value; }
		}
		
		[MonoTODO]
		public string XmlCulture {
			get { return xmlCulture; }
			set { xmlCulture = value; }
		}
		
		[MonoTODO]
		public string XmlName {
			get { return xmlName; }
			set { xmlName = value; }
		}
		
		[MonoTODO]
		public string XmlProcessorArchitecture {
			get { return xmlProcessorArchitecture; }
			set { xmlProcessorArchitecture = value; }
		}
		
		[MonoTODO]
		public string XmlPublicKeyToken {
			get { return xmlPublicKeyToken; }
			set { xmlPublicKeyToken = value; }
		}
		
		[MonoTODO]
		public string XmlType {
			get { return xmlType; }
			set { xmlType = value; }
		}
		
		[MonoTODO]
		public string XmlVersion {
			get { return xmlVersion; }
			set { xmlVersion = value; }
		}
		
		[Flags]
		public enum FullNameFlags {
			Default,
			ProcessorArchitecture,
			Type,
			All
		}
	}
}

#endif
