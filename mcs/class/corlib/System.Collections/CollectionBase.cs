//
// System.Collections.CollectionBase.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;

namespace System.Collections {

	public abstract class CollectionBase : IList, ICollection, IEnumerable {

		// private instance properties
		private System.Collections.ArrayList myList;
		
		// public instance properties
		public virtual int Count { get { return InnerList.Count; } }
		
		// Public Instance Methods
		public virtual System.Collections.IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }
		public virtual void Clear() { 
			OnClear();
			InnerList.Clear(); 
			OnClearComplete();
		}
		public virtual void RemoveAt (int index) {
			InnerList.RemoveAt(index);
		}
		
		// Protected Instance Constructors
		protected CollectionBase() { 
			this.myList = new System.Collections.ArrayList();
		}
		
		// Protected Instance Properties
		protected System.Collections.ArrayList InnerList {get { return this.myList; } }
		protected System.Collections.IList List {get { return this; } }
		
		// Protected Instance Methods
		protected virtual void OnClear() { }
		protected virtual void OnClearComplete() { }
		
		protected virtual void OnInsert(int index, object value) { }
		protected virtual void OnInsertComplete(int index, object value) { }

		protected virtual void OnRemove(int index, object value) { }
		protected virtual void OnRemoveComplete(int index, object value) { }

		protected virtual void OnSet(int index, object oldValue, object newValue) { }
		protected virtual void OnSetComplete(int index, object oldValue, object newValue) { }

		protected virtual void OnValidate(object value) {
			if (null == value) {
				throw new System.ArgumentNullException("CollectionBase.OnValidate: Invalid parameter value passed to method: null");
			}
		}
		
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

		// IList methods
		int IList.Add (object value) {
			int newPosition;
			OnValidate(value);
			newPosition = InnerList.Count;
			OnInsert(newPosition, value);
			InnerList.Add(value);
			OnInsertComplete(newPosition, value);
			return newPosition;
		}
		
		bool IList.Contains (object value) {
			return InnerList.Contains(value);
		}

		int IList.IndexOf (object value) {
			return InnerList.IndexOf(value);
		}

		void IList.Insert (int index, object value) {
			OnValidate(value);
			OnInsert(index, value);
			InnerList.Insert(index, value);
			OnInsertComplete(index, value);
		}

		void IList.Remove (object value) {
			int removeIndex;
			OnValidate(value);
			removeIndex = InnerList.IndexOf(value);
			OnRemove(removeIndex, value);
			InnerList.Remove(value);
			OnRemoveComplete(removeIndex, value);
		}

		// IList properties
		bool IList.IsFixedSize { 
			get { return InnerList.IsFixedSize; }
		}

		bool IList.IsReadOnly { 
			get { return InnerList.IsReadOnly; }
		}

		object IList.this[int index] { 
			get { return InnerList[index]; }
			set { 
				object oldValue;
				// make sure we have been given a valid value
				OnValidate(value);
				// save a reference to the object that is in the list now
				oldValue = InnerList[index];
				
				OnSet(index, oldValue, value);
				InnerList[index] = value;
				OnSetComplete(index, oldValue, value);
			}
		}
	}
}
