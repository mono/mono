
//
// System.Data.Odbc.OdbcConnection
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public sealed class OdbcConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		string connectionString;
		int connectionTimeout;
		OdbcDataReader dataReader;
		int henv=0, hdbc=0;
		private string _uid, _pwd, _dsn;

		#endregion

		#region Constructors

		public OdbcConnection ()
		{
			OdbcReturn ret;

			// allocate Environment handle
			ret=libodbc.SQLAllocHandle((ushort) OdbcHandleType.Env, 0, ref henv);
			libodbc.DisplayError("SQLAllocHandle", ret);

			ret=libodbc.SQLSetEnvAttr(henv, (ushort) OdbcEnv.OdbcVersion, (IntPtr) 3 
, 0);
			libodbc.DisplayError("SQLSetEnvAttr", ret);

			Console.WriteLine("ODBCInit Complete.");
			connectionTimeout = 15;
			connectionString = null;
			dataReader = null;
		}

		public OdbcConnection (string connectionString) : this ()
		{
			ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		public int hDbc
		{
			get { return hdbc; }
		}

		public string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;

				string[] items=connectionString.Split(new char[1]{';'});
				foreach (string item in items)
				{
					string[] parts=item.Split(new char[1] {'='});
					switch (parts[0].Trim().ToLower())
					{
						case "dsn":
							_dsn=parts[1].Trim();
							break;
						case "uid":
							_uid=parts[1].Trim();
							break;
						case "pwd":
							_pwd=parts[1].Trim();
							break;
					}
				}
			}
		}

		public int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
		}

		public string DataSource {
			get {
				if (State==ConnectionState.Open)
					return _dsn;
				else
					return null;
			}
		}

		public string Database {
			get {
				return "";
			}
		}

		public ConnectionState State
		{
			get {
				if (hdbc!=0) {
					return ConnectionState.Open;
				}
				else
					return ConnectionState.Closed;
			}
		}

		internal OdbcDataReader DataReader
	        {
			get {
				return dataReader;
			}
			set {
				dataReader = value;
			}
		}

		#endregion // Properties

		#region Methods

		public void BeginTransaction()
		{
			OdbcReturn ret;
			// Set Auto-commit to false
			ret=libodbc.SQLSetConnectAttr(hdbc, 102, 0, 0);
			libodbc.DisplayError("SQLSetConnectAttr(NoAutoCommit)", ret);
		}

		public void CommitTransaction()
		{
			OdbcReturn ret;
			ret=libodbc.SQLEndTran((short) OdbcHandleType.Dbc, hdbc, 0);
			libodbc.DisplayError("SQLEndTran(commit)", ret);
		}

		public void RollbackTransaction()
		{
			OdbcReturn ret;
			ret=libodbc.SQLEndTran((short) OdbcHandleType.Dbc, hdbc, 1);
			libodbc.DisplayError("SQLEndTran(rollback)", ret);
		}

//		public OdbcTransaction BeginTransaction ()
//		{
//              }

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			throw new NotImplementedException ();
	//		return BeginTransaction ();
		}

//		public OdbcTransaction BeginTransaction (IsolationLevel level)
//		{
//
//		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			throw new NotImplementedException ();
		//	return BeginTransaction (level);
		}

		public void Close ()
		{
			if (State == ConnectionState.Open) {
				hdbc = 0;
			}

			dataReader = null;
		}

		public OdbcCommand CreateCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ChangeDatabase(string Database)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			throw new NotImplementedException();
	//		return CreateCommand ();
		}

		public void Open ()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			OdbcReturn ret;

			// allocate connection handle
			ret=libodbc.SQLAllocHandle((ushort) OdbcHandleType.Dbc, henv, ref hdbc);
			libodbc.DisplayError("SQLAllocHandle(hdbc)", ret);

			// Connect to data source
			ret=libodbc.SQLConnect(hdbc, _dsn, -3, _uid, -3, _pwd, -3);
			libodbc.DisplayError("SQLConnect",ret);

		}

		[MonoTODO]
		public static void ReleaseObjectPool ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Events and Delegates

		public event StateChangeEventHandler StateChange;

		#endregion
	}
}

