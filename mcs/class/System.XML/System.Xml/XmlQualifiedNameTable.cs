//
// XmlQualifiedNameTable.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
//
#if NET_2_0

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlQualifiedNameTable
	{
		public XmlQualifiedNameTable (XmlNameTable nt)
		{
			throw new NotImplementedException ();
		}

		public XmlNameTable NameTable {
			 get { throw new NotImplementedException (); }
		} 

		public XmlQualifiedName Add (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public XmlQualifiedName AtomizedAdd (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public XmlQualifiedName AtomizedGet (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public XmlQualifiedName Get (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
