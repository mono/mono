//
// System.Data.SqlClient.SqlClientFactory.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
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

using System.Data;
using System.Data.Common;
using System.Data.Sql;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient
{
	public sealed class SqlClientFactory : DbProviderFactory
	{
		#region Fields

		public static readonly SqlClientFactory Instance = new SqlClientFactory ();
		
		#endregion //Fields

		#region Constructors

		private SqlClientFactory ()
		{
		}

		#endregion //Constructors

		#region Properties
		
		public override bool CanCreateDataSourceEnumerator {
			get { return true; }
		}
		
		#endregion //Properties

		#region public overrides

		public override DbCommand CreateCommand ()
		{
			return new SqlCommand ();
		}

		public override DbCommandBuilder CreateCommandBuilder ()
		{
			return new SqlCommandBuilder ();
		}

		public override DbConnection CreateConnection ()
		{
			return new SqlConnection ();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder ()
		{
			return new SqlConnectionStringBuilder ();
		}

		public override DbDataAdapter CreateDataAdapter ()
		{
			return new SqlDataAdapter ();
		}

		public override DbDataSourceEnumerator CreateDataSourceEnumerator ()
		{
			return SqlDataSourceEnumerator.Instance;
		}

		public override DbParameter CreateParameter ()
		{
			return new SqlParameter ();
		}
		
		public override CodeAccessPermission CreatePermission (PermissionState state)
		{
			return new SqlClientPermission(state);
		}
	
		#endregion // public overrides
	}
}
