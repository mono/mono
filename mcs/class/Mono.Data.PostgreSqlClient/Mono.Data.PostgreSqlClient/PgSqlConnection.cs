//
// System.Data.SqlClient.SqlConnection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents an open connection to a SQL data source
	/// </summary>
	public class SqlConnection : IDBConnection
	{
		#region methods
		
		public SqlTransaction BeginTransaction()
		{
			return new SqlTransaction (this);
		}

		public SqlTransaction BeginTransaction(IsolationLevel il)
		{
			SqlTransaction xaction = new SqlTransaction (cnc);
			xaction.IsolationLevel = il;

			return xaction;
		}

		[MonoTODO]
		public void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException ();
		}
				
		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand CreateCommand()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Open()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region properties

		[MonoTODO]
		public string ConnectionString
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int ConnectionTimeout
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Database
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ConnectionState State
		{
			get { throw new NotImplementedException (); }
		}

		#endregion
	}
}
