/* System.Collections.Specialized.StringCollection.cs
 * Authors:
 *   John Barnette (jbarn@httcb.net)
 *   Sean MacIsaac (macisaac@ximian.com)
 *
 *  Copyright (C) 2001 John Barnette
 * (C) Ximian, Inc.  http://www.ximian.com
 *
 * NOTES:
 * I bet Microsoft uses ArrayList as a backing store for this; I wonder what
 * the performance difference will be.
*/

using System;

namespace System.Collections.Specialized {
	[Serializable]
	public class StringCollection : IList, ICollection, IEnumerable {
		private static int   InitialCapacity    = 11;
		private static float CapacityMultiplier = 2.0f;
		
		private int count;
		private int modCount;
		
		private string[] entries;
		
		// Public Constructor
		public StringCollection() {
			entries  = new string[InitialCapacity];
			count    = 0;
			modCount = 0;
		}
		
		// Public Instance Properties
		public int Count {
			get { return count; }
		}
		
		public bool IsFixedSize {
			get { return false; }
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		object IList.this[int index] {
			get { return this[index]; }
			set { this[index] = value.ToString(); } 
		}
		
		public string this[int index] {
			get {
				if (index < 0 || index >= count) {
					throw new ArgumentOutOfRangeException("index");
				}
				
				return entries[index];
			}
			
			set {
				if (index < 0 || index >= count) {
					throw new ArgumentOutOfRangeException("index");
				}
				
				modCount++;
				entries[index] = value;
			}
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		
		// Public Instance Methods
		
		int IList.Add(object value) {
			return Add(value.ToString());
		}
		
		public int Add(string value) {
			modCount++;
			Resize(count + 1);
			int index = count++;
			entries[index] = value;
			
			return index;
		}
		
		public void AddRange(string[] value) {
			int numEntries = value.Length;
			
			modCount++;
			Resize(count + numEntries);
			Array.Copy(value, 0, entries, count, numEntries);
			count += numEntries;
		}
		
		public void Clear() {
			modCount++;
			count = 0;
		}
		
		bool IList.Contains(object value) {
			return Contains(value.ToString());
		}
		
		public bool Contains(string value) {
			foreach (string entry in entries) {
				if (value.Equals(entry)) {
					return true;
				}
			}
			
			return false;
		}
		
		void ICollection.CopyTo(Array array, int index) {
			if (array == null) {
				throw new ArgumentNullException("array");
			} else if (index < 0) {
				throw new ArgumentOutOfRangeException("index");
			} else if (array.Rank > 1) {
				throw new ArgumentException("Rank must be 1", "array");
			} else if (array.Length - index < count) {
				throw new ArgumentException("Count is smaller than the number of objects to copy",
							    "array");
			}
			
			Array.Copy(entries, 0, array, index, count);
		}
		
		public void CopyTo(string[] array, int index) {
			if (array == null) {
				throw new ArgumentNullException("array");
			} else if (index < 0) {
				throw new ArgumentOutOfRangeException("index");
			} else if (array.Rank > 1) {
				throw new ArgumentException("Rank must be 1", "array");
			} else if (array.Length - index < count) {
				throw new ArgumentException("Count is smaller than the number of objects to copy",
							    "array");
			}
			
			Array.Copy(entries, 0, array, index, count);
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return new InternalEnumerator(this);
		}
		
		public StringEnumerator GetEnumerator() {
			return new StringEnumerator(this);
		}
		
		int IList.IndexOf(object value) {
			return IndexOf(value.ToString());
		}
		
		public int IndexOf(string value) {
			for (int i = 0; i < count; i++) {
				if (value.Equals(entries[i])) {
					return i;
				}
			}
			
			return -1;
		}
		
		void IList.Insert(int index, object value) {
			Insert(index, value.ToString());
		}
		
		public void Insert(int index, string value) {
			if (index < 0 || index > count) {
				throw new ArgumentOutOfRangeException("index");
			}
			
			modCount++;
			Resize(count + 1);
			Array.Copy(entries, index, entries, index + 1, count - index);
			entries[index] = value;
			count++;
		}

		
		void IList.Remove(object value) {
			Remove(value.ToString());
		}
		
		public void Remove(string value) {
			for (int i = 0; i < count; i++) {
				if (value.Equals(entries[i])) {
					RemoveAt(i);
					return;
				}
			}
		}
		
		public void RemoveAt(int index) {
			if (index < 0 || index >= count) {
				throw new ArgumentOutOfRangeException("index");
			}
			
			int remaining = count - index - 1;
			
			modCount++;
			
			if (remaining > 0) {
				Array.Copy(entries, index + 1, entries, index, remaining);
			}
			
			count--;
			entries[count] = null;
		}
		
		
		// Private Instance Methods
		
		private void Resize(int minSize) {
			int oldSize = entries.Length;
			
			if (minSize > oldSize) {
				string[] oldEntries = entries;
				int newSize = (int) (oldEntries.Length * CapacityMultiplier);
				
				if (newSize < minSize) newSize = minSize;
				entries = new string[newSize];
				Array.Copy(oldEntries, 0, entries, 0, count);
			}
		}
		
		
		// Private classes
		
		private class InternalEnumerator : IEnumerator {
			private StringCollection data;
			private int index;
			private int myModCount;
			
			public InternalEnumerator(StringCollection data) {
				this.data  = data;
				myModCount = data.modCount;
				index      = -1;
			}
			
			
			// Public Instance Properties
			
			public object Current {
				get {
					if (myModCount != data.modCount) {
						throw new InvalidOperationException();
					} else if (index < 0 || index > data.count - 1) {
						throw new InvalidOperationException();
					}
					
					return data[index];
				}
			}
			
			
			// Public Instance Methods
			
			public bool MoveNext() {
				if (myModCount != data.modCount) {
					throw new InvalidOperationException();
				}
				
				if (++index >= data.count) {
					return false;
				}
				
				return true;
			}
			
			public void Reset() {
				if (myModCount != data.modCount) {
					throw new InvalidOperationException();
				}
				
				index = -1;
			}
		}
	}
}
