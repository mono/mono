//
// Pkcs9AttributeCollection.cs - System.Security.Cryptography.Pkcs.Pkcs9AttributeCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs9AttributeCollection : ICollection {

		private ArrayList _list;

		public Pkcs9AttributeCollection () 
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

		public Pkcs9Attribute this [int index] {
			get { return (Pkcs9Attribute) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (Pkcs9Attribute attribute)
		{
			return _list.Add (attribute);
		}

		public void CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public void CopyTo (Pkcs9Attribute[] array, int index)
		{
			_list.CopyTo (array, index);
		}

		public Pkcs9AttributeEnumerator GetEnumerator () 
		{
			return new Pkcs9AttributeEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new Pkcs9AttributeEnumerator (_list);
		}

		public void Remove (Pkcs9Attribute attribute) 
		{
			_list.Remove (attribute);
		}
	}
}

#endif