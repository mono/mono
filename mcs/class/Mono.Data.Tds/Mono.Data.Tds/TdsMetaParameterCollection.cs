//
// Mono.Data.Tds.TdsMetaParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace Mono.Data.Tds {
	public class TdsMetaParameterCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;

		#endregion // Fields

		public TdsMetaParameterCollection ()
		{
			list = new ArrayList ();
		}

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}
		
		public TdsMetaParameter this [int index] {
			get { return (TdsMetaParameter) list [index]; }
		}

		public TdsMetaParameter this [string name] {
			get { return (TdsMetaParameter) this [IndexOf (name)]; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsMetaParameter value)
		{
			return list.Add (value); 
		}

		public void Clear ()
		{
			list.Clear ();	
		}

		public bool Contains (TdsMetaParameter value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (TdsMetaParameter value)
		{
			return list.IndexOf (value);
		}

		public int IndexOf (string name)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (name))
					return i;
			return -1;
		}

		public void Insert (int index, TdsMetaParameter value)
		{
			list.Insert (index, value);
		}

		public void Remove (TdsMetaParameter value)
		{
			list.Remove (value);
		}

		public void Remove (string name)
		{
			RemoveAt (IndexOf (name));
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}
