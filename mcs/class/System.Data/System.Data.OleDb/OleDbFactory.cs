//
// System.Data.oleDb.OleDbFactory
//
// Author:
//	Gonzalo Paniagua (gonzalo@novell.com)
//    
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb {
	public sealed class OleDbFactory : DbProviderFactory {
		public static readonly OleDbFactory Instance = new OleDbFactory ();

		private OleDbFactory()
		{
		}

		public override DbCommand CreateCommand ()
		{
			return new OleDbCommand ();
		}

		public override DbCommandBuilder CreateCommandBuilder ()
		{
			return new OleDbCommandBuilder ();
		}

		public override DbConnection CreateConnection ()
		{
			return new OleDbConnection ();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder ()
		{
			//TODO: Once we have an OleDbConnectionStringBuilder implementation, comment the next line out
			//return new OleDbConnectionStringBuilder ();
			return null;
		}

		public override DbDataAdapter CreateDataAdapter ()
		{
			return new OleDbDataAdapter ();
		}

		public override DbParameter CreateParameter ()
		{
			return new OleDbParameter ();
		}

		public override CodeAccessPermission CreatePermission (PermissionState state)
		{
			return new OleDbPermission (state);
		}
	}
}
#endif

