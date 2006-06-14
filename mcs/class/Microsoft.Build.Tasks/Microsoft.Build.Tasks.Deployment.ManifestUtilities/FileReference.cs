//
// FileReference.cs
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
	
	public sealed class FileReference : BaseReference {
	
		ComClass[]	comClasses;
		bool		isDataFile;
		ProxyStub[]	proxyStubs;
		TypeLib[]	typeLibs;
		ComClass[]	xmlComClasses;
		ProxyStub[]	xmlProxyStubs;
		TypeLib[]	xmlTypeLibs;
		string		xmlWriteableType;
	
		[MonoTODO]
		public FileReference ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public FileReference (string path)
		{
			throw new NotImplementedException ();
		}
		
		public ComClass[] ComClasses {
			get { return comClasses; }
		}
		
		public bool IsDataFile {
			get { return isDataFile; }
			set { isDataFile = value; }
		}
		
		public ProxyStub[] ProxyStubs {
			get { return proxyStubs; }
		}
		
		public TypeLib[] TypeLibs {
			get { return typeLibs; }
		}
		
		public ComClass[] XmlComClasses {
			get { return xmlComClasses; }
			set { xmlComClasses = value; }
		}
		
		public ProxyStub[] XmlProxyStubs {
			get { return xmlProxyStubs; }
			set { xmlProxyStubs = value; }
		}
		
		public TypeLib[] XmlTypeLibs {
			get { return xmlTypeLibs; }
			set { xmlTypeLibs = value; }
		}
		
		public string XmlWriteableType {
			get { return xmlWriteableType; }
			set { xmlWriteableType = value; }
		}
		
		protected internal override string SortName {
			get { return null; }
		}
	}
}

#endif
