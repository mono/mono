using System;
using System.Data;
using System.Data.Common;

namespace IBM.Data.DB2 {
	public sealed class DB2RowUpdatingEventArgs : RowUpdatingEventArgs
	{
		public DB2RowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		public new DB2Command Command {
			get { return (DB2Command) base.Command; }
			set { base.Command = value; }
		}

	}
}
