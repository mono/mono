//
// RecipientInfoCollection.cs - System.Security.Cryptography.Pkcs.RecipientInfoCollection
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

	public class RecipientInfoCollection : ICollection {

		private ArrayList _list;

		// only accessible from EnvelopedPkcs7.RecipientInfos
		internal RecipientInfoCollection () 
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

		public RecipientInfo this [int index] {
			get { return (RecipientInfo) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		internal int Add (RecipientInfo ri) 
		{
			return _list.Add (ri);
		}

		public void CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public void CopyTo (RecipientInfo[] array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public RecipientInfoEnumerator GetEnumerator ()
		{
			return new RecipientInfoEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new RecipientInfoEnumerator (_list);
		}
	}
}

#endif