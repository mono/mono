//
// ReceivedCollection.cs: 
//	Handles a collection of Received objects
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Collections;

namespace Microsoft.Web.Services.Timestamp {

	public class ReceivedCollection : ICollection, IEnumerable {

		private ArrayList list;

		public ReceivedCollection()
		{
			list = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public Received this [int index] {
			get { return (Received) list [index]; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (Received received)
		{
			if (received == null)
				throw new System.ArgumentNullException ("received");
			list.Add (received);
		}

		public bool Contains (Received received)
		{
			return list.Contains (received);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public void Remove (Received received) 
		{
			list.Remove (received);
		}
	}
}
