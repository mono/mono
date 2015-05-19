//
// System.Web.Hosting.ApplicationInfo
// 
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
//
using System;

namespace System.Web.Hosting {
	[Serializable]
	public sealed class ApplicationInfo {
		string id;
		string physical_path;
		string virtual_path;

		internal ApplicationInfo (string id, string phys, string virt)
		{
			this.id = id;
			this.physical_path = phys;
			this.virtual_path = virt;
		}

		public string ID {
			get { return id; }
		}

		public string PhysicalPath {
			get { return physical_path; }
		}

		public string VirtualPath {
			get { return virtual_path; }
		}
	}
}


