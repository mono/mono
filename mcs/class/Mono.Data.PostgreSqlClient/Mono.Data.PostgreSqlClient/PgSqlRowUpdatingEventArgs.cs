//
// System.Data.SqlClient.SqlRowUpdatingEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	public sealed class SqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		[MonoTODO]
		public SqlRowUpdatingEventArgs(	DataRow row,
			IDbCommand command, StatementType statementType,
			DataTableMapping tableMapping) {
			// FIXME: do the constructor
		}

		[MonoTODO]
		public new SqlCommand Command {
			get {
				throw new NotImplementedException ();
			} 
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		~SqlRowUpdatingEventArgs() {
			// FIXME: create destructor to release resources
		}
	}
}
