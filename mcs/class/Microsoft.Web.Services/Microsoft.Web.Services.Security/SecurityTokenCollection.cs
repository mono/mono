//
// SecurityTokenCollection.cs: Handles WS-Security SecurityTokenCollection
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

	public class SecurityTokenCollection : ICollection, IEnumerable {

		private ArrayList list;

		public SecurityTokenCollection () 
		{
			list = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		// TODO
		public SecurityToken this [string refid]  {
			get { return null; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public int Add (SecurityToken token) 
		{
			return list.Add (token);
		}

		public void AddRange (ICollection collection) 
		{
			IEnumerator e = collection.GetEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is SecurityToken)
					list.Add (e.Current as SecurityToken);
				else
					throw new ArgumentException (e.Current.ToString ());
			}
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (SecurityToken token) 
		{
			return list.Contains (token);
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

		public void Remove (SecurityToken token) 
		{
			if (token == null)
				throw new ArgumentNullException ("token");
			list.Remove (token);
		}
	}
}
