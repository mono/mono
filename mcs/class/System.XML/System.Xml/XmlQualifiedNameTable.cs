//
// XmlQualifiedNameTable.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
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
using System.Collections;

using QName = System.Xml.XmlQualifiedName;

namespace System.Xml
{
	public class XmlQualifiedNameTable
	{
		XmlNameTable nameTable;
		Hashtable namespaces = new Hashtable ();

		public XmlQualifiedNameTable (XmlNameTable nameTable)
		{
			this.nametable = nameTable;
		}

		public XmlNameTable NameTable {
			 get { return nameTable; }
		} 

		public XmlQualifiedName Add (string name, string ns)
		{
			return AddInternal (name, ns, false);
		}

		public XmlQualifiedName AtomizedAdd (string name, string ns)
		{
			return AddInternal (name, ns, true);
		}

		private XmlQualifiedName AddInternal (string name, string ns,
			bool atomized)
		{
			if (!atomized) {
				name = nameTable.Add (name);
				ns = nameTable.Add (ns);
			}
			Hashtable nsTable = namespaces [ns] as Hashtable;
			if (nsTable == null) {
				nsTable = new Hashtable ();
				namespaces [ns] = nsTable;
			}
			QName qname = nsTable [name] as QName;
			if (name == null) {
				qname = new QName (name, ns);
				nsTable [name] = qname;
			}
			return qname;
		}

		public XmlQualifiedName AtomizedGet (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public XmlQualifiedName Get (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		private XmlQualifiedName GetInternal (string name, string ns,
			bool atomized)
		{
			if (atomized) {
				if (nameTable.Get (name) == null ||
					nameTable.Get (ns) == null)
					return null;
			}
			Hashtable nsTable = namespaces [ns] as Hashtable;
			return nsTable != null ?
				nsTable [name] as QName : null;
		}

		public IEnumerator GetEnumerator ()
		{
			return new XmlQualifiedNameTableEnumerator (this);
		}

		internal class XmlQualifiedNameTableEnumerator : IEnumerator
		{
			public XmlQualifiedNameTableEnumerator (
				XmlQualifiedNameTable qnameTable)
			{
				namespaces = qnameTable.namespaces.GetEnumerator ();
			}

			IEnumerator namespaces;
			IEnumerator names;

			public bool MoveNext ()
			{
				while (names == null || !names.MoveNext ()) {
					if (!namespaces.MoveNext ())
						return false;
					names = ((Hashtable) namespaces.Current).GetEnumerator ();
				}
				return true;
			}

			public object Current {
				get { return names.Current; }
			}
		}
	}
}

#endif
