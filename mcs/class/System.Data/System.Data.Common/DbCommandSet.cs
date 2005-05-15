//
// System.Data.Common.DbCommandSet
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

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public abstract class DbCommandSet : IDisposable
	{
		#region Constructors

		protected DbCommandSet ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int CommandCount { get; }
		public abstract int CommandTimeout { get; set; }

		public DbConnection Connection {
			get { return DbConnection; }
			set { DbConnection = value; }
		}

		protected abstract DbConnection DbConnection { get; set; }
		protected abstract DbTransaction DbTransaction { get; set; }

		public DbTransaction Transaction {
			get { return DbTransaction; }
		}

		#endregion // Properties

		#region Methods

		public abstract void Append (DbCommand command);
		public abstract void Cancel ();
		public abstract void Clear ();
		public abstract void CopyToParameter (int commandIndex, int parameterIndex, DbParameter destination);
		public abstract void CopyToParameter (int commandIndex, string parameterName, DbParameter destination);
		public abstract void CopyToParameterCollection (int commandIndex, DbParameterCollection destination);
		public abstract void Dispose ();
		public abstract DbDataReader ExecuteDbDataReader (CommandBehavior behavior);
		public abstract int ExecuteNonQuery ();

		[MonoTODO]
		public DbDataReader ExecuteReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		public abstract int GetParameterCount (int commandIndex);
		
		#endregion // Methods

	}
}

#endif
