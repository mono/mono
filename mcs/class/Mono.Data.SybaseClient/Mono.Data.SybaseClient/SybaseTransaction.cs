//
// Mono.Data.SybaseClient.SybaseTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//
// Copyright (C) Tim Coleman, 2002, 2008
//

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

namespace Mono.Data.SybaseClient {
#if NET_2_0
	public sealed class SybaseTransaction : DbTransaction, IDbTransaction, IDisposable
#else
	public sealed class SybaseTransaction : MarshalByRefObject, IDbTransaction, IDisposable
#endif // NET_2_0
	{
		#region Fields

		bool disposed = false;

		SybaseConnection connection;
		IsolationLevel isolationLevel;
		bool isOpen;

		#endregion

		#region Constructors

		internal SybaseTransaction (SybaseConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
			isOpen = true;
		}

		#endregion // Constructors

		#region Properties

		public
#if NET_2_0
		new
#endif // NET_2_0
		SybaseConnection Connection {
			get { return connection; }
		}

		internal bool IsOpen {
			get { return isOpen; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		IsolationLevel IsolationLevel {
			get { return isolationLevel; }
		}
		
#if NET_2_0
		protected override DbConnection DbConnection {
			get { return Connection; }
		}
#else
		IDbConnection IDbTransaction.Connection {
			get { return Connection; }
		}
#endif

		#endregion // Properties
               
		#region Methods

		public 
#if NET_2_0
		override
#endif // NET_2_0
		void Commit ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.Execute ("COMMIT TRANSACTION");
			connection.Transaction = null;
			isOpen = false;
		}		

#if NET_2_0
		protected override
#endif
		void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing)
					Rollback ();
				disposed = true;
			}
		}

#if !NET_2_0
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
#endif

		public 
#if NET_2_0
		override
#endif // NET_2_0
		void Rollback ()
		{
			Rollback (String.Empty);
		}

		public void Rollback (string transactionName)
		{
#if NET_2_0
			if (disposed)
				return;
#endif

			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");

			connection.Tds.Execute (String.Format ("IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION {0}",
								transactionName));
			isOpen = false;
			connection.Transaction = null;
			connection = null;
		}

		public void Save (string savePointName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.Execute (String.Format ("SAVE TRANSACTION {0}", savePointName));
		}

		#endregion // Methods
	}
}
