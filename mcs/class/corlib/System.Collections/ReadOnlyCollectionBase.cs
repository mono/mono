//
// System.Collections.ReadOnlyCollectionBase.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;

namespace System.Collections {

	public abstract class ReadOnlyCollectionBase : ICollection,	IEnumerable {

		// private instance properties
		private System.Collections.ArrayList myList;
		
		// public instance properties
		public virtual int Count { get { return InnerList.Count; } }
		
		// Public Instance Methods
		public virtual System.Collections.IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }
		
		// Protected Instance Constructors
		protected ReadOnlyCollectionBase() {
			this.myList = new System.Collections.ArrayList();
		}
		
		// Protected Instance Properties
		protected virtual System.Collections.ArrayList InnerList {get { return this.myList; } }
		
		// ICollection methods
		void ICollection.CopyTo(Array array, int index) {
			lock (InnerList) { InnerList.CopyTo(array, index); }
		}
		object ICollection.SyncRoot {
				get { return InnerList.SyncRoot; }
			}
		bool ICollection.IsSynchronized {
			get { return InnerList.IsSynchronized; }
		}
	}
}
