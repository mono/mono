//
// Mono.Data.PostgreSqlClient.PgSqlError.cs
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

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[MonoTODO]
	public sealed class PgSqlErrorCollection : ICollection, IEnumerable
	{
		ArrayList errorList = new ArrayList();

		internal PgSqlErrorCollection() {
		}

		internal PgSqlErrorCollection(byte theClass, int lineNumber,
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
		public PgSqlError this[int index] {
			get {
				return (PgSqlError) errorList[index];
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

		internal void Add(PgSqlError error) {
			errorList.Add(error);
		}

		internal void Add(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			PgSqlError error = new PgSqlError(theClass,
				lineNumber, message,
				number, procedure,
				server, source, state);
			Add(error);
		}

		#region Destructors

		[MonoTODO]
		~PgSqlErrorCollection()
		{
			// FIXME: do the destructor - release resources
		}

		#endregion		
	}
}
