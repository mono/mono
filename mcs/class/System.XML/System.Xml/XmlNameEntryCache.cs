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
		Hashtable table = new Hashtable ();
		XmlNameTable nameTable;
		XmlNameEntry dummy = new XmlNameEntry (String.Empty, String.Empty, String.Empty);
		char [] cacheBuffer;

		public XmlNameEntryCache (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;
		}

		public string GetAtomizedPrefixedName (string prefix, string local)
		{
			if (prefix == null || prefix.Length == 0)
				return local;

			if (cacheBuffer == null)
				cacheBuffer = new char [20];
			if (cacheBuffer.Length < prefix.Length + local.Length + 1)
				cacheBuffer = new char [Math.Max (
					prefix.Length + local.Length + 1,
					cacheBuffer.Length << 1)];
			prefix.CopyTo (0, cacheBuffer, 0, prefix.Length);
			cacheBuffer [prefix.Length] = ':';
			local.CopyTo (0, cacheBuffer, prefix.Length + 1, local.Length);
			return nameTable.Add (cacheBuffer, 0, prefix.Length + local.Length + 1);
		}

		public XmlNameEntry Add (string prefix, string local, string ns,
			bool atomic)
		{
			if (!atomic) {
				prefix = nameTable.Add (prefix);
				local = nameTable.Add (local);
				ns = nameTable.Add (ns);
			}
			XmlNameEntry e = GetInternal (prefix, local, ns, true);
			if (e == null) {
				e = new XmlNameEntry (prefix, local, ns);
				table [e] = e;
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
			if (!atomic) {
				if (nameTable.Get (prefix) == null ||
				    nameTable.Get (local) == null ||
				    nameTable.Get (ns) == null)
					return null;
			}
			dummy.Update (prefix, local, ns);

			return table [dummy] as XmlNameEntry;
		}
	}
}
