//
// Mono.Data.Tds.Protocol.TdsDataRow.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsDataRow : IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;
		int bigDecimalIndex;

		#endregion // Fields

		#region Constructors

		public TdsDataRow ()
		{
			list = new ArrayList ();
			bigDecimalIndex = -1;
		}

		#endregion // Constructors

		#region Properties

		public int BigDecimalIndex {
			get { return bigDecimalIndex; }
			set { bigDecimalIndex = value; }
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
	
		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public object this[int index] {
			get { 
				if (index > list.Count)
					throw new IndexOutOfRangeException ();
				return list[index]; 
			}
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods 

		public int Add (object value)
		{
			return list.Add (value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public void CopyTo (int index, Array array, int arrayIndex, int count)
		{
			list.CopyTo (index, array, arrayIndex, count);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Remove (object value)
		{
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}
