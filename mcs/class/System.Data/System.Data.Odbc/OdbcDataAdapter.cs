//
// System.Data.Odbc.OdbcDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc {
	[DefaultEvent ("RowUpdated")]
	public sealed class OdbcDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		OdbcCommand deleteCommand;
		OdbcCommand insertCommand;
		OdbcCommand selectCommand;
		OdbcCommand updateCommand;

		#endregion

		#region Constructors
		
		public OdbcDataAdapter () 	
			: this (new OdbcCommand ())
		{
		}

		public OdbcDataAdapter (OdbcCommand selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		public OdbcDataAdapter (string selectCommandText, OdbcConnection selectConnection) 
			: this (new OdbcCommand (selectCommandText, selectConnection))
		{ 
		}

		public OdbcDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OdbcConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for deleted rows in DataSet.")]
		[DefaultValue (null)]
		public OdbcCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for new rows in DataSet.")]
		[DefaultValue (null)]
		public OdbcCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		[DataCategory ("Fill")]
		[DataSysDescription ("Used during Fill/FillSchema.")]
		[DefaultValue (null)]
		public OdbcCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for modified rows in DataSet.")]
		[DefaultValue (null)]
		public OdbcCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is OdbcCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (OdbcCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is OdbcCommand)) 
					throw new ArgumentException ();
				InsertCommand = (OdbcCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is OdbcCommand)) 
					throw new ArgumentException ();
				SelectCommand = (OdbcCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is OdbcCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (OdbcCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OdbcRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OdbcRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
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
				RowUpdated (this, (OdbcRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (OdbcRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered before every DataRow during Update.")]
		public event OdbcRowUpdatedEventHandler RowUpdated;

		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered after every DataRow during Update.")]
		public event OdbcRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
