//
// System.Collections.ArrayList
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//    Duncan Mak (duncan@ximian.com)
//		Patrik Torstensson (totte@crepundia.net)
//
// (C) 2001 Vladimir Vukicevic
// (C) 2002 Ximian, Inc.
//

using System;

namespace System.Collections {

	[MonoTODO ("add versioning, changing the arraylist should invalidate all enumerators")]
	[Serializable]
	public class ArrayList : IList, ICollection, IEnumerable, ICloneable {

		// Keep these three fields in sync with mono-reflection.h.
		private int count = 0;
		private int capacity = defaultCapacity;
		private object[] dataArray;
		
		// constructors
		public ArrayList () {
			dataArray = new object[capacity];
		}

		public ArrayList (ICollection c) {
			if (null == c)
				throw new ArgumentNullException();

			//Emulate MS.NET behavior. Throw RankException when passed a
			// multi-dimensional Array.
			Array arr = c as Array;
			if (null != arr && arr.Rank > 1)
				throw new RankException ();

			this.capacity = (c.Count == 0) ? defaultCapacity : c.Count;
			dataArray = new object [capacity];
			foreach (object o in c) 
				Add (o);
		}

		public ArrayList (int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity", capacity, "Value must be greater than or equal to zero.");

			if (capacity > 0)
				this.capacity = capacity;
			// else if capacity == 0 then use defaultCapacity

			dataArray = new object[this.capacity];
		}

		private ArrayList (object[] dataArray, int count, int capacity,
			bool fixedSize, bool readOnly, bool synchronized) {
			this.dataArray = new object [capacity];
			dataArray.CopyTo (this.dataArray, 0);
			this.count = count;
			this.capacity = capacity;
			this.fixedSize = fixedSize;
			this.readOnly = readOnly;
			this.synchronized = synchronized;
		}

		public static ArrayList ReadOnly (ArrayList list) {
			if (list == null)
				throw new ArgumentNullException ();

			if (list.IsSynchronized) 
				return ArrayList.Synchronized (list);
			else
				return new ArrayList (list.ToArray (), list.Count, list.Capacity, list.IsFixedSize, true, list.IsSynchronized);
		}

		public static IList ReadOnly (IList list) {
			if (list == null)
				throw new ArgumentNullException ();

			ArrayList al = new ArrayList ();

			foreach (object o in list)
				al.Add (o);

			return (IList) ArrayList.ReadOnly (al);
		}

		public static ArrayList Synchronized (ArrayList list) {
			if (list == null)
				throw new ArgumentNullException ();

			return new SyncArrayList(new ArrayList (list.ToArray (), list.Count, list.Capacity, list.IsFixedSize, list.IsReadOnly, true));
		}

		public static IList Synchronized (IList list) {
			if (list == null)
				throw new ArgumentNullException ();

			ArrayList al = new ArrayList ();

			foreach (object o in list)
				al.Add (o);

			return (IList) ArrayList.Synchronized (al);
		}

		public static ArrayList FixedSize (ArrayList list) {
			if (list == null)
				throw new ArgumentNullException ();

			if (list.IsSynchronized) 
				return Synchronized(list);
			
			return new ArrayList (list.ToArray (), list.Count, list.Capacity, true, list.IsReadOnly, list.IsSynchronized);
		}

		public static IList FixedSize (IList list) {
			if (list == null)
				throw new ArgumentNullException ();

			if (list.IsSynchronized) 
				return Synchronized(list);

			ArrayList al = new ArrayList ();

			foreach (object o in list)
				al.Add (o);

			return (IList) ArrayList.FixedSize (al);			
		}

		public static ArrayList Repeat (object value, int count) {
			ArrayList al = new ArrayList (count);
			for (int i = 0; i < count; i++) {
				al.dataArray[i] = value;
			}
			al.count = count;

			return al;
		}

		[Serializable]
		private class ListWrapper : ArrayList {
			IList list;

			public ListWrapper (IList list) {
				if (null == list)
					throw new ArgumentNullException();

				this.list = list;
				count = ((ICollection) list).Count;
			}
			
			// ArrayList
			[MonoTODO]
			public override int Capacity {
				get { return list.Count; }
				set { throw new NotSupportedException (); }
			}

			[MonoTODO]
			public override void AddRange (ICollection collection) {
				if (collection == null)
					throw new ArgumentNullException ("colllection");
				if (IsFixedSize || IsReadOnly)
					throw new NotSupportedException ();
			}

			[MonoTODO]
			public override int BinarySearch (object value) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override int BinarySearch (object value, IComparer comparer) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override int BinarySearch (int index, int count, object value,
				IComparer comparer) {
				throw new NotImplementedException ();
			}

			public override void CopyTo (Array array) {
				if (null == array)
					throw new ArgumentNullException("array");
				if (array.Rank > 1)
					throw new ArgumentException("array cannot be multidimensional");

				CopyTo (array, 0);
			}

			[MonoTODO]
			public override void CopyTo (int index, Array array,
				int arrayIndex, int count) {
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || arrayIndex < 0 || count < 0)
					throw new ArgumentOutOfRangeException ();
				if (array.Rank > 1 || index >= Count || Count > (array.Length - arrayIndex))
					throw new ArgumentException ();
				// FIXME: handle casting error here
			}

			public override ArrayList GetRange (int index, int count) {
				if (index < 0 || count < 0)
					throw new ArgumentOutOfRangeException ();
				if (Count < (index + count))
					throw new ArgumentException ();
				
				ArrayList result = new ArrayList (count);

				for (int i = 0; i < count; i++)
					result.Add (list [i]);

				return result;
			}

			[MonoTODO]
			public override void InsertRange (int index, ICollection col) {
				if (col == null)
					throw new ArgumentNullException ();
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				if (IsReadOnly || IsFixedSize)
					throw new NotSupportedException ();

				if (index == Count) {
					foreach (object element in col)
						list.Add (element);

				} //else if ((index + count) < Count) {
				// 					for (int i = index; i < (index + count); i++)
				// 						list [i] = col [i];

				// 				} else {
				// 					int added = Count - (index + count);
				// 					for (int i = index; i < Count; i++)
				// 						list [i] = col [i];
				// 					for (int i = 0; i < added; i++)
				// 						list.Add (col [Count +i]);
				// 				}
			}

			public override int LastIndexOf (object value) {
				return LastIndexOf (value, Count, 0);
			}

			public override int LastIndexOf (object value, int startIndex) {
				return LastIndexOf (value, startIndex, 0);
			}

			public override int LastIndexOf (object value, int startIndex, int count) {
				if (null == value){
					return -1;
				}

				if (startIndex > Count || count < 0 || (startIndex + count > Count))
					throw new ArgumentOutOfRangeException ();
				
				int length = startIndex - count + 1;

				for (int i = startIndex; i >= length; i--)
					if (list [i] == value)
						return i;
				return -1;
			}

			public override void RemoveRange (int index, int count) {
				if ((index < 0) || (count < 0))
					throw new ArgumentOutOfRangeException ();
				if ((index > Count) || (index + count) > Count)
					throw new ArgumentException ();
				if (IsReadOnly || IsFixedSize)
					throw new NotSupportedException ();

				for (int i  = 0; i < count; i++)
					list.RemoveAt (index);
			}

			public override void Reverse () {
				Reverse (0, Count);
			}

			public override void Reverse (int index, int count) {
				if ((index < 0) || (count < 0))
					throw new ArgumentOutOfRangeException ();
				if ((index > Count) || (index + count) > Count)
					throw new ArgumentException ();
				if (IsReadOnly)
					throw new NotSupportedException ();

				object tmp = null;

				for (int i = index; i < count; i++) {
					tmp = list [i];
					list [i] = list [count - i];
					list [count - i] = tmp;
				}
			}

			public override void SetRange (int index, ICollection col) {
				if (index < 0 || (index + col.Count) > Count)
					throw new ArgumentOutOfRangeException ();
				if (col == null)
					throw new ArgumentNullException ();
				if (IsReadOnly)
					throw new NotSupportedException ();

				for (int i = index; i < col.Count; i++)
					foreach (object o in col)
						list [i] = o;
			}

			[MonoTODO]
			public override void Sort () {
			}

			[MonoTODO]
			public override void Sort (IComparer comparer) {
			}

			[MonoTODO]
			public override void Sort (int index, int count, IComparer comparer) {
			}

			public override object [] ToArray () {
				return (object []) ToArray (typeof (object));
			}

			public override Array ToArray (Type type) {
				int count = Count;
				Array result = Array.CreateInstance (type, count);

				for (int i = 0; i < count; i++)
					result.SetValue (list [i], i);

				return result;
			}

			[MonoTODO]
			public override void TrimToSize () {
			}

			// IList
			public override bool IsFixedSize {
				get { return list.IsFixedSize; }
			}

			public override bool IsReadOnly {
				get { return list.IsReadOnly; }
			}

			public override object this [int index] {
				get { return list [index]; }
				set { list [index] = value; }
			}

			public override int Add (object value) {
				return list.Add (value);
			}

			public override void Clear () {
				list.Clear ();
			}

			public override bool Contains (object value) {
				return list.Contains (value);
			}

			public override int IndexOf (object value) {
				return list.IndexOf (value);
			}

			public override void Insert (int index, object value) {
				list.Insert (index, value);
			}

			public override void Remove (object value) {
				list.Remove (value);
			}

			public override void RemoveAt (int index) {
				list.RemoveAt (index);
			}

			// ICollection			
			public override int Count {
				get { return count; }
			}

			public override bool IsSynchronized {
				get { return ((ICollection) list).IsSynchronized; }
			}

			public override object SyncRoot {
				get { return ((ICollection) list).SyncRoot; }
			}

			public override void CopyTo (Array array, int index) {
				((ICollection) list).CopyTo (array, index);
			}

			// ICloneable
			public override object Clone () {
				return new ListWrapper (list);
			}

			// IEnumerable
			public override IEnumerator GetEnumerator () {
				return ((IEnumerable) list).GetEnumerator ();
			}
		}

		[MonoTODO]
		public static ArrayList Adapter (IList list) {
			return new ListWrapper (list);
		}

		// properties

		private bool fixedSize = false;
		private bool readOnly = false;
		private bool synchronized = false;

		private long version = 0;
		private ArrayList source = null;

		private const int defaultCapacity = 16;

		private void copyDataArray (object[] outArray) {
			for (int i = 0; i < count; i++) {
				outArray[i] = dataArray[i];
			}
		}

		private void setSize (int newSize) {
			if (newSize == capacity) 
				return;
			
			capacity = (newSize == 0) ? defaultCapacity : newSize;

			// note that this assumes that we've already sanity-checked
			// the new size
			object[] newDataArray = new object[newSize];
			copyDataArray (newDataArray);
			dataArray = newDataArray;
		}

		// note that this DOES NOT update count
		private void shiftElements (int startIndex, int numshift) {
			if (numshift == 0) { 
				return;
			}

			if (count + numshift > capacity) {
				setSize (capacity * 2);
				shiftElements (startIndex, numshift);
			} else {
				if (numshift > 0) {
					int numelts = count - startIndex;
					for (int i = numelts-1; i >= 0; i--) {
						dataArray[startIndex + numshift + i] = dataArray[startIndex + i];
					}

					for (int i = startIndex; i < startIndex + numshift; i++) {
						dataArray[i] = null;
					}
				} else {
					int numelts = count - startIndex + numshift;
					for (int i = 0; i < numelts; i++) {
						dataArray [i + startIndex] = dataArray [i + startIndex - numshift];
					}
					for (int i = count + numshift; i < count; i++) {
						dataArray[i] = null;
					}
				}
			}
		}

		public virtual int Capacity {
			get {
				return capacity;
			}

			set {
				if (readOnly) {
					throw new NotSupportedException
						("Collection is read-only.");
				}

				if (value < count) {
					throw new ArgumentOutOfRangeException
						("ArrayList Capacity being set to less than Count");
				}

				if (fixedSize && value != capacity) {
					throw new NotSupportedException
						("Collection is fixed size.");
				}

				setSize (value);
			}
		}

		private void CheckSourceVersion() {
			if (null != this.source && this.version != this.source.version) {
				throw new InvalidOperationException();
			}
		}

		public virtual int Count {
			get {
				CheckSourceVersion();
				return count;
			}
		}

		public virtual bool IsFixedSize {
			get {
				return fixedSize;
			}
		}

		public virtual bool IsReadOnly {
			get {
				return readOnly;
			}
		}

		public virtual bool IsSynchronized {
			get {
				return synchronized;
			}
		}

		public virtual object this[int index] {
			get {
				CheckSourceVersion();

				if (index < 0) {
					throw new ArgumentOutOfRangeException ("index < 0");
				}

				if (index >= count) {
					throw new ArgumentOutOfRangeException ("index out of range");
				}

				return dataArray[index];
			}
			set {
				if (index < 0) {
					throw new ArgumentOutOfRangeException ("index < 0");
				}

				if (index >= count) {
					throw new ArgumentOutOfRangeException ("index out of range");
				}

				if (readOnly) {
					throw new NotSupportedException ("Collection is read-only.");
				}

				dataArray[index] = value;
				version++;
			}
		}

		public virtual object SyncRoot {
			get {
				return this;
			}
		}

		// methods
		public virtual int Add (object value) {
			if (readOnly)
				throw new NotSupportedException ("ArrayList is read-only.");
			if (fixedSize)
				throw new NotSupportedException ("ArrayList is fixed size.");

			if (count + 1 >= capacity)
				setSize (capacity * 2);

			dataArray[count] = value;
			version++;
			return count++;
		}

		public virtual void AddRange (ICollection c) {
			if (null == c)
				throw new ArgumentNullException ("c");
			if (readOnly || fixedSize)
				throw new NotSupportedException ();

			int cc = c.Count;
			if (count + cc >= capacity)
				Capacity = cc < count? count * 2: count + cc + 1;
			c.CopyTo (dataArray, count);
			count += cc;
			version++;
		}

		public virtual int BinarySearch (object value) {
			return BinarySearch (0, count, value, null);
		}

		public virtual int BinarySearch (object value, IComparer comparer) {
			return BinarySearch (0, count, value, comparer);
		}

		public virtual int BinarySearch (int index, int count,
			object value, IComparer comparer) {
			return Array.BinarySearch (dataArray, index, count, value, comparer);
		}

		public virtual void Clear () {
			if (readOnly || fixedSize)
				throw new NotSupportedException();

			count = 0;
			version++;
		}

		public virtual object Clone () {
			return new ArrayList (dataArray, count, capacity,
				fixedSize, readOnly, synchronized);
		}

		public virtual bool Contains (object item) {
			for (int i = 0; i < count; i++) {
				if (Object.Equals (dataArray[i], item)) {
					return true;
				}
			}

			return false;
		}

		public virtual void CopyTo (Array array) {
			if (null == array)
				throw new ArgumentNullException("array");
			if (array.Rank > 1)
				throw new ArgumentException("array cannot be multidimensional");

			Array.Copy (dataArray, 0, array, 0, this.count);
		}

		public virtual void CopyTo (Array array, int arrayIndex) {
			if (null == array)
				throw new ArgumentNullException("array");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex");
			if (array.Rank > 1)
				throw new ArgumentException("array cannot be multidimensional");
			if (this.count > array.Length - arrayIndex)
				throw new ArgumentException("this ArrayList has more items than the space available in array from arrayIndex to the end of array");
			
			Array.Copy (dataArray, 0, array, arrayIndex, this.count);
		}

		public virtual void CopyTo (int index, Array array,
			int arrayIndex, int count) {
			if (null == array)
				throw new ArgumentNullException("array");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			if (index >= this.count)
				throw new ArgumentException("index is greater than or equal to the source ArrayList.Count");
			if (array.Rank > 1)
				throw new ArgumentException("array cannot be multidimensional");
			if (arrayIndex >= array.Length)
				throw new ArgumentException("arrayIndex is greater than or equal to array's length");
			if (this.count > array.Length - arrayIndex)
				throw new ArgumentException("this ArrayList has more items than the space available in array from arrayIndex to the end of array");

			Array.Copy (dataArray, index, array, arrayIndex, count);
		}

		[Serializable]
		private class ArrayListEnumerator : IEnumerator, ICloneable {
			private object[] data;
			private int idx;
			private int start;
			private int num;
			private ArrayList enumeratee;
			private long version;

			internal ArrayListEnumerator(int index, int count, object[] items, ArrayList al, long ver) {
				data = items;
				start = index;
				num = count;
				idx = start - 1;
				enumeratee = al;
				version = ver;
			}

			public object Clone () {
				return new ArrayListEnumerator (start, num, data, enumeratee, version);
			}

			public virtual object Current {
				get {
					return data [idx];
				}
			}
			public virtual bool MoveNext() {
				if (enumeratee.version != version)
					throw new InvalidOperationException();
				if (++idx < start + num)
					return true;
				return false;
			}
			public virtual void Reset() {
				idx = start - 1;
			}
		}

		public virtual IEnumerator GetEnumerator () {
			return new ArrayListEnumerator(0, this.Count, dataArray, this, this.version);
		}

		private void ValidateRange(int index, int count) {
			if (index < 0) {
				throw new ArgumentOutOfRangeException("index", index, "Must be equal to or greater than zero");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count", count, "Must be equal to or greater than zero");
			}
			if (index > this.count - 1) {
				throw new ArgumentException();
			}
			if (index + count > this.count - 1) {
				throw new ArgumentException();
			}
		}

		public virtual IEnumerator GetEnumerator (int index, int count) {
			ValidateRange(index, count);
			return new ArrayListEnumerator(index, count, dataArray, this, this.version);
		}

		public virtual ArrayList GetRange (int index, int count) {
			ValidateRange(index, count);
			ArrayList retVal = new ArrayList(count);

			for (int i = index; i < count + index; i++) {
				retVal.Add(this[i]);
			}
			retVal.version = this.version;
			retVal.source = this;
			return retVal;
		}

		public virtual int IndexOf (object value) {
			return IndexOf (value, 0, count);
		}

		public virtual int IndexOf (object value, int startIndex) {
			return IndexOf (value, startIndex, count - startIndex);
		}

		public virtual int IndexOf (object value, int startIndex, int count) {
			if (startIndex < 0 || startIndex + count > this.count || count < 0) {
				throw new ArgumentOutOfRangeException ("IndexOf arguments out of range");
			}
			for (int i = startIndex; i < (startIndex + count); i++) {
				if (Object.Equals (dataArray[i], value)) {
					return i;
				}
			}

			return -1;
		}

		public virtual void Insert (int index, object value) {
			if (readOnly) {
				throw new NotSupportedException
					("Collection is read-only.");
			}

			if (fixedSize) {
				throw new NotSupportedException
					("Collection is fixed size.");
			}

			if (index < 0 || index > count) {
				throw new ArgumentOutOfRangeException ("index < 0 or index >= capacity");
			}

			shiftElements (index, 1);
			dataArray[index] = value;
			count++;
			version++;
		}

		public virtual void InsertRange (int index, ICollection c) {

			if (c == null)
				throw new ArgumentNullException ();

			if (index < 0 || index > count)
				throw new ArgumentOutOfRangeException ();

			if (IsReadOnly || IsFixedSize)
				throw new NotSupportedException ();

			// Get a copy of the collection before the shift in case the collection
			// is this.  Otherwise the enumerator will be confused.
			Array source = Array.CreateInstance(typeof(object), c.Count);
			c.CopyTo(source, 0);

			shiftElements (index, c.Count);
			count += c.Count;

			foreach (object o in source)
				dataArray[index++] = o;

			version++;
		}

		public virtual int LastIndexOf (object value) {
			return LastIndexOf (value, count - 1, count);
		}

		public virtual int LastIndexOf (object value, int startIndex) {
			if (startIndex < 0 || startIndex > count - 1) {
				throw new ArgumentOutOfRangeException("startIndex", startIndex, "");
			}
			return LastIndexOf (value, startIndex, startIndex + 1);
		}

		public virtual int LastIndexOf (object value, int startIndex,
			int count) {
			if (null == value){
				return -1;
			}
			if (startIndex >= this.count)
				throw new ArgumentOutOfRangeException ("startIndex >= Count");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count < 0");
			if (startIndex + 1 < count)
				throw new ArgumentOutOfRangeException ("startIndex + 1 < count");
			
			int EndIndex = startIndex - count + 1;
			for (int i = startIndex; i >= EndIndex; i--) {
				if (Object.Equals (dataArray[i], value)) {
					return i;
				}
			}

			return -1;
		}

		public virtual void Remove (object obj) {

			if (IsFixedSize || IsReadOnly)
				throw new NotSupportedException ();
			
			int objIndex = IndexOf (obj);

			if (objIndex == -1) {
				// shouldn't an exception be thrown here??
				// the MS docs don't indicate one, and testing
				// with the MS .net framework doesn't indicate one
				return;
			}

			RemoveRange (objIndex, 1);
		}

		public virtual void RemoveAt (int index) {
			RemoveRange (index, 1);
		}

		public virtual void RemoveRange (int index, int count) {
			if (readOnly) {
				throw new NotSupportedException
					("Collection is read-only.");
			}

			if (fixedSize) {
				throw new NotSupportedException
					("Collection is fixed size.");
			}

			if (index < 0 || index >= this.count || index + count > this.count) {
				throw new ArgumentOutOfRangeException
					("index/count out of range");
			}

			shiftElements (index, - count);
			this.count -= count;
			version++;
		}

		public virtual void Reverse () {
			Reverse (0, count);
		}

		public virtual void Reverse (int index, int count) {
			if (readOnly) {
				throw new NotSupportedException
					("Collection is read-only.");
			}

			if (index < 0 || index + count > this.count) {
				throw new ArgumentOutOfRangeException
					("index/count out of range");
			}

			Array.Reverse (dataArray, index, count);
			version++;
		}

		public virtual void SetRange (int index, ICollection c) {
			if (c == null)
				throw new ArgumentNullException ();
			if (readOnly)
				throw new NotSupportedException ();
			if (index < 0 || (index + c.Count) > count)
				throw new ArgumentOutOfRangeException ();

			c.CopyTo(dataArray, index);
		}

		public virtual void Sort () {
			Sort (0, count, null);
		}

		public virtual void Sort (IComparer comparer) {
			Sort (0, count, comparer);
		}

		public virtual void Sort (int index, int count, IComparer comparer) {
			if (readOnly) {
				throw new NotSupportedException
					("Collection is read-only.");
			}

			if (index < 0 || index + count > this.count) {
				throw new ArgumentOutOfRangeException
					("index/count out of range");
			}
            
			Array.Sort (dataArray, index, count, comparer);
			version++;
		}

		public virtual object[] ToArray() {
			object[] outArray = new object[count];
			Array.Copy (dataArray, outArray, count);
			return outArray;
		}

		public virtual Array ToArray (Type type) {
			Array outArray = Array.CreateInstance (type, count);
			Array.Copy (dataArray, outArray, count);
			return outArray;
		}

		public virtual void TrimToSize () {

			if (IsReadOnly || IsFixedSize)
				throw new NotSupportedException ();
			
			setSize(count);

			version++;
		}

		private class SyncArrayList : ArrayList {
			private ArrayList	_list;

			// constructors
			public SyncArrayList(ArrayList list) {
				_list = list;
			}

			// properties
			public override int Capacity {
				get {
					lock (_list.SyncRoot) {
						return _list.Capacity;
					}
				}
				set {
					lock (_list.SyncRoot) {
						_list.Capacity = value;
					}
				}
			}

			public override int Count {
				get {
					lock (_list.SyncRoot) {
						return _list.Count;
					}
				}
			}

			public override bool IsFixedSize {
				get {
					lock (_list.SyncRoot) {
						return _list.IsFixedSize;
					}
				}
			}

			public override bool IsReadOnly {
				get {
					lock (_list.SyncRoot) {
						return _list.IsReadOnly;
					}
				}
			}

			public override bool IsSynchronized {
				get {
					lock (_list.SyncRoot) {
						return _list.IsSynchronized;
					}
				}
			}

			public override object this[int index] {
				get {
					lock (_list.SyncRoot) {
						return _list[index];
					}
				}
				set {
					lock (_list.SyncRoot) {
						_list[index] = value;
					}
				}
			}

			// methods
			public override int Add (object value) {
				lock (_list.SyncRoot) {
					return _list.Add(value);
				}
			}

			public override void AddRange (ICollection c) {
				lock (_list.SyncRoot) {
					_list.AddRange(c);
				}
			}

			public override int BinarySearch (int index, int count, object value, IComparer comparer) {
				lock (_list.SyncRoot) {
					return Array.BinarySearch (dataArray, index, count, value, comparer);
				}
			}

			public override void Clear () {
				lock (_list.SyncRoot) {
					_list.Clear();
				}
			}

			public override object Clone () {
				lock (_list.SyncRoot) {
					return new SyncArrayList((ArrayList) _list.Clone());
				}
			}

			public override bool Contains (object item) {
				lock (_list.SyncRoot) {
					return _list.Contains(item);
				}
			}

			public override void CopyTo (Array array) {
				lock (_list.SyncRoot) {
					_list.CopyTo(array);
				}
			}

			public override void CopyTo (Array array, int arrayIndex) {
				lock (_list.SyncRoot) {
					_list.CopyTo(array, arrayIndex);
				}
			}

			public override void CopyTo (int index, Array array, int arrayIndex, int count) {
				lock (_list.SyncRoot) {
					_list.CopyTo(index, array, arrayIndex, count);
				}
			}

			public override IEnumerator GetEnumerator () {
				lock (_list.SyncRoot) {
					return _list.GetEnumerator();
				}
			}

			public override IEnumerator GetEnumerator (int index, int count) {
				lock (_list.SyncRoot) {
					return _list.GetEnumerator(index, count);
				}
			}

			public override ArrayList GetRange (int index, int count) {
				lock (_list.SyncRoot) {
					return new SyncArrayList(_list.GetRange(index, count));
				}
			}

			public override int IndexOf (object value, int startIndex, int count) {
				lock (_list.SyncRoot) {
					return _list.IndexOf(value, startIndex, count);
				}
			}

			public override void Insert (int index, object value) {
				lock (_list.SyncRoot) {
					_list.Insert(index, value);
				}
			}

			public override void InsertRange (int index, ICollection c) {
				lock (_list.SyncRoot) {
					_list.InsertRange(index, c);
				}
			}

			public override int LastIndexOf (object value, int startIndex) {
				lock (_list.SyncRoot) {
					return _list.LastIndexOf(value, startIndex);
				}
			}

			public override int LastIndexOf (object value, int startIndex, int count) {
				lock (_list.SyncRoot) {
					return _list.LastIndexOf(value, startIndex, count);
				}
			}

			public override void Remove (object obj) {
				lock (_list.SyncRoot) {
					_list.Remove(obj);
				}
			}

			public override void RemoveRange (int index, int count) {
				lock (_list.SyncRoot) {
					_list.RemoveRange(index, count);
				}
			}

			public override void Reverse (int index, int count) {
				lock (_list.SyncRoot) {
					_list.Reverse(index, count);
				}
			}

			public override void SetRange (int index, ICollection c) {
				lock (_list.SyncRoot) {
					_list.SetRange(index, c);
				}
			}

			public override void Sort (int index, int count, IComparer comparer) {
				lock (_list.SyncRoot) {
					_list.Sort(index, count, comparer);
				}
			}

			public override object[] ToArray() {
				lock (_list.SyncRoot) {
					return _list.ToArray();
				}
			}

			public override Array ToArray (Type type) {
				lock (_list.SyncRoot) {
					return _list.ToArray(type);
				}
			}

			public override void TrimToSize () {
				lock (_list.SyncRoot) {
					_list.TrimToSize();
				}
			}
		}
	}
}
