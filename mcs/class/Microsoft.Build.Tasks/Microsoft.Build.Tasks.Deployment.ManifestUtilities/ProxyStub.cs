//
// ProxyStub.cs
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
	public class ProxyStub {
	
		string	baseInterface;
		string	iid;
		string	name;
		string	numMethods;
		string	tlbId;
		string	xmlBaseInterface;
		string	xmlIID;
		string	xmlName;
		string	xmlNumMethods;
		string	xmlTlbId;
	
		[MonoTODO]
		public ProxyStub ()
		{
			throw new NotImplementedException ();
		}
		
		public string BaseInterface {
			get { return baseInterface; }
		}
		
		public string IID {
			get { return iid; }
		}
		
		public string Name {
			get { return name; }
		}
		
		public string NumMethods {
			get { return numMethods; }
		}
		
		public string TlbId {
			get { return tlbId; }
		}
		
		public string XmlBaseInterface {
			get { return xmlBaseInterface; }
			set { xmlBaseInterface = value; }
		}
		
		public string XmlIID {
			get { return xmlIID; }
			set { xmlIID = value; }
		}
		
		public string XmlName {
			get { return xmlName; }
			set { xmlName = value; }
		}
		
		public string XmlNumMethods {
			get { return xmlNumMethods; }
			set { xmlNumMethods = value; }
		}
		
		public string XmlTlbId {
			get { return xmlTlbId; }
			set { xmlTlbId = value; }
		}
	}
}

#endif
