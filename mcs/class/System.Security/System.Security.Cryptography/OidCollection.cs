//
// OidCollection.cs - System.Security.Cryptography.OidCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class OidCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public OidCollection ()
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

		public Oid this [int index] {
			get { return (Oid) _list [index]; }
		}

		public Oid this [string oid] {
			get { 
				foreach (Oid o in _list) {
					if (o.Value == oid)
						return o;
				}
				return null; 
			}
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (Oid oid)
		{
			return _list.Add (oid);
		}

		public void CopyTo (Oid[] array, int index)
		{
			_list.CopyTo ((Array)array, index);
		}

		// to satisfy ICollection - private
		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public OidEnumerator GetEnumerator () 
		{
			return new OidEnumerator (this);
		}

		// to satisfy IEnumerator - private
		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new OidEnumerator (this);
		}
	}
}

#endif