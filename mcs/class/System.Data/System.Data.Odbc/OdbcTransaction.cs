//
// System.Data.Odbc.OdbcTransaction
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//
using System;
using System.Data;

namespace System.Data.Odbc
{
	/// <summary>
	/// Summary description for OdbcTransaction.
	/// </summary>
	public class OdbcTransaction : MarshalByRefObject
	{
		private OdbcConnection connection;
		private IsolationLevel isolationlevel;

		internal OdbcTransaction(OdbcConnection conn, IsolationLevel isolationlevel)
		{
			// Set Auto-commit (102) to false
			OdbcReturn ret=libodbc.SQLSetConnectAttr(conn.hDbc, 102, 0, 0); 
			libodbchelper.DisplayError("SQLSetConnectAttr(NoAutoCommit)", ret);
			// TODO: Handle isolation level
			this.isolationlevel=isolationlevel;
			connection=conn;
		}

		public void Commit()
		{
			if (connection.transaction==this)
			{
				OdbcReturn ret=libodbc.SQLEndTran((short) OdbcHandleType.Dbc, connection.hDbc, 0);
				libodbchelper.DisplayError("SQLEndTran(commit)", ret);
				connection.transaction=null;
			}
			else
				throw new InvalidOperationException();
		}

		public void Rollback()
		{
			if (connection.transaction==this)
			{
				OdbcReturn ret=libodbc.SQLEndTran((short) OdbcHandleType.Dbc, connection.hDbc, 1);
				libodbchelper.DisplayError("SQLEndTran(rollback)", ret);
				connection.transaction=null;
			}
			else
				throw new InvalidOperationException();
		}
	}
}
