//
// System.Data.SqlClient.SqlTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Data.Common;

namespace System.Data.SqlClient
{
	public sealed class SqlTransaction : DbTransaction, IDbTransaction, IDisposable
	{
		#region Fields

		bool disposed;
		SqlConnection connection;
		IsolationLevel isolationLevel;
		bool isOpen;

		#endregion

		#region Constructors

		internal SqlTransaction (SqlConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
			isOpen = true;
		}

		#endregion // Constructors

		#region Properties

		public
		new
		SqlConnection Connection {
			get { return connection; }
		}

		internal bool IsOpen {
			get { return isOpen; }
		}

		public
		override
		IsolationLevel IsolationLevel {
			get {
				if (!isOpen)
					throw ExceptionHelper.TransactionNotUsable (GetType ());
				return isolationLevel;
			}
		}

		protected override DbConnection DbConnection {
			get { return Connection; }
		}

		#endregion // Properties

		#region Methods

		public 
		override
		void Commit ()
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			connection.Tds.Execute ("COMMIT TRANSACTION");
			connection.Transaction = null;
			connection = null;
			isOpen = false;
		}

		protected override
		void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (isOpen) // in case it is called in the dispose of the class, then the isOpen is already false 
						Rollback ();
				}
				disposed = true;
			}
		}


		public 
		override
		void Rollback ()
		{
			Rollback (String.Empty);
		}

		public void Rollback (string transactionName)
		{
			if (disposed)
				return;

			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());

			if (connection.Tds.IsConnected)
				connection.Tds.Execute (String.Format ("IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION {0}",
									transactionName));
			isOpen = false;
			connection.Transaction = null;
			connection = null;
		}

		public void Save (string savePointName)
		{
			if (!isOpen)
				throw ExceptionHelper.TransactionNotUsable (GetType ());
			connection.Tds.Execute (String.Format ("SAVE TRANSACTION {0}", savePointName));
		}

		#endregion // Methods
	}
}
