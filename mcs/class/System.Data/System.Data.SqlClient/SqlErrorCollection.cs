//
// System.Data.SqlClient.SqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//
using System;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[MonoTODO]
	public sealed class SqlErrorCollection : ICollection, IEnumerable
	{
		ArrayList errorList = new ArrayList();

		internal SqlErrorCollection() {
		}

		internal SqlErrorCollection(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			Add (theClass, lineNumber, message,
				number, procedure,
				server, source, state);
		}

		#region Properties
                
		[MonoTODO]
		public int Count {
			get {	
				return errorList.Count;
			}			  
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index) {
			throw new NotImplementedException ();
		}

		// [MonoTODO]
		bool ICollection.IsSynchronized {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		// [MonoTODO]
		object ICollection.SyncRoot {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			throw new NotImplementedException ();
		}
		
		// Index property (indexer)
		// [MonoTODO]
		public SqlError this[int index] {
			get {
				return (SqlError) errorList[index];
			}
		}

		#endregion

		#region Methods
		
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		#endregion

		internal void Add(SqlError error) {
			errorList.Add(error);
		}

		internal void Add(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			SqlError error = new SqlError(theClass,
				lineNumber, message,
				number, procedure,
				server, source, state);
			Add(error);
		}

		#region Destructors

		[MonoTODO]
		~SqlErrorCollection()
		{
			// FIXME: do the destructor - release resources
		}

		#endregion		
	}
}
