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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object this[string parameterName]
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

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
			throw new NotImplementedException ();
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

/*
		[MonoTODO]
		public bool Contains(string value)
		{
			throw new NotImplementedException ();
		}
		
*/

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
/*
		[MonoTODO]
		public int IndexOf(string parameterName)
		{
			throw new NotImplementedException ();
		}
*/
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
/*
		[MonoTODO]
		public void RemoveAt(string parameterName)
		{
			throw new NotImplementedException ();
		}
*/		
/*
 		[MonoTODO]
		[Serializable]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlParameterCollection();
*/

		[MonoTODO]
		public int Count {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		object IList.this[int index] {
			get {	
				throw new NotImplementedException ();
			}			  
			
			set {	
				throw new NotImplementedException ();
			}			  
		}

		[MonoTODO]
		public SqlParameter this[int index] {
			get {	
				throw new NotImplementedException ();
			}			  
			
			set {	
				throw new NotImplementedException ();
			}			  
		}
/*
		[MonoTODO]
		public SqlParameter this[string parameterName] {
			get {	
				throw new NotImplementedException ();
			}			  
			
			set {	
				throw new NotImplementedException ();
			}			  
		}
*/

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
