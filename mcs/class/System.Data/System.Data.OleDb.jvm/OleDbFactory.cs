//
// System.Data.OleDb.OleDbFactory.cs
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

#if NET_2_0
namespace System.Data.OleDb
{
	using System.Data;
	using System.Data.Common;
	using System.Security;
	using System.Security.Permissions;

	public sealed class OleDbFactory : DbProviderFactory
	{
		#region Fields
		public static readonly OleDbFactory Instance = new OleDbFactory ();
		#endregion //Fields

		#region Constructors

		private OleDbFactory ()
		{

		}

		#endregion //Constructors

		public override bool CanCreateDataSourceEnumerator
		{
			get { return false; }
		}

		#region public overrides
		public override DbCommand CreateCommand ()
		{
			return (DbCommand) new OleDbCommand ();
		}

		public override DbCommandBuilder CreateCommandBuilder ()
		{
			return (DbCommandBuilder) new OleDbCommandBuilder ();
		}

		public override DbConnection CreateConnection ()
		{
			return (DbConnection) new OleDbConnection ();
		}

		public override DbDataAdapter CreateDataAdapter ()
		{
			return (DbDataAdapter) new OleDbDataAdapter ();
		}

		[MonoLimitation("Code Access Security is not supported")]
		public override CodeAccessPermission CreatePermission (PermissionState state) 
		{
			return new OleDbPermission (state);
		}

		[MonoTODO]
		public override DbDataSourceEnumerator CreateDataSourceEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public override DbParameter CreateParameter ()
		{
			return (DbParameter) new OleDbParameter ();
		}

		#endregion // public overrides
	}
}
#endif // NET_2_0
