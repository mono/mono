//
// ComClass.cs
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
	public class ComClass {
	
		string	clsId;
		string	description;
		string	progId;
		string	threadingModel;
		string	tlbId;
		string	xmlClsId;
		string	xmlDescription;
		string	xmlProgId;
		string	xmlThreadingModel;
		string	xmlTlbId;
	
		[MonoTODO]
		public ComClass ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ClsId {
			get { return clsId; }
		}
		
		[MonoTODO]
		public string Description {
			get { return description; }
		}
		
		[MonoTODO]
		public string ProgId {
			get { return progId; }
		}
		
		[MonoTODO]
		public string ThreadingModel {
			get { return threadingModel; }
		}
		
		[MonoTODO]
		public string TlbId {
			get { return tlbId; }
		}
		
		[MonoTODO]
		public string XmlClsId {
			get { return xmlClsId; }
			set { xmlClsId = value; }
		}
		
		[MonoTODO]
		public string XmlDescription {
			get { return xmlDescription; }
			set { xmlDescription = value; }
		}
		
		[MonoTODO]
		public string XmlProgId {
			get { return xmlProgId; }
			set { xmlProgId = value; }
		}
		
		[MonoTODO]
		public string XmlThreadingModel {
			get { return xmlThreadingModel; }
			set { xmlThreadingModel = value; }
		}
		
		[MonoTODO]
		public string XmlTlbId {
			get { return xmlTlbId; }
			set { xmlTlbId = value; }
		}
	}
}

#endif
