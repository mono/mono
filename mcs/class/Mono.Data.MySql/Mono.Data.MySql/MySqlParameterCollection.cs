//
// Mono.Data.MySql.MySqlParameterCollection.cs
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

namespace Mono.Data.MySql
{
	/// <summary>
	/// Collects all parameters relevant to a Command object 
	/// and their mappings to DataSet columns.
	/// </summary>
	public sealed class MySqlParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
	{
		private ArrayList parameterList = new ArrayList();

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			return new MySqlParameterEnumerator (parameterList);
		}
		
		public int Add(	object value)
		{
			// Call the add version that receives a SqlParameter 
			
			// Check if value is a MySqlParameter.
			CheckType(value);
			Add((MySqlParameter) value);

			return IndexOf (value);
		}

		
		public MySqlParameter Add(MySqlParameter value)
		{
			parameterList.Add(value);
			return value;
		}

		
		public MySqlParameter Add(string parameterName, object value)
		{
			MySqlParameter sqlparam = new MySqlParameter();
			sqlparam.Value = value;
			// TODO: Get the DbType from system type of value.
			
			return Add(sqlparam);
		}

		
		public MySqlParameter Add(string parameterName, DbType dbType)
		{
			MySqlParameter sqlparam = new MySqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.DbType = dbType;
			return Add(sqlparam);			
		}

		
		public MySqlParameter Add(string parameterName,
			DbType dbType, int size)
		{
			MySqlParameter sqlparam = new MySqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.DbType = dbType;
			sqlparam.Size = size;
			return Add(sqlparam);			
		}

		
		public MySqlParameter Add(string parameterName,
			DbType dbType, int size, string sourceColumn)
		{
			MySqlParameter sqlparam = new MySqlParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.DbType = dbType;
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
			return Contains(((MySqlParameter)value).ParameterName);
		}


		[MonoTODO]
		public bool Contains(string value)
		{
			for(int p = 0; p < parameterList.Count; p++) {
				if(((MySqlParameter)parameterList[p]).ParameterName.Equals(value))
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
			return IndexOf(((MySqlParameter)value).ParameterName);
		}

		
		public int IndexOf(string parameterName)
		{
			int p = -1;

			for(p = 0; p < parameterList.Count; p++) {
				if(((MySqlParameter)parameterList[p]).ParameterName.Equals(parameterName))
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
				return (MySqlParameter) this[index];
			}
			
			[MonoTODO]
			set { 
				this[index] = (MySqlParameter) value;
			}
		}

		public MySqlParameter this[int index] {
			get {	
				return (MySqlParameter) parameterList[index];
			}			  
			
			set {	
				parameterList[index] = (MySqlParameter) value;
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
				this[parameterName] = (MySqlParameter) value;
			}
		}

		public MySqlParameter this[string parameterName] {
			get {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((MySqlParameter)parameterList[p]).ParameterName))
						return (MySqlParameter) parameterList[p];
				}
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}	  
			
			set {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((MySqlParameter)parameterList[p]).ParameterName))
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
		/// MySqlParameter type. If it doesn't, throws an InvalidCastException.
		/// </summary>
		private void CheckType(object value)
		{
			if(!(value is MySqlParameter))
				throw new InvalidCastException("Only MySqlParameter objects can be used.");
		}

		private class MySqlParameterEnumerator : IEnumerator {
			public MySqlParameterEnumerator (IList list) {
				this.list = list;
				Reset ();
			}

			public object Current {
				get {
					if (ptr >= list.Count)
						throw new InvalidOperationException ();

					return list[ptr];
				}
			}

			public bool MoveNext () {
				if (ptr > list.Count)
					throw new InvalidOperationException ();
				
				return ++ ptr < list.Count;
			}

			public void Reset () {
				ptr = -1;
			}

			private IList list;
			private int ptr;
		}
	}
}
