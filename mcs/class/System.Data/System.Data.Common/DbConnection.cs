//
// System.Data.Common.DbConnection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.ComponentModel;
using System.Data;
using System.EnterpriseServices;

namespace System.Data.Common {
	public abstract class DbConnection : Component, IDbConnection, IDisposable
	{
		#region Constructors

		protected DbConnection ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract string ConnectionString { get; set; }
		public abstract int ConnectionTimeout { get; }
		public abstract string Database { get; }
		public abstract string DataSource { get; }
		public abstract string ServerVersion { get; }
		public abstract ConnectionState State { get; }

		#endregion // Properties

		#region Methods

		protected abstract DbTransaction BeginDbTransaction (IsolationLevel isolationLevel);

		[MonoTODO]
		public DbTransaction BeginTransaction ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbTransaction BeginTransaction (IsolationLevel isolationLevel)
		{
			throw new NotImplementedException ();
		}

		public abstract void ChangeDatabase (string databaseName);
		public abstract void Close ();

		public DbCommand CreateCommand ()
		{
			return CreateDbCommand ();
		}

		protected abstract DbCommand CreateDbCommand ();

                [MonoTODO]
                public virtual void EnlistTransaction (ITransaction transaction)
                {
			throw new NotImplementedException ();                        
                }

		[MonoTODO]
		public virtual void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema (string collectionName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema (string collectionName, string[] restrictionValues)
		{
			throw new NotImplementedException ();
		}

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}
		
		public abstract void Open ();

		#endregion // Methods

	}
}

#endif
