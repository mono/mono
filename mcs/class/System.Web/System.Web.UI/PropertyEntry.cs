//
// System.Web.UI.PropertyEntry
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
//

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
#if NET_2_0
using System;
using System.Reflection;

namespace System.Web.UI
{
	public abstract class PropertyEntry {
		Type type;
		string name;
		string filter;
		PropertyInfo pinfo;

		internal PropertyEntry () { }

		public Type DeclaringType {
			get { return pinfo.DeclaringType; }
		}

		public string Filter {
			get { return filter; }
			set { filter = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public PropertyInfo PropertyInfo {
			get { return pinfo; }
			set { pinfo = value; }
		}

		public Type Type {
			get { return type; }
			set { type = value; }
		}
	}
	
}
#endif // NET_2_0

