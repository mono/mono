using System;
using System.Data;
using System.Data.Common;

namespace System.Data.Db2Client {
	public sealed class Db2RowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		public Db2RowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		public new Db2Command Command {
			get { return (Db2Command) base.Command; }
		}	
	}
}