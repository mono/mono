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
		#region Properties

		[MonoTODO]
		public byte Class {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
			   throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Message {
			get { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Procedure {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Source {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int Count {
			get {	
				throw new NotImplementedException ();
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

		#endregion

		#region Methods
		
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region Destructors

		// FIXME: do the destructor
/*
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlError()
		{

		}
*/

		#endregion
		
	}
}
