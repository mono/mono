//
// Mono.Data.PostgreSqlClient.PgSqlParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Collects all parameters relevant to a Command object 
	/// and their mappings to DataSet columns.
	/// </summary>
	// public sealed class PgSqlParameterCollection : MarshalByRefObject,
	// IDataParameterCollection, IList, ICollection, IEnumerable
	public sealed class PgSqlParameterCollection : IDataParameterCollection,
		IList
	{
		private ArrayList parameterList = new ArrayList();

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		
		public int Add(	object value)
		{
			// Call the add version that receives a SqlParameter 
			
			// Check if value is a PgSqlParameter.
			CheckType(value);
			Add((PgSqlParameter) value);

			return IndexOf (value);
		}

		
		public PgSqlParameter Add(PgSqlParameter value)
		{
			parameterList.Add(value);
			return value;
		}

		
		public PgSqlParameter Add(string parameterName, object value)
		{
			PgSqlParameter sqlparam = new PgSqlParameter();
			sqlparam.Value = value;
			// TODO: Get the dbtype and Sqldbtype from system type of value.
			
			return Add(sqlparam);
		}

		
		public PgSqlParameter Add(string parameterName, SqlDbType sqlDbType)
		{
			PgSqlParameter sqlparam = new PgSqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			return Add(sqlparam);			
		}

		
		public PgSqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size)
		{
			PgSqlParameter sqlparam = new PgSqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SqlDbType = sqlDbType;
			sqlparam.Size = size;
			return Add(sqlparam);			
		}

		
		public PgSqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size, string sourceColumn)
		{
			PgSqlParameter sqlparam = new PgSqlParameter();
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
			return Contains(((PgSqlParameter)value).ParameterName);
		}


		[MonoTODO]
		public bool Contains(string value)
		{
			for(int p = 0; p < parameterList.Count; p++) {
				if(((PgSqlParameter)parameterList[p]).ParameterName.Equals(value))
					return true;
			}
			return false;
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index)
		{
			throw new NotImplementedException ();
		}

		
		public int IndexOf(object value)
		{
			// Check if value is a SqlParameter
			CheckType(value);
			return IndexOf(((PgSqlParameter)value).ParameterName);
		}

		
		public int IndexOf(string parameterName)
		{
			int p = -1;

			for(p = 0; p < parameterList.Count; p++) {
				if(((PgSqlParameter)parameterList[p]).ParameterName.Equals(parameterName))
					return p;
			}
			return p;
		}

		[MonoTODO]
		public void Insert(int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(string parameterName)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public int Count {
			get {	
				return parameterList.Count;
			}			  
		}

		object IList.this[int index] {
			[MonoTODO]
			get { 
				return (PgSqlParameter) this[index];
			}
			
			[MonoTODO]
			set { 
				this[index] = (PgSqlParameter) value;
			}
		}

		public PgSqlParameter this[int index] {
			get {	
				return (PgSqlParameter) parameterList[index];
			}			  
			
			set {	
				parameterList[index] = (PgSqlParameter) value;
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
				this[parameterName] = (PgSqlParameter) value;
			}
		}

		public PgSqlParameter this[string parameterName] {
			get {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((PgSqlParameter)parameterList[p]).ParameterName))
						return (PgSqlParameter) parameterList[p];
				}
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}	  
			
			set {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((PgSqlParameter)parameterList[p]).ParameterName))
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
		/// PgSqlParameter type. If it doesn't, throws an InvalidCastException.
		/// </summary>
		private void CheckType(object value)
		{
			if(!(value is PgSqlParameter))
				throw new InvalidCastException("Only PgSqlParameter objects can be used.");
		}
		
	}
}
