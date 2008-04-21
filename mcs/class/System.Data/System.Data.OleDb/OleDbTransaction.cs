//
// System.Data.OleDb.OleDbTransaction
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Data.Common;

namespace System.Data.OleDb
{
#if NET_2_0
	public sealed class OleDbTransaction : DbTransaction, IDbTransaction
#else
	public sealed class OleDbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
#endif
	{
		#region Fields

		bool disposed;
		OleDbConnection connection;
		IntPtr gdaTransaction;
		int depth;
		bool isOpen;

		#endregion // Fields

		#region Constructors

		internal OleDbTransaction (OleDbConnection connection, int depth)
			: this (connection, depth, IsolationLevel.ReadCommitted)
		{
		}

		internal OleDbTransaction (OleDbConnection connection)
			: this (connection, 1)
		{
		}

		internal OleDbTransaction (OleDbConnection connection, int depth, IsolationLevel isolevel) 
		{
			this.connection = connection;

			gdaTransaction = libgda.gda_transaction_new (depth.ToString ());
			
			switch (isolevel) {
			case IsolationLevel.ReadCommitted :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.ReadCommitted);
				break;
			case IsolationLevel.ReadUncommitted :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.ReadUncommitted);
				break;
			case IsolationLevel.RepeatableRead :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.RepeatableRead);
				break;
			case IsolationLevel.Serializable :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.Serializable);
				break;
			}
			
			libgda.gda_connection_begin_transaction (connection.GdaConnection, gdaTransaction);
			isOpen = true;
		}

		internal OleDbTransaction (OleDbConnection connection, IsolationLevel isolevel) 
			: this (connection, 1, isolevel)
		{
		}


		#endregion // Constructors

		#region Properties

		public new OleDbConnection Connection {
			get {
				return connection;
			}
		}

#if NET_2_0
		protected override DbConnection DbConnection {
			get { return connection; }
		}
#else
		IDbConnection IDbTransaction.Connection {
			get {
				return connection;
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

				switch (libgda.gda_transaction_get_isolation_level (gdaTransaction)) {
				case GdaTransactionIsolation.ReadCommitted :
					return IsolationLevel.ReadCommitted;
				case GdaTransactionIsolation.ReadUncommitted :
					return IsolationLevel.ReadUncommitted;
				case GdaTransactionIsolation.RepeatableRead :
					return IsolationLevel.RepeatableRead;
				case GdaTransactionIsolation.Serializable :
					return IsolationLevel.Serializable;
				}
				return IsolationLevel.Unspecified;
			}
		}

		#endregion // Properties

		#region Methods

		public OleDbTransaction Begin ()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());
			return new OleDbTransaction (connection, depth + 1);
		}

		public OleDbTransaction Begin (IsolationLevel isolevel) 
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());
			return new OleDbTransaction (connection, depth + 1, isolevel);
		}

		public
#if NET_2_0
		override
#endif
		void Commit ()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			if (!libgda.gda_connection_commit_transaction (connection.GdaConnection,
				gdaTransaction))
				throw new InvalidOperationException ();
			connection = null;
			isOpen = false;
		}

#if ONLY_1_1
		~OleDbTransaction ()
		{
			libgda.FreeObject (gdaTransaction);
			gdaTransaction = IntPtr.Zero;
		}
#endif

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

#if NET_2_0
			base.Dispose (disposing);
#endif
		}

#if !NET_2_0
		void IDisposable.Dispose ()
		{
			Dispose (true);
		}
#endif

		public
#if NET_2_0
		override
#endif
		void Rollback ()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			if (!libgda.gda_connection_rollback_transaction (connection.GdaConnection,
				gdaTransaction))
				throw new InvalidOperationException ();
			connection = null;
			isOpen = false;
		}

		#endregion // Methods
	}
}
