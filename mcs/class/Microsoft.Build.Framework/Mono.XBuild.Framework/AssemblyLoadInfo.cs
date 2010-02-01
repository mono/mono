//
// AssemblyLoadInfo.cs: Information needed to load logger or task class.
//
// NOTE: It's not included in Microsoft.Build.Framework only in
// Microsoft.Build.Engine and tools/xbuild.
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
using System.Globalization;
using System.Reflection;

namespace Mono.XBuild.Framework {
	internal class AssemblyLoadInfo {
	
		AssemblyName	assemblyName;
		string		assemblyNameString;
		string		className;
		string		filename;
		LoadInfoType	infoType;
	
		public AssemblyLoadInfo ()
		{
		}
		
		public AssemblyLoadInfo (string assemblyName, string className)
		{
			this.assemblyName = new AssemblyName (assemblyName);
			this.className = className;
			assemblyNameString = null;
			infoType = LoadInfoType.AssemblyName;
		}
		
		public AssemblyLoadInfo (LoadInfoType loadInfoType, string filename, string name,
					 string version, string culture, string publicKeyToken, string className)
		{
			SetAssemblyName (loadInfoType, filename, name, version, culture, publicKeyToken, className);
		}
		
		protected void SetAssemblyName (LoadInfoType loadInfoType, string filename, string name, string version,
						string culture, string publicKeyToken, string className)
		{
			assemblyName = new AssemblyName ();
			this.infoType = loadInfoType;
			this.className = className;
			if (infoType == LoadInfoType.AssemblyName) {
				if (version != null)
					assemblyName.Version = new Version (version);
				if (culture != null) {
					if (culture == "neutral")
						culture = String.Empty;
					assemblyName.CultureInfo = new CultureInfo (culture);
				}
				if (publicKeyToken != null) {
					char[] chars = publicKeyToken.ToCharArray ();
					byte[] bytes = new byte [Buffer.ByteLength (chars)];
					
					for (int i  = 0; i < Buffer.ByteLength (chars); i++)
						bytes [i] = Buffer.GetByte (chars, i); 
					assemblyName.SetPublicKeyToken (bytes);
				}
				
				assemblyName.Name = name;
			} else if (infoType == LoadInfoType.AssemblyFilename) {
				this.filename = filename;
			} else {
				;
			}
		}
		
		public AssemblyName AssemblyName {
			get { return assemblyName; }
		}
		
		public string AssemblyNameString {
			get { return assemblyNameString; }
		}
		
		public string Filename {
			get { return filename; }
		}
		
		public LoadInfoType InfoType {
			get { return infoType; }
		}
		
		public string ClassName {
			get { return className; }
		}
		
		public Type Type {
			get { return Type.GetType (className); }
		}
	}

	internal enum LoadInfoType {
		AssemblyName,
		AssemblyFilename,
		AssemblyNameFromString
	}
}
