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
			OdbcReturn ret=libodbc.SQLSetConnectAttr(conn.hDbc, OdbcConnectionAttribute.AutoCommit, IntPtr.Zero, 0); 
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLSetConnectAttr",OdbcHandleType.Dbc,conn.hDbc));
			// Handle isolation level
			int lev=0;
			switch (isolationlevel)
			{
				case IsolationLevel.ReadUncommitted:
					lev=1;
					break;				
				case IsolationLevel.ReadCommitted:
					lev=2;
					break;
				case IsolationLevel.RepeatableRead:
					lev=3;
					break;
				case IsolationLevel.Serializable:
					lev=4;
					break;
				case IsolationLevel.Unspecified:
					lev=0;
					break;
				default:
					throw new NotSupportedException();
			}
			libodbc.SQLSetConnectAttr(conn.hDbc, OdbcConnectionAttribute.TransactionIsolation, (IntPtr) lev, 0);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLSetConnectAttr",OdbcHandleType.Dbc,conn.hDbc));
			this.isolationlevel=isolationlevel;
			connection=conn;
		}

		public void Commit()
		{
			if (connection.transaction==this)
			{
				OdbcReturn ret=libodbc.SQLEndTran((short) OdbcHandleType.Dbc, connection.hDbc, 0);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLEndTran",OdbcHandleType.Dbc,connection.hDbc));
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
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLEndTran",OdbcHandleType.Dbc,connection.hDbc));
				connection.transaction=null;
			}
			else
				throw new InvalidOperationException();
		}
	}
}
