//
// System.Data.SqlClient.SqlDataAdapter.cs
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

namespace System.Data.SqlClient {
	[DefaultEvent ("RowUpdated")]
	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields
	
		SqlCommand deleteCommand;
		SqlCommand insertCommand;
		SqlCommand selectCommand;
		SqlCommand updateCommand;

		#endregion

		#region Constructors
		
		public SqlDataAdapter () 	
			: this (new SqlCommand ())
		{
		}

		public SqlDataAdapter (SqlCommand selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		public SqlDataAdapter (string selectCommandText, SqlConnection selectConnection) 
			: this (new SqlCommand (selectCommandText, selectConnection))
		{ 
		}

		public SqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for deleted rows in DataSet.")]
		[DefaultValue (null)]
		public SqlCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for new rows in DataSet.")]
		[DefaultValue (null)]
		public SqlCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		[DataCategory ("Fill")]
		[DataSysDescription ("Used during Fill/FillSchema.")]
		[DefaultValue (null)]
		public SqlCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		[DataCategory ("Update")]
		[DataSysDescription ("Used during Update for modified rows in DataSet.")]
		[DefaultValue (null)]
		public SqlCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				InsertCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				SelectCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (SqlCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (SqlRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (SqlRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered before every DataRow during Update.")]
		public event SqlRowUpdatedEventHandler RowUpdated;

		[DataCategory ("Update")]
		[DataSysDescription ("Event triggered after every DataRow during Update.")]
		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
