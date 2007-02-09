//
// System.Data.Common.DbProviderFactory.cs
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

#if NET_2_0 || TARGET_JVM

using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common {
	public abstract class DbProviderFactory
	{
		#region Constructors

		protected DbProviderFactory ()
		{
		}

		#endregion // Constructors

		#region Properties

		private void ThrowNotImplementedException ()
		{
			throw new NotImplementedException ();
		}

		public virtual bool CanCreateDataSourceEnumerator { 
			get {
				ThrowNotImplementedException ();
				return false; // To fix compiler error
			}
		}
		#endregion // Properties

		#region Methods

		public virtual DbCommand CreateCommand ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbCommandBuilder CreateCommandBuilder ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbConnection CreateConnection ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbDataAdapter CreateDataAdapter ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbDataSourceEnumerator CreateDataSourceEnumerator ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbParameter CreateParameter ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual CodeAccessPermission CreatePermission (PermissionState state)
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}

		public virtual DbConnectionStringBuilder CreateConnectionStringBuilder ()
		{
			ThrowNotImplementedException ();
			return null; // To fix compiler error
		}
		#endregion // Methods
	}
}

#endif
