//
// System.Data.SqlClient.SqlRowUpdatedEventArgs.cs
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

namespace System.Data.SqlClient {
	public sealed class SqlRowUpdatedEventArgs : RowUpdatedEventArgs {	
		
		[MonoTODO]
		public SqlRowUpdatedEventArgs (DataRow row,
			IDbCommand command, StatementType statementType,
			DataTableMapping tableMapping) {
			// FIXME: do the constructor
		}

		[MonoTODO]
		public new SqlCommand Command {
			get {
		
			}
		}

		[MonoTODO]
		~SqlRowUpdatedEventArgs () {
			// FIXME: need destructor to release resources
		}

	}
}
