//
// System.Data.ProviderBase.DbConnectionBase
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

using System.Data.Common;
using System.EnterpriseServices;

namespace System.Data.ProviderBase {
	public abstract class DbConnectionBase : DbConnection
	{
		#region Fields

		DbConnectionFactory connectionFactory;
		DbConnectionString connectionOptions;
		string connectionString;
		
		#endregion // Fields

		#region Constructors

		protected DbConnectionBase (DbConnectionBase connection)
			: this (connection.ConnectionFactory)
		{
		}

		protected DbConnectionBase (DbConnectionFactory connectionFactory)
		{
			this.connectionFactory = connectionFactory;
		}

                protected DbConnectionBase ()
                {
                        
                }
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected int CloseCount {
			get { throw new NotImplementedException (); }
		}

		protected internal DbConnectionFactory ConnectionFactory {
			get { return connectionFactory; }
		}

		protected internal DbConnectionString ConnectionOptions {
			get { return connectionOptions; }
		}

		[MonoTODO]
		public override string ConnectionString {
			get { return connectionString; }
			set { 
				connectionOptions = ConnectionFactory.CreateConnectionOptionsInternal (value);
				connectionString = value;
			}
		}

		[MonoTODO]
		public override int ConnectionTimeout {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual int ConnectionTimeoutInternal {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string Database {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string DataSource {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal DbConnectionInternal InnerConnection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ServerVersion {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override ConnectionState State {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Events

		public event StateChangeEventHandler StateChange;

		#endregion // Events

		#region Methods

		[MonoTODO]
		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void ChangeDatabase (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		protected override DbCommand CreateDbCommand ()
		{
			return (DbCommand) ConnectionFactory.ProviderFactory.CreateCommand ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
                public override void EnlistTransaction (ITransaction transaction)
                {
			throw new NotImplementedException ();                        
                }

		[MonoTODO]
		protected virtual DbMetaDataFactory GetMetaDataFactory (DbConnectionInternal internalConnection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void OnStateChange (ConnectionState originalState, ConnectionState currentState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Open ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchema (string collectionName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchema (string collectionName, string [] restrictionValues)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
