//
// SecurityCollection.cs: Handles WS-Security SecurityCollection
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

	public class SecurityCollection : ICollection, IEnumerable {

		private ArrayList list;

		public SecurityCollection ()
		{
			list = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public Security this [Uri actor] {
			get {
				if (actor == null)
					throw new ArgumentNullException ("actor");
				return null;
			}
		}

		public Security this [string actor] {
			get {
				if (actor == null)
					throw new ArgumentNullException ("actor");
				return null;
			}
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (Security security) 
		{
			// note: doc says it not ArgumentNullException
			if (security == null)
				throw new ArgumentException ("security");
			if (list.Contains (security))
				throw new ArgumentException ("duplicate");
			list.Add (security);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (string actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
			return false;
		}

		public bool Contains (Uri actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
			return false;
		}

		public void CopyTo (Array array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public void Remove (string actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
		}

		public void Remove (Uri actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
		}
	}
}
