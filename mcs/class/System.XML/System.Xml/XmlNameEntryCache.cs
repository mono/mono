//
// System.Xml.XmlNameEntryCache.cs
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2005 Novell, Inc.
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

using System;
using System.Collections;

namespace System.Xml
{
	internal class XmlNameEntryCache
	{
		ArrayList list = new ArrayList ();
		int insertAt = 0;

		public XmlNameEntryCache ()
		{
		}

		public XmlNameEntry Add (string prefix, string local, string ns,
			bool atomic)
		{
			XmlNameEntry e = GetInternal (prefix, local, ns, atomic);
			if (e == null) {
				e = new XmlNameEntry (prefix, local, ns);
				// Let's just limit the maximum pool size.
				if (list.Count < 1000)
					list.Add (e);
				else {
					if (insertAt == 1000)
						insertAt = 0;
					list [insertAt++] = e;
				}
			}
			return e;
		}

		public XmlNameEntry Get (string prefix, string local, string ns,
			bool atomic)
		{
			XmlNameEntry e = GetInternal (prefix, local, ns, atomic);
			return e;
		}

		private XmlNameEntry GetInternal (string prefix, string local,
			string ns, bool atomic)
		{
			if (atomic) {
				for (int i = 0; i < list.Count; i++) {
					XmlNameEntry e = (XmlNameEntry) list [i];
					if (Object.ReferenceEquals (e.Prefix, prefix) &&
						Object.ReferenceEquals (e.LocalName, local) &&
						Object.ReferenceEquals (e.NS, ns))
						return e;
				}
			} else {
				for (int i = 0; i < list.Count; i++) {
					XmlNameEntry e = (XmlNameEntry) list [i];
					if (e.Prefix == prefix &&
						e.LocalName == local &&
						e.NS == ns)
						return e;
				}
			}
			return null;
		}
	}
}
