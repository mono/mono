//
// X509ChainElementCollection.cs - System.Security.Cryptography.X509Certificates.X509ChainElementCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509ChainElementCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		// only accessible from X509Chain
		internal X509ChainElementCollection () 
		{
			_list = new ArrayList ();
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public X509ChainElement this [int index] {
			get { return (X509ChainElement) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public void CopyTo (X509ChainElement[] array, int index) 
		{
			_list.CopyTo ((Array)array, index);
		}

		void ICollection.CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public X509ChainElementEnumerator GetEnumerator ()
		{
			return new X509ChainElementEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new X509ChainElementEnumerator (_list);
		}
	}
}

#endif