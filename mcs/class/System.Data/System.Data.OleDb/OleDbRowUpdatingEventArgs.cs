//
// System.Data.OleDb.OleDbRowUpdatingEventArgs
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
	public sealed class OleDbRowUpdatingEventArgs : RowUpdatingEventArgs
	{

		#region Fields

		OleDbCommand command = null;

		#endregion

		#region Constructors

		[MonoTODO]
		public OleDbRowUpdatingEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base (dataRow, command, statementType, tableMapping)

		{
			this.command = (OleDbCommand) command;
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties
		
		public new OleDbCommand Command {
			get { return command; }
			set { command = value; }
		}

		#endregion
	}
}
