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
		public OdbcTransaction transaction;
		IntPtr henv=IntPtr.Zero, hdbc=IntPtr.Zero;
		
		#endregion

		#region Constructors
		
		public OdbcConnection ()
		{
			OdbcReturn ret;
		
			// allocate Environment handle	
			ret=libodbc.SQLAllocHandle(OdbcHandleType.Env, IntPtr.Zero, ref henv);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLAllocHandle"));
		
			ret=libodbc.SQLSetEnvAttr(henv, OdbcEnv.OdbcVersion, (IntPtr) 3 , 0); 
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLSetEnvAttr",OdbcHandleType.Env,henv));
		
			//Console.WriteLine("ODBCInit Complete.");
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

		internal IntPtr hDbc
		{
			get { return hdbc; }
		}
		
		public string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}

		public int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
		}

//		public string DataSource {
//			get {
//				if (State==ConnectionState.Open)
//					return _dsn;
//				else
//					return null;
//			}
//		}

		public string Database {
			get {
				return "";
			}
		}

		public ConnectionState State
		{
			get {
				if (hdbc!=IntPtr.Zero) {
					return ConnectionState.Open;
				}
				else
					return ConnectionState.Closed;
			}
		}

		public OdbcDataReader DataReader
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
	
		public OdbcTransaction BeginTransaction ()
		{
			return BeginTransaction(IsolationLevel.Unspecified);
        }
              
		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return (IDbTransaction) BeginTransaction();
		}
		
		public OdbcTransaction BeginTransaction (IsolationLevel level)
		{
			if (transaction==null)
			{
				transaction=new OdbcTransaction(this,level);
				return transaction;
			}
			else
				throw new InvalidOperationException();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			return (IDbTransaction) BeginTransaction(level);
		}

		public void Close ()
		{
			if (State == ConnectionState.Open) {
				// TODO: Free handles
				dataReader = null;
				hdbc = IntPtr.Zero;
				transaction=null;
			}
			else
				throw new InvalidOperationException();
		}

		public OdbcCommand CreateCommand ()
		{
			return new OdbcCommand("", this, transaction); 
		}

		[MonoTODO]
		public void ChangeDatabase(string Database)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return (IDbCommand) CreateCommand ();
		}

		public void Open ()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();
						
			// allocate connection handle
			OdbcReturn ret=libodbc.SQLAllocHandle(OdbcHandleType.Dbc, henv, ref hdbc);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLAllocHandle",OdbcHandleType.Env,henv));
			
			// DSN connection
			if (connectionString.ToLower().IndexOf("dsn=")>=0)
			{
				string _uid="", _pwd="", _dsn="";
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
				ret=libodbc.SQLConnect(hdbc, _dsn, -3, _uid, -3, _pwd, -3);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLConnect",OdbcHandleType.Dbc,hdbc));
			}
			else 
			{
				// DSN-less Connection
				string OutConnectionString=new String(' ',1024);
				short OutLen=0;
				ret=libodbc.SQLDriverConnect(hdbc, IntPtr.Zero, connectionString, -3, 
					OutConnectionString, (short) OutConnectionString.Length, ref OutLen, 0);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLDriverConnect",OdbcHandleType.Dbc,hdbc));
			}

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
