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
	/// <summary>
	/// Collects all parameters relevant to a Command object 
	/// and their mappings to DataSet columns.
	/// </summary>
	public sealed class SqlParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		private ArrayList parameterList = new ArrayList();

		public IEnumerator GetEnumerator()
		{
			return parameterList.GetEnumerator ();
		}

		
		public int Add(	object value)
		{
			// Call the add version that receives a SqlParameter 
			
			// Check if value is a SqlParameter.
			CheckType(value);
			Add((SqlParameter) value);

			return IndexOf (value);
		}

		
		public SqlParameter Add(SqlParameter value)
		{
			parameterList.Add(value);
			return value;
		}

		
		public SqlParameter Add(string parameterName, object value)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.Value = value;
			// TODO: Get the dbtype and Sqldbtype from system type of value.
			
			return Add(sqlparam);
		}
		
		public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			return Add(sqlparam);			
		}

		public SqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			sqlparam.Size = size;
			return Add(sqlparam);			
		}

		
		public SqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size, string sourceColumn)
		{
			SqlParameter sqlparam = new SqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			sqlparam.Size = size;
			sqlparam.SourceColumn = sourceColumn;
			return Add(sqlparam);			
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		
		public bool Contains(object value)
		{
			// Check if value is a SqlParameter
			CheckType(value);
			return Contains(((SqlParameter)value).ParameterName);
		}


		[MonoTODO]
		public bool Contains(string value)
		{
			for(int p = 0; p < parameterList.Count; p++) {
				if(((SqlParameter)parameterList[p]).ParameterName.Equals(value))
					return true;
			}
			return false;
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index)
		{
			throw new NotImplementedException ();
		}

		
		public int IndexOf (object value)
		{
			// Check if value is a SqlParameter
			CheckType(value);
			return IndexOf(((SqlParameter)value).ParameterName);
		}

		
		public int IndexOf (string parameterName)
		{
			return parameterList.IndexOf (parameterName);
		}

		public void Insert (int index, object value)
		{
			parameterList.Insert (index, value);
		}

		public void Remove (object value)
		{
			parameterList.Remove (value);
		}

		public void RemoveAt (int index)
		{
			parameterList.RemoveAt (index);
		}

		[MonoTODO]
		public void RemoveAt (string parameterName)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public int Count {
			get { return parameterList.Count; }			  
		}

		object IList.this[int index] {
			[MonoTODO]
			get { 
				return (SqlParameter) this[index];
			}
			
			[MonoTODO]
			set { 
				this[index] = (SqlParameter) value;
			}
		}

		public SqlParameter this[int index] {
			get {	
				return (SqlParameter) parameterList[index];
			}			  
			
			set {	
				parameterList[index] = (SqlParameter) value;
			}			  
		}

		object IDataParameterCollection.this[string parameterName] {
			[MonoTODO]
			get { 
				return this[parameterName];
			}
			
			[MonoTODO]
			set { 
				CheckType(value);
				this[parameterName] = (SqlParameter) value;
			}
		}

		public SqlParameter this[string parameterName] {
			get {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((SqlParameter)parameterList[p]).ParameterName))
						return (SqlParameter) parameterList[p];
				}
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}	  
			
			set {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((SqlParameter)parameterList[p]).ParameterName))
						parameterList[p] = value;
				}
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}			  
		}

		bool IList.IsFixedSize {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		bool IList.IsReadOnly {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		bool ICollection.IsSynchronized {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		object ICollection.SyncRoot {
			get {	
				throw new NotImplementedException ();
			}			  
		}
		
		/// <summary>
		/// This method checks if the parameter value is of 
		/// SqlParameter type. If it doesn't, throws an InvalidCastException.
		/// </summary>
		private void CheckType(object value)
		{
			if(!(value is SqlParameter))
				throw new InvalidCastException("Only SQLParameter objects can be used.");
		}
		
	}
}
