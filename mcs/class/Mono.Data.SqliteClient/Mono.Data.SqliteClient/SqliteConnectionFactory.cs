//
// Mono.Data.SqliteClient.SqliteConnectionFactory
//
// Author:
//   Chris Toshok <toshok@ximian.com>
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SqliteClient {

	internal class SqliteConnectionFactory : DbConnectionFactory
	{
		internal static SqliteConnectionFactory Instance; // singleton
                private static DbProviderFactory _providerFactory;
		static readonly object lockobj = new object ();

		private SqliteConnectionFactory (DbProviderFactory pvdrfactory)
		{
                        _providerFactory = pvdrfactory;
		}

		public override DbProviderFactory ProviderFactory { get { return _providerFactory; }}

		// create singleton connection factory.
		internal static DbConnectionFactory GetSingleton (DbProviderFactory pvdrFactory)
		{
			lock (lockobj) {
				if (Instance == null)
					Instance = new SqliteConnectionFactory (pvdrFactory);
				return Instance;
			}
		}

		[MonoTODO]
                public new void ClearAllPools ()
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                public new void ClearPool (DbConnectionBase connection)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                protected override DbMetaDataFactory CreateMetaDataFactory (DbConnectionInternal internalConnection)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                protected override DbConnectionInternal EndCreateConnection (IAsyncResult asyncResult)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                public new void SetConnectionPoolOptions (string connectionString, DbConnectionPoolOptions poolOptions)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
		protected override DbConnectionInternal CreateConnection (DbConnectionString options, DbConnectionBase owningObject)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbConnectionString CreateConnectionOptions (string connectionString)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbConnectionPoolOptions CreateConnectionPoolOptions (DbConnectionString options)
		{
                        throw new NotImplementedException ();
		}
        }
}

#endif
