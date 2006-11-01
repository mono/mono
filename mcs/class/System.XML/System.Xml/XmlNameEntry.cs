//
// System.Xml.XmlNameEntry.cs
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
	internal class XmlNameEntry
	{
		public XmlNameEntry (string prefix, string local, string ns)
		{
			Update (prefix, local, ns);
		}

		public void Update (string prefix, string local, string ns)
		{
			Prefix = prefix;
			LocalName = local;
			NS = ns;
			Hash = local.GetHashCode () + (prefix.Length > 0 ? prefix.GetHashCode () : 0);
		}

		public string Prefix;
		public string LocalName;
		public string NS;
		public int Hash;

		string prefixed_name_cache;

		public override bool Equals (object other)
		{
			XmlNameEntry e = other as XmlNameEntry;
			return e != null && e.Hash == Hash &&
				Object.ReferenceEquals (e.LocalName, LocalName) &&
				Object.ReferenceEquals (e.NS, NS) &&
				Object.ReferenceEquals (e.Prefix, Prefix);
		}

		public override int GetHashCode ()
		{
			return Hash;
		}

		public string GetPrefixedName (XmlNameEntryCache owner)
		{
			if (prefixed_name_cache == null)
				prefixed_name_cache =
					owner.GetAtomizedPrefixedName (Prefix, LocalName);
			return prefixed_name_cache;
		}
	}
}
