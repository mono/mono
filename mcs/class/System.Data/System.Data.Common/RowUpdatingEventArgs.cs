//
// System.Data.Common.RowUpdatingEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

namespace System.Data.Common {
	public abstract class RowUpdatingEventArgs : EventArgs
	{
		#region Fields

		DataRow dataRow;
		IDbCommand command;
		StatementType statementType;
		DataTableMapping tableMapping;
		UpdateStatus status;
		Exception errors;

		#endregion // Fields

		#region Constructors

		protected RowUpdatingEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			this.dataRow = dataRow;
			this.command = command;
			this.statementType = statementType;
			this.tableMapping = tableMapping;
			this.status = UpdateStatus.Continue;
			this.errors = null;
		}

		#endregion // Constructors

		#region Properties
		
		public IDbCommand Command {
			get { return command; }
			set { command = value; }
		}

		public Exception Errors {
			get { return errors; }
			set { errors = value; }
		}

		public DataRow Row {
			get { return dataRow; }
		}

		public StatementType StatementType {
			get { return statementType; }
		}

		public UpdateStatus Status {
			get { return status; }
			set { status = value; }
		}

		public DataTableMapping TableMapping {
			get { return tableMapping; }
		}

		#endregion // Properties
	}
}
