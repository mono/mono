//
// System.Data.Odbc.OdbcTransaction
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Data;

#if NET_2_0
using System.Data.Common;
#endif // NET_2_0


namespace System.Data.Odbc
{

#if NET_2_0
	public sealed class OdbcTransaction : DbTransaction
#else
	public sealed class OdbcTransaction : MarshalByRefObject, IDbTransaction
#endif // NET_2_0
	{
		private bool disposed = false;
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

		#region Implementation of IDisposable

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					Rollback();
				}
				disposed = true;
			}
		}

#if ONLY_1_1
		void IDisposable.Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
#else
                public override void Dispose ()
                {
                        Dispose (true);
                        GC.SuppressFinalize (this);
                }
#endif // ONLY_1_1


		#endregion Implementation of IDisposable

		#region Implementation of IDbTransaction

		public 
#if NET_2_0
                override
#endif //NET_2_0
                void Commit()
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

		public 
#if NET_2_0
                override
#endif //NET_2_0
                void Rollback()
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

#if ONLY_1_1
		IDbConnection IDbTransaction.Connection
		{
			get
			{
				return Connection;
			}
		}

#endif // ONLY_1_1
#if NET_2_0
                protected override DbConnection DbConnection 
                { 
                        get {return Connection;}
                }
#endif // NET_2_0

		public 
#if NET_2_0
                override
#endif //NET_2_0
                IsolationLevel IsolationLevel
		{
			get
			{
				return isolationlevel;
			}
		}

		#endregion Implementation of IDbTransaction

		#region Public Instance Properties

#if ONLY_1_1
		public OdbcConnection Connection
		{
			get
			{
				return connection;
			}
		}

#endif // ONLY_1_1
		#endregion Public Instance Properties
	}
}
