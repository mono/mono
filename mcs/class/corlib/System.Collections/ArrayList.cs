// ArrayList.cs
// 
// Implementation of the ECMA ArrayList.
//
// Copyright (c) 2003 Thong (Tum) Nguyen [tum@veridicus.com]
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections 
{
	[Serializable]
#if INSIDE_CORLIB
	[ComVisible(true)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	public class ArrayList : IList, ICloneable, ICollection, IEnumerable {
#else
	internal class ArrayList : IList {
#endif
		#region Enumerator

		private sealed class ArrayListEnumerator
			: IEnumerator, ICloneable 
		{			
			private object m_Current;
			private ArrayList m_List;
			private int m_Pos;
			private int m_Index;
			private int m_Count;
			private int m_ExpectedStateChanges;

			public ArrayListEnumerator(ArrayList list)
				: this(list, 0, list.Count) 
			{				
			}

			public object Clone() 
			{
				return this.MemberwiseClone();
			}

			public ArrayListEnumerator(ArrayList list, int index, int count) 
			{
				m_List = list;
				m_Index = index;
				m_Count = count;
				m_Pos = m_Index - 1;
				m_Current = null;
				m_ExpectedStateChanges = list._version;
			}

			public object Current 
			{
				get 
				{
					if (m_Pos == m_Index - 1) {
						throw new InvalidOperationException("Enumerator unusable (Reset pending, or past end of array.");
					}

					return m_Current;
				}
			}

			public bool MoveNext() 
			{
				if (m_List._version != m_ExpectedStateChanges) 
				{
					throw new InvalidOperationException("List has changed.");
				}

				m_Pos++;

				if (m_Pos - m_Index < m_Count) 
				{
					m_Current = m_List[m_Pos];

					return true;
				}

				return false;
			}

			public void Reset() 
			{
				m_Current = null;
				m_Pos = m_Index - 1;
			}
		}
		
		sealed class SimpleEnumerator : IEnumerator, ICloneable
		{
			ArrayList list;
			object currentElement;
			int index;
			int version;
			static readonly object endFlag = new object ();
							
			public SimpleEnumerator (ArrayList list)
			{
				this.list = list;
				index = -1;
				version = list._version;
				currentElement = endFlag;
			}

			public object Clone ()
			{
				return MemberwiseClone ();
			}
	
			public bool MoveNext ()
			{
				if (version != list._version)
					throw new InvalidOperationException("List has changed.");
				
				if (++index < list.Count) {
					currentElement = list [index];
					return true;
				} else {
					currentElement = endFlag;
					return false;
				}
			}
	
			public object Current {
				get {
					if (currentElement == endFlag) {
						if (index == -1)
							throw new InvalidOperationException ("Enumerator not started");
						else
							throw new InvalidOperationException ("Enumerator ended");						 
					}
					
					return currentElement;
				}
			}
	
			public void Reset ()
			{
				if (version != list._version)
					throw new InvalidOperationException ("List has changed.");
				
				currentElement = endFlag;
				index = -1;
			}
		}

		#endregion

		#region ArrayListAdapter

		/// <summary>
		/// Adapts various ILists into an ArrayList.
		/// </summary>
		[Serializable]
		private sealed class ArrayListAdapter
			: ArrayList 
		{
			private sealed class EnumeratorWithRange
				: IEnumerator, ICloneable 
			{
				private int m_StartIndex;
				private int m_Count;
				private int m_MaxCount;
				private IEnumerator m_Enumerator;

				public EnumeratorWithRange(IEnumerator enumerator, int index, int count) 
				{
					m_Count = 0;
					m_StartIndex = index;
					m_MaxCount = count;
					m_Enumerator = enumerator;

					Reset();
				}

				public object Clone() 
				{
					return this.MemberwiseClone();
				}

				public object Current 
				{
					get 
					{
						return m_Enumerator.Current;
					}
				}

				public bool MoveNext() 
				{
					if (m_Count >= m_MaxCount) 
					{
						return false;
					}
					
					m_Count++;

					return m_Enumerator.MoveNext();
				}

				public void Reset() 
				{
					m_Count = 0;				
					m_Enumerator.Reset();

					for (int i = 0; i < m_StartIndex; i++) 
					{
						m_Enumerator.MoveNext();
					}
				}
			}

			private IList m_Adaptee;

			public ArrayListAdapter(IList adaptee)
				: base(0, true) 
			{
				m_Adaptee = adaptee;
			}

			public override object this[int index] 
			{
				get 
				{
					return m_Adaptee[index];
				}

				set 
				{
					m_Adaptee[index] = value;
				}
			}	

			public override int Count 
			{
				get 
				{
					return m_Adaptee.Count;
				}
			}

			public override int Capacity 
			{
				get 
				{
					return m_Adaptee.Count;
				}

				set 
				{
					if (value < m_Adaptee.Count) 
					{
						throw new ArgumentException("capacity");
					}
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					return m_Adaptee.IsFixedSize;
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					return m_Adaptee.IsReadOnly;
				}
			}

			public override object SyncRoot 
			{
				get 
				{
					return m_Adaptee.SyncRoot;
				}
			}

			public override int Add(object value) 
			{
				return m_Adaptee.Add(value);
			}

			public override void Clear() 
			{
				m_Adaptee.Clear();
			}

			public override bool Contains(object value) 
			{
				return m_Adaptee.Contains(value);
			}

			public override int IndexOf(object value) 
			{
				return m_Adaptee.IndexOf(value);
			}

			public override int IndexOf(object value, int startIndex) 
			{
				return IndexOf(value, startIndex, m_Adaptee.Count - startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count) 
			{
				if (startIndex < 0 || startIndex > m_Adaptee.Count) 
				{
					ThrowNewArgumentOutOfRangeException ("startIndex", startIndex,
						"Does not specify valid index.");
				}

				if (count < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("count", count,
						"Can't be less than 0.");
				}

				// re-ordered to avoid possible integer overflow
				if (startIndex > m_Adaptee.Count - count) {
					// LAMESPEC: Every other method throws ArgumentException
					throw new ArgumentOutOfRangeException("count",
						"Start index and count do not specify a valid range.");
				}

				if (value == null) 
				{
					for (int i = startIndex; i < startIndex + count; i++) 
					{
						if (m_Adaptee[i] == null) 
						{
							return i;
						}
					}
				}
				else 
				{
					for (int i = startIndex; i < startIndex + count; i++) 
					{
						if (value.Equals(m_Adaptee[i])) 
						{
							return i;
						}
					}
				}

				return -1;
			}

			public override int LastIndexOf(object value) 
			{
				return LastIndexOf(value, m_Adaptee.Count - 1);
			}

			public override int LastIndexOf(object value, int startIndex) 
			{
				return LastIndexOf(value, startIndex, startIndex + 1);
			}

			public override int LastIndexOf(object value, int startIndex, int count) 
			{
				if (startIndex < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("startIndex", startIndex, "< 0");
				}

				if (count < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("count", count, "count is negative.");
				}

				if (startIndex - count  + 1 < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("count", count, "count is too large.");
				}

				if (value == null) 
				{
					for (int i = startIndex; i > startIndex - count; i--) 
					{
						if (m_Adaptee[i] == null) 
						{
							return i;
						}
					}
				}
				else 
				{
					for (int i = startIndex; i > startIndex - count; i--) 
					{
						if (value.Equals(m_Adaptee[i])) 
						{
							return i;
						}
					}
				}

				return -1;
			}

			public override void Insert(int index, object value) 
			{
				m_Adaptee.Insert(index, value);
			}

			public override void InsertRange(int index, ICollection c) 
			{
				if (c == null) 
				{
					throw new ArgumentNullException("c");
				}

				if (index > m_Adaptee.Count) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index must be >= 0 and <= Count.");
				}

				foreach (object value in c) 
				{
					m_Adaptee.Insert(index++, value);
				}
			}

			public override void Remove(object value) 
			{
				m_Adaptee.Remove(value);
			}

			public override void RemoveAt(int index) 
			{
				m_Adaptee.RemoveAt(index);
			}

			public override void RemoveRange(int index, int count) 
			{
				CheckRange(index, count, m_Adaptee.Count);

				for (int i = 0; i < count; i++) 
				{
					m_Adaptee.RemoveAt(index);
				}			
			}

			public override void Reverse() 
			{
				Reverse(0, m_Adaptee.Count);
			}

			public override void Reverse(int index, int count) 
			{
				object tmp;

				CheckRange(index, count, m_Adaptee.Count);
			
				for (int i = 0; i < count / 2; i++) 
				{
					tmp = m_Adaptee[i + index];
					m_Adaptee[i + index] = m_Adaptee[(index + count) - i + index - 1];
					m_Adaptee[(index + count) - i + index - 1] = tmp;				
				}
			}

			public override void SetRange(int index, ICollection c) 
			{
				if (c == null) 
				{
					throw new ArgumentNullException("c");
				}

				if (index < 0 || index + c.Count > m_Adaptee.Count) 
				{
					throw new ArgumentOutOfRangeException("index");
				}

				int x = index;

				foreach (object value in c) 
				{
					m_Adaptee[x++] = value;
				}
			}

			public override void CopyTo(System.Array array) 
			{
				m_Adaptee.CopyTo(array, 0);
			}

			public override void CopyTo(System.Array array, int index) 
			{
				m_Adaptee.CopyTo(array, index);
			}

			public override void CopyTo(int index, System.Array array, int arrayIndex, int count) 
			{
				if (index < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Can't be less than zero.");
				}

				if (arrayIndex < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("arrayIndex", arrayIndex,
						"Can't be less than zero.");
				}

				if (count < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Can't be less than zero.");
				}

				if (index >= m_Adaptee.Count) 
				{
					throw new ArgumentException("Can't be more or equal to list count.",
						"index");
				}

				if (array.Rank > 1) 
				{
					throw new ArgumentException("Can't copy into multi-dimensional array.");
				}

				if (arrayIndex >= array.Length) 
				{
					throw new ArgumentException("arrayIndex can't be greater than array.Length - 1.");
				}

				if (array.Length - arrayIndex + 1 < count) 
				{
					throw new ArgumentException("Destination array is too small.");
				}

				// re-ordered to avoid possible integer overflow
				if (index > m_Adaptee.Count - count) {
					throw new ArgumentException("Index and count do not denote a valid range of elements.", "index");
				}

				for (int i = 0; i < count; i++) 
				{
					array.SetValue(m_Adaptee[index + i], arrayIndex + i); 
				}
			}

			public override bool IsSynchronized 
			{
				get 
				{
					return m_Adaptee.IsSynchronized;
				}
			}

			public override IEnumerator GetEnumerator() 
			{
				return m_Adaptee.GetEnumerator();
			}
			
			public override IEnumerator GetEnumerator(int index, int count) 
			{
				CheckRange(index, count, m_Adaptee.Count);

				return new EnumeratorWithRange(m_Adaptee.GetEnumerator(), index, count);
			}

			public override void AddRange(ICollection c) 
			{
				foreach (object value in c) 
				{
					m_Adaptee.Add(value);
				}
			}

			public override int BinarySearch(object value) 
			{
				return BinarySearch(value, null);
			}

			public override int BinarySearch(object value, IComparer comparer) 
			{
				return BinarySearch(0, m_Adaptee.Count, value, comparer);
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer) 
			{
				int r, x, y, z;

				// Doing a direct BinarySearch on the adaptee will perform poorly if the adaptee is a linked-list.
				// Alternatives include copying the adaptee to a temporary array first.

				CheckRange(index, count, m_Adaptee.Count);

				if (comparer == null) 
				{
					comparer = Comparer.Default;
				}

				x = index;
				y = index + count - 1;

				while (x <= y) 
				{
					// Be careful with overflows
					z = x + ((y - x) / 2);
				
					r = comparer.Compare(value, m_Adaptee[z]);
				
					if (r < 0) 
					{
						y = z - 1;
					}
					else if (r > 0) 
					{
						x = z + 1;
					}	
					else 
					{
						return z;
					}
				}

				return ~x;
			}

			public override object Clone() 
			{
				return new ArrayList.ArrayListAdapter(m_Adaptee);
			}

			public override ArrayList GetRange(int index, int count) 
			{
				CheckRange(index, count, m_Adaptee.Count);
				
				return new RangedArrayList(this, index, count);
			}

			public override void TrimToSize() 
			{
				// N/A
			}

			public override void Sort() 
			{
				Sort(Comparer.Default);
			}

			public override void Sort(IComparer comparer) 
			{
				Sort(0, m_Adaptee.Count, comparer);
			}

			public override void Sort(int index, int count, IComparer comparer) 
			{
				CheckRange(index, count, m_Adaptee.Count);

				if (comparer == null) 
				{
					comparer = Comparer.Default;
				}

				// Doing a direct sort on the adaptee will perform poorly if the adaptee is a linked-list.
				// Alternatives include copying the adaptee into a temporary array first.

				QuickSort(m_Adaptee, index, index + count - 1, comparer);
			}


			/// <summary>
			/// Swaps two items in a list at the specified indexes.
			/// </summary>
			private static void Swap(IList list, int x, int y) 
			{
				object tmp;
				
				tmp = list[x];
				list[x] = list[y];
				list[y] = tmp;
			}

			/// <summary>
			/// Quicksort for lists.
			/// </summary>
			/// <remarks>
			/// This function acts as both qsort() and partition().
			/// </remarks>
			internal static void QuickSort(IList list, int left, int right, IComparer comparer) 
			{
				int i, j, middle;
				object pivot;
					
				if (left >= right) 
				{
					return;
				}

				// Pick the pivot using the median-of-three strategy.

				// Be careful with overflows
				middle = left + ((right - left) / 2);

				if (comparer.Compare(list[middle], list[left]) < 0) 
				{
					Swap(list, middle, left);
				}

				if (comparer.Compare(list[right], list[left]) < 0) 
				{
					Swap(list, right, left);
				}

				if (comparer.Compare(list[right], list[middle]) < 0) 
				{
					Swap(list, right, middle);
				}
		
				if (right - left + 1 <= 3) 
				{
					return;
				}
		
				// Put the pivot in right - 1.
				Swap(list, right - 1, middle);

				// List should look like:
				//
				// [Small] ..Numbers.. [Middle] ..Numbers.. [Pivot][Large]

				pivot = list[right - 1];

				// Sort from (left + 1) to (right - 2).

				i = left;
				j = right - 1;			
		
				for (;;) 
				{
					while (comparer.Compare(list[++i], pivot) < 0);
					while (comparer.Compare(list[--j], pivot) > 0);
			
					if (i < j) 
					{
						Swap(list, i, j);
					}
					else 
					{
						break;
					}
				}

				// Put pivot into the right position (real middle).

				Swap(list, right - 1, i);

				// Recursively sort the left and right sub lists.

				QuickSort(list, left, i - 1, comparer);
				QuickSort(list, i + 1, right, comparer);		
			}

			public override object[] ToArray() 
			{
				object[] retval;

				retval = new object[m_Adaptee.Count];

				m_Adaptee.CopyTo(retval, 0);
			
				return retval;
			}

			public override Array ToArray(Type elementType) 
			{
				Array retval;

				retval = Array.CreateInstance(elementType, m_Adaptee.Count);

				m_Adaptee.CopyTo(retval, 0);
			
				return retval;
			}
		}

		#endregion // ArrayListAdapter

		//
		// ArrayList wrappers
		//

		#region ArrayListWrapper

		/// <summary>
		/// Base wrapper/decorator for ArrayLists.  Simply delegates all methods to
		/// the underlying wrappee.
		/// </summary>
		[Serializable]
		private class ArrayListWrapper
			: ArrayList 
		{		
			protected ArrayList m_InnerArrayList;

			#region Constructors

			public ArrayListWrapper(ArrayList innerArrayList) 
			{			
				m_InnerArrayList = innerArrayList;
			}		

			#endregion

			#region Indexers

			public override object this[int index] 
			{
				get 
				{
					return m_InnerArrayList[index];
				}

				set 
				{
					m_InnerArrayList[index] = value;
				}
			}

			#endregion

			#region Properties

			public override int Count 
			{
				get 
				{
					return m_InnerArrayList.Count;
				}
			}

			public override int Capacity 
			{
				get 
				{
					return m_InnerArrayList.Capacity;
				}

				set 
				{
					m_InnerArrayList.Capacity = value;
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					return m_InnerArrayList.IsFixedSize;
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					return m_InnerArrayList.IsReadOnly;
				}
			}

			public override bool IsSynchronized 
			{
				get 
				{
					return m_InnerArrayList.IsSynchronized;
				}
			}

			public override object SyncRoot 
			{
				get 
				{
					return m_InnerArrayList.SyncRoot;
				}
			}

			#endregion

			#region Methods

			public override int Add(object value) 
			{
				return m_InnerArrayList.Add(value);
			}

			public override void Clear() 
			{
				m_InnerArrayList.Clear();
			}

			public override bool Contains(object value) 
			{
				return m_InnerArrayList.Contains(value);
			}

			public override int IndexOf(object value) 
			{
				return m_InnerArrayList.IndexOf(value);
			}

			public override int IndexOf(object value, int startIndex) 
			{
				return m_InnerArrayList.IndexOf(value, startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count) 
			{
				return m_InnerArrayList.IndexOf(value, startIndex, count);
			}

			public override int LastIndexOf(object value) 
			{
				return m_InnerArrayList.LastIndexOf(value);
			}

			public override int LastIndexOf(object value, int startIndex) 
			{
				return m_InnerArrayList.LastIndexOf(value, startIndex);
			}

			public override int LastIndexOf(object value, int startIndex, int count) 
			{
				return m_InnerArrayList.LastIndexOf(value, startIndex, count);
			}

			public override void Insert(int index, object value) 
			{
				m_InnerArrayList.Insert(index, value);
			}

			public override void InsertRange(int index, ICollection c) 
			{
				m_InnerArrayList.InsertRange(index, c);
			}

			public override void Remove(object value) 
			{
				m_InnerArrayList.Remove(value);
			}

			public override void RemoveAt(int index) 
			{
				m_InnerArrayList.RemoveAt(index);
			}

			public override void RemoveRange(int index, int count) 
			{
				m_InnerArrayList.RemoveRange(index, count);
			}

			public override void Reverse() 
			{
				m_InnerArrayList.Reverse();
			}

			public override void Reverse(int index, int count) 
			{
				m_InnerArrayList.Reverse(index, count);
			}

			public override void SetRange(int index, ICollection c) 
			{
				m_InnerArrayList.SetRange(index, c);
			}

			public override void CopyTo(System.Array array) 
			{
				m_InnerArrayList.CopyTo(array);
			}

			public override void CopyTo(System.Array array, int index) 
			{
				m_InnerArrayList.CopyTo(array, index);
			}

			public override void CopyTo(int index, System.Array array, int arrayIndex, int count) 
			{
				m_InnerArrayList.CopyTo(index, array, arrayIndex, count);
			}

			public override IEnumerator GetEnumerator() 
			{
				return m_InnerArrayList.GetEnumerator();
			}

			public override IEnumerator GetEnumerator(int index, int count) 
			{
				return m_InnerArrayList.GetEnumerator(index, count);
			}

			public override void AddRange(ICollection c) 
			{
				m_InnerArrayList.AddRange(c);
			}

			public override int BinarySearch(object value) 
			{
				return m_InnerArrayList.BinarySearch(value);
			}

			public override int BinarySearch(object value, IComparer comparer) 
			{
				return m_InnerArrayList.BinarySearch(value, comparer);
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer) 
			{
				return m_InnerArrayList.BinarySearch(index, count, value, comparer);
			}

			public override object Clone() 
			{
				return m_InnerArrayList.Clone();
			}

			public override ArrayList GetRange(int index, int count) 
			{
				return m_InnerArrayList.GetRange(index, count);
			}

			public override void TrimToSize() 
			{
				m_InnerArrayList.TrimToSize();
			}

			public override void Sort() 
			{
				m_InnerArrayList.Sort();
			}

			public override void Sort(IComparer comparer) 
			{
				m_InnerArrayList.Sort(comparer);
			}

			public override void Sort(int index, int count, IComparer comparer) 
			{
				m_InnerArrayList.Sort(index, count, comparer);
			}

			public override object[] ToArray() 
			{
				return m_InnerArrayList.ToArray();
			}

			public override Array ToArray(Type elementType) 
			{
				return m_InnerArrayList.ToArray(elementType);
			}

			#endregion
		}

		#endregion

		#region SynchronizedArrayListWrapper

		/// <summary>
		/// ArrayListWrapper that synchronizes calls to all methods/properties.
		/// </summary>
		/// <remarks>
		/// Works by just synchronizing all method calls.  In the future careful optimisation
		/// could give better performance...
		/// </remarks>
		[Serializable]
		private sealed class SynchronizedArrayListWrapper
			: ArrayListWrapper 
		{
			private object m_SyncRoot;

			#region Constructors

			/// <summary>
			/// Creates a new synchronized wrapper for the given <see cref="ArrayList"/>.
			/// </summary>
			/// <param name="innerArrayList"></param>
			internal SynchronizedArrayListWrapper(ArrayList innerArrayList)
				: base(innerArrayList) 
			{
				m_SyncRoot = innerArrayList.SyncRoot;
			}		

			#endregion

			#region Indexers

			public override object this[int index] 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerArrayList[index];
					}
				}

				set 
				{
					lock (m_SyncRoot) 
					{
						m_InnerArrayList[index] = value;
					}
				}
			}	
	
			#endregion

			#region Properties
			
			// Some of these properties may be calculated so it's best to synchronize 
			// them even though it might cause a performance hit.
			// Better safe than sorry ;D.

			public override int Count 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerArrayList.Count;
					}
				}
			}

			public override int Capacity 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerArrayList.Capacity;
					}
				}

				set 
				{
					lock (m_SyncRoot) 
					{
						m_InnerArrayList.Capacity = value;
					}
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerArrayList.IsFixedSize;
					}
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerArrayList.IsReadOnly;
					}
				}
			}

			public override bool IsSynchronized 
			{
				get 
				{
					return true;
				}
			}

			public override object SyncRoot 
			{
				get 
				{
					return m_SyncRoot;
				}
			}

			#endregion

			#region Methods

			public override int Add(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.Add(value);
				}
			}

			public override void Clear() 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Clear();
				}
			}

			public override bool Contains(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.Contains(value);
				}
			}

			public override int IndexOf(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.IndexOf(value);
				}
			}

			public override int IndexOf(object value, int startIndex) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.IndexOf(value, startIndex);
				}
			}

			public override int IndexOf(object value, int startIndex, int count) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.IndexOf(value, startIndex, count);
				}
			}

			public override int LastIndexOf(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.LastIndexOf(value);			
				}
			}

			public override int LastIndexOf(object value, int startIndex) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.LastIndexOf(value, startIndex);
				}
			}

			public override int LastIndexOf(object value, int startIndex, int count) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.LastIndexOf(value, startIndex, count);
				}
			}

			public override void Insert(int index, object value) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Insert(index, value);
				}
			}

			public override void InsertRange(int index, ICollection c) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.InsertRange(index, c);
				}
			}

			public override void Remove(object value) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Remove(value);
				}
			}

			public override void RemoveAt(int index) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.RemoveAt(index);
				}
			}

			public override void RemoveRange(int index, int count) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.RemoveRange(index, count);
				}
			}

			public override void Reverse() 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Reverse();
				}
			}

			public override void Reverse(int index, int count) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Reverse(index, count);
				}
			}

			public override void CopyTo(System.Array array) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.CopyTo(array);
				}
			}

			public override void CopyTo(System.Array array, int index) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.CopyTo(array, index);
				}
			}

			public override void CopyTo(int index, System.Array array, int arrayIndex, int count) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.CopyTo(index, array, arrayIndex, count);
				}
			}

			public override IEnumerator GetEnumerator() 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.GetEnumerator();
				}
			}

			public override IEnumerator GetEnumerator(int index, int count) 
			{
				lock (m_SyncRoot) 
				{				
					return m_InnerArrayList.GetEnumerator(index, count);
				}
			}

			public override void AddRange(ICollection c) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.AddRange(c);
				}
			}

			public override int BinarySearch(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.BinarySearch(value);
				}
			}

			public override int BinarySearch(object value, IComparer comparer) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.BinarySearch(value, comparer);
				}
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.BinarySearch(index, count, value, comparer);
				}
			}

			public override object Clone() 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.Clone();
				}
			}

			public override ArrayList GetRange(int index, int count) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.GetRange(index, count);
				}
			}

			public override void TrimToSize() 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.TrimToSize();
				}
			}

			public override void Sort() 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Sort();
				}
			}

			public override void Sort(IComparer comparer) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Sort(comparer);
				}
			}

			public override void Sort(int index, int count, IComparer comparer) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerArrayList.Sort(index, count, comparer);
				}
			}

			public override object[] ToArray() 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.ToArray();
				}
			}

			public override Array ToArray(Type elementType) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerArrayList.ToArray(elementType);
				}
			}

			#endregion
		}

		#endregion

		#region FixedSizeArrayListWrapper

		[Serializable]
		private class FixedSizeArrayListWrapper
			: ArrayListWrapper 
		{
			#region Constructors

			public FixedSizeArrayListWrapper(ArrayList innerList)
				: base(innerList) 
			{
			}

			#endregion

			#region Properties
		
			/// <summary>
			/// Gets the error message to display when an readonly/fixedsize related exception is
			/// thrown.
			/// </summary>
			protected virtual string ErrorMessage 
			{
				get 
				{
					return "Can't add or remove from a fixed-size list.";
				}
			}

			public override int Capacity 
			{
				get 
				{
					return base.Capacity;
				}

				set 
				{
					throw new NotSupportedException(this.ErrorMessage);
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					return true;
				}
			}

			#endregion

			#region Methods

			public override int Add(object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void AddRange(ICollection c) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Clear() 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Insert(int index, object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void InsertRange(int index, ICollection c) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Remove(object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void RemoveAt(int index) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void RemoveRange(int index, int count) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}
				
			public override void TrimToSize() 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			#endregion
		}

		#endregion		

		#region ReadOnlyArrayListWrapper

		[Serializable]
		private sealed class ReadOnlyArrayListWrapper
			: FixedSizeArrayListWrapper 
		{
			protected override string ErrorMessage 
			{
				get 
				{
					return "Can't modify a readonly list.";
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					return true;
				}
			}

			public ReadOnlyArrayListWrapper(ArrayList innerArrayList)
				: base(innerArrayList) 
			{
			}

			public override object this[int index] 
			{
				get 
				{
					return m_InnerArrayList[index];
				}

				set 
				{
					throw new NotSupportedException(this.ErrorMessage);
				}
			}

			public override void Reverse() 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Reverse(int index, int count) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void SetRange(int index, ICollection c) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Sort() 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Sort(IComparer comparer) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Sort(int index, int count, IComparer comparer) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}
		}

		#endregion

		#region RangedArrayList

		[Serializable]
		private sealed class RangedArrayList
			: ArrayListWrapper 
		{
			private int m_InnerIndex;
			private int m_InnerCount;
			private int m_InnerStateChanges;

			public RangedArrayList(ArrayList innerList, int index, int count)
				: base(innerList) 
			{
				m_InnerIndex = index;
				m_InnerCount = count;
				m_InnerStateChanges = innerList._version;
			}

			#region Indexers

			public override bool IsSynchronized
			{
				get
				{
					return false;
				}
			}

			public override object this[int index] 
			{
				get 
				{
					if (index < 0 || index > m_InnerCount) 
					{
						throw new ArgumentOutOfRangeException("index");
					}

					return m_InnerArrayList[m_InnerIndex + index];
				}

				set 
				{
					if (index < 0 || index > m_InnerCount) 
					{
						throw new ArgumentOutOfRangeException("index");
					}

					m_InnerArrayList[m_InnerIndex + index] = value;
				}
			}

			#endregion

			#region Properties

			public override int Count 
			{
				get 
				{
					VerifyStateChanges();

					return m_InnerCount;
				}
			}

			public override int Capacity 
			{
				get 
				{
					return m_InnerArrayList.Capacity;
				}

				set 
				{
					if (value < m_InnerCount) 
					{
						throw new ArgumentOutOfRangeException();
					}
				}
			}

			#endregion

			#region Methods

			private void VerifyStateChanges() 
			{
				if (m_InnerStateChanges != m_InnerArrayList._version) 
				{
					throw new InvalidOperationException
						("ArrayList view is invalid because the underlying ArrayList was modified.");
				}
			}

			public override int Add(object value) 
			{
				VerifyStateChanges();

				m_InnerArrayList.Insert(m_InnerIndex + m_InnerCount, value);

				m_InnerStateChanges = m_InnerArrayList._version;

				return ++m_InnerCount;
			}

			public override void Clear() 
			{
				VerifyStateChanges();

				m_InnerArrayList.RemoveRange(m_InnerIndex, m_InnerCount);
				m_InnerCount = 0;

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override bool Contains(object value) 
			{
				return m_InnerArrayList.Contains(value, m_InnerIndex, m_InnerCount);
			}

			public override int IndexOf(object value) 
			{
				return IndexOf(value, 0);
			}

			public override int IndexOf(object value, int startIndex) 
			{
				return IndexOf(value, startIndex, m_InnerCount - startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count) 
			{
				if (startIndex < 0 || startIndex > m_InnerCount) 
				{
					ThrowNewArgumentOutOfRangeException ("startIndex", startIndex,
						"Does not specify valid index.");
				}

				if (count < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("count", count,
						"Can't be less than 0.");
				}

				// re-ordered to avoid possible integer overflow
				if (startIndex > m_InnerCount - count) 
				{
					// LAMESPEC: Every other method throws ArgumentException

					throw new ArgumentOutOfRangeException("count",
						"Start index and count do not specify a valid range.");
				}

				int retval = m_InnerArrayList.IndexOf(value, m_InnerIndex + startIndex, count);

				if (retval == -1) 
				{
					return -1;
				}
				else 
				{
					return retval - m_InnerIndex;
				}
			}

			public override int LastIndexOf(object value) 
			{
				return LastIndexOf(value, m_InnerCount - 1);
			}

			public override int LastIndexOf(object value, int startIndex) 
			{
				return LastIndexOf(value, startIndex, startIndex + 1);
			}

			public override int LastIndexOf(object value, int startIndex, int count) 
			{
				if (startIndex < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("startIndex", startIndex,	"< 0");
				}

				if (count < 0) 
				{
					ThrowNewArgumentOutOfRangeException ("count", count, "count is negative.");
				}

				int retval = m_InnerArrayList.LastIndexOf(value, m_InnerIndex + startIndex, count);

				if (retval == -1) 
				{
					return -1;
				}
				else 
				{
					return retval - m_InnerIndex;
				}
			}

			public override void Insert(int index, object value) 
			{
				VerifyStateChanges();
				
				if (index < 0 || index > m_InnerCount) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index must be >= 0 and <= Count.");
				}

				m_InnerArrayList.Insert(m_InnerIndex + index, value);

				m_InnerCount++;

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void InsertRange(int index, ICollection c) 
			{
				VerifyStateChanges();

				if (index < 0 || index > m_InnerCount) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index must be >= 0 and <= Count.");
				}

				m_InnerArrayList.InsertRange(m_InnerIndex + index, c);

				m_InnerCount += c.Count;

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void Remove(object value) 
			{
				VerifyStateChanges();

				int x = IndexOf(value);

				if (x > -1) 
				{
					RemoveAt(x);
				}

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void RemoveAt(int index) 
			{
				VerifyStateChanges();

				if (index < 0 || index > m_InnerCount) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index must be >= 0 and <= Count.");
				}

				m_InnerArrayList.RemoveAt(m_InnerIndex + index);

				m_InnerCount--;
				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void RemoveRange(int index, int count) 
			{
				VerifyStateChanges();

				CheckRange(index, count, m_InnerCount);				

				m_InnerArrayList.RemoveRange(m_InnerIndex + index, count);

				m_InnerCount -= count;

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void Reverse() 
			{
				Reverse(0, m_InnerCount);
			}

			public override void Reverse(int index, int count) 
			{
				VerifyStateChanges();

				CheckRange(index, count, m_InnerCount);				

				m_InnerArrayList.Reverse(m_InnerIndex + index, count);

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void SetRange(int index, ICollection c) 
			{
				VerifyStateChanges();

				if (index < 0 || index > m_InnerCount) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index must be >= 0 and <= Count.");
				}

				m_InnerArrayList.SetRange(m_InnerIndex + index, c);

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override void CopyTo(System.Array array) 
			{
				CopyTo(array, 0);
			}

			public override void CopyTo(System.Array array, int index) 
			{
				CopyTo(0, array, index, m_InnerCount);
			}

			public override void CopyTo(int index, System.Array array, int arrayIndex, int count) 
			{
				CheckRange(index, count, m_InnerCount);				

				m_InnerArrayList.CopyTo(m_InnerIndex + index, array, arrayIndex, count);
			}

			public override IEnumerator GetEnumerator() 
			{
				return GetEnumerator(0, m_InnerCount);
			}

			public override IEnumerator GetEnumerator(int index, int count) 
			{
				CheckRange(index, count, m_InnerCount);

				return m_InnerArrayList.GetEnumerator(m_InnerIndex + index, count);
			}

			public override void AddRange(ICollection c) 
			{
				VerifyStateChanges();

				m_InnerArrayList.InsertRange(m_InnerCount, c);

				m_InnerCount += c.Count;

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override int BinarySearch(object value) 
			{
				return BinarySearch(0, m_InnerCount, value, Comparer.Default);
			}

			public override int BinarySearch(object value, IComparer comparer) 
			{
				return BinarySearch(0, m_InnerCount, value, comparer);
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer) 
			{
				CheckRange(index, count, m_InnerCount);

				return m_InnerArrayList.BinarySearch(m_InnerIndex + index, count, value, comparer);
			}

			public override object Clone() 
			{
				return new RangedArrayList((ArrayList)m_InnerArrayList.Clone(), m_InnerIndex, m_InnerCount);
			}

			public override ArrayList GetRange(int index, int count) 
			{
				CheckRange(index, count, m_InnerCount);

				return new RangedArrayList(this, index, count);
			}

			public override void TrimToSize() 
			{
				throw new NotSupportedException();
			}

			public override void Sort() 
			{
				Sort(Comparer.Default);
			}

			public override void Sort(IComparer comparer) 
			{
				Sort(0, m_InnerCount, comparer);
			}

			public override void Sort(int index, int count, IComparer comparer) 
			{
				VerifyStateChanges();

				CheckRange(index, count, m_InnerCount);

				m_InnerArrayList.Sort(m_InnerIndex + index, count, comparer);

				m_InnerStateChanges = m_InnerArrayList._version;
			}

			public override object[] ToArray() 
			{
				object[] array;

				array = new object[m_InnerCount];

				m_InnerArrayList.CopyTo (m_InnerIndex, array, 0, m_InnerCount);

				return array;
			}

			public override Array ToArray(Type elementType) 
			{
				Array array;

				array = Array.CreateInstance(elementType, m_InnerCount);

				m_InnerArrayList.CopyTo(m_InnerIndex, array, 0, m_InnerCount);

				return array;
			}

			#endregion
		}

		#endregion

		//
		// List wrappers
		//

		#region SynchronizedListWrapper

		[Serializable]
		private sealed class SynchronizedListWrapper
			: ListWrapper 
		{
			private object m_SyncRoot;

			public SynchronizedListWrapper(IList innerList)
				: base(innerList) 
			{
				m_SyncRoot = innerList.SyncRoot;
			}

			public override int Count 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerList.Count;
					}
				}
			}

			public override bool IsSynchronized 
			{
				get 
				{
					return true;
				}
			}

			public override object SyncRoot 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerList.SyncRoot;
					}
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerList.IsFixedSize;
					}
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerList.IsReadOnly;
					}
				}
			}

			public override object this[int index] 
			{
				get 
				{
					lock (m_SyncRoot) 
					{
						return m_InnerList[index];
					}
				}

				set 
				{
					lock (m_SyncRoot) 
					{
						m_InnerList[index] = value;
					}
				}
			}

			public override int Add(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerList.Add(value);
				}
			}

			public override void Clear() 
			{
				lock (m_SyncRoot) 
				{
					m_InnerList.Clear();
				}
			}

			public override bool Contains(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerList.Contains(value);
				}
			}

			public override int IndexOf(object value) 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerList.IndexOf(value);
				}
			}

			public override void Insert(int index, object value) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerList.Insert(index, value);
				}
			}

			public override void Remove(object value) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerList.Remove(value);
				}
			}

			public override void RemoveAt(int index) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerList.RemoveAt(index);
				}
			}

			public override void CopyTo(Array array, int index) 
			{
				lock (m_SyncRoot) 
				{
					m_InnerList.CopyTo(array, index);
				}
			}

			public override IEnumerator GetEnumerator() 
			{
				lock (m_SyncRoot) 
				{
					return m_InnerList.GetEnumerator();
				}
			}
		}

		#endregion

		#region FixedSizeListWrapper

		[Serializable]
			private class FixedSizeListWrapper
			: ListWrapper 
		{
			protected virtual string ErrorMessage 
			{		
				get 
				{
					return "List is fixed-size.";
				}
			}

			public override bool IsFixedSize 
			{
				get 
				{
					return true;
				}
			}

			public FixedSizeListWrapper(IList innerList)
				: base(innerList) 
			{
			}

			public override int Add(object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Clear() 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Insert(int index, object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void Remove(object value) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}

			public override void RemoveAt(int index) 
			{
				throw new NotSupportedException(this.ErrorMessage);
			}
		}

		#endregion

		#region ReadOnlyListWrapper

		[Serializable]
		private sealed class ReadOnlyListWrapper
			: FixedSizeListWrapper 
		{
			protected override string ErrorMessage 
			{		
				get 
				{
					return "List is read-only.";
				}
			}

			public override bool IsReadOnly 
			{
				get 
				{
					return true;
				}
			}

			public ReadOnlyListWrapper(IList innerList)
				: base(innerList) 
			{
			}

			public override object this[int index] 
			{
				get 
				{
					return m_InnerList[index];
				}

				set 
				{
					throw new NotSupportedException(this.ErrorMessage);
				}
			}
		}

		#endregion

		#region ListWrapper

		/// <summary>
		/// Decorates/Wraps any <c>IList</c> implementing object.
		/// </summary>
		[Serializable]
		private class ListWrapper
			: IList 
		{
			#region Fields

			protected IList m_InnerList;

			#endregion

			#region Constructors

			public ListWrapper(IList innerList) 
			{			
				m_InnerList = innerList;
			}

			#endregion

			#region Indexers

			public virtual object this[int index] 
			{
				get 
				{
					return m_InnerList[index];
				}

				set 
				{
					m_InnerList[index] = value;
				}
			}

			#endregion

			#region Properties

			public virtual int Count 
			{
				get 
				{
					return m_InnerList.Count;
				}
			}

			public virtual bool IsSynchronized 
			{
				get 
				{
					return m_InnerList.IsSynchronized;
				}
			}

			public virtual object SyncRoot 
			{
				get 
				{
					return m_InnerList.SyncRoot;
				}
			}

			public virtual bool IsFixedSize 
			{
				get 
				{
					return m_InnerList.IsFixedSize;
				}
			}

			public virtual bool IsReadOnly 
			{
				get 
				{
					return m_InnerList.IsReadOnly;
				}
			}

			#endregion

			#region Methods

			public virtual int Add(object value) 
			{
				return m_InnerList.Add(value);
			}

			public virtual void Clear() 
			{
				m_InnerList.Clear();
			}

			public virtual bool Contains(object value) 
			{
				return m_InnerList.Contains(value);
			}

			public virtual int IndexOf(object value) 
			{
				return m_InnerList.IndexOf(value);
			}

			public virtual void Insert(int index, object value) 
			{
				m_InnerList.Insert(index, value);
			}

			public virtual void Remove(object value) 
			{
				m_InnerList.Remove(value);
			}

			public virtual void RemoveAt(int index) 
			{
				m_InnerList.RemoveAt(index);
			}

			public virtual void CopyTo(Array array, int index) 
			{
				m_InnerList.CopyTo(array, index);
			}

			public virtual IEnumerator GetEnumerator() 
			{
				return m_InnerList.GetEnumerator();
			}

			#endregion
		}

		#endregion

		//
		// Start of ArrayList
		//

		#region Fields

		private const int DefaultInitialCapacity = 4;

		/// <summary>
		/// Array to store the items.
		/// </summary>
		private object[] _items;
				
		/// <summary>
		/// Number of items in the list.
		/// </summary>
		private int _size;

		/// <summary>
		/// Total number of state changes.
		/// </summary>
		private int _version;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ArrayList"/> class that is empty and
		/// has the default initial capacity (16).
		/// </summary>
		public ArrayList()
		{
			_items = EmptyArray<object>.Value;
		}		

		/// <summary>
		/// Initializes a new instance of the <see cref="ArrayList"/> class that contains 
		/// elements copied from the specified collection and that has the same initial capacity
		/// as the number of elements copied.
		/// </summary>
		/// <param name="c">
		/// The <see cref="ICollection"/> whose elements are copied into the new list.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// The argument <c>c</c> is a null reference.
		/// </exception>
		public ArrayList(ICollection c) 
		{
			Array array;

			if (c == null) 
			{
				throw new ArgumentNullException("c");
			}
						
			array = c as Array;

			if (array != null && array.Rank != 1) 
			{
				throw new RankException();
			}

			_items = new object[c.Count];

			AddRange(c);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArrayList"/> class that is empty and
		/// has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		/// The number of elements that hte new list is initially capable of storing.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The <c>capacity</c> is less than zero.
		/// </exception>
		public ArrayList(int capacity) 
		{
			if (capacity < 0) 
			{
				ThrowNewArgumentOutOfRangeException ("capacity",
					capacity, "The initial capacity can't be smaller than zero.");
			}

			if (capacity == 0) 
			{
				capacity = DefaultInitialCapacity;
			}
			_items = new object [capacity];
		}

		/// <summary>
		/// Used by ArrayListAdapter to allow creation of an ArrayList with no storage buffer.
		/// </summary>
		private ArrayList(int initialCapacity, bool forceZeroSize) 
		{
			if (forceZeroSize) 
			{				
				_items = null;
			}
			else 
			{
				throw new InvalidOperationException("Use ArrayList(int)");
			}
		}

		/// <summary>
		/// Initializes a new array list that contains a copy of the given array and with the
		/// given count.
		/// </summary>
		/// <param name="array"></param>
		private ArrayList(object[] array, int index, int count) 
		{
			if (count == 0) 
			{
				_items = new object[DefaultInitialCapacity];
			}
			else 
			{
				_items = new object[count];
			}

			Array.Copy(array, index, _items, 0, count);

			_size = count;
		}

		#endregion

		#region Indexers

		/// <summary>
		/// Gets/Sets an element in the list by index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The index is less than 0 or more then or equal to the list count.
		/// </exception>
		public virtual object this[int index] 
		{
			get 
			{
				if (index < 0 || index >= _size) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index is less than 0 or more than or equal to the list count.");
				}

				return _items[index];
			}

			set 
			{
				if (index < 0 || index >= _size) 
				{
					ThrowNewArgumentOutOfRangeException ("index", index,
						"Index is less than 0 or more than or equal to the list count.");
				}

				_items[index] = value;
				_version++;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of elements in the list.
		/// </summary>
		public virtual int Count 
		{
			get 
			{				
				return _size;
			}
		}

		/// <summary>
		/// Gets the number of elements the list can carry without needing to expand.
		/// </summary>
		/// <remarks>
		/// ArrayLists automatically double their capacity when the capacity limit is broken.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The capacity is less than the count.
		/// </exception>
		public virtual int Capacity 
		{
			get 
			{
				return _items.Length;
			}

			set 
			{
				if (value < _size) 
				{
					ThrowNewArgumentOutOfRangeException ("Capacity", value,
						"Must be more than count.");
				}

				object[] newArray;

				newArray = new object[value];

				Array.Copy(_items, 0, newArray, 0, _size);

				_items = newArray;
			}
		}

		/// <summary>
		/// <see cref="IList.IsFixedSize"/>
		/// </summary>
		/// <remarks/>
		public virtual bool IsFixedSize 
		{
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// <see cref="IList.IsReadOnly"/>
		/// </summary>
		public virtual bool IsReadOnly 
		{
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// <see cref="ICollection.IsSynchronized"/>
		/// </summary>
		public virtual bool IsSynchronized 
		{
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// <see cref="ICollection.SyncRoot"/>
		/// </summary>
		public virtual object SyncRoot 
		{
			get 
			{
				return this;
			}
		}

		#endregion

		#region Methods

		/// <remarks>
		/// Ensures that the list has the capacity to contain the given <c>count</c> by
		/// automatically expanding the capacity when required.
		/// </remarks>
		private void EnsureCapacity(int count) 
		{
			if (count <= _items.Length) 
			{
				return;
			}

			int newLength;
			object[] newData;

			newLength = _items.Length << 1;
			if (newLength == 0)
				newLength = DefaultInitialCapacity;

			while (newLength < count) 
			{
				newLength <<= 1;
			}

			newData = new object[newLength];

			Array.Copy(_items, 0, newData, 0, _items.Length);

			_items = newData;
		}
		
		/// <summary>
		/// Shifts a section of the list.
		/// </summary>
		/// <param name="index">
		/// The start of the section to shift (the element at index is included in the shift).
		/// </param>
		/// <param name="count">
		/// The number of positions to shift by (can be negative).
		/// </param>
		private void Shift(int index, int count) 
		{
			if (count > 0) 
			{
				if (_size + count > _items.Length) 
				{
					int newLength;
					object[] newData;
					
					newLength = (_items.Length > 0) ? _items.Length << 1 : 1;

					while (newLength < _size + count) 
					{
						newLength <<= 1;
					}
					
					newData = new object[newLength];

					Array.Copy(_items, 0, newData, 0, index);
					Array.Copy(_items, index, newData, index + count, _size - index);

					_items = newData;
				}
				else 
				{
					Array.Copy(_items, index, _items, index + count, _size - index);
				}
			}
			else if (count < 0) 
			{
				// Remember count is negative so this is actually index + (-count)

				int x = index - count ;

				Array.Copy(_items, x, _items, index, _size - x);
				Array.Clear(_items, _size + count, - count);
			}
		}

		public virtual int Add(object value) 
		{
			// Do a check here in case EnsureCapacity isn't inlined.

			if (_items.Length <= _size /* same as _items.Length < _size + 1) */) 
			{
				EnsureCapacity(_size + 1);
			}

			_items[_size] = value;
			
			_version++;

			return _size++;
		}

		public virtual void Clear() 
		{
			// Keep the array but null all members so they can be garbage collected.

			Array.Clear(_items, 0, _size);

			_size = 0;
			_version++;
		}

		public virtual bool Contains(object item) 
		{
			return IndexOf(item, 0, _size) > -1;
		}

		internal virtual bool Contains(object value, int startIndex, int count) 
		{
			return IndexOf(value, startIndex, count) > -1;
		}

		public virtual int IndexOf(object value) 
		{			
			return IndexOf(value, 0);
		}

		public virtual int IndexOf(object value, int startIndex) 
		{
			return IndexOf(value, startIndex, _size - startIndex);
		}

		public virtual int IndexOf(object value, int startIndex, int count) 
		{
			if (startIndex < 0 || startIndex > _size) 
			{
				ThrowNewArgumentOutOfRangeException ("startIndex", startIndex,
					"Does not specify valid index.");
			}

			if (count < 0) 
			{
				ThrowNewArgumentOutOfRangeException ("count", count,
					"Can't be less than 0.");
			}

			// re-ordered to avoid integer overflow
			if (startIndex > _size - count) 
			{
				// LAMESPEC: Every other method throws ArgumentException

				throw new ArgumentOutOfRangeException("count",
					"Start index and count do not specify a valid range.");
			}

			return Array.IndexOf(_items, value, startIndex, count);
		}

		public virtual int LastIndexOf(object value) 
		{
			return LastIndexOf(value, _size - 1);
		}

		public virtual int LastIndexOf(object value, int startIndex) 
		{
			return LastIndexOf(value, startIndex, startIndex + 1);
		}

		public virtual int LastIndexOf (object value, int startIndex, int count) 
		{
			// Array will catch the exceptions
			return Array.LastIndexOf (_items, value, startIndex, count);
		}

		public virtual void Insert(int index, object value) 
		{
			if (index < 0 || index > _size) 
			{
				ThrowNewArgumentOutOfRangeException ("index", index,
					"Index must be >= 0 and <= Count.");
			}

			Shift(index, 1);

			_items[index] = value;
			_size++;
			_version++;
		}

		public virtual void InsertRange(int index, ICollection c) 
		{
			int i;

			if (c == null) 
			{
				throw new ArgumentNullException("c");
			}

			if (index < 0 || index > _size) 
			{
				ThrowNewArgumentOutOfRangeException ("index", index,
					"Index must be >= 0 and <= Count.");
			}

			i = c.Count;
			
			// Do a check here in case EnsureCapacity isn't inlined.

			if (_items.Length < _size + i) 
			{
				EnsureCapacity(_size + i);
			}

			if (index < _size) 
			{
				Array.Copy(_items, index, _items, index + i, _size - index);
			}
		
			// Handle inserting a range from a list to itself specially.

			if (this == c.SyncRoot) 
			{
				// Copy range before the insert point.

				Array.Copy(_items, 0, _items, index, index);

				// Copy range after the insert point.

				Array.Copy(_items, index + i, _items, index << 1, _size - index);
			}
			else 
			{
				c.CopyTo(_items, index);
			}
		
			_size += c.Count;
			_version++;
		}

		public virtual void Remove(object obj) 
		{
			int x;

			x = IndexOf(obj);

			if (x > -1) 
			{
				RemoveAt(x);
			}

			_version++;
		}

		public virtual void RemoveAt(int index) 
		{
			if (index < 0 || index >= _size) 
			{
				ThrowNewArgumentOutOfRangeException ("index", index,
					"Less than 0 or more than list count.");
			}

			Shift(index, -1);
			_size--;
			_version++;
		}

		public virtual void RemoveRange(int index, int count) 
		{
			ArrayList.CheckRange(index, count, _size);
						
			Shift(index, -count);
			_size -= count;
			_version++;			
		}

		public virtual void Reverse() 
		{
			Array.Reverse(_items, 0, _size);
			_version++;
		}

		public virtual void Reverse(int index, int count) 
		{
			ArrayList.CheckRange(index, count, _size);

			Array.Reverse(_items, index, count);
			_version++;
		}

		public virtual void CopyTo(System.Array array) 
		{
			Array.Copy(_items, array, _size);
		}

		public virtual void CopyTo(System.Array array, int arrayIndex) 
		{			
			CopyTo(0, array, arrayIndex, _size);
		}

		public virtual void CopyTo(int index, System.Array array, int arrayIndex, int count) 
		{
			if (array == null) 
			{
				throw new ArgumentNullException("array");
			}

			if (array.Rank != 1) 
			{
				// LAMESPEC:
				// This should be a RankException because Array.Copy throws RankException.

				throw new ArgumentException("Must have only 1 dimensions.", "array");
			}

			Array.Copy(_items, index, array, arrayIndex, count);
		}

		public virtual IEnumerator GetEnumerator() 
		{
			return new SimpleEnumerator(this);
		}

		public virtual IEnumerator GetEnumerator(int index, int count) 
		{
			ArrayList.CheckRange(index, count, _size);

			return new ArrayListEnumerator(this, index, count);
		}

		public virtual void AddRange(ICollection c) 
		{
			InsertRange(_size, c);
		}

		public virtual int BinarySearch(object value) 
		{
			try 
			{
				return Array.BinarySearch(_items, 0, _size, value);
			}
			catch (InvalidOperationException e) 
			{
				throw new ArgumentException(e.Message);
			}
		}

		public virtual int BinarySearch(object value, IComparer comparer) 
		{
			try 
			{
				return Array.BinarySearch(_items, 0, _size, value, comparer);
			}
			catch (InvalidOperationException e) 
			{
				throw new ArgumentException(e.Message);
			}
		}

		public virtual int BinarySearch(int index, int count, object value, IComparer comparer) 
		{
			try 
			{
				return Array.BinarySearch(_items, index, count, value, comparer);
			}
			catch (InvalidOperationException e) 
			{
				throw new ArgumentException(e.Message);
			}
		}
		public virtual ArrayList GetRange(int index, int count) 
		{
			ArrayList.CheckRange(index, count, _size);

			if (this.IsSynchronized)
			{
				return ArrayList.Synchronized(new RangedArrayList(this, index, count));
			}
			else
			{
				return new RangedArrayList(this, index, count);
			}
		}

		public virtual void SetRange(int index, ICollection c) 
		{
			if (c == null) 
			{
				throw new ArgumentNullException("c");
			}

			if (index < 0 || index + c.Count > _size) 
			{
				throw new ArgumentOutOfRangeException("index");
			}

			c.CopyTo(_items, index);

			_version++;
		}

		public virtual void TrimToSize() 
		{
			if (_items.Length > _size) 
			{
				object[] newArray;

				if (_size == 0) 
				{
					newArray = new object[DefaultInitialCapacity];
				}
				else 
				{
					newArray = new object[_size];
				}
								
				Array.Copy(_items, 0, newArray, 0, _size);

				_items = newArray;
			}
		}

		public virtual void Sort() 
		{
			Array.Sort(_items, 0, _size);

			_version++;
		}

		public virtual void Sort(IComparer comparer) 
		{
			Array.Sort(_items, 0, _size, comparer);
		}

		public virtual void Sort(int index, int count, IComparer comparer) 
		{
			ArrayList.CheckRange(index, count, _size);

			Array.Sort(_items, index, count, comparer);
		}

		public virtual object[] ToArray() 
		{
			object[] retval;

			retval = new object[_size];

			CopyTo(retval);
			
			return retval;
		}

		public virtual Array ToArray(Type type) 
		{
			Array retval;
			
			retval = Array.CreateInstance(type, _size);

			CopyTo(retval);

			return retval;
		}

		public virtual object Clone() 
		{
			return new ArrayList(this._items, 0, this._size);
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Does a check of the arguments many of the methods in ArrayList use.
		/// </summary>
		/// <remarks>
		/// The choice of exceptions thrown sometimes seem to be arbitrarily chosen so
		/// not all methods actually make use of CheckRange.
		/// </remarks>
		internal static void CheckRange(int index, int count, int listCount) 
		{
			if (index < 0) 
			{
				ThrowNewArgumentOutOfRangeException ("index", index, "Can't be less than 0.");
			}

			if (count < 0) 
			{
				ThrowNewArgumentOutOfRangeException ("count", count, "Can't be less than 0.");
			}

			// re-ordered to avoid possible integer overflow
			if (index > listCount - count) 
			{
				throw new ArgumentException("Index and count do not denote a valid range of elements.", "index");
			}
		}

		internal static void ThrowNewArgumentOutOfRangeException (string name, object actual, string message)
		{
			throw new ArgumentOutOfRangeException (
#if !INSIDE_CORLIB && NET_2_1
				name, message
#else
				name, actual, message
#endif
			);
		}

		public static ArrayList Adapter(IList list) 
		{
			// LAMESPEC: EWWW.  Other lists aren't *Array*Lists.

			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			ArrayList arrayList = list as ArrayList;
			if (arrayList != null)
				return arrayList;
			else
				arrayList = new ArrayListAdapter(list);

			if (list.IsSynchronized)
				return ArrayList.Synchronized(arrayList);
			else 
				return arrayList;
		}

		public static ArrayList Synchronized(ArrayList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsSynchronized)
			{
				return list;
			}

			return new SynchronizedArrayListWrapper(list);
		}

		public static IList Synchronized(IList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsSynchronized)
			{
				return list;
			}

			return new SynchronizedListWrapper(list);
		}

		public static ArrayList ReadOnly(ArrayList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsReadOnly)
			{
				return list;
			}

			return new ReadOnlyArrayListWrapper(list);
		}

		public static IList ReadOnly(IList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsReadOnly)
			{
				return list;
			}

			return new ReadOnlyListWrapper(list);
		}

		public static ArrayList FixedSize(ArrayList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsFixedSize)
			{
				return list;
			}

			return new FixedSizeArrayListWrapper(list);
		}

		public static IList FixedSize(IList list) 
		{
			if (list == null) 
			{
				throw new ArgumentNullException("list");
			}

			if (list.IsFixedSize)
			{
				return list;
			}

			return new FixedSizeListWrapper(list);
		}

		public static ArrayList Repeat(object value, int count) 
		{
			ArrayList arrayList = new ArrayList(count);

			for (int i = 0; i < count; i++) 
			{
				arrayList.Add(value);
			}

			return arrayList;
		}

		#endregion
	}
}
