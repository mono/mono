// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
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

	public class ArrayList : IList, ICollection, IEnumerable, ICloneable {
		// constructors

		public ArrayList () {
			dataArray = new object[capacity];
		}

		public ArrayList (ICollection c) {
		}

		public ArrayList (int capacity) {
			dataArray = new object[capacity];
			this.capacity = capacity;
		}

		private ArrayList (object[] dataArray, int count, int capacity,
				   bool fixedSize, bool readOnly, bool synchronized)
		{
			this.dataArray = dataArray;
			this.count = count;
			this.capacity = capacity;
			this.fixedSize = fixedSize;
			this.readOnly = readOnly;
			this.synchronized = synchronized;
		}

		public static ArrayList ReadOnly (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.ReadOnly");
		}

		public static ArrayList ReadOnly (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.ReadOnly");
		}

		public static ArrayList Synchronized (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Synchronized");
		}

		public static ArrayList Synchronized (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Synchronized");
		}

		public static ArrayList FixedSize (ArrayList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.FixedSize");
		}

		public static ArrayList FixedSize (IList list) {
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

		public static ArrayList Adapter (IList list) {
			throw new NotImplementedException ("System.Collections.ArrayList.Adapter");
		}

		// properties

		private bool fixedSize = false;
		private bool readOnly = false;
		private bool synchronized = false;

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
					for (int i = startIndex; i < numelts; i++) {
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

		public virtual int Count {
			get {
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
				// FIXME -- should setting an index implicitly extend the array?
				// the docs aren't clear -- I'm assuming not, since the exception
				// is listed for both get and set
				if (index >= count) {
					throw new ArgumentOutOfRangeException ("index out of range");
				}

				if (readOnly) {
					throw new NotSupportedException ("Collection is read-only.");
				}

				dataArray[index] = value;
			}
		}

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
			return count-1;
		}

		public virtual void AddRange (ICollection c) {
			throw new NotImplementedException ("System.Collections.ArrayList.AddRange");
		}

		public virtual int BinarySearch (object value) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		public virtual int BinarySearch (object value, IComparer comparer) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		public virtual int BinarySearch (int index, int count,
						 object value, IComparer comparer) {
			throw new NotImplementedException ("System.Collections.ArrayList.BinarySearch");
		}

		public virtual void Clear () {
			count = 0;
			setSize(capacity);
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
		}

		public virtual void CopyTo (Array array, int arrayIndex) {
		}

		public virtual void CopyTo (int index, Array array,
					    int arrayIndex, int count) {
		}

		public virtual IEnumerator GetEnumerator () {
			return null;
		}

		public virtual IEnumerator GetEnumerator (int index, int count) {
			return null;
		}

		public virtual ArrayList GetRange (int index, int count) {
			return null;
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

			if (index < 0 || index >= count) {
				throw new ArgumentOutOfRangeException ("index < 0 or index >= count");
			}

			shiftElements (index, 1);
			dataArray[index] = value;
			count++;
		}

		public virtual void InsertRange (int index, ICollection c) {
		}

		public virtual int LastIndexOf (object value) {
			return LastIndexOf (value, 0, count);
		}

		public virtual int LastIndexOf (object value, int startIndex) {
			return LastIndexOf (value, startIndex, count - startIndex);
		}

		public virtual int LastIndexOf (object value, int StartIndex,
						int count)
			{
				for (int i = count - 1; i >= 0; i--) {
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
		}

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
			// FIXME: implement this
		}
	}
}
