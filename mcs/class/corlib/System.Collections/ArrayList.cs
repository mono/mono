// System.Collections.ArrayList
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//    Duncan Mak (duncan@ximian.com)
//    Patrik Torstensson (totte@crepundia.net)
//    Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2001 Vladimir Vukicevic
// (C) 2002 Ximian, Inc.
// (C) 2003 Ben Maurer
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
			public override int Capacity {
				get { return list.Count; }
				set {
					// MS seems to do this
					if (value < list.Count) {
						throw new ArgumentOutOfRangeException
						("ArrayList Capacity being set to less than Count");
					}
					// There is no idiom for Capacity for
					// IList, so we do nothing, as MS does.
				}
			}

			public override void AddRange (ICollection collection) {
				if (collection == null)
					throw new ArgumentNullException ("colllection");
				
				if (IsFixedSize || IsReadOnly)
					throw new NotSupportedException ();

				InsertRange (Count, collection);
				
			}

			public override int BinarySearch (object value) {
				return BinarySearch (value, null);
			}

			public override int BinarySearch (object value, IComparer comparer) {
				return BinarySearch (0, Count, value, comparer);
			}

			public override int BinarySearch (int index, int count, object value,
				IComparer comparer) {

				if ((index < 0) || (count < 0))
					throw new ArgumentOutOfRangeException ();
				if ((index > Count) || (index + count) > Count)
					throw new ArgumentException ();
				
				if (comparer == null)
					comparer = Comparer.Default;
                
				int low = index;
				int hi = index + count - 1;
				int mid;
				while (low <= hi) {
					mid = (low + hi) / 2;
					int r = comparer.Compare (value, list [mid]);
					if (r == 0)
						return mid;
					if (r < 0)
						hi = mid-1;
					else 
						low = mid+1;
				}

				return ~low;
			}
			
			public override void CopyTo (Array array) {
				if (null == array)
					throw new ArgumentNullException("array");
				if (array.Rank > 1)
					throw new ArgumentException("array cannot be multidimensional");

				CopyTo (array, 0);
			}

			public override void CopyTo (int index, Array array,
				int arrayIndex, int count) {
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || arrayIndex < 0 || count < 0)
					throw new ArgumentOutOfRangeException ();
				if (array.Rank > 1 || index >= Count || Count > (array.Length - arrayIndex))
					throw new ArgumentException ();
				
				for (int i = index; i < index + count; i++)
					array.SetValue(list [i], arrayIndex++);
			}

			public override ArrayList GetRange (int index, int count) {
				if (index < 0 || count < 0)
					throw new ArgumentOutOfRangeException ();
				if (Count < (index + count))
					throw new ArgumentException ();
				
				return (ArrayList) new Range (this, index, count);
			}

			public override void InsertRange (int index, ICollection col) {
				if (col == null)
					throw new ArgumentNullException ();
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				if (IsReadOnly || IsFixedSize)
					throw new NotSupportedException ();

				if (index == Count) {
					foreach (object element in col)
						list.Insert (index++, element);
				}
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

			// Other overloads just call this
			public override void Sort (int index, int count, IComparer comparer) {
				if ((index < 0) || (count < 0))
					throw new ArgumentOutOfRangeException ();
				if ((index > Count) || (index + count) > Count)
					throw new ArgumentException ();
				if (IsReadOnly)
					throw new NotSupportedException ();
				
				// TODO: do some real sorting
				object [] tmpArr = new Object [count];
				CopyTo (index, tmpArr, 0, count);
				Array.Sort (tmpArr, 0, count, comparer);
				for (int i = 0; i < count; i++)
					list [i + index] = tmpArr [i];
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

			public override void TrimToSize () {
				// Microsoft does not implement this method
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
			object[] newDataArray = new object[capacity];
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
			if (index + count > this.count) {
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
			if (readOnly) {
				throw new NotSupportedException
					("Collection is read-only.");
			}

			if (fixedSize) {
				throw new NotSupportedException
					("Collection is fixed size.");
			}
			if ((index < 0) || (index >= this.count))
				throw new ArgumentOutOfRangeException ("index", "Index was out of range.  Must be non-negative and less than the size of the collection.");
			shiftElements (index, -1);
			this.count -= 1;
			version ++;
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

			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "Non-negative number required.");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Non-negative number required.");

			if (index + count > this.count)
				throw new ArgumentException ("Offset and length were out of bounds for the array " +
											 "or count is greater than the number of elements from index " +
											 "to the end of the source collection.");
 
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

		
		private class Range : ArrayList {
			ArrayList baseList;
			int baseIndex;
			int baseSize;
			long baseVersion;
			
			internal Range (ArrayList list, int index, int count)
			{
				baseList = list;
				baseIndex = index;
				baseSize = count;
				baseVersion = list.version;
			}
			
			int RealIndex (int index)
			{
				return index + baseIndex;
			}
			
			int RealEnd {
				get {return baseIndex + baseSize;}
			}
			
			
			private void CheckVersion ()
			{
				if (baseVersion != this.baseList.version)
					throw new InvalidOperationException ();
			}
			
			public override int Add (object value) 
			{
				CheckVersion ();
				baseList.Insert (RealEnd, value);
				baseVersion++;
				return baseSize++;
			}
			
			public override void AddRange (ICollection c)
			{
				CheckVersion ();
				baseList.InsertRange (RealEnd, c);
				baseVersion++;
				baseSize += c.Count;
			}
			
			public override int BinarySearch (int index, int count, object value, IComparer comparer)
			{
				CheckVersion ();
				
				if ((index < 0) || (count < 0))
					throw new ArgumentOutOfRangeException ();
				if ((index > Count) || (index + count) > Count)
					throw new ArgumentException ();
				
				int i = baseList.BinarySearch (RealIndex (index), count, value, comparer);
				
				// Account for how a BinarySearch works
				if (i >= 0) return i - baseIndex;
				return i + baseIndex;
			}
			
			public override int Capacity {
				get {return baseList.Capacity;}
             
				set {
					if (value < Count) 
						throw new ArgumentOutOfRangeException("value");
					// From profiling it looks like Microsoft does not do anything
					// This make sense, since we can't very well have a "capacity" for the range.
				}
			}
    

			public override void Clear ()
			{
				CheckVersion ();
				if (baseSize != 0) {
					baseList.RemoveRange (baseIndex, baseSize);
					baseVersion++;
					baseSize = 0;
				}
			}

			public override object Clone ()
			{
				CheckVersion ();
				
				// Debugging Microsoft shows that this is _exactly_ how they do it.
				Range arrayList = new Range (baseList, baseIndex, baseSize);
				arrayList.baseList = (ArrayList)baseList.Clone ();
				
				return arrayList;
			}

			public override bool Contains (object item)
			{
				CheckVersion ();
				
				// Is much faster to check for null than to call Equals.
				if (item == null) {
					for (int i = 0; i < baseSize; i++)
						if (baseList[baseIndex + i] == null)
							return true;
					return false;
				} else {
					for (int i = 0; i < baseSize; i++)
						if (item.Equals (baseList [baseIndex + i]))
							return true;
					return false;
				}
			}
    
		
			public override void CopyTo (Array array, int index) 
			{
				CheckVersion ();
				if (null == array)
					throw new ArgumentNullException ("array");
				if (array.Rank > 1)
					throw new ArgumentException ("array cannot be multidimensional");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (array.Length - index < baseSize)
					throw new ArgumentException ();
				
				Array.Copy (baseList.dataArray, baseIndex, array, index, baseSize);
			}

			public override void CopyTo (int index, Array array, int arrayIndex, int count)
			{
				CheckVersion ();
				
				if (null == array)
					throw new ArgumentNullException ("array");
				if (array.Rank > 1)
					throw new ArgumentException ("array cannot be multidimensional");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");
				if (array.Length - index < baseSize)
					throw new ArgumentException ();
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				Array.Copy (baseList.dataArray, RealIndex (index), array, arrayIndex, count);
			}

			public override int Count {
				get {
					CheckVersion ();
					return baseSize; 
				}
			}

			public override bool IsReadOnly {
				get { return baseList.IsReadOnly; }
			}

			public override bool IsFixedSize {
				get { return baseList.IsFixedSize; }
			}

			public override bool IsSynchronized {
				get { return baseList.IsSynchronized; }
			}
								
			public override IEnumerator GetEnumerator ()
			{
				return GetEnumerator (0, baseSize);
			}

			public override IEnumerator GetEnumerator (int index, int count) 
			{
				CheckVersion ();
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");			
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				return baseList.GetEnumerator (RealIndex (index), count);
			}

			public override ArrayList GetRange (int index, int count)
			{
				CheckVersion ();
				
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");			
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				// We have to create a wrapper around a wrapper
				// because if we update the inner most wrapper, the
				// outer ones must still function. If we just wrapped
				// the outer most ArrayList, the others would not update
				// their version.
				return new Range (this, index, count);
			}

			public override object SyncRoot {
				get {return baseList.SyncRoot;}
			}
        

			public override int IndexOf (object value)
			{
				CheckVersion ();
				int i = baseList.IndexOf (value, baseIndex, baseSize);
				
				if (i >= 0) return i - baseIndex;
				else return -1;
			}

			public override int IndexOf (object value, int startIndex)
			{
				CheckVersion ();
				if (startIndex < 0 || startIndex > baseSize)
					throw new ArgumentOutOfRangeException ();

				int i = baseList.IndexOf (value, RealIndex (startIndex), baseSize - startIndex);
				if (i >= 0) return i - baseIndex;
				return -1;
			}
			
			public override int IndexOf (object value, int startIndex, int count)
			{
				CheckVersion ();
				if (startIndex < 0 || startIndex > baseSize)
					throw new ArgumentOutOfRangeException("startIndex");
					
				if (count < 0 || (startIndex > baseSize - count))
					throw new ArgumentOutOfRangeException("count");

				int i = baseList.IndexOf (value, RealIndex (startIndex), count);
				if (i >= 0) return i - baseIndex;
				return -1;
			}

			public override void Insert (int index, object value)
			{
				CheckVersion ();
				if (index < 0 || index > baseSize)
					throw new ArgumentOutOfRangeException("index");
				baseList.Insert ( RealIndex (index), value);
				baseVersion++;
				baseSize++;
			}

			public override void InsertRange (int index, ICollection c) 
			{
				CheckVersion ();
				if (index < 0 || index > baseSize) 
					throw new ArgumentOutOfRangeException("index");
				
				baseList.InsertRange (RealIndex (index), c);
				baseVersion++;
				baseSize += c.Count;
			}

			public override int LastIndexOf (object value)
			{
				CheckVersion ();
				int i = baseList.LastIndexOf (value, baseIndex, baseSize);
				if (i >= 0) return i - baseIndex;
				return -1;
			}

			public override int LastIndexOf (object value, int startIndex)
			{
				return LastIndexOf (value, startIndex, startIndex + 1);
			}

			public override int LastIndexOf (object value, int startIndex, int count)
			{
				CheckVersion ();
				if (baseSize == 0)
					return -1;
   
				if (startIndex < 0 || startIndex >= baseSize)
					throw new ArgumentOutOfRangeException("startIndex");
				
				int i = baseList.LastIndexOf (value, RealIndex (startIndex), count);
				if (i >= 0) return i - baseIndex;
				return -1;
			}

			// Remove will just call the overrided methods in here

			public override void RemoveAt (int index) 
			{
				CheckVersion ();
				if (index < 0 || index >= baseSize) 
					throw new ArgumentOutOfRangeException ("index");
				baseList.RemoveAt (RealIndex (index));
				baseVersion++;
				baseSize--;
			}

			public override void RemoveRange (int index, int count)
			{
				CheckVersion ();
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				baseList.RemoveRange (RealIndex (index), count);
				baseVersion++;
				baseSize -= count;
			}

			public override void Reverse (int index, int count)
			{
				CheckVersion ();
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				baseList.Reverse (RealIndex (index), count);
				baseVersion++;
			}


			public override void SetRange (int index, ICollection c) 
			{
				CheckVersion ();
				if (index < 0 || index >= baseSize) 
					throw new ArgumentOutOfRangeException("index");
				
				baseList.SetRange (RealIndex (index), c);
				baseVersion++;
			}
			
			// Other overloads just call this
			public override void Sort (int index, int count, IComparer comparer) 
			{
				CheckVersion ();
				
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0)
					throw new ArgumentOutOfRangeException ("count");
				if (baseSize - index < count)
					throw new ArgumentException ();
				
				baseList.Sort (RealIndex (index), count, comparer);
				baseVersion++;
			}

			public override object this [int index] {
				get {
					CheckVersion ();
					if (index < 0 || index >= baseSize)
						throw new ArgumentOutOfRangeException("index");
					
					return baseList[baseIndex + index];
				}
				set {
					CheckVersion ();
					if (index < 0 || index >= baseSize)
						throw new ArgumentOutOfRangeException("index");
					
					baseList [baseIndex + index] = value;
					baseVersion++;
				}
			}

			public override object [] ToArray () 
			{
				CheckVersion ();
				object [] array = new object [baseSize];
				Array.Copy (baseList.dataArray, baseIndex, array, 0, baseSize);
				return array;
			}

			public override Array ToArray (Type type)
			{
				CheckVersion ();
				if (type == null)
					throw new ArgumentNullException("type");
				
				Array array = Array.CreateInstance (type, baseSize);
				Array.Copy (baseList.dataArray, baseIndex, array, 0, baseSize);
				return array;
			}

			public override void TrimToSize ()
			{
				throw new NotSupportedException ("Can not trim range");
			}
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
