//
// System.Data.SqlClient.SqlErrorCollection
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

namespace System.Data.SqlClient
{
	/// <summary>
	/// Provides a means of reading one or more forward-only streams of result sets obtained by executing a command at a SQL database.
	/// </summary>
	public sealed class SqlErrorCollection : ICollection, IEnumerable
	{
		#region Properties

		[MonoTODO]
		public int Count 
		{
			get 
			{ 
				throw new NotImplementedException ();
			}
		}

		bool ICollection.IsSynchronized {
			get 
			{ 
				throw new NotImplementedException ();
			}
		}

		object ICollection.SyncRoot {
			get 
			{ 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public SqlError this[int index]
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region Methods

		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Deconstructors

		// FIXME: do the deconstructor
/*
		[MonoTODO]
		~SqlErrorCollection()
		{
			throw new NotImplementedException ();
		}
*/
		#endregion
	}
}
