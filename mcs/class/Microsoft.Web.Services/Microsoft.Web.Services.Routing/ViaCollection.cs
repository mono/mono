//
// Microsoft.Web.Services.Routing.ViaCollection.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Collections;

namespace Microsoft.Web.Services.Routing {
	
	public class ViaCollection : ICollection, IEnumerable, ICloneable
	{
		ArrayList list;

		public ViaCollection ()
		{
			list = new ArrayList ();
		}

	        ViaCollection (ArrayList list)
		{
			this.list = list;
		}

				
		public int Count { 
			get { return list.Count; }
		}


		public bool IsSynchronized {

			get { return list.IsSynchronized; }
		}


		public Via this [int filter] {
			get {
				return (Via) list [filter];
			}
			set {
				list[filter] = value;
			}

		}
		
		public virtual object SyncRoot {
			get {
				return list.SyncRoot;
			}
		}
		
		public int Add (Via via)
		{
			return list.Add (via);
		}
		
		public virtual object Clone ()
		{
			return new Via (list);
		}

		public virtual void CopyTo (Array array, int index) 
		{
			list.Copyto(array, index);
		}

		public virtual IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator();
		}

		public void Insert (int index, Via via) 
		{
			list.insert(index, via);
		}

		public void InsertRange (int index, ViaCollection collection) 
		{
			list.InsertRange(index, collection);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt(index);
		}
	}
}
