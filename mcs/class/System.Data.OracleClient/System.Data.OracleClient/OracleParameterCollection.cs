// 
// OracleParameterCollection.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.OCI;

namespace System.Data.OracleClient {
	public class OracleParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		OracleCommand command;
		ArrayList list;

		#endregion // Fields

		#region Constructors

		internal OracleParameterCollection (OracleCommand command)
			: this ()
		{
			this.command = command;
		}

		public OracleParameterCollection ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

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
			get { return false; }
		}

		public OracleParameter this [string parameterName] {
			get {
				foreach (OracleParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p;
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}
			set {
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = value;
			}
		}

		object IList.this [int index] {
			get { return (OracleParameter) this [index]; }
			set { this [index] = (OracleParameter) value; }
		}

		bool IList.IsFixedSize {
			get { return IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return IsReadOnly; }
		}

		bool ICollection.IsSynchronized {
			get { return IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return SyncRoot; }
		}

		public OracleParameter this [int index] {
			get { return (OracleParameter) list [index]; }
			set { list [index] = value; }
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return this [parameterName]; }
			set {
				if (!(value is OracleParameter))
					throw new InvalidCastException ("The parameter was not an OracleParameter.");
				this [parameterName] = (OracleParameter) value;
			}
		}

		public object SyncRoot {
			get { return this; }
		}

		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is OracleParameter))
				throw new InvalidCastException ("The parameter was not an OracleParameter.");
                        Add ((OracleParameter) value);
                        return IndexOf (value);
		}

		public OracleParameter Add (OracleParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The OracleParameter specified in the value parameter is already added to this or another OracleParameterCollection.");
			value.Container = this;
			list.Add (value);
                        return value;
		}

		public OracleParameter Add (string parameterName, object value)
		{
			return Add (new OracleParameter (parameterName, value));
		}

		public OracleParameter Add (string parameterName, OracleType dataType)
		{
			return Add (new OracleParameter (parameterName, dataType));
		}

		public OracleParameter Add (string parameterName, OracleType dataType, int size)
		{
			return Add (new OracleParameter (parameterName, dataType, size));
		}

		public OracleParameter Add (string parameterName, OracleType dataType, int size, string srcColumn)
		{
			return Add (new OracleParameter (parameterName, dataType, size, srcColumn));
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			if (!(value is OracleParameter))
				throw new InvalidCastException ("The parameter was not an OracleParameter.");
			return Contains (((OracleParameter) value).ParameterName);
		}

		public bool Contains (string parameterName)
		{
			foreach (OracleParameter p in list)
				if (p.ParameterName.Equals (parameterName))
					return true;
			return false;
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object value)
		{
			if (!(value is OracleParameter))
				throw new InvalidCastException ("The parameter was not an OracleParameter.");
			return IndexOf (((OracleParameter) value).ParameterName);
		}

		public int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (parameterName))
					return i;
			return -1;
		}

		public void Insert (int index, object value)
		{
			if (!(value is OracleParameter))
				throw new InvalidCastException ("The parameter was not an OracleParameter.");
			list.Insert (index, value);
		}

		public void Remove (object value)
		{
			if (!(value is OracleParameter))
				throw new InvalidCastException ("The parameter was not an OracleParameter.");
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public void RemoveAt (string parameterName)
		{
			list.RemoveAt (IndexOf (parameterName));
		}

		#endregion // Methods
	}
}
