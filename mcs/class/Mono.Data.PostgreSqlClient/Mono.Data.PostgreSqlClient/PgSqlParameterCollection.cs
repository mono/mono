//
// System.Data.SqlClient.SqlParameterCollection.cs
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

namespace System.Data.SqlClient
{
	/// <summary>
	/// Collects all parameters relevant to a Command object 
	/// and their mappings to DataSet columns.
	/// </summary>
	// public sealed class SqlParameterCollection : MarshalByRefObject,
	// IDataParameterCollection, IList, ICollection, IEnumerable
	public sealed class SqlParameterCollection : IDataParameterCollection
	{
		private ArrayList parameterList = new ArrayList();
		private Hashtable parameterNames = new Hashtable();

/*
		[MonoTODO]
		public void RemoveAt(string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(string parameterName)
	        {
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public bool Contains(string parameterName)
		{
			return parameterNames.ContainsKey(parameterName);
		}
*/

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add(	object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlParameter Add(SqlParameter value)
		{
			parameterList.Add(value);
			parameterNames.Add(value.ParameterName, parameterList.Add(value));
			return value;
		}

		[MonoTODO]
		public SqlParameter Add(string parameterName, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlParameter Add(string parameterName,
			SqlDbType sqlDbType, int size, string sourceColumn)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(object value)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public bool Contains(string value)
		{
			return parameterNames.ContainsKey(value);
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(string parameterName)
		{
			throw new NotImplementedException ();
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
				return (SqlParameter) this[parameterName];
			}
			
			[MonoTODO]
			set { 
				this[parameterName] = (SqlParameter) value;
			}
		}

		public SqlParameter this[string parameterName] {
			get {	
				if(parameterNames.ContainsKey(parameterName))
					return (SqlParameter) parameterList[(int)parameterNames[parameterName]];
				else
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}			  
			
			set {	
				if(parameterNames.ContainsKey(parameterName))
					parameterList[(int)parameterNames[parameterName]] = (SqlParameter) value;
				else
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
	}
}
