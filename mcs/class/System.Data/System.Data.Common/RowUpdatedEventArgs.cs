//
// System.Data.Common.RowUpdatedEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using System.Data;

namespace System.Data.Common {
	public abstract class RowUpdatedEventArgs : EventArgs
	{
		#region Fields

		DataRow dataRow;
		IDbCommand command;
		StatementType statementType;
		DataTableMapping tableMapping;	
		Exception errors;
		UpdateStatus status;
		int recordsAffected;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected RowUpdatedEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			this.dataRow = dataRow;
			this.command = command;
			this.statementType = statementType;
			this.tableMapping = tableMapping;
			this.errors = null;
			this.status = UpdateStatus.Continue;
			this.recordsAffected = 0; // FIXME
		}

		#endregion // Constructors

		#region Properties
		
		[MonoTODO]
		public IDbCommand Command {
			get { return command; }
		}

		[MonoTODO]
		public Exception Errors {
			get { return errors; }
			set { errors = value; }
		}

		[MonoTODO]
		public int RecordsAffected {
			get { return recordsAffected; }
		}

		[MonoTODO]
		public DataRow Row {
			get { return dataRow; }
		}

		[MonoTODO]
		public StatementType StatementType {
			get { return statementType; }
		}

		[MonoTODO]
		public UpdateStatus Status {
			get { return status; }
			set { status = value; }
		}

		[MonoTODO]
		public DataTableMapping TableMapping {
			get { return tableMapping; }
		}

		#endregion // Properties
	}
}
