//
// System.Data.SqlClient.SqlParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace System.Data.SqlClient {
	[ListBindable (false)]
	public sealed class SqlParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();
		SqlCommand command;

		#endregion // Fields

		#region Constructors

		internal SqlParameterCollection (SqlCommand command)
		{
			this.command = command;
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public int Count {
			get { return list.Count; }			  
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public SqlParameter this [int index] {
			get { return (SqlParameter) list [index]; }			  
			set { list [index] = (SqlParameter) value; }			  
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return this[parameterName]; }
			set { 
				CheckType (value);
				this [parameterName] = (SqlParameter) value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public SqlParameter this [string parameterName] {
			get {
				foreach (SqlParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p;
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}	  
			set {	
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = value;
			}			  
		}

		object IList.this [int index] {
			get { return (SqlParameter) this [index]; }
			set { this [index] = (SqlParameter) value; }
		}

		bool IList.IsFixedSize {
			get { return list.IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return list.IsReadOnly; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}
		
		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			// Check if value is a SqlParameter.
			CheckType (value);
			Add ((SqlParameter) value);
			return IndexOf (value);
		}
		
		public SqlParameter Add (SqlParameter value)
		{
			list.Add (value);
			return value;
		}
		
		public SqlParameter Add (string parameterName, object value)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.Value = value;
			// TODO: Get the dbtype and Sqldbtype from system type of value.
			
			return Add(sqlparam);
		}
		
		public SqlParameter Add (string parameterName, SqlDbType sqlDbType)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			return Add(sqlparam);			
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			sqlparam.Size = size;
			return Add(sqlparam);			
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
		{
			SqlParameter sqlparam = new SqlParameter ();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			sqlparam.Size = size;
			sqlparam.SourceColumn = sourceColumn;
			return Add (sqlparam);
		}

		public void Clear()
		{
			list.Clear ();
		}
		
		public bool Contains (object value)
		{
			CheckType (value);
			return Contains (((SqlParameter) value).ParameterName);
		}

		public bool Contains (string value)
		{
			foreach (SqlParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public int IndexOf (object value)
		{
			CheckType (value);
			return IndexOf (((SqlParameter) value).ParameterName);
		}
		
		public int IndexOf (string parameterName)
		{
			return list.IndexOf (parameterName);
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

		/// <summary>
		/// This method checks if the parameter value is of 
		/// SqlParameter type. If it doesn't, throws an InvalidCastException.
		/// </summary>
		private void CheckType(object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("Only SQLParameter objects can be used.");
		}

		#endregion // Methods	
	}
}
