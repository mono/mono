//
// SecurityElementCollection.cs: Handles WS-Security SecurityElementCollection
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

namespace Microsoft.Web.Services.Security {

	public class SecurityElementCollection : ICollection, IEnumerable {

		private ArrayList list;

		public SecurityElementCollection () 
		{
			list = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public ISecurityElement this [int index] {
			get { return (ISecurityElement) list [index]; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (ISecurityElement element) 
		{
			list.Add (element);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public void CopyTo (Array array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public void Remove (ISecurityElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			list.Remove (element);
		}
	}
}
