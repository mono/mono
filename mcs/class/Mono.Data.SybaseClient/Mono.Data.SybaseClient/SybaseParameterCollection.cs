//
// Mono.Data.SybaseClient.SybaseParameterCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		private ArrayList parameterList = new ArrayList();

		public IEnumerator GetEnumerator()
		{
			return parameterList.GetEnumerator ();
		}

		
		public int Add (object value)
		{
			// Call the add version that receives a SybaseParameter 
			
			// Check if value is a SybaseParameter.
			CheckType(value);
			Add((SybaseParameter) value);

			return IndexOf (value);
		}

		
		public SybaseParameter Add (SybaseParameter value)
		{
			parameterList.Add (value);
			return value;
		}

		
		public SybaseParameter Add (string parameterName, object value)
		{
			SybaseParameter sqlparam = new SybaseParameter();
			sqlparam.Value = value;
			// TODO: Get the dbtype and SybaseType from system type of value.
			
			return Add(sqlparam);
		}
		
		public SybaseParameter Add(string parameterName, SybaseType sqlDbType)
		{
			SybaseParameter sqlparam = new SybaseParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SybaseType = sqlDbType;
			return Add(sqlparam);			
		}

		public SybaseParameter Add(string parameterName,
			SybaseType sqlDbType, int size)
		{
			SybaseParameter sqlparam = new SybaseParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SybaseType = sqlDbType;
			sqlparam.Size = size;
			return Add(sqlparam);			
		}

		
		public SybaseParameter Add(string parameterName,
			SybaseType sqlDbType, int size, string sourceColumn)
		{
			SybaseParameter sqlparam = new SybaseParameter();
			sqlparam.ParameterName = parameterName;
			sqlparam.SybaseType = sqlDbType;
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
			// Check if value is a SybaseParameter
			CheckType(value);
			return Contains(((SybaseParameter)value).ParameterName);
		}


		[MonoTODO]
		public bool Contains(string value)
		{
			for(int p = 0; p < parameterList.Count; p++) {
				if(((SybaseParameter)parameterList[p]).ParameterName.Equals(value))
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
			// Check if value is a SybaseParameter
			CheckType(value);
			return IndexOf(((SybaseParameter)value).ParameterName);
		}

		
		public int IndexOf(string parameterName)
		{
			int p = -1;

			for(p = 0; p < parameterList.Count; p++) {
				if(((SybaseParameter)parameterList[p]).ParameterName.Equals(parameterName))
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
				return (SybaseParameter) this[index];
			}
			
			[MonoTODO]
			set { 
				this[index] = (SybaseParameter) value;
			}
		}

		public SybaseParameter this[int index] {
			get {	
				return (SybaseParameter) parameterList[index];
			}			  
			
			set {	
				parameterList[index] = (SybaseParameter) value;
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
				this[parameterName] = (SybaseParameter) value;
			}
		}

		public SybaseParameter this[string parameterName] {
			get {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((SybaseParameter)parameterList[p]).ParameterName))
						return (SybaseParameter) parameterList[p];
				}
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
			}	  
			
			set {	
				for(int p = 0; p < parameterList.Count; p++) {
					if(parameterName.Equals(((SybaseParameter)parameterList[p]).ParameterName))
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
		/// SybaseParameter type. If it doesn't, throws an InvalidCastException.
		/// </summary>
		private void CheckType(object value)
		{
			if(!(value is SybaseParameter))
				throw new InvalidCastException("Only SQLParameter objects can be used.");
		}
		
	}
}
