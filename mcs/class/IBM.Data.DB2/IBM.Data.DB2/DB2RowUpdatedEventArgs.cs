using System;
using System.Data;
using System.Data.Common;

namespace IBM.Data.DB2 {
	public sealed class DB2RowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		public DB2RowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		public new DB2Command Command {
			get { return (DB2Command) base.Command; }
		}	
	}
}
