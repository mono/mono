//
// SqlSharpDataAdapter.cs - data adapter for SQL#
//                          but uses a data reader 
//                          as the source of data
//
// based on
// System.Data.SqlSharpClient.SqlSharpDataAdapter.cs
// in Mono http://www.go-mono.com/
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danielmorgan@verizon.net)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
// Copyright (C) 2002, 2003 Daniel Morgan
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SqlSharp 
{
	[DefaultEvent ("RowUpdated")]
	public sealed class SqlSharpDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		IDbCommand deleteCommand;
		IDbCommand insertCommand;
		IDbCommand selectCommand;
		IDbCommand updateCommand;

		#endregion

		#region Constructors
		
		public SqlSharpDataAdapter () 	
		{
		}

		public SqlSharpDataAdapter (IDbCommand selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		#endregion

		#region Properties

//		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for deleted rows in DataSet.")]
		[DefaultValue (null)]
		public IDbCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

//		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for new rows in DataSet.")]
		[DefaultValue (null)]
		public IDbCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

//		[DataCategory ("Fill")]
		[DataSysDescription ("Used during Fill/FillSchema.")]
		[DefaultValue (null)]
		public IDbCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

//		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for modified rows in DataSet.")]
		[DefaultValue (null)]
		public IDbCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				DeleteCommand = value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				InsertCommand = value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				SelectCommand = value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				UpdateCommand = value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlSharpRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlSharpRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
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
				RowUpdated (this, (SqlSharpRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (SqlSharpRowUpdatingEventArgs) value);
		}
                
		public int FillTable (DataTable dataTable, IDataReader dataReader) 
		{
			return base.Fill (dataTable, dataReader);
		}

		#endregion // Methods

		#region Events and Delegates

//		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered before every DataRow during Update.")]
		public event SqlSharpRowUpdatedEventHandler RowUpdated;

//		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered after every DataRow during Update.")]
		public event SqlSharpRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}

	public sealed class SqlSharpRowUpdatedEventArgs : RowUpdatedEventArgs {
		#region Constructors

		public SqlSharpRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping) {
		}

		#endregion // Constructors

		#region Properties

		public new IDbCommand Command {
			get { return base.Command; }
		}

		#endregion // Properties
	}

	public delegate void SqlSharpRowUpdatedEventHandler (object sender, SqlSharpRowUpdatedEventArgs e);

	public sealed class SqlSharpRowUpdatingEventArgs : RowUpdatingEventArgs {
		#region Constructors

		public SqlSharpRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping) {
		}

		#endregion // Constructors

		#region Properties

		public new IDbCommand Command {
			get { return base.Command; }
			set { base.Command = value; }
		}

		#endregion // Properties
	}

	public delegate void SqlSharpRowUpdatingEventHandler(object sender, SqlSharpRowUpdatingEventArgs e);

}

