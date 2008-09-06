//
// Mono.Data.SybaseClient.SybaseClientFactory.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//   Daniel Morgan <monodanmorg@yahoo.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2008 Daniel Morgan
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
using System.Data.Sql;
using System.Security;
using System.Security.Permissions;

namespace Mono.Data.SybaseClient
{
	public sealed class SybaseClientFactory : DbProviderFactory
	{
		#region Fields

		public static readonly SybaseClientFactory Instance = new SybaseClientFactory ();
		
		#endregion //Fields

		#region Constructors

		private SybaseClientFactory ()
		{
		}

		#endregion //Constructors

		#region Properties
		
		public override bool CanCreateDataSourceEnumerator {
			get { return false; }
		}
		
		#endregion //Properties

		#region public overrides

		public override DbCommand CreateCommand ()
		{
			return new SybaseCommand ();
		}

		public override DbCommandBuilder CreateCommandBuilder ()
		{
			//return new SybaseCommandBuilder ();
			throw new NotImplementedException ();
		}

		public override DbConnection CreateConnection ()
		{
			return new SybaseConnection ();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder ()
		{
			return new SybaseConnectionStringBuilder ();
		}

		public override DbDataAdapter CreateDataAdapter ()
		{
			return new SybaseDataAdapter ();
		}

		public override DbDataSourceEnumerator CreateDataSourceEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public override DbParameter CreateParameter ()
		{
			return new SybaseParameter ();
		}
		
		public override CodeAccessPermission CreatePermission (PermissionState state)
		{
			return new SybasePermission(state);
		}
	
		#endregion // public overrides
	}
}
#endif // NET_2_0


