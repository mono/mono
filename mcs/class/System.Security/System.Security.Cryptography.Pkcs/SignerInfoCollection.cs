//
// SignerInfoCollection.cs - System.Security.Cryptography.Pkcs.SignerInfoCollection
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

	public class SignerInfoCollection : ICollection {

		private ArrayList _list;

		// only accessible from SignedPkcs7.SignerInfos or SignerInfo.CounterSignerInfos
		internal SignerInfoCollection () 
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

		public SignerInfo this [int index] {
			get { return (SignerInfo) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		internal void Add (SignerInfo signer) 
		{
			_list.Add (signer);
		}

		public void CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public void CopyTo (RecipientInfo[] array, int index) {}

		public SignerInfoEnumerator GetEnumerator ()
		{
			return new SignerInfoEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new SignerInfoEnumerator (_list);
		}
	}
}

#endif