//
// System.Data.SqlClient.SqlErrorCollection.cs
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient {
	[ListBindable (false)]
	[Serializable]
	public sealed class SqlErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();

		#endregion // Fields

		#region Constructors

		internal SqlErrorCollection () 
		{
		}

		internal SqlErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties
                
		public int Count {
			get { return list.Count; }			  
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		public SqlError this[int index] {
			get { return (SqlError) list [index]; }
		}

		#endregion

		#region Methods
		
		internal void Add(SqlError error) 
		{
			list.Add (error);
		}

		internal void Add(byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			SqlError error = new SqlError (theClass, lineNumber, message, number, procedure, server, source, state);
			Add (error);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		#endregion		
	}
}
