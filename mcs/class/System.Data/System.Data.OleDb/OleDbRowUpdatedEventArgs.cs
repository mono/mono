//
// System.Data.OleDb.OleDbRowUpdatedEventArgs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbRowUpdatedEventArgs : RowUpdatedEventArgs
	{
		#region Fields
		
		OleDbCommand command;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public OleDbRowUpdatedEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base (dataRow, command, statementType, tableMapping)

		{
			this.command = (OleDbCommand) command;
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public new OleDbCommand Command {
			get { return command; }
		}

		#endregion // Properties
	}
}
