//
// OracleDataAdapter.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Parts transferred from System.Data.SqlClient/SqlDataAdapter.cs
// Authors:
//      Rodrigo Moya (rodrigo@ximian.com)
//      Daniel Morgan (danmorg@sc.rr.com)
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
// (C) Ximian, Inc 2002
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OracleClient {
	public sealed class OracleDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		OracleCommand deleteCommand;
		OracleCommand insertCommand;
		OracleCommand selectCommand;
		OracleCommand updateCommand;

		#endregion

		#region Constructors
		
		public OracleDataAdapter () 	
			: this (new OracleCommand ())
		{
		}

		public OracleDataAdapter (OracleCommand selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		public OracleDataAdapter (string selectCommandText, OracleConnection selectConnection) 
			: this (new OracleCommand (selectCommandText, selectConnection))
		{ 
		}

		public OracleDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OracleConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public OracleCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public OracleCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public OracleCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		public OracleCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is OracleCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (OracleCommand) value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is OracleCommand)) 
					throw new ArgumentException ();
				InsertCommand = (OracleCommand) value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is OracleCommand)) 
					throw new ArgumentException ();
				SelectCommand = (OracleCommand) value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is OracleCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (OracleCommand) value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OracleRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OracleRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// Release managed resources
				}
				// Release unmanaged resources
				disposed = true;
			}
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (OracleRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (OracleRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		public event OracleRowUpdatedEventHandler RowUpdated;
		public event OracleRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
