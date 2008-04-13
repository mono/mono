//
// System.Collections.CollectionBase.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

namespace System.Collections {

#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
#if INSIDE_CORLIB
	public
#else
	internal
#endif
	abstract class CollectionBase : IList, ICollection, IEnumerable {

		// private instance properties
		private ArrayList list;
		
		// public instance properties
		public int Count { get { return InnerList.Count; } }
		
		// Public Instance Methods
		public IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }
		public void Clear() { 
			OnClear();
			InnerList.Clear(); 
			OnClearComplete();
		}
		public void RemoveAt (int index) {
			object objectToRemove;
			objectToRemove = InnerList[index];
			OnValidate(objectToRemove);
			OnRemove(index, objectToRemove);
			InnerList.RemoveAt(index);
			OnRemoveComplete(index, objectToRemove);
		}
		
		// Protected Instance Constructors
		protected CollectionBase()
		{ 
		}

#if NET_2_0
		protected CollectionBase (int capacity)
		{
			list = new ArrayList (capacity);
		}

		[ComVisible (false)]
		public int Capacity {
			get {
				if (list == null)
					list = new ArrayList ();
				
				return list.Capacity;
			}

			set {
				if (list == null)
					list = new ArrayList ();
							      
				list.Capacity = value;
			}
		}
			
#endif
		
		// Protected Instance Properties
		protected ArrayList InnerList {
			get {
				if (list == null)
					list = new ArrayList ();
				return list;
			} 
		}
		
		protected IList List {get { return this; } }
		
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
			InnerList.CopyTo(array, index);
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
			try {
				OnInsertComplete(newPosition, value);
			} catch {
				InnerList.RemoveAt (newPosition);
				throw;
			}
			
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
			try {
				OnInsertComplete(index, value);
			} catch {
				InnerList.RemoveAt (index);
				throw;
			}
		}

		void IList.Remove (object value) {
			int removeIndex;
			OnValidate(value);
			removeIndex = InnerList.IndexOf(value);
			if (removeIndex == -1)
				throw new ArgumentException ("The element cannot be found.", "value");
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
				if (index < 0 || index >= InnerList.Count)
					throw new ArgumentOutOfRangeException ("index");

				object oldValue;
				// make sure we have been given a valid value
				OnValidate(value);
				// save a reference to the object that is in the list now
				oldValue = InnerList[index];
				
				OnSet(index, oldValue, value);
				InnerList[index] = value;
				try {
					OnSetComplete(index, oldValue, value);
				} catch {
					InnerList[index] = oldValue;
					throw;
				}
			}
		}
	}
}
