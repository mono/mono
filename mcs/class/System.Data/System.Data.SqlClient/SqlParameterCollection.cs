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

using Mono.Data.Tds;
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
		TdsMetaParameterCollection metaParameters;
		SqlCommand command;

		#endregion // Fields

		#region Constructors

		internal SqlParameterCollection (SqlCommand command)
		{
			this.command = command;
			metaParameters = new TdsMetaParameterCollection ();
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
				if (!(value is SqlParameter))
					throw new InvalidCastException ("Only SQLParameter objects can be used.");
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

		internal TdsMetaParameterCollection MetaParameters {
			get { return metaParameters; }
		}
		
		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			Add ((SqlParameter) value);
			return IndexOf (value);
		}
		
		public SqlParameter Add (SqlParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The SqlParameter specified in the value parameter is already added to this or another SqlParameterCollection.");
			
			value.Container = this;
			list.Add (value);
			metaParameters.Add (value.MetaParameter);
			return value;
		}
		
		public SqlParameter Add (string parameterName, object value)
		{
			return Add (new SqlParameter (parameterName, value));
		}
		
		public SqlParameter Add (string parameterName, SqlDbType sqlDbType)
		{
			return Add (new SqlParameter (parameterName, sqlDbType));
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size)
		{
			return Add (new SqlParameter (parameterName, sqlDbType, size));
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
		{
			return Add (new SqlParameter (parameterName, sqlDbType, size, sourceColumn));
		}

		public void Clear()
		{
			metaParameters.Clear ();
			list.Clear ();
		}
		
		public bool Contains (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
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
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
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
			metaParameters.Remove (((SqlParameter) value).MetaParameter);
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			metaParameters.RemoveAt (index);
			list.RemoveAt (index);
		}

		public void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

		#endregion // Methods	
	}
}
