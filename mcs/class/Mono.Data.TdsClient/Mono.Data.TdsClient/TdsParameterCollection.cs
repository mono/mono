//
// Mono.Data.TdsClient.TdsParameterCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace Mono.Data.TdsClient {
	public sealed class TdsParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		object IList.this [int index] {
			get { return (TdsParameter) this[index]; }
			set { this[index] = (TdsParameter) value; }
		}

		public TdsParameter this [int index] {
			get { 
				if (index >= Count)
					throw new IndexOutOfRangeException ();
				return (TdsParameter) list[index]; 
			}
			set { 
				if (index >= Count)
					throw new IndexOutOfRangeException ();
				list[index] = (TdsParameter) value; 
			}
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return (TdsParameter) this[parameterName]; }
			set { this [parameterName] = (TdsParameter) value; }
		}

		public TdsParameter this [string parameterName] {
			get { return this[IndexOf (parameterName)]; }
			set { this[IndexOf (parameterName)] = value; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods 

		public int Add (object value)
		{
			if (!(value is TdsParameter))
				throw new InvalidCastException ();
			Add ((TdsParameter) value);
			return IndexOf (value);
		}

		public TdsParameter Add (TdsParameter value)
		{
			list.Add (value);
			return value; 
		}

		public TdsParameter Add (string parameterName, object value)
		{
			return Add (new TdsParameter (parameterName, value));
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public bool Contains (string value)
		{
			return (IndexOf (value) >= 0);
		}

		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public int IndexOf (string parameterName)
		{
			for (int i = 0; i < list.Count; i += 1) 
				if (((TdsParameter) list[i]).ParameterName == parameterName)
					return i;
			return -1;
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

		public void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}


		#endregion // Methods
	}
}

