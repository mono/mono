//
// TypeLib.cs
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
	public class TypeLib {
	
		string	flags;
		string	helpDirectory;
		string	resourceId;
		string	tlbId;
		string	version;
		string	xmlFlags;
		string	xmlHelpDirectory;
		string	xmlResourceId;
		string	xmlTlbId;
		string	xmlVersion;
	
		[MonoTODO]
		public TypeLib ()
		{
			throw new NotImplementedException ();
		}
		
		public string Flags {
			get { return flags; }
		}
		
		public string HelpDirectory {
			get { return helpDirectory; }
		}
		
		public string ResourceId {
			get { return resourceId; }
		}
		
		public string TlbId {
			get { return tlbId; }
		}
		
		public string Version {
			get { return version; }
		}
		
		public string XmlFlags {
			get { return xmlFlags; }
			set { xmlFlags = value; }
		}
		
		public string XmlHelpDirectory {
			get { return xmlHelpDirectory; }
			set { xmlHelpDirectory = value; }
		}
		
		public string XmlResourceId {
			get { return xmlResourceId; }
			set { xmlResourceId = value; }
		}
		
		public string XmlTlbId {
			get { return xmlTlbId; }
			set { xmlTlbId = value; }
		}
		
		public string XmlVersion {
			get { return xmlVersion; }
			set { xmlVersion = value; }
		}
	}
}

#endif
