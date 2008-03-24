//
// System.Data.Odbc.OdbcTransaction
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
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
using System.Data.Common;
using System.Globalization;

namespace System.Data.Odbc
{
#if NET_2_0
	public sealed class OdbcTransaction : DbTransaction, IDisposable
#else
	public sealed class OdbcTransaction : MarshalByRefObject, IDbTransaction
#endif
	{
		private bool disposed;
		private OdbcConnection connection;
		private IsolationLevel isolationlevel;
		private bool isOpen;

		internal OdbcTransaction (OdbcConnection conn, IsolationLevel isolationlevel)
		{
			// Set Auto-commit (102) to false
			SetAutoCommit (conn, false);
			// Handle isolation level
			OdbcIsolationLevel lev = OdbcIsolationLevel.ReadCommitted;
			OdbcConnectionAttribute attr = OdbcConnectionAttribute.TransactionIsolation;
			switch (isolationlevel) {
			case IsolationLevel.ReadUncommitted:
				lev = OdbcIsolationLevel.ReadUncommitted;
				break;
			case IsolationLevel.ReadCommitted:
				lev = OdbcIsolationLevel.ReadCommitted;
				break;
			case IsolationLevel.RepeatableRead:
				lev = OdbcIsolationLevel.RepeatableRead;
				break;
			case IsolationLevel.Serializable:
				lev = OdbcIsolationLevel.Serializable;
				break;
#if NET_2_0
			case IsolationLevel.Snapshot:
				// badly broken on MS:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=305736
				lev = OdbcIsolationLevel.Snapshot;

				// SQL_ATTR_TXN_ISOLATION can be used to set all other isolation
				// levels except for SQL_TXN_SS_SNAPSHOT. If you want to use snapshot
				// isolation, you must set SQL_TXN_SS_SNAPSHOT through
				// SQL_COPT_SS_TXN_ISOLATION. However, you can retrieve the
				// isolation level by using either SQL_ATTR_TXN_ISOLATION or
				// SQL_COPT_SS_TXN_ISOLATION.
				// Source:
				// http://msdn2.microsoft.com/en-us/library/ms131709.aspx
				attr = OdbcConnectionAttribute.CoptTransactionIsolation;
				break;
#endif
			case IsolationLevel.Unspecified:
				// when isolationlevel is not specified, then use
				// default isolation level of the driver and
				// lazy initialize it in the IsolationLevel property
				break;
#if NET_2_0
			case IsolationLevel.Chaos:
				throw new ArgumentOutOfRangeException ("IsolationLevel",
					string.Format (CultureInfo.CurrentCulture,
						"The IsolationLevel enumeration " +
						"value, {0}, is not supported by " +
						"the .Net Framework Odbc Data " +
						"Provider.", (int) isolationlevel));
#endif
			default:
#if NET_2_0
				throw new ArgumentOutOfRangeException ("IsolationLevel",
					string.Format (CultureInfo.CurrentCulture,
						"The IsolationLevel enumeration value, {0}, is invalid.",
						(int) isolationlevel));
#else
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture,
					"Not supported isolationlevel - {0}",
					isolationlevel));
#endif
			}

			// only change isolation level if it was explictly set
			if (isolationlevel != IsolationLevel.Unspecified) {
				// mbd: Getting the return code of the second call to SQLSetConnectAttr is missing from original code!
				OdbcReturn ret = libodbc.SQLSetConnectAttr (conn.hDbc,
					attr, (IntPtr) lev, 0);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw conn.CreateOdbcException (OdbcHandleType.Dbc, conn.hDbc);
			}
			this.isolationlevel = isolationlevel;
			connection = conn;
			isOpen = true;
		}

		// Set Auto-commit (102) connection attribute
		// [MonoTODO]: nice to have before svn: define libodbc.SQL_IS_UINTEGER = -5
		private static void SetAutoCommit (OdbcConnection conn, bool isAuto)
		{
			OdbcReturn ret = libodbc.SQLSetConnectAttr (conn.hDbc,
				OdbcConnectionAttribute.AutoCommit,
				(IntPtr) (isAuto ? 1 : 0), -5);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw conn.CreateOdbcException (OdbcHandleType.Dbc, conn.hDbc);
		}

		private static IsolationLevel GetIsolationLevel (OdbcConnection conn)
		{
			int lev;
			int length;
			OdbcReturn ret = libodbc.SQLGetConnectAttr (conn.hDbc,
				OdbcConnectionAttribute.TransactionIsolation,
				out lev, 0, out length);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw conn.CreateOdbcException (OdbcHandleType.Dbc, conn.hDbc);
			return MapOdbcIsolationLevel ((OdbcIsolationLevel) lev);
		}

		private static IsolationLevel MapOdbcIsolationLevel (OdbcIsolationLevel odbcLevel)
		{
			IsolationLevel isoLevel = IsolationLevel.Unspecified;

			switch (odbcLevel) {
			case OdbcIsolationLevel.ReadUncommitted:
				isoLevel = IsolationLevel.ReadUncommitted;
				break;
			case OdbcIsolationLevel.ReadCommitted:
				isoLevel = IsolationLevel.ReadCommitted;
				break;
			case OdbcIsolationLevel.RepeatableRead:
				isoLevel = IsolationLevel.RepeatableRead;
				break;
			case OdbcIsolationLevel.Serializable:
				isoLevel = IsolationLevel.Serializable;
				break;
#if NET_2_0
			case OdbcIsolationLevel.Snapshot:
				isoLevel = IsolationLevel.Snapshot;
				break;
#else
			default:
				throw new NotSupportedException (string.Format (
					CultureInfo.InvariantCulture,
					"Isolation level {0} is not supported.",
					odbcLevel));
#endif
			}
			return isoLevel;
		}

		#region Implementation of IDisposable

#if NET_2_0
		protected override
#endif
		void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing && isOpen)
					Rollback ();
				disposed = true;
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion Implementation of IDisposable

		#region Implementation of IDbTransaction

		public
#if NET_2_0
		override
#endif //NET_2_0
		void Commit ()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			if (connection.transaction == this) {
				OdbcReturn ret = libodbc.SQLEndTran ((short) OdbcHandleType.Dbc, connection.hDbc, 0);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw connection.CreateOdbcException (OdbcHandleType.Dbc, connection.hDbc);
				SetAutoCommit (connection, true); // restore default auto-commit
				connection.transaction = null;
				connection = null;
				isOpen = false;
			} else
				throw new InvalidOperationException ();
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		void Rollback()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			if (connection.transaction == this) {
				OdbcReturn ret = libodbc.SQLEndTran ((short) OdbcHandleType.Dbc, connection.hDbc, 1);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw connection.CreateOdbcException (OdbcHandleType.Dbc, connection.hDbc);
				SetAutoCommit (connection, true);    // restore default auto-commit
				connection.transaction = null;
				connection = null;
				isOpen = false;
			} else
				throw new InvalidOperationException ();
		}

#if NET_2_0
		protected override DbConnection DbConnection {
			get {
				return Connection;
			}
		}
#else
		IDbConnection IDbTransaction.Connection {
			get {
				return Connection;
			}
		}

#endif

		public
#if NET_2_0
		override
#endif
		IsolationLevel IsolationLevel {
			get {
				if (!isOpen)
					throw ExceptionHelper.TransactionNotUsable (GetType ());

				if (isolationlevel == IsolationLevel.Unspecified)
					isolationlevel = GetIsolationLevel (Connection);
				return isolationlevel;
			}
		}

		#endregion Implementation of IDbTransaction

		#region Public Instance Properties

		public new OdbcConnection Connection {
			get {
				return connection;
			}
		}

		#endregion Public Instance Properties
	}
}
