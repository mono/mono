//
// System.Collections.ArrayList
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

	[Serializable]
	public class ArrayList : IList, ICollection, IEnumerable, ICloneable {
		// constructors

		public ArrayList () {
			dataArray = new object[capacity];
		}

		public ArrayList (ICollection c) {
			if (null == c) {
				throw new ArgumentNullException();
			}
			dataArray = new object[c.Count];
			this.capacity = c.Count;
			foreach(object o in c) {
				Add(o);
			}
		}

		public ArrayList (int capacity) {
			if (capacity < 0) {
				throw new ArgumentOutOfRangeException("capacity", capacity, "Value must be greater than or equal to zero.");
			}
			dataArray = new object[capacity];
			this.capacity = capacity;
		}

		private ArrayList (object[] dataArray, int count, int capacity,
				   bool fixedSize, bool readOnly, bool synchronized)
		{
			this.dataArray = (object[])dataArray.Clone();
			this.count = count;
			this.capacity = capacity;
			this.fixedSize = fixedSize;
			this.readOnly = readOnly;
			this.synchronized = synchronized;
		}

		[MonoTODO]
		public static ArrayList ReadOnly (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.ReadOnly");
		}

		[MonoTODO]
		public static IList ReadOnly (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.ReadOnly");
		}

		[MonoTODO]
		public static ArrayList Synchronized (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Synchronized");
		}

		[MonoTODO]
		public static IList Synchronized (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Synchronized");
		}

		[MonoTODO]
		public static ArrayList FixedSize (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.FixedSize");
		}

		[MonoTODO]
		public static IList FixedSize (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.FixedSize");
		}

		public static ArrayList Repeat (object value, int count) {
			ArrayList al = new ArrayList (count);
			for (int i = 0; i < count; i++) {
				al.dataArray[i] = value;
			}
			al.count = count;

			return al;
		}

		[MonoTODO]
		public static ArrayList Adapter (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Adapter");
		}

		// properties

		private bool fixedSize = false;
		private bool readOnly = false;
		private bool synchronized = false;

		private long version = 0;
		private ArrayList source = null;

		private int count = 0;
		private int capacity = 16;

		private object[] dataArray;

		private void copyDataArray (object[] outArray) {
			for (int i = 0; i < count; i++) {
				outArray[i] = dataArray[i];
			}
		}

		private void setSize (int newSize) {
			if (newSize == capacity) {
				return;
			}

			// note that this assumes that we've already sanity-checked
			// the new size
			object[] newDataArray = new object[newSize];
			copyDataArray (newDataArray);
			dataArray = newDataArray;

			capacity = newSize;
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
					for (int i = startIndex; i <= numelts; i++) {
						dataArray[i] = dataArray[i - numshift];
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

		[MonoTODO]
		public virtual object SyncRoot {
			get {
				throw new NotImplementedException ("System.Collections.ArrayList.SyncRoot.get");
			}
		}


		// methods

		public virtual int Add (object value) {
			if (readOnly) {
				throw new NotSupportedException ("Collection is read-only.");
			}

			if (count + 1 >= capacity) {
				setSize (capacity * 2);
			}

			dataArray[count++] = value;
			version++;
			return count-1;
		}

		public virtual void AddRange (ICollection c) {
			int cc = c.Count;
			if (count + cc >= capacity)
				Capacity = cc < count? count * 2: count + cc + 1;
			c.CopyTo (dataArray, count);
			count += cc;
			version++;
		}

		[MonoTODO]
		public virtual int BinarySearch (object value) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		[MonoTODO]
		public virtual int BinarySearch (object value, IComparer comparer) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		[MonoTODO]
		public virtual int BinarySearch (int index, int count,
						 object value, IComparer comparer) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		public virtual void Clear () {
			count = 0;
			setSize(capacity);
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
			CopyTo (array, 0);
		}

		public virtual void CopyTo (Array array, int arrayIndex) {
			System.Array.Copy (dataArray, 0, array, arrayIndex, count);
		}

		[MonoTODO]
		public virtual void CopyTo (int index, Array array,
					    int arrayIndex, int count) {
			// FIXME: check count...
			System.Array.Copy (dataArray, index, array, arrayIndex, count);
		}

		[Serializable]
		private class ArrayListEnumerator : IEnumerator {
			private object[] data;
			private int idx;
			private int start;
			private int num;

			public ArrayListEnumerator(int index, int count, object[] items) {
				data = items;
				start = index;
				num = count;
				idx = start - 1;
			}

			public object Current {
				get {
					return data [idx];
				}
			}
			public bool MoveNext() {
				if (++idx < start + num)
					return true;
				return false;
			}
			public void Reset() {
				idx = start - 1;
			}
		}

		public virtual IEnumerator GetEnumerator () {
			return new ArrayListEnumerator(0, this.Count, dataArray);
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
			return new ArrayListEnumerator(index, count, dataArray);
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

			if (index < 0 || index >= capacity) {
				throw new ArgumentOutOfRangeException ("index < 0 or index >= capacity");
			}

			shiftElements (index, 1);
			dataArray[index] = value;
			count++;
			version++;
		}

		[MonoTODO]
		public virtual void InsertRange (int index, ICollection c) {
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

		public virtual int LastIndexOf (object value, int StartIndex,
						int count)
			{
				int EndIndex = StartIndex - count + 1;
				for (int i = StartIndex; i >= EndIndex; i--) {
					if (Object.Equals (dataArray[i], value)) {
						return i;
					}
				}

				return -1;
			}

		public virtual void Remove (object obj) {
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

		[MonoTODO]
		public virtual void SetRange (int index, ICollection c) {
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

		[MonoTODO]
		public virtual void TrimToSize () {
			// FIXME: implement this
			version++;
		}
	}
}
